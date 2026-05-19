// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The standard XPath / XQuery function library (fn, math, map, array, xs)
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Frozen;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// The standard XPath / XQuery function library (fn, math, map, array, xs).
/// </summary>
public static class FunctionLibrary
{
    private static readonly FrozenDictionary<(string ns, string name, int arity), FunctionSignature> StandardFunctions;

    static FunctionLibrary()
    {
        var functions = new Dictionary<(string, string, int), FunctionSignature>
        {
            // ----- fn:string --------------------------------------------------
            [(Namespaces.Fn, "string", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = String_0
            },
            [(Namespaces.Fn, "string", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = String_1
            },

            // ----- fn:concat --------------------------------------------------
            [(Namespaces.Fn, "concat", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = Concat
            },

            // ----- fn:count ---------------------------------------------------
            [(Namespaces.Fn, "count", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "count",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Integer,
                Implementation = Count
            },

            // ----- fn:position / fn:last --------------------------------------
            [(Namespaces.Fn, "position", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "position",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Integer,
                Implementation = Position
            },
            [(Namespaces.Fn, "last", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "last",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Integer,
                Implementation = Last
            },

            // ----- fn:exists --------------------------------------------------
            [(Namespaces.Fn, "exists", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "exists",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Exists
            },

            // ----- fn:empty ---------------------------------------------------
            [(Namespaces.Fn, "empty", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "empty",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Empty
            },

            // ----- fn:head ----------------------------------------------------
            [(Namespaces.Fn, "head", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "head",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Head
            },

            // ----- fn:tail ----------------------------------------------------
            [(Namespaces.Fn, "tail", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "tail",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Tail
            },

            // ----- fn:not -----------------------------------------------------
            [(Namespaces.Fn, "not", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "not",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Boolean],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Not
            },

            // ----- fn:true / fn:false -----------------------------------------
            [(Namespaces.Fn, "true", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "true",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = (_, _) => XdmValue.True
            },
            [(Namespaces.Fn, "false", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "false",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = (_, _) => XdmValue.False
            },
        };

        StandardFunctions = functions.ToFrozenDictionary();
    }

    /// <summary>
    /// Populates the evaluation context with all standard functions.
    /// </summary>
    public static void Populate(EvaluationContext context)
    {
        foreach (var sig in StandardFunctions.Values)
        {
            context.RegisterFunction(sig);
        }
    }

    /// <summary>
    /// Attempts to resolve a standard function by qualified name and arity.
    /// </summary>
    public static bool TryGetFunction(string namespaceUri, string localName, int arity, out FunctionSignature signature)
        => StandardFunctions.TryGetValue((namespaceUri, localName, arity), out signature!);

    // ------------------------------------------------------------------
    // Implementations
    // ------------------------------------------------------------------

    private static XdmValue String_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:string() called with no context item.");
        return XdmValue.FromString(item.ToString());
    }

    private static XdmValue String_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsSequence)
        {
            // fn:string on a sequence takes the first item
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
                return XdmValue.FromString(item.ToString());
            return XdmValue.FromString(string.Empty);
        }
        return XdmValue.FromString(arg.ToString());
    }

    private static XdmValue Concat(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(args[0].ToString() + args[1].ToString());

    private static XdmValue Count(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return XdmValue.FromInteger(1);

        if (seq.SequenceValue!.TryGetLength(out var len))
            return XdmValue.FromInteger(len);

        // Materialize to count
        long count = 0;
        foreach (var _ in XdmSequence.FromSource(seq.SequenceValue!))
            count++;
        return XdmValue.FromInteger(count);
    }

    private static XdmValue Exists(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(!args[0].IsUndefined && args[0].EffectiveBooleanValue());

    private static XdmValue Empty(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].IsUndefined || !args[0].EffectiveBooleanValue());

    private static XdmValue Head(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return seq;

        foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
            return item;

        return XdmValue.Undefined;
    }

    private static XdmValue Tail(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return XdmValue.FromSequence(XdmSequence.Empty);

        // TODO: Return a lazy sequence view skipping the first item.
        // For now, materialize.
        var list = new List<XdmValue>();
        bool first = true;
        foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
        {
            if (first) { first = false; continue; }
            list.Add(item);
        }
        return XdmValue.FromSequence(Bosak.XPath.Core.Xdm.MaterializedSequence.FromList(list));
    }

    private static XdmValue Not(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(!args[0].EffectiveBooleanValue());

    private static XdmValue Position(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(ctx.ContextPosition);

    private static XdmValue Last(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(ctx.ContextSize);
}

file static class Namespaces
{
    public const string Fn = "http://www.w3.org/2005/xpath-functions";
    public const string Math = "http://www.w3.org/2005/xpath-functions/math";
    public const string Map = "http://www.w3.org/2005/xpath-functions/map";
    public const string Array = "http://www.w3.org/2005/xpath-functions/array";
}


