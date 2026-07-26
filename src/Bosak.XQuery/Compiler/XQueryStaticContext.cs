// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 July 2026
// PURPOSE              : Holds the static context accumulated from an XQuery prolog (namespaces, defaults, variables, functions).
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-07-2026     | Creation — minimal static context for prolog-less queries                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 25-07-2026     | Store option declarations in the static context                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 26-07-2026     | UserFunctionDeclaration/UserVariableDeclaration records with storage, cloning, predeclared 'local' prefix |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XQuery.Compiler;

/// <summary>A user function declared in the prolog (<c>declare function p:name($p as T, ...) as R { body }</c>).</summary>
public sealed record UserFunctionDeclaration(
    string LocalName,
    string NamespaceUri,
    IReadOnlyList<UserFunctionParameter> Parameters,
    string? ReturnType,
    XPathAstNode Body,
    int Position);

/// <summary>One parameter of a user function declaration: a name and an optional sequence type.</summary>
public sealed record UserFunctionParameter(string Name, string? TypeName);

/// <summary>A user variable declared in the prolog (<c>declare variable $p:name (as T)? := expr;</c> or <c>external</c>).</summary>
public sealed record UserVariableDeclaration(
    string LocalName,
    string NamespaceUri,
    string? TypeName,
    XPathAstNode? Body,
    bool IsExternal,
    int Position);

/// <summary>
/// The static context derived from an XQuery prolog. It is immutable after construction
/// and is used during compilation to resolve QNames and during execution to configure the
/// <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext"/>.
/// </summary>
public sealed class XQueryStaticContext
{
    private readonly Dictionary<string, string> _namespaces;
    private readonly Dictionary<(string LocalName, string NamespaceUri), XdmValue> _variables;
    private readonly HashSet<(string LocalName, string NamespaceUri, int Arity)> _functionSignatures;
    private readonly List<(string LocalName, string NamespaceUri, string Value)> _options;
    private readonly List<UserFunctionDeclaration> _userFunctions;
    private readonly List<UserVariableDeclaration> _userVariables;

    /// <summary>
    /// Creates a static context with the standard XQuery namespace prefixes pre-bound.
    /// </summary>
    public XQueryStaticContext()
    {
        _namespaces = new Dictionary<string, string>
        {
            ["xml"] = "http://www.w3.org/XML/1998/namespace",
            ["xs"] = "http://www.w3.org/2001/XMLSchema",
            ["fn"] = "http://www.w3.org/2005/xpath-functions",
            ["math"] = "http://www.w3.org/2005/xpath-functions/math",
            ["map"] = "http://www.w3.org/2005/xpath-functions/map",
            ["array"] = "http://www.w3.org/2005/xpath-functions/array",
            ["local"] = "http://www.w3.org/2005/xquery-local-functions",
            ["err"] = "http://www.w3.org/2005/xqt-errors"
        };
        _variables = new Dictionary<(string, string), XdmValue>();
        _functionSignatures = new HashSet<(string, string, int)>();
        _options = new List<(string, string, string)>();
        _userFunctions = new List<UserFunctionDeclaration>();
        _userVariables = new List<UserVariableDeclaration>();
    }

    private XQueryStaticContext(
        Dictionary<string, string> namespaces,
        Dictionary<(string LocalName, string NamespaceUri), XdmValue> variables,
        HashSet<(string LocalName, string NamespaceUri, int Arity)> functionSignatures,
        List<(string LocalName, string NamespaceUri, string Value)> options,
        List<UserFunctionDeclaration> userFunctions,
        List<UserVariableDeclaration> userVariables,
        string? defaultElementNamespace,
        string? defaultFunctionNamespace,
        string? defaultCollation,
        string? baseUri)
    {
        _namespaces = namespaces;
        _variables = variables;
        _functionSignatures = functionSignatures;
        _options = options;
        _userFunctions = userFunctions;
        _userVariables = userVariables;
        DefaultElementNamespace = defaultElementNamespace;
        DefaultFunctionNamespace = defaultFunctionNamespace;
        DefaultCollation = defaultCollation;
        BaseUri = baseUri;
    }

    /// <summary>
    /// The default namespace URI for unprefixed element and type names.
    /// </summary>
    public string? DefaultElementNamespace { get; private init; }

    /// <summary>
    /// The default namespace URI for unprefixed function names.
    /// Defaults to the fn namespace when not explicitly declared.
    /// </summary>
    public string? DefaultFunctionNamespace { get; private init; } = "http://www.w3.org/2005/xpath-functions";

    /// <summary>
    /// The default collation URI used by string comparison functions.
    /// </summary>
    public string? DefaultCollation { get; private init; }

    /// <summary>
    /// The static base URI for resolving relative URIs in the query.
    /// </summary>
    public string? BaseUri { get; private init; }

    /// <summary>
    /// Returns a read-only view of the namespace bindings (prefix → URI).
    /// </summary>
    public IReadOnlyDictionary<string, string> Namespaces => _namespaces;

