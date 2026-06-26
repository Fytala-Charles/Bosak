// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 mei 2026
// PURPOSE              : Picture-string parser and formatter for fn:format-date, fn:format-time, fn:format-dateTime.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Full rewrite: digit families, grouping separators, fractional seconds, AM/PM case       |
//                      | Charles Korthout | 0.3   | 13-06-2026     | AM/PM marker width modifier truncates only; no zero padding                               |
//                      | Charles Korthout | 0.4   | 13-06-2026     | Name width modifiers, default component widths, ordinal suffixes, fallback lang/cal   |
//                      | Charles Korthout | 0.5   | 13-06-2026     | English number words, era-aware negative years, ordinal-year width handling          |
//                      | Charles Korthout | 0.6   | 26-06-2026     | Bracket escapes, default widths, roman/alpha/timezone/week-of-month fixes             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

internal enum DateTimeComponents { Date, Time, DateTime }

internal static class FormatDateTimeEngine
{
    public static string Format(XPathDateTime value, string picture, string? language, string? calendar, string? place, DateTimeComponents components)
    {
        bool hasEra = ContainsEraMarker(picture);
        var sb = new StringBuilder();
        int pos = 0;

        while (pos < picture.Length)
        {
            int bracket = picture.IndexOf('[', pos);
            if (bracket < 0)
            {
                sb.Append(EscapeLiteral(picture[pos..]));
                break;
            }

            if (bracket > pos)
            {
                sb.Append(EscapeLiteral(picture[pos..bracket]));
            }

            // An unescaped '[[' is a literal '['.
            if (bracket + 1 < picture.Length && picture[bracket + 1] == '[')
            {
                sb.Append('[');
                pos = bracket + 2;
                continue;
            }

            int close = FindClosingBracket(picture, bracket);
            if (close < 0)
                throw FormatError("FOFD1340");

            string marker = picture[(bracket + 1)..close];
            sb.Append(FormatMarker(value, marker, components, language, calendar, hasEra));
            pos = close + 1;
        }

        return sb.ToString();
    }

    private static bool ContainsEraMarker(string picture)
    {
        int pos = 0;
        while (pos < picture.Length)
        {
            int bracket = picture.IndexOf('[', pos);
            if (bracket < 0)
                break;
            if (bracket + 1 < picture.Length && picture[bracket + 1] == '[')
            {
                pos = bracket + 2;
                continue;
            }
            int close = picture.IndexOf(']', bracket + 1);
            if (close < 0)
                break;
            if (bracket + 1 < close && picture[bracket + 1] == 'E')
                return true;
            pos = close + 1;
        }
        return false;
    }

    private static int FindClosingBracket(string picture, int openPos)
    {
        for (int i = openPos + 1; i < picture.Length; i++)
        {
            if (picture[i] == ']')
                return i;
        }
        return -1;
    }

    private static string EscapeLiteral(string text)
    {
        return text.Replace("~~", "~").Replace("[[", "[").Replace("]]", "]");
    }

    private static Exception FormatError(string code)
    {
        return new InvalidOperationException(code);
    }

