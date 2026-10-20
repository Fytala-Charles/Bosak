// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Runs native array/select boundary fixtures against both processor profiles.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Runs native array/select boundary fixtures against both processor profiles.</summary>
public class ArraySelectNativeBoundaryTests
{
    private static readonly XNamespace CatalogNamespace = "http://www.w3.org/2012/10/xslt-test-catalog";
    private static string FixtureRoot => Path.Combine(AppContext.BaseDirectory, "Fixtures", "ArraySelectBoundaries");

    /// <summary>Supplies each native catalog case under the legacy and draft profiles.</summary>
    public static TheoryData<bool, string, string> FixtureCases
    {
        get
        {
            var data = new TheoryData<bool, string, string>();
            var catalog = XDocument.Load(Path.Combine(FixtureRoot, "catalog.xml"));
            foreach (var set in catalog.Root!.Elements(CatalogNamespace + "test-set"))
            {
                string relativePath = set.Attribute("file")!.Value;
                var document = XDocument.Load(Path.Combine(FixtureRoot, relativePath));
                foreach (var test in document.Root!.Elements(CatalogNamespace + "test-case"))
                foreach (bool draft in new[] { false, true })
                    data.Add(draft, relativePath, test.Attribute("name")!.Value);
            }
            return data;
        }
    }

    /// <summary>Checks the native expected XML or exact error identity through the public API.</summary>
    /// <param name="draft">Whether to use the draft processor profile.</param>
    /// <param name="relativePath">The native test-set path relative to the fixture catalog.</param>
    /// <param name="caseName">The native case identifier.</param>
    [Theory]
    [MemberData(nameof(FixtureCases))]
    public void NativeBoundary(bool draft, string relativePath, string caseName)
    {
        string path = Path.Combine(FixtureRoot, relativePath);
        var test = XDocument.Load(path).Root!.Elements(CatalogNamespace + "test-case")
            .Single(c => c.Attribute("name")!.Value == caseName);
        string stylesheet = test.Element(CatalogNamespace + "test")!
            .Element(CatalogNamespace + "stylesheet")!.Attribute("file")!.Value;
        string stylesheetPath = Path.Combine(Path.GetDirectoryName(path)!, stylesheet);
        var compiler = new XsltCompiler
        {
            ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy
        };
        string Run() => compiler.Compile(File.ReadAllText(stylesheetPath), new Uri(stylesheetPath).AbsoluteUri)
            .TransformToString(null, initialTemplate: "xsl:initial-template");
        var result = test.Element(CatalogNamespace + "result")!;
        if (result.Element(CatalogNamespace + "error") is { } expectedError)
        {
            string code = expectedError.Attribute("code")!.Value;
            var error = Assert.ThrowsAny<Exception>(() => Run());
            if (error is XPathErrorException structured)
            {
                Assert.Equal("http://www.w3.org/2005/xqt-errors", structured.CodeNamespaceUri);
                Assert.Equal(code, structured.CodeLocalName);
            }
            else
            {
                Assert.Matches(@"\b" + Regex.Escape(code) + @"\b", error.Message);
            }
        }
        else
        {
            string expected = result.Element(CatalogNamespace + "assert-xml")!.Value;
            string actual = Run();
            Assert.True(XNode.DeepEquals(XElement.Parse(expected), XElement.Parse(actual)),
                $"{caseName}: expected {expected}; actual {actual}");
        }
    }
}

