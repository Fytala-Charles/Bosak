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
//                      | Charles Korthout | 1.1   | 19-07-2026     | CurrentDateTimeSnapshot initializes implicit timezone from snapshot offset when not set |
//                      | Charles Korthout | 1.1   | 13-06-2026     | Added RegexGroups property for xsl:analyze-string / regex-group()                       |
//                      | Charles Korthout | 1.2   | 24-06-2026     | Added DefiningElementDefaultNamespace for element-available default namespace            |
//                      | Charles Korthout | 1.3   | 24-06-2026     | Added DocumentPostProcessor for XSLT whitespace stripping on loaded documents          |
//                      | Charles Korthout | 1.4   | 25-06-2026     | Added InitialTemplateCallParameters/TunnelParameters for named-template entry points   |
//                      | Charles Korthout | 1.5   | 25-06-2026     | Added RegisterDocument to pre-cache source documents for fn:doc identity               |
//                      | Charles Korthout | 1.6   | 25-06-2026     | Added IsStaticEvaluation flag for use-when/static-expression function libraries        |
//                      | Charles Korthout | 1.7   | 26-06-2026     | Added SnapshotLazyGlobals/RestoreLazyGlobals to isolate function-local lazy variables  |
//                      | Charles Korthout | 1.8   | 29-06-2026     | Added SkipLazyGlobalCacheOnce and TryGetBoundVariable for deferred locals              |
//                      | Charles Korthout | 2.0   | 26-06-2026     | Added CurrentOutputUri for fn:current-output-uri                                       |
//                      | Charles Korthout | 1.9   | 26-06-2026     | File-not-found document loads report FODC0002 so xsl:catch can match                    |
//                      | Charles Korthout | 2.1   | 15-07-2026     | Added ResourceUriMapper to redirect published http: resource URIs to local files        |
//                      | Charles Korthout | 2.2   | 15-07-2026     | Added XsltVersion override for fn:system-property('xsl:version')                       |
//                      | Charles Korthout | 2.3   | 18-07-2026     | Added Collections dictionary for fn:collection/fn:uri-collection resolution             |
//                      | Charles Korthout | 2.4   | 20-07-2026     | Added IsXsltMode to expose XSLT-only functions (fn:current, fn:system-property) only in XSLT mode |
//                      | Charles Korthout | 2.5   | 21-07-2026     | Convert UriFormatException/IOException/XmlException to FODC0005/FODC0002 in LoadDocument |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.6   | 25-07-2026     | TryResolveFunction falls back to variadic signatures (fn:concat#N for any N >= 2)      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.7   | 25-07-2026     | Added ElementConstructorHook and ContentNodeConstructorHook for XQuery constructors    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.8   | 25-07-2026     | Added RemoveNamespace; predefined xsi and local namespace prefixes                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.9   | 25-07-2026     | Added AttributeConstructorHook and DocumentConstructorHook                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.10  | 25-07-2026     | Added StaticOutputParameters for XQuery output declarations                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.11  | 29-07-2026     | WithNamespace removes the binding on zero-length URI (namespace undeclaration) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.12  | 29-07-2026     | Constructor-local namespace tracking for materialization on built elements |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.13  | 01-08-2026     | Added XQueryModuleSources registry for fn:load-xquery-module resolution |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.14  | 01-08-2026     | Snapshot/restore helpers for module-local decimal formats |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.15  | 15-08-2026     | Added CollectionValues for query-based environment collections in QT3 harness        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using System.IO;
using System.Xml;
using System.Xml.Schema;

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
    /// When true, values returned by <see cref="LazyVariableResolver"/> are not cached in
    /// <see cref="_evaluatedLazyGlobals"/>. Used by XSLT function calls to keep function-local
    /// lazy variables from leaking into the global cache.
    /// </summary>
    public bool SuppressLazyGlobalCaching { get; set; }

    /// <summary>
    /// When set to <c>true</c>, the next value returned by <see cref="LazyVariableResolver"/>
    /// will not be cached in <see cref="_evaluatedLazyGlobals"/>, but the flag is then reset.
    /// This allows a resolver to suppress caching for a specific resolution (e.g. a function-local
    /// variable) while still letting globals cache normally.
    /// </summary>
    public bool SkipLazyGlobalCacheOnce { get; set; }

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

    /// <summary>
    /// Provider hook for XQuery element constructors. When set, the ConstructElement
    /// opcode builds nodes through it; when null, element construction raises an error.
    /// The API layers register an XDocument-based implementation by default.
    /// </summary>
    public Func<XdmElementSpec, IXdmNode>? ElementConstructorHook { get; set; }

    /// <summary>
    /// Provider hook for standalone comment and processing-instruction constructors.
    /// The API layers register an XDocument-based implementation by default.
    /// </summary>
    public Func<XdmContentItem, IXdmNode>? ContentNodeConstructorHook { get; set; }

    /// <summary>
    /// Provider hook for computed attribute constructors (free-standing attribute nodes).
    /// The API layers register an XDocument-based implementation by default.
    /// </summary>
    public Func<XdmAttributeValue, IXdmNode>? AttributeConstructorHook { get; set; }

    /// <summary>
    /// Provider hook for computed document constructors. The API layers register an
    /// XDocument-based implementation by default.
    /// </summary>
    public Func<IReadOnlyList<XdmContentItem>, IXdmNode>? DocumentConstructorHook { get; set; }

    /// <summary>
    /// Static output declarations from an XQuery prolog (<c>declare option output:* "..."</c>),
    /// keyed by (namespace URI, local name). QName-valued parameters
    /// (cdata-section-elements, suppress-indentation) carry space-separated expanded
    /// <c>{uri}local</c> tokens. Consumed by <c>fn:serialize</c> as default serialization
    /// parameters.
    /// </summary>
    public IReadOnlyDictionary<(string NamespaceUri, string LocalName), string>? StaticOutputParameters { get; set; }

    // Namespace prefixes
    private readonly Dictionary<string, string> _namespaces;

    // Function libraries, indexed by (namespace, localName, arity)
    private readonly Dictionary<(string, string, int), FunctionSignature> _functions;

    // Document cache for fn:doc / fn:collection identity
    private readonly Dictionary<string, IXdmNode> _documentCache;

    // XML Schema set for schema-aware kind tests and typed values.
    private XmlSchemaSet? _schemaSet;

    /// <summary>
    /// Optional resolver used to load schemas referenced by <c>import schema</c>.
    /// Arguments are the target namespace URI and the ordered location hints;
    /// returns a stream for the schema content or null when the schema cannot be located.
    /// </summary>
    public Func<string, IReadOnlyList<string>, Stream?>? SchemaResolver { get; set; }

    // Stable snapshot for current-dateTime / current-date / current-time
    private DateTimeOffset? _currentDateTimeSnapshot;
    private bool _implicitTimezoneOffsetSet;

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
            ["xsi"] = "http://www.w3.org/2001/XMLSchema-instance",
            ["fn"] = "http://www.w3.org/2005/xpath-functions",
            ["math"] = "http://www.w3.org/2005/xpath-functions/math",
            ["map"] = "http://www.w3.org/2005/xpath-functions/map",
            ["array"] = "http://www.w3.org/2005/xpath-functions/array",
            ["local"] = "http://www.w3.org/2005/xquery-local-functions",
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
    /// The URI of the current output destination in XSLT. Empty or null when the
    /// evaluation is in a temporary output state (e.g., inside xsl:variable, a
    /// stylesheet function, a sort key, or a pattern predicate).
    /// </summary>
    public string? CurrentOutputUri { get; set; }

    /// <summary>
    /// When true, the expression is being evaluated in a static context (e.g. a
    /// <c>use-when</c> attribute or a static variable select expression). Function
    /// availability checks restrict the function library to context-independent
    /// functions.
    /// </summary>
    public bool IsStaticEvaluation { get; set; }

    /// <summary>
    /// When true, the evaluation is being performed by the XSLT processor and
    /// XSLT-only functions such as <c>fn:current</c> and <c>fn:system-property</c>
    /// are available. XPath-only contexts leave this false.
    /// </summary>
    public bool IsXsltMode { get; set; }

    /// <summary>
    /// When true, XML 1.1 semantics apply: prefixed namespace undeclarations
    /// (<c>xmlns:p=""</c>) are accepted in element constructors instead of raising
    /// XQST0085. This is set by the host based on the document's declared XML version.
    /// </summary>
    public bool Xml11Mode { get; set; }

    /// <summary>
    /// The implicit timezone offset in minutes used when a date, time, or dateTime value
    /// has no explicit timezone. Defaults to UTC (0 minutes).
    /// </summary>
    public int ImplicitTimezoneOffsetMinutes
    {
        get => _implicitTimezoneOffsetMinutes;
        set
        {
            _implicitTimezoneOffsetMinutes = value;
            _implicitTimezoneOffsetSet = true;
        }
    }
    private int _implicitTimezoneOffsetMinutes;

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
    /// Optional mapper that translates a requested resource URI (for example an <c>http:</c> URI
    /// published by a test suite) to a local file path. Returns <c>null</c> when the URI is not
    /// mapped. Consulted by fn:doc, fn:json-doc, fn:unparsed-text(-available/-lines), and
    /// fn:transform's stylesheet-location before any filesystem/network access.
    /// </summary>
    public Func<string, string?>? ResourceUriMapper { get; set; }

    /// <summary>
    /// Collection URI resolver. Keys are absolute collection URIs (the empty string key
    /// designates the default collection); values are the absolute URIs or file paths of the
    /// documents in the collection. Used by fn:collection and fn:uri-collection.
    /// </summary>
    public Dictionary<string, IReadOnlyList<string>> Collections { get; } = new();

    /// <summary>
    /// Precomputed collection values declared by environment &lt;collection&gt;&lt;query&gt; elements.
    /// Keys are collection URIs (empty string for the default collection); values are the
    /// already-evaluated XDM sequences. Checked before <see cref="Collections"/> by
    /// fn:collection / fn:uri-collection.
    /// </summary>
    public Dictionary<string, XdmValue> CollectionValues { get; } = new();

    /// <summary>
    /// XQuery library-module sources available to fn:load-xquery-module, keyed by the
    /// module's target namespace URI. Each entry lists candidate sources with an optional
    /// location hint (matched by the import's <c>at</c> clause or the load options'
    /// <c>location-hints</c>). When no candidate matches a requested URI, the loader falls
    /// back to treating the URI as a filesystem path relative to the static base URI.
    /// </summary>
    public Dictionary<string, List<(string? Location, string Source)>> XQueryModuleSources { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Optional post-processor applied to documents loaded through <see cref="DocumentLoader"/>.
    /// Used by XSLT to apply xsl:strip-space / xsl:preserve-space rules to documents
    /// returned by fn:doc and fn:document.
    /// </summary>
    public Func<IXdmNode, IXdmNode>? DocumentPostProcessor { get; set; }

    /// <summary>
    /// When true, XPath comparisons use XSLT 1.0 / XPath 1.0 backwards-compatible
    /// coercion rules (e.g., string-to-boolean, number-to-boolean in general comparisons).
    /// </summary>
    public bool BackwardsCompatible { get; set; }

    /// <summary>
    /// Optional override for the effective XSLT version reported by
    /// <c>fn:system-property('xsl:version')</c> during a transformation.
    /// </summary>
    public double? XsltVersion { get; set; }

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
    /// Optional call parameters supplied for the initial named-template entry point.
    /// Keys are expanded QNames in Clark notation (<c>{uri}local</c> or <c>local</c>);
    /// values are the corresponding XDM values.
    /// </summary>
    public Dictionary<string, XdmValue>? InitialTemplateCallParameters { get; set; }

    /// <summary>
    /// Optional tunnel parameters supplied for the initial named-template entry point.
    /// Keys are expanded QNames in Clark notation (<c>{uri}local</c> or <c>local</c>);
    /// values are the corresponding XDM values.
    /// </summary>
    public Dictionary<string, XdmValue>? InitialTemplateTunnelParameters { get; set; }

    /// <summary>
    /// Loads a document by URI, using the cache and <see cref="DocumentLoader"/>.
    /// </summary>
    public IXdmNode LoadDocument(string uri)
    {
        if (DocumentLoader is null)
            throw new InvalidOperationException($"No document loader configured. Cannot load document: {uri}");

        IXdmNode node;
        try
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) && !string.IsNullOrEmpty(BaseUri))
            {
                uri = new Uri(new Uri(BaseUri), uri).AbsoluteUri;
            }

            if (_documentCache.TryGetValue(uri, out var cached))
                return cached;

            // A resource mapper may redirect published (e.g. http:) URIs to local files;
            // the cache key remains the originally requested URI.
            var loadUri = ResourceUriMapper?.Invoke(uri) ?? uri;
            node = DocumentLoader(loadUri);
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException($"FODC0002: Document not available: {uri}");
        }
        catch (DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"FODC0002: Document not available: {uri}");
        }
        catch (UriFormatException)
        {
            throw new InvalidOperationException($"FODC0005: Invalid document URI: {uri}");
        }
        catch (IOException)
        {
            throw new InvalidOperationException($"FODC0002: Document not available: {uri}");
        }
        catch (XmlException)
        {
            throw new InvalidOperationException($"FODC0002: Document not available: {uri}");
        }

        if (DocumentPostProcessor != null)
            node = DocumentPostProcessor(node);
        _documentCache[uri] = node;
        return node;
    }

    /// <summary>
    /// Registers a document node under the supplied URI without invoking <see cref="DocumentLoader"/>.
    /// Used by XSLT to make the source document available to <c>fn:doc</c> via its document URI.
    /// </summary>
    /// <param name="uri">The absolute URI to register.</param>
    /// <param name="node">The document node to associate with the URI.</param>
    public void RegisterDocument(string uri, IXdmNode node)
    {
        if (!string.IsNullOrEmpty(uri))
            _documentCache[uri] = node;
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
                if (!SuppressLazyGlobalCaching)
                    _evaluatedLazyGlobals[(localName, namespaceUri)] = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Captures the current lazy-global cache so it can be restored later. This lets
    /// XSLT function calls isolate function-local lazy variables from the global cache.
    /// </summary>
    public IDisposable SnapshotLazyGlobals()
    {
        var saved = new Dictionary<(string LocalName, string NamespaceUri), XdmValue>(_evaluatedLazyGlobals);
        return new LazyGlobalsRestorer(this, saved);
    }

    private sealed class LazyGlobalsRestorer : IDisposable
    {
        private readonly EvaluationContext _context;
        private readonly Dictionary<(string LocalName, string NamespaceUri), XdmValue> _saved;

        public LazyGlobalsRestorer(EvaluationContext context, Dictionary<(string LocalName, string NamespaceUri), XdmValue> saved)
        {
            _context = context;
            _saved = saved;
        }

        public void Dispose()
        {
            _context._evaluatedLazyGlobals.Clear();
            foreach (var kv in _saved)
                _context._evaluatedLazyGlobals[kv.Key] = kv.Value;
        }
    }

    public bool RemoveVariable(string localName, string namespaceUri = "")
        => _variables.Remove((localName, namespaceUri));

    /// <summary>
    /// Looks up a variable in the direct variable dictionary and the evaluated lazy-global
    /// cache without invoking <see cref="LazyVariableResolver"/>. Used by the XSLT global
    /// variable resolver to avoid recursive re-entry through its own resolver.
    /// </summary>
    public bool TryGetBoundVariable(string localName, out XdmValue value, string namespaceUri = "")
    {
        if (_variables.TryGetValue((localName, namespaceUri), out value))
            return true;

        if (_evaluatedLazyGlobals.TryGetValue((localName, namespaceUri), out value))
            return true;

        return false;
    }

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
        // A zero-length URI undeclares the prefix (XQuery namespace undeclaration).
        if (namespaceUri.Length == 0 && prefix.Length > 0)
            _namespaces.Remove(prefix);
        else
            _namespaces[prefix] = namespaceUri;
        return this;
    }

    /// <summary>Removes a namespace prefix binding (used for namespace undeclarations).</summary>
    public EvaluationContext RemoveNamespace(string prefix)
    {
        _namespaces.Remove(prefix);
        return this;
    }

    // ------------------------------------------------------------------
    // Schemas
    // ------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the compiled XML Schema set available to schema-aware operations.
    /// </summary>
    public XmlSchemaSet? SchemaSet
    {
        get => _schemaSet;
        set => _schemaSet = value;
    }

    /// <summary>
    /// Looks up a global element declaration in the compiled schema set, if any.
    /// </summary>
    public XmlSchemaElement? GetSchemaElement(string namespaceUri, string localName)
        => _schemaSet?.GlobalElements[new XmlQualifiedName(localName, namespaceUri)] as XmlSchemaElement;

    /// <summary>
    /// Looks up a global attribute declaration in the compiled schema set, if any.
    /// </summary>
    public XmlSchemaAttribute? GetSchemaAttribute(string namespaceUri, string localName)
        => _schemaSet?.GlobalAttributes[new XmlQualifiedName(localName, namespaceUri)] as XmlSchemaAttribute;

    /// <summary>
    /// Looks up a global type definition in the compiled schema set, if any.
    /// </summary>
    public XmlSchemaType? GetSchemaType(string namespaceUri, string localName)
        => _schemaSet?.GlobalTypes[new XmlQualifiedName(localName, namespaceUri)] as XmlSchemaType;

    // Constructor-local namespace declarations (xmlns on enclosing constructors, applied
    // via the DeclareNamespace opcode). Elements built inside a constructor materialize
    // these bindings; the element's own declarations win (K2-InScopePrefixesFunc-18).
    private readonly List<(string Prefix, string Uri)> _constructorLocalNamespaces = new();

    /// <summary>The constructor-local namespace bindings currently in effect.</summary>
    public IReadOnlyList<(string Prefix, string Uri)> ConstructorLocalNamespaces => _constructorLocalNamespaces;

    /// <summary>The number of constructor-local bindings (for snapshot/restore).</summary>
    public int ConstructorLocalNamespaceCount => _constructorLocalNamespaces.Count;

    /// <summary>
    /// Records a constructor-local namespace declaration; an empty URI undeclares the
    /// prefix (removing any earlier local binding of the same prefix).
    /// </summary>
    public void AddConstructorLocalNamespace(string prefix, string uri)
    {
        for (int i = _constructorLocalNamespaces.Count - 1; i >= 0; i--)
        {
            if (_constructorLocalNamespaces[i].Prefix == prefix)
            {
                _constructorLocalNamespaces.RemoveAt(i);
                break;
            }
        }
        if (uri.Length > 0)
            _constructorLocalNamespaces.Add((prefix, uri));
    }

    /// <summary>Drops constructor-local bindings recorded after the given snapshot point.</summary>
    public void TruncateConstructorLocalNamespaces(int count)
    {
        if (_constructorLocalNamespaces.Count > count)
            _constructorLocalNamespaces.RemoveRange(count, _constructorLocalNamespaces.Count - count);
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
    {
        if (_functions.TryGetValue((namespaceUri, localName, arity), out signature!))
            return true;
        // Variadic fallback: a variadic signature accepts any arity >= its declared arity
        // (e.g. fn:concat#99 resolves against the variadic fn:concat registration).
        foreach (var ((ns, name, minArity), sig) in _functions)
        {
            if (ns == namespaceUri && name == localName && sig.IsVariadic && arity >= minArity)
            {
                signature = sig;
                return true;
            }
        }
        signature = null!;
        return false;
    }

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

    /// <summary>
    /// Copies the current named decimal formats for save/restore around module-context
    /// switches (a library module's decimal-format declarations apply only within that module).
    /// </summary>
    public Dictionary<(string LocalName, string NamespaceUri), DecimalFormat> SnapshotDecimalFormats()
        => new(_namedDecimalFormats);

    /// <summary>Replaces the named decimal formats with a previously snapshotted set.</summary>
    public void RestoreDecimalFormats(Dictionary<(string LocalName, string NamespaceUri), DecimalFormat> snapshot)
    {
        _namedDecimalFormats.Clear();
        foreach (var kv in snapshot)
            _namedDecimalFormats[kv.Key] = kv.Value;
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
    /// Also initializes the implicit timezone from the snapshot's offset when it has
    /// not been explicitly set.
    /// </summary>
    public DateTimeOffset CurrentDateTimeSnapshot
    {
        get
        {
            if (!_currentDateTimeSnapshot.HasValue)
            {
                var now = System.DateTimeOffset.Now;
                _currentDateTimeSnapshot = now;
                if (!_implicitTimezoneOffsetSet)
                {
                    _implicitTimezoneOffsetMinutes = (int)now.Offset.TotalMinutes;
                }
            }
            return _currentDateTimeSnapshot.Value;
        }
    }
}
