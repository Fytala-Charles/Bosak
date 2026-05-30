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
    /// Custom document loader. If null, fn:doc will throw unless the API layer provides one.
    /// </summary>
    public Func<string, IXdmNode>? DocumentLoader { get; set; }

    /// <summary>
    /// When true, XPath comparisons use XSLT 1.0 / XPath 1.0 backwards-compatible
    /// coercion rules (e.g., string-to-boolean, number-to-boolean in general comparisons).
    /// </summary>
    public bool BackwardsCompatible { get; set; }

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
        => _variables.TryGetValue((localName, namespaceUri), out value);

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
        => _namespaces.TryGetValue(prefix, out namespaceUri!);

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
