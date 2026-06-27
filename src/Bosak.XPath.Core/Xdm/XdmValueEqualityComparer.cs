// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 mei 2026
// PURPOSE              : Equality comparer for XdmValue map keys supporting numeric promotion.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 26-06-2026     | String/URI equality and NaN-safe hashing for map keys                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Equality comparer for <see cref="XdmValue"/> used as map keys.
/// Supports numeric type promotion (integer, decimal, float, double are comparable).
/// </summary>
public sealed class XdmValueEqualityComparer : IEqualityComparer<XdmValue>
{
    public static readonly XdmValueEqualityComparer Instance = new();

    public bool Equals(XdmValue x, XdmValue y)
    {
        if (IsNumeric(x) && IsNumeric(y))
            return NumericEquals(x, y);

        if (x.Kind == y.Kind)
        {
            return x.Kind switch
            {
                XdmValueKind.Undefined => true,
                XdmValueKind.Boolean => x.BooleanValue == y.BooleanValue,
                XdmValueKind.String => x.StringValue == y.StringValue,
                XdmValueKind.Uri => x.StringValue == y.StringValue,
                XdmValueKind.DateTime => x.DateTimeValue == y.DateTimeValue,
                XdmValueKind.Date => x.DateValue == y.DateValue,
                XdmValueKind.Time => x.TimeValue == y.TimeValue,
                XdmValueKind.QName => x.QNameValue.Equals(y.QNameValue),
                _ => false
            };
        }

        // xs:anyURI values are comparable to xs:string values for map-key purposes.
        if ((x.Kind == XdmValueKind.String && y.Kind == XdmValueKind.Uri)
            || (x.Kind == XdmValueKind.Uri && y.Kind == XdmValueKind.String))
        {
            return x.StringValue == y.StringValue;
        }

        return false;
    }

    public int GetHashCode(XdmValue obj)
    {
        if (IsNumeric(obj))
        {
            // NaN cannot be cast to decimal; all NaN values share a fixed hash so
            // that NaN double and NaN float compare equal as map keys.
            if ((obj.Kind == XdmValueKind.Double || obj.Kind == XdmValueKind.Float) && double.IsNaN(obj.DoubleValue))
                return 0x7FC00000;

            // Normalize all numeric values to decimal for consistent hashing
            decimal d = obj.Kind switch
            {
                XdmValueKind.Integer => obj.IntegerValue,
                XdmValueKind.Decimal => obj.DecimalValue,
                XdmValueKind.Float => (decimal)obj.DoubleValue,
                XdmValueKind.Double => (decimal)obj.DoubleValue,
                _ => 0m
            };
            return d.GetHashCode();
        }

        return obj.Kind switch
        {
            XdmValueKind.Boolean => obj.BooleanValue.GetHashCode(),
            XdmValueKind.String => obj.StringValue.GetHashCode(),
            XdmValueKind.Uri => obj.StringValue.GetHashCode(),
            XdmValueKind.DateTime => obj.DateTimeValue.GetHashCode(),
            XdmValueKind.Date => obj.DateValue.GetHashCode(),
            XdmValueKind.Time => obj.TimeValue.GetHashCode(),
            XdmValueKind.QName => obj.QNameValue.GetHashCode(),
            _ => (int)obj.Kind
        };
    }

    private static bool IsNumeric(XdmValue value)
        => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double;

    private static bool NumericEquals(XdmValue a, XdmValue b)
    {
        // NaN equals NaN for map key purposes (consistent with deep-equal)
        bool aIsNaN = a.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(a.DoubleValue);
        bool bIsNaN = b.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(b.DoubleValue);
        if (aIsNaN && bIsNaN)
            return true;

        // If either is double, promote both to double
        if (a.Kind == XdmValueKind.Double || b.Kind == XdmValueKind.Double)
        {
            double da = a.Kind == XdmValueKind.Integer ? a.IntegerValue :
                        a.Kind == XdmValueKind.Decimal ? (double)a.DecimalValue :
                        a.Kind == XdmValueKind.Float ? a.DoubleValue : a.DoubleValue;
            double db = b.Kind == XdmValueKind.Integer ? b.IntegerValue :
                        b.Kind == XdmValueKind.Decimal ? (double)b.DecimalValue :
                        b.Kind == XdmValueKind.Float ? b.DoubleValue : b.DoubleValue;
            return da == db;
        }

        // If either is float, promote both to float
        if (a.Kind == XdmValueKind.Float || b.Kind == XdmValueKind.Float)
        {
            float fa = a.Kind == XdmValueKind.Integer ? a.IntegerValue :
                       a.Kind == XdmValueKind.Decimal ? (float)a.DecimalValue : (float)a.DoubleValue;
            float fb = b.Kind == XdmValueKind.Integer ? b.IntegerValue :
                       b.Kind == XdmValueKind.Decimal ? (float)b.DecimalValue : (float)b.DoubleValue;
            return fa == fb;
        }

        // Both are integer or decimal
        decimal ma = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.DecimalValue;
        decimal mb = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.DecimalValue;
        return ma == mb;
    }
}
