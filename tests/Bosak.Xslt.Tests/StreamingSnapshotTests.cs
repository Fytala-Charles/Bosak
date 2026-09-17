// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for fn:snapshot over burst-mode streamed sources (streaming Phase D)
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
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for <c>fn:snapshot</c> over streamed (burst-mode) sources (streaming Phase D):
/// snapshotting the streamed document node and the shell root element grounds the stream
/// and produces an ordinary detached in-memory tree equivalent to an in-memory load;
/// record snapshots keep nested content, namespaces, and accumulator annotations.
/// Every behavioral test asserts parity with the in-memory transform of the same
/// stylesheet (W3C sf-snapshot-0101a/b, 0102, 0311 shapes).
/// </summary>
public class StreamingSnapshotTests : IDisposable
{
    private const string Namespaced = """
        <br:Bridge xmlns:gml="http://www.opengis.net/gml/3.2" xmlns:br="http://www.bridge.org" gml:id="Iowa-IllinoisMemorialBridge">
           <gml:description>The Interstate 74 Bridge</gml:description>
           <gml:name>I-74 Bridge</gml:name>
           <gml:boundedBy>
               <gml:Envelope srsName="http://earth-info.nga.mil/">
                   <gml:lowerCorner>41.512674 -90.513161</gml:lowerCorner>
                   <gml:upperCorner>41.524113 -90.51299</gml:upperCorner>
               </gml:Envelope>
           </gml:boundedBy>
           <br:length>3,372 feet (1,028 m)</br:length>
        </br:Bridge>
        """;

    private const string Book = """
        <book>
        <chap><fig>A</fig><fig>B</fig></chap>
        <chap><fig>C</fig></chap>
        </book>
        """;

    private readonly string _sourcePath;
    private readonly string _refPath;

    public StreamingSnapshotTests()
    {
        _sourcePath = Path.Combine(Path.GetTempPath(), $"bosak-snap-{Guid.NewGuid():N}.xml");
        File.WriteAllText(_sourcePath, Namespaced, Encoding.UTF8);
        // A second copy of the same content under a different URI: doc() of the streamed
        // URI returns the (consumed) streamed node by design, so the reference load must
        // come from a distinct file.
        _refPath = Path.Combine(Path.GetTempPath(), $"bosak-snapref-{Guid.NewGuid():N}.xml");
        File.WriteAllText(_refPath, Namespaced, Encoding.UTF8);
    }

    public void Dispose()
    {
        try { File.Delete(_sourcePath); } catch { /* best-effort cleanup */ }
        try { File.Delete(_refPath); } catch { /* best-effort cleanup */ }
    }

