// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 mei 2026
// PURPOSE              : Stores XPath date/time values with extended year support (negative years, year > 9999).
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | ToDateTimeOffset range failure carries FODT0001                                          |
//                      | Charles Korthout | 0.3   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Extension methods for converting .NET date/time values to <see cref="XPathDateTime"/>.
/// </summary>
public static class XPathDateTimeExtensions
{
    /// <summary>Converts a <see cref="DateTimeOffset"/> to an <see cref="XPathDateTime"/> with the given timezone presence.</summary>
    public static XPathDateTime ToXPathDateTime(this DateTimeOffset dto, bool hasTimezone)
        => new(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, (int)dto.Offset.TotalMinutes, hasTimezone);
}

/// <summary>
/// Stores the components of an XPath date, time, or dateTime value.
/// Uses <see cref="long"/> for the year to support XML Schema extended years
/// (negative years and years &gt; 9999) that .NET's <see cref="DateTimeOffset"/> cannot represent.
/// </summary>
public readonly struct XPathDateTime
{
    /// <summary>Gets the year; supports XML Schema extended years (negative and greater than 9999).</summary>
    public long Year { get; }
    /// <summary>Gets the month (1-12).</summary>
    public int Month { get; }
    /// <summary>Gets the day of the month (1-31).</summary>
    public int Day { get; }
    /// <summary>Gets the hour (0-24; 24:00:00 is a valid XPath lexical form).</summary>
    public int Hour { get; }
    /// <summary>Gets the minute (0-59).</summary>
    public int Minute { get; }
    /// <summary>Gets the second (0-59).</summary>
    public int Second { get; }
    /// <summary>Gets the millisecond (0-999).</summary>
    public int Millisecond { get; }
    /// <summary>Gets the timezone offset from UTC in minutes; meaningful only when <see cref="HasTimezone"/> is true.</summary>
    public int TimezoneOffsetMinutes { get; }
    /// <summary>Gets whether the value carries an explicit timezone.</summary>
    public bool HasTimezone { get; }

    /// <summary>Creates a date/time value from its individual components.</summary>
    public XPathDateTime(long year, int month, int day, int hour, int minute, int second, int millisecond, int timezoneOffsetMinutes, bool hasTimezone)
    {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
        Millisecond = millisecond;
        TimezoneOffsetMinutes = timezoneOffsetMinutes;
        HasTimezone = hasTimezone;
    }

    /// <summary>
    /// Returns <c>true</c> if this date/time can be represented by .NET's <see cref="DateTimeOffset"/>.
    /// </summary>
    public bool IsRepresentableAsDateTimeOffset => Year is >= 1 and <= 9999;

    /// <summary>
    /// Converts to <see cref="DateTimeOffset"/> if the year is in the supported range.
    /// Throws <see cref="InvalidOperationException"/> otherwise.
    /// </summary>
    public DateTimeOffset ToDateTimeOffset()
    {
        if (!IsRepresentableAsDateTimeOffset)
            throw new InvalidOperationException($"FODT0001: Year {Year} is outside the range supported by DateTimeOffset.");

        var offset = TimeSpan.FromMinutes(TimezoneOffsetMinutes);
        return new DateTimeOffset((int)Year, Month, Day, Hour, Minute, Second, Millisecond, offset);
    }

    /// <summary>
    /// Formats the year according to XPath/XML Schema rules.
    /// Negative years use a leading '-'.
    /// Year 0 is formatted as <c>0000</c>.
    /// Positive years are zero-padded to at least 4 digits.
    /// </summary>
    public string FormatYear()
    {
        if (Year == 0)
            return "0000";
        if (Year < 0)
            return $"-{Math.Abs(Year):D4}";
        return $"{Year:D4}";
    }

    /// <summary>
    /// Formats the timezone suffix (<c>Z</c> or <c>±HH:mm</c>) or empty string if no timezone.
    /// </summary>
    public string FormatTimezone()
    {
        if (!HasTimezone)
            return "";
        if (TimezoneOffsetMinutes == 0)
            return "Z";
        int totalMinutes = TimezoneOffsetMinutes;
        string sign = totalMinutes < 0 ? "-" : "+";
        totalMinutes = Math.Abs(totalMinutes);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return $"{sign}{hours:D2}:{minutes:D2}";
    }
}
