// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Observes matching-conversion allocations and qualified VM closure construction
// SPECIAL NOTES        : Diagnostic only; invokes qualified unchanged VM helpers and catches all failures.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 14-11-2026     | Isolate matching-conversion allocations and inspect constructor IL                        |
// ===========================================================================================================================================================
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Providers.Xml;

internal static class Probe
{
    private const int Iterations = 100000;
    private const int Warmup = 10000;
    private static object? _sink;
    private static XdmValue _valueSink;
    private static readonly List<object> Rows = new();
    private static readonly List<object> Checks = new();
    private static readonly Func<XdmValue, XdmValue[]> ArrayHelper = Bind<Func<XdmValue, XdmValue[]>>("MaterializeSequence");
    private static readonly Func<XdmValue, IReadOnlyList<XdmValue>> ViewHelper = Bind<Func<XdmValue, IReadOnlyList<XdmValue>>>("MaterializeSequenceView");
    private static readonly Func<IrOpCode, XdmValue, XdmValue, EvaluationContext, bool, XdmValue> Compare = Bind<Func<IrOpCode, XdmValue, XdmValue, EvaluationContext, bool, XdmValue>>("Compare");
    private static readonly Func<IrOpCode, XdmValue, XdmValue, EvaluationContext, XdmValue> General = Bind<Func<IrOpCode, XdmValue, XdmValue, EvaluationContext, XdmValue>>("CompareGeneral");

    private static T Bind<T>(string name) where T : Delegate
        => (T)typeof(VmEngine).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate(typeof(T));

