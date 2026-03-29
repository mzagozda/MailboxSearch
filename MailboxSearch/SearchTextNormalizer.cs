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

        string decoded = DecodeUnicodeEscapes(value);
        return decoded.Normalize(NormalizationForm.FormC);
    }

    private static string DecodeUnicodeEscapes(string value)
    {
        StringBuilder builder = new StringBuilder(value.Length);

        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] != '\\' || index + 1 >= value.Length)
            {
                builder.Append(value[index]);
                continue;
            }

            char marker = value[index + 1];
            if (marker == 'u' && TryParseHexCodePoint(value, index + 2, 4, out int unicodeCodePoint))
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

        string hexValue = value.Substring(startIndex, length);
        if (!int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))
        {
            return false;
        }

        if (codePoint is < 0 or > 0x10FFFF || codePoint is >= 0xD800 and <= 0xDFFF)
        {
            throw new InvalidDataException($"Invalid Unicode escape sequence '\\u{hexValue}'.");
        }

        return true;
    }
}