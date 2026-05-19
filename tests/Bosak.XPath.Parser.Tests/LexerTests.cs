// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for the XPath lexer tokenization pipeline.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Parser.Lexer;
using Xunit;

namespace Bosak.XPath.Parser.Tests;

public class LexerTests
{
    private static Token[] Tokenize(string xpath)
    {
        var lexer = new XPathLexer(xpath);
        var tokens = new List<Token>();
        while (true)
        {
            var tok = lexer.NextToken();
            tokens.Add(tok);
            if (tok.Kind == TokenKind.Eof) break;
        }
        return tokens.ToArray();
    }

    [Fact]
    public void Tokenize_IntegerLiteral()
    {
        var tokens = Tokenize("42");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenKind.IntegerLiteral, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Start);
        Assert.Equal(2, tokens[0].Length);
    }

    [Fact]
    public void Tokenize_StringLiteral_SingleQuote()
    {
        var tokens = Tokenize("'hello'");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenKind.StringLiteral, tokens[0].Kind);
    }

    [Fact]
    public void Tokenize_StringLiteral_DoubleQuote()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenKind.StringLiteral, tokens[0].Kind);
    }

    [Fact]
    public void Tokenize_VariableReference()
    {
        var tokens = Tokenize("$name");
        Assert.Equal(3, tokens.Length); // Dollar, Name, Eof
        Assert.Equal(TokenKind.Dollar, tokens[0].Kind);
        Assert.Equal(TokenKind.Name, tokens[1].Kind);
    }

    [Fact]
    public void Tokenize_QName()
    {
        var tokens = Tokenize("fn:string");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenKind.Name, tokens[0].Kind);
    }

    [Fact]
    public void Tokenize_Operators()
    {
        var tokens = Tokenize("+ - * div idiv mod = != < <= > >= eq ne lt le gt ge is << >> to || | union intersect except");
        var kinds = tokens.Where(t => t.Kind != TokenKind.Eof).Select(t => t.Kind).ToArray();
        Assert.Contains(TokenKind.Plus, kinds);
        Assert.Contains(TokenKind.Minus, kinds);
        Assert.Contains(TokenKind.Star, kinds);
        Assert.Contains(TokenKind.KeywordDiv, kinds);
        Assert.Contains(TokenKind.KeywordIdiv, kinds);
        Assert.Contains(TokenKind.KeywordMod, kinds);
        Assert.Contains(TokenKind.Equal, kinds);
        Assert.Contains(TokenKind.NotEqual, kinds);
        Assert.Contains(TokenKind.KeywordUnion, kinds);
        Assert.Contains(TokenKind.KeywordIntersect, kinds);
        Assert.Contains(TokenKind.KeywordExcept, kinds);
    }

    [Fact]
    public void Tokenize_KeywordVsName()
    {
        var tokens = Tokenize("for $x in $y return $x");
        var kinds = tokens.Where(t => t.Kind != TokenKind.Eof).Select(t => t.Kind).ToArray();
        Assert.Equal(TokenKind.KeywordFor, kinds[0]);
        Assert.Equal(TokenKind.Dollar, kinds[1]);
        Assert.Equal(TokenKind.Name, kinds[2]);
        Assert.Equal(TokenKind.KeywordIn, kinds[3]);
        Assert.Equal(TokenKind.Dollar, kinds[4]);
        Assert.Equal(TokenKind.Name, kinds[5]);
        Assert.Equal(TokenKind.KeywordReturn, kinds[6]);
        Assert.Equal(TokenKind.Dollar, kinds[7]);
        Assert.Equal(TokenKind.Name, kinds[8]);
    }

    [Fact]
    public void Tokenize_MapArrayKeywords()
    {
        var tokens = Tokenize("map { } array { } [ ]");
        var kinds = tokens.Where(t => t.Kind != TokenKind.Eof).Select(t => t.Kind).ToArray();
        Assert.Contains(TokenKind.KeywordMap, kinds);
        Assert.Contains(TokenKind.KeywordArray, kinds);
        Assert.Contains(TokenKind.LBracket, kinds);
        Assert.Contains(TokenKind.RBracket, kinds);
    }

    [Fact]
    public void Tokenize_Comment_Skipped()
    {
        var tokens = Tokenize("(: this is a comment :) 42");
        var kinds = tokens.Where(t => t.Kind != TokenKind.Eof).Select(t => t.Kind).ToArray();
        Assert.Single(kinds);
        Assert.Equal(TokenKind.IntegerLiteral, kinds[0]);
    }
}
