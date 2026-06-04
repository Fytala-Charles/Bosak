// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : XPath-compliant IComparer<XdmValue> for fn:sort and xsl:sort.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Compares two <see cref="XdmValue"/> instances according to XPath 3.1 ordering rules.
/// Used by <c>fn:sort</c> and <c>xsl:sort</c>.
/// </summary>
public sealed class XdmValueComparer : IComparer<XdmValue>
{
    /// <summary>
    /// The singleton instance (comparer is stateless).
    /// </summary>
    public static XdmValueComparer Instance { get; } = new();

    private XdmValueComparer() { }

    public int Compare(XdmValue x, XdmValue y)
    {
        var a = Atomize(x);
        var b = Atomize(y);

        if (a.IsUndefined && b.IsUndefined)
            return 0;
        if (a.IsUndefined)
            return -1;
        if (b.IsUndefined)
            return 1;

        bool aIsNumeric = IsNumeric(a);
        bool bIsNumeric = IsNumeric(b);

        if (aIsNumeric && bIsNumeric)
        {
            return CompareNumeric(a, b);
        }

        if (a.Kind == XdmValueKind.String && b.Kind == XdmValueKind.String)
        {
            return string.CompareOrdinal(a.StringValue, b.StringValue);
        }

        if (a.Kind == XdmValueKind.Boolean && b.Kind == XdmValueKind.Boolean)
        {
            return a.BooleanValue.CompareTo(b.BooleanValue);
        }

        // Date/time equality checking is already in VmEngine; ordering is deferred.
        if (IsDateTime(a) && IsDateTime(b))
        {
            var aSub = GetDateTimeSubtype(a);
            var bSub = GetDateTimeSubtype(b);
            if (aSub != bSub)
                throw new InvalidOperationException("XPTY0004");
            return string.CompareOrdinal(a.ToString(), b.ToString());
        }

        // Duration ordering only for same subtype
        if (a.Kind == XdmValueKind.Duration && b.Kind == XdmValueKind.Duration)
        {
            return CompareDuration(a, b);
        }

        // QName comparison: only equality is defined; ordering is not
        if (a.Kind == XdmValueKind.QName || b.Kind == XdmValueKind.QName)
            throw new InvalidOperationException("XPTY0004");

        // Mixed incomparable types
        throw new InvalidOperationException("XPTY0004");
    }

    private static int CompareNumeric(XdmValue a, XdmValue b)
    {
        bool aIsNaN = IsNaN(a);
        bool bIsNaN = IsNaN(b);
        if (aIsNaN && bIsNaN) return 0;
        if (aIsNaN) return 1;
        if (bIsNaN) return 1;

        int aRank = NumericRank(a);
        int bRank = NumericRank(b);
        int maxRank = Math.Max(aRank, bRank);

        return maxRank switch
        {
            1 => a.IntegerValue.CompareTo(b.IntegerValue),
            2 => ToDecimal(a).CompareTo(ToDecimal(b)),
            3 => ToFloat(a).CompareTo(ToFloat(b)),
            _ => ToDouble(a).CompareTo(ToDouble(b))
        };
    }

    private static bool IsNaN(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Float || value.Kind == XdmValueKind.Double)
            return double.IsNaN(value.DoubleValue);
        return false;
    }

    private static int NumericRank(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => 1,
        XdmValueKind.Decimal => 2,
        XdmValueKind.Float => 3,
        XdmValueKind.Double => 4,
        _ => 0
    };

    private static bool IsNumeric(XdmValue value) => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;

    private static bool IsDateTime(XdmValue value) => value.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time
        || (value.Kind == XdmValueKind.String && !string.IsNullOrEmpty(value.SchemaTypeName));

    private static string? GetDateTimeSubtype(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.DateTime => "dateTime",
            XdmValueKind.Date => "date",
            XdmValueKind.Time => "time",
            XdmValueKind.String => value.SchemaTypeName?.ToLowerInvariant() switch
            {
                "gyear" => "gYear",
                "gyearmonth" => "gYearMonth",
                "gmonthday" => "gMonthDay",
                "gday" => "gDay",
                "gmonth" => "gMonth",
                _ => null
            },
            _ => null
        };
    }

    private static int CompareDuration(XdmValue a, XdmValue b)
    {
        return string.CompareOrdinal(a.ToString(), b.ToString());
    }

    private static double ToDouble(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => (double)value.DecimalValue,
        XdmValueKind.Float => value.DoubleValue,
        XdmValueKind.Double => value.DoubleValue,
        _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to double")
    };

    private static float ToFloat(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => (float)value.DecimalValue,
        XdmValueKind.Float or XdmValueKind.Double => (float)value.DoubleValue,
        _ => float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : throw new InvalidOperationException($"Cannot convert {value.Kind} to float")
    };

    private static decimal ToDecimal(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => value.DecimalValue,
        XdmValueKind.Float or XdmValueKind.Double => (decimal)value.DoubleValue,
        _ => decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to decimal")
    };

    private static XdmValue Atomize(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);

        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = Materialize(value.SequenceValue);
            if (items.Length == 1)
                return Atomize(items[0]);
            if (items.Length == 0)
                return XdmValue.Undefined;
            return Atomize(items[0]);
        }

        return value;
    }

    private static XdmValue[] Materialize(IXdmSequence sequence)
    {
        var list = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(sequence))
            list.Add(item);
        return list.ToArray();
    }
}
