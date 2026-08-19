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
        Assert.Contains("output", names);
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
}
