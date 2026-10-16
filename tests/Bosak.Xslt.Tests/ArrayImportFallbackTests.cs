// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies array fallback and member chains after apply-imports.
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

/// <summary>Verifies array fallback and member chains after apply-imports.</summary>
public class ArrayImportFallbackTests
{
    private static string Wrap(string declarations, string version) => "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='" + version + "' exclude-result-prefixes='#all'>" + declarations + "</xsl:stylesheet>";
    private sealed class Resolver(string module) : Api.IXsltUriResolver
    {
        /// <summary>Returns the imported module without external IO.</summary>
        /// <param name="href">The requested reference.</param>
        /// <param name="baseUri">The importing module's base URI.</param>
        /// <returns>The module document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(module);
    }
    private static string Run(string declarations, string body, string imported = "", string version = "4.0") =>
        XElement.Parse(new Api.XsltCompiler { UriResolver = new Resolver(Wrap(imported, version)) }.Compile(Wrap(
            "<xsl:import href='base.xsl'/>" + declarations + "<xsl:template name='start'><out>" + body + "</out></xsl:template>", version))
            .TransformToString(null, initialTemplate: "start")).Value;
    private static string Apply(string select, string mode = "#unnamed") => new XElement(XName.Get("apply-templates", "http://www.w3.org/1999/XSL/Transform"), new XAttribute("select", select), new XAttribute("mode", mode)).ToString();
    private const string ArrayRule = "<xsl:template match='array(.*)'><xsl:apply-imports/></xsl:template>";

    /// <summary>Processes array members when no imported rule matches the array.</summary>
    /// <param name="value">The selected array.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData("[42]", "42")]
    [InlineData("[]", "")]
    [InlineData("[(),(1,2),(),3]", "123")]
    [InlineData("[[1],[2,[3]]]", "123")]
    public void Members(string value, string expected) => Assert.Equal(expected, Run(ArrayRule, Apply(value)));

    /// <summary>Uses the flattened member focus and restores the calling array's focus.</summary>
    [Fact]
    public void Focus() => Assert.Equal("1/3:1;2/3:2;3/3:3;1:3/1;", Run("""
        <xsl:template match='array(.*)'><xsl:apply-imports/><xsl:value-of select="concat(position(),':',count(current()?*),'/',last(),';')"/></xsl:template>
        <xsl:template match='atomic(xs:integer)'><xsl:value-of select="concat(position(),'/',last(),':',.,';')"/></xsl:template>
        """, Apply("[(),(1,2),3]")));

    /// <summary>Passes ordinary and tunnel parameters to members in the same named mode.</summary>
    [Fact]
    public void ModeAndParameters() => Assert.Equal("PT", Run("""
        <xsl:mode name='m'/>
        <xsl:template match='array(.*)' mode='m'><xsl:apply-imports><xsl:with-param name='p' select="'P'"/><xsl:with-param name='t' select="'T'" tunnel='yes'/></xsl:apply-imports></xsl:template>
        <xsl:template match='atomic(xs:integer)' mode='m'><xsl:param name='p'/><xsl:param name='t' tunnel='yes'/><xsl:value-of select='concat($p,$t)'/></xsl:template>
        <xsl:template match='atomic(xs:integer)'>wrong-mode</xsl:template>
        """, Apply("[1]", "m")));

    /// <summary>Uses node template dispatch for node-valued array members.</summary>
    [Fact]
    public void NodeMembers() => Assert.Equal("node", Run(ArrayRule + "<xsl:template match='child'>node</xsl:template>",
        "<xsl:variable name='n' as='element()'><n><child/></n></xsl:variable>" + Apply("[$n]")));

    /// <summary>Chooses imported matching array rules before falling back to built-ins.</summary>
    [Fact]
    public void ImportedRuleWins() => Assert.Equal("imported", Run(ArrayRule, Apply("[1]"),
        "<xsl:template match='array(.*)'>imported</xsl:template>"));

    /// <summary>Starts a fresh member next-match chain without the array caller's import restriction.</summary>
    [Fact]
    public void MemberNextMatch() => Assert.Equal("HL", Run(ArrayRule + """
        <xsl:template match='atomic(xs:integer)' priority='2'>H<xsl:next-match/></xsl:template>
        <xsl:template match='atomic(xs:integer)' priority='1'>L</xsl:template>
        """, Apply("[1]")));

    /// <summary>Restores the enclosing import restriction after processing members.</summary>
    [Fact]
    public void RestoreImportRestriction() => Assert.Equal("memberlower", Run(ArrayRule +
        "<xsl:template match='atomic(xs:integer)'>member</xsl:template>", Apply("[1]"), """
        <xsl:template match='array(.*)' priority='2'><xsl:apply-imports/><xsl:next-match/></xsl:template>
        <xsl:template match='array(.*)' priority='1'>lower</xsl:template>
        """));

    /// <summary>Honors shallow-skip member traversal and preserves deep-skip behavior.</summary>
    /// <param name="behavior">The unmatched-item policy.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData("text-only-copy", "member")]
    [InlineData("shallow-skip", "member")]
    [InlineData("deep-skip", "")]
    public void SkipModes(string behavior, string expected) => Assert.Equal(expected, Run(
        "<xsl:mode on-no-match='" + behavior + "'/>" + ArrayRule + "<xsl:template match='atomic(xs:integer)'>member</xsl:template>", Apply("[1]")));

    /// <summary>Supports the inherited XSLT3.0 predicate-pattern form too.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("42", Run(
        "<xsl:template match='.[. instance of array(*)]'><xsl:apply-imports/></xsl:template>", Apply("[42]"), version: "3.0"));

    /// <summary>Retains atomic fallback when no imported rule matches.</summary>
    [Fact]
    public void AtomicFallback() => Assert.Equal("42", Run(
        "<xsl:template match='atomic(xs:integer)'><xsl:apply-imports/></xsl:template>", Apply("42")));

    /// <summary>Restores the outer import restriction even when a member raises an error.</summary>
    [Fact]
    public void MemberErrorRestoresRestriction() => Assert.Equal("caughtlower", Run(ArrayRule +
        "<xsl:template match='atomic(xs:integer)'><xsl:sequence select='error()'/></xsl:template>", Apply("[1]"), """
        <xsl:template match='array(.*)' priority='2'><xsl:try><xsl:apply-imports/><xsl:catch>caught</xsl:catch></xsl:try><xsl:next-match/></xsl:template>
        <xsl:template match='array(.*)' priority='1'>lower</xsl:template>
        """));

    /// <summary>Isolates member chains when next-match reaches fallback inside an imported array rule.</summary>
    [Fact]
    public void ImportedNextMatchFallback() => Assert.Equal("HL", Run(ArrayRule + """
        <xsl:template match='atomic(xs:integer)' priority='2'>H<xsl:next-match/></xsl:template>
        <xsl:template match='atomic(xs:integer)' priority='1'>L</xsl:template>
        """, Apply("[1]"), "<xsl:template match='array(.*)'><xsl:next-match/></xsl:template>"));
}
