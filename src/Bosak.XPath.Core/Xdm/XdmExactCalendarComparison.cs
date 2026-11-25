// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Compares bounded exact calendar values with explicit timezone semantics.
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

/// <summary>Represents an XSD calendar partial-order result.</summary>
public enum XdmExactCalendarOrder
{
    /// <summary>The left value is strictly earlier.</summary>
    Less = -1,
    /// <summary>The values denote the same timeline point.</summary>
    Equal = 0,
    /// <summary>The left value is strictly later.</summary>
    Greater = 1,
    /// <summary>Timezone uncertainty prevents a definite order.</summary>
    Indeterminate = 2
}

/// <summary>Provides opt-in exact calendar comparison without changing legacy execution or authority.</summary>
public static class XdmExactCalendarComparison
{
    private static readonly int[] MonthStarts = [0,31,59,90,120,151,181,212,243,273,304,334];
    /// <summary>Compares compatible calendar values using an explicit XPath implicit timezone.</summary>
    /// <param name="left">Initialized left calendar.</param>
    /// <param name="right">Initialized right calendar of a compatible family.</param>
    /// <param name="implicitTimezoneMinutes">Required context offset from -840 through840 minutes.</param>
    /// <returns>Minus one, zero or one for earlier, equal or later.</returns>
    /// <exception cref="ArgumentException">An operand is default or families are incompatible.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The context offset exceeds fourteen hours.</exception>
    public static int CompareXPath(XdmExactCalendarValue left, XdmExactCalendarValue right, int implicitTimezoneMinutes)
    {
        if (implicitTimezoneMinutes is < -840 or > 840) throw new ArgumentOutOfRangeException(nameof(implicitTimezoneMinutes));
        Compatible(left,right);
        return Compare(Point(left,implicitTimezoneMinutes),Point(right,implicitTimezoneMinutes));
    }
    /// <summary>Compares compatible values using XSD partial ordering without an ambient timezone.</summary>
    /// <param name="left">Initialized left calendar.</param>
    /// <param name="right">Initialized right calendar of a compatible family.</param>
    /// <returns>A definite order or Indeterminate for timezone uncertainty.</returns>
    /// <exception cref="ArgumentException">An operand is default or families are incompatible.</exception>
    public static XdmExactCalendarOrder CompareXsd(XdmExactCalendarValue left, XdmExactCalendarValue right)
    {
        Compatible(left,right);
        if (left.HasTimezone == right.HasTimezone) return (XdmExactCalendarOrder)Compare(Point(left,0),Point(right,0));
        // Earliest uses +14:00 and latest uses -14:00. Only strict separation is definite.
        var leftMin=Point(left,840);var leftMax=Point(left,-840);
        var rightMin=Point(right,840);var rightMax=Point(right,-840);
        if (Compare(leftMax,rightMin)<0) return XdmExactCalendarOrder.Less;
        if (Compare(leftMin,rightMax)>0) return XdmExactCalendarOrder.Greater;
        return XdmExactCalendarOrder.Indeterminate;
    }
    /// <summary>Tests same-key equivalence for exact calendar objects independently of implicit timezone.</summary>
    /// <param name="left">Initialized left calendar.</param>
    /// <param name="right">Initialized right calendar.</param>
    /// <returns>False for different families/presence; otherwise exact timeline equality.</returns>
    /// <exception cref="ArgumentException">An operand is default.</exception>
    public static bool SameKey(XdmExactCalendarValue left, XdmExactCalendarValue right)
    {
        Validate(left,nameof(left));Validate(right,nameof(right));
        return IsTime(left)==IsTime(right) && left.HasTimezone==right.HasTimezone && Point(left,0).Equals(Point(right,0));
    }
    /// <summary>Hashes exact same-key identity; this is not a stable serialization or authority fingerprint.</summary>
    /// <param name="value">Initialized exact calendar.</param>
    /// <returns>A process-local hash consistent with SameKey.</returns>
    /// <exception cref="ArgumentException">The operand is default.</exception>
    public static int GetSameKeyHashCode(XdmExactCalendarValue value)
    {
        Validate(value,nameof(value));return HashCode.Combine(IsTime(value),value.HasTimezone,Point(value,0));
    }
    private static bool IsTime(XdmExactCalendarValue value)=>value.Kind==XdmExactCalendarKind.Time;
    private static void Validate(XdmExactCalendarValue value,string parameter)
    {
        if (value.Kind is not (XdmExactCalendarKind.DateTime or XdmExactCalendarKind.Time or XdmExactCalendarKind.DateTimeStamp))
            throw new ArgumentException("Initialized exact calendar required.",parameter);
    }
    private static void Compatible(XdmExactCalendarValue left,XdmExactCalendarValue right)
    {
        Validate(left,nameof(left));Validate(right,nameof(right));
        if (IsTime(left)!=IsTime(right)) throw new ArgumentException("Time and dateTime comparison families are incompatible.",nameof(right));
    }
    private static BigInteger Floor(BigInteger value,int divisor)
    {
        var quotient=BigInteger.DivRem(value,divisor,out var remainder);return remainder.Sign<0?quotient-1:quotient;
    }
    private static XdmExactDecimal Point(XdmExactCalendarValue value,int absentOffset)
    {
        bool time=IsTime(value);BigInteger year=time?1972:value.Year;
        int month=time?12:value.Month,day=time?31:value.Day;
        BigInteger previous=year-1;
        BigInteger days=365*previous+Floor(previous,4)-Floor(previous,100)+Floor(previous,400)+MonthStarts[month-1]+day-1;
        if (month>2 && year%4==0 && (year%100!=0 || year%400==0)) days++;
        int zone=value.HasTimezone?value.TimezoneOffsetMinutes:absentOffset;
        BigInteger whole=days*86400+value.Hour*3600+value.Minute*60-zone*60;
        var second=value.Second;
        // Input years/seconds already have fixed bounds; at most ~283 coefficient digits result.
        return new XdmExactDecimal(whole*BigInteger.Pow(10,second.Scale)+second.Coefficient,second.Scale,320,256);
    }
    private static int Compare(XdmExactDecimal left,XdmExactDecimal right)
    {
        int scale=Math.Max(left.Scale,right.Scale);
        return Math.Sign((left.Coefficient*BigInteger.Pow(10,scale-left.Scale)).CompareTo(right.Coefficient*BigInteger.Pow(10,scale-right.Scale)));
    }
}

