// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Unit tests for the language-server document symbol handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 31-08-2026     | Added comprehensive XSLT document-symbol tests (REQ-073)                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Linq;
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class DocumentSymbolHandlerTests
{
    private const string SampleXslt = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
            <xsl:output method="xml"/>
            <xsl:param name="global-param"/>
            <xsl:variable name="global-var" select="1"/>
            <xsl:template match="/">
                <out/>
            </xsl:template>
            <xsl:template name="named"/>
            <xsl:function name="my:helper" xmlns:my="http://example.com/my">
                <xsl:sequence select="1"/>
            </xsl:function>
            <xsl:key name="by-id" match="item" use="@id"/>
        </xsl:stylesheet>
        """;

    [Fact]
    public async System.Threading.Tasks.Task ReturnsSymbolsForXsltTopLevelDeclarations()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/sample.xslt").ToString();
        documents.Update(uri, SampleXslt);

        var handler = new DocumentSymbolHandler(documents);
        var result = await handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/sample.xslt"))
        }, default);

        var names = result.Select(s => s.DocumentSymbol?.Name ?? s.SymbolInformation?.Name).ToList();
        Assert.Contains("output (xml)", names);
        Assert.Contains("global-param", names);
        Assert.Contains("global-var", names);
        Assert.Contains("template match=\"/\"", names);
        Assert.Contains("template named", names);
        Assert.Contains("function my:helper", names);
        Assert.Contains("key by-id", names);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsEmptyForNonXsltDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new DocumentSymbolHandler(documents);
        var result = await handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath"))
        }, default);

        Assert.Empty(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsEmptyForMalformedXml()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/broken.xslt").ToString();
        documents.Update(uri, "<xsl:stylesheet");

        var handler = new DocumentSymbolHandler(documents);
        var result = await handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/broken.xslt"))
        }, default);

        Assert.Empty(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsSymbolsForReq073TopLevelDeclarations()
    {
        var xslt = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema">
                <xsl:output method="html"/>
                <xsl:param name="req-param"/>
                <xsl:variable name="req-var" select="1"/>
                <xsl:template match="/">
                    <out/>
                </xsl:template>
                <xsl:template name="req-named"/>
                <xsl:function name="req:func" xmlns:req="http://example.com/req"/>
                <xsl:attribute-set name="req-attrs"/>
                <xsl:key name="req-key" match="item" use="@id"/>
            </xsl:stylesheet>
            """;

        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/req073.xslt").ToString();
        documents.Update(uri, xslt);

        var handler = new DocumentSymbolHandler(documents);
        var result = await handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/req073.xslt"))
        }, default);

        var names = result.Select(s => s.DocumentSymbol?.Name ?? s.SymbolInformation?.Name).ToList();
        Assert.Contains("output (html)", names);
        Assert.Contains("req-param", names);
        Assert.Contains("req-var", names);
        Assert.Contains("template match=\"/\"", names);
        Assert.Contains("template req-named", names);
        Assert.Contains("function req:func", names);
        Assert.Contains("attribute-set req-attrs", names);
        Assert.Contains("key req-key", names);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsSymbolsForAdditionalTopLevelDeclarations()
    {
        var xslt = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                <xsl:import href="common.xsl"/>
                <xsl:include href="helpers.xsl"/>
                <xsl:mode name="req-mode"/>
                <xsl:decimal-format name="req-format"/>
                <xsl:character-map name="req-map"/>
                <xsl:accumulator name="req-acc" initial-value="0"/>
            </xsl:stylesheet>
            """;

        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/extras.xslt").ToString();
        documents.Update(uri, xslt);

        var handler = new DocumentSymbolHandler(documents);
        var result = await handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/extras.xslt"))
        }, default);

        var names = result.Select(s => s.DocumentSymbol?.Name ?? s.SymbolInformation?.Name).ToList();
        Assert.Contains("import common.xsl", names);
        Assert.Contains("include helpers.xsl", names);
        Assert.Contains("mode req-mode", names);
        Assert.Contains("decimal-format req-format", names);
        Assert.Contains("character-map req-map", names);
        Assert.Contains("accumulator req-acc", names);
    }
}
