// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 October 2026
// PURPOSE              : Runs native switch boundary fixtures against both processor profiles.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-10-2026     | Cover when/otherwise node identity in template and function dispatchers.                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Runs native switch boundary fixtures against both processor profiles.</summary>
public class SwitchNativeBoundaryTests
{
    /// <summary>Preserves branch-selected nodes in template and function raw sequences.</summary>
    /// <param name="draft">Whether to use the draft processor profile.</param>
    /// <param name="otherwise">Whether to select the otherwise branch.</param>
    /// <param name="functionBody">Whether to evaluate the switch in a function body.</param>
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void SelectedNodeKeepsIdentity(bool draft, bool otherwise, bool functionBody)
    {
        string branches = otherwise
            ? "<xsl:when test='0' select='error()'/><xsl:otherwise select='$n'/>"
            : "<xsl:when test='1' select='$n'/><xsl:otherwise select='error()'/>";
        string instruction = "<xsl:switch select='1'>" + branches + "</xsl:switch>";
        string declarations = functionBody
            ? "<xsl:function name='f:node' as='element(n)'><xsl:param name='n' as='element(n)'/>" + instruction + "</xsl:function>"
            : "";
        string body = functionBody ? "<xsl:sequence select='f:node($n)'/>" : instruction;
        string stylesheet = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:switch-boundaries' exclude-result-prefixes='#all' version='4.0'>" +
            declarations + "<xsl:template name='start'><xsl:variable name='n' as='element(n)'><n/></xsl:variable>" +
            "<xsl:variable name='result' as='element(n)'>" + body + "</xsl:variable><out same='{$result is $n}'/></xsl:template></xsl:stylesheet>";
        var compiler = new XsltCompiler { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };
        string actual = compiler.Compile(stylesheet).TransformToString(null, initialTemplate: "start");
        Assert.True(XNode.DeepEquals(XElement.Parse("<out same='true'/>"), XElement.Parse(actual)), actual);
    }

    private static readonly XNamespace CatalogNamespace = "http://www.w3.org/2012/10/xslt-test-catalog";
    private static string FixtureRoot => Path.Combine(AppContext.BaseDirectory, "Fixtures", "SwitchBoundaries");

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
