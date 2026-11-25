// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Projects plain legacy and exact calendar values into explicit exact comparisons.
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
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Projects plain legacy and exact calendars into explicit exact component comparisons.</summary>
public static class XdmCalendarInterop
{
    /// <summary>Reads validated component data without creating or carrying schema authority.</summary>
    /// <param name="value">Plain dateTime, dateTimeStamp or time atomic value.</param>
    /// <returns>Exact components preserving stored precision; time discards its incidental date.</returns>
    /// <exception cref="NotSupportedException">The value carries authority or an unsupported built-in label.</exception>
    /// <exception cref="ArgumentException">The value is not a calendar or its components are inconsistent.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A component is invalid.</exception>
    public static XdmExactCalendarValue Read(XdmValue value)
    {
        // Never use a public provenance observer as an authorization or liveness check.
        if (XdmValue.SchemaTransportMarker(value) is not null)
            throw new NotSupportedException("Calendar interop requires a plain value; authority carriers require a guarded projection.");
        if (value.Kind is not (XdmValueKind.DateTime or XdmValueKind.Time))
            throw new ArgumentException("A plain dateTime or time calendar is required.", nameof(value));
        if (value.IsExactCalendar) return value.ExactCalendarValue;

        bool time = value.Kind == XdmValueKind.Time;
        string? label = value.SchemaTypeName;
        if (time ? label is not (null or "time") : label is not (null or "dateTime" or "dateTimeStamp"))
            throw new NotSupportedException("The calendar built-in label is not supported for this physical family.");
        var raw = time ? value.TimeXPathValue : value.DateTimeXPathValue;
        if (raw.HasTimezone != value.HasTimezone || (!raw.HasTimezone && raw.TimezoneOffsetMinutes != 0))
            throw new ArgumentException("Legacy timezone flags or stored absent offset are inconsistent.", nameof(value));
        if (raw.Second is < 0 or > 59 || raw.Millisecond is < 0 or > 999)
            throw new ArgumentOutOfRangeException(nameof(value), "Invalid legacy seconds or milliseconds.");
        var seconds = new XdmExactSecond(new BigInteger(raw.Second) * 1000 + raw.Millisecond, 3, 5, 3);
        var kind = time ? XdmExactCalendarKind.Time : label == "dateTimeStamp" ? XdmExactCalendarKind.DateTimeStamp : XdmExactCalendarKind.DateTime;
        return new XdmExactCalendarValue(kind, time ? 0 : raw.Year, time ? 0 : raw.Month,
            time ? 0 : raw.Day, raw.Hour, raw.Minute, seconds, raw.HasTimezone, raw.TimezoneOffsetMinutes);
    }

    /// <summary>Compares plain calendar values with a mandatory XPath implicit timezone.</summary>
    /// <param name="left">Plain calendar operand.</param>
    /// <param name="right">Plain calendar operand of a compatible family.</param>
    /// <param name="implicitTimezoneMinutes">Context offset from -840 through 840 minutes.</param>
    /// <returns>Minus one, zero or one for earlier, equal or later.</returns>
    /// <exception cref="NotSupportedException">An operand cannot be projected by Read.</exception>
    /// <exception cref="ArgumentException">An operand, family or component is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The offset exceeds fourteen hours.</exception>
    public static int CompareXPath(XdmValue left, XdmValue right, int implicitTimezoneMinutes)
    {
        if (implicitTimezoneMinutes is < -840 or > 840)
            throw new ArgumentOutOfRangeException(nameof(implicitTimezoneMinutes));
        return XdmExactCalendarComparison.CompareXPath(Read(left), Read(right), implicitTimezoneMinutes);
    }

    /// <summary>Compares plain calendars using XSD partial ordering.</summary>
    /// <param name="left">Plain calendar operand.</param>
    /// <param name="right">Plain calendar operand of a compatible family.</param>
    /// <returns>A definite order or Indeterminate for timezone uncertainty.</returns>
    /// <exception cref="NotSupportedException">An operand cannot be projected by Read.</exception>
    /// <exception cref="ArgumentException">An operand, family or component is invalid.</exception>
    public static XdmExactCalendarOrder CompareXsd(XdmValue left, XdmValue right)
        => XdmExactCalendarComparison.CompareXsd(Read(left), Read(right));

    /// <summary>Tests mixed representation same-key equivalence without using an implicit timezone.</summary>
    /// <param name="left">Plain calendar operand.</param>
    /// <param name="right">Plain calendar operand.</param>
    /// <returns>Whether primitive family, timezone presence and exact timeline point agree.</returns>
    /// <exception cref="NotSupportedException">An operand cannot be projected by Read.</exception>
    /// <exception cref="ArgumentException">An operand or component is invalid.</exception>
    public static bool SameKey(XdmValue left, XdmValue right)
        => XdmExactCalendarComparison.SameKey(Read(left), Read(right));

    /// <summary>Hashes plain calendars consistently with mixed representation SameKey.</summary>
    /// <param name="value">Plain calendar operand.</param>
    /// <returns>A process-local hash, unsuitable as an authority fingerprint or serialization.</returns>
    /// <exception cref="NotSupportedException">The operand cannot be projected by Read.</exception>
    /// <exception cref="ArgumentException">The operand or a component is invalid.</exception>
    public static int GetSameKeyHashCode(XdmValue value)
        => XdmExactCalendarComparison.GetSameKeyHashCode(Read(value));
}

/// <summary>Provides an opt-in CLR comparer for plain legacy and exact calendar keys.</summary>
public sealed class XdmCalendarInteropKeyComparer : IEqualityComparer<XdmValue>
{
    private XdmCalendarInteropKeyComparer() { }

    /// <summary>Gets the explicit calendar comparer; existing XdmValue maps retain their behavior.</summary>
    public static readonly XdmCalendarInteropKeyComparer Instance = new();

    /// <summary>Tests same-key equivalence of two plain calendars.</summary>
    /// <param name="left">Plain calendar operand.</param>
    /// <param name="right">Plain calendar operand.</param>
    /// <returns>Whether the calendar keys are equivalent.</returns>
    /// <exception cref="NotSupportedException">An operand cannot be projected.</exception>
    /// <exception cref="ArgumentException">An operand or component is invalid.</exception>
    public bool Equals(XdmValue left, XdmValue right) => XdmCalendarInterop.SameKey(left, right);

    /// <summary>Hashes a plain calendar consistently with Equals.</summary>
    /// <param name="value">Plain calendar operand.</param>
    /// <returns>A process-local calendar key hash.</returns>
    /// <exception cref="NotSupportedException">The operand cannot be projected.</exception>
    /// <exception cref="ArgumentException">The operand or a component is invalid.</exception>
    public int GetHashCode(XdmValue value) => XdmCalendarInterop.GetSameKeyHashCode(value);
}
