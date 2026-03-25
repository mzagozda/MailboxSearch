using System.Globalization;
using System.Text;

namespace MailboxSearch;

internal static class SearchTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decoded = DecodeUnicodeEscapes(value);
        return decoded.Normalize(NormalizationForm.FormC);
    }

    private static string DecodeUnicodeEscapes(string value)
    {
        var builder = new StringBuilder(value.Length);

        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '\\' || index + 1 >= value.Length)
            {
                builder.Append(value[index]);
                continue;
            }

            var marker = value[index + 1];
            if (marker == 'u' && TryParseHexCodePoint(value, index + 2, 4, out var unicodeCodePoint))
            {
                builder.Append(char.ConvertFromUtf32(unicodeCodePoint));
                index += 5;
                continue;
            }

            if (marker == 'U' && TryParseHexCodePoint(value, index + 2, 8, out unicodeCodePoint))
            {
                builder.Append(char.ConvertFromUtf32(unicodeCodePoint));
                index += 9;
                continue;
            }

            builder.Append(value[index]);
        }

        return builder.ToString();
    }

    private static bool TryParseHexCodePoint(string value, int startIndex, int length, out int codePoint)
    {
        codePoint = 0;
        if (startIndex + length > value.Length)
        {
            return false;
        }

        var hexValue = value.Substring(startIndex, length);
        if (!int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))
        {
            return false;
        }

        return codePoint is >= 0 and <= 0x10FFFF;
    }
}