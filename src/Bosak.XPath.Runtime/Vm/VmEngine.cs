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
//                      | Charles Korthout | 1.4   | 29-05-2026     | Fixed TryCast to return empty sequence for empty input (xs:type(()) semantics)         |
//                      | Charles Korthout | 1.5   | 30-05-2026     | Fixed Compare/CompareGeneral to return empty sequence for empty operands; added backwards-compatible coercion |
//                      | Charles Korthout | 1.6   | 30-05-2026     | Added PathStepMap opcode for per-context-item predicate evaluation on path steps        |
//                      | Charles Korthout | 1.7   | 30-05-2026     | Filter opcode treats double/decimal/float predicates as numeric position (fixes path-007/008) |
//                      | Charles Korthout | 1.8   | 30-05-2026     | IsSameNode unwraps singleton sequences; returns empty for empty-seq operand (fixes boolean-074/075) |
//                      | Charles Korthout | 1.9   | 31-05-2026     | Implemented PrecedesNode/FollowsNode (<< / >>) using DocumentOrder                          |
//                      | Charles Korthout | 2.0   | 01-06-2026     | Include XPST0017 error code in function-not-found exceptions                             |
//                      | Charles Korthout | 2.1   | 02-06-2026     | Numeric predicate uses exact equality, not Math.Round (XPath 2.0 §3.2.4)                 |
//                      | Charles Korthout | 2.2   | 03-06-2026     | MultiplyOrAddInteger: detect overflow, promote to decimal (fixes number-0111)            |
//                      | Charles Korthout | 2.3   | 01-06-2026     | Use module.MaxRegisterCount instead of hardcoded 256 for register array sizing           |
//                      | Charles Korthout | 2.4   | 05-06-2026     | Inline function sequence param validation; numeric promotion; node()/anyAtomicType matching |
//                      | Charles Korthout | 2.5   | 05-06-2026     | Removed global NormalizeSequence from Execute; path/union already normalize via opcodes     |
//                      | Charles Korthout | 2.6   | 05-06-2026     | Added function(*)/map(*)/array(*) support to ValueMatchesType for instance-of checks      |
//                      | Charles Korthout | 2.7   | 05-06-2026     | Added typed function signature matching (function(T...) as R) with contravariant params   |
//                      | Charles Korthout | 2.8   | 05-06-2026     | Node comparison operators raise XPTY0004 for non-node operands; ParseException XPST0003  |
//                      | Charles Korthout | 2.9   | 05-06-2026     | ResolveVariableName handles Q{uri}local; inline function params bind by expanded QName     |
//                      | Charles Korthout | 2.10  | 11-06-2026     | Apply opcode invokes map/array functions; date comparison casts untypedAtomic operands    |
//                      | Charles Korthout | 2.11  | 13-06-2026     | Empty-URI EQName support in ResolveVariableName (Q{}local)                              |
//                      | Charles Korthout | 2.12  | 13-06-2026     | Parameterized map(K,V) and array(T) matching in ValueMatchesType                         |
//                      | Charles Korthout | 2.13  | 13-06-2026     | Date/time comparison uses implicit timezone; time constructor avoids DateTimeOffset       |
//                      | Charles Korthout | 2.14  | 25-06-2026     | QName equality compares namespace URI + local name, ignoring prefix (fixes type-0129)   |
//                      | Charles Korthout | 2.15  | 25-06-2026     | Value comparison casts xs:untypedAtomic operands to xs:string before comparing (fixes type-0165)            |
//                      | Charles Korthout | 2.16  | 25-06-2026     | LoadContextItem raises XPDY0002 when the XPath context item is absent                      |
//                      | Charles Korthout | 2.17  | 25-06-2026     | InstanceOf applies default element namespace and reports unknown types (XPST0051)        |
//                      | Charles Korthout | 2.18  | 26-06-2026     | InstanceOf recognises parameterised sequence type names and avoids spurious XPST0051   |
//                      | Charles Korthout | 2.19  | 26-06-2026     | CompareGeneral returns false (not empty sequence) for empty general-comparison operands |
//                      | Charles Korthout | 2.20  | 28-06-2026     | NormalizeSequence uses HashSet for duplicate removal; restores catalog self-test speed   |
//                      | Charles Korthout | 2.21  | 26-06-2026     | Integer/decimal division and modulo by zero raise FOAR0001 DynamicException            |
//                      | Charles Korthout | 2.22  | 30-06-2026     | Cast to xs:float parses via float.TryParse to preserve single-precision lexical form  |
//                      | Charles Korthout | 2.23  | 02-07-2026     | Root opcode handles parentless nodes and raises XPDY0050; Range atomizes operands       |
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
        // The lowerer uses monotonic register allocation; size is determined at compile time.
        var registers = new XdmValue[module.MaxRegisterCount];
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

                        bool found = context.TryResolveFunction(nsUri, localName, argCount, out var sig);
                        if (!found)
                            throw new InvalidOperationException(
                                $"XPST0017: Function {{{nsUri}}}{localName}#{argCount} not found.");

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
                    if (context.ContextItem.IsUndefined)
                        throw new InvalidOperationException("XPDY0002: The context item is absent.");
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
                        {
                            string displayName = string.IsNullOrEmpty(nsUri) ? localName : $"Q{{{nsUri}}}{localName}";
                            throw new InvalidOperationException($"XPST0008: Variable ${displayName} is not defined.");
                        }

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
                        bool enforceNodeResult = instr.RegisterC != 0;

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

                        // XPath 2.0/3.0: path expression result must not contain both nodes and non-nodes
                        if (enforceNodeResult)
                        {
                            bool hasNode = results.Any(r => r.IsNode);
                            bool hasNonNode = results.Any(r => !r.IsNode);
                            if (hasNode && hasNonNode)
                                throw new InvalidOperationException("XPTY0018: result of a path expression step contains both nodes and non-nodes");
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.PathStepMap:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                            throw new InvalidOperationException("XPDY0002: An axis step requires a context item.");
                        int rhsEntry = instr.Operand;

                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            // Path-step predicates must see position=1, size=1
                            // for each context item (predicate is relative to the
                            // step result, not the outer sequence).
                            context.WithFocus(items[i], 1, 1);
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

                case IrOpCode.Normalize:
                    {
                        registers[instr.RegisterA] = NormalizeSequence(registers[instr.RegisterB]);
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

                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        foreach (var item in items)
                        {
                            // FLWOR for-expression does NOT change the focus;
                            // it only binds the variable.
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

                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        bool result = false;
                        foreach (var item in items)
                        {
                            // Quantified expression does NOT change the focus;
                            // it only binds the variable.
                            context.WithVariable(info.VariableName, item);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (rhsResult.EffectiveBooleanValue())
                            {
                                result = true;
                                break;
                            }
                        }

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

                        bool hadVariable = context.TryGetVariable(info.VariableName, out var savedVar);

                        bool result = true;
                        foreach (var item in items)
                        {
                            // Quantified expression does NOT change the focus;
                            // it only binds the variable.
                            context.WithVariable(info.VariableName, item);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (!rhsResult.EffectiveBooleanValue())
                            {
                                result = false;
                                break;
                            }
                        }

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
                            var node = input.NodeValue;
                            var root = node.Document;
                            if (root == null)
                            {
                                // Parentless node: the root of its tree is the node itself.
                                root = node;
                            }
                            if (root.NodeKind != XdmNodeKind.Document)
                            {
                                throw new InvalidOperationException("XPDY0050: The root of the tree containing the context item is not a document node.");
                            }
                            registers[instr.RegisterA] = XdmValue.FromNode(root);
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
                        {
                            // Wildcard: match any name (kind test already restricted node kind).
                            if (name == "*")
                                return true;

                            // Namespace wildcard prefix:* — match any local name in the namespace.
                            if (name.EndsWith(":*", StringComparison.Ordinal))
                            {
                                var wildcardPrefix = name[..^2];
                                if (context.TryResolveNamespace(wildcardPrefix, out var wildcardNsUri))
                                    return n.NamespaceUri == wildcardNsUri;
                                return false;
                            }

                            if (n.LocalName != name && !(name.Contains(':') && n.LocalName == name.Split(':')[1]))
                                return false;
                            // Unprefixed attribute names always match no namespace
                            if (n.NodeKind == XdmNodeKind.Attribute && !name.Contains(':'))
                                return n.NamespaceUri == "";
                            return true;
                        });
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

                case IrOpCode.NamespaceTest:
                    {
                        string prefix = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        XdmValue filtered;
                        if (context.TryResolveNamespace(prefix, out var nsUri))
                        {
                            filtered = FilterNodes(input, n => n.NamespaceUri == nsUri);
                        }
                        else if (prefix.Contains('/') || prefix.Contains(':'))
                        {
                            // Operand is a URI (e.g. from Q{uri}local syntax) — use directly
                            filtered = FilterNodes(input, n => n.NamespaceUri == prefix);
                        }
                        else
                        {
                            // Unresolved prefix (including empty prefix for default element namespace):
                            // match empty namespace
                            filtered = FilterNodes(input, n => n.NamespaceUri == "");
                        }
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
                            // XPath 2.0 §3.2.4: true iff the numeric value is equal to context position.
                            // Any numeric type (integer, decimal, float, double) counts.
                            if (IsNumeric(predResult))
                            {
                                if (ToDouble(predResult) == i + 1)
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
                        var cmpResult = Compare(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC], context);
                        registers[instr.RegisterA] = cmpResult;
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
                    {
                        var cmpResult = CompareGeneral(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC], context);
                        registers[instr.RegisterA] = cmpResult;
                        ip++;
                        break;
                    }

                case IrOpCode.IsSameNode:
                    {
                        var left = UnwrapSingleton(registers[instr.RegisterB]);
                        var right = UnwrapSingleton(registers[instr.RegisterC]);
                        // Empty sequence operand -> empty sequence result
                        if (left.IsUndefined || right.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!left.IsNode || !right.IsNode)
                        {
                            throw new InvalidOperationException("XPTY0004: Node comparison operator 'is' requires single node operands.");
                        }
                        else
                        {
                            bool result = left.NodeValue.IsSameNode(right.NodeValue);
                            registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.PrecedesNode:
                case IrOpCode.FollowsNode:
                    {
                        var left = UnwrapSingleton(registers[instr.RegisterB]);
                        var right = UnwrapSingleton(registers[instr.RegisterC]);
                        if (left.IsUndefined || right.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!left.IsNode || !right.IsNode)
                        {
                            throw new InvalidOperationException("XPTY0004: Node comparison operators '<<' and '>>' require single node operands.");
                        }
                        else
                        {
                            bool result = instr.OpCode == IrOpCode.PrecedesNode
                                ? left.NodeValue.DocumentOrder < right.NodeValue.DocumentOrder
                                : left.NodeValue.DocumentOrder > right.NodeValue.DocumentOrder;
                            registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        }
                        ip++;
                        break;
                    }

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
                        bool instance = InstanceOf(registers[instr.RegisterB], typeName, occurrence, context.DefaultElementNamespace);
                        registers[instr.RegisterA] = XdmValue.FromBoolean(instance);
                        ip++;
                        break;
                    }

                case IrOpCode.TreatAs:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        if (!InstanceOf(value, typeName, occurrence, context.DefaultElementNamespace))
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

                        registers[instr.RegisterA] = LookupValue(container, key);
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
                        else if (container.IsSequence && container.SequenceValue is not null)
                        {
                            foreach (var item in XdmSequence.FromSource(container.SequenceValue))
                            {
                                if (item.Kind == XdmValueKind.Map)
                                {
                                    foreach (var v in item.MapValue.Values)
                                        result.Add(v);
                                }
                                else if (item.Kind == XdmValueKind.Array)
                                {
                                    foreach (var v in item.ArrayValue.Values)
                                        result.Add(v);
                                }
                            }
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
                        var funcValue = registers[instr.RegisterB];
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.Operand;
                        XdmValue[] args = new XdmValue[argCount];
                        for (int i = 0; i < argCount; i++)
                            args[i] = registers[firstArgReg + i];
                        registers[instr.RegisterA] = InvokeFunctionItem(funcValue, context, args);
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
        // Braced URI literal: Q{uri}localname or Q{uri}prefix:local
        // The empty URI form Q{}local is permitted and means "no namespace".
        if (varName.Length > 2 && varName[0] == 'Q' && varName[1] == '{')
        {
            int closeBrace = varName.IndexOf('}');
            if (closeBrace >= 2)
            {
                string uri = varName[2..closeBrace];
                string local = varName[(closeBrace + 1)..];
                return (local, uri);
            }
        }

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
            throw new InvalidOperationException("XPDY0002: An axis step requires a context item.");
        if (input.IsAtomic)
            throw new InvalidOperationException("XPTY0020: An axis step requires a context item that is a node.");

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
                    throw new InvalidOperationException($"XPST0017: Function {{{named.NamespaceUri}}}{named.LocalName}#{args.Length} not found.");
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
                                string typeTrimmed = expectedType.TrimEnd();
                                bool allowsMany = typeTrimmed.EndsWith('*') || typeTrimmed.EndsWith('+');
                                bool allowsEmpty = typeTrimmed.EndsWith('?') || typeTrimmed.EndsWith('*');

                                if (arg.IsSequence)
                                {
                                    var items = new List<XdmValue>();
                                    foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
                                        items.Add(item);
                                    if (!allowsMany && items.Count > 1)
                                        throw new InvalidOperationException("XPTY0004");
                                    if (!allowsEmpty && items.Count == 0)
                                        throw new InvalidOperationException("XPTY0004");
                                    foreach (var item in items)
                                    {
                                        if (!ValueMatchesType(item, expectedType))
                                            throw new InvalidOperationException("XPTY0004");
                                    }
                                }
                                else
                                {
                                    if (!ValueMatchesType(arg, expectedType))
                                        throw new InvalidOperationException("XPTY0004");
                                }
                            }
                        }
                    }

                    var saved = new (string LocalName, string NamespaceUri, bool Had, XdmValue Value)[inline.Parameters.Count];
                    for (int i = 0; i < inline.Parameters.Count; i++)
                    {
                        var (localName, nsUri) = ResolveVariableName(inline.Parameters[i], context);
                        saved[i].LocalName = localName;
                        saved[i].NamespaceUri = nsUri;
                        saved[i].Had = context.TryGetVariable(localName, out var oldVal, nsUri);
                        saved[i].Value = oldVal;
                        context.WithVariable(localName, i < args.Length ? args[i] : XdmValue.Undefined, nsUri);
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
                                context.WithVariable(saved[i].LocalName, saved[i].Value, saved[i].NamespaceUri);
                            else
                                context.RemoveVariable(saved[i].LocalName, saved[i].NamespaceUri);
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
            return XdmValue.FromString(value.NodeValue.StringValue, "untypedAtomic");

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
    /// Unwraps a singleton sequence to its single item, or returns the value as-is.
    /// Returns Undefined for an empty sequence.
    /// </summary>
    private static XdmValue UnwrapSingleton(XdmValue value)
    {
        if (value.IsUndefined || !value.IsSequence)
            return value;

        var items = MaterializeSequence(value);
        if (items.Length == 0)
            return XdmValue.Undefined;
        if (items.Length == 1)
            return items[0];
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

        // Separate nodes from non-node items and remove duplicate nodes.
        var nodes = new List<XdmValue>(items.Length);
        var nonNodes = new List<XdmValue>();
        var seen = new HashSet<IXdmNode>();
        bool hasNodes = false;
        foreach (var item in items)
        {
            if (!item.IsNode)
            {
                nonNodes.Add(item);
                continue;
            }

            hasNodes = true;
            var node = item.NodeValue;
            if (seen.Add(node))
                nodes.Add(item);
        }

        if (!hasNodes)
            return XdmValue.FromSequence(MaterializedSequence.FromList(nonNodes));

        if (nodes.Count > 1)
        {
            nodes.Sort((a, b) => a.NodeValue!.DocumentOrder.CompareTo(b.NodeValue!.DocumentOrder));
        }

        if (nonNodes.Count == 0)
            return XdmValue.FromSequence(MaterializedSequence.FromList(nodes));

        var combined = new List<XdmValue>(nodes.Count + nonNodes.Count);
        combined.AddRange(nodes);
        combined.AddRange(nonNodes);
        return XdmValue.FromSequence(MaterializedSequence.FromList(combined));
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

    private static bool IsEmptySequence(XdmValue value)
    {
        if (value.IsUndefined)
            return true;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue))
                return false;
            return true;
        }
        return false;
    }

    private static XdmValue Add(XdmValue left, XdmValue right)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

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

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) + ToDouble(right));

        return MultiplyOrAddInteger(ToInteger(left), ToInteger(right), false);
    }

    private static XdmValue AddDuration(XdmValue dateTimeValue, string duration)
    {
        bool hasTz = dateTimeValue.HasTimezone;
        var xdt = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeXPathValue,
            XdmValueKind.Date => dateTimeValue.DateXPathValue,
            XdmValueKind.Time => dateTimeValue.TimeXPathValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };
        int tzMinutes = xdt.TimezoneOffsetMinutes;
        bool isTime = dateTimeValue.Kind == XdmValueKind.Time;

        XPathDateTime result;
        if (IsYearMonthDurationString(duration))
        {
            var (years, months, _, _, _, _) = ParseDuration(duration);
            if (isTime)
            {
                result = xdt;
            }
            else
            {
                var (ny, nm, nd) = XPathDateTimeHelper.AddMonths(xdt.Year, xdt.Month, xdt.Day, years * 12 + months);
                result = new XPathDateTime(ny, nm, nd, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, tzMinutes, hasTz);
            }
        }
        else if (IsDayTimeDurationString(duration))
        {
            result = AddDayTimeDuration(xdt, duration, isTime, tzMinutes, hasTz);
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(result, hasTz),
            XdmValueKind.Date => XdmValue.FromDate(result, hasTz),
            XdmValueKind.Time => XdmValue.FromTime(result, hasTz),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static XPathDateTime AddDayTimeDuration(XPathDateTime xdt, string duration, bool isTime, int tzMinutes, bool hasTz)
    {
        var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
        long deltaMs = ((days * 24L + hours) * 3600L + minutes * 60L) * 1000L + (long)(seconds * 1000m);
        long msOfDay = (xdt.Hour * 3600L + xdt.Minute * 60L + xdt.Second) * 1000L + xdt.Millisecond;
        if (isTime)
        {
            long totalMs = (msOfDay + deltaMs) % 86400000L;
            if (totalMs < 0) totalMs += 86400000L;
            return MsToTime(totalMs, tzMinutes, hasTz);
        }
        long totalMsFull = msOfDay + deltaMs;
        long dayOffset = totalMsFull / 86400000L;
        long newMsOfDay = totalMsFull % 86400000L;
        if (newMsOfDay < 0) { newMsOfDay += 86400000L; dayOffset--; }
        var (ny, nm, nd) = XPathDateTimeHelper.CivilFromDays(XPathDateTimeHelper.DaysFromCivil(xdt.Year, xdt.Month, xdt.Day) + dayOffset);
        return MsToDateTime(ny, nm, nd, newMsOfDay, tzMinutes, hasTz);
    }

    private static XPathDateTime MsToTime(long totalMs, int tzMinutes, bool hasTz)
    {
        int hour = (int)(totalMs / 3600000L); totalMs %= 3600000L;
        int minute = (int)(totalMs / 60000L); totalMs %= 60000L;
        int second = (int)(totalMs / 1000L);
        int ms = (int)(totalMs % 1000L);
        return new XPathDateTime(1, 1, 1, hour, minute, second, ms, tzMinutes, hasTz);
    }

    private static XPathDateTime MsToDateTime(long year, int month, int day, long totalMs, int tzMinutes, bool hasTz)
    {
        int hour = (int)(totalMs / 3600000L); totalMs %= 3600000L;
        int minute = (int)(totalMs / 60000L); totalMs %= 60000L;
        int second = (int)(totalMs / 1000L);
        int ms = (int)(totalMs % 1000L);
        return new XPathDateTime(year, month, day, hour, minute, second, ms, tzMinutes, hasTz);
    }

    private static XdmValue Subtract(XdmValue left, XdmValue right)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

        if (left.Kind == XdmValueKind.Date && right.Kind == XdmValueKind.Date)
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(left.DateXPathValue, right.DateXPathValue));
        if (left.Kind == XdmValueKind.DateTime && right.Kind == XdmValueKind.DateTime)
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(left.DateTimeXPathValue, right.DateTimeXPathValue));
        if (left.Kind == XdmValueKind.Time && right.Kind == XdmValueKind.Time)
        {
            var leftDt = left.TimeXPathValue;
            var rightDt = right.TimeXPathValue;
            var leftRef = new XPathDateTime(1972, 12, 31, leftDt.Hour, leftDt.Minute, leftDt.Second, leftDt.Millisecond, leftDt.TimezoneOffsetMinutes, left.HasTimezone);
            var rightRef = new XPathDateTime(1972, 12, 31, rightDt.Hour, rightDt.Minute, rightDt.Second, rightDt.Millisecond, rightDt.TimezoneOffsetMinutes, right.HasTimezone);
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(leftRef, rightRef));
        }
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

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) - ToDouble(right));

        return MultiplyOrAddInteger(ToInteger(left), -ToInteger(right), false);
    }

    private static string FormatDurationFromDateTimeDiff(XPathDateTime left, XPathDateTime right)
    {
        var ul = XPathDateTimeHelper.NormalizeToUtc(left);
        var ur = XPathDateTimeHelper.NormalizeToUtc(right);
        decimal msL = (decimal)XPathDateTimeHelper.DaysFromCivil(ul.Year, ul.Month, ul.Day) * 86400000m
            + ((ul.Hour * 3600m + ul.Minute * 60m + ul.Second) * 1000m + ul.Millisecond);
        decimal msR = (decimal)XPathDateTimeHelper.DaysFromCivil(ur.Year, ur.Month, ur.Day) * 86400000m
            + ((ur.Hour * 3600m + ur.Minute * 60m + ur.Second) * 1000m + ur.Millisecond);
        return FormatDurationFromMilliseconds(msL - msR);
    }

    private static XdmValue SubtractDuration(XdmValue dateTimeValue, string duration)
    {
        bool hasTz = dateTimeValue.HasTimezone;
        var xdt = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeXPathValue,
            XdmValueKind.Date => dateTimeValue.DateXPathValue,
            XdmValueKind.Time => dateTimeValue.TimeXPathValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };
        int tzMinutes = xdt.TimezoneOffsetMinutes;
        bool isTime = dateTimeValue.Kind == XdmValueKind.Time;

        XPathDateTime result;
        if (IsYearMonthDurationString(duration))
        {
            var (years, months, _, _, _, _) = ParseDuration(duration);
            if (isTime)
            {
                result = xdt;
            }
            else
            {
                var (ny, nm, nd) = XPathDateTimeHelper.AddMonths(xdt.Year, xdt.Month, xdt.Day, -(years * 12 + months));
                result = new XPathDateTime(ny, nm, nd, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, tzMinutes, hasTz);
            }
        }
        else if (IsDayTimeDurationString(duration))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
            long deltaMs = ((days * 24L + hours) * 3600L + minutes * 60L) * 1000L + (long)(seconds * 1000m);
            result = AddDayTimeDuration(xdt, $"-P{days}DT{hours}H{minutes}M{seconds}S", isTime, tzMinutes, hasTz);
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(result, hasTz),
            XdmValueKind.Date => XdmValue.FromDate(result, hasTz),
            XdmValueKind.Time => XdmValue.FromTime(result, hasTz),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static string FormatDuration(TimeSpan ts) => FormatDurationFromMilliseconds((decimal)ts.TotalMilliseconds);

    private static string FormatDurationFromMilliseconds(decimal totalMs)
    {
        if (totalMs == 0) return "PT0S";
        bool negative = totalMs < 0;
        decimal remaining = negative ? -totalMs : totalMs;
        long days = (long)(remaining / 86400000m);
        remaining -= (decimal)days * 86400000m;
        int hours = (int)(remaining / 3600000m);
        remaining -= hours * 3600000m;
        int minutes = (int)(remaining / 60000m);
        remaining -= minutes * 60000m;
        int seconds = (int)(remaining / 1000m);
        decimal frac = remaining - seconds * 1000m;
        decimal sec = seconds + frac / 1000m;
        return FormatDayTimeDurationParts(negative, days, hours, minutes, sec);
    }

    private static string FormatDayTimeDurationParts(bool negative, long days, int hours, int minutes, decimal seconds)
    {
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (days > 0) sb.Append(days).Append('D');
        if (hours > 0 || minutes > 0 || seconds > 0)
        {
            sb.Append('T');
            if (hours > 0) sb.Append(hours).Append('H');
            if (minutes > 0) sb.Append(minutes).Append('M');
            if (seconds > 0 || (hours == 0 && minutes == 0))
            {
                sb.Append(FormatDecimalTrim(seconds)).Append('S');
            }
        }
        if (sb.Length == (negative ? 2 : 1)) sb.Append("T0S");
        return sb.ToString();
    }

    private static string FormatDecimalTrim(decimal value)
    {
        string s = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (s.Contains('.')) s = s.TrimEnd('0').TrimEnd('.');
        return s;
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
    /// A value without a timezone is treated as having the implicit timezone from the dynamic context.
    /// </summary>
    private static int? CompareDateTimeValues(XdmValue left, XdmValue right, string subtype, EvaluationContext context)
    {
        var leftXdt = AsComparableDateTime(GetXPathDateTime(left, subtype), subtype);
        var rightXdt = AsComparableDateTime(GetXPathDateTime(right, subtype), subtype);

        bool leftHasTz = left.HasTimezone;
        bool rightHasTz = right.HasTimezone;

        // Neither has timezone: compare local components directly
        if (!leftHasTz && !rightHasTz)
        {
            return XPathDateTimeHelper.CompareComponents(leftXdt, rightXdt);
        }

        int implicitTz = GetImplicitTimezoneOffsetMinutes(context);

        var leftEffective = leftHasTz
            ? leftXdt
            : new XPathDateTime(leftXdt.Year, leftXdt.Month, leftXdt.Day,
                leftXdt.Hour, leftXdt.Minute, leftXdt.Second, leftXdt.Millisecond,
                implicitTz, true);
        var rightEffective = rightHasTz
            ? rightXdt
            : new XPathDateTime(rightXdt.Year, rightXdt.Month, rightXdt.Day,
                rightXdt.Hour, rightXdt.Minute, rightXdt.Second, rightXdt.Millisecond,
                implicitTz, true);

        var leftUtc = XPathDateTimeHelper.NormalizeToUtc(leftEffective);
        var rightUtc = XPathDateTimeHelper.NormalizeToUtc(rightEffective);
        return XPathDateTimeHelper.CompareComponents(leftUtc, rightUtc);
    }

    private static int GetImplicitTimezoneOffsetMinutes(EvaluationContext context)
        => context.ImplicitTimezoneOffsetMinutes;

    private static XPathDateTime AsComparableDateTime(XPathDateTime xdt, string subtype)
    {
        if (subtype == "time")
        {
            // xs:time comparisons use the reference date 1972-12-31 (per XPath spec).
            return new XPathDateTime(1972, 12, 31, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, xdt.TimezoneOffsetMinutes, xdt.HasTimezone);
        }
        return xdt;
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
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

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

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) * ToDouble(right));

        return MultiplyOrAddInteger(ToInteger(left), ToInteger(right), true);
    }

    private static XdmValue Divide(XdmValue left, XdmValue right)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

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
        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) / ToDouble(right));

        var divisor = ToDecimal(right);
        if (divisor == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromDecimal(ToDecimal(left) / divisor);
    }

    private static XdmValue IntegerDivide(XdmValue left, XdmValue right)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

        if (IsDouble(left) || IsDouble(right))
        {
            if (ToDouble(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromInteger((long)(ToDouble(left) / ToDouble(right)));
        }
        if (IsFloat(left) || IsFloat(right))
        {
            if (ToFloat(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromInteger((long)(ToFloat(left) / ToFloat(right)));
        }
        if (IsDecimal(left) || IsDecimal(right))
        {
            if (ToDecimal(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromInteger((long)(ToDecimal(left) / ToDecimal(right)));
        }

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
        {
            if (ToDouble(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromInteger((long)(ToDouble(left) / ToDouble(right)));
        }

        if (ToInteger(right) == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromInteger(ToInteger(left) / ToInteger(right));
    }

    private static XdmValue Modulo(XdmValue left, XdmValue right)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
            return XdmValue.Undefined;

        if (IsDouble(left) || IsDouble(right))
        {
            if (ToDouble(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        }
        if (IsFloat(left) || IsFloat(right))
        {
            if (ToFloat(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromFloat(ToFloat(left) % ToFloat(right));
        }
        if (IsDecimal(left) || IsDecimal(right))
        {
            if (ToDecimal(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromDecimal(ToDecimal(left) % ToDecimal(right));
        }

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
        {
            if (ToDouble(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        }

        if (ToInteger(right) == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromInteger(ToInteger(left) % ToInteger(right));
    }

    private static XdmValue Negate(XdmValue value)
    {
        if (IsEmptySequence(value))
            return XdmValue.Undefined;

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

    /// <summary>
    /// Performs integer multiplication or addition with overflow detection.
    /// If the result overflows <see cref="long"/>, promotes to <see cref="decimal"/>.
    /// </summary>
    private static XdmValue MultiplyOrAddInteger(long a, long b, bool multiply)
    {
        try
        {
            checked
            {
                long result = multiply ? a * b : a + b;
                return XdmValue.FromInteger(result);
            }
        }
        catch (OverflowException)
        {
            return XdmValue.FromDecimal(multiply ? (decimal)a * (decimal)b : (decimal)a + (decimal)b);
        }
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    private static XdmValue Compare(IrOpCode op, XdmValue left, XdmValue right, EvaluationContext context, bool strict = true)
    {
        bool leftFromNode = IsNodeOrigin(left);
        bool rightFromNode = IsNodeOrigin(right);

        // Value comparisons (eq/ne/lt/... and their value-comparison opcodes) require
        // each operand to be a singleton after atomization.
        if (strict && (SequenceLength(left) > 1 || SequenceLength(right) > 1))
            throw new InvalidOperationException("XPTY0004: Value comparison requires singleton operands");

        left = Atomize(left);
        right = Atomize(right);

        // XPath value comparisons with empty sequence operand return empty sequence
        if (left.IsUndefined || right.IsUndefined)
            return XdmValue.Undefined;

        // XPath 3.1 §17.2: in a value comparison, an xs:untypedAtomic operand is
        // cast to xs:string before the comparison proceeds, unless the other operand
        // is an xs:QName, in which case the untypedAtomic value is cast to xs:QName.
        if (strict)
        {
            if (IsUntypedAtomic(left) && right.Kind != XdmValueKind.QName)
            {
                left = XdmValue.FromString(left.StringValue);
                leftFromNode = false;
            }
            if (IsUntypedAtomic(right) && left.Kind != XdmValueKind.QName)
            {
                right = XdmValue.FromString(right.StringValue);
                rightFromNode = false;
            }
        }

        return XdmValue.FromBoolean(CompareCore(op, left, right, strict, leftFromNode, rightFromNode, context));
    }

    /// <summary>
    /// Returns the number of items in <paramref name="value"/> if it is a sequence,
    /// or 1 for any other defined value, or 0 for undefined.
    /// </summary>
    private static int SequenceLength(XdmValue value)
    {
        if (value.IsUndefined)
            return 0;
        if (!value.IsSequence)
            return 1;
        return MaterializeSequence(value).Length;
    }

    private static int CompareStrings(string left, string right, string collation, EvaluationContext context)
        => context.CollationComparer?.Invoke(left, right, collation)
           ?? string.CompareOrdinal(left, right);

    private static bool CompareCore(IrOpCode op, XdmValue left, XdmValue right, bool strict, bool leftFromNode, bool rightFromNode, EvaluationContext context)
    {
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

        // QName comparison: prefix is ignored; only namespace URI and local name matter.
        // Ordering comparisons are not defined for QNames.
        if (left.Kind == XdmValueKind.QName && right.Kind == XdmValueKind.QName)
        {
            if (op is IrOpCode.LessThan or IrOpCode.ValueLessThan
                or IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual
                or IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan
                or IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual)
            {
                throw new InvalidOperationException("XPTY0004: Ordering comparison is not defined for xs:QName values.");
            }
            bool eq = left.QNameValue.Equals(right.QNameValue);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => eq,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => !eq,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // General comparison promotion: an xs:untypedAtomic operand is cast to the
        // type of the other operand. When that type is xs:QName, resolve the lexical
        // QName using the static namespace context of the expression.
        if (left.Kind == XdmValueKind.QName && IsUntypedAtomic(right))
        {
            return CompareCore(op, left, CastUntypedAtomicToQName(right, context), strict, leftFromNode, false, context);
        }
        if (IsUntypedAtomic(left) && right.Kind == XdmValueKind.QName)
        {
            return CompareCore(op, CastUntypedAtomicToQName(left, context), right, strict, false, rightFromNode, context);
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
            {
                // XPath general comparison promotion: an atomized untypedAtomic value
                // (typically from a node) is cast to the date/time subtype of the other
                // operand when the types would otherwise be incompatible.
                if (leftDateSub is not null && right.Kind == XdmValueKind.String && right.SchemaTypeName == "untypedAtomic" && TryCast(right, leftDateSub, out var castedRight))
                {
                    right = castedRight;
                    rightDateSub = leftDateSub;
                }
                else if (rightDateSub is not null && left.Kind == XdmValueKind.String && left.SchemaTypeName == "untypedAtomic" && TryCast(left, rightDateSub, out var castedLeft))
                {
                    left = castedLeft;
                    leftDateSub = rightDateSub;
                }
                else
                {
                    throw new InvalidOperationException("XPTY0004");
                }
            }
            // gYear/gYearMonth/gMonth/gMonthDay/gDay only support equality; use lexical comparison.
            if (leftDateSub is "gYear" or "gYearMonth" or "gMonth" or "gMonthDay" or "gDay")
            {
                if (op is IrOpCode.LessThan or IrOpCode.ValueLessThan or IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan
                    or IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual or IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual)
                    throw new InvalidOperationException("XPTY0004");
                int lexCmp = string.CompareOrdinal(left.ToString(), right.ToString());
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => lexCmp == 0,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => lexCmp != 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            var cmp = CompareDateTimeValues(left, right, leftDateSub, context);
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
                ? CompareStrings(left.ToString(), right.ToString(), context.DefaultCollation, context) == (op is IrOpCode.Equal or IrOpCode.ValueEqual ? 0 : 1)
                : false;
        }

        // Atomized nodes become strings; try numeric parsing for untyped values
        string lStr = left.ToString();
        string rStr = right.ToString();

        // If both are explicitly strings, compare as strings (don't parse as numbers)
        if (left.Kind == XdmValueKind.String && right.Kind == XdmValueKind.String)
        {
            int cmp = CompareStrings(lStr, rStr, context.DefaultCollation, context);
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

        // In strict mode, only same-kind atomic values are comparable (cross-kind
        // mismatches such as boolean vs numeric should have been handled above).
        if (strict && left.Kind != right.Kind)
            throw new InvalidOperationException("XPTY0004");

        int cmp2 = CompareStrings(lStr, rStr, context.DefaultCollation, context);
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

    private static XdmValue CompareGeneral(IrOpCode op, XdmValue left, XdmValue right, EvaluationContext context)
    {
        // General comparisons use existential semantics over sequences.
        // XPath 3.1 §17.3: if one operand is an empty sequence, the result is false.
        if (left.IsUndefined || right.IsUndefined)
            return XdmValue.FromBoolean(false);

        // For now, materialize both sides and compare pairwise.
        var leftItems = MaterializeSequence(left);
        var rightItems = MaterializeSequence(right);

        foreach (var l in leftItems)
        {
            foreach (var r in rightItems)
            {
                // Atomize and check for empty sequence on each pair
                var atomizedL = Atomize(l);
                var atomizedR = Atomize(r);
                if (atomizedL.IsUndefined || atomizedR.IsUndefined)
                    continue;

                // XPath 1.0 backwards compatibility coercion rules
                if (context.BackwardsCompatible)
                {
                    ApplyBackwardsCompatibleCoercion(ref atomizedL, ref atomizedR);
                }

                bool match = CompareCore(
                    op switch
                    {
                        IrOpCode.GeneralEqual => IrOpCode.Equal,
                        IrOpCode.GeneralNotEqual => IrOpCode.NotEqual,
                        IrOpCode.GeneralLessThan => IrOpCode.LessThan,
                        IrOpCode.GeneralLessThanOrEqual => IrOpCode.LessThanOrEqual,
                        IrOpCode.GeneralGreaterThan => IrOpCode.GreaterThan,
                        IrOpCode.GeneralGreaterThanOrEqual => IrOpCode.GreaterThanOrEqual,
                        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                    },
                    atomizedL, atomizedR, strict: !context.BackwardsCompatible,
                    IsNodeOrigin(l), IsNodeOrigin(r), context);

                if (match)
                    return XdmValue.FromBoolean(true);
            }
        }

        return XdmValue.FromBoolean(false);
    }

    /// <summary>
    /// Applies XPath 1.0 backwards-compatible coercion rules for general comparisons:
    /// 1. If either operand is boolean, convert the other to boolean.
    /// 2. If either operand is numeric, convert the other to numeric.
    /// 3. Otherwise, convert both to strings.
    /// </summary>
    private static void ApplyBackwardsCompatibleCoercion(ref XdmValue left, ref XdmValue right)
    {
        bool leftIsBool = left.Kind == XdmValueKind.Boolean;
        bool rightIsBool = right.Kind == XdmValueKind.Boolean;
        if (leftIsBool || rightIsBool)
        {
            if (!leftIsBool) left = XdmValue.FromBoolean(left.EffectiveBooleanValue());
            if (!rightIsBool) right = XdmValue.FromBoolean(right.EffectiveBooleanValue());
            return;
        }

        bool leftIsNum = IsNumeric(left);
        bool rightIsNum = IsNumeric(right);
        if (leftIsNum || rightIsNum)
        {
            if (!leftIsNum) left = XdmValue.FromDouble(ToDoubleOrNaN(left));
            if (!rightIsNum) right = XdmValue.FromDouble(ToDoubleOrNaN(right));
            return;
        }

        // Otherwise both become strings (they already are after atomization)
    }

    /// <summary>
    /// Converts a value to double, returning NaN for unparseable strings
    /// (XPath 1.0 semantics) instead of throwing.
    /// </summary>
    private static double ToDoubleOrNaN(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN
        };
    }

    private static bool IsNumeric(XdmValue value)
        => IsDouble(value) || IsFloat(value) || IsDecimal(value) || value.Kind == XdmValueKind.Integer;

    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Casts an xs:untypedAtomic value to xs:QName, resolving any prefix against
    /// the static namespace context and using the default element namespace for
    /// unprefixed lexical QNames.
    /// </summary>
    private static XdmValue CastUntypedAtomicToQName(XdmValue value, EvaluationContext context)
    {
        string lexical = value.StringValue.Trim();
        if (string.IsNullOrEmpty(lexical))
            throw new InvalidOperationException("XPTY0004: Cannot cast empty xs:untypedAtomic value to xs:QName.");

        int colon = lexical.IndexOf(':');
        string prefix = colon >= 0 ? lexical[..colon] : string.Empty;
        string local = colon >= 0 ? lexical[(colon + 1)..] : lexical;

        if (!IsValidNcName(prefix) || !IsValidNcName(local))
            throw new InvalidOperationException("XPTY0004: Invalid lexical QName for cast to xs:QName.");

        if (!context.TryResolveNamespace(prefix, out string? namespaceUri))
            throw new InvalidOperationException($"XPTY0004: Namespace prefix '{prefix}' is not declared for xs:QName cast.");

        return XdmValue.FromQName(new XsQName(local, namespaceUri, prefix));
    }

    private static bool IsValidNcName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return true; // empty prefix is allowed
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
                return false;
        }
        return true;
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
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "").Replace("xsd:", "");
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        // Empty sequence casts to empty sequence for all types
        if (value.IsUndefined)
        {
            result = XdmValue.Undefined;
            return true;
        }

        // If value is a sequence, only allow single-item sequences for atomic casts
        if (value.IsSequence)
        {
            if (!TryGetSequenceLength(value.SequenceValue, out var seqLen))
                return false;
            if (seqLen == 0)
            {
                result = XdmValue.Undefined;
                return true;
            }
            if (seqLen != 1)
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
                if (TryParseFloat(value.ToString(), out var flt))
                {
                    result = XdmValue.FromFloat(flt);
                    return true;
                }
                return false;

            case "boolean":
                if (value.Kind == XdmValueKind.Boolean)
                    return true;
                if (value.Kind == XdmValueKind.String)
                {
                    // xs:boolean lexical values are case-sensitive: true, false, 0, 1.
                    var s = CollapseWhitespace(value.StringValue);
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
                        result = XdmValue.FromTime(xdtT, hasTzT);
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

    private static bool InstanceOf(XdmValue value, string typeName, OccurrenceIndicator occurrence, string? defaultElementNamespace)
    {
        string normalized = NormalizeTypeName(typeName);

        if (normalized is "empty-sequence" or "empty-sequence()")
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

        string effective;
        if (typeName.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
        {
            effective = normalized.StartsWith("xs:") ? normalized[3..] : normalized;
        }
        else if (typeName.StartsWith("xsd:", StringComparison.OrdinalIgnoreCase))
        {
            effective = normalized.StartsWith("xsd:") ? normalized[4..] : normalized;
        }
        else if (typeName.Contains(':') && !typeName.Contains('('))
        {
            // A prefixed name that is not a node-kind test and not in the XML Schema
            // namespace is not a valid sequence type name.
            throw new InvalidOperationException("XPST0051");
        }
        else
        {
            if (defaultElementNamespace == "http://www.w3.org/2001/XMLSchema")
            {
                effective = normalized;
            }
            else
            {
                // No prefix and the default namespace is not XML Schema: only node kind
                // and function/map/array/item tests are valid; bare atomic type names are not.
                if (IsKnownSequenceTypeName(normalized))
                    effective = normalized;
                else
                    throw new InvalidOperationException("XPST0051");
            }
        }

        // Function, map, array and item tests are valid regardless of the default namespace.
        if (effective is "function" or "function(*)" or "function()")
        {
            if (!value.IsSequence) return value.IsFunction;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsFunction) return false;
            return true;
        }

        if (effective is "map" or "map(*)" or "map()")
        {
            if (!value.IsSequence) return value.IsMap;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsMap) return false;
            return true;
        }

        if (effective is "array" or "array(*)" or "array()")
        {
            if (!value.IsSequence) return value.IsArray;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsArray) return false;
            return true;
        }

        if (effective is "item" or "item()")
        {
            if (!value.IsSequence) return !value.IsUndefined;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (item.IsUndefined) return false;
            return true;
        }

        // Node kind tests (use the original typeName so that parameterised forms
        // such as element(*, xs:anyType) are evaluated by ValueMatchesType).
        if (IsKnownSequenceTypeName(effective))
        {
            if (!value.IsSequence) return ValueMatchesType(value, typeName);
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                if (!ValueMatchesType(item, typeName))
                    return false;
            }
            return true;
        }

        // Atomic type names: must be in the XML Schema namespace (either via xs: prefix
        // or via the default element/type namespace).
        if (!IsKnownAtomicTypeName(effective))
            throw new InvalidOperationException("XPST0051");

        if (!value.IsSequence)
            return ValueMatchesType(value, effective);

        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
        {
            if (!ValueMatchesType(item, effective))
                return false;
        }
        return true;
    }

    private static string NormalizeTypeName(string typeName)
    {
        var s = typeName.Trim().ToLowerInvariant();
        if (s.Length > 0 && (s[^1] is '?' or '*' or '+'))
            s = s[..^1].TrimEnd();
        // Strip any parenthesised parameters so that forms such as
        // element(*, xs:anyType), attribute(*, T), and item() all reduce
        // to their base kind name for the initial classification.
        int paren = s.IndexOf('(');
        if (paren >= 0)
            s = s[..paren].TrimEnd();
        return s;
    }

    private static string ResolveTypeName(string original, string normalized, string? defaultElementNamespace)
    {
        if (original.Contains(':'))
        {
            if (original.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
                return normalized.StartsWith("xs:") ? normalized[3..] : normalized;
            if (original.StartsWith("xsd:", StringComparison.OrdinalIgnoreCase))
                return normalized.StartsWith("xsd:") ? normalized[4..] : normalized;
            throw new InvalidOperationException("XPST0051");
        }

        if (defaultElementNamespace == "http://www.w3.org/2001/XMLSchema")
            return normalized;

        return normalized;
    }

    private static bool IsKnownSequenceTypeName(string name)
        => name is "node" or "node()" or "element" or "element()" or "attribute" or "attribute()"
            or "document-node" or "document-node()" or "text" or "text()" or "comment" or "comment()"
            or "processing-instruction" or "processing-instruction()" or "namespace-node" or "namespace-node()"
            or "item" or "item()"
            or "function" or "function(*)" or "function()"
            or "map" or "map(*)" or "map()"
            or "array" or "array(*)" or "array()";

    private static bool IsKnownAtomicTypeName(string name)
        => name is "string" or "normalizedstring" or "token" or "language" or "nmtoken" or "name"
            or "ncname" or "id" or "idref" or "entity" or "boolean" or "integer" or "int" or "long"
            or "short" or "byte" or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
            or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger"
            or "decimal" or "double" or "float" or "numeric" or "datetime" or "date" or "time"
            or "duration" or "daytimeduration" or "yearmonthduration" or "qname" or "anyuri"
            or "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth"
            or "hexbinary" or "base64binary" or "untypedatomic" or "anyatomictype";

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
            "numeric" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float,
            "anyatomictype" => value.Kind is >= XdmValueKind.String and <= XdmValueKind.Binary,
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
            "element" or "element()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Element,
            "attribute" or "attribute()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Attribute,
            "document-node" or "document-node()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Document,
            "text" or "text()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Text,
            "comment" or "comment()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Comment,
            "processing-instruction" or "processing-instruction()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction,
            "namespace-node" or "namespace-node()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Namespace,
            "item" => !value.IsUndefined,
            _ => false
        };
    }

    private static bool IsStringSubtype(string? schemaTypeName)
    {
        if (schemaTypeName is null) return true;
        return schemaTypeName.ToLowerInvariant() is
            "normalizedstring" or "token" or "language" or "nmtoken" or "name"
            or "ncname" or "id" or "idref" or "entity";
    }

    private static bool IsElementTypeCompatible(string typeName)
    {
        typeName = typeName.ToLowerInvariant().Replace("xs:", "").Replace("*", "").Trim();
        if (typeName.EndsWith('?'))
            typeName = typeName[..^1].Trim();
        // In non-schema-aware processing, elements have type xs:anyType / xs:untyped.
        // xs:anyAtomicType and xs:untypedAtomic are atomic types, not element types.
        return typeName is "anytype" or "untyped";
    }

    private static bool IsAttributeTypeCompatible(string typeName)
    {
        typeName = typeName.ToLowerInvariant().Replace("xs:", "").Replace("*", "").Trim();
        if (typeName.EndsWith('?'))
            typeName = typeName[..^1].Trim();
        // In non-schema-aware processing, attributes have type xs:untypedAtomic.
        // xs:untypedAtomic is derived from xs:anyAtomicType, so both match.
        // xs:anyType and xs:untyped are element types, not attribute types.
        return typeName is "untypedatomic" or "anyatomictype";
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
    public static bool ValueMatchesType(XdmValue value, string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return true;

        // empty-sequence() only matches the empty sequence.
        if (typeName.Trim().Equals("empty-sequence()", StringComparison.OrdinalIgnoreCase))
            return value.IsUndefined;

        // Sequence values (including the empty sequence) must be checked against the
        // occurrence indicator of the sequence type. Each item is matched against the
        // base type recursively so that node tests, function types, and atomic types
        // are handled uniformly.
        if (value.IsUndefined || (value.IsSequence && value.SequenceValue != null))
        {
            var trimmed = typeName.Trim();
            char occ = '\0';
            if (trimmed.Length > 0 && "?+*".Contains(trimmed[^1]))
            {
                occ = trimmed[^1];
                trimmed = trimmed[..^1].TrimEnd();
            }

            var items = new List<XdmValue>();
            if (!value.IsUndefined && value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                    items.Add(item);
            }

            switch (occ)
            {
                case '?':
                    if (items.Count > 1) return false;
                    break;
                case '*':
                    break;
                case '+':
                    if (items.Count == 0) return false;
                    break;
                default:
                    if (items.Count != 1) return false;
                    break;
            }

            foreach (var item in items)
            {
                if (!ValueMatchesType(item, trimmed))
                    return false;
            }
            return true;
        }

        string normalized = typeName.Trim().ToLowerInvariant();

        // Strip occurrence indicator for non-sequence values.
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        // Strip xs:/xsd: prefix
        if (normalized.StartsWith("xs:"))
            normalized = normalized[3..];
        else if (normalized.StartsWith("xsd:"))
            normalized = normalized[4..];

        if (normalized == "item()")
            return !value.IsUndefined;

        if (normalized == "node()")
            return value.IsNode;

        if (normalized.StartsWith("element(") && normalized.EndsWith(')'))
        {
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Element)
                return false;
            var inner = normalized.Substring(8, normalized.Length - 9).Trim();
            // element() or element(*) → any element
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            // element(*, T) → check type compatibility
            if (inner.StartsWith("*, "))
            {
                var typePart = inner.Substring(3).Trim();
                return IsElementTypeCompatible(typePart);
            }
            // element(name) or element(name, T) → check name match (basic, no namespace).
            // Use the case-preserved type string so local names such as 'A' are not lowercased.
            var casePreserved = GetCasePreservedTypeName(typeName);
            var cpInner = casePreserved.Substring(8, casePreserved.Length - 9).Trim();
            var namePart = cpInner.Split(',')[0].Trim();
            if (namePart != "*")
            {
                var testLocalName = namePart.Contains(':') ? namePart[(namePart.IndexOf(':') + 1)..] : namePart;
                if (value.NodeValue.LocalName != testLocalName)
                    return false;
            }
            if (inner.Contains(','))
            {
                var typePart = inner.Substring(inner.IndexOf(',') + 1).Trim();
                return IsElementTypeCompatible(typePart);
            }
            return true;
        }

        if (normalized.StartsWith("attribute(") && normalized.EndsWith(')'))
        {
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Attribute)
                return false;
            var inner = normalized.Substring(10, normalized.Length - 11).Trim();
            // attribute() or attribute(*) → any attribute
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            // attribute(*, T) → check type compatibility
            if (inner.StartsWith("*, "))
            {
                var typePart = inner.Substring(3).Trim();
                return IsAttributeTypeCompatible(typePart);
            }
            // attribute(name) or attribute(name, T) → check name match.
            // Use the case-preserved type string so local names keep their original case.
            var casePreserved = GetCasePreservedTypeName(typeName);
            var cpInner = casePreserved.Substring(10, casePreserved.Length - 11).Trim();
            var namePart = cpInner.Split(',')[0].Trim();
            if (namePart != "*")
            {
                var testLocalName = namePart.Contains(':') ? namePart[(namePart.IndexOf(':') + 1)..] : namePart;
                if (value.NodeValue.LocalName != testLocalName)
                    return false;
            }
            if (inner.Contains(','))
            {
                var typePart = inner.Substring(inner.IndexOf(',') + 1).Trim();
                return IsAttributeTypeCompatible(typePart);
            }
            return true;
        }

        // document-node(element(...)) — check document node and its single child element
        if (normalized.StartsWith("document-node(element(") && normalized.EndsWith(')'))
        {
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Document)
                return false;
            var childElems = new List<XdmValue>();
            foreach (var c in value.NodeValue.Axis(XdmAxis.Child))
            {
                if (c.NodeValue?.NodeKind == XdmNodeKind.Element)
                    childElems.Add(c);
            }
            if (childElems.Count != 1)
                return false;
            var inner = normalized.Substring("document-node(".Length, normalized.Length - "document-node(".Length - 1);
            return ValueMatchesType(XdmValue.FromNode(childElems[0].NodeValue!), inner);
        }

        if (normalized is "document-node()" or "document-node")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Document;

        if (normalized is "text()" or "text")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Text;

        if (normalized is "comment()" or "comment")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Comment;

        if (normalized is "processing-instruction()" or "processing-instruction")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction;

        if (normalized is "namespace-node()" or "namespace-node")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Namespace;

        // Handle typed function signatures before general normalization, because function
        // type strings contain nested type names whose occurrence indicators and xs: prefixes
        // must not be stripped by the general normalization logic.
        string trimmedLower = typeName.Trim().ToLowerInvariant();
        if (trimmedLower.StartsWith("function(") && !trimmedLower.StartsWith("function(*)"))
        {
            if (TryParseFunctionType(typeName.Trim(), out var testParamTypes, out var testReturnType))
            {
                // function(*) wildcard falls through to the check below
                bool isFunctionStar = testParamTypes.Length == 1 && testParamTypes[0] == "*";
                if (!isFunctionStar)
                {
                    if (!value.IsFunction) return false;
                    if (TryGetInlineFunctionSignature(value, out var actualParamTypes, out var actualReturnType))
                    {
                        if (actualParamTypes.Length != testParamTypes.Length) return false;
                        // Parameter types are contravariant: test param must be subtype of actual param
                        for (int i = 0; i < testParamTypes.Length; i++)
                        {
                            if (!IsSequenceTypeSubtype(testParamTypes[i], actualParamTypes[i]))
                                return false;
                        }
                        // Return type is covariant: actual return must be subtype of test return
                        if (!IsSequenceTypeSubtype(actualReturnType, testReturnType))
                            return false;
                        return true;
                    }
                    return false;
                }
            }
        }

        if (normalized is "function(*)" or "function")
            return value.IsFunction;

        // Parameterized map types: map(K, V). Empty maps match any key/value types;
        // otherwise every entry must match the declared key and value types.
        if (normalized.StartsWith("map(") && normalized.EndsWith(')'))
        {
            if (!value.IsMap) return false;
            var inner = normalized.Substring(4, normalized.Length - 5).Trim();
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            var parts = SplitTopLevel(inner, ',');
            if (parts.Length != 2)
                return true; // malformed, be permissive
            string keyType = parts[0].Trim();
            string valueType = parts[1].Trim();
            foreach (var entry in value.MapValue.Entries)
            {
                if (!ValueMatchesType(entry.Key, keyType)) return false;
                if (!ValueMatchesType(entry.Value, valueType)) return false;
            }
            return true;
        }

        // Parameterized array types: array(T). Empty arrays match any member type;
        // otherwise every member must match the declared type.
        if (normalized.StartsWith("array(") && normalized.EndsWith(')'))
        {
            if (!value.IsArray) return false;
            var inner = normalized.Substring(6, normalized.Length - 7).Trim();
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            foreach (var member in value.ArrayValue.Values)
            {
                if (!ValueMatchesType(member, inner)) return false;
            }
            return true;
        }

        if (normalized is "map(*)" or "map")
            return value.IsMap;

        if (normalized is "array(*)" or "array")
            return value.IsArray;

        return ItemInstanceOf(value, normalized);
    }

    /// <summary>
    /// Strips occurrence indicators and the <c>xs:/xsd:</c> prefix from a type name
    /// while preserving the original case of the remaining text (needed for element
    /// and attribute local-name matching).
    /// </summary>
    private static string GetCasePreservedTypeName(string typeName)
    {
        var s = typeName.Trim();
        if (s.EndsWith('?') || s.EndsWith('*') || s.EndsWith('+'))
            s = s[..^1].TrimEnd();
        if (s.StartsWith("xs:"))
            s = s[3..];
        else if (s.StartsWith("xsd:"))
            s = s[4..];
        return s;
    }

    /// <summary>
    /// Parses a function type string such as <c>function(item()*, xs:double) as xs:double</c>.
    /// </summary>
    private static bool TryParseFunctionType(string typeName, out string[] paramTypes, out string returnType)
    {
        paramTypes = [];
        returnType = "item()*";

        string s = typeName.Trim();
        if (!s.StartsWith("function(", StringComparison.OrdinalIgnoreCase))
            return false;

        int openIdx = s.IndexOf('(');
        int closeIdx = FindMatchingParen(s, openIdx);
        if (closeIdx < 0) return false;

        string paramList = s.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
        if (string.IsNullOrEmpty(paramList))
        {
            paramTypes = [];
        }
        else if (paramList == "*")
        {
            paramTypes = ["*"];
        }
        else
        {
            paramTypes = SplitTopLevel(paramList, ',');
        }

        string after = s.Substring(closeIdx + 1).Trim();
        if (after.StartsWith("as ", StringComparison.OrdinalIgnoreCase))
        {
            returnType = after.Substring(3).Trim();
        }
        else
        {
            returnType = "item()*";
        }

        return true;
    }

    /// <summary>
    /// Finds the index of the closing parenthesis that matches the opening parenthesis at <paramref name="openIdx"/>.
    /// </summary>
    private static int FindMatchingParen(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Splits a string by a delimiter, respecting nested parentheses.
    /// </summary>
    private static string[] SplitTopLevel(string s, char delimiter)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')') depth--;
            else if (s[i] == delimiter && depth == 0)
            {
                parts.Add(s.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        parts.Add(s.Substring(start).Trim());
        return parts.ToArray();
    }

    /// <summary>
    /// Extracts the declared parameter and return types from an inline function item.
    /// </summary>
    private static bool TryGetInlineFunctionSignature(XdmValue value, out string[] paramTypes, out string returnType)
    {
        paramTypes = [];
        returnType = "item()*";

        if (!value.IsFunction) return false;

        var func = value.FunctionValue as FunctionItem;
        if (func is InlineFunctionItem inline)
        {
            paramTypes = inline.ParameterTypes.Select(pt => pt ?? "item()*").ToArray();
            returnType = inline.ReturnType ?? "item()*";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="actualType"/> is a subtype of <paramref name="testType"/>
    /// according to XPath 3.1 sequence type subtyping rules.
    /// </summary>
    private static bool IsSequenceTypeSubtype(string actualType, string testType)
    {
        string actual = actualType.Trim().ToLowerInvariant();
        string test = testType.Trim().ToLowerInvariant();

        if (actual.StartsWith("xs:")) actual = actual[3..];
        if (test.StartsWith("xs:")) test = test[3..];

        // Extract occurrence indicators
        char actualOcc = '\0';
        char testOcc = '\0';
        if (actual.Length > 0 && "?+*".Contains(actual[^1]))
        {
            actualOcc = actual[^1];
            actual = actual[..^1].TrimEnd();
        }
        if (test.Length > 0 && "?+*".Contains(test[^1]))
        {
            testOcc = test[^1];
            test = test[..^1].TrimEnd();
        }

        // Every sequence matching actual must also match test
        bool occOk = (actualOcc, testOcc) switch
        {
            ('\0', '\0') => true,
            ('\0', '?') => true,
            ('\0', '+') => true,
            ('\0', '*') => true,
            ('?', '?') => true,
            ('?', '*') => true,
            ('+', '+') => true,
            ('+', '*') => true,
            ('*', '*') => true,
            _ => false,
        };

        if (!occOk) return false;
        if (actual == test) return true;

        return IsBaseTypeSubtype(actual, test);
    }

    /// <summary>
    /// Checks whether <paramref name="actual"/> is a subtype of <paramref name="test"/>
    /// by walking the type hierarchy.
    /// </summary>
    private static bool IsBaseTypeSubtype(string actual, string test)
    {
        var queue = new Queue<string>();
        queue.Enqueue(actual);
        var visited = new HashSet<string>();

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            if (current == test) return true;
            if (!visited.Add(current)) continue;

            foreach (var super in GetDirectSupertypes(current))
                queue.Enqueue(super);
        }

        return false;
    }

    /// <summary>
    /// Returns the immediate supertypes of a given base type name.
    /// </summary>
    private static IEnumerable<string> GetDirectSupertypes(string type)
    {
        return type switch
        {
            "integer" => ["decimal"],
            "decimal" => ["numeric"],
            "double" => ["numeric"],
            "float" => ["numeric"],
            "numeric" => ["anyatomictype"],
            "string" => ["anyatomictype"],
            "boolean" => ["anyatomictype"],
            "date" => ["anyatomictype"],
            "time" => ["anyatomictype"],
            "datetime" => ["anyatomictype"],
            "duration" => ["anyatomictype"],
            "daytimeduration" => ["duration"],
            "yearmonthduration" => ["duration"],
            "anyuri" => ["anyatomictype"],
            "qname" => ["anyatomictype"],
            "notation" => ["anyatomictype"],
            "hexbinary" => ["anyatomictype"],
            "base64binary" => ["anyatomictype"],
            "anyatomictype" => ["item()"],
            "element()" => ["node()"],
            "attribute()" => ["node()"],
            "text()" => ["node()"],
            "comment()" => ["node()"],
            "processing-instruction()" => ["node()"],
            "document-node()" => ["node()"],
            "namespace-node()" => ["node()"],
            "node()" => ["item()"],
            "function(*)" => ["item()"],
            "map(*)" => ["item()"],
            "array(*)" => ["item()"],
            _ => [],
        };
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
        if (year > int.MaxValue || year < int.MinValue) return false;
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
        if (hour == 24 && (minute != 0 || second != 0 || millisecond != 0)) return false;

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

        xdt = NormalizeHour24(new XPathDateTime(year, month, day, hour, minute, second, millisecond, tzMinutes, hasTz));
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
        if (year > int.MaxValue || year < int.MinValue) return false;
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
        if (hour == 24 && (minute != 0 || second != 0 || millisecond != 0)) return false;

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

        // xs:time normalizes 24:00:00 to 00:00:00 on the same (reference) day.
        if (hour == 24)
            hour = 0;
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
        if (year < 0) year = -year; // BCE leap years align with the negated year number
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
            // Allow T24:00:00 followed by all-zero fractional seconds.
            if (after >= s.Length || s[after] != '.')
            {
                // no fractional seconds - normalize directly
            }
            else
            {
                int i = after + 1;
                while (i < s.Length && char.IsDigit(s[i])) i++;
                if (!s[(after + 1)..i].All(c => c == '0'))
                    return s; // leave non-zero fractional T24 for the parser to reject
                after = i;
            }
            string datePart = s[..idx];
            string rest = s[after..];
            if (DateTimeOffset.TryParse(datePart, out var dto))
            {
                dto = dto.AddDays(1);
                string newDate = dto.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                return newDate + "T00:00:00" + rest;
            }
        }
        return s;
    }

    private static XPathDateTime NormalizeHour24(XPathDateTime xdt)
    {
        if (xdt.Hour != 24)
            return xdt;
        var (year, month, day) = XPathDateTimeHelper.AddDays(xdt.Year, xdt.Month, xdt.Day, 1);
        return new XPathDateTime(year, month, day, 0, 0, 0, 0, xdt.TimezoneOffsetMinutes, xdt.HasTimezone);
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

    private static bool TryParseFloat(string s, out float result)
    {
        s = s.Trim();
        if (s == "INF" || s == "+INF")
        {
            result = float.PositiveInfinity;
            return true;
        }
        if (s == "-INF")
        {
            result = float.NegativeInfinity;
            return true;
        }
        if (s == "NaN")
        {
            result = float.NaN;
            return true;
        }
        // Explicitly reject case variants that .NET's float.TryParse would accept
        string upper = s.ToUpperInvariant();
        if (upper is "NAN" or "INF" or "+INF" or "-INF" or "INFINITY" or "+INFINITY" or "-INFINITY")
        {
            result = 0;
            return false;
        }
        return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
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

    private static double ToDouble(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to double")
        };
    }

    private static decimal ToDecimal(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
            _ => decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to decimal")
        };
    }

    private static long ToInteger(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (long)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (long)value.DoubleValue,
            _ => long.TryParse(value.ToString(), out var l) ? l : throw new InvalidOperationException($"Cannot convert {value.Kind} to integer")
        };
    }

    private static float ToFloat(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (float)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (float)value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0f : 0.0f,
            _ => float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : throw new InvalidOperationException($"Cannot convert {value.Kind} to float")
        };
    }

    // ------------------------------------------------------------------
    // Opcode helpers
    // ------------------------------------------------------------------

    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");
        return Atomize(value);
    }

    private static XdmValue LookupValue(XdmValue container, XdmValue key)
    {
        if (container.Kind == XdmValueKind.Map)
        {
            var vkey = AtomizeMapKey(key);
            if (container.MapValue.TryGetValue(vkey, out var value))
                return value;
            return XdmValue.Undefined;
        }
        if (container.Kind == XdmValueKind.Array)
        {
            int idx = (int)ToInteger(key);
            return container.ArrayValue.Get(idx);
        }
        if (container.IsSequence && container.SequenceValue is not null)
        {
            var results = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(container.SequenceValue))
            {
                var sub = LookupValue(item, key);
                if (!sub.IsUndefined)
                {
                    if (sub.IsSequence && sub.SequenceValue is not null)
                    {
                        foreach (var s in XdmSequence.FromSource(sub.SequenceValue))
                            results.Add(s);
                    }
                    else
                    {
                        results.Add(sub);
                    }
                }
            }
            if (results.Count == 0)
                return XdmValue.Undefined;
            return XdmValue.FromSequence(MaterializedSequence.FromList(results));
        }
        return XdmValue.Undefined;
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
