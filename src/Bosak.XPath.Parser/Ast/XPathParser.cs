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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Runtime.CompilerServices;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T WithSpan<T>(T node, int start, int end) where T : XPathAstNode
        => node with { Span = new TextSpan(start, end - start) };

    private ReadOnlySpan<char> GetText(Token token) => token.Text(_source.AsSpan());
    private string GetString(Token token) => token.Text(_source.AsSpan()).ToString();

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
        var nameTok = Expect(TokenKind.Name);
        var (prefix, local) = SplitQName(GetString(nameTok));
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
        var nameTok = Expect(TokenKind.Name);
        var (prefix, local) = SplitQName(GetString(nameTok));
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
            var (typePrefix, typeLocal) = ParseTypeName();
            return WithSpan(new InstanceOfNode(left, typeLocal, typePrefix), start, End);
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
            var (typePrefix, typeLocal) = ParseTypeName();
            return WithSpan(new TreatNode(left, typeLocal, typePrefix), start, End);
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
            var (typePrefix, typeLocal) = ParseTypeName();
            return WithSpan(new CastableNode(left, typeLocal, typePrefix), start, End);
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
            var (typePrefix, typeLocal) = ParseTypeName();
            return WithSpan(new CastNode(left, typeLocal, typePrefix), start, End);
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
            var (prefix, local) = SplitQName(name);
            Advance();
            var args = ParseArgumentList();
            return WithSpan(new FunctionCallNode(local, args, prefix), start, End);
        }
        if (Current.Kind == TokenKind.Dollar)
        {
            // Variable reference as function: $x => $f()
            Advance();
            var nameTok = Expect(TokenKind.Name);
            var (prefix, local) = SplitQName(GetString(nameTok));
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
            // Inline function as arrow target: not common, skip for now
            throw new ParseException("Inline function as arrow target not yet supported", Current.Start);
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
        if (Current.Kind == TokenKind.Name && Peek(1).Kind == TokenKind.DoubleColon)
        {
            return ParseAxisStep(start);
        }

        // Name that is a kind test: node(), text(), etc.
        if (Current.Kind == TokenKind.Name)
        {
            var name = GetString(Current);
            var (_, local) = SplitQName(name);
            if (IsKindTestName(local) && Peek(1).Kind == TokenKind.LParen)
            {
                return ParseAxisStep(start);
            }
        }

        // Name test or wildcard in a step context
        // Exclude names followed by LParen (function calls/kind tests already handled)
        // Exclude names followed by Hash (named function refs)
        if ((Current.Kind == TokenKind.Name && Peek(1).Kind != TokenKind.LParen && Peek(1).Kind != TokenKind.Hash) || Current.Kind == TokenKind.Star)
        {
            return ParseAxisStep(start);
        }

        // Otherwise, it's a postfix expression (primary + predicates/args/lookup)
        return ParsePostfixExpr();
    }

    private StepNode ParseAxisStep(int start)
    {
        XdmAxis axis = XdmAxis.Child;
        if (Current.Kind == TokenKind.Name && Peek(1).Kind == TokenKind.DoubleColon)
        {
            axis = ParseAxisName();
            Expect(TokenKind.DoubleColon);
        }
        var test = ParseNodeTest();
        var preds = ParsePredicateList();
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
                var local = Expect(TokenKind.Name);
                return new NodeTest(NameTestKind.QName, GetString(local), "*");
            }
            return new NodeTest(NameTestKind.AnyName);
        }

        if (Current.Kind == TokenKind.Name)
        {
            var name = GetString(Current);
            var (prefix, local) = SplitQName(name);

            // Kind test: node(), text(), etc.
            if (IsKindTestName(local) && Peek(1).Kind == TokenKind.LParen)
            {
                return ParseKindTest();
            }

            Advance();

            // prefix:*
            if (Match(TokenKind.Colon) && Match(TokenKind.Star))
            {
                return new NodeTest(NameTestKind.NamespaceAny, prefix);
            }

            if (prefix is null)
                return new NodeTest(NameTestKind.LocalName, local);

            return new NodeTest(NameTestKind.QName, local, prefix);
        }

        throw new ParseException($"Expected node test but found {Current.Kind}", start);
    }

    private NodeTest ParseKindTest()
    {
        var name = GetString(Current);
        Advance();
        Expect(TokenKind.LParen);

        // For now, skip the content inside kind test parentheses.
        // A full implementation would parse PI names, element names/types, etc.
        int depth = 1;
        while (!IsAtEnd && depth > 0)
        {
            if (Current.Kind == TokenKind.LParen) depth++;
            else if (Current.Kind == TokenKind.RParen) depth--;
            if (depth > 0) Advance();
        }
        Expect(TokenKind.RParen);
        return new NodeTest(NameTestKind.KindTest, name);
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
            else if (Current.Kind == TokenKind.LParen && expr is not FunctionCallNode)
            {
                // Dynamic function call: $f(1,2) or (fn:abs#1)(3)
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
            var val = long.Parse(GetString(Current));
            Advance();
            return WithSpan(new IntegerLiteralNode(val), start, End);
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
                var i = long.Parse(GetString(Current), CultureInfo.InvariantCulture);
                Advance();
                return WithSpan(new IntegerLiteralNode(i), start, End);

            case TokenKind.DecimalLiteral:
                var d = decimal.Parse(GetString(Current), CultureInfo.InvariantCulture);
                Advance();
                return WithSpan(new DecimalLiteralNode(d), start, End);

            case TokenKind.DoubleLiteral:
                var f = double.Parse(GetString(Current), CultureInfo.InvariantCulture);
                Advance();
                return WithSpan(new DoubleLiteralNode(f), start, End);

            case TokenKind.Dollar:
                Advance();
                var varTok = Expect(TokenKind.Name);
                var (vp, vl) = SplitQName(GetString(varTok));
                return WithSpan(new VariableReferenceNode(vl, vp), start, End);

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
                var (prefix, local) = SplitQName(name);
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
        var (prefix, local) = SplitQName(name);
        Advance();
        var args = ParseArgumentList();
        return WithSpan(new FunctionCallNode(local, args, prefix), start, End);
    }

    private NamedFunctionRefNode ParseNamedFunctionRef(int start)
    {
        var name = GetString(Current);
        var (prefix, local) = SplitQName(name);
        Advance(); // name
        Advance(); // #
        var arityTok = Expect(TokenKind.IntegerLiteral);
        var arity = int.Parse(GetString(arityTok), CultureInfo.InvariantCulture);
        return WithSpan(new NamedFunctionRefNode(local, arity, prefix), start, End);
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
                var nameTok = Expect(TokenKind.Name);
                parameters.Add(new ParamNode(GetString(nameTok)));
            } while (Match(TokenKind.Comma));
            Expect(TokenKind.RParen);
        }
        // TODO: parse "as" SequenceType
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
        return WithSpan(new InlineFunctionNode(parameters, body), start, End);
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

    private static (string? Prefix, string Local) SplitQName(string qname)
    {
        int colon = qname.IndexOf(':');
        return colon < 0 ? (null, qname) : (qname[..colon], qname[(colon + 1)..]);
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



    private (string? Prefix, string Local) ParseTypeName()
    {
        var tok = Expect(TokenKind.Name);
        var name = GetString(tok);
        return SplitQName(name);
    }
}

// ------------------------------------------------------------------
// Supporting AST nodes used by parser
// ------------------------------------------------------------------


