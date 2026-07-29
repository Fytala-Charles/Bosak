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
//                      | Charles Korthout | 1.4   | 13-06-2026     | Empty-URI EQName support in SplitQName (Q{}local)                                       |
//                      | Charles Korthout | 1.5   | 13-06-2026     | Resolve xml prefix in node tests to the XML namespace                                    |
//                      | Charles Korthout | 1.6   | 13-06-2026     | Fixed Unquote to preserve doubled quotes that do not match the enclosing delimiter      |
//                      | Charles Korthout | 1.7   | 26-06-2026     | Static errors for removed map functions and obsolete map namespace; XPST0003 for :=    |
//                      | Charles Korthout | 1.8   | 15-07-2026     | UnaryLookup (?KS ≡ .?KS); empty-paren lookup key .?(); keyword NCName lookup keys; qualified-name keys are XPST0003; argument-placeholder vs lookup disambiguation |
//                      | Charles Korthout | 1.8   | 26-06-2026     | Parse Q{uri}* URI-qualified wildcards                                                    |
//                      | Charles Korthout | 1.9   | 15-07-2026     | Keep ' as ' separated in nested function tests inside map/array type parens (ArrayTest-063) |
//                      | Charles Korthout | 1.10  | 15-07-2026     | FLWOR completion: 'at $pos' positional var, 'where' clause, mixed for/let chains          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.11  | 15-07-2026     | EQName URI whitespace normalized in SplitQName; for/let bindings capture prefix/namespace |
//                      | Charles Korthout | 1.12  | 19-07-2026     | Reserved function names in function calls and named function references raise XPST0003    |
//                      | Charles Korthout | 1.13  | 19-07-2026     | Reserved function name check applies only to named function references, not function calls |
//                      | Charles Korthout | 1.14  | 20-07-2026     | Allow 'is' as a non-reserved function name in function calls (K-NodeSame-6)              |
//                      | Charles Korthout | 1.15  | 20-07-2026     | Only treat 'for'/'let' as FLWOR keywords when followed by '$' (K2-NameTest-78/79)        |
//                      | Charles Korthout | 1.16  | 20-07-2026     | Disallow consecutive for/let clauses in FLWOR (XPath-only; LetExpr020a)                   |
//                      | Charles Korthout | 1.17  | 20-07-2026     | Require closing parenthesis in sequence type tests (K-SeqExprTreat-16)                    |
//                      | Charles Korthout | 1.18  | 22-07-2026     | Parse full XQuery FLWOR with order by, empty order, and collation                       |
//                      | Charles Korthout | 1.19  | 22-07-2026     | Added allowFullFlwor flag to Parse; XPath-only mode rejects multi-clause FLWOR/order by |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.20  | 23-07-2026     | Parse XQuery FLWOR count clause                                                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.21  | 25-07-2026     | Parse XQuery FLWOR group by clause                                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.22  | 25-07-2026     | Parse XQuery FLWOR window clause (tumbling/sliding)                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.23  | 25-07-2026     | Optional window end condition; XQST0103; stable order by; 'as' type declarations        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.24  | 25-07-2026     | Direct element/comment/PI constructors; allowing empty; XQST0118 tag mismatch           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.25  | 25-07-2026     | Constructor validations: XQST0022/0046/0070/0071/0090; CDATA/ref text; empty-expr rules |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.26  | 25-07-2026     | Parse XQuery computed constructors; EQName char/entity reference expansion; boundary-whitespace CDATA fix |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.27  | 25-07-2026     | Parse XQuery switch and typeswitch expressions incl. sequence-type unions in case clauses |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.28  | 25-07-2026     | xml:space is an ordinary constructor attribute; boundary whitespace stripped at flush time |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.29  | 26-07-2026     | SkipSequenceType stops at expression boundaries (:=/in/return/then/else/|) after function-type 'as' |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.30  | 27-07-2026     | Full try/catch parsing: named code patterns, multiple clauses, empty bodies |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.31  | 27-07-2026     | String constructor scanning; XPath-mode string literals no longer expand references |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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
    private readonly bool _allowFullFlwor;
    private bool _xml11LineEndings;
    private int _position;

    public XPathParser(Token[] tokens, string source, bool allowFullFlwor = false)
    {
        _tokens = tokens;
        _source = source;
        _allowFullFlwor = allowFullFlwor;
        _position = 0;
    }

    // Functions removed from the XPath / XSLT 3.0 specifications. Calls to these
    // are reported as static errors (XPST0017) at parse time.
    private static readonly HashSet<(string NamespaceUri, string LocalName)> RemovedFunctions = new()
    {
        ("http://www.w3.org/2005/xpath-functions/map", "new"),
        ("http://www.w3.org/2005/xpath-functions/map", "for-each-entry"),
        ("http://www.w3.org/2005/xpath-functions/map", "collation"),
        ("http://www.w3.org/2005/xpath-functions", "deep-equal2"),
    };

    private const string OldMapNamespace = "http://www.w3.org/2011/xpath-functions/map";

    /// <summary>
    /// XPath reserved function names. These cannot be used as function names in
    /// function calls or named function references when the name has no prefix
    /// (i.e., is in the default function namespace).
    /// </summary>
    private static readonly HashSet<string> ReservedFunctionNames = new(StringComparer.Ordinal)
    {
        "attribute", "comment", "document-node", "element", "empty-sequence",
        "function", "if", "item", "namespace-node", "node", "processing-instruction",
        "schema-attribute", "schema-element", "switch", "text", "typeswitch",
        "array", "map"
    };

    /// <summary>
    /// Convenience method: lexes and parses an XPath expression string.
    /// </summary>
    /// <param name="xpath">The XPath expression to parse.</param>
    /// <param name="allowFullFlwor">When true, allows full XQuery FLWOR syntax (multiple for/let/where/order-by clauses). Default is false (XPath-only FLWOR).</param>
    /// <param name="xml11LineEndings">When true, string literals get XML 1.1 line-ending
    /// normalization; when false (default), references produce their exact characters.</param>
    public static XPathAstNode Parse(string xpath, bool allowFullFlwor = false, bool xml11LineEndings = false)
    {
        var lexer = new XPathLexer(xpath.AsSpan(), allowConstructors: allowFullFlwor);
        var tokens = new List<Token>();
        Token tok;
        while ((tok = lexer.NextToken()).Kind != TokenKind.Eof)
            tokens.Add(tok);

        var parser = new XPathParser(tokens.ToArray(), xpath, allowFullFlwor) { _xml11LineEndings = xml11LineEndings };
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
            // 'for' and 'let' are FLWOR keywords only when followed by a variable binding.
            // Otherwise they are ordinary names (e.g., name tests) (K2-NameTest-78/79).
            TokenKind.KeywordFor when Peek(1).Kind == TokenKind.Dollar => ParseForExpr(),
            TokenKind.KeywordFor when _allowFullFlwor && IsWindowKeyword(Peek(1)) => ParseForExpr(),
            TokenKind.KeywordLet when Peek(1).Kind == TokenKind.Dollar => ParseLetExpr(),
            TokenKind.KeywordSome or TokenKind.KeywordEvery => ParseQuantifiedExpr(),
            TokenKind.KeywordIf => ParseIfExpr(),
            TokenKind.KeywordTry => ParseTryExpr(),
            // switch/typeswitch are XQuery-only ExprSingle forms; 'switch'/'typeswitch'
            // followed by '(' anywhere else remains an ordinary name (K2-NameTest-*).
            TokenKind.Name when _allowFullFlwor && GetString(Current) == "switch" && Peek(1).Kind == TokenKind.LParen => ParseSwitchExpr(),
            TokenKind.Name when _allowFullFlwor && GetString(Current) == "typeswitch" && Peek(1).Kind == TokenKind.LParen => ParseTypeswitchExpr(),
            _ => ParseOrExpr()
        };
    }

    // FLWORExpr ::= InitialClause IntermediateClause* "return" ExprSingle
    // InitialClause ::= ForClause | LetClause
    // IntermediateClause ::= ForClause | LetClause | WhereClause | CountClause | OrderByClause | GroupByClause
    // ForClause ::= "for" ForBinding ("," ForBinding)*
    // ForBinding ::= "$" VarName ("at" "$" VarName)? "in" ExprSingle
    // LetClause ::= "let" LetBinding ("," LetBinding)*
    // LetBinding ::= "$" VarName ":="" ExprSingle
    // WhereClause ::= "where" ExprSingle
    // CountClause ::= "count" "$" VarName
    // OrderByClause ::= "order by" OrderSpec ("," OrderSpec)*
    // OrderSpec ::= ExprSingle ("ascending" | "descending")? ("empty" ("least" | "greatest"))? ("collation" URILiteral)?
    // GroupByClause ::= "group by" GroupingSpec ("," GroupingSpec)*
    // GroupingSpec ::= "$" VarName (":=" ExprSingle)? ("collation" URILiteral)?
    // WindowClause ::= "for" ("tumbling" | "sliding") "window" "$" VarName "in" ExprSingle WindowStartCondition WindowEndCondition?
    // WindowStartCondition ::= "start" WindowVars "when" ExprSingle
    // WindowEndCondition ::= ("only")? "end" WindowVars "when" ExprSingle
    // WindowVars ::= ("$" VarName)? ("at" "$" VarName)? ("previous" "$" VarName)? ("next" "$" VarName)?
    private XPathAstNode ParseForExpr() => ParseFlworExpr();

    private XPathAstNode ParseLetExpr() => ParseFlworExpr();

    private XPathAstNode ParseFlworExpr()
    {
        int start = Current.Start;
        var clauses = new List<FlworClauseNode>();

        // Initial clause (dispatch guarantees 'for' or 'let').
        if (Current.Kind == TokenKind.KeywordFor)
        {
            if (IsWindowKeyword(Peek(1)))
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow a window clause.", Current.Start);
                clauses.Add(ParseWindowClause());
            }
            else
            {
                clauses.Add(new ForClauseNode(ParseForClauseBindings()));
            }
        }
        else
        {
            clauses.Add(new LetClauseNode(ParseLetClauseBindings()));
        }

        // Intermediate clauses.
        while (true)
        {
            if (Current.Kind == TokenKind.KeywordFor)
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow multiple for/let clauses in a FLWOR expression.", Current.Start);
                if (IsWindowKeyword(Peek(1)))
                    clauses.Add(ParseWindowClause());
                else
                    clauses.Add(new ForClauseNode(ParseForClauseBindings()));
            }
            else if (Current.Kind == TokenKind.KeywordLet)
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow multiple for/let clauses in a FLWOR expression.", Current.Start);
                clauses.Add(new LetClauseNode(ParseLetClauseBindings()));
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "where")
            {
                Advance();
                clauses.Add(new WhereClauseNode(ParseExprSingle()));
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "count" && Peek(1).Kind == TokenKind.Dollar)
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow a count clause.", Current.Start);
                Advance();
                Expect(TokenKind.Dollar);
                var nameTok = ExpectName();
                var (prefix, local, ns) = SplitQName(GetString(nameTok));
                clauses.Add(new CountClauseNode(local, prefix, ns));
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "order")
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow an order by clause.", Current.Start);
                Advance();
                if (Current.Kind == TokenKind.Name && GetString(Current) == "by")
                {
                    Advance();
                    clauses.Add(new OrderByClauseNode(ParseOrderBySpecs()));
                }
                else
                {
                    throw new ParseException("XPST0003: Expected 'by' after 'order'.", Current.Start);
                }
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "stable" &&
                     Peek(1).Kind == TokenKind.Name && GetString(Peek(1)) == "order")
            {
                // "stable" is the default ordering behavior; parse and ignore it.
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow an order by clause.", Current.Start);
                Advance();
                Advance();
                if (Current.Kind == TokenKind.Name && GetString(Current) == "by")
                {
                    Advance();
                    clauses.Add(new OrderByClauseNode(ParseOrderBySpecs()));
                }
                else
                {
                    throw new ParseException("XPST0003: Expected 'by' after 'stable order'.", Current.Start);
                }
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "group")
            {
                if (!_allowFullFlwor)
                    throw new ParseException("XPST0003: XPath does not allow a group by clause.", Current.Start);
                Advance();
                if (Current.Kind == TokenKind.Name && GetString(Current) == "by")
                {
                    Advance();
                    clauses.Add(new GroupByClauseNode(ParseGroupingSpecs()));
                }
                else
                {
                    throw new ParseException("XPST0003: Expected 'by' after 'group'.", Current.Start);
                }
            }
            else
            {
                break;
            }
        }

        Expect(TokenKind.KeywordReturn);
        var body = ParseExprSingle();

        // For simple cases with no order by, count, or group by (and only one initial for/let + optional where),
        // preserve the existing nested For/Let/If AST for backward compatibility.
        if (clauses.Count == 1 && clauses[0] is ForClauseNode forClause && !clauses.Any(c => c is OrderByClauseNode or CountClauseNode or GroupByClauseNode))
        {
            return WithSpan(new ForExpressionNode(forClause.Bindings, body), start, End);
        }
        if (clauses.Count == 1 && clauses[0] is LetClauseNode letClause && !clauses.Any(c => c is OrderByClauseNode or CountClauseNode or GroupByClauseNode))
        {
            return WithSpan(new LetExpressionNode(letClause.Bindings, body), start, End);
        }
        if (clauses.Count == 2 && clauses[0] is ForClauseNode forClause2 && clauses[1] is WhereClauseNode whereClause && !clauses.Any(c => c is OrderByClauseNode or CountClauseNode or GroupByClauseNode))
        {
            var filteredBody = WithSpan(new IfExpressionNode(whereClause.Condition, body,
                new SequenceExpressionNode(Array.Empty<XPathAstNode>())), start, End);
            return WithSpan(new ForExpressionNode(forClause2.Bindings, filteredBody), start, End);
        }
        if (clauses.Count == 2 && clauses[0] is LetClauseNode letClause2 && clauses[1] is WhereClauseNode whereClause2 && !clauses.Any(c => c is OrderByClauseNode or CountClauseNode or GroupByClauseNode))
        {
            var filteredBody = WithSpan(new IfExpressionNode(whereClause2.Condition, body,
                new SequenceExpressionNode(Array.Empty<XPathAstNode>())), start, End);
            return WithSpan(new LetExpressionNode(letClause2.Bindings, filteredBody), start, End);
        }

        return WithSpan(new FlworExpressionNode(clauses, body), start, End);
    }

    private IReadOnlyList<OrderSpec> ParseOrderBySpecs()
    {
        var specs = new List<OrderSpec>();
        do
        {
            var key = ParseExprSingle();
            bool descending = false;
            var emptyOrder = EmptyOrder.Least;
            string? collation = null;

            if (Current.Kind == TokenKind.Name && GetString(Current) == "ascending")
            {
                Advance();
            }
            else if (Current.Kind == TokenKind.Name && GetString(Current) == "descending")
            {
                Advance();
                descending = true;
            }

            if (Current.Kind == TokenKind.Name && GetString(Current) == "empty")
            {
                Advance();
                if (Current.Kind == TokenKind.Name && GetString(Current) == "greatest")
                {
                    Advance();
                    emptyOrder = EmptyOrder.Greatest;
                }
                else if (Current.Kind == TokenKind.Name && GetString(Current) == "least")
                {
                    Advance();
                }
                else
                {
                    throw new ParseException("XPST0003: Expected 'greatest' or 'least' after 'empty'.", Current.Start);
                }
            }

            if (Current.Kind == TokenKind.Name && GetString(Current) == "collation")
            {
                Advance();
                var lit = Expect(TokenKind.StringLiteral);
                collation = Unquote(GetString(lit));
            }

            specs.Add(new OrderSpec(key, descending, emptyOrder, collation));
        } while (Match(TokenKind.Comma));
        return specs;
    }

    private IReadOnlyList<GroupingSpec> ParseGroupingSpecs()
    {
        var specs = new List<GroupingSpec>();
        do
        {
            Expect(TokenKind.Dollar);
            var nameTok = ExpectName();
            var (prefix, local, ns) = SplitQName(GetString(nameTok));

            FlworTypeDeclaration? declaredType = null;
            if (Current.Kind == TokenKind.KeywordAs)
            {
                Advance();
                var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
                declaredType = new FlworTypeDeclaration(typeLocal, typePrefix, occurrence);
            }

            XPathAstNode? keyExpression = null;
            if (Match(TokenKind.Assign))
            {
                keyExpression = ParseExprSingle();
            }

            string? collation = null;
            if (Current.Kind == TokenKind.Name && GetString(Current) == "collation")
            {
                Advance();
                var lit = Expect(TokenKind.StringLiteral);
                collation = Unquote(GetString(lit));
            }

            specs.Add(new GroupingSpec(local, keyExpression, collation, prefix, ns, declaredType));
        } while (Match(TokenKind.Comma));
        return specs;
    }

    private bool IsWindowKeyword(Token token) =>
        token.Kind == TokenKind.Name && (GetString(token) == "tumbling" || GetString(token) == "sliding");

    private WindowClauseNode ParseWindowClause()
    {
        // Current is 'for'; the next token is 'tumbling' or 'sliding'.
        Expect(TokenKind.KeywordFor);
        bool sliding = GetString(Current) == "sliding";
        Advance();

        if (Current.Kind != TokenKind.Name || GetString(Current) != "window")
            throw new ParseException("XPST0003: Expected 'window' after 'tumbling'/'sliding'.", Current.Start);
        Advance();

        Expect(TokenKind.Dollar);
        var nameTok = ExpectName();
        var (prefix, local, ns) = SplitQName(GetString(nameTok));

        FlworTypeDeclaration? declaredType = null;
        if (Current.Kind == TokenKind.KeywordAs)
        {
            Advance();
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            declaredType = new FlworTypeDeclaration(typeLocal, typePrefix, occurrence);
        }

        Expect(TokenKind.KeywordIn);
        var inExpression = ParseExprSingle();

        var startCondition = ParseWindowCondition("start");

        // The end condition is optional; a tumbling window without one closes when the
        // next window starts (or at the end of the input sequence), and a sliding
        // window without one extends to the end of the input sequence.
        bool onlyEnd = false;
        WindowCondition? endCondition = null;
        if (Current.Kind == TokenKind.Name && GetString(Current) == "only")
        {
            onlyEnd = true;
            Advance();
            endCondition = ParseWindowCondition("end");
        }
        else if (Current.Kind == TokenKind.Name && GetString(Current) == "end")
        {
            endCondition = ParseWindowCondition("end");
        }

        // XQST0103: all variables bound by a single window clause must be distinct.
        var boundNames = new List<string> { local };
        void Collect(string? name)
        {
            if (name is not null)
                boundNames.Add(name);
        }
        Collect(startCondition.CurrentItemVariable);
        Collect(startCondition.PositionalVariable);
        Collect(startCondition.PreviousItemVariable);
        Collect(startCondition.NextItemVariable);
        if (endCondition is not null)
        {
            Collect(endCondition.CurrentItemVariable);
            Collect(endCondition.PositionalVariable);
            Collect(endCondition.PreviousItemVariable);
            Collect(endCondition.NextItemVariable);
        }
        if (boundNames.Distinct().Count() != boundNames.Count)
            throw new ParseException("XQST0103: Duplicate variable binding in window clause.", Current.Start);

        return new WindowClauseNode(sliding, local, inExpression, startCondition, endCondition, onlyEnd, prefix, ns, declaredType);
    }

    private WindowCondition ParseWindowCondition(string keyword)
    {
        // Current must be the contextual keyword ('start' or 'end').
        if (Current.Kind != TokenKind.Name || GetString(Current) != keyword)
            throw new ParseException($"XPST0003: Expected '{keyword}' condition in window clause.", Current.Start);
        Advance();

        string? currentItem = null, positional = null, previousItem = null, nextItem = null;
        if (Current.Kind == TokenKind.Dollar)
        {
            Advance();
            currentItem = GetString(ExpectName());
        }
        if (Current.Kind == TokenKind.Name && GetString(Current) == "at")
        {
            Advance();
            Expect(TokenKind.Dollar);
            positional = GetString(ExpectName());
        }
        if (Current.Kind == TokenKind.Name && GetString(Current) == "previous")
        {
            Advance();
            Expect(TokenKind.Dollar);
            previousItem = GetString(ExpectName());
        }
        if (Current.Kind == TokenKind.Name && GetString(Current) == "next")
        {
            Advance();
            Expect(TokenKind.Dollar);
            nextItem = GetString(ExpectName());
        }

        if (Current.Kind != TokenKind.Name || GetString(Current) != "when")
            throw new ParseException($"XPST0003: Expected 'when' in window {keyword} condition.", Current.Start);
        Advance();

        var whenExpression = ParseExprSingle();
        return new WindowCondition(whenExpression, currentItem, positional, previousItem, nextItem);
    }

    private IReadOnlyList<QuantifiedBinding> ParseForClauseBindings()
    {
        Expect(TokenKind.KeywordFor);
        var bindings = new List<QuantifiedBinding>();
        do
        {
            bindings.Add(ParseSimpleForBinding(allowPositional: true));
        } while (Match(TokenKind.Comma));
        return bindings;
    }

    private IReadOnlyList<QuantifiedBinding> ParseLetClauseBindings()
    {
        Expect(TokenKind.KeywordLet);
        var bindings = new List<QuantifiedBinding>();
        do
        {
            bindings.Add(ParseSimpleLetBinding());
        } while (Match(TokenKind.Comma));
        return bindings;
    }

    private QuantifiedBinding ParseSimpleForBinding(bool allowPositional = false)
    {
        Expect(TokenKind.Dollar);
        var nameTok = ExpectName();
        var (prefix, local, ns) = SplitQName(GetString(nameTok));

        // XQuery TypeDeclaration ("as SequenceType"); XPath 3.1 does not allow it.
        FlworTypeDeclaration? declaredType = null;
        if (Current.Kind == TokenKind.KeywordAs)
        {
            if (!_allowFullFlwor)
                throw new ParseException("XPST0003: XPath does not allow a type declaration in a for binding.", Current.Start);
            Advance();
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            declaredType = new FlworTypeDeclaration(typeLocal, typePrefix, occurrence);
        }

        string? positionalVar = null;
        // PositionalVar ::= "at" "$" VarName  (contextual keyword, lexed as Name)
        if (allowPositional && Current.Kind == TokenKind.Name && GetString(Current) == "at")
        {
            Advance();
            Expect(TokenKind.Dollar);
            var posTok = ExpectName();
            var (_, posLocal, _) = SplitQName(GetString(posTok));
            positionalVar = posLocal;
        }

        // XQuery "allowing empty" option; XPath 3.1 does not allow it.
        bool allowingEmpty = false;
        if (Current.Kind == TokenKind.Name && GetString(Current) == "allowing")
        {
            if (!_allowFullFlwor)
                throw new ParseException("XPST0003: XPath does not allow 'allowing empty' in a for binding.", Current.Start);
            Advance();
            if (Current.Kind != TokenKind.Name || GetString(Current) != "empty")
                throw new ParseException("XPST0003: Expected 'empty' after 'allowing'.", Current.Start);
            Advance();
            allowingEmpty = true;
        }

        Expect(TokenKind.KeywordIn);
        var expr = ParseExprSingle();
        return new QuantifiedBinding(local, expr, positionalVar, prefix, ns, declaredType, allowingEmpty);
    }

    private QuantifiedBinding ParseSimpleLetBinding()
    {
        Expect(TokenKind.Dollar);
        var nameTok = ExpectName();
        var (prefix, local, ns) = SplitQName(GetString(nameTok));

        // XQuery TypeDeclaration ("as SequenceType"); XPath 3.1 does not allow it.
        FlworTypeDeclaration? declaredType = null;
        if (Current.Kind == TokenKind.KeywordAs)
        {
            if (!_allowFullFlwor)
                throw new ParseException("XPST0003: XPath does not allow a type declaration in a let binding.", Current.Start);
            Advance();
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            declaredType = new FlworTypeDeclaration(typeLocal, typePrefix, occurrence);
        }

        Expect(TokenKind.Assign);  // := 
        var expr = ParseExprSingle();
        return new QuantifiedBinding(local, expr, null, prefix, ns, declaredType);
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
    // CatchErrorList ::= NameTest ("|" NameTest)*   — error-code name tests:
    //   '*', 'prefix:local', 'prefix:*', '*:local', 'Q{uri}local', 'Q{uri}*', NCName
    private XPathAstNode ParseTryExpr()
    {
        int start = Current.Start;
        Expect(TokenKind.KeywordTry);
        Expect(TokenKind.LBrace);
        // An empty try body is the empty sequence (try-019).
        var tryBody = Current.Kind == TokenKind.RBrace
            ? new SequenceExpressionNode(Array.Empty<XPathAstNode>())
            : ParseExpr();
        Expect(TokenKind.RBrace);

        var clauses = new List<TryCatchClause>();
        while (Current.Kind == TokenKind.KeywordCatch)
        {
            Advance();
            var patterns = new List<CatchCodePattern>();
            do
            {
                patterns.Add(ParseCatchCodePattern());
            } while (Match(TokenKind.VBar));
            Expect(TokenKind.LBrace);
            // An empty catch body is the empty sequence (try-020).
            var catchBody = Current.Kind == TokenKind.RBrace
                ? new SequenceExpressionNode(Array.Empty<XPathAstNode>())
                : ParseExpr();
            Expect(TokenKind.RBrace);
            clauses.Add(new TryCatchClause(patterns, catchBody));
        }
        if (clauses.Count == 0)
            throw new ParseException("XPST0003: Expected at least one catch clause after 'try { ... }'.", Current.Start);
        return WithSpan(new TryCatchNode(tryBody, clauses), start, End);
    }

    // Parses one error-code name test of a catch clause.
    private CatchCodePattern ParseCatchCodePattern()
    {
        int start = Current.Start;

        // Wildcard: '*' (any error) or '*:local' (any namespace).
        if (Match(TokenKind.Star))
        {
            if (Match(TokenKind.Colon))
            {
                var wildcardLocal = GetString(ExpectName());
                return new CatchCodePattern("*", wildcardLocal, null);
            }
            return new CatchCodePattern(null, null, null);
        }

        if (Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind))
        {
            var name = GetString(Current);

            // URI-qualified wildcard: Q{uri}* (including the empty URI Q{}*).
            if (name.Length > 2 && name[0] == 'Q' && name[1] == '{' && name[^1] == '*')
            {
                int closeBrace = name.IndexOf('}');
                if (closeBrace >= 2)
                {
                    Advance();
                    return new CatchCodePattern(null, null, name[2..closeBrace]);
                }
            }

            Advance();

            // prefix:* (namespace wildcard).
            if (Match(TokenKind.Colon) && Match(TokenKind.Star))
                return new CatchCodePattern(name, null, null);

            var (prefix, local, nsUri) = SplitQName(name);
            if (nsUri is not null)
                return new CatchCodePattern(null, local, nsUri);
            if (prefix is not null)
                return new CatchCodePattern(prefix, local, null);
            // An unprefixed code name matches the empty namespace.
            return new CatchCodePattern(null, local, "");
        }

        throw new ParseException($"XPST0003: Expected error code pattern but found {Current.Kind}", start);
    }

    // SwitchExpr ::= "switch" "(" Expr ")" SwitchCaseClause+ "default" "return" ExprSingle
    // SwitchCaseClause ::= ("case" ExprSingle)+ "return" ExprSingle
    // Consecutive 'case' keywords before a 'return' accumulate values of one clause.
    private XPathAstNode ParseSwitchExpr()
    {
        int start = Current.Start;
        Advance(); // 'switch'
        Expect(TokenKind.LParen);
        var operand = ParseExpr();
        Expect(TokenKind.RParen);

        var cases = new List<SwitchCaseClause>();
        while (Current.Kind == TokenKind.Name && GetString(Current) == "case")
        {
            Advance();
            var values = new List<XPathAstNode> { ParseExprSingle() };
            while (Current.Kind == TokenKind.Name && GetString(Current) == "case")
            {
                Advance();
                values.Add(ParseExprSingle());
            }
            Expect(TokenKind.KeywordReturn);
            cases.Add(new SwitchCaseClause(values, ParseExprSingle()));
        }
        if (cases.Count == 0)
            throw new ParseException("XPST0003: A switch expression requires at least one case clause.", Current.Start);
        if (Current.Kind != TokenKind.Name || GetString(Current) != "default")
            throw new ParseException("XPST0003: Expected 'default' in switch expression.", Current.Start);
        Advance();
        Expect(TokenKind.KeywordReturn);
        var defaultReturn = ParseExprSingle();
        return WithSpan(new SwitchExpressionNode(operand, cases, defaultReturn), start, End);
    }

    // TypeswitchExpr ::= "typeswitch" "(" Expr ")" CaseClause+ "default" ("$" VarName)? "return" ExprSingle
    // CaseClause ::= "case" ("$" VarName "as")? SequenceType "return" ExprSingle
    private XPathAstNode ParseTypeswitchExpr()
    {
        int start = Current.Start;
        Advance(); // 'typeswitch'
        Expect(TokenKind.LParen);
        var operand = ParseExpr();
        Expect(TokenKind.RParen);

        var cases = new List<TypeswitchCaseClause>();
        while (Current.Kind == TokenKind.Name && GetString(Current) == "case")
        {
            Advance();
            string? varLocal = null, varPrefix = null, varNs = null;
            if (Current.Kind == TokenKind.Dollar)
            {
                Advance();
                var nameTok = ExpectName();
                (varPrefix, varLocal, varNs) = SplitQName(GetString(nameTok));
                Expect(TokenKind.KeywordAs);
            }
            // SequenceTypeUnion ::= SequenceType ("|" SequenceType)*
            var types = new List<TypeswitchCaseType>();
            var (typePrefix, typeLocal, occurrence) = ParseSequenceType();
            types.Add(new TypeswitchCaseType(typePrefix, typeLocal, occurrence));
            while (Match(TokenKind.VBar))
            {
                (typePrefix, typeLocal, occurrence) = ParseSequenceType();
                types.Add(new TypeswitchCaseType(typePrefix, typeLocal, occurrence));
            }
            Expect(TokenKind.KeywordReturn);
            cases.Add(new TypeswitchCaseClause(types, ParseExprSingle(), varLocal, varPrefix, varNs));
        }
        if (cases.Count == 0)
            throw new ParseException("XPST0003: A typeswitch expression requires at least one case clause.", Current.Start);
        if (Current.Kind != TokenKind.Name || GetString(Current) != "default")
            throw new ParseException("XPST0003: Expected 'default' in typeswitch expression.", Current.Start);
        Advance();
        string? defLocal = null, defPrefix = null, defNs = null;
        if (Current.Kind == TokenKind.Dollar)
        {
            Advance();
            var nameTok = ExpectName();
            (defPrefix, defLocal, defNs) = SplitQName(GetString(nameTok));
        }
        Expect(TokenKind.KeywordReturn);
        var defaultReturn = ParseExprSingle();
        return WithSpan(new TypeswitchExpressionNode(operand, cases, defaultReturn, defLocal, defPrefix, defNs), start, End);
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

        // XQuery computed constructors (element/attribute/text/document/comment/PI/namespace)
        // are primary expressions, not name-test steps.
        if (_allowFullFlwor && Current.Kind == TokenKind.Name && IsComputedConstructorForm(GetString(Current)))
        {
            return ParsePostfixExpr();
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

            // URI-qualified wildcard: Q{uri}* (including empty URI Q{}*).
            if (name.Length > 3 && name[0] == 'Q' && name[1] == '{' && name[^1] == '*')
            {
                int closeBrace = name.IndexOf('}');
                if (closeBrace >= 2)
                {
                    Advance();
                    return new NodeTest(NameTestKind.NamespaceAny, null, name[2..closeBrace]);
                }
            }

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

            // The xml prefix is predefined in every XML document. Resolve it to the
            // standard XML namespace so node tests such as @xml:lang match attributes
            // in that namespace rather than being treated as prefix-only wildcards.
            if (prefix == "xml")
                return new NodeTest(NameTestKind.QName, local, "http://www.w3.org/XML/1998/namespace");

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
        if (Current.Kind == TokenKind.Name || IsKeywordName(Current.Kind))
        {
            var name = GetString(Current);
            // KeySpecifier allows only a plain NCName — qualified names (xs:integer,
            // Q{}integer) are a static error here (Lookup-156/157).
            if (name.Contains(':') || name.Contains('{'))
                throw new ParseException($"XPST0003: Qualified name '{name}' is not allowed as a lookup key", Current.Start);
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
            if (Match(TokenKind.RParen))
                return WithSpan(new SequenceExpressionNode(Array.Empty<XPathAstNode>()), start, End);
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
            case TokenKind.Constructor:
                return ParseDirectElementConstructor();

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

            case TokenKind.Question:
                // UnaryLookup: ?KS is equivalent to .?KS (lookup on the context item).
                Advance();
                if (Match(TokenKind.Star))
                    return WithSpan(new LookupWildcardNode(new ContextItemNode()), start, End);
                return WithSpan(new LookupNode(new ContextItemNode(), ParseLookupKey()), start, End);

            case TokenKind.Name:
                var name = GetString(Current);
                if (_allowFullFlwor && IsComputedConstructorForm(name))
                    return ParseComputedConstructor(start);
                var (prefix, local, _) = SplitQName(name);
                if (Peek(1).Kind == TokenKind.LParen)
                    return ParseFunctionCall(start);
                if (Peek(1).Kind == TokenKind.Hash)
                    return ParseNamedFunctionRef(start);
                throw new ParseException($"Unexpected name '{name}' in primary expression", start);

            case TokenKind.ValueIs:
                // 'is' is an operator keyword, but not a reserved function name,
                // so 'is()' is a valid function call (K-NodeSame-6).
                if (Peek(1).Kind == TokenKind.LParen)
                    return ParseFunctionCall(start);
                throw new ParseException($"Unexpected token {Current.Kind} in primary expression", start);

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

    // ------------------------------------------------------------------
    // XQuery computed constructors
    // ------------------------------------------------------------------

    private bool IsComputedConstructorForm(string name)
    {
        switch (name)
        {
            case "element":
            case "attribute":
            case "processing-instruction":
            case "namespace":
                // "keyword" "{" | "keyword" QName "{"
                // The name may itself be a keyword used as an ordinary NCName
                // (e.g. 'attribute return {()}' constructs an attribute named 'return').
                return Peek(1).Kind == TokenKind.LBrace ||
                       ((Peek(1).Kind == TokenKind.Name || IsKeywordName(Peek(1).Kind)) && Peek(2).Kind == TokenKind.LBrace);
            case "text":
            case "document":
            case "comment":
                return Peek(1).Kind == TokenKind.LBrace;
            default:
                return false;
        }
    }

    private XPathAstNode ParseComputedConstructor(int start)
    {
        string keyword = GetString(Current);
        Advance();

        switch (keyword)
        {
            case "element":
            case "attribute":
            case "processing-instruction":
            case "namespace":
            {
                // Name form: EQName (possibly Q{uri}local) or computed "{" Expr "}".
                XPathAstNode? nameExpression = null;
                string? local = null, prefix = null, ns = null;
                if (Current.Kind == TokenKind.LBrace)
                {
                    Advance();
                    nameExpression = ParseExpr();
                    Expect(TokenKind.RBrace);
                }
                else
                {
                    var nameTok = ExpectName();
                    (prefix, local, ns) = SplitQName(GetString(nameTok));
                    // A static processing-instruction target must be an NCName; a
                    // prefixed or URI-qualified name is a syntax error (XPST0003).
                    if (keyword == "processing-instruction" && (prefix is not null || ns is not null))
                        throw new ParseException($"XPST0003: A processing-instruction target must be an unprefixed NCName, not '{GetString(nameTok)}'.", start);
                }
                Expect(TokenKind.LBrace);
                // Computed constructor content may be empty ({}), producing the empty sequence.
                var content = Current.Kind == TokenKind.RBrace
                    ? new SequenceExpressionNode(Array.Empty<XPathAstNode>())
                    : ParseExpr();
                Expect(TokenKind.RBrace);
                return keyword switch
                {
                    "element" => WithSpan(new ComputedElementConstructorNode(nameExpression, local, prefix, ns, content), start, End),
                    "attribute" => WithSpan(new ComputedAttributeConstructorNode(nameExpression, local, prefix, ns, content), start, End),
                    "processing-instruction" => WithSpan(new ComputedPIConstructorNode(nameExpression, local, content), start, End),
                    _ => WithSpan(new ComputedNamespaceConstructorNode(nameExpression, local, content), start, End)
                };
            }
            case "text":
            case "document":
            case "comment":
            {
                Expect(TokenKind.LBrace);
                // Computed constructor content may be empty ({}), producing the empty sequence.
                var content = Current.Kind == TokenKind.RBrace
                    ? new SequenceExpressionNode(Array.Empty<XPathAstNode>())
                    : ParseExpr();
                Expect(TokenKind.RBrace);
                return keyword switch
                {
                    "text" => WithSpan(new ComputedTextConstructorNode(content), start, End),
                    "document" => WithSpan(new ComputedDocumentConstructorNode(content), start, End),
                    _ => WithSpan(new ComputedCommentConstructorNode(content), start, End)
                };
            }
            default:
                throw new ParseException($"XPST0003: Unknown computed constructor '{keyword}'.", start);
        }
    }

    private FunctionCallNode ParseFunctionCall(int start)
    {
        var name = GetString(Current);
        var (prefix, local, nsUri) = SplitQName(name);
        ThrowIfRemovedFunction(nsUri, local, start);
        Advance();
        var args = ParseArgumentList();
        return WithSpan(new FunctionCallNode(local, args, prefix, nsUri), start, End);
    }

    private NamedFunctionRefNode ParseNamedFunctionRef(int start)
    {
        var name = GetString(Current);
        var (prefix, local, nsUri) = SplitQName(name);
        ThrowIfRemovedFunction(nsUri, local, start);
        ThrowIfReservedFunctionName(prefix, local, start);
        Advance(); // name
        Advance(); // #
        var arityTok = Expect(TokenKind.IntegerLiteral);
        var arity = int.Parse(GetString(arityTok), CultureInfo.InvariantCulture);
        return WithSpan(new NamedFunctionRefNode(local, arity, prefix, nsUri), start, End);
    }

    private static void ThrowIfRemovedFunction(string? nsUri, string localName, int position)
    {
        if (nsUri == OldMapNamespace)
            throw new ParseException($"XPST0017: Function in obsolete map namespace '{nsUri}' is not available", position);

        if (!string.IsNullOrEmpty(nsUri) && RemovedFunctions.Contains((nsUri, localName)))
            throw new ParseException($"XPST0017: Function {{{nsUri}}}{localName} has been removed", position);
    }

    private static void ThrowIfReservedFunctionName(string? prefix, string localName, int position)
    {
        if (string.IsNullOrEmpty(prefix) && ReservedFunctionNames.Contains(localName))
            throw new ParseException($"XPST0003: '{localName}' is a reserved function name and cannot be used in a function call or named function reference", position);
    }

    private List<XPathAstNode> ParseArgumentList()
    {
        Expect(TokenKind.LParen);
        var args = new List<XPathAstNode>();
        if (!Match(TokenKind.RParen))
        {
            do
            {
                // A bare '?' is an argument placeholder only when it cannot start a
                // UnaryLookup, i.e. when the next token ends the argument (',' or ')').
                // Otherwise ('?1', '?name', '?(') it is a lookup on the context item.
                if (Current.Kind == TokenKind.Question
                    && (Peek(1).Kind == TokenKind.Comma || Peek(1).Kind == TokenKind.RParen))
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
            // EnclosedExpr ::= "{" Expr? "}" — an empty body evaluates to the empty sequence.
            body = Current.Kind == TokenKind.RBrace
                ? new SequenceExpressionNode(Array.Empty<XPathAstNode>())
                : ParseExpr();
            Expect(TokenKind.RBrace);
        }
        else
        {
            body = ParsePrimaryExpr();
        }
        return WithSpan(new InlineFunctionNode(parameters, body, returnType), start, End);
    }

    // ------------------------------------------------------------------
    // XQuery direct element constructors
    // ------------------------------------------------------------------

    private XPathAstNode ParseDirectElementConstructor()
    {
        // The lexer emitted the whole constructor as a single token; build the AST by
        // scanning the raw source text (attribute values and text are not tokenizable).
        int start = Current.Start;
        int pos = start;
        char after = pos + 1 < _source.Length ? _source[pos + 1] : '\0';
        XPathAstNode node;
        if (_source[pos] == '`')
        {
            node = ScanStringConstructor(ref pos, start);
        }
        else if (after == '?')
        {
            node = ScanPiConstructor(ref pos, start);
        }
        else if (after == '!')
        {
            node = ScanCommentConstructor(ref pos, start);
        }
        else
        {
            node = ScanElementConstructor(ref pos, start);
        }
        Advance();
        return WithSpan(node, start, pos);
    }

    // Scans an XQuery string constructor "``[ ... ]``": literal text runs become
    // StringLiteralNode parts, interpolations "`{` Expr `}`" become expression parts.
    private XPathAstNode ScanStringConstructor(ref int pos, int ctorStart)
    {
        // pos is at the first '`' of "``[".
        pos += 3;
        var parts = new List<XPathAstNode>();
        var text = new StringBuilder();
        void FlushText()
        {
            if (text.Length > 0)
            {
                parts.Add(new StringLiteralNode(text.ToString()));
                text.Clear();
            }
        }
        while (true)
        {
            if (pos >= _source.Length)
                throw ConstructorError("unterminated string constructor", ctorStart);
            char c = _source[pos];
            if (c == ']' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '`')
            {
                pos += 3;
                FlushText();
                return new StringConstructorNode(parts);
            }
            if (c == '`' && pos + 1 < _source.Length && _source[pos + 1] == '{')
            {
                FlushText();
                parts.Add(ScanStringInterpolation(ref pos, ctorStart));
                continue;
            }
            text.Append(c);
            pos++;
        }
    }

    // Scans one string-constructor interpolation body (pos is at the '`' of "`{") and
    // returns the parsed expression; an empty or comment-only body is the empty sequence.
    private XPathAstNode ScanStringInterpolation(ref int pos, int ctorStart)
    {
        int exprStart = (pos += 2);
        int depth = 1;
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == '\'' || c == '"')
            {
                char q = c;
                pos++;
                while (pos < _source.Length)
                {
                    if (_source[pos] == q)
                    {
                        if (pos + 1 < _source.Length && _source[pos + 1] == q)
                        {
                            pos += 2;
                            continue;
                        }
                        pos++;
                        break;
                    }
                    pos++;
                }
                continue;
            }
            if (c == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':')
            {
                SkipConstructorComment(ref pos);
                continue;
            }
            if (c == '`' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '[')
            {
                // A nested string constructor inside the interpolation expression.
                if (!SkipStringConstructorSpan(ref pos))
                    throw ConstructorError("unterminated string constructor", ctorStart);
                continue;
            }
            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    // The interpolation must close with '}`' (XPST0003 otherwise).
                    if (pos + 1 >= _source.Length || _source[pos + 1] != '`')
                        throw new ParseException("XPST0003: An interpolation in a string constructor must end with '}`'.", pos);
                    var inner = _source[exprStart..pos];
                    pos += 2;
                    // An empty or comment-only interpolation is the empty sequence.
                    if (string.IsNullOrWhiteSpace(StripXQueryComments(inner)))
                        return new SequenceExpressionNode(Array.Empty<XPathAstNode>());
                    return Parse(inner, _allowFullFlwor);
                }
            }
            pos++;
        }
        throw ConstructorError("unterminated interpolation in string constructor", ctorStart);
    }

    // Skips a nested string constructor span (pos is at the first '`' of "``["),
    // interpolation-aware. Returns false when unterminated.
    private bool SkipStringConstructorSpan(ref int pos)
    {
        pos += 3;
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == ']' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '`')
            {
                pos += 3;
                return true;
            }
            if (c == '`' && pos + 1 < _source.Length && _source[pos + 1] == '{')
            {
                // Skip the nested interpolation with the same scanning rules.
                int exprStart = (pos += 2);
                int depth = 1;
                bool closed = false;
                while (pos < _source.Length)
                {
                    char ic = _source[pos];
                    if (ic == '\'' || ic == '"')
                    {
                        char q = ic;
                        pos++;
                        while (pos < _source.Length)
                        {
                            if (_source[pos] == q)
                            {
                                if (pos + 1 < _source.Length && _source[pos + 1] == q)
                                {
                                    pos += 2;
                                    continue;
                                }
                                pos++;
                                break;
                            }
                            pos++;
                        }
                        continue;
                    }
                    if (ic == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':')
                    {
                        SkipConstructorComment(ref pos);
                        continue;
                    }
                    if (ic == '`' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '[')
                    {
                        if (!SkipStringConstructorSpan(ref pos))
                            return false;
                        continue;
                    }
                    if (ic == '{')
                    {
                        depth++;
                    }
                    else if (ic == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            if (pos + 1 >= _source.Length || _source[pos + 1] != '`')
                                return false;
                            pos += 2;
                            closed = true;
                            break;
                        }
                    }
                    pos++;
                }
                if (!closed)
                    return false;
                continue;
            }
            pos++;
        }
        return false;
    }

    private void SkipConstructorComment(ref int pos)
    {
        // pos is at '(:' — XQuery comments nest.
        int depth = 1;
        pos += 2;
        while (pos < _source.Length && depth > 0)
        {
            if (_source[pos] == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':') { depth++; pos += 2; }
            else if (_source[pos] == ':' && pos + 1 < _source.Length && _source[pos + 1] == ')') { depth--; pos += 2; }
            else pos++;
        }
    }

    private DirectProcessingInstructionNode ScanPiConstructor(ref int pos, int ctorStart)
    {
        // pos is at '<', source[pos+1] is '?'.
        pos += 2;
        int targetStart = pos;
        if (pos >= _source.Length || (!char.IsLetter(_source[pos]) && _source[pos] != '_'))
            throw ConstructorError("expected a processing instruction target", pos);
        while (pos < _source.Length && IsConstructorNameChar(_source[pos]))
            pos++;
        var target = _source[targetStart..pos];
        if (target.Equals("xml", StringComparison.OrdinalIgnoreCase))
            throw ConstructorError("processing instruction target 'xml' is reserved", targetStart);

        SkipConstructorWhitespace(ref pos);
        int close = _source.IndexOf("?>", pos, StringComparison.Ordinal);
        if (close < 0)
            throw ConstructorError("unterminated processing instruction", ctorStart);
        var data = _source[pos..close];
        pos = close + 2;
        return new DirectProcessingInstructionNode(target, data);
    }

    private DirectCommentNode ScanCommentConstructor(ref int pos, int ctorStart)
    {
        // pos is at '<', source is "<!--".
        int dataStart = pos + 4;
        int close = _source.IndexOf("-->", dataStart, StringComparison.Ordinal);
        if (close < 0)
            throw ConstructorError("unterminated comment", ctorStart);
        var data = _source[dataStart..close];
        if (data.Contains("--", StringComparison.Ordinal) || data.EndsWith('-'))
            throw ConstructorError("comment must not contain '--' or end with '-'", dataStart);
        pos = close + 3;
        return new DirectCommentNode(data);
    }

    private XPathAstNode ScanElementConstructor(ref int pos, int ctorStart)
    {
        // pos is at '<'.
        pos++;
        var (tagPrefix, tagLocal) = ScanConstructorQName(ref pos, ctorStart);

        var attributes = new List<DirectAttributeNode>();
        var content = new List<XPathAstNode>();
        var declaredPrefixes = new HashSet<string>(StringComparer.Ordinal);
        bool xmlSpacePreserve = false;

        while (true)
        {
            SkipConstructorWhitespace(ref pos);
            if (pos >= _source.Length)
                throw ConstructorError("unterminated element constructor", ctorStart);
            char c = _source[pos];
            if (c == '>')
            {
                pos++;
                break;
            }
            if (c == '/' && pos + 1 < _source.Length && _source[pos + 1] == '>')
            {
                pos += 2;
                return new DirectElementConstructorNode(tagLocal, tagPrefix, attributes, content);
            }

            var (attrPrefix, attrLocal) = ScanConstructorQName(ref pos, ctorStart);
            SkipConstructorWhitespace(ref pos);
            if (pos >= _source.Length || _source[pos] != '=')
                throw ConstructorError("expected '=' after attribute name", pos);
            pos++;
            SkipConstructorWhitespace(ref pos);
            if (pos >= _source.Length || (_source[pos] != '"' && _source[pos] != '\''))
                throw ConstructorError("expected quoted attribute value", pos);
            char quote = _source[pos++];
            bool isNamespaceDecl = (attrLocal == "xmlns" && attrPrefix is null) || attrPrefix == "xmlns";
            var valueParts = ScanConstructorAttributeValue(ref pos, quote, ctorStart, isNamespaceDecl);

            // XQST0070: the xml prefix must only be declared with the XML namespace URI,
            // no other prefix may use the XML namespace URI, and xmlns must not be declared.
            if (isNamespaceDecl)
            {
                var nsValue = string.Concat(valueParts.OfType<StringLiteralNode>().Select(p => p.Value));
                if (attrPrefix == "xmlns" && attrLocal == "xmlns")
                    throw new ParseException("XQST0070: The 'xmlns' prefix must not be declared.", ctorStart);
                if (attrPrefix == "xmlns" && attrLocal == "xml" &&
                    nsValue != "http://www.w3.org/XML/1998/namespace")
                    throw new ParseException("XQST0070: The 'xml' prefix must only be bound to the XML namespace URI.", ctorStart);
                if (attrPrefix == "xmlns" && attrLocal != "xml" &&
                    nsValue == "http://www.w3.org/XML/1998/namespace")
                    throw new ParseException("XQST0070: The XML namespace URI must only be bound to the 'xml' prefix.", ctorStart);
            }

            // xml:space is an ordinary attribute for element constructors: it does NOT
            // affect boundary-whitespace stripping (XQuery 3.1 §3.9.1.1 note; the
            // serializer honors it instead). XQST0071: duplicate prefix declarations.
            if (isNamespaceDecl)
            {
                string declPrefix = attrPrefix == "xmlns" ? attrLocal : "";
                if (!declaredPrefixes.Add(declPrefix))
                    throw new ParseException($"XQST0071: The namespace prefix '{(declPrefix.Length == 0 ? "(default)" : declPrefix)}' is declared more than once.", ctorStart);
            }

            attributes.Add(new DirectAttributeNode(attrLocal, attrPrefix, valueParts));
        }

        ScanConstructorContent(ref pos, tagLocal, tagPrefix, content, ctorStart, xmlSpacePreserve);

        return new DirectElementConstructorNode(tagLocal, tagPrefix, attributes, content);
    }

    private void ScanConstructorContent(ref int pos, string tagLocal, string? tagPrefix, List<XPathAstNode> content, int ctorStart, bool xmlSpacePreserve)
    {
        var text = new StringBuilder();
        bool textHasReference = false;
        void FlushText()
        {
            if (text.Length > 0)
            {
                // Text containing a character/entity reference is never boundary whitespace.
                // A plain whitespace-only literal run IS boundary whitespace and is stripped
                // (unless xml:space="preserve"). Enclosed expressions never pass through
                // here, so a whitespace-only {' '} result is always preserved.
                if (textHasReference || xmlSpacePreserve || !IsXmlWhitespaceOnly(text.ToString()))
                {
                    content.Add(textHasReference
                        ? new SignificantTextNode(text.ToString())
                        : new StringLiteralNode(text.ToString()));
                }
                text.Clear();
            }
            // Always reset: a reference/CDATA only protects the text run it belongs to.
            textHasReference = false;
        }

        while (true)
        {
            if (pos >= _source.Length)
                throw ConstructorError("unterminated element content", ctorStart);
            char c = _source[pos];
            if (c == '<')
            {
                if (pos + 1 >= _source.Length)
                    throw ConstructorError("unterminated element content", ctorStart);
                char next = _source[pos + 1];
                if (next == '/')
                {
                    pos += 2;
                    var (endPrefix, endLocal) = ScanConstructorQName(ref pos, ctorStart);
                    SkipConstructorWhitespace(ref pos);
                    if (pos >= _source.Length || _source[pos] != '>')
                        throw ConstructorError("expected '>' in end tag", pos);
                    pos++;
                    if (endLocal != tagLocal || endPrefix != tagPrefix)
                    {
                        string expected = tagPrefix is null ? tagLocal : $"{tagPrefix}:{tagLocal}";
                        string found = endPrefix is null ? endLocal : $"{endPrefix}:{endLocal}";
                        throw new ParseException($"XQST0118: Mismatched end tag '</{found}>' (expected '</{expected}>').", pos);
                    }
                    FlushText();
                    return;
                }
                if (next == '!')
                {
                    if (_source.AsSpan(pos).StartsWith("<!--", StringComparison.Ordinal))
                    {
                        FlushText();
                        int dataStart = pos + 4;
                        int close = _source.IndexOf("-->", dataStart, StringComparison.Ordinal);
                        if (close < 0)
                            throw ConstructorError("unterminated comment", pos);
                        var data = _source[dataStart..close];
                        if (data.Contains("--", StringComparison.Ordinal) || data.EndsWith('-'))
                            throw ConstructorError("comment must not contain '--' or end with '-'", pos);
                        content.Add(new DirectCommentNode(data));
                        pos = close + 3;
                        continue;
                    }
                    if (_source.AsSpan(pos).StartsWith("<![CDATA[", StringComparison.Ordinal))
                    {
                        int dataStart = pos + 9;
                        int close = _source.IndexOf("]]>", dataStart, StringComparison.Ordinal);
                        if (close < 0)
                            throw ConstructorError("unterminated CDATA section", pos);
                        // CDATA content (and its mere presence) is never boundary whitespace:
                        // even an empty CDATA section protects the surrounding text run.
                        text.Append(NormalizeNewlines(_source[dataStart..close]));
                        textHasReference = true;
                        pos = close + 3;
                        continue;
                    }
                    throw ConstructorError("unsupported markup declaration in element content", pos);
                }
                if (next == '?')
                {
                    FlushText();
                    int dataStart = pos + 2;
                    int close = _source.IndexOf("?>", dataStart, StringComparison.Ordinal);
                    if (close < 0)
                        throw ConstructorError("unterminated processing instruction", pos);
                    var pi = _source[dataStart..close];
                    int space = IndexOfWhitespace(pi);
                    var (target, data) = space < 0 ? (pi, string.Empty) : (pi[..space], pi[(space + 1)..]);
                    if (target.Length == 0 || target.Equals("xml", StringComparison.OrdinalIgnoreCase))
                        throw ConstructorError($"invalid processing instruction target '{target}'", pos);
                    content.Add(new DirectProcessingInstructionNode(target, data));
                    pos = close + 2;
                    continue;
                }
                if (char.IsLetter(next) || next == '_')
                {
                    FlushText();
                    content.Add(ScanElementConstructor(ref pos, ctorStart));
                    continue;
                }
                throw ConstructorError("unexpected '<' in element content", pos);
            }
            if (c == '{')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '{')
                {
                    text.Append('{');
                    pos += 2;
                    continue;
                }
                FlushText();
                content.Add(ScanEnclosedExpression(ref pos, ctorStart));
                continue;
            }
            if (c == '}')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '}')
                {
                    text.Append('}');
                    pos += 2;
                    continue;
                }
                throw ConstructorError("unescaped '}' in element content (use '}}')", pos);
            }
            if (c == '&')
            {
                text.Append(ScanConstructorCharReference(ref pos, ctorStart));
                textHasReference = true;
                continue;
            }
            if (c == '\r')
            {
                text.Append('\n');
                pos++;
                if (pos < _source.Length && _source[pos] == '\n')
                    pos++;
                continue;
            }
            text.Append(c);
            pos++;
        }
    }

    private IReadOnlyList<XPathAstNode> ScanConstructorAttributeValue(ref int pos, char quote, int ctorStart, bool isNamespaceDecl = false)
    {
        var parts = new List<XPathAstNode>();
        var text = new StringBuilder();
        void FlushText()
        {
            if (text.Length > 0)
            {
                parts.Add(new StringLiteralNode(text.ToString()));
                text.Clear();
            }
        }

        while (true)
        {
            if (pos >= _source.Length)
                throw ConstructorError("unterminated attribute value", ctorStart);
            char c = _source[pos];
            if (c == quote)
            {
                // A doubled quote is an escaped literal quote in the value.
                if (pos + 1 < _source.Length && _source[pos + 1] == quote)
                {
                    text.Append(quote);
                    pos += 2;
                    continue;
                }
                pos++;
                FlushText();
                var result = parts;
                if (isNamespaceDecl)
                {
                    var uri = string.Concat(result.OfType<StringLiteralNode>().Select(p => p.Value));
                    if (uri.Any(char.IsWhiteSpace))
                        throw new ParseException($"XQST0046: Invalid character in namespace URI '{uri}'.", ctorStart);
                }
                return result;
            }
            if (c == '{')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '{')
                {
                    text.Append('{');
                    pos += 2;
                    continue;
                }
                if (isNamespaceDecl)
                    throw new ParseException("XQST0022: A namespace declaration must have a literal URI.", pos);
                FlushText();
                parts.Add(ScanEnclosedExpression(ref pos, ctorStart));
                continue;
            }
            if (c == '}')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '}')
                {
                    text.Append('}');
                    pos += 2;
                    continue;
                }
                throw ConstructorError("unescaped '}' in attribute value (use '}}')", pos);
            }
            if (c == '&')
            {
                text.Append(ScanConstructorCharReference(ref pos, ctorStart));
                continue;
            }
            if (c == '<')
            {
                // A raw '<' is never allowed literally in an attribute value (use '&lt;').
                throw ConstructorError("attribute values must not contain a literal '<'", pos);
            }
            // XML attribute-value normalization: raw whitespace becomes a space.
            text.Append(c is '\t' or '\n' or '\r' ? ' ' : c);
            pos++;
        }
    }

    private XPathAstNode ScanEnclosedExpression(ref int pos, int ctorStart)
    {
        // pos is at '{'.
        int exprStart = ++pos;
        int depth = 1;
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    var inner = _source[exprStart..pos];
                    pos++;
                    // An enclosed expression with no tokens at all (pure whitespace) is a
                    // syntax error; a comment-only expression evaluates to the empty sequence.
                    if (string.IsNullOrWhiteSpace(inner))
                        throw new ParseException("XPST0003: An enclosed expression must not be empty.", exprStart);
                    if (string.IsNullOrWhiteSpace(StripXQueryComments(inner)))
                        return new SequenceExpressionNode(Array.Empty<XPathAstNode>());
                    return Parse(inner, _allowFullFlwor);
                }
            }
            else if (c == '\'' || c == '"')
            {
                char q = c;
                pos++;
                while (pos < _source.Length)
                {
                    if (_source[pos] == q)
                    {
                        if (pos + 1 < _source.Length && _source[pos + 1] == q)
                        {
                            pos += 2;
                            continue;
                        }
                        break;
                    }
                    pos++;
                }
            }
            else if (c == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':')
            {
                int commentDepth = 1;
                pos += 2;
                while (pos < _source.Length && commentDepth > 0)
                {
                    if (_source[pos] == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':')
                    {
                        commentDepth++;
                        pos += 2;
                    }
                    else if (_source[pos] == ':' && pos + 1 < _source.Length && _source[pos + 1] == ')')
                    {
                        commentDepth--;
                        pos += 2;
                    }
                    else
                    {
                        pos++;
                    }
                }
                continue;
            }
            pos++;
        }
        throw ConstructorError("unterminated enclosed expression", ctorStart);
    }

    private string ScanConstructorCharReference(ref int pos, int ctorStart)
    {
        int semi = _source.IndexOf(';', pos + 1);
        if (semi < 0)
            throw ConstructorError("unterminated entity or character reference", pos);
        var reference = _source[(pos + 1)..semi];
        pos = semi + 1;
        return reference switch
        {
            "amp" => "&",
            "lt" => "<",
            "gt" => ">",
            "quot" => "\"",
            "apos" => "'",
            _ when reference.StartsWith("#x", StringComparison.Ordinal) &&
                   int.TryParse(reference[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) =>
                ValidateCharReference(hex, ctorStart),
            _ when reference.StartsWith('#') &&
                   int.TryParse(reference[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec) =>
                ValidateCharReference(dec, ctorStart),
            _ => throw ConstructorError($"invalid entity or character reference '&{reference};'", ctorStart)
        };
    }

    // XQST0090: a character reference must denote a valid XML 1.1 character (any
    // codepoint except NUL, surrogates, and noncharacters; control characters are
    // permitted as references in XML 1.1).
    private string ValidateCharReference(int codePoint, int ctorStart)
    {
        bool valid = codePoint is >= 0x1 and <= 0xD7FF
            || (codePoint >= 0xE000 && codePoint <= 0xFFFD)
            || (codePoint >= 0x10000 && codePoint <= 0x10FFFF);
        if (!valid)
            throw new ParseException($"XQST0090: Character reference '&#{codePoint};' does not denote a valid XML character.", ctorStart);
        return char.ConvertFromUtf32(codePoint);
    }

    private (string? Prefix, string Local) ScanConstructorQName(ref int pos, int ctorStart)
    {
        int nameStart = pos;
        if (pos >= _source.Length || (!char.IsLetter(_source[pos]) && _source[pos] != '_'))
            throw ConstructorError("expected a name", pos);
        pos++;
        while (pos < _source.Length && IsConstructorNameChar(_source[pos]))
            pos++;
        var first = _source[nameStart..pos];
        if (pos < _source.Length && _source[pos] == ':')
        {
            pos++;
            int localStart = pos;
            if (pos >= _source.Length || (!char.IsLetter(_source[pos]) && _source[pos] != '_'))
                throw ConstructorError("expected a local name after ':'", pos);
            pos++;
            while (pos < _source.Length && IsConstructorNameChar(_source[pos]))
                pos++;
            return (first, _source[localStart..pos]);
        }
        return (null, first);
    }

    private static bool IsConstructorNameChar(char c) =>
        char.IsLetterOrDigit(c) || c is '_' or '-' or '.';

    private void SkipConstructorWhitespace(ref int pos)
    {
        while (pos < _source.Length && char.IsWhiteSpace(_source[pos]))
            pos++;
    }

    private static int IndexOfWhitespace(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
                return i;
        }
        return -1;
    }

    private static string NormalizeNewlines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n');

    /// <summary>Removes XQuery comments (possibly nested) so emptiness can be detected.</summary>
    private static string StripXQueryComments(string text)
    {
        var sb = new StringBuilder(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '(' && i + 1 < text.Length && text[i + 1] == ':')
            {
                int depth = 1;
                i += 2;
                while (i < text.Length && depth > 0)
                {
                    if (text[i] == '(' && i + 1 < text.Length && text[i + 1] == ':') { depth++; i += 2; }
                    else if (text[i] == ':' && i + 1 < text.Length && text[i + 1] == ')') { depth--; i += 2; }
                    else i++;
                }
                continue;
            }
            sb.Append(text[i]);
            i++;
        }
        return sb.ToString();
    }

    private static bool IsXmlWhitespaceOnly(string text)
    {
        foreach (char c in text)
        {
            if (c is not (' ' or '\t' or '\n' or '\r'))
                return false;
        }
        return true;
    }

    private ParseException ConstructorError(string message, int pos) =>
        new($"XPST0003: {message}.", pos);

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
                if (Current.Kind == TokenKind.Assign)
                    throw new ParseException("XPST0003: Invalid map constructor syntax (use ':' to separate key and value)", Current.Start);
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
        // The empty URI form Q{}local is permitted and means "no namespace".
        // Whitespace in the URI part is not significant: leading/trailing whitespace
        // is stripped and internal runs are collapsed to a single space.
        if (qname.Length > 2 && qname[0] == 'Q' && qname[1] == '{')
        {
            int closeBrace = qname.IndexOf('}');
            if (closeBrace >= 2)
            {
                // Braced URI literals may not contain braces (XPST0003).
                if (qname[2..closeBrace].Contains('{'))
                    throw new ParseException($"XPST0003: Braces are not allowed in the URI part of an EQName ('{qname}').", 0);
                string nsUri = NormalizeEQNameUri(ExpandEQNameRefs(qname[2..closeBrace]));
                string rest = qname[(closeBrace + 1)..];
                int restColon = rest.IndexOf(':');
                return restColon < 0 ? (null, rest, nsUri) : (rest[..restColon], rest[(restColon + 1)..], nsUri);
            }
        }

        int colon = qname.IndexOf(':');
        return colon < 0 ? (null, qname, null) : (qname[..colon], qname[(colon + 1)..], null);
    }

    /// <summary>
    /// Normalizes the URI part of an EQName: strips leading/trailing whitespace and
    /// collapses internal whitespace runs (space, tab, CR, LF) to a single space.
    /// </summary>
    private static string NormalizeEQNameUri(string uri)
    {
        uri = uri.Trim();
        if (uri.Length == 0) return uri;
        var sb = new System.Text.StringBuilder(uri.Length);
        bool pendingSpace = false;
        foreach (char c in uri)
        {
            if (c is ' ' or '\t' or '\n' or '\r')
            {
                pendingSpace = true;
                continue;
            }
            if (pendingSpace && sb.Length > 0)
                sb.Append(' ');
            pendingSpace = false;
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Expands character references (<c>&amp;#x20;</c>, <c>&amp;#32;</c>) and predefined
    /// entity references (<c>&amp;amp;</c>, <c>&amp;lt;</c>, <c>&amp;gt;</c>, <c>&amp;quot;</c>,
    /// <c>&amp;apos;</c>) in the braced URI literal of an EQName. Malformed or unknown
    /// references are a syntax error (XPST0003).
    /// </summary>
    private static string ExpandEQNameRefs(string uri)
    {
        if (!uri.Contains('&'))
            return uri;
        var sb = new System.Text.StringBuilder(uri.Length);
        for (int i = 0; i < uri.Length; i++)
        {
            char c = uri[i];
            if (c != '&')
            {
                sb.Append(c);
                continue;
            }
            int semi = uri.IndexOf(';', i + 1);
            if (semi < 0)
                throw new ParseException($"XPST0003: Unterminated reference in EQName URI ('{uri}').", 0);
            var body = uri[(i + 1)..semi];
            if (body.StartsWith("#x", StringComparison.Ordinal) || body.StartsWith("#X", StringComparison.Ordinal))
            {
                if (!int.TryParse(body[2..], System.Globalization.NumberStyles.HexNumber, null, out int hex) || hex is < 1 or > 0x10FFFF)
                    throw new ParseException($"XPST0003: Invalid character reference '&{body};' in EQName URI.", 0);
                sb.Append(char.ConvertFromUtf32(hex));
            }
            else if (body.StartsWith('#'))
            {
                if (!int.TryParse(body[1..], System.Globalization.NumberStyles.None, null, out int dec) || dec is < 1 or > 0x10FFFF)
                    throw new ParseException($"XPST0003: Invalid character reference '&{body};' in EQName URI.", 0);
                sb.Append(char.ConvertFromUtf32(dec));
            }
            else
            {
                sb.Append(body switch
                {
                    "amp" => '&',
                    "lt" => '<',
                    "gt" => '>',
                    "quot" => '"',
                    "apos" => '\'',
                    _ => throw new ParseException($"XPST0003: Unknown entity reference '&{body};' in EQName URI.", 0)
                });
            }
            i = semi;
        }
        return sb.ToString();
    }

    private static bool IsKindTestName(string localName) => localName switch
    {
        "node" or "text" or "comment" or "processing-instruction" or "namespace-node"
        or "element" or "attribute" or "schema-element" or "schema-attribute"
        or "document-node" or "item" => true,
        _ => false
    };

    private string Unquote(string text)
    {
        if (text.Length >= 2 &&
            ((text[0] == '\'' && text[^1] == '\'') || (text[0] == '"' && text[^1] == '"')))
        {
            char quote = text[0];
            var inner = text[1..^1];
            var sb = new StringBuilder(inner.Length);
            for (int i = 0; i < inner.Length; i++)
            {
                // Predefined entity and character references expand in XQuery string
                // literals only; XPath 3.1 does not expand them (assert-eq expectations
                // evaluate per XPath rules, e.g. string-constructor-029).
                if (inner[i] == '&' && _allowFullFlwor)
                {
                    sb.Append(ExpandCharReference(inner, ref i));
                    continue;
                }
                sb.Append(inner[i]);
                if (inner[i] == quote && i + 1 < inner.Length && inner[i + 1] == quote)
                    i++;
            }
            // Line-ending normalization applies only in XML 1.1 mode (xml-version
            // dependency); otherwise references produce their exact characters.
            return _xml11LineEndings ? NormalizeXml11LineEndings(sb.ToString()) : sb.ToString();
        }
        return text;
    }

    // XML 1.1 line-ending normalization (applied only in XML 1.1 mode): #xD#xA and
    // #xD#x85 normalize to #xA, and lone #xD, #x85 (NEL), and #x2028 (LS) to #xA.
    private static string NormalizeXml11LineEndings(string text)
    {
        if (text.IndexOfAny('\r', '\u0085', '\u2028') < 0)
            return text;
        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\r')
            {
                sb.Append('\n');
                if (i + 1 < text.Length && (text[i + 1] == '\n' || text[i + 1] == '\u0085'))
                    i++;
                continue;
            }
            if (c is '\u0085' or '\u2028')
            {
                sb.Append('\n');
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    private string ExpandCharReference(string inner, ref int i)
    {
        int semi = inner.IndexOf(';', i + 1);
        if (semi < 0)
            throw new ParseException("XPST0003: Unterminated entity or character reference in string literal.", Current.Start);
        var reference = inner[(i + 1)..semi];
        string result = reference switch
        {
            "amp" => "&",
            "lt" => "<",
            "gt" => ">",
            "quot" => "\"",
            "apos" => "'",
            _ when reference.StartsWith("#x", StringComparison.Ordinal) &&
                   int.TryParse(reference[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) &&
                   hex is >= 0 and <= 0x10FFFF => char.ConvertFromUtf32(hex),
            _ when reference.StartsWith('#') &&
                   int.TryParse(reference[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec) &&
                   dec is >= 0 and <= 0x10FFFF => char.ConvertFromUtf32(dec),
            _ => throw new ParseException($"XPST0003: Invalid entity or character reference '&{reference};' in string literal.", Current.Start)
        };
        i = semi;
        return result;
    }

    private static bool CanStartStepExpr(Token token) => token.Kind switch
    {
        TokenKind.Dot or TokenKind.DotDot or TokenKind.At or TokenKind.Star
        or TokenKind.Name or TokenKind.Dollar or TokenKind.LParen
        or TokenKind.StringLiteral or TokenKind.IntegerLiteral
        or TokenKind.DecimalLiteral or TokenKind.DoubleLiteral
        or TokenKind.KeywordFunction or TokenKind.KeywordMap
        or TokenKind.KeywordArray or TokenKind.LBracket => true,
        _ => IsKeywordName(token.Kind)
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
                // Expression boundaries: a sequence type never contains these at top level,
                // so a function-type return type must stop here (let-`as` before ':=' /
                // 'in' / 'return', typeswitch unions before '|', if-conditions before
                // 'then', etc.).
                if (Current.Kind is TokenKind.Assign or TokenKind.VBar
                    or TokenKind.KeywordIn or TokenKind.KeywordReturn or TokenKind.KeywordThen
                    or TokenKind.KeywordElse or TokenKind.KeywordSatisfies
                    or TokenKind.KeywordFor or TokenKind.KeywordLet)
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

        var (prefix, local, nsUri) = SplitQName(name);

        // If the input was a braced URI literal (e.g. Q{http://www.w3.org/2001/XMLSchema}double),
        // restore the conventional prefix so that downstream sequence-type processing can
        // recognise standard type families (xs:*, map:*, array:*).
        if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(nsUri))
        {
            prefix = nsUri switch
            {
                "http://www.w3.org/2001/XMLSchema" => "xs",
                "http://www.w3.org/2005/xpath-functions/map" => "map",
                "http://www.w3.org/2005/xpath-functions/array" => "array",
                _ => prefix
            };
        }

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
                else if (Current.Kind == TokenKind.KeywordAs)
                {
                    // Nested function tests: keep ' as ' separated so that the
                    // concatenated type string remains parseable (ArrayTest-063).
                    sb.Append(' ');
                    sb.Append(GetString(Current));
                    sb.Append(' ');
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

            if (parenDepth > 0)
            {
                throw new ParseException("Unclosed sequence type parenthesis", Current.Start);
            }
        }

        return (prefix, local, hasParens);
    }
}

// ------------------------------------------------------------------
// Supporting AST nodes used by parser
// ------------------------------------------------------------------


