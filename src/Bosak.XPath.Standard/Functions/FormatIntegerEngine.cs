// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 mei 2026
// PURPOSE              : Formats integers according to XPath 3.1 fn:format-integer picture strings.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Text;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Implements fn:format-integer per XPath and XQuery Functions and Operators 3.1 §4.6.
/// </summary>
internal static class FormatIntegerEngine
{
    public static string Format(EvaluationContext ctx, long value, string picture, string? language)
    {
        // 1. Split picture into primary format token and format modifier
        ParsePicture(picture, out string primaryToken, out string modifier);

        // Empty primary token is always an error
        if (string.IsNullOrEmpty(primaryToken))
            throw new InvalidOperationException("FODF1310");

        // 2. Parse format modifier
        ParseModifier(modifier, out bool ordinal, out string? ordinalSuffix, out bool titleCase);

        // 3. Format based on primary token
        long absValue = Math.Abs(value);
        string result;
        if (TryFormatNamedToken(absValue, primaryToken, out result))
        {
            // Named token handled
        }
        else if (TryFormatCustomSequence(absValue, primaryToken, out result))
        {
            // Custom numbering sequence handled
        }
        else if (HasMandatoryDigits(primaryToken))
        {
            // Token contains at least one mandatory digit.
            // It must be a valid decimal-digit-pattern; any syntax error is FODF1310.
            result = FormatDecimalPattern(absValue, primaryToken);
        }
        else
        {
            // No mandatory digits: check for missing semicolon before modifier
            if (!picture.Contains(';') && TryDetectMissingSemicolon(primaryToken))
                throw new InvalidOperationException("FODF1310");

            // Unsupported numbering sequence: fallback to format token "1"
            result = FormatDecimalPattern(absValue, "1");
        }

        // 4. Apply ordinal transformation
        if (ordinal)
        {
            result = ToOrdinal(result, absValue, ordinalSuffix, language);
        }

        // 5. Apply title case
        if (titleCase)
        {
            result = ToTitleCase(result);
        }

        // 6. Handle negative sign
        if (value < 0)
        {
            result = "-" + result;
        }

        return result;
    }

    private static bool HasMandatoryDigits(string token)
    {
        foreach (int cp in GetCodepoints(token))
        {
            if (IsDecimalDigit(cp, out _, out _))
                return true;
        }
        return false;
    }

    private static bool TryDetectMissingSemicolon(string primaryToken)
    {
        // If token ends with o/t/c and the prefix is a valid token, it's a missing semicolon
        if (primaryToken.Length < 2) return false;
        char last = primaryToken[primaryToken.Length - 1];
        if (last == 'o' || last == 't' || last == 'c')
        {
            string prefix = primaryToken.Substring(0, primaryToken.Length - 1);
            return TryFormatNamedToken(1, prefix, out _)
                || HasMandatoryDigits(prefix);
        }
        // If token contains "o(" and the prefix is valid
        int idx = primaryToken.IndexOf("o(");
        if (idx > 0)
        {
            string prefix = primaryToken.Substring(0, idx);
            return TryFormatNamedToken(1, prefix, out _)
                || HasMandatoryDigits(prefix);
        }
        return false;
    }

    private static void ParsePicture(string picture, out string primaryToken, out string modifier)
    {
        int lastSemi = picture.LastIndexOf(';');
        if (lastSemi >= 0)
        {
            primaryToken = picture.Substring(0, lastSemi);
            modifier = picture.Substring(lastSemi + 1);
        }
        else
        {
            primaryToken = picture;
            modifier = string.Empty;
        }
    }

    private static void ParseModifier(string modifier, out bool ordinal, out string? ordinalSuffix, out bool titleCase)
    {
        ordinal = false;
        ordinalSuffix = null;
        titleCase = false;

        if (string.IsNullOrEmpty(modifier))
            return;

        string remaining = modifier;

        // Check for title case flag
        if (remaining.Contains('t'))
        {
            titleCase = true;
            remaining = remaining.Replace("t", "");
        }

        // Check for cardinal
        if (remaining == "c" || string.IsNullOrEmpty(remaining))
        {
            return;
        }

        // Check for ordinal
        if (remaining == "o")
        {
            ordinal = true;
            return;
        }

        // Check for ordinal with suffix: o(-suffix)
        if (remaining.StartsWith("o(") && remaining.EndsWith(")"))
        {
            // Validate parentheses are balanced
            if (remaining.Count(c => c == '(') != remaining.Count(c => c == ')'))
                throw new InvalidOperationException("FODF1310");

            ordinal = true;
            ordinalSuffix = remaining.Substring(2, remaining.Length - 3);
            return;
        }

        // Invalid modifier
        throw new InvalidOperationException("FODF1310");
    }