    /// <summary>Runs bounded observations with all diagnostic failures caught.</summary>
    /// <param name="args">One output JSON path.</param>
    /// <returns>Zero when the observation and protective checks succeed; otherwise two.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("One output path required.");
            Run();
            var report = new { success = true, diagnosticOnly = true, engineChanged = false,
                framework = Environment.Version.ToString(), iterations = Iterations, warmup = Warmup,
                allocationScope = "GC.GetAllocatedBytesForCurrentThread; delegates and inputs prepared before measured loops",
                rows = Rows, checks = Checks };
            File.WriteAllText(args[0], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.ToString());
            return 2;
        }
    }

    private static void Run()
    {
        var primitives = new[] {
            ("integer", XdmValue.FromInteger(42)), ("string", XdmValue.FromString("alpha")),
            ("boolean", XdmValue.FromBoolean(true)), ("decimal", XdmValue.FromDecimal(12.5m)),
            ("double", XdmValue.FromDouble(2.5)) };
        foreach (var (name, value) in primitives)
        {
            Check("helper-scalar-" + name, ArrayHelper(value).Length == 1 && ViewHelper(value).Count == 1);
            Objects("array/scalar/" + name, () => ArrayHelper(value));
            Objects("view/scalar/" + name, () => ViewHelper(value));
        }
        Objects("array/undefined", () => ArrayHelper(XdmValue.Undefined));
        Objects("view/undefined", () => ViewHelper(XdmValue.Undefined));
        var data = new[] { XdmValue.FromInteger(1), XdmValue.FromInteger(2) };
        var arrayValue = XdmValue.FromSequence(MaterializedSequence.FromArray(data));
        var list = data.ToList();
        var listValue = XdmValue.FromSequence(MaterializedSequence.FromList(list));
        Check("materialized-array-reused", ReferenceEquals(ArrayHelper(arrayValue), data) && ReferenceEquals(ViewHelper(arrayValue), data));
        Check("materialized-list-view-reused", ReferenceEquals(ViewHelper(listValue), list) && !ReferenceEquals(ArrayHelper(listValue), list));
        Objects("array/materialized-array", () => ArrayHelper(arrayValue));
        Objects("view/materialized-array", () => ViewHelper(arrayValue));
        Objects("array/materialized-list", () => ArrayHelper(listValue));
        Objects("view/materialized-list", () => ViewHelper(listValue));
        var singleton = XdmValue.FromSequence(XdmSequence.Singleton(XdmValue.FromInteger(1)));
        Objects("array/singleton-sequence", () => ArrayHelper(singleton));
        Objects("view/singleton-sequence", () => ViewHelper(singleton));
        var empty = XdmValue.FromSequence(XdmSequence.Empty);
        Objects("array/empty-sequence", () => ArrayHelper(empty));
        Objects("view/empty-sequence", () => ViewHelper(empty));

        var snapshot = ArrayHelper(listValue);
        var alias = ViewHelper(listValue);
        list[0] = XdmValue.FromInteger(99);
        Check("list-copy-versus-view-alias", snapshot[0].IntegerValue == 1 && alias[0].IntegerValue == 99);
        TraceBehavior("array", v => ArrayHelper(v));
        TraceBehavior("view", ViewHelper);

        var context = new EvaluationContext();
        var one = XdmValue.FromInteger(1); var two = XdmValue.FromInteger(2);
        Check("strict-scalar-result", Compare(IrOpCode.LessThan, one, two, context, true).BooleanValue);
        Check("general-scalar-result", General(IrOpCode.GeneralLessThan, one, two, context).BooleanValue);
        Values("strict-comparison/primitive-integers", () => Compare(IrOpCode.LessThan, one, two, context, true));
        Values("general-comparison/primitive-integers", () => General(IrOpCode.GeneralLessThan, one, two, context));
        Values("function-conversion/matching-integer", () => VmEngine.ApplyFunctionConversion(one, "xs:integer", context));
        Values("function-conversion/integer-to-double", () => VmEngine.ApplyFunctionConversion(one, "xs:double", context));

        var singleValue = XdmValue.FromSequence(MaterializedSequence.FromArray(one));
        Values("strict-comparison/materialized-singletons", () => Compare(IrOpCode.LessThan, singleValue, two, context, true));
        Values("function-conversion/materialized-singleton", () => VmEngine.ApplyFunctionConversion(singleValue, "xs:integer", context));
        TypedNodeChecks();
        var validate = Bind<Action<string, EvaluationContext>>("ValidateFunctionConversionTarget");
        var normalize = Bind<Func<string, string>>("NormalizeTypeName");
        var atomicNormalize = Bind<Func<string, string>>("NormalizeTypeNameForAtomicMatch");
        var choice = Bind<ChoiceHelper>("TryGetChoiceAlternatives");
        Objects("validation/integer", () => { validate("xs:integer", context); return null!; });
        Objects("normalization/integer", () => normalize("xs:integer"));
        Objects("atomic-normalization/integer-uncached", () => atomicNormalize("xs:integer"));
        Values("choice-check/integer", () => XdmValue.FromBoolean(choice("xs:integer", out _)));
        Values("nominal-record-lookup/integer", () => XdmValue.FromBoolean(context.TryResolveNamedRecordType("xs:integer", context.DefaultElementNamespace, out _)));
        foreach (var (name, value) in primitives)
        {
            var target = "xs:" + (name == "boolean" ? "boolean" : name);
            Check("matching-" + name, VmEngine.ValueMatchesType(value, target, context));
            Values("value-matches/" + name, () => XdmValue.FromBoolean(VmEngine.ValueMatchesType(value, target, context)));
            Values("conversion/" + name, () => VmEngine.ApplyFunctionConversion(value, target, context));
        }
        Values("value-matches/integer-null-context", () => XdmValue.FromBoolean(VmEngine.ValueMatchesType(one, "xs:integer", null)));
        Values("conversion/integer-null-context", () => VmEngine.ApplyFunctionConversion(one, "xs:integer", null));
        Objects("constructor-inventory", () => null!);
        InspectConstructors();
    }

    private delegate bool ChoiceHelper(string text, out string[] alternatives);

    private static void InspectConstructors()
    {
        var method = typeof(VmEngine).GetMethod("ValueMatchesType", BindingFlags.Public | BindingFlags.Static,
            new[] { typeof(XdmValue), typeof(string), typeof(EvaluationContext) })!;
        var bytes = method.GetMethodBody()!.GetILAsByteArray()!;
        var codes = typeof(System.Reflection.Emit.OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(System.Reflection.Emit.OpCode))
            .Select(f => (System.Reflection.Emit.OpCode)f.GetValue(null)!).ToDictionary(c => unchecked((ushort)c.Value));
        var constructors = new List<object>();
        for (var offset = 0; offset < bytes.Length;)
        {
            var start = offset; ushort code = bytes[offset++];
            if (code == 0xfe) code = (ushort)(0xfe00 | bytes[offset++]);
            var op = codes[code];
            if (op == System.Reflection.Emit.OpCodes.Newobj)
            {
                var ctor = method.Module.ResolveMethod(BitConverter.ToInt32(bytes, offset))!;
                constructors.Add(new { offset = start, type = ctor.DeclaringType!.FullName,
                    fields = ctor.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(f => new { f.Name, type = f.FieldType.FullName }).ToArray() });
            }
            offset += op.OperandType switch {
                System.Reflection.Emit.OperandType.InlineNone => 0,
                System.Reflection.Emit.OperandType.ShortInlineBrTarget or System.Reflection.Emit.OperandType.ShortInlineI or System.Reflection.Emit.OperandType.ShortInlineVar => 1,
                System.Reflection.Emit.OperandType.InlineVar => 2,
                System.Reflection.Emit.OperandType.InlineI8 or System.Reflection.Emit.OperandType.InlineR => 8,
                System.Reflection.Emit.OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(bytes, offset),
                _ => 4 };
        }
        Checks.Add(new { name = "value-matches-constructor-il", success = true, method = method.ToString(), constructors });
    }

    private static void TypedNodeChecks()
    {
        const string schema = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:simpleType name='Ints'><xs:list itemType='xs:integer'/></xs:simpleType><xs:element name='e' type='Ints'/></xs:schema>";
        var schemas = new XmlSchemaSet { XmlResolver = null };
        using var reader = XmlReader.Create(new StringReader(schema));
        schemas.Add(null, reader); schemas.Compile();
        var document = XDocument.Parse("<e>1 2</e>");document.Validate(schemas, null, true);
        var node = new XDocumentNode(document.Root!);var value = XdmValue.FromNode(node);
        Check("typed-list-node-preserved-by-helpers", ArrayHelper(value)[0].IsNode && ViewHelper(value)[0].IsNode && ReferenceEquals(ArrayHelper(value)[0].NodeValue, node));
        var typed = node.TypedValue;
        Check("typed-list-expands-when-materialized", ArrayHelper(typed).Length == 2 && ViewHelper(typed).Count == 2);
        var context = new EvaluationContext { SchemaSet = schemas };
        var converted = VmEngine.ApplyFunctionConversion(value, "xs:integer*", context);
        var items = ArrayHelper(converted);
        Check("conversion-atomizes-typed-list", items.Length == 2 && items[0].IntegerValue == 1 && items[1].IntegerValue == 2);
        Check("general-comparison-typed-list", General(IrOpCode.GeneralEqual, value, XdmValue.FromInteger(2), context).BooleanValue);
    }

    private static void TraceBehavior(string name, Func<XdmValue, IReadOnlyList<XdmValue>> helper)
    {
        var source = new TraceSequence(false);
        var value = XdmValue.FromSequence(XdmSequence.FromSource(source));
        Check(name + "/lazy-construction", source.Events.Count == 0);
        var result = helper(value);
        Check(name + "/single-pass-once", source.Starts == 1 && result.Count == 2 && source.Events.SequenceEqual(new[] { "get", "move:0", "move:1", "move:2" }));
        var fault = new TraceSequence(true);
        string? caught = null;
        try { _ = helper(XdmValue.FromSequence(XdmSequence.FromSource(fault))); }
        catch (InvalidOperationException error) { caught = error.Message; }
        Check(name + "/fault-before-return", caught == "probe-sequence-fault" && fault.Starts == 1 && fault.Events.SequenceEqual(new[] { "get", "move:0", "move:1" }));
        Checks.Add(new { name = name + "/observed-events", success = true, normal = source.Events, fault = fault.Events });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Objects(string name, Func<object> call)
    {
        for (var i = 0; i < Warmup; i++) _sink = call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++) _sink = call();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Rows.Add(new { name, iterations = Iterations, bytes, bytesPerCall = (double)bytes / Iterations });
        GC.KeepAlive(_sink);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Values(string name, Func<XdmValue> call)
    {
        for (var i = 0; i < Warmup; i++) _valueSink = call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++) _valueSink = call();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Rows.Add(new { name, iterations = Iterations, bytes, bytesPerCall = (double)bytes / Iterations });
        GC.KeepAlive(_valueSink);
    }

    private static void Check(string name, bool success)
    {
        Checks.Add(new { name, success });
        if (!success) throw new InvalidOperationException("Protective check failed: " + name);
    }

    private sealed class TraceSequence(bool fault) : ISinglePassSequence
    {
        internal List<string> Events { get; } = new();
        internal int Starts { get; private set; }
        /// <summary>Reports that this diagnostic source has no known length.</summary>
        /// <param name="length">Always zero when length is unknown.</param>
        /// <returns>False.</returns>
        public bool TryGetLength(out int length) { length = 0; return false; }
        /// <summary>Starts the single permitted enumeration.</summary>
        /// <returns>The diagnostic enumerator.</returns>
        /// <exception cref="InvalidOperationException">A second enumeration is requested.</exception>
        public IXdmSequenceEnumerator GetEnumerator()
        {
            if (++Starts != 1) throw new InvalidOperationException("probe-replay");
            Events.Add("get");return new Cursor(this, fault);
        }
        private sealed class Cursor(TraceSequence owner, bool fault) : IXdmSequenceEnumerator
        {
            private int _index = -1;
            /// <inheritdoc/>
            public XdmValue Current => XdmValue.FromInteger(_index + 1);
            /// <summary>Advances once, optionally raising the configured caught fault.</summary>
            /// <returns>True for each of two items; false after completion.</returns>
            /// <exception cref="InvalidOperationException">The diagnostic fault is reached.</exception>
            public bool MoveNext()
            {
                _index++;owner.Events.Add("move:" + _index);
                if (fault && _index == 1) throw new InvalidOperationException("probe-sequence-fault");
                return _index < 2;
            }
        }
    }
}
