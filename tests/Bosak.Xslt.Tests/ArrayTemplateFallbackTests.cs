// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies array member processing after template rule fallback.
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
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies array member processing after template rule fallback.</summary>
public class ArrayTemplateFallbackTests
{
    private static string Run(string declarations, string body, string version = "4.0") =>
        new Api.XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='" + version + "' exclude-result-prefixes='#all'>" + declarations +
            "<xsl:template name='start'><out>" + body + "</out></xsl:template></xsl:stylesheet>")
            .TransformToString(null, initialTemplate: "start");
    private static string Apply(string select, string mode = "") => new XElement(XName.Get("apply-templates", "http://www.w3.org/1999/XSL/Transform"), new XAttribute("select", select), new XAttribute("mode", mode.Length == 0 ? "#unnamed" : mode)).ToString();

    /// <summary>Traverses members when a current array pattern exhausts its next-match chain.</summary>
    /// <param name="value">The selected array.</param>
    /// <param name="expected">The member text.</param>
    [Theory]
    [InlineData("[42]", "42")]
    [InlineData("[]", "")]
    [InlineData("[(),(1,2),(),3]", "123")]
    public void CurrentPattern(string value, string expected) => Assert.Equal(expected, XElement.Parse(Run(
        "<xsl:template match='array(.*)'><xsl:next-match/></xsl:template>", Apply(value))).Value);

    /// <summary>Retains the unchanged native array-023 result independently of modern priority semantics.</summary>
    [Fact]
    public void NativeArray023()
    {
        var output = XElement.Parse(Run("""
            <xsl:template match='~array(*)[array:size(.)=0]' xmlns:array='http://www.w3.org/2005/xpath-functions/array'/>
            <xsl:template match='~array(xs:long)'><L><xsl:next-match/></L></xsl:template>
            <xsl:template match='~array(xs:integer)'><I><xsl:next-match/></I></xsl:template>
            <xsl:template match='~array(xs:decimal)'><D><xsl:next-match/></D></xsl:template>
            <xsl:template match='~array(record(first,last))'><AT/></xsl:template>
            """, Apply("[xs:long(42)]")));
        Assert.Equal("42", output.Element("D")?.Element("I")?.Element("L")?.Value);
    }

    /// <summary>Passes the flattened member focus and starts a fresh next-match chain for nested arrays.</summary>
    [Fact]
    public void MemberFocusAndNestedChains()
    {
        Assert.Equal("1/3:1;2/3:2;3/3:3;", XElement.Parse(Run("""
            <xsl:template match='array(.*)'><xsl:next-match/></xsl:template>
            <xsl:template match='atomic(xs:integer)'><xsl:value-of select="concat(position(),'/',last(),':',.,';')"/></xsl:template>
            """, Apply("[(),(1,2),3]") )).Value);
        Assert.Equal("123", XElement.Parse(Run("<xsl:template match='array(.*)'><xsl:next-match/></xsl:template>", Apply("[[1],[2,[3]]]"))).Value);
    }

    /// <summary>Dispatches element members through their matching template rules.</summary>
    [Fact]
    public void NodeMembers() => Assert.Equal("node", XElement.Parse(Run("""
        <xsl:template match='array(.*)'><xsl:next-match/></xsl:template>
        <xsl:template match='n'>node</xsl:template>
        """, "<xsl:variable name='n' as='element()'><n/></xsl:variable>" + Apply("[$n]"))).Value);

    /// <summary>Retains the selected mode and passes ordinary and tunnel parameters to member rules.</summary>
    [Fact]
    public void ModeAndParameters() => Assert.Equal("PT", XElement.Parse(Run("""
        <xsl:mode name='m'/>
        <xsl:template match='array(.*)' mode='m'><xsl:next-match><xsl:with-param name='p' select="'P'"/><xsl:with-param name='t' tunnel='yes' select="'T'"/></xsl:next-match></xsl:template>
        <xsl:template match='atomic(xs:integer)' mode='m'><xsl:param name='p'/><xsl:param name='t' tunnel='yes'/><xsl:value-of select='concat($p,$t)'/></xsl:template>
        <xsl:template match='atomic(xs:integer)'>wrong-mode</xsl:template>
        """, Apply("[1]", "m"))).Value);

    /// <summary>Restores the array caller's focus after the implicit member application.</summary>
    [Fact]
    public void CallerFocus() => Assert.Equal("11/2;22/2;", XElement.Parse(Run("""
        <xsl:template match='array(.*)'><xsl:next-match/><xsl:value-of select="concat(?1,'/',last(),';')"/></xsl:template>
        """, Apply("[1],[2]"))).Value);

    /// <summary>Uses member templates for shallow-skip while keeping deep-skip silent.</summary>
    /// <param name="behavior">The unmatched-item behavior.</param>
    /// <param name="expected">The expected output.</param>
    [Theory]
    [InlineData("text-only-copy", "member")]
    [InlineData("shallow-skip", "member")]
    [InlineData("deep-skip", "")]
    public void SkipModes(string behavior, string expected) => Assert.Equal(expected, XElement.Parse(Run(
        "<xsl:mode on-no-match='" + behavior + "'/><xsl:template match='array(.*)'><xsl:next-match/></xsl:template><xsl:template match='atomic(xs:integer)'>member</xsl:template>", Apply("[1]"))).Value);

    /// <summary>Uses normal member application when no explicit array rule exists either.</summary>
    [Fact]
    public void OrdinaryArrayFallback() => Assert.Equal("1/3;2/3;3/3;", XElement.Parse(Run(
        "<xsl:template match='atomic(xs:integer)'><xsl:value-of select=\"concat(position(),'/',last(),';')\"/></xsl:template>", Apply("[(),(1,2),3]"))).Value);

    /// <summary>Dispatches unmatched node members through their built-in tree traversal.</summary>
    [Fact]
    public void UnmatchedElementMember() => Assert.Equal("child", XElement.Parse(Run(
        "<xsl:template match='array(.*)'><xsl:next-match/></xsl:template><xsl:template match='child'>child</xsl:template>",
        "<xsl:variable name='n' as='element()'><n><child/></n></xsl:variable>" + Apply("[$n]"))).Value);

    /// <summary>Restores caller position and current item as well as size after member processing.</summary>
    [Fact]
    public void CallerPositionAndCurrent() => Assert.Equal("11:1:1/2;22:2:2/2;", XElement.Parse(Run("""
        <xsl:template match='array(.*)'><xsl:next-match/><xsl:value-of select="concat(position(),':',?1,':',current()?1,'/',last(),';')"/></xsl:template>
        """, Apply("[1],[2]"))).Value);

    /// <summary>Uses the same array fallback in XSLT 3.0 predicate-pattern dispatch.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("42", XElement.Parse(Run(
        "<xsl:template match='.[. instance of array(*)]'><xsl:next-match/></xsl:template>", Apply("[42]"), version: "3.0")).Value);

    /// <summary>Keeps atomic next-match fallback behavior intact.</summary>
    [Fact]
    public void AtomicFallback() => Assert.Equal("42", XElement.Parse(Run(
        "<xsl:template match='atomic(xs:integer)'><xsl:next-match/></xsl:template>", Apply("42"))).Value);
}
