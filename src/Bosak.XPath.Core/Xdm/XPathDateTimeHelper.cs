// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 juni 2026
// PURPOSE              : Calendar arithmetic and UTC normalization for XPath date/time values.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-06-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-06-2026     | Fixed proleptic Gregorian leap-year calculation for negative years                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Provides proleptic-Gregorian calendar arithmetic for <see cref="XPathDateTime"/> values,
/// including negative years, year zero, and years outside the .NET <see cref="DateTimeOffset"/> range.
/// </summary>
public static class XPathDateTimeHelper
{
    /// <summary>
    /// Returns the number of days between the civil date <c>1970-01-01</c> and the supplied date,
    /// using the proleptic Gregorian calendar for all years (including year 0 and negative years).
    /// </summary>
    public static long DaysFromCivil(long year, int month, int day)
    {
        // Algorithm adapted from Howard Hinnant's public-domain civil-from-days.
        // It uses floor division so that negative years are handled correctly.
        long y = year;
        int m = month;
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }
        long era = y >= 0 ? y / 400 : (y - 399) / 400;
        long yoe = y - era * 400;                 // [0, 399]
        long mp = m - 3;                           // [0, 11]
        long doy = (153 * mp + 2) / 5 + day - 1;   // [0, 365]
        long doe = yoe * 365 + yoe / 4 - yoe / 100 + doy; // [0, 146096]
        return era * 146097 + doe - 719468;
    }

    /// <summary>
    /// Converts a day count relative to <c>1970-01-01</c> back to a civil date.
    /// </summary>
    public static (long Year, int Month, int Day) CivilFromDays(long days)
    {
        long z = days + 719468;
        long era = z >= 0 ? z / 146097 : (z - 146096) / 146097;
        long doe = z - era * 146097; // [0, 146096]
        long yoe = (doe - doe / 1460 + doe / 36524 - doe / 146096) / 365; // [0, 399]
        long y = yoe + era * 400;
        long doy = doe - (365 * yoe + yoe / 4 - yoe / 100); // [0, 365]
        long mp = (5 * doy + 2) / 153; // [0, 11]
        long d = doy - (153 * mp + 2) / 5 + 1;
        long m = mp + (mp < 10 ? 3 : -9);
        y += (m <= 2) ? 1 : 0;
        return (y, (int)m, (int)d);
    }

    /// <summary>
    /// Returns the number of days in the given month, accounting for leap years
    /// in the proleptic Gregorian calendar (year 0 is a leap year).
    /// </summary>
    public static int DaysInMonth(long year, int month)
    {
        return month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => IsLeapYear(year) ? 29 : 28,
            _ => 0
        };
    }

    /// <summary>
    /// Determines whether the supplied year is a leap year in the proleptic Gregorian calendar.
    /// Year 0 is treated as a leap year; negative years count back from 1 BCE.
    /// </summary>
    public static bool IsLeapYear(long year)
    {
        if (year == 0) return true;
        if (year < 0) year = -year;
        return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    }

    /// <summary>
    /// Normalizes a date/time value to UTC by applying its timezone offset.
    /// Works for any year, including those outside the .NET <see cref="DateTimeOffset"/> range.
    /// </summary>
    public static XPathDateTime NormalizeToUtc(XPathDateTime xdt)
    {
        if (!xdt.HasTimezone)
            return xdt;

        long days = DaysFromCivil(xdt.Year, xdt.Month, xdt.Day);
        long msOfDay = ((xdt.Hour * 3600L + xdt.Minute * 60L + xdt.Second) * 1000L + xdt.Millisecond)
            - xdt.TimezoneOffsetMinutes * 60L * 1000L;
        long offsetDays = msOfDay / 86400000L;
        msOfDay %= 86400000L;
        if (msOfDay < 0)
        {
            msOfDay += 86400000L;
            offsetDays--;
        }

        var (year, month, day) = CivilFromDays(days + offsetDays);
        int hour = (int)(msOfDay / 3600000L);
        msOfDay %= 3600000L;
        int minute = (int)(msOfDay / 60000L);
        msOfDay %= 60000L;
        int second = (int)(msOfDay / 1000L);
        int ms = (int)(msOfDay % 1000L);
        return new XPathDateTime(year, month, day, hour, minute, second, ms, 0, true);
    }

    /// <summary>
    /// Adds the supplied number of days to a civil date and returns the resulting date.
    /// </summary>
    public static (long Year, int Month, int Day) AddDays(long year, int month, int day, long days)
    {
        long d = DaysFromCivil(year, month, day) + days;
        return CivilFromDays(d);
    }

    /// <summary>
    /// Adds months to a civil date, clamping the day to the last day of the resulting month.
    /// </summary>
    public static (long Year, int Month, int Day) AddMonths(long year, int month, int day, long months)
    {
        long totalMonths = year * 12 + (month - 1) + months;
        long newYear = totalMonths / 12;
        int newMonth = (int)(totalMonths % 12) + 1;
        if (newMonth <= 0)
        {
            newYear--;
            newMonth += 12;
        }
        int newDay = Math.Min(day, DaysInMonth(newYear, newMonth));
        return (newYear, newMonth, newDay);
    }

    /// <summary>
    /// Compares two date/time values by their normalized UTC instant.
    /// Suitable for dateTime, date (treated as 00:00:00) and time (caller supplies a reference date).
    /// </summary>
    public static int CompareByInstant(XPathDateTime a, XPathDateTime b)
    {
        var ua = NormalizeToUtc(a);
        var ub = NormalizeToUtc(b);
        return CompareComponents(ua, ub);
    }

    private static readonly System.Text.RegularExpressions.Regex DurationPartsRegex = new(
        @"^(?<sign>[+-]?)P(?<Y>\d+Y)?(?<M>\d+M)?(?<D>\d+D)?(?<T>T(?<H>\d+H)?(?<Tm>\d+M)?(?<S>\d+(\.\d+)?S)?)?$",
        System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    /// <summary>
    /// Normalizes a duration string to total months and total seconds.
    /// Only the yearMonth and dayTime portions are separated; a generic xs:duration
    /// returns the combined totals.
    /// </summary>
    public static (long TotalMonths, decimal TotalSeconds) NormalizeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return (0, 0);
        bool negative = m.Groups["sign"].Value == "-";

        long years = m.Groups["Y"].Success ? long.Parse(m.Groups["Y"].Value.TrimEnd('Y'), System.Globalization.CultureInfo.InvariantCulture) : 0;
        long months = m.Groups["M"].Success ? long.Parse(m.Groups["M"].Value.TrimEnd('M'), System.Globalization.CultureInfo.InvariantCulture) : 0;
        long days = m.Groups["D"].Success ? long.Parse(m.Groups["D"].Value.TrimEnd('D'), System.Globalization.CultureInfo.InvariantCulture) : 0;
        long hours = m.Groups["H"].Success ? long.Parse(m.Groups["H"].Value.TrimEnd('H'), System.Globalization.CultureInfo.InvariantCulture) : 0;
        long minutes = m.Groups["Tm"].Success ? long.Parse(m.Groups["Tm"].Value.TrimEnd('M'), System.Globalization.CultureInfo.InvariantCulture) : 0;
        decimal seconds = m.Groups["S"].Success ? decimal.Parse(m.Groups["S"].Value.TrimEnd('S'), System.Globalization.CultureInfo.InvariantCulture) : 0;

        long totalMonths = years * 12 + months;
        decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;

        if (negative)
        {
            totalMonths = -totalMonths;
            totalSeconds = -totalSeconds;
        }

        return (totalMonths, totalSeconds);
    }

    /// <summary>
    /// Lexicographic comparison of date/time components.
    /// </summary>
    public static int CompareComponents(XPathDateTime a, XPathDateTime b)
    {
        if (a.Year != b.Year) return a.Year < b.Year ? -1 : 1;
        if (a.Month != b.Month) return a.Month < b.Month ? -1 : 1;
        if (a.Day != b.Day) return a.Day < b.Day ? -1 : 1;
        if (a.Hour != b.Hour) return a.Hour < b.Hour ? -1 : 1;
        if (a.Minute != b.Minute) return a.Minute < b.Minute ? -1 : 1;
        if (a.Second != b.Second) return a.Second < b.Second ? -1 : 1;
        if (a.Millisecond != b.Millisecond) return a.Millisecond < b.Millisecond ? -1 : 1;
        return 0;
    }
}
