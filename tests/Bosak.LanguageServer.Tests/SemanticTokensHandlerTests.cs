// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server semantic tokens handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1.1 | 14-10-2026     | Verify expression scopes, functions, strings and XML exclusions.                             |
//                      | Charles Korthout | 0.1.2 | 18-10-2026     | Correct language-server nullability and async/analyzer diagnostics.                       |
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


    [Theory]
    [InlineData("<xsl:value-of select=\"concat('a','b')\"/>")]
    /// <summary>Verifies distinct function and string tokens inside either XML quote style.</summary>
    /// <param name="text">The stylesheet fragment to tokenize.</param>
    [InlineData("<xsl:value-of select='concat(\"a\",\"b\")'/>")]
    public void Tokenize_XsltSeparatesFunctionAndStringArguments(string text)
    {
        var tokens = SemanticTokensHandler.Tokenize(text, true, false, true);
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Function && text.Substring(t.Start, t.Length) == "concat");
        Assert.Equal(2, tokens.Count(t => t.Type == SemanticTokenType.String));
        int attributeName = text.IndexOf("select");
        Assert.DoesNotContain(tokens, t => t.Start <= attributeName && t.Start + t.Length > attributeName);
        for (int i = 1; i < tokens.Count; i++)
            Assert.True(tokens[i].Start >= tokens[i - 1].Start + tokens[i - 1].Length);
    }

    /// <summary>Verifies literal XML values, comments and text do not receive XPath tokens.</summary>
    [Fact]
    public void Tokenize_XsltIgnoresLiteralValuesCommentsAndText()
    {
        const string text = "<!-- <xsl:value-of select=\"bad(123)\"/> -->"
            + "<xsl:include href=\"file:///for/123/fn:concat()\"/>"
            + "<result select=\"fake(456)\" title=\"if $x fn:concat()\">fn:concat(789)</result>";
        var tokens = SemanticTokensHandler.Tokenize(text, true, false, true);
        Assert.DoesNotContain(tokens, t => t.Type is "function" or "number" or "variable");
        Assert.Single(tokens, t => t.Type == SemanticTokenType.Keyword);
    }

    /// <summary>Verifies string contents and nested comments do not emit code tokens.</summary>
    [Fact]
    public void Tokenize_XPathIgnoresCodeInsideStringsAndNestedComments()
    {
        const string text = "concat('fn:fake(123) $x and', (: outer (: inner :) fn:fake(456) :) 'it''s')";
        var tokens = SemanticTokensHandler.Tokenize(text, false, false, true);
        Assert.Single(tokens, t => t.Type == SemanticTokenType.Function);
        Assert.Equal(2, tokens.Count(t => t.Type == SemanticTokenType.String));
        Assert.DoesNotContain(tokens, t => t.Type is "number" or "variable" or "operator");
    }

    /// <summary>Verifies multiline expression offsets and single-line semantic token ranges.</summary>
    [Fact]
    public void Tokenize_XsltPreservesMultilineOffsetsAndSingleLineTokens()
    {
        const string text = "<xsl:value-of\r\n select=\"concat(\r\n 'a\r\nb', $value)\"/>";
        var tokens = SemanticTokensHandler.Tokenize(text, true, false, true);
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Function && text.Substring(t.Start, t.Length) == "concat");
        Assert.Contains(tokens, t => t.Type == SemanticTokenType.Variable && text.Substring(t.Start, t.Length) == "$value");
        Assert.All(tokens, t => Assert.DoesNotContain('\n', text.Substring(t.Start, t.Length)));
        Assert.All(tokens, t => Assert.DoesNotContain('\r', text.Substring(t.Start, t.Length)));
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
