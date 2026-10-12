// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies catalog examples and their declared compatibility outcomes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: exact published catalog examples, edge cases, and version gates.              |
//                      | Charles Korthout | 0.2   | 12-10-2026     | Verify declared 3.1 outcomes for changed existing-function contracts.                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Xunit;

namespace Bosak.Xslt.Tests.Documentation;

/// <summary>Verifies catalog examples and their declared compatibility outcomes.</summary>
public class CatalogExampleTests
{
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory,
        "Documentation", "ReferenceExamples", "Catalog");

    /// <summary>Gets the IDs of all published catalog cases.</summary>
    /// <returns>One theory row per catalog manifest entry.</returns>
    public static IEnumerable<object[]> Cases()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Folder, "cases.json")));
        foreach (var row in doc.RootElement.EnumerateArray())
            yield return new object[] { row.GetProperty("id").GetString()! };
    }

    /// <summary>Runs the exact published file and verifies result, error, and compatibility.</summary>
    /// <param name="id">The case ID from the published manifest.</param>
    [Theory]
    [MemberData(nameof(Cases))]
    public void PublishedExample_MatchesDeclaredOutcome(string id)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Folder, "cases.json")));
        var row = doc.RootElement.EnumerateArray().Single(r => r.GetProperty("id").GetString() == id);
        var file = Path.Combine(Folder, row.GetProperty("file").GetString()!);
        var code = File.ReadAllText(file);
        if (row.GetProperty("kind").GetString() == "xslt")
        {
            var source = XDocument.Load(Path.Combine(Folder, row.GetProperty("input").GetString()!));
            var result = new XsltCompiler().Compile(code, new Uri(file).AbsoluteUri)
                .CaptureTransform(new XDocumentNode(source), new XsltCaptureOptions());
            Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
            Assert.Empty(result.SecondaryOutputs);
            Assert.Equal(row.GetProperty("expected").GetString(), Encoding.UTF8.GetString(result.Principal!.Bytes.ToArray()));
            return;
        }

        var mode = row.GetProperty("mode").GetString();
        var compatibility = mode switch
        {
            "31" => XPathCompatibility.XPath31,
            "experimental" => XPathCompatibility.XPath40Experimental,
            _ => XPathCompatibility.XPath40,
        };
        string Evaluate() => XPath31Expression.Compile(code, new CompileOptions { Compatibility = compatibility })
            .Evaluate(new EvaluationContext()).ToString();

        if (row.TryGetProperty("error", out var error))
        {
            var failure = Assert.ThrowsAny<Exception>(() => Evaluate());
            Assert.Contains(error.GetString()!, failure.Message);
        }
        else
        {
            Assert.Equal(row.GetProperty("expected").GetString(), Evaluate());
            if (mode != "31" && row.TryGetProperty("baselineError", out var baselineError))
            {
                var failure = Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(code)
                    .Evaluate(new EvaluationContext()).ToString());
                Assert.Contains(baselineError.GetString()!, failure.Message);
            }
            else if (mode != "31" && row.TryGetProperty("baselineExpected", out var baselineExpected))
            {
                Assert.Equal(baselineExpected.GetString(), XPath31Expression.Compile(code)
                    .Evaluate(new EvaluationContext()).ToString());
            }
            else if (mode != "31")
                Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(code));
            if (mode == "experimental")
                Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(code,
                    new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        }
    }
}
