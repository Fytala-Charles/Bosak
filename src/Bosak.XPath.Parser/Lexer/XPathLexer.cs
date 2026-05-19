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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.CompilerServices;

namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// A zero-allocation XPath 3.1 lexer operating over <see cref="ReadOnlySpan{char}"/>.
/// </summary>
public ref struct XPathLexer
{
    private readonly ReadOnlySpan<char> _source;
    private int _position;

    public XPathLexer(ReadOnlySpan<char> source)
    {
        _source = source;
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
            // Unterminated comment — let the parser report the error.
            // The lexer simply consumed to EOF.
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
        bool hasDigitsAfterDot = false;
        bool hasExponent = false;

        // Integer part
        while (_position < _source.Length && char.IsDigit(_source[_position]))
        {
            _position++;
        }

        // Decimal point
        if (_position < _source.Length && _source[_position] == '.')
        {
            int dotPos = _position;
            _position++;

            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                hasDigitsAfterDot = true;
                _position++;
            }

            // If no digits after the dot, rollback so the dot is tokenized separately.
            // "123." → IntegerLiteral(123) + Dot
            if (!hasDigitsAfterDot)
            {
                _position = dotPos;
            }
            else
            {
                hasDot = true;
            }
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
}
