// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 July 2026
// PURPOSE              : Parses an XQuery 3.1 module into a static context and an executable query body.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-07-2026     | Creation — prolog-less delegation to XPathParser                                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Runtime.CompilerServices;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XQuery.Compiler;

/// <summary>
/// Parses XQuery 3.1 source text into a static context and an XPath-compatible AST body.
/// The parser owns the XQuery top-level grammar (version declaration, prolog, and query body)
/// and delegates expression parsing to the proven XPath parser.
/// </summary>
public sealed class XQueryParser
{
    private readonly string _source;
    private int _position;

    private XQueryParser(string source)
    {
        _source = source;
        _position = 0;
    }

    /// <summary>
    /// Parses the supplied XQuery source text.
    /// </summary>
    /// <param name="source">The XQuery source text.</param>
    /// <returns>A parse result containing the static context and the query body AST.</returns>
    public static XQueryParseResult Parse(string source)
    {
        var parser = new XQueryParser(source);
        return parser.ParseModule();
    }

    private XQueryParseResult ParseModule()
    {
        var context = new XQueryStaticContext();

        SkipWhitespace();

        // Optional version declaration: "xquery version '...';" or "xquery version '...' encoding '...';"
        if (TryMatchLiteral("xquery"))
        {
            SkipWhitespace();
            ExpectLiteral("version");
            SkipWhitespace();
            var versionLiteral = ReadStringLiteral();
            SkipWhitespace();

            if (TryMatchLiteral("encoding"))
            {
                SkipWhitespace();
                ReadStringLiteral(); // encoding is currently ignored
                SkipWhitespace();
            }

            ExpectChar(';');
            SkipWhitespace();
        }

        // Prolog parsing is intentionally minimal in this first iteration.
        // We only consume namespace declarations that are needed for the
        // XPath parser's static context.
        while (TryParsePrologDeclaration(ref context))
        {
            SkipWhitespace();
        }

        // The rest of the source is the query body (an Expr).
        int bodyStart = _position;
        var remaining = _source[bodyStart..];
        if (string.IsNullOrWhiteSpace(remaining))
            throw new ParseException("XQST0003: Query body is missing.", _position);

        var bodyAst = XPathParser.Parse(remaining);
        return new XQueryParseResult(context, bodyAst);
    }

    private bool TryParsePrologDeclaration(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();

        if (TryMatchLiteral("declare namespace"))
        {
            SkipWhitespace();
            string prefix = ReadNCName();
            SkipWhitespace();
            ExpectLiteral("=");
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            context = context.WithNamespace(prefix, uri);
            return true;
        }

        if (TryMatchLiteral("declare default element namespace"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            context = context.WithDefaultElementNamespace(uri);
            return true;
        }

        if (TryMatchLiteral("declare default function namespace"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            context = context.WithDefaultFunctionNamespace(uri);
            return true;
        }

        if (TryMatchLiteral("declare default collation"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            context = context.WithDefaultCollation(uri);
            return true;
        }

        // Not a recognized prolog declaration; stop consuming prolog items.
        _position = savedPosition;
        return false;
    }

    // ------------------------------------------------------------------
    // Lexical helpers
    // ------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipWhitespace()
    {
        while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
            _position++;
    }

    private bool TryMatchLiteral(string literal)
    {
        int savedPosition = _position;
        SkipWhitespace();
        if (_source.AsSpan(_position).StartsWith(literal.AsSpan(), StringComparison.Ordinal))
        {
            int end = _position + literal.Length;
            // Ensure the match is a whole keyword/phrase boundary.
            if (end < _source.Length && IsNameChar(_source[end]))
            {
                _position = savedPosition;
                return false;
            }
            _position = end;
            return true;
        }
        _position = savedPosition;
        return false;
    }

    private void ExpectLiteral(string literal)
    {
        SkipWhitespace();
        if (!_source.AsSpan(_position).StartsWith(literal.AsSpan(), StringComparison.Ordinal))
            throw new ParseException($"XQST0003: Expected '{literal}'.", _position);
        _position += literal.Length;
    }

    private void ExpectChar(char c)
    {
        SkipWhitespace();
        if (_position >= _source.Length || _source[_position] != c)
            throw new ParseException($"XQST0003: Expected '{c}'.", _position);
        _position++;
    }

    private string ReadStringLiteral()
    {
        SkipWhitespace();
        if (_position >= _source.Length)
            throw new ParseException("XQST0003: Expected string literal.", _position);

        char quote = _source[_position];
        if (quote != '"' && quote != '\'')
            throw new ParseException("XQST0003: Expected string literal.", _position);

        _position++;
        int start = _position;
        while (_position < _source.Length)
        {
            char c = _source[_position];
            if (c == quote)
            {
                // Check for doubled quote escape
                if (_position + 1 < _source.Length && _source[_position + 1] == quote)
                {
                    _position += 2;
                    continue;
                }
                string value = _source[start.._position];
                _position++;
                return value;
            }
            _position++;
        }
        throw new ParseException("XQST0003: Unterminated string literal.", start - 1);
    }

    private string ReadNCName()
    {
        SkipWhitespace();
        if (_position >= _source.Length || !IsNameStartChar(_source[_position]))
            throw new ParseException("XQST0003: Expected NCName.", _position);

        int start = _position;
        _position++;
        while (_position < _source.Length && IsNameChar(_source[_position]))
            _position++;

        return _source[start.._position];
    }

    private static bool IsNameStartChar(char c)
    {
        // Simplified NCName start char check (XPath 1.0-compatible subset plus common Unicode).
        return char.IsLetter(c) || c == '_';
    }

    private static bool IsNameChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-';
    }
}

/// <summary>
/// The result of parsing an XQuery module.
/// </summary>
public sealed class XQueryParseResult
{
    /// <summary>
    /// The static context derived from the prolog.
    /// </summary>
    public XQueryStaticContext StaticContext { get; }

    /// <summary>
    /// The query body expression AST.
    /// </summary>
    public XPathAstNode Body { get; }

    internal XQueryParseResult(XQueryStaticContext staticContext, XPathAstNode body)
    {
        StaticContext = staticContext;
        Body = body;
    }
}