    /// <summary>
    /// Returns a read-only view of the statically declared variables and their values.
    /// </summary>
    public IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> Variables => _variables;

    /// <summary>
    /// Returns a read-only view of the declared function signatures.
    /// </summary>
    public IReadOnlyCollection<(string LocalName, string NamespaceUri, int Arity)> FunctionSignatures => _functionSignatures;

    /// <summary>
    /// Returns a read-only view of the option declarations
    /// (<c>declare option output:* "..."</c>) in prolog order.
    /// </summary>
    public IReadOnlyList<(string LocalName, string NamespaceUri, string Value)> Options => _options;

    /// <summary>Returns a read-only view of the user function declarations.</summary>
    public IReadOnlyList<UserFunctionDeclaration> UserFunctions => _userFunctions;

    /// <summary>Returns a read-only view of the user variable declarations.</summary>
    public IReadOnlyList<UserVariableDeclaration> UserVariables => _userVariables;

    /// <summary>
    /// Creates a new context with an option declaration appended.
    /// </summary>
    public XQueryStaticContext WithOption(string localName, string namespaceUri, string value)
    {
        var copy = new List<(string, string, string)>(_options) { (localName, namespaceUri, value) };
        return CloneWith(options: copy);
    }

    /// <summary>Creates a new context with a user function declaration appended.</summary>
    public XQueryStaticContext WithUserFunction(UserFunctionDeclaration declaration)
    {
        var copy = new List<UserFunctionDeclaration>(_userFunctions) { declaration };
        return CloneWith(userFunctions: copy);
    }

    /// <summary>Creates a new context with a user variable declaration appended.</summary>
    public XQueryStaticContext WithUserVariable(UserVariableDeclaration declaration)
    {
        var copy = new List<UserVariableDeclaration>(_userVariables) { declaration };
        return CloneWith(userVariables: copy);
    }

    /// <summary>
    /// Creates a new context with the specified namespace binding added or replaced.
    /// </summary>
    public XQueryStaticContext WithNamespace(string prefix, string namespaceUri)
    {
        var copy = new Dictionary<string, string>(_namespaces) { [prefix] = namespaceUri };
        return CloneWith(namespaces: copy);
    }

    /// <summary>
    /// Creates a new context with the specified default element namespace.
    /// </summary>
    public XQueryStaticContext WithDefaultElementNamespace(string? namespaceUri)
        => CloneWith(defaultElementNamespace: namespaceUri);

    /// <summary>
    /// Creates a new context with the specified default function namespace.
    /// </summary>
    public XQueryStaticContext WithDefaultFunctionNamespace(string? namespaceUri)
        => CloneWith(defaultFunctionNamespace: namespaceUri);

    /// <summary>
    /// Creates a new context with the specified default collation.
    /// </summary>
    public XQueryStaticContext WithDefaultCollation(string? collation)
        => CloneWith(defaultCollation: collation);

    /// <summary>
    /// Creates a new context with the specified static base URI.
    /// </summary>
    public XQueryStaticContext WithBaseUri(string? baseUri)
        => CloneWith(baseUri: baseUri);

    /// <summary>
    /// Creates a new context with a declared variable.
    /// </summary>
    public XQueryStaticContext WithVariable(string localName, string namespaceUri, XdmValue value)
    {
        var copy = new Dictionary<(string, string), XdmValue>(_variables) { [(localName, namespaceUri)] = value };
        return CloneWith(variables: copy);
    }

    /// <summary>
    /// Creates a new context with a declared function signature.
    /// </summary>
    public XQueryStaticContext WithFunctionSignature(string localName, string namespaceUri, int arity)
    {
        var copy = new HashSet<(string, string, int)>(_functionSignatures) { (localName, namespaceUri, arity) };
        return CloneWith(functionSignatures: copy);
    }

    private XQueryStaticContext CloneWith(
        Dictionary<string, string>? namespaces = null,
        Dictionary<(string LocalName, string NamespaceUri), XdmValue>? variables = null,
        HashSet<(string LocalName, string NamespaceUri, int Arity)>? functionSignatures = null,
        List<(string LocalName, string NamespaceUri, string Value)>? options = null,
        List<UserFunctionDeclaration>? userFunctions = null,
        List<UserVariableDeclaration>? userVariables = null,
        string? defaultElementNamespace = null,
        string? defaultFunctionNamespace = null,
        string? defaultCollation = null,
        string? baseUri = null)
    {
        return new XQueryStaticContext(
            namespaces ?? _namespaces,
            variables ?? _variables,
            functionSignatures ?? _functionSignatures,
            options ?? _options,
            userFunctions ?? _userFunctions,
            userVariables ?? _userVariables,
            defaultElementNamespace ?? DefaultElementNamespace,
            defaultFunctionNamespace ?? DefaultFunctionNamespace,
            defaultCollation ?? DefaultCollation,
            baseUri ?? BaseUri);
    }
}
