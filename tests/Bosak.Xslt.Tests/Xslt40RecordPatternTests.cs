// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies named record patterns, identity, predicates and entry sequences.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named record patterns, identity, predicates and entry sequences.</summary>
public class Xslt40RecordPatternTests
{
    private const string Types = """
        <xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
        <xsl:record-type name='t:other' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
        <xsl:item-type name='t:alias' as='t:point'/>
        <xsl:item-type name='t:shape' as='record(x as xs:integer)'/>
        <xsl:item-type name='t:any-record' as='record(*)'/>
        """;

    private static string Run(string pattern, string value, string declarations = "", string extra = "", string version = "4.0")
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var body = declarations + Types + new XElement(xsl + "template", new XAttribute("match", "/"), new XElement("out",
            new XElement(xsl + "apply-templates", new XAttribute("select", value)))) +
            new XElement(xsl + "template", new XAttribute("match", pattern), "match") +
            "<xsl:template match='.'>fallback</xsl:template>";
        return XElement.Parse(new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version +
            "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' exclude-result-prefixes='#all' " + extra + ">" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
    }

    /// <summary>Matches named records and aliases without constructing or coercing values.</summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="value">The selected value.</param>
    /// <param name="expected">The selected rule.</param>
    [Theory]
    [InlineData("record(t:point)", "t:point(7)", "match")]
    [InlineData("record(t:point)", "t:other(7)", "fallback")]
    [InlineData("record(t:alias)", "t:point(7)", "match")]
    [InlineData("record(t:alias)", "t:other(7)", "fallback")]
    [InlineData("record(t:shape)", "t:point(7)", "match")]
    [InlineData("record(t:shape)", "t:other(7)", "match")]
    [InlineData("record(t:any-record)", "t:point(7)", "match")]
    [InlineData("record(t:point)", "map{'x':7}", "fallback")]
    [InlineData("record(t:shape)", "map{'x':7}", "fallback")]
    [InlineData("record(t:any-record)", "map{}", "fallback")]
    [InlineData("record(t:point)", "7", "fallback")]
    [InlineData("record(t:point)", "/*", "fallback")]
    [InlineData("record(t:point)", "[t:point(7)]", "fallback")]
    [InlineData("record(t:point)[?x=7 and position()=1 and last()=1 and current()?x=7]", "t:point(7)", "match")]
    [InlineData("record (:gap:) ( t:point )[2]", "t:point(7)", "fallback")]
    [InlineData("record(t:point)[error()]", "t:point(7)", "fallback")]
    [InlineData("record(t:point)[?x=8]", "t:point(7)", "fallback")]
    [InlineData("{'v':record(t:point)+}", "map{'v':(t:point(1),t:point(2))}", "match")]
    [InlineData("{'v':record(t:point)+}", "map{'v':(t:point(1),t:other(2))}", "fallback")]
    [InlineData("{'v':record(t:point)+}", "map{'v':()}", "fallback")]
    [InlineData("{'v':record(t:point)?}", "map{'v':()}", "match")]
    [InlineData("{'v':record(t:shape)*}", "map{'v':(t:point(1),t:other(2))}", "match")]
    [InlineData("{'v':record(t:point)?}", "map{}", "fallback")]
    [InlineData("(record(t:point))", "t:point(7)", "fallback")]
    public void Matching(string pattern, string value, string expected) => Assert.Equal(expected, Run(pattern, value));

    /// <summary>Resolves default type namespaces, EQNames and alias chains.</summary>
    [Fact]
    public void TypeNames()
    {
        Assert.Equal("match", Run("record(point)", "t:point(7)", extra: "xpath-default-namespace='urn:types'"));
        Assert.Equal("match", Run("record(Q{urn:types}point)", "t:point(7)"));
        Assert.Equal("match", Run("record(t:again)", "t:point(7)", "<xsl:item-type name='t:again' as='t:alias'/>"));
        Assert.Equal("match", Run("record(Q{urn:a|b[|]}shape)", "t:point(7)", "<xsl:item-type name='Q{urn:a|b[|]}shape' as='record(x as xs:integer)'/>"));
    }

