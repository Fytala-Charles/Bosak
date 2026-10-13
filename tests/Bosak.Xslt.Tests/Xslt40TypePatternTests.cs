// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies matching, focus and priority of XSLT 4.0 template type patterns.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Keep the quoted record-field regression on a standalone item pattern.
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies matching, focus and priority of XSLT 4.0 template type patterns.</summary>
public class Xslt40TypePatternTests
{
    private static string Wrap(string body, string extra = "", string version = "4.0") =>
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' exclude-result-prefixes='#all' " + extra + ">" + body + "</xsl:stylesheet>";
    private static string Run(string body, string extra = "", string version = "4.0") => XElement.Parse(
        new Api.XsltCompiler().Compile(Wrap(body, extra, version)).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Matches record aliases and selects the filtered rule by default priority.</summary>
    [Fact]
    public void AnnotatedRecordAliases()
    {
        Assert.Equal("generalzero", Run("""
            <xsl:item-type name='t:complex' as='record(r as xs:double,i as xs:double)'/>
            <xsl:template match='/'><out><xsl:variable name='a' as='t:complex' select="map{'r':1,'i':2}"/>
              <xsl:variable name='b' as='t:complex' select="map{'r':3,'i':0}"/><xsl:apply-templates select='($a,$b)'/>
            </out></xsl:template>
            <xsl:template match='~t:complex[?i=0]'>zero</xsl:template>
            <xsl:template match='~t:complex'>general</xsl:template>
            """));
    }

    /// <summary>Matches nominative record types while preserving record identity.</summary>
    [Fact]
    public void NominativeRecordPattern()
    {
        Assert.Equal("pointrecord", Run("""
            <xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
            <xsl:record-type name='t:other' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
            <xsl:template match='/'><out><xsl:apply-templates select='(t:point(7),t:other(7))'/></out></xsl:template>
            <xsl:template match='~t:point'>point</xsl:template>
            <xsl:template match='~record(*)' priority='-0.1'>record</xsl:template>
            """));
    }

    /// <summary>Separates predicates from EQName namespace URIs containing brackets.</summary>
    [Fact]
    public void EqNameDelimiter()
    {
        Assert.Equal("matched", Run("""
            <xsl:template match='/'><out><xsl:variable name='n' as='element()'><root xmlns='urn:a[b]'/></xsl:variable>
              <xsl:apply-templates select='$n'/></out></xsl:template>
            <xsl:template match='~element(Q{urn:a[b]}root)[true()]'>matched</xsl:template>
            """));
    }

    /// <summary>Matches structural records without admitting ordinary unannotated maps.</summary>
    [Fact]
    public void StructuralRecordsKeepAnnotationBoundary()
    {
        Assert.Equal("recordmap", Run("""
            <xsl:template match='/'><out><xsl:variable name='r' as='record(x as xs:double)' select="map{'x':1}"/>
              <xsl:apply-templates select="($r,map{'x':1})"/></out></xsl:template>
            <xsl:template match='~record(x as xs:double)'>record</xsl:template>
            <xsl:template match='~map(*)'>map</xsl:template>
            """));
    }

    /// <summary>Uses predicate-pattern numeric and singleton-focus semantics.</summary>
    /// <param name="predicate">The predicate text.</param>
    /// <param name="expected">The expected matching result.</param>
    [Theory]
    [InlineData("1", "match")]
    [InlineData("2", "fallback")]
    [InlineData("position()=1 and last()=1", "match")]
    [InlineData(". eq 7", "match")]
    [InlineData("error()", "fallback")]
    [InlineData("1][. eq 7", "match")]
    public void PredicateSemantics(string predicate, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='7'/></out></xsl:template>" +
            "<xsl:template match='~xs:integer[" + predicate + "]'>match</xsl:template><xsl:template match='~item()'>fallback</xsl:template>"));
    }

    /// <summary>Computes family and derived-atomic priorities independently of declaration order.</summary>
    /// <param name="specific">The more specific type.</param>
    /// <param name="general">The lower-priority type.</param>
    /// <param name="select">An item matched by both types.</param>
    [Theory]
    [InlineData("xs:integer", "xs:decimal", "7")]
    [InlineData("xs:int", "xs:long", "xs:int(7)")]
    [InlineData("xs:dayTimeDuration", "xs:duration", "xs:dayTimeDuration('P1D')")]
    [InlineData("xs:string", "xs:anyAtomicType", "'a'")]
    [InlineData("map(*)", "function(*)", "map{}")]
    [InlineData("array(*)", "item()", "[1]")]
    [InlineData("element(*)", "node()", "/*")]
    [InlineData("node()", "item()", "/*")]
    [InlineData("enum('a','b')", "xs:string", "'a'")]
    [InlineData("(xs:string|xs:integer)", "xs:decimal", "7")]
    public void TypePriorities(string specific, string general, string select)
    {
        var specificXml = specific.Replace("'", "&apos;");
        Assert.Equal("specific", Run("<xsl:template match='/'><out><xsl:apply-templates select=\"" + select + "\"/></out></xsl:template>" +
            "<xsl:template match='~" + specificXml + "'>specific</xsl:template><xsl:template match='~" + general + "'>general</xsl:template>"));
    }

    /// <summary>Resolves named type priorities and fixed namespace bindings despite local rebinding.</summary>
    [Fact]
    public void AliasAndFixedNamespaces()
    {
        Assert.Equal("specific", Run("""
            <xsl:item-type name='t:small' as='xs:int'/>
            <xsl:template match='/'><out><xsl:apply-templates select='xs:int(7)'/></out></xsl:template>
            <xsl:template xmlns:t='urn:wrong' match='~t:small'>specific</xsl:template>
            <xsl:template match='~xs:integer'>general</xsl:template>
            """, "fixed-namespaces='#standard t=urn:types'"));
    }

    /// <summary>Matches standalone type patterns and record annotations with predicate delimiters in field names.</summary>
    [Fact]
    public void QuotedFields()
    {
        Assert.Equal("matched", Run("""
            <xsl:template match='/'><out><xsl:variable name='r' as="record('x[y]' as xs:integer)" select="map{'x[y]':7}"/>
              <xsl:apply-templates select='$r'/></out></xsl:template>
            <xsl:template match="~record('x[y]' as xs:integer)[?('x[y]')=7]">matched</xsl:template>
            """));
    }

    /// <summary>Preserves explicit template priorities.</summary>
    [Fact]
    public void ExplicitPriority()
    {
        Assert.Equal("explicit", Run("""
            <xsl:template match='/' priority='10'><out><xsl:apply-templates select='7'/></out></xsl:template>
            <xsl:template match='~xs:integer'>derived</xsl:template>
            <xsl:template match='~item()' priority='0.4'>explicit</xsl:template>
            """));
    }

    /// <summary>Rejects non-item grammar, malformed patterns and the previous compatibility profile.</summary>
    /// <param name="pattern">The invalid pattern.</param>
    /// <param name="version">The owning XSLT version.</param>
    [Theory]
    [InlineData("~empty-sequence()", "4.0")]
    [InlineData("~xs:integer?", "4.0")]
    [InlineData("~xs:integer or true()", "4.0")]
    [InlineData("~record(x as xs:integer)[true()]/child::x", "4.0")]
    [InlineData("~xs:integer", "3.0")]
    public void InvalidPatterns(string pattern, string version)
    {
        Assert.ThrowsAny<Exception>(() => Run("<xsl:template match='/'><out><xsl:apply-templates select='7'/></out></xsl:template><xsl:template match='" + pattern + "'>bad</xsl:template>", version: version));
    }
}
