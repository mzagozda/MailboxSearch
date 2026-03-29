namespace MailboxSearch;

public sealed class SearchQuery
{
    public required string RawValue { get; init; }

    public required IReadOnlyList<string> Terms { get; init; }

    public bool ExactPhrase { get; private init; }

    public static SearchQuery Parse(string input)
    {
        string trimmed = input.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            string phrase = SearchTextNormalizer.Normalize(trimmed[1..^1].Trim());
            return new SearchQuery
            {
                RawValue = input,
                Terms = string.IsNullOrWhiteSpace(phrase) ? Array.Empty<string>() : new[] { phrase },
                ExactPhrase = true
            };
        }

        string[] terms = trimmed
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