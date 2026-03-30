using System.Net;
using System.Text;
using System.Text.Json;
using MimeKit;
using Serilog;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace MailboxSearch;

public sealed class EmailSearchService
{
    private const int CurrentCacheVersion = 2;

    public async Task<IReadOnlyList<EmailSearchResult>> SearchAsync(
        string rootFolderPath,
        SearchOptions searchOptions,
        Action<SearchProgress>? progressCallback = null,
        Action<EmailSearchResult>? resultCallback = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            SearchQuery query = SearchQuery.Parse(searchOptions.QueryText);
            if (query.Terms.Count == 0)
            {
                return [];
            }

            Guid searchId = Guid.NewGuid();
            ILogger searchLogger = Log.ForContext("SearchId", searchId)
                .ForContext("RootFolderPath", rootFolderPath)
                .ForContext("QueryText", searchOptions.QueryText)
                .ForContext("DateFrom", searchOptions.DateFrom)
                .ForContext("DateTo", searchOptions.DateTo)
                .ForContext("SortCriterion", searchOptions.SortCriterion);
            
            string cacheDirectoryPath = Path.Combine(rootFolderPath, "_cache");
            Directory.CreateDirectory(cacheDirectoryPath);

            string[] filePaths = Directory
                .EnumerateFiles(rootFolderPath, "*.eml", SearchOption.AllDirectories)
                .Where(filePath => !IsInCacheDirectory(filePath, cacheDirectoryPath))
                .ToArray();

            searchLogger.Information(
                "Search initiated for {TotalMessages} message(s).",
                filePaths.Length);

            progressCallback?.Invoke(new SearchProgress(0, filePaths.Length));

            Exception? priorityResult = TrySetCurrentThreadPriority(ThreadPriority.BelowNormal);
            if (priorityResult is not null)
                searchLogger.Warning("Could not reduce current thread priority");

            try
            {
                List<EmailSearchResult> results = [];
                int skippedMessages = 0;
                for (int index = 0; index < filePaths.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string filePath = filePaths[index];

                    ILogger messageLogger = searchLogger
                        .ForContext("MessageFilePath", filePath)
                        .ForContext("MessageNumber", index + 1)
                        .ForContext("TotalMessages", filePaths.Length);

                    try
                    {
                        CachedEmailDocument? cachedDocument = await GetCachedDocumentAsync(
                            rootFolderPath,
                            cacheDirectoryPath,
                            filePath,
                            messageLogger,
                            cancellationToken).ConfigureAwait(false);

                        if (cachedDocument is not null
                            && MatchesDateRange(cachedDocument.Date, searchOptions)
                            && Matches(cachedDocument.SearchableContent, query))
                        {
                            EmailSearchResult result = cachedDocument.ToSearchResult();
                            results.Add(result);
                            resultCallback?.Invoke(result);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        skippedMessages++;
                        messageLogger.Warning(ex, "Skipping message because processing failed.");
                    }
                    finally
                    {
                        progressCallback?.Invoke(new SearchProgress(index + 1, filePaths.Length));
                    }
                }

                IReadOnlyList<EmailSearchResult> orderedResults = OrderResults(results, searchOptions.SortCriterion);

                searchLogger.Information(
                    "Search completed with {ResultCount} result(s) and {SkippedMessages} skipped message(s).",
                    orderedResults.Count,
                    skippedMessages);

                return orderedResults;
            }
            catch (OperationCanceledException)
            {
                searchLogger.Information("Search cancelled.");
                throw;
            }
            finally
            {
                priorityResult = TrySetCurrentThreadPriority(ThreadPriority.Normal);
                if (priorityResult is not null)
                    searchLogger.Warning("Could not restore current thread priority");
            }
        }, cancellationToken);
    }

    private static Exception? TrySetCurrentThreadPriority(ThreadPriority priority)
    {
        try
        {
            Thread.CurrentThread.Priority = priority;
        }
        catch(Exception exception)
        {
            return exception;
        }

        return null;
    }

    private static bool IsInCacheDirectory(string filePath, string cacheDirectoryPath)
    {
        string normalizedFilePath = Path.GetFullPath(filePath);
        string normalizedCacheDirectory = Path.GetFullPath(cacheDirectoryPath)
                                              .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                          + Path.DirectorySeparatorChar;

        return normalizedFilePath.StartsWith(normalizedCacheDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<MimeMessage> LoadMessageAsync(string filePath, CancellationToken cancellationToken)
    {
        await using FileStream stream = File.OpenRead(filePath);
        return await MimeMessage.LoadAsync(stream, cancellationToken);
    }

    private static async Task<CachedEmailDocument?> GetCachedDocumentAsync(
        string rootFolderPath,
        string cacheDirectoryPath,
        string filePath,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        string cacheFilePath = GetCacheFilePath(rootFolderPath, cacheDirectoryPath, filePath);
        FileInfo sourceFileInfo = new FileInfo(filePath);

        CachedEmailDocument? cachedDocument = await TryReadCachedDocumentAsync(cacheFilePath, sourceFileInfo, logger, cancellationToken);
        if (cachedDocument is not null)
        {
            return cachedDocument;
        }

        MimeMessage message = await LoadMessageAsync(filePath, cancellationToken);
        cachedDocument = CreateCachedDocument(filePath, message);

        try
        {
            await WriteCachedDocumentAsync(cacheFilePath, sourceFileInfo, cachedDocument, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to write cache entry for message.");
        }

        return cachedDocument;
    }

    private static string BuildSearchableContent(MimeMessage message)
    {
        StringBuilder builder = new();
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
        string relativePath = Path.GetRelativePath(rootFolderPath, filePath);
        string safeRelativePath = relativePath.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty);
        string cacheRelativePath = Path.ChangeExtension(safeRelativePath, ".search.json");
        return Path.Combine(cacheDirectoryPath, cacheRelativePath);
    }

    private static async Task<CachedEmailDocument?> TryReadCachedDocumentAsync(
        string cacheFilePath,
        FileInfo sourceFileInfo,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(cacheFilePath))
        {
            return null;
        }

        try
        {
            await using FileStream stream = File.OpenRead(cacheFilePath);
            SearchableContentCacheEntry? cacheEntry = await JsonSerializer.DeserializeAsync<SearchableContentCacheEntry>(stream, cancellationToken: cancellationToken);
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
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to read cache entry for message. The source file will be reprocessed.");
            return null;
        }
    }

    private static async Task WriteCachedDocumentAsync(
        string cacheFilePath,
        FileInfo sourceFileInfo,
        CachedEmailDocument cachedDocument,
        CancellationToken cancellationToken)
    {
        string? cacheDirectory = Path.GetDirectoryName(cacheFilePath);
        if (!string.IsNullOrWhiteSpace(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        SearchableContentCacheEntry cacheEntry = new SearchableContentCacheEntry
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

        await using FileStream stream = File.Create(cacheFilePath);
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
        string normalizedSubject = SearchTextNormalizer.Normalize(subject);
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
        IEnumerable<string> lines = value
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(SearchTextNormalizer.Normalize)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractTextFromHtml(string html)
    {
        HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
        document.LoadHtml(html);
        string text = document.DocumentNode.InnerText;
        return WebUtility.HtmlDecode(text);
    }

    private static bool Matches(string content, SearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        if (query.ExactPhrase)
        {
            return content.Contains(query.Terms[0], comparison);
        }

        return query.Terms.Any(term => content.Contains(term, comparison));
    }

    private static bool MatchesDateRange(DateTimeOffset? messageDate, SearchOptions searchOptions)
    {
        if (!searchOptions.HasDateFilter)
        {
            return true;
        }

        if (messageDate is null)
        {
            return false;
        }

        if (searchOptions.DateFrom is { } dateFrom && messageDate.Value < dateFrom)
        {
            return false;
        }

        if (searchOptions.DateTo is { } dateTo && messageDate.Value > dateTo)
        {
            return false;
        }

        return true;
    }

    private static IReadOnlyList<EmailSearchResult> OrderResults(
        IEnumerable<EmailSearchResult> results,
        SearchSortCriterion sortCriterion)
    {
        return sortCriterion switch
        {
            SearchSortCriterion.Title => results
                .OrderBy(result => result.Subject, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(result => result.Sender, StringComparer.CurrentCultureIgnoreCase)
                .ThenByDescending(result => result.Date.HasValue)
                .ThenByDescending(result => result.Date)
                .ToList(),
            SearchSortCriterion.Author => results
                .OrderBy(result => result.Sender, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(result => result.Subject, StringComparer.CurrentCultureIgnoreCase)
                .ThenByDescending(result => result.Date.HasValue)
                .ThenByDescending(result => result.Date)
                .ToList(),
            _ => results
                .OrderByDescending(result => result.Date.HasValue)
                .ThenByDescending(result => result.Date)
                .ThenBy(result => result.Subject, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(result => result.Sender, StringComparer.CurrentCultureIgnoreCase)
                .ToList()
        };
    }

    private static string FormatSender(MimeMessage message)
    {
        MailboxAddress? mailbox = message.From.Mailboxes.FirstOrDefault();
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