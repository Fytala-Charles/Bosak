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
//                      | Charles Korthout | 0.2   | 19-05-2026     | Implemented string, sequence, and aggregate VM opcodes                                 |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added Intersect, Except, and SimpleMap VM handlers                                     |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added Map, Array, and Lookup VM handlers                                               |
//                      | Charles Korthout | 0.5   | 19-05-2026     | Added occurrence indicator support for InstanceOf, Cast, Castable, TreatAs             |
//                      | Charles Korthout | 0.6   | 22-05-2026     | Expanded TryCast with validation for xs: types, whitespace, Unicode, extended years      |
//                      | Charles Korthout | 0.6   | 19-05-2026     | Optimized Subscript, First, Last VM handlers to avoid full sequence materialization    |
//                      | Charles Korthout | 0.7   | 21-05-2026     | Divide opcode returns decimal for integer operands (XPath div semantics)               |
//                      | Charles Korthout | 0.8   | 21-05-2026     | MapAdd uses XdmValue keys with numeric promotion; fixed xs:boolean string cast         |
//                      | Charles Korthout | 0.9   | 22-05-2026     | ItemInstanceOf recognizes duration, dayTimeDuration, yearMonthDuration                 |
//                      | Charles Korthout | 1.0   | 23-05-2026     | Added TryCast support for many xs: types, duration normalization, boolean→numeric       |
//                      | Charles Korthout | 1.1   | 24-05-2026     | Range opcode uses lazy IntegerRangeSequence to avoid OOM on huge ranges                 |
//                      | Charles Korthout | 1.2   | 24-05-2026     | Added date/time value comparison type checking (XPTY0004 for cross-subtype)            |
//                      | Charles Korthout | 1.3   | 27-05-2026     | Added DocumentRoot VM handler for absolute XPath paths                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;

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
        return NormalizeSequence(result);
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
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.RegisterB;

                        string localName;
                        string nsUri;
                        var funcLiteral = literalPool[instr.Operand]!;
                        if (funcLiteral is ValueTuple<string, string> resolved)
                        {
                            localName = resolved.Item1;
                            nsUri = resolved.Item2;
                        }
                        else
                        {
                            string funcName = (string)funcLiteral;
                            (localName, nsUri) = ResolveFunctionName(funcName, context);
                        }

                        if (!context.TryResolveFunction(nsUri, localName, argCount, out var sig))
                            throw new InvalidOperationException(
                                $"Function {{{nsUri}}}{localName}#{argCount} not found.");

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
                        string localName;
                        string nsUri;
                        var varLiteral = literalPool[instr.Operand]!;
                        if (varLiteral is ValueTuple<string, string> resolvedVar)
                        {
                            localName = resolvedVar.Item1;
                            nsUri = resolvedVar.Item2;
                        }
                        else
                        {
                            string varName = (string)varLiteral;
                            (localName, nsUri) = ResolveVariableName(varName, context);
                        }

                        if (!context.TryGetVariable(localName, out var value, nsUri))
                            throw new InvalidOperationException($"Variable ${localName} is not defined.");

                        registers[instr.RegisterA] = value;
                        ip++;
                        break;
                    }

                case IrOpCode.StoreVariable:
                    {
                        string localName;
                        string nsUri;
                        var varLiteral = literalPool[instr.Operand]!;
                        if (varLiteral is ValueTuple<string, string> resolvedVar)
                        {
                            localName = resolvedVar.Item1;
                            nsUri = resolvedVar.Item2;
                        }
                        else
                        {
                            string varName = (string)varLiteral;
                            (localName, nsUri) = ResolveVariableName(varName, context);
                        }
                        context.WithVariable(localName, registers[instr.RegisterB], nsUri);
                        ip++;
                        break;
                    }

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
                        var item = registers[instr.RegisterB];
                        // Flatten empty sequences (e.g., () in (a, (), b))
                        if (item.IsUndefined)
                        {
                            ip++;
                            break;
                        }
                        if (item.IsSequence && item.SequenceValue is not null)
                        {
                            if (item.SequenceValue.TryGetLength(out var len) && len == 0)
                            {
                                ip++;
                                break;
                            }
                            // Sequences are always flat in XPath; flatten nested sequences
                            foreach (var seqItem in item.SequenceValue)
                                list.Add(seqItem);
                        }
                        else
                        {
                            list.Add(item);
                        }
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
                        var left = registers[instr.RegisterB];
                        var right = registers[instr.RegisterC];
                        if (left.IsUndefined || IsEmptySeq(left) || right.IsUndefined || IsEmptySeq(right))
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            ip++;
                            break;
                        }
                        long from = ToInteger(left);
                        long to = ToInteger(right);
                        if (from > to)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            ip++;
                            break;
                        }
                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            XdmSequence.FromSource(new IntegerRangeSequence(from, to)));
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
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(combined)));
                        ip++;
                        break;
                    }

                case IrOpCode.Intersect:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        var rightNodes = new List<IXdmNode>();
                        foreach (var item in right)
                            if (item.IsNode)
                                rightNodes.Add(item.NodeValue);

                        var result = new List<XdmValue>();
                        foreach (var item in left)
                        {
                            if (!item.IsNode) continue;
                            foreach (var rn in rightNodes)
                            {
                                if (rn.IsSameNode(item.NodeValue))
                                {
                                    result.Add(item);
                                    break;
                                }
                            }
                        }
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(result)));
                        ip++;
                        break;
                    }

                case IrOpCode.Except:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        var rightNodes = new List<IXdmNode>();
                        foreach (var item in right)
                            if (item.IsNode)
                                rightNodes.Add(item.NodeValue);

                        var result = new List<XdmValue>();
                        foreach (var item in left)
                        {
                            if (!item.IsNode) continue;
                            bool inRight = false;
                            foreach (var rn in rightNodes)
                            {
                                if (rn.IsSameNode(item.NodeValue))
                                {
                                    inRight = true;
                                    break;
                                }
                            }
                            if (!inRight)
                                result.Add(item);
                        }
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(result)));
                        ip++;
                        break;
                    }

                case IrOpCode.SimpleMap:
                    {
                        var sequence = registers[instr.RegisterB];
                        int rhsEntry = instr.Operand;

                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            context.WithFocus(items[i], i + 1, items.Length);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, rhsEntry);

                            if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!rhsResult.IsUndefined)
                            {
                                results.Add(rhsResult);
                            }
                        }

                        // Restore context
                        context.WithFocus(savedItem, savedPos, savedSize);

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // FLWOR / Quantified
                // ------------------------------------------------------------------
                case IrOpCode.For:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;
                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        foreach (var item in items)
                        {
                            context.WithFocus(item, 1, 1);
                            context.WithVariable(info.VariableName, item);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!rhsResult.IsUndefined)
                            {
                                results.Add(rhsResult);
                            }
                        }

                        context.WithFocus(savedItem, savedPos, savedSize);
                        if (hadVariable)
                            context.WithVariable(info.VariableName, savedVar);
                        else
                            context.RemoveVariable(info.VariableName);

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.Some:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);

                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;
                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        bool result = false;
                        foreach (var item in items)
                        {
                            context.WithFocus(item, 1, 1);
                            context.WithVariable(info.VariableName, item);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (rhsResult.EffectiveBooleanValue())
                            {
                                result = true;
                                break;
                            }
                        }

                        context.WithFocus(savedItem, savedPos, savedSize);
                        if (hadVariable)
                            context.WithVariable(info.VariableName, savedVar);
                        else
                            context.RemoveVariable(info.VariableName);

                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Every:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);

                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;
                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        bool result = true;
                        foreach (var item in items)
                        {
                            context.WithFocus(item, 1, 1);
                            context.WithVariable(info.VariableName, item);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (!rhsResult.EffectiveBooleanValue())
                            {
                                result = false;
                                break;
                            }
                        }

                        context.WithFocus(savedItem, savedPos, savedSize);
                        if (hadVariable)
                            context.WithVariable(info.VariableName, savedVar);
                        else
                            context.RemoveVariable(info.VariableName);

                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.TryCatch:
                    {
                        var info = (TryCatchInfo)literalPool[instr.Operand]!;
                        try
                        {
                            var (result, _) = ExecuteBlock(module, context, registers, info.TryEntryPoint);
                            registers[instr.RegisterA] = result;
                        }
                        catch (Exception ex)
                        {
                            const string ErrNs = "http://www.w3.org/2005/xqt-errors";
                            context.WithVariable("code", XdmValue.FromString(ex.GetType().Name), ErrNs);
                            context.WithVariable("description", XdmValue.FromString(ex.Message), ErrNs);
                            var (catchResult, _) = ExecuteBlock(module, context, registers, info.CatchEntryPoint);
                            registers[instr.RegisterA] = catchResult;
                        }
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

                case IrOpCode.DocumentRoot:
                    {
                        var input = registers[instr.RegisterB];
                        if (input.IsNode && input.NodeValue != null)
                        {
                            var doc = input.NodeValue.Document;
                            registers[instr.RegisterA] = doc != null
                                ? XdmValue.FromNode(doc)
                                : XdmValue.FromNode(input.NodeValue);
                        }
                        else
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
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

                        if (sequence.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = index == 1 ? sequence : XdmValue.Undefined;
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                            {
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            }
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                XdmValue? result = null;
                                for (int i = 0; i < index; i++)
                                {
                                    if (!en.MoveNext())
                                    {
                                        result = null;
                                        break;
                                    }
                                    result = en.Current;
                                }
                                registers[instr.RegisterA] = result ?? XdmValue.Undefined;
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.First:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = sequence;
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                registers[instr.RegisterA] = en.MoveNext() ? en.Current : XdmValue.Undefined;
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Last:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = sequence;
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                XdmValue last = XdmValue.Undefined;
                                while (en.MoveNext())
                                    last = en.Current;
                                registers[instr.RegisterA] = last;
                            }
                        }
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
                                      registers[instr.RegisterB].NodeValue.IsSameNode(
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
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        registers[instr.RegisterA] = XdmValue.FromInteger(s.Length);
                        ip++;
                        break;
                    }

                case IrOpCode.Substring:
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        double startD = ToDouble(Atomize(registers[instr.RegisterC]));
                        if (double.IsNaN(startD))
                        {
                            registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                        }
                        else
                        {
                            int start = (int)Math.Round(startD);
                            if (start <= 0) start = 1;
                            if (start > s.Length)
                                registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                            else if (instr.Operand != 0)
                            {
                                double lenD = ToDouble(Atomize(registers[instr.Operand]));
                                if (double.IsNaN(lenD) || lenD <= 0)
                                    registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                                else
                                {
                                    int len = (int)Math.Round(lenD);
                                    int end = Math.Min(start - 1 + len, s.Length);
                                    registers[instr.RegisterA] = XdmValue.FromString(s[(start - 1)..end]);
                                }
                            }
                            else
                            {
                                registers[instr.RegisterA] = XdmValue.FromString(s[(start - 1)..]);
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Contains:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).Contains(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.StartsWith:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).StartsWith(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.EndsWith:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).EndsWith(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.NormalizeSpace:
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        registers[instr.RegisterA] = XdmValue.FromString(NormalizeSpaceString(s));
                        ip++;
                        break;
                    }

                case IrOpCode.Translate:
                    {
                        string arg = AtomizedString(registers[instr.RegisterB]);
                        string map = AtomizedString(registers[instr.RegisterC]);
                        string trans = AtomizedString(registers[instr.RegisterB + 1]);
                        var sb = new System.Text.StringBuilder(arg.Length);
                        foreach (char c in arg)
                        {
                            int idx = map.IndexOf(c);
                            if (idx >= 0)
                            {
                                if (idx < trans.Length)
                                    sb.Append(trans[idx]);
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        registers[instr.RegisterA] = XdmValue.FromString(sb.ToString());
                        ip++;
                        break;
                    }

                case IrOpCode.UpperCase:
                    registers[instr.RegisterA] = XdmValue.FromString(AtomizedString(registers[instr.RegisterB]).ToUpperInvariant());
                    ip++;
                    break;

                case IrOpCode.LowerCase:
                    registers[instr.RegisterA] = XdmValue.FromString(AtomizedString(registers[instr.RegisterB]).ToLowerInvariant());
                    ip++;
                    break;

                case IrOpCode.MatchesRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        registers[instr.RegisterA] = XdmValue.FromBoolean(
                            System.Text.RegularExpressions.Regex.IsMatch(input, pattern, options));
                        ip++;
                        break;
                    }

                case IrOpCode.ReplaceRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        string replacement = AtomizedString(registers[instr.RegisterB + 1]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        registers[instr.RegisterA] = XdmValue.FromString(
                            System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, options));
                        ip++;
                        break;
                    }

                case IrOpCode.TokenizeRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        var tokens = System.Text.RegularExpressions.Regex.Split(input, pattern, options)
                            .Where(t => !string.IsNullOrEmpty(t))
                            .Select(XdmValue.FromString)
                            .ToList();
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(tokens));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Type operations
                // ------------------------------------------------------------------
                case IrOpCode.Cast:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        bool isEmpty = value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);
                        if (occurrence == OccurrenceIndicator.ZeroOrOne && isEmpty)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (occurrence is OccurrenceIndicator.ZeroOrMore or OccurrenceIndicator.OneOrMore)
                        {
                            throw new InvalidOperationException("Cannot cast to a sequence type with * or + occurrence indicator.");
                        }
                        else
                        {
                            registers[instr.RegisterA] = Cast(value, typeName);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Castable:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        bool isEmpty = value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);
                        bool castable;
                        if (occurrence == OccurrenceIndicator.ZeroOrOne && isEmpty)
                        {
                            castable = true;
                        }
                        else if (occurrence is OccurrenceIndicator.ZeroOrMore or OccurrenceIndicator.OneOrMore)
                        {
                            castable = false;
                        }
                        else
                        {
                            try
                            {
                                castable = TryCast(value, typeName, out _);
                            }
                            catch (InvalidOperationException ex) when (ex.Message == "FOCA0003")
                            {
                                castable = false;
                            }
                        }
                        registers[instr.RegisterA] = XdmValue.FromBoolean(castable);
                        ip++;
                        break;
                    }

                case IrOpCode.InstanceOf:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        bool instance = InstanceOf(registers[instr.RegisterB], typeName, occurrence);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(instance);
                        ip++;
                        break;
                    }

                case IrOpCode.TreatAs:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        if (!InstanceOf(value, typeName, occurrence))
                            throw new InvalidOperationException($"Treat as assertion failed for type {typeName} with occurrence {occurrence}.");
                        registers[instr.RegisterA] = value;
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Sequence functions
                // ------------------------------------------------------------------
                case IrOpCode.Count:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                            registers[instr.RegisterA] = XdmValue.FromInteger(1);
                        else if (seq.SequenceValue!.TryGetLength(out var len))
                            registers[instr.RegisterA] = XdmValue.FromInteger(len);
                        else
                        {
                            long count = 0;
                            foreach (var _ in XdmSequence.FromSource(seq.SequenceValue!))
                                count++;
                            registers[instr.RegisterA] = XdmValue.FromInteger(count);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Exists:
                    {
                        var seq = registers[instr.RegisterB];
                        registers[instr.RegisterA] = XdmValue.FromBoolean(
                            !seq.IsUndefined && seq.EffectiveBooleanValue());
                        ip++;
                        break;
                    }

                case IrOpCode.Empty:
                    {
                        var seq = registers[instr.RegisterB];
                        registers[instr.RegisterA] = XdmValue.FromBoolean(
                            seq.IsUndefined || !seq.EffectiveBooleanValue());
                        ip++;
                        break;
                    }

                case IrOpCode.Head:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                            registers[instr.RegisterA] = seq;
                        else
                        {
                            XdmValue? first = null;
                            foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
                            {
                                first = item;
                                break;
                            }
                            registers[instr.RegisterA] = first ?? XdmValue.Undefined;
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Tail:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else
                        {
                            var list = new List<XdmValue>();
                            bool first = true;
                            foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
                            {
                                if (first) { first = false; continue; }
                                list.Add(item);
                            }
                            registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(list));
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.InsertBefore:
                    {
                        var target = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        long pos = ToInteger(Atomize(registers[instr.RegisterC]));
                        var inserts = MaterializeSequence(registers[instr.RegisterB + 1]).ToList();
                        if (pos < 1) pos = 1;
                        if (pos > target.Count + 1) pos = target.Count + 1;
                        target.InsertRange((int)pos - 1, inserts);
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(target));
                        ip++;
                        break;
                    }

                case IrOpCode.Remove:
                    {
                        var target = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        long pos = ToInteger(Atomize(registers[instr.RegisterC]));
                        if (pos >= 1 && pos <= target.Count)
                            target.RemoveAt((int)pos - 1);
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(target));
                        ip++;
                        break;
                    }

                case IrOpCode.Reverse:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        items.Reverse();
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(items));
                        ip++;
                        break;
                    }

                case IrOpCode.Subsequence:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        double startD = ToDouble(Atomize(registers[instr.RegisterC]));
                        if (double.IsNaN(startD))
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else
                        {
                            int start = (int)Math.Round(startD);
                            if (start < 1) start = 1;
                            if (start > items.Count)
                            {
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            }
                            else if (instr.Operand != 0)
                            {
                                double lenD = ToDouble(Atomize(registers[instr.Operand]));
                                if (double.IsNaN(lenD) || lenD <= 0)
                                    registers[instr.RegisterA] = XdmValue.Undefined;
                                else
                                {
                                    int len = (int)Math.Round(lenD);
                                    int count = Math.Min(len, items.Count - start + 1);
                                    registers[instr.RegisterA] = XdmValue.FromSequence(
                                        MaterializedSequence.FromList(items.Skip(start - 1).Take(count).ToList()));
                                }
                            }
                            else
                            {
                                registers[instr.RegisterA] = XdmValue.FromSequence(
                                    MaterializedSequence.FromList(items.Skip(start - 1).ToList()));
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.DistinctValues:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        var seen = new HashSet<string>();
                        var result = new List<XdmValue>();
                        foreach (var item in items)
                        {
                            string key = AtomizedString(item);
                            if (seen.Add(key))
                                result.Add(item);
                        }
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                case IrOpCode.IndexOf:
                    {
                        var seq = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        string search = AtomizedString(registers[instr.RegisterC]);
                        var result = new List<XdmValue>();
                        for (int i = 0; i < seq.Count; i++)
                        {
                            if (AtomizedString(seq[i]) == search)
                                result.Add(XdmValue.FromInteger(i + 1));
                        }
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Aggregation
                // ------------------------------------------------------------------
                case IrOpCode.Sum:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = instr.Operand != 0 ? registers[instr.Operand] : XdmValue.FromInteger(0);
                        else
                            registers[instr.RegisterA] = Sum(items);
                        ip++;
                        break;
                    }

                case IrOpCode.Avg:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                        {
                            var total = Sum(items);
                            if (total.Kind == XdmValueKind.Decimal)
                                registers[instr.RegisterA] = XdmValue.FromDecimal(total.DecimalValue / items.Count);
                            else
                                registers[instr.RegisterA] = XdmValue.FromDouble(ToDouble(total) / items.Count);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Min:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                            registers[instr.RegisterA] = MinMax(items, true);
                        ip++;
                        break;
                    }

                case IrOpCode.Max:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                            registers[instr.RegisterA] = MinMax(items, false);
                        ip++;
                        break;
                    }

                case IrOpCode.StringJoin:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        string sep = instr.Operand != 0 ? AtomizedString(registers[instr.Operand]) : string.Empty;
                        var strings = new List<string>(items.Count);
                        foreach (var item in items)
                            strings.Add(AtomizedString(item));
                        registers[instr.RegisterA] = XdmValue.FromString(string.Join(sep, strings));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Higher-order (XPath 3.1)
                // ------------------------------------------------------------------
                case IrOpCode.Map:
                    registers[instr.RegisterA] = XdmValue.FromMap(new XdmMap());
                    ip++;
                    break;

                case IrOpCode.MapAdd:
                    {
                        var map = registers[instr.RegisterA].MapValue;
                        var key = AtomizeMapKey(registers[instr.RegisterB]);
                        map.Add(key, registers[instr.RegisterC]);
                        ip++;
                        break;
                    }

                case IrOpCode.Array:
                    registers[instr.RegisterA] = XdmValue.FromArray(new XdmArray());
                    ip++;
                    break;

                case IrOpCode.ArrayAdd:
                    {
                        var arr = registers[instr.RegisterA].ArrayValue;
                        arr.Add(registers[instr.RegisterB]);
                        ip++;
                        break;
                    }

                case IrOpCode.ArrayAddAll:
                    {
                        var arr = registers[instr.RegisterA].ArrayValue;
                        var seq = registers[instr.RegisterB];
                        if (seq.IsSequence && seq.SequenceValue is not null)
                        {
                            foreach (var item in seq.SequenceValue)
                                arr.Add(item);
                        }
                        else if (!seq.IsUndefined)
                        {
                            arr.Add(seq);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Lookup:
                    {
                        var container = registers[instr.RegisterB];
                        var key = registers[instr.RegisterC];

                        if (container.Kind == XdmValueKind.Map)
                        {
                            var vkey = AtomizeMapKey(key);
                            if (container.MapValue.TryGetValue(vkey, out var value))
                                registers[instr.RegisterA] = value;
                            else
                                registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (container.Kind == XdmValueKind.Array)
                        {
                            int idx = (int)ToInteger(key);
                            registers[instr.RegisterA] = container.ArrayValue.Get(idx);
                        }
                        else
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.LookupWildcard:
                    {
                        var container = registers[instr.RegisterB];
                        var result = new List<XdmValue>();

                        if (container.Kind == XdmValueKind.Map)
                        {
                            foreach (var v in container.MapValue.Values)
                                result.Add(v);
                        }
                        else if (container.Kind == XdmValueKind.Array)
                        {
                            foreach (var v in container.ArrayValue.Values)
                                result.Add(v);
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                case IrOpCode.LoadFunction:
                    {
                        var raw = literalPool[instr.Operand]!;
                        FunctionItem funcItem = raw switch
                        {
                            NamedFunctionItem named => named,
                            CurriedFunctionItem curried => curried,
                            InlineFunctionItem inline => inline,
                            CompilerInlineFunction cif => new InlineFunctionItem(cif.Parameters, cif.Body, cif.ParameterTypes, cif.ReturnType),
                            ValueTuple<string, int> namedTuple => ResolveNamedFunctionTuple(namedTuple, context),
                            _ => throw new InvalidOperationException($"Unknown function item type: {raw.GetType().Name}")
                        };
                        registers[instr.RegisterA] = XdmValue.FromFunction(funcItem);
                        ip++;
                        break;
                    }

                case IrOpCode.Curry:
                    {
                        var baseFunc = (FunctionItem)registers[instr.RegisterB].FunctionValue;
                        var descriptor = (int[])literalPool[instr.Operand]!;
                        var fixedArgs = new XdmValue?[descriptor.Length];
                        for (int i = 0; i < descriptor.Length; i++)
                        {
                            fixedArgs[i] = descriptor[i] >= 0 ? registers[descriptor[i]] : null;
                        }
                        registers[instr.RegisterA] = XdmValue.FromFunction(new CurriedFunctionItem(baseFunc, fixedArgs));
                        ip++;
                        break;
                    }

                case IrOpCode.Apply:
                    {
                        var func = (FunctionItem)registers[instr.RegisterB].FunctionValue;
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.Operand;
                        XdmValue[] args = new XdmValue[argCount];
                        for (int i = 0; i < argCount; i++)
                            args[i] = registers[firstArgReg + i];
                        registers[instr.RegisterA] = InvokeFunctionItem(func, context, args);
                        ip++;
                        break;
                    }

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

    private static (string LocalName, string NamespaceUri) ResolveFunctionName(string funcName, EvaluationContext? context)
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
            if (context is null || !context.TryResolveNamespace(prefix, out nsUri))
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
    private static FunctionItem ResolveNamedFunctionTuple(ValueTuple<string, int> tuple, EvaluationContext context)
    {
        var (localName, nsUri) = ResolveFunctionName(tuple.Item1, context);
        if (!context.TryResolveFunction(nsUri, localName, tuple.Item2, out _))
            throw new InvalidOperationException($"XPST0017: Function {{{nsUri}}}{localName}#{tuple.Item2} not found.");
        return new NamedFunctionItem(nsUri, localName, tuple.Item2);
    }

    public static XdmValue InvokeFunctionItem(FunctionItem func, EvaluationContext context, ReadOnlySpan<XdmValue> args)
    {
        switch (func)
        {
            case NamedFunctionItem named:
                if (!context.TryResolveFunction(named.NamespaceUri, named.LocalName, args.Length, out var sig))
                    throw new InvalidOperationException($"Function {{{named.NamespaceUri}}}{named.LocalName}#{args.Length} not found.");
                return sig.Implementation(context, args);

            case DelegateFunctionItem del:
                return del.Implementation(context, args);

            case InlineFunctionItem inline:
                {
                    // Validate parameter types
                    for (int i = 0; i < inline.Parameters.Count; i++)
                    {
                        var expectedType = i < inline.ParameterTypes.Count ? inline.ParameterTypes[i] : null;
                        if (!string.IsNullOrEmpty(expectedType))
                        {
                            var arg = i < args.Length ? args[i] : XdmValue.Undefined;
                            if (!arg.IsUndefined)
                            {
                                if (arg.IsSequence)
                                {
                                    int count = 0;
                                    XdmValue first = default;
                                    foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
                                    {
                                        count++;
                                        if (count == 1) first = item;
                                    }
                                    if (count > 1)
                                        throw new InvalidOperationException("XPTY0004");
                                    arg = count == 1 ? first : XdmValue.Undefined;
                                }
                                if (!arg.IsUndefined && !ValueMatchesType(arg, expectedType))
                                    throw new InvalidOperationException("XPTY0004");
                            }
                        }
                    }

                    var saved = new (string Name, bool Had, XdmValue Value)[inline.Parameters.Count];
                    for (int i = 0; i < inline.Parameters.Count; i++)
                    {
                        saved[i].Name = inline.Parameters[i];
                        saved[i].Had = context.TryGetVariable(inline.Parameters[i], out var oldVal);
                        saved[i].Value = oldVal;
                        context.WithVariable(inline.Parameters[i], i < args.Length ? args[i] : XdmValue.Undefined);
                    }
                    try
                    {
                        var result = Execute(inline.Body, context);
                        // Validate return type
                        if (!string.IsNullOrEmpty(inline.ReturnType))
                        {
                            bool matches;
                            if (result.IsUndefined)
                            {
                                matches = inline.ReturnType.Trim().EndsWith('?') || inline.ReturnType.Trim().EndsWith('*');
                            }
                            else if (result.IsSequence)
                            {
                                int count = 0;
                                matches = true;
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
                                {
                                    count++;
                                    if (!ValueMatchesType(item, inline.ReturnType))
                                    {
                                        matches = false;
                                        break;
                                    }
                                }
                                if (matches && count > 1 &&
                                    !(inline.ReturnType.Trim().EndsWith('*') || inline.ReturnType.Trim().EndsWith('+')))
                                {
                                    matches = false;
                                }
                            }
                            else
                            {
                                matches = ValueMatchesType(result, inline.ReturnType);
                            }
                            if (!matches)
                                throw new InvalidOperationException("XPTY0004");
                        }
                        return result;
                    }
                    finally
                    {
                        for (int i = 0; i < inline.Parameters.Count; i++)
                        {
                            if (saved[i].Had)
                                context.WithVariable(saved[i].Name, saved[i].Value);
                            else
                                context.RemoveVariable(saved[i].Name);
                        }
                    }
                }

            case CurriedFunctionItem curried:
                {
                    var merged = new XdmValue[curried.FixedArgs.Length];
                    int argIdx = 0;
                    for (int i = 0; i < curried.FixedArgs.Length; i++)
                    {
                        if (curried.FixedArgs[i] is { } boundArg)
                            merged[i] = boundArg;
                        else
                            merged[i] = argIdx < args.Length ? args[argIdx++] : XdmValue.Undefined;
                    }
                    return InvokeFunctionItem(curried.BaseFunction, context, merged);
                }

            default:
                throw new InvalidOperationException($"Unknown function item type: {func.GetType().Name}");
        }
    }

    public static XdmValue InvokeFunctionItem(XdmValue funcValue, EvaluationContext context, ReadOnlySpan<XdmValue> args)
    {
        if (funcValue.IsFunction)
            return InvokeFunctionItem((FunctionItem)funcValue.FunctionValue, context, args);

        if (funcValue.IsMap)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("XPTY0004");
            var key = AtomizeMapKey(args[0]);
            var map = funcValue.MapValue;
            if (map.TryGetValue(key, out var value))
                return value;
            return XdmValue.Undefined;
        }

        if (funcValue.IsArray)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("XPTY0004");
            var index = (int)ToInteger(args[0]);
            var arr = funcValue.ArrayValue;
            if (index >= 1 && index <= arr.Count)
                return arr.Get(index);
            return XdmValue.Undefined;
        }

        throw new InvalidOperationException("XPTY0004");
    }

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

    /// <summary>
    /// Returns true if the value is a node or a singleton sequence containing a node.
    /// Used to determine whether atomization produces an untyped atomic value.
    /// </summary>
    private static bool IsNodeOrigin(XdmValue value)
    {
        if (value.IsUndefined)
            return false;
        if (value.IsNode)
            return true;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = MaterializeSequence(value);
            if (items.Length == 1)
                return IsNodeOrigin(items[0]);
        }
        return false;
    }

    /// <summary>
    /// Removes duplicate nodes and sorts the remaining nodes into document order.
    /// Non-node items are preserved in their original relative order after all nodes.
    /// </summary>
    private static XdmValue NormalizeSequence(XdmValue value)
    {
        if (value.IsUndefined || !value.IsSequence)
            return value;

        var items = MaterializeSequence(value);
        if (items.Length <= 1)
            return value;

        // Remove duplicate nodes (keep first occurrence)
        var unique = new List<XdmValue>(items.Length);
        bool hasNodes = false;
        foreach (var item in items)
        {
            if (!item.IsNode)
            {
                unique.Add(item);
                continue;
            }

            hasNodes = true;
            bool isDup = false;
            foreach (var existing in unique)
            {
                if (existing.IsNode && item.NodeValue.IsSameNode(existing.NodeValue))
                {
                    isDup = true;
                    break;
                }
            }

            if (!isDup)
                unique.Add(item);
        }

        if (unique.Count <= 1 || !hasNodes)
            return XdmValue.FromSequence(MaterializedSequence.FromList(unique));

        // Sort nodes by document order; keep non-nodes at the end in original order
        unique.Sort((a, b) =>
        {
            bool aNode = a.IsNode;
            bool bNode = b.IsNode;

            if (aNode && bNode)
                return a.NodeValue.DocumentOrder.CompareTo(b.NodeValue.DocumentOrder);

            if (aNode) return -1;
            if (bNode) return 1;
            return 0;
        });

        return XdmValue.FromSequence(MaterializedSequence.FromList(unique));
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
        // Date/Time + Duration
        if (left.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time && (right.Kind == XdmValueKind.String || right.Kind == XdmValueKind.Duration))
            return AddDuration(left, right.ToString());
        if (right.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time && (left.Kind == XdmValueKind.String || left.Kind == XdmValueKind.Duration))
            return AddDuration(right, left.ToString());

        // Duration + Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
            return AddDurations(left, right);

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) + ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) + ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) + ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) + ToInteger(right));
    }

    private static XdmValue AddDuration(XdmValue dateTimeValue, string duration)
    {
        var dto = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeValue,
            XdmValueKind.Date => dateTimeValue.DateValue,
            XdmValueKind.Time => dateTimeValue.TimeValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };

        if (IsYearMonthDurationString(duration))
        {
            var (years, months, _, _, _, _) = ParseDuration(duration);
            var dt = dto.DateTime;
            int newMonth = dt.Month + (int)months;
            int newYear = dt.Year + (int)years + (newMonth - 1) / 12;
            newMonth = ((newMonth - 1) % 12) + 1;
            if (newMonth <= 0) { newYear -= 1; newMonth += 12; }
            int newDay = Math.Min(dt.Day, DateTime.DaysInMonth(newYear, newMonth));
            var newDt = new DateTime(newYear, newMonth, newDay, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
            dto = new DateTimeOffset(newDt, dto.Offset);
        }
        else if (IsDayTimeDurationString(duration))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
            long ticks = (long)(seconds * TimeSpan.TicksPerSecond);
            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromTicks(ticks);
            dto = dto + ts;
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(dto),
            XdmValueKind.Date => XdmValue.FromDate(dto),
            XdmValueKind.Time => XdmValue.FromTime(dto),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static XdmValue Subtract(XdmValue left, XdmValue right)
    {
        if (left.Kind == XdmValueKind.Date && right.Kind == XdmValueKind.Date)
            return XdmValue.FromDuration(FormatDuration(left.DateValue - right.DateValue));
        if (left.Kind == XdmValueKind.DateTime && right.Kind == XdmValueKind.DateTime)
            return XdmValue.FromDuration(FormatDuration(left.DateTimeValue - right.DateTimeValue));
        if (left.Kind == XdmValueKind.Time && right.Kind == XdmValueKind.Time)
            return XdmValue.FromDuration(FormatDuration(left.TimeValue - right.TimeValue));
        if (left.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time && (right.Kind == XdmValueKind.String || right.Kind == XdmValueKind.Duration))
            return SubtractDuration(left, right.ToString());

        // Duration - Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
            return SubtractDurations(left, right);

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) - ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) - ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) - ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) - ToInteger(right));
    }

    private static XdmValue SubtractDuration(XdmValue dateTimeValue, string duration)
    {
        var dto = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeValue,
            XdmValueKind.Date => dateTimeValue.DateValue,
            XdmValueKind.Time => dateTimeValue.TimeValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };

        if (IsYearMonthDurationString(duration))
        {
            var (years, months, _, _, _, _) = ParseDuration(duration);
            var dt = dto.DateTime;
            int newMonth = dt.Month - (int)months;
            int newYear = dt.Year - (int)years + (newMonth - 1) / 12;
            newMonth = ((newMonth - 1) % 12) + 1;
            if (newMonth <= 0) { newYear -= 1; newMonth += 12; }
            int newDay = Math.Min(dt.Day, DateTime.DaysInMonth(newYear, newMonth));
            var newDt = new DateTime(newYear, newMonth, newDay, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
            dto = new DateTimeOffset(newDt, dto.Offset);
        }
        else if (IsDayTimeDurationString(duration))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
            long ticks = (long)(seconds * TimeSpan.TicksPerSecond);
            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromTicks(ticks);
            dto = dto - ts;
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(dto),
            XdmValueKind.Date => XdmValue.FromDate(dto),
            XdmValueKind.Time => XdmValue.FromTime(dto),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static string FormatDuration(TimeSpan ts)
    {
        bool negative = ts.TotalMilliseconds < 0;
        ts = negative ? ts.Negate() : ts;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (ts.Days > 0) sb.Append($"{ts.Days}D");
        if (ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 || ts.Milliseconds > 0)
        {
            sb.Append('T');
            if (ts.Hours > 0) sb.Append($"{ts.Hours}H");
            if (ts.Minutes > 0) sb.Append($"{ts.Minutes}M");
            if (ts.Seconds > 0 || ts.Milliseconds > 0)
            {
                sb.Append($"{ts.Seconds}");
                if (ts.Milliseconds > 0)
                    sb.Append($".{ts.Milliseconds:000}");
                sb.Append('S');
            }
        }
        if (sb.Length == (negative ? 2 : 1)) sb.Append("T0S");
        return sb.ToString();
    }

    private static (long Years, long Months, long Days, long Hours, long Minutes, decimal Seconds) ParseDuration(string s)
    {
        bool negative = s.StartsWith('-');
        s = negative ? s[1..] : s;
        if (!s.StartsWith('P')) return (0, 0, 0, 0, 0, 0m);
        s = s[1..];

        long years = 0, months = 0, days = 0, hours = 0, minutes = 0;
        decimal seconds = 0m;

        int tIndex = s.IndexOf('T');
        string datePart = tIndex >= 0 ? s[..tIndex] : s;
        string timePart = tIndex >= 0 ? s[(tIndex + 1)..] : string.Empty;

        years = ParseDurationNumber(ref datePart, 'Y');
        months = ParseDurationNumber(ref datePart, 'M');
        days = ParseDurationNumber(ref datePart, 'D');

        hours = ParseDurationNumber(ref timePart, 'H');
        minutes = ParseDurationNumber(ref timePart, 'M');
        seconds = ParseDurationDecimal(ref timePart, 'S');

        if (negative)
        {
            years = -years;
            months = -months;
            days = -days;
            hours = -hours;
            minutes = -minutes;
            seconds = -seconds;
        }

        return (years, months, days, hours, minutes, seconds);
    }

    private static long ParseDurationNumber(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return long.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static decimal ParseDurationDecimal(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0m;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return decimal.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }

    private static bool IsYearMonthDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith('-')) s = s[1..];
        return s.StartsWith('P') && !s.Contains('D') && !s.Contains('T');
    }

    private static bool IsDayTimeDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith('-')) s = s[1..];
        return s.StartsWith('P') && (s.Contains('D') || s.Contains('T'));
    }

    private enum DurationSubtype { YearMonthDuration, DayTimeDuration, Duration }

    private static DurationSubtype GetDurationSubtype(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return DurationSubtype.Duration;
        bool hasYm = m.Groups["Y"].Success || m.Groups["M"].Success;
        bool hasDt = m.Groups["D"].Success || m.Groups["T"].Success;
        if (hasYm && !hasDt) return DurationSubtype.YearMonthDuration;
        if (!hasYm && hasDt) return DurationSubtype.DayTimeDuration;
        return DurationSubtype.Duration;
    }

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
                "gmonth" => "gMonth",
                "gmonthday" => "gMonthDay",
                "gday" => "gDay",
                _ => null
            },
            _ => null
        };
    }

    /// <summary>
    /// Compares two date/time values of the same subtype.
    /// Returns null if the comparison is indeterminate (one operand lacks a timezone
    /// and the two possible positions straddle the other operand).
    /// </summary>
    private static int? CompareDateTimeValues(XdmValue left, XdmValue right, string subtype)
    {
        var leftXdt = GetXPathDateTime(left, subtype);
        var rightXdt = GetXPathDateTime(right, subtype);

        bool leftHasTz = left.HasTimezone;
        bool rightHasTz = right.HasTimezone;

        // Both have timezones: normalize to UTC and compare
        if (leftHasTz && rightHasTz)
        {
            var leftUtc = NormalizeToUtc(leftXdt, subtype);
            var rightUtc = NormalizeToUtc(rightXdt, subtype);
            return CompareNormalizedDateTime(leftUtc, rightUtc);
        }

        // Neither has timezone: compare components directly
        if (!leftHasTz && !rightHasTz)
        {
            return CompareNormalizedDateTime(leftXdt, rightXdt);
        }

        // Exactly one has timezone: test with ±14:00 implicit timezone bounds
        // zoned = operand with timezone, unzoned = operand without timezone
        var zonedXdt = leftHasTz ? leftXdt : rightXdt;
        var unzonedXdt = leftHasTz ? rightXdt : leftXdt;
        var zonedUtc = NormalizeToUtc(zonedXdt, subtype);

        // Test with unzoned implicit timezone = +14:00
        var unzonedPlus14 = NormalizeToUtc(
            new XPathDateTime(unzonedXdt.Year, unzonedXdt.Month, unzonedXdt.Day,
                unzonedXdt.Hour, unzonedXdt.Minute, unzonedXdt.Second, unzonedXdt.Millisecond,
                14 * 60, true), subtype);
        int cmpPlus14 = leftHasTz
            ? CompareNormalizedDateTime(zonedUtc, unzonedPlus14)
            : CompareNormalizedDateTime(unzonedPlus14, zonedUtc);

        // Test with unzoned implicit timezone = -14:00
        var unzonedMinus14 = NormalizeToUtc(
            new XPathDateTime(unzonedXdt.Year, unzonedXdt.Month, unzonedXdt.Day,
                unzonedXdt.Hour, unzonedXdt.Minute, unzonedXdt.Second, unzonedXdt.Millisecond,
                -14 * 60, true), subtype);
        int cmpMinus14 = leftHasTz
            ? CompareNormalizedDateTime(zonedUtc, unzonedMinus14)
            : CompareNormalizedDateTime(unzonedMinus14, zonedUtc);

        // If both evaluations agree, return that result; otherwise indeterminate
        if (cmpPlus14 == cmpMinus14)
            return cmpPlus14;
        return null;
    }

    private static XPathDateTime GetXPathDateTime(XdmValue value, string subtype)
    {
        return subtype switch
        {
            "dateTime" => value.DateTimeXPathValue,
            "date" => value.DateXPathValue,
            "time" => value.TimeXPathValue,
            _ => throw new InvalidOperationException($"Unsupported date/time subtype: {subtype}")
        };
    }

    /// <summary>
    /// Normalizes a date/time value to UTC by applying its timezone offset.
    /// For xs:time, uses 1972-12-31 as the reference date.
    /// </summary>
    private static XPathDateTime NormalizeToUtc(XPathDateTime xdt, string subtype)
    {
        // For xs:time, use a reference date (1972-12-31 per spec)
        long year = xdt.Year;
        int month = xdt.Month;
        int day = xdt.Day;
        if (subtype == "time")
        {
            year = 1972;
            month = 12;
            day = 31;
        }

        if (!xdt.HasTimezone)
        {
            // No timezone: return as-is (used for comparing two no-timezone values)
            return new XPathDateTime(year, month, day, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, 0, false);
        }

        var offset = TimeSpan.FromMinutes(xdt.TimezoneOffsetMinutes);
        if (year is >= 1 and <= 9999)
        {
            // Use DateTimeOffset for values in the supported range
            var dto = new DateTimeOffset((int)year, month, day, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, offset);
            var utc = dto.UtcDateTime;
            return new XPathDateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond, 0, true);
        }
        else
        {
            // Extended years: normalize manually
            var totalMinutes = xdt.Hour * 60L + xdt.Minute - xdt.TimezoneOffsetMinutes;
            var totalSeconds = totalMinutes * 60 + xdt.Second;
            var totalMs = totalSeconds * 1000 + xdt.Millisecond;
            // Normalize day boundary crossings manually (simplified)
            var newDay = day + (int)(totalMs / 86400000);
            totalMs %= 86400000;
            if (totalMs < 0) { totalMs += 86400000; newDay--; }
            var newHour = (int)(totalMs / 3600000);
            totalMs %= 3600000;
            var newMinute = (int)(totalMs / 60000);
            totalMs %= 60000;
            var newSecond = (int)(totalMs / 1000);
            var newMs = (int)(totalMs % 1000);
            return new XPathDateTime(year, month, newDay, newHour, newMinute, newSecond, newMs, 0, true);
        }
    }

    /// <summary>
    /// Compares two normalized XPathDateTime values (both in UTC or both without timezone).
    /// </summary>
    private static int CompareNormalizedDateTime(XPathDateTime a, XPathDateTime b)
    {
        if (a.Year != b.Year) return a.Year < b.Year ? -1 : 1;
        if (a.Month != b.Month) return a.Month < b.Month ? -1 : 1;
        if (a.Day != b.Day) return a.Day < b.Day ? -1 : 1;
        if (a.Hour != b.Hour) return a.Hour < b.Hour ? -1 : 1;
        if (a.Minute != b.Minute) return a.Minute < b.Minute ? -1 : 1;
        if (a.Second != b.Second) return a.Second < b.Second ? -1 : 1;
        if (a.Millisecond != b.Millisecond) return a.Millisecond < b.Millisecond ? -1 : 1;
        return 0;
    }

    private static (long TotalMonths, decimal TotalSeconds) NormalizeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return (0, 0);
        bool negative = m.Groups["sign"].Value == "-";

        long years = m.Groups["Y"].Success ? long.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture) : 0;
        long months = m.Groups["M"].Success ? long.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture) : 0;
        long days = m.Groups["D"].Success ? long.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture) : 0;
        long hours = m.Groups["H"].Success ? long.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture) : 0;
        long minutes = m.Groups["Tm"].Success ? long.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture) : 0;
        decimal seconds = m.Groups["S"].Success ? decimal.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture) : 0;

        long totalMonths = years * 12 + months;
        decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;

        if (negative)
        {
            totalMonths = -totalMonths;
            totalSeconds = -totalSeconds;
        }

        return (totalMonths, totalSeconds);
    }

    private static XdmValue AddDurations(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            long totalMonths = y1 * 12 + m1 + y2 * 12 + m2;
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            long totalTicks = (d1 + d2) * TimeSpan.TicksPerDay
                + (h1 + h2) * TimeSpan.TicksPerHour
                + (min1 + min2) * TimeSpan.TicksPerMinute
                + (long)((s1 + s2) * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue SubtractDurations(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            long totalMonths = y1 * 12 + m1 - (y2 * 12 + m2);
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            long totalTicks = (d1 - d2) * TimeSpan.TicksPerDay
                + (h1 - h2) * TimeSpan.TicksPerHour
                + (min1 - min2) * TimeSpan.TicksPerMinute
                + (long)((s1 - s2) * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue MultiplyDuration(XdmValue duration, XdmValue factor)
    {
        var d = duration.DurationValue;
        double f = ToDouble(factor);
        if (IsYearMonthDurationString(d))
        {
            var (y, m, _, _, _, _) = ParseDuration(d);
            long totalMonths = (long)Math.Round((y * 12 + m) * f);
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(d))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(d);
            decimal totalSeconds = (days * 86400m + hours * 3600m + minutes * 60m + seconds) * (decimal)f;
            long totalTicks = (long)(totalSeconds * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue DivideDuration(XdmValue duration, XdmValue divisor)
    {
        var d = duration.DurationValue;
        double div = ToDouble(divisor);
        if (IsYearMonthDurationString(d))
        {
            var (y, m, _, _, _, _) = ParseDuration(d);
            long totalMonths = (long)Math.Round((y * 12 + m) / div);
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(d))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(d);
            decimal totalSeconds = (days * 86400m + hours * 3600m + minutes * 60m + seconds) / (decimal)div;
            long totalTicks = (long)(totalSeconds * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static string FormatYearMonthDuration(long totalMonths)
    {
        bool negative = totalMonths < 0;
        totalMonths = Math.Abs(totalMonths);
        long years = totalMonths / 12;
        long months = totalMonths % 12;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (years > 0) sb.Append($"{years}Y");
        if (months > 0) sb.Append($"{months}M");
        if (years == 0 && months == 0) sb.Append("0M");
        return sb.ToString();
    }

    private static XdmValue DivideDurationByDuration(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            decimal totalMonths1 = y1 * 12m + m1;
            decimal totalMonths2 = y2 * 12m + m2;
            if (totalMonths2 == 0) throw new InvalidOperationException("FODT0002");
            return XdmValue.FromDecimal(totalMonths1 / totalMonths2);
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            decimal totalSeconds1 = d1 * 86400m + h1 * 3600m + min1 * 60m + s1;
            decimal totalSeconds2 = d2 * 86400m + h2 * 3600m + min2 * 60m + s2;
            if (totalSeconds2 == 0) throw new InvalidOperationException("FODT0002");
            return XdmValue.FromDecimal(totalSeconds1 / totalSeconds2);
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue Multiply(XdmValue left, XdmValue right)
    {
        // Duration * number or number * Duration
        if (left.Kind == XdmValueKind.Duration)
            return MultiplyDuration(left, right);
        if (right.Kind == XdmValueKind.Duration)
            return MultiplyDuration(right, left);

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) * ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) * ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) * ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) * ToInteger(right));
    }

    private static XdmValue Divide(XdmValue left, XdmValue right)
    {
        // Duration div number
        if (left.Kind == XdmValueKind.Duration && !IsDuration(right))
            return DivideDuration(left, right);

        // Duration div Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
            return DivideDurationByDuration(left, right);

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) / ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) / ToFloat(right));
        // XPath div always returns decimal (or double), never integer
        return XdmValue.FromDecimal(ToDecimal(left) / ToDecimal(right));
    }

    private static XdmValue IntegerDivide(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromInteger((long)(ToDouble(left) / ToDouble(right)));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromInteger((long)(ToFloat(left) / ToFloat(right)));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromInteger((long)(ToDecimal(left) / ToDecimal(right)));
        return XdmValue.FromInteger(ToInteger(left) / ToInteger(right));
    }

    private static XdmValue Modulo(XdmValue left, XdmValue right)
    {
        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) % ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) % ToDecimal(right));
        return XdmValue.FromInteger(ToInteger(left) % ToInteger(right));
    }

    private static XdmValue Negate(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Duration)
        {
            var s = value.DurationValue;
            if (s.StartsWith('-'))
                return XdmValue.FromDuration(s[1..]);
            return XdmValue.FromDuration("-" + s);
        }
        if (IsDouble(value))
            return XdmValue.FromDouble(-ToDouble(value));
        if (IsFloat(value))
            return XdmValue.FromFloat(-ToFloat(value));
        if (IsDecimal(value))
            return XdmValue.FromDecimal(-ToDecimal(value));
        return XdmValue.FromInteger(-ToInteger(value));
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    private static bool Compare(IrOpCode op, XdmValue left, XdmValue right, bool strict = true)
    {
        bool leftFromNode = IsNodeOrigin(left);
        bool rightFromNode = IsNodeOrigin(right);

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

        if (IsFloat(left) || IsFloat(right))
        {
            float l = ToFloat(left);
            float r = ToFloat(right);
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

        if (left.Kind == XdmValueKind.Boolean && right.Kind == XdmValueKind.Boolean)
        {
            bool l = left.BooleanValue;
            bool r = right.BooleanValue;
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => !l && r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => !l || r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l && !r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l || !r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // Duration comparison: normalize to total months and total seconds
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
        {
            var (lMonths, lSeconds) = NormalizeDuration(left.DurationValue);
            var (rMonths, rSeconds) = NormalizeDuration(right.DurationValue);
            var lSub = GetDurationSubtype(left.DurationValue);
            var rSub = GetDurationSubtype(right.DurationValue);

            bool isEquality = op is IrOpCode.Equal or IrOpCode.ValueEqual
                              or IrOpCode.NotEqual or IrOpCode.ValueNotEqual;
            if (isEquality)
            {
                bool eq = lMonths == rMonths && lSeconds == rSeconds;
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => eq,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => !eq,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            // Ordering is only defined when both operands are the same subtype
            if (lSub == DurationSubtype.YearMonthDuration && rSub == DurationSubtype.YearMonthDuration)
            {
                int cmp = lMonths.CompareTo(rMonths);
                return op switch
                {
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }
            if (lSub == DurationSubtype.DayTimeDuration && rSub == DurationSubtype.DayTimeDuration)
            {
                int cmp = lSeconds.CompareTo(rSeconds);
                return op switch
                {
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            throw new InvalidOperationException("XPTY0004");
        }

        // Date/time comparison: only defined between operands of the same subtype
        string? leftDateSub = GetDateTimeSubtype(left);
        string? rightDateSub = GetDateTimeSubtype(right);
        if (leftDateSub is not null || rightDateSub is not null)
        {
            if (leftDateSub is null || rightDateSub is null || leftDateSub != rightDateSub)
                throw new InvalidOperationException("XPTY0004");
            var cmp = CompareDateTimeValues(left, right, leftDateSub);
            if (cmp.HasValue)
            {
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => cmp.Value == 0,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp.Value != 0,
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp.Value < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp.Value <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp.Value > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp.Value >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }
            // Indeterminate comparison: lt/gt/le/ge return false; eq/ne use string fallback
            return op is IrOpCode.Equal or IrOpCode.ValueEqual or IrOpCode.NotEqual or IrOpCode.ValueNotEqual
                ? string.CompareOrdinal(left.ToString(), right.ToString()) == (op is IrOpCode.Equal or IrOpCode.ValueEqual ? 0 : 1)
                : false;
        }

        // Atomized nodes become strings; try numeric parsing for untyped values
        string lStr = left.ToString();
        string rStr = right.ToString();

        // If both are explicitly strings, compare as strings (don't parse as numbers)
        if (left.Kind == XdmValueKind.String && right.Kind == XdmValueKind.String)
        {
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

        // In strict mode, a string may only be compared with a numeric if it originated
        // from a node (untyped atomic) or is explicitly typed as xs:untypedAtomic.
        // Typed string literals are not comparable with numbers.
        bool leftIsString = left.Kind == XdmValueKind.String;
        bool rightIsString = right.Kind == XdmValueKind.String;
        bool leftIsNumeric = IsDouble(left) || IsFloat(left) || IsDecimal(left) || left.Kind == XdmValueKind.Integer;
        bool rightIsNumeric = IsDouble(right) || IsFloat(right) || IsDecimal(right) || right.Kind == XdmValueKind.Integer;

        bool numericMismatch = (leftIsString && rightIsNumeric) || (leftIsNumeric && rightIsString);
        if (strict && numericMismatch)
        {
            bool stringIsCastable = (leftIsString && (leftFromNode || left.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true))
                                 || (rightIsString && (rightFromNode || right.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true));
            if (!stringIsCastable)
                throw new InvalidOperationException("XPTY0004");
        }

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

        if (strict && numericMismatch)
        {
            // String came from a node but didn't parse as a number
            throw new InvalidOperationException("XPTY0004");
        }

        int cmp2 = string.CompareOrdinal(lStr, rStr);
        return op switch
        {
            IrOpCode.Equal or IrOpCode.ValueEqual => cmp2 == 0,
            IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp2 != 0,
            IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp2 < 0,
            IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp2 <= 0,
            IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp2 > 0,
            IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp2 >= 0,
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
                    IrOpCode.GeneralEqual => Compare(IrOpCode.Equal, l, r, strict: false),
                    IrOpCode.GeneralNotEqual => Compare(IrOpCode.NotEqual, l, r, strict: false),
                    IrOpCode.GeneralLessThan => Compare(IrOpCode.LessThan, l, r, strict: false),
                    IrOpCode.GeneralLessThanOrEqual => Compare(IrOpCode.LessThanOrEqual, l, r, strict: false),
                    IrOpCode.GeneralGreaterThan => Compare(IrOpCode.GreaterThan, l, r, strict: false),
                    IrOpCode.GeneralGreaterThanOrEqual => Compare(IrOpCode.GreaterThanOrEqual, l, r, strict: false),
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

    public static XdmValue Cast(XdmValue value, string typeName)
    {
        if (!TryCast(value, typeName, out var result))
            throw new InvalidOperationException($"Cannot cast '{value}' to {typeName}.");
        return result;
    }

    public static bool TryCast(XdmValue value, string typeName, out XdmValue result)
    {
        result = value;
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "");

        // If value is a sequence, only allow single-item sequences for atomic casts
        if (value.IsSequence)
        {
            if (!TryGetSequenceLength(value.SequenceValue, out var seqLen) || seqLen != 1)
                return false;
            var enumerator = XdmSequence.FromSource(value.SequenceValue!).GetEnumerator();
            enumerator.MoveNext();
            value = enumerator.Current;
        }

        // Atomize nodes before casting
        if (value.IsNode)
        {
            value = XdmValue.FromString(value.NodeValue.StringValue);
        }

        // Schema type cast restrictions: some typed values can only cast to specific types
        if (value.SchemaTypeName is not null
            && normalized is not "string" and not "untypedatomic"
            && !IsCastAllowed(value.SchemaTypeName, normalized))
        {
            return false;
        }

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
            case "unsignedshort":
            case "unsignedint":
            case "unsignedlong":
            case "unsignedbyte":
            case "positiveinteger":
            case "negativeinteger":
            case "nonpositiveinteger":
            case "nonnegativeinteger":
                if (value.Kind == XdmValueKind.Integer)
                {
                    if (!IsIntegerInRange(value.IntegerValue, normalized))
                        return false;
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    long lVal = (long)value.DecimalValue;
                    if (!IsIntegerInRange(lVal, normalized))
                        return false;
                    result = XdmValue.FromInteger(lVal);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        return false;
                    if (d > long.MaxValue || d < long.MinValue)
                        throw new InvalidOperationException("FOCA0003");
                    long lDbl = (long)d;
                    if (!IsIntegerInRange(lDbl, normalized))
                        return false;
                    result = XdmValue.FromInteger(lDbl);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    long lBool = value.BooleanValue ? 1 : 0;
                    if (!IsIntegerInRange(lBool, normalized))
                        return false;
                    result = XdmValue.FromInteger(lBool);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (long.TryParse(value.ToString(), out var lInt))
                {
                    if (!IsIntegerInRange(lInt, normalized))
                        return false;
                    result = XdmValue.FromInteger(lInt);
                    return true;
                }
                return false;

            case "decimal":
                if (value.Kind == XdmValueKind.Decimal)
                    return true;
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromDecimal(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        return false;
                    result = XdmValue.FromDecimal((decimal)d);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromDecimal(value.BooleanValue ? 1m : 0m);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                {
                    string sDec = value.ToString();
                    // xs:decimal does not allow exponent notation
                    if (sDec.Contains('e', StringComparison.OrdinalIgnoreCase))
                        return false;
                    if (decimal.TryParse(sDec, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    {
                        result = XdmValue.FromDecimal(dec);
                        return true;
                    }
                }
                return false;

            case "double":
                if (value.Kind == XdmValueKind.Double)
                    return true;
                if (value.Kind == XdmValueKind.Float)
                {
                    result = XdmValue.FromDouble(value.DoubleValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromDouble(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromDouble((double)value.DecimalValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromDouble(value.BooleanValue ? 1.0 : 0.0);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (TryParseDouble(value.ToString(), out var dbl))
                {
                    result = XdmValue.FromDouble(dbl);
                    return true;
                }
                return false;

            case "float":
                if (value.Kind == XdmValueKind.Float)
                    return true;
                if (value.Kind == XdmValueKind.Double)
                {
                    result = XdmValue.FromFloat((float)value.DoubleValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromFloat(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromFloat((float)value.DecimalValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromFloat(value.BooleanValue ? 1.0f : 0.0f);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (TryParseDouble(value.ToString(), out var flt))
                {
                    result = XdmValue.FromFloat((float)flt);
                    return true;
                }
                return false;

            case "boolean":
                if (value.Kind == XdmValueKind.Boolean)
                    return true;
                if (value.Kind == XdmValueKind.String)
                {
                    var s = value.StringValue.Trim().ToLowerInvariant();
                    if (s == "true" || s == "1")
                    {
                        result = XdmValue.True;
                        return true;
                    }
                    if (s == "false" || s == "0")
                    {
                        result = XdmValue.False;
                        return true;
                    }
                    return false;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromBoolean(value.IntegerValue != 0);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromBoolean(value.DecimalValue != 0m);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    result = XdmValue.FromBoolean(d != 0.0 && !double.IsNaN(d));
                    return true;
                }
                return false;

            case "datetime":
                if (value.Kind == XdmValueKind.DateTime)
                    return true;
                if (value.Kind == XdmValueKind.Date)
                {
                    var xdtSrc = value.DateXPathValue;
                    result = XdmValue.FromDateTime(new XPathDateTime(xdtSrc.Year, xdtSrc.Month, xdtSrc.Day, 0, 0, 0, 0, xdtSrc.TimezoneOffsetMinutes, value.HasTimezone), value.HasTimezone);
                    return true;
                }
                if (value.Kind == XdmValueKind.Time)
                    return false;
                {
                    string sDt = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathDateTime(sDt, out var xdtDt, out var hasTzDt))
                    {
                        if (xdtDt.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sDt, out var dtoDt))
                        {
                            result = XdmValue.FromDateTime(dtoDt, hasTzDt);
                        }
                        else
                        {
                            result = XdmValue.FromDateTime(xdtDt, hasTzDt);
                        }
                        return true;
                    }
                }
                return false;

            case "date":
                if (value.Kind == XdmValueKind.Date)
                    return true;
                if (value.Kind == XdmValueKind.DateTime)
                {
                    bool hasTz = value.HasTimezone;
                    var xdtDt = value.DateTimeXPathValue;
                    result = XdmValue.FromDate(new XPathDateTime(xdtDt.Year, xdtDt.Month, xdtDt.Day, 0, 0, 0, 0, xdtDt.TimezoneOffsetMinutes, hasTz), hasTz);
                    return true;
                }
                if (value.Kind == XdmValueKind.Time)
                    return false;
                {
                    string sD = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathDate(sD, out var xdtD, out var hasTzD))
                    {
                        if (xdtD.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sD, out var dtoD))
                        {
                            result = XdmValue.FromDate(dtoD, hasTzD);
                        }
                        else
                        {
                            result = XdmValue.FromDate(xdtD, hasTzD);
                        }
                        return true;
                    }
                    // Fallback for backward compatibility with dateTime-shaped strings cast to date
                    // (removed - DateTimeOffset.TryParse is too lenient and accepts invalid formats)
                }
                return false;

            case "time":
                if (value.Kind == XdmValueKind.Time)
                    return true;
                if (value.Kind == XdmValueKind.DateTime)
                {
                    var xdtDt = value.DateTimeXPathValue;
                    bool hasTz = value.HasTimezone;
                    result = XdmValue.FromTime(new XPathDateTime(1, 1, 1, xdtDt.Hour, xdtDt.Minute, xdtDt.Second, xdtDt.Millisecond, xdtDt.TimezoneOffsetMinutes, hasTz), hasTz);
                    return true;
                }
                if (value.Kind == XdmValueKind.Date)
                    return false;
                {
                    string sT = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathTime(sT, out var xdtT, out var hasTzT))
                    {
                        if (xdtT.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sT, out var dtoT))
                        {
                            result = XdmValue.FromTime(dtoT, hasTzT);
                        }
                        else
                        {
                            result = XdmValue.FromTime(xdtT, hasTzT);
                        }
                        return true;
                    }
                    // Fallback for backward compatibility with dateTime-shaped strings cast to time
                    // (removed - DateTimeOffset.TryParse is too lenient and accepts invalid formats)
                }
                return false;

            case "untypedatomic":
                result = XdmValue.FromString(value.ToString(), "untypedAtomic");
                return true;

            case "anyuri":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Date
                    or XdmValueKind.Time or XdmValueKind.DateTime or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                {
                    // XML Schema anyURI has whiteSpace="collapse"
                    string sUri = CollapseWhitespace(value.ToString());
                    // Reject invalid percent-encoding sequences
                    if (!IsValidAnyUri(sUri))
                        return false;
                    result = XdmValue.FromString(sUri, "anyURI");
                    return true;
                }

            case "base64binary":
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Integer
                    or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float
                    or XdmValueKind.Boolean or XdmValueKind.Node)
                    return false;
                {
                    string sB64 = value.ToString();
                    // Cross-cast from hexBinary: decode hex to bytes, encode as base64
                    if (value.SchemaTypeName is not null && value.SchemaTypeName.Equals("hexBinary", StringComparison.OrdinalIgnoreCase))
                    {
                        string hex = sB64.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        try
                        {
                            byte[] bytes = Convert.FromHexString(hex);
                            result = XdmValue.FromString(Convert.ToBase64String(bytes), "base64Binary");
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    string normalizedB64 = sB64.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    if (!IsValidBase64(normalizedB64))
                        return false;
                    result = XdmValue.FromString(normalizedB64, "base64Binary");
                    return true;
                }

            case "hexbinary":
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Integer
                    or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float
                    or XdmValueKind.Boolean or XdmValueKind.Node)
                    return false;
                {
                    string sHex = value.ToString().Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    // Cross-cast from base64Binary: decode base64 to bytes, encode as hex
                    if (value.SchemaTypeName is not null && value.SchemaTypeName.Equals("base64Binary", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            byte[] bytes = Convert.FromBase64String(sHex);
                            result = XdmValue.FromString(Convert.ToHexString(bytes), "hexBinary");
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    if (!Regex.IsMatch(sHex, @"^[0-9a-fA-F]*$"))
                        return false;
                    if (sHex.Length % 2 != 0)
                        return false;
                    result = XdmValue.FromString(sHex.ToUpperInvariant(), "hexBinary");
                    return true;
                }

            case "duration":
                if (value.Kind == XdmValueKind.Duration)
                    return true;
                {
                    string sDur = value.ToString().Trim();
                    if (IsValidDuration(sDur))
                    {
                        result = XdmValue.FromDuration(CanonicalizeDuration(sDur));
                        return true;
                    }
                }
                return false;

            case "yearmonthduration":
                if (value.Kind == XdmValueKind.Duration)
                {
                    result = XdmValue.FromDuration(ExtractYearMonthDuration(value.DurationValue));
                    return true;
                }
                {
                    string sYm = value.ToString().Trim();
                    if (IsValidYearMonthDuration(sYm))
                    {
                        result = XdmValue.FromDuration(ExtractYearMonthDuration(sYm));
                        return true;
                    }
                }
                return false;

            case "daytimeduration":
                if (value.Kind == XdmValueKind.Duration)
                {
                    result = XdmValue.FromDuration(ExtractDayTimeDuration(value.DurationValue));
                    return true;
                }
                {
                    string sDt = value.ToString().Trim();
                    if (IsValidDayTimeDuration(sDt))
                    {
                        result = XdmValue.FromDuration(ExtractDayTimeDuration(sDt));
                        return true;
                    }
                }
                return false;

            case "gyear":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtY = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtY.FormatTimezone();
                    result = XdmValue.FromString($"{xdtY.FormatYear()}{tz}", "gYear");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^(-?)(\d{4,})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        string sign = m.Groups[1].Value;
                        string yearStr = m.Groups[2].Value;
                        string tz = m.Groups[3].Value;
                        // Reject leading zeros for years longer than 4 digits
                        if (yearStr.Length > 4 && yearStr[0] == '0')
                            return false;
                        // Validate and normalize timezone
                        if (!string.IsNullOrEmpty(tz))
                        {
                            if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase))
                            {
                                tz = "Z";
                            }
                            else
                            {
                                int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
                                int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
                                if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
                                    return false;
                                if (tzHour == 0 && tzMin == 0)
                                    tz = "Z";
                            }
                        }
                        // Reject years too large to fit in long (overflow)
                        if (yearStr.Length > 18)
                            return false;
                        // Normalize -0000 to 0000
                        if (sign == "-" && yearStr.TrimStart('0') == "")
                        {
                            sign = "";
                            yearStr = "0000";
                        }
                        result = XdmValue.FromString(sign + yearStr + tz, "gYear");
                        return true;
                    }
                }
                return false;

            case "gyearmonth":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtYm = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtYm.FormatTimezone();
                    result = XdmValue.FromString($"{xdtYm.FormatYear()}-{xdtYm.Month:00}{tz}", "gYearMonth");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^(-?)(\d{4,})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        string sign = m.Groups[1].Value;
                        string yearStr = m.Groups[2].Value;
                        string monthStr = m.Groups[3].Value;
                        if (!int.TryParse(monthStr, out int monthVal) || monthVal < 1 || monthVal > 12)
                            return false;
                        string rest = $"-{monthStr}{m.Groups[4].Value}";
                        // Reject leading zeros for years longer than 4 digits
                        if (yearStr.Length > 4 && yearStr[0] == '0')
                            return false;
                        // Validate and normalize timezone
                        string tz = rest[3..]; // after -MM
                        if (!string.IsNullOrEmpty(tz))
                        {
                            if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase))
                            {
                                rest = rest[..3] + "Z";
                            }
                            else
                            {
                                int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
                                int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
                                if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
                                    return false;
                                if (tzHour == 0 && tzMin == 0)
                                    rest = rest[..3] + "Z";
                            }
                        }
                        // Reject years too large to fit in long (overflow)
                        if (yearStr.Length > 18)
                            return false;
                        // Normalize -0000 to 0000
                        if (sign == "-" && yearStr.TrimStart('0') == "")
                        {
                            sign = "";
                            yearStr = "0000";
                        }
                        result = XdmValue.FromString(sign + yearStr + rest, "gYearMonth");
                        return true;
                    }
                }
                return false;

            case "gmonthday":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtMd = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtMd.FormatTimezone();
                    result = XdmValue.FromString($"--{xdtMd.Month:00}-{xdtMd.Day:00}{tz}", "gMonthDay");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^--(\d{2})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int month) || month < 1 || month > 12)
                            return false;
                        if (!int.TryParse(m.Groups[2].Value, out int day) || day < 1 || day > 31)
                            return false;
                        // Validate days per month (Feb max 29, Apr/Jun/Sep/Nov max 30)
                        int maxDay = month == 2 ? 29 : (month is 4 or 6 or 9 or 11 ? 30 : 31);
                        if (day > maxDay) return false;
                        string tz = m.Groups[3].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"--{m.Groups[1].Value}-{m.Groups[2].Value}{normalizedTz}", "gMonthDay");
                        return true;
                    }
                }
                return false;

            case "gday":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtD = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtD.FormatTimezone();
                    result = XdmValue.FromString($"---{xdtD.Day:00}{tz}", "gDay");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^---(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int day) || day < 1 || day > 31)
                            return false;
                        string tz = m.Groups[2].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"---{m.Groups[1].Value}{normalizedTz}", "gDay");
                        return true;
                    }
                }
                return false;

            case "gmonth":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtM = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtM.FormatTimezone();
                    result = XdmValue.FromString($"--{xdtM.Month:00}{tz}", "gMonth");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^--(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int month) || month < 1 || month > 12)
                            return false;
                        string tz = m.Groups[2].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"--{m.Groups[1].Value}{normalizedTz}", "gMonth");
                        return true;
                    }
                }
                return false;

            case "ncname":
            case "id":
            case "idref":
            case "entity":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\p{L}_][\w.\-]*$"))
                {
                    result = XdmValue.FromString(s, normalized);
                    return true;
                }
                return false;
            }

            case "name":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\p{L}_:][\w.:\-]*$"))
                {
                    result = XdmValue.FromString(s, "Name");
                    return true;
                }
                return false;
            }

            case "nmtoken":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\w.:\-]+$"))
                {
                    result = XdmValue.FromString(s, "NMTOKEN");
                    return true;
                }
                return false;
            }

            case "language":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$"))
                {
                    result = XdmValue.FromString(s, "language");
                    return true;
                }
                return false;
            }

            case "normalizedstring":
            {
                string s = value.ToString();
                // XML Schema whiteSpace="replace": replace tab, CR, LF with space
                s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                result = XdmValue.FromString(s, "normalizedString");
                return true;
            }
            case "token":
            {
                string s = value.ToString();
                // XML Schema whiteSpace="collapse": replace tab/CR/LF with space,
                // trim leading/trailing spaces, collapse internal runs of spaces
                s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                s = s.Trim(' ');
                while (s.Contains("  "))
                    s = s.Replace("  ", " ");
                result = XdmValue.FromString(s, "token");
                return true;
            }

            case "idrefs":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var idrefTokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var idrefItems = new XdmValue[idrefTokens.Length];
                for (int i = 0; i < idrefTokens.Length; i++)
                {
                    if (!Regex.IsMatch(idrefTokens[i], @"^[\p{L}_][\w.\-]*$"))
                        return false;
                    idrefItems[i] = XdmValue.FromString(idrefTokens[i], "IDREF");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(idrefItems));
                return true;
            }

            case "nmtokens":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var nmtokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var nmtokenItems = new XdmValue[nmtokens.Length];
                for (int i = 0; i < nmtokens.Length; i++)
                {
                    if (!Regex.IsMatch(nmtokens[i], @"^[\w.:\-]+$"))
                        return false;
                    nmtokenItems[i] = XdmValue.FromString(nmtokens[i], "NMTOKEN");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(nmtokenItems));
                return true;
            }

            case "entities":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var entityTokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var entityItems = new XdmValue[entityTokens.Length];
                for (int i = 0; i < entityTokens.Length; i++)
                {
                    if (!Regex.IsMatch(entityTokens[i], @"^[\p{L}_][\w.\-]*$"))
                        return false;
                    entityItems[i] = XdmValue.FromString(entityTokens[i], "ENTITY");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(entityItems));
                return true;
            }

            case "qname":
                if (value.Kind == XdmValueKind.QName)
                    return true;
                if (value.Kind == XdmValueKind.String)
                {
                    string sQName = value.StringValue.Trim();
                    if (string.IsNullOrEmpty(sQName))
                        return false;
                    // Validate lexical QName: prefix:local or local (no prefix)
                    int colon = sQName.IndexOf(':');
                    if (colon >= 0)
                    {
                        string prefix = sQName[..colon];
                        string local = sQName[(colon + 1)..];
                        if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local))
                            return false;
                        // Prefix must be valid NCName
                        if (!Regex.IsMatch(prefix, @"^[\p{L}_][\w.\-]*$"))
                            return false;
                        // Local must be valid NCName
                        if (!Regex.IsMatch(local, @"^[\p{L}_][\w.\-]*$"))
                            return false;
                    }
                    else
                    {
                        // No prefix - just local name
                        if (!Regex.IsMatch(sQName, @"^[A-Za-z_][\w.\-]*$"))
                            return false;
                    }
                    result = XdmValue.FromString(sQName);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static readonly Regex DurationPartsRegex = new(
        @"^(?<sign>[-+]?)(?<P>P)(?<Y>\d+Y)?(?<M>\d+M)?(?<D>\d+D)?(?<T>T(?<H>\d+H)?(?<Tm>\d+M)?(?<S>\d+(?:\.\d+)?S)?)?$",
        RegexOptions.Compiled);

    private static bool IsValidAnyUri(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '%')
            {
                if (i + 2 >= s.Length)
                    return false;
                if (!IsHexDigit(s[i + 1]) || !IsHexDigit(s[i + 2]))
                    return false;
            }
        }
        return true;
    }

    private static bool IsHexDigit(char c)
        => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');

    private static string CollapseWhitespace(string s)
    {
        s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
        s = s.Trim(' ');
        while (s.Contains("  "))
            s = s.Replace("  ", " ");
        return s;
    }

    private static bool IsMixedDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return false;
        bool hasYm = m.Groups["Y"].Success || m.Groups["M"].Success;
        bool hasDt = m.Groups["D"].Success || m.Groups["T"].Success;
        return hasYm && hasDt;
    }

    private static string? NormalizeTimezone(string tz)
    {
        if (string.IsNullOrEmpty(tz)) return "";
        if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase)) return "Z";
        int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
        int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
        if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
            return null;
        if (tzHour == 0 && tzMin == 0)
            return "Z";
        return tz;
    }

    private static string CanonicalizeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int years = 0, months = 0, days = 0, hours = 0, minutes = 0;
        double seconds = 0;
        if (m.Groups["Y"].Success) years = int.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture);
        if (m.Groups["M"].Success) months = int.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["D"].Success) days = int.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture);
        if (m.Groups["H"].Success) hours = int.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture);
        if (m.Groups["Tm"].Success) minutes = int.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["S"].Success) seconds = double.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture);

        years += months / 12;
        months %= 12;

        minutes += (int)(seconds / 60);
        seconds = seconds % 60;
        hours += minutes / 60;
        minutes %= 60;
        days += hours / 24;
        hours %= 24;

        if (years == 0 && months == 0 && days == 0 && hours == 0 && minutes == 0 && seconds == 0)
            sign = "";

        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(sign)) sb.Append('-');
        sb.Append('P');
        if (years > 0) sb.Append(years).Append('Y');
        if (months > 0) sb.Append(months).Append('M');
        if (days > 0) sb.Append(days).Append('D');
        bool hasTime = hours > 0 || minutes > 0 || seconds > 0;
        if (hasTime || (years == 0 && months == 0 && days == 0))
        {
            sb.Append('T');
            if (hours > 0) sb.Append(hours).Append('H');
            if (minutes > 0) sb.Append(minutes).Append('M');
            if (seconds > 0 || (hours == 0 && minutes == 0))
                sb.Append(FormatDurationSeconds(seconds)).Append('S');
        }
        return sb.ToString();
    }

    public static string ExtractYearMonthDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int years = 0, months = 0;
        if (m.Groups["Y"].Success) years = int.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture);
        if (m.Groups["M"].Success) months = int.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        years += months / 12;
        months %= 12;
        if (years == 0 && months == 0) sign = "";
        string result = sign + "P";
        if (years > 0) result += years + "Y";
        if (months > 0 || years == 0) result += months + "M";
        return result;
    }

    public static string ExtractDayTimeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int days = 0, hours = 0, minutes = 0;
        double seconds = 0;
        if (m.Groups["D"].Success) days = int.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture);
        if (m.Groups["H"].Success) hours = int.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture);
        if (m.Groups["Tm"].Success) minutes = int.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["S"].Success) seconds = double.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture);

        minutes += (int)(seconds / 60);
        seconds = seconds % 60;
        hours += minutes / 60;
        minutes %= 60;
        days += hours / 24;
        hours %= 24;

        if (days == 0 && hours == 0 && minutes == 0 && seconds == 0) sign = "";

        string result = sign + "P";
        if (days > 0) result += days + "D";
        bool hasTime = hours > 0 || minutes > 0 || seconds > 0 || days == 0;
        if (hasTime)
        {
            result += "T";
            if (hours > 0) result += hours + "H";
            if (minutes > 0) result += minutes + "M";
            if (seconds > 0 || (hours == 0 && minutes == 0)) result += FormatDurationSeconds(seconds) + "S";
        }
        return result;
    }

    private static string FormatDurationSeconds(double seconds)
    {
        string s = seconds.ToString("0.0#########", CultureInfo.InvariantCulture);
        s = s.TrimEnd('0').TrimEnd('.');
        if (s == "0" || s == "-0") s = "0";
        return s;
    }

    private static readonly Regex DurationComponentRegex = new(@"(\d+)([YMDHST])", RegexOptions.Compiled);

    private static bool IsValidDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // Disallow leading '+'; 'T' must be followed by at least one time component
        if (!Regex.IsMatch(s, @"^-?P(\d+Y)?(\d+M)?(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$"))
            return false;
        if (s == "P" || s == "-P") return false;
        // Reject 'T' without following components (e.g., P1DT, P1Y24MT)
        int tIdx = s.IndexOf('T');
        if (tIdx >= 0)
        {
            bool hasTimeComponent = s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
            if (!hasTimeComponent) return false;
        }
        // Reject absurdly large components
        foreach (Match m in DurationComponentRegex.Matches(s))
        {
            if (long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) > 999999999999L)
                return false;
        }
        return true;
    }

    private static bool IsValidYearMonthDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (!Regex.IsMatch(s, @"^-?P(\d+Y)?(\d+M)?$"))
            return false;
        return s.Contains('Y') || s.Contains('M');
    }

    private static bool IsValidDayTimeDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // Disallow leading '+'; 'T' must be followed by at least one time component
        if (!Regex.IsMatch(s, @"^-?P(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$"))
            return false;
        if (s.Contains('Y')) return false;
        // Reject 'M' before 'T' (months), but allow 'M' after 'T' (minutes)
        int tIdx = s.IndexOf('T');
        if (tIdx >= 0)
        {
            if (s.IndexOf('M') >= 0 && s.IndexOf('M') < tIdx) return false;
            bool hasTimeComponent = s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
            if (!hasTimeComponent) return false;
        }
        else
        {
            if (s.Contains('M')) return false;
        }
        if (s.Contains('D')) return true;
        if (tIdx < 0) return false;
        return s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
    }

    public static bool IsValidBase64(string s)
    {
        if (string.IsNullOrEmpty(s)) return true;
        s = s.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
        if (s.Length % 4 != 0) return false;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        foreach (char c in s)
            if (!chars.Contains(c) && c != '=')
                return false;
        int eq = s.IndexOf('=');
        if (eq >= 0)
        {
            for (int i = eq; i < s.Length; i++)
                if (s[i] != '=')
                    return false;
            if (s.Length - eq > 2)
                return false;
            // Validate padding rules:
            // '==' : preceding char must be one of AQgw (lower 4 bits == 0)
            // '='  : preceding char must be one of AEIMQUYcgkosw048 (lower 2 bits == 0)
            if (eq > 0)
            {
                char lastData = s[eq - 1];
                int padding = s.Length - eq;
                if (padding == 2)
                {
                    const string validForDoublePad = "AQgw";
                    if (!validForDoublePad.Contains(lastData))
                        return false;
                }
                else if (padding == 1)
                {
                    const string validForSinglePad = "AEIMQUYcgkosw048";
                    if (!validForSinglePad.Contains(lastData))
                        return false;
                }
            }
        }
        return true;
    }

    private static bool InstanceOf(XdmValue value, string typeName, OccurrenceIndicator occurrence)
    {
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "");

        if (normalized == "empty-sequence")
            return value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);

        // Check cardinality
        int count;
        if (value.IsUndefined)
            count = 0;
        else if (!value.IsSequence)
            count = 1;
        else if (!TryGetSequenceLength(value.SequenceValue, out count))
        {
            // Materialize to count
            count = 0;
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
                count++;
        }

        bool cardinalityOk = occurrence switch
        {
            OccurrenceIndicator.One => count == 1,
            OccurrenceIndicator.ZeroOrOne => count <= 1,
            OccurrenceIndicator.ZeroOrMore => true,
            OccurrenceIndicator.OneOrMore => count >= 1,
            _ => count == 1
        };

        if (!cardinalityOk)
            return false;

        if (count == 0)
            return true;

        // Check each item's type
        if (!value.IsSequence)
            return ItemInstanceOf(value, normalized);

        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
        {
            if (!ItemInstanceOf(item, normalized))
                return false;
        }
        return true;
    }

    private static bool ItemInstanceOf(XdmValue value, string normalized)
    {
        return normalized switch
        {
            "string" => value.Kind == XdmValueKind.String && IsStringSubtype(value.SchemaTypeName),
            "integer" or "int" or "long" or "short" or "byte"
                or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
                or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger"
                => value.Kind == XdmValueKind.Integer,
            "decimal" => value.Kind is XdmValueKind.Decimal or XdmValueKind.Integer,
            "double" => value.Kind == XdmValueKind.Double,
            "float" => value.Kind == XdmValueKind.Float,
            "boolean" => value.Kind == XdmValueKind.Boolean,
            "datetime" => value.Kind == XdmValueKind.DateTime,
            "date" => value.Kind == XdmValueKind.Date,
            "time" => value.Kind == XdmValueKind.Time,
            "duration" or "daytimeduration" or "yearmonthduration" => value.Kind == XdmValueKind.Duration,
            "qname" => value.Kind == XdmValueKind.QName,
            "gyear" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gYear", StringComparison.OrdinalIgnoreCase) == true,
            "gyearmonth" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gYearMonth", StringComparison.OrdinalIgnoreCase) == true,
            "gmonthday" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gMonthDay", StringComparison.OrdinalIgnoreCase) == true,
            "gday" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gDay", StringComparison.OrdinalIgnoreCase) == true,
            "gmonth" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gMonth", StringComparison.OrdinalIgnoreCase) == true,
            "hexbinary" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("hexBinary", StringComparison.OrdinalIgnoreCase) == true,
            "base64binary" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("base64Binary", StringComparison.OrdinalIgnoreCase) == true,
            "anyuri" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("anyURI", StringComparison.OrdinalIgnoreCase) == true,
            "untypedatomic" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true,
            "normalizedstring" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("normalizedString", StringComparison.OrdinalIgnoreCase) == true,
            "token" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("token", StringComparison.OrdinalIgnoreCase) == true,
            "language" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("language", StringComparison.OrdinalIgnoreCase) == true,
            "nmtoken" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("NMTOKEN", StringComparison.OrdinalIgnoreCase) == true,
            "name" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("Name", StringComparison.OrdinalIgnoreCase) == true,
            "ncname" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("NCName", StringComparison.OrdinalIgnoreCase) == true,
            "id" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("ID", StringComparison.OrdinalIgnoreCase) == true,
            "idref" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("IDREF", StringComparison.OrdinalIgnoreCase) == true,
            "entity" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("ENTITY", StringComparison.OrdinalIgnoreCase) == true,
            "node" => value.IsNode,
            "item" => !value.IsUndefined,
            _ => false
        };
    }

    private static bool IsStringSubtype(string? schemaTypeName)
    {
        if (schemaTypeName is null) return true;
        return schemaTypeName.ToLowerInvariant() is
            "normalizedstring" or "token" or "language" or "nmtoken" or "name"
            or "ncname" or "id" or "idref" or "entity" or "untypedatomic";
    }

    private static bool IsCastAllowed(string? sourceSchemaType, string targetType)
    {
        if (string.IsNullOrEmpty(sourceSchemaType))
            return true;

        sourceSchemaType = sourceSchemaType.ToLowerInvariant().Replace("xs:", "");
        targetType = targetType.ToLowerInvariant().Replace("xs:", "");

        // gYear, gYearMonth, gMonthDay, gDay, gMonth can only cast to themselves, string, untypedAtomic
        if (sourceSchemaType is "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth")
        {
            return sourceSchemaType == targetType || targetType is "string" or "untypedatomic";
        }

        // hexBinary and base64Binary can cast to themselves, each other, string, untypedAtomic
        if (sourceSchemaType is "hexbinary" or "base64binary")
        {
            return targetType is "hexbinary" or "base64binary" or "string" or "untypedatomic";
        }

        // anyURI can cast to itself, string, untypedAtomic
        if (sourceSchemaType == "anyuri")
        {
            return targetType is "anyuri" or "string" or "untypedatomic";
        }

        // Other schema types (normalizedString, token, etc.) allow any cast
        return true;
    }

    private static bool IsIntegerInRange(long value, string typeName)
    {
        return typeName switch
        {
            "byte" => value >= sbyte.MinValue && value <= sbyte.MaxValue,
            "short" => value >= short.MinValue && value <= short.MaxValue,
            "int" => value >= int.MinValue && value <= int.MaxValue,
            "long" or "integer" => true,
            "unsignedbyte" => value >= byte.MinValue && value <= byte.MaxValue,
            "unsignedshort" => value >= ushort.MinValue && value <= ushort.MaxValue,
            "unsignedint" => value >= uint.MinValue && value <= uint.MaxValue,
            "unsignedlong" => value >= 0,
            "positiveinteger" => value > 0,
            "negativeinteger" => value < 0,
            "nonpositiveinteger" => value <= 0,
            "nonnegativeinteger" => value >= 0,
            _ => true
        };
    }

    /// <summary>
    /// Checks whether an XDM value matches a declared type name (e.g. "xs:string", "element(foo)").
    /// </summary>
    private static bool ValueMatchesType(XdmValue value, string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return true;

        string normalized = typeName.Trim().ToLowerInvariant();

        // Strip occurrence indicator
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        // Strip xs: prefix
        if (normalized.StartsWith("xs:"))
            normalized = normalized[3..];

        if (normalized == "item()")
            return !value.IsUndefined;

        if (normalized == "empty-sequence()")
            return value.IsUndefined;

        if (normalized.StartsWith("element(") && normalized.EndsWith(')'))
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Element;

        if (normalized.StartsWith("attribute(") && normalized.EndsWith(')'))
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Attribute;

        return ItemInstanceOf(value, normalized);
    }

    /// <summary>
    /// Checks whether an XDM value matches a simple <see cref="XdmValueKind"/>.
    /// </summary>
    private static bool ValueMatchesXdmKind(XdmValue value, XdmValueKind kind)
    {
        return kind switch
        {
            XdmValueKind.String => value.Kind == XdmValueKind.String,
            XdmValueKind.Integer => value.Kind == XdmValueKind.Integer,
            XdmValueKind.Decimal => value.Kind == XdmValueKind.Decimal,
            XdmValueKind.Double => value.Kind == XdmValueKind.Double,
            XdmValueKind.Float => value.Kind == XdmValueKind.Float,
            XdmValueKind.Boolean => value.Kind == XdmValueKind.Boolean,
            XdmValueKind.DateTime => value.Kind == XdmValueKind.DateTime,
            XdmValueKind.Date => value.Kind == XdmValueKind.Date,
            XdmValueKind.Time => value.Kind == XdmValueKind.Time,
            XdmValueKind.Duration => value.Kind == XdmValueKind.Duration,
            XdmValueKind.Node => value.IsNode,
            _ => true
        };
    }

    // ------------------------------------------------------------------
    // Type promotion helpers
    // ------------------------------------------------------------------

    private static bool HasTimezoneSuffix(string s)
    {
        string t = s.Trim();
        return t.EndsWith('Z') || t.EndsWith('z') || System.Text.RegularExpressions.Regex.IsMatch(t, @"[Tt]\d{2}:\d{2}:\d{2}[Zz]|[Tt]\d{2}:\d{2}:\d{2}[+\-]\d{2}:\d{2}$|[+\-]\d{2}:\d{2}$");
    }

    private static string FormatXPathTimezone(DateTimeOffset dto, bool hasTz)
    {
        if (!hasTz) return "";
        string tz = dto.ToString("zzz", System.Globalization.CultureInfo.InvariantCulture);
        return tz == "+00:00" ? "Z" : tz;
    }

    private static readonly Regex XPathDateTimeRegex = new(
        @"^(?<year>[+-]?\d{4,})-(?<month>\d{2})-(?<day>\d{2})T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<frac>\d+))?(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static readonly Regex XPathDateRegex = new(
        @"^(?<year>[+-]?\d{4,})-(?<month>\d{2})-(?<day>\d{2})(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static readonly Regex XPathTimeRegex = new(
        @"^(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<frac>\d+))?(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static bool TryParseXPathDateTime(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathDateTimeRegex.Match(s);
        if (!m.Success) return false;

        string yearStr = m.Groups["year"].Value;
        long year = long.Parse(yearStr, CultureInfo.InvariantCulture);
        if (Math.Abs(year) > 999999999999L) return false;
        // Reject + sign and leading zeros for years longer than 4 digits
        if (yearStr.StartsWith('+')) return false;
        if (yearStr.Length > 4 && yearStr[0] == '0') return false;

        int month = int.Parse(m.Groups["month"].Value, CultureInfo.InvariantCulture);
        int day = int.Parse(m.Groups["day"].Value, CultureInfo.InvariantCulture);
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;
        if (day > DaysInMonth(year, month)) return false;

        int hour = int.Parse(m.Groups["hour"].Value, CultureInfo.InvariantCulture);
        int minute = int.Parse(m.Groups["minute"].Value, CultureInfo.InvariantCulture);
        int second = int.Parse(m.Groups["second"].Value, CultureInfo.InvariantCulture);
        int millisecond = 0;
        bool hasFrac = m.Groups["frac"].Success;
        if (hasFrac)
        {
            string frac = m.Groups["frac"].Value;
            // Take up to 3 digits for milliseconds
            if (frac.Length > 3) frac = frac[..3];
            millisecond = int.Parse(frac.PadRight(3, '0'), CultureInfo.InvariantCulture);
        }

        // Validate time components
        if (hour > 24 || minute > 59 || second > 59) return false;
        if (hour == 24 && (minute != 0 || second != 0 || hasFrac)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
            {
                tzMinutes = 0;
            }
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        xdt = new XPathDateTime(year, month, day, hour, minute, second, millisecond, tzMinutes, hasTz);
        return true;
    }

    private static bool TryParseXPathDate(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathDateRegex.Match(s);
        if (!m.Success) return false;

        string yearStr = m.Groups["year"].Value;
        long year = long.Parse(yearStr, CultureInfo.InvariantCulture);
        if (Math.Abs(year) > 999999999999L) return false;
        // Reject + sign and leading zeros for years longer than 4 digits
        if (yearStr.StartsWith('+')) return false;
        if (yearStr.Length > 4 && yearStr[0] == '0') return false;

        int month = int.Parse(m.Groups["month"].Value, CultureInfo.InvariantCulture);
        int day = int.Parse(m.Groups["day"].Value, CultureInfo.InvariantCulture);
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;
        if (day > DaysInMonth(year, month)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
                tzMinutes = 0;
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        xdt = new XPathDateTime(year, month, day, 0, 0, 0, 0, tzMinutes, hasTz);
        return true;
    }

    private static bool TryParseXPathTime(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathTimeRegex.Match(s);
        if (!m.Success) return false;

        int hour = int.Parse(m.Groups["hour"].Value, CultureInfo.InvariantCulture);
        int minute = int.Parse(m.Groups["minute"].Value, CultureInfo.InvariantCulture);
        int second = int.Parse(m.Groups["second"].Value, CultureInfo.InvariantCulture);
        int millisecond = 0;
        bool hasFrac = m.Groups["frac"].Success;
        if (hasFrac)
        {
            string frac = m.Groups["frac"].Value;
            if (frac.Length > 3) frac = frac[..3];
            millisecond = int.Parse(frac.PadRight(3, '0'), CultureInfo.InvariantCulture);
        }

        // Validate time components
        if (hour > 24 || minute > 59 || second > 59) return false;
        if (hour == 24 && (minute != 0 || second != 0 || hasFrac)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
                tzMinutes = 0;
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        xdt = new XPathDateTime(1, 1, 1, hour, minute, second, millisecond, tzMinutes, hasTz);
        return true;
    }

    private static int DaysInMonth(long year, int month)
    {
        return month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => IsLeapYear(year) ? 29 : 28,
            _ => 0
        };
    }

    private static bool IsLeapYear(long year)
    {
        if (year == 0) return true; // Year 0 is a leap year in XML Schema (proleptic Gregorian)
        if (year < 0) year = -year + 1; // Adjust for year -1 = 2 BCE, etc.
        return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    }

    private static bool IsValidTimezone(string tz)
    {
        // tz is like "+14:00" or "-05:00"
        int hours = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
        int minutes = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
        return hours <= 14 && !(hours == 14 && minutes > 0) && minutes <= 59;
    }

    private static int ParseTimezoneOffset(string tz)
    {
        // tz is like "+14:00" or "-05:00"
        bool negative = tz[0] == '-';
        var parts = tz[1..].Split(':');
        int hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
        int total = hours * 60 + minutes;
        return negative ? -total : total;
    }

    private static string NormalizeDateTimeString(string s)
    {
        // XML Schema allows T24:00:00 to represent midnight of the next day.
        // .NET's DateTimeOffset.TryParse does not handle this, so normalize it.
        int idx = s.IndexOf("T24:00:00");
        if (idx >= 0)
        {
            int after = idx + "T24:00:00".Length;
            // Do not normalize if followed by fractional seconds (e.g., T24:00:00.001)
            if (after >= s.Length || s[after] != '.')
            {
                string datePart = s[..idx];
                string rest = s[after..];
                if (DateTimeOffset.TryParse(datePart, out var dto))
                {
                    dto = dto.AddDays(1);
                    string newDate = dto.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    return newDate + "T00:00:00" + rest;
                }
            }
        }
        return s;
    }

    private static bool TryParseDouble(string s, out double result)
    {
        s = s.Trim();
        if (s == "INF" || s == "+INF")
        {
            result = double.PositiveInfinity;
            return true;
        }
        if (s == "-INF")
        {
            result = double.NegativeInfinity;
            return true;
        }
        if (s == "NaN")
        {
            result = double.NaN;
            return true;
        }
        // Explicitly reject case variants that .NET's double.TryParse would accept
        string upper = s.ToUpperInvariant();
        if (upper is "NAN" or "INF" or "+INF" or "-INF" or "INFINITY" or "+INFINITY" or "-INFINITY")
        {
            result = 0;
            return false;
        }
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool IsDouble(XdmValue value) =>
        value.Kind == XdmValueKind.Double;

    private static bool IsFloat(XdmValue value) =>
        value.Kind == XdmValueKind.Float;

    private static bool IsDuration(XdmValue value) =>
        value.Kind == XdmValueKind.Duration;

    private static bool IsEmptySeq(XdmValue value)
    {
        if (!value.IsSequence || value.SequenceValue is null)
            return false;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue))
            return false;
        return true;
    }

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

    private static float ToFloat(XdmValue value) => value.Kind switch
    {
        XdmValueKind.Integer => value.IntegerValue,
        XdmValueKind.Decimal => (float)value.DecimalValue,
        XdmValueKind.Double or XdmValueKind.Float => (float)value.DoubleValue,
        XdmValueKind.Boolean => value.BooleanValue ? 1.0f : 0.0f,
        _ => float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : throw new InvalidOperationException($"Cannot convert {value.Kind} to float")
    };

    // ------------------------------------------------------------------
    // Opcode helpers
    // ------------------------------------------------------------------

    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");
        return Atomize(value);
    }

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
            return value.NodeValue.StringValue;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizedString(item);
            return string.Empty;
        }

        return value.ToString();
    }

    private static string NormalizeSpaceString(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static RegexOptions ParseRegexFlags(string flags)
    {
        var options = RegexOptions.None;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break;
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
            }
        }
        return options;
    }

    private static XdmValue Sum(System.Collections.Generic.List<XdmValue> items)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = Atomize(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
        }
        if (allIntegerOrDecimal)
        {
            decimal sum = 0m;
            foreach (var item in items)
                sum += ToDecimal(Atomize(item));
            return XdmValue.FromDecimal(sum);
        }
        double sumD = 0.0;
        foreach (var item in items)
            sumD += ToDouble(Atomize(item));
        return XdmValue.FromDouble(sumD);
    }

    private static XdmValue MinMax(System.Collections.Generic.List<XdmValue> items, bool min)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = Atomize(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
        }
        if (allIntegerOrDecimal)
        {
            decimal result = ToDecimal(Atomize(items[0]));
            for (int i = 1; i < items.Count; i++)
            {
                decimal v = ToDecimal(Atomize(items[i]));
                if (min ? v < result : v > result)
                    result = v;
            }
            return XdmValue.FromDecimal(result);
        }
        double resultD = ToDouble(Atomize(items[0]));
        for (int i = 1; i < items.Count; i++)
        {
            double v = ToDouble(Atomize(items[i]));
            if (min ? v < resultD : v > resultD)
                resultD = v;
        }
        return XdmValue.FromDouble(resultD);
    }
}
