// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies quoted and EQName delimiters in XPath template patterns.
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

/// <summary>Verifies quoted and EQName delimiters in XPath template patterns.</summary>
public class XsltPatternDelimiterTests
{
    private static string Run(string pattern, string version = "4.0", string source = "<root><child/></root>", string priority = "-1.1") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M</xsl:template><xsl:template match='.' priority='" + priority + "'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Ignores delimiter characters and escaped quotes inside predicate strings.</summary>
    /// <param name="pattern">The match pattern.</param>
    [Theory]
    [InlineData("(.[contains(&quot;)&quot;, &quot;)&quot;)])")]
    [InlineData("((.[contains(&quot;(&quot;, &quot;(&quot;)]))")]
    [InlineData(".[contains(&quot;]&quot;, &quot;]&quot;)]")]
    [InlineData(".[contains(&quot;[&quot;, &quot;[&quot;)]")]
    [InlineData("(root)[contains(&quot;]&quot;, &quot;]&quot;)]")]
    [InlineData("(root)[contains(&quot;[&quot;, &quot;[&quot;)]")]
    [InlineData("root[contains(&quot;]&quot;, &quot;]&quot;)]")]
    [InlineData("root[contains(&quot;[&quot;, &quot;[&quot;)]")]
    [InlineData("(root[contains(&quot;)&quot;, &quot;)&quot;)])[true()]")]
    [InlineData("(root)[contains(&apos;a&apos;&apos;]b&apos;, &apos;]&apos;)]")]
    [InlineData("(root)[contains(&quot;a&quot;&quot;]b&quot;, &quot;]&quot;)]")]
    [InlineData("(root)[child[contains(&quot;]&quot;, &quot;]&quot;)]][true()]")]
    public void QuotedDelimiters(string pattern) => Assert.Equal("M", Run(pattern));

    /// <summary>Preserves nonmatches despite quoted opening delimiters.</summary>
    [Fact]
    public void FalseOuterPredicate() => Assert.Equal("F", Run("(root)[contains(&quot;[&quot;, &quot;x&quot;)]"));

    /// <summary>Ignores parentheses inside EQName namespace URIs.</summary>
    /// <param name="pattern">The namespaced pattern.</param>
    [Theory]
    [InlineData("(Q{urn:n)}root)")]
    [InlineData("(Q{urn:n(}root)[true()]")]
    [InlineData("(~element(Q{urn:n)}root))")]
    public void UriParentheses(string pattern) => Assert.Equal("M", Run(pattern, source: pattern.Contains("n(") ? "<root xmlns='urn:n('/>" : "<root xmlns='urn:n)'/>"));

    /// <summary>Computes contained predicate priority despite a quoted closing parenthesis.</summary>
    [Fact]
    public void ContainedPriority() => Assert.Equal("M", Run("(.[contains(&quot;)&quot;, &quot;)&quot;)])", priority: "0.75"));

    /// <summary>Retains earlier-profile grammar while fixing ordinary node-pattern lexical delimiters.</summary>
    /// <param name="pattern">The node pattern.</param>
    [Theory]
    [InlineData("root[contains(&quot;]&quot;, &quot;]&quot;)]")]
    [InlineData("(root)[contains(&quot;[&quot;, &quot;[&quot;)]")]
    public void EarlierNodePatterns(string pattern) => Assert.Equal("M", Run(pattern, "3.0"));
}
