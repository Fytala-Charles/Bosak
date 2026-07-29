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
//                      | Charles Korthout | 0.6   | 26-07-2026     | declare function / declare variable prolog parsing with XQST0034/0039/0045/0049 and XPST0003 validations |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 27-07-2026     | Library module declaration, import module, %public/%private annotations (XQST0047/0048/0070/0088/0106/0116) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 27-07-2026     | declare ordering (XQST0065) and declare default order empty (XQST0069) |
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

        // Optional library module declaration: 'module namespace prefix = "uri";'.
        // Only a library module carries one; XQueryCompiler rejects it for a main module.
        int beforeModuleDecl = _position;
        if (TryMatchLiteral("module") && TryMatchLiteral("namespace"))
        {
            SkipWhitespace();
            string modulePrefix = ReadNCName();
            SkipWhitespace();
            ExpectLiteral("=");
            SkipWhitespace();
            string moduleNs = NormalizeModuleUri(ReadStringLiteral());
            SkipWhitespace();
            ExpectChar(';');
            // XQST0088: the target namespace of a library module must not be empty.
            if (moduleNs.Length == 0)
                throw new ParseException("XQST0088: The target namespace of a library module must not be a zero-length string.", _position);
            // The module declaration also binds its prefix in the module's own static context.
            context = context.WithModuleNamespace(moduleNs).WithNamespace(modulePrefix, moduleNs);
            _isLibraryModule = true;
            SkipWhitespace();
        }
        else
        {
            _position = beforeModuleDecl;
        }

        // Prolog parsing is intentionally minimal in this first iteration.
        // We only consume namespace declarations that are needed for the
        // XPath parser's static context.
        while (TryParsePrologDeclaration(ref context)
               || TryParseModuleImport(ref context)
               || TryParseFunctionDeclaration(ref context)
               || TryParseVariableDeclaration(ref context))
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

        // The rest of the source is the query body (an Expr) — a library module has none.
        int bodyStart = _position;
        var remaining = _source[bodyStart..];
        if (_isLibraryModule)
        {
            // XQST0048: every function and variable declared in a library module must be
            // in the module's target namespace.
            string targetNs = context.ModuleNamespaceUri!;
            foreach (var fn in context.UserFunctions)
            {
                if (fn.NamespaceUri != targetNs)
                    throw new ParseException($"XQST0048: Function '{fn.LocalName}' is not in the library module's target namespace '{targetNs}'.", fn.Position);
            }
            foreach (var v in context.UserVariables)
            {
                if (v.NamespaceUri != targetNs)
                    throw new ParseException($"XQST0048: Variable '${v.LocalName}' is not in the library module's target namespace '{targetNs}'.", v.Position);
            }
            // XQST0108: an output declaration must not appear in a library module.
            foreach (var (local, optionNs, _) in context.Options)
            {
                if (optionNs == "http://www.w3.org/2010/xslt-xquery-serialization")
                    throw new ParseException($"XQST0108: An output declaration ('{local}') must not appear in a library module.", _position);
            }
            // XPST0003: a library module must not contain a query body expression.
            if (!string.IsNullOrWhiteSpace(StripComments(remaining)))
                throw new ParseException("XPST0003: A library module must not contain a query body expression.", _position);
            return new XQueryParseResult(context, new SequenceExpressionNode(Array.Empty<XPathAstNode>()), isLibraryModule: true);
        }
        if (string.IsNullOrWhiteSpace(remaining))
            throw new ParseException("XPST0003: Query body is missing.", _position);

        var bodyAst = XPathParser.Parse(remaining, allowFullFlwor: true, xml11LineEndings: _xml11LineEndings);
        return new XQueryParseResult(context, bodyAst);
    }

    private bool TryParseModuleImport(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();
        if (!TryMatchLiteral("import") || !TryMatchLiteral("module"))
        {
            // 'import schema' and bare 'import' used as a name are not module imports.
            _position = savedPosition;
            return false;
        }

        SkipWhitespace();
        string? importPrefix = null;
        if (TryMatchLiteral("namespace"))
        {
            SkipWhitespace();
            importPrefix = ReadNCName();
            SkipWhitespace();
            ExpectLiteral("=");
            SkipWhitespace();
        }
        string importNs = NormalizeModuleUri(ReadStringLiteral());
        var locationHints = new List<string>();
        SkipWhitespace();
        if (TryMatchLiteral("at"))
        {
            do
            {
                SkipWhitespace();
                locationHints.Add(NormalizeModuleUri(ReadStringLiteral()));
                SkipWhitespace();
            } while (TryMatchChar(','));
        }
        ExpectChar(';');

        // XQST0088: the target namespace of a module import must not be empty.
        if (importNs.Length == 0)
            throw new ParseException("XQST0088: The target namespace of a module import must not be a zero-length string.", _position);
        // XQST0070: the prefixes xml and xmlns must not be (re)bound by a module import.
        if (importPrefix == "xmlns"
            || (importPrefix == "xml" && importNs != "http://www.w3.org/XML/1998/namespace"))
        {
            throw new ParseException($"XQST0070: The prefix '{importPrefix}' must not be bound by a module import.", _position);
        }
        // XQST0047: one module must not import the same target namespace twice.
        if (context.ImportedModules.Any(m => m.NamespaceUri == importNs))
            throw new ParseException($"XQST0047: The module namespace '{importNs}' is imported more than once.", _position);

        if (importPrefix is not null)
            context = context.WithNamespace(importPrefix, importNs);
        context = context.WithImportedModule(new ModuleImport(importPrefix, importNs, locationHints, _position));
        return true;
    }

    private bool TryParsePrologDeclaration(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();

        if (!TryMatchLiteral("declare"))
        {
            _position = savedPosition;
            return false;
        }

        if (TryMatchPhrase("namespace"))
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

        if (TryMatchPhrase("default", "element", "namespace"))
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

        if (TryMatchPhrase("default", "function", "namespace"))
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

        if (TryMatchPhrase("default", "collation"))
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

        if (TryMatchPhrase("default", "order", "empty"))
        {
            SkipWhitespace();
            var emptyMode = ReadNCName();
            if (emptyMode is not ("least" or "greatest"))
                throw new ParseException($"XPST0003: Expected 'least' or 'greatest' after 'declare default order empty' but found '{emptyMode}'.", _position);
            SkipWhitespace();
            ExpectChar(';');
            // XQST0069: the default order for empty sequences must not be declared twice.
            if (context.DefaultEmptyOrderLeast is not null)
                throw new ParseException("XQST0069: More than one 'declare default order empty' declaration.", _position);
            context = context.WithDefaultEmptyOrderLeast(emptyMode == "least");
            return true;
        }

        if (TryMatchPhrase("ordering"))
        {
            SkipWhitespace();
            var mode = ReadNCName();
            if (mode is not ("ordered" or "unordered"))
                throw new ParseException($"XPST0003: Expected 'ordered' or 'unordered' after 'declare ordering' but found '{mode}'.", _position);
            SkipWhitespace();
            ExpectChar(';');
            // XQST0065: the ordering mode must not be declared twice.
            if (_seenOrderingDecl)
                throw new ParseException("XQST0065: More than one ordering mode declaration.", _position);
            _seenOrderingDecl = true;
            return true;
        }

        if (TryMatchPhrase("base-uri"))
        {
            SkipWhitespace();
            string uri = ReadStringLiteral();
            SkipWhitespace();
            ExpectChar(';');
            // XQST0032: the base URI must not be declared twice.
            if (context.BaseUri is not null)
                throw new ParseException("XQST0032: More than one base URI declaration.", _position);
            context = context.WithBaseUri(uri);
            return true;
        }

        if (TryMatchPhrase("option"))
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

        // A library module may declare the context item (type constraint and/or external);
        // an initial or default value in a library module is XQST0113. The declaration is
        // parsed and validated but its type constraint is not enforced at runtime.
        if (_isLibraryModule && TryMatchPhrase("context", "item"))
        {
            SkipWhitespace();
            if (TryMatchLiteral("as"))
            {
                SkipWhitespace();
                ReadSequenceTypeText();
            }
            SkipWhitespace();
            bool hasValue = false;
            if (TryMatchLiteral("external"))
            {
                SkipWhitespace();
                if (TryMatchLiteral(":="))
                {
                    hasValue = true;
                    SkipWhitespace();
                    ReadExpressionTo(';');
                    SkipWhitespace();
                }
            }
            else if (TryMatchLiteral(":="))
            {
                hasValue = true;
                SkipWhitespace();
                ReadExpressionTo(';');
                SkipWhitespace();
            }
            ExpectChar(';');
            // XQST0113: more than one context item declaration in one module.
            if (_seenContextItemDecl)
                throw new ParseException("XQST0113: More than one context item declaration in a module.", _position);
            _seenContextItemDecl = true;
            // XQST0113: an initial or default value is not allowed in a library module.
            if (hasValue)
                throw new ParseException("XQST0113: A context item declaration in a library module must not specify an initial or default value.", _position);
            return true;
        }

        // Not a recognized prolog declaration; stop consuming prolog items.
        _position = savedPosition;
        return false;
    }

    private bool TryParseFunctionDeclaration(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();
        if (!TryMatchLiteral("declare"))
        {
            _position = savedPosition;
            return false;
        }
        var annotations = ReadAnnotations();
        if (!TryMatchLiteral("function"))
        {
            _position = savedPosition;
            return false;
        }
        bool isPrivate = ValidateAnnotations(context, annotations, isFunction: true);

        SkipWhitespace();
        var (prefix, local, eqNameUri) = ReadQName();
        string fnNs = eqNameUri
            ?? (prefix is null
                ? context.DefaultFunctionNamespace ?? "http://www.w3.org/2005/xpath-functions"
                : context.Namespaces.TryGetValue(prefix, out var declaredFnNs)
                    ? declaredFnNs
                    : throw new ParseException($"XPST0081: Prefix '{prefix}' is not declared.", _position));

        SkipWhitespace();
        ExpectChar('(');
        var parameters = new List<UserFunctionParameter>();
        SkipWhitespace();
        if (!TryMatchChar(')'))
        {
            do
            {
                SkipWhitespace();
                ExpectChar('$');
                var paramName = ReadQNameText();
                SkipWhitespace();
                string? paramType = null;
                if (TryMatchLiteral("as"))
                {
                    SkipWhitespace();
                    paramType = ReadSequenceTypeText();
                }
                parameters.Add(new UserFunctionParameter(paramName, paramType));
                SkipWhitespace();
            } while (TryMatchChar(','));
            ExpectChar(')');
        }
        SkipWhitespace();
        string? returnType = null;
        if (TryMatchLiteral("as"))
        {
            SkipWhitespace();
            returnType = ReadSequenceTypeText();
        }
        SkipWhitespace();
        var body = ReadBracedExpression();
        SkipWhitespace();
        ExpectChar(';');

        // XQST0039: two parameters of one function must not have the same name.
        if (parameters.Select(p => p.Name).Distinct().Count() != parameters.Count)
            throw new ParseException($"XQST0039: Function '{local}' declares a parameter name more than once.", _position);
        // XQST0034: the same function name and arity must not be declared twice.
        if (context.UserFunctions.Any(f => f.LocalName == local && f.NamespaceUri == fnNs && f.Parameters.Count == parameters.Count))
            throw new ParseException($"XQST0034: Function '{local}' with arity {parameters.Count} is declared more than once.", _position);
        // XQST0045: functions must not be declared in a reserved function namespace.
        if (fnNs is "http://www.w3.org/2005/xpath-functions"
            or "http://www.w3.org/2005/xpath-functions/math"
            or "http://www.w3.org/2005/xpath-functions/map"
            or "http://www.w3.org/2005/xpath-functions/array"
            or "http://www.w3.org/2001/XMLSchema"
            or "http://www.w3.org/2001/XMLSchema-instance"
            or "http://www.w3.org/XML/1998/namespace"
            or "http://www.w3.org/2000/xmlns/")
        {
            throw new ParseException($"XQST0045: Functions must not be declared in the reserved namespace '{fnNs}'.", _position);
        }
        // XPST0003: reserved function names cannot be declared as user functions.
        if (prefix is null && ReservedFunctionNames.Contains(local))
            throw new ParseException($"XPST0003: '{local}' is a reserved function name and cannot be declared.", _position);
        context = context.WithUserFunction(new UserFunctionDeclaration(local, fnNs, parameters, returnType, body, _position, isPrivate));
        return true;
    }

    private bool TryParseVariableDeclaration(ref XQueryStaticContext context)
    {
        int savedPosition = _position;
        SkipWhitespace();
        if (!TryMatchLiteral("declare"))
        {
            _position = savedPosition;
            return false;
        }
        var annotations = ReadAnnotations();
        if (!TryMatchLiteral("variable"))
        {
            _position = savedPosition;
            return false;
        }
        bool isPrivate = ValidateAnnotations(context, annotations, isFunction: false);

        SkipWhitespace();
        ExpectChar('$');
        var (prefix, local, eqNameUri) = ReadQName();
        string varNs = eqNameUri
            ?? (prefix is null
                ? string.Empty
                : context.Namespaces.TryGetValue(prefix, out var declaredVarNs)
                    ? declaredVarNs
                    : throw new ParseException($"XPST0081: Prefix '{prefix}' is not declared.", _position));
        SkipWhitespace();
        string? varType = null;
        if (TryMatchLiteral("as"))
        {
            SkipWhitespace();
            varType = ReadSequenceTypeText();
        }
        SkipWhitespace();

        XPathAstNode? varBody = null;
        bool isExternal = false;
        if (TryMatchLiteral("external"))
        {
            isExternal = true;
            SkipWhitespace();
            // An external declaration may carry a default value.
            if (TryMatchLiteral(":="))
            {
                SkipWhitespace();
                varBody = ReadExpressionTo(';');
                SkipWhitespace();
            }
        }
        else
        {
            ExpectLiteral(":=");
            SkipWhitespace();
            varBody = ReadExpressionTo(';');
            SkipWhitespace();
        }
        ExpectChar(';');

        // XQST0049: the same variable name must not be declared twice.
        if (context.UserVariables.Any(v => v.LocalName == local && v.NamespaceUri == varNs))
            throw new ParseException($"XQST0049: Variable '${local}' is declared more than once.", _position);
        context = context.WithUserVariable(new UserVariableDeclaration(local, varNs, varType, varBody, isExternal, _position, isPrivate));
        return true;
    }

    // Reads a sequence of annotations ('%' EQName ('(' Literal (',' Literal)* ')')?) after
    // 'declare'. Annotation arguments must be literals (XPST0003 otherwise); the annotations
    // themselves are validated only once the declaration kind is known (ValidateAnnotations).
    private List<(string? Prefix, string Local, string? NamespaceUri, int Position)> ReadAnnotations()
    {
        var annotations = new List<(string? Prefix, string Local, string? NamespaceUri, int Position)>();
        while (true)
        {
            SkipWhitespace();
            if (_position >= _source.Length || _source[_position] != '%')
                break;
            _position++;
            var (prefix, local, eqNameUri) = ReadQName();
            annotations.Add((prefix, local, eqNameUri, _position));
            SkipWhitespace();
            if (_position < _source.Length && _source[_position] == '(')
            {
                _position++;
                SkipWhitespace();
                if (TryMatchChar(')'))
                    continue;
                while (true)
                {
                    SkipWhitespace();
                    ReadAnnotationArgumentLiteral();
                    SkipWhitespace();
                    if (TryMatchChar(','))
                        continue;
                    ExpectChar(')');
                    break;
                }
            }
        }
        return annotations;
    }

    // Annotation arguments are restricted to string and numeric literals.
    private void ReadAnnotationArgumentLiteral()
    {
        if (_position >= _source.Length)
            throw new ParseException("XPST0003: Expected a literal in annotation arguments.", _position);
        char c = _source[_position];
        if (c is '"' or '\'')
        {
            ReadStringLiteral();
            return;
        }
        int start = _position;
        while (_position < _source.Length && char.IsAsciiDigit(_source[_position]))
            _position++;
        if (_position < _source.Length && _source[_position] == '.')
        {
            _position++;
            while (_position < _source.Length && char.IsAsciiDigit(_source[_position]))
                _position++;
        }
        if (_position < _source.Length && _source[_position] is 'e' or 'E')
        {
            _position++;
            if (_position < _source.Length && _source[_position] is '+' or '-')
                _position++;
            while (_position < _source.Length && char.IsAsciiDigit(_source[_position]))
                _position++;
        }
        if (_position == start)
            throw new ParseException("XPST0003: Annotation arguments must be literals.", start);
    }

    // Validates the annotations on a declaration and returns whether it is private:
    //   - %public/%private (unprefixed or in the XQuery namespace) set visibility; more than
    //     one visibility annotation on a single declaration is XQST0106 (function) or
    //     XQST0116 (variable);
    //   - annotations in a reserved namespace (fn, math, map, array, xs, xsi, xml, xmlns) or
    //     unknown annotations in the XQuery namespace are XQST0045;
    //   - annotations in any other (bound) namespace are ignored.
    private bool ValidateAnnotations(
        XQueryStaticContext context,
        List<(string? Prefix, string Local, string? NamespaceUri, int Position)> annotations,
        bool isFunction)
    {
        int visibilityCount = 0;
        bool isPrivate = false;
        foreach (var (prefix, local, eqNameUri, position) in annotations)
        {
            string? ns = eqNameUri
                ?? (prefix is null
                    ? null
                    : context.Namespaces.TryGetValue(prefix, out var annotationNs)
                        ? annotationNs
                        : throw new ParseException($"XPST0081: Prefix '{prefix}' is not declared.", position));
            // XQST0045: annotations must not be in a reserved namespace.
            if (ns is "http://www.w3.org/2005/xpath-functions"
                or "http://www.w3.org/2005/xpath-functions/math"
                or "http://www.w3.org/2005/xpath-functions/map"
                or "http://www.w3.org/2005/xpath-functions/array"
                or "http://www.w3.org/2001/XMLSchema"
                or "http://www.w3.org/2001/XMLSchema-instance"
                or "http://www.w3.org/XML/1998/namespace"
                or "http://www.w3.org/2000/xmlns/")
            {
                throw new ParseException($"XQST0045: Annotation '%{local}' is in the reserved namespace '{ns}'.", position);
            }
            // Unprefixed annotation names are in the XQuery namespace, where only the
            // reserved visibility annotations are defined; anything else is XQST0045.
            if (ns is null or "http://www.w3.org/2012/xquery")
            {
                if (local is not ("public" or "private"))
                    throw new ParseException($"XQST0045: Unknown annotation '%{local}' in the XQuery namespace.", position);
                visibilityCount++;
                isPrivate = local == "private";
            }
            // Annotations in other namespaces are implementation-defined and ignored.
        }
        if (visibilityCount > 1)
        {
            throw new ParseException(
                isFunction
                    ? "XQST0106: A function declaration must not contain more than one %public or %private annotation."
                    : "XQST0116: A variable declaration must not contain more than one %public or %private annotation.",
                _position);
        }
        return isPrivate;
    }

    // Module namespace URIs and location hints get whitespace normalization: leading and
    // trailing whitespace is removed and internal whitespace runs collapse to a single space.
    internal static string NormalizeModuleUri(string uri)
    {
        var trimmed = uri.Trim();
        var sb = new StringBuilder(trimmed.Length);
        bool inWhitespaceRun = false;
        foreach (char c in trimmed)
        {
            if (c is ' ' or '\t' or '\r' or '\n')
            {
                inWhitespaceRun = true;
                continue;
            }
            if (inWhitespaceRun)
            {
                sb.Append(' ');
                inWhitespaceRun = false;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    private bool _seenOptionDecl;
    private bool _seenContextItemDecl;
    private bool _seenOrderingDecl;
    private bool _isLibraryModule;
    private bool _xml11LineEndings;
    private readonly List<(string Prefix, string Local, string Value, int Position)> _pendingOptions = new();

    // Matches a sequence of prolog keywords, each separated by optional whitespace/comments.
    // On failure the position is restored to the phrase start so the next alternative
    // phrase can be attempted.
    private bool TryMatchPhrase(params string[] words)
    {
        int start = _position;
        foreach (var word in words)
        {
            if (!TryMatchLiteral(word))
            {
                _position = start;
                return false;
            }
        }
        return true;
    }

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

    /// <summary>
    /// Reserved function names (kind-test names and keywords) that cannot be declared as
    /// unprefixed user functions.
    /// </summary>
    private static readonly HashSet<string> ReservedFunctionNames = new(StringComparer.Ordinal)
    {
        "attribute", "comment", "document-node", "element", "empty-sequence",
        "function", "if", "item", "namespace-node", "node", "processing-instruction",
        "schema-attribute", "schema-element", "switch", "text", "typeswitch",
        "array", "map"
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
        if (_position < _source.Length && _source[_position] == ':'
            && _position + 1 < _source.Length && _source[_position + 1] != '=')
        {
            _position++;
            return (first, ReadNCName(), null);
        }
        return (null, first, null);
    }

    // Reads a lexical QName and returns it in lexical form (prefix:local or Q{uri}local).
    private string ReadQNameText()
    {
        var (prefix, local, nsUri) = ReadQName();
        if (nsUri is not null)
            return $"Q{{{nsUri}}}{local}";
        return prefix is null ? local : $"{prefix}:{local}";
    }

    // Reads a sequence type in lexical form: an item type (with optional parenthesized
    // arguments) possibly unioned with '|', followed by an optional occurrence indicator.
    private string ReadSequenceTypeText()
    {
        SkipWhitespace();
        int start = _position;
        ReadItemTypeText();
        while (TryMatchChar('|'))
            ReadItemTypeText();
        if (_position < _source.Length && _source[_position] is '?' or '*' or '+')
            _position++;
        var text = _source[start.._position].Trim();
        // XPST0003: empty-sequence() must not carry an occurrence indicator.
        if (text.StartsWith("empty-sequence(", StringComparison.Ordinal)
            && text.Length > "empty-sequence()".Length)
        {
            throw new ParseException("XPST0003: empty-sequence() must not have an occurrence indicator.", start);
        }
        return text;
    }

    private void ReadItemTypeText()
    {
        SkipWhitespace();
        // Parenthesized item type: '(' ItemType ('|' ItemType)* ')' — e.g. a function
        // test used as a declared return type: (function(xs:string) as xs:string*).
        if (TryMatchChar('('))
        {
            ReadItemTypeText();
            while (TryMatchChar('|'))
                ReadItemTypeText();
            SkipWhitespace();
            ExpectChar(')');
            return;
        }
        int nameStart = _position;
        while (_position < _source.Length && (IsNameChar(_source[_position]) || _source[_position] == ':' || _source[_position] == '{'))
        {
            if (_source[_position] == '{')
            {
                int close = _source.IndexOf('}', _position);
                if (close < 0)
                    throw new ParseException("XPST0003: Unterminated EQName in sequence type.", _position);
                _position = close + 1;
            }
            else
            {
                _position++;
            }
        }
        if (_position == nameStart)
            throw new ParseException("XPST0003: Expected item type.", _position);
        // Optional parenthesized arguments: (), (*), (element-name), (*:name), etc.
        SkipWhitespace();
        if (_position < _source.Length && _source[_position] == '(')
        {
            int depth = 0;
            do
            {
                if (_position >= _source.Length)
                    throw new ParseException("XPST0003: Unterminated '(' in sequence type.", _position);
                if (_source[_position] == '(') depth++;
                else if (_source[_position] == ')') depth--;
                _position++;
            } while (depth > 0);
        }
        // Function tests may carry a return type: function(xs:integer) as xs:integer.
        SkipWhitespace();
        if (TryMatchLiteral("as"))
        {
            SkipWhitespace();
            ReadSequenceTypeText();
        }
    }

    // Reads '{' Expr '}' and parses the enclosed expression with the XPath parser.
    private XPathAstNode ReadBracedExpression()
    {
        ExpectChar('{');
        int start = _position;
        int depth = 1;
        while (_position < _source.Length && depth > 0)
        {
            char c = _source[_position];
            if (c == '\'' || c == '"')
            {
                SkipStringLiteral(c);
                continue;
            }
            if (c == '(' && _position + 1 < _source.Length && _source[_position + 1] == ':')
            {
                SkipComment();
                continue;
            }
            if (c == '{') depth++;
            else if (c == '}') depth--;
            _position++;
        }
        if (depth > 0)
            throw new ParseException("XPST0003: Unterminated '{' in function declaration.", _position);
        var inner = _source[start..(_position - 1)];
        // An empty (or comment-only) function body is the empty sequence (XQuery 3.1).
        if (string.IsNullOrWhiteSpace(StripComments(inner)))
            return new SequenceExpressionNode(Array.Empty<XPathAstNode>());
        return XPathParser.Parse(inner, allowFullFlwor: true, xml11LineEndings: _xml11LineEndings);
    }

    // Scans an expression up to the top-level terminator and parses it with the XPath parser.
    private XPathAstNode ReadExpressionTo(char terminator)
    {
        int start = _position;
        int depth = 0;
        while (_position < _source.Length)
        {
            char c = _source[_position];
            if (c == '\'' || c == '"')
            {
                SkipStringLiteral(c);
                continue;
            }
            if (c == '(' && _position + 1 < _source.Length && _source[_position + 1] == ':')
            {
                SkipComment();
                continue;
            }
            if (c == terminator && depth == 0)
            {
                var inner = _source[start.._position];
                return XPathParser.Parse(inner, allowFullFlwor: true, xml11LineEndings: _xml11LineEndings);
            }
            if (c is '(' or '[' or '{') depth++;
            else if (c is ')' or ']' or '}') depth--;
            _position++;
        }
        throw new ParseException($"XPST0003: Expected expression before '{terminator}'.", _position);
    }

    private void SkipStringLiteral(char quote)
    {
        _position++;
        while (_position < _source.Length)
        {
            if (_source[_position] == quote)
            {
                if (_position + 1 < _source.Length && _source[_position + 1] == quote)
                {
                    _position += 2;
                    continue;
                }
                _position++;
                return;
            }
            _position++;
        }
    }

    private void SkipComment()
    {
        int depth = 1;
        _position += 2; // at '(:'
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
    }

    private bool TryMatchChar(char c)
    {
        SkipWhitespace();
        if (_position < _source.Length && _source[_position] == c)
        {
            _position++;
            return true;
        }
        return false;
    }

    // Removes XQuery comments (possibly nested) from a text span.
    private static string StripComments(string text)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
            {
                int depth = 1;
                i += 2;
                while (i < text.Length && depth > 0)
                {
                    if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':') { depth++; i += 2; }
                    else if (i + 1 < text.Length && text[i] == ':' && text[i + 1] == ')') { depth--; i += 2; }
                    else i++;
                }
            }
            else
            {
                sb.Append(text[i]);
                i++;
            }
        }
        return sb.ToString();
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
    /// The query body expression AST (the empty sequence for a library module).
    /// </summary>
    public XPathAstNode Body { get; }

    /// <summary>
    /// True when the source was a library module (<c>module namespace ...;</c>) rather
    /// than a main module.
    /// </summary>
    public bool IsLibraryModule { get; }

    internal XQueryParseResult(XQueryStaticContext staticContext, XPathAstNode body, bool isLibraryModule = false)
    {
        StaticContext = staticContext;
        Body = body;
        IsLibraryModule = isLibraryModule;
    }
}
