// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Recursive-descent parser for XPath 3.1. Consumes tokens from <see cref="XPathLexer"/> and produce...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added SimpleMap (!) expression parsing                                                 |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added string literal lookup keys and LookupWildcardNode                                |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Parse sequence type occurrence indicators (*, +, ?)                                    |
//                      | Charles Korthout | 0.5   | 19-05-2026     | Support inline functions as arrow expression targets                                   |
//                      | Charles Korthout | 0.6   | 21-05-2026     | Fixed SkipSequenceType RParen consumption; ParseSequenceType handles item()/function(*)/empty-sequence() |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added ExpectName() to allow XPath keywords as variable names ($mod, $div, etc.)                       |
//                      | Charles Korthout | 0.8   | 31-05-2026     | decimal.TryParse fallback to double for oversized decimal literals                       |
//                      | Charles Korthout | 0.9   | 10-06-2026     | Kind-test parsing no longer treats prefixed names (e.g. my:node()) as kind tests         |
//                      | Charles Korthout | 0.9   | 01-06-2026     | Prevent map/array/function keywords from being parsed as name tests in step expr       |
//                      | Charles Korthout | 1.0   | 01-06-2026     | ParseAxisStep defaults to attribute/namespace axis for attribute()/namespace-node()    |
//                      | Charles Korthout | 1.1   | 05-06-2026     | Fixed SkipSequenceType to use token char spans; ParseTypeNameAndParens consumes function return type |
//                      | Charles Korthout | 1.2   | 05-06-2026     | Added XQST0039 duplicate parameter name check in ParseInlineFunction                     |
//                      | Charles Korthout | 1.3   | 11-06-2026     | xml prefix falls back to PrefixedName; other prefixes use QName for NamespaceTest       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Runtime.CompilerServices;
using Bosak.XPath.Core;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Lexer;

namespace Bosak.XPath.Parser.Ast;

/// <summary>
/// Recursive-descent parser for XPath 3.1.
/// Consumes tokens from <see cref="XPathLexer"/> and produces an immutable AST.
/// </summary>
public sealed class XPathParser
{
    private readonly Token[] _tokens;
    private readonly string _source;
    private int _position;

    public XPathParser(Token[] tokens, string source)
    {
        _tokens = tokens;
        _source = source;
        _position = 0;
    }

