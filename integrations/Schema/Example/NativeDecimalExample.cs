// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 November 2026
// PURPOSE              : Assesses a finite native decimal document and exports detached values and XML.
// SPECIAL NOTES        : Optional public Schema consumer qualification; no Core engine semantic change.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Schema.Example;

/// <summary>Assesses a finite native decimal document and exports detached values and XML.</summary>
public static class NativeDecimalExample
{
    /// <summary>Runs the explicit decimal-only example through the owned public API.</summary>
    /// <param name="schema">A complete finite native decimal schema.</param>
    /// <param name="instance">The complete instance text to assess.</param>
    /// <returns>A detached result with exact values and exported XML, after all live owners are disposed.</returns>
    /// <exception cref="ArgumentNullException">An input is null.</exception>
    /// <exception cref="InvalidOperationException">Compilation or assessment refuses the example input.</exception>
    /// <remarks>The fixed capacities and episode are explicit sample choices, not production API defaults.</remarks>
    public static string Run(string schema, string instance)
    {
        var options = new NativeDecimalSessionOptions(256, 256, 1, 4, 1, 256, 256);
        var episode = new NativeDecimalAssessmentContext(new DateTimeOffset(2026, 11, 17, 12, 0, 0, TimeSpan.Zero), 60);
        var compiled = NativeDecimalSession.Create(schema, options);
        if (compiled.Outcome != NativeDecimalOutcome.Valid)
            throw new InvalidOperationException(compiled.Diagnostics[0].Code);
        using var session = compiled.Value!;
        var assessed = session.Assess(instance, episode);
        if (assessed.Outcome != NativeDecimalOutcome.Valid)
            throw new InvalidOperationException(assessed.Diagnostics[0].Code);
        using var document = assessed.Value!;
        var result = new XElement("native-result", new XAttribute("decimal-count", document.DecimalCount));
        for (var index = 0; index < document.DecimalCount; index++)
            result.Add(new XElement("decimal", document.ReadDecimal(index).ToInvariantString(256)));
        result.Add(new XElement("export", XElement.Parse(document.ExportXml())));
        return result.ToString(SaveOptions.DisableFormatting);
    }
}
