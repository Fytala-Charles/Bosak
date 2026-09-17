// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for streamable xsl:source-document (burst-mode mid-transform loads)
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
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for <c>xsl:source-document streamable="yes"</c> (streaming Phase C): the content
/// constructor sees a forward-only burst-mode document; output parity with the in-memory
/// source-document, fragment identifiers, whitespace stripping, and accumulators.
/// </summary>
public class StreamingSourceDocumentTests : IDisposable
{
    private const string SourceXml = """
        <inventory>
        <product id="1"><name>Hammer</name><price>9.99</price></product>
        <product id="2"><name>Saw</name><price>19.99</price></product>
        <product id="3"><name xml:id="featured">Drill</name><price>29.99</price></product>
        </inventory>
        """;

    private readonly string _sourcePath;

    public StreamingSourceDocumentTests()
    {
        _sourcePath = Path.Combine(Path.GetTempPath(), $"bosak-sd-{Guid.NewGuid():N}.xml");
        File.WriteAllText(_sourcePath, SourceXml, Encoding.UTF8);
    }

    public void Dispose()
    {
        try { File.Delete(_sourcePath); } catch { /* best-effort cleanup */ }
    }

    private const string PrincipalXml = "<dummy/>";

    private string Transform(string sourceDocumentElement)
    {
        var xsl = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out>{sourceDocumentElement}</out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        return new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(PrincipalXml)).NodeValue!.ToXmlString();
    }

    private string SourceDocument(string extraAttrs, string content)
        => Transform($"<xsl:source-document href=\"{_sourcePath}\" {extraAttrs}>{content}</xsl:source-document>");

    [Fact]
    public void ForEach_ParityWithInMemorySourceDocument()
    {
        const string content = """
            <xsl:for-each select="*/product"><item id="{@id}"><xsl:value-of select="name"/>|<xsl:value-of select="price"/></item></xsl:for-each>
            """;
        var streamed = SourceDocument("streamable=\"yes\"", content);
        var inMemory = SourceDocument("", content);
        Assert.Equal(inMemory, streamed);
        Assert.Contains("<item id=\"1\">Hammer|9.99</item>", streamed);
        Assert.Contains("<item id=\"3\">Drill|29.99</item>", streamed);
    }

    [Fact]
    public void DescendantScan_Parity()
    {
        const string content = "<xsl:for-each select=\"//name\"><n><xsl:value-of select=\".\"/></n></xsl:for-each>";
        var streamed = SourceDocument("streamable=\"true\"", content);
        var inMemory = SourceDocument("", content);
        Assert.Equal(inMemory, streamed);
    }

    [Fact]
    public void FragmentIdentifier_SelectsElementByXmlId()
    {
        var xsl = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:template match="/">
                <out><xsl:source-document href="{_sourcePath}#featured" streamable="yes">
                  <xsl:copy-of select="."/>
                </xsl:source-document></out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var result = new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(PrincipalXml)).NodeValue!.ToXmlString();
        Assert.Contains("Drill", result);
        Assert.DoesNotContain("Hammer", result);
        Assert.DoesNotContain("Saw", result);
    }

    [Fact]
    public void StripSpace_DropsWhitespaceTextRecords()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-sd-ws-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, "<inventory>\n  <product id=\"1\">\n    <name>Hammer</name>\n  </product>\n</inventory>", Encoding.UTF8);
        try
        {
            var xsl = $$"""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:output method="xml" indent="no"/>
                  <xsl:strip-space elements="*"/>
                  <xsl:template match="/">
                    <out><xsl:source-document href="{{path}}" streamable="yes">
                      <xsl:for-each select="*/product"><item><xsl:value-of select="count(text())"/>|<xsl:value-of select="count(name/text())"/></item></xsl:for-each>
                    </xsl:source-document></out>
                  </xsl:template>
                </xsl:stylesheet>
                """;
            var result = new XsltCompiler().Compile(xsl)
                .Transform(XDocumentProvider.ParseXml(PrincipalXml)).NodeValue!.ToXmlString();
            Assert.Contains("<item>0|1</item>", result);
        }
        finally
        {
            try { File.Delete(path); } catch { /* best-effort cleanup */ }
        }
    }

    [Fact]
    public void Accumulator_PushedPerRecord()
    {
        var xsl = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="seq" initial-value="0">
                <xsl:accumulator-rule match="product" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="seq"/>
              <xsl:template match="/">
                <out><xsl:source-document href="{{_sourcePath}}" streamable="yes">
                  <xsl:for-each select="*/product"><r n="{accumulator-before('seq')}"/></xsl:for-each>
                </xsl:source-document></out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var result = new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(PrincipalXml)).NodeValue!.ToXmlString();
        Assert.Contains("n=\"1\"", result);
        Assert.Contains("n=\"2\"", result);
        Assert.Contains("n=\"3\"", result);
    }
}
