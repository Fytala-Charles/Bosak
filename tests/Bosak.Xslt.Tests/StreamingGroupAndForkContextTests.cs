// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 september 2026
// PURPOSE              : Unit tests for group-context isolation across template invocation and streaming map errors.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-09-2026     | Creation (si-fork-113/114/115/116/801/814 fixes)                                         |
//                      | Charles Korthout | 0.2   | 02-10-2026     | PC-1 W7-4: streamed group-starting-with predicate patterns (si-group-054/056)          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Text.RegularExpressions;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the residual si-fork fixes: current-group()/current-grouping-key() must be
/// absent inside templates invoked via xsl:call-template and xsl:apply-templates
/// (dynamic XTDE1061/XTDE1071, caught with xsl:catch in these shapes), current-group()
/// in a streamable-mode template body with no group in scope is a static XTSE3430,
/// fn:generate-id() must be stable across xsl:fork prongs, and duplicate map-constructor
/// keys inside xsl:fork raise XTDE3365 instead of XQDY0137.
/// </summary>
public class StreamingGroupAndForkContextTests
{
    private const string BooksXml = """
        <BOOKLIST><BOOKS>
          <ITEM AUTHOR="Austen"><TITLE>Emma</TITLE></ITEM>
          <ITEM AUTHOR="Bronte"><TITLE>Jane Eyre</TITLE></ITEM>
          <ITEM AUTHOR="Austen"><TITLE>Persuasion</TITLE></ITEM>
        </BOOKS></BOOKLIST>
        """;

