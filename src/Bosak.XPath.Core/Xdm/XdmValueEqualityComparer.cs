// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 mei 2026
// PURPOSE              : Equality comparer for XdmValue map keys supporting numeric promotion.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
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
//                      | Charles Korthout | 0.5   | 25-07-2026     | Map keys distinguish string-family subtypes from g* date types                         |
//                      | Charles Korthout | 0.6   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 04-11-2026     | Compare/hash normalized exact numeric pairs without scale-expanded strings (1.49.0).     |
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
    /// <summary>The shared singleton instance.</summary>
    public static readonly XdmValueEqualityComparer Instance = new();

    /// <summary>Compares two values with op:same-key semantics (numeric promotion, string/URI comparability).</summary>
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
                XdmValueKind.String => x.StringValue == y.StringValue && StringFamily(x) == StringFamily(y),
                XdmValueKind.Uri => x.StringValue == y.StringValue && StringFamily(x) == StringFamily(y),
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

    /// <summary>Returns a hash code consistent with op:same-key equality, so equal keys hash alike.</summary>
    public int GetHashCode(XdmValue obj)
    {
        if (IsNumeric(obj))
        {
            // Normalized exact pairs share hashes without padding a supplied decimal scale.
            // Hash codes remain process-local implementation details.
            return ExactNumericKey.FromValue(obj).GetHashCode();
        }

        return obj.Kind switch
        {
            XdmValueKind.Boolean => obj.BooleanValue.GetHashCode(),
            XdmValueKind.String => HashCode.Combine(obj.StringValue, StringFamily(obj)),
            XdmValueKind.Uri => HashCode.Combine(obj.StringValue, StringFamily(obj)),
            XdmValueKind.DateTime => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.Date => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.Time => HashCode.Combine(InstantKey(obj), obj.HasTimezone),
            XdmValueKind.QName => obj.QNameValue.GetHashCode(),
            XdmValueKind.Duration => DurationHash(obj.DurationValue),
            _ => (int)obj.Kind
        };
    }

    // Map-key type family for string/URI values: the string-family subtypes and
    // xs:anyURI share one family; the g* date types (and any other named type) form
    // their own, so xs:gYear('2015') is not the same key as xs:string('2015')
    // (Serialization-adaptive-31).
    private static string StringFamily(XdmValue value)
        => value.SchemaTypeName is null || StringFamilyNames.Contains(value.SchemaTypeName)
            ? ""
            : value.SchemaTypeName;

    private static readonly HashSet<string> StringFamilyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "", "string", "untypedAtomic", "anyURI",
        "normalizedString", "token", "language", "NMTOKEN", "NMTOKENS",
        "Name", "NCName", "ID", "IDREF", "IDREFS", "ENTITY", "ENTITIES",
        "ncname", "id", "idref", "idrefs", "entity", "entities", "nmtoken", "nmtokens",
        "name", "normalizedstring"
    };

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
    private static bool NumericEquals(XdmValue a,XdmValue b)
        => ExactNumericKey.FromValue(a).Equals(ExactNumericKey.FromValue(b));

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
