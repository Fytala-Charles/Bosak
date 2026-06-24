// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Runtime evaluation context for XPath expressions. Holds the focus (context item, position, size),...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added document cache and loader for fn:doc / fn:collection                             |
//                      | Charles Korthout | 0.3   | 22-05-2026     | Added stable current-dateTime/date/time snapshot                                       |
//                      | Charles Korthout | 0.4   | 22-05-2026     | Added decimal-format support for fn:format-number                                      |
//                      | Charles Korthout | 0.5   | 24-05-2026     | Added SnapshotVariables / RestoreVariables for lexical scoping                         |
//                      | Charles Korthout | 0.6   | 27-05-2026     | Added DefaultCollation property and WithDefaultCollation helper                        |
//                      | Charles Korthout | 0.7   | 30-05-2026     | Added BackwardsCompatible property for XPath 1.0 general-comparison coercion rules     |
//                      | Charles Korthout | 0.8   | 10-06-2026     | Added LazyVariableResolver and _evaluatedLazyGlobals for deferred XSLT globals         |
//                      | Charles Korthout | 0.9   | 11-06-2026     | TryResolveNamespace resolves predefined xml prefix to XML namespace URI                 |
//                      | Charles Korthout | 1.0   | 13-06-2026     | Added ImplicitTimezoneOffsetMinutes property (defaults to UTC)                          |
//                      | Charles Korthout | 1.1   | 13-06-2026     | Added RegexGroups property for xsl:analyze-string / regex-group()                       |
//                      | Charles Korthout | 1.2   | 24-06-2026     | Added DefiningElementDefaultNamespace for element-available default namespace            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// Runtime evaluation context for XPath expressions.
/// Holds the focus (context item, position, size), variable bindings,
/// namespace resolution, and function libraries.
/// </summary>
public sealed class EvaluationContext
{
    // Focus
    private XdmValue _contextItem;
    private int _contextPosition;
    private int _contextSize;

    // Current item for fn:current() (XSLT current node)
    private XdmValue _currentItem = XdmValue.Undefined;

    // Variable bindings: QName key -> XdmValue
    private readonly Dictionary<(string LocalName, string NamespaceUri), XdmValue> _variables;

    // Cache for evaluated lazy variables (e.g. XSLT global variables with sequence constructors).
    // Survives RestoreVariables so once-evaluated globals remain available across template scopes.
    private readonly Dictionary<(string LocalName, string NamespaceUri), XdmValue> _evaluatedLazyGlobals;

    /// <summary>
    /// Optional callback for lazy variable resolution. Invoked when a variable is not found
    /// in <see cref="_variables"/> but a lazy resolver is registered. This enables XSLT global
    /// variables with sequence constructors to be evaluated on first reference.
    /// </summary>
    public Func<string, string, XdmValue?>? LazyVariableResolver { get; set; }

    /// <summary>
    /// When true, <see cref="Bosak.XPath.Standard.Functions.FunctionLibrary.Populate"/> will not
    /// be called automatically by <see cref="XPath31Expression.Evaluate"/>. Used by XSLT's
    /// <c>xsl:evaluate</c> to supply a restricted function library.
    /// </summary>
    public bool SkipStandardFunctionPopulation { get; set; }

    /// <summary>
    /// Optional collation-aware string comparer used by XPath comparison operators.
    /// Arguments are (left, right, collationUri); returns a value with the same sign
    /// conventions as <see cref="string.Compare(string, string)"/>.
    /// </summary>
    public Func<string, string, string, int>? CollationComparer { get; set; }

    // Namespace prefixes
    private readonly Dictionary<string, string> _namespaces;

    // Function libraries, indexed by (namespace, localName, arity)
    private readonly Dictionary<(string, string, int), FunctionSignature> _functions;

    // Document cache for fn:doc / fn:collection identity
    private readonly Dictionary<string, IXdmNode> _documentCache;

    // Stable snapshot for current-dateTime / current-date / current-time
    private DateTimeOffset? _currentDateTimeSnapshot;

    // Decimal formats for fn:format-number
    private DecimalFormat _defaultDecimalFormat = new();
    private readonly Dictionary<(string LocalName, string NamespaceUri), DecimalFormat> _namedDecimalFormats;

    public EvaluationContext()
    {
        _contextItem = XdmValue.Undefined;
        _contextPosition = 0;
        _contextSize = 0;
        _variables = new Dictionary<(string, string), XdmValue>();
        _evaluatedLazyGlobals = new Dictionary<(string, string), XdmValue>();
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
        _functions = new Dictionary<(string, string, int), FunctionSignature>();
        _documentCache = new Dictionary<string, IXdmNode>();
        _namedDecimalFormats = new Dictionary<(string, string), DecimalFormat>();
    }