    /// <summary>
    /// Convenience method: lexes and parses an XPath expression string.
    /// </summary>
    public static XPathAstNode Parse(string xpath)
    {
        var lexer = new XPathLexer(xpath.AsSpan());
        var tokens = new List<Token>();
        Token tok;
        while ((tok = lexer.NextToken()).Kind != TokenKind.Eof)
            tokens.Add(tok);

        var parser = new XPathParser(tokens.ToArray(), xpath);
        return parser.ParseExpression();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private Token Current => _position < _tokens.Length ? _tokens[_position] : Token.Eof;
    private Token Peek(int offset) => _position + offset < _tokens.Length ? _tokens[_position + offset] : Token.Eof;
    private bool IsAtEnd => _position >= _tokens.Length;

    private int End => _position > 0 ? _tokens[_position - 1].Start + _tokens[_position - 1].Length : 0;

    private void Advance() => _position++;

    private bool Match(TokenKind kind)
    {
        if (Current.Kind == kind)
        {
            _position++;
            return true;
        }
        return false;
    }

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind == kind)
        {
            var t = Current;
            _position++;
            return t;
        }
        throw new ParseException($"Expected {kind} but found {Current.Kind}", Current.Start);
    }

    /// <summary>
    /// Expects a name token, allowing XPath keywords to be used as names
    /// (e.g., variable names like $mod, $div, $and).
    /// </summary>
    private Token ExpectName()
    {
        if (Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind))
        {
            var t = Current;
            _position++;
            return t;
        }
        throw new ParseException($"Expected name but found {Current.Kind}", Current.Start);
    }

    private static bool IsKeywordName(TokenKind kind) => kind switch
    {
        TokenKind.KeywordAnd or TokenKind.KeywordOr or TokenKind.KeywordDiv
        or TokenKind.KeywordIdiv or TokenKind.KeywordMod or TokenKind.KeywordUnion
        or TokenKind.KeywordIntersect or TokenKind.KeywordExcept or TokenKind.KeywordTo
        or TokenKind.KeywordInstance or TokenKind.KeywordOf or TokenKind.KeywordTreat
        or TokenKind.KeywordAs or TokenKind.KeywordCastable or TokenKind.KeywordCast
        or TokenKind.KeywordIf or TokenKind.KeywordThen or TokenKind.KeywordElse
        or TokenKind.KeywordFor or TokenKind.KeywordLet or TokenKind.KeywordIn
        or TokenKind.KeywordReturn or TokenKind.KeywordSome or TokenKind.KeywordEvery
        or TokenKind.KeywordSatisfies or TokenKind.KeywordFunction or TokenKind.KeywordMap
        or TokenKind.KeywordArray or TokenKind.KeywordTry or TokenKind.KeywordCatch
        or TokenKind.ValueEq or TokenKind.ValueNe or TokenKind.ValueLt
        or TokenKind.ValueLe or TokenKind.ValueGt or TokenKind.ValueGe
        or TokenKind.ValueIs => true,
        _ => false
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T WithSpan<T>(T node, int start, int end) where T : XPathAstNode
        => node with { Span = new TextSpan(start, end - start) };

    private ReadOnlySpan<char> GetText(Token token) => token.Text(_source.AsSpan());
    private string GetString(Token token) => token.Text(_source.AsSpan()).ToString();
    private string GetSpanText(int start, int end) => _source[start..end];

    // ------------------------------------------------------------------
    // Entry point
    // ------------------------------------------------------------------

    public XPathAstNode ParseExpression()
    {
        int start = Current.Start;
        var expr = ParseExpr();
        if (!IsAtEnd)
            throw new ParseException($"Unexpected token {Current.Kind}", Current.Start);
        return expr;
    }

    // ------------------------------------------------------------------
    // Expression hierarchy (lowest → highest precedence)
    // ------------------------------------------------------------------

    // Expr ::= ExprSingle ("," ExprSingle)*
    private XPathAstNode ParseExpr()
    {
        int start = Current.Start;
        var exprs = new List<XPathAstNode> { ParseExprSingle() };
        while (Match(TokenKind.Comma))
            exprs.Add(ParseExprSingle());

        if (exprs.Count == 1) return exprs[0];
        return WithSpan(new SequenceExpressionNode(exprs), start, End);
    }

    // ExprSingle ::= ForExpr | QuantifiedExpr | IfExpr | OrExpr
    private XPathAstNode ParseExprSingle()
    {
        return Current.Kind switch
        {
            TokenKind.KeywordFor => ParseForExpr(),
            TokenKind.KeywordLet => ParseLetExpr(),
            TokenKind.KeywordSome or TokenKind.KeywordEvery => ParseQuantifiedExpr(),
            TokenKind.KeywordIf => ParseIfExpr(),
            TokenKind.KeywordTry => ParseTryExpr(),
            _ => ParseOrExpr()
        };
    }

    // ForExpr ::= SimpleForClause "return" ExprSingle
    private XPathAstNode ParseForExpr()
    {
        int start = Current.Start;
        Expect(TokenKind.KeywordFor);
        var bindings = new List<QuantifiedBinding>();
        do
        {
            bindings.Add(ParseSimpleForBinding());
        } while (Match(TokenKind.Comma));
        Expect(TokenKind.KeywordReturn);
        var body = ParseExprSingle();
        return WithSpan(new ForExpressionNode(bindings, body), start, End);
    }

    private QuantifiedBinding ParseSimpleForBinding()
    {
        Expect(TokenKind.Dollar);
        var nameTok = ExpectName();
        var (prefix, local, _) = SplitQName(GetString(nameTok));
        Expect(TokenKind.KeywordIn);
        var expr = ParseExprSingle();
        return new QuantifiedBinding(local, expr);
    }

    // LetExpr ::= SimpleLetClause "return" ExprSingle
    private XPathAstNode ParseLetExpr()
    {
        int start = Current.Start;
        Expect(TokenKind.KeywordLet);
        var bindings = new List<QuantifiedBinding>();
        do
        {
            bindings.Add(ParseSimpleLetBinding());
        } while (Match(TokenKind.Comma));
        Expect(TokenKind.KeywordReturn);
        var body = ParseExprSingle();
        return WithSpan(new LetExpressionNode(bindings, body), start, End);
    }

    private QuantifiedBinding ParseSimpleLetBinding()
    {
        Expect(TokenKind.Dollar);
        var nameTok = ExpectName();
        var (prefix, local, _) = SplitQName(GetString(nameTok));
        Expect(TokenKind.Assign);  // := 
        var expr = ParseExprSingle();
        return new QuantifiedBinding(local, expr);
    }

    // QuantifiedExpr ::= ("some" | "every") SimpleForBinding ("satisfies" ExprSingle)+
    private XPathAstNode ParseQuantifiedExpr()
    {
        int start = Current.Start;
        var quantifier = Match(TokenKind.KeywordSome) ? QuantifierKind.Some : QuantifierKind.Every;
        if (quantifier == QuantifierKind.Every) Expect(TokenKind.KeywordEvery);
        var bindings = new List<QuantifiedBinding>();
        do
        {
            bindings.Add(ParseSimpleForBinding());
        } while (Match(TokenKind.Comma));
        Expect(TokenKind.KeywordSatisfies);
        var body = ParseExprSingle();
        return WithSpan(new QuantifiedExpressionNode(quantifier, bindings, body), start, End);
    }

    // IfExpr ::= "if" "(" Expr ")" "then" ExprSingle "else" ExprSingle
    private XPathAstNode ParseIfExpr()
    {
        int start = Current.Start;
        Expect(TokenKind.KeywordIf);
        Expect(TokenKind.LParen);
        var cond = ParseExpr();
        Expect(TokenKind.RParen);
        Expect(TokenKind.KeywordThen);
        var thenBranch = ParseExprSingle();
        Expect(TokenKind.KeywordElse);
        var elseBranch = ParseExprSingle();
        return WithSpan(new IfExpressionNode(cond, thenBranch, elseBranch), start, End);
    }

    // TryExpr ::= TryClause CatchClause+
    // TryClause ::= "try" "{" Expr "}"
    // CatchClause ::= "catch" CatchErrorList "{" Expr "}"
    // For now: only "catch *" is supported
    private XPathAstNode ParseTryExpr()
    {
        int start = Current.Start;
        Expect(TokenKind.KeywordTry);
        Expect(TokenKind.LBrace);
        var tryBody = ParseExpr();
        Expect(TokenKind.RBrace);
        Expect(TokenKind.KeywordCatch);
        if (Match(TokenKind.Star))
        {
            Expect(TokenKind.LBrace);
            var catchBody = ParseExpr();
            Expect(TokenKind.RBrace);
            return WithSpan(new TryCatchNode(tryBody, catchBody), start, End);
        }
        throw new ParseException("Only catch * is currently supported", Current.Start);
    }

    // OrExpr ::= AndExpr ("or" AndExpr)*
    private XPathAstNode ParseOrExpr()
    {
        int start = Current.Start;
        var left = ParseAndExpr();
        while (Match(TokenKind.KeywordOr))
        {
            var right = ParseAndExpr();
            left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Or, right), start, End);
        }
        return left;
    }

    // AndExpr ::= ComparisonExpr ("and" ComparisonExpr)*
    private XPathAstNode ParseAndExpr()
    {
        int start = Current.Start;
        var left = ParseComparisonExpr();
        while (Match(TokenKind.KeywordAnd))
        {
            var right = ParseComparisonExpr();
            left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.And, right), start, End);
        }
        return left;
    }

    // ComparisonExpr ::= StringConcatExpr (ComparisonOp StringConcatExpr)?
    private XPathAstNode ParseComparisonExpr()
    {
        int start = Current.Start;
        var left = ParseStringConcatExpr();
        BinaryOperator? op = Current.Kind switch
        {
            TokenKind.Equal => BinaryOperator.Equal,
            TokenKind.NotEqual => BinaryOperator.NotEqual,
            TokenKind.LessThan => BinaryOperator.LessThan,
            TokenKind.LessThanOrEqual => BinaryOperator.LessThanOrEqual,
            TokenKind.GreaterThan => BinaryOperator.GreaterThan,
            TokenKind.GreaterThanOrEqual => BinaryOperator.GreaterThanOrEqual,
            TokenKind.ValueEq => BinaryOperator.Eq,
            TokenKind.ValueNe => BinaryOperator.Ne,
            TokenKind.ValueLt => BinaryOperator.Lt,
            TokenKind.ValueLe => BinaryOperator.Le,
            TokenKind.ValueGt => BinaryOperator.Gt,
            TokenKind.ValueGe => BinaryOperator.Ge,
            TokenKind.ValueIs => BinaryOperator.Is,
            TokenKind.NodeBefore => BinaryOperator.Precedes,
            TokenKind.NodeAfter => BinaryOperator.Follows,
            _ => null
        };
        if (op.HasValue)
        {
            Advance();
            var right = ParseStringConcatExpr();
            left = WithSpan(new BinaryExpressionNode(left, op.Value, right), start, End);
        }
        return left;
    }

    // StringConcatExpr ::= RangeExpr ("||" RangeExpr)*
    private XPathAstNode ParseStringConcatExpr()
    {
        int start = Current.Start;
        var left = ParseRangeExpr();
        while (Match(TokenKind.StringConcat))
        {
            var right = ParseRangeExpr();
            left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.StringConcat, right), start, End);
        }
        return left;
    }

    // RangeExpr ::= AdditiveExpr ("to" AdditiveExpr)?
    private XPathAstNode ParseRangeExpr()
    {
        int start = Current.Start;
        var left = ParseAdditiveExpr();
        if (Match(TokenKind.KeywordTo))
        {
            var right = ParseAdditiveExpr();
            return WithSpan(new RangeExpressionNode(left, right), start, End);
        }
        return left;
    }

    // AdditiveExpr ::= MultiplicativeExpr (("+" | "-") MultiplicativeExpr)*
    private XPathAstNode ParseAdditiveExpr()
    {
        int start = Current.Start;
        var left = ParseMultiplicativeExpr();
        while (true)
        {
            if (Match(TokenKind.Plus))
            {
                var right = ParseMultiplicativeExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Plus, right), start, End);
            }
            else if (Match(TokenKind.Minus))
            {
                var right = ParseMultiplicativeExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Minus, right), start, End);
            }
            else break;
        }
        return left;
    }

    // MultiplicativeExpr ::= UnionExpr (("*" | "div" | "idiv" | "mod") UnionExpr)*
    private XPathAstNode ParseMultiplicativeExpr()
    {
        int start = Current.Start;
        var left = ParseUnionExpr();
        while (true)
        {
            if (Match(TokenKind.Star))
            {
                var right = ParseUnionExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Multiply, right), start, End);
            }
            else if (Match(TokenKind.KeywordDiv))
            {
                var right = ParseUnionExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Divide, right), start, End);
            }
            else if (Match(TokenKind.KeywordIdiv))
            {
                var right = ParseUnionExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Idiv, right), start, End);
            }
            else if (Match(TokenKind.KeywordMod))
            {
                var right = ParseUnionExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Mod, right), start, End);
            }
            else break;
        }
        return left;
    }

    // UnionExpr ::= IntersectExceptExpr (("union" | "|") IntersectExceptExpr)*
    private XPathAstNode ParseUnionExpr()
    {
        int start = Current.Start;
        var left = ParseIntersectExceptExpr();
        while (true)
        {
            if (Match(TokenKind.KeywordUnion) || Match(TokenKind.VBar))
            {
                var right = ParseIntersectExceptExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Union, right), start, End);
            }
            else break;
        }
        return left;
    }

    // IntersectExceptExpr ::= InstanceofExpr (("intersect" | "except") InstanceofExpr)*
    private XPathAstNode ParseIntersectExceptExpr()
    {
        int start = Current.Start;
        var left = ParseInstanceofExpr();
        while (true)
        {
            if (Match(TokenKind.KeywordIntersect))
            {
                var right = ParseInstanceofExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Intersect, right), start, End);
            }
            else if (Match(TokenKind.KeywordExcept))
            {
                var right = ParseInstanceofExpr();
                left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.Except, right), start, End);
            }
            else break;
        }
        return left;
    }

    // InstanceofExpr ::= TreatExpr ("instance" "of" SequenceType)?
    private XPathAstNode ParseInstanceofExpr()
    {
        int start = Current.Start;
        var left = ParseTreatExpr();
        if (Match(TokenKind.KeywordInstance))
        {
            Expect(TokenKind.KeywordOf);
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            return WithSpan(new InstanceOfNode(left, typeLocal, typePrefix, occurrence), start, End);
        }
        return left;
    }

    // TreatExpr ::= CastableExpr ("treat" "as" SequenceType)?
    private XPathAstNode ParseTreatExpr()
    {
        int start = Current.Start;
        var left = ParseCastableExpr();
        if (Match(TokenKind.KeywordTreat))
        {
            Expect(TokenKind.KeywordAs);
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            return WithSpan(new TreatNode(left, typeLocal, typePrefix, occurrence), start, End);
        }
        return left;
    }

    // CastableExpr ::= CastExpr ("castable" "as" SingleType)?
    private XPathAstNode ParseCastableExpr()
    {
        int start = Current.Start;
        var left = ParseCastExpr();
        if (Match(TokenKind.KeywordCastable))
        {
            Expect(TokenKind.KeywordAs);
            var (typePrefix, typeLocal, occurrence) = ParseSingleType();
            return WithSpan(new CastableNode(left, typeLocal, typePrefix, occurrence), start, End);
        }
        return left;
    }

    // CastExpr ::= ArrowExpr ("cast" "as" SingleType)?
    private XPathAstNode ParseCastExpr()
    {
        int start = Current.Start;
        var left = ParseArrowExpr();
        if (Match(TokenKind.KeywordCast))
        {
            Expect(TokenKind.KeywordAs);
            var (typePrefix, typeLocal, occurrence) = ParseSingleType();
            return WithSpan(new CastNode(left, typeLocal, typePrefix, occurrence), start, End);
        }
        return left;
    }

    // ArrowExpr ::= UnaryExpr ("=>" ArrowFunctionSpecifier)*
    private XPathAstNode ParseArrowExpr()
    {
        int start = Current.Start;
        var left = ParseUnaryExpr();
        while (Match(TokenKind.Arrow))
        {
            var target = ParseArrowTarget();
            left = WithSpan(new ArrowExprNode(left, target), start, End);
        }
        return left;
    }

    private XPathAstNode ParseArrowTarget()
    {
        int start = Current.Start;
        if (Current.Kind == TokenKind.Name)
        {
            var name = GetString(Current);
            var (prefix, local, _) = SplitQName(name);
            Advance();
            var args = ParseArgumentList();
            return WithSpan(new FunctionCallNode(local, args, prefix), start, End);
        }
        if (Current.Kind == TokenKind.Dollar)
        {
            // Variable reference as function: $x => $f()
            Advance();
            var nameTok = ExpectName();
            var (prefix, local, _) = SplitQName(GetString(nameTok));
            var args = ParseArgumentList();
            return WithSpan(new DynamicFunctionCallNode(new VariableReferenceNode(local, prefix), args), start, End);
        }
        if (Current.Kind == TokenKind.LParen)
        {
            // Parenthesized expression as function: $x => ($f)()
            Advance();
            var expr = ParseExpr();
            Expect(TokenKind.RParen);
            var args = ParseArgumentList();
            return WithSpan(new DynamicFunctionCallNode(expr, args), start, End);
        }
        if (Current.Kind == TokenKind.KeywordFunction)
        {
            var inlineFunc = ParseInlineFunction(start);
            var args = ParseArgumentList();
            return WithSpan(new DynamicFunctionCallNode(inlineFunc, args), start, End);
        }
        throw new ParseException("Expected function name, variable reference, or parenthesized expression after =>", Current.Start);
    }

    // UnaryExpr ::= ("+" | "-")* ValueExpr
    private XPathAstNode ParseUnaryExpr()
    {
        int start = Current.Start;
        var ops = new List<UnaryOperator>();
        while (true)
        {
            if (Match(TokenKind.Plus)) ops.Add(UnaryOperator.Plus);
            else if (Match(TokenKind.Minus)) ops.Add(UnaryOperator.Minus);
            else break;
        }
        var expr = ParseSimpleMapExpr();
        for (int i = ops.Count - 1; i >= 0; i--)
            expr = WithSpan(new UnaryExpressionNode(ops[i], expr), start, End);
        return expr;
    }

    // SimpleMapExpr ::= PathExpr ("!" PathExpr)*
    private XPathAstNode ParseSimpleMapExpr()
    {
        int start = Current.Start;
        var left = ParsePathExpr();
        while (Match(TokenKind.Bang))
        {
            var right = ParsePathExpr();
            left = WithSpan(new BinaryExpressionNode(left, BinaryOperator.SimpleMap, right), start, End);
        }
        return left;
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    // PathExpr ::= ("/" RelativePathExpr?) | ("//" RelativePathExpr) | RelativePathExpr
    private XPathAstNode ParsePathExpr()
    {
        int start = Current.Start;
        if (Match(TokenKind.Slash))
        {
            if (IsAtEnd || !CanStartStepExpr(Current))
                return WithSpan(new PathExprNode(true, Array.Empty<XPathAstNode>()), start, End);
            var steps = ParseRelativePathExpr();
            return WithSpan(new PathExprNode(true, steps), start, End);
        }
        if (Match(TokenKind.SlashSlash))
        {
            var steps = ParseRelativePathExpr();
            steps.Insert(0, new StepNode(XdmAxis.DescendantOrSelf, new NodeTest(NameTestKind.KindTest, "node"), Array.Empty<XPathAstNode>()));
            return WithSpan(new PathExprNode(true, steps), start, End);
        }
        var relSteps = ParseRelativePathExpr();
        if (relSteps.Count == 1)
            return relSteps[0];
        return WithSpan(new PathExprNode(false, relSteps), start, End);
    }

    // RelativePathExpr ::= StepExpr (("/" | "//") StepExpr)*
    private List<XPathAstNode> ParseRelativePathExpr()
    {
        var steps = new List<XPathAstNode> { ParseStepExpr() };
        while (true)
        {
            if (Match(TokenKind.Slash))
            {
                steps.Add(ParseStepExpr());
            }
            else if (Match(TokenKind.SlashSlash))
            {
                steps.Add(new StepNode(XdmAxis.DescendantOrSelf, new NodeTest(NameTestKind.KindTest, "node"), Array.Empty<XPathAstNode>()));
                steps.Add(ParseStepExpr());
            }
            else break;
        }
        return steps;
    }

    // StepExpr ::= PostfixExpr | AxisStep
    private XPathAstNode ParseStepExpr()
    {
        int start = Current.Start;

        // Abbreviated reverse step: ..
        if (Current.Kind == TokenKind.DotDot)
        {
            Advance();
            var preds = ParsePredicateList();
            return WithSpan(new StepNode(XdmAxis.Parent, new NodeTest(NameTestKind.KindTest, "node"), preds), start, End);
        }

        // Abbreviated forward step: @ NodeTest
        if (Current.Kind == TokenKind.At)
        {
            Advance();
            var test = ParseNodeTest();
            var preds = ParsePredicateList();
            return WithSpan(new StepNode(XdmAxis.Attribute, test, preds), start, End);
        }

        // Explicit axis: axis::NodeTest
        if ((Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind)) && Peek(1).Kind == TokenKind.DoubleColon)
        {
            return ParseAxisStep(start);
        }

        // Name that is a kind test: node(), text(), etc.
        // Prefixed names are always function calls, never kind tests.
        if (Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind))
        {
            var name = GetString(Current);
            var (prefix, local, _) = SplitQName(name);
            if (string.IsNullOrEmpty(prefix) && IsKindTestName(local) && Peek(1).Kind == TokenKind.LParen)
            {
                return ParseAxisStep(start);
            }
        }

        // Name test or wildcard in a step context
        // Exclude names followed by LParen (function calls/kind tests already handled)
        // Exclude names followed by Hash (named function refs)
        // Exclude keyword primary-expr starters (map {, array {, array [, function () when followed by their opener)
        bool isPrimaryExprKeyword = (Current.Kind == TokenKind.KeywordMap && Peek(1).Kind == TokenKind.LBrace)
            || (Current.Kind == TokenKind.KeywordArray && (Peek(1).Kind == TokenKind.LBrace || Peek(1).Kind == TokenKind.LBracket))
            || (Current.Kind == TokenKind.KeywordFunction && Peek(1).Kind == TokenKind.LParen);
        if (((Current.Kind == TokenKind.Name || (IsKeywordName(Current.Kind) && !isPrimaryExprKeyword)) && Peek(1).Kind != TokenKind.LParen && Peek(1).Kind != TokenKind.Hash) || Current.Kind == TokenKind.Star)
        {
            return ParseAxisStep(start);
        }

        // Otherwise, it's a postfix expression (primary + predicates/args/lookup)
        return ParsePostfixExpr();
    }

    private StepNode ParseAxisStep(int start)
    {
        XdmAxis axis = XdmAxis.Child;
        bool axisExplicit = false;
        if ((Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind)) && Peek(1).Kind == TokenKind.DoubleColon)
        {
            axis = ParseAxisName();
            Expect(TokenKind.DoubleColon);
            axisExplicit = true;
        }
        var test = ParseNodeTest();
        var preds = ParsePredicateList();

        // XPath 2.0 §3.2.1.1: default axis for attribute/namespace kind tests
        if (!axisExplicit && test.Kind == NameTestKind.KindTest)
        {
            axis = test.Name switch
            {
                "attribute" or "schema-attribute" => XdmAxis.Attribute,
                "namespace-node" => XdmAxis.Namespace,
                _ => axis
            };
        }

        return WithSpan(new StepNode(axis, test, preds), start, End);
    }

    private XdmAxis ParseAxisName()
    {
        var name = GetString(Current).ToLowerInvariant();
        Advance();
        return name switch
        {
            "ancestor" => XdmAxis.Ancestor,
            "ancestor-or-self" => XdmAxis.AncestorOrSelf,
            "attribute" => XdmAxis.Attribute,
            "child" => XdmAxis.Child,
            "descendant" => XdmAxis.Descendant,
            "descendant-or-self" => XdmAxis.DescendantOrSelf,
            "following" => XdmAxis.Following,
            "following-sibling" => XdmAxis.FollowingSibling,
            "namespace" => XdmAxis.Namespace,
            "parent" => XdmAxis.Parent,
            "preceding" => XdmAxis.Preceding,
            "preceding-sibling" => XdmAxis.PrecedingSibling,
            "self" => XdmAxis.Self,
            _ => throw new ParseException($"Unknown axis: {name}", Current.Start)
        };
    }

    private NodeTest ParseNodeTest()
    {
        int start = Current.Start;

        // Wildcard: *
        if (Match(TokenKind.Star))
        {
            // *:local
            if (Match(TokenKind.Colon))
            {
                var local = ExpectName();
                return new NodeTest(NameTestKind.QName, GetString(local), "*");
            }
            return new NodeTest(NameTestKind.AnyName);
        }

        if (Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind))
        {
            var name = GetString(Current);
            var (prefix, local, nsUri) = SplitQName(name);

            // Kind test: node(), text(), etc.
            // Prefixed names are always function calls, never kind tests.
            if (string.IsNullOrEmpty(prefix) && IsKindTestName(local) && Peek(1).Kind == TokenKind.LParen)
            {
                return ParseKindTest();
            }

            Advance();

            // prefix:*
            if (Match(TokenKind.Colon) && Match(TokenKind.Star))
            {
                return new NodeTest(NameTestKind.NamespaceAny, name);
            }

            if (!string.IsNullOrEmpty(nsUri))
                return new NodeTest(NameTestKind.QName, local, nsUri);
            if (prefix is null)
                return new NodeTest(NameTestKind.LocalName, local);

            // The xml prefix is predefined; if it is not present in the static namespace
            // context, fall back to PrefixedName so the VM matches the full name without
            // requiring empty-namespace resolution (needed for e.g. @xml:base).
            if (prefix == "xml")
                return new NodeTest(NameTestKind.PrefixedName, prefix + ":" + local);

            return new NodeTest(NameTestKind.QName, local, prefix);
        }

        throw new ParseException($"Expected node test but found {Current.Kind}", start);
    }

    private NodeTest ParseKindTest()
    {
        var name = GetString(Current);
        Advance();
        Expect(TokenKind.LParen);

        string? argument = null;

        // Parse simple kind-test arguments: processing-instruction(name), element(name), attribute(name).
        // For now we do not parse schema types or nested kind tests (e.g. document-node(element(x))).
        if (Current.Kind == TokenKind.RParen)
        {
            // empty parentheses, e.g. node(), text(), element()
        }
        else if (name == "processing-instruction")
        {
            // argument is an NCName or string literal
            if (Current.Kind == TokenKind.StringLiteral)
                argument = Unquote(GetString(Current));
            else if (Current.Kind == TokenKind.Name)
                argument = GetString(Current);
            // skip to closing paren
            while (!IsAtEnd && Current.Kind != TokenKind.RParen)
                Advance();
        }
        else if (name == "element" || name == "attribute")
        {
            // argument is a name test (QName, *, prefix:*, or NCName)
            if (Current.Kind == TokenKind.Star)
            {
                argument = "*";
                Advance();
            }
            else if (Current.Kind == TokenKind.Name)
            {
                var argName = GetString(Current);
                Advance();
                if (Current.Kind == TokenKind.Colon)
                {
                    Advance();
                    if (Current.Kind == TokenKind.Star)
                    {
                        argument = argName + ":*";
                        Advance();
                    }
                    else if (Current.Kind == TokenKind.Name)
                    {
                        argument = argName + ":" + GetString(Current);
                        Advance();
                    }
                }
                else
                {
                    argument = argName;
                }
            }
            // skip remainder (e.g. comma + type) to closing paren
            while (!IsAtEnd && Current.Kind != TokenKind.RParen)
                Advance();
        }
        else
        {
            // For other kind tests (schema-element, schema-attribute, document-node) skip content.
            int depth = 1;
            while (!IsAtEnd && depth > 0)
            {
                if (Current.Kind == TokenKind.LParen) depth++;
                else if (Current.Kind == TokenKind.RParen) depth--;
                if (depth > 0) Advance();
            }
        }

        Expect(TokenKind.RParen);
        return new NodeTest(NameTestKind.KindTest, name, KindTestArgument: argument);
    }

    private List<XPathAstNode> ParsePredicateList()
    {
        var preds = new List<XPathAstNode>();
        while (Current.Kind == TokenKind.LBracket)
        {
            preds.Add(ParsePredicate());
        }
        return preds;
    }

    private XPathAstNode ParsePredicate()
    {
        int start = Current.Start;
        Expect(TokenKind.LBracket);
        var expr = ParseExpr();
        Expect(TokenKind.RBracket);
        return WithSpan(new PredicateNode(expr), start, End);
    }

    // ------------------------------------------------------------------
    // Postfix expressions
    // ------------------------------------------------------------------

    private XPathAstNode ParsePostfixExpr()
    {
        int start = Current.Start;
        var expr = ParsePrimaryExpr();
        while (true)
        {
            if (Current.Kind == TokenKind.LBracket)
            {
                var pred = ParsePredicate();
                expr = WithSpan(new PostfixPredicateNode(expr, pred), start, End);
            }
            else if (Current.Kind == TokenKind.LParen)
            {
                // Dynamic function call: $f(1,2) or (fn:abs#1)(3) or function-lookup(...)(...)
                var args = ParseArgumentList();
                expr = WithSpan(new DynamicFunctionCallNode(expr, args), start, End);
            }
            else if (Current.Kind == TokenKind.Question)
            {
                Advance();
                if (Match(TokenKind.Star))
                {
                    expr = WithSpan(new LookupWildcardNode(expr), start, End);
                }
                else
                {
                    var key = ParseLookupKey();
                    expr = WithSpan(new LookupNode(expr, key), start, End);
                }
            }
            else break;
        }
        return expr;
    }

    private XPathAstNode ParseLookupKey()
    {
        int start = Current.Start;
        if (Match(TokenKind.Star))
            return WithSpan(new StringLiteralNode("*"), start, End);
        if (Current.Kind == TokenKind.IntegerLiteral)
        {
            var str = GetString(Current);
            XPathAstNode node;
            if (long.TryParse(str, out var val))
                node = new IntegerLiteralNode(val);
            else if (decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var decVal))
                node = new DecimalLiteralNode(decVal);
            else
                node = new DoubleLiteralNode(double.Parse(str, CultureInfo.InvariantCulture));
            Advance();
            return WithSpan(node, start, End);
        }
        if (Current.Kind == TokenKind.Name)
        {
            var name = GetString(Current);
            Advance();
            return WithSpan(new StringLiteralNode(name), start, End);
        }
        if (Current.Kind == TokenKind.StringLiteral)
        {
            var str = Unquote(GetString(Current));
            Advance();
            return WithSpan(new StringLiteralNode(str), start, End);
        }
        if (Current.Kind == TokenKind.LParen)
        {
            Advance();
            var expr = ParseExpr();
            Expect(TokenKind.RParen);
            return WithSpan(new ParenthesizedExprNode(expr), start, End);
        }
        throw new ParseException("Expected lookup key", Current.Start);
    }

    // ------------------------------------------------------------------
    // Primary expressions
    // ------------------------------------------------------------------

    private XPathAstNode ParsePrimaryExpr()
    {
        int start = Current.Start;
        switch (Current.Kind)
        {
            case TokenKind.StringLiteral:
                var s = Unquote(GetString(Current));
                Advance();
                return WithSpan(new StringLiteralNode(s), start, End);

            case TokenKind.IntegerLiteral:
                var strI = GetString(Current);
                XPathAstNode nodeI;
                if (long.TryParse(strI, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    nodeI = new IntegerLiteralNode(i);
                else if (decimal.TryParse(strI, NumberStyles.Float, CultureInfo.InvariantCulture, out var decI))
                    nodeI = new DecimalLiteralNode(decI);
                else
                    nodeI = new DoubleLiteralNode(double.Parse(strI, CultureInfo.InvariantCulture));
                Advance();
                return WithSpan(nodeI, start, End);

            case TokenKind.DecimalLiteral:
                var strD = GetString(Current);
                XPathAstNode nodeD;
                if (decimal.TryParse(strD, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                    nodeD = new DecimalLiteralNode(d);
                else
                    nodeD = new DoubleLiteralNode(double.Parse(strD, CultureInfo.InvariantCulture));
                Advance();
                return WithSpan(nodeD, start, End);

            case TokenKind.DoubleLiteral:
                var f = double.Parse(GetString(Current), CultureInfo.InvariantCulture);
                Advance();
                return WithSpan(new DoubleLiteralNode(f), start, End);

            case TokenKind.Dollar:
                Advance();
                var varTok = ExpectName();
                var (vp, vl, vns) = SplitQName(GetString(varTok));
                return WithSpan(new VariableReferenceNode(vl, vp, vns), start, End);

            case TokenKind.LParen:
                Advance();
                if (Match(TokenKind.RParen))
                    return WithSpan(new SequenceExpressionNode(Array.Empty<XPathAstNode>()), start, End);
                var inner = ParseExpr();
                Expect(TokenKind.RParen);
                return WithSpan(new ParenthesizedExprNode(inner), start, End);

            case TokenKind.Dot:
                Advance();
                return WithSpan(new ContextItemNode(), start, End);

            case TokenKind.Name:
                var name = GetString(Current);
                var (prefix, local, _) = SplitQName(name);
                if (Peek(1).Kind == TokenKind.LParen)
                    return ParseFunctionCall(start);
                if (Peek(1).Kind == TokenKind.Hash)
                    return ParseNamedFunctionRef(start);
                throw new ParseException($"Unexpected name '{name}' in primary expression", start);

            case TokenKind.KeywordFunction:
                return ParseInlineFunction(start);

            case TokenKind.KeywordMap:
                return ParseMapConstructor(start);

            case TokenKind.KeywordArray:
                return ParseCurlyArrayConstructor(start);

            case TokenKind.LBracket:
                return ParseSquareArrayConstructor(start);

            default:
                throw new ParseException($"Unexpected token {Current.Kind} in primary expression", start);
        }
    }

    private FunctionCallNode ParseFunctionCall(int start)
    {
        var name = GetString(Current);
        var (prefix, local, nsUri) = SplitQName(name);
        Advance();
        var args = ParseArgumentList();
        return WithSpan(new FunctionCallNode(local, args, prefix, nsUri), start, End);
    }

    private NamedFunctionRefNode ParseNamedFunctionRef(int start)
    {
        var name = GetString(Current);
        var (prefix, local, nsUri) = SplitQName(name);
        Advance(); // name
        Advance(); // #
        var arityTok = Expect(TokenKind.IntegerLiteral);
        var arity = int.Parse(GetString(arityTok), CultureInfo.InvariantCulture);
        return WithSpan(new NamedFunctionRefNode(local, arity, prefix, nsUri), start, End);
    }

    private List<XPathAstNode> ParseArgumentList()
    {
        Expect(TokenKind.LParen);
        var args = new List<XPathAstNode>();
        if (!Match(TokenKind.RParen))
        {
            do
            {
                if (Current.Kind == TokenKind.Question)
                {
                    Advance();
                    args.Add(new ArgumentPlaceholderNode());
                }
                else
                {
                    args.Add(ParseExprSingle());
                }
            } while (Match(TokenKind.Comma));
            Expect(TokenKind.RParen);
        }
        return args;
    }

    // ------------------------------------------------------------------
    // XPath 3.1 constructors
    // ------------------------------------------------------------------

    private InlineFunctionNode ParseInlineFunction(int start)
    {
        Expect(TokenKind.KeywordFunction);
        Expect(TokenKind.LParen);
        var parameters = new List<ParamNode>();
        if (!Match(TokenKind.RParen))
        {
            do
            {
                Expect(TokenKind.Dollar);
                var nameTok = ExpectName();
                string? typeName = null;
                if (Current.Kind == TokenKind.KeywordAs)
                {
                    Advance(); // as
                    typeName = SkipSequenceType();
                }
                parameters.Add(new ParamNode(GetString(nameTok), typeName));
            } while (Match(TokenKind.Comma));
            Expect(TokenKind.RParen);

            // Check for duplicate parameter names (XQST0039)
            var seenNames = new HashSet<string>();
            foreach (var param in parameters)
            {
                if (!seenNames.Add(param.Name))
                    throw new ParseException($"XQST0039: Duplicate parameter name ${param.Name} in inline function.", start);
            }
        }
        string? returnType = null;
        if (Current.Kind == TokenKind.KeywordAs)
        {
            Advance(); // as
            returnType = SkipSequenceType();
        }
        XPathAstNode body;
        if (Current.Kind == TokenKind.LBrace)
        {
            Advance();
            body = ParseExpr();
            Expect(TokenKind.RBrace);
        }
        else
        {
            body = ParsePrimaryExpr();
        }
        return WithSpan(new InlineFunctionNode(parameters, body, returnType), start, End);
    }

    private MapConstructorNode ParseMapConstructor(int start)
    {
        Expect(TokenKind.KeywordMap);
        Expect(TokenKind.LBrace);
        var entries = new List<MapEntryNode>();
        if (!Match(TokenKind.RBrace))
        {
            do
            {
                var key = ParseExprSingle();
                Expect(TokenKind.Colon);
                var value = ParseExprSingle();
                entries.Add(new MapEntryNode(key, value));
            } while (Match(TokenKind.Comma));
            Expect(TokenKind.RBrace);
        }
        return WithSpan(new MapConstructorNode(entries), start, End);
    }

    private ArrayConstructorNode ParseSquareArrayConstructor(int start)
    {
        Expect(TokenKind.LBracket);
        var items = new List<XPathAstNode>();
        if (!Match(TokenKind.RBracket))
        {
            do
            {
                items.Add(ParseExprSingle());
            } while (Match(TokenKind.Comma));
            Expect(TokenKind.RBracket);
        }
        return WithSpan(new ArrayConstructorNode(items, IsSquare: true), start, End);
    }

    private ArrayConstructorNode ParseCurlyArrayConstructor(int start)
    {
        Expect(TokenKind.KeywordArray);
        Expect(TokenKind.LBrace);
        var items = new List<XPathAstNode>();
        if (!Match(TokenKind.RBrace))
        {
            var expr = ParseExpr();
            // Wrap single expression as the array content
            items.Add(expr);
            Expect(TokenKind.RBrace);
        }
        return WithSpan(new ArrayConstructorNode(items, IsSquare: false), start, End);
    }

    // ------------------------------------------------------------------
    // Utilities
    // ------------------------------------------------------------------

    private static (string? Prefix, string Local, string? NamespaceUri) SplitQName(string qname)
    {
        // Braced URI literal: Q{uri}localname or Q{uri}prefix:local
        if (qname.Length > 2 && qname[0] == 'Q' && qname[1] == '{')
        {
            int closeBrace = qname.IndexOf('}');
            if (closeBrace > 2)
            {
                string nsUri = qname[2..closeBrace];
                string rest = qname[(closeBrace + 1)..];
                int restColon = rest.IndexOf(':');
                return restColon < 0 ? (null, rest, nsUri) : (rest[..restColon], rest[(restColon + 1)..], nsUri);
            }
        }

        int colon = qname.IndexOf(':');
        return colon < 0 ? (null, qname, null) : (qname[..colon], qname[(colon + 1)..], null);
    }

    private static bool IsKindTestName(string localName) => localName switch
    {
        "node" or "text" or "comment" or "processing-instruction" or "namespace-node"
        or "element" or "attribute" or "schema-element" or "schema-attribute"
        or "document-node" or "item" => true,
        _ => false
    };

    private static string Unquote(string text)
    {
        if (text.Length >= 2 &&
            ((text[0] == '\'' && text[^1] == '\'') || (text[0] == '"' && text[^1] == '"')))
        {
            return text[1..^1].Replace("\"\"", "\"").Replace("''", "'");
        }
        return text;
    }

    private static bool CanStartStepExpr(Token token) => token.Kind switch
    {
        TokenKind.Dot or TokenKind.DotDot or TokenKind.At or TokenKind.Star
        or TokenKind.Name or TokenKind.Dollar or TokenKind.LParen
        or TokenKind.StringLiteral or TokenKind.IntegerLiteral
        or TokenKind.DecimalLiteral or TokenKind.DoubleLiteral
        or TokenKind.KeywordFunction or TokenKind.KeywordMap
        or TokenKind.KeywordArray or TokenKind.LBracket => true,
        _ => false
    };



    /// <summary>
    /// Skips over a SequenceType and returns a string representation.
    /// Handles simple types, item(), element(name), function(...) as type, etc.
    /// </summary>
    private string SkipSequenceType()
    {
        int start = _position;
        int parenDepth = 0;
        while (true)
        {
            if (Current.Kind == TokenKind.Eof)
                break;

            if (Current.Kind == TokenKind.LParen)
            {
                parenDepth++;
                Advance();
                continue;
            }

            if (Current.Kind == TokenKind.RParen)
            {
                if (parenDepth == 0)
                    break; // RParen belongs to the caller, not the sequence type

                parenDepth--;
                Advance();
                // Occurrence indicator after closing paren
                if (Current.Kind == TokenKind.Question ||
                    Current.Kind == TokenKind.Star ||
                    Current.Kind == TokenKind.Plus)
                {
                    Advance();
                }
                // Function return type: function(...) as ReturnType
                if (Current.Kind == TokenKind.KeywordAs)
                {
                    Advance(); // as
                    continue;
                }
                if (parenDepth <= 0)
                    break;
                continue;
            }

            if (parenDepth == 0)
            {
                if (Current.Kind == TokenKind.Comma ||
                    Current.Kind == TokenKind.RParen ||
                    Current.Kind == TokenKind.LBrace)
                {
                    break;
                }
                // 'as' at top level is part of function return type
                if (Current.Kind == TokenKind.KeywordAs)
                {
                    Advance();
                    continue;
                }
            }

            Advance();
        }
        if (_position == start)
            return string.Empty;
        int charStart = _tokens[start].Start;
        int charEnd = _tokens[_position - 1].Start + _tokens[_position - 1].Length;
        return _source[charStart..charEnd];
    }

    private (string? Prefix, string Local, OccurrenceIndicator Occurrence) ParseSequenceType()
    {
        var (prefix, local, _) = ParseTypeNameAndParens();

        OccurrenceIndicator occurrence = OccurrenceIndicator.One;
        if (Match(TokenKind.Question))
            occurrence = OccurrenceIndicator.ZeroOrOne;
        else if (Match(TokenKind.Star))
            occurrence = OccurrenceIndicator.ZeroOrMore;
        else if (Match(TokenKind.Plus))
            occurrence = OccurrenceIndicator.OneOrMore;
        return (prefix, local, occurrence);
    }

    private (string? Prefix, string Local, OccurrenceIndicator Occurrence) ParseSingleType()
    {
        var (prefix, local, hasParens) = ParseTypeNameAndParens();
        if (hasParens)
            throw new ParseException("XPST0003: Type tests with parentheses are not allowed in 'cast' or 'castable as' expressions.", Current.Start);

        if (Match(TokenKind.Question))
            return (prefix, local, OccurrenceIndicator.ZeroOrOne);
        if (Current.Kind is TokenKind.Star or TokenKind.Plus)
            throw new ParseException("XPST0003: '*' and '+' are not allowed as occurrence indicators in 'cast' or 'castable as' expressions.", Current.Start);
        return (prefix, local, OccurrenceIndicator.One);
    }

    private (string? Prefix, string Local, bool HasParens) ParseTypeNameAndParens()
    {
        string name;
        if (Current.Kind == TokenKind.Name)
        {
            name = GetString(Current);
            Advance();
        }
        else if (Current.Kind == TokenKind.KeywordFunction)
        {
            name = "function";
            Advance();
        }
        else if (Current.Kind == TokenKind.KeywordMap)
        {
            name = "map";
            Advance();
        }
        else if (Current.Kind == TokenKind.KeywordArray)
        {
            name = "array";
            Advance();
        }
        else
        {
            throw new ParseException($"Expected sequence type but found {Current.Kind}", Current.Start);
        }

        var (prefix, local, _) = SplitQName(name);

        // Consume optional parens and their contents: item(), node(), empty-sequence(), function(*), function(int) as int, map(*), element(foo), etc.
        bool hasParens = false;
        if (Current.Kind == TokenKind.LParen)
        {
            hasParens = true;
            int parenDepth = 0;
            var sb = new System.Text.StringBuilder();
            sb.Append(local);
            do
            {
                if (Current.Kind == TokenKind.LParen)
                {
                    parenDepth++;
                    sb.Append(GetString(Current));
                    Advance();
                }
                else if (Current.Kind == TokenKind.RParen)
                {
                    parenDepth--;
                    sb.Append(GetString(Current));
                    Advance();
                }
                else
                {
                    sb.Append(GetString(Current));
                    Advance();
                }
            } while (parenDepth > 0 && Current.Kind != TokenKind.Eof);
            local = sb.ToString();

            // Function tests may have a return type: function(item()*) as xs:double
            if (local.StartsWith("function", StringComparison.OrdinalIgnoreCase)
                && Current.Kind == TokenKind.KeywordAs)
            {
                sb.Append(' ');
                sb.Append(GetString(Current)); // 'as'
                Advance();
                sb.Append(' ');
                string returnType = SkipSequenceType();
                sb.Append(returnType);
                local = sb.ToString();
            }
        }

        return (prefix, local, hasParens);
    }
}

// ------------------------------------------------------------------
// Supporting AST nodes used by parser
// ------------------------------------------------------------------


