// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Unit tests for the language-server evaluate/transform request handlers.
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
using System.IO;
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class EvaluationHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task EvaluateXPathReturnsResult()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new EvaluateXPathHandler(documents);
        var result = await handler.Handle(new EvaluateXPathParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath"))
        }, default);

        Assert.Null(result.Error);
        Assert.Equal("3", result.Result);
    }

    [Fact]
    public async System.Threading.Tasks.Task EvaluateXPathReturnsErrorForInvalidExpression()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 +");

        var handler = new EvaluateXPathHandler(documents);
        var result = await handler.Handle(new EvaluateXPathParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath"))
        }, default);

        Assert.Null(result.Result);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async System.Threading.Tasks.Task TransformXsltRunsStylesheet()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/style.xslt").ToString();
        documents.Update(uri, """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                <xsl:template match="/"><out><xsl:value-of select="/root/text"/></out></xsl:template>
            </xsl:stylesheet>
            """);

        var sourcePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");
        File.WriteAllText(sourcePath, "<root><text>hello</text></root>");
        try
        {
            var handler = new TransformXsltHandler(documents);
            var result = await handler.Handle(new TransformXsltParams
            {
                TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/style.xslt")),
                SourcePath = sourcePath
            }, default);

            Assert.Null(result.Error);
            Assert.NotNull(result.Result);
            Assert.Contains("hello", result.Result);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task TransformXsltRequiresSourcePath()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/style.xslt").ToString();
        documents.Update(uri, "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"/>");

        var handler = new TransformXsltHandler(documents);
        var result = await handler.Handle(new TransformXsltParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/style.xslt")),
            SourcePath = null
        }, default);

        Assert.Null(result.Result);
        Assert.NotNull(result.Error);
    }
}
