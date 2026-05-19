// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A register-based virtual machine that interprets <see cref="IrModule"/> instructions
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
using System.Globalization;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// A register-based virtual machine that interprets <see cref="IrModule"/> instructions.
/// </summary>
public static class VmEngine
{
    /// <summary>
    /// Executes a compiled IR module against the given evaluation context.
    /// </summary>
    public static XdmValue Execute(IrModule module, EvaluationContext context)
    {
        // The lowerer uses monotonic register allocation; 256 is generous for most expressions.
        var registers = new XdmValue[256];
        var (result, _) = ExecuteBlock(module, context, registers, 0);
        return result;
    }

    private static (XdmValue Result, int NextIp) ExecuteBlock(
        IrModule module,
        EvaluationContext context,
        XdmValue[] registers,
        int startIp)
    {
        var instructions = module.Instructions;
        var literalPool = module.LiteralPool;
        int ip = startIp;

        while (ip < instructions.Length)
        {
            var instr = instructions[ip];

            switch (instr.OpCode)
            {
                // ------------------------------------------------------------------
                // Control flow
                // ------------------------------------------------------------------
                case IrOpCode.Nop:
                    ip++;
                    break;

                case IrOpCode.Return:
                    return (registers[instr.RegisterA], ip + 1);

                case IrOpCode.Jump:
                    ip = instr.Operand;
                    break;

                case IrOpCode.JumpIfTrue:
                    ip = registers[instr.RegisterA].EffectiveBooleanValue() ? instr.Operand : ip + 1;
                    break;

                case IrOpCode.JumpIfFalse:
                    ip = !registers[instr.RegisterA].EffectiveBooleanValue() ? instr.Operand : ip + 1;
                    break;

                case IrOpCode.JumpIfEmpty:
                    {
                        var seq = registers[instr.RegisterA];
                        bool isEmpty = seq.IsUndefined ||
                            (seq.IsSequence && seq.SequenceValue is not null &&
                             TryGetSequenceLength(seq.SequenceValue, out var len) && len == 0);
                        ip = isEmpty ? instr.Operand : ip + 1;
                        break;
                    }

                case IrOpCode.Call:
                    {
                        string funcName = (string)literalPool[instr.Operand]!;
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.RegisterB;

                        var (localName, nsUri) = ResolveFunctionName(funcName, context);

                        if (!context.TryResolveFunction(nsUri, localName, argCount, out var sig))
                            throw new InvalidOperationException(
                                $"Function {funcName}#{argCount} not found in namespace '{nsUri}'.");

                        // Build argument span
                        XdmValue[] args = new XdmValue[argCount];
                        for (int i = 0; i < argCount; i++)
                            args[i] = registers[firstArgReg + i];

                        registers[instr.RegisterA] = sig.Implementation(context, args);
                        ip++;
                        break;
                    }

                case IrOpCode.TailCall:
                    throw new NotImplementedException("TailCall is not yet implemented.");

                // ------------------------------------------------------------------
                // Context
                // ------------------------------------------------------------------
                case IrOpCode.LoadContextItem:
                    registers[instr.RegisterA] = context.ContextItem;
                    ip++;
                    break;

                case IrOpCode.LoadContextPosition:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextPosition);
                    ip++;
                    break;

                case IrOpCode.LoadContextSize:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextSize);
                    ip++;
                    break;

                case IrOpCode.SetContext:
                    // Not used by current lowerer; placeholder for future use.
                    throw new NotImplementedException("SetContext is not yet implemented.");

                // ------------------------------------------------------------------
                // Variables
                // ------------------------------------------------------------------
                case IrOpCode.LoadVariable:
                    {
                        string varName = (string)literalPool[instr.Operand]!;
                        var (localName, nsUri) = ResolveVariableName(varName, context);

                        if (!context.TryGetVariable(localName, out var value, nsUri))
                            throw new InvalidOperationException($"Variable ${varName} is not defined.");

                        registers[instr.RegisterA] = value;
                        ip++;
                        break;
                    }

                case IrOpCode.StoreVariable:
                    throw new NotImplementedException("StoreVariable is not yet implemented.");

                // ------------------------------------------------------------------
                // Literals
                // ------------------------------------------------------------------
                case IrOpCode.LoadString:
                    registers[instr.RegisterA] = XdmValue.FromString((string)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadInteger:
                    registers[instr.RegisterA] = XdmValue.FromInteger((long)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadDecimal:
                    registers[instr.RegisterA] = XdmValue.FromDecimal((decimal)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadDouble:
                    registers[instr.RegisterA] = XdmValue.FromDouble((double)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadBoolean:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(instr.Operand != 0);
                    ip++;
                    break;

                case IrOpCode.LoadEmptySequence:
                    registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                    ip++;
                    break;

                case IrOpCode.Move:
                    registers[instr.RegisterA] = registers[instr.RegisterB];
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Sequences
                // ------------------------------------------------------------------
                case IrOpCode.SequenceStart:
                    // Store a mutable list as an external value during construction.
                    registers[instr.RegisterA] = XdmValue.FromExternal(new List<XdmValue>());
                    ip++;
                    break;

                case IrOpCode.SequenceAdd:
                    {
                        var list = (List<XdmValue>)registers[instr.RegisterA].ExternalValue!;
                        list.Add(registers[instr.RegisterB]);
                        ip++;
                        break;
                    }

                case IrOpCode.SequenceEnd:
                    {
                        var list = (List<XdmValue>)registers[instr.RegisterA].ExternalValue!;
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(list));
                        ip++;
                        break;
                    }

                case IrOpCode.Singleton:
                    registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Singleton(registers[instr.RegisterB]));
                    ip++;
                    break;

                case IrOpCode.Range:
                    {
                        long from = ToInteger(registers[instr.RegisterB]);
                        long to = ToInteger(registers[instr.RegisterC]);
                        var items = new List<XdmValue>();
                        for (long v = from; v <= to; v++)
                            items.Add(XdmValue.FromInteger(v));
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(items));
                        ip++;
                        break;
                    }

                case IrOpCode.Concatenate:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        var combined = new List<XdmValue>(left.Length + right.Length);
                        combined.AddRange(left);
                        combined.AddRange(right);
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(combined));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Axes
                // ------------------------------------------------------------------
                case IrOpCode.Child:
                case IrOpCode.Descendant:
                case IrOpCode.DescendantOrSelf:
                case IrOpCode.Ancestor:
                case IrOpCode.AncestorOrSelf:
                case IrOpCode.Attribute:
                case IrOpCode.Parent:
                case IrOpCode.Self:
                case IrOpCode.Following:
                case IrOpCode.FollowingSibling:
                case IrOpCode.Preceding:
                case IrOpCode.PrecedingSibling:
                case IrOpCode.Namespace:
                    {
                        var input = registers[instr.RegisterB];
                        var axis = ToXdmAxis(instr.OpCode);
                        registers[instr.RegisterA] = ApplyAxis(input, axis);
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Node tests
                // ------------------------------------------------------------------
                case IrOpCode.NameTest:
                    {
                        string name = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        var filtered = FilterNodes(input, n =>
                            n.LocalName == name ||
                            (name.Contains(':') && n.LocalName == name.Split(':')[1]));
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                case IrOpCode.KindTest:
                    {
                        string kindName = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        var filtered = FilterNodes(input, n => MatchesKindTest(n, kindName));
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Predicates / Filtering
                // ------------------------------------------------------------------
                case IrOpCode.Filter:
                    {
                        var sequence = registers[instr.RegisterB];
                        int predicateEntry = instr.Operand;

                        var items = MaterializeSequence(sequence);
                        var kept = new List<XdmValue>();

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            context.WithFocus(items[i], i + 1, items.Length);
                            var (predResult, _) = ExecuteBlock(module, context, registers, predicateEntry);

                            // Numeric predicate: [n] means position() = n
                            if (predResult.Kind == XdmValueKind.Integer)
                            {
                                if (predResult.IntegerValue == i + 1)
                                    kept.Add(items[i]);
                            }
                            else if (predResult.EffectiveBooleanValue())
                            {
                                kept.Add(items[i]);
                            }
                        }

                        // Restore context
                        context.WithFocus(savedItem, savedPos, savedSize);

                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(kept));
                        ip++;
                        break;
                    }

                case IrOpCode.Subscript:
                    {
                        var sequence = registers[instr.RegisterB];
                        int index = instr.Operand; // 1-based
                        var items = MaterializeSequence(sequence);

                        if (index >= 1 && index <= items.Length)
                            registers[instr.RegisterA] = items[index - 1];
                        else
                            registers[instr.RegisterA] = XdmValue.Undefined;

                        ip++;
                        break;
                    }

                case IrOpCode.First:
                    {
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);
                        registers[instr.RegisterA] = items.Length > 0 ? items[0] : XdmValue.Undefined;
                        ip++;
                        break;
                    }

                case IrOpCode.Last:
                    {
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);
                        registers[instr.RegisterA] = items.Length > 0 ? items[^1] : XdmValue.Undefined;
                        ip++;
                        break;
                    }

