// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server XPath and XQuery code-lens handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 20-08-2026     | Added XQuery lens tests                                                                  |
//                      | Charles Korthout | 0.3   | 20-08-2026     | Added XSLT lens tests                                                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Linq;
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class CodeLensHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task ReturnsLensWithXPathResultForXPathDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.StartsWith("= 3", lens.Command!.Title);
        Assert.Equal("bosak.evaluateXPath", lens.Command.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsLensWithXQueryResultForXQueryDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xquery").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xquery"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.StartsWith("= 3", lens.Command!.Title);
        Assert.Equal("bosak.evaluateXQuery", lens.Command.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsXsltTransformLensForXslDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xsl").ToString();
        documents.Update(uri, "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xsl"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.Equal("Run XSLT transformation", lens.Command!.Title);
        Assert.Equal("bosak.transformXslt", lens.Command.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsXsltTransformLensForXsltDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        documents.Update(uri, "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.Equal("Run XSLT transformation", lens.Command!.Title);
        Assert.Equal("bosak.transformXslt", lens.Command.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsEmptyForUnsupportedDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/notes.txt").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/notes.txt"))
        }, default);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsLensWithErrorForInvalidExpression()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 +");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.StartsWith("XPath error:", lens.Command!.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsLensWithErrorForInvalidXQuery()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        documents.Update(uri, "for $x in 1 to");

        var handler = new CodeLensHandler(documents);
        var result = await handler.Handle(new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq"))
        }, default);

        Assert.NotNull(result);
        var lens = Assert.Single(result!);
        Assert.NotNull(lens.Command);
        Assert.StartsWith("XQuery error:", lens.Command!.Title);
    }
}
