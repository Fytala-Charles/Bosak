// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies wildcard node kind-test matching and profile-specific default priorities.
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

/// <summary>Verifies wildcard node kind-test matching and profile-specific default priorities.</summary>
public class Xslt40WildcardNodePatternTests
{
    private const string Source = "<n:root xmlns:n='urn:n' n:code='7' code='8'><n:child/><other/></n:root>";
    private static string Run(string body, string source = Source, string version = "4.0") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:n='urn:n' exclude-result-prefixes='#all'>" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Matches namespace and local wildcards in standalone, axis and path kind tests.</summary>
    /// <param name="pattern">The match pattern.</param>
    /// <param name="select">The input nodes.</param>
    /// <param name="expected">The expected matching markers.</param>
    [Theory]
    [InlineData("element(n:*)", "/*/*", "MF")]
    [InlineData("element(*:child)", "/*/*", "MF")]
    [InlineData("element(Q{urn:n}*)", "/*/*", "MF")]
    [InlineData("element(n:*, xs:untyped)", "/*/*", "MF")]
    [InlineData("element(*:child, xs:untyped)", "/*/*", "MF")]
    [InlineData("attribute(n:*)", "/*/@*", "MF")]
    [InlineData("attribute(*:code)", "/*/@*", "MM")]
    [InlineData("attribute(Q{urn:n}*, xs:untypedAtomic)", "/*/@*", "MF")]
    [InlineData("attribute(*:code, xs:untypedAtomic)", "/*/@*", "MM")]
    [InlineData("child::element(n:*)", "/*/*", "MF")]
    [InlineData("attribute::attribute(n:*)", "/*/@*", "MF")]
    [InlineData("n:root/element(n:*)", "/*/*", "MF")]
    [InlineData("n:root/attribute(n:*)", "/*/@*", "MF")]
    [InlineData("element(n:root|other)", "/*/*", "FM")]
    [InlineData("element(n:*|other)", "/*/*", "MM")]
    [InlineData("attribute(n:missing|code)", "/*/@*", "FM")]
    [InlineData("element(Q{}*)", "/*/*", "FM")]
    [InlineData("attribute(Q{}*)", "/*/@*", "FM")]
    public void WildcardMatching(string pattern, string select, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/' priority='10'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M</xsl:template><xsl:template match='element()|attribute()' priority='-2'>F</xsl:template>"));
    }

    /// <summary>Preserves schema type checking alongside name wildcards.</summary>
    [Fact]
    public void WrongTypeDoesNotMatch()
    {
        Assert.Equal("F", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*/@n:code'/></out></xsl:template>
            <xsl:template match='attribute(n:*, xs:integer)'>M</xsl:template>
            <xsl:template match='attribute()' priority='-2'>F</xsl:template>
            """));
    }

    /// <summary>Verifies calculated priorities against explicit intermediate competitors.</summary>
    /// <param name="pattern">The defaulted pattern.</param>
    /// <param name="select">A matching input node.</param>
    /// <param name="competitor">The competing explicit priority.</param>
    /// <param name="expected">The winning template marker.</param>
    [Theory]
    [InlineData("element(n:*)", "/*", "-0.3", "default")]
    [InlineData("child::element(n:*)[true()]", "/*", "0.4", "default")]
    [InlineData("attribute::attribute(n:*)[true()]", "/*/@n:code", "0.4", "default")]
    [InlineData("element(n:*)", "/*", "-0.2", "explicit")]
    [InlineData("element(*, xs:untyped)", "/*", "0.1", "explicit")]
    [InlineData("element(n:*, xs:untyped)", "/*", "0.1", "default")]
    [InlineData("element(*:root, xs:untyped)", "/*", "0.2", "explicit")]
    [InlineData("element(n:root, xs:untyped)", "/*", "0.2", "default")]
    [InlineData("attribute(n:*)", "/*/@n:code", "-0.2", "explicit")]
    [InlineData("attribute(*, xs:untypedAtomic)", "/*/@n:code", "0.1", "explicit")]
    [InlineData("attribute(*:code, xs:untypedAtomic)", "/*/@n:code", "0.1", "default")]
    [InlineData("element(n:*|*:missing, xs:untyped)", "/*", "0.2", "explicit")]
    [InlineData("element(n:*|n:root, xs:untyped)", "/*", "0.2", "default")]
    [InlineData("~element(n:*, xs:untyped)", "/*", "0.2", "explicit")]
    [InlineData("~attribute(*, xs:untypedAtomic)", "/*/@n:code", "0.1", "explicit")]
    [InlineData("document-node(element(n:*, xs:untyped))", ".", "0.1", "default")]
    public void DefaultPriority(string pattern, string select, string competitor, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/' priority='10'><out><xsl:apply-templates select='" + select + "' mode='test'/></out></xsl:template>" +
            "<xsl:template mode='test' match='" + pattern + "'>default</xsl:template><xsl:template mode='test' match='node()|attribute()' priority='" + competitor + "'>explicit</xsl:template>"));
    }

    /// <summary>Retains the earlier profile priority for a type-only wildcard.</summary>
    /// <param name="version">The owning XSLT version.</param>
    /// <param name="expected">The winning template marker.</param>
    [Theory]
    [InlineData("3.0", "typed")]
    [InlineData("4.0", "explicit")]
    public void OwningProfile(string version, string expected)
    {
        Assert.Equal(expected, Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match='element(*, xs:untyped)'>typed</xsl:template>
            <xsl:template match='*' priority='0.1'>explicit</xsl:template>
            """, version: version));
    }

    /// <summary>Rejects malformed wildcard and union-name syntax.</summary>
    /// <param name="pattern">The malformed match pattern.</param>
    [Theory]
    [InlineData("element(n:**)")]
    [InlineData("attribute(*:*)")]
    [InlineData("element(n:*|)")]
    public void InvalidNameTests(string pattern)
    {
        Assert.ThrowsAny<Exception>(() => Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template><xsl:template match='" + pattern + "'>bad</xsl:template>"));
    }

    /// <summary>Handles delimiter characters inside EQName namespace URIs.</summary>
    [Fact]
    public void EqNameNamespaceDelimiters()
    {
        Assert.Equal("default", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match='element(Q{urn:n,part|extra}*, xs:untyped)'>default</xsl:template>
            <xsl:template match='*' priority='0.1'>explicit</xsl:template>
            """, "<root xmlns='urn:n,part|extra'/>") );
    }
}
