// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 October 2026
// PURPOSE              : Verifies EQName brackets remain namespace content in XSLT match patterns.
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

/// <summary>Verifies EQName brackets remain namespace content in XSLT match patterns.</summary>
public class EqNameBracketPatternTests
{
    /// <summary>Exercises matching and real predicates independently of brackets within namespace URIs.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    /// <param name="pattern">The complete match pattern.</param>
    /// <param name="select">The source selection.</param>
    /// <param name="expected">The expected template output.</param>
    [Theory]
    [InlineData("3.0", "Q{urn:n[part]}root", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part}root", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[true()]", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[false()]", "/*", "fallback")]
    [InlineData("3.0", "@Q{urn:n[part]}code", "/*/@*", "named")]
    [InlineData("3.0", "@Q{urn:n[part]}code[true()]", "/*/@*", "named")]
    [InlineData("3.0", "@Q{urn:n[part]}code[false()]", "/*/@*", "fallback")]
    [InlineData("3.0", "child::Q{urn:n[part]}root", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root/Q{urn:n[part]}child", "/*/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[true()]/Q{urn:n[part]}child[true()]", "/*/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[false()]/Q{urn:n[part]}child", "/*/*", "fallback")]
    [InlineData("3.0", "Q{urn:n[part]}root | missing", "/*", "named")]
    [InlineData("3.0", "(Q{urn:n[part]}root)[true()]", "/*", "named")]
    [InlineData("3.0", "element(Q{urn:n[part]}root)", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}*", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}missing", "/*", "fallback")]
    [InlineData("4.0", "Q{urn:n[part]}root", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part}root", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[true()]", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[false()]", "/*", "fallback")]
    [InlineData("4.0", "@Q{urn:n[part]}code", "/*/@*", "named")]
    [InlineData("4.0", "@Q{urn:n[part]}code[true()]", "/*/@*", "named")]
    [InlineData("4.0", "@Q{urn:n[part]}code[false()]", "/*/@*", "fallback")]
    [InlineData("4.0", "child::Q{urn:n[part]}root", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root/Q{urn:n[part]}child", "/*/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[true()]/Q{urn:n[part]}child[true()]", "/*/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[false()]/Q{urn:n[part]}child", "/*/*", "fallback")]
    [InlineData("4.0", "Q{urn:n[part]}root | missing", "/*", "named")]
    [InlineData("4.0", "(Q{urn:n[part]}root)[true()]", "/*", "named")]
    [InlineData("4.0", "element(Q{urn:n[part]}root)", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}*", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}missing", "/*", "fallback")]
    [InlineData("3.0", "attribute::Q{urn:n[part]}code", "/*/@*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root/Q{urn:n[part]}child[false()]", "/*/*", "fallback")]
    [InlineData("3.0", "Q{urn:n[part]}root[Q{urn:n[part]}child]", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[@Q{urn:n[part]}code = \"A\"]", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[1][true()]", "/*", "named")]
    [InlineData("3.0", "Q{urn:n[part]}root[2]", "/*", "fallback")]
    [InlineData("4.0", "attribute::Q{urn:n[part]}code", "/*/@*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root/Q{urn:n[part]}child[false()]", "/*/*", "fallback")]
    [InlineData("4.0", "Q{urn:n[part]}root[Q{urn:n[part]}child]", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[@Q{urn:n[part]}code = \"A\"]", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[1][true()]", "/*", "named")]
    [InlineData("4.0", "Q{urn:n[part]}root[2]", "/*", "fallback")]
    public void NamespaceBrackets(string version, string pattern, string select, string expected)
    {
        string uri = pattern.Contains("urn:n[part}") ? "urn:n[part" : "urn:n[part]";
        XNamespace ns = uri;
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var source = new XElement(ns + "root", new XAttribute(XNamespace.Xmlns + "n", uri),
            new XAttribute(ns + "code", "A"), new XElement(ns + "child"));
        var stylesheet = new XElement(xsl + "stylesheet", new XAttribute("version", version),
            new XAttribute(XNamespace.Xmlns + "xsl", xsl.NamespaceName),
            new XElement(xsl + "template", new XAttribute("match", "/"), new XAttribute("priority", "10"),
                new XElement("out", new XElement(xsl + "apply-templates", new XAttribute("select", select)))),
            new XElement(xsl + "template", new XAttribute("match", pattern), new XAttribute("priority", "2"), "named"),
            new XElement(xsl + "template", new XAttribute("match", "."), new XAttribute("priority", "1"), "fallback"));
        var compiled = new Api.XsltCompiler().Compile(stylesheet.ToString());
        Assert.Equal(expected, XElement.Parse(compiled.TransformToString(new XDocumentNode(new XDocument(source)))).Value);
    }
}