    /// <summary>
    /// Optional base URI used to resolve relative document URIs.
    /// </summary>
    public string? BaseUri { get; set; }

    /// <summary>
    /// The implicit timezone offset in minutes used when a date, time, or dateTime value
    /// has no explicit timezone. Defaults to UTC (0 minutes).
    /// </summary>
    public int ImplicitTimezoneOffsetMinutes { get; set; }

    /// <summary>
    /// Captured substring values for the current <c>xsl:analyze-string</c> matching substring,
    /// indexed by group number (0 is the whole match). Used by <c>regex-group()</c>.
    /// </summary>
    public string[]? RegexGroups { get; set; }

    /// <summary>
    /// Custom document loader. If null, fn:doc will throw unless the API layer provides one.
    /// </summary>
    public Func<string, IXdmNode>? DocumentLoader { get; set; }

    /// <summary>
    /// When true, XPath comparisons use XSLT 1.0 / XPath 1.0 backwards-compatible
    /// coercion rules (e.g., string-to-boolean, number-to-boolean in general comparisons).
    /// </summary>
    public bool BackwardsCompatible { get; set; }

    /// <summary>
    /// The default element namespace URI for unprefixed element and type names.
    /// When set, <see cref="TryResolveNamespace"/> returns this value for the empty prefix.
    /// </summary>
    public string? DefaultElementNamespace { get; set; }

    /// <summary>
    /// The default namespace URI of the element that contains the XPath expression.
    /// Used by XSLT's <c>fn:element-available</c> to expand unprefixed lexical QNames
    /// per the XSLT specification, which differs from the XPath default element namespace.
    /// </summary>
    public string? DefiningElementDefaultNamespace { get; set; }