    private static string Streamed(string xsl, string xml)
    {
        var doc = XmlStreamingProvider.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(xml)),
            new StreamingLoadOptions { BaseUri = "http://example.org/in.xml" });
        return new XsltCompiler().Compile(xsl).Transform(doc).NodeValue!.ToXmlString().Replace(" />", "/>");
    }

    // ---------------- current-group context isolation (si-fork-113/114/115) ----------------

    [Fact]
    public void CallTemplate_GroupFunctionsAreAbsent()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode streamable="yes"/>
              <xsl:template match="/">
                <out>
                  <xsl:for-each-group select="/BOOKLIST/BOOKS/ITEM" group-by="@AUTHOR">
                    <g><xsl:call-template name="probe"/></g>
                  </xsl:for-each-group>
                </out>
              </xsl:template>
              <xsl:template name="probe">
                <xsl:try>
                  <h key="{current-grouping-key()}" size="{count(current-group())}"/>
                  <xsl:catch errors="*:XTDE1061 *:XTDE1071"><h key="#absent#" size="#absent#"/></xsl:catch>
                </xsl:try>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var result = Streamed(xsl, BooksXml);
        Assert.Equal(
            """<out><g><h key="#absent#" size="#absent#"/></g><g><h key="#absent#" size="#absent#"/></g></out>""",
            result);
    }

    [Fact]
    public void ApplyTemplates_GroupFunctionsAreAbsent()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode name="cgk" streamable="yes"/>
              <xsl:template match="/">
                <out>
                  <xsl:for-each-group select="/BOOKLIST/BOOKS/ITEM" group-by="@AUTHOR">
                    <g><xsl:apply-templates select="current-group()" mode="cgk"/></g>
                  </xsl:for-each-group>
                </out>
              </xsl:template>
              <xsl:template match="." mode="cgk">
                <xsl:try>
                  <h key="{current-grouping-key()}"/>
                  <xsl:catch errors="*:XTDE1071"><h key="#absent#"/></xsl:catch>
                </xsl:try>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var result = Streamed(xsl, BooksXml);
        Assert.Equal(
            """<out><g><h key="#absent#"/><h key="#absent#"/></g><g><h key="#absent#"/></g></out>""",
            result);
    }

    [Fact]
    public void GroupBody_GroupFunctionsRemainAvailable_AroundTemplateCall()
    {
        // The group must be visible directly in the group body and again after the
        // called template returns.
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode streamable="yes"/>
              <xsl:template match="/">
                <out>
                  <xsl:for-each-group select="/BOOKLIST/BOOKS/ITEM" group-by="@AUTHOR">
                    <g key="{current-grouping-key()}" n="{count(current-group())}"><xsl:call-template name="probe"/></g>
                  </xsl:for-each-group>
                </out>
              </xsl:template>
              <xsl:template name="probe"><ok/></xsl:template>
            </xsl:stylesheet>
            """;
        var result = Streamed(xsl, BooksXml);
        Assert.Equal(
            """<out><g key="Austen" n="2"><ok/></g><g key="Bronte" n="1"><ok/></g></out>""",
            result);
    }

    // ---------------- static XTSE3430 for current-group() (si-fork-116) ----------------

    [Fact]
    public void StreamableTemplate_CurrentGroupWithoutGroup_ThrowsXtse3430()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode name="cgk" streamable="yes"/>
              <xsl:template match="." mode="cgk"><h size="{count(current-group())}"/></xsl:template>
            </xsl:stylesheet>
            """;
        var ex = Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(xsl));
        Assert.StartsWith("XTSE3430", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StreamableTemplate_CurrentGroupingKeyWithoutGroup_Compiles()
    {
        // current-grouping-key() is motionless; with no group in scope it is a dynamic
        // XTDE1071 at worst, never a static error (si-fork-115).
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode name="cgk" streamable="yes"/>
              <xsl:template match="." mode="cgk"><h key="{current-grouping-key()}"/></xsl:template>
            </xsl:stylesheet>
            """;
        new XsltCompiler().Compile(xsl);
    }

    // ---------------- generate-id stability across fork prongs (si-fork-801) ----------------

    [Fact]
    public void Fork_GenerateIdMatchesAcrossProngs()
    {
        const string xml = """<body><heading>One</heading><p>x</p><heading>Two</heading></body>""";
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode streamable="yes" on-no-match="shallow-copy"/>
              <xsl:mode name="toc" streamable="yes" on-no-match="shallow-copy"/>
              <xsl:template match="body">
                <xsl:copy>
                  <xsl:fork>
                    <xsl:sequence>
                      <toc><xsl:apply-templates select="heading" mode="toc"/></toc>
                    </xsl:sequence>
                    <xsl:sequence>
                      <xsl:apply-templates/>
                    </xsl:sequence>
                  </xsl:fork>
                </xsl:copy>
              </xsl:template>
              <xsl:template match="heading" mode="toc">
                <link href="#{generate-id()}"><xsl:apply-templates mode="#current"/></link>
              </xsl:template>
              <xsl:template match="heading">
                <xsl:copy>
                  <xsl:attribute name="id" select="generate-id()"/>
                  <xsl:apply-templates/>
                </xsl:copy>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var result = Streamed(xsl, xml);
        var links = Regex.Matches(result, "href=\"#(id\\d+)\"").Select(m => m.Groups[1].Value).ToList();
        var ids = Regex.Matches(result, "id=\"(id\\d+)\"").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(2, links.Count);
        Assert.Equal(links, ids);
    }

    // ---------------- XTDE3365 duplicate map keys inside xsl:fork (si-fork-814) ----------------

    [Fact]
    public void Fork_DuplicateMapKey_ThrowsXtde3365()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:mode streamable="yes"/>
              <xsl:template match="cities">
                <xsl:fork>
                  <xsl:for-each-group select="city" group-by="@country">
                    <xsl:sequence select="map{'country': current-grouping-key(), 'country': 'uk'}"/>
                  </xsl:for-each-group>
                </xsl:fork>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Streamed(xsl, """<cities><city country="fr"/><city country="de"/></cities>"""));
        Assert.StartsWith("XTDE3365", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NonStreamingMap_DuplicateKeyStillThrowsXqdy0137()
    {
        // Outside XSLT streaming constructs the XPath map-constructor error applies.
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="xsl:initial-template"><xsl:sequence select="map{'a': 1, 'a': 2}"/></xsl:template>
            </xsl:stylesheet>
            """;
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new XsltCompiler().Compile(xsl).Transform(null));
        Assert.StartsWith("XQDY0137", ex.Message, StringComparison.Ordinal);
    }

    // ---------------- streamed group-starting-with predicate patterns (PC-1 W7-4) ----------------

    private const string ContentsXml = """
        <contents>
          <unnumbered type="PT"><title>Title one</title></unnumbered>
          <unnumbered type="chapter" manid="01"/>
          <unnumbered type="PT"><title>Title Two</title></unnumbered>
          <unnumbered type="chapter" manid="04"/>
        </contents>
        """;

    private const string GroupStartingXsl = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:mode streamable="yes"/>
          <xsl:strip-space elements="*"/>
          <xsl:template match="contents">
            <out>
              <xsl:for-each-group select="unnumbered" group-starting-with="unnumbered[@type = 'PT']">
                <g size="{count(current-group())}" ctx="{@type}"/>
              </xsl:for-each-group>
            </out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    [Fact]
    public void GroupStartingWith_AttributePredicateOnStreamedNodes_GroupsCorrectly()
    {
        // Regression (si-group-054/056): pattern matching re-ran child::base[pred] from
        // the parent, which a streamed parent cannot service (its child pump is consumed);
        // the swallowed StreamingException made every candidate fail the pattern, so all
        // items collapsed into one group. Non-positional predicates now self-evaluate.
        var result = Streamed(GroupStartingXsl, ContentsXml);
        Assert.Equal(
            "<out><g size=\"2\" ctx=\"PT\"/><g size=\"2\" ctx=\"PT\"/></out>",
            result);
    }

    [Fact]
    public void GroupStartingWith_AttributePredicate_InMemoryParityUnchanged()
    {
        // Edge: in-memory documents keep the parent-based evaluation path; the streamed
        // self-evaluation shortcut must not change their grouping semantics.
        var doc = XDocumentProvider.ParseXml(ContentsXml);
        var result = new XsltCompiler().Compile(GroupStartingXsl).Transform(doc).NodeValue!.ToXmlString();
        Assert.Equal(
            "<out><g size=\"2\" ctx=\"PT\"/><g size=\"2\" ctx=\"PT\"/></out>",
            result.Replace(" />", "/>"));
    }
}