    private static string InMemory(string xsl, string xml)
        => new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(xml)).NodeValue!.ToXmlString();

    private static string Streamed(string xsl, string xml)
        => new XsltCompiler().Compile(xsl)
            .TransformStreaming(new MemoryStream(Encoding.UTF8.GetBytes(xml))).NodeValue!.ToXmlString();

    private static void AssertParity(string xsl, string xml)
        => Assert.Equal(InMemory(xsl, xml), Streamed(xsl, xml));

    [Fact]
    public void SnapshotDocumentNode_ParityWithInMemory()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:copy-of select="snapshot(/)"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void SnapshotShellRoot_ParityWithInMemory()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:copy-of select="snapshot(*)"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void SnapshotShellRoot_KeepsDocumentParent()
    {
        // The spec reference implementation retains the ancestors of the snapshot node,
        // so the copy of the outermost element has a (new) document parent.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <xsl:variable name="v" select="snapshot(*)"/>
                <out name="{name($v)}" parent="{exists($v/..)}">
                  <xsl:copy-of select="$v"/>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, Book);
        Assert.Contains("name=\"book\"", result);
        Assert.Contains("parent=\"true\"", result);
        Assert.Contains("<book>", result);
    }

    [Fact]
    public void SnapshotDocumentNode_PreservesWhitespaceTextRecords()
    {
        // No xsl:strip-space: the inter-record whitespace of the source survives the
        // grounded snapshot exactly as an in-memory load would keep it. The snapshot is
        // bound once: a second snapshot() call on the same stream cannot be grounded.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <xsl:variable name="s" select="snapshot(/)"/>
                <out ws="{count($s/*/text()[not(normalize-space())])}"
                     children="{count($s/*/*)}">
                  <xsl:copy-of select="$s"/>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        AssertParity(xsl, Book);
        var result = Streamed(xsl, Book);
        Assert.Contains("ws=\"3\"", result);
        Assert.Contains("children=\"2\"", result);
    }

    [Fact]
    public void SnapshotRecord_PreservesNestedContentAndNamespaces()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                            xmlns:gml="http://www.opengis.net/gml/3.2">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:copy-of select="snapshot(//gml:Envelope)"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Namespaced);

    [Fact]
    public void SnapshotCarriesAccumulatorValues_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="figNr" initial-value="0">
                <xsl:accumulator-rule match="chap" select="0"/>
                <xsl:accumulator-rule match="fig" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="figNr"/>
              <xsl:template match="/">
                <out>
                  <xsl:for-each select="snapshot(/)//fig">
                    <f nr="{accumulator-before('figNr')}"/>
                  </xsl:for-each>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void SnapshotShellRootCarriesAccumulatorValues_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="figNr" initial-value="0">
                <xsl:accumulator-rule match="chap" select="0"/>
                <xsl:accumulator-rule match="fig" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="figNr"/>
              <xsl:template match="/">
                <xsl:variable name="s" select="snapshot(*)"/>
                <out>
                  <xsl:for-each select="$s//fig">
                    <f nr="{accumulator-before('figNr')}"/>
                  </xsl:for-each>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void SourceDocument_SnapshotDocumentNode_DeepEqualWithDoc()
    {
        // W3C sf-snapshot-0101a shape: snapshot(/) inside a streamable
        // xsl:source-document must deep-equal the in-memory load of the same file.
        var xsl = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template name="main">
                <xsl:source-document streamable="yes" href="{{_sourcePath}}">
                  <xsl:variable name="v" select="snapshot(/)"/>
                  <xsl:choose>
                    <xsl:when test="deep-equal(doc('{{_refPath}}'), $v)"><ok/></xsl:when>
                    <xsl:otherwise><bad/></xsl:otherwise>
                  </xsl:choose>
                </xsl:source-document>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = new XsltCompiler().Compile(xsl).Transform(source: null, initialTemplate: "main");
        Assert.Contains("<ok />", result.NodeValue!.ToXmlString());
    }

    [Fact]
    public void SourceDocument_SnapshotAfterElementAxis_GroundsFromArgumentSequence()
    {
        // W3C sf-snapshot-0102 shape: the //* axis consumes the stream before fn:snapshot
        // runs; the shell root snapshot (position 1) must still contain the full subtree,
        // including the whitespace text records the element-only axis never delivered.
        var xsl = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template name="main">
                <xsl:source-document streamable="yes" href="{{_sourcePath}}">
                  <out>
                    <xsl:for-each select="snapshot(//*)[1]">
                      <first name="{name()}" children="{count(*)}" ws="{count(text()[not(normalize-space())])}"/>
                    </xsl:for-each>
                  </out>
                </xsl:source-document>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = new XsltCompiler().Compile(xsl).Transform(source: null, initialTemplate: "main");
        var xml = result.NodeValue!.ToXmlString();
        Assert.Contains("name=\"br:Bridge\"", xml);
        Assert.Contains("children=\"4\"", xml);
        Assert.Contains("ws=\"5\"", xml);
    }

    [Fact]
    public void SourceDocument_SnapshotShellRoot_DeepEqualWithDoc()
    {
        // W3C sf-snapshot-0101b shape: snapshot(/*) deep-equals doc()/* and the copy
        // keeps a document parent.
        var xsl = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template name="main">
                <xsl:source-document streamable="yes" href="{{_sourcePath}}">
                  <xsl:variable name="v" select="snapshot(/*)"/>
                  <xsl:choose>
                    <xsl:when test="deep-equal(doc('{{_refPath}}')/*, $v) and exists($v/..)"><ok/></xsl:when>
                    <xsl:otherwise><bad/></xsl:otherwise>
                  </xsl:choose>
                </xsl:source-document>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = new XsltCompiler().Compile(xsl).Transform(source: null, initialTemplate: "main");
        Assert.Contains("<ok />", result.NodeValue!.ToXmlString());
    }
}
