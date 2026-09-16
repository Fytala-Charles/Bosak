// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for XsltExecutable.TransformStreaming (burst-mode streaming input)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for <see cref="XsltExecutable.TransformStreaming"/>: output parity with the
/// in-memory transform across a matrix of stylesheet shapes, the error contract for
/// operations a single-pass source cannot support, the whitespace-rule handoff, and
/// bounded-memory record processing.
/// </summary>
[Collection("MemorySensitive")]
public class StreamingTransformTests
{
    private const string Xml = """
        <inventory>
        <product id="1"><name>Hammer</name><price>9.99</price></product>
        <product id="2"><name>Saw</name><price>19.99</price></product>
        <product id="3"><name>Drill</name><price>29.99</price></product>
        </inventory>
        """;

    private static string InMemory(string xsl, string xml)
        => new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(xml)).NodeValue!.ToXmlString();

    private static string Streamed(string xsl, string xml, StreamingTransformOptions? options = null)
        => new XsltCompiler().Compile(xsl)
            .TransformStreaming(new MemoryStream(Encoding.UTF8.GetBytes(xml)), options).NodeValue!.ToXmlString();

    private static void AssertParity(string xsl, string xml)
        => Assert.Equal(InMemory(xsl, xml), Streamed(xsl, xml));

    [Fact]
    public void ForEach_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <item id="{@id}"><xsl:value-of select="name"/>|<xsl:value-of select="price"/></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void ApplyTemplates_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/"><out><xsl:apply-templates select="/inventory"/></out></xsl:template>
              <xsl:template match="inventory"><xsl:apply-templates/></xsl:template>
              <xsl:template match="product"><item pos="{position()}"><xsl:value-of select="name"/></item></xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void ApplyTemplatesMatchAbsolutePattern_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
              <xsl:template match="/inventory/product"><item><xsl:value-of select="name"/></item></xsl:template>
              <xsl:template match="text()"/>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void PredicateAndPosition_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product[@id != '2']">
                  <item pos="{position()}"><xsl:value-of select="name"/></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void CopyOf_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product"><xsl:copy-of select="."/></xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void Sort_BuffersButIsCorrect_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <xsl:sort select="price" data-type="number" order="descending"/>
                  <item><xsl:value-of select="name"/></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void DescendantScan_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="//name"><n><xsl:value-of select="."/></n></xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void StripSpace_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:strip-space elements="*"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <item><xsl:value-of select="name"/>|<xsl:value-of select="count(text())"/></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """,
            "<inventory>\n  <product id=\"1\">\n    <name>Hammer</name>\n  </product>\n  <product id=\"2\">\n    <name>Saw</name>\n  </product>\n</inventory>");

    [Fact]
    public void PreserveSpace_WinsOverStripSpace_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:strip-space elements="*"/>
              <xsl:preserve-space elements="name"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <item><xsl:value-of select="count(name/text())"/></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """,
            "<inventory>\n  <product id=\"1\">\n    <name>  Hammer  </name>\n  </product>\n</inventory>");

    [Fact]
    public void ChooseAndLiteralElements_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <xsl:choose>
                    <xsl:when test="number(price) &gt; 15"><expensive><xsl:value-of select="name"/></expensive></xsl:when>
                    <xsl:otherwise><cheap><xsl:value-of select="name"/></cheap></xsl:otherwise>
                  </xsl:choose>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """, Xml);

    [Fact]
    public void LastFunction_OverStream_RaisesStreamingError()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product">
                  <item><xsl:value-of select="name"/><xsl:if test="position() = last()">!</xsl:if></item>
                </xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, Xml));
        Assert.Contains("fn:last()", ex.Message);
    }

    [Fact]
    public void SecondPassOverStream_RaisesForwardOnlyError()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/inventory/product"><a/></xsl:for-each>
                <xsl:for-each select="/inventory/product"><b/></xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, Xml));
        Assert.True(
            ex.Message.Contains("forward-only") || ex.Message.Contains("single pass"),
            $"unexpected message: {ex.Message}");
    }

    [Fact]
    public void AccumulatorStylesheet_IsSupported()
    {
        // Phase B: accumulators over a streamed source are supported (push-style).
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:accumulator name="total" initial-value="0">
                <xsl:accumulator-rule match="price" select="$value + number(.)"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="total"/>
              <xsl:template match="price"><p n="{accumulator-before('total')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, Xml);
        Assert.Contains("<p n=", result);
    }

    [Fact]
    public void BoundedMemoryOverLargeStream()
    {
        const int recordCount = 500_000;
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("<records>");
            for (int i = 0; i < recordCount; i++)
                writer.Write($"<record id=\"{i}\"><value>item-{i}</value></record>");
            writer.Write("</records>");
        }
        stream.Position = 0;

        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:for-each select="/records/record"><xsl:if test="position() mod 100000 = 0"><v/></xsl:if></xsl:for-each></out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetTotalMemory(true);

        var result = new XsltCompiler().Compile(xsl).TransformStreaming(stream);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long growth = GC.GetTotalMemory(true) - before;

        int vCount = 0;
        foreach (var descendant in result.NodeValue!.Axis(XdmAxis.Descendant))
            if (descendant.NodeValue!.LocalName == "v")
                vCount++;

        Assert.Equal(5, vCount);
        // Input side stays bounded: only the tiny result tree is retained.
        Assert.True(growth < 50_000_000, $"live growth {growth / 1_000_000.0:F1} MB exceeds the bounded-memory budget");
    }
}
