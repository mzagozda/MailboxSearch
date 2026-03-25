using System.Net;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using MimeKit;

namespace MailboxSearch;

public sealed class EmailSearchService
{
    private const int CurrentCacheVersion = 2;

    public async Task<IReadOnlyList<EmailSearchResult>> SearchAsync(
        string rootFolderPath,
        string queryText,
        Action<SearchProgress>? progressCallback = null,
        Action<EmailSearchResult>? resultCallback = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            TrySetCurrentThreadPriority(ThreadPriority.BelowNormal);

            var query = SearchQuery.Parse(queryText);
            if (query.Terms.Count == 0)
            {
                return (IReadOnlyList<EmailSearchResult>)Array.Empty<EmailSearchResult>();
            }

            var cacheDirectoryPath = Path.Combine(rootFolderPath, "_cache");
            Directory.CreateDirectory(cacheDirectoryPath);

            var filePaths = Directory
                .EnumerateFiles(rootFolderPath, "*.eml", SearchOption.AllDirectories)
                .Where(filePath => !IsInCacheDirectory(filePath, cacheDirectoryPath))
                .ToArray();

            progressCallback?.Invoke(new SearchProgress(0, filePaths.Length));

            var results = new List<EmailSearchResult>();
            for (var index = 0; index < filePaths.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var filePath = filePaths[index];

                var cachedDocument = await GetCachedDocumentAsync(rootFolderPath, cacheDirectoryPath, filePath, cancellationToken).ConfigureAwait(false);
                if (cachedDocument is null || !Matches(cachedDocument.SearchableContent, query))
                {
                    progressCallback?.Invoke(new SearchProgress(index + 1, filePaths.Length));
                    continue;
                }

                var result = cachedDocument.ToSearchResult();
                results.Add(result);
                resultCallback?.Invoke(result);
                progressCallback?.Invoke(new SearchProgress(index + 1, filePaths.Length));
            }

            return (IReadOnlyList<EmailSearchResult>)results
                .OrderByDescending(result => result.Date ?? DateTimeOffset.MinValue)
                .ThenBy(result => result.Subject, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }, cancellationToken);
    }

    private static void TrySetCurrentThreadPriority(ThreadPriority priority)
    {
        try
        {
            Thread.CurrentThread.Priority = priority;
        }
        catch
        {
        }
    }

    private static bool IsInCacheDirectory(string filePath, string cacheDirectoryPath)
    {
        var normalizedFilePath = Path.GetFullPath(filePath);
        var normalizedCacheDirectory = Path.GetFullPath(cacheDirectoryPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return normalizedFilePath.StartsWith(normalizedCacheDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<MimeMessage?> LoadMessageAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(filePath);
            return await MimeMessage.LoadAsync(stream, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<CachedEmailDocument?> GetCachedDocumentAsync(
        string rootFolderPath,
        string cacheDirectoryPath,
        string filePath,
        CancellationToken cancellationToken)
    {
        var cacheFilePath = GetCacheFilePath(rootFolderPath, cacheDirectoryPath, filePath);
        var sourceFileInfo = new FileInfo(filePath);

        var cachedDocument = await TryReadCachedDocumentAsync(cacheFilePath, sourceFileInfo, cancellationToken);
        if (cachedDocument is not null)
        {
            return cachedDocument;
        }

        var message = await LoadMessageAsync(filePath, cancellationToken);
        if (message is null)
        {
            return null;
        }

        cachedDocument = CreateCachedDocument(filePath, message);
        await WriteCachedDocumentAsync(cacheFilePath, sourceFileInfo, cachedDocument, cancellationToken);
        return cachedDocument;
    }

    private static string BuildSearchableContent(MimeMessage message)
    {
        var builder = new StringBuilder();
        builder.AppendLine(SearchTextNormalizer.Normalize(message.Subject));
        builder.AppendLine(SearchTextNormalizer.Normalize(FormatSender(message)));

        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            builder.AppendLine(SearchTextNormalizer.Normalize(message.TextBody));
        }

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            builder.AppendLine(SearchTextNormalizer.Normalize(ExtractTextFromHtml(message.HtmlBody)));
        }

        return SearchTextNormalizer.Normalize(builder.ToString());
    }

    private static string GetCacheFilePath(string rootFolderPath, string cacheDirectoryPath, string filePath)
    {
        var relativePath = Path.GetRelativePath(rootFolderPath, filePath);
        var safeRelativePath = relativePath.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty);
        var cacheRelativePath = Path.ChangeExtension(safeRelativePath, ".search.json");
        return Path.Combine(cacheDirectoryPath, cacheRelativePath);
    }

    private static async Task<CachedEmailDocument?> TryReadCachedDocumentAsync(
        string cacheFilePath,
        FileInfo sourceFileInfo,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(cacheFilePath))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(cacheFilePath);
            var cacheEntry = await JsonSerializer.DeserializeAsync<SearchableContentCacheEntry>(stream, cancellationToken: cancellationToken);
            if (cacheEntry is null)
            {
                return null;
            }

            if (cacheEntry.SourceLastWriteTimeUtcTicks != sourceFileInfo.LastWriteTimeUtc.Ticks)
            {
                return null;
            }

            if (cacheEntry.SourceFileLength != sourceFileInfo.Length)
            {
                return null;
            }

            if (cacheEntry.CacheVersion != CurrentCacheVersion)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(cacheEntry.SearchableContent))
            {
                return null;
            }

            return new CachedEmailDocument
            {
                FilePath = cacheEntry.FilePath ?? sourceFileInfo.FullName,
                Subject = string.IsNullOrWhiteSpace(cacheEntry.Subject) ? "(No subject)" : SearchTextNormalizer.Normalize(cacheEntry.Subject),
                Sender = SearchTextNormalizer.Normalize(cacheEntry.Sender),
                Date = cacheEntry.Date,
                PreviewText = SearchTextNormalizer.Normalize(cacheEntry.PreviewText),
                SearchableContent = SearchTextNormalizer.Normalize(cacheEntry.SearchableContent)
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static async Task WriteCachedDocumentAsync(
        string cacheFilePath,
        FileInfo sourceFileInfo,
        CachedEmailDocument cachedDocument,
        CancellationToken cancellationToken)
    {
        var cacheDirectory = Path.GetDirectoryName(cacheFilePath);
        if (!string.IsNullOrWhiteSpace(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        var cacheEntry = new SearchableContentCacheEntry
        {
            CacheVersion = CurrentCacheVersion,
            SourceLastWriteTimeUtcTicks = sourceFileInfo.LastWriteTimeUtc.Ticks,
            SourceFileLength = sourceFileInfo.Length,
            FilePath = cachedDocument.FilePath,
            Subject = cachedDocument.Subject,
            Sender = cachedDocument.Sender,
            Date = cachedDocument.Date,
            PreviewText = cachedDocument.PreviewText,
            SearchableContent = cachedDocument.SearchableContent
        };

        await using var stream = File.Create(cacheFilePath);
        await JsonSerializer.SerializeAsync(stream, cacheEntry, cancellationToken: cancellationToken);
    }

    private static CachedEmailDocument CreateCachedDocument(string filePath, MimeMessage message)
    {
        return new CachedEmailDocument
        {
            FilePath = filePath,
            Subject = NormalizeSubject(message.Subject),
            Sender = SearchTextNormalizer.Normalize(FormatSender(message)),
            Date = message.Date != DateTimeOffset.MinValue ? message.Date : null,
            PreviewText = BuildPreviewText(message),
            SearchableContent = BuildSearchableContent(message)
        };
    }

    private static string NormalizeSubject(string? subject)
    {
        var normalizedSubject = SearchTextNormalizer.Normalize(subject);
        return string.IsNullOrWhiteSpace(normalizedSubject) ? "(No subject)" : normalizedSubject;
    }

    private static string BuildPreviewText(MimeMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            return NormalizePreviewText(message.TextBody);
        }

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            return NormalizePreviewText(ExtractTextFromHtml(message.HtmlBody));
        }

        return string.Empty;
    }

    private static string NormalizePreviewText(string value)
    {
        var lines = value
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(SearchTextNormalizer.Normalize)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractTextFromHtml(string html)
    {
        var document = new HtmlAgilityPack.HtmlDocument();
        document.LoadHtml(html);
        var text = document.DocumentNode.InnerText;
        return WebUtility.HtmlDecode(text);
    }

    private static bool Matches(string content, SearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var comparison = StringComparison.OrdinalIgnoreCase;
        if (query.ExactPhrase)
        {
            return content.Contains(query.Terms[0], comparison);
        }

        return query.Terms.Any(term => content.Contains(term, comparison));
    }

    private static string FormatSender(MimeMessage message)
    {
        var mailbox = message.From.Mailboxes.FirstOrDefault();
        if (mailbox is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(mailbox.Name) && !string.IsNullOrWhiteSpace(mailbox.Address))
        {
            return $"{mailbox.Name} <{mailbox.Address}>";
        }

        return mailbox.ToString();
    }

    private sealed class SearchableContentCacheEntry
    {
        public int CacheVersion { get; init; }

        public long SourceLastWriteTimeUtcTicks { get; init; }

        public long SourceFileLength { get; init; }

        public string? FilePath { get; init; }

        public string? Subject { get; init; }

        public string? Sender { get; init; }

        public DateTimeOffset? Date { get; init; }

        public string? PreviewText { get; init; }

        public string? SearchableContent { get; init; }
    }

    private sealed class CachedEmailDocument
    {
        public required string FilePath { get; init; }

        public required string Subject { get; init; }

        public required string Sender { get; init; }

        public DateTimeOffset? Date { get; init; }

        public required string PreviewText { get; init; }

        public required string SearchableContent { get; init; }

        public EmailSearchResult ToSearchResult()
        {
            return new EmailSearchResult
            {
                FilePath = FilePath,
                Subject = Subject,
                Sender = Sender,
                Date = Date,
                PreviewText = PreviewText
            };
        }
    }
}

public readonly record struct SearchProgress(int CurrentMessageNumber, int TotalMessages);