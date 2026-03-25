namespace MailboxSearch;

public sealed class EmailSearchResult
{
    public required string FilePath { get; init; }

    public required string Subject { get; init; }

    public required string Sender { get; init; }

    public DateTimeOffset? Date { get; init; }

    public string PreviewText { get; init; } = string.Empty;

    public string DateDisplay => Date?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
}