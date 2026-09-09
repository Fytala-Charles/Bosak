// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents any value in the XQuery Data Model. This is a discriminated-union struct optimized for...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
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
//                      | Charles Korthout | 0.8   | 15-07-2026     | FormatXPathDouble expands R-scientific to fixed-point inside the decimal range (1e-6)   |
//                      | Charles Korthout | 0.8   | 25-06-2026     | FromNode(null) returns Undefined to prevent null-node context-item bugs                |
//                      | Charles Korthout | 0.9   | 26-06-2026     | Fixed EffectiveBooleanValue for singleton/multi-item sequences                         |
//                      | Charles Korthout | 1.0   | 27-06-2026     | Use shortest round-trip format (G17/G9) for XPath double/float scientific notation      |
//                      | Charles Korthout | 1.1   | 26-06-2026     | Use shortest round-trip (\"R\") format for xs:float scientific notation                  |
//                      | Charles Korthout | 1.2   | 15-07-2026     | EffectiveBooleanValue raises FORG0006 for maps, arrays, and function items             |
//                      | Charles Korthout | 1.3   | 15-07-2026     | EBV: xs:anyURI string-like (non-empty); FORG0006 for date/time/duration/QName/binary    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 15-07-2026     | Tier-2k: annotated FromInteger/FromDuration overloads; EBV FORG0006 for String-kind hexBinary/base64Binary/gYear-family annotations |
//                      | Charles Korthout | 1.5   | 19-07-2026     | Added FromDecimal with schemaTypeName for xs:unsignedLong overflow values               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 19-07-2026     | FormatXPathDouble uses G17 to preserve round-trip digits for MAX_VALUE                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.7   | 20-07-2026     | EBV multi-item sequence: FORG0006 when first item is not a node                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 20-07-2026     | FormatXPathDouble/Float use R and decimal-point exponent for fixed-point scientific     |
//                      | Charles Korthout | 2.0   | 21-07-2026     | Added FromDateTime overload with schemaTypeName for xs:dateTimeStamp                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 20-07-2026     | FormatXPathFloat expands R-scientific to fixed-point inside the decimal range (1e-6)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.1   | 25-07-2026     | FromDecimal normalizes negative zero (XPath decimals have no -0)                        |
//                      | Charles Korthout | 2.2   | 22-08-2026     | Format gYear/gMonth/gDay/etc. schema-type annotations correctly in ToString           |
//                      | Charles Korthout | 2.3   | 23-08-2026     | Added FromQName(XsQName, string schemaTypeName) overload for schema-typed QNames/NOTATIONs |
//                      | Charles Korthout | 2.4   | 09-09-2026     | ThrowInvalidAccess prefixes XPTY0004 (function-argument type errors surface the declared |
//                      | Charles Korthout | 2.5   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

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

    /// <summary>The undefined (absent) value, used for out-of-range lookups and empty results.</summary>
    public static XdmValue Undefined => new(XdmValueKind.Undefined);
    /// <summary>The singleton xs:boolean true value.</summary>
    public static XdmValue True => new(XdmValueKind.Boolean, integer: 1);
    /// <summary>The singleton xs:boolean false value.</summary>
    public static XdmValue False => new(XdmValueKind.Boolean, integer: 0);

    /// <summary>Creates an xs:boolean value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromBoolean(bool value) => value ? True : False;

    /// <summary>Creates an xs:integer value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromInteger(long value) => new(XdmValueKind.Integer, integer: value);

    /// <summary>Creates an integer-family value with a derived-type annotation (e.g. xs:long, xs:byte).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromInteger(long value, string schemaTypeName) => new(XdmValueKind.Integer, integer: value, schemaTypeName: schemaTypeName);

    /// <summary>Creates an xs:double value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDouble(double value) => new(XdmValueKind.Double, @double: value);

    /// <summary>Creates an xs:float value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromFloat(float value) => new(XdmValueKind.Float, @double: value);

    /// <summary>Creates an xs:decimal value (negative zero is normalized).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDecimal(decimal value) => new(XdmValueKind.Decimal, reference: NormalizeZero(value));

    /// <summary>Creates a decimal-family value with a derived-type annotation (e.g. xs:unsignedLong overflow).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDecimal(decimal value, string schemaTypeName) => new(XdmValueKind.Decimal, reference: NormalizeZero(value), schemaTypeName: schemaTypeName);

    // XPath decimals have no negative zero; the .NET decimal type preserves a -0.0 sign
    // that would leak into casts and string conversions. Clear it, preserving scale.
    private static decimal NormalizeZero(decimal value) =>
        value == 0m && decimal.GetBits(value)[3] < 0 ? new decimal(0, 0, 0, false, value.Scale) : value;

    /// <summary>Creates an xs:string value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromString(string value) => new(XdmValueKind.String, reference: value);

    /// <summary>Creates a string-family value with a schema type annotation (e.g. xs:hexBinary, xs:gYear).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromString(string value, string schemaTypeName) => new(XdmValueKind.String, reference: value, schemaTypeName: schemaTypeName);

    /// <summary>Creates an xs:duration value from its lexical representation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDuration(string value) => new(XdmValueKind.Duration, reference: value);

    /// <summary>Creates a duration-family value with a derived-type annotation (e.g. xs:dayTimeDuration).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDuration(string value, string schemaTypeName) => new(XdmValueKind.Duration, reference: value, schemaTypeName: schemaTypeName);

    /// <summary>Creates a node value; a null node yields <see cref="Undefined"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromNode(IXdmNode node) => node != null ? new(XdmValueKind.Node, reference: node) : Undefined;

    /// <summary>Creates a sequence value wrapping the given sequence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromSequence(XdmSequence sequence)
        => new(XdmValueKind.Sequence, reference: sequence.Source);

    /// <summary>Creates a map value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromMap(XdmMap map) => new(XdmValueKind.Map, reference: map);

    /// <summary>Creates an array value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromArray(XdmArray array) => new(XdmValueKind.Array, reference: array);

    /// <summary>Creates a function-item value wrapping the given function item.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromFunction(object functionItem)
        => new(XdmValueKind.Function, reference: functionItem);

    /// <summary>Creates a value wrapping an opaque external .NET object.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromExternal(object externalObject)
        => new(XdmValueKind.External, reference: externalObject);

    /// <summary>Creates an xs:dateTime value with a timezone.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(DateTimeOffset value)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    /// <summary>Creates an xs:dateTime value; hasTimezone records whether a timezone was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    /// <summary>Creates a dateTime-family value with a schema type annotation (e.g. xs:dateTimeStamp).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(DateTimeOffset value, bool hasTimezone, string schemaTypeName)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone), schemaTypeName: schemaTypeName);

    /// <summary>Creates an xs:dateTime value from an extended-year date/time.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value, hasTimezone));

    /// <summary>Creates a dateTime-family value from an extended-year date/time with a schema type annotation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDateTime(XPathDateTime value, bool hasTimezone, string schemaTypeName)
        => new(XdmValueKind.DateTime, reference: new DateTimeWrapper(value, hasTimezone), schemaTypeName: schemaTypeName);

    /// <summary>Creates an xs:date value with a timezone.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(DateTimeOffset value)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    /// <summary>Creates an xs:date value; hasTimezone records whether a timezone was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    /// <summary>Creates an xs:date value from an extended-year date/time.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromDate(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.Date, reference: new DateTimeWrapper(value, hasTimezone));

    /// <summary>Creates an xs:time value with a timezone.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(DateTimeOffset value)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone: true), hasTimezone: true));

    /// <summary>Creates an xs:time value; hasTimezone records whether a timezone was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(DateTimeOffset value, bool hasTimezone)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value.ToXPathDateTime(hasTimezone), hasTimezone));

    /// <summary>Creates an xs:time value from an extended-year date/time.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromTime(XPathDateTime value, bool hasTimezone)
        => new(XdmValueKind.Time, reference: new DateTimeWrapper(value, hasTimezone));

    /// <summary>Creates an xs:QName value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromQName(XsQName value)
        => new(XdmValueKind.QName, reference: value);

    /// <summary>Creates a QName value with a schema type annotation (e.g. xs:NOTATION).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromQName(XsQName value, string schemaTypeName)
        => new(XdmValueKind.QName, reference: value, schemaTypeName: schemaTypeName);

    // ------------------------------------------------------------------
    // Accessors
    // ------------------------------------------------------------------

    /// <summary>Gets the kind discriminator of this value.</summary>
    public XdmValueKind Kind => _kind;
    /// <summary>Gets the schema type annotation local name (e.g. integer, dateTimeStamp), or null when unannotated.</summary>
    public string? SchemaTypeName => _schemaTypeName;

    /// <summary>Returns whether this is the undefined (absent) value.</summary>
    public bool IsUndefined => _kind == XdmValueKind.Undefined;
    /// <summary>Returns whether this value is an atomic value (String through Uri kinds).</summary>
    public bool IsAtomic => _kind is >= XdmValueKind.String and <= XdmValueKind.Uri;
    /// <summary>Returns whether this value is a node.</summary>
    public bool IsNode => _kind == XdmValueKind.Node;
    /// <summary>Returns whether this value is a sequence.</summary>
    public bool IsSequence => _kind == XdmValueKind.Sequence;
    /// <summary>Returns whether this value is a function item.</summary>
    public bool IsFunction => _kind == XdmValueKind.Function;
    /// <summary>Returns whether this value is a map.</summary>
    public bool IsMap => _kind == XdmValueKind.Map;
    /// <summary>Returns whether this value is an array.</summary>
    public bool IsArray => _kind == XdmValueKind.Array;

    /// <summary>Gets the boolean payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Boolean"/>.</summary>
    public bool BooleanValue
    {
        get
        {
            if (_kind != XdmValueKind.Boolean)
                ThrowInvalidAccess(nameof(BooleanValue));
            return _integer != 0;
        }
    }

    /// <summary>Gets the integer payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Integer"/>.</summary>
    public long IntegerValue
    {
        get
        {
            if (_kind != XdmValueKind.Integer)
                ThrowInvalidAccess(nameof(IntegerValue));
            return _integer;
        }
    }

    /// <summary>Gets the double payload; valid for <see cref="XdmValueKind.Double"/> and <see cref="XdmValueKind.Float"/> values.</summary>
    public double DoubleValue
    {
        get
        {
            if (_kind != XdmValueKind.Double && _kind != XdmValueKind.Float)
                ThrowInvalidAccess(nameof(DoubleValue));
            return _double;
        }
    }

    /// <summary>Gets the decimal payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Decimal"/>.</summary>
    public decimal DecimalValue
    {
        get
        {
            if (_kind != XdmValueKind.Decimal)
                ThrowInvalidAccess(nameof(DecimalValue));
            return (decimal)_reference!;
        }
    }

    /// <summary>Gets the string payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.String"/>.</summary>
    public string StringValue
    {
        get
        {
            if (_kind != XdmValueKind.String)
                ThrowInvalidAccess(nameof(StringValue));
            return (string)_reference!;
        }
    }

    /// <summary>Gets the duration lexical form; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Duration"/>.</summary>
    public string DurationValue
    {
        get
        {
            if (_kind != XdmValueKind.Duration)
                ThrowInvalidAccess(nameof(DurationValue));
            return (string)_reference!;
        }
    }

    /// <summary>Gets the node payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Node"/>.</summary>
    public IXdmNode NodeValue
    {
        get
        {
            if (_kind != XdmValueKind.Node)
                ThrowInvalidAccess(nameof(NodeValue));
            return (IXdmNode)_reference!;
        }
    }

    /// <summary>Gets the underlying sequence source; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Sequence"/>.</summary>
    public IXdmSequence? SequenceValue
    {
        get
        {
            if (_kind != XdmValueKind.Sequence)
                ThrowInvalidAccess(nameof(SequenceValue));
            return (IXdmSequence?)_reference;
        }
    }

    /// <summary>Gets the external .NET object; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.External"/>.</summary>
    public object? ExternalValue
    {
        get
        {
            if (_kind != XdmValueKind.External)
                ThrowInvalidAccess(nameof(ExternalValue));
            return _reference;
        }
    }

    /// <summary>Gets the function item payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Function"/>.</summary>
    public object FunctionValue
    {
        get
        {
            if (_kind != XdmValueKind.Function)
                ThrowInvalidAccess(nameof(FunctionValue));
            return _reference!;
        }
    }

    /// <summary>Gets the map payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Map"/>.</summary>
    public XdmMap MapValue
    {
        get
        {
            if (_kind != XdmValueKind.Map)
                ThrowInvalidAccess(nameof(MapValue));
            return (XdmMap)_reference!;
        }
    }

    /// <summary>Gets the array payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.Array"/>.</summary>
    public XdmArray ArrayValue
    {
        get
        {
            if (_kind != XdmValueKind.Array)
                ThrowInvalidAccess(nameof(ArrayValue));
            return (XdmArray)_reference!;
        }
    }

    /// <summary>Gets the dateTime payload as a <see cref="DateTimeOffset"/>; throws for extended years outside its range.</summary>
    public DateTimeOffset DateTimeValue
    {
        get
        {
            if (_kind != XdmValueKind.DateTime)
                ThrowInvalidAccess(nameof(DateTimeValue));
            return _reference is DateTimeWrapper w ? w.Value.ToDateTimeOffset() : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true).ToDateTimeOffset();
        }
    }

    /// <summary>Gets the date payload as a <see cref="DateTimeOffset"/>; throws for extended years outside its range.</summary>
    public DateTimeOffset DateValue
    {
        get
        {
            if (_kind != XdmValueKind.Date)
                ThrowInvalidAccess(nameof(DateValue));
            return _reference is DateTimeWrapper w ? w.Value.ToDateTimeOffset() : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true).ToDateTimeOffset();
        }
    }

    /// <summary>Gets the time payload as a <see cref="DateTimeOffset"/>; throws for extended years outside its range.</summary>
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

    /// <summary>
    /// Returns the underlying <see cref="XPathDateTime"/> for date values,
    /// including support for extended years that cannot be represented by <see cref="DateTimeOffset"/>.
    /// </summary>
    public XPathDateTime DateXPathValue
    {
        get
        {
            if (_kind != XdmValueKind.Date)
                ThrowInvalidAccess(nameof(DateXPathValue));
            return _reference is DateTimeWrapper w ? w.Value : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true);
        }
    }

    /// <summary>
    /// Returns the underlying <see cref="XPathDateTime"/> for time values,
    /// including support for extended years that cannot be represented by <see cref="DateTimeOffset"/>.
    /// </summary>
    public XPathDateTime TimeXPathValue
    {
        get
        {
            if (_kind != XdmValueKind.Time)
                ThrowInvalidAccess(nameof(TimeXPathValue));
            return _reference is DateTimeWrapper w ? w.Value : ((DateTimeOffset)_reference!).ToXPathDateTime(hasTimezone: true);
        }
    }

    /// <summary>Returns whether a date/time value carries an explicit timezone; false for other kinds.</summary>
    public bool HasTimezone
    {
        get
        {
            if (_kind is not (XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time))
                return false;
            return _reference is not DateTimeWrapper w || w.HasTimezone;
        }
    }

    /// <summary>Gets the QName payload; valid only when <see cref="Kind"/> is <see cref="XdmValueKind.QName"/>.</summary>
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
    /// Maps, arrays, and function items have no effective boolean value (FORG0006).
    /// </summary>
    public bool EffectiveBooleanValue()
    {
        return _kind switch
        {
            XdmValueKind.Boolean => _integer != 0,
            // Only string-family annotations (xs:string and derived, xs:untypedAtomic,
            // xs:anyURI) have a string-like EBV; values annotated hexBinary/base64Binary/
            // gYear etc. are not in the string group and raise FORG0006.
            XdmValueKind.String => IsEbvStringLike(_schemaTypeName)
                ? !string.IsNullOrEmpty((string?)_reference)
                : throw new InvalidOperationException(
                    "FORG0006: The effective boolean value is not defined for values of this atomic type"),
            XdmValueKind.Integer => _integer != 0,
            XdmValueKind.Decimal => (decimal)_reference! != 0m,
            XdmValueKind.Double or XdmValueKind.Float => _double != 0.0 && !double.IsNaN(_double),
            XdmValueKind.Sequence => SequenceEffectiveBooleanValue(),
            XdmValueKind.Node => true,
            // xs:anyURI is in the string-like group: EBV is true iff the value is non-empty.
            XdmValueKind.Uri => !string.IsNullOrEmpty((string?)_reference),
            XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime or XdmValueKind.Duration
                or XdmValueKind.QName or XdmValueKind.Binary =>
                throw new InvalidOperationException(
                    "FORG0006: The effective boolean value is not defined for values of this atomic type"),
            XdmValueKind.Function or XdmValueKind.Map or XdmValueKind.Array =>
                throw new InvalidOperationException(
                    "FORG0006: The effective boolean value is not defined for maps, arrays, and function items"),
            _ => false
        };
    }

    /// <summary>
    /// Returns whether a schema type annotation belongs to the string-like group for EBV
    /// (xs:string and its derived types, xs:untypedAtomic, xs:anyURI).
    /// </summary>
    private static bool IsEbvStringLike(string? schemaTypeName)
    {
        if (schemaTypeName is null) return true;
        return schemaTypeName.ToLowerInvariant() is
            "string" or "untypedatomic" or "anyuri"
            or "normalizedstring" or "token" or "language" or "nmtoken" or "name"
            or "ncname" or "id" or "idref" or "entity";
    }

    /// <summary>
    /// Computes the effective boolean value of a sequence per XPath 3.1 §2.4.3.
    /// </summary>
    private bool SequenceEffectiveBooleanValue()
    {
        if (_reference is not IXdmSequence seq)
            return false;

        int length = 0;
        if (seq.TryGetLength(out var knownLength))
        {
            length = knownLength;
        }
        else
        {
            foreach (var _ in XdmSequence.FromSource(seq))
                length++;
        }

        if (length == 0)
            return false;

        if (length == 1)
        {
            foreach (var item in XdmSequence.FromSource(seq))
                return item.EffectiveBooleanValue();
            return false;
        }

        // A sequence of more than one item has an effective boolean value of true
        // if its first item is a node; otherwise it is a type error (FORG0006).
        foreach (var item in XdmSequence.FromSource(seq))
        {
            if (item.IsNode)
                return true;
            throw new InvalidOperationException(
                "FORG0006: Invalid argument type for fn:boolean() / effective boolean value");
        }

        throw new InvalidOperationException(
            "FORG0006: Invalid argument type for fn:boolean() / effective boolean value");
    }

    /// <inheritdoc/>
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
            XdmValueKind.DateTime => FormatDateTimeBySchemaType(DateTimeXPathValue, _schemaTypeName),
            XdmValueKind.Date => FormatXPathDateTime(DateXPathValue, false),
            XdmValueKind.Time => FormatXPathTime(TimeXPathValue),
            XdmValueKind.QName => ((XsQName)_reference!).ToString(),
            XdmValueKind.Duration => (string?)_reference ?? string.Empty,
            XdmValueKind.External => $"(external: {_reference?.GetType().Name})",
            _ => $"(kind: {_kind})"
        };
    }

    private void ThrowInvalidAccess(string propertyName)
        => throw new InvalidOperationException($"XPTY0004: Cannot access {propertyName} on XDM value of kind '{_kind}'");

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
            // "R" gives the shortest round-trippable form for double values. We then force
            // scientific notation for values whose magnitude requires XPath canonical
            // representation (e.g. 1000001 must serialize as 1.000001E6, not 1000001).
            string s = value.ToString("R", CultureInfo.InvariantCulture);
            if (!s.Contains('E') && !s.Contains('e') && abs >= 1e6)
            {
                // The round-trip form is fixed-point (e.g. 1230000 or 6553503.2); normalize to
                // a single leading digit and an exponent. The exponent is derived from the
                // position of the decimal point, not the total digit count, because the round-trip
                // string may contain a fractional part whose digits must not inflate the exponent.
                bool negative = s.StartsWith('-');
                var signed = negative ? s[1..] : s;
                var digits = signed.Replace(".", "");
                int decimalPos = signed.IndexOf('.');
                if (decimalPos < 0) decimalPos = digits.Length;
                int exponent = decimalPos - 1;
                string mantissa = digits.Insert(1, ".");
                mantissa = mantissa.TrimEnd('0').TrimEnd('.');
                if (!mantissa.Contains('.')) mantissa += ".0";
                s = (negative ? "-" : "") + mantissa + "E" + exponent;
            }
            return NormalizeScientific(s);
        }
        // For non-scientific range, use round-trip format and trim trailing zeros
        string r = value.ToString("R", CultureInfo.InvariantCulture);
        if (r.Contains('E') || r.Contains('e'))
        {
            // "R" may choose scientific notation inside the decimal range (e.g. for
            // 1e-6); XPath requires fixed-point notation for 1e-6 <= |x| < 1e6.
            r = ExpandScientificToFixed(r);
        }
        if (r.Contains('.'))
        {
            r = r.TrimEnd('0').TrimEnd('.');
            if (r == "-0") r = "0";
        }
        return r;
    }

    /// <summary>
    /// Expands a scientific-notation double string (e.g. "1E-06", "1.23E-05") to
    /// fixed-point notation, preserving the shortest-round-trip digits.
    /// </summary>
    private static string ExpandScientificToFixed(string s)
    {
        bool negative = s.StartsWith('-');
        if (negative) s = s[1..];
        int eIdx = s.IndexOf('E');
        if (eIdx < 0) eIdx = s.IndexOf('e');
        string mantissa = s[..eIdx];
        int exponent = int.Parse(s[(eIdx + 1)..], CultureInfo.InvariantCulture);
        var digits = mantissa.Replace(".", "");
        // The decimal point starts after the first digit and shifts by the exponent.
        int pointPos = 1 + exponent;
        var sb = new StringBuilder();
        if (negative) sb.Append('-');
        if (pointPos <= 0)
        {
            sb.Append("0.");
            sb.Append('0', -pointPos);
            sb.Append(digits);
        }
        else if (pointPos >= digits.Length)
        {
            sb.Append(digits);
            sb.Append('0', pointPos - digits.Length);
        }
        else
        {
            sb.Append(digits[..pointPos]);
            sb.Append('.');
            sb.Append(digits[pointPos..]);
        }
        return sb.ToString();
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
            // "R" gives the shortest round-trippable form for float values.
            string s = value.ToString("R", CultureInfo.InvariantCulture);
            if (!s.Contains('E') && !s.Contains('e') && abs >= 1e6f)
            {
                bool negative = s.StartsWith('-');
                var signed = negative ? s[1..] : s;
                var digits = signed.Replace(".", "");
                int decimalPos = signed.IndexOf('.');
                if (decimalPos < 0) decimalPos = digits.Length;
                int exponent = decimalPos - 1;
                string mantissa = digits.Insert(1, ".");
                mantissa = mantissa.TrimEnd('0').TrimEnd('.');
                if (!mantissa.Contains('.')) mantissa += ".0";
                s = (negative ? "-" : "") + mantissa + "E" + exponent;
            }
            return NormalizeScientific(s);
        }
        // For non-scientific range, use round-trip format and trim trailing zeros
        string r = value.ToString("R", CultureInfo.InvariantCulture);
        if (r.Contains('E') || r.Contains('e'))
        {
            // "R" may choose scientific notation inside the decimal range (e.g. for
            // 1e-5); XPath requires fixed-point notation for 1e-6 <= |x| < 1e6.
            r = ExpandScientificToFixed(r);
        }
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

    /// <summary>
    /// Formats a dateTime-stored value according to its schema type annotation.
    /// gYear, gYearMonth, gMonth, gMonthDay, and gDay values are stored internally
    /// as a full <see cref="XPathDateTime"/> but must serialize in their specific form.
    /// </summary>
    private static string FormatDateTimeBySchemaType(XPathDateTime xdt, string? schemaTypeName)
    {
        return schemaTypeName?.ToLowerInvariant() switch
        {
            "gyear" => $"{xdt.FormatYear()}{xdt.FormatTimezone()}",
            "gyearmonth" => $"{xdt.FormatYear()}-{xdt.Month:00}{xdt.FormatTimezone()}",
            "gmonth" => $"--{xdt.Month:00}{xdt.FormatTimezone()}",
            "gmonthday" => $"--{xdt.Month:00}-{xdt.Day:00}{xdt.FormatTimezone()}",
            "gday" => $"---{xdt.Day:00}{xdt.FormatTimezone()}",
            _ => FormatXPathDateTime(xdt, true)
        };
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
