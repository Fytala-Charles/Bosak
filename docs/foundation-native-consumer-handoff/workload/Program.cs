// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Measures bounded candidate-only native public consumer cycles without budget acceptance.
// SPECIAL NOTES        : Isolated provider/consumer qualification; not a production or published API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation; finite observational public consumer workload with caught diagnostic faults.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Bosak.Schema.ContainerPrototype;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Schema.PublicConsumerWorkload;

/// <summary>Measures a bounded candidate-only public native provider cycle without accepting owner budgets.</summary>
internal static class Program
{
    private const string SchemaText = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
        <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
        <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
        """;
    private const string InstanceText = "<order><amount>1.25</amount></order>";
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
    private static readonly Episode FixedEpisode = new();
    private static IXdmNode First(XdmSequence sequence)
    { foreach (var value in sequence) return value.NodeValue; throw new InvalidOperationException("Missing node."); }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Cycle(NativeProviderSession provider)
    {
        using var result = provider.Assess(XDocument.Parse(InstanceText), FixedEpisode);
        using var snapshot = result.OpenSnapshot();
        var root = result.Assessment.ValidatedRoot!;
        var child = First(First(root.Children(XdmNodeKind.Element)).Children(XdmNodeKind.Element));
        var value = provider.Engine.Run([snapshot], op => op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child))));
        if (value.DecimalValue != 1.25m) throw new InvalidOperationException("Incorrect native decimal result.");
        return new WeakReference(root);
    }
    /// <summary>Writes finite observational pilot metrics and catches faults inside the diagnostic process.</summary>
    /// <param name="args">Exactly one output JSON path supplied by the scoped runner.</param>
    /// <returns>Zero for a completed pilot; one for a caught diagnostic failure.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("Supply the metrics output path.");
            const int warmup = 10, iterations = 100;
            using var provider = new NativeProviderSession(XDocument.Parse(SchemaText));
            for (int i = 0; i < warmup; i++) Cycle(provider);
            var weak = new WeakReference[iterations]; var times = new double[iterations];
            long retainedBefore = GC.GetTotalMemory(true);
            var gcBefore = Enumerable.Range(0, 3).Select(GC.CollectionCount).ToArray();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                long itemStart = Stopwatch.GetTimestamp(); weak[i] = Cycle(provider);
                times[i] = Stopwatch.GetElapsedTime(itemStart).TotalMilliseconds;
            }
            double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            var gcDeltas = Enumerable.Range(0, 3).Select(i => GC.CollectionCount(i) - gcBefore[i]).ToArray();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long retainedAfter = GC.GetTotalMemory(true);
            int alive = weak.Count(w => w.IsAlive); Array.Sort(times);
            string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
            var metrics = new
            {
                date = "2026-11-07", scenario = "PF-03 bounded native decimal public consumer subset pilot",
                sourceVersion = "1.81.0", ownerAccepted = false, budgetsAccepted = false,
                baselineRun = false, comparisonRatio = (double?)null,
                scope = "Candidate-only serial whole request/assessment/admission/data/dispose cycle;includes parse and guard overhead;not owner workload or regression acceptance.",
                warmup, iterations, runtime = Environment.Version.ToString(), architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                schemaSha256 = Hash(SchemaText), instanceSha256 = Hash(InstanceText),
                elapsedMs, cyclesPerSecond = iterations * 1000 / elapsedMs,
                latencyMs = new { min = times[0], median = (times[49] + times[50]) / 2, p95 = times[94], max = times[99] },
                allocatedBytes = allocated, allocatedBytesPerCycle = (double)allocated / iterations, gcDeltas,
                retainedBefore, retainedAfter, retainedDeltaBytes = retainedAfter - retainedBefore,
                outputRootsAliveAfterForcedGc = alive,
                retentionScope = "Weak roots and whole-process managed memory observation only;not proof of zero retention or an accepted resource budget."
            };
            File.WriteAllText(args[0], JsonSerializer.Serialize(metrics, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine($"Completed {iterations} bounded candidate cycles; {allocated / iterations} allocated bytes/cycle; {alive} observed roots alive after forced GC. No baseline/budget acceptance.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
