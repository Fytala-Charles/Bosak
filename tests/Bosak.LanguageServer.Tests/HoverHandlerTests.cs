// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Unit tests for the language-server hover handler.
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
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class HoverHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HoverOverFunctionShowsSignature()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        var text = "fn:concat('a', 'b')";
        documents.Update(uri, text);

        var handler = new HoverHandler(documents);
        // Position over "concat" (line 0, character 5).
        var hover = await handler.Handle(new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Position = new Position(0, 5)
        }, default);

        Assert.NotNull(hover);
        var content = hover!.Contents.MarkupContent?.Value;
        Assert.NotNull(content);
        Assert.Contains("fn:concat", content);
        Assert.Contains("xs:string", content);
    }

    [Fact]
    public async System.Threading.Tasks.Task HoverOverUnknownNameReturnsNull()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var handler = new HoverHandler(documents);
        var hover = await handler.Handle(new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Position = new Position(0, 0)
        }, default);

        Assert.Null(hover);
    }

    [Fact]
    public void GetWordAtExtractsPrefixedName()
    {
        var text = "fn:concat('a', 'b')";
        Assert.Equal("fn:concat", HoverHandler.GetWordAt(text, 0, 6));
        Assert.Equal("fn:concat", HoverHandler.GetWordAt(text, 0, 2));
    }

    [Fact]
    public void GetWordAtReturnsNullForEmptyPosition()
    {
        Assert.Null(HoverHandler.GetWordAt("", 0, 0));
        // The '+' operator is not a name.
        Assert.Null(HoverHandler.GetWordAt("1 + 2", 0, 2));
    }
}
