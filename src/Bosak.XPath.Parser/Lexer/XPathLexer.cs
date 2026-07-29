// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A zero-allocation XPath 3.1 lexer operating over <see cref="ReadOnlySpan{char}"/>
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-06-2026     | Trailing dot in number is DecimalLiteral (fixes select-3501/3502)                        |
//                      | Charles Korthout | 0.3   | 26-06-2026     | Lex Q{uri}* URI-qualified wildcards                                                      |
//                      | Charles Korthout | 0.4   | 19-07-2026     | NumericLiteral followed by NameStartChar is Invalid (10idiv → XPST0003)                 |
//                      | Charles Korthout | 0.5   | 20-07-2026     | Unterminated XPath comments now raise XPST0003                                         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 25-07-2026     | Constructor mode: direct element/comment/PI constructors as single tokens               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 27-07-2026     | Whole-span token scan for XQuery string constructors (interpolation/nesting aware) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.CompilerServices;
using Bosak.XPath.Parser;

namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// A zero-allocation XPath 3.1 lexer operating over <see cref="ReadOnlySpan{char}"/>.
/// </summary>
public ref struct XPathLexer
{
    private readonly ReadOnlySpan<char> _source;
    private readonly bool _allowConstructors;
    private int _position;

    public XPathLexer(ReadOnlySpan<char> source, bool allowConstructors = false)
    {
        _source = source;
        _allowConstructors = allowConstructors;
        _position = 0;
    }

    public ReadOnlySpan<char> Source => _source;
    public int Position => _position;
    public bool IsAtEnd => _position >= _source.Length;

    /// <summary>
    /// Returns the next token and advances the lexer.
    /// </summary>
    public Token NextToken()
    {
        SkipWhitespaceAndComments();

        if (_position >= _source.Length)
            return Token.Eof;

        int start = _position;
        char c = _source[_position];

        // ---- XQuery direct element constructors -----------------------
        // '<' followed by a name-start char begins a constructor; the whole span
        // (attributes, text, nested elements, enclosed expressions) is one token.
        // Anything that does not match the structure falls back to the '<' operator.
        if (_allowConstructors && c == '<' && _position + 1 < _source.Length && IsNameStartChar(_source[_position + 1]))
        {
            var ctor = TryScanConstructor(start);
            if (ctor.Kind != TokenKind.Invalid)
            {
                _position = start + ctor.Length;
                return ctor;
            }
            _position = start;
        }

        // Direct comment / processing-instruction constructors (standalone expressions).
        if (_allowConstructors && c == '<' && _position + 1 < _source.Length && _source[_position + 1] == '?')
        {
            int close = IndexOf(_position + 2, "?>");
            if (close >= 0)
            {
                _position = close + 2;
                return new Token(TokenKind.Constructor, start, _position - start);
            }
        }
        if (_allowConstructors && c == '<' && _position + 1 < _source.Length && _source[_position + 1] == '!' &&
            _position + 3 < _source.Length && _source[_position + 2] == '-' && _source[_position + 3] == '-')
        {
            int close = IndexOf(_position + 4, "-->");
            if (close >= 0)
            {
                _position = close + 3;
                return new Token(TokenKind.Constructor, start, _position - start);
            }
        }

        // ---- XQuery string constructors --------------------------------------
        // "``[" begins a string constructor; the whole span (literal text and
        // `{`...`}` interpolations, including nested constructors) is one token.
        if (_allowConstructors && c == '`' && _position + 2 < _source.Length
            && _source[_position + 1] == '`' && _source[_position + 2] == '[')
        {
            int end = ScanStringConstructorEnd(start + 3);
            if (end >= 0)
            {
                _position = end;
                return new Token(TokenKind.Constructor, start, end - start);
            }
            _position = start;
        }

        // ---- Literals ------------------------------------------------
        if (c == '"' || c == '\'')
            return ReadStringLiteral(start, c);

        // ---- Numbers -------------------------------------------------
        if (char.IsDigit(c))
            return ReadNumber(start);

        if (c == '.')
        {
            if (_position + 1 < _source.Length && char.IsDigit(_source[_position + 1]))
                return ReadNumber(start); // .5
            if (_position + 1 < _source.Length && _source[_position + 1] == '.')
            {
                _position += 2;
                return new Token(TokenKind.DotDot, start, 2);
            }
            _position++;
            return new Token(TokenKind.Dot, start, 1);
        }

        // ---- Names / Keywords ----------------------------------------
        // Exclude ':' here — it is handled by ReadOperator as Colon or DoubleColon.
        // ReadNameOrKeyword still handles QNames (prefix:local) internally.
        if (IsNameStartChar(c) && c != ':')
            return ReadNameOrKeyword(start);

        // ---- Multi-char operators (check longest first) --------------
        return ReadOperator(start, c);
    }

    // ------------------------------------------------------------------
    // Whitespace & Comments
    // ------------------------------------------------------------------

    private void SkipWhitespaceAndComments()
    {
        while (_position < _source.Length)
        {
            char c = _source[_position];

            if (char.IsWhiteSpace(c))
            {
                _position++;
                continue;
            }

            if (c == '(' && _position + 1 < _source.Length && _source[_position + 1] == ':')
            {
                SkipComment();
                continue;
            }

            break;
        }
    }

    /// <summary>
    /// Skips a nested XPath comment <c>(: ... :)</c>.
    /// </summary>
    private void SkipComment()
    {
        // Skip opening '(:'
        int start = _position;
        _position += 2;
        int depth = 1;

        while (_position < _source.Length && depth > 0)
        {
            char c = _source[_position];

            if (c == ':' && _position + 1 < _source.Length && _source[_position + 1] == ')')
            {
                depth--;
                _position += 2;
            }
            else if (c == '(' && _position + 1 < _source.Length && _source[_position + 1] == ':')
            {
                depth++;
                _position += 2;
            }
            else
            {
                _position++;
            }
        }

        if (depth > 0)
        {
            throw new ParseException("Unterminated comment", start);
        }
    }

    // ------------------------------------------------------------------
    // String literals
    // ------------------------------------------------------------------

    private Token ReadStringLiteral(int start, char quote)
    {
        _position++; // skip opening quote

        while (_position < _source.Length)
        {
            char c = _source[_position];

            if (c == quote)
            {
                // Check for doubled quote (escape sequence)
                if (_position + 1 < _source.Length && _source[_position + 1] == quote)
                {
                    _position += 2; // skip both quotes
                    continue;
                }

                _position++; // skip closing quote
                return new Token(TokenKind.StringLiteral, start, _position - start);
            }

            _position++;
        }

        // Unterminated string
        return new Token(TokenKind.Invalid, start, _position - start);
    }

    // ------------------------------------------------------------------
    // Numbers
    // ------------------------------------------------------------------

    private Token ReadNumber(int start)
    {
        bool hasDot = false;
        bool hasExponent = false;

        // Integer part
        while (_position < _source.Length && char.IsDigit(_source[_position]))
        {
            _position++;
        }

        // Decimal point
        if (_position < _source.Length && _source[_position] == '.')
        {
            _position++;

            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                _position++;
            }

            // A trailing dot is part of a decimal literal per XPath 3.1:
            // DecimalLiteral ::= Digits '.' Digits?
            // This makes "5.*." parse as (5.0 * .) and "5.+*" as (5.0 + *).
            hasDot = true;
        }

        // Exponent
        if (_position < _source.Length && (_source[_position] == 'e' || _source[_position] == 'E'))
        {
            int expPos = _position;
            _position++;

            if (_position < _source.Length && (_source[_position] == '+' || _source[_position] == '-'))
                _position++;

            bool hasExpDigits = false;
            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                hasExpDigits = true;
                _position++;
            }

            if (hasExpDigits)
            {
                hasExponent = true;
            }
            else
            {
                // Rollback exponent — it's not a double
                _position = expPos;
            }
        }

        int length = _position - start;

        if (hasExponent)
            return new Token(TokenKind.DoubleLiteral, start, length);

        if (hasDot)
            return new Token(TokenKind.DecimalLiteral, start, length);

        // NumericLiteral must be followed by a terminal or whitespace; a
        // name-start character immediately after the number is a lexical error
        // (e.g. "10idiv 3" must raise XPST0003, not parse as "10 idiv 3").
        if (_position < _source.Length && IsNameStartChar(_source[_position]) && _source[_position] != ':')
        {
            while (_position < _source.Length && IsNameChar(_source[_position]))
                _position++;
            return new Token(TokenKind.Invalid, start, _position - start);
        }

        return new Token(TokenKind.IntegerLiteral, start, length);
    }

    // ------------------------------------------------------------------
    // Names & Keywords
    // ------------------------------------------------------------------

    private Token ReadNameOrKeyword(int start)
    {
        // Consume name chars, but stop at ':' so we can handle QNames,
        // axis separators (::), and wildcard patterns (prefix:*, *:local).
        while (_position < _source.Length && IsNameChar(_source[_position]) && _source[_position] != ':')
            _position++;

        // Braced URI literal: Q{uri}localname
        if (_position - start == 1 && _source[start] == 'Q' &&
            _position < _source.Length && _source[_position] == '{')
        {
            _position++; // consume '{'
            while (_position < _source.Length && _source[_position] != '}')
                _position++;
            if (_position < _source.Length)
                _position++; // consume '}'

            // URI-qualified wildcard: Q{uri}*
            if (_position < _source.Length && _source[_position] == '*')
            {
                _position++; // consume '*'
                return new Token(TokenKind.Name, start, _position - start);
            }

            // Read local name or prefix:local
            if (_position < _source.Length && IsNameStartChar(_source[_position]))
            {
                while (_position < _source.Length && IsNameChar(_source[_position]))
                    _position++;

                // Handle prefix:local
                if (_position < _source.Length && _source[_position] == ':')
                {
                    int colonPos = _position;
                    _position++;
                    if (_position < _source.Length && IsNameStartChar(_source[_position]))
                    {
                        while (_position < _source.Length && IsNameChar(_source[_position]))
                            _position++;
                        return new Token(TokenKind.Name, start, _position - start);
                    }
                    _position = colonPos; // rollback
                }
            }
            return new Token(TokenKind.Name, start, _position - start);
        }

        // Check for colon (QName)
        if (_position < _source.Length && _source[_position] == ':')
        {
            int colonPos = _position;
            _position++;

            // "::" is axis separator, not part of a QName
            if (_position < _source.Length && _source[_position] == ':')
            {
                // Rollback — this is a prefix followed by ::
                _position = colonPos;
                return new Token(TokenKind.Name, start, colonPos - start);
            }

            // prefix:* is a Name, Colon, Star sequence — don't consume the star
            if (_position < _source.Length && _source[_position] == '*')
            {
                _position = colonPos;
                return new Token(TokenKind.Name, start, colonPos - start);
            }

            // prefix:localname
            if (_position < _source.Length && IsNameStartChar(_source[_position]))
            {
                while (_position < _source.Length && IsNameChar(_source[_position]))
                    _position++;

                return new Token(TokenKind.Name, start, _position - start);
            }

            // Trailing colon with no valid local name — treat as name + colon
            _position = colonPos;
            return new Token(TokenKind.Name, start, colonPos - start);
        }

        ReadOnlySpan<char> text = _source.Slice(start, _position - start);
        TokenKind kind = text.Length switch
        {
            2 => ResolveKeyword2(text),
            3 => ResolveKeyword3(text),
            4 => ResolveKeyword4(text),
            5 => ResolveKeyword5(text),
            6 => ResolveKeyword6(text),
            7 => ResolveKeyword7(text),
            8 => ResolveKeyword8(text),
            9 => ResolveKeyword9(text),
            _ => TokenKind.Name
        };

        return new Token(kind, start, _position - start);
    }

    // ------------------------------------------------------------------
    // Operators
    // ------------------------------------------------------------------

    private Token ReadOperator(int start, char c)
    {
        _position++;

        switch (c)
        {
            case '(':
                return new Token(TokenKind.LParen, start, 1);
            case ')':
                return new Token(TokenKind.RParen, start, 1);
            case '[':
                return new Token(TokenKind.LBracket, start, 1);
            case ']':
                return new Token(TokenKind.RBracket, start, 1);
            case '{':
                return new Token(TokenKind.LBrace, start, 1);
            case '}':
                return new Token(TokenKind.RBrace, start, 1);
            case ',':
                return new Token(TokenKind.Comma, start, 1);
            case '+':
                return new Token(TokenKind.Plus, start, 1);
            case '-':
                return new Token(TokenKind.Minus, start, 1);
            case '*':
                return new Token(TokenKind.Star, start, 1);
            case '$':
                return new Token(TokenKind.Dollar, start, 1);
            case '@':
                return new Token(TokenKind.At, start, 1);
            case '?':
                return new Token(TokenKind.Question, start, 1);
            case '#':
                return new Token(TokenKind.Hash, start, 1);
            case ';':
                return new Token(TokenKind.Semicolon, start, 1);

            case '/':
                if (_position < _source.Length && _source[_position] == '/')
                {
                    _position++;
                    return new Token(TokenKind.SlashSlash, start, 2);
                }
                return new Token(TokenKind.Slash, start, 1);

            case '|':
                if (_position < _source.Length && _source[_position] == '|')
                {
                    _position++;
                    return new Token(TokenKind.StringConcat, start, 2);
                }
                return new Token(TokenKind.VBar, start, 1);

            case ':':
                if (_position < _source.Length && _source[_position] == ':')
                {
                    _position++;
                    return new Token(TokenKind.DoubleColon, start, 2);
                }
                if (_position < _source.Length && _source[_position] == '=')
                {
                    _position++;
                    return new Token(TokenKind.Assign, start, 2);
                }
                return new Token(TokenKind.Colon, start, 1);

            case '=':
                if (_position < _source.Length && _source[_position] == '>')
                {
                    _position++;
                    return new Token(TokenKind.Arrow, start, 2);
                }
                return new Token(TokenKind.Equal, start, 1);

            case '!':
                if (_position < _source.Length && _source[_position] == '=')
                {
                    _position++;
                    return new Token(TokenKind.NotEqual, start, 2);
                }
                return new Token(TokenKind.Bang, start, 1);

            case '<':
                if (_position < _source.Length)
                {
                    if (_source[_position] == '=')
                    {
                        _position++;
                        return new Token(TokenKind.LessThanOrEqual, start, 2);
                    }
                    if (_source[_position] == '<')
                    {
                        _position++;
                        return new Token(TokenKind.NodeBefore, start, 2);
                    }
                }
                return new Token(TokenKind.LessThan, start, 1);

            case '>':
                if (_position < _source.Length)
                {
                    if (_source[_position] == '=')
                    {
                        _position++;
                        return new Token(TokenKind.GreaterThanOrEqual, start, 2);
                    }
                    if (_source[_position] == '>')
                    {
                        _position++;
                        return new Token(TokenKind.NodeAfter, start, 2);
                    }
                }
                return new Token(TokenKind.GreaterThan, start, 1);

            default:
                return new Token(TokenKind.Invalid, start, 1);
        }
    }

    // ------------------------------------------------------------------
    // Keyword resolution
    // ------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SeqEqual(ReadOnlySpan<char> a, string b)
        => a.Length == b.Length && a.SequenceEqual(b.AsSpan());

    private static TokenKind ResolveKeyword2(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "as")) return TokenKind.KeywordAs;
        if (SeqEqual(text, "eq")) return TokenKind.ValueEq;
        if (SeqEqual(text, "ge")) return TokenKind.ValueGe;
        if (SeqEqual(text, "gt")) return TokenKind.ValueGt;
        if (SeqEqual(text, "if")) return TokenKind.KeywordIf;
        if (SeqEqual(text, "in")) return TokenKind.KeywordIn;
        if (SeqEqual(text, "is")) return TokenKind.ValueIs;
        if (SeqEqual(text, "le")) return TokenKind.ValueLe;
        if (SeqEqual(text, "lt")) return TokenKind.ValueLt;
        if (SeqEqual(text, "ne")) return TokenKind.ValueNe;
        if (SeqEqual(text, "of")) return TokenKind.KeywordOf;
        if (SeqEqual(text, "or")) return TokenKind.KeywordOr;
        if (SeqEqual(text, "to")) return TokenKind.KeywordTo;
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword3(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "and")) return TokenKind.KeywordAnd;
        if (SeqEqual(text, "div")) return TokenKind.KeywordDiv;
        if (SeqEqual(text, "for")) return TokenKind.KeywordFor;
        if (SeqEqual(text, "let")) return TokenKind.KeywordLet;
        if (SeqEqual(text, "map")) return TokenKind.KeywordMap;
        if (SeqEqual(text, "mod")) return TokenKind.KeywordMod;
        if (SeqEqual(text, "not")) return TokenKind.Name; // not is a function, not a keyword
        if (SeqEqual(text, "try")) return TokenKind.KeywordTry;
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword4(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "cast")) return TokenKind.KeywordCast;
        if (SeqEqual(text, "else")) return TokenKind.KeywordElse;
        if (SeqEqual(text, "idiv")) return TokenKind.KeywordIdiv;
        if (SeqEqual(text, "then")) return TokenKind.KeywordThen;
        if (SeqEqual(text, "some")) return TokenKind.KeywordSome;
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword5(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "array")) return TokenKind.KeywordArray;
        if (SeqEqual(text, "catch")) return TokenKind.KeywordCatch;
        if (SeqEqual(text, "every")) return TokenKind.KeywordEvery;
        if (SeqEqual(text, "idiv")) return TokenKind.KeywordIdiv;
        if (SeqEqual(text, "treat")) return TokenKind.KeywordTreat;
        if (SeqEqual(text, "union")) return TokenKind.KeywordUnion;
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword6(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "except")) return TokenKind.KeywordExcept;
        if (SeqEqual(text, "return")) return TokenKind.KeywordReturn;
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword7(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "castable")) return TokenKind.KeywordCastable;
        if (SeqEqual(text, "default")) return TokenKind.Name;
        if (SeqEqual(text, "element")) return TokenKind.Name; // kind test, parsed as name
        if (SeqEqual(text, "foreach")) return TokenKind.Name; // function name
        if (SeqEqual(text, "item")) return TokenKind.Name;
        if (SeqEqual(text, "satisfies")) return TokenKind.KeywordSatisfies;
        if (SeqEqual(text, "switch")) return TokenKind.Name; // XQuery
        if (SeqEqual(text, "version")) return TokenKind.Name; // XQuery
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword8(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "ancestor")) return TokenKind.Name;
        if (SeqEqual(text, "attribute")) return TokenKind.Name;
        if (SeqEqual(text, "castable")) return TokenKind.KeywordCastable;
        if (SeqEqual(text, "children")) return TokenKind.Name;
        if (SeqEqual(text, "function")) return TokenKind.KeywordFunction;
        if (SeqEqual(text, "instance")) return TokenKind.KeywordInstance;
        if (SeqEqual(text, "variable")) return TokenKind.Name; // XQuery
        return TokenKind.Name;
    }

    private static TokenKind ResolveKeyword9(ReadOnlySpan<char> text)
    {
        if (SeqEqual(text, "descendant")) return TokenKind.Name;
        if (SeqEqual(text, "following")) return TokenKind.Name;
        if (SeqEqual(text, "intersect")) return TokenKind.KeywordIntersect;
        if (SeqEqual(text, "namespace")) return TokenKind.Name;
        if (SeqEqual(text, "preceding")) return TokenKind.Name;
        if (SeqEqual(text, "satisfies")) return TokenKind.KeywordSatisfies;
        if (SeqEqual(text, "transform")) return TokenKind.Name;
        if (SeqEqual(text, "typeswitch")) return TokenKind.Name; // XQuery
        return TokenKind.Name;
    }

    // ------------------------------------------------------------------
    // XQuery direct element constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Attempts to scan a whole direct element constructor starting at '<'. Returns an
    /// Invalid token when the structure does not hold (the caller then re-lexes '<' as
    /// the comparison operator). Structure validation is shallow: name matching and
    /// expression parsing are left to the parser.
    /// </summary>
    private Token TryScanConstructor(int start)
    {
        int pos = start + 1;
        if (!ScanNcName(ref pos))
            return default;

        // Attributes / open tag.
        while (true)
        {
            ScanWhitespace(ref pos);
            if (pos >= _source.Length)
                return default;
            char c = _source[pos];
            if (c == '>')
            {
                pos++;
                break;
            }
            if (c == '/' && pos + 1 < _source.Length && _source[pos + 1] == '>')
                return new Token(TokenKind.Constructor, start, pos + 2 - start);
            if (!ScanNcName(ref pos))
                return default;
            ScanWhitespace(ref pos);
            if (pos >= _source.Length || _source[pos] != '=')
                return default;
            pos++;
            ScanWhitespace(ref pos);
            if (pos >= _source.Length || (_source[pos] != '"' && _source[pos] != '\''))
                return default;
            char quote = _source[pos++];
            if (!ScanConstructorAttributeValue(ref pos, quote))
                return default;
        }

        // Content until the matching end tag (depth tracks nested open elements).
        int depth = 1;
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == '<')
            {
                if (MatchesAt(pos, "<!--"))
                {
                    int close = IndexOf(pos + 4, "-->");
                    if (close < 0) return default;
                    pos = close + 3;
                    continue;
                }
                if (MatchesAt(pos, "<![CDATA["))
                {
                    int close = IndexOf(pos + 9, "]]>");
                    if (close < 0) return default;
                    pos = close + 3;
                    continue;
                }
                if (pos + 1 < _source.Length && _source[pos + 1] == '?')
                {
                    int close = IndexOf(pos + 2, "?>");
                    if (close < 0) return default;
                    pos = close + 2;
                    continue;
                }
                if (pos + 1 < _source.Length && _source[pos + 1] == '/')
                {
                    pos += 2;
                    ScanWhitespace(ref pos);
                    if (!ScanNcName(ref pos))
                        return default;
                    ScanWhitespace(ref pos);
                    if (pos >= _source.Length || _source[pos] != '>')
                        return default;
                    pos++;
                    depth--;
                    if (depth == 0)
                        return new Token(TokenKind.Constructor, start, pos - start);
                    continue;
                }
                if (pos + 1 < _source.Length && IsNameStartChar(_source[pos + 1]))
                {
                    pos++;
                    if (!ScanNcName(ref pos))
                        return default;
                    bool selfClosed = false;
                    while (true)
                    {
                        ScanWhitespace(ref pos);
                        if (pos >= _source.Length)
                            return default;
                        char c2 = _source[pos];
                        if (c2 == '>')
                        {
                            pos++;
                            break;
                        }
                        if (c2 == '/' && pos + 1 < _source.Length && _source[pos + 1] == '>')
                        {
                            pos += 2;
                            selfClosed = true;
                            break;
                        }
                        if (!ScanNcName(ref pos))
                            return default;
                        ScanWhitespace(ref pos);
                        if (pos >= _source.Length || _source[pos] != '=')
                            return default;
                        pos++;
                        ScanWhitespace(ref pos);
                        if (pos >= _source.Length || (_source[pos] != '"' && _source[pos] != '\''))
                            return default;
                        char quote2 = _source[pos++];
                        if (!ScanConstructorAttributeValue(ref pos, quote2))
                            return default;
                    }
                    if (!selfClosed)
                        depth++;
                    continue;
                }
                return default;
            }
            if (c == '{')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '{')
                {
                    pos += 2;
                    continue;
                }
                if (!ScanBalancedBraces(ref pos))
                    return default;
                continue;
            }
            if (c == '}' && pos + 1 < _source.Length && _source[pos + 1] == '}')
            {
                pos += 2;
                continue;
            }
            pos++;
        }
        return default;
    }

    private bool ScanNcName(ref int pos)
    {
        if (pos >= _source.Length || !IsNameStartChar(_source[pos]) || _source[pos] == ':')
            return false;
        pos++;
        while (pos < _source.Length && IsNameChar(_source[pos]) && _source[pos] != ':')
            pos++;
        // Optional prefix:local part.
        if (pos < _source.Length && _source[pos] == ':')
        {
            pos++;
            if (pos >= _source.Length || !IsNameStartChar(_source[pos]) || _source[pos] == ':')
                return false;
            pos++;
            while (pos < _source.Length && IsNameChar(_source[pos]))
                pos++;
        }
        return true;
    }

    private void ScanWhitespace(ref int pos)
    {
        while (pos < _source.Length && char.IsWhiteSpace(_source[pos]))
            pos++;
    }

    private bool MatchesAt(int pos, string text)
        => _source.Length - pos >= text.Length && _source.Slice(pos, text.Length).SequenceEqual(text.AsSpan());

    private int IndexOf(int from, string text)
    {
        int idx = _source.Slice(from).IndexOf(text.AsSpan(), StringComparison.Ordinal);
        return idx < 0 ? -1 : from + idx;
    }

    private bool ScanConstructorAttributeValue(ref int pos, char quote)
    {
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == quote)
            {
                // A doubled quote is an escaped literal quote, not the delimiter.
                if (pos + 1 < _source.Length && _source[pos + 1] == quote)
                {
                    pos += 2;
                    continue;
                }
                pos++;
                return true;
            }
            if (c == '{')
            {
                if (pos + 1 < _source.Length && _source[pos + 1] == '{')
                {
                    pos += 2;
                    continue;
                }
                if (!ScanBalancedBraces(ref pos))
                    return false;
                continue;
            }
            if (c == '}' && pos + 1 < _source.Length && _source[pos + 1] == '}')
            {
                pos += 2;
                continue;
            }
            pos++;
        }
        return false;
    }

    private bool ScanBalancedBraces(ref int pos)
    {
        int depth = 1;
        pos++;
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
                    pos++;
                    return true;
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
        return false;
    }

    // ------------------------------------------------------------------
    // Character classification
    // ------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNameStartChar(char c)
    {
        // XML 1.0 NameStartChar simplified for ASCII + common Unicode
        return c == ':'
            || c == '_'
            || (c >= 'A' && c <= 'Z')
            || (c >= 'a' && c <= 'z')
            || char.IsLetter(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNameChar(char c)
    {
        return IsNameStartChar(c)
            || c == '-'
            || c == '.'
            || (c >= '0' && c <= '9')
            || c == '\u00B7'
            || (c >= '\u0300' && c <= '\u036F')
            || (c >= '\u203F' && c <= '\u2040');
    }

    // ------------------------------------------------------------------
    // XQuery string constructors
    // ------------------------------------------------------------------

    // Scans the body of a string constructor starting just past the opening "``[".
    // Returns the position just past the closing "]``" or -1 when unterminated.
    // Interpolations are skipped with full expression awareness: string literals,
    // nested comments, brace depth, and nested string constructors.
    private int ScanStringConstructorEnd(int pos)
    {
        while (pos < _source.Length)
        {
            char c = _source[pos];
            if (c == ']' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '`')
                return pos + 3;
            if (c == '`' && pos + 1 < _source.Length && _source[pos + 1] == '{')
            {
                pos = ScanStringInterpolationEnd(pos + 2);
                if (pos < 0)
                    return -1;
                continue;
            }
            pos++;
        }
        return -1;
    }

    // Scans one string-constructor interpolation body starting just past "`{".
    // Returns the position just past the closing "}`" or -1 when unterminated.
    private int ScanStringInterpolationEnd(int pos)
    {
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
                int commentDepth = 1;
                pos += 2;
                while (pos < _source.Length && commentDepth > 0)
                {
                    if (_source[pos] == '(' && pos + 1 < _source.Length && _source[pos + 1] == ':') { commentDepth++; pos += 2; }
                    else if (_source[pos] == ':' && pos + 1 < _source.Length && _source[pos + 1] == ')') { commentDepth--; pos += 2; }
                    else pos++;
                }
                continue;
            }
            if (c == '`' && pos + 2 < _source.Length && _source[pos + 1] == '`' && _source[pos + 2] == '[')
            {
                // A nested string constructor inside the interpolation expression.
                pos = ScanStringConstructorEnd(pos + 3);
                if (pos < 0)
                    return -1;
                continue;
            }
            if (c == '{')
            {
                depth++;
                pos++;
                continue;
            }
            if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    // The interpolation must close with '}`'.
                    if (pos + 1 < _source.Length && _source[pos + 1] == '`')
                        return pos + 2;
                    return -1;
                }
                pos++;
                continue;
            }
            pos++;
        }
        return -1;
    }
}
