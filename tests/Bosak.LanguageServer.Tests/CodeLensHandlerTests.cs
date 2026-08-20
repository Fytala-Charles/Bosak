// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server XPath code-lens handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
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
    public async System.Threading.Tasks.Task ReturnsEmptyForNonXPathDocument()
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
}
