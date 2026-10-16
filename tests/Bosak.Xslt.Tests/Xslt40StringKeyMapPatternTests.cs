// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies string-key map pattern matching and default priorities.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Move supported entry constraints and outer predicates to the sequence-pattern fixture.  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies string-key map pattern matching and default priorities.</summary>
public class Xslt40StringKeyMapPatternTests
{
    private static string Run(string body, string version = "4.0")
    {
        var sheet = "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
            "xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>" + body + "</xsl:stylesheet>";
        return XElement.Parse(new Api.XsltCompiler().Compile(sheet)
            .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
    }

    private static string Rule(string pattern, string text) =>
        new XElement(XName.Get("template", "http://www.w3.org/1999/XSL/Transform"),
            new XAttribute("match", pattern), text).ToString();

    /// <summary>Matches maps by key presence and exact or open key sets, without testing entry values.</summary>
    /// <param name="pattern">The map pattern.</param>
    /// <param name="value">The selected XPath value.</param>
    /// <param name="expected">Whether the map rule matches.</param>
    [Theory]
    [InlineData("{}", "map{}", "match")]
    [InlineData("{}", "map{'a':1}", "fallback")]
    [InlineData("{*}", "map{}", "match")]
    [InlineData("{*}", "map{1:'a'}", "match")]
    [InlineData("{'a'}", "map{'a':()}", "match")]
    [InlineData("{'a'}", "map{'a':(1,2)}", "match")]
    [InlineData("{'a'}", "map{'a':1,'b':2}", "fallback")]
    [InlineData("{'a',*}", "map{'a':1,'b':2}", "match")]
    [InlineData("{'a',*}", "map{'b':2}", "fallback")]
    [InlineData("{'a','b'}", "map{'b':2,'a':1}", "match")]
    [InlineData("{'a','a'}", "map{'a':1}", "match")]
    [InlineData("{'A'}", "map{'a':1}", "fallback")]
    [InlineData("{'a'}", "map{xs:anyURI('a'):1}", "match")]
    [InlineData("{'1'}", "map{1:'a'}", "fallback")]
    [InlineData("{'a'}", "[]", "fallback")]
    [InlineData("{*}", "7", "fallback")]
    [InlineData("{'a'}", "/*", "fallback")]
    [InlineData("map {'a'}", "map{'a':1}", "match")]
    [InlineData("{(:outer(:inner:) :) 'a', *}", "map{'a':1}", "match")]
    [InlineData("{'a''b'}", "map{\"a'b\":1}", "match")]
    [InlineData("{'a|b/[x]:{q}'}", "map{'a|b/[x]:{q}':1}", "match")]
    public void KeySets(string pattern, string value, string expected)
    {
        var entry = new XElement(XName.Get("template", "http://www.w3.org/1999/XSL/Transform"),
            new XAttribute("match", "/"), new XElement("out",
                new XElement(XName.Get("apply-templates", "http://www.w3.org/1999/XSL/Transform"),
                    new XAttribute("select", value))));
        Assert.Equal(expected, Run(entry + Rule(pattern, "match") + Rule(".", "fallback")));
    }

    /// <summary>Matches annotated structural records as maps without changing their annotation.</summary>
    [Fact]
    public void RecordsAreMaps()
    {
        Assert.Equal("record", Run("""
            <xsl:template match='/'><out><xsl:variable name='r' as='record(a)' select="map{'a':1}"/>
              <xsl:apply-templates select='$r'/></out></xsl:template>
            """ + Rule("{'a'}", "record")));
    }

    /// <summary>Uses the draft's map-family default priorities independently of declaration order.</summary>
    [Fact]
    public void DefaultPriorityAndNextMatch()
    {
        Assert.Equal("emptyanykeyany", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select="(map{},map{'a':1})"/></out></xsl:template>
            <xsl:template match="{'a'}">key<xsl:next-match/></xsl:template>
            <xsl:template match='{}'>empty<xsl:next-match/></xsl:template>
            <xsl:template match='{*}'>any</xsl:template>
            """));
    }

    /// <summary>Rejects invalid syntax and unsupported paths/constants at compilation.</summary>
    /// <param name="pattern">The invalid or unsupported pattern.</param>
    [Theory]
    [InlineData("{a}")]
    [InlineData("{'a'")]
    [InlineData("{'a',}")]
    [InlineData("{*,'a'}")]
    [InlineData("{'a',*,*}")]
    [InlineData("{1}")]
    [InlineData("{'a'}|root")]
    [InlineData("root|{'a'}")]
    [InlineData("root union map{'a'}")]
    [InlineData("root intersect {'a'}")]
    [InlineData("root except {'a'}")]
    public void UnsupportedPatternsFailStatically(string pattern)
    {
        Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => Run(Rule(pattern, "bad"))).ToString());
    }

    /// <summary>Rejects new map syntax in locally earlier expression profiles.</summary>
    [Fact]
    public void EarlierProfilesRejectMapPatterns()
    {
        Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => Run(Rule("{'a'}", "bad"), "3.0")).ToString());
    }

    /// <summary>Honors a locally earlier template profile inside a 4.0 stylesheet.</summary>
    [Fact]
    public void LocalEarlierProfileRejectsMapPattern()
    {
        Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:template version='3.0' match="{'a'}">bad</xsl:template>
            """)).ToString());
    }

    /// <summary>Parentheses introduce a node pattern, so the enclosed map pattern cannot match a map.</summary>
    [Fact]
    public void ParenthesizedMapPatternRequiresNode()
    {
        Assert.Equal("fallback", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select="map{'a':1}"/></out></xsl:template>
            """ + Rule("({'a'})", "bad") + Rule(".", "fallback")));
    }
}