/// <summary>Provides an explicit CLR same-key comparer for exact calendar objects only.</summary>
public sealed class XdmExactCalendarKeyComparer : IEqualityComparer<XdmExactCalendarValue>
{
    private XdmExactCalendarKeyComparer() { }
    /// <summary>Gets the shared immutable comparer; existing XdmValue maps are unchanged.</summary>
    public static readonly XdmExactCalendarKeyComparer Instance = new();
    /// <summary>Tests exact same-key identity without granting authority or using ambient timezone.</summary>
    /// <param name="left">Initialized left calendar.</param>
    /// <param name="right">Initialized right calendar.</param>
    /// <returns>Whether the operands are equivalent keys.</returns>
    /// <exception cref="ArgumentException">An operand is default.</exception>
    public bool Equals(XdmExactCalendarValue left,XdmExactCalendarValue right)=>XdmExactCalendarComparison.SameKey(left,right);
    /// <summary>Hashes exact same-key identity consistently with Equals.</summary>
    /// <param name="value">Initialized calendar.</param>
    /// <returns>A process-local hash.</returns>
    /// <exception cref="ArgumentException">The operand is default.</exception>
    public int GetHashCode(XdmExactCalendarValue value)=>XdmExactCalendarComparison.GetSameKeyHashCode(value);
}
