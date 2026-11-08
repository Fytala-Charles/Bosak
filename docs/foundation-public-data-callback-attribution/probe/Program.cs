// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Attributes first and cached public atomization to exact provider callback counts.
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
using System.Text.Json;
using System.Xml.Linq;
using Bosak.Schema.ContainerPrototype;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Schema.PublicDataCallbackProbe;

/// <summary>Counts provider callbacks for first and cached public atomization without eliding checks.</summary>
internal static class Program
{
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
    private static IXdmNode First(XdmSequence sequence)
    { foreach (var value in sequence) return value.NodeValue; throw new InvalidOperationException("Missing node."); }
    /// <summary>Writes exact serial provider boundary counts and catches faults inside the diagnostic.</summary>
    /// <param name="args">One output JSON path.</param>
    /// <returns>Zero for completed attribution; one for a caught diagnostic fault.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("Supply the output path.");
            var rows = new List<object>();
            foreach (int count in new[] { 1, 8 })
            {
                var schema = XDocument.Parse("""
                    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
                    <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="8"/></xs:sequence>
                    <xs:assert test="count(amount) le 8"/></xs:complexType></xs:element></xs:schema>
                    """);
                using var provider = new NativeProviderSession(schema);
                var xml = XDocument.Parse("<order>" + string.Concat(Enumerable.Repeat("<amount>1.25</amount>", count)) + "</order>");
                using var result = provider.Assess(xml, new Episode()); using var snapshot = result.OpenSnapshot();
                var root = First(result.Assessment.ValidatedRoot!.Children(XdmNodeKind.Element));
                var children = new List<IXdmNode>(); foreach (var value in root.Children(XdmNodeKind.Element)) children.Add(value.NodeValue);
                if (children.Count != count) throw new InvalidOperationException("Incorrect child count.");
                foreach (string phase in new[] { "firstData", "cachedData" })
                {
                    ProviderDiagnostics.Reset(); long allocated = GC.GetAllocatedBytesForCurrentThread(), start = Stopwatch.GetTimestamp();
                    decimal sum = provider.Engine.Run([snapshot], op =>
                    {
                        decimal total = 0;
                        foreach (var child in children) total += op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child))).DecimalValue;
                        return total;
                    });
                    double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds; long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
                    var counters = ProviderDiagnostics.Snapshot();
                    if (sum != count * 1.25m) throw new InvalidOperationException("Incorrect typed result.");
                    rows.Add(new { children = count, phase, sum, elapsedMs, allocatedBytes, counters });
                }
            }
            File.WriteAllText(args[0], JsonSerializer.Serialize(new { date = "2026-11-07", scope = "Instrumented serial provider boundary counts;not exclusive CPU profile or comparable uninstrumented timings", coreChanged = false, checksElided = false, rows }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Counted first/cached public data for1/8children without removing checks;no performance acceptance.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
