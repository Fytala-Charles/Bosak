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
//                      | Charles Korthout | 0.1   | 22-07-2026     | Creation — prolog-less delegation to XPathParser                                        |
//                      | Charles Korthout | 0.2   | 22-07-2026     | Parse XQuery body with allowFullFlwor=true to enable full FLWOR                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 25-07-2026     | Parse 'declare base-uri' prolog declarations                                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 25-07-2026     | Version/encoding validation; XPST0003 syntax codes; char refs; xquery-name backtrack    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 25-07-2026     | Parse declare option (output declarations) with QName/EQName option names               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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
    /// <param name="xml11LineEndings">When true, string literals get XML 1.1 line-ending normalization.</param>
    /// <returns>A parse result containing the static context and the query body AST.</returns>
    public static XQueryParseResult Parse(string source, bool xml11LineEndings = false)
    {
        var parser = new XQueryParser(source) { _xml11LineEndings = xml11LineEndings };
        return parser.ParseModule();
    }

    private XQueryParseResult ParseModule()
    {
        var context = new XQueryStaticContext();

        SkipWhitespace();

        // Optional version declaration: "xquery version '...';" or "xquery version '...' encoding '...';".
        // A leading name 'xquery' that is not followed by 'version' is a path expression,
        // not a version declaration (e.g. 'xquery gt xquery').
        int beforeVersionDecl = _position;
        bool versionDeclParsed = false;
        if (TryMatchLiteral("xquery") && TryMatchLiteral("version"))
        {
            SkipWhitespace();
            var versionLiteral = ReadStringLiteral();
            SkipWhitespace();

            // XQST0031: only XQuery versions supported by this implementation are accepted.
            if (versionLiteral is not ("1.0" or "3.0" or "3.1"))
                throw new ParseException($"XQST0031: XQuery version '{versionLiteral}' is not supported.", _position);

            if (TryMatchLiteral("encoding"))
            {
                SkipWhitespace();
                var encoding = ReadStringLiteral();
                // XQST0087: the encoding name must match the XML EncName production.
                if (!IsValidEncodingName(encoding))
                    throw new ParseException($"XQST0087: Encoding '{encoding}' is not supported.", _position);
                SkipWhitespace();
            }

            ExpectChar(';');
            SkipWhitespace();
            versionDeclParsed = true;
        }
        if (!versionDeclParsed)
        {
            _position = beforeVersionDecl;
        }

        // Prolog parsing is intentionally minimal in this first iteration.
        // We only consume namespace declarations that are needed for the
        // XPath parser's static context.
        while (TryParsePrologDeclaration(ref context))
        {
            SkipWhitespace();
        }

        // Deferred option-prefix resolution: a prefix undeclared after the whole
        // prolog is XPST0081 (an option that precedes namespace declarations was
        // already rejected with XPST0003 by the ordering check).
        foreach (var (prefix, local, value, position) in _pendingOptions)
        {
            if (!context.Namespaces.TryGetValue(prefix, out var deferredNs))
                throw new ParseException($"XPST0081: Prefix '{prefix}' is not declared.", position);
            ValidateOutputOption(context, local, deferredNs, position);
            context = context.WithOption(local, deferredNs, value);
        }

        // The rest of the source is the query body (an Expr).
        int bodyStart = _position;
        var remaining = _source[bodyStart..];
        if (string.IsNullOrWhiteSpace(remaining))
            throw new ParseException("XPST0003: Query body is missing.", _position);

        var bodyAst = XPathParser.Parse(remaining, allowFullFlwor: true, xml11LineEndings: _xml11LineEndings);
        return new XQueryParseResult(context, bodyAst);
    }

    private bool TryParsePrologDeclaration(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();

        if (TryMatchLiteral("declare namespace"))
        {
            // XQuery prolog ordering: namespace declarations precede option declarations.
            if (_seenOptionDecl)
                throw new ParseException("XPST0003: Namespace declarations must precede option declarations.", _position);
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
            if (_seenOptionDecl)
                throw new ParseException("XPST0003: Namespace declarations must precede option declarations.", _position);
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            // XQST0066: the default element namespace must not be declared twice.
            if (context.DefaultElementNamespace is not null)
                throw new ParseException("XQST0066: More than one default element namespace declaration.", _position);
            context = context.WithDefaultElementNamespace(uri);
            return true;
        }

        if (TryMatchLiteral("declare default function namespace"))
        {
            if (_seenOptionDecl)
                throw new ParseException("XPST0003: Namespace declarations must precede option declarations.", _position);
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            // XQST0066: the default function namespace must not be declared twice.
            if (context.DefaultFunctionNamespace is not null
                && context.DefaultFunctionNamespace != "http://www.w3.org/2005/xpath-functions")
                throw new ParseException("XQST0066: More than one default function namespace declaration.", _position);
            context = context.WithDefaultFunctionNamespace(uri);
            return true;
        }

        if (TryMatchLiteral("declare default collation"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            // XQST0038: duplicate default collation declaration.
            if (context.DefaultCollation is not null)
                throw new ParseException("XQST0038: More than one default collation declaration.", _position);
            // XQST0087: the collation must be known to the implementation.
            if (!IsSupportedCollation(ResolveCollationUri(uri, context.BaseUri)))
                throw new ParseException($"XQST0087: Collation '{uri}' is not supported.", _position);
            context = context.WithDefaultCollation(uri);
            return true;
        }

        if (TryMatchLiteral("declare base-uri"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            context = context.WithBaseUri(uri);
            return true;
        }

        if (TryMatchLiteral("declare option"))
        {
            SkipWhitespace();
            var (prefix, local, eqNameUri) = ReadQName();
            SkipWhitespace();
            string value = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            _seenOptionDecl = true;
            if (eqNameUri is null && prefix is not null && !context.Namespaces.ContainsKey(prefix))
            {
                // The prefix may be declared by a later namespace declaration — but that
                // is an ordering violation (XPST0003); otherwise it is undeclared (XPST0081).
                _pendingOptions.Add((prefix, local, value, _position));
                return true;
            }
            string optionNs = eqNameUri ?? (prefix is null ? string.Empty : context.Namespaces[prefix]);
            ValidateOutputOption(context, local, optionNs, _position);
            context = context.WithOption(local, optionNs, value);
            return true;
        }

        // Not a recognized prolog declaration; stop consuming prolog items.
        _position = savedPosition;
        return false;
    }

    private bool _seenOptionDecl;
    private bool _xml11LineEndings;
    private readonly List<(string Prefix, string Local, string Value, int Position)> _pendingOptions = new();

    // XQST0109/XQST0110 validation for output declarations in the serialization namespace.
    private void ValidateOutputOption(XQueryStaticContext context, string local, string optionNs, int position)
    {
        if (optionNs != "http://www.w3.org/2010/xslt-xquery-serialization")
            return;
        if (!SerializationParameterNames.Contains(local))
            throw new ParseException($"XQST0109: Unknown serialization parameter '{local}'.", position);
        if (context.Options.Any(o => o.NamespaceUri == optionNs && o.LocalName == local))
            throw new ParseException($"XQST0110: Duplicate output declaration for serialization parameter '{local}'.", position);
    }

    // ------------------------------------------------------------------
    // Lexical helpers
    // ------------------------------------------------------------------

    private static bool IsValidEncodingName(string encoding)
    {
        // XML EncName: [A-Za-z] ([A-Za-z0-9._] | '-')*
        if (encoding.Length == 0 || !char.IsAsciiLetter(encoding[0]))
            return false;
        foreach (char c in encoding)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c is not ('.' or '_' or '-'))
                return false;
        }
        return true;
    }

    private static bool IsSupportedCollation(string collation)
    {
        if (collation == "http://www.w3.org/2005/xpath-functions/collation/codepoint")
            return true;
        if (collation == "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive")
            return true;
        if (collation == "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind")
            return true;
        if (collation.StartsWith("http://www.w3.org/2013/collation/UCA", StringComparison.Ordinal))
            return true;
        return false;
    }

    private static string ResolveCollationUri(string collation, string? baseUri)
    {
        if (string.IsNullOrEmpty(collation))
            return string.Empty;
        if (Uri.IsWellFormedUriString(collation, UriKind.Absolute))
            return collation;
        if (!string.IsNullOrEmpty(baseUri) &&
            Uri.TryCreate(new Uri(baseUri), collation, out var resolved))
        {
            return resolved.AbsoluteUri;
        }
        return collation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipWhitespace()
    {
        while (_position < _source.Length)
        {
            if (char.IsWhiteSpace(_source[_position]))
            {
                _position++;
                continue;
            }
            // XQuery comments (: ... :) nest and are whitespace to the prolog parser.
            if (_position + 1 < _source.Length && _source[_position] == '(' && _source[_position + 1] == ':')
            {
                int depth = 1;
                _position += 2;
                while (_position < _source.Length && depth > 0)
                {
                    if (_position + 1 < _source.Length && _source[_position] == '(' && _source[_position + 1] == ':')
                    {
                        depth++;
                        _position += 2;
                    }
                    else if (_position + 1 < _source.Length && _source[_position] == ':' && _source[_position + 1] == ')')
                    {
                        depth--;
                        _position += 2;
                    }
                    else
                    {
                        _position++;
                    }
                }
                continue;
            }
            break;
        }
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
            throw new ParseException($"XPST0003: Expected '{literal}'.", _position);
        _position += literal.Length;
    }

    private void ExpectChar(char c)
    {
        SkipWhitespace();
        if (_position >= _source.Length || _source[_position] != c)
            throw new ParseException($"XPST0003: Expected '{c}'.", _position);
        _position++;
    }

    private string ReadStringLiteral()
    {
        SkipWhitespace();
        if (_position >= _source.Length)
            throw new ParseException("XPST0003: Expected string literal.", _position);

        char quote = _source[_position];
        if (quote != '"' && quote != '\'')
            throw new ParseException("XPST0003: Expected string literal.", _position);

        _position++;
        var value = new StringBuilder();
        while (_position < _source.Length)
        {
            char c = _source[_position];
            if (c == quote)
            {
                // Check for doubled quote escape
                if (_position + 1 < _source.Length && _source[_position + 1] == quote)
                {
                    value.Append(quote);
                    _position += 2;
                    continue;
                }
                _position++;
                return value.ToString();
            }
            // XQuery string literals support predefined entity and character references;
            // a raw '&' that forms no valid reference is XPST0003.
            if (c == '&')
            {
                value.Append(ExpandCharReference());
                continue;
            }
            value.Append(c);
            _position++;
        }
        throw new ParseException("XPST0003: Unterminated string literal.", _position);
    }

    private string ExpandCharReference()
    {
        // _position is at the '&'.
        int start = _position;
        int semi = _source.IndexOf(';', _position + 1);
        if (semi < 0)
            throw new ParseException("XPST0003: Unterminated entity or character reference in string literal.", start);
        var reference = _source[(_position + 1)..semi];
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
            _ => throw new ParseException($"XPST0003: Invalid entity or character reference '&{reference};' in string literal.", start)
        };
        _position = semi + 1;
        return result;
    }

    private string ReadNCName()
    {
        SkipWhitespace();
        if (_position >= _source.Length || !IsNameStartChar(_source[_position]))
            throw new ParseException("XPST0003: Expected NCName.", _position);

        int start = _position;
        _position++;
        while (_position < _source.Length && IsNameChar(_source[_position]))
            _position++;

        return _source[start.._position];
    }

    /// <summary>
    /// Known serialization parameter names for output declarations (XQST0109 validation).
    /// </summary>
    private static readonly HashSet<string> SerializationParameterNames = new(StringComparer.Ordinal)
    {
        "method", "version", "encoding", "indent", "omit-xml-declaration", "standalone",
        "item-separator", "media-type", "doctype-system", "doctype-public", "normalization-form",
        "json-node-output-method", "html-version", "allow-duplicate-names", "byte-order-mark",
        "escape-uri-attributes", "include-content-type", "undeclare-prefixes",
        "cdata-section-elements", "suppress-indentation", "parameter-document"
    };

    // Reads a lexical QName (NCName, prefix:NCName, or Q{uri}NCName) without resolving prefixes.
    private (string? Prefix, string Local, string? NamespaceUri) ReadQName()
    {
        string first = ReadNCName();
        if (first == "Q" && _position < _source.Length && _source[_position] == '{')
        {
            int close = _source.IndexOf('}', _position);
            if (close < 0)
                throw new ParseException("XPST0003: Unterminated braced URI literal in EQName.", _position);
            var uri = _source[(_position + 1)..close];
            _position = close + 1;
            return (null, ReadNCName(), uri);
        }
        if (_position < _source.Length && _source[_position] == ':')
        {
            _position++;
            return (first, ReadNCName(), null);
        }
        return (null, first, null);
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
