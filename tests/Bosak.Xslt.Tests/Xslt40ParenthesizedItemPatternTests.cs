// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies XSLT 4.0 parenthesized item-pattern matching and declaring scopes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies XSLT 4.0 parenthesized item-pattern matching and declaring scopes.</summary>
public class Xslt40ParenthesizedItemPatternTests
{
    private static string Wrap(string body, string version = "4.0") => "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' exclude-result-prefixes='#all'>" + body + "</xsl:stylesheet>";
    private static string Run(string pattern, string select = "/*", string declarations = "", string priority = "-1.1", string extra = "", string source = "<root><child/></root>") => XElement.Parse(
        new Api.XsltCompiler().Compile(Wrap(declarations + "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "' " + extra + ">matched</xsl:template><xsl:template match='.' priority='" + priority + "'>fallback</xsl:template>"))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Matches nodes through nested parenthesized predicate and type patterns.</summary>
    /// <param name="pattern">The match pattern.</param>
    [Theory]
    [InlineData("(.[true()])")]
    [InlineData("((.[true()]))")]
    [InlineData("(. [child/..])")]
    [InlineData("(.[contains(\"a/b\", \"/\")])")]
    [InlineData("(~element(root))")]
    [InlineData("((~element(root)))")]
    [InlineData("(~item())")]
    [InlineData("(.[true()])|root")]
    [InlineData("(~element(root))|missing")]
    [InlineData("(.[true()])[child]")]
    [InlineData("(~element(root))[child]")]
    public void MatchesNodes(string pattern) => Assert.Equal("matched", Run(pattern));

    /// <summary>Restricts a parenthesized XNode pattern even when its contained pattern accepts non-nodes.</summary>
    /// <param name="pattern">The parenthesized pattern.</param>
    /// <param name="select">The non-node input.</param>
    [Theory]
    [InlineData("(.)", "42")]
    [InlineData("(.[true()])", "42")]
    [InlineData("((.[true()]))", "42")]
    [InlineData("(~xs:integer)", "42")]
    [InlineData("(~map(*))", "map{}")]
    [InlineData("(.[true()])|root", "42")]
    [InlineData("(~record(x as xs:integer))", "map{&quot;x&quot;:7}")]
    public void NonNodesDoNotMatch(string pattern, string select) => Assert.Equal("fallback", Run(pattern, select));

    /// <summary>Resolves aliases inside a parenthesized type pattern and retains its contained priority.</summary>
    /// <param name="priority">The competing priority.</param>
    /// <param name="expected">The expected winning marker.</param>
    [Theory]
    [InlineData("-0.1", "matched")]
    [InlineData("0.1", "fallback")]
    public void AliasPriority(string priority, string expected) => Assert.Equal(expected, Run("(~t:root)", declarations: "<xsl:item-type name='t:root' as='element(root)'/>", priority: priority));

    /// <summary>Retains aliases in an enclosed predicate expression.</summary>
    [Fact]
    public void PredicateAlias() => Assert.Equal("matched", Run("(.[. instance of t:root])", declarations: "<xsl:item-type name='t:root' as='element(root)'/>"));

    /// <summary>Retains default element namespaces during nested compilation.</summary>
    [Fact]
    public void DefaultNamespace() => Assert.Equal("matched", Run("(~element(root))", select: "/*", extra: "xpath-default-namespace='urn:n'", source: "<root xmlns='urn:n'/>") );

    /// <summary>Preserves predicate nonmatches.</summary>
    [Fact]
    public void FalsePredicate() => Assert.Equal("fallback", Run("(.[false()])"));

    /// <summary>Retains the earlier-profile rejection of parenthesized predicate patterns.</summary>
    [Fact]
    public void EarlierProfileRejectsPredicate() => Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("<xsl:template match='(.[true()])'/>", "3.0"))).Message);

    /// <summary>Rejects unparenthesized item patterns as union operands.</summary>
    /// <param name="pattern">The invalid union.</param>
    [Theory]
    [InlineData(".[true()]|root")]
    [InlineData("~item()|root")]
    public void DirectItemUnionRejected(string pattern) => Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => Run(pattern)).Message);

    /// <summary>Reports undeclared aliases inside nested patterns.</summary>
    [Fact]
    public void UnknownAlias() => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Run("(~t:missing)")).Message);
}
