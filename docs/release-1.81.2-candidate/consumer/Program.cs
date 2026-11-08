// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Checks ordinary XPath, XQuery and XSLT behavior through preview packages only.
// SPECIAL NOTES        : Isolated package consumer; no production source or provider acceptance.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 08-11-2026     | Include the tenth Database package in local consumption checks.                         |
// ===========================================================================================================================================================
using System.Reflection;
using System.Text.Json;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;
using Bosak.Xslt.Api;

namespace Bosak.Preview.PackageConsumer;

/// <summary>Runs finite package-only behavior checks with caught diagnostic faults.</summary>
internal static class Program
{
    /// <summary>Checks ordinary paths and emits a bounded result receipt.</summary>
    /// <param name="args">One output receipt path.</param>
    /// <returns>Zero for all checks passing; one for a caught failure.</returns>
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("Supply one result receipt path.");
            var cases = new List<string>();
            void Check(string name, bool value)
            { if (!value) throw new InvalidOperationException(name); cases.Add(name); }
            Check("xpath-arithmetic", XPath31Expression.Compile("2+2").Evaluate(new EvaluationContext()).IntegerValue == 4);
            Check("xpath-empty", XPath31Expression.Compile("count(())").Evaluate(new EvaluationContext()).IntegerValue == 0);
            Check("xpath-map", XPath31Expression.Compile("map:get(map{'a':42},'a')").Evaluate(new EvaluationContext()).IntegerValue == 42);
            Check("xquery-flwor", new XQueryCompiler().Compile("sum(for $x in (1,2,3) return $x * 2)").Evaluate(new XQueryContext()).IntegerValue == 12);
            Check("xquery-empty", new XQueryCompiler().Compile("count(for $x in () return $x)").Evaluate(new XQueryContext()).IntegerValue == 0);
            var stylesheet = new XsltCompiler().Compile("""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:output method="text"/>
                  <xsl:template name="main"><xsl:value-of select="6*7"/></xsl:template>
                </xsl:stylesheet>
                """);
            Check("xslt-sourcefree-template", stylesheet.TransformToString(null, initialTemplate: "main") == "42");
            Check("database-package-type", typeof(Bosak.XPath.Providers.Database.DatabaseLoaderOptions).Assembly.GetName().Name == "Bosak.XPath.Providers.Database");
            foreach (var name in new[] { "Bosak.XPath.Core", "Bosak.XPath.Parser", "Bosak.XPath.Compiler", "Bosak.XPath.Runtime", "Bosak.XPath.Standard", "Bosak.XPath.Providers", "Bosak.XPath.Api", "Bosak.XQuery", "Bosak.Xslt", "Bosak.XPath.Providers.Database" })
                Assembly.Load(name); // Ensure the observer records every package assembly; not behavioral coverage.
            File.WriteAllText(args[0], JsonSerializer.Serialize(new { passed = cases.Count, failed = 0, cases, ownerAccepted = false, publicationAuthorized = false }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine($"Passed {cases.Count} package-only behavior controls.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
