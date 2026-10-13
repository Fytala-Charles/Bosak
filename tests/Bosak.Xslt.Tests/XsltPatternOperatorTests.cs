// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies lexical splitting of XPath template pattern operators.
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

/// <summary>Verifies lexical splitting of XPath template pattern operators.</summary>
public class XsltPatternOperatorTests
{
    private static string Run(string pattern, string version = "4.0", string source = "<root/>") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M</xsl:template><xsl:template match='.' priority='-1.1'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Splits actual operators outside quoted delimiters and operator text.</summary>
    /// <param name="pattern">The match pattern.</param>
    [Theory]
    [InlineData("root[contains(&quot;]&quot;, &quot;]&quot;)]|missing")]
    [InlineData("root[contains(&quot;[&quot;, &quot;[&quot;)] union missing")]
    [InlineData("root[contains(&quot;)&quot;, &quot;)&quot;)] intersect root")]
    [InlineData("root[contains(&quot;(&quot;, &quot;(&quot;)] except missing")]
    [InlineData("root[contains(&quot;]|except&quot;, &quot;]&quot;)]|missing")]
    [InlineData("root[contains(&quot;} union&quot;, &quot;union&quot;)] intersect root")]
    [InlineData("root[contains(&apos;a&apos;&apos;]b&apos;, &apos;]&apos;)] except missing")]
    [InlineData("(root[contains(&quot;]&quot;, &quot;]&quot;)]|missing) intersect root")]
    [InlineData("root[true()[contains(&quot;]&quot;, &quot;]&quot;)]]|missing")]
    [InlineData("root (: operator comment :) union missing")]
    public void QuotedOperators(string pattern) => Assert.Equal("M", Run(pattern));

    /// <summary>Retains set membership and predicate nonmatches after lexical splitting.</summary>
    /// <param name="pattern">The nonmatching pattern.</param>
    [Theory]
    [InlineData("root[contains(&quot;]&quot;, &quot;x&quot;)]|missing")]
    [InlineData("root[contains(&quot;)&quot;, &quot;)&quot;)] except root")]
    [InlineData("root[contains(&quot;[&quot;, &quot;[&quot;)] intersect missing")]
    public void Nonmatches(string pattern) => Assert.Equal("F", Run(pattern));

    /// <summary>Ignores delimiter and operator punctuation in EQName URIs.</summary>
    /// <param name="pattern">The EQName pattern.</param>
    /// <param name="source">The matching source tree.</param>
    [Theory]
    [InlineData("Q{urn:n)}root|missing", "<root xmlns='urn:n)'/>")]
    [InlineData("Q{urn:n(}root union missing", "<root xmlns='urn:n('/>")]
    [InlineData("Q{urn:except|union}root except missing", "<root xmlns='urn:except|union'/>")]
    public void UriOperators(string pattern, string source) => Assert.Equal("M", Run(pattern, source: source));

    /// <summary>Does not split operator-like words inside element names.</summary>
    /// <param name="name">The element name.</param>
    [Theory]
    [InlineData("pre-except-post")]
    [InlineData("pre_intersect_post")]
    [InlineData("pre.union.post")]
    [InlineData("union")]
    [InlineData("except")]
    [InlineData("intersect")]
    public void OperatorNames(string name) => Assert.Equal("M", Run(name, source: "<" + name + "/>"));

    /// <summary>Preserves earlier-profile lexical splitting without changing priority rules.</summary>
    [Fact]
    public void EarlierUnion() => Assert.Equal("M", Run("root[contains(&quot;]&quot;, &quot;]&quot;)]|missing", "3.0"));
}
