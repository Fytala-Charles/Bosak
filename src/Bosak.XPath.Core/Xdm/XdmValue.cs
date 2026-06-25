// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents any value in the XQuery Data Model. This is a discriminated-union struct optimized for...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added Map and Array value support                                                      |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added DateTime, Date, and Time value factories and accessors                           |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added QName value factory and accessor                                                   |
//                      | Charles Korthout | 0.5   | 22-05-2026     | Added Duration value factory, accessor, and ToString support                             |
//                      | Charles Korthout | 0.6   | 22-05-2026     | Added FormatXPathFloat, fixed FormatXPathDouble exponent and negative zero               |
//                      | Charles Korthout | 0.6   | 23-05-2026     | Fixed decimal ToString invariant culture; added XPath canonical double formatting        |
//                      | Charles Korthout | 0.7   | 08-06-2026     | Fixed FormatXPathDouble/Float stripping trailing zeros from whole numbers (e.g. 50→5)   |
//                      | Charles Korthout | 0.8   | 25-06-2026     | FromNode(null) returns Undefined to prevent null-node context-item bugs                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Represents any value in the XQuery Data Model.
/// This is a discriminated-union struct optimized for inline storage of
/// small values (booleans, integers, doubles) and unboxed reference storage
/// for nodes, sequences, functions, and strings.
/// </summary>
public readonly struct XdmValue
{
    private readonly XdmValueKind _kind;
    private readonly long _integer;
    private readonly double _double;
    private readonly object? _reference;
    private readonly string? _schemaTypeName;

    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    private XdmValue(XdmValueKind kind, long integer = 0, double @double = 0, object? reference = null, string? schemaTypeName = null)
    {
        _kind = kind;
        _integer = integer;
        _double = @double;
        _reference = reference;
        _schemaTypeName = schemaTypeName;
    }

    public static XdmValue Undefined => new(XdmValueKind.Undefined);
    public static XdmValue True => new(XdmValueKind.Boolean, integer: 1);
    public static XdmValue False => new(XdmValueKind.Boolean, integer: 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromBoolean(bool value) => value ? True : False;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromInteger(long value) => new(XdmValueKind.Integer, integer: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDouble(double value) => new(XdmValueKind.Double, @double: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromFloat(float value) => new(XdmValueKind.Float, @double: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDecimal(decimal value) => new(XdmValueKind.Decimal, reference: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromString(string value) => new(XdmValueKind.String, reference: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromString(string value, string schemaTypeName) => new(XdmValueKind.String, reference: value, schemaTypeName: schemaTypeName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDuration(string value) => new(XdmValueKind.Duration, reference: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromNode(IXdmNode node) => node != null ? new(XdmValueKind.Node, reference: node) : Undefined;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromSequence(XdmSequence sequence)
        => new(XdmValueKind.Sequence, reference: sequence.Source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromMap(XdmMap map) => new(XdmValueKind.Map, reference: map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromArray(XdmArray array) => new(XdmValueKind.Array, reference: array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromFunction(object functionItem)
        => new(XdmValueKind.Function, reference: functionItem);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromExternal(object externalObject)
        => new(XdmValueKind.External, reference: externalObject);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(DateTimeOffset value)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value, hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(DateTimeOffset value)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value, hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(DateTimeOffset value)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value, hasTimezone));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromQName(XsQName value)
        => new(XdmValueKind.QName, reference: value);

    // ------------------------------------------------------------------
    // Accessors
    // ------------------------------------------------------------------

    public XdmValueKind Kind => _kind;
    public string? SchemaTypeName => _schemaTypeName;

    public bool IsUndefined => _kind == XdmValueKind.Undefined;
    public bool IsAtomic => _kind is >= XdmValueKind.String and <= XdmValueKind.Uri;
    public bool IsNode => _kind == XdmValueKind.Node;
    public bool IsSequence => _kind == XdmValueKind.Sequence;
    public bool IsFunction => _kind == XdmValueKind.Function;
    public bool IsMap => _kind == XdmValueKind.Map;
    public bool IsArray => _kind == XdmValueKind.Array;

    public bool BooleanValue
    {
        get
        {
            if (_kind != XdmValueKind.Boolean)
                ThrowInvalidAccess(nameof(BooleanValue));
            return _integer != 0;
        }
    }

    public long IntegerValue
    {
        get
        {
            if (_kind != XdmValueKind.Integer)
                ThrowInvalidAccess(nameof(IntegerValue));
            return _integer;
        }
    }

    public double DoubleValue
    {
        get
        {
            if (_kind != XdmValueKind.Double && _kind != XdmValueKind.Float)
                ThrowInvalidAccess(nameof(DoubleValue));
            return _double;
        }
    }

    public decimal DecimalValue
    {
        get
        {
            if (_kind != XdmValueKind.Decimal)
                ThrowInvalidAccess(nameof(DecimalValue));
            return (decimal)_reference!;
        }
    }

    public string StringValue
    {
        get
        {
            if (_kind != XdmValueKind.String)
                ThrowInvalidAccess(nameof(StringValue));
            return (string)_reference!;
        }
    }

    public string DurationValue
    {
        get
        {
            if (_kind != XdmValueKind.Duration)
                ThrowInvalidAccess(nameof(DurationValue));
            return (string)_reference!;
        }
    }

    public IXdmNode NodeValue
    {
        get
        {
            if (_kind != XdmValueKind.Node)
                ThrowInvalidAccess(nameof(NodeValue));
            return (IXdmNode)_reference!;
        }
    }

    public IXdmSequence? SequenceValue
    {
        get
        {
            if (_kind != XdmValueKind.Sequence)
                ThrowInvalidAccess(nameof(SequenceValue));
            return (IXdmSequence?)_reference;
        }
    }

    public object? ExternalValue
    {
        get
        {
            if (_kind != XdmValueKind.External)
                ThrowInvalidAccess(nameof(ExternalValue));
            return _reference;
        }
    }

    public object FunctionValue
    {
        get
        {
            if (_kind != XdmValueKind.Function)
                ThrowInvalidAccess(nameof(FunctionValue));
            return _reference!;
        }
    }

    public XdmMap MapValue
    {
        get
        {
            if (_kind != XdmValueKind.Map)
                ThrowInvalidAccess(nameof(MapValue));
            return (XdmMap)_reference!;
        }
    }

    public XdmArray ArrayValue
    {
        get
        {
            if (_kind != XdmValueKind.Array)
                ThrowInvalidAccess(nameof(ArrayValue));
            return (XdmArray)_reference!;
        }
    }

    public DateTimeOffset DateTimeValue
    {
        get
        {
            if (_kind != XdmValueKind.DateTime)
                ThrowInvalidAccess(nameof(DateTimeValue));
            return _reference is DateTimeWrapper w ? w.Value.ToDateTimeOffset() : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true).ToDateTimeOffset();
        }
    }

    public DateTimeOffset DateValue
    {
        get
        {
            if (_kind != XdmValueKind.Date)
                ThrowInvalidAccess(nameof(DateValue));
            return _reference is DateTimeWrapper w ? w.Value.ToDateTimeOffset() : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true).ToDateTimeOffset();
        }
    }

    public DateTimeOffset TimeValue
    {
        get
        {
            if (_kind != XdmValueKind.Time)
                ThrowInvalidAccess(nameof(TimeValue));
            return _reference is DateTimeWrapper w ? w.Value.ToDateTimeOffset() : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true).ToDateTimeOffset();
        }
    }

    /// <summary>
    /// Returns the underlying <see cref="XPathDateTime"/> for dateTime values,
    /// including support for extended years that cannot be represented by <see cref="DateTimeOffset"/>.
    /// </summary>
    public XPathDateTime DateTimeXPathValue
    {
        get
        {
            if (_kind != XdmValueKind.DateTime)
                ThrowInvalidAccess(nameof(DateTimeXPathValue));
            return _reference is DateTimeWrapper w ? w.Value : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true);
        }
    }

    public XPathDateTime DateXPathValue
    {
        get
        {
            if (_kind != XdmValueKind.Date)
                ThrowInvalidAccess(nameof(DateXPathValue));
            return _reference is DateTimeWrapper w ? w.Value : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true);
        }
    }

    public XPathDateTime TimeXPathValue
    {
        get
        {
            if (_kind != XdmValueKind.Time)
                ThrowInvalidAccess(nameof(TimeXPathValue));
            return _reference is DateTimeWrapper w ? w.Value : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true);
        }
    }

    public bool HasTimezone
    {
        get
        {
            if (_kind is not (XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time))
                return false;
            return _reference is not DateTimeWrapper w || w.HasTimezone;
        }
    }

    public XsQName QNameValue
    {
        get
        {
            if (_kind != XdmValueKind.QName)
                ThrowInvalidAccess(nameof(QNameValue));
            return (XsQName)_reference!;
        }
    }

    /// <summary>
    /// Returns the effective boolean value per XPath/XQuery semantics.
    /// </summary>
    public bool EffectiveBooleanValue()
    {
        return _kind switch
        {
            XdmValueKind.Boolean => _integer != 0,
            XdmValueKind.String => !string.IsNullOrEmpty((string?)_reference),
            XdmValueKind.Integer => _integer != 0,
            XdmValueKind.Decimal => (decimal)_reference! != 0m,
            XdmValueKind.Double or XdmValueKind.Float => _double != 0.0 && !double.IsNaN(_double),
            XdmValueKind.Sequence => _reference is not null && ((IXdmSequence)_reference).TryGetLength(out var len) && len > 0,
            XdmValueKind.Node => true,
            _ => false
        };
    }

    public override string ToString()
    {
        return _kind switch
        {
            XdmValueKind.Undefined => "()",
            XdmValueKind.Boolean => _integer != 0 ? "true" : "false",
            XdmValueKind.Integer => _integer.ToString(),
            XdmValueKind.Decimal => FormatCanonicalDecimal((decimal)_reference!),
            XdmValueKind.Double => FormatXPathDouble(_double),
            XdmValueKind.Float => FormatXPathFloat((float)_double),
            XdmValueKind.String => (string?)_reference ?? string.Empty,
            XdmValueKind.Node => ((IXdmNode?)_reference)?.StringValue ?? string.Empty,
            XdmValueKind.Sequence => "(sequence)",
            XdmValueKind.Function => "(function)",
            XdmValueKind.Map => "(map)",
            XdmValueKind.Array => "(array)",
            XdmValueKind.DateTime => FormatXPathDateTime(DateTimeXPathValue, true),
            XdmValueKind.Date => FormatXPathDateTime(DateXPathValue, false),
            XdmValueKind.Time => FormatXPathTime(TimeXPathValue),
            XdmValueKind.QName => ((XsQName)_reference!).ToString(),
            XdmValueKind.Duration => (string?)_reference ?? string.Empty,
            XdmValueKind.External => $"(external: {_reference?.GetType().Name})",
            _ => $"(kind: {_kind})"
        };
    }

    private void ThrowInvalidAccess(string propertyName)
        => throw new InvalidOperationException($"Cannot access {propertyName} on XDM value of kind '{_kind}'");

    private static string FormatExponent(string s)
        => s.Replace("E+", "E");

    private static string FormatCanonicalDecimal(decimal value)
    {
        string s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
        {
            s = s.TrimEnd('0').TrimEnd('.');
        }
        return string.IsNullOrEmpty(s) ? "0" : s;
    }

    private static string FormatXPathDouble(double value)
    {
        if (double.IsPositiveInfinity(value)) return "INF";
        if (double.IsNegativeInfinity(value)) return "-INF";
        if (double.IsNaN(value)) return "NaN";
        // Preserve negative zero
        if (value == 0.0) return double.IsNegative(value) ? "-0" : "0";

        double abs = Math.Abs(value);
        // XPath canonical double uses scientific notation when abs >= 1e6 or abs < 1e-6
        if (abs >= 1e6 || abs < 1e-6)
        {
            // E16 gives 16 digits after decimal = 17 significant digits, round-trip for double
            string s = value.ToString("E16", CultureInfo.InvariantCulture);
            return NormalizeScientific(s);
        }
        // For non-scientific range, use round-trip format and trim trailing zeros
        string r = value.ToString("R", CultureInfo.InvariantCulture);
        if (r.Contains('E') || r.Contains('e'))
            return NormalizeScientific(r);
        if (r.Contains('.'))
        {
            r = r.TrimEnd('0').TrimEnd('.');
            if (r == "-0") r = "0";
        }
        return r;
    }

    private static string NormalizeScientific(string s)
    {
        s = s.Replace("E+", "E");
        int eIdx = s.IndexOf('E');
        if (eIdx < 0) eIdx = s.IndexOf('e');
        if (eIdx > 0)
        {
            string mantissa = s[..eIdx];
            string exp = s[(eIdx + 1)..];
            mantissa = mantissa.TrimEnd('0').TrimEnd('.');
            if (!mantissa.Contains('.')) mantissa += ".0";
            bool neg = exp.StartsWith('-');
            if (neg) exp = exp[1..];
            exp = exp.TrimStart('+').TrimStart('0');
            if (string.IsNullOrEmpty(exp)) exp = "0";
            if (neg) exp = "-" + exp;
            s = mantissa + "E" + exp;
        }
        return s;
    }

    private static string FormatXPathFloat(float value)
    {
        if (float.IsPositiveInfinity(value)) return "INF";
        if (float.IsNegativeInfinity(value)) return "-INF";
        if (float.IsNaN(value)) return "NaN";
        if (value == 0.0f) return float.IsNegative(value) ? "-0" : "0";

        float abs = Math.Abs(value);
        // XPath canonical float uses scientific notation when abs >= 1e6 or abs < 1e-6
        if (abs >= 1e6f || abs < 1e-6f)
        {
            // E7 gives 7 digits after decimal = 8 significant digits, round-trip for float
            string s = value.ToString("E7", CultureInfo.InvariantCulture);
            return NormalizeScientific(s);
        }
        // For non-scientific range, use round-trip format and trim trailing zeros
        string r = value.ToString("R", CultureInfo.InvariantCulture);
        if (r.Contains('E') || r.Contains('e'))
            return NormalizeScientific(r);
        if (r.Contains('.'))
        {
            r = r.TrimEnd('0').TrimEnd('.');
            if (r == "-0") r = "0";
        }
        return r;
    }

    private static string FormatXPathDateTime(XPathDateTime xdt, bool includeTime)
    {
        string result = xdt.FormatYear();
        if (includeTime)
            result += $"-{xdt.Month:00}-{xdt.Day:00}T{xdt.Hour:00}:{xdt.Minute:00}:{FormatXPathSeconds(xdt.Second, xdt.Millisecond)}";
        else
            result += $"-{xdt.Month:00}-{xdt.Day:00}";
        if (xdt.HasTimezone)
            result += xdt.FormatTimezone();
        return result;
    }

    private static string FormatXPathTime(XPathDateTime xdt)
    {
        int hour = xdt.Hour;
        if (hour == 24) hour = 0;
        string result = $"{hour:00}:{xdt.Minute:00}:{FormatXPathSeconds(xdt.Second, xdt.Millisecond)}";
        if (xdt.HasTimezone)
            result += xdt.FormatTimezone();
        return result;
    }

    private static string FormatXPathSeconds(int second, int millisecond)
    {
        if (millisecond == 0) return $"{second:00}";
        string frac = millisecond.ToString("000").TrimEnd('0');
        return $"{second:00}.{frac}";
    }

    private static string FormatDateTimeOffset(DateTimeOffset dto, string format)
    {
        string result = dto.ToString(format + "zzz", System.Globalization.CultureInfo.InvariantCulture);
        return result.Replace("+00:00", "Z");
    }
}

/// <summary>
/// Wraps an <see cref="XPathDateTime"/> together with a flag indicating whether the original
/// XPath literal included an explicit timezone. Used by xs:date, xs:time, and xs:dateTime.
/// </summary>
internal sealed class DateTimeWrapper
{
    public XPathDateTime Value { get; }
    public bool HasTimezone { get; }
    public DateTimeWrapper(XPathDateTime value, bool hasTimezone)
    {
        Value = value;
        HasTimezone = hasTimezone;
    }
}
