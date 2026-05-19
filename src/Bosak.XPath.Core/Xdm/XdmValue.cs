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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
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

    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    private XdmValue(XdmValueKind kind, long integer = 0, double @double = 0, object? reference = null)
    {
        _kind = kind;
        _integer = integer;
        _double = @double;
        _reference = reference;
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
    public static XdmValue FromDecimal(decimal value) => new(XdmValueKind.Decimal, reference: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromString(string value) => new(XdmValueKind.String, reference: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XdmValue FromNode(IXdmNode node) => new(XdmValueKind.Node, reference: node);

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

    // ------------------------------------------------------------------
    // Accessors
    // ------------------------------------------------------------------

    public XdmValueKind Kind => _kind;

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
            XdmValueKind.Decimal => ((decimal)_reference!).ToString(),
            XdmValueKind.Double => _double.ToString(),
            XdmValueKind.Float => _double.ToString(),
            XdmValueKind.String => (string?)_reference ?? string.Empty,
            XdmValueKind.Node => ((IXdmNode?)_reference)?.StringValue ?? string.Empty,
            XdmValueKind.Sequence => "(sequence)",
            XdmValueKind.Function => "(function)",
            XdmValueKind.Map => "(map)",
            XdmValueKind.Array => "(array)",
            XdmValueKind.External => $"(external: {_reference?.GetType().Name})",
            _ => $"(kind: {_kind})"
        };
    }

    private void ThrowInvalidAccess(string propertyName)
        => throw new InvalidOperationException($"Cannot access {propertyName} on XDM value of kind '{_kind}'");
}
