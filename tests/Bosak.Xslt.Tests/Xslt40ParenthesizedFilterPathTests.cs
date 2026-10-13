// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies path classification and focus of XSLT 4.0 parenthesized outer filters.
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

/// <summary>Verifies path classification and focus of XSLT 4.0 parenthesized outer filters.</summary>
public class Xslt40ParenthesizedFilterPathTests
{
    private static string Run(string pattern, string source = "<root><child/></root>", string select = "/*", string declarations = "", string version = "4.0") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:types' exclude-result-prefixes='#all'>" + declarations +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M<xsl:value-of select='@id'/></xsl:template><xsl:template match='.' priority='-1.1'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Evaluates outer filters on the candidate when contained slashes are not path operators.</summary>
    /// <param name="pattern">The parenthesized pattern and outer filter.</param>
    [Theory]
    [InlineData("(.[child/..])[true()]")]
    [InlineData("((.[child/..]))[true()]")]
    [InlineData("(.[contains(\"a/b\", \"/\")])[true()]")]
    [InlineData("(~element(root)[child/..])[true()]")]
    [InlineData("(root[child/..])[true()]")]
    [InlineData("(root[contains(\"a/b\", \"/\")])[true()]")]
    [InlineData("(root[child/..])[1]")]
    [InlineData("(root[child[../child]])[true()]")]
    [InlineData("(root[contains(&apos;a&apos;&apos;/b&apos;, &apos;/&apos;)])[true()]")]
    [InlineData("(root[child/..])[position()=last()]")]
    [InlineData("(root[child/..])[true()][true()]")]
    public void CandidateFilter(string pattern) => Assert.Equal("M", Run(pattern));

    /// <summary>Ignores slash punctuation inside an EQName namespace URI.</summary>
    /// <param name="pattern">The namespaced pattern.</param>
    [Theory]
    [InlineData("(Q{https://example.test/n}root)[true()]")]
    [InlineData("(~element(Q{https://example.test/n}root))[true()]")]
    [InlineData("((Q{https://example.test/n}root))[true()]")]
    public void UriSlashes(string pattern) => Assert.Equal("M", Run(pattern, "<root xmlns='https://example.test/n'/>"));

    /// <summary>Preserves position-based filtering of the sequence selected by a real path.</summary>
    /// <param name="pattern">The path pattern.</param>
    [Theory]
    [InlineData("(root/child)[2]")]
    [InlineData("((root/child))[2]")]
    [InlineData("(root/child[contains(\"a/b\", \"/\")])[2]")]
    [InlineData("(root/child|root/other)[2]")]
    public void RealPathPositions(string pattern) => Assert.Equal("FM2F", Run(pattern, "<root><child id='1'/><child id='2'/><child id='3'/></root>", "/*/*"));

    /// <summary>Recognizes a real path even when its EQName URIs also contain slashes.</summary>
    [Fact]
    public void NamespacedPathPositions() => Assert.Equal("FM2F", Run("(Q{https://example.test/n}root/Q{https://example.test/n}child)[2]", "<root xmlns='https://example.test/n'><child id='1'/><child id='2'/><child id='3'/></root>", "/*/*"));

    /// <summary>Retains descendant path operators and positional filtering.</summary>
    [Fact]
    public void DescendantPathPositions() => Assert.Equal("FM2F", Run("(root//child)[2]", "<root><child id='1'/><nest><child id='2'/><child id='3'/></nest></root>", "/*//child"));

    /// <summary>Preserves inner and outer nonmatches after classification.</summary>
    /// <param name="pattern">The nonmatching pattern.</param>
    [Theory]
    [InlineData("(root[child/..])[false()]")]
    [InlineData("(missing[child/..])[true()]")]
    [InlineData("(root[child/..])[2]")]
    public void Nonmatches(string pattern) => Assert.Equal("F", Run(pattern));

    /// <summary>Retains declaring type aliases in a contained predicate with a path.</summary>
    [Fact]
    public void AliasInPredicate() => Assert.Equal("M", Run("(.[. instance of t:root][child/..])[true()]", declarations: "<xsl:item-type name='t:root' as='element(root)'/>"));

    /// <summary>Retains earlier-profile real-path positional filtering.</summary>
    [Fact]
    public void EarlierPathPositions() => Assert.Equal("FM2F", Run("(root/child)[2]", "<root><child id='1'/><child id='2'/><child id='3'/></root>", "/*/*", version: "3.0"));
}
