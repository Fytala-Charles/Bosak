// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Measures warmed VM allocations in a standalone test worker
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
// ===========================================================================================================================================================
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.AllocationChecks;

/// <summary>Enters the diagnostic without resolving engine types before its error guard.</summary>
internal static class Program
{
    /// <summary>Runs one fixed allocation workload and catches managed diagnostic errors.</summary>
    /// <param name="args">The comparisons or matching workload name.</param>
    /// <returns>Zero after a complete JSON report, otherwise two.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1 || args[0] is not ("comparisons" or "matching"))
                throw new ArgumentException("Expected comparisons or matching.");
            // Resolve the engine-bearing type inside the guard, not at entry-point load.
            var type = typeof(Program).Assembly.GetType("Bosak.XPath.AllocationChecks.Workloads", true)!;
            var report = type.GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, new object[] { args[0] });
            Console.WriteLine(JsonSerializer.Serialize(report));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 2;
        }
    }
}

/// <summary>Contains engine-bearing state loaded only inside the entry-point error guard.</summary>
internal static class Workloads
{
    private static XdmValue _sink;

    internal static object Run(string workload)
    {
        var vm = typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine", true)!;
        var context = new EvaluationContext();
        var rows = new List<object>();
        if (workload == "comparisons")
        {
            var apply = vm.GetMethod("ApplyBinaryOperator", BindingFlags.Public | BindingFlags.Static)!
                .CreateDelegate<Func<string, XdmValue, XdmValue, EvaluationContext, XdmValue>>();
            var one = XdmValue.FromInteger(1); var two = XdmValue.FromInteger(2);
            foreach (var op in new[] { "=", "!=", "<", "<=", ">", ">=" })
                rows.Add(Measure(op, () => apply(op, one, two, context)));
        }
        else
        {
            var matches = vm.GetMethod("ValueMatchesType", BindingFlags.Public | BindingFlags.Static,
                new[] { typeof(XdmValue), typeof(string), typeof(EvaluationContext) })!
                .CreateDelegate<Func<XdmValue, string, EvaluationContext, bool>>();
            var convert = vm.GetMethod("ApplyFunctionConversion", BindingFlags.Public | BindingFlags.Static)!
                .CreateDelegate<Func<XdmValue, string, EvaluationContext, XdmValue>>();
            var cases = new[] { ("integer", XdmValue.FromInteger(1)), ("string", XdmValue.FromString("a")),
                ("boolean", XdmValue.FromBoolean(true)), ("decimal", XdmValue.FromDecimal(1m)),
                ("double", XdmValue.FromDouble(1)) };
            foreach (var (name, value) in cases)
            {
                var target = "xs:" + name;
                rows.Add(Measure("match/" + name, () => XdmValue.FromBoolean(matches(value, target, context))));
                rows.Add(Measure("convert/" + name, () => convert(value, target, context)));
            }
        }
        return new { protocol = 1, success = true, workload = workload,
            processId = Environment.ProcessId, framework = Environment.Version.ToString(),
            engine = typeof(EvaluationContext).Assembly.GetName().Version!.ToString(),
            iterations = 100000, warmup = 10000, rows };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object Measure(string name, Func<XdmValue> call)
    {
        for (var i = 0; i < 10000; i++) _sink = call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100000; i++) _sink = call();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(_sink);
        return new { name, bytes, kind = _sink.Kind.ToString(),
            boolean = _sink.Kind == XdmValueKind.Boolean ? (bool?)_sink.BooleanValue : null };
    }
}
