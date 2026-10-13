// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies XSLT 4.0 attribute and forward-axis pattern default priorities.
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

/// <summary>Verifies XSLT 4.0 attribute and forward-axis pattern default priorities.</summary>
public class Xslt40ForwardAxisPriorityTests
{
    private const string Source = "<root xmlns:n='urn:n' code='7' n:code='8'><n:child/><?target data?><?other data?></root>";
    private static string Run(string body, string version = "4.0", string source = Source) => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:n='urn:n' exclude-result-prefixes='#all'>" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Compares defaulted attribute and axis priorities with explicit competing priorities.</summary>
    /// <param name="pattern">The supported match pattern.</param>
    /// <param name="select">The selected input node.</param>
    /// <param name="priority">The competitor's explicit priority.</param>
    /// <param name="expected">The expected winning marker.</param>
    [Theory]
    [InlineData("@code", "/*/@code", "0.1", "explicit")]
    [InlineData("@n:code", "/*/@n:code", "-0.1", "default")]
    [InlineData("@Q{}code", "/*/@code", "0.1", "explicit")]
    [InlineData("attribute::code", "/*/@code", "0.1", "explicit")]
    [InlineData("attribute::n:code", "/*/@n:code", "-0.1", "default")]
    [InlineData("child::root", "/*", "0.1", "explicit")]
    [InlineData("self::root", "/*", "0.1", "explicit")]
    [InlineData("self::root", "/*", "-0.1", "default")]
    [InlineData("descendant::root", "/*", "0.1", "explicit")]
    [InlineData("descendant-or-self::root", "/*", "-0.1", "default")]
    [InlineData("self::element(*)", "/*", "-0.4", "explicit")]
    [InlineData("self::element(*, xs:untyped)", "/*", "0.1", "explicit")]
    [InlineData("self::element(root, xs:untyped)", "/*", "0.1", "default")]
    [InlineData("@n:*", "/*/@n:code", "-0.2", "explicit")]
    [InlineData("@*:code", "/*/@code", "-0.3", "default")]
    [InlineData("attribute::n:*", "/*/@n:code", "-0.2", "explicit")]
    [InlineData("self::*", "/*", "-0.4", "explicit")]
    [InlineData("descendant::n:*", "/*/*", "-0.3", "default")]
    [InlineData("child::processing-instruction('target')", "/*/processing-instruction('target')", "-0.1", "default")]
    [InlineData("self::processing-instruction(target)", "/*/processing-instruction('target')", "0.1", "explicit")]
    [InlineData("descendant-or-self::processing-instruction(target)", "/*/processing-instruction('target')", "-0.1", "default")]
    [InlineData("child::processing-instruction()", "/*/processing-instruction('target')", "-0.4", "explicit")]
    [InlineData("@code[true()]", "/*/@code", "0.25", "default")]
    [InlineData("attribute::code[true()]", "/*/@code", "0.25", "default")]
    [InlineData("self::root[true()]", "/*", "0.25", "default")]
    public void SupportedPriorities(string pattern, string select, string priority, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select=\"" + select + "\" mode='test'/></out></xsl:template>" +
            "<xsl:template mode='test' match=\"" + pattern + "\">default</xsl:template><xsl:template mode='test' match='node()|attribute()' priority='" + priority + "'>explicit</xsl:template>"));
    }

    /// <summary>Preserves older profile priorities for attribute shorthand and self-axis names.</summary>
    /// <param name="version">The effective profile.</param>
    /// <param name="pattern">The named node pattern.</param>
    /// <param name="select">The selected node.</param>
    /// <param name="expected">The winning template.</param>
    [Theory]
    [InlineData("3.0", "@code", "/*/@code", "default")]
    [InlineData("4.0", "@code", "/*/@code", "explicit")]
    [InlineData("3.0", "self::root", "/*", "default")]
    [InlineData("4.0", "self::root", "/*", "explicit")]
    public void EarlierProfile(string version, string pattern, string select, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='" + select + "' mode='test'/></out></xsl:template><xsl:template match='" + pattern + "' mode='test'>default</xsl:template><xsl:template match='node()|attribute()' mode='test' priority='0.1'>explicit</xsl:template>", version));
    }

    /// <summary>Dispatches a union of a named attribute and a wildcard as one rule with priority zero.</summary>
    [Fact]
    public void AttributeUnionNextMatch()
    {
        Assert.Equal("explicituniontail", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*/@code'/></out></xsl:template>
            <xsl:template match='@*' priority='-2'>tail</xsl:template>
            <xsl:template match='@code|@*'>union<xsl:next-match/></xsl:template>
            <xsl:template match='@*' priority='0.1'>explicit<xsl:next-match/></xsl:template>
            """));
    }

    /// <summary>Does not interpret axis separators inside EQName URIs.</summary>
    /// <param name="priority">The explicit competing priority.</param>
    /// <param name="expected">The winning template marker.</param>
    [Theory]
    [InlineData("0.1", "explicit")]
    [InlineData("-0.1", "default")]
    public void EqNameUriAxisPunctuation(string priority, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='Q{urn::n}root'>default</xsl:template><xsl:template match='*' priority='" + priority + "'>explicit</xsl:template>", source: "<root xmlns='urn::n'/>") );
    }

    /// <summary>Does not match a named processing-instruction axis test against another target.</summary>
    [Fact]
    public void ProcessingInstructionTargetBoundary()
    {
        Assert.Equal("other", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select="/*/processing-instruction('other')"/></out></xsl:template>
            <xsl:template match='child::processing-instruction(target)'>wrong</xsl:template>
            <xsl:template match='processing-instruction()'>other</xsl:template>
            """));
    }
}
