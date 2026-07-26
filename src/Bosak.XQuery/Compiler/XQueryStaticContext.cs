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
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;

namespace Bosak.XQuery.Compiler;

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
            ["err"] = "http://www.w3.org/2005/xqt-errors"
        };
        _variables = new Dictionary<(string, string), XdmValue>();
        _functionSignatures = new HashSet<(string, string, int)>();
        _options = new List<(string, string, string)>();
    }

    private XQueryStaticContext(
        Dictionary<string, string> namespaces,
        Dictionary<(string LocalName, string NamespaceUri), XdmValue> variables,
        HashSet<(string LocalName, string NamespaceUri, int Arity)> functionSignatures,
        List<(string LocalName, string NamespaceUri, string Value)> options,
        string? defaultElementNamespace,
        string? defaultFunctionNamespace,
        string? defaultCollation,
        string? baseUri)
    {
        _namespaces = namespaces;
        _variables = variables;
        _functionSignatures = functionSignatures;
        _options = options;
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

    /// <summary>
    /// Creates a new context with an option declaration appended.
    /// </summary>
    public XQueryStaticContext WithOption(string localName, string namespaceUri, string value)
    {
        var copy = new List<(string, string, string)>(_options) { (localName, namespaceUri, value) };
        return CloneWith(options: copy);
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
            defaultElementNamespace ?? DefaultElementNamespace,
            defaultFunctionNamespace ?? DefaultFunctionNamespace,
            defaultCollation ?? DefaultCollation,
            baseUri ?? BaseUri);
    }
}
