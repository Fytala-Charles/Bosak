// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for xsl:fork prong composition: prong-order concatenation in complex content, side-effect-only prongs writing secondary result documents, fork and xsl:where-populated in function bodies, and map patterns with QName key types.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for xsl:fork prong composition regressions (si-fork-119/810/811/815): prong results
/// concatenate in prong order under the complex-content rules, a side-effect-only prong
/// contributes zero items while its xsl:result-document output is written, xsl:fork and
/// xsl:where-populated are executable as function-body instructions, and map patterns with
/// QName key types such as map(xs:string, element()?) match.
/// </summary>
[Collection("MemorySensitive")]
public class ForkProngCompositionTests
{
    private const string ItemsXml = "<items><item id='a'/><item id='b'/></items>";

    private static string Streamed(string xsl, string xml)
    {
        var doc = XmlStreamingProvider.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(xml)),
            new StreamingLoadOptions { BaseUri = "http://example.org/in.xml" });
        return new XsltCompiler().Compile(xsl).Transform(doc).NodeValue!.ToXmlString();
    }

    [Fact]
    public void Fork_InElementContent_ProngResultsConcatenateInProngOrder()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes"/>
              <xsl:template match="items"><ul><xsl:fork>
                <xsl:sequence><xsl:apply-templates select="item"/></xsl:sequence>
                <xsl:sequence><xsl:text>tail</xsl:text></xsl:sequence>
              </xsl:fork></ul></xsl:template>
              <xsl:template match="item"><li>Item <xsl:value-of select="@id"/></li></xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, ItemsXml);

        Assert.Equal("<ul><li>Item a</li><li>Item b</li>tail</ul>", result);
    }

    [Fact]
    public void Fork_InElementContent_AdjacentAtomicValuesAreSpaceJoined()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes"/>
              <xsl:template match="data"><out><xsl:fork>
                <xsl:sequence select="1"/>
                <xsl:sequence select="2, 3"/>
                <xsl:sequence><b/></xsl:sequence>
                <xsl:sequence select="4"/>
              </xsl:fork></out></xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, "<data/>");

        Assert.Equal("<out>1 2 3<b />4</out>", result);
    }

    [Fact]
    public void Fork_SideEffectProng_WritesSecondaryDocumentsWithoutLeakingItems()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-fork-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var baseOutputUri = new Uri(dir + Path.DirectorySeparatorChar).AbsoluteUri;
            var xsl = """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" expand-text="yes">
                  <xsl:output method="xml" indent="no"/>
                  <xsl:mode streamable="yes"/>
                  <xsl:template match="items"><ul><xsl:fork>
                    <xsl:sequence><xsl:apply-templates select="item"/></xsl:sequence>
                    <xsl:sequence><xsl:for-each select="item">
                      <xsl:result-document href="item_{@id}.html" method="html">
                        <html><body><p>Item {@id}</p><br/></body></html>
                      </xsl:result-document>
                    </xsl:for-each></xsl:sequence>
                  </xsl:fork></ul></xsl:template>
                  <xsl:template match="item"><li>Item {@id}</li></xsl:template>
                </xsl:stylesheet>
                """;

            var doc = XmlStreamingProvider.Load(
                new MemoryStream(Encoding.UTF8.GetBytes("<items><item id='i1'/><item id='i2'/></items>")),
                new StreamingLoadOptions { BaseUri = "http://example.org/in.xml" });
            var result = new XsltCompiler().Compile(xsl).Transform(doc, baseOutputUri: baseOutputUri).NodeValue!.ToXmlString();

            // The side-effect-only prong contributes zero items to the fork result.
            Assert.Equal("<ul><li>Item i1</li><li>Item i2</li></ul>", result);

            // The secondary result documents were written with their full content
            // (the si-fork-119 regression produced zero-byte files) and self-close
            // void elements so they can be reloaded as XML.
            foreach (var id in new[] { "i1", "i2" })
            {
                var file = Path.Combine(dir, $"item_{id}.html");
                Assert.True(File.Exists(file), $"Expected secondary result document {file}");
                var content = File.ReadAllText(file);
                Assert.Contains($"Item {id}", content);
                Assert.Contains("<br/>", content);
                System.Xml.Linq.XDocument.Load(file);
            }
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Fork_InFunctionBody_WithWherePopulatedAndForEachGroup()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                  xmlns:mf="http://example.com/mf" xmlns:xs="http://www.w3.org/2001/XMLSchema"
                  exclude-result-prefixes="mf xs">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes" on-no-match="shallow-copy"/>
              <xsl:mode name="num" streamable="yes"/>
              <xsl:function name="mf:nest" as="node()*" _streamability="absorbing">
                <xsl:param name="input" as="node()*"/>
                <xsl:param name="level" as="xs:integer"/>
                <xsl:where-populated><orderlist type="manual"><xsl:for-each-group select="$input" group-starting-with="*[@class = 'l' || $level]"><item><xsl:fork>
                  <xsl:sequence><xsl:apply-templates select="node()[1]" mode="num"/></xsl:sequence>
                  <xsl:sequence><para><xsl:apply-templates select="node()[position() gt 2]"/></para></xsl:sequence>
                  <xsl:sequence><rest size="{count(current-group())}"/></xsl:sequence>
                </xsl:fork></item></xsl:for-each-group></orderlist></xsl:where-populated>
              </xsl:function>
              <xsl:template match="root"><xsl:copy><xsl:sequence select="mf:nest(*, 1)"/></xsl:copy></xsl:template>
              <xsl:template match="*[matches(@class, 'l[0-9]+')]/node()" mode="num">
                <xsl:attribute name="num" select="replace(., '^\s+', '')"/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var xml = """
            <root>
              <p class="l1">(a)</p>
              <p class="l1">(b)</p>
              <p class="l2">(i)</p>
            </root>
            """;

        var result = Streamed(xsl, xml);

        Assert.Equal(
            "<root><orderlist type=\"manual\">" +
            "<item num=\"(a)\"><para /><rest size=\"1\" /></item>" +
            "<item num=\"(b)\"><para /><rest size=\"2\" /></item>" +
            "</orderlist></root>",
            result);
    }

    [Fact]
    public void MapPattern_WithQNameKeyType_MatchesInMemoryDocument()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/"><out>
                <xsl:apply-templates select="map { 'current-item' : items/item[1] }"/>
              </out></xsl:template>
              <xsl:template match=".[. instance of map(xs:string, element()?)]">
                <matched><xsl:value-of select="?current-item/@id"/></matched>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var doc = new XDocumentNode(System.Xml.Linq.XDocument.Parse("<items><item id='i1'/><item id='i2'/></items>"));

        var result = new XsltCompiler().Compile(xsl).Transform(doc).NodeValue!.ToXmlString();

        Assert.Equal("<out><matched>i1</matched></out>", result);
    }

    [Fact]
    public void MapPattern_WithQNameKeyType_MatchesStreamedDocument()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes"/>
              <xsl:template match="items">
                <xsl:variable name="first" select="copy-of(item[1])"/>
                <out>
                  <xsl:apply-templates select="map { 'current-item' : $first }"/>
                </out>
              </xsl:template>
              <xsl:template match=".[. instance of map(xs:string, element()?)]">
                <matched><xsl:value-of select="?current-item/@id"/></matched>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, ItemsXml);

        Assert.Equal("<out><matched>a</matched></out>", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Fork_AroundForEachGroup_MatchesNonForkGrouping(bool withFork)
    {
        var forkOpen = withFork ? "<xsl:fork>" : string.Empty;
        var forkClose = withFork ? "</xsl:fork>" : string.Empty;
        var xsl = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                  xmlns:xs="http://www.w3.org/2001/XMLSchema"
                  xmlns:array="http://www.w3.org/2005/xpath-functions/array"
                  xmlns:mf="http://example.com/mf" exclude-result-prefixes="xs array mf">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes"/>
              <xsl:function name="mf:group" as="xs:string*" _streamability="absorbing">
                <xsl:param name="input" as="element()"/>
                {{forkOpen}}<xsl:for-each-group select="$input/transaction" group-by="@date">
                  <xsl:sequence select="string-join(array:flatten(array { current-group()!@value!xs:decimal(.) }), ',')"/>
                </xsl:for-each-group>{{forkClose}}
              </xsl:function>
              <xsl:template match="account"><out><xsl:sequence select="mf:group(.)"/></out></xsl:template>
            </xsl:stylesheet>
            """;
        var xml = """
            <account>
              <transaction date="2020-01-01" value="1.5"/>
              <transaction date="2020-01-01" value="2.5"/>
              <transaction date="2020-01-02" value="3"/>
              <transaction date="2020-01-03" value="4"/>
            </account>
            """;

        var result = Streamed(xsl, xml);

        Assert.Equal("<out>1.5,2.5 3 4</out>", result);
    }
}
