// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Unit tests for the language-server XQuery support.
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

public class XQueryHandlerTests
{
    private const string SampleXQuery = """
        declare variable $greeting := "hello";
        declare function local:greet($name as xs:string) as xs:string { $greeting || " " || $name };
        local:greet("world")
        """;

    [Fact]
    public async System.Threading.Tasks.Task EvaluateXQueryReturnsResult()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        documents.Update(uri, SampleXQuery);

        var handler = new EvaluateXQueryHandler(documents);
        var result = await handler.Handle(new EvaluateXQueryParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq"))
        }, default);

        Assert.Null(result.Error);
        Assert.Equal("hello world", result.Result);
    }

    [Fact]
    public async System.Threading.Tasks.Task EvaluateXQueryReturnsErrorForInvalidQuery()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        documents.Update(uri, "for $x in 1 to");

        var handler = new EvaluateXQueryHandler(documents);
        var result = await handler.Handle(new EvaluateXQueryParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq"))
        }, default);

        Assert.Null(result.Result);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void FindXQueryDefinitionFindsFunction()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq");
        var location = DefinitionHandler.FindXQueryDefinition(SampleXQuery, "local:greet", uri);
        Assert.NotNull(location);
        Assert.Equal(uri, location!.Uri);
    }

    [Fact]
    public void FindXQueryDefinitionFindsVariable()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq");
        var location = DefinitionHandler.FindXQueryDefinition(SampleXQuery, "$greeting", uri);
        Assert.NotNull(location);
    }

    [Fact]
    public void DocumentSymbolsFindsXQueryDeclarations()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        documents.Update(uri, SampleXQuery);

        var handler = new DocumentSymbolHandler(documents);
        var result = handler.Handle(new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq"))
        }, default).GetAwaiter().GetResult();

        var names = result.Select(s => s.DocumentSymbol?.Name ?? s.SymbolInformation?.Name).ToList();
        Assert.Contains("variable $greeting", names);
        Assert.Contains("function local:greet", names);
    }
}