    /// <summary>Uses bare priority zero and filtered priority one half.</summary>
    /// <param name="pattern">The record pattern.</param>
    /// <param name="below">A priority just below its default.</param>
    /// <param name="above">A priority just above its default.</param>
    [Theory]
    [InlineData("record(t:point)", "-0.01", "0.01")]
    [InlineData("record(t:shape)", "-0.01", "0.01")]
    [InlineData("record(t:point)[true()]", "0.49", "0.51")]
    public void Priorities(string pattern, string below, string above)
    {
        string Other(string priority) => "<xsl:template match='.[. instance of map(*)]' priority='"+priority+"'>other</xsl:template>";
        Assert.Equal("match", Run(pattern, "t:point(7)", Other(below)));
        Assert.Equal("other", Run(pattern, "t:point(7)", Other(above)));
    }

    /// <summary>Rejects unnamed, unknown, non-record or malformed type arguments statically.</summary>
    /// <param name="pattern">The invalid pattern.</param>
    [Theory]
    [InlineData("record()")]
    [InlineData("record(*)")]
    [InlineData("record(x as xs:integer)")]
    [InlineData("record(t:unknown)")]
    [InlineData("record(no:point)")]
    [InlineData("record(xs:integer)")]
    [InlineData("record(t:point | t:other)")]
    [InlineData("record(t:point?)")]
    [InlineData("record(t:point)/x")]
    [InlineData("record(t:point)[true()")]
    [InlineData("record(t:point) union x")]
    [InlineData("x | record(t:point)")]
    public void Invalid(string pattern) => Assert.ThrowsAny<Exception>(() => Run(pattern, "()"));

    /// <summary>Allows a named record context declaration on a record pattern.</summary>
    [Fact]
    public void RecordContextItem()
    {
        var stylesheet = """
            <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
              xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types'>
              <xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
              <xsl:template match='/'><out><xsl:apply-templates select='t:point(7)'/></out></xsl:template>
              <xsl:template match='record(t:point)'><xsl:context-item as='t:point'/>match</xsl:template>
            </xsl:stylesheet>
            """;
        Assert.Equal("match", XElement.Parse(new Api.XsltCompiler().Compile(stylesheet).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>")))).Value);
        var aliasContext = stylesheet.Replace("as='t:point'/>", "as='t:alias'/>")
            .Replace("</xsl:stylesheet>", "<xsl:item-type name='t:alias' as='t:point'/></xsl:stylesheet>");
        Assert.Equal("match", XElement.Parse(new Api.XsltCompiler().Compile(aliasContext).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>")))).Value);
        var mismatch = stylesheet.Replace("as='t:point'/>", "as='xs:string'/>");
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(mismatch).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Contains("XTTE0590", error.ToString());
        var otherType = "<xsl:record-type name='t:other'><xsl:field name='x' as='xs:integer'/></xsl:record-type>";
        var nominalMismatch = stylesheet.Replace("</xsl:stylesheet>", otherType + "</xsl:stylesheet>")
            .Replace("as='t:point'/>", "as='t:other'/>");
        error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(nominalMismatch).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Contains("XTTE0590", error.ToString());
        foreach (var invalidType in new[] { "item() or true()", "t:point?", "t:unknown", "empty-sequence()" })
            Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(stylesheet.Replace("as='t:point'/>", "as='" + invalidType + "'/>")));
    }

    /// <summary>Rejects aliases for non-record types, choices and non-4.0 use.</summary>
    [Fact]
    public void NonRecordAliases()
    {
        foreach (var type in new[] { "map(*)", "xs:integer", "(t:point | t:other)", "(record(x) | record(y))" })
            Assert.ThrowsAny<Exception>(() => Run("record(t:bad)", "()", "<xsl:item-type name='t:bad' as='"+type+"'/>"));
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile("""
            <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:types'>
              <xsl:template match='record(t:point)'/>
            </xsl:stylesheet>
            """));
        Assert.Contains("XTSE0340", error.ToString());
    }
}
