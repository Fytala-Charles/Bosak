// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Collects bounded owner-protocol candidate repetitions without accepting budgets.
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Bosak.Schema.ContainerPrototype;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Schema.OwnerWorkloadProtocol;

/// <summary>Collects bounded candidate-only repetitions for the proposed native decimal owner protocol.</summary>
internal static class Program
{
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
    private static readonly Episode FixedEpisode = new();
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static IXdmNode First(XdmSequence sequence)
    { foreach (var value in sequence) return value.NodeValue; throw new InvalidOperationException("Missing node."); }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Cycle(NativeProviderSession provider, string xml, int count, decimal expected)
    {
        using var result = provider.Assess(XDocument.Parse(xml), FixedEpisode);
        using var snapshot = result.OpenSnapshot(); var root = result.Assessment.ValidatedRoot!;
        var children = new List<IXdmNode>();
        foreach (var cell in First(root.Children(XdmNodeKind.Element)).Children(XdmNodeKind.Element)) children.Add(cell.NodeValue);
        if (children.Count != count) throw new InvalidOperationException("Incorrect child count.");
        decimal sum = provider.Engine.Run([snapshot], op =>
        {
            decimal total = 0;
            foreach (var node in children) total += op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(node))).DecimalValue;
            return total;
        });
        if (sum != expected) throw new InvalidOperationException("Incorrect native decimal sum.");
        return new WeakReference(root);
    }
    /// <summary>Runs the finite proposal and writes observations, catching diagnostic faults inside the process.</summary>
    /// <param name="args">Protocol JSON path followed by output JSON path.</param>
    /// <returns>Zero for completed measurements; one for a caught fault.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Supply protocol and output paths.");
            string protocolPath = Path.GetFullPath(args[0]), root = Path.GetDirectoryName(protocolPath)!;
            using var document = JsonDocument.Parse(File.ReadAllText(protocolPath)); var protocol = document.RootElement;
            if (protocol.GetProperty("repetitions").GetInt32() != 3 || protocol.GetProperty("warmup").GetInt32() != 2 || protocol.GetProperty("iterations").GetInt32() != 5)
                throw new NotSupportedException("Only the finite three/two/five protocol is implemented.");
            var limits = protocol.GetProperty("limits");
            foreach (var pair in new[] { ("openSnapshots", 4), ("cachedValues", 8), ("typedAtoms", 8), ("numericDigits", 128), ("decimalScale", 128) })
                if (limits.GetProperty(pair.Item1).GetInt32() != pair.Item2) throw new NotSupportedException("Provider limits differ from the proposal.");
            string schema = Path.Combine(root, protocol.GetProperty("schema").GetString()!);
            if (Hash(schema) != protocol.GetProperty("schemaSha256").GetString()) throw new InvalidOperationException("Schema hash differs.");
            var rows = new List<object>();
            foreach (var workload in protocol.GetProperty("workloads").EnumerateArray())
            {
                int children = workload.GetProperty("children").GetInt32();
                if (children is not (1 or 8)) throw new NotSupportedException("Unselected size.");
                string instance = Path.Combine(root, workload.GetProperty("instance").GetString()!);
                if (Hash(instance) != workload.GetProperty("instanceSha256").GetString()) throw new InvalidOperationException("Instance hash differs.");
                string xml = File.ReadAllText(instance); decimal expected = decimal.Parse(workload.GetProperty("expectedSum").GetString()!, CultureInfo.InvariantCulture);
                for (int repetition = 0; repetition < 3; repetition++)
                {
                    long compileStart = Stopwatch.GetTimestamp();
                    using var provider = new NativeProviderSession(XDocument.Load(schema), protocol.GetProperty("expression").GetString()!);
                    double compileAndBindMs = Stopwatch.GetElapsedTime(compileStart).TotalMilliseconds;
                    if (provider.Engine.Environment.BehaviorProfile != protocol.GetProperty("behaviorProfile").GetString()) throw new InvalidOperationException("Profile differs.");
                    for (int i = 0; i < 2; i++) Cycle(provider, xml, children, expected);
                    var weak = new WeakReference[5]; var times = new double[5];
                    long retainedBefore = GC.GetTotalMemory(true);
                    int[] gcBefore = Enumerable.Range(0, 3).Select(GC.CollectionCount).ToArray();
                    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread(), start = Stopwatch.GetTimestamp();
                    for (int i = 0; i < 5; i++) { long item = Stopwatch.GetTimestamp(); weak[i] = Cycle(provider, xml, children, expected); times[i] = Stopwatch.GetElapsedTime(item).TotalMilliseconds; }
                    double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                    int[] gcDeltas = Enumerable.Range(0, 3).Select(i => GC.CollectionCount(i) - gcBefore[i]).ToArray();
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); long retainedAfter = GC.GetTotalMemory(true);
                    rows.Add(new { workload = workload.GetProperty("id").GetString(), children, repetition, iterations = 5, warmup = 2, compileAndBindMs, elapsedMs, latencySamplesMs = times, allocatedBytes, allocatedBytesPerCycle = allocatedBytes / 5d, allocatedBytesPerCell = allocatedBytes / (5d * children), gcDeltas, retainedBefore, retainedAfter, outputRootsAliveAfterForcedGc = weak.Count(w => w.IsAlive) });
                }
            }
            var receipt = new
            {
                format = "bosak-owner-measurement-v1", date = "2026-11-07", protocolSha256 = Hash(protocolPath),
                runtime = Environment.Version.ToString(), architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                osDescription = RuntimeInformation.OSDescription,
                sourceVersion = "1.81.0", role = "candidate", ownerAccepted = false, budgetsAccepted = false,
                baselineRun = false, comparisonRatio = (double?)null,
                scope = "Proposed serial native decimal PF-03 subset;not complete owner coverage or statistical regression acceptance.", rows
            };
            File.WriteAllText(args[1], JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Measured two bounded fixtures in three independent provider sessions each;30 cycles/135 decimal reads. No baseline or owner budget acceptance.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