    /// <summary>
    /// Loads a document by URI, using the cache and <see cref="DocumentLoader"/>.
    /// </summary>
    public IXdmNode LoadDocument(string uri)
    {
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) && !string.IsNullOrEmpty(BaseUri))
        {
            uri = new Uri(new Uri(BaseUri), uri).AbsoluteUri;
        }

        if (_documentCache.TryGetValue(uri, out var cached))
            return cached;

        if (DocumentLoader is null)
            throw new InvalidOperationException($"No document loader configured. Cannot load document: {uri}");

        var node = DocumentLoader(uri);
        _documentCache[uri] = node;
        return node;
    }

    // ------------------------------------------------------------------
    // Focus
    // ------------------------------------------------------------------

    public XdmValue ContextItem => _contextItem;
    public int ContextPosition => _contextPosition;
    public int ContextSize => _contextSize;

    /// <summary>
    /// The current item for fn:current(). In XSLT this is the node matched by the
    /// current template rule or selected by xsl:for-each / xsl:apply-templates.
    /// </summary>
    public XdmValue CurrentItem => _currentItem;

    public EvaluationContext WithFocus(XdmValue item, int position, int size)
    {
        _contextItem = item;
        _contextPosition = position;
        _contextSize = size;
        return this;
    }

    public EvaluationContext WithCurrentItem(XdmValue item)
    {
        _currentItem = item;
        return this;
    }

    // ------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------

    public EvaluationContext WithVariable(string localName, XdmValue value, string namespaceUri = "")
    {
        _variables[(localName, namespaceUri)] = value;
        return this;
    }

    public bool TryGetVariable(string localName, out XdmValue value, string namespaceUri = "")
    {
        if (_variables.TryGetValue((localName, namespaceUri), out value))
            return true;

        if (_evaluatedLazyGlobals.TryGetValue((localName, namespaceUri), out value))
            return true;

        if (LazyVariableResolver != null)
        {
            var lazyValue = LazyVariableResolver(localName, namespaceUri);
            if (lazyValue != null)
            {
                value = lazyValue.Value;
                _evaluatedLazyGlobals[(localName, namespaceUri)] = value;
                return true;
            }
        }

        return false;
    }

    public bool RemoveVariable(string localName, string namespaceUri = "")
        => _variables.Remove((localName, namespaceUri));

    /// <summary>
    /// Creates a snapshot of all current variable bindings.
    /// </summary>
    public Dictionary<(string LocalName, string NamespaceUri), XdmValue> SnapshotVariables()
        => new Dictionary<(string, string), XdmValue>(_variables);

    /// <summary>
    /// Restores variable bindings from a snapshot, removing any variables added since.
    /// </summary>
    public void RestoreVariables(Dictionary<(string LocalName, string NamespaceUri), XdmValue> snapshot)
    {
        _variables.Clear();
        foreach (var (key, value) in snapshot)
            _variables[key] = value;
    }

    // ------------------------------------------------------------------
    // Namespaces
    // ------------------------------------------------------------------

    public EvaluationContext WithNamespace(string prefix, string namespaceUri)
    {
        _namespaces[prefix] = namespaceUri;
        return this;
    }

    public bool TryResolveNamespace(string prefix, out string namespaceUri)
    {
        if (prefix == "" && !string.IsNullOrEmpty(DefaultElementNamespace))
        {
            namespaceUri = DefaultElementNamespace;
            return true;
        }
        if (prefix == "xml")
        {
            namespaceUri = "http://www.w3.org/XML/1998/namespace";
            return true;
        }
        return _namespaces.TryGetValue(prefix, out namespaceUri!);
    }

    /// <summary>
    /// Returns a snapshot of the current namespace bindings.
    /// </summary>
    public Dictionary<string, string> SnapshotNamespaces()
        => new(_namespaces);

    /// <summary>
    /// Restores namespace bindings from a snapshot, discarding any bindings
    /// added since the snapshot was taken.
    /// </summary>
    public void RestoreNamespaces(Dictionary<string, string> snapshot)
    {
        _namespaces.Clear();
        foreach (var kv in snapshot)
            _namespaces[kv.Key] = kv.Value;
    }

    // ------------------------------------------------------------------
    // Functions
    // ------------------------------------------------------------------

    public EvaluationContext RegisterFunction(FunctionSignature signature)
    {
        var key = (signature.NamespaceUri, signature.LocalName, signature.Arity);
        _functions[key] = signature;
        return this;
    }

    public bool TryResolveFunction(string namespaceUri, string localName, int arity, out FunctionSignature signature)
        => _functions.TryGetValue((namespaceUri, localName, arity), out signature!);

    /// <summary>
    /// Removes a registered function signature, if present.
    /// </summary>
    public bool UnregisterFunction(string namespaceUri, string localName, int arity)
        => _functions.Remove((namespaceUri, localName, arity));

    /// <summary>
    /// Returns a shallow copy of the currently registered function signatures.
    /// </summary>
    public Dictionary<(string NamespaceUri, string LocalName, int Arity), FunctionSignature> SnapshotFunctions()
        => new Dictionary<(string, string, int), FunctionSignature>(_functions);

    /// <summary>
    /// Replaces the current function library with the supplied snapshot.
    /// </summary>
    public void RestoreFunctions(Dictionary<(string NamespaceUri, string LocalName, int Arity), FunctionSignature> snapshot)
    {
        _functions.Clear();
        foreach (var (key, value) in snapshot)
            _functions[key] = value;
    }

    /// <summary>
    /// Removes all registered functions.
    /// </summary>
    public void ClearFunctions()
    {
        _functions.Clear();
    }

    // ------------------------------------------------------------------
    // Decimal Formats
    // ------------------------------------------------------------------

    public DecimalFormat DefaultDecimalFormat
    {
        get => _defaultDecimalFormat;
        set => _defaultDecimalFormat = value;
    }

    public EvaluationContext WithDecimalFormat(string? name, DecimalFormat format)
    {
        if (string.IsNullOrEmpty(name))
        {
            _defaultDecimalFormat = format;
        }
        else
        {
            _namedDecimalFormats[(name, "")] = format;
        }
        return this;
    }

    public EvaluationContext WithDecimalFormat(string localName, string namespaceUri, DecimalFormat format)
    {
        _namedDecimalFormats[(localName, namespaceUri)] = format;
        return this;
    }

    public DecimalFormat? GetDecimalFormat(string name)
    {
        // Try empty namespace first (local name)
        if (_namedDecimalFormats.TryGetValue((name, ""), out var fmt))
            return fmt;

        // Try as prefixed name: resolve prefix using namespaces
        if (name.Contains(':'))
        {
            int colon = name.IndexOf(':');
            string prefix = name.Substring(0, colon);
            string local = name.Substring(colon + 1);
            if (TryResolveNamespace(prefix, out var ns))
            {
                if (_namedDecimalFormats.TryGetValue((local, ns), out fmt))
                    return fmt;
            }
        }

        return null;
    }

    public DecimalFormat? GetDecimalFormat(string localName, string namespaceUri)
    {
        if (_namedDecimalFormats.TryGetValue((localName, namespaceUri), out var fmt))
            return fmt;
        return null;
    }

    // ------------------------------------------------------------------
    // Default Collation
    // ------------------------------------------------------------------

    public string DefaultCollation { get; set; } = string.Empty;

    public EvaluationContext WithDefaultCollation(string collation)
    {
        DefaultCollation = collation;
        return this;
    }

    /// <summary>
    /// Returns a stable snapshot of the current date/time for the lifetime of this context.
    /// Used by fn:current-dateTime, fn:current-date, and fn:current-time.
    /// </summary>
    public DateTimeOffset CurrentDateTimeSnapshot => _currentDateTimeSnapshot ??= System.DateTimeOffset.Now;
}
