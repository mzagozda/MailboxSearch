namespace MailboxSearch;

public sealed class SearchQuery
{
    public required string RawValue { get; init; }

    public required IReadOnlyList<string> Terms { get; init; }

    public bool ExactPhrase { get; init; }

    public static SearchQuery Parse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            var phrase = SearchTextNormalizer.Normalize(trimmed[1..^1].Trim());
            return new SearchQuery
            {
                RawValue = input,
                Terms = string.IsNullOrWhiteSpace(phrase) ? Array.Empty<string>() : new[] { phrase },
                ExactPhrase = true
            };
        }

        var terms = trimmed
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(SearchTextNormalizer.Normalize)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new SearchQuery
        {
            RawValue = input,
            Terms = terms,
            ExactPhrase = false
        };
    }
}