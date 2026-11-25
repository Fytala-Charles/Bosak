// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Stores validated bounded calendar components and exact decimal seconds.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Identifies the explicitly constructed exact calendar family.</summary>
public enum XdmExactCalendarKind
{
    /// <summary>Uninitialized default value.</summary>
    Unspecified,
    /// <summary>XML Schema dateTime components.</summary>
    DateTime,
    /// <summary>XML Schema time components without a date.</summary>
    Time,
    /// <summary>XML Schema dateTimeStamp components with required timezone.</summary>
    DateTimeStamp
}

/// <summary>Stores bounded exact calendar components, without XdmValue or schema authority attachment.</summary>
public readonly record struct XdmExactCalendarValue
{
    private static readonly Regex Lexical = new(@"\A(?:(?<y>-?[0-9]{4,})-(?<m>[0-9]{2})-(?<d>[0-9]{2})T)?(?<h>[0-9]{2}):(?<n>[0-9]{2}):(?<s>[0-9]{2}(?:\.[0-9]+)?)(?<z>Z|[+-][0-9]{2}:[0-9]{2})?\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    /// <summary>Gets the calendar family; default is uninitialized.</summary>
    public XdmExactCalendarKind Kind { get; }
    /// <summary>Gets the astronomical year, or zero for time-only values.</summary>
    public long Year { get; }
    /// <summary>Gets the month, or zero for time-only values.</summary>
    public int Month { get; }
    /// <summary>Gets the day, or zero for time-only values.</summary>
    public int Day { get; }
    /// <summary>Gets the normalized hour.</summary>
    public int Hour { get; }
    /// <summary>Gets the minute.</summary>
    public int Minute { get; }
    /// <summary>Gets exact decimal seconds.</summary>
    public XdmExactSecond Second { get; }
    /// <summary>Gets whether an explicit timezone was supplied.</summary>
    public bool HasTimezone { get; }
    /// <summary>Gets the timezone minutes, zero when absent.</summary>
    public int TimezoneOffsetMinutes { get; }
    /// <summary>Validates and constructs exact calendar components, normalizing 24:00.</summary>
    /// <param name="kind">DateTime, Time or DateTimeStamp.</param>
    /// <param name="year">Astronomical year, excluding Int64.MinValue; zero for Time.</param>
    /// <param name="month">Calendar month; zero for Time.</param>
    /// <param name="day">Calendar day; zero for Time.</param>
    /// <param name="hour">Hour from 0 through 24.</param>
    /// <param name="minute">Minute from 0 through 59.</param>
    /// <param name="second">Exact seconds.</param>
    /// <param name="hasTimezone">Whether timezone is present.</param>
    /// <param name="timezoneOffsetMinutes">Offset from -840 through 840; zero when absent.</param>
    /// <exception cref="ArgumentOutOfRangeException">Components are invalid.</exception>
    /// <exception cref="NotSupportedException">Normalization exceeds year capacity.</exception>
    public XdmExactCalendarValue(XdmExactCalendarKind kind, long year, int month, int day, int hour, int minute, XdmExactSecond second, bool hasTimezone, int timezoneOffsetMinutes)
    {
        if (kind is < XdmExactCalendarKind.DateTime or > XdmExactCalendarKind.DateTimeStamp) throw new ArgumentOutOfRangeException(nameof(kind));
        if (year == long.MinValue) throw new ArgumentOutOfRangeException(nameof(year));
        if (kind == XdmExactCalendarKind.Time)
        {
            if (year != 0 || month != 0 || day != 0) throw new ArgumentOutOfRangeException(nameof(year));
        }
        else if (month is < 1 or > 12 || day < 1 || day > Days(year, month)) throw new ArgumentOutOfRangeException(nameof(day));
        if (hour is < 0 or > 24 || minute is < 0 or > 59 || (hour == 24 && (minute != 0 || !second.Coefficient.IsZero))) throw new ArgumentOutOfRangeException(nameof(hour));
        if (timezoneOffsetMinutes is < -840 or > 840 || (!hasTimezone && timezoneOffsetMinutes != 0) || (kind == XdmExactCalendarKind.DateTimeStamp && !hasTimezone)) throw new ArgumentOutOfRangeException(nameof(timezoneOffsetMinutes));
        if (hour == 24)
        {
            hour = 0;
            if (kind != XdmExactCalendarKind.Time && ++day > Days(year, month))
            {
                day = 1;
                if (++month > 12)
                {
                    month = 1;
                    if (year == long.MaxValue) throw new NotSupportedException("Year capacity exceeded by 24:00 normalization.");
                    year++;
                }
            }
        }
        Kind = kind; Year = year; Month = month; Day = day; Hour = hour; Minute = minute; Second = second; HasTimezone = hasTimezone; TimezoneOffsetMinutes = timezoneOffsetMinutes;
    }
    private static int Days(long year, int month) => month switch { 2 => year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? 29 : 28, 4 or 6 or 9 or 11 => 30, _ => 31 };
    /// <summary>Parses a bounded XML Schema calendar lexical token; no expression engine admission is implied.</summary>
    /// <param name="text">At most 1024 characters including surrounding XML whitespace.</param>
    /// <param name="kind">Required calendar family.</param>
    /// <param name="maxDigits">Decimal coefficient capacity.</param>
    /// <param name="maxScale">Decimal scale capacity.</param>
    /// <returns>Validated normalized components.</returns>
    /// <exception cref="FormatException">Lexical grammar or year representation is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Calendar components or supplied bounds are invalid.</exception>
    /// <exception cref="NotSupportedException">Input or representation exceeds capacity.</exception>
    public static XdmExactCalendarValue Parse(string text, XdmExactCalendarKind kind, int maxDigits, int maxScale)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length > 1024) throw new NotSupportedException("Calendar lexical capacity exceeded.");
        var m = Lexical.Match(text.Trim(' ', '\t', '\r', '\n'));
        bool time = kind == XdmExactCalendarKind.Time;
        if (!m.Success || m.Groups["y"].Success == time) throw new FormatException("Invalid calendar lexical token.");
        string y = m.Groups["y"].Value;
        string magnitude = y.StartsWith('-') ? y[1..] : y;
        if (!time && (magnitude.Length > 4 && magnitude[0] == '0' || y == "-0000")) throw new FormatException("Invalid year token.");
        if (!time && !long.TryParse(y, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)) throw new NotSupportedException("Year capacity exceeded.");
        int Number(string group) => int.Parse(m.Groups[group].Value, CultureInfo.InvariantCulture);
        string zone = m.Groups["z"].Value; int offset = 0;
        if (zone.Length > 1)
        {
            int hours = int.Parse(zone.AsSpan(1, 2), CultureInfo.InvariantCulture), minutes = int.Parse(zone.AsSpan(4, 2), CultureInfo.InvariantCulture);
            if (minutes > 59 || hours > 14 || (hours == 14 && minutes != 0)) throw new FormatException("Invalid timezone.");
            offset = (zone[0] == '-' ? -1 : 1) * (hours * 60 + minutes);
        }
        return new(kind, time ? 0 : long.Parse(y, CultureInfo.InvariantCulture), time ? 0 : Number("m"), time ? 0 : Number("d"), Number("h"), Number("n"), XdmExactSecond.Parse(m.Groups["s"].Value, maxDigits, maxScale), zone.Length != 0, offset);
    }
    /// <summary>Formats normalized components; this is not canonical UTC serialization.</summary>
    /// <param name="maxCharacters">Output capacity.</param>
    /// <returns>Invariant calendar text preserving timezone presence and offset.</returns>
    /// <exception cref="InvalidOperationException">The value is uninitialized.</exception>
    /// <exception cref="NotSupportedException">Output exceeds capacity.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Output bound is invalid.</exception>
    public string ToInvariantString(int maxCharacters)
    {
        EnsureInitialized(); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCharacters);
        string prefix = Kind == XdmExactCalendarKind.Time ? "" : (Year < 0 ? "-" : "") + Math.Abs(Year).ToString("D4", CultureInfo.InvariantCulture) + FormattableString.Invariant($"-{Month:00}-{Day:00}T");
        int offset = Math.Abs(TimezoneOffsetMinutes);
        string zone = !HasTimezone ? "" : offset == 0 ? "Z" : (TimezoneOffsetMinutes < 0 ? "-" : "+") + FormattableString.Invariant($"{offset / 60:00}:{offset % 60:00}");
        string value = prefix + FormattableString.Invariant($"{Hour:00}:{Minute:00}:") + Second.ToInvariantString(maxCharacters) + zone;
        if (value.Length > maxCharacters) throw new NotSupportedException("Calendar output capacity exceeded.");
        return value;
    }
    /// <summary>Converts exactly to DateTimeOffset, refusing absent timezone, extended years and lost fractions.</summary>
    /// <returns>An exact DTO for the explicit components.</returns>
    /// <exception cref="InvalidOperationException">The value is uninitialized.</exception>
    /// <exception cref="NotSupportedException">Exact conversion is unavailable.</exception>
    public DateTimeOffset ToDateTimeOffsetExact()
    {
        EnsureInitialized();
        if (!HasTimezone || Kind == XdmExactCalendarKind.Time || Year is < 1 or > 9999) throw new NotSupportedException("Explicit timezone and representable full date required.");
        long ticks = Second.ToExactUnits(7);
        try { return new DateTimeOffset((int)Year, Month, Day, Hour, Minute, 0, TimeSpan.FromMinutes(TimezoneOffsetMinutes)).AddTicks(ticks); }
        catch (ArgumentOutOfRangeException ex) { throw new NotSupportedException("Calendar is outside DateTimeOffset instant range.", ex); }
        catch (ArgumentException ex) { throw new NotSupportedException("Calendar is outside DateTimeOffset instant range.", ex); }
    }
    /// <summary>Captures every DTO tick without using the legacy millisecond carrier.</summary>
    /// <param name="value">Explicit zoned timestamp.</param>
    /// <returns>Exact dateTime components with timezone.</returns>
    public static XdmExactCalendarValue FromDateTimeOffset(DateTimeOffset value) => new(XdmExactCalendarKind.DateTime, value.Year, value.Month, value.Day, value.Hour, value.Minute, new(value.Ticks % TimeSpan.TicksPerMinute, 7, 9, 7), true, (int)value.Offset.TotalMinutes);
    /// <summary>Converts to the legacy component carrier only at exact millisecond resolution.</summary>
    /// <returns>Lossless legacy components; time-only dates remain zero.</returns>
    /// <exception cref="InvalidOperationException">The value is uninitialized.</exception>
    /// <exception cref="NotSupportedException">Fractions exceed millisecond resolution.</exception>
    public XPathDateTime ToXPathDateTimeExact()
    {
        EnsureInitialized(); long units = Second.ToExactUnits(3);
        return new(Year, Month, Day, Hour, Minute, (int)(units / 1000), (int)(units % 1000), TimezoneOffsetMinutes, HasTimezone);
    }
    /// <summary>Validates and imports legacy components without silently changing timezone presence.</summary>
    /// <param name="value">Legacy components.</param>
    /// <param name="kind">Required family.</param>
    /// <returns>Exact validated components.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Legacy components are invalid.</exception>
    public static XdmExactCalendarValue FromXPathDateTime(XPathDateTime value, XdmExactCalendarKind kind)
    {
        if (value.Second is < 0 or > 59 || value.Millisecond is < 0 or > 999) throw new ArgumentOutOfRangeException(nameof(value));
        return new(kind, value.Year, value.Month, value.Day, value.Hour, value.Minute, new(value.Second * 1000 + value.Millisecond, 3, 5, 3), value.HasTimezone, value.TimezoneOffsetMinutes);
    }
    private void EnsureInitialized() { if (Kind == XdmExactCalendarKind.Unspecified) throw new InvalidOperationException("Uninitialized calendar value."); }
}