    private static string FormatMarker(XPathDateTime value, string marker, DateTimeComponents components, string? language, string? calendar, bool hasEra)
    {
        // Remove all whitespace from the marker content (whitespace within a variable marker is ignored)
        marker = new string(marker.Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (marker.Length == 0)
            throw FormatError("FOFD1340");

        char component = marker[0];
        string rest = marker.Length > 1 ? marker[1..] : string.Empty;

        // Parse width modifier - the width modifier comma is the LAST comma,
        // since grouping separators can also be commas within the presentation.
        string presentation = rest;
        string? widthSpec = null;
        int comma = rest.LastIndexOf(',');
        if (comma >= 0 && IsWidthModifier(rest[(comma + 1)..]))
        {
            presentation = rest[..comma].TrimEnd();
            widthSpec = rest[(comma + 1)..].TrimStart();
        }

        int minWidth = 1;
        int maxWidth = int.MaxValue;
        if (widthSpec is not null)
        {
            ParseWidth(widthSpec, out minWidth, out maxWidth);
        }

        // Component-specific default minimum widths when no explicit width or digit pattern is given.
        bool hasDigitPattern = ContainsDecimalDigit(presentation);
        if (widthSpec is null && !hasDigitPattern)
        {
            int defaultMin = GetDefaultMinWidth(component);
            if (component == 'Y' && hasEra)
                defaultMin = 1;
            minWidth = Math.Max(minWidth, defaultMin);
        }

        bool languageFallback = !IsLanguageSupported(language);
        bool calendarFallback = !IsCalendarSupported(calendar);

        // Validate component is available for the value type
        ValidateComponentAvailable(component, components);

        string result = component switch
        {
            'Y' => FormatYear(value, presentation, minWidth, maxWidth, hasEra),
            'M' => FormatMonth(value, presentation, minWidth, maxWidth),
            'D' => FormatDay(value, presentation, minWidth, maxWidth),
            'd' => FormatDayOfYear(value, presentation, minWidth, maxWidth),
            'F' => FormatDayOfWeek(value, presentation, minWidth, maxWidth),
            'W' => FormatWeekOfYear(value, presentation, minWidth, maxWidth),
            'w' => FormatWeekOfMonth(value, presentation, minWidth, maxWidth),
            'H' => FormatHour24(value, presentation, minWidth, maxWidth),
            'h' => FormatHour12(value, presentation, minWidth, maxWidth),
            'm' => FormatMinute(value, presentation, minWidth, maxWidth),
            's' => FormatSecond(value, presentation, minWidth, maxWidth),
            'f' => FormatFractionalSeconds(value, presentation, minWidth, maxWidth),
            'P' => FormatAmPm(value, presentation, minWidth, maxWidth),
            'Z' => FormatTimezone(value, presentation, minWidth, maxWidth),
            'z' => FormatTimezoneGmt(value, presentation, minWidth, maxWidth),
            'C' => "ISO",
            'E' => value.Year > 0 ? "AD" : "BC",
            _ => throw FormatError("FOFD1340")
        };

        if (languageFallback)
            result = $"[Language: en]{result}";
        else if (calendarFallback)
            result = $"[Calendar: AD]{result}";

        return result;
    }

    private static int GetDefaultMinWidth(char component)
        => component switch
        {
            'Y' => 4,
            'd' => 3,
            'm' => 2,
            's' => 2,
            _ => 1
        };

    private static bool ContainsDecimalDigit(string presentation)
    {
        foreach (var rune in presentation.EnumerateRunes())
            if (Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber)
                return true;
        return false;
    }

    private static bool IsLanguageSupported(string? language)
        => string.IsNullOrEmpty(language) ||
           language.StartsWith("en", StringComparison.OrdinalIgnoreCase);

    private static bool IsCalendarSupported(string? calendar)
        => string.IsNullOrEmpty(calendar) ||
           calendar.Equals("AD", StringComparison.OrdinalIgnoreCase) ||
           calendar.Equals("Gregorian", StringComparison.OrdinalIgnoreCase) ||
           calendar.Equals("ISO", StringComparison.OrdinalIgnoreCase) ||
           calendar.Equals("ISO8601", StringComparison.OrdinalIgnoreCase);

    private static void ValidateComponentAvailable(char component, DateTimeComponents components)
    {
        bool isDateComponent = component is 'Y' or 'M' or 'D' or 'd' or 'F' or 'W' or 'w' or 'C' or 'E';
        bool isTimeComponent = component is 'H' or 'h' or 'm' or 's' or 'f' or 'P';
        // Note: Z and z (timezone) are available for all date/time value types

        if (components == DateTimeComponents.Date && isTimeComponent)
            throw FormatError("FOFD1350");
        if (components == DateTimeComponents.Time && isDateComponent)
            throw FormatError("FOFD1350");
    }

    private static bool IsWidthModifier(string text)
    {
        // Width modifier syntax: * | number | number-* | *-number | number-number
        if (string.IsNullOrEmpty(text))
            return false;
        var parts = text.Split('-', 2);
        if (parts.Length == 2)
        {
            return (parts[0] == "*" || int.TryParse(parts[0], out _))
                && (parts[1] == "*" || int.TryParse(parts[1], out _));
        }
        return text == "*" || int.TryParse(text, out _);
    }

    private static void ParseWidth(string spec, out int min, out int max)
    {
        min = 1;
        max = int.MaxValue;
        var parts = spec.Split('-', 2);
        if (parts.Length == 2)
        {
            if (parts[0] != "*" && int.TryParse(parts[0], out var mn))
            {
                if (mn < 1) throw FormatError("FOFD1340");
                min = mn;
            }
            if (parts[1] != "*" && int.TryParse(parts[1], out var mx))
            {
                if (mx < 1) throw FormatError("FOFD1340");
                max = mx;
            }
        }
        else if (parts.Length == 1)
        {
            if (parts[0] != "*" && int.TryParse(parts[0], out var w))
            {
                if (w < 1) throw FormatError("FOFD1340");
                min = max = w;
            }
        }
        if (min > max)
            throw FormatError("FOFD1340");
    }

    // ------------------------------------------------------------------
    // Digit family helpers
    // ------------------------------------------------------------------

    private static Rune? DetectZeroDigit(string presentation)
    {
        for (int i = 0; i < presentation.Length;)
        {
            if (Rune.TryGetRuneAt(presentation, i, out var rune)
                && Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber)
            {
                int digitValue = CharUnicodeInfo.GetDigitValue(presentation, i);
                if (digitValue >= 0 && digitValue <= 9)
                    return new Rune(rune.Value - digitValue);
            }
            i += rune.Utf16SequenceLength;
        }
        return null;
    }

    private static Rune GetZeroDigit(Rune digit)
    {
        int digitValue = CharUnicodeInfo.GetDigitValue(digit.ToString(), 0);
        if (digitValue >= 0 && digitValue <= 9)
            return new Rune(digit.Value - digitValue);
        return digit;
    }

    private static string MapDigit(char asciiDigit, Rune zeroDigit)
    {
        if (zeroDigit.Value == '0')
            return asciiDigit.ToString();
        int val = asciiDigit - '0';
        if (val < 0 || val > 9)
            return asciiDigit.ToString();
        return new Rune(zeroDigit.Value + val).ToString();
    }

    private static string MapDigits(string asciiDigits, Rune zeroDigit)
    {
        if (zeroDigit.Value == '0')
            return asciiDigits;
        var sb = new StringBuilder(asciiDigits.Length);
        foreach (char c in asciiDigits)
        {
            sb.Append(MapDigit(c, zeroDigit));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses the presentation into digit info. Validates # placement and digit family consistency.
    /// </summary>
    private static DigitInfo ParseDigitPresentation(string presentation, bool isFractional)
    {
        Rune zeroDigit = new('0');
        int mandatoryCount = 0;
        int optionalCount = 0;
        bool seenMandatory = false;
        Rune? firstZeroDigit = null;

        foreach (var rune in presentation.EnumerateRunes())
        {
            if (rune.Value == '#')
            {
                if (isFractional)
                {
                    if (!seenMandatory)
                        throw FormatError("FOFD1340");
                    optionalCount++;
                }
                else
                {
                    if (seenMandatory)
                        throw FormatError("FOFD1340");
                    optionalCount++;
                }
            }
            else if (Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber)
            {
                Rune zd = GetZeroDigit(rune);
                if (firstZeroDigit is null)
                    firstZeroDigit = zd;
                else if (firstZeroDigit.Value != zd)
                    throw FormatError("FOFD1340");
                zeroDigit = zd;
                seenMandatory = true;
                mandatoryCount++;
            }
            // Non-digit, non-# characters are grouping separators or literals
        }

        return new DigitInfo(zeroDigit, mandatoryCount, optionalCount);
    }

    private readonly record struct DigitInfo(Rune ZeroDigit, int Mandatory, int Optional)
    {
        public int TotalPositions => Mandatory + Optional;
    }

    // ------------------------------------------------------------------
    // Integer formatting
    // ------------------------------------------------------------------

    private static string FormatInteger(long value, string presentation, int minWidth, int maxWidth, bool allowTruncate = true)
    {
        if (TryFormatWords(value, presentation, out string? wordsResult))
            return wordsResult;

        // Alphabetic numbering (A, B, ... Z, AA, ...). Zero is formatted as "0".
        if (presentation == "A" || presentation == "a")
        {
            if (value == 0)
                return "0";
            bool upper = presentation == "A";
            var alphaSb = new StringBuilder();
            long v = value;
            while (v > 0)
            {
                v--;
                char c = (char)(v % 26 + (upper ? 'A' : 'a'));
                alphaSb.Insert(0, c);
                v /= 26;
            }
            return alphaSb.ToString();
        }

        // Roman numerals (i/I). Values outside the representable range fall back to digits.
        if (presentation == "i" || presentation == "I")
        {
            if (value is >= 1 and <= 3999)
            {
                int nonNumericMin = maxWidth == int.MaxValue ? 1 : minWidth;
                return ApplyAlphabeticWidth(ToRoman(value, upper: presentation == "I"), nonNumericMin, int.MaxValue);
            }
            return value.ToString(CultureInfo.InvariantCulture);
        }

        bool ordinal = presentation.EndsWith("o") || presentation.EndsWith("O");
        string digitPresentation = ordinal ? presentation[..^1] : presentation;

        var info = ParseDigitPresentation(digitPresentation, isFractional: false);
        int totalPositions = info.TotalPositions;
        if (totalPositions == 0)
        {
            // No digit characters in presentation - default to the value as string
            string result = value.ToString(CultureInfo.InvariantCulture);
            result = ApplyWidth(result, minWidth, maxWidth, info.ZeroDigit);
            if (ordinal && value is >= int.MinValue and <= int.MaxValue)
                result += GetOrdinalSuffix((int)value);
            return result;
        }

        // Determine effective min/max digits
        int effectiveMax = maxWidth == int.MaxValue ? totalPositions : maxWidth;
        int effectiveMin = Math.Max(info.Mandatory, minWidth);

        // Format the absolute value as ASCII digits
        string digits = Math.Abs(value).ToString(CultureInfo.InvariantCulture);
        int valueDigits = digits.Length;

        // Truncate or pad
        if (allowTruncate && valueDigits > effectiveMax)
        {
            // Truncate from the left (only for year component)
            digits = digits[(valueDigits - effectiveMax)..];
        }
        else if (valueDigits < effectiveMin)
        {
            digits = digits.PadLeft(effectiveMin, '0');
        }

        // Insert grouping separators from the right
        var sb = new StringBuilder();
        sb.Append(digits);

        var groupingSeps = ExtractGroupingSeparators(presentation, fromRight: true);
        foreach (var (pos, sep) in groupingSeps.OrderByDescending(g => g.posFromRight))
        {
            if (pos > 0 && pos < sb.Length)
            {
                sb.Insert(sb.Length - pos, sep);
            }
        }

        string result2 = sb.ToString();
        if (value < 0)
            result2 = "-" + result2;

        // Apply width modifier to final string
        result2 = ApplyWidth(result2, minWidth, maxWidth, info.ZeroDigit);
        result2 = MapDigits(result2, info.ZeroDigit);
        if (ordinal && value is >= int.MinValue and <= int.MaxValue)
            result2 += GetOrdinalSuffix((int)value);
        return result2;
    }

    private static string GetOrdinalSuffix(int value)
    {
        int n = Math.Abs(value);
        int lastTwo = n % 100;
        if (lastTwo >= 11 && lastTwo <= 13)
            return "th";
        return (n % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    // ------------------------------------------------------------------
    // Fractional seconds formatting
    // ------------------------------------------------------------------

    private static string FormatFractionalSeconds(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        // Extract fractional seconds string
        string frac = value.Millisecond.ToString(CultureInfo.InvariantCulture).TrimEnd('0');
        if (frac.Length == 0) frac = "0";

        var info = ParseDigitPresentation(presentation, isFractional: true);
        int totalPositions = info.TotalPositions;

        if (totalPositions == 0)
        {
            // No digit characters - use width modifier only
            string fracResult = frac;
            fracResult = ApplyWidthFractional(fracResult, minWidth, maxWidth, info.ZeroDigit);
            return MapDigits(fracResult, info.ZeroDigit);
        }

        // Special case: single digit in presentation means "show all available fractional digits"
        if (totalPositions == 1 && info.Mandatory == 1 && info.Optional == 0)
        {
            string fracResult = frac;
            fracResult = ApplyWidthFractional(fracResult, minWidth, maxWidth, info.ZeroDigit);
            return MapDigits(fracResult, info.ZeroDigit);
        }

        // For fractional seconds with only fixed digits (>1), ignore width max
        int effectiveMax = (info.Optional == 0 && totalPositions > 1)
            ? totalPositions
            : (maxWidth == int.MaxValue ? totalPositions : maxWidth);
        int effectiveMin = Math.Max(info.Mandatory, minWidth);

        int inputLen = frac.Length;
        string digits;
        if (inputLen > effectiveMax)
        {
            digits = frac[..effectiveMax];
        }
        else if (inputLen < effectiveMin)
        {
            digits = frac.PadRight(effectiveMin, '0');
        }
        else
        {
            digits = frac;
        }

        // Build output: map digits to correct family, interleaving with separators from presentation
        var mappedDigits = new StringBuilder();
        foreach (char c in digits)
        {
            mappedDigits.Append(MapDigit(c, info.ZeroDigit));
        }

        // Extract separator pattern from the presentation (positions between digits, from left)
        var separators = new List<(int afterDigit, string sep)>();
        int digitPos = 0;
        string currentSep = "";
        foreach (char c in presentation)
        {
            if (c == '#' || char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
            {
                if (currentSep.Length > 0)
                {
                    separators.Add((digitPos, currentSep));
                    currentSep = "";
                }
                digitPos++;
            }
            else
            {
                currentSep += c;
            }
        }

        // Build result by inserting separators after the specified digits
        var sb = new StringBuilder();
        for (int i = 0; i < mappedDigits.Length; i++)
        {
            sb.Append(mappedDigits[i]);
            foreach (var (afterDigit, sep) in separators)
            {
                if (afterDigit == i + 1)
                {
                    sb.Append(sep);
                }
            }
        }

        return sb.ToString();
    }

    // ------------------------------------------------------------------
    // Grouping separator extraction
    // ------------------------------------------------------------------

    private static List<(int posFromRight, string sep)> ExtractGroupingSeparators(string presentation, bool fromRight)
    {
        var groupingSeps = new List<(int posFromRight, string sep)>();
        if (fromRight)
        {
            int digitPosFromRight = 0;
            string currentSep = "";
            for (int i = presentation.Length - 1; i >= 0; i--)
            {
                char c = presentation[i];
                if (c == '#' || char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
                {
                    if (currentSep.Length > 0)
                    {
                        groupingSeps.Add((digitPosFromRight, currentSep));
                        currentSep = "";
                    }
                    digitPosFromRight++;
                }
                else
                {
                    currentSep = c + currentSep;
                }
            }
        }
        else
        {
            int digitPosFromLeft = 0;
            string currentSep = "";
            for (int i = 0; i < presentation.Length; i++)
            {
                char c = presentation[i];
                if (c == '#' || char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
                {
                    if (currentSep.Length > 0)
                    {
                        groupingSeps.Add((digitPosFromLeft, currentSep));
                        currentSep = "";
                    }
                    digitPosFromLeft++;
                }
                else
                {
                    currentSep += c;
                }
            }
        }
        return groupingSeps;
    }

    private static string ApplyWidth(string value, int minWidth, int maxWidth, char zeroDigit)
        => ApplyWidth(value, minWidth, maxWidth, new Rune(zeroDigit));

    private static string ApplyWidth(string value, int minWidth, int maxWidth, Rune zeroDigit)
    {
        if (maxWidth != int.MaxValue && value.Length > maxWidth)
            return value[..maxWidth];
        if (value.Length < minWidth)
        {
            string pad = zeroDigit.Value == '0' ? "0" : zeroDigit.ToString();
            int padCount = minWidth - value.Length;
            var sb = new StringBuilder(minWidth);
            for (int i = 0; i < padCount; i++)
                sb.Append(pad);
            sb.Append(value);
            return sb.ToString();
        }
        return value;
    }

    private static string ApplyWidthFractional(string value, int minWidth, int maxWidth, char zeroDigit)
        => ApplyWidthFractional(value, minWidth, maxWidth, new Rune(zeroDigit));

    private static string ApplyWidthFractional(string value, int minWidth, int maxWidth, Rune zeroDigit)
    {
        if (maxWidth != int.MaxValue && value.Length > maxWidth)
            return value[..maxWidth];
        if (value.Length < minWidth)
        {
            string pad = zeroDigit.Value == '0' ? "0" : zeroDigit.ToString();
            return value.PadRight(minWidth, pad[0]);
        }
        return value;
    }

    private static string ApplyAlphabeticWidth(string value, int minWidth, int maxWidth)
    {
        // Width modifiers on non-numeric presentations only pad to the minimum;
        // they do not truncate content such as roman numerals or words.
        if (value.Length < minWidth)
            return value.PadRight(minWidth);
        return value;
    }

    // ------------------------------------------------------------------
    // Component formatters
    // ------------------------------------------------------------------

    private static string FormatYear(XPathDateTime value, string presentation, int minWidth, int maxWidth, bool hasEra)
    {
        long year = value.Year;

        // Non-numeric presentations (words, roman numerals) ignore the default numeric
        // minimum width; an explicit width modifier (maxWidth constrained) is honoured.
        int nonNumericMin = maxWidth == int.MaxValue ? 1 : minWidth;
        if (TryFormatWords(year, presentation, out string? wordsResult))
            return ApplyAlphabeticWidth(wordsResult, nonNumericMin, int.MaxValue);

        if (presentation == "i")
            return ApplyAlphabeticWidth(ToRoman(year, upper: false), nonNumericMin, int.MaxValue);
        if (presentation == "I")
            return ApplyAlphabeticWidth(ToRoman(year, upper: true), nonNumericMin, int.MaxValue);

        var info = ParseDigitPresentation(presentation, isFractional: false);
        if (info.TotalPositions > 0)
        {
            bool ordinal = presentation.EndsWith("o", StringComparison.OrdinalIgnoreCase)
                        || presentation.EndsWith("O", StringComparison.OrdinalIgnoreCase);
            long displayYear = hasEra && year < 0 ? -year : year;
            return FormatInteger(displayYear, presentation, minWidth, maxWidth, allowTruncate: !ordinal);
        }

        long displayYear2 = hasEra && year < 0 ? -year : year;
        bool negative = year < 0 && !hasEra;
        string digits = displayYear2.ToString(CultureInfo.InvariantCulture).TrimStart('-');

        // Width modifiers on a year truncate the least significant digits, not the most
        // significant ones, and pad with leading zeros when necessary.
        if (digits.Length < minWidth)
            digits = digits.PadLeft(minWidth, '0');
        if (maxWidth != int.MaxValue && digits.Length > maxWidth)
            digits = digits[(digits.Length - maxWidth)..];

        if (negative)
            digits = "-" + digits;
        return digits;
    }

    private static string FormatMonth(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (TryFormatName(value.Month, presentation, GetMonthNames(), GetAbbreviatedMonthNames(), minWidth, maxWidth, out string? nameResult))
            return nameResult;

        return FormatInteger(value.Month, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatDay(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        return FormatInteger(value.Day, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatDayOfYear(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        return FormatInteger(GetDayOfYear(value), presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatDayOfWeek(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        int dow = (int)GetDayOfWeek(value);
        if (dow == 0) dow = 7; // ISO: Monday=1, Sunday=7

        if (TryFormatName(dow, presentation, GetDayNames(), GetAbbreviatedDayNames(), minWidth, maxWidth, out string? nameResult))
            return nameResult;

        return FormatInteger(dow, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatWeekOfYear(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        int week = GetIsoWeekOfYear(value);
        return FormatInteger(week, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatWeekOfMonth(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        // The first week of the month is the ISO week that contains its first Thursday.
        // This avoids negative week numbers around ISO year boundaries.
        var firstOfMonth = new XPathDateTime(value.Year, value.Month, 1, 0, 0, 0, 0, value.TimezoneOffsetMinutes, value.HasTimezone);
        int dowFirst = (int)GetDayOfWeek(firstOfMonth);
        if (dowFirst == 0)
            dowFirst = 7;
        int daysToThursday = (4 - dowFirst + 7) % 7;
        var (y, m, d) = XPathDateTimeHelper.AddDays(firstOfMonth.Year, firstOfMonth.Month, firstOfMonth.Day, daysToThursday);
        var firstThursday = new XPathDateTime(y, m, d, 0, 0, 0, 0, value.TimezoneOffsetMinutes, value.HasTimezone);

        int baseWeek = GetIsoWeekOfYear(firstThursday);
        int week = GetIsoWeekOfYear(value) - baseWeek + 1;
        return FormatInteger(week, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatHour24(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        return FormatInteger(value.Hour, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatHour12(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        int h = value.Hour % 12;
        if (h == 0) h = 12;
        return FormatInteger(h, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatMinute(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        return FormatInteger(value.Minute, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatSecond(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        return FormatInteger(value.Second, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatAmPm(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        string ampm = value.Hour < 12 ? "am" : "pm";

        ampm = presentation switch
        {
            "N" or "NN" or "NNN" => ampm.ToUpperInvariant(),
            "Nn" or "NNn" => char.ToUpperInvariant(ampm[0]) + ampm[1..].ToLowerInvariant(),
            _ => ampm.ToLowerInvariant()
        };

        // Width modifiers on the am/pm marker only truncate; they do not pad with zeros.
        if (maxWidth != int.MaxValue && ampm.Length > maxWidth)
            return ampm[..maxWidth];
        return ampm;
    }

    private static string FormatTimezone(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (!value.HasTimezone)
            return "";

        var offset = TimeSpan.FromMinutes(value.TimezoneOffsetMinutes);
        string sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset < TimeSpan.Zero ? -offset : offset;
        int hours = abs.Hours;
        int minutes = abs.Minutes;

        // Explicit "Z" presentation requests the short "Z" form for a zero offset.
        if (presentation == "Z" || presentation == "z")
        {
            if (offset == TimeSpan.Zero)
                return "Z";
        }

        // Hours only, no leading zeros; include minutes if they are non-zero.
        if (presentation == "0")
        {
            string result = $"{sign}{hours}";
            if (minutes != 0)
                result += $":{minutes:00}";
            return result;
        }

        // Hours only, two digits.
        if (presentation == "00")
            return $"{sign}{hours:00}";

        // Hours and minutes without colon.
        if (presentation == "0000")
            return $"{sign}{hours:00}{minutes:00}";

        // Default is the full ±HH:MM representation.
        return $"{sign}{hours:00}:{minutes:00}";
    }

    private static string FormatTimezoneGmt(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (!value.HasTimezone)
            return "";

        var offset = TimeSpan.FromMinutes(value.TimezoneOffsetMinutes);
        string sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset < TimeSpan.Zero ? -offset : offset;
        int hours = abs.Hours;
        int minutes = abs.Minutes;

        // [z0] => hours only, no leading zero; minutes appended only when non-zero.
        if (presentation == "0")
        {
            string result = $"GMT{sign}{hours}";
            if (minutes != 0)
                result += $":{minutes:00}";
            return result;
        }

        // [z,2-2] (empty presentation, width 2-2) => two-digit hours, omit minutes when zero.
        if (string.IsNullOrEmpty(presentation) && minWidth == 2 && maxWidth == 2)
        {
            string result = $"GMT{sign}{hours:00}";
            if (minutes != 0)
                result += $":{minutes:00}";
            return result;
        }

        // Default [z] => GMT±HH:MM, even for a zero offset.
        return $"GMT{sign}{hours:00}:{minutes:00}";
    }

    // ------------------------------------------------------------------
    // Name formatting helpers
    // ------------------------------------------------------------------

    private static bool TryFormatName(int index1Based, string presentation, string[] names, string[] abbreviatedNames, int minWidth, int maxWidth, [NotNullWhen(true)] out string? result)
    {
        result = null;
        int idx = index1Based - 1;
        if (idx < 0 || idx >= names.Length)
            return false;

        string name = names[idx];

        switch (presentation)
        {
            case "N":
            case "NN":
            case "NNN":
                result = name.ToUpperInvariant();
                break;
            case "n":
            case "nn":
            case "nnn":
                result = name.ToLowerInvariant();
                break;
            case "Nn":
            case "NNn":
                result = char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant();
                break;
        }

        // For day-of-week with F prefix
        if (result == null && presentation.StartsWith("F"))
        {
            string sub = presentation[1..];
            switch (sub)
            {
                case "N":
                case "NN":
                case "NNN":
                    result = name.ToUpperInvariant();
                    break;
                case "n":
                case "nn":
                case "nnn":
                    result = name.ToLowerInvariant();
                    break;
                case "Nn":
                case "NNn":
                    result = char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant();
                    break;
            }
        }

        if (result == null)
            return false;

        // Apply width modifier: prefer a culturally-abbreviated form when it fits,
        // otherwise truncate to maxWidth. Pad only if a minWidth is explicitly set.
        if (maxWidth != int.MaxValue && result.Length > maxWidth)
        {
            string abbrev = abbreviatedNames[idx];
            // Re-apply the same case transformation to the abbreviation.
            result = presentation switch
            {
                "N" or "NN" or "NNN" or "FN" or "FNN" or "FNNN" => abbrev.ToUpperInvariant(),
                "n" or "nn" or "nnn" or "Fn" or "Fnn" or "Fnnn" => abbrev.ToLowerInvariant(),
                _ => char.ToUpperInvariant(abbrev[0]) + abbrev[1..].ToLowerInvariant()
            };

            if (result.Length > maxWidth)
                result = result[..maxWidth];
        }

        if (result.Length < minWidth)
            result = result.PadRight(minWidth);

        return true;
    }

    private static string[] GetMonthNames() => CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
    private static string[] GetAbbreviatedMonthNames() => CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;

    // ISO 8601 ordering: Monday = index 0, Sunday = index 6.
    private static string[] GetDayNames()
    {
        var net = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
        return new[] { net[(int)DayOfWeek.Monday], net[(int)DayOfWeek.Tuesday], net[(int)DayOfWeek.Wednesday], net[(int)DayOfWeek.Thursday], net[(int)DayOfWeek.Friday], net[(int)DayOfWeek.Saturday], net[(int)DayOfWeek.Sunday] };
    }

    private static string[] GetAbbreviatedDayNames()
    {
        var net = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
        return new[] { net[(int)DayOfWeek.Monday], net[(int)DayOfWeek.Tuesday], net[(int)DayOfWeek.Wednesday], net[(int)DayOfWeek.Thursday], net[(int)DayOfWeek.Friday], net[(int)DayOfWeek.Saturday], net[(int)DayOfWeek.Sunday] };
    }

    // ------------------------------------------------------------------
    // ISO week / day-of-week / day-of-year calculation
    // ------------------------------------------------------------------

    private static int GetIsoWeekOfYear(XPathDateTime value)
    {
        var dt = new DateTime((int)Math.Clamp(value.Year, 1, 9999), value.Month, value.Day);
        var cal = CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static int GetIsoWeekOfYear(DateTime dt)
    {
        var cal = CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static DayOfWeek GetDayOfWeek(XPathDateTime value)
    {
        long y = value.Year;
        int m = value.Month;
        int d = value.Day;
        if (m < 3)
        {
            m += 12;
            y -= 1;
        }
        long k = y % 100;
        long j = y / 100;
        int dayOfWeek = (int)((d + (13 * (m + 1)) / 5 + k + k / 4 + j / 4 + 5 * j) % 7);
        return dayOfWeek switch
        {
            0 => DayOfWeek.Saturday,
            1 => DayOfWeek.Sunday,
            2 => DayOfWeek.Monday,
            3 => DayOfWeek.Tuesday,
            4 => DayOfWeek.Wednesday,
            5 => DayOfWeek.Thursday,
            6 => DayOfWeek.Friday,
            _ => DayOfWeek.Sunday
        };
    }

    private static int GetDayOfYear(XPathDateTime value)
    {
        int[] daysBeforeMonth = value.Year % 4 == 0 && (value.Year % 100 != 0 || value.Year % 400 == 0)
            ? new[] { 0, 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335 }
            : new[] { 0, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
        return daysBeforeMonth[value.Month] + value.Day;
    }

    // ------------------------------------------------------------------
    // English number-to-words formatting
    // ------------------------------------------------------------------

    private static bool TryFormatWords(long value, string presentation, [NotNullWhen(true)] out string? result)
    {
        result = null;

        bool ordinal = presentation.EndsWith("o", StringComparison.OrdinalIgnoreCase);
        string stem = ordinal ? presentation[..^1] : presentation;
        string? caseSpec = stem switch
        {
            "W" => "upper",
            "w" => "lower",
            "Ww" => "title",
            _ => null
        };
        if (caseSpec is null)
            return false;

        if (value is < int.MinValue or > int.MaxValue)
            return false;

        int intValue = (int)value;
        string words = ordinal ? ToOrdinalWords(intValue) : ToCardinalWords(intValue);
        result = caseSpec switch
        {
            "upper" => words.ToUpperInvariant(),
            "lower" => words.ToLowerInvariant(),
            "title" => ToTitleCase(words),
            _ => words
        };
        return true;
    }

    private static string ToTitleCase(string words)
        => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words.ToLowerInvariant());

    private static readonly string[] Units =
    {
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    };

    private static readonly string[] Scales = { "", "thousand", "million", "billion" };

    private static readonly Dictionary<string, string> OrdinalMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zero"] = "zeroth",
        ["one"] = "first",
        ["two"] = "second",
        ["three"] = "third",
        ["four"] = "fourth",
        ["five"] = "fifth",
        ["six"] = "sixth",
        ["seven"] = "seventh",
        ["eight"] = "eighth",
        ["nine"] = "ninth",
        ["ten"] = "tenth",
        ["eleven"] = "eleventh",
        ["twelve"] = "twelfth",
        ["thirteen"] = "thirteenth",
        ["fourteen"] = "fourteenth",
        ["fifteen"] = "fifteenth",
        ["sixteen"] = "sixteenth",
        ["seventeen"] = "seventeenth",
        ["eighteen"] = "eighteenth",
        ["nineteen"] = "nineteenth",
        ["twenty"] = "twentieth",
        ["thirty"] = "thirtieth",
        ["forty"] = "fortieth",
        ["fifty"] = "fiftieth",
        ["sixty"] = "sixtieth",
        ["seventy"] = "seventieth",
        ["eighty"] = "eightieth",
        ["ninety"] = "ninetieth",
        ["hundred"] = "hundredth",
        ["thousand"] = "thousandth",
        ["million"] = "millionth",
        ["billion"] = "billionth"
    };

    private static string ToCardinalWords(int value)
    {
        if (value < 0)
            return "minus " + ToCardinalWords(-value);
        if (value == 0)
            return "zero";

        var groups = new List<int>();
        int temp = value;
        while (temp > 0)
        {
            groups.Add(temp % 1000);
            temp /= 1000;
        }

        var sb = new StringBuilder();
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            int group = groups[i];
            if (group == 0)
                continue;

            if (sb.Length > 0)
            {
                if (i == 0 && group < 100)
                    sb.Append(" and ");
                else
                    sb.Append(' ');
            }

            sb.Append(ConvertLessThanOneThousand(group));
            if (i > 0)
                sb.Append(' ').Append(Scales[i]);
        }

        return sb.ToString();
    }

    private static string ConvertLessThanOneThousand(int value)
    {
        if (value < 20)
            return Units[value];

        if (value < 100)
        {
            int unit = value % 10;
            return unit == 0
                ? Tens[value / 10]
                : Tens[value / 10] + " " + Units[unit];
        }

        int hundreds = value / 100;
        int rest = value % 100;
        return rest == 0
            ? Units[hundreds] + " hundred"
            : Units[hundreds] + " hundred and " + ConvertLessThanOneHundred(rest);
    }

    private static string ConvertLessThanOneHundred(int value)
    {
        int unit = value % 10;
        return unit == 0
            ? Tens[value / 10]
            : Tens[value / 10] + " " + Units[unit];
    }

    private static string ToOrdinalWords(int value)
    {
        if (value < 0)
            return "minus " + ToOrdinalWords(-value);
        if (value == 0)
            return "zeroth";

        string[] words = ToCardinalWords(value).Split(' ');
        string last = words[^1];
        words[^1] = OrdinalMap.TryGetValue(last, out string? ordinal)
            ? ordinal
            : last + "th";
        return string.Join(' ', words);
    }

    // ------------------------------------------------------------------
    // Roman numerals
    // ------------------------------------------------------------------

    private static string ToRoman(long value, bool upper)
    {
        if (value <= 0 || value > 3999)
            return value.ToString(CultureInfo.InvariantCulture);
        var numerals = upper
            ? new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" }
            : new[] { "m", "cm", "d", "cd", "c", "xc", "l", "xl", "x", "ix", "v", "iv", "i" };
        var values = new[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        var sb = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            while (value >= values[i])
            {
                sb.Append(numerals[i]);
                value -= values[i];
            }
        }
        return sb.ToString();
    }
}