                case IrOpCode.Position:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextPosition);
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Comparisons (value comparisons for atomics)
                // ------------------------------------------------------------------
                case IrOpCode.Equal:
                case IrOpCode.NotEqual:
                case IrOpCode.LessThan:
                case IrOpCode.LessThanOrEqual:
                case IrOpCode.GreaterThan:
                case IrOpCode.GreaterThanOrEqual:
                case IrOpCode.ValueEqual:
                case IrOpCode.ValueNotEqual:
                case IrOpCode.ValueLessThan:
                case IrOpCode.ValueLessThanOrEqual:
                case IrOpCode.ValueGreaterThan:
                case IrOpCode.ValueGreaterThanOrEqual:
                    {
                        bool result = Compare(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC]);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.GeneralEqual:
                case IrOpCode.GeneralNotEqual:
                case IrOpCode.GeneralLessThan:
                case IrOpCode.GeneralLessThanOrEqual:
                case IrOpCode.GeneralGreaterThan:
                case IrOpCode.GeneralGreaterThanOrEqual:
                    // General comparisons have existential semantics over sequences.
                    // For now, delegate to value comparison (works for singletons).
                    {
                        bool result = CompareGeneral(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC]);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.IsSameNode:
                    {
                        bool result = registers[instr.RegisterB].IsNode &&
                                      registers[instr.RegisterC].IsNode &&
                                      ReferenceEquals(
                                          registers[instr.RegisterB].NodeValue,
                                          registers[instr.RegisterC].NodeValue);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.PrecedesNode:
                case IrOpCode.FollowsNode:
                    throw new NotImplementedException("Node ordering comparisons are not yet implemented.");

                // ------------------------------------------------------------------
                // Arithmetic
                // ------------------------------------------------------------------
                case IrOpCode.Add:
                    registers[instr.RegisterA] = Add(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.Subtract:
                    registers[instr.RegisterA] = Subtract(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.Multiply:
                    registers[instr.RegisterA] = Multiply(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.Divide:
                    registers[instr.RegisterA] = Divide(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.IntegerDivide:
                    registers[instr.RegisterA] = IntegerDivide(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.Modulo:
                    registers[instr.RegisterA] = Modulo(registers[instr.RegisterB], registers[instr.RegisterC]);
                    ip++;
                    break;

                case IrOpCode.UnaryPlus:
                    registers[instr.RegisterA] = registers[instr.RegisterB];
                    ip++;
                    break;

                case IrOpCode.UnaryMinus:
                    registers[instr.RegisterA] = Negate(registers[instr.RegisterB]);
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Boolean logic
                // ------------------------------------------------------------------
                case IrOpCode.And:
                    {
                        bool result = registers[instr.RegisterB].EffectiveBooleanValue() &&
                                      registers[instr.RegisterC].EffectiveBooleanValue();
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Or:
                    {
                        bool result = registers[instr.RegisterB].EffectiveBooleanValue() ||
                                      registers[instr.RegisterC].EffectiveBooleanValue();
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Not:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(!registers[instr.RegisterB].EffectiveBooleanValue());
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // String
                // ------------------------------------------------------------------
                case IrOpCode.StringConcat:
                    registers[instr.RegisterA] = XdmValue.FromString(
                        registers[instr.RegisterB].ToString() + registers[instr.RegisterC].ToString());
                    ip++;
                    break;

                case IrOpCode.StringLength:
                case IrOpCode.Substring:
                case IrOpCode.Contains:
                case IrOpCode.StartsWith:
                case IrOpCode.EndsWith:
                case IrOpCode.NormalizeSpace:
                case IrOpCode.Translate:
                case IrOpCode.UpperCase:
                case IrOpCode.LowerCase:
                case IrOpCode.MatchesRegex:
                case IrOpCode.ReplaceRegex:
                case IrOpCode.TokenizeRegex:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Type operations
                // ------------------------------------------------------------------
                case IrOpCode.Cast:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        registers[instr.RegisterA] = Cast(registers[instr.RegisterB], typeName);
                        ip++;
                        break;
                    }

                case IrOpCode.Castable:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        bool castable = TryCast(registers[instr.RegisterB], typeName, out _);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(castable);
                        ip++;
                        break;
                    }

                case IrOpCode.InstanceOf:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        bool instance = InstanceOf(registers[instr.RegisterB], typeName);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(instance);
                        ip++;
                        break;
                    }

                case IrOpCode.TreatAs:
                    // TreatAs is a runtime assertion; for now, just pass through.
                    registers[instr.RegisterA] = registers[instr.RegisterB];
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Sequence functions
                // ------------------------------------------------------------------
                case IrOpCode.Count:
                case IrOpCode.Exists:
                case IrOpCode.Empty:
                case IrOpCode.Head:
                case IrOpCode.Tail:
                case IrOpCode.InsertBefore:
                case IrOpCode.Remove:
                case IrOpCode.Reverse:
                case IrOpCode.Subsequence:
                case IrOpCode.DistinctValues:
                case IrOpCode.IndexOf:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Aggregation
                // ------------------------------------------------------------------
                case IrOpCode.Sum:
                case IrOpCode.Avg:
                case IrOpCode.Min:
                case IrOpCode.Max:
                case IrOpCode.StringJoin:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Higher-order (XPath 3.1)
                // ------------------------------------------------------------------
                case IrOpCode.Map:
                case IrOpCode.Array:
                case IrOpCode.Lookup:
                case IrOpCode.LookupWildcard:
                case IrOpCode.Curry:
                case IrOpCode.Apply:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Constructors
                // ------------------------------------------------------------------
                case IrOpCode.ElementConstructor:
                case IrOpCode.AttributeConstructor:
                case IrOpCode.TextConstructor:
                case IrOpCode.DocumentConstructor:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Error
                // ------------------------------------------------------------------
                case IrOpCode.Error:
                    throw new InvalidOperationException("Runtime error instruction encountered.");

                default:
                    throw new NotSupportedException($"Unsupported opcode: {instr.OpCode}");
            }
        }

        throw new InvalidOperationException("VM reached end of instructions without Return.");
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static XdmValue[] MaterializeSequence(XdmValue sequence)
    {
        if (sequence.IsUndefined)
            return Array.Empty<XdmValue>();

        if (sequence.IsSequence)
        {
            var seq = sequence.SequenceValue;
            if (seq is null)
                return Array.Empty<XdmValue>();

            var list = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(seq))
                list.Add(item);
            return list.ToArray();
        }

        return new[] { sequence };
    }

    private static bool TryGetSequenceLength(IXdmSequence? seq, out int length)
    {
        if (seq is null)
        {
            length = 0;
            return true;
        }
        return seq.TryGetLength(out length);
    }

    private static (string LocalName, string NamespaceUri) ResolveFunctionName(string funcName, EvaluationContext context)
    {
        string localName = funcName;
        string? prefix = null;
        int colon = funcName.IndexOf(':');
        if (colon >= 0)
        {
            prefix = funcName[..colon];
            localName = funcName[(colon + 1)..];
        }

        string nsUri;
        if (prefix is not null)
        {
            if (!context.TryResolveNamespace(prefix, out nsUri))
                throw new InvalidOperationException($"Unknown namespace prefix: {prefix}");
        }
        else
        {
            nsUri = "http://www.w3.org/2005/xpath-functions"; // default function namespace
        }

        return (localName, nsUri);
    }

    private static (string LocalName, string NamespaceUri) ResolveVariableName(string varName, EvaluationContext context)
    {
        string localName = varName;
        string? prefix = null;
        int colon = varName.IndexOf(':');
        if (colon >= 0)
        {
            prefix = varName[..colon];
            localName = varName[(colon + 1)..];
        }

        string nsUri = "";
        if (prefix is not null)
        {
            if (!context.TryResolveNamespace(prefix, out var resolvedNs))
                throw new InvalidOperationException($"Unknown namespace prefix: {prefix}");
            nsUri = resolvedNs;
        }

        return (localName, nsUri);
    }

    private static XdmAxis ToXdmAxis(IrOpCode opcode) => opcode switch
    {
        IrOpCode.Child => XdmAxis.Child,
        IrOpCode.Descendant => XdmAxis.Descendant,
        IrOpCode.DescendantOrSelf => XdmAxis.DescendantOrSelf,
        IrOpCode.Ancestor => XdmAxis.Ancestor,
        IrOpCode.AncestorOrSelf => XdmAxis.AncestorOrSelf,
        IrOpCode.Attribute => XdmAxis.Attribute,
        IrOpCode.Parent => XdmAxis.Parent,
        IrOpCode.Self => XdmAxis.Self,
        IrOpCode.Following => XdmAxis.Following,
        IrOpCode.FollowingSibling => XdmAxis.FollowingSibling,
        IrOpCode.Preceding => XdmAxis.Preceding,
        IrOpCode.PrecedingSibling => XdmAxis.PrecedingSibling,
        IrOpCode.Namespace => XdmAxis.Namespace,
        _ => throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null)
    };

    private static XdmValue ApplyAxis(XdmValue input, XdmAxis axis)
    {
        if (input.IsUndefined)
            return XdmValue.Undefined;

        if (input.IsNode)
            return XdmValue.FromSequence(input.NodeValue.Axis(axis));

        if (input.IsSequence)
        {
            var items = MaterializeSequence(input);
            var result = new List<XdmValue>();
            foreach (var item in items)
            {
                if (item.IsNode)
                {
                    var seq = item.NodeValue.Axis(axis);
                    foreach (var node in seq)
                        result.Add(node);
                }
            }
            return XdmValue.FromSequence(MaterializedSequence.FromList(result));
        }

        throw new InvalidOperationException(
            $"Axis {axis} requires a node or sequence of nodes, but got {input.Kind}.");
    }

    /// <summary>
    /// Atomizes an XDM value for comparison: nodes become their string value,
    /// singleton sequences are unpacked, and other values pass through.
    /// </summary>
    private static XdmValue Atomize(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);

        if (value.IsSequence)
        {
            var items = MaterializeSequence(value);
            if (items.Length == 1)
                return Atomize(items[0]);
            if (items.Length == 0)
                return XdmValue.Undefined;
            // For value comparisons with multiple items, take the first
            // (strictly this is an error in XPath, but we are lenient).
            return Atomize(items[0]);
        }

        return value;
    }

    private static XdmValue FilterNodes(XdmValue input, Func<IXdmNode, bool> predicate)
    {
        var items = MaterializeSequence(input);
        var filtered = new List<XdmValue>();
        foreach (var item in items)
        {
            if (item.IsNode && predicate(item.NodeValue))
                filtered.Add(item);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(filtered));
    }

    private static bool MatchesKindTest(IXdmNode node, string kindName)
    {
        return kindName.ToLowerInvariant() switch
        {
            "node" => true,
            "text" => node.NodeKind == XdmNodeKind.Text,
            "comment" => node.NodeKind == XdmNodeKind.Comment,
            "processing-instruction" => node.NodeKind == XdmNodeKind.ProcessingInstruction,
            "element" => node.NodeKind == XdmNodeKind.Element,
            "attribute" => node.NodeKind == XdmNodeKind.Attribute,
            "document-node" => node.NodeKind == XdmNodeKind.Document,
            "namespace-node" => node.NodeKind == XdmNodeKind.Namespace,
            _ => true // permissive fallback
        };
    }

    // ------------------------------------------------------------------
    // Arithmetic
    // ------------------------------------------------------------------

    private static XdmValue Add(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) + ToDouble(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) + ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) + ToInteger(right));
    }

    private static XdmValue Subtract(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) - ToDouble(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) - ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) - ToInteger(right));
    }

    private static XdmValue Multiply(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) * ToDouble(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) * ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) * ToInteger(right));
    }

    private static XdmValue Divide(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) / ToDouble(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) / ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) / ToInteger(right));
    }

    private static XdmValue IntegerDivide(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromInteger((long)(ToDouble(left) / ToDouble(right)));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromInteger((long)(ToDecimal(left) / ToDecimal(right)));
        return XdmValue.FromInteger(ToInteger(left) / ToInteger(right));
    }

    private static XdmValue Modulo(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) % ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) % ToInteger(right));
    }

    private static XdmValue Negate(XdmValue value)
    {
        if (IsDouble(value))
            return XdmValue.FromDouble(-ToDouble(value));
        if (IsDecimal(value))
            return XdmValue.FromDecimal(-ToDecimal(value));
        return XdmValue.FromInteger(-ToInteger(value));
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    private static bool Compare(IrOpCode op, XdmValue left, XdmValue right)
    {
        left = Atomize(left);
        right = Atomize(right);

        if (IsDouble(left) || IsDouble(right))
        {
            double l = ToDouble(left);
            double r = ToDouble(right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (IsDecimal(left) || IsDecimal(right))
        {
            decimal l = ToDecimal(left);
            decimal r = ToDecimal(right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (left.Kind == XdmValueKind.Integer && right.Kind == XdmValueKind.Integer)
        {
            long l = left.IntegerValue;
            long r = right.IntegerValue;
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // Atomized nodes become strings; try numeric parsing for untyped values
        string lStr = left.ToString();
        string rStr = right.ToString();

        if (double.TryParse(lStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lDbl) &&
            double.TryParse(rStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double rDbl))
        {
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => lDbl == rDbl,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => lDbl != rDbl,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => lDbl < rDbl,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => lDbl <= rDbl,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => lDbl > rDbl,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => lDbl >= rDbl,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        int cmp = string.CompareOrdinal(lStr, rStr);
        return op switch
        {
            IrOpCode.Equal or IrOpCode.ValueEqual => cmp == 0,
            IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp != 0,
            IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
            IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
            IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
            IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    private static bool CompareGeneral(IrOpCode op, XdmValue left, XdmValue right)
    {
        // General comparisons use existential semantics over sequences.
        // For now, materialize both sides and compare pairwise.
        var leftItems = MaterializeSequence(left);
        var rightItems = MaterializeSequence(right);

        foreach (var l in leftItems)
        {
            foreach (var r in rightItems)
            {
                bool match = op switch
                {
                    IrOpCode.GeneralEqual => Compare(IrOpCode.Equal, l, r),
                    IrOpCode.GeneralNotEqual => Compare(IrOpCode.NotEqual, l, r),
                    IrOpCode.GeneralLessThan => Compare(IrOpCode.LessThan, l, r),
                    IrOpCode.GeneralLessThanOrEqual => Compare(IrOpCode.LessThanOrEqual, l, r),
                    IrOpCode.GeneralGreaterThan => Compare(IrOpCode.GreaterThan, l, r),
                    IrOpCode.GeneralGreaterThanOrEqual => Compare(IrOpCode.GreaterThanOrEqual, l, r),
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };

                if (match)
                    return true;
            }
        }

        return false;
    }

    // ------------------------------------------------------------------
    // Type operations
    // ------------------------------------------------------------------

    private static XdmValue Cast(XdmValue value, string typeName)
    {
        if (!TryCast(value, typeName, out var result))
            throw new InvalidOperationException($"Cannot cast '{value}' to {typeName}.");
        return result;
    }

    private static bool TryCast(XdmValue value, string typeName, out XdmValue result)
    {
        result = value;
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "");

        switch (normalized)
        {
            case "string":
                result = XdmValue.FromString(value.ToString());
                return true;

            case "integer":
            case "int":
            case "long":
            case "short":
            case "byte":
                if (value.Kind == XdmValueKind.Integer)
                    return true;
                if (value.Kind == XdmValueKind.Decimal && decimal.TryParse(value.ToString(), out var dInt))
                {
                    result = XdmValue.FromInteger((long)dInt);
                    return true;
                }
                if (long.TryParse(value.ToString(), out var lInt))
                {
                    result = XdmValue.FromInteger(lInt);
                    return true;
                }
                return false;

            case "decimal":
                if (value.Kind == XdmValueKind.Decimal)
                    return true;
                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                {
                    result = XdmValue.FromDecimal(dec);
                    return true;
                }
                return false;

            case "double":
            case "float":
                if (IsDouble(value))
                    return true;
                if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
                {
                    result = XdmValue.FromDouble(dbl);
                    return true;
                }
                return false;

            case "boolean":
                result = XdmValue.FromBoolean(value.EffectiveBooleanValue());
                return true;

            default:
                return false;
        }
    }

    private static bool InstanceOf(XdmValue value, string typeName)
    {
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "");

        return normalized switch
        {
            "string" => value.Kind == XdmValueKind.String,
            "integer" or "int" or "long" or "short" or "byte" => value.Kind == XdmValueKind.Integer,
            "decimal" => value.Kind == XdmValueKind.Decimal,
            "double" => value.Kind == XdmValueKind.Double,
            "float" => value.Kind == XdmValueKind.Float,
            "boolean" => value.Kind == XdmValueKind.Boolean,
            "node" => value.IsNode,
            "item" => !value.IsUndefined,
            "empty-sequence" => value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0),
            _ => false
        };
    }

    // ------------------------------------------------------------------
    // Type promotion helpers
    // ------------------------------------------------------------------

    private static bool IsDouble(XdmValue value) =>
        value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float;

    private static bool IsDecimal(XdmValue value) =>
        value.Kind == XdmValueKind.Decimal;

    private static double ToDouble(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => (double)value.DecimalValue,
        XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
        XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
        _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to double")
    };

    private static decimal ToDecimal(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => value.DecimalValue,
        XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
        _ => decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to decimal")
    };

    private static long ToInteger(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => (long)value.DecimalValue,
        XdmValueKind.Double or XdmValueKind.Float => (long)value.DoubleValue,
        _ => long.TryParse(value.ToString(), out var l) ? l : throw new InvalidOperationException($"Cannot convert {value.Kind} to integer")
    };
}
