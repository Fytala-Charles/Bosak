// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies map entry sequence patterns, focus, cardinality and priorities.
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

/// <summary>Verifies map entry sequence patterns, focus, cardinality and priorities.</summary>
public class Xslt40MapEntryPatternTests
{
    private static string Run(string body) => XElement.Parse(new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:n='urn:node' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>").TransformToString(
            new XDocumentNode(XDocument.Parse("<root xmlns:n='urn:node'><n:bar/><n:other/><bar/></root>")))).Value;

    private static string SelectRule(string pattern, string value)
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        return new XElement(xsl + "template", new XAttribute("match", "/"), new XElement("out",
            new XElement(xsl + "apply-templates", new XAttribute("select", value)))).ToString() +
            new XElement(xsl + "template", new XAttribute("match", pattern), "match") +
            "<xsl:template match='.'>fallback</xsl:template>";
    }

    /// <summary>Checks entry presence, each value item and required sequence cardinality.</summary>
    /// <param name="pattern">The map entry pattern.</param>
    /// <param name="value">The selected map expression.</param>
    /// <param name="expected">The expected selected rule.</param>
    [Theory]
    [InlineData("{'v':.}", "map{'v':1}", "match")]
    [InlineData("{'v':.}", "map{'v':()}", "fallback")]
    [InlineData("{'v':.}", "map{'v':(1,2)}", "fallback")]
    [InlineData("{'v':.?}", "map{'v':()}", "match")]
    [InlineData("{'v':.?}", "map{'v':1}", "match")]
    [InlineData("{'v':.?}", "map{'v':(1,2)}", "fallback")]
    [InlineData("{'v':.*}", "map{'v':()}", "match")]
    [InlineData("{'v':.+}", "map{'v':()}", "fallback")]
    [InlineData("{'v':.+}", "map{'v':(1,2)}", "match")]
    [InlineData("{'v':empty-sequence()}", "map{'v':()}", "match")]
    [InlineData("{'v':empty-sequence()}", "map{'v':1}", "fallback")]
    [InlineData("{'v':empty-sequence()}", "map{}", "fallback")]
    [InlineData("{'v':empty-sequence ( )}", "map{'v':()}", "match")]
    [InlineData("{'v':empty-sequence(: gap :)()}", "map{'v':()}", "match")]
    [InlineData("{'v':.[. instance of xs:integer]+}", "map{'v':(1,2)}", "match")]
    [InlineData("{'v':.[. instance of xs:integer]+}", "map{'v':(1,'2')}", "fallback")]
    [InlineData("{'v':.[false()]*}", "map{'v':()}", "match")]
    [InlineData("{'v':element(n:bar)}", "map{'v':/*/*[1]}", "match")]
    [InlineData("{'v':n:bar}", "map{'v':/*/*[1]}", "match")]
    [InlineData("{'v':bar}", "map{'v':/*/*[3]}", "match")]
    [InlineData("{'v':element(n:bar)+}", "map{'v':(/*/*[1],/*/*[1])}", "match")]
    [InlineData("{'v':element(n:bar)+}", "map{'v':(/*/*[1],/*/*[2])}", "fallback")]
    [InlineData("{'v':element(n:bar)?}", "map{'v':()}", "match")]
    [InlineData("{'v':element(n:bar)}", "map{'v':7}", "fallback")]
    [InlineData("{'v':{'x':element(n:bar)}}", "map{'v':map{'x':/*/*[1]}}", "match")]
    [InlineData("{'v':{'x':element(n:bar)}}", "map{'v':map{'x':/*/*[2]}}", "fallback")]
    [InlineData("{'v':.[. gt 0],'v':.[. lt 3]}", "map{'v':2}", "match")]
    [InlineData("{'v':.[. gt 0],'v':.[. lt 3]}", "map{'v':4}", "fallback")]
    [InlineData("{'v':. ,*}", "map{'v':1,'extra':2}", "match")]
    [InlineData("{'v':.}", "map{'v':1,'extra':2}", "fallback")]
    [InlineData("{'v':.[position()=1 and last()=1 and current()=.]+}", "map{'v':(1,2)}", "match")]
    [InlineData("{'v':.[error()]}", "map{'v':1}", "fallback")]
    [InlineData("{'v':*}", "map{'v':/*/*[1]}", "match")]
    [InlineData("{'v':*}", "map{'v':()}", "fallback")]
    [InlineData("{'v':**}", "map{'v':()}", "match")]
    [InlineData("{'v':n:*+}", "map{'v':(/*/*[1],/*/*[2])}", "match")]
    [InlineData("{'v':n:*}", "map{'v':/*/*[3]}", "fallback")]
    [InlineData("{'v':element(Q{urn:node}bar)}", "map{'v':/*/*[1]}", "match")]
    [InlineData("{'v':.[contains('x,y', ',')]}", "map{'v':1}", "match")]
    public void SequenceValues(string pattern, string value, string expected) =>
        Assert.Equal(expected, Run(SelectRule(pattern, value)));

    /// <summary>Evaluates outer predicates with singleton map focus and numeric predicate semantics.</summary>
    /// <param name="predicate">The outer predicate.</param>
    /// <param name="expected">The expected selected rule.</param>
    [Theory]
    [InlineData("?v=1", "match")]
    [InlineData("?v=2", "fallback")]
    [InlineData("1", "match")]
    [InlineData("2", "fallback")]
    [InlineData("position()=1 and last()=1 and current()?v=1", "match")]
    [InlineData("error()", "fallback")]
    public void OuterPredicates(string predicate, string expected) =>
        Assert.Equal(expected, Run(SelectRule("{'v':.}[" + predicate + "]", "map{'v':1}")));

    /// <summary>Ranks filtered, constrained and presence-only maps and supports next-match.</summary>
    [Fact]
    public void PrioritiesAndNextMatch()
    {
        Assert.Equal("filteredconstrainedpresenceany", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select="map{'v':1}"/></out></xsl:template>
            <xsl:template match="{'v'}[true()]">filtered<xsl:next-match/></xsl:template>
            <xsl:template match="{'v':.}">constrained<xsl:next-match/></xsl:template>
            <xsl:template match="{'v'}">presence<xsl:next-match/></xsl:template>
            <xsl:template match='{*}'>any</xsl:template>
            """));
    }

    /// <summary>Rejects malformed constraints and unsupported paths/constants at compile time.</summary>
    /// <param name="pattern">The invalid or unsupported pattern.</param>
    [Theory]
    [InlineData("{'v':}")]
    [InlineData("{'v':empty-sequence()+}")]
    [InlineData("{'v':element(}")]
    [InlineData("{'v':.}[true()")]
    [InlineData("{'v':.}/child::x")]
    [InlineData("{1:.}")]
    [InlineData("{'v':.[(true()]]}")]
    public void InvalidConstraints(string pattern) => Assert.ThrowsAny<Exception>(() => Run(SelectRule(pattern, "map{}")));
}
