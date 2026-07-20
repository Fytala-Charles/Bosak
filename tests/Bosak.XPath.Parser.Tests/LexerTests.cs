// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for LexerTests in the Development project
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-06-2026     | Update IntegerFollowedByDot for decimal-literal grammar                                |
//                      | Charles Korthout | 0.3   | 19-07-2026     | NumericLiteral+keyword boundary test (10idiv → Invalid)                                |
//                      | Charles Korthout | 0.4   | 20-07-2026     | Unterminated comment regression tests                                                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Parser.Lexer;
using Xunit;

namespace Bosak.XPath.Parser.Tests;

public class LexerTests
{
    private static Token[] Tokenize(string xpath)
    {
        var lexer = new XPathLexer(xpath.AsSpan());
        var tokens = new List<Token>();
        Token tok;
        while ((tok = lexer.NextToken()).Kind != TokenKind.Eof)
        {
            tokens.Add(tok);
        }
        return tokens.ToArray();
    }

    private static void AssertToken(Token token, TokenKind kind, string text, string source)
    {
        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text(source.AsSpan()).ToString());
    }

    [Fact]
    public void EmptyString_YieldsEof()
    {
        var lexer = new XPathLexer("");
        Assert.Equal(TokenKind.Eof, lexer.NextToken().Kind);
    }

    [Fact]
    public void Whitespace_IsSkipped()
    {
        var toks = Tokenize("  \t\n  ");
        Assert.Empty(toks);
    }

    [Fact]
    public void SimpleComment_IsSkipped()
    {
        var toks = Tokenize("(: this is a comment :) 42");
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.IntegerLiteral, "42", "(: this is a comment :) 42");
    }

    [Fact]
    public void NestedComments_AreSkipped()
    {
        var src = "(: outer (: inner :) still outer :) foo";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.Name, "foo", src);
    }

    [Fact]
    public void UnterminatedComment_AfterExpression_RaisesXPST0003()
    {
        // Regression for QT3 K-XQueryComment-14.
        var src = "1(: this comment does not end";
        var ex = Assert.Throws<ParseException>(() => Tokenize(src));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void NestedUnterminatedComment_AfterExpression_RaisesXPST0003()
    {
        // Regression for QT3 K-XQueryComment-15: inner comment opened but outer never closed.
        var src = "1(: content (: this comment does not end :)";
        var ex = Assert.Throws<ParseException>(() => Tokenize(src));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void StringLiteral_DoubleQuotes()
    {
        var src = "\"hello world\"";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.StringLiteral, "\"hello world\"", src);
    }

    [Fact]
    public void StringLiteral_SingleQuotes()
    {
        var src = "'hello world'";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.StringLiteral, "'hello world'", src);
    }

    [Fact]
    public void StringLiteral_EscapedQuotes()
    {
        var src = "\"He said \"\"hello\"\"\"";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.StringLiteral, "\"He said \"\"hello\"\"\"", src);
    }

    [Fact]
    public void IntegerLiteral()
    {
        var src = "12345";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.IntegerLiteral, "12345", src);
    }

    [Fact]
    public void DecimalLiteral_DotLeading()
    {
        var src = ".5";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DecimalLiteral, ".5", src);
    }

    [Fact]
    public void DecimalLiteral_BothSides()
    {
        var src = "123.456";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DecimalLiteral, "123.456", src);
    }

    [Fact]
    public void IntegerFollowedByDot()
    {
        // Per the XPath grammar, "123." is a single decimal literal, not an integer
        // followed by a path step. The trailing dot is part of the literal.
        var src = "123.";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DecimalLiteral, "123.", src);
    }

    [Fact]
    public void DoubleLiteral_LowerE()
    {
        var src = "1.5e10";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DoubleLiteral, "1.5e10", src);
    }

    [Fact]
    public void DoubleLiteral_UpperENegative()
    {
        var src = "42E-3";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DoubleLiteral, "42E-3", src);
    }

    [Fact]
    public void DoubleLiteral_PositiveExponent()
    {
        var src = "1e+6";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.DoubleLiteral, "1e+6", src);
    }

    [Fact]
    public void Names_And_Keywords()
    {
        var src = "and or div idiv mod union intersect except to instance of treat as castable cast if then else for in return some every satisfies function map array";
        var toks = Tokenize(src);
        Assert.Equal(27, toks.Length);
        Assert.Equal(TokenKind.KeywordAnd, toks[0].Kind);
        Assert.Equal(TokenKind.KeywordOr, toks[1].Kind);
        Assert.Equal(TokenKind.KeywordDiv, toks[2].Kind);
        Assert.Equal(TokenKind.KeywordIdiv, toks[3].Kind);
        Assert.Equal(TokenKind.KeywordMod, toks[4].Kind);
        Assert.Equal(TokenKind.KeywordUnion, toks[5].Kind);
        Assert.Equal(TokenKind.KeywordIntersect, toks[6].Kind);
        Assert.Equal(TokenKind.KeywordExcept, toks[7].Kind);
        Assert.Equal(TokenKind.KeywordTo, toks[8].Kind);
        Assert.Equal(TokenKind.KeywordInstance, toks[9].Kind);
        Assert.Equal(TokenKind.KeywordOf, toks[10].Kind);
        Assert.Equal(TokenKind.KeywordTreat, toks[11].Kind);
        Assert.Equal(TokenKind.KeywordAs, toks[12].Kind);
        Assert.Equal(TokenKind.KeywordCastable, toks[13].Kind);
        Assert.Equal(TokenKind.KeywordCast, toks[14].Kind);
        Assert.Equal(TokenKind.KeywordIf, toks[15].Kind);
        Assert.Equal(TokenKind.KeywordThen, toks[16].Kind);
        Assert.Equal(TokenKind.KeywordElse, toks[17].Kind);
        Assert.Equal(TokenKind.KeywordFor, toks[18].Kind);
        Assert.Equal(TokenKind.KeywordIn, toks[19].Kind);
        Assert.Equal(TokenKind.KeywordReturn, toks[20].Kind);
        Assert.Equal(TokenKind.KeywordSome, toks[21].Kind);
        Assert.Equal(TokenKind.KeywordEvery, toks[22].Kind);
        Assert.Equal(TokenKind.KeywordSatisfies, toks[23].Kind);
        Assert.Equal(TokenKind.KeywordFunction, toks[24].Kind);
        Assert.Equal(TokenKind.KeywordMap, toks[25].Kind);
        Assert.Equal(TokenKind.KeywordArray, toks[26].Kind);
    }

    [Fact]
    public void NumericLiteral_FollowedByKeyword_IsInvalid()
    {
        var src = "10idiv 3";
        var toks = Tokenize(src);
        Assert.Equal(2, toks.Length);
        Assert.Equal(TokenKind.Invalid, toks[0].Kind);
        Assert.Equal(TokenKind.IntegerLiteral, toks[1].Kind);
    }

    [Fact]
    public void ValueComparisons_Keywords()
    {
        var src = "eq ne lt le gt ge is";
        var toks = Tokenize(src);
        Assert.Equal(7, toks.Length);
        Assert.Equal(TokenKind.ValueEq, toks[0].Kind);
        Assert.Equal(TokenKind.ValueNe, toks[1].Kind);
        Assert.Equal(TokenKind.ValueLt, toks[2].Kind);
        Assert.Equal(TokenKind.ValueLe, toks[3].Kind);
        Assert.Equal(TokenKind.ValueGt, toks[4].Kind);
        Assert.Equal(TokenKind.ValueGe, toks[5].Kind);
        Assert.Equal(TokenKind.ValueIs, toks[6].Kind);
    }

    [Fact]
    public void QName_TokenizedAsSingleName()
    {
        var src = "xs:string";
        var toks = Tokenize(src);
        Assert.Single(toks);
        AssertToken(toks[0], TokenKind.Name, "xs:string", src);
    }

    [Fact]
    public void PrefixWildcard_TokenizedAsThreeTokens()
    {
        var src = "prefix:*";
        var toks = Tokenize(src);
        Assert.Equal(3, toks.Length);
        AssertToken(toks[0], TokenKind.Name, "prefix", src);
        AssertToken(toks[1], TokenKind.Colon, ":", src);
        AssertToken(toks[2], TokenKind.Star, "*", src);
    }

    [Fact]
    public void LocalWildcard_TokenizedAsThreeTokens()
    {
        var src = "*:local";
        var toks = Tokenize(src);
        Assert.Equal(3, toks.Length);
        AssertToken(toks[0], TokenKind.Star, "*", src);
        AssertToken(toks[1], TokenKind.Colon, ":", src);
        AssertToken(toks[2], TokenKind.Name, "local", src);
    }

    [Fact]
    public void AxisSeparator_TokenizedCorrectly()
    {
        var src = "child::foo";
        var toks = Tokenize(src);
        Assert.Equal(3, toks.Length);
        AssertToken(toks[0], TokenKind.Name, "child", src);
        AssertToken(toks[1], TokenKind.DoubleColon, "::", src);
        AssertToken(toks[2], TokenKind.Name, "foo", src);
    }

    [Fact]
    public void Star_Alone_IsStarToken()
    {
        var src = "*";
        var toks = Tokenize(src);
        Assert.Single(toks);
        Assert.Equal(TokenKind.Star, toks[0].Kind);
    }

    [Fact]
    public void Operators()
    {
        var src = "+ - / // | , . .. @ $ = != < <= > >= << >> || => : :: { } [ ] ( ) ? # ! ;";
        var toks = Tokenize(src);
        Assert.Equal(32, toks.Length);
        Assert.Equal(TokenKind.Plus, toks[0].Kind);
        Assert.Equal(TokenKind.Minus, toks[1].Kind);
        Assert.Equal(TokenKind.Slash, toks[2].Kind);
        Assert.Equal(TokenKind.SlashSlash, toks[3].Kind);
        Assert.Equal(TokenKind.VBar, toks[4].Kind);
        Assert.Equal(TokenKind.Comma, toks[5].Kind);
        Assert.Equal(TokenKind.Dot, toks[6].Kind);
        Assert.Equal(TokenKind.DotDot, toks[7].Kind);
        Assert.Equal(TokenKind.At, toks[8].Kind);
        Assert.Equal(TokenKind.Dollar, toks[9].Kind);
        Assert.Equal(TokenKind.Equal, toks[10].Kind);
        Assert.Equal(TokenKind.NotEqual, toks[11].Kind);
        Assert.Equal(TokenKind.LessThan, toks[12].Kind);
        Assert.Equal(TokenKind.LessThanOrEqual, toks[13].Kind);
        Assert.Equal(TokenKind.GreaterThan, toks[14].Kind);
        Assert.Equal(TokenKind.GreaterThanOrEqual, toks[15].Kind);
        Assert.Equal(TokenKind.NodeBefore, toks[16].Kind);
        Assert.Equal(TokenKind.NodeAfter, toks[17].Kind);
        Assert.Equal(TokenKind.StringConcat, toks[18].Kind);
        Assert.Equal(TokenKind.Arrow, toks[19].Kind);
        Assert.Equal(TokenKind.Colon, toks[20].Kind);
        Assert.Equal(TokenKind.DoubleColon, toks[21].Kind);
        Assert.Equal(TokenKind.LBrace, toks[22].Kind);
        Assert.Equal(TokenKind.RBrace, toks[23].Kind);
        Assert.Equal(TokenKind.LBracket, toks[24].Kind);
        Assert.Equal(TokenKind.RBracket, toks[25].Kind);
        Assert.Equal(TokenKind.LParen, toks[26].Kind);
        Assert.Equal(TokenKind.RParen, toks[27].Kind);
        Assert.Equal(TokenKind.Question, toks[28].Kind);
        Assert.Equal(TokenKind.Hash, toks[29].Kind);
        Assert.Equal(TokenKind.Bang, toks[30].Kind);
        Assert.Equal(TokenKind.Semicolon, toks[31].Kind);
    }

    [Fact]
    public void ComplexExpression()
    {
        var src = "//book[price gt 10]/title";
        var toks = Tokenize(src);
        Assert.Equal(9, toks.Length);
        Assert.Equal(TokenKind.SlashSlash, toks[0].Kind);
        Assert.Equal(TokenKind.Name, toks[1].Kind);
        Assert.Equal("book", toks[1].Text(src.AsSpan()).ToString());
        Assert.Equal(TokenKind.LBracket, toks[2].Kind);
        Assert.Equal(TokenKind.Name, toks[3].Kind);
        Assert.Equal("price", toks[3].Text(src.AsSpan()).ToString());
        Assert.Equal(TokenKind.ValueGt, toks[4].Kind);
        Assert.Equal(TokenKind.IntegerLiteral, toks[5].Kind);
        Assert.Equal("10", toks[5].Text(src.AsSpan()).ToString());
        Assert.Equal(TokenKind.RBracket, toks[6].Kind);
        Assert.Equal(TokenKind.Slash, toks[7].Kind);
        Assert.Equal(TokenKind.Name, toks[8].Kind);
        Assert.Equal("title", toks[8].Text(src.AsSpan()).ToString());
    }

    [Fact]
    public void XPath31_ArrowAndBang()
    {
        var src = "$x => upper-case() ! concat(., '!')";
        var toks = Tokenize(src);
        Assert.Equal(13, toks.Length);
        Assert.Equal(TokenKind.Dollar, toks[0].Kind);
        Assert.Equal(TokenKind.Name, toks[1].Kind); // x
        Assert.Equal(TokenKind.Arrow, toks[2].Kind);
        Assert.Equal(TokenKind.Name, toks[3].Kind); // upper-case
        Assert.Equal(TokenKind.LParen, toks[4].Kind);
        Assert.Equal(TokenKind.RParen, toks[5].Kind);
        Assert.Equal(TokenKind.Bang, toks[6].Kind);
        Assert.Equal(TokenKind.Name, toks[7].Kind); // concat
        Assert.Equal(TokenKind.LParen, toks[8].Kind);
        Assert.Equal(TokenKind.Dot, toks[9].Kind);
        Assert.Equal(TokenKind.Comma, toks[10].Kind);
        Assert.Equal(TokenKind.StringLiteral, toks[11].Kind);
        Assert.Equal(TokenKind.RParen, toks[12].Kind);
    }

    [Fact]
    public void XPath31_MapConstructor()
    {
        var src = "map { 'a': 1, 'b': 2 }";
        var toks = Tokenize(src);
        Assert.Equal(10, toks.Length);
        Assert.Equal(TokenKind.KeywordMap, toks[0].Kind);
        Assert.Equal(TokenKind.LBrace, toks[1].Kind);
    }

    [Fact]
    public void XPath31_InlineFunction()
    {
        var src = "function($x) { $x + 1 }";
        var toks = Tokenize(src);
        Assert.Equal(11, toks.Length);
        Assert.Equal(TokenKind.KeywordFunction, toks[0].Kind);
        Assert.Equal(TokenKind.LParen, toks[1].Kind);
        Assert.Equal(TokenKind.Dollar, toks[2].Kind);
        Assert.Equal(TokenKind.Name, toks[3].Kind);
        Assert.Equal(TokenKind.RParen, toks[4].Kind);
    }
}
