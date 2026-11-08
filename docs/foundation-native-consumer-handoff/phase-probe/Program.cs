// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Attributes candidate public consumer allocations to individual lifecycle phases.
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

namespace Bosak.Schema.PublicConsumerPhaseProbe;

/// <summary>Attributes the observed candidate allocation volume to public request/query/disposal phases.</summary>
internal static class Program
{
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
    private static IXdmNode First(XdmSequence sequence)
    { foreach (var value in sequence) return value.NodeValue; throw new InvalidOperationException("Missing node."); }
    /// <summary>Writes one phase-attribution diagnostic while catching failures inside the process.</summary>
    /// <param name="args">One output JSON path.</param>
    /// <returns>Zero for completed diagnosis; one for a caught failure.</returns>
    public static int Main(string[] args)
    {
        try
        {
            var rows = new List<object>();
            T Measure<T>(string name, Func<T> action)
            {
                long allocated = GC.GetAllocatedBytesForCurrentThread(), start = Stopwatch.GetTimestamp();
                T result = action(); rows.Add(new { phase = name, elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds, allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocated }); return result;
            }
            var schema = XDocument.Parse("""
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
                <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
                <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
                """);
            using var provider = Measure("compileAndBind", () => new NativeProviderSession(schema));
            using var result = Measure("captureAndAssess", () => provider.Assess(XDocument.Parse("<order><amount>1.25</amount></order>"), new Episode()));
            using var snapshot = Measure("openSnapshot", () => result.OpenSnapshot());
            var child = Measure("projectNavigation", () => First(First(result.Assessment.ValidatedRoot!.Children(XdmNodeKind.Element)).Children(XdmNodeKind.Element)));
            var value = Measure("publicDataEvaluate", () => provider.Engine.Run([snapshot], op => op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child)))));
            if (value.DecimalValue != 1.25m) throw new InvalidOperationException("Incorrect value.");
            Measure("closeAdmission", () => { snapshot.Dispose(); return 0; });
            Measure("closeResult", () => { result.Dispose(); return 0; });
            File.WriteAllText(args[0], JsonSerializer.Serialize(new { date = "2026-11-07", scope = "Single candidate-only cold phase probe;not accepted budget or statistical benchmark", phases = rows }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
