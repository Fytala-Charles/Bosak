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
//                      | Charles Korthout | 0.3   | 15-07-2026     | Exact numeric comparison (no lossy promotion) and duration keys via normalized totals    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 25-07-2026     | Date/time keys require same timezone presence; throw-safe UTC instant keys             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

using System.Globalization;
using System.Numerics;

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
                XdmValueKind.DateTime => x.HasTimezone == y.HasTimezone && InstantKey(x) == InstantKey(y),
                XdmValueKind.Date => x.HasTimezone == y.HasTimezone && InstantKey(x) == InstantKey(y),
                XdmValueKind.Time => x.HasTimezone == y.HasTimezone && InstantKey(x) == InstantKey(y),
                XdmValueKind.QName => x.QNameValue.Equals(y.QNameValue),
                XdmValueKind.Duration => DurationsEqual(x.DurationValue, y.DurationValue),
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
            // Hash the canonical exact-decimal key string so that numerically equal
            // keys (integer 1, decimal 1.0, double 1.0) share a hash code.
            return NumericKeyString(obj).GetHashCode();
        }

        return obj.Kind switch
        {
            XdmValueKind.Boolean => obj.BooleanValue.GetHashCode(),
            XdmValueKind.String => obj.StringValue.GetHashCode(),
            XdmValueKind.Uri => obj.StringValue.GetHashCode(),
            XdmValueKind.DateTime => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.Date => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.Time => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.QName => obj.QNameValue.GetHashCode(),
            XdmValueKind.Duration => DurationHash(obj.DurationValue),
            _ => (int)obj.Kind
        };
    }

    private static bool IsNumeric(XdmValue value)
        => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double;

    /// <summary>
    /// UTC-normalized instant of a date/time key as milliseconds since 0001-01-01, computed
    /// with civil-date arithmetic so year-1 values with positive timezones do not overflow
    /// the .NET DateTimeOffset range. Values without a timezone are used as-is.
    /// </summary>
    private static long InstantKey(XdmValue value)
    {
        var xdt = value.Kind switch
        {
            XdmValueKind.DateTime => value.DateTimeXPathValue,
            XdmValueKind.Date => value.DateXPathValue,
            XdmValueKind.Time => value.TimeXPathValue,
            _ => default
        };
        var utc = XPathDateTimeHelper.NormalizeToUtc(xdt);
        long days = XPathDateTimeHelper.DaysFromCivil(utc.Year, utc.Month, utc.Day);
        return days * 86_400_000L + ((long)utc.Hour * 3600 + utc.Minute * 60 + utc.Second) * 1000 + utc.Millisecond;
    }

    /// <summary>
    /// Numeric map-key equality (op:same-key semantics): values compare by their exact
    /// mathematical value, without any rounding or loss of precision. A binary
    /// floating-point key is therefore only the same key as an integer/decimal key
    /// when its exact decimal expansion matches (same-key-007: xs:double('1.1') is
    /// NOT the same key as xs:decimal('1.1')).
    /// </summary>
    private static bool NumericEquals(XdmValue a, XdmValue b)
        => NumericKeyString(a) == NumericKeyString(b);

    /// <summary>
    /// Canonical exact decimal representation of a numeric key, used for both equality
    /// and hashing. NaN and infinities map to fixed sentinels.
    /// </summary>
    private static string NumericKeyString(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue.ToString(CultureInfo.InvariantCulture),
            XdmValueKind.Decimal => CanonicalDecimalString(value.DecimalValue),
            XdmValueKind.Float or XdmValueKind.Double => ExactDoubleKeyString(value.DoubleValue),
            _ => string.Empty
        };

    private static string ExactDoubleKeyString(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "INF";
        if (double.IsNegativeInfinity(d)) return "-INF";
        return ExactDecimalString(d);
    }

    private static string CanonicalDecimalString(decimal m)
    {
        var s = m.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    /// <summary>
    /// Exact decimal expansion of a finite IEEE-754 double, with trailing zeros
    /// removed (e.g. 1.1d → "1.100000000000000088817841970012523233890533447265625").
    /// </summary>
    private static string ExactDecimalString(double d)
    {
        long bits = BitConverter.DoubleToInt64Bits(d);
        bool negative = bits < 0;
        long rawExp = (bits >> 52) & 0x7FF;
        long frac = bits & 0xFFFFFFFFFFFFF;

        BigInteger mantissa;
        int exp;
        if (rawExp == 0)
        {
            mantissa = frac; // subnormal
            exp = -1074;
        }
        else
        {
            mantissa = frac | (1L << 52);
            exp = (int)rawExp - 1075;
        }

        string digits;
        int scale;
        if (exp >= 0)
        {
            digits = (mantissa << exp).ToString(CultureInfo.InvariantCulture);
            scale = 0;
        }
        else
        {
            int k = -exp;
            // mantissa / 2^k == mantissa * 5^k / 10^k — an exact finite decimal.
            digits = (mantissa * BigInteger.Pow(5, k)).ToString(CultureInfo.InvariantCulture);
            scale = k;
        }

        string s;
        if (scale == 0)
            s = digits;
        else if (digits.Length > scale)
            s = digits.Insert(digits.Length - scale, ".");
        else
            s = "0." + new string('0', scale - digits.Length) + digits;

        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        if (s.Length == 0)
            s = "0";
        return negative && s != "0" ? "-" + s : s;
    }

    /// <summary>
    /// Duration equality uses the normalized (months, seconds) totals so that e.g.
    /// xs:duration('P1Y') and xs:yearMonthDuration('P12M') are the same key.
    /// </summary>
    private static bool DurationsEqual(string a, string b)
    {
        var na = XPathDateTimeHelper.NormalizeDuration(a);
        var nb = XPathDateTimeHelper.NormalizeDuration(b);
        return na.TotalMonths == nb.TotalMonths && na.TotalSeconds == nb.TotalSeconds;
    }

    private static int DurationHash(string lexical)
    {
        var n = XPathDateTimeHelper.NormalizeDuration(lexical);
        return (n.TotalMonths, n.TotalSeconds).GetHashCode();
    }
}
