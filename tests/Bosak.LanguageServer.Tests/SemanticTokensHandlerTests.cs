// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server semantic tokens handler.
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
using System.Threading.Tasks;
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class SemanticTokensHandlerTests
{
    [Fact]
    public void Tokenize_FindsFunctionCall()
    {
        const string text = "fn:concat('a', 'b')";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: false, isXQuery: false, isXPath: true);

        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Function && text.Substring(t.Start, t.Length) == "concat");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Namespace && text.Substring(t.Start, t.Length) == "fn");
    }

    [Fact]
    public void Tokenize_FindsVariableReference()
    {
        const string text = "$greeting || $name";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: false, isXQuery: false, isXPath: true);

        var vars = tokens.Where(t => t.Type == SemanticTokenType.Variable).ToList();
        Assert.Equal(2, vars.Count);
        Assert.Contains(vars, v => text.Substring(v.Start, v.Length) == "$greeting");
        Assert.Contains(vars, v => text.Substring(v.Start, v.Length) == "$name");
    }

    [Fact]
    public void Tokenize_FindsXsltInstruction()
    {
        const string text = "<xsl:template match='/'><xsl:variable name='x' select='1'/></xsl:template>";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: true, isXQuery: false, isXPath: false);

        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Keyword && text.Substring(t.Start, t.Length) == "template");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Keyword && text.Substring(t.Start, t.Length) == "variable");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Namespace && text.Substring(t.Start, t.Length) == "xsl");
    }

    [Fact]
    public void Tokenize_FindsXQueryKeywords()
    {
        const string text = "for $i in 1 to 10 return $i";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: false, isXQuery: true, isXPath: true);

        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Keyword && text.Substring(t.Start, t.Length) == "for");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Keyword && text.Substring(t.Start, t.Length) == "in");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Keyword && text.Substring(t.Start, t.Length) == "return");
    }

    [Fact]
    public void Tokenize_FindsTypeNames()
    {
        const string text = "$x as xs:string";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: false, isXQuery: true, isXPath: true);

        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Type && text.Substring(t.Start, t.Length) == "string");
    }

    [Fact]
    public void Tokenize_FindsNumberLiterals()
    {
        const string text = "1 + 2.5";
        var tokens = SemanticTokensHandler.Tokenize(text, isXslt: false, isXQuery: false, isXPath: true);

        var numbers = tokens.Where(t => t.Type == SemanticTokenType.Number).ToList();
        Assert.Equal(2, numbers.Count);
    }

    [Fact]
    public async Task Handle_ReturnsTokensForXPathDocument()
    {
        const string text = "fn:concat($a, $b)";
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xpath").ToString();
        documents.Update(uri, text);

        var handler = new SemanticTokensHandler(documents);
        var result = await handler.Handle(new SemanticTokensParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xpath"))
        }, default);

        Assert.NotNull(result);
        Assert.True(result!.Data.Length > 0);
        Assert.Equal(0, result.Data.Length % 5);
    }
}