    private static bool TryFormatNamedToken(long value, string primaryToken, out string result)
    {
        result = primaryToken switch
        {
            "a" => ToAlphabetic(value, false),
            "A" => ToAlphabetic(value, true),
            "i" => ToRoman(value, false),
            "I" => ToRoman(value, true),
            "w" => ToWords(value, false),
            "W" => ToWords(value, true),
            "Ww" => ToWordsTitle(value),
            _ => null!
        };
        return result is not null;
    }

    private static bool TryFormatCustomSequence(long value, string primaryToken, out string result)
    {
        result = string.Empty;
        if (string.IsNullOrEmpty(primaryToken) || value <= 0)
            return false;

        var codepoints = GetCodepoints(primaryToken).ToList();
        if (codepoints.Count != 1)
            return false;

        int cp = codepoints[0];

        // Circled digits: U+2460-U+2473 (1-20)
        if (cp == 0x2460)
        {
            if (value > 20) return false;
            result = char.ConvertFromUtf32((int)(0x2460 + value - 1));
            return true;
        }

        // Parenthesized digits: U+2474-U+2487 (1-20)
        if (cp == 0x2474)
        {
            if (value > 20) return false;
            result = char.ConvertFromUtf32((int)(0x2474 + value - 1));
            return true;
        }

        // Digits with full stop: U+2488-U+249B (1-20)
        if (cp == 0x2488)
        {
            if (value > 20) return false;
            result = char.ConvertFromUtf32((int)(0x2488 + value - 1));
            return true;
        }

        // Greek uppercase letters: U+0391-U+03A1 (24 letters, excluding U+03A2)
        if (cp == 0x0391)
        {
            result = ToGreekAlphabetic(value, true);
            return true;
        }

        // Greek lowercase letters: U+03B1-U+03C9 (24 letters, excluding U+03C2)
        if (cp == 0x03B1)
        {
            result = ToGreekAlphabetic(value, false);
            return true;
        }

        return false;
    }

    private static string ToGreekAlphabetic(long n, bool upper)
    {
        if (n <= 0) return n.ToString(CultureInfo.InvariantCulture);
        // Greek alphabet has 24 letters
        // Uppercase: U+0391-U+03A1 (skip U+03A2)
        // Lowercase: U+03B1-U+03C9 (skip U+03C2)
        int[] upperCodes = { 0x0391, 0x0392, 0x0393, 0x0394, 0x0395, 0x0396, 0x0397, 0x0398, 0x0399, 0x039A, 0x039B, 0x039C, 0x039D, 0x039E, 0x039F, 0x03A0, 0x03A1, 0x03A3, 0x03A4, 0x03A5, 0x03A6, 0x03A7, 0x03A8, 0x03A9 };
        int[] lowerCodes = { 0x03B1, 0x03B2, 0x03B3, 0x03B4, 0x03B5, 0x03B6, 0x03B7, 0x03B8, 0x03B9, 0x03BA, 0x03BB, 0x03BC, 0x03BD, 0x03BE, 0x03BF, 0x03C0, 0x03C1, 0x03C3, 0x03C4, 0x03C5, 0x03C6, 0x03C7, 0x03C8, 0x03C9 };
        var codes = upper ? upperCodes : lowerCodes;
        var sb = new StringBuilder();
        while (n > 0)
        {
            n--;
            sb.Insert(0, char.ConvertFromUtf32(codes[n % 24]));
            n /= 24;
        }
        return sb.ToString();
    }

