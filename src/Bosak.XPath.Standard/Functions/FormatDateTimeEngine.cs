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

            int close = FindClosingBracket(picture, bracket);
            if (close < 0)
                throw FormatError("FOFD1340");

            string marker = picture[(bracket + 1)..close];
            sb.Append(FormatMarker(value, marker, components));
            pos = close + 1;
        }

        return sb.ToString();
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

    private static string FormatMarker(XPathDateTime value, string marker, DateTimeComponents components)
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

        // Validate component is available for the value type
        ValidateComponentAvailable(component, components);

        string result = component switch
        {
            'Y' => FormatYear(value, presentation, minWidth, maxWidth),
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

        return result;
    }

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

    private static char? DetectZeroDigit(string presentation)
    {
        foreach (char c in presentation)
        {
            if (char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
            {
                char zd = GetDigitZero(c);
                int digitValue = c - zd;
                if (digitValue >= 0 && digitValue <= 9)
                    return zd;
            }
        }
        return null;
    }

    private static char GetDigitZero(char digit)
    {
        int val = digit - '0';
        if (val >= 0 && val <= 9)
            return '0';

        val = digit - '\u0660';
        if (val >= 0 && val <= 9)
            return '\u0660';

        val = digit - '\u06F0';
        if (val >= 0 && val <= 9)
            return '\u06F0';

        val = digit - '\u0E50';
        if (val >= 0 && val <= 9)
            return '\u0E50';

        val = digit - '\u0966';
        if (val >= 0 && val <= 9)
            return '\u0966';

        val = digit - '\u09E6';
        if (val >= 0 && val <= 9)
            return '\u09E6';

        return digit;
    }

    private static string MapDigit(char asciiDigit, char zeroDigit)
    {
        if (zeroDigit == '0')
            return asciiDigit.ToString();
        int val = asciiDigit - '0';
        if (val < 0 || val > 9)
            return asciiDigit.ToString();
        return ((char)(zeroDigit + val)).ToString();
    }

    private static string MapDigits(string asciiDigits, char zeroDigit)
    {
        if (zeroDigit == '0')
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
        char zeroDigit = '0';
        int mandatoryCount = 0;
        int optionalCount = 0;
        bool seenMandatory = false;
        char? firstZeroDigit = null;

        foreach (char c in presentation)
        {
            if (c == '#')
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
            else if (char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
            {
                char zd = GetDigitZero(c);
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

    private readonly record struct DigitInfo(char ZeroDigit, int Mandatory, int Optional)
    {
        public int TotalPositions => Mandatory + Optional;
    }

    // ------------------------------------------------------------------
    // Integer formatting
    // ------------------------------------------------------------------

    private static string FormatInteger(long value, string presentation, int minWidth, int maxWidth, bool allowTruncate = true)
    {
        var info = ParseDigitPresentation(presentation, isFractional: false);
        int totalPositions = info.TotalPositions;
        if (totalPositions == 0)
        {
            // No digit characters in presentation - default to the value as string
            string result = value.ToString(CultureInfo.InvariantCulture);
            result = ApplyWidth(result, minWidth, maxWidth, info.ZeroDigit);
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
        return result2;
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
    {
        if (maxWidth != int.MaxValue && value.Length > maxWidth)
            return value[..maxWidth];
        if (value.Length < minWidth)
        {
            char padChar = zeroDigit == '0' ? '0' : zeroDigit;
            return value.PadLeft(minWidth, padChar);
        }
        return value;
    }

    private static string ApplyWidthFractional(string value, int minWidth, int maxWidth, char zeroDigit)
    {
        if (maxWidth != int.MaxValue && value.Length > maxWidth)
            return value[..maxWidth];
        if (value.Length < minWidth)
        {
            char padChar = zeroDigit == '0' ? '0' : zeroDigit;
            return value.PadRight(minWidth, padChar);
        }
        return value;
    }

    // ------------------------------------------------------------------
    // Component formatters
    // ------------------------------------------------------------------

    private static string FormatYear(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        long year = value.Year;
        if (presentation == "i")
            return ToRoman(year, upper: false);
        if (presentation == "I")
            return ToRoman(year, upper: true);

        var info = ParseDigitPresentation(presentation, isFractional: false);
        if (info.TotalPositions > 0)
        {
            return FormatInteger(year, presentation, minWidth, maxWidth);
        }

        string digits = year.ToString(CultureInfo.InvariantCulture);
        digits = ApplyWidth(digits, minWidth, maxWidth, '0');
        return digits;
    }

    private static string FormatMonth(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (TryFormatName(value.Month, presentation, GetMonthNames(), out string? nameResult))
            return nameResult;

        return FormatInteger(value.Month, presentation, minWidth, maxWidth, allowTruncate: false);
    }

    private static string FormatDay(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        var dayNames = GetDayNames();
        if (TryFormatName((int)GetDayOfWeek(value), presentation, dayNames, out string? nameResult))
            return nameResult;

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

        var dayNames = GetDayNames();
        if (TryFormatName(dow, presentation, dayNames, out string? nameResult))
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
        int week = GetIsoWeekOfYear(value)
            - GetIsoWeekOfYear(new XPathDateTime(value.Year, value.Month, 1, 0, 0, 0, 0, value.TimezoneOffsetMinutes, value.HasTimezone))
            + 1;
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
            "n" or "nn" or "nnn" => ampm.ToLowerInvariant(),
            "N" or "NN" or "NNN" => ampm.ToUpperInvariant(),
            "Nn" or "NNn" => char.ToUpperInvariant(ampm[0]) + ampm[1..].ToLowerInvariant(),
            _ => ampm.ToUpperInvariant()
        };

        return ApplyWidth(ampm, minWidth, maxWidth, '0');
    }

    private static string FormatTimezone(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (!value.HasTimezone)
            return "";

        var offset = TimeSpan.FromMinutes(value.TimezoneOffsetMinutes);
        if (offset == TimeSpan.Zero)
            return "Z";

        string sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset < TimeSpan.Zero ? -offset : offset;

        if (presentation == "z" || presentation == "0")
            return $"{sign}{abs.Hours:00}";
        if (presentation == "zz" || presentation == "00" || presentation.All(c => c == '0'))
            return $"{sign}{abs.Hours:00}{abs.Minutes:00}";

        return $"{sign}{abs.Hours:00}:{abs.Minutes:00}";
    }

    private static string FormatTimezoneGmt(XPathDateTime value, string presentation, int minWidth, int maxWidth)
    {
        if (!value.HasTimezone)
            return "";

        var offset = TimeSpan.FromMinutes(value.TimezoneOffsetMinutes);
        if (offset == TimeSpan.Zero)
            return "GMT";

        string sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset < TimeSpan.Zero ? -offset : offset;
        return $"GMT{sign}{abs.Hours:00}:{abs.Minutes:00}";
    }

    // ------------------------------------------------------------------
    // Name formatting helpers
    // ------------------------------------------------------------------

    private static bool TryFormatName(int index1Based, string presentation, string[] names, [NotNullWhen(true)] out string? result)
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
                return true;
            case "n":
            case "nn":
            case "nnn":
                result = name.ToLowerInvariant();
                return true;
            case "Nn":
            case "NNn":
                result = char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant();
                return true;
        }

        // For day-of-week with F prefix
        if (presentation.StartsWith("F"))
        {
            string sub = presentation[1..];
            switch (sub)
            {
                case "N":
                case "NN":
                case "NNN":
                    result = name.ToUpperInvariant();
                    return true;
                case "n":
                case "nn":
                case "nnn":
                    result = name.ToLowerInvariant();
                    return true;
                case "Nn":
                case "NNn":
                    result = char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant();
                    return true;
            }
        }

        return false;
    }

    private static string[] GetMonthNames() => CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
    private static string[] GetDayNames() => CultureInfo.InvariantCulture.DateTimeFormat.DayNames;

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
