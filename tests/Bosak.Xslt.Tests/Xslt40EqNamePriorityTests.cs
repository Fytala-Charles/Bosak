// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 October 2026
// PURPOSE              : Verifies EQName URI punctuation in effective XSLT 4.0 pattern priorities.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies EQName URI punctuation in effective XSLT 4.0 pattern priorities.</summary>
public class Xslt40EqNamePriorityTests
{
    private static string Run(string pattern, string source, string select = "/*", string competitor = "0.25",
        string rootVersion = "4.0", string templateVersion = "4.0")
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var stylesheet = new XElement(xsl + "stylesheet", new XAttribute("version", rootVersion),
            new XAttribute(XNamespace.Xmlns + "xsl", xsl.NamespaceName),
            new XElement(xsl + "template", new XAttribute("match", "/"), new XAttribute("priority", "2"),
                new XElement("out", new XElement(xsl + "apply-templates", new XAttribute("select", select)))),
            new XElement(xsl + "template", new XAttribute("version", templateVersion), new XAttribute("match", pattern), "named"),
            new XElement(xsl + "template", new XAttribute("match", "."), new XAttribute("priority", competitor), "competitor"));
        var compiled = new Api.XsltCompiler().Compile(stylesheet.ToString());
        return XElement.Parse(compiled.TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;
    }

    /// <summary>Keeps URI punctuation out of the name test's implicit priority calculation.</summary>
    /// <param name="uri">The namespace URI containing punctuation.</param>
    [Theory]
    [InlineData("urn:n|part")]
    [InlineData("https://example.test/ns")]
    [InlineData("urn:n(part)|more")]
    [InlineData("urn:n(part)")]
    [InlineData("urn:n*part")]
    [InlineData("urn:n::part")]
    [InlineData("urn:n]part")]
    [InlineData("urn:n)part")]
    public void NamedElementPriority(string uri)
    {
        string source = new XElement(XName.Get("root", uri)).ToString();
        Assert.Equal("competitor", Run("Q{" + uri + "}root", source));
    }

    /// <summary>Applies the correction only in the effective local 4.0 template profile.</summary>
    /// <param name="rootVersion">The stylesheet version.</param>
    /// <param name="templateVersion">The local template version.</param>
    /// <param name="expected">The selected template output.</param>
    [Theory]
    [InlineData("3.0", "3.0", "named")]
    [InlineData("4.0", "4.0", "competitor")]
    [InlineData("3.0", "4.0", "competitor")]
    [InlineData("4.0", "3.0", "named")]
    public void EffectiveTemplateVersion(string rootVersion, string templateVersion, string expected)
    {
        Assert.Equal(expected, Run("Q{urn:n|part}root", "<root xmlns='urn:n|part'/>",
            rootVersion: rootVersion, templateVersion: templateVersion));
    }

    /// <summary>Retains real predicates and paths outside the URI while normalizing forward axes.</summary>
    /// <param name="pattern">The complete match pattern.</param>
    /// <param name="expected">The selected template output.</param>
    [Theory]
    [InlineData("Q{urn:n(part)}root", "competitor")]
    [InlineData("Q{urn:n(part)}root[true()]", "named")]
    [InlineData("Q{urn:n(part)}root[false()]", "competitor")]
    [InlineData("child::Q{urn:n|part}root", "competitor")]
    [InlineData("self::Q{urn:n|part}root", "competitor")]
    [InlineData("Q{urn:n|part}root | missing[true()]", "competitor")]
    [InlineData("Q{urn:n|part}root union missing[true()]", "competitor")]
    [InlineData("(Q{urn:n|part}root | missing[true()]) intersect *", "competitor")]
    public void SyntaxOutsideUri(string pattern, string expected)
    {
        string uri = pattern.Contains("(part)") ? "urn:n(part)" : "urn:n|part";
        Assert.Equal(expected, Run(pattern, new XElement(XName.Get("root", uri)).ToString()));
    }

    /// <summary>Preserves priorities for actual paths and wildcard name tests.</summary>
    /// <param name="pattern">The complete match pattern.</param>
    /// <param name="priority">The competing explicit priority.</param>
    /// <param name="expected">The selected template output.</param>
    [Theory]
    [InlineData("Q{https://example.test/ns}root/Q{https://example.test/ns}child", "0.25", "named")]
    [InlineData("Q{https://example.test/ns}*", "-0.1", "competitor")]
    [InlineData("element(Q{https://example.test/ns}child)", "0.25", "competitor")]
    public void PathAndWildcardControls(string pattern, string priority, string expected)
    {
        Assert.Equal(expected, Run(pattern, "<root xmlns='https://example.test/ns'><child/></root>",
            select: "/*/*", competitor: priority));
    }

    /// <summary>Normalizes attribute EQNames without changing the earlier attribute priority.</summary>
    /// <param name="pattern">The attribute match pattern.</param>
    /// <param name="version">The effective profile.</param>
    /// <param name="expected">The selected template output.</param>
    [Theory]
    [InlineData("@Q{urn:n(part)}code", "4.0", "competitor")]
    [InlineData("attribute::Q{urn:n|part}code", "4.0", "competitor")]
    [InlineData("@Q{urn:n|part}code", "3.0", "named")]
    public void AttributePriority(string pattern, string version, string expected)
    {
        string uri = pattern.Contains("(part)") ? "urn:n(part)" : "urn:n|part";
        var source = new XElement("root", new XAttribute(XNamespace.Xmlns + "n", uri), new XAttribute(XName.Get("code", uri), "A"));
        Assert.Equal(expected, Run(pattern, source.ToString(), select: "/*/@*", rootVersion: version, templateVersion: version));
    }
}
