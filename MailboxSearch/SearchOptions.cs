namespace MailboxSearch;

public sealed class SearchOptions
{
    public required string QueryText { get; init; }

    public DateTimeOffset? DateFrom { get; init; }

    public DateTimeOffset? DateTo { get; init; }

    public SearchSortCriterion SortCriterion { get; init; } = SearchSortCriterion.Date;

    public bool HasDateFilter => DateFrom is not null || DateTo is not null;
}

public enum SearchSortCriterion
{
    Date,
    Title,
    Author
}