    private static bool TryFormatDecimalPattern(long value, string primaryToken, out string result)
    {
        result = string.Empty;
        try
        {
            result = FormatDecimalPattern(value, primaryToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatDecimalPattern(long value, string primaryToken)
    {
        if (string.IsNullOrEmpty(primaryToken))
            throw new InvalidOperationException("FODF1310");

        // Check for decimal separator (for default decimal format, it's '.')
        // TODO: Use actual decimal format from context
        if (primaryToken.Contains('.'))
            throw new InvalidOperationException("FODF1310");

        // Parse the token using codepoints to handle astral plane characters
        var separators = new List<(int Position, int Codepoint)>();
        bool sawHash = false;
        bool hasMandatory = false;
        int? zeroDigitCodepoint = null;
        int totalDigits = 0;

        var codepoints = GetCodepoints(primaryToken).ToList();

        for (int i = codepoints.Count - 1; i >= 0; i--)
        {
            int cp = codepoints[i];
            if (cp == '#')
            {
                sawHash = true;
                totalDigits++;
            }
            else if (IsDecimalDigit(cp, out int digitValue, out int zeroCp))
            {
                if (sawHash)
                    throw new InvalidOperationException("FODF1310"); // mandatory digit after optional digit (from right)
                hasMandatory = true;
                if (zeroDigitCodepoint.HasValue)
                {
                    if (zeroCp != zeroDigitCodepoint.Value)
                        throw new InvalidOperationException("FODF1310"); // mixed families
                }
                else
                {
                    zeroDigitCodepoint = zeroCp;
                }
                totalDigits++;
            }
            else if (IsGroupingSeparator(cp))
            {
                if (i == 0 || i == codepoints.Count - 1)
                    throw new InvalidOperationException("FODF1310"); // at start or end
                // Check adjacent grouping separators (optional-digit-sign # is not a grouping separator)
                if (i > 0 && codepoints[i - 1] != '#' && IsGroupingSeparator(codepoints[i - 1]))
                    throw new InvalidOperationException("FODF1310");
                separators.Add((totalDigits, cp)); // position is number of digits to the right
            }
            else
            {
                throw new InvalidOperationException("FODF1310"); // invalid character
            }
        }

        if (!hasMandatory)
            throw new InvalidOperationException("FODF1310"); // no mandatory digit

        // Build grouping separator template
        var template = BuildGroupingTemplate(separators, totalDigits);

        // Format the number
        long absValue = Math.Abs(value);
        string s1 = absValue.ToString(CultureInfo.InvariantCulture);
        int minWidth = codepoints.Count(cp => IsDecimalDigit(cp, out _, out _));
        
        // Pad with zeroes
        string s2 = s1.PadLeft(minWidth, '0');

        // Map digits to selected family
        string s3 = zeroDigitCodepoint.HasValue && zeroDigitCodepoint.Value != '0'
            ? MapDigits(s2, zeroDigitCodepoint.Value)
            : s2;

        // Insert grouping separators
        string s4 = InsertGroupingSeparators(s3, template);

        return s4;
    }

    private static IEnumerable<int> GetCodepoints(string s)
    {
        for (int i = 0; i < s.Length;)
        {
            int cp = char.ConvertToUtf32(s, i);
            yield return cp;
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
    }

    private static bool IsDecimalDigit(int codepoint, out int digitValue, out int zeroCodepoint)
    {
        // Check for ASCII digits
        if (codepoint >= '0' && codepoint <= '9')
        {
            digitValue = codepoint - '0';
            zeroCodepoint = '0';
            return true;
        }

        // Check for other Unicode decimal digits (Nd category)
        string s = char.ConvertFromUtf32(codepoint);
        var cat = CharUnicodeInfo.GetUnicodeCategory(s, 0);
        if (cat == UnicodeCategory.DecimalDigitNumber)
        {
            digitValue = (int)CharUnicodeInfo.GetDigitValue(s, 0);
            // Find zero digit of this family
            zeroCodepoint = codepoint - digitValue;
            return true;
        }

        digitValue = -1;
        zeroCodepoint = 0;
        return false;
    }

    private static bool IsGroupingSeparator(int codepoint)
    {
        string s = char.ConvertFromUtf32(codepoint);
        var cat = CharUnicodeInfo.GetUnicodeCategory(s, 0);
        // Non-alphanumeric: not Nd, Nl, No, Lu, Ll, Lt, Lm, Lo
        return cat != UnicodeCategory.DecimalDigitNumber
            && cat != UnicodeCategory.LetterNumber
            && cat != UnicodeCategory.OtherNumber
            && cat != UnicodeCategory.UppercaseLetter
            && cat != UnicodeCategory.LowercaseLetter
            && cat != UnicodeCategory.TitlecaseLetter
            && cat != UnicodeCategory.ModifierLetter
            && cat != UnicodeCategory.OtherLetter;
    }

    private static List<(int Position, int Codepoint)> BuildGroupingTemplate(List<(int Position, int Codepoint)> separators, int totalDigits)
    {
        if (separators.Count == 0)
            return new List<(int, int)>();

        // Check if regular
        bool sameChar = separators.All(s => s.Codepoint == separators[0].Codepoint);
        if (sameChar)
        {
            var positions = separators.Select(s => s.Position).OrderBy(p => p).ToList();
            // Find G: the smallest position
            int g = positions[0];
            // Check if all positions are multiples of g
            bool allMultiples = positions.All(p => p % g == 0);
            // Check if every multiple of g less than totalDigits is present
            bool allPresent = true;
            for (int m = g; m < totalDigits; m += g)
            {
                if (!positions.Contains(m))
                {
                    allPresent = false;
                    break;
                }
            }

            if (allMultiples && allPresent)
            {
                // Regular grouping - infinite template
                var template = new List<(int, int)>();
                int cp = separators[0].Codepoint;
                for (int n = 1; n * g < 1000; n++) // practical limit
                {
                    template.Add((n * g, cp));
                }
                return template;
            }
        }

        // Non-regular - return explicit separators
        return separators.OrderBy(s => s.Position).ToList();
    }

    private static string MapDigits(string digits, int zeroCodepoint)
    {
        var sb = new StringBuilder(digits.Length * 2);
        foreach (char c in digits)
        {
            if (c >= '0' && c <= '9')
            {
                int digitValue = c - '0';
                int targetCp = zeroCodepoint + digitValue;
                sb.Append(char.ConvertFromUtf32(targetCp));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static string InsertGroupingSeparators(string digits, List<(int Position, int Codepoint)> template)
    {
        if (template.Count == 0)
            return digits;

        // Count actual digit characters (not surrogate pairs)
        int digitCount = CountCodepoints(digits);

        // Build a list of insertion points
        var insertions = new List<(int FromRight, int Codepoint)>();
        foreach (var (pos, cp) in template)
        {
            if (pos < digitCount)
            {
                insertions.Add((pos, cp));
            }
        }

        // Sort by position from right, descending
        insertions = insertions.OrderByDescending(i => i.FromRight).ToList();

        var sb = new StringBuilder(digits.Length + insertions.Count * 2);
        int currentDigitFromRight = 0;
        
        // Iterate through digits from right to left
        for (int i = digits.Length - 1; i >= 0;)
        {
            // Get the current codepoint
            int cp;
            if (i > 0 && char.IsLowSurrogate(digits[i]) && char.IsHighSurrogate(digits[i - 1]))
            {
                cp = char.ConvertToUtf32(digits[i - 1], digits[i]);
                sb.Insert(0, char.ConvertFromUtf32(cp));
                i -= 2;
            }
            else
            {
                cp = digits[i];
                sb.Insert(0, (char)cp);
                i--;
            }
            
            currentDigitFromRight++;
            
            // Check if we need to insert a separator after this digit
            foreach (var (pos, sepCp) in insertions)
            {
                if (pos == currentDigitFromRight)
                {
                    sb.Insert(0, char.ConvertFromUtf32(sepCp));
                    break;
                }
            }
        }

        return sb.ToString();
    }

    private static int CountCodepoints(string s)
    {
        int count = 0;
        for (int i = 0; i < s.Length;)
        {
            count++;
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
        return count;
    }

    private static string ToOrdinal(string result, long value, string? suffix, string? language)
    {
        // Simple heuristic: if result contains only digits (and separators), treat as digits
        bool isDigits = result.All(c => char.IsDigit(c) || IsGroupingSeparatorChar(c));
        
        if (isDigits)
        {
            // For digits, append English ordinal suffix
            // Remove grouping separators for suffix determination
            string clean = new string(result.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(clean) && long.TryParse(clean, out long num))
            {
                return result + GetEnglishOrdinalSuffix(num);
            }
            return result + "th";
        }
        else
        {
            // For words, convert to ordinal word
            return ToOrdinalWords(result, value, language);
        }
    }

    private static bool IsGroupingSeparatorChar(char c)
    {
        var cat = CharUnicodeInfo.GetUnicodeCategory(c);
        return cat != UnicodeCategory.DecimalDigitNumber
            && cat != UnicodeCategory.LetterNumber
            && cat != UnicodeCategory.OtherNumber
            && cat != UnicodeCategory.UppercaseLetter
            && cat != UnicodeCategory.LowercaseLetter
            && cat != UnicodeCategory.TitlecaseLetter
            && cat != UnicodeCategory.ModifierLetter
            && cat != UnicodeCategory.OtherLetter;
    }

    private static string GetEnglishOrdinalSuffix(long n)
    {
        long lastTwo = Math.Abs(n) % 100;
        if (lastTwo >= 11 && lastTwo <= 13)
            return "th";
        
        long lastDigit = lastTwo % 10;
        return lastDigit switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    private static string ToOrdinalWords(string cardinalWord, long value, string? language)
    {
        string lower = cardinalWord.ToLowerInvariant();
        
        // Special cases for simple numbers
        string? ordinal = lower switch
        {
            "one" => "first",
            "two" => "second",
            "three" => "third",
            "five" => "fifth",
            "eight" => "eighth",
            "nine" => "ninth",
            "twelve" => "twelfth",
            "twenty" => "twentieth",
            "thirty" => "thirtieth",
            "forty" => "fortieth",
            "fifty" => "fiftieth",
            "sixty" => "sixtieth",
            "seventy" => "seventieth",
            "eighty" => "eightieth",
            "ninety" => "ninetieth",
            "zero" => "zeroth",
            _ => null
        };

        if (ordinal is not null)
        {
            return MatchCase(cardinalWord, ordinal);
        }

        // If it ends with "y", replace with "ieth"
        if (lower.EndsWith("y"))
            return cardinalWord.Substring(0, cardinalWord.Length - 1) + "ieth";

        // If it contains compound number words with spaces/hyphens, replace only the last word
        if ((lower.Contains(' ') || lower.Contains('-')) &&
            (lower.Contains("hundred") || lower.Contains("thousand") || lower.Contains("million") || lower.Contains("billion")))
        {
            return ReplaceLastWordWithOrdinal(cardinalWord, value, language);
        }

        // Default: append "th"
        return cardinalWord + "th";
    }

    private static string MatchCase(string original, string replacement)
    {
        // Preserve case of the original word
        if (original.All(char.IsUpper))
            return replacement.ToUpperInvariant();
        if (char.IsUpper(original[0]))
            return char.ToUpperInvariant(replacement[0]) + replacement.Substring(1);
        return replacement;
    }

    private static string ReplaceLastWordWithOrdinal(string original, long value, string? language)
    {
        int lastSpace = original.LastIndexOf(' ');
        int lastHyphen = original.LastIndexOf('-');
        int splitAt = Math.Max(lastSpace, lastHyphen);
        
        if (splitAt < 0)
        {
            return original + "th";
        }
        
        string prefix = original.Substring(0, splitAt + 1);
        string lastWord = original.Substring(splitAt + 1);
        string ordinalLast = ToOrdinalWords(lastWord, value, language);
        return prefix + ordinalLast;
    }

    private static string ToTitleCase(string s)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
    }

    private static string ToAlphabetic(long n, bool upper)
    {
        if (n <= 0) return n.ToString(CultureInfo.InvariantCulture);
        StringBuilder sb = new();
        while (n > 0)
        {
            n--;
            char c = (char)(n % 26 + (upper ? 'A' : 'a'));
            sb.Insert(0, c);
            n /= 26;
        }
        return sb.ToString();
    }

    private static string ToRoman(long n, bool upper)
    {
        if (n <= 0 || n > 3999) return n.ToString(CultureInfo.InvariantCulture);
        var values = new[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        var symbols = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        StringBuilder sb = new();
        for (int i = 0; i < values.Length; i++)
        {
            while (n >= values[i])
            {
                sb.Append(symbols[i]);
                n -= values[i];
            }
        }
        return upper ? sb.ToString() : sb.ToString().ToLowerInvariant();
    }

    private static string ToWords(long n, bool upper)
    {
        string s = NumberToWords(n);
        return upper ? s.ToUpperInvariant() : s.ToLowerInvariant();
    }

    private static string ToWordsTitle(long n)
    {
        string s = NumberToWords(n);
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
    }

    private static string NumberToWords(long n)
    {
        if (n == 0) return "zero";
        if (n < 0) return "minus " + NumberToWords(-n);
        if (n <= 19) return new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" }[n - 1];
        if (n < 100)
        {
            var tens = new[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string r = tens[n / 10 - 2];
            if (n % 10 > 0) r += "-" + NumberToWords(n % 10);
            return r;
        }
        if (n < 1000)
        {
            string r = NumberToWords(n / 100) + " hundred";
            if (n % 100 > 0) r += " and " + NumberToWords(n % 100);
            return r;
        }
        if (n < 1000000)
        {
            string r = NumberToWords(n / 1000) + " thousand";
            if (n % 1000 > 0) r += " " + NumberToWords(n % 1000);
            return r;
        }
        if (n < 1000000000)
        {
            string r = NumberToWords(n / 1000000) + " million";
            if (n % 1000000 > 0) r += " " + NumberToWords(n % 1000000);
            return r;
        }
        string rr = NumberToWords(n / 1000000000) + " billion";
        if (n % 1000000000 > 0) rr += " " + NumberToWords(n % 1000000000);
        return rr;
    }
}
