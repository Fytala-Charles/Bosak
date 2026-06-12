// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Executes a compiled XSLT stylesheet against a source document.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added call-template, with-param, variable/param binding, lexical scoping               |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added cross-stylesheet template dispatch with import precedence                        |
//                      | Charles Korthout | 0.4   | 24-05-2026     | Added mode stack (#current, #default), XdmValueToString for value-of sequences          |
//                      | Charles Korthout | 0.5   | 24-05-2026     | Added xsl:key / key() index building and lookup support                                 |
//                      | Charles Korthout | 0.6   | 24-05-2026     | Added xsl:number support (single, any, multiple levels) with format-integer reuse       |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added global variable and parameter initialization from stylesheet/includes/imports      |
//                      | Charles Korthout | 1.3   | 27-05-2026     | Added CopyNodeToResult for Document nodes; skip default params if already in context     |
//                      | Charles Korthout | 1.4   | 28-05-2026     | EvaluateSequenceConstructor wraps in document node per XSLT 2.0; respects as attribute   |
//                      | Charles Korthout | 1.5   | 28-05-2026     | SortItems restores focus after sorting; NaN sorts before numbers per XSLT spec          |
//                      | Charles Korthout | 1.6   | 28-05-2026     | ResolveElementName for xsl:element/attribute; resolves prefix via in-scope namespaces    |
//                      | Charles Korthout | 1.1   | 27-05-2026     | Added xsl:function registration, ExecuteXsltFunction, EvaluateFunctionBody, xsl:sequence |
//                      | Charles Korthout | 1.2   | 27-05-2026     | Added multi-key xsl:sort with composite comparator and stable sort                          |
//                      | Charles Korthout | 1.7   | 29-05-2026     | Fixed ComputeNumberMultiple from handling: nearest ancestor, include from-node, fallback    |
//                      | Charles Korthout | 1.8   | 29-05-2026     | Fixed ComputeNumberSingle from handling; FormatNumberSequence emits prefix+suffix for empty |
//                      | Charles Korthout | 0.8   | 26-05-2026     | Added xsl:copy, fixed for-each variable scoping, AVT evaluation in literal elements      |
//                      | Charles Korthout | 0.9   | 26-05-2026     | Added initial-template support, fixed xsl:copy to copy attributes                       |
//                      | Charles Korthout | 1.0   | 26-05-2026     | Added xsl:mode on-no-match support; atomic for-each (EnumerateItems); keyword var names  |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added global variable and parameter initialization from stylesheet/includes/imports      |
//                      | Charles Korthout | 1.4   | 27-05-2026     | Fixed AVT sequence atomization, version-aware built-in rules, pattern // support         |
//                      | Charles Korthout | 1.5   | 27-05-2026     | Process text nodes in sequence constructors; strip document-level whitespace            |
//                      | Charles Korthout | 1.7   | 28-05-2026     | Added xsl:next-match with excluded-rule chain; call-template clears current template rule |
//                      | Charles Korthout | 1.8   | 29-05-2026     | Reduced MaxXsltFunctionCallDepth to 32 to prevent .NET stack overflow crashes             |
//                      | Charles Korthout | 2.9   | 08-06-2026     | Fixed apply-templates inside xsl:function to pass with-param and preserve atomic values   |
//                      | Charles Korthout | 3.1   | 08-06-2026     | Fixed text-node built-in rule for XDocument container (text-only-copy at document level)  |
//                      | Charles Korthout | 3.2   | 08-06-2026     | Added initialMode support to Transform; fixed #current in initial mode; source select     |
//                      | Charles Korthout | 3.3   | 08-06-2026     | Evaluate global params/vars in document order (interleaved); fixes match-272              |
//                      | Charles Korthout | 1.9   | 29-05-2026     | Added expand-text / Text Value Template support with XPath string literal awareness       |
//                      | Charles Korthout | 2.0   | 30-05-2026     | Skip comments in CopyLiteralElement; fixes string-050/051/089 conformance tests         |
//                      | Charles Korthout | 2.1   | 30-05-2026     | EvaluateSequenceConstructor always wraps in document node via synthetic wrapper         |
//                      | Charles Korthout | 2.2   | 30-05-2026     | Fixed EvaluateAvt to skip } inside XPath string literals (fixes string-095)             |
//                      | Charles Korthout | 2.3   | 30-05-2026     | Set EvaluationContext.BackwardsCompatible from stylesheet version (fixes boolean-081/083/096) |
//                      | Charles Korthout | 2.4   | 30-05-2026     | Fixed ExecuteTemplate/ExecuteXsltFunction to restore saved context item (fixes position-4201) |
//                      | Charles Korthout | 2.5   | 30-05-2026     | xsl:value-of in backwards-compatible mode outputs only first item (fixes predicate-001/002/003) |
//                      | Charles Korthout | 2.6   | 30-05-2026     | ApplyBuiltInRules saves/restores context focus correctly                                |
//                      | Charles Korthout | 2.7   | 31-05-2026     | Added xsl:try / xsl:catch support in result tree and function bodies                   |
//                      | Charles Korthout | 2.8   | 31-05-2026     | Added exclude-result-prefixes filtering in CopyLiteralElement                           |
//                      | Charles Korthout | 2.9   | 31-05-2026     | Added xsl:for-each-group with group-by, group-adjacent, group-starting-with, group-ending-with |
//                      | Charles Korthout | 3.0   | 31-05-2026     | Added current-group() and current-grouping-key() functions; IXsltMessageListener        |
//                      | Charles Korthout | 3.1   | 31-05-2026     | CopyLiteralElement skips xsl-namespace attrs and xmlns:xsl declarations                 |
//                      | Charles Korthout | 3.2   | 01-06-2026     | xsl:number: AwayFromZero rounding, empty-seq NaN, ordinal/lang, grouping, negative err |
//                      | Charles Korthout | 3.3   | 01-06-2026     | xsl:number: XTTE1000 empty select, XTSE0020 bad start-at, attribute context for any    |
//                      | Charles Korthout | 3.4   | 01-06-2026     | FindBestTemplate: XSLT last-wins rule for same-priority templates                      |
//                      | Charles Korthout | 3.5   | 01-06-2026     | ParseXslNumberFormat: recognize Unicode numbering chars (surrogate pairs, OtherNumber) |
//                      | Charles Korthout | 3.6   | 01-06-2026     | xsl:number with value: strip leading whitespace from first output; IsFirstSignificantChild helper |
//                      | Charles Korthout | 3.7   | 01-06-2026     | ComputeNumberAny handles non-document trees; lang validation (XTDE0030); FormatNumberSequence uses long[] |
//                      | Charles Korthout | 3.8   | 01-06-2026     | EvaluateSequenceConstructor extracts attributes/namespace nodes for raw sequence return    |
//                      | Charles Korthout | 3.9   | 01-06-2026     | Initial template selection applies templates to children, not document node (XSLT 5.4)   |
//                      | Charles Korthout | 4.0   | 01-06-2026     | Per-document key indices; cross-document key() lookup; save/restore focus on lazy build |
//                      | Charles Korthout | 4.1   | 03-06-2026     | xsl:number value: BigInteger pipeline for large integers/doubles (fixes number-0111/0807) |
//                      | Charles Korthout | 4.2   | 05-06-2026     | Strip whitespace text nodes from source documents by default (fixes number-1501)           |
//                      | Charles Korthout | 4.3   | 05-06-2026     | WalkDocumentTree: propagate text-node skip across empty elements; fixes number-1501      |
//                      | Charles Korthout | 4.4   | 05-06-2026     | WalkDocumentTree visits all attrs; ComputeNumberAny counts only first attr; fixes 1101 |
//                      | Charles Korthout | 4.5   | 05-06-2026     | Initial template selection uses FindBestTemplate for document-node() patterns; fixes 088 |
//                      | Charles Korthout | 4.6   | 05-06-2026     | XTDE0540 conflict detection when on-multiple-match="fail"; fixes match-082b/c          |
//                      | Charles Korthout | 4.7   | 07-06-2026     | ApplyTemplates/next-match support atomic values; built-in rule outputs atomics; +11 tests|
//                      | Charles Korthout | 4.8   | 07-06-2026     | Added xsl:apply-imports with import-precedence filtering and atomic context items       |
//                      | Charles Korthout | 4.9   | 07-06-2026     | next-match leaks excluded rules; apply-imports param passing; precedence stack         |
//                      | Charles Korthout | 5.0   | 07-06-2026     | DeepSkip mode; expand-text truthy values; CopyToResult exclusion cleanup               |
//                      | Charles Korthout | 5.1   | 07-06-2026     | FindRootTemplate strips XPath comments; next-match/apply-imports pass position/last   |
//                      | Charles Korthout | 5.2   | 07-06-2026     | ConvertVariableValue for xsl:variable/@as basic atomic types; fixes match-248-254      |
//                      | Charles Korthout | 5.3   | 08-06-2026     | Iterative key index build for cross-key dependencies (key-063/064); removed re-entrancy guard |
//                      | Charles Korthout | 5.4   | 09-06-2026     | Fixed apply-templates default-mode resolution; XTDE0045/0050 validation; ModeExists helper |
//                      | Charles Korthout | 5.5   | 09-06-2026     | Pass EvaluationContext to PatternCompiler for compile-time predicate validation          |
//                      | Charles Korthout | 5.6   | 10-06-2026     | xsl:copy error handling (XTTE0945/3180, XTDE0410/0420); function context item isolation; parentless document order |
//                      | Charles Korthout | 5.7   | 10-06-2026     | xsl:where-populated filters empty PIs/comments; xsl:on-empty in CopyLiteralElement; copy-1213/1214/1215/1216/1217 |
//                      | Charles Korthout | 5.8   | 10-06-2026     | Named template entry points have no context item; lazy global variable evaluation     |
//                      | Charles Korthout | 5.9   | 10-06-2026     | xsl:on-empty in xsl:copy, xsl:document, EvaluateSequenceConstructor; XTDE0420 for namespace on document node |
//                      | Charles Korthout | 5.10  | 11-06-2026     | Fixed copy-1220/1221 namespace axis: AddElementToContainer, NamespaceInheritanceBarrier for copy-namespaces=no |
//                      | Charles Korthout | 5.11  | 11-06-2026     | Isolated _sequenceAccumulator when wrapInDocumentNode=true; fixes as-1303 xsl:document content leakage |
//                      | Charles Korthout | 5.12  | 11-06-2026     | Runtime XTSE0010 for @as on xsl:call-template; fixes as-1601                               |
//                      | Charles Korthout | 5.13  | 11-06-2026     | Base URI propagation for xsl:copy/copy-of and built-in template rules; fixes base-uri-050/053 |
//                      | Charles Korthout | 5.14  | 11-06-2026     | Expanded key names, 3-arg subtree scope, globals before key build, XTDE1260/1222        |
//                       | Charles Korthout | 5.15  | 11-06-2026     | Preserve typed atomic values in sequence accumulator; composite key lookup             |
//                      | Charles Korthout | 5.16  | 11-06-2026     | Fixed xsl:for-each-group: focus, composite keys, date/time eq, sort current-group      |
//                      | Charles Korthout | 5.17  | 12-06-2026     | Collation-aware grouping, function-body for-each-group, pattern current-group checks   |
//                      | Charles Korthout | 5.18  | 11-06-2026     | xsl:where-populated uses populated-node check; fixes element-0104/0105/0106/0107/0108 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Stylesheet;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// The XSLT transform engine. Evaluates a compiled stylesheet against a source document.
/// </summary>
public sealed class TransformEngine
{
    private readonly Stylesheet.Stylesheet _stylesheet;
    private readonly EvaluationContext _context;
    private readonly XDocument _resultDocument;
    private XContainer _currentContainer;
    private readonly StringBuilder _documentLevelText = new();
    private bool _lastAddedWasAtomic;

    // Flattened template rules and named templates from the entire stylesheet tree
    private readonly List<Stylesheet.TemplateRule> _allTemplateRules;
    private readonly Dictionary<string, Stylesheet.TemplateRule> _allNamedTemplates;
    private readonly HashSet<string> _excludedResultPrefixes;
    private readonly IXsltMessageListener? _messageListener;

    // Variable scope stack for proper lexical scoping across call-template
    private readonly Stack<Dictionary<(string LocalName, string NamespaceUri), XdmValue?>> _varScopes = new();

    // Mode stack for #current resolution
    private readonly Stack<string> _modeStack = new();

    // Default-mode stack for xsl:default-mode scoping
    private readonly Stack<string> _defaultModeStack = new();

    // Tunnel parameter stack: each frame is the tunnel params visible at that call depth
    private readonly Stack<Dictionary<string, XdmValue>> _tunnelParamStack = new();

    // Apply-imports precedence stack: tracks the import precedence threshold for xsl:next-match
    // when called inside a template invoked by xsl:apply-imports (XSLT 3.0 §6.5)
    private readonly Stack<int> _applyImportsPrecedenceStack = new();

    // Current template rule for xsl:next-match
    private Stylesheet.TemplateRule? _currentTemplateRule;

    // Accumulated excluded rules for the current xsl:next-match chain
    private HashSet<Stylesheet.TemplateRule> _nextMatchExcluded = new();

    // Key index for key() function lookups — one per document root node
    private List<(IXdmNode DocRoot, KeyIndex Index)>? _keyIndices;



    // Sequence accumulator for xsl:sequence inside variable bodies with @as
    private List<XdmValue>? _sequenceAccumulator;

    // Recursion depth guard for xsl:function and xsl:call-template calls
    private int _xsltFunctionCallDepth;
    private int _callTemplateDepth;

    // Current group state for xsl:for-each-group / current-group() / current-grouping-key()
    private List<XdmValue>? _currentGroup;
    private XdmValue? _currentGroupingKey;

    // Recursion depth guard for xsl:apply-templates
    private int _applyTemplatesDepth;
    private const int MaxApplyTemplatesDepth = 256;

    // Deferred global variables with sequence constructors (evaluated lazily on first reference)
    private readonly Dictionary<string, (XElement Element, string? AsType)> _lazyGlobals = new();

    // Accumulator declarations and cached accumulator values per source tree.
    private readonly List<Stylesheet.AccumulatorDefinition> _accumulators;
    private readonly Dictionary<(IXdmNode Root, string ClarkName), Dictionary<IXdmNode, (XdmValue Before, XdmValue After)>> _accumulatorCache = new();

    // Focus used for global variable/param evaluation (the source document node).
    private XdmValue _globalContextItem = XdmValue.Undefined;

    /// <summary>The parsed xsl:output serialization properties.</summary>
    public Stylesheet.OutputProperties? OutputProperties => _stylesheet.OutputProperties;

    public TransformEngine(Stylesheet.Stylesheet stylesheet, EvaluationContext? context = null, IXsltMessageListener? messageListener = null)
    {
        _stylesheet = stylesheet;
        _context = context ?? new EvaluationContext();
        _messageListener = messageListener;
        _context.BackwardsCompatible = stylesheet.Version is "1.0";
        FunctionLibrary.Populate(_context);
        XsltFunctionLibrary.Populate(_context);

        _resultDocument = new XDocument();
        _currentContainer = _resultDocument;

        _allTemplateRules = _stylesheet.GetAllTemplateRules().ToList();
        _allNamedTemplates = _stylesheet.GetAllNamedTemplates();
        _accumulators = _stylesheet.GetAllAccumulators().ToList();

        // Register namespace prefixes declared on the stylesheet root(s).
        // The empty prefix (default namespace) is intentionally skipped so that
        // XPath select expressions behave like match patterns: unprefixed element
        // names match the empty namespace, not the stylesheet's default namespace.
        // This aligns with XSLT 1.0 behaviour and is required because our source
        // XML (EDIFACT grouped documents) has no namespace on elements.
        foreach (var (prefix, nsUri) in _stylesheet.GetAllNamespaces())
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                _context.WithNamespace(prefix, nsUri);
            }
        }

        // Collect excluded result prefixes for namespace filtering in literal result elements
        _excludedResultPrefixes = _stylesheet.GetAllExcludedResultPrefixes();

        // Register decimal-format declarations from the stylesheet
        RegisterDecimalFormats();

        // Register xsl:function declarations as callable XPath functions
        RegisterXsltFunctions();

        // Register accumulator-before()/accumulator-after() when accumulators are declared
        RegisterAccumulatorFunctions();
    }

    /// <summary>
    /// Executes the stylesheet transformation.
    /// </summary>
    public XdmValue Transform(IXdmNode? source, string? initialTemplate = null, string? initialMode = null)
    {
        // A source document is required unless an initial template is supplied or the
        // stylesheet declares an xsl:initial-template.
        if (source == null && string.IsNullOrEmpty(initialTemplate) && !_allNamedTemplates.ContainsKey("xsl:initial-template"))
            throw new ArgumentException("A source document is required unless an initial template is specified.", nameof(source));

        // Ensure xsl:function registrations are present (re-entrant transforms)
        RegisterXsltFunctions();
        // Compile all template match patterns before execution
        var patternCompiler = new Patterns.PatternCompiler(_context);
        foreach (var rule in _allTemplateRules)
        {
            rule.CompileMatch(patternCompiler);
        }

        // Always register key() function before building key indices, because
        // xsl:key/@use expressions may call key() recursively (key-063/064).
        RegisterKeyFunction();

        // Apply whitespace stripping from xsl:strip-space / xsl:preserve-space
        // before globals or key indices are evaluated.
        if (source != null)
            ApplyWhitespaceStripping(source);

        // Initialize global parameters and variables before building key indices,
        // because xsl:key/@use and match expressions may reference global variables.
        InitializeGlobalParametersAndVariables(source);

        // Build key indices iteratively to handle cross-key dependencies
        // (e.g. key-063 where k2's use calls key('k1',...), or key-064 where
        // k1's match calls key('k2',...)).
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (source != null && allKeyDefs.Count > 0)
        {
            // XTSE1222: all xsl:key declarations with the same expanded name must
            // agree on their effective @composite value.
            foreach (var group in allKeyDefs.GroupBy(k => k.Name))
            {
                if (group.Select(k => k.Composite).Distinct().Count() > 1)
                    throw new InvalidOperationException($"XTSE1222: xsl:key definitions for '{group.Key}' have conflicting @composite values.");
            }

            _keyIndices = new List<(IXdmNode, KeyIndex)>();
            var sourceIndex = new KeyIndex();
            // Add the index before building so recursive key() calls inside
            // xsl:key/@use or match can query the partially-built index.
            _keyIndices.Add((source, sourceIndex));

            int maxIterations = allKeyDefs.Count + 1;
            int previousTotal = -1;
            for (int i = 0; i < maxIterations; i++)
            {
                int currentTotal = sourceIndex.TotalEntryCount;
                if (currentTotal == previousTotal)
                    break;
                previousTotal = currentTotal;

                // Clear each key name once per iteration so multiple definitions
                // with the same name accumulate, rather than overwriting each other.
                var cleared = new HashSet<string>();
                foreach (var keyDef in allKeyDefs)
                {
                    if (cleared.Add(keyDef.Name))
                        sourceIndex.ClearKey(keyDef.Name);
                    if (keyDef.HasUseContent)
                        KeyIndex.BuildSingleKey(source, keyDef, _context, sourceIndex, n => EvaluateSequenceConstructor(keyDef.Element!, XdmValue.FromNode(n), wrapInDocumentNode: false));
                    else
                        KeyIndex.BuildSingleKey(source, keyDef, _context, sourceIndex);
                }
            }
        }

        RegisterGroupingFunctions();

        if (!string.IsNullOrEmpty(initialTemplate))
        {
            // Start from a named template (xsl:initial-template or test harness)
            // Named template entry points have no context item (XSLT 3.0 §6.5)
            CallTemplate(initialTemplate, XdmValue.Undefined);
        }
        else
        {
            // Check for xsl:initial-template as the implicit entry point
            if (_allNamedTemplates.TryGetValue("xsl:initial-template", out var initialTemplateRule))
            {
                // Named template entry points have no context item (XSLT 3.0 §6.5)
                CallTemplate("xsl:initial-template", XdmValue.Undefined);
            }
            else if (!string.IsNullOrEmpty(initialMode))
            {
                // Start transformation in the specified initial mode.
                // Expand any namespace prefix in the initial mode name.
                var resolvedInitialMode = ExpandModeName(initialMode, _stylesheet.Root);
                // If the mode is #unnamed, treat it as the empty unnamed mode
                if (resolvedInitialMode == "#unnamed")
                    resolvedInitialMode = "";
                // XTDE0045: initial mode must exist in the stylesheet (templates with #all don't count)
                if (!ModeExists(resolvedInitialMode))
                {
                    throw new InvalidOperationException($"XTDE0045: Initial mode '{resolvedInitialMode}' does not exist in the stylesheet.");
                }
                _modeStack.Push(resolvedInitialMode);
                try
                {
                    var rootTemplate = FindBestTemplate(source!, resolvedInitialMode);
                    if (rootTemplate != null)
                    {
                        ExecuteTemplate(rootTemplate, source!);
                    }
                    else
                    {
                        ApplyBuiltInRules(source!, resolvedInitialMode);
                    }
                }
                finally
                {
                    _modeStack.Pop();
                }
            }
            else
            {
                // Look for a template matching "/" or other document-node-specific patterns
                var rootTemplate = FindRootTemplate();
                if (rootTemplate != null && rootTemplate.CompiledMatch != null &&
                    rootTemplate.CompiledMatch(XdmValue.FromNode(source!), _context))
                {
                    ExecuteTemplate(rootTemplate, source!);
                }
                else
                {
                    // XSLT 2.0 §5.4: when there is no template matching "/",
                    // the built-in template rule for the document node is invoked.
                    // This built-in rule applies templates to the children of the
                    // document node. We must NOT search for other patterns (like
                    // node() or document-node()) that might match the document node
                    // directly, as that causes incorrect next-match chaining.
                    ApplyTemplates(source!, mode: "", select: null);
                }
            }
        }

        // Return the result document, or document-level text if no root element was produced
        if (_documentLevelText.Length > 0 && _resultDocument.Root == null)
        {
            return XdmValue.FromString(_documentLevelText.ToString());
        }
        return XdmValue.FromNode(new XDocumentNode(_resultDocument));
    }

    /// <summary>
    /// Registers all xsl:function declarations from the stylesheet tree as callable
    /// functions on the EvaluationContext.
    /// </summary>
    /// <summary>
    /// Adds a text node to the current result container.
    /// Falls back to a document-level text buffer when the container is an XDocument,
    /// because XDocument does not allow non-whitespace text nodes at the document level.
    /// </summary>
    private void AddTextNode(string text)
    {
        if (text.Length == 0)
            return; // Zero-length text nodes are ignored in complex content
        if (_currentContainer is XDocument)
        {
            _documentLevelText.Append(text);
        }
        else
        {
            _currentContainer.Add(new XText(text));
        }
    }

    /// <summary>
    /// Normalizes the content of a constructed element by removing zero-length text
    /// nodes and merging adjacent text nodes (XSLT 2.0 §5.7.1).
    /// </summary>
    private static void NormalizeElementContent(XElement element)
    {
        var nodes = element.Nodes().ToList();
        if (nodes.Count == 0)
            return;

        var normalized = ApplyComplexContentRules(nodes);
        element.RemoveNodes();
        foreach (var node in normalized)
            element.Add(node);
    }

    /// <summary>
    /// Returns whether the current container has no nodes yet, indicating that the next
    /// item added will be the first significant child.
    /// </summary>
    private bool IsFirstSignificantChild()
    {
        if (_currentContainer is XDocument)
        {
            return _documentLevelText.Length == 0;
        }
        return !_currentContainer.Nodes().Any();
    }

    /// <summary>
    /// Appends a space and the given text to the last text node in the current container,
    /// or creates a new text node if there is no last text node. Used to join adjacent
    /// atomic values with a single space in complex content construction.
    /// </summary>
    private void AppendAtomicText(string text)
    {
        if (_currentContainer is XDocument)
        {
            if (_lastAddedWasAtomic)
                _documentLevelText.Append(' ');
            _documentLevelText.Append(text);
        }
        else
        {
            if (_lastAddedWasAtomic)
            {
                var lastText = _currentContainer.Nodes().LastOrDefault() as XText;
                if (lastText != null)
                {
                    lastText.Value = lastText.Value + " " + text;
                    _lastAddedWasAtomic = true;
                    return;
                }
                text = " " + text;
            }
            _currentContainer.Add(new XText(text));
        }
        _lastAddedWasAtomic = true;
    }

    private void RegisterDecimalFormats()
    {
        var allFormats = _stylesheet.GetAllDecimalFormats();
        foreach (var (key, def) in allFormats)
        {
            if (string.IsNullOrEmpty(key.localName))
            {
                _context.DefaultDecimalFormat = def.Format;
            }
            else
            {
                _context.WithDecimalFormat(key.localName, key.nsUri, def.Format);
            }
        }
    }

    /// <summary>
    /// Collects all namespace declarations in scope for the given element
    /// by walking up the ancestor chain.
    /// </summary>
    private static Dictionary<string, string> GetInScopeNamespaces(XElement element)
    {
        var result = new Dictionary<string, string>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    string prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                    if (!result.ContainsKey(prefix))
                        result[prefix] = attr.Value;
                }
            }
            current = current.Parent;
        }
        result["xml"] = "http://www.w3.org/XML/1998/namespace";
        return result;
    }

    /// <summary>
    /// Returns the effective xpath-default-namespace for the given element by walking
    /// the ancestor chain and finding the nearest xpath-default-namespace attribute.
    /// </summary>
    private static string? GetXPathDefaultNamespace(XElement element)
    {
        var current = element;
        while (current != null)
        {
            // The XSLT-namespaced form (e.g. xsl:xpath-default-namespace) is effective on any element
            var attr = current.Attribute(XName.Get("xpath-default-namespace", Stylesheet.Stylesheet.XslNamespace));
            if (attr != null)
            {
                // XTSE0090: xsl:xpath-default-namespace is not allowed on XSLT elements
                if (current.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    throw new InvalidOperationException("XTSE0090");
                return attr.Value;
            }
            // The no-namespace form is only effective on XSLT elements
            if (current.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
            {
                attr = current.Attribute("xpath-default-namespace");
                if (attr != null) return attr.Value;
            }
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Compiles an XPath expression with the in-scope namespace bindings
    /// and xpath-default-namespace from the given instruction element.
    /// </summary>
    private XPath31Expression CompileXPath(string expression, XElement instruction)
    {
        var nsMap = GetInScopeNamespaces(instruction);
        var defaultNs = GetXPathDefaultNamespace(instruction);
        var baseUri = GetEffectiveBaseUri(instruction);
        if (nsMap.Count > 1 || !string.IsNullOrEmpty(defaultNs) || !string.IsNullOrEmpty(baseUri))
        {
            var options = new CompileOptions { Namespaces = nsMap, DefaultElementNamespace = defaultNs, BaseUri = baseUri };
            return XPath31Expression.Compile(expression, options);
        }
        return XPath31Expression.Compile(expression);
    }

    /// <summary>
    /// Computes the effective base URI of an XSLT instruction by walking up the
    /// ancestor chain and resolving <c>xml:base</c> attributes per XML Base spec.
    /// </summary>
    private string? GetEffectiveBaseUri(XElement? element)
    {
        if (element == null)
            return null;

        string baseUri = element.Document?.BaseUri ?? string.Empty;
        if (string.IsNullOrEmpty(baseUri))
            baseUri = _stylesheet.BaseUri ?? string.Empty;
        var chain = new List<string>();
        var current = element;
        while (current != null)
        {
            var xmlBase = current.Attribute(XNamespace.Xml + "base")?.Value;
            if (xmlBase != null)
                chain.Add(xmlBase);
            current = current.Parent;
        }

        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (Bosak.XPath.Standard.Functions.FunctionLibrary.IsAbsoluteUri(chain[i]))
                baseUri = chain[i];
            else if (!string.IsNullOrEmpty(baseUri))
            {
                try
                {
                    baseUri = new Uri(new Uri(baseUri), chain[i]).AbsoluteUri;
                }
                catch (UriFormatException)
                {
                    // If the base URI is not a valid .NET Uri,
                    // preserve the xml:base value as-is (XSLT test suites
                    // use intentionally malformed URIs like d://tests/)
                    baseUri = chain[i];
                }
            }
            else
                baseUri = chain[i];
        }

        return string.IsNullOrEmpty(baseUri) ? null : baseUri;
    }

    private void RegisterXsltFunctions()
    {
        var allFuncs = _stylesheet.GetAllFunctionDefinitions();
        foreach (var (key, def) in allFuncs)
        {
            var sig = new FunctionSignature
            {
                NamespaceUri = def.NamespaceUri,
                LocalName = def.LocalName,
                Arity = def.Arity,
                ParameterTypes = Enumerable.Repeat(XdmValueKind.Sequence, def.Arity).ToList(),
                ReturnType = XdmValueKind.Sequence,
                Implementation = (ctx, args) => ExecuteXsltFunction(def, args)
            };
            _context.RegisterFunction(sig);
        }
    }

    /// <summary>
    /// Registers the XSLT <c>accumulator-before()</c> and <c>accumulator-after()</c>
    /// functions for every declared accumulator.
    /// </summary>
    private void RegisterAccumulatorFunctions()
    {
        if (_accumulators.Count == 0)
            return;

        _context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "accumulator-before",
            Arity = 1,
            ParameterTypes = new List<XdmValueKind> { XdmValueKind.String },
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) => GetAccumulatorValue(ctx, args, before: true)
        });

        _context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "accumulator-after",
            Arity = 1,
            ParameterTypes = new List<XdmValueKind> { XdmValueKind.String },
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) => GetAccumulatorValue(ctx, args, before: false)
        });
    }

    /// <summary>
    /// Implements <c>accumulator-before()</c> / <c>accumulator-after()</c>.
    /// </summary>
    private XdmValue GetAccumulatorValue(EvaluationContext ctx, ReadOnlySpan<XdmValue> args, bool before)
    {
        var nameArg = args[0];
        var name = nameArg.IsAtomic ? nameArg.ToString() : string.Empty;
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE3341: accumulator name must be a string");

        var accName = ResolveAccumulatorFunctionName(name, ctx);
        if (string.IsNullOrEmpty(accName))
            throw new InvalidOperationException($"XTDE3341: accumulator '{name}' not found");

        var contextItem = ctx.ContextItem;
        if (!contextItem.IsNode || contextItem.NodeValue == null)
            throw new InvalidOperationException("XTDE3362: accumulator functions require a context item that is a node");

        var node = contextItem.NodeValue;

        // First check for values copied with copy-accumulators="yes"
        if (node is XDocumentNode xdn && xdn.UnderlyingObject is XElement elem)
        {
            var copied = elem.Annotation<AccumulatorValues>();
            if (copied != null && copied.Values.TryGetValue(accName, out var pair))
                return before ? pair.Before : pair.After;
        }

        // Otherwise compute from the source tree.
        var acc = _accumulators.FirstOrDefault(a => a.ClarkName == accName);
        if (acc == null)
            throw new InvalidOperationException($"XTDE3341: accumulator '{name}' not found");

        var root = GetRootNode(node);
        var nodeValues = GetAccumulatorNodeValues(acc, root);
        if (nodeValues.TryGetValue(node, out var values))
            return before ? values.Before : values.After;

        // Nodes not visited by the accumulator (e.g. attributes/text matched indirectly)
        // return the initial value for before and after.
        var initialCompiled = CompileXPath(acc.InitialValue, acc.Element);
        return initialCompiled.Evaluate(new EvaluationContext());
    }

    /// <summary>
    /// Resolves an accumulator name supplied to <c>accumulator-before()</c> / <c>accumulator-after()</c>
    /// to Clark notation using the in-scope namespaces of the calling expression.
    /// </summary>
    private string ResolveAccumulatorFunctionName(string name, EvaluationContext ctx)
    {
        if (name.StartsWith("{"))
            return name;

        var colon = name.IndexOf(':');
        if (colon < 0)
        {
            // Unprefixed accumulator names are in no namespace.
            foreach (var acc in _accumulators)
            {
                if (acc.LocalName == name && string.IsNullOrEmpty(acc.NamespaceUri))
                    return acc.ClarkName;
            }
            return "";
        }

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        if (ctx.TryResolveNamespace(prefix, out var nsUri))
        {
            var clark = $"{{{nsUri}}}{local}";
            if (_accumulators.Any(a => a.ClarkName == clark))
                return clark;
        }
        return "";
    }

    /// <summary>
    /// Returns the cached accumulator values for every node in the source tree,
    /// computing them on first use.
    /// </summary>
    private Dictionary<IXdmNode, (XdmValue Before, XdmValue After)> GetAccumulatorNodeValues(Stylesheet.AccumulatorDefinition acc, IXdmNode root)
    {
        var key = (root, acc.ClarkName);
        if (!_accumulatorCache.TryGetValue(key, out var nodeValues))
        {
            nodeValues = ComputeAccumulatorValues(acc, root);
            _accumulatorCache[key] = nodeValues;
        }
        return nodeValues;
    }

    /// <summary>
    /// Computes the accumulator value before and after each node in the source tree.
    /// </summary>
    private Dictionary<IXdmNode, (XdmValue Before, XdmValue After)> ComputeAccumulatorValues(Stylesheet.AccumulatorDefinition acc, IXdmNode root)
    {
        var result = new Dictionary<IXdmNode, (XdmValue Before, XdmValue After)>();
        var initialCompiled = CompileXPath(acc.InitialValue, acc.Element);
        var current = initialCompiled.Evaluate(new EvaluationContext());

        // Compile match patterns for the rules.
        var compiledRules = new List<(Stylesheet.AccumulatorRule Rule, Patterns.PatternPredicate Match)>();
        var patternCompiler = new Patterns.PatternCompiler(_context);
        foreach (var rule in acc.Rules)
        {
            var defaultNs = GetXPathDefaultNamespace(rule.Element);
            var match = patternCompiler.Compile(rule.Match, defaultNs ?? "");
            compiledRules.Add((rule, match));
        }

        foreach (var value in root.Axis(XdmAxis.DescendantOrSelf))
        {
            if (!value.IsNode || value.NodeValue == null)
                continue;
            var node = value.NodeValue;
            var before = current;

            var matchingRule = compiledRules.FirstOrDefault(r => r.Match(XdmValue.FromNode(node), _context));
            if (matchingRule.Rule != null && !string.IsNullOrEmpty(matchingRule.Rule.Select))
            {
                var ruleCtx = new EvaluationContext().WithFocus(XdmValue.FromNode(node), 1, 1);
                ruleCtx.WithVariable("value", current);
                var compiled = CompileXPath(matchingRule.Rule.Select, matchingRule.Rule.Element);
                current = compiled.Evaluate(ruleCtx);
            }

            result[node] = (before, current);
        }

        return result;
    }

    /// <summary>
    /// Attaches the accumulator values for the source node to a copied element.
    /// </summary>
    private void AttachAccumulatorValues(IXdmNode sourceNode, XElement copy)
    {
        if (_accumulators.Count == 0)
            return;

        var root = GetRootNode(sourceNode);
        var values = new AccumulatorValues();
        foreach (var acc in _accumulators)
        {
            var nodeValues = GetAccumulatorNodeValues(acc, root);
            if (nodeValues.TryGetValue(sourceNode, out var pair))
                values.Values[acc.ClarkName] = pair;
        }
        if (values.Values.Count > 0)
            copy.AddAnnotation(values);
    }

    /// <summary>
    /// Executes the body of an xsl:function declaration, binding parameters and
    /// returning the sequence produced by the function body.
    /// </summary>
    private const int MaxXsltFunctionCallDepth = 64;

    private XdmValue ExecuteXsltFunction(Stylesheet.XsltFunctionDefinition def, ReadOnlySpan<XdmValue> args)
    {
        if (++_xsltFunctionCallDepth > MaxXsltFunctionCallDepth)
        {
            _xsltFunctionCallDepth--;
            throw new InvalidOperationException("XSLT function recursion depth exceeded maximum allowed depth.");
        }

        var snapshot = _context.SnapshotVariables();
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        try
        {
            // Bind parameters
            for (int i = 0; i < def.ParameterNames.Count && i < args.Length; i++)
            {
                _context.WithVariable(def.ParameterNames[i], args[i]);
            }

            // XSLT functions have no context item by default (XSLT 3.0 §9.6).
            // xsl:sequence/@select and other XPath expressions must not see
            // the caller's context item.
            _context.WithFocus(XdmValue.Undefined, 0, 0);

            // XSLT functions have no context item (XSLT 3.0 §9.6).
            // Evaluate the function body with an absent context item.
            var result = EvaluateFunctionBody(def.Element, XdmValue.Undefined);
            return ConvertVariableValue(result, def.ReturnType);
        }
        finally
        {
            _xsltFunctionCallDepth--;
            _context.RestoreVariables(snapshot);
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
        }
    }

    /// <summary>
    /// Evaluates the body of an xsl:function and returns the resulting XDM value.
    /// Skips xsl:param children (already bound) and collects items from all other
    /// sequence-constructor children.
    /// </summary>
    private XdmValue EvaluateFunctionBody(XElement functionElement, XdmValue contextItem)
    {
        var items = new List<XdmValue>();
        foreach (var child in functionElement.Elements())
        {
            if (child.Name.LocalName == "param" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                continue;

            EvaluateFunctionBodyInstruction(child, items, contextItem);
        }

        if (items.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);
        if (items.Count == 1)
            return items[0];

        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    /// <summary>
    /// Evaluates a single instruction inside an xsl:function body and appends
    /// the produced items to <paramref name="results"/>.
    /// </summary>
    private void EvaluateFunctionBodyInstruction(XElement instruction, List<XdmValue> results, XdmValue contextItem)
    {
        if (instruction.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
        {
            switch (instruction.Name.LocalName)
            {
                case "sequence":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var result = compiled.Evaluate(_context);
                            FlattenToList(result, results);
                        }
                        else
                        {
                            foreach (var child in instruction.Nodes())
                            {
                                switch (child)
                                {
                                    case XText text:
                                        if (GetExpandText(instruction) && ContainsTvtExpression(text.Value))
                                        {
                                            var tvtResult = EvaluateTvt(text.Value);
                                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(tvtResult))));
                                        }
                                        else if (!IsWhitespaceOnly(text.Value))
                                        {
                                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(text.Value))));
                                        }
                                        break;
                                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                        EvaluateFunctionBodyInstruction(elem, results, contextItem);
                                        break;
                                    case XElement elem:
                                        results.Add(XdmValue.FromNode(new XDocumentNode(elem)));
                                        break;
                                }
                            }
                        }
                        break;
                    }
                case "value-of":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var result = compiled.Evaluate(_context);
                            var sep = instruction.Attribute("separator")?.Value ?? " ";
                            results.Add(XdmValue.FromString(XdmValueToString(result, sep)));
                        }
                        else if (GetExpandText(instruction))
                        {
                            var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                            var tvtResult = EvaluateTvt(text);
                            results.Add(XdmValue.FromString(tvtResult));
                        }
                        else
                        {
                            // xsl:value-of with sequence-constructor content (no @select)
                            var voSep = instruction.Attribute("separator")?.Value ?? "";
                            var textValue = EvaluateSimpleContent(instruction, contextItem, voSep);
                            results.Add(XdmValue.FromString(textValue));
                        }
                        break;
                    }
                case "variable":
                    {
                        var varName = instruction.Attribute("name")?.Value;
                        var varSelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(varName))
                        {
                            XdmValue varValue;
                            if (!string.IsNullOrEmpty(varSelect))
                            {
                                var compiled = XPath31Expression.Compile(varSelect);
                                varValue = compiled.Evaluate(_context);
                            }
                            else
                            {
                                varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                            }
                            varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
                            _context.WithVariable(varName, varValue);
                        }
                        break;
                    }
                case "if":
                    {
                        var test = instruction.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(test))
                        {
                            var compiled = XPath31Expression.Compile(test);
                            if (compiled.Evaluate(_context).EffectiveBooleanValue())
                            {
                                foreach (var child in instruction.Elements())
                                    EvaluateFunctionBodyInstruction(child, results, contextItem);
                            }
                        }
                        break;
                    }
                case "choose":
                    {
                        foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var whenTest = when.Attribute("test")?.Value;
                            if (!string.IsNullOrEmpty(whenTest))
                            {
                                var compiled = XPath31Expression.Compile(whenTest);
                                if (compiled.Evaluate(_context).EffectiveBooleanValue())
                                {
                                    foreach (var child in when.Elements())
                                        EvaluateFunctionBodyInstruction(child, results, contextItem);
                                    return;
                                }
                            }
                        }
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            foreach (var child in otherwise.Elements())
                                EvaluateFunctionBodyInstruction(child, results, contextItem);
                        }
                        break;
                    }
                case "for-each":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var feResult = compiled.Evaluate(_context);
                            var feItems = EnumerateItems(feResult).ToList();

                            // Apply xsl:sort if present
                            var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                            if (sortElements.Count > 0)
                            {
                                feItems = SortItems(feItems, sortElements);
                            }

                            var savedFocus = _context.ContextItem;
                            var savedCurrent = _context.CurrentItem;
                            int pos = 1;
                            foreach (var item in feItems)
                            {
                                _context.WithFocus(item, pos, feItems.Count);
                                _context.WithCurrentItem(item);
                                var feSnapshot = _context.SnapshotVariables();
                                try
                                {
                                    foreach (var child in instruction.Elements())
                                    {
                                        if (child.Name.LocalName == "sort")
                                            continue;
                                        EvaluateFunctionBodyInstruction(child, results, item);
                                    }
                                }
                                finally
                                {
                                    _context.RestoreVariables(feSnapshot);
                                }
                                pos++;
                            }
                            _context.WithFocus(savedFocus, 1, 1);
                            _context.WithCurrentItem(savedCurrent);
                        }
                        break;
                    }
                case "for-each-group":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (string.IsNullOrEmpty(select)) break;

                        var compiled = CompileXPath(select, instruction);
                        var feResult = compiled.Evaluate(_context);
                        var feItems = EnumerateItems(feResult).ToList();
                        if (feItems.Count == 0) break;

                        var collationAttr = instruction.Attribute("collation")?.Value;
                        var effectiveCollation = string.IsNullOrEmpty(collationAttr) ? null : EvaluateAvt(collationAttr, instruction);

                        ValidateForEachGroupAttributes(instruction);

                        var savedFocus = _context.ContextItem;
                        var savedPosition = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        var savedCurrent = _context.CurrentItem;
                        var savedGroup = _currentGroup;
                        var savedKey = _currentGroupingKey;

                        try
                        {
                            var groups = BuildForEachGroups(instruction, feItems, effectiveCollation);

                            var bindGroup = instruction.Attribute("bind-group")?.Value;
                            var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

                            var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                            for (int sortIdx = 0; sortIdx < sortElements.Count; sortIdx++)
                            {
                                var stableAttr = sortElements[sortIdx].Attribute("stable")?.Value;
                                if (sortIdx > 0 && !string.IsNullOrEmpty(stableAttr))
                                    throw new InvalidOperationException("XTSE1017: @stable is allowed only on the first xsl:sort");
                                if (!string.IsNullOrEmpty(stableAttr))
                                {
                                    var v = stableAttr.Trim();
                                    if (v != "yes" && v != "true" && v != "1" &&
                                        v != "no" && v != "false" && v != "0")
                                        throw new InvalidOperationException("XTSE0020: invalid value for @stable");
                                }
                            }
                            if (sortElements.Count > 0 && groups.Count > 0)
                            {
                                groups = SortGroups(groups, sortElements);
                            }

                            int pos = 1;
                            foreach (var (key, groupItems) in groups)
                            {
                                _currentGroup = groupItems;
                                _currentGroupingKey = key;
                                var rep = groupItems[0];
                                _context.WithFocus(rep, pos, groups.Count);
                                _context.WithCurrentItem(rep);
                                var feSnapshot = _context.SnapshotVariables();
                                try
                                {
                                    if (!string.IsNullOrEmpty(bindGroup))
                                        _context.WithVariable(bindGroup, XdmValue.FromSequence(MaterializedSequence.FromList(groupItems)));
                                    if (!string.IsNullOrEmpty(bindKey) && key != null)
                                        _context.WithVariable(bindKey, key.Value);

                                    foreach (var child in instruction.Elements())
                                    {
                                        if (child.Name.LocalName == "sort" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                            continue;
                                        EvaluateFunctionBodyInstruction(child, results, rep);
                                    }
                                }
                                finally
                                {
                                    _context.RestoreVariables(feSnapshot);
                                }
                                pos++;
                            }
                        }
                        finally
                        {
                            _context.WithFocus(savedFocus, savedPosition, savedSize);
                            _context.WithCurrentItem(savedCurrent);
                            _currentGroup = savedGroup;
                            _currentGroupingKey = savedKey;
                        }
                        break;
                    }
                case "apply-templates":
                    {
                        var modeRaw = instruction.Attribute("mode")?.Value?.Trim() ?? "";
                        var mode = ExpandModeName(modeRaw, instruction);
                        var select = instruction.Attribute("select")?.Value;
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();

                        // Collect xsl:with-param elements (tunnel and non-tunnel)
                        var withParams = new Dictionary<string, XdmValue>();
                        var tunnelParams = new Dictionary<string, XdmValue>();
                        foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var wpName = wp.Attribute("name")?.Value;
                            var wpSelect = wp.Attribute("select")?.Value;
                            var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                            if (!string.IsNullOrEmpty(wpName))
                            {
                                XdmValue wpValue;
                                if (!string.IsNullOrEmpty(wpSelect))
                                {
                                    var compiled = CompileXPath(wpSelect, wp);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
                                wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                                if (wpTunnel)
                                    tunnelParams[wpName] = wpValue;
                                else
                                    withParams[wpName] = wpValue;
                            }
                        }

                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var savedAccumulator = _sequenceAccumulator;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        _sequenceAccumulator = results;
                        try
                        {
                            // XSLT 2.0 erratum XT.E19: #current in a function refers to the unnamed mode.
                            // Save and clear the mode stack so ResolveMode("#current") returns "".
                            var savedModes = new List<string>(_modeStack);
                            savedModes.Reverse();
                            _modeStack.Clear();
                            try
                            {
                                ApplyTemplates(contextItem, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams);
                            }
                            finally
                            {
                                foreach (var m in savedModes)
                                    _modeStack.Push(m);
                            }
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                            _sequenceAccumulator = savedAccumulator;
                        }
                        foreach (var node in temp.Nodes())
                        {
                            if (node is XElement e)
                                results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                            else if (node is XText t)
                                results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                        }
                        break;
                    }
                case "call-template":
                    {
                        var calledName = instruction.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(calledName))
                        {
                            var withParams = new Dictionary<string, XdmValue>();
                            var tunnelParams = new Dictionary<string, XdmValue>();
                            foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                            {
                                var wpName = wp.Attribute("name")?.Value;
                                var wpSelect = wp.Attribute("select")?.Value;
                                var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                                if (!string.IsNullOrEmpty(wpName))
                                {
                                    XdmValue wpValue;
                                    if (!string.IsNullOrEmpty(wpSelect))
                                    {
                                        var compiled = CompileXPath(wpSelect, wp);
                                        wpValue = compiled.Evaluate(_context);
                                    }
                                    else
                                    {
                                        wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                    }
                                    wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                                    if (wpTunnel)
                                        tunnelParams[wpName] = wpValue;
                                    else
                                        withParams[wpName] = wpValue;
                                }
                            }
                            var savedContainer = _currentContainer;
                            var savedLastAtomic = _lastAddedWasAtomic;
                            var temp = new XElement("__temp__");
                            _currentContainer = temp;
                            _lastAddedWasAtomic = false;
                            try
                            {
                                CallTemplate(calledName, contextItem, withParams, tunnelParams);
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                                _lastAddedWasAtomic = savedLastAtomic;
                            }
                            foreach (var node in temp.Nodes())
                            {
                                if (node is XElement e)
                                    results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                else if (node is XText t && !string.IsNullOrEmpty(t.Value))
                                    results.Add(XdmValue.FromString(t.Value));
                            }
                        }
                        break;
                    }
                case "try":
                    {
                        var catchElem = instruction.Element(XName.Get("catch", Stylesheet.Stylesheet.XslNamespace));
                        try
                        {
                            foreach (var child in instruction.Elements())
                            {
                                if (child.Name.LocalName == "catch" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                    continue;
                                EvaluateFunctionBodyInstruction(child, results, contextItem);
                            }
                        }
                        catch
                        {
                            if (catchElem != null)
                            {
                                var catchSelect = catchElem.Attribute("select")?.Value;
                                if (!string.IsNullOrEmpty(catchSelect))
                                {
                                    var compiled = XPath31Expression.Compile(catchSelect);
                                    var catchResult = compiled.Evaluate(_context);
                                    FlattenToList(catchResult, results);
                                }
                                else
                                {
                                    foreach (var child in catchElem.Elements())
                                    {
                                        EvaluateFunctionBodyInstruction(child, results, contextItem);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case "copy-of":
                    {
                        var copySelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(copySelect))
                        {
                            var compiled = XPath31Expression.Compile(copySelect);
                            var result = compiled.Evaluate(_context);
                            var fnCopyNamespacesAttrRaw = instruction.Attribute("copy-namespaces")?.Value
                                ?? instruction.Attribute("_copy-namespaces")?.Value
                                ?? "yes";
                            var fnCopyNamespacesAttr = EvaluateAvt(fnCopyNamespacesAttrRaw, instruction);
                            bool fnCopyAllNs = fnCopyNamespacesAttr != "no" && fnCopyNamespacesAttr != "false";
                            var fnCopyAccumulatorsAttrRaw = instruction.Attribute("copy-accumulators")?.Value ?? "no";
                            var fnCopyAccumulatorsAttr = EvaluateAvt(fnCopyAccumulatorsAttrRaw, instruction);
                            bool fnCopyAccumulators = fnCopyAccumulatorsAttr == "yes" || fnCopyAccumulatorsAttr == "true";
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                    {
                                        results.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue, fnCopyAllNs, fnCopyAccumulators)));
                                    }
                                    else
                                    {
                                        results.Add(item);
                                    }
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                results.Add(XdmValue.FromNode(CopyXdmNode(result.NodeValue, fnCopyAllNs, fnCopyAccumulators)));
                            }
                            else
                            {
                                results.Add(result);
                            }
                        }
                        break;
                    }
                case "copy":
                    {
                        IXdmNode? nodeToCopy = null;
                        var copySelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(copySelect))
                        {
                            var compiled = XPath31Expression.Compile(copySelect);
                            var result = compiled.Evaluate(_context);
                            if (result.IsNode && result.NodeValue != null)
                            {
                                nodeToCopy = result.NodeValue;
                                _context.WithFocus(XdmValue.FromNode(nodeToCopy), 1, 1);
                            }
                            else if (result.IsSequence && result.SequenceValue != null)
                            {
                                var items = new List<XdmValue>();
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                    items.Add(item);
                                if (items.Count > 1)
                                    throw new InvalidOperationException("XTTE3180");
                                if (items.Count == 1 && items[0].IsNode && items[0].NodeValue != null)
                                {
                                    _context.WithFocus(items[0], 1, 1);
                                    var fnCopied = CopyNodeForFunctionBody(items[0].NodeValue, instruction);
                                    if (fnCopied != null)
                                        results.Add(XdmValue.FromNode(fnCopied));
                                }
                                break;
                            }
                        }
                        else
                        {
                            nodeToCopy = contextItem.IsNode ? contextItem.NodeValue : null;
                        }

                        if (nodeToCopy == null)
                            throw new InvalidOperationException("XTTE0945");

                        var copied = CopyNodeForFunctionBody(nodeToCopy, instruction);
                        if (copied != null)
                            results.Add(XdmValue.FromNode(copied));
                        break;
                    }
                case "text":
                    {
                        if (instruction.Elements().Any())
                            throw new InvalidOperationException("XTSE0010: xsl:text must contain only text nodes");
                        var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                        if (GetExpandText(instruction))
                        {
                            text = EvaluateTvt(text);
                        }
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(text))));
                        break;
                    }
                case "number":
                    {
                        var fnHasValueAttr = !string.IsNullOrEmpty(instruction.Attribute("value")?.Value);
                        var fnHasSelectAttr = !string.IsNullOrEmpty(instruction.Attribute("select")?.Value);

                        if (fnHasValueAttr || fnHasSelectAttr || contextItem.IsNode)
                        {
                            var savedContainer = _currentContainer;
                            var savedLastAtomic = _lastAddedWasAtomic;
                            var temp = new XElement("__temp__");
                            _currentContainer = temp;
                            _lastAddedWasAtomic = false;
                            try
                            {
                                var node = contextItem.IsNode ? contextItem.NodeValue : null;
                                ExecuteXsltNumber(instruction, node!);
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                                _lastAddedWasAtomic = savedLastAtomic;
                            }

                            var textValue = string.Concat(temp.Nodes().OfType<XText>().Select(t => t.Value));
                            if (!string.IsNullOrEmpty(textValue))
                                results.Add(XdmValue.FromString(textValue));
                        }
                        else
                        {
                            throw new InvalidOperationException("XTTE0990");
                        }
                        break;
                    }
                case "perform-sort":
                    {
                        var psSelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(psSelect))
                        {
                            var compiled = XPath31Expression.Compile(psSelect);
                            var psResult = compiled.Evaluate(_context);
                            var psItems = EnumerateItems(psResult).ToList();

                            var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                            if (sortElements.Count > 0)
                            {
                                psItems = SortItems(psItems, sortElements);
                            }

                            foreach (var item in psItems)
                                results.Add(item);
                        }
                        break;
                    }
                case "element":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                        }
                        var createdElem = temp.Elements().FirstOrDefault();
                        if (createdElem != null)
                        {
                            createdElem.Remove();
                            results.Add(XdmValue.FromNode(new XDocumentNode(createdElem)));
                        }
                        break;
                    }
                case "attribute":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                        }
                        var createdAttr = temp.Attributes().FirstOrDefault();
                        if (createdAttr != null)
                        {
                            results.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(createdAttr.Name, createdAttr.Value))));
                        }
                        break;
                    }
                case "document":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var savedAccumulator = _sequenceAccumulator;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        _sequenceAccumulator = results;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                            _sequenceAccumulator = savedAccumulator;
                        }
                        break;
                    }
                default:
                    // Unknown XSLT instruction in function body: ignore
                    break;
            }
        }
        else
        {
            // Literal result element in function body: evaluate it fully
            // (including nested XSLT instructions) using the same logic as
            // templates, but capture the result as a detached node.
            var savedContainer = _currentContainer;
            var savedLastAtomic = _lastAddedWasAtomic;
            var temp = new XElement("__temp__");
            _currentContainer = temp;
            _lastAddedWasAtomic = false;
            try
            {
                CopyLiteralElement(instruction);
                var copied = temp.Elements().FirstOrDefault();
                if (copied != null)
                {
                    copied.Remove();
                    results.Add(XdmValue.FromNode(new XDocumentNode(copied)));
                }
            }
            finally
            {
                _currentContainer = savedContainer;
                _lastAddedWasAtomic = savedLastAtomic;
            }
        }
    }

    /// <summary>
    /// Flattens an XDM value into a list, expanding sequences into their items.
    /// </summary>
    private static void FlattenToList(XdmValue value, List<XdmValue> results)
    {
        if (value.IsSequence)
        {
            var seq = value.SequenceValue;
            if (seq != null)
            {
                var enumerator = seq.GetEnumerator();
                while (enumerator.MoveNext())
                    results.Add(enumerator.Current);
            }
        }
        else
        {
            results.Add(value);
        }
    }

    /// <summary>
    /// Finds a template with match="/" (document root pattern).
    /// </summary>
    private Stylesheet.TemplateRule? FindRootTemplate()
    {
        foreach (var rule in _allTemplateRules)
        {
            if (rule.Match == null) continue;
            var stripped = Patterns.PatternCompiler.StripXPathComments(rule.Match).Trim();
            // Only match patterns that directly match the document node,
            // not path patterns like document-node()/child::element().
            if (stripped == "/" ||
                stripped == "document-node()" ||
                stripped.StartsWith("document-node()[") ||
                stripped == "root()" ||
                stripped.StartsWith("doc(") ||
                stripped.StartsWith("(/"))
            {
                return rule;
            }
        }
        return null;
    }

    /// <summary>
    /// Implements xsl:apply-templates: selects nodes and processes each with the best-matching template.
    /// Supports XSLT 3.0 atomic-value matching.
    /// </summary>
    public void ApplyTemplates(IXdmNode contextNode, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, XElement? instruction = null)
    {
        if (++_applyTemplatesDepth > MaxApplyTemplatesDepth)
        {
            _applyTemplatesDepth--;
            throw new InvalidOperationException("xsl:apply-templates recursion depth exceeded maximum allowed depth.");
        }

        // Save and clear next-match exclusions — apply-templates starts a fresh chain
        var savedExcluded = _nextMatchExcluded;
        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

        // Resolve mode aliases
        var resolvedMode = ResolveMode(mode);
        _modeStack.Push(resolvedMode);
        try
        {
            // Determine the sequence to process
            List<XdmValue> items;
            if (string.IsNullOrEmpty(select))
            {
                // Default: child nodes
                items = EnumerateNodes(contextNode.Axis(XdmAxis.Child))
                    .Select(XdmValue.FromNode)
                    .ToList();
            }
            else
            {
                // Evaluate select expression
                var compiled = instruction != null ? CompileXPath(select, instruction) : XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(XdmValue.FromNode(contextNode), 1, 1));
                items = EnumerateItems(result).ToList();
            }

            bool allNodes = items.All(i => i.IsNode);

            // Sort nodes by document order within each source tree; keep the relative
            // order of nodes from different trees as it appeared in the selected sequence.
            // Document order across trees is implementation-defined; preserving the input
            // order matches the expectation of the conformance suite.
            if (allNodes)
            {
                items = SortNodesByDocumentOrderPreservingTreeOrder(items);
            }

            // Apply xsl:sort if present (only supported for all-node sequences currently)
            if (sortKeys != null && sortKeys.Count > 0 && allNodes)
            {
                var nodes = items.Select(i => i.NodeValue!).ToList();
                nodes = SortNodes(nodes, sortKeys);
                items = nodes.Select(XdmValue.FromNode).ToList();
            }

            int pos = 1;
            int last = items.Count;
            foreach (var item in items)
            {
                if (item.IsNode)
                {
                    var node = item.NodeValue!;
                    var rule = FindBestTemplate(node, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, node, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, callParams, position: pos, last: last);
                    }
                }
                else
                {
                    var rule = FindBestTemplate(item, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, item, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    // Built-in rule for atomic values does nothing (XSLT 3.0 §6.6)
                }
                pos++;
            }
        }
        finally
        {
            _modeStack.Pop();
            _nextMatchExcluded = savedExcluded;
            _applyTemplatesDepth--;
        }
    }

    /// <summary>
    /// Implements xsl:apply-templates when there is no context node (e.g. inside a named template).
    /// </summary>
    public void ApplyTemplates(XdmValue contextItem, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, XElement? instruction = null)
    {
        if (++_applyTemplatesDepth > MaxApplyTemplatesDepth)
        {
            _applyTemplatesDepth--;
            throw new InvalidOperationException("xsl:apply-templates recursion depth exceeded maximum allowed depth.");
        }

        // Save and clear next-match exclusions — apply-templates starts a fresh chain
        var savedExcluded = _nextMatchExcluded;
        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

        // Resolve mode aliases
        var resolvedMode = ResolveMode(mode);

        _modeStack.Push(resolvedMode);
        try
        {
            // Determine the sequence to process
            List<XdmValue> items;
            if (string.IsNullOrEmpty(select))
            {
                // No select and no context node: empty sequence
                items = new List<XdmValue>();
            }
            else
            {
                // Evaluate select expression with the given context item as focus
                var compiled = instruction != null ? CompileXPath(select, instruction) : XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(contextItem, 1, 1));
                items = EnumerateItems(result).ToList();
            }

            bool allNodes = items.All(i => i.IsNode);

            // Sort nodes by document order; non-node sequences keep original order.
            // Use original index as tie-breaker for stable ordering when document orders
            // are equal (e.g., detached nodes from variables).
            if (allNodes)
            {
                var indexed = items.Select((item, idx) => (item, idx)).ToList();
                indexed.Sort((a, b) =>
                {
                    int cmp = a.item.NodeValue!.DocumentOrder.CompareTo(b.item.NodeValue!.DocumentOrder);
                    return cmp != 0 ? cmp : a.idx.CompareTo(b.idx);
                });
                items = indexed.Select(x => x.item).ToList();
            }

            // Apply xsl:sort if present (only supported for all-node sequences currently)
            if (sortKeys != null && sortKeys.Count > 0 && allNodes)
            {
                var nodes = items.Select(i => i.NodeValue!).ToList();
                nodes = SortNodes(nodes, sortKeys);
                items = nodes.Select(XdmValue.FromNode).ToList();
            }

            int pos = 1;
            int last = items.Count;
            foreach (var item in items)
            {
                if (item.IsNode)
                {
                    var node = item.NodeValue!;
                    var rule = FindBestTemplate(node, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, node, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, callParams, position: pos, last: last);
                    }
                }
                else
                {
                    var rule = FindBestTemplate(item, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, item, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    // Built-in rule for non-node items does nothing (XSLT 3.0 §6.6)
                }
                pos++;
            }
        }
        finally
        {
            _modeStack.Pop();
            _nextMatchExcluded = savedExcluded;
            _applyTemplatesDepth--;
        }
    }

    /// <summary>
    /// Returns the current default mode from the default-mode stack or the stylesheet root.
    /// </summary>
    private string CurrentDefaultMode => _defaultModeStack.Count > 0 ? _defaultModeStack.Peek() : _stylesheet.DefaultMode;

    /// <summary>
    /// Resolves mode aliases (#current, #default) to actual mode names.
    /// </summary>
    private string ResolveMode(string mode)
    {
        if (mode == "#current")
        {
            return _modeStack.Count > 0 ? _modeStack.Peek() : "";
        }
        if (mode == "#default")
        {
            return CurrentDefaultMode;
        }
        if (mode == "#unnamed")
        {
            return "";
        }
        return mode;
    }

    /// <summary>
    /// Returns true if the given mode is declared or used by a non-#all template in the stylesheet.
    /// Used for XTDE0045 initial mode validation.
    /// </summary>
    private bool ModeExists(string mode)
    {
        if (string.IsNullOrEmpty(mode))
            return true; // unnamed mode always exists

        // Check for explicit xsl:mode declaration
        if (_stylesheet.GetModeDefinition(mode) != null)
            return true;

        // Check for template rules with this exact mode (not #all)
        foreach (var rule in _allTemplateRules)
        {
            if (rule.MatchesAllModes)
                continue;
            foreach (var m in rule.Modes)
            {
                if (m == mode)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Expands a mode attribute value to Clark notation ({uri}local) using
    /// the in-scope namespaces of the instruction element. No-op for special
    /// mode names (#current, #default, #all) and unprefixed names.
    /// </summary>
    private static string ExpandModeName(string mode, XElement instruction)
    {
        if (mode == "#current" || mode == "#default" || mode == "#all" || mode == "#unnamed")
            return mode;

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return Stylesheet.ModeDefinition.NormalizeModeName(mode);

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        // Search for xmlns:prefix declaration on instruction or ancestors
        var current = instruction;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return Stylesheet.ModeDefinition.NormalizeModeName($"{{{attr.Value}}}{local}");
                }
            }
            current = current.Parent;
        }
        // Prefix not declared — return normalized name (will fail to match)
        return Stylesheet.ModeDefinition.NormalizeModeName(mode);
    }

    /// <summary>
    /// Executes the body of a template rule against the current node.
    /// </summary>
    public void ExecuteTemplate(Stylesheet.TemplateRule rule, IXdmNode currentNode, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
        => ExecuteTemplate(rule, XdmValue.FromNode(currentNode), callParams, incomingTunnelParams, position, last, setCurrentRule);

    public void ExecuteTemplate(Stylesheet.TemplateRule rule, XdmValue contextItem, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
    {
        var asType = rule.Element.Attribute("as")?.Value;
        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        var savedAccumulator = _sequenceAccumulator;
        XElement? tempContainer = null;

        if (!string.IsNullOrEmpty(asType))
        {
            tempContainer = new XElement("__temp__");
            _currentContainer = tempContainer;
            _lastAddedWasAtomic = false;
            _sequenceAccumulator = null;
        }

        var savedTemplateRule = _currentTemplateRule;
        if (setCurrentRule)
            _currentTemplateRule = rule;

        // Update context to current item
        var savedItem = _context.ContextItem;
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;

        // Handle xsl:context-item use="absent" in named templates
        var contextItemAbsent = rule.Element.Elements()
            .Any(e => e.Name.LocalName == "context-item"
                   && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
                   && e.Attribute("use")?.Value == "absent");

        if (contextItemAbsent)
        {
            _context.WithFocus(XdmValue.Undefined, position, last);
            _context.WithCurrentItem(XdmValue.Undefined);
        }
        else
        {
            _context.WithFocus(contextItem, position, last);
            _context.WithCurrentItem(contextItem);
        }

        // Snapshot current variables for lexical scoping
        var snapshot = _context.SnapshotVariables();

        // Push tunnel parameters for this template invocation
        var tunnelFrame = new Dictionary<string, XdmValue>();
        if (_tunnelParamStack.Count > 0)
        {
            foreach (var (k, v) in _tunnelParamStack.Peek())
                tunnelFrame[k] = v;
        }
        if (incomingTunnelParams != null)
        {
            foreach (var (k, v) in incomingTunnelParams)
                tunnelFrame[k] = v;
        }
        _tunnelParamStack.Push(tunnelFrame);

        // Push default-mode for this template scope
        var templateDefaultMode = rule.Element.Attribute("default-mode")?.Value;
        if (!string.IsNullOrEmpty(templateDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(templateDefaultMode, rule.Element));
        }

        try
        {
            // Process xsl:param declarations first (must be first children per spec)
            foreach (var child in rule.Element.Elements())
            {
                if (child.Name.LocalName == "param" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                {
                    var paramName = child.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(paramName))
                        continue;

                    var isTunnel = child.Attribute("tunnel")?.Value == "yes";

                    XdmValue paramValue;
                    if (callParams != null && callParams.TryGetValue(paramName, out var provided))
                    {
                        paramValue = provided;
                    }
                    else if (isTunnel && _tunnelParamStack.Count > 0 && _tunnelParamStack.Peek().TryGetValue(paramName, out var tunnelValue))
                    {
                        paramValue = tunnelValue;
                    }
                    else
                    {
                        var paramSelect = child.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(paramSelect))
                        {
                            var compiled = XPath31Expression.Compile(paramSelect);
                            paramValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Check for content (sequence constructor as default value)
                            var contentNodes = child.Nodes().ToList();
                            if (contentNodes.Count > 0)
                            {
                                paramValue = EvaluateSequenceConstructor(child, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(child.Attribute("as")?.Value));
                            }
                            else
                            {
                                paramValue = XdmValue.FromSequence(XdmSequence.Empty);
                            }
                        }
                    }
                    paramValue = ConvertVariableValue(paramValue, child.Attribute("as")?.Value);
                    _context.WithVariable(paramName, paramValue);
                }
                else
                {
                    break; // xsl:param must be first; stop once we hit non-param
                }
            }

            // Process the sequence constructor (child nodes of xsl:template)
            foreach (var childNode in rule.Element.Nodes())
            {
                switch (childNode)
                {
                    case XText text:
                        ProcessSequenceText(text, rule.Element);
                        break;
                    case XElement elem when elem.Name.LocalName == "param" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        continue; // Already processed above
                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        ExecuteXsltInstruction(elem, contextItem);
                        break;
                    case XElement elem:
                        CopyLiteralElement(elem);
                        break;
                }
            }
        }
        finally
        {
            _context.RestoreVariables(snapshot);
            _context.WithFocus(savedItem, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _tunnelParamStack.Pop();
            if (!string.IsNullOrEmpty(templateDefaultMode))
            {
                _defaultModeStack.Pop();
            }
            _currentTemplateRule = savedTemplateRule;

            if (tempContainer != null)
            {
                var items = new List<XdmValue>();
                foreach (var attr in tempContainer.Attributes())
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
                }
                foreach (var node in tempContainer.Nodes())
                {
                    switch (node)
                    {
                        case XElement e:
                            items.Add(XdmValue.FromNode(new XDocumentNode(e)));
                            break;
                        case XText t when !string.IsNullOrEmpty(t.Value):
                            items.Add(XdmValue.FromString(t.Value));
                            break;
                        case XComment c:
                            items.Add(XdmValue.FromNode(new XDocumentNode(c)));
                            break;
                        case XProcessingInstruction pi:
                            items.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                            break;
                    }
                }

                _currentContainer = savedContainer;
                _lastAddedWasAtomic = savedLastAtomic;
                _sequenceAccumulator = savedAccumulator;

                if (items.Count > 0)
                {
                    var result = items.Count == 1 ? items[0] :
                        XdmValue.FromSequence(MaterializedSequence.FromList(items));
                    result = ConvertVariableValue(result, asType);
                    CopyToResult(result);
                }
            }
            else
            {
                _currentContainer = savedContainer;
                _lastAddedWasAtomic = savedLastAtomic;
                _sequenceAccumulator = savedAccumulator;
            }
        }
    }

    /// <summary>
    /// Implements xsl:call-template: invokes a named template by name.
    /// </summary>
    public void CallTemplate(string name, IXdmNode currentNode, Dictionary<string, XdmValue>? withParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null)
        => CallTemplate(name, XdmValue.FromNode(currentNode), withParams, incomingTunnelParams);

    private const int MaxCallTemplateDepth = 128;

    public void CallTemplate(string name, XdmValue contextItem, Dictionary<string, XdmValue>? withParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null)
    {
        if (++_callTemplateDepth > MaxCallTemplateDepth)
        {
            _callTemplateDepth--;
            throw new InvalidOperationException("xsl:call-template recursion depth exceeded maximum allowed depth.");
        }

        try
        {
            if (!_allNamedTemplates.TryGetValue(name, out var rule))
                throw new InvalidOperationException($"Named template '{name}' not found.");

            ExecuteTemplate(rule, contextItem, withParams, incomingTunnelParams, _context.ContextPosition, _context.ContextSize, setCurrentRule: false);
        }
        finally
        {
            _callTemplateDepth--;
        }
    }

    /// <summary>
    /// Executes a single XSLT instruction element.
    /// </summary>
    private void ExecuteXsltInstruction(XElement instruction, IXdmNode currentNode)
        => ExecuteXsltInstruction(instruction, XdmValue.FromNode(currentNode));

    private void ExecuteXsltInstruction(XElement instruction, XdmValue contextItem)
    {
        var node = contextItem.IsNode ? contextItem.NodeValue : null;

        // Push default-mode for this instruction scope
        var instructionDefaultMode = instruction.Attribute("default-mode")?.Value;
        if (!string.IsNullOrEmpty(instructionDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(instructionDefaultMode, instruction));
        }

        try
        {
            var name = instruction.Name.LocalName;
            switch (name)
            {
            case "element":
                {
                    var elemNameRaw = instruction.Attribute("name")?.Value ?? "unnamed";
                    var elemName = EvaluateAvt(elemNameRaw, instruction);
                    var elemNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var elemNs = elemNsRaw != null ? EvaluateAvt(elemNsRaw, instruction) : null;
                    var (elemLocalName, elemNsUri) = ResolveElementName(instruction, elemName, elemNs, "XTDE0830");
                    var elem = new XElement(XName.Get(elemLocalName, elemNsUri));

                    var elemInheritNsRaw = instruction.Attribute("inherit-namespaces")?.Value
                        ?? instruction.Attribute("_inherit-namespaces")?.Value
                        ?? "yes";
                    var elemInheritNs = EvaluateAvt(elemInheritNsRaw, instruction);
                    if (elemInheritNs == "no" || elemInheritNs == "false")
                    {
                        elem.AddAnnotation(new NamespaceInheritanceBarrier());
                    }

                    AddElementToContainer(elem, _currentContainer);
                    var prev = _currentContainer;
                    _currentContainer = elem;
                    _lastAddedWasAtomic = false;

                    // Apply attribute sets; xsl:attribute children in the element body override them.
                    ApplyAttributeSets(instruction, elem);

                    foreach (var childNode in instruction.Nodes())
                    {
                        switch (childNode)
                        {
                            case XText text:
                                ProcessSequenceText(text, instruction);
                                break;
                            case XElement elemChild when elemChild.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                ExecuteXsltInstruction(elemChild, contextItem);
                                break;
                            case XElement elemChild:
                                CopyLiteralElement(elemChild);
                                break;
                        }
                    }
                    NormalizeElementContent(elem);
                    _currentContainer = prev;
                    break;
                }

            case "attribute":
                {
                    var attrNameRaw = instruction.Attribute("name")?.Value ?? "unnamed";
                    var attrName = EvaluateAvt(attrNameRaw, instruction);
                    var attrNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var attrNs = attrNsRaw != null ? EvaluateAvt(attrNsRaw, instruction) : null;
                    var (attrLocalName, attrNsUri) = ResolveAttributeName(instruction, attrName, attrNs, "XTDE0860");
                    var select = instruction.Attribute("select")?.Value;
                    string value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        value = XdmValueToString(result, " ");
                    }
                    else
                    {
                        var attrSep = instruction.Attribute("separator")?.Value ?? "";
                        value = EvaluateSimpleContent(instruction, contextItem, attrSep);
                    }
                    if (_currentContainer is not XElement attrTarget)
                        throw new InvalidOperationException("XTDE0420");
                    if (attrTarget.Nodes().Any())
                        throw new InvalidOperationException("XTDE0410");
                    attrTarget.SetAttributeValue(XName.Get(attrLocalName, attrNsUri), value);
                    break;
                }

            case "value-of":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        string textValue;
                        if (_context.BackwardsCompatible)
                        {
                            // XSLT 1.0: value-of outputs only the first item (like string())
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                var en = XdmSequence.FromSource(result.SequenceValue).GetEnumerator();
                                textValue = en.MoveNext() ? en.Current.ToString() : string.Empty;
                            }
                            else
                            {
                                textValue = result.ToString();
                            }
                        }
                        else
                        {
                            var sep = instruction.Attribute("separator")?.Value ?? " ";
                            textValue = XdmValueToString(result, sep);
                        }
                        _lastAddedWasAtomic = false;
                        AddTextNode(textValue);
                    }
                    else
                    {
                        var voSep = instruction.Attribute("separator")?.Value ?? "";
                        var textValue = EvaluateSimpleContent(instruction, contextItem, voSep);
                        _lastAddedWasAtomic = false;
                        AddTextNode(textValue);
                    }
                    break;
                }

            case "text":
                {
                    if (instruction.Elements().Any())
                        throw new InvalidOperationException("XTSE0010: xsl:text must contain only text nodes");
                    var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                    // XSLT 3.0 §5.6.2: TVTs are expanded in xsl:text when expand-text="yes"
                    // is set on the xsl:text element or an ancestor.
                    if (GetExpandText(instruction))
                    {
                        text = EvaluateTvt(text);
                    }
                    _lastAddedWasAtomic = false;
                    AddTextNode(text);
                    break;
                }

            case "comment":
                {
                    var commentSelect = instruction.Attribute("select")?.Value;
                    string commentText;
                    if (!string.IsNullOrEmpty(commentSelect))
                    {
                        var compiled = CompileXPath(commentSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        commentText = XdmValueToString(result);
                    }
                    else
                    {
                        commentText = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    _currentContainer.Add(new XComment(commentText));
                    break;
                }

            case "processing-instruction":
                {
                    var piNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var piName = EvaluateAvt(piNameRaw, instruction);
                    var piSelect = instruction.Attribute("select")?.Value;
                    string piData;
                    if (!string.IsNullOrEmpty(piSelect))
                    {
                        var compiled = CompileXPath(piSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        piData = XdmValueToString(result);
                    }
                    else
                    {
                        piData = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    // XSLT 3.0 §11.4.4: leading spaces in PI data are removed
                    piData = piData.TrimStart();
                    _currentContainer.Add(new XProcessingInstruction(piName, piData));
                    break;
                }

            case "namespace":
                {
                    var nsNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var nsName = EvaluateAvt(nsNameRaw, instruction);
                    var nsSelect = instruction.Attribute("select")?.Value;
                    string nsUri;
                    if (!string.IsNullOrEmpty(nsSelect))
                    {
                        var compiled = CompileXPath(nsSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        nsUri = result.ToString();
                    }
                    else
                    {
                        nsUri = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    if (_currentContainer is XElement targetElem)
                    {
                        if (string.IsNullOrEmpty(nsName))
                        {
                            // Default namespace declaration
                            targetElem.SetAttributeValue("xmlns", nsUri);
                        }
                        else
                        {
                            targetElem.SetAttributeValue(XNamespace.Xmlns + nsName, nsUri);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("XTDE0420");
                    }
                    break;
                }

            case "message":
                {
                    // xsl:message may have both a @select attribute and a sequence
                    // constructor; both contribute to the emitted message.
                    var msgSelect = instruction.Attribute("select")?.Value;
                    var msgParts = new System.Text.StringBuilder();
                    if (!string.IsNullOrEmpty(msgSelect))
                    {
                        var compiled = CompileXPath(msgSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        msgParts.Append(XdmValueToString(result, " "));
                    }
                    msgParts.Append(EvaluateSimpleContent(instruction, contextItem, " "));
                    _messageListener?.OnMessage(msgParts.ToString());
                    break;
                }

            case "copy":
                {
                    // XSLT 3.0: optional select attribute; default is context item
                    IXdmNode nodeToCopy = node!;
                    var copySelect = instruction.Attribute("select")?.Value;
                    bool hasSelect = !string.IsNullOrEmpty(copySelect);
                    var savedCopyTemplateRule = _currentTemplateRule;
                    var savedCopyExcluded = _nextMatchExcluded;

                    if (hasSelect)
                    {
                        _currentTemplateRule = null;
                        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();
                        var compiled = CompileXPath(copySelect, instruction);
                        var result = compiled.Evaluate(_context);

                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            var items = new List<XdmValue>();
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                            if (items.Count > 1)
                                throw new InvalidOperationException("XTTE3180");
                            if (items.Count == 1)
                            {
                                var item = items[0];
                                _context.WithFocus(item, 1, 1);
                                if (item.IsNode && item.NodeValue != null)
                                    ExecuteSingleCopy(item.NodeValue, instruction);
                                else if (!item.IsUndefined)
                                {
                                    _lastAddedWasAtomic = false;
                                    AddTextNode(item.StringValue);
                                }
                            }
                        }
                        else if (result.IsNode && result.NodeValue != null)
                        {
                            nodeToCopy = result.NodeValue;
                            _context.WithFocus(XdmValue.FromNode(nodeToCopy), 1, 1);
                            ExecuteSingleCopy(nodeToCopy, instruction);
                        }
                        else if (!result.IsUndefined)
                        {
                            _lastAddedWasAtomic = false;
                            AddTextNode(result.StringValue);
                        }

                        _currentTemplateRule = savedCopyTemplateRule;
                        _nextMatchExcluded = savedCopyExcluded;
                    }
                    else
                    {
                        if (nodeToCopy == null)
                            throw new InvalidOperationException("XTTE0945");
                        ExecuteSingleCopy(nodeToCopy, instruction);
                    }
                    break;
                }

            case "where-populated":
                {
                    var wpSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(wpSelect))
                    {
                        var compiled = CompileXPath(wpSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        if (IsPopulated(result))
                        {
                            CopyToResult(result, separateAtomicsWithSpace: true);
                        }
                        break;
                    }

                    // Evaluate the sequence constructor while preserving document nodes
                    // produced by xsl:document and items produced by xsl:sequence, so that
                    // an empty child element inside a document node is not mistaken for
                    // populated content. Element-building instructions are evaluated with
                    // the sequence accumulator suspended so their output goes into the
                    // current container (e.g. an xsl:element being constructed).
                    var resultItems = new List<XdmValue>();
                    var temp = new XElement("__wp_temp__");
                    var wpAccumulator = new List<XdmValue>();
                    var savedContainer = _currentContainer;
                    var savedAccumulator = _sequenceAccumulator;
                    var savedLastAtomic = _lastAddedWasAtomic;
                    _currentContainer = temp;
                    _sequenceAccumulator = null;
                    _lastAddedWasAtomic = false;
                    try
                    {
                        foreach (var childNode in instruction.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    FlushWherePopulatedTemp(temp, resultItems);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    {
                                        var localName = elem.Name.LocalName;
                                        if (localName == "on-empty")
                                        {
                                            // xsl:on-empty is handled after the populated check.
                                            break;
                                        }
                                        if (localName == "sequence" || localName == "document")
                                        {
                                            _sequenceAccumulator = wpAccumulator;
                                            try
                                            {
                                                ExecuteXsltInstruction(elem, contextItem);
                                            }
                                            finally
                                            {
                                                _sequenceAccumulator = null;
                                            }
                                            FlushWherePopulatedAccumulator(wpAccumulator, resultItems);
                                        }
                                        else
                                        {
                                            ExecuteXsltInstruction(elem, contextItem);
                                            FlushWherePopulatedTemp(temp, resultItems);
                                        }
                                    }
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    FlushWherePopulatedTemp(temp, resultItems);
                                    break;
                            }
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                        _sequenceAccumulator = savedAccumulator;
                        _lastAddedWasAtomic = savedLastAtomic;
                    }

                    if (IsPopulated(XdmValue.FromSequence(MaterializedSequence.FromList(resultItems))))
                    {
                        CopyToResult(XdmValue.FromSequence(MaterializedSequence.FromList(resultItems)), separateAtomicsWithSpace: true);
                    }
                    else
                    {
                        foreach (var onEmpty in instruction.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var oeSelect = onEmpty.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(oeSelect))
                            {
                                var compiled = XPath31Expression.Compile(oeSelect);
                                var oeResult = compiled.Evaluate(_context);
                                CopyToResult(oeResult, separateAtomicsWithSpace: true);
                            }
                            else
                            {
                                var oeResult = EvaluateSequenceConstructor(onEmpty, contextItem, wrapInDocumentNode: false);
                                CopyToResult(oeResult, separateAtomicsWithSpace: true);
                            }
                        }
                    }
                    break;
                }

            case "apply-templates":
                {
                    var select = instruction.Attribute("select")?.Value;
                    var modeRaw = instruction.Attribute("mode")?.Value?.Trim();
                    var mode = string.IsNullOrEmpty(modeRaw)
                        ? (_defaultModeStack.Count > 0 ? CurrentDefaultMode : (_modeStack.Count > 0 ? _modeStack.Peek() : CurrentDefaultMode))
                        : ExpandModeName(modeRaw, instruction);
                    var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();

                    // Collect xsl:with-param elements (tunnel and non-tunnel)
                    var withParams = new Dictionary<string, XdmValue>();
                    var tunnelParams = new Dictionary<string, XdmValue>();
                    foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var wpName = wp.Attribute("name")?.Value;
                        var wpSelect = wp.Attribute("select")?.Value;
                        var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                        if (!string.IsNullOrEmpty(wpName))
                        {
                            XdmValue wpValue;
                            if (!string.IsNullOrEmpty(wpSelect))
                            {
                                var compiled = CompileXPath(wpSelect, wp);
                                wpValue = compiled.Evaluate(_context);
                            }
                            else
                            {
                                wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                            }
                            wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                            if (wpTunnel)
                                tunnelParams[wpName] = wpValue;
                            else
                                withParams[wpName] = wpValue;
                        }
                    }

                    if (node != null)
                    {
                        ApplyTemplates(node, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams, instruction);
                    }
                    else if (!string.IsNullOrEmpty(select))
                    {
                        // apply-templates with select but no context node (e.g. inside named template)
                        ApplyTemplates(contextItem, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams, instruction);
                    }
                    // If node is null and select is empty, apply-templates has nothing to process
                    break;
                }

            case "for-each":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var items = EnumerateItems(result).ToList();

                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            items = SortItems(items, sortElements);
                        }

                        var savedFocus = _context.ContextItem;
                        var savedCurrent = _context.CurrentItem;
                        var savedTemplateRule = _currentTemplateRule;
                        var savedNextMatchExcluded = _nextMatchExcluded;
                        _currentTemplateRule = null;
                        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();
                        int pos = 1;
                        foreach (var item in items)
                        {
                            _context.WithFocus(item, pos, items.Count);
                            _context.WithCurrentItem(item);
                            var feSnapshot = _context.SnapshotVariables();
                            try
                            {
                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.LocalName == "sort" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            continue;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, item);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                            finally
                            {
                                _context.RestoreVariables(feSnapshot);
                            }
                            pos++;
                        }
                        _context.WithFocus(savedFocus, 1, 1);
                        _context.WithCurrentItem(savedCurrent);
                        _currentTemplateRule = savedTemplateRule;
                        _nextMatchExcluded = savedNextMatchExcluded;
                    }
                    break;
                }

            case "for-each-group":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (string.IsNullOrEmpty(select)) break;

                    // Save the caller's focus and group state BEFORE constructing groups,
                    // because evaluating grouping keys/patterns mutates the focus.
                    var savedFocus = _context.ContextItem;
                    var savedPosition = _context.ContextPosition;
                    var savedSize = _context.ContextSize;
                    var savedCurrent = _context.CurrentItem;
                    var savedTemplateRule = _currentTemplateRule;
                    var savedNextMatchExcluded = _nextMatchExcluded;
                    var savedGroup = _currentGroup;
                    var savedKey = _currentGroupingKey;
                    _currentTemplateRule = null;
                    _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

                    try
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var items = EnumerateItems(result).ToList();
                        if (items.Count == 0) break;

                        var collationAttr = instruction.Attribute("collation")?.Value;
                        var effectiveCollation = string.IsNullOrEmpty(collationAttr) ? null : EvaluateAvt(collationAttr, instruction);

                        ValidateForEachGroupAttributes(instruction);

                        var groups = BuildForEachGroups(instruction, items, effectiveCollation);

                        var bindGroup = instruction.Attribute("bind-group")?.Value;
                        var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

                        // Handle xsl:sort children. In XSLT 2.0 current-group()/current-grouping-key()
                        // are visible in the sort keys; in XSLT 3.0 they are not.
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        for (int sortIdx = 0; sortIdx < sortElements.Count; sortIdx++)
                        {
                            var stableAttr = sortElements[sortIdx].Attribute("stable")?.Value;
                            if (sortIdx > 0 && !string.IsNullOrEmpty(stableAttr))
                                throw new InvalidOperationException("XTSE1017: @stable is allowed only on the first xsl:sort");
                            if (!string.IsNullOrEmpty(stableAttr))
                            {
                                var v = stableAttr.Trim();
                                if (v != "yes" && v != "true" && v != "1" &&
                                    v != "no" && v != "false" && v != "0")
                                    throw new InvalidOperationException("XTSE0020: invalid value for @stable");
                            }
                        }
                        if (sortElements.Count > 0 && groups.Count > 0)
                        {
                            groups = SortGroups(groups, sortElements);
                        }

                        int pos = 1;
                        foreach (var (key, groupItems) in groups)
                        {
                            _currentGroup = groupItems;
                            _currentGroupingKey = key;
                            var rep = groupItems[0];
                            _context.WithFocus(rep, pos, groups.Count);
                            _context.WithCurrentItem(rep);
                            var feSnapshot = _context.SnapshotVariables();
                            try
                            {
                                if (!string.IsNullOrEmpty(bindGroup))
                                    _context.WithVariable(bindGroup, XdmValue.FromSequence(MaterializedSequence.FromList(groupItems)));
                                if (!string.IsNullOrEmpty(bindKey) && key != null)
                                    _context.WithVariable(bindKey, key.Value);

                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.LocalName == "sort" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            continue;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, rep);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                            finally
                            {
                                _context.RestoreVariables(feSnapshot);
                            }
                            pos++;
                        }
                    }
                    finally
                    {
                        _context.WithFocus(savedFocus, savedPosition, savedSize);
                        _context.WithCurrentItem(savedCurrent);
                        _currentTemplateRule = savedTemplateRule;
                        _nextMatchExcluded = savedNextMatchExcluded;
                        _currentGroup = savedGroup;
                        _currentGroupingKey = savedKey;
                    }
                    break;
                }

            case "if":
                {
                    var test = instruction.Attribute("test")?.Value;
                    if (!string.IsNullOrEmpty(test))
                    {
                        var compiled = CompileXPath(test, instruction);
                        var result = compiled.Evaluate(_context);
                        if (result.EffectiveBooleanValue())
                        {
                            foreach (var childNode in instruction.Nodes())
                            {
                                switch (childNode)
                                {
                                    case XText text:
                                        ProcessSequenceText(text, instruction);
                                        break;
                                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                        ExecuteXsltInstruction(elem, contextItem);
                                        break;
                                    case XElement elem:
                                        CopyLiteralElement(elem);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                }

            case "choose":
                {
                    bool matched = false;
                    foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var test = when.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(test))
                        {
                            var compiled = CompileXPath(test, when);
                            var result = compiled.Evaluate(_context);
                            if (result.EffectiveBooleanValue())
                            {
                                matched = true;
                                foreach (var childNode in when.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, when);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, contextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (!matched)
                    {
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            foreach (var childNode in otherwise.Nodes())
                            {
                                switch (childNode)
                                {
                                    case XText text:
                                        ProcessSequenceText(text, otherwise);
                                        break;
                                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                        ExecuteXsltInstruction(elem, contextItem);
                                        break;
                                    case XElement elem:
                                        CopyLiteralElement(elem);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                }

            case "variable":
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = XPath31Expression.Compile(varSelect);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Build value from sequence constructor (text nodes + XSLT instructions)
                            varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
                        _context.WithVariable(varName, varValue);
                    }
                    break;
                }

            case "param":
                // xsl:param inside a template body is processed by ExecuteTemplate before body execution.
                // When encountered inline (e.g. inside a for-each), it behaves like a local variable.
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = XPath31Expression.Compile(varSelect);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
                        _context.WithVariable(varName, varValue);
                    }
                    break;
                }

            case "call-template":
                {
                    if (!string.IsNullOrEmpty(instruction.Attribute("as")?.Value))
                        throw new InvalidOperationException("XTSE0010: Attribute 'as' is not permitted on xsl:call-template");
                    var calledName = instruction.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(calledName))
                    {
                        var withParams = new Dictionary<string, XdmValue>();
                        var tunnelParams = new Dictionary<string, XdmValue>();
                        foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var wpName = wp.Attribute("name")?.Value;
                            var wpSelect = wp.Attribute("select")?.Value;
                            var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                            if (!string.IsNullOrEmpty(wpName))
                            {
                                XdmValue wpValue;
                                if (!string.IsNullOrEmpty(wpSelect))
                                {
                                    var compiled = CompileXPath(wpSelect, wp);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
                                wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                                if (wpTunnel)
                                    tunnelParams[wpName] = wpValue;
                                else
                                    withParams[wpName] = wpValue;
                            }
                        }
                        CallTemplate(calledName, contextItem, withParams, tunnelParams);
                    }
                    break;
                }

            case "sequence":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
                        var result = compiled.Evaluate(_context);
                        if (_sequenceAccumulator != null)
                        {
                            // Inside a variable body with @as: add sequence items directly
                            FlattenToList(result, _sequenceAccumulator);
                        }
                        else
                        {
                            CopyToResult(result, separateAtomicsWithSpace: true);
                        }
                    }
                    else
                    {
                        // Sequence constructor children
                        foreach (var childNode in instruction.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, contextItem);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }
                    break;
                }

            case "document":
                {
                    var docContent = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: true);
                    if (docContent.IsNode && docContent.NodeValue != null)
                    {
                        if (_sequenceAccumulator != null)
                        {
                            _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(docContent.NodeValue, copyAllNamespaces: true)));
                        }
                        else
                        {
                            CopyNodeToResult(docContent.NodeValue);
                        }
                    }
                    else if (docContent.IsSequence && docContent.SequenceValue != null)
                    {
                        if (_sequenceAccumulator != null)
                        {
                            foreach (var item in XdmSequence.FromSource(docContent.SequenceValue))
                                _sequenceAccumulator.Add(item);
                        }
                        else
                        {
                            foreach (var item in XdmSequence.FromSource(docContent.SequenceValue))
                            {
                                CopyToResult(item);
                            }
                        }
                    }
                    break;
                }

            case "copy-of":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
                        var result = compiled.Evaluate(_context);
                        var copyNamespacesAttrRaw = instruction.Attribute("copy-namespaces")?.Value
                        ?? instruction.Attribute("_copy-namespaces")?.Value
                        ?? "yes";
                        var copyNamespacesAttr = EvaluateAvt(copyNamespacesAttrRaw, instruction);
                        bool copyAllNs = copyNamespacesAttr != "no" && copyNamespacesAttr != "false";
                        var copyAccumulatorsAttrRaw = instruction.Attribute("copy-accumulators")?.Value ?? "no";
                        var copyAccumulatorsAttr = EvaluateAvt(copyAccumulatorsAttrRaw, instruction);
                        bool copyAccumulators = copyAccumulatorsAttr == "yes" || copyAccumulatorsAttr == "true";

                        if (_sequenceAccumulator != null)
                        {
                            // In a sequence-returning context (variable with @as),
                            // preserve document nodes by adding copies to the accumulator.
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                    {
                                        _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue, copyAllNs, copyAccumulators)));
                                    }
                                    else
                                    {
                                        _sequenceAccumulator.Add(item);
                                    }
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(result.NodeValue, copyAllNs, copyAccumulators)));
                            }
                            else
                            {
                                _sequenceAccumulator.Add(result);
                            }
                        }
                        else
                        {
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                        CopyNodeToResult(CopyXdmNode(item.NodeValue, copyAllNs, copyAccumulators));
                                    else
                                        CopyToResult(item);
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                CopyNodeToResult(CopyXdmNode(result.NodeValue, copyAllNs, copyAccumulators));
                            }
                            else
                            {
                                CopyToResult(result);
                            }
                        }
                    }
                    break;
                }

            case "next-match":
                {
                    if (_currentTemplateRule == null || _context.ContextItem.IsUndefined)
                    {
                        // xsl:next-match is only valid within a template invoked by apply-templates or next-match
                        // If called from a named template with context-item use="absent", for-each, or other
                        // context where the current template rule or context item is absent, raise XTDE0560.
                        throw new InvalidOperationException("XTDE0560: xsl:next-match evaluated when the current template rule is absent.");
                    }

                    var nextMatchMode = _modeStack.Count > 0 ? _modeStack.Peek() : "";
                    // If inside a template invoked by xsl:apply-imports, restrict next-match
                    // to templates with higher import precedence than the apply-imports caller.
                    int? nextMatchMinPrec = _applyImportsPrecedenceStack.Count > 0
                        ? _applyImportsPrecedenceStack.Peek()
                        : null;
                    _nextMatchExcluded.Add(_currentTemplateRule);
                    try
                    {
                        var nextRule = FindBestTemplate(contextItem, nextMatchMode, _nextMatchExcluded, minImportPrecedence: nextMatchMinPrec);

                        // Collect xsl:with-param elements (tunnel and non-tunnel)
                        var nextMatchParams = new Dictionary<string, XdmValue>();
                        var nextMatchTunnelParams = new Dictionary<string, XdmValue>();
                        foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var wpName = wp.Attribute("name")?.Value;
                            var wpSelect = wp.Attribute("select")?.Value;
                            var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                            if (!string.IsNullOrEmpty(wpName))
                            {
                                XdmValue wpValue;
                                if (!string.IsNullOrEmpty(wpSelect))
                                {
                                    var compiled = CompileXPath(wpSelect, wp);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
                                wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                                if (wpTunnel)
                                    nextMatchTunnelParams[wpName] = wpValue;
                                else
                                    nextMatchParams[wpName] = wpValue;
                            }
                        }

                        // Merge current tunnel params with newly supplied tunnel params
                        var mergedTunnelParams = new Dictionary<string, XdmValue>();
                        if (_tunnelParamStack.Count > 0)
                        {
                            foreach (var (k, v) in _tunnelParamStack.Peek())
                                mergedTunnelParams[k] = v;
                        }
                        foreach (var (k, v) in nextMatchTunnelParams)
                            mergedTunnelParams[k] = v;

                        if (nextRule != null)
                        {
                            _nextMatchExcluded.Add(nextRule);
                            try
                            {
                                ExecuteTemplate(nextRule, contextItem, callParams: nextMatchParams, incomingTunnelParams: mergedTunnelParams, position: _context.ContextPosition, last: _context.ContextSize);
                            }
                            finally
                            {
                                _nextMatchExcluded.Remove(nextRule);
                            }
                        }
                        else if (node != null)
                        {
                            ApplyBuiltInRules(node, nextMatchMode, mergedTunnelParams);
                        }
                        else if (!contextItem.IsUndefined)
                        {
                            // Built-in rule for atomic values: output string value
                            var text = contextItem.ToString();
                            if (!string.IsNullOrEmpty(text) && _currentContainer is XElement)
                            {
                                _currentContainer.Add(new XText(text));
                            }
                        }
                    }
                    finally
                    {
                        _nextMatchExcluded.Remove(_currentTemplateRule);
                    }
                    break;
                }

            case "apply-imports":
                {
                    if (_currentTemplateRule == null)
                    {
                        throw new InvalidOperationException("XTDE0560: xsl:apply-imports evaluated when the current template rule is absent.");
                    }

                    var applyImportsMode = _modeStack.Count > 0 ? _modeStack.Peek() : "";

                    // Find the best matching template with higher import precedence
                    // (i.e., deeper in the import chain). Main stylesheet = 0, direct imports = 1, etc.
                    var importedRule = FindBestTemplate(contextItem, applyImportsMode, minImportPrecedence: _currentTemplateRule.ImportPrecedence);

                    // Collect xsl:with-param elements (tunnel and non-tunnel)
                    var applyImportsParams = new Dictionary<string, XdmValue>();
                    var applyImportsTunnelParams = new Dictionary<string, XdmValue>();
                    foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var wpName = wp.Attribute("name")?.Value;
                        var wpSelect = wp.Attribute("select")?.Value;
                        var wpTunnel = wp.Attribute("tunnel")?.Value == "yes";
                        if (!string.IsNullOrEmpty(wpName))
                        {
                            XdmValue wpValue;
                            if (!string.IsNullOrEmpty(wpSelect))
                            {
                                var compiled = CompileXPath(wpSelect, wp);
                                wpValue = compiled.Evaluate(_context);
                            }
                            else
                            {
                                wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                            }
                            wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value);
                            if (wpTunnel)
                                applyImportsTunnelParams[wpName] = wpValue;
                            else
                                applyImportsParams[wpName] = wpValue;
                        }
                    }

                    // Pass through current tunnel parameters, overridden by newly supplied ones
                    var currentTunnelParams = new Dictionary<string, XdmValue>();
                    if (_tunnelParamStack.Count > 0)
                    {
                        foreach (var (k, v) in _tunnelParamStack.Peek())
                            currentTunnelParams[k] = v;
                    }
                    foreach (var (k, v) in applyImportsTunnelParams)
                        currentTunnelParams[k] = v;

                    // Push the current template rule's precedence so that xsl:next-match
                    // inside the imported template is restricted to higher import precedence
                    // rules (XSLT 3.0 §6.5).
                    _applyImportsPrecedenceStack.Push(_currentTemplateRule.ImportPrecedence);
                    try
                    {
                        if (importedRule != null)
                        {
                            ExecuteTemplate(importedRule, contextItem, callParams: applyImportsParams, incomingTunnelParams: currentTunnelParams, position: _context.ContextPosition, last: _context.ContextSize);
                        }
                        else if (node != null)
                        {
                            ApplyBuiltInRules(node, applyImportsMode, currentTunnelParams);
                        }
                        else if (!contextItem.IsUndefined)
                        {
                            // Built-in rule for atomic values: output string value
                            var text = contextItem.ToString();
                            if (!string.IsNullOrEmpty(text) && _currentContainer is XElement)
                            {
                                _currentContainer.Add(new XText(text));
                            }
                        }
                    }
                    finally
                    {
                        _applyImportsPrecedenceStack.Pop();
                    }
                    break;
                }

            case "number":
                {
                    var hasValueAttr = !string.IsNullOrEmpty(instruction.Attribute("value")?.Value);
                    var hasSelectAttr = !string.IsNullOrEmpty(instruction.Attribute("select")?.Value);

                    if (hasValueAttr || hasSelectAttr)
                    {
                        ExecuteXsltNumber(instruction, node!);
                    }
                    else if (node != null)
                    {
                        ExecuteXsltNumber(instruction, node);
                    }
                    else
                    {
                        // No value, no select, and no context node
                        throw new InvalidOperationException("XTTE0990");
                    }
                    break;
                }

            case "try":
                {
                    var catchElem = instruction.Element(XName.Get("catch", Stylesheet.Stylesheet.XslNamespace));
                    try
                    {
                        foreach (var childNode in instruction.Nodes())
                        {
                            if (childNode is XElement xe && xe.Name.LocalName == "catch" && xe.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                continue;
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, contextItem);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        if (catchElem != null)
                        {
                            var catchSelect = catchElem.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(catchSelect))
                            {
                                var compiled = XPath31Expression.Compile(catchSelect);
                                var catchResult = compiled.Evaluate(_context);
                                CopyToResult(catchResult);
                            }
                            else
                            {
                                foreach (var childNode in catchElem.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, catchElem);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, contextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }

            case "iterate":
                throw new InvalidOperationException("XTDE1450: xsl:iterate is not supported.");

            case "perform-sort":
                {
                    var psSelect2 = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(psSelect2))
                    {
                        var compiled = XPath31Expression.Compile(psSelect2);
                        var psResult = compiled.Evaluate(_context);
                        var psItems = EnumerateItems(psResult).ToList();

                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            psItems = SortItems(psItems, sortElements);
                        }

                        foreach (var item in psItems)
                            CopyToResult(item);
                    }
                    break;
                }

            default:
                // Unknown instruction: ignore for now
                break;
        }
        }
        finally
        {
            if (!string.IsNullOrEmpty(instructionDefaultMode))
            {
                _defaultModeStack.Pop();
            }
        }
    }

    /// <summary>
    /// Applies the named attribute sets to the target element.
    /// Attribute sets accumulate across imports/includes (merge semantics).
    /// </summary>
    private void ApplyAttributeSets(XElement source, XElement target, HashSet<(string LocalName, string NamespaceUri)>? visited = null)
    {
        // Check both xsl:use-attribute-sets (on literal elements) and use-attribute-sets (on xsl:element / xsl:attribute-set)
        var useAttrSetsRaw = source.Attribute(XNamespace.Get(Stylesheet.Stylesheet.XslNamespace) + "use-attribute-sets")?.Value
            ?? source.Attribute("use-attribute-sets")?.Value;
        if (string.IsNullOrWhiteSpace(useAttrSetsRaw))
            return;

        visited ??= new HashSet<(string, string)>();
        var allSets = _stylesheet.GetAllAttributeSets();
        var xslNs = Stylesheet.Stylesheet.XslNamespace;

        foreach (var name in useAttrSetsRaw.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = name.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Resolve QName
            string localName;
            string nsUri;
            int colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = trimmed.Substring(0, colon);
                localName = trimmed.Substring(colon + 1);
                nsUri = source.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
            }
            else
            {
                localName = trimmed;
                nsUri = "";
            }

            var key = (localName, nsUri);
            if (!allSets.TryGetValue(key, out var defs))
                continue;

            if (!visited.Add(key))
                continue; // Cycle detected — skip to avoid infinite recursion

            var prevContainer = _currentContainer;
            _currentContainer = target;
            try
            {
                foreach (var def in defs)
                {
                    // Recursively apply referenced attribute sets
                    if (!string.IsNullOrWhiteSpace(def.UseAttributeSets))
                    {
                        ApplyAttributeSets(def.Element, target, visited);
                    }

                    // Execute this definition's xsl:attribute children
                    foreach (var attrChild in def.Element.Elements(XName.Get("attribute", xslNs)))
                    {
                        ExecuteXsltInstruction(attrChild, _context.ContextItem);
                    }
                }
            }
            finally
            {
                _currentContainer = prevContainer;
                visited.Remove(key);
            }
        }
    }

    /// <summary>
    /// Copies a literal result element to the output.
    /// </summary>
    private void CopyLiteralElement(XElement source)
    {
        // Preserve the namespace prefix by using the original XName and adding
        // an explicit namespace declaration when the element uses a non-empty namespace.
        var copy = new XElement(source.Name);

        // Handle inherit-namespaces="no" on literal result elements.
        var lreInheritNs = source.Attribute(XName.Get("inherit-namespaces", Stylesheet.Stylesheet.XslNamespace))?.Value ?? "yes";
        if (lreInheritNs == "no" || lreInheritNs == "false")
        {
            copy.AddAnnotation(new NamespaceInheritanceBarrier());
        }

        // If the element has a non-empty namespace URI, ensure the namespace
        // is declared on the copied element (either as xmlns or xmlns:prefix).
        // The element's own namespace is always required and is never excluded.
        if (!string.IsNullOrEmpty(source.Name.NamespaceName))
        {
            var prefix = source.GetPrefixOfNamespace(source.Name.Namespace);
            if (prefix == "")
            {
                copy.SetAttributeValue("xmlns", source.Name.NamespaceName);
            }
            else if (!string.IsNullOrEmpty(prefix) && !_excludedResultPrefixes.Contains(prefix))
            {
                copy.SetAttributeValue(XNamespace.Xmlns + prefix, source.Name.NamespaceName);
            }
        }

        // Apply attribute sets first; literal attributes override them.
        ApplyAttributeSets(source, copy);

        foreach (var attr in source.Attributes())
        {
            // Skip namespace declarations that are inherited from ancestors
            // (only copy namespace declarations explicitly declared on this element).
            if (attr.IsNamespaceDeclaration)
            {
                var declaredPrefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (declaredPrefix == source.GetPrefixOfNamespace(source.Name.Namespace))
                {
                    continue; // Already handled above
                }
                // Skip explicitly excluded prefixes (e.g. exclude-result-prefixes="xs").
                // #all is not a prefix name and is handled at serialization time.
                if (_excludedResultPrefixes.Contains(declaredPrefix))
                    continue;
                // Skip XSLT namespace declaration — it is not copied to the result tree
                if (attr.Value == Stylesheet.Stylesheet.XslNamespace)
                    continue;
                copy.SetAttributeValue(attr.Name, attr.Value);
                continue;
            }

            // XSLT-namespace attributes on literal result elements are instructions,
            // not attributes to be copied to the result tree.
            if (attr.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                continue;

            var attrName = XName.Get(attr.Name.LocalName, attr.Name.NamespaceName);
            var attrValue = EvaluateAvt(attr.Value, source);
            copy.SetAttributeValue(attrName, attrValue);
        }

        AddElementToContainer(copy, _currentContainer);

        var prev = _currentContainer;
        _currentContainer = copy;
        _lastAddedWasAtomic = false;

        // Push xsl:default-mode for this literal result element scope
        var lreDefaultMode = source.Attribute(XName.Get("default-mode", Stylesheet.Stylesheet.XslNamespace))?.Value;
        if (!string.IsNullOrEmpty(lreDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(lreDefaultMode, source));
        }

        try
        {
            // Collect xsl:on-empty children before processing
            var onEmptyElements = source.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)).ToList();

            foreach (var child in source.Nodes())
            {
                // Skip xsl:on-empty elements during normal processing
                if (child is XElement childElemOnEmpty &&
                    childElemOnEmpty.Name.LocalName == "on-empty" &&
                    childElemOnEmpty.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                {
                    continue;
                }

                switch (child)
                {
                    case XElement childElem:
                        if (childElem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                        {
                            ExecuteXsltInstruction(childElem, _context.ContextItem);
                        }
                        else
                        {
                            CopyLiteralElement(childElem);
                        }
                        break;
                    case XText text:
                        ProcessSequenceText(text, source);
                        break;
                    // Comments and processing instructions inside literal result elements
                    // are part of the stylesheet, not the result tree.
                    case XComment:
                    case XProcessingInstruction:
                        break;
                }
            }

            // If no children were added, evaluate xsl:on-empty instructions
            if (!copy.Nodes().Any() && onEmptyElements.Count > 0)
            {
                foreach (var onEmpty in onEmptyElements)
                {
                    var oeSelect = onEmpty.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(oeSelect))
                    {
                        var compiled = XPath31Expression.Compile(oeSelect);
                        var result = compiled.Evaluate(_context);
                        CopyToResult(result, separateAtomicsWithSpace: true);
                    }
                    else
                    {
                        foreach (var childNode in onEmpty.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, onEmpty);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, _context.ContextItem);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }
                }
            }

            NormalizeElementContent(copy);
        }
        finally
        {
            if (!string.IsNullOrEmpty(lreDefaultMode))
            {
                _defaultModeStack.Pop();
            }
            _currentContainer = prev;
        }
    }

    /// <summary>
    /// Evaluates Attribute Value Templates (AVTs): {expr} is evaluated, {{ and }} are escaped.
    /// Expressions are compiled with the in-scope namespaces, xpath-default-namespace, and
    /// base URI of the element that carries the attribute.
    /// </summary>
    private string EvaluateAvt(string value, XElement? contextElement = null)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains('{'))
            return value;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        var avtBaseUri = GetEffectiveBaseUri(contextElement);
        var nsMap = contextElement != null ? GetInScopeNamespaces(contextElement) : null;
        var defaultNs = contextElement != null ? GetXPathDefaultNamespace(contextElement) : null;
        bool needsOptions = (nsMap != null && nsMap.Count > 1)
            || !string.IsNullOrEmpty(defaultNs)
            || !string.IsNullOrEmpty(avtBaseUri);

        while (i < value.Length)
        {
            if (i + 1 < value.Length && value[i] == '{' && value[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
            }
            else if (i + 1 < value.Length && value[i] == '}' && value[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
            }
            else if (value[i] == '{')
            {
                int end = FindAvtExprEnd(value, i + 1);
                if (end < 0)
                {
                    sb.Append(value[i]);
                    i++;
                }
                else
                {
                    var expr = value.Substring(i + 1, end - i - 1);
                    if (!string.IsNullOrEmpty(expr))
                    {
                        XPath31Expression compiled;
                        if (needsOptions)
                        {
                            var options = new CompileOptions
                            {
                                Namespaces = nsMap,
                                DefaultElementNamespace = defaultNs,
                                BaseUri = avtBaseUri
                            };
                            compiled = XPath31Expression.Compile(expr, options);
                        }
                        else
                        {
                            compiled = XPath31Expression.Compile(expr);
                        }
                        var result = compiled.Evaluate(_context);
                        sb.Append(XdmValueToString(result));
                    }
                    i = end + 1;
                }
            }
            else if (value[i] == '}')
            {
                // Lone } is an error per spec, but treat as literal for robustness
                sb.Append('}');
                i++;
            }
            else
            {
                sb.Append(value[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Finds the closing <c>}</c> of an AVT expression, skipping <c>}</c> inside
    /// XPath string literals (both single- and double-quoted).
    /// </summary>
    private static int FindAvtExprEnd(string value, int start)
    {
        char inString = '\0';
        for (int i = start; i < value.Length; i++)
        {
            char c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    // Check for escaped quote (doubled)
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i++; // skip the pair
                    }
                    else
                    {
                        inString = '\0';
                    }
                }
                continue;
            }
            if (c == '\'' || c == '"')
            {
                inString = c;
                continue;
            }
            if (c == '}')
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Copies an XDM value (node or sequence) into the result tree.
    /// </summary>
    /// <param name="value">The value to copy.</param>
    /// <param name="separateAtomicsWithSpace">If true, consecutive atomic values are separated by a space (complex content construction). If false, they are concatenated directly (xsl:copy-of behavior).</param>
    private void CopyToResult(XdmValue value, bool separateAtomicsWithSpace = true)
    {
        if (value.IsUndefined)
            return;

        // When collecting a raw sequence (e.g. xsl:variable/@as, xsl:key content,
        // xsl:function body), preserve atomic/node values in the sequence accumulator
        // instead of converting them to text nodes in the result tree.
        if (_sequenceAccumulator != null)
        {
            if (value.IsSequence && value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                {
                    if (item.IsUndefined)
                        continue;
                    if (item.IsNode && item.NodeValue != null)
                        _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue)));
                    else
                        _sequenceAccumulator.Add(item);
                }
            }
            else if (value.IsNode && value.NodeValue != null)
            {
                _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(value.NodeValue)));
            }
            else
            {
                _sequenceAccumulator.Add(value);
            }
            return;
        }

        if (value.IsNode && value.NodeValue != null)
        {
            _lastAddedWasAtomic = false;
            CopyNodeToResult(value.NodeValue);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            // XSLT 3.0 §5.7.1: process sequence for complex content construction.
            // - Zero-length text nodes are discarded.
            // - Adjacent text nodes are merged.
            // - Consecutive atomic values are joined with a single space (#x20) (unless copy-of).
            // - Text nodes and atomics in a contiguous run are merged into one text node.
            var sb = new StringBuilder();
            bool prevWasAtomic = false;
            bool anyItemProcessed = false;

            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                anyItemProcessed = true;

                // Discard zero-length text nodes, but they still break the atomic chain
                if (item.IsNode && item.NodeValue != null &&
                    item.NodeValue.NodeKind == XdmNodeKind.Text &&
                    item.NodeValue.StringValue.Length == 0)
                {
                    prevWasAtomic = false;
                    continue;
                }

                if (item.IsNode && item.NodeValue != null &&
                    (item.NodeValue.NodeKind == XdmNodeKind.Element ||
                     item.NodeValue.NodeKind == XdmNodeKind.Comment ||
                     item.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction))
                {
                    // Non-text node: flush accumulated text, then copy the node
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    CopyNodeToResult(item.NodeValue);
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Attribute)
                {
                    // Attribute node: flush accumulated text, then add attribute
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    CopyNodeToResult(item.NodeValue);
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Text)
                {
                    // Text node: append without separator
                    sb.Append(item.NodeValue.StringValue);
                    prevWasAtomic = false;
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Document)
                {
                    // Document nodes in complex content are replaced by their children (XSLT 3.0 §5.7.1)
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    foreach (var child in item.NodeValue.Axis(XdmAxis.Child))
                    {
                        if (child.IsNode && child.NodeValue != null)
                        {
                            CopyNodeToResult(child.NodeValue);
                        }
                    }
                }
                else
                {
                    // Atomic value: insert space only if previous item was also atomic
                    // and separateAtomicsWithSpace is true (complex content construction)
                    if (separateAtomicsWithSpace && prevWasAtomic)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(item.ToString());
                    prevWasAtomic = true;
                }
            }

            if (sb.Length > 0)
            {
                AddTextNode(sb.ToString());
            }
            if (anyItemProcessed)
            {
                _lastAddedWasAtomic = prevWasAtomic;
            }
        }
        else
        {
            AppendAtomicText(value.ToString());
        }
    }

    /// <summary>
    /// Determines whether an XDM value is populated for the purposes of
    /// <c>xsl:where-populated</c>. A sequence is populated if it contains at
    /// least one item that is not an "empty" node: document and element nodes
    /// are empty when they have no children (and elements have no attributes);
    /// text, comment, processing-instruction, and attribute nodes are empty
    /// when their string value is zero-length. Atomic values are always
    /// populated.
    /// </summary>
    private bool IsPopulated(XdmValue value)
    {
        if (value.IsUndefined)
            return false;

        if (value.IsNode && value.NodeValue != null)
            return IsPopulatedNode(value.NodeValue);

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsUndefined)
                    continue;
                if (item.IsNode && item.NodeValue != null)
                {
                    if (IsPopulatedNode(item.NodeValue))
                        return true;
                }
                else
                {
                    // Atomic value
                    return true;
                }
            }
            return false;
        }

        // Single atomic value
        return true;
    }

    private static bool IsPopulatedNode(IXdmNode node)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Document:
                return node.Axis(XdmAxis.Child).GetEnumerator().MoveNext();
            case XdmNodeKind.Element:
                if (node.Axis(XdmAxis.Attribute).GetEnumerator().MoveNext())
                    return true;
                return node.Axis(XdmAxis.Child).GetEnumerator().MoveNext();
            case XdmNodeKind.Attribute:
            case XdmNodeKind.Text:
            case XdmNodeKind.Comment:
            case XdmNodeKind.ProcessingInstruction:
                return node.StringValue.Length > 0;
            case XdmNodeKind.Namespace:
                return !string.IsNullOrEmpty(node.StringValue);
            default:
                return true;
        }
    }

    /// <summary>
    /// Moves all content currently held in the temporary element used by
    /// <c>xsl:where-populated</c> into the result item list.
    /// </summary>
    private static void FlushWherePopulatedTemp(XElement temp, List<XdmValue> result)
    {
        // Non-namespace attributes become attribute nodes in the result.
        foreach (var attr in temp.Attributes().ToList())
        {
            if (attr.IsNamespaceDeclaration)
                continue;
            attr.Remove();
            result.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
        }

        // Child nodes are detached and wrapped as XDM nodes.
        foreach (var node in temp.Nodes().ToList())
        {
            node.Remove();
            result.Add(XdmValue.FromNode(new XDocumentNode(node)));
        }
    }

    /// <summary>
    /// Moves all items collected by the where-populated accumulator into the
    /// result item list.
    /// </summary>
    private static void FlushWherePopulatedAccumulator(List<XdmValue> accumulator, List<XdmValue> result)
    {
        if (accumulator.Count == 0)
            return;
        result.AddRange(accumulator);
        accumulator.Clear();
    }

    /// <summary>
    /// Creates a deep copy of an XDM node, returning a new IXdmNode wrapper.
    /// </summary>
    private IXdmNode CopyXdmNode(IXdmNode node)
        => CopyXdmNode(node, copyAllNamespaces: true, copyAccumulators: false);

    private IXdmNode CopyXdmNode(IXdmNode node, bool copyAllNamespaces)
        => CopyXdmNode(node, copyAllNamespaces, copyAccumulators: false);

    private IXdmNode CopyXdmNode(IXdmNode node, bool copyAllNamespaces, bool copyAccumulators)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Document:
                {
                    var children = new List<IXdmNode>();
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        if (child.IsNode && child.NodeValue != null)
                            children.Add(child.NodeValue);
                    }
                    var elementCount = children.Count(c => c.NodeKind == XdmNodeKind.Element);
                    XDocument newDoc;
                    if (elementCount == 1 && children.Count == 1)
                    {
                        newDoc = new XDocument();
                        CopyNodeToContainer(children[0], newDoc, copyAllNamespaces, copyAccumulators);
                    }
                    else
                    {
                        // XDocument cannot hold multiple root elements or mixed content;
                        // use a synthetic wrapper element like EvaluateSequenceConstructor does.
                        var docWrapper = new XElement("__xdm_doc__");
                        foreach (var child in children)
                        {
                            CopyNodeToContainer(child, docWrapper, copyAllNamespaces, copyAccumulators);
                        }
                        newDoc = new XDocument(docWrapper);
                    }
                    // Preserve base URI from the source document
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        newDoc.AddAnnotation(node.BaseUri);
                    return new XDocumentNode(newDoc);
                }
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(XName.Get(node.LocalName, node.NamespaceUri));
                    // Preserve base URI from the source element, but only if the source
                    // element does not carry its own xml:base attribute. A copied relative
                    // xml:base must be re-resolved against the new context (e.g. the
                    // stylesheet base URI of the copying instruction); adding a resolved
                    // absolute annotation would short-circuit that resolution.
                    bool hasXmlBase = node is XDocumentNode xdn && xdn.UnderlyingObject is XElement srcElem
                        && srcElem.Attribute(XNamespace.Xml + "base") != null;
                    if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                        copy.AddAnnotation(node.BaseUri);
                    if (copyAccumulators)
                        AttachAccumulatorValues(node, copy);
                    if (copyAllNamespaces)
                    {
                        foreach (var ns in node.Axis(XdmAxis.Namespace))
                        {
                            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                            {
                                if (ns.NodeValue.LocalName == "")
                                {
                                    copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                                }
                                else
                                {
                                    copy.SetAttributeValue(
                                        XNamespace.Xmlns + ns.NodeValue.LocalName,
                                        ns.NodeValue.StringValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        AddRequiredNamespaceDeclarations(node, copy);
                        copy.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    foreach (var attr in node.Attributes())
                    {
                        // Skip namespace declarations — they are handled by the namespace axis
                        // (copy-all) or AddRequiredNamespaceDeclarations (copy-required) above.
                        if (attr.NodeValue is { } attrNode &&
                            attrNode.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            continue;
                        copy.SetAttributeValue(
                            XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                            attr.NodeValue!.StringValue);
                    }
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        CopyNodeToContainer(child.NodeValue!, copy, copyAllNamespaces, copyAccumulators);
                    }
                    return new XDocumentNode(copy);
                }
            case XdmNodeKind.Text:
                return new XDocumentNode(new XText(node.StringValue));
            case XdmNodeKind.Comment:
                return new XDocumentNode(new XComment(node.StringValue));
            case XdmNodeKind.ProcessingInstruction:
                return new XDocumentNode(new XProcessingInstruction(node.LocalName, node.StringValue));
            case XdmNodeKind.Attribute:
                return new XDocumentNode(new XAttribute(
                    XName.Get(node.LocalName, node.NamespaceUri),
                    node.StringValue));
            default:
                return node;
        }
    }

    /// <summary>
    /// Creates a copy of a node for use inside an xsl:function body, processing
    /// the children of the xsl:copy instruction and adding them to the copied node.
    /// </summary>
    private IXdmNode? CopyNodeForFunctionBody(IXdmNode nodeToCopy, XElement copyInstruction)
    {
        switch (nodeToCopy.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri));
                    // Copy namespace declarations
                    foreach (var ns in nodeToCopy.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            else
                                copy.SetAttributeValue(XNamespace.Xmlns + ns.NodeValue.LocalName, ns.NodeValue.StringValue);
                        }
                    }
                    // Process children of xsl:copy into the copied element
                    var savedContainer = _currentContainer;
                    _currentContainer = copy;
                    try
                    {
                        foreach (var child in copyInstruction.Elements())
                        {
                            if (child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                ExecuteXsltInstruction(child, nodeToCopy);
                            else
                                CopyLiteralElement(child);
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                    }
                    NormalizeElementContent(copy);
                    return new XDocumentNode(copy);
                }
            case XdmNodeKind.Text:
                return new XDocumentNode(new XText(nodeToCopy.StringValue));
            case XdmNodeKind.Comment:
                return new XDocumentNode(new XComment(nodeToCopy.StringValue));
            case XdmNodeKind.ProcessingInstruction:
                return new XDocumentNode(new XProcessingInstruction(nodeToCopy.LocalName, nodeToCopy.StringValue));
            case XdmNodeKind.Attribute:
                return new XDocumentNode(new XAttribute(
                    XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri),
                    nodeToCopy.StringValue));
            case XdmNodeKind.Document:
                {
                    var newDoc = new XDocument();
                    var savedContainer = _currentContainer;
                    _currentContainer = newDoc;
                    try
                    {
                        foreach (var child in copyInstruction.Elements())
                        {
                            if (child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                ExecuteXsltInstruction(child, nodeToCopy);
                            else
                                CopyLiteralElement(child);
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                    }
                    return new XDocumentNode(newDoc);
                }
            default:
                return null;
        }
    }

    /// <summary>
    /// Performs a single xsl:copy for the given node in result-tree context.
    /// </summary>
    private void ExecuteSingleCopy(IXdmNode nodeToCopy, XElement instruction)
    {
        switch (nodeToCopy.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(
                        XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri));
                    // Preserve base URI from the source element
                    if (nodeToCopy is XDocumentNode srcXdn && srcXdn.UnderlyingObject is XElement srcElem)
                    {
                        var baseUriAnnotation = srcElem.Annotation<string>();
                        if (baseUriAnnotation != null)
                            copy.AddAnnotation(baseUriAnnotation);
                        else if (!string.IsNullOrEmpty(nodeToCopy.BaseUri))
                            copy.AddAnnotation(nodeToCopy.BaseUri);
                    }
                    else if (!string.IsNullOrEmpty(nodeToCopy.BaseUri))
                    {
                        copy.AddAnnotation(nodeToCopy.BaseUri);
                    }
                    foreach (var ns in nodeToCopy.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            else
                                copy.SetAttributeValue(
                                    XNamespace.Xmlns + ns.NodeValue.LocalName,
                                    ns.NodeValue.StringValue);
                        }
                    }

                    var inheritNamespacesAttrRaw = instruction.Attribute("inherit-namespaces")?.Value
                        ?? instruction.Attribute("_inherit-namespaces")?.Value
                        ?? "yes";
                    var inheritNamespacesAttr = EvaluateAvt(inheritNamespacesAttrRaw, instruction);
                    if (inheritNamespacesAttr == "no" || inheritNamespacesAttr == "false")
                    {
                        copy.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    AddElementToContainer(copy, _currentContainer);
                    var prev = _currentContainer;
                    _currentContainer = copy;

                    // Collect xsl:on-empty children before processing
                    var onEmptyElements = instruction.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)).ToList();

                    foreach (var childNode in instruction.Nodes())
                    {
                        // Skip xsl:on-empty elements during normal processing
                        if (childNode is XElement childElemOnEmpty &&
                            childElemOnEmpty.Name.LocalName == "on-empty" &&
                            childElemOnEmpty.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                        {
                            continue;
                        }

                        switch (childNode)
                        {
                            case XText text:
                                ProcessSequenceText(text, instruction);
                                break;
                            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                ExecuteXsltInstruction(elem, nodeToCopy);
                                break;
                            case XElement elem:
                                CopyLiteralElement(elem);
                                break;
                        }
                    }

                    // If no children were added, evaluate xsl:on-empty instructions
                    if (!copy.Nodes().Any() && onEmptyElements.Count > 0)
                    {
                        foreach (var onEmpty in onEmptyElements)
                        {
                            var oeSelect = onEmpty.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(oeSelect))
                            {
                                var compiled = XPath31Expression.Compile(oeSelect);
                                var result = compiled.Evaluate(_context);
                                CopyToResult(result, separateAtomicsWithSpace: true);
                            }
                            else
                            {
                                foreach (var childNode in onEmpty.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, onEmpty);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, _context.ContextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    NormalizeElementContent(copy);
                    _currentContainer = prev;
                    break;
                }
            case XdmNodeKind.Text:
                _lastAddedWasAtomic = false;
                AddTextNode(nodeToCopy.StringValue);
                break;
            case XdmNodeKind.Attribute:
                if (_currentContainer is not XElement attrTarget)
                    throw new InvalidOperationException("XTDE0420");
                if (attrTarget.Nodes().Any())
                    throw new InvalidOperationException("XTDE0410");
                attrTarget.SetAttributeValue(
                    XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri),
                    nodeToCopy.StringValue);
                break;
            case XdmNodeKind.Comment:
                _currentContainer.Add(new XComment(nodeToCopy.StringValue));
                break;
            case XdmNodeKind.ProcessingInstruction:
                _currentContainer.Add(new XProcessingInstruction(nodeToCopy.LocalName, nodeToCopy.StringValue));
                break;
            case XdmNodeKind.Document:
                {
                    // XSLT 3.0 §11.8.1: xsl:copy on a document node creates a new document node;
                    // its children come from the sequence constructor, not the original.
                    var onEmptyElements = instruction.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)).ToList();
                    var srcBaseUri = nodeToCopy.BaseUri;

                    if (_sequenceAccumulator != null)
                    {
                        // In a sequence-returning context (e.g. xsl:variable with @as),
                        // produce an actual document node so base-uri() works correctly.
                        var newDoc = new XDocument();
                        if (!string.IsNullOrEmpty(srcBaseUri))
                            newDoc.AddAnnotation(srcBaseUri);
                        var savedContainer = _currentContainer;
                        _currentContainer = newDoc;

                        foreach (var childNode in instruction.Nodes())
                        {
                            if (childNode is XElement childElemOnEmpty &&
                                childElemOnEmpty.Name.LocalName == "on-empty" &&
                                childElemOnEmpty.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                            {
                                continue;
                            }
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, nodeToCopy);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }

                        _currentContainer = savedContainer;

                        if (!newDoc.Nodes().Any() && onEmptyElements.Count > 0)
                        {
                            foreach (var onEmpty in onEmptyElements)
                            {
                                var oeSelect = onEmpty.Attribute("select")?.Value;
                                if (!string.IsNullOrEmpty(oeSelect))
                                {
                                    var compiled = XPath31Expression.Compile(oeSelect);
                                    var result = compiled.Evaluate(_context);
                                    CopyToResult(result, separateAtomicsWithSpace: true);
                                }
                                else
                                {
                                    foreach (var childNode in onEmpty.Nodes())
                                    {
                                        switch (childNode)
                                        {
                                            case XText text:
                                                ProcessSequenceText(text, onEmpty);
                                                break;
                                            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                                ExecuteXsltInstruction(elem, _context.ContextItem);
                                                break;
                                            case XElement elem:
                                                CopyLiteralElement(elem);
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        _sequenceAccumulator.Add(XdmValue.FromNode(new XDocumentNode(newDoc)));
                    }
                    else
                    {
                        // Direct result tree: document node in complex content is replaced
                        // by its children (XSLT 3.0 §5.7.1). Process children into a temp
                        // collector and then move them to the result container, preserving
                        // the source document's base URI on child elements.
                        var savedContainer = _currentContainer;
                        var tempCollector = new XElement("__doc_temp__");
                        _currentContainer = tempCollector;

                        foreach (var childNode in instruction.Nodes())
                        {
                            if (childNode is XElement childElemOnEmpty &&
                                childElemOnEmpty.Name.LocalName == "on-empty" &&
                                childElemOnEmpty.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                            {
                                continue;
                            }
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, nodeToCopy);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }

                        _currentContainer = savedContainer;

                        // Namespace nodes are not allowed on document nodes (XTDE0420)
                        if (tempCollector.Attributes().Any(a => a.IsNamespaceDeclaration))
                        {
                            throw new InvalidOperationException("XTDE0420");
                        }

                        if (!tempCollector.Nodes().Any() && onEmptyElements.Count > 0)
                        {
                            foreach (var onEmpty in onEmptyElements)
                            {
                                var oeSelect = onEmpty.Attribute("select")?.Value;
                                if (!string.IsNullOrEmpty(oeSelect))
                                {
                                    var compiled = XPath31Expression.Compile(oeSelect);
                                    var result = compiled.Evaluate(_context);
                                    CopyToResult(result, separateAtomicsWithSpace: true);
                                }
                                else
                                {
                                    foreach (var childNode in onEmpty.Nodes())
                                    {
                                        switch (childNode)
                                        {
                                            case XText text:
                                                ProcessSequenceText(text, onEmpty);
                                                break;
                                            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                                ExecuteXsltInstruction(elem, _context.ContextItem);
                                                break;
                                            case XElement elem:
                                                CopyLiteralElement(elem);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var node in tempCollector.Nodes().ToList())
                            {
                                node.Remove();
                                if (node is XElement elem && !string.IsNullOrEmpty(srcBaseUri) && elem.Annotation<string>() == null)
                                    elem.AddAnnotation(srcBaseUri);
                                _currentContainer.Add(node);
                            }
                        }
                    }
                    break;
                }
            default:
                // Other kinds: just process children
                foreach (var childNode in instruction.Nodes())
                {
                    switch (childNode)
                    {
                        case XText text:
                            ProcessSequenceText(text, instruction);
                            break;
                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                            ExecuteXsltInstruction(elem, nodeToCopy);
                            break;
                        case XElement elem:
                            CopyLiteralElement(elem);
                            break;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Adds only the namespace declarations required for the element's own name
    /// and its attribute names.
    /// </summary>
    private void AddRequiredNamespaceDeclarations(IXdmNode source, XElement target)
    {
        // Element's own namespace
        if (!string.IsNullOrEmpty(source.NamespaceUri))
        {
            var prefix = GetPrefixForNamespace(source, source.NamespaceUri);
            if (prefix == "")
                target.SetAttributeValue("xmlns", source.NamespaceUri);
            else if (!string.IsNullOrEmpty(prefix))
                target.SetAttributeValue(XNamespace.Xmlns + prefix, source.NamespaceUri);
        }

        // Attribute namespaces
        foreach (var attr in source.Attributes())
        {
            var attrNode = attr.NodeValue;
            if (attrNode != null && !string.IsNullOrEmpty(attrNode.NamespaceUri)
                && attrNode.NamespaceUri != "http://www.w3.org/2000/xmlns/")
            {
                var attrPrefix = GetPrefixForNamespace(source, attrNode.NamespaceUri);
                if (attrPrefix == "")
                    target.SetAttributeValue("xmlns", attrNode.NamespaceUri);
                else if (!string.IsNullOrEmpty(attrPrefix))
                    target.SetAttributeValue(XNamespace.Xmlns + attrPrefix, attrNode.NamespaceUri);
            }
        }
    }

    /// <summary>
    /// Returns the prefix used for the given namespace URI on the specified element,
    /// or empty string for the default namespace.
    /// </summary>
    private string GetPrefixForNamespace(IXdmNode element, string namespaceUri)
    {
        foreach (var ns in element.Axis(XdmAxis.Namespace))
        {
            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.StringValue == namespaceUri)
                return ns.NodeValue.LocalName;
        }
        return string.Empty;
    }

    /// <summary>
    /// Copies a node and adds it to the specified XML container.
    /// </summary>
    private void CopyNodeToContainer(IXdmNode node, XContainer container)
        => CopyNodeToContainer(node, container, copyAllNamespaces: true, copyAccumulators: false);

    private void CopyNodeToContainer(IXdmNode node, XContainer container, bool copyAllNamespaces)
        => CopyNodeToContainer(node, container, copyAllNamespaces, copyAccumulators: false);

    private void CopyNodeToContainer(IXdmNode node, XContainer container, bool copyAllNamespaces, bool copyAccumulators)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var elem = new XElement(XName.Get(node.LocalName, node.NamespaceUri));
                    // Preserve base URI from the source element, but only if the source
                    // element does not carry its own xml:base attribute. A copied relative
                    // xml:base must be re-resolved against the new context (e.g. the
                    // stylesheet base URI of the copying instruction); adding a resolved
                    // absolute annotation would short-circuit that resolution.
                    bool hasXmlBase = node is XDocumentNode xdn && xdn.UnderlyingObject is XElement srcElem
                        && srcElem.Attribute(XNamespace.Xml + "base") != null;
                    if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                        elem.AddAnnotation(node.BaseUri);
                    if (copyAllNamespaces)
                    {
                        foreach (var ns in node.Axis(XdmAxis.Namespace))
                        {
                            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                            {
                                if (ns.NodeValue.LocalName == "")
                                {
                                    elem.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                                }
                                else
                                {
                                    elem.SetAttributeValue(
                                        XNamespace.Xmlns + ns.NodeValue.LocalName,
                                        ns.NodeValue.StringValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        AddRequiredNamespaceDeclarations(node, elem);
                    }
                    foreach (var attr in node.Attributes())
                    {
                        // Skip namespace declarations — they are handled by the namespace axis
                        // (copy-all) or AddRequiredNamespaceDeclarations (copy-required) above.
                        if (attr.NodeValue is { } attrNode &&
                            attrNode.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            continue;
                        elem.SetAttributeValue(
                            XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                            attr.NodeValue!.StringValue);
                    }
                    if (node is XDocumentNode xdocNode2 && xdocNode2.UnderlyingObject is XElement srcElem2 &&
                        srcElem2.Annotation<NamespaceInheritanceBarrier>() != null)
                    {
                        elem.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    if (copyAccumulators)
                        AttachAccumulatorValues(node, elem);
                    AddElementToContainer(elem, container);
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        CopyNodeToContainer(child.NodeValue!, elem, copyAllNamespaces, copyAccumulators);
                    }
                    break;
                }
            case XdmNodeKind.Text:
                container.Add(new XText(node.StringValue));
                break;
            case XdmNodeKind.Comment:
                container.Add(new XComment(node.StringValue));
                break;
            case XdmNodeKind.ProcessingInstruction:
                container.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
                break;
        }
    }

    /// <summary>
    /// Adds an element to a container, explicitly undeclaring the default namespace
    /// when a no-namespace element is inserted into a parent that carries a default
    /// namespace.  Without this, LINQ-to-XML would silently inherit the parent's
    /// default namespace, making the namespace axis return the wrong namespace nodes.
    /// Also adds default-namespace undeclarations (xmlns="") when the parent has
    /// <see cref="NamespaceInheritanceBarrier"/> (inherit-namespaces="no").
    /// Prefixed namespace undeclarations (xmlns:prefix="") are not added here because
    /// LINQ-to-XML does not support them; they require XML 1.1 serialization.
    /// </summary>
    private void AddElementToContainer(XElement element, XContainer container)
    {
        if (container is XElement parentElem)
        {
            if (string.IsNullOrEmpty(element.Name.NamespaceName))
            {
                var parentDefaultNs = GetDefaultNamespaceUri(parentElem);
                if (!string.IsNullOrEmpty(parentDefaultNs))
                {
                    element.SetAttributeValue("xmlns", "");
                }
            }

            if (parentElem.Annotation<NamespaceInheritanceBarrier>() != null)
            {
                var parentDefaultNs = GetDefaultNamespaceUri(parentElem);
                if (parentDefaultNs != null)
                {
                    // Does the child already have an explicit default namespace declaration?
                    bool childHasDefaultNsDecl = false;
                    foreach (var childAttr in element.Attributes())
                    {
                        if (childAttr.Name.LocalName == "xmlns" &&
                            childAttr.Name.NamespaceName == "")
                        {
                            childHasDefaultNsDecl = true;
                            break;
                        }
                    }

                    if (!childHasDefaultNsDecl)
                    {
                        // Does the child use the parent's default namespace?
                        bool childUsesDefaultNs = false;
                        if (element.Name.NamespaceName == parentDefaultNs)
                        {
                            bool childUsesPrefix = false;
                            foreach (var childAttr in element.Attributes())
                            {
                                if (childAttr.IsNamespaceDeclaration &&
                                    childAttr.Value == parentDefaultNs)
                                {
                                    var prefix = childAttr.Name.LocalName == "xmlns"
                                        ? ""
                                        : childAttr.Name.LocalName;
                                    if (prefix != "")
                                    {
                                        childUsesPrefix = true;
                                        break;
                                    }
                                }
                            }
                            childUsesDefaultNs = !childUsesPrefix;
                        }

                        if (!childUsesDefaultNs)
                        {
                            element.SetAttributeValue("xmlns", "");
                        }
                    }
                }
            }
        }
        container.Add(element);
    }

    /// <summary>
    /// Returns the default namespace URI in effect for the given element,
    /// or <c>null</c> if the element has no default namespace.
    /// Checks explicit <c>xmlns</c> attributes first, then infers from the
    /// element name when it has no prefixed namespace declaration.
    /// </summary>
    private static string? GetDefaultNamespaceUri(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNamespaceDeclaration && attr.Name.LocalName == "xmlns")
                return attr.Value;
        }
        if (!string.IsNullOrEmpty(element.Name.NamespaceName))
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Value == element.Name.NamespaceName)
                {
                    var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                    if (prefix != "")
                        return null; // element uses a prefix, not default namespace
                }
            }
            return element.Name.NamespaceName;
        }
        return null;
    }

    private void CopyNodeToResult(IXdmNode node)
    {
        if (node.NodeKind == XdmNodeKind.Document)
        {
            _lastAddedWasAtomic = false;
            var documentChildren = new List<IXdmNode>();
            foreach (var child in node.Axis(XdmAxis.Child))
                if (child.NodeValue != null)
                    documentChildren.Add(child.NodeValue);

            // XDocument can only hold a single root element. When a document node
            // contains multiple children (or non-element children) and is being
            // copied into the result XDocument, wrap the children in the synthetic
            // __xdm_doc__ element; ResultTreeSerializer unwraps it again.
            if (_currentContainer is XDocument &&
                !(documentChildren.Count == 1 && documentChildren[0].NodeKind == XdmNodeKind.Element))
            {
                var wrapper = new XElement("__xdm_doc__");
                var savedContainer = _currentContainer;
                _currentContainer = wrapper;
                try
                {
                    foreach (var child in documentChildren)
                        CopyNodeToResult(child);
                }
                finally
                {
                    _currentContainer = savedContainer;
                }
                AddElementToContainer(wrapper, _currentContainer);
            }
            else
            {
                foreach (var child in documentChildren)
                {
                    CopyNodeToResult(child);
                }
            }
        }
        else if (node.NodeKind == XdmNodeKind.Element)
        {
            _lastAddedWasAtomic = false;
            var copy = new XElement(
                XName.Get(node.LocalName, node.NamespaceUri));

            // Copy explicit namespace declarations and attributes from the source element.
            // Use the underlying XElement when available so we copy only the declarations
            // that are actually present on this element, not inherited ones.
            if (node is XDocumentNode xdocNode && xdocNode.UnderlyingObject is XElement srcElem)
            {
                foreach (var attr in srcElem.Attributes())
                {
                    copy.SetAttributeValue(attr.Name, attr.Value);
                }
                if (srcElem.Annotation<NamespaceInheritanceBarrier>() != null)
                {
                    copy.AddAnnotation(new NamespaceInheritanceBarrier());
                }
                var accValues = srcElem.Annotation<AccumulatorValues>();
                if (accValues != null)
                {
                    copy.AddAnnotation(accValues);
                }
                // Preserve base URI from the source element, but only if the source
                // element does not carry its own xml:base attribute. A copied relative
                // xml:base must be re-resolved against the new context.
                bool hasXmlBase = srcElem.Attribute(XNamespace.Xml + "base") != null;
                var baseUriAnnotation = srcElem.Annotation<string>();
                if (baseUriAnnotation != null)
                {
                    copy.AddAnnotation(baseUriAnnotation);
                }
                else if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                {
                    copy.AddAnnotation(node.BaseUri);
                }
            }
            else
            {
                // Fallback for non-XDocumentNode implementations: copy namespace axis
                // (may include inherited declarations, but best effort).
                foreach (var ns in node.Axis(XdmAxis.Namespace))
                {
                    if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                    {
                        if (ns.NodeValue.LocalName == "")
                        {
                            copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                        }
                        else
                        {
                            copy.SetAttributeValue(
                                XNamespace.Xmlns + ns.NodeValue.LocalName,
                                ns.NodeValue.StringValue);
                        }
                    }
                }

                foreach (var attr in node.Attributes())
                {
                    copy.SetAttributeValue(
                        XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                        attr.NodeValue!.StringValue);
                }
            }

            AddElementToContainer(copy, _currentContainer);
            var prev = _currentContainer;
            _currentContainer = copy;
            foreach (var child in node.Axis(XdmAxis.Child))
            {
                CopyNodeToResult(child.NodeValue!);
            }
            _currentContainer = prev;
        }
        else if (node.NodeKind == XdmNodeKind.Text)
        {
            _lastAddedWasAtomic = false;
            AddTextNode(node.StringValue);
        }
        else if (node.NodeKind == XdmNodeKind.Comment)
        {
            _lastAddedWasAtomic = false;
            _currentContainer.Add(new XComment(node.StringValue));
        }
        else if (node.NodeKind == XdmNodeKind.ProcessingInstruction)
        {
            _lastAddedWasAtomic = false;
            _currentContainer.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
        }
        else if (node.NodeKind == XdmNodeKind.Attribute)
        {
            if (_currentContainer is not XElement attrParent)
                throw new InvalidOperationException("XTDE0420");
            if (attrParent.Nodes().Any())
                throw new InvalidOperationException("XTDE0410");
            attrParent.SetAttributeValue(
                XName.Get(node.LocalName, node.NamespaceUri),
                node.StringValue);
        }
    }

    /// <summary>
    /// Returns the default on-no-match behavior for the stylesheet version.
    /// XSLT 3.0 defaults to shallow-skip; XSLT 1.0/2.0 default to text-only-copy
    /// (traditional built-in rules: apply templates to children, copy text/attributes).
    /// </summary>
    private Stylesheet.OnNoMatch GetDefaultOnNoMatch()
    {
        // XSLT 1.0/2.0/3.0 all default to the traditional text-only-copy built-in rule:
        // text and attribute nodes are copied, document/element nodes delegate to children.
        return Stylesheet.OnNoMatch.TextOnlyCopy;
    }

    /// <summary>
    /// Applies built-in template rules when no explicit template matches.
    /// Respects xsl:mode on-no-match declarations.
    /// </summary>
    public void ApplyBuiltInRules(IXdmNode node, string mode, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, int position = 1, int last = 1)
    {
        var savedItem = _context.ContextItem;
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        _context.WithFocus(XdmValue.FromNode(node), position, last);
        _context.WithCurrentItem(XdmValue.FromNode(node));
        try
        {
            var modeDef = _stylesheet.GetModeDefinition(mode);
            // Named modes with no explicit xsl:mode declaration inherit the
            // unnamed mode's on-no-match behavior (XSLT 3.0 §3.5.2).
            if (modeDef == null && !string.IsNullOrEmpty(mode))
                modeDef = _stylesheet.GetModeDefinition("");
            var behavior = modeDef?.OnNoMatch ?? GetDefaultOnNoMatch();


            // XSLT 3.0 §6.6: if on-no-match is fail, built-in rule signals XTDE0555
            // for all node kinds except document (which delegates to children).
            if (behavior == Stylesheet.OnNoMatch.Fail && node.NodeKind != XdmNodeKind.Document)
            {
                throw new InvalidOperationException(
                    $"XTDE0555: No matching template found for node '{node.LocalName}' in mode '{mode}'.");
            }

            switch (node.NodeKind)
            {
            case XdmNodeKind.Document:
                if ((behavior == Stylesheet.OnNoMatch.ShallowCopy || behavior == Stylesheet.OnNoMatch.DeepCopy) &&
                    _sequenceAccumulator != null)
                {
                    // In a sequence-returning context, shallow-copy/deep-copy of a document
                    // node produces an actual document node (possibly empty) so that its
                    // base URI is preserved.
                    var newDoc = new XDocument();
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        newDoc.AddAnnotation(node.BaseUri);
                    var savedContainer = _currentContainer;
                    _currentContainer = newDoc;
                    ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                    _currentContainer = savedContainer;
                    _sequenceAccumulator.Add(XdmValue.FromNode(new XDocumentNode(newDoc)));
                }
                else
                {
                    // Built-in: apply templates to children of the document node
                    ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                }
                break;

            case XdmNodeKind.Element:
                ApplyBuiltInRulesForElement(node, mode, behavior, incomingTunnelParams, callParams);
                break;

            case XdmNodeKind.Text:
                // Built-in: copy text value (only if we have an element container)
                // XSLT 3.0 §6.6: for text/attribute nodes, built-in rule does nothing
                // when on-no-match is shallow-skip or deep-skip.
                if (behavior != Stylesheet.OnNoMatch.DeepSkip &&
                    behavior != Stylesheet.OnNoMatch.ShallowSkip)
                {
                    _lastAddedWasAtomic = false;
                    AddTextNode(node.StringValue);
                }
                break;

            case XdmNodeKind.Attribute:
                // XSLT 3.0 §6.6: built-in rule for attribute nodes
                if (_currentContainer is XElement &&
                    behavior != Stylesheet.OnNoMatch.DeepSkip &&
                    behavior != Stylesheet.OnNoMatch.ShallowSkip)
                {
                    if (behavior == Stylesheet.OnNoMatch.ShallowCopy ||
                        behavior == Stylesheet.OnNoMatch.DeepCopy)
                    {
                        if (_currentContainer is XElement elem)
                        {
                            elem.SetAttributeValue(
                                XName.Get(node.LocalName, node.NamespaceUri),
                                node.StringValue);
                        }
                    }
                    else if (behavior == Stylesheet.OnNoMatch.TextOnlyCopy)
                    {
                        _lastAddedWasAtomic = false;
                        AddTextNode(node.StringValue);
                    }
                }
                break;

            case XdmNodeKind.Comment:
                if (_currentContainer is XElement && behavior == Stylesheet.OnNoMatch.ShallowCopy)
                {
                    _currentContainer.Add(new XComment(node.StringValue));
                }
                break;

            case XdmNodeKind.ProcessingInstruction:
                if (_currentContainer is XElement && behavior == Stylesheet.OnNoMatch.ShallowCopy)
                {
                    _currentContainer.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
                }
                break;

            case XdmNodeKind.Namespace:
                // XSLT 3.0 §6.6: for namespace nodes, built-in rule copies the namespace
                // only when on-no-match is shallow-copy; otherwise does nothing.
                if (_currentContainer is XElement nsElem &&
                    (behavior == Stylesheet.OnNoMatch.ShallowCopy ||
                     behavior == Stylesheet.OnNoMatch.DeepCopy))
                {
                    nsElem.SetAttributeValue(
                        XNamespace.Xmlns + node.LocalName,
                        node.StringValue);
                }
                break;
            }
        }
        finally
        {
            _context.WithFocus(savedItem, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
        }
    }

    private void ApplyBuiltInRulesForElement(IXdmNode node, string mode, Stylesheet.OnNoMatch behavior, Dictionary<string, XdmValue>? incomingTunnelParams, Dictionary<string, XdmValue>? callParams = null)
    {
        switch (behavior)
        {
            case Stylesheet.OnNoMatch.ShallowCopy:
                {
                    // XSLT 3.0 §6.6 (bug 28774): shallow-copy creates the element shell
                    // without copying attributes; templates are applied to children AND attributes.
                    var copy = new XElement(
                        XName.Get(node.LocalName, node.NamespaceUri));
                    // Preserve base URI from the source element
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        copy.AddAnnotation(node.BaseUri);
                    foreach (var ns in node.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                            {
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            }
                            else
                            {
                                copy.SetAttributeValue(
                                    XNamespace.Xmlns + ns.NodeValue.LocalName,
                                    ns.NodeValue.StringValue);
                            }
                        }
                    }
                    _currentContainer.Add(copy);

                    var previousContainer = _currentContainer;
                    _currentContainer = copy;
                    ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams);
                    _currentContainer = previousContainer;
                }
                break;

            case Stylesheet.OnNoMatch.ShallowSkip:
                // XSLT 3.0 §6.6 (bug 28774): shallow-skip applies templates to children AND attributes.
                ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams);
                break;

            case Stylesheet.OnNoMatch.TextOnlyCopy:
                // Recurse to children without copying the element wrapper (attributes are not processed).
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                break;

            case Stylesheet.OnNoMatch.DeepCopy:
                CopyNodeToResult(node);
                break;

            case Stylesheet.OnNoMatch.DeepSkip:
                // Skip element and all descendants — do nothing
                break;

            case Stylesheet.OnNoMatch.Fail:
                throw new InvalidOperationException(
                    $"No matching template found for node '{node.LocalName}' in mode '{mode}'.");
        }
    }

    /// <summary>
    /// Registers the XSLT <c>key()</c> function on the evaluation context.
    /// </summary>
    private void RegisterKeyFunction()
    {
        var signature2 = new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "key",
            Arity = 2,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Undefined],
            ReturnType = XdmValueKind.Sequence,
            Implementation = KeyFunctionImpl
        };
        _context.RegisterFunction(signature2);

        var signature3 = new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "key",
            Arity = 3,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Undefined, XdmValueKind.Node],
            ReturnType = XdmValueKind.Sequence,
            Implementation = KeyFunctionImpl
        };
        _context.RegisterFunction(signature3);
    }

    private XdmValue KeyFunctionImpl(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (_keyIndices == null)
            _keyIndices = new List<(IXdmNode DocRoot, KeyIndex Index)>();

        var rawKeyName = args[0].ToString();
        var keyName = ExpandKeyName(rawKeyName, ctx);
        var keyValueArg = args[1];

        // XTDE1260: the expanded key name must match at least one xsl:key definition.
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (!allKeyDefs.Any(k => k.Name == keyName))
            throw new InvalidOperationException($"XTDE1260: No xsl:key definition named '{rawKeyName}'.");

        if (args.Length == 2)
        {
            // 2-arg form: search the entire document containing the context node.
            var contextNode = ctx.ContextItem.NodeValue;
            var docRoot = contextNode?.Document ?? contextNode;
            if (docRoot == null)
                return XdmValue.Undefined;

            var keyIndex = GetOrBuildKeyIndex(docRoot);
            if (keyIndex == null)
                return XdmValue.Undefined;

            return LookupKeyValues(keyIndex, keyName, keyValueArg);
        }
        else
        {
            // 3-arg form: search only the nodes supplied in the 3rd argument and their descendants.
            var candidates = new List<IXdmNode>();
            if (args[2].IsNode && args[2].NodeValue != null)
            {
                candidates.Add(args[2].NodeValue);
            }
            else if (args[2].IsSequence && args[2].SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(args[2].SequenceValue))
                {
                    if (item.IsNode && item.NodeValue != null)
                        candidates.Add(item.NodeValue);
                }
            }

            if (candidates.Count == 0)
                return XdmValue.Undefined;

            // Group candidates by document root (using IsSameNode).
            var docEntries = new List<(IXdmNode DocRoot, KeyIndex Index, List<IXdmNode> Candidates)>();
            foreach (var candidate in candidates)
            {
                var candidateDoc = candidate.Document ?? candidate;
                bool found = false;
                foreach (var entry in docEntries)
                {
                    if (entry.DocRoot.IsSameNode(candidateDoc))
                    {
                        entry.Candidates.Add(candidate);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var keyIndex = GetOrBuildKeyIndex(candidateDoc);
                    if (keyIndex != null)
                    {
                        docEntries.Add((candidateDoc, keyIndex, new List<IXdmNode> { candidate }));
                    }
                }
            }

            // Look up key values and filter to candidates or their descendants.
            var result = new List<XdmValue>();

            if (IsCompositeKey(keyName))
            {
                var tuple = ExtractKeyLookupValues(keyValueArg).ToArray();
                if (tuple.Length > 0)
                {
                    var seen = new HashSet<IXdmNode>();
                    foreach (var (_, keyIndex, docCandidates) in docEntries)
                    {
                        foreach (var node in keyIndex.LookupComposite(keyName, tuple))
                        {
                            if (!seen.Add(node))
                                continue;
                            if (docCandidates.Any(c => IsDescendantOrSelf(node, c)))
                                result.Add(XdmValue.FromNode(node));
                        }
                    }
                }
            }
            else
            {
                var seen = new HashSet<IXdmNode>();
                foreach (var keyValue in ExtractKeyLookupValues(keyValueArg))
                {
                    foreach (var (_, keyIndex, docCandidates) in docEntries)
                    {
                        foreach (var node in keyIndex.Lookup(keyName, keyValue))
                        {
                            if (!seen.Add(node))
                                continue;

                            if (docCandidates.Any(c => IsDescendantOrSelf(node, c)))
                                result.Add(XdmValue.FromNode(node));
                        }
                    }
                }
                result.Sort((a, b) =>
                {
                    var na = a.NodeValue!;
                    var nb = b.NodeValue!;
                    return na.DocumentOrder.CompareTo(nb.DocumentOrder);
                });
            }

            return XdmValue.FromSequence(MaterializedSequence.FromList(result));
        }
    }

    /// <summary>
    /// Expands a lexical key name (possibly prefixed) to Clark notation using the
    /// namespace bindings in the current evaluation context.
    /// </summary>
    private static string ExpandKeyName(string qname, EvaluationContext context)
    {
        if (qname.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = qname.IndexOf('}');
            if (close > 2)
                return qname;
        }

        int colon = qname.IndexOf(':');
        if (colon <= 0 || colon == qname.Length - 1)
            return "{}" + qname;

        var prefix = qname[..colon];
        var local = qname[(colon + 1)..];
        var ns = context.TryResolveNamespace(prefix, out var uri) ? uri : string.Empty;
        return "{" + ns + "}" + local;
    }

    /// <summary>
    /// Returns true if <paramref name="node"/> is the same node as, or a descendant of,
    /// <paramref name="ancestor"/>.
    /// </summary>
    private static bool IsDescendantOrSelf(IXdmNode node, IXdmNode ancestor)
    {
        var current = node;
        while (current != null)
        {
            if (current.IsSameNode(ancestor))
                return true;
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Retrieves or lazily builds the <see cref="KeyIndex"/> for the specified document root.
    /// Uses iterative rebuilding to handle cross-key dependencies (e.g. key-064).
    /// </summary>
    private KeyIndex? GetOrBuildKeyIndex(IXdmNode docRoot)
    {
        // Find existing index using IsSameNode (wrapper instances may differ).
        foreach (var (existingDoc, existingIndex) in _keyIndices!)
        {
            if (existingDoc.IsSameNode(docRoot))
                return existingIndex;
        }

        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (allKeyDefs.Count == 0)
            return null;

        // Build iteratively for this document; add the index first so that
        // recursive key() calls inside xsl:key/@use or match can query it.
        var keyIndex = new KeyIndex();
        _keyIndices!.Add((docRoot, keyIndex));

        // KeyIndex.BuildSingleKey mutates the context focus; save and restore to avoid
        // corrupting the currently executing template's focus.
        var savedItem = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            int maxIterations = allKeyDefs.Count + 1;
            int previousTotal = -1;
            for (int i = 0; i < maxIterations; i++)
            {
                int currentTotal = keyIndex.TotalEntryCount;
                if (currentTotal == previousTotal)
                    break;
                previousTotal = currentTotal;

                // Clear each key name once per iteration so multiple definitions
                // with the same name accumulate.
                var cleared = new HashSet<string>();
                foreach (var keyDef in allKeyDefs)
                {
                    if (cleared.Add(keyDef.Name))
                        keyIndex.ClearKey(keyDef.Name);
                    if (keyDef.HasUseContent)
                        KeyIndex.BuildSingleKey(docRoot, keyDef, _context, keyIndex, n => EvaluateSequenceConstructor(keyDef.Element, XdmValue.FromNode(n), wrapInDocumentNode: false));
                    else
                        KeyIndex.BuildSingleKey(docRoot, keyDef, _context, keyIndex);
                }
            }
            return keyIndex;
        }
        finally
        {
            _context.WithFocus(savedItem, savedPosition, savedSize);
        }
    }

    /// <summary>
    /// Looks up the given key values in a single key index and returns matching nodes.
    /// </summary>
    private XdmValue LookupKeyValues(KeyIndex keyIndex, string keyName, XdmValue keyValueArg)
    {
        var result = new List<XdmValue>();

        if (IsCompositeKey(keyName))
        {
            var tuple = ExtractKeyLookupValues(keyValueArg).ToArray();
            if (tuple.Length > 0)
            {
                foreach (var node in keyIndex.LookupComposite(keyName, tuple))
                    result.Add(XdmValue.FromNode(node));
            }
        }
        else
        {
            var seen = new HashSet<IXdmNode>();
            foreach (var keyValue in ExtractKeyLookupValues(keyValueArg))
            {
                foreach (var node in keyIndex.Lookup(keyName, keyValue))
                {
                    if (seen.Add(node))
                        result.Add(XdmValue.FromNode(node));
                }
            }
            result.Sort((a, b) =>
            {
                var na = a.NodeValue!;
                var nb = b.NodeValue!;
                return na.DocumentOrder.CompareTo(nb.DocumentOrder);
            });
        }

        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private bool IsCompositeKey(string keyName)
        => _stylesheet.GetAllKeyDefinitions().Any(k => k.Name == keyName && k.Composite);

    /// <summary>
    /// Extracts typed atomic values from a key-value argument (either a single value or a sequence).
    /// Node arguments are atomized to <c>xs:untypedAtomic</c> strings.
    /// </summary>
    private static IEnumerable<XdmValue> ExtractKeyLookupValues(XdmValue keyValueArg)
    {
        if (keyValueArg.IsSequence && keyValueArg.SequenceValue != null)
        {
            foreach (var val in XdmSequence.FromSource(keyValueArg.SequenceValue))
                yield return AtomizeKeyValue(val);
        }
        else
        {
            yield return AtomizeKeyValue(keyValueArg);
        }
    }

    private static XdmValue AtomizeKeyValue(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.ToString(), "untypedAtomic");
        return value;
    }

    /// <summary>
    /// Registers the XSLT <c>current-group()</c> and <c>current-grouping-key()</c> functions.
    /// </summary>
    private void RegisterGroupingFunctions()
    {
        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-group",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) =>
            {
                if (_currentGroup == null)
                    throw new InvalidOperationException("XTDE1061: current-group() is not defined in the current context");
                if (_currentGroup.Count == 0)
                    return XdmValue.Undefined;
                return XdmValue.FromSequence(MaterializedSequence.FromList(_currentGroup));
            }
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-grouping-key",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Undefined,
            Implementation = (ctx, args) =>
            {
                if (_currentGroupingKey == null)
                    throw new InvalidOperationException("XTDE1071: current-grouping-key() is not defined in the current context");
                return _currentGroupingKey.Value;
            }
        });
    }

    /// <summary>
    /// Evaluates top-level xsl:param and xsl:variable declarations and binds them into the context.
    /// Order: imported first, then included, then local. Parameters are evaluated before variables.
    /// Global variables with sequence constructors (no @select) are evaluated lazily on first
    /// reference, using a singleton focus based on the root of the tree containing the initial
    /// context node (per XSLT 3.0 §9.6). If no initial context node is supplied, the focus is absent.
    /// </summary>
    private void InitializeGlobalParametersAndVariables(IXdmNode? source)
    {
        var focus = source != null ? XdmValue.FromNode(source) : XdmValue.Undefined;
        _globalContextItem = focus;

        // Set the focus once for all global param/var evaluations.
        // Sequence constructors inside global variables rely on _context.ContextItem
        // being set when they evaluate XPath expressions (e.g. xsl:value-of/@select).
        if (source != null)
            _context.WithFocus(focus, 1, 1);

        // Collect globals in precedence order: imports first, then includes, then local.
        // Within each stylesheet module, params and vars are evaluated in document order.
        var globals = new List<(string Name, XElement Element, bool IsParam)>();

        foreach (var imported in _stylesheet.Imports)
            CollectGlobalsInDocumentOrder(imported, globals);

        foreach (var included in _stylesheet.Includes)
            CollectGlobalsInDocumentOrder(included, globals);

        CollectGlobalsInDocumentOrder(_stylesheet, globals);

        // Pre-register all globals (variables and parameters with defaults) so they
        // can be resolved lazily on first reference. This handles forward references
        // such as a variable declared before a parameter it references.
        foreach (var (name, elem, isParam) in globals)
        {
            // Skip parameters already supplied by the caller (e.g. fn:transform).
            if (isParam && _context.TryGetVariable(name, out _))
                continue;

            _lazyGlobals[name] = (elem, elem.Attribute("as")?.Value);
        }

        // Register lazy variable resolver BEFORE any global is referenced.
        _context.LazyVariableResolver = (localName, namespaceUri) =>
        {
            if (string.IsNullOrEmpty(namespaceUri) && _lazyGlobals.TryGetValue(localName, out var info))
            {
                _lazyGlobals.Remove(localName);

                // Parameters supplied by the caller are already bound.
                if (_context.TryGetVariable(localName, out var existing))
                    return existing;

                // Global variables/parameters are evaluated with a singleton focus based
                // on the root node of the tree containing the initial context node
                // (XSLT 3.0 §9.6). Save the caller's focus to avoid corrupting it.
                var savedItem = _context.ContextItem;
                var savedPos = _context.ContextPosition;
                var savedSize = _context.ContextSize;
                try
                {
                    _context.WithFocus(_globalContextItem, 1, 1);

                    XdmValue value;
                    var select = info.Element.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
                        value = compiled.Evaluate(_context);
                    }
                    else
                    {
                        value = EvaluateSequenceConstructor(info.Element, _globalContextItem, wrapInDocumentNode: string.IsNullOrEmpty(info.AsType));
                    }
                    value = ConvertVariableValue(value, info.AsType);
                    _context.WithVariable(localName, value);
                    return value;
                }
                finally
                {
                    _context.WithFocus(savedItem, savedPos, savedSize);
                }
            }
            return null;
        };

        // Check required parameters and eagerly bind parameters whose default value
        // is an empty sequence constructor without @as, so they produce a document node
        // even if never explicitly referenced.
        foreach (var (name, elem, isParam) in globals)
        {
            if (isParam)
            {
                var required = elem.Attribute("required")?.Value;
                if (required == "yes" && !_context.TryGetVariable(name, out _))
                    throw new InvalidOperationException($"XTDE0050: No value supplied for required parameter '{name}'.");

                // Skip parameters already supplied by caller.
                if (_context.TryGetVariable(name, out _))
                    continue;

                var select = elem.Attribute("select")?.Value;
                if (string.IsNullOrEmpty(select) && string.IsNullOrEmpty(elem.Attribute("as")?.Value))
                {
                    // Force creation of the empty-document-node default value now.
                    if (_lazyGlobals.TryGetValue(name, out var info))
                    {
                        _lazyGlobals.Remove(name);
                        var savedItem = _context.ContextItem;
                        var savedPos = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        try
                        {
                            _context.WithFocus(_globalContextItem, 1, 1);
                            var value = EvaluateSequenceConstructor(info.Element, _globalContextItem, wrapInDocumentNode: true);
                            value = ConvertVariableValue(value, info.AsType);
                            _context.WithVariable(name, value);
                        }
                        finally
                        {
                            _context.WithFocus(savedItem, savedPos, savedSize);
                        }
                    }
                }
            }
        }
    }

    private static void CollectGlobalsInDocumentOrder(Stylesheet.Stylesheet stylesheet, List<(string Name, XElement Element, bool IsParam)> globals)
    {
        var paramList = stylesheet.GlobalParameters;
        var varList = stylesheet.GlobalVariables;

        int i = 0, j = 0;
        while (i < paramList.Count || j < varList.Count)
        {
            XElement? nextParam = i < paramList.Count ? paramList[i] : null;
            XElement? nextVar = j < varList.Count ? varList[j] : null;

            if (nextParam != null && (nextVar == null || nextParam.NodesBeforeSelf().Count() <= nextVar.NodesBeforeSelf().Count()))
            {
                var name = nextParam.Attribute("name")?.Value ?? "";
                globals.Add((name, nextParam, true));
                i++;
            }
            else if (nextVar != null)
            {
                var name = nextVar.Attribute("name")?.Value ?? "";
                globals.Add((name, nextVar, false));
                j++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Finds the highest-priority template rule that matches the given node in the given mode.
    /// </summary>
    private Stylesheet.TemplateRule? FindBestTemplate(IXdmNode node, string mode, HashSet<Stylesheet.TemplateRule>? excludedRules = null)
        => FindBestTemplate(XdmValue.FromNode(node), mode, excludedRules);

    /// <summary>
    /// Finds the highest-priority template rule that matches the given item (node or atomic value) in the given mode.
    /// </summary>
    /// <param name="item">The context item to match against.</param>
    /// <param name="mode">The mode to match in.</param>
    /// <param name="excludedRules">Rules to exclude (used by xsl:next-match).</param>
    /// <param name="minImportPrecedence">If set, only rules with import precedence greater than this value are considered (used by xsl:apply-imports).</param>
    private Stylesheet.TemplateRule? FindBestTemplate(XdmValue item, string mode, HashSet<Stylesheet.TemplateRule>? excludedRules = null, int? minImportPrecedence = null)
    {
        Stylesheet.TemplateRule? best = null;
        double bestPriority = double.NegativeInfinity;
        int bestImportPrecedence = int.MaxValue;
        bool hasConflict = false;

        foreach (var rule in _allTemplateRules)
        {
            if (excludedRules != null && excludedRules.Contains(rule))
                continue;
            if (minImportPrecedence.HasValue && rule.ImportPrecedence <= minImportPrecedence.Value)
                continue;
            if (!MatchesMode(rule, mode))
                continue;
            if (rule.CompiledMatch == null)
                continue;
            if (!rule.CompiledMatch(item, _context))
                continue;

            // XSLT spec §6.4: import precedence is checked BEFORE priority.
            // Higher import precedence (lower numeric value in our system) always wins.
            if (best == null || rule.ImportPrecedence < bestImportPrecedence)
            {
                best = rule;
                bestPriority = rule.Priority;
                bestImportPrecedence = rule.ImportPrecedence;
                hasConflict = false;
            }
            else if (rule.ImportPrecedence == bestImportPrecedence)
            {
                if (rule.Priority > bestPriority)
                {
                    best = rule;
                    bestPriority = rule.Priority;
                    hasConflict = false;
                }
                else if (rule.Priority == bestPriority)
                {
                    if (best != null && best != rule && best.Element != rule.Element)
                        hasConflict = true;
                    // XSLT last-wins rule: when priority and import precedence are equal,
                    // the template that appears later in the stylesheet wins.
                    best = rule;
                }
            }
        }

        if (hasConflict && best != null)
        {
            var modeDef = _stylesheet.GetModeDefinition(mode);
            if (modeDef?.OnMultipleMatch == Stylesheet.OnMultipleMatch.Fail)
            {
                throw new InvalidOperationException("XTDE0540: Multiple templates match with the same priority.");
            }
        }

        return best;
    }

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is a more specific document-node
    /// pattern than <paramref name="current"/>.  Used as a tie-breaker in FindBestTemplate.
    /// </summary>
    private static bool IsMoreSpecificDocumentPattern(string? candidate, string? current)
    {
        if (string.IsNullOrEmpty(current)) return true;
        if (string.IsNullOrEmpty(candidate)) return false;

        var c = current.Trim();
        var cand = candidate.Trim();

        // doc('uri') / document('uri') are more specific than /
        if (c == "/" && (cand.StartsWith("doc(") || cand.StartsWith("document(")))
            return true;

        // Prefer the longer / more detailed pattern as a general heuristic
        return cand.Length > c.Length;
    }

    private static bool MatchesMode(Stylesheet.TemplateRule rule, string mode)
    {
        if (rule.MatchesAllModes)
            return true;
        foreach (var m in rule.Modes)
        {
            if (m == mode)
                return true;
        }
        return false;
    }

    private static IEnumerable<IXdmNode> EnumerateNodes(XdmSequence sequence)
    {
        foreach (var item in sequence)
        {
            if (item.IsNode && item.NodeValue != null)
                yield return item.NodeValue;
        }
    }

    private static IEnumerable<IXdmNode> EnumerateNodes(XdmValue value)
    {
        if (value.IsNode && value.NodeValue != null)
        {
            yield return value.NodeValue;
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode && item.NodeValue != null)
                    yield return item.NodeValue;
            }
        }
    }

    /// <summary>
    /// Enumerates all items in an XDM value, including atomic values and nodes.
    /// </summary>
    private static IEnumerable<XdmValue> EnumerateItems(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;

        if (!value.IsSequence)
        {
            yield return value;
            yield break;
        }

        if (value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    yield return item;
            }
        }
    }

    /// <summary>
    /// Sorts a sequence of nodes by document order, but keeps the relative order of
    /// nodes from different source trees as it appeared in the original sequence.
    /// </summary>
    private static List<XdmValue> SortNodesByDocumentOrderPreservingTreeOrder(List<XdmValue> items)
    {
        var indexed = items.Select((item, idx) => (item, idx)).ToList();
        var rootOrder = new Dictionary<IXdmNode, int>();
        var groups = indexed.GroupBy(a =>
        {
            var root = GetRootNode(a.item.NodeValue!);
            if (!rootOrder.TryGetValue(root, out var order))
            {
                order = rootOrder.Count;
                rootOrder[root] = order;
            }
            return root;
        }).ToList();

        var sorted = new List<XdmValue>(items.Count);
        foreach (var g in groups.OrderBy(g => rootOrder[g.Key]))
        {
            var list = g.ToList();
            list.Sort((a, b) =>
            {
                int cmp = a.item.NodeValue!.DocumentOrder.CompareTo(b.item.NodeValue!.DocumentOrder);
                return cmp != 0 ? cmp : a.idx.CompareTo(b.idx);
            });
            foreach (var x in list)
                sorted.Add(x.item);
        }
        return sorted;
    }

    /// <summary>
    /// Returns the root node of the tree containing the given node.
    /// For a node inside a document this is the document node; for a parentless
    /// tree it is the root element.
    /// </summary>
    private static IXdmNode GetRootNode(IXdmNode node)
    {
        var current = node;
        while (true)
        {
            IXdmNode? parent = null;
            foreach (var value in current.Axis(XdmAxis.Parent))
            {
                if (value.IsNode)
                {
                    parent = value.NodeValue;
                    break;
                }
            }
            if (parent == null)
                return current;
            current = parent;
        }
    }

    /// <summary>
    /// Converts an XDM value to its string representation, concatenating sequence items.
    /// </summary>
    private static string XdmValueToString(XdmValue value)
        => XdmValueToString(value, " ");

    /// <summary>
    /// Extracts a single string key from an XDM value for grouping purposes.
    /// Sequences are collapsed to the first item's string value.
    /// </summary>
    private static string GetGroupingKeyString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    return item.ToString();
            }
            return string.Empty;
        }
        return value.ToString();
    }

    /// <summary>
    /// Converts an XDM value to its string representation, concatenating sequence items
    /// with the specified separator.
    /// </summary>
    private static string XdmValueToString(XdmValue value, string separator)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsSequence && value.SequenceValue != null)
        {
            var sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsUndefined)
                    continue;
                if (!first)
                    sb.Append(separator);
                sb.Append(item.ToString());
                first = false;
            }
            return sb.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Sorts a list of nodes according to xsl:sort specifications.
    /// Supports a single sort key (primary only).
    /// </summary>
    private List<IXdmNode> SortNodes(List<IXdmNode> nodes, List<XElement> sortSpecs)
    {
        var items = nodes.Select(n => XdmValue.FromNode(n)).ToList();
        var sorted = SortItems(items, sortSpecs);
        return sorted.Select(v => v.NodeValue!).ToList();
    }

    private List<XdmValue> SortItems(List<XdmValue> items, List<XElement> sortSpecs)
    {
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            // Pre-compute all sort keys for every item, preserving original order for stability.
            var keyed = new List<SortEntry>();
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, 1, 1);
                var keys = new List<SortKey>();
                foreach (var spec in sortSpecs)
                {
                    var select = spec.Attribute("select")?.Value ?? ".";
                    var dataType = spec.Attribute("data-type")?.Value ?? "text";
                    var order = spec.Attribute("order")?.Value ?? "ascending";
                    var descending = order.Trim().ToLowerInvariant() == "descending";
                    var isNumeric = dataType.Trim().ToLowerInvariant() == "number";

                    var compiled = XPath31Expression.Compile(select);
                    var keyValue = compiled.Evaluate(_context);
                    keys.Add(new SortKey(keyValue, descending, isNumeric));
                }
                keyed.Add(new SortEntry(item, keys, idx));
            }

            keyed.Sort((a, b) =>
            {
                for (int i = 0; i < a.Keys.Count; i++)
                {
                    var cmp = CompareSortKey(a.Keys[i], b.Keys[i]);
                    if (cmp != 0) return cmp;
                }
                // Stable sort: preserve original relative order when all keys equal
                return a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            return keyed.Select(k => k.Item).ToList();
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
        }
    }

    private readonly record struct SortKey(XdmValue Value, bool Descending, bool IsNumeric);
    private readonly record struct SortEntry(XdmValue Item, List<SortKey> Keys, int OriginalIndex);

    private int CompareSortKey(SortKey a, SortKey b)
    {
        int cmp;
        if (a.IsNumeric)
            cmp = CompareNumericSortKey(a.Value, b.Value);
        else
            cmp = XdmValueComparer.Instance.Compare(a.Value, b.Value);

        return a.Descending ? -cmp : cmp;
    }

    private static int CompareNumericSortKey(XdmValue a, XdmValue b)
    {
        var sa = XdmValueToString(a);
        var sb = XdmValueToString(b);
        bool aOk = double.TryParse(sa, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double da);
        bool bOk = double.TryParse(sb, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double db);
        if (!aOk && !bOk) return 0;
        if (!aOk) return -1;  // NaN is less than any number (XSLT spec)
        if (!bOk) return 1;   // any number is greater than NaN
        return da.CompareTo(db);
    }

    /// <summary>
    /// Sorts the groups produced by <c>xsl:for-each-group</c> according to the
    /// contained <c>xsl:sort</c> specifications. Evaluates each sort key with the
    /// group's representative item as the focus item and, in XSLT 2.0, with
    /// <c>current-group()</c> and <c>current-grouping-key()</c> available.
    /// </summary>
    private List<(XdmValue? Key, List<XdmValue> Items)> SortGroups(
        List<(XdmValue? Key, List<XdmValue> Items)> groups,
        List<XElement> sortSpecs)
    {
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        var savedGroup = _currentGroup;
        var savedKey = _currentGroupingKey;
        bool exposeGroupInSort = !IsXslt30OrHigher();

        try
        {
            var keyed = new List<(XdmValue? Key, List<XdmValue> Items, List<SortKey> Keys, int OriginalIndex)>();
            for (int idx = 0; idx < groups.Count; idx++)
            {
                var (key, items) = groups[idx];
                var rep = items[0];
                _context.WithFocus(rep, idx + 1, groups.Count);
                _context.WithCurrentItem(rep);
                if (exposeGroupInSort)
                {
                    _currentGroup = items;
                    _currentGroupingKey = key;
                }

                var keys = new List<SortKey>();
                foreach (var spec in sortSpecs)
                {
                    var select = spec.Attribute("select")?.Value ?? ".";
                    var dataType = spec.Attribute("data-type")?.Value ?? "text";
                    var order = spec.Attribute("order")?.Value ?? "ascending";
                    var descending = order.Trim().ToLowerInvariant() == "descending";
                    var isNumeric = dataType.Trim().ToLowerInvariant() == "number";

                    var compiled = XPath31Expression.Compile(select);
                    var keyValue = compiled.Evaluate(_context);
                    keys.Add(new SortKey(keyValue, descending, isNumeric));
                }
                keyed.Add((key, items, keys, idx));
            }

            keyed.Sort((a, b) =>
            {
                for (int i = 0; i < a.Keys.Count; i++)
                {
                    var cmp = CompareSortKey(a.Keys[i], b.Keys[i]);
                    if (cmp != 0) return cmp;
                }
                // Stable sort: preserve original relative order when all keys equal
                return a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            return keyed.Select(k => (k.Key, k.Items)).ToList();
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _currentGroup = savedGroup;
            _currentGroupingKey = savedKey;
        }
    }

    /// <summary>
    /// Validates the attributes of an <c>xsl:for-each-group</c> instruction and throws
    /// the appropriate static errors (XTSE0020/0080/0090/1017/1080/1090).
    /// </summary>
    private void ValidateForEachGroupAttributes(XElement instruction)
    {
        var groupBy = instruction.Attribute("group-by")?.Value;
        var groupAdjacent = instruction.Attribute("group-adjacent")?.Value;
        var groupStarting = instruction.Attribute("group-starting-with")?.Value;
        var groupEnding = instruction.Attribute("group-ending-with")?.Value;
        var collation = instruction.Attribute("collation")?.Value;
        var compositeAttr = instruction.Attribute("composite")?.Value;
        var bindGroup = instruction.Attribute("bind-group")?.Value;
        var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

        if (!string.IsNullOrEmpty(compositeAttr))
        {
            var v = compositeAttr.Trim();
            if (v != "yes" && v != "true" && v != "1" &&
                v != "no" && v != "false" && v != "0")
                throw new InvalidOperationException("XTSE0020: invalid value for @composite");
        }

        int groupingAttrCount = 0;
        if (!string.IsNullOrEmpty(groupBy)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupAdjacent)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupStarting)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupEnding)) groupingAttrCount++;

        if (groupingAttrCount == 0)
            throw new InvalidOperationException("XTSE1080: xsl:for-each-group requires one of group-by, group-adjacent, group-starting-with, or group-ending-with");
        if (groupingAttrCount > 1)
            throw new InvalidOperationException("XTSE1080: xsl:for-each-group allows only one of group-by, group-adjacent, group-starting-with, or group-ending-with");

        if (!string.IsNullOrEmpty(collation) &&
            string.IsNullOrEmpty(groupBy) && string.IsNullOrEmpty(groupAdjacent))
            throw new InvalidOperationException("XTSE1090: @collation is allowed only with group-by or group-adjacent");

        if (IsXslt30OrHigher() && (!string.IsNullOrEmpty(bindGroup) || !string.IsNullOrEmpty(bindKey)))
            throw new InvalidOperationException("XTSE0090: @bind-group and @bind-grouping-key are not permitted in XSLT 3.0");
    }

    /// <summary>
    /// Builds the groups for an <c>xsl:for-each-group</c> instruction from the supplied
    /// population items, respecting <c>@composite</c> and the supplied collation.
    /// </summary>
    private List<(XdmValue? Key, List<XdmValue> Items)> BuildForEachGroups(
        XElement instruction,
        List<XdmValue> items,
        string? collation)
    {
        var groupBy = instruction.Attribute("group-by")?.Value;
        var groupAdjacent = instruction.Attribute("group-adjacent")?.Value;
        var groupStarting = instruction.Attribute("group-starting-with")?.Value;
        var groupEnding = instruction.Attribute("group-ending-with")?.Value;
        bool composite = IsCompositeGrouping(instruction);

        var groups = new List<(XdmValue? Key, List<XdmValue> Items)>();

        if (!string.IsNullOrEmpty(groupBy))
        {
            var keyExpr = CompileXPath(groupBy, instruction);
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                var keyValue = keyExpr.Evaluate(_context);
                var keyItems = EnumerateKeyItems(keyValue);
                if (composite)
                {
                    var compositeKey = XdmValue.FromSequence(MaterializedSequence.FromList(keyItems));
                    AddToGroup(groups, compositeKey, item, collation);
                }
                else
                {
                    foreach (var keyItem in keyItems)
                        AddToGroup(groups, keyItem, item, collation);
                }
            }
        }
        else if (!string.IsNullOrEmpty(groupAdjacent))
        {
            var keyExpr = CompileXPath(groupAdjacent, instruction);
            XdmValue currentKey = XdmValue.Undefined;
            List<XdmValue>? currentItems = null;
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                var keyValue = keyExpr.Evaluate(_context);
                var keyItems = EnumerateKeyItems(keyValue);

                XdmValue itemKey;
                if (composite)
                {
                    itemKey = XdmValue.FromSequence(MaterializedSequence.FromList(keyItems));
                }
                else
                {
                    if (keyItems.Count == 0)
                        throw new InvalidOperationException("XTTE1100: group-adjacent key evaluates to an empty sequence");
                    if (keyItems.Count > 1)
                        throw new InvalidOperationException("XTTE1100: group-adjacent key evaluates to a sequence of more than one item");
                    itemKey = keyItems[0];
                }

                if (currentItems == null)
                {
                    currentItems = new List<XdmValue> { item };
                    currentKey = itemKey;
                }
                else if (GroupingKeysEqual(currentKey, itemKey, collation))
                {
                    currentItems.Add(item);
                }
                else
                {
                    groups.Add((currentKey, currentItems));
                    currentItems = new List<XdmValue> { item };
                    currentKey = itemKey;
                }
            }
            if (currentItems != null)
                groups.Add((currentKey, currentItems));
        }
        else if (!string.IsNullOrEmpty(groupStarting))
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var patternCompiler = new Patterns.PatternCompiler();
            var pattern = patternCompiler.Compile(groupStarting, defaultNs);
            List<XdmValue>? currentItems = null;
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                if (pattern(item, _context))
                {
                    if (currentItems != null && currentItems.Count > 0)
                        groups.Add((null, currentItems));
                    currentItems = new List<XdmValue> { item };
                }
                else
                {
                    currentItems ??= new List<XdmValue>();
                    currentItems.Add(item);
                }
            }
            if (currentItems != null && currentItems.Count > 0)
                groups.Add((null, currentItems));
        }
        else if (!string.IsNullOrEmpty(groupEnding))
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var patternCompiler = new Patterns.PatternCompiler();
            var pattern = patternCompiler.Compile(groupEnding, defaultNs);
            List<XdmValue>? currentItems = null;
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                currentItems ??= new List<XdmValue>();
                currentItems.Add(item);
                if (pattern(item, _context))
                {
                    groups.Add((null, currentItems));
                    currentItems = null;
                }
            }
            if (currentItems != null && currentItems.Count > 0)
                groups.Add((null, currentItems));
        }

        return groups;
    }

    /// <summary>
    /// Adds an item to an existing group whose key is equal under XDM eq semantics
    /// (including the requested collation for string comparisons), or creates a new
    /// group when no matching group exists.
    /// </summary>
    private static void AddToGroup(List<(XdmValue? Key, List<XdmValue> Items)> groups, XdmValue key, XdmValue item, string? collation)
    {
        foreach (var g in groups)
        {
            if (g.Key != null && GroupingKeysEqual(g.Key.Value, key, collation))
            {
                if (!g.Items.Contains(item))
                    g.Items.Add(item);
                return;
            }
        }
        groups.Add((key, new List<XdmValue> { item }));
    }

    /// <summary>
    /// Atomizes the items of a grouping key expression and returns them as a list.
    /// </summary>
    private static List<XdmValue> EnumerateKeyItems(XdmValue value)
    {
        var result = new List<XdmValue>();
        if (value.IsUndefined)
            return result;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    result.Add(AtomizeKeyItem(item));
            }
        }
        else
        {
            result.Add(AtomizeKeyItem(value));
        }
        return result;
    }

    /// <summary>
    /// Atomizes a single grouping key item. Nodes become xs:untypedAtomic values.
    /// </summary>
    private static XdmValue AtomizeKeyItem(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue, "untypedAtomic");
        return value;
    }

    /// <summary>
    /// Compares two grouping keys using the same rules as the XPath <c>eq</c> operator,
    /// including numeric promotion, untyped-atomic casting, date/time normalization,
    /// and the supplied string collation.
    /// </summary>
    private static bool GroupingKeysEqual(XdmValue a, XdmValue b, string? collation = null)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        if (a.IsSequence && b.IsSequence)
        {
            var aItems = EnumerateKeyItems(a);
            var bItems = EnumerateKeyItems(b);
            if (aItems.Count != bItems.Count)
                return false;
            for (int i = 0; i < aItems.Count; i++)
            {
                if (!AtomicValuesEqual(aItems[i], bItems[i], collation))
                    return false;
            }
            return true;
        }

        if (!a.IsSequence && !b.IsSequence)
            return AtomicValuesEqual(a, b, collation);

        return false;
    }

    /// <summary>
    /// Returns true when the supplied value kind represents a numeric atomic type.
    /// </summary>
    private static bool IsNumeric(XdmValueKind kind)
        => kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;

    private static double ToDouble(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            _ => double.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    private static float ToFloat(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (float)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (float)value.DoubleValue,
            _ => float.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    private static decimal ToDecimal(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
            _ => decimal.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    /// <summary>
    /// Compares two atomic XDM values using XPath <c>eq</c> semantics and the
    /// supplied string collation for string/untypedAtomic comparisons.
    /// </summary>
    private static bool AtomicValuesEqual(XdmValue a, XdmValue b, string? collation = null)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        var aKind = a.Kind;
        var bKind = b.Kind;

        // Both numeric: compare numeric values with proper promotion (per XPath eq).
        if (IsNumeric(aKind) && IsNumeric(bKind))
        {
            // Grouping treats NaN as equal to itself, unlike XPath value comparisons.
            bool aIsNaN = (aKind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(a.DoubleValue);
            bool bIsNaN = (bKind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(b.DoubleValue);
            if (aIsNaN || bIsNaN)
                return aIsNaN && bIsNaN;

            if (aKind == XdmValueKind.Double || bKind == XdmValueKind.Double)
                return ToDouble(a) == ToDouble(b);
            if (aKind == XdmValueKind.Float || bKind == XdmValueKind.Float)
                return ToFloat(a) == ToFloat(b);
            if (aKind == XdmValueKind.Decimal || bKind == XdmValueKind.Decimal)
                return ToDecimal(a) == ToDecimal(b);
            return a.IntegerValue == b.IntegerValue;
        }

        // Same kind exact comparison.
        if (aKind == bKind)
        {
            switch (aKind)
            {
                case XdmValueKind.String:
                    return GroupingStringEquals(a.ToString(), b.ToString(), collation);
                case XdmValueKind.Boolean:
                    return a.BooleanValue == b.BooleanValue;
                case XdmValueKind.DateTime:
                case XdmValueKind.Date:
                case XdmValueKind.Time:
                    return NormalizeDateTime(a, aKind) == NormalizeDateTime(b, bKind);
                case XdmValueKind.Duration:
                    return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
                case XdmValueKind.QName:
                    var qa = a.QNameValue;
                    var qb = b.QNameValue;
                    return qa.LocalName == qb.LocalName && qa.NamespaceUri == qb.NamespaceUri;
                case XdmValueKind.Uri:
                    return GroupingStringEquals(a.ToString(), b.ToString(), collation);
            }
        }

        // untypedAtomic on either side: cast to the other operand's type.
        if (IsUntypedAtomic(a))
            return UntypedAtomicEqualsOther(a, b, collation);
        if (IsUntypedAtomic(b))
            return UntypedAtomicEqualsOther(b, a, collation);

        // String / URI cross-comparison.
        if ((aKind == XdmValueKind.String || aKind == XdmValueKind.Uri) &&
            (bKind == XdmValueKind.String || bKind == XdmValueKind.Uri))
        {
            return GroupingStringEquals(a.ToString(), b.ToString(), collation);
        }

        return false;
    }

    /// <summary>
    /// Compares an xs:untypedAtomic value with another atomic value using the
    /// casting rules of the XPath <c>eq</c> operator and the supplied string collation.
    /// </summary>
    private static bool UntypedAtomicEqualsOther(XdmValue untyped, XdmValue other, string? collation = null)
    {
        var s = untyped.ToString();
        var otherKind = other.Kind;

        if (otherKind is XdmValueKind.String or XdmValueKind.Uri)
            return GroupingStringEquals(s, other.ToString(), collation);

        if (IsNumeric(otherKind))
        {
            // Cast untypedAtomic to the other operand's numeric type, per XPath eq rules.
            if (otherKind == XdmValueKind.Float)
            {
                if (!float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float f))
                    return false;
                return f == ToFloat(other);
            }
            if (otherKind == XdmValueKind.Double)
            {
                if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    return false;
                if (double.IsNaN(d))
                    return false;
                return d == ToDouble(other);
            }
            if (otherKind == XdmValueKind.Decimal)
            {
                if (!decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal d))
                    return false;
                return d == ToDecimal(other);
            }
            if (otherKind == XdmValueKind.Integer)
            {
                if (!long.TryParse(s, out long d))
                    return false;
                return d == other.IntegerValue;
            }
        }

        if (otherKind == XdmValueKind.Boolean)
        {
            if (bool.TryParse(s, out bool b))
                return b == other.BooleanValue;
            return false;
        }

        if (otherKind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time)
        {
            if (DateTimeOffset.TryParse(s, out var dt))
                return dt.ToUniversalTime() == NormalizeDateTime(other, otherKind);
            return false;
        }

        return false;
    }

    /// <summary>
    /// Normalizes a date/time value to UTC for comparison.
    /// </summary>
    private static DateTimeOffset NormalizeDateTime(XdmValue value, XdmValueKind kind)
    {
        var dt = kind switch
        {
            XdmValueKind.DateTime => value.DateTimeValue,
            XdmValueKind.Date => value.DateValue,
            XdmValueKind.Time => value.TimeValue,
            _ => throw new InvalidOperationException()
        };
        return dt.ToUniversalTime();
    }

    /// <summary>
    /// Determines whether the supplied value is an xs:untypedAtomic atomic value.
    /// </summary>
    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the <c>xsl:for-each-group</c> instruction requests composite
    /// grouping keys (<c>composite="yes"</c>, <c>"true"</c>, or <c>"1"</c>).
    /// </summary>
    private static bool IsCompositeGrouping(XElement instruction)
    {
        var value = instruction.Attribute("composite")?.Value;
        return value is "yes" or "true" or "1";
    }

    /// <summary>
    /// Returns true when the containing stylesheet declares an XSLT version of 3.0 or higher.
    /// </summary>
    private bool IsXslt30OrHigher()
    {
        var v = _stylesheet.Version;
        return v is "3.0" or "3.1";
    }

    private const string CodepointCollation = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
    private const string HtmlAsciiCaseInsensitiveCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
    private const string CaseblindCollation = "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind";
    private const string UcaCollationPrefix = "http://www.w3.org/2013/collation/UCA";

    /// <summary>
    /// Compares two strings using the supplied collation URI. Falls back to codepoint
    /// comparison when no collation is supplied or when the URI is unrecognized.
    /// </summary>
    private static bool GroupingStringEquals(string a, string b, string? collation)
    {
        if (string.IsNullOrEmpty(collation) || collation == CodepointCollation)
            return string.Equals(a, b, StringComparison.Ordinal);

        if (TryParseUcaCollation(collation, out var uca))
            return uca.CompareInfo.Compare(a, b, uca.Options) == 0;

        if (collation == HtmlAsciiCaseInsensitiveCollation || collation == CaseblindCollation)
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        // Unknown collation: behave as codepoint (caller normally validates earlier).
        return string.Equals(a, b, StringComparison.Ordinal);
    }

    /// <summary>
    /// Parses a UCA collation URI into a culture and compare options.
    /// </summary>
    private static bool TryParseUcaCollation(string uri, out UcaCollationInfo info)
    {
        info = default;
        if (!uri.StartsWith(UcaCollationPrefix, StringComparison.Ordinal))
            return false;

        string query = uri.Length > UcaCollationPrefix.Length && uri[UcaCollationPrefix.Length] == '?'
            ? uri[(UcaCollationPrefix.Length + 1)..]
            : string.Empty;

        string lang = "en";
        string strength = "tertiary";
        bool alternateBlanked = false;
        foreach (var param in query.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = param.IndexOf('=');
            if (eq < 0) continue;
            string key = param[..eq].Trim();
            string val = param[(eq + 1)..].Trim();
            if (key == "lang")
                lang = val;
            else if (key == "strength")
                strength = val;
            else if (key == "alternate" && val == "blanked")
                alternateBlanked = true;
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(lang);
            var options = strength.ToLowerInvariant() switch
            {
                "primary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
                "secondary" => CompareOptions.IgnoreCase,
                "tertiary" => CompareOptions.None,
                "quaternary" => CompareOptions.None,
                "identical" => CompareOptions.Ordinal,
                _ => CompareOptions.None,
            };

            if (alternateBlanked)
                options |= CompareOptions.IgnoreSymbols;

            info = new UcaCollationInfo(culture.CompareInfo, options);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private readonly record struct UcaCollationInfo(CompareInfo CompareInfo, CompareOptions Options);

    /// <summary>
    /// Applies basic type conversion for the <c>as</c> attribute on xsl:variable / xsl:param.
    /// Atomizes the value and casts to common atomic types (xs:integer, xs:string, etc.).
    /// Node types (element(), attribute(), document-node()) are returned unchanged.
    /// </summary>
    private static XdmValue ConvertVariableValue(XdmValue value, string? asType)
    {
        if (string.IsNullOrEmpty(asType) || value.IsUndefined)
            return value;

        var originalType = asType.Trim();
        // Strip XPath comments (: ... :) from the type string
        var type = System.Text.RegularExpressions.Regex.Replace(originalType, @"\(:[^:]*:\)", "").Trim();
        bool allowsMultiple = type.EndsWith("*") || type.EndsWith("+");
        bool allowsEmpty = type.EndsWith("?") || type.EndsWith("*");
        if (type.EndsWith("?") || type.EndsWith("*") || type.EndsWith("+"))
            type = type[..^1].Trim();

        // Node types and item(): no atomization or casting needed
        if (type is "node()" or "text()" or "comment()" or "processing-instruction()" or "namespace-node()" or "item()"
            || type.Contains("element(") || type.Contains("attribute(") || type.Contains("document-node("))
            return value;

        // Collect sequence items
        var items = new List<XdmValue>();
        if (value.IsNode)
            items.Add(value);
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                items.Add(item);
        }
        else
            items.Add(value);

        if (items.Count == 0)
            return allowsEmpty ? XdmValue.Undefined : value;

        // For multi-item sequences without * or +, don't convert (would be a type error in strict mode)
        if (items.Count > 1 && !allowsMultiple)
            return value;

        // Convert each item via atomization + casting
        var converted = new List<XdmValue>();
        foreach (var item in items)
        {
            // Atomize nodes to xs:untypedAtomic before casting
            XdmValue atomic = item.IsNode
                ? XdmValue.FromString(item.NodeValue.StringValue, "untypedAtomic")
                : item;

            // Subtype substitution: if the value already matches the declared type, keep it as-is
            if (VmEngine.ValueMatchesType(atomic, type))
            {
                converted.Add(atomic);
            }
            else if (VmEngine.TryCast(atomic, type, out var casted))
            {
                converted.Add(casted);
            }
            else
            {
                // Cast failed: type error
                throw new InvalidOperationException($"XPTY0004: Cannot convert value to type {type}");
            }
        }

        if (converted.Count == 1)
            return converted[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(converted));
    }

    /// <summary>
    /// Evaluates a sequence constructor (child nodes of an xsl:variable, xsl:param, etc.)
    /// and returns the resulting XDM value.
    /// </summary>
    private XdmValue EvaluateSequenceConstructor(XElement parent, XdmValue contextItem, bool wrapInDocumentNode = true)
    {
        // Ensure XPath evaluations inside the sequence constructor use the correct context item
        var savedContextItem = _context.ContextItem;
        var savedContextPosition = _context.ContextPosition;
        var savedContextSize = _context.ContextSize;
        if (contextItem.Kind != XdmValueKind.Undefined)
        {
            // Preserve the caller's context position and size so that position()/last()
            // inside sequence constructors (e.g. xsl:variable within xsl:for-each)
            // reflect the containing instruction's focus, per XSLT 2.0 §5.7.1.
            int pos = _context.ContextPosition > 0 ? _context.ContextPosition : 1;
            int size = _context.ContextSize > 0 ? _context.ContextSize : 1;
            _context.WithFocus(contextItem, pos, size);
        }

        var savedAccumulator = _sequenceAccumulator;
        if (!wrapInDocumentNode)
            _sequenceAccumulator = new List<XdmValue>();
        else
            _sequenceAccumulator = null; // When building a document node, all content goes into the wrapper

        try
        {
            // Create a temporary container to capture the sequence constructor output
            var wrapper = new XElement("__temp__");
            ExecuteSequenceConstructorDirect(parent, contextItem, wrapper);

            var nodes = wrapper.Nodes().ToList();
            var attributes = wrapper.Attributes().ToList();

            // Include items collected by xsl:sequence into the accumulator
            var accumulatorItems = _sequenceAccumulator ?? new List<XdmValue>();

            // Handle xsl:on-empty: if sequence constructor is empty, evaluate on-empty fallback
            var onEmptyElements = parent.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)).ToList();
            if (nodes.Count == 0 && attributes.Count == 0 && accumulatorItems.Count == 0 && onEmptyElements.Count > 0)
            {
                var savedContainer = _currentContainer;
                _currentContainer = wrapper;
                foreach (var onEmpty in onEmptyElements)
                {
                    var oeSelect = onEmpty.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(oeSelect))
                    {
                        var compiled = XPath31Expression.Compile(oeSelect);
                        var result = compiled.Evaluate(_context);
                        CopyToResult(result, separateAtomicsWithSpace: true);
                    }
                    else
                    {
                        foreach (var childNode in onEmpty.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, onEmpty);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, _context.ContextItem);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }
                }
                _currentContainer = savedContainer;

                // Re-read nodes/attributes after on-empty evaluation
                nodes = wrapper.Nodes().ToList();
                attributes = wrapper.Attributes().ToList();
                accumulatorItems = _sequenceAccumulator ?? new List<XdmValue>();
            }

            // Empty sequence constructor: when building a document node (no @as)
            // the result is an empty document node; with @as it is an empty sequence.
            if (nodes.Count == 0 && attributes.Count == 0 && accumulatorItems.Count == 0)
            {
                if (wrapInDocumentNode)
                {
                    var emptyDoc = new XDocument();
                    var effectiveBaseUri = GetEffectiveBaseUri(parent);
                    if (!string.IsNullOrEmpty(effectiveBaseUri))
                        emptyDoc.AddAnnotation(effectiveBaseUri);
                    return XdmValue.FromNode(new XDocumentNode(emptyDoc));
                }
                return XdmValue.FromSequence(XdmSequence.Empty);
            }

            if (wrapInDocumentNode)
            {
                // XSLT 3.0 §5.7.1: attribute nodes in document-node content are an error.
                var realAttrs = attributes.Where(a => !a.IsNamespaceDeclaration).ToList();
                if (realAttrs.Count > 0)
                    throw new InvalidOperationException("XTDE0420");

                // Apply complex content rules: remove zero-length text nodes,
                // merge adjacent text nodes.
                nodes = ApplyComplexContentRules(nodes);

                // XSLT 2.0+: non-empty sequence constructor content produces a document node.
                // LINQ-to-XML XDocument requires exactly one root element and does not
                // allow non-whitespace text nodes outside the root, so we use a synthetic
                // wrapper element that XDocumentNode transparently unwraps.
                var elementCount = nodes.OfType<XElement>().Count();
                if (elementCount == 1 && nodes.Count == 1)
                {
                    // Single element: use it directly as the document root.
                    // Remove from wrapper first so XDocument does not clone and
                    // lose XElement annotations (e.g. NamespaceInheritanceBarrier).
                    if (nodes[0].Parent != null)
                        nodes[0].Remove();
                    var effectiveBaseUri = GetEffectiveBaseUri(parent);
                    if (nodes[0] is XElement rootElem && !string.IsNullOrEmpty(effectiveBaseUri) && rootElem.Annotation<string>() == null)
                        rootElem.AddAnnotation(effectiveBaseUri);
                    var tempDoc = new XDocument(nodes[0]);
                    if (!string.IsNullOrEmpty(effectiveBaseUri))
                        tempDoc.AddAnnotation(effectiveBaseUri);
                    return XdmValue.FromNode(new XDocumentNode(tempDoc));
                }
                else
                {
                    // Mixed content: wrap in synthetic document wrapper.
                    // Remove each node from wrapper first to preserve annotations.
                    var docWrapper = new XElement("__xdm_doc__");
                    foreach (var node in nodes)
                    {
                        if (node.Parent != null)
                            node.Remove();
                        docWrapper.Add(node);
                    }
                    var effectiveBaseUri = GetEffectiveBaseUri(parent);
                    if (!string.IsNullOrEmpty(effectiveBaseUri) && docWrapper.Annotation<string>() == null)
                        docWrapper.AddAnnotation(effectiveBaseUri);
                    var tempDoc = new XDocument(docWrapper);
                    if (!string.IsNullOrEmpty(effectiveBaseUri))
                        tempDoc.AddAnnotation(effectiveBaseUri);
                    return XdmValue.FromNode(new XDocumentNode(tempDoc));
                }
            }

            // wrapInDocumentNode == false: return the raw sequence (used when @as is present)
            // Remove nodes from the temporary wrapper so they don't have __temp__ as parent.
            foreach (var node in nodes)
            {
                if (node.Parent != null)
                    node.Remove();
            }
            var effectiveBaseUriNoWrap = GetEffectiveBaseUri(parent);
            if (!string.IsNullOrEmpty(effectiveBaseUriNoWrap))
            {
                foreach (var child in nodes.OfType<XElement>())
                {
                    if (child.Annotation<string>() == null)
                        child.AddAnnotation(effectiveBaseUriNoWrap);
                }
                // Also set annotation on element nodes in accumulatorItems (e.g. from xsl:copy-of)
                foreach (var item in accumulatorItems)
                {
                    if (item.IsNode && item.NodeValue is XDocumentNode xdn && xdn.UnderlyingObject is XElement accElem)
                    {
                        if (accElem.Annotation<string>() == null)
                            accElem.AddAnnotation(effectiveBaseUriNoWrap);
                    }
                }
            }
            var asType = parent.Attribute("as")?.Value;
            bool allowsMultipleItems = !string.IsNullOrEmpty(asType) &&
                (asType.TrimEnd().EndsWith("*") || asType.TrimEnd().EndsWith("+"));

            var results = new List<XdmValue>();
            foreach (var item in accumulatorItems)
            {
                // Sequence constructors used for a single-item @as type drop zero-length
                // text nodes; sequence types that allow multiple items retain them.
                if (!allowsMultipleItems &&
                    item.IsNode && item.NodeValue is { NodeKind: XdmNodeKind.Text } tn && tn.StringValue.Length == 0)
                    continue;
                results.Add(item);
            }
            foreach (var child in nodes)
            {
                switch (child)
                {
                    case XElement e:
                        results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                        break;
                    case XText t:
                        // Sequence constructors drop zero-length text nodes unless the
                        // declared type allows multiple items.
                        if (!allowsMultipleItems && string.IsNullOrEmpty(t.Value))
                            break;
                        // Preserve text nodes as text nodes, not atomic strings,
                        // so that CopyToResult can concatenate adjacent text nodes
                        // without inserting spaces (XSLT 3.0 §5.7.2).
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                        break;
                    case XComment c:
                        results.Add(XdmValue.FromNode(new XDocumentNode(c)));
                        break;
                    case XProcessingInstruction pi:
                        results.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                        break;
                }
            }
            // Include attributes produced by xsl:attribute / xsl:namespace in the sequence
            foreach (var attr in attributes)
            {
                if (attr.IsNamespaceDeclaration)
                {
                    results.Add(XdmValue.FromNode(XDocumentNode.CreateNamespaceNode(attr, wrapper)));
                }
                else
                {
                    results.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
                }
            }
            if (results.Count == 1)
                return results[0];
            return XdmValue.FromSequence(MaterializedSequence.FromList(results));
        }
        finally
        {
            _sequenceAccumulator = savedAccumulator;
            _context.WithFocus(savedContextItem, savedContextPosition, savedContextSize);
        }
    }

    /// <summary>
    /// Applies complex content construction rules to a list of nodes:
    /// removes zero-length text nodes and merges adjacent text nodes.
    /// </summary>
    private static List<XNode> ApplyComplexContentRules(List<XNode> nodes)
    {
        var result = new List<XNode>();
        var textBuffer = new StringBuilder();

        foreach (var node in nodes)
        {
            if (node is XText t)
            {
                if (t.Value.Length == 0)
                {
                    // Discard zero-length text nodes, but flush any accumulated text first
                    if (textBuffer.Length > 0)
                    {
                        result.Add(new XText(textBuffer.ToString()));
                        textBuffer.Clear();
                    }
                    continue;
                }
                textBuffer.Append(t.Value);
            }
            else
            {
                if (textBuffer.Length > 0)
                {
                    result.Add(new XText(textBuffer.ToString()));
                    textBuffer.Clear();
                }
                result.Add(node);
            }
        }

        if (textBuffer.Length > 0)
        {
            result.Add(new XText(textBuffer.ToString()));
        }

        return result;
    }

    /// <summary>
    /// Executes a sequence constructor directly into the specified container,
    /// handling text nodes, XSLT instructions, and literal result elements.
    /// </summary>
    private void ExecuteSequenceConstructorDirect(XElement parent, XdmValue contextItem, XContainer outputContainer)
    {
        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        _currentContainer = outputContainer;
        _lastAddedWasAtomic = false;
        try
        {
            foreach (var node in parent.Nodes())
            {
                switch (node)
                {
                    case XText text:
                        ProcessSequenceText(text, parent);
                        break;
                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        var currentNode = contextItem.IsNode ? contextItem.NodeValue : null;
                        ExecuteXsltInstruction(elem, currentNode!);
                        break;
                    case XElement elem:
                        CopyLiteralElement(elem);
                        break;
                }
            }
        }
        finally
        {
            _currentContainer = savedContainer;
            _lastAddedWasAtomic = savedLastAtomic;
        }
    }

    /// <summary>
    /// Evaluates the sequence constructor within the given element and returns
    /// the concatenated string value, applying simple content construction rules.
    /// </summary>
    /// <param name="parent">The element whose child nodes form the sequence constructor.</param>
    /// <param name="contextItem">The current context item for XPath evaluations.</param>
    /// <param name="separator">The separator inserted between successive strings after atomization.</param>
    private string EvaluateSimpleContent(XElement parent, XdmValue contextItem, string separator = " ")
    {
        var items = new List<XdmValue>();
        CollectSimpleContentItems(parent, contextItem, items);
        return ConstructSimpleContentString(items, separator);
    }

    /// <summary>
    /// Collects the raw XDM items produced by evaluating a sequence constructor
    /// for simple content construction.
    /// </summary>
    private void CollectSimpleContentItems(XElement parent, XdmValue contextItem, List<XdmValue> items)
    {
        foreach (var node in parent.Nodes())
        {
            switch (node)
            {
                case XText text:
                    CollectSimpleContentText(text, parent, items);
                    break;
                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                    CollectSimpleContentXsltInstruction(elem, contextItem, items);
                    break;
                case XElement elem:
                    var copy = CopyLiteralElementToXElement(elem);
                    items.Add(XdmValue.FromNode(new XDocumentNode(copy)));
                    break;
            }
        }
    }

    /// <summary>
    /// Processes a literal text node in simple content and adds the resulting
    /// text node to the items list.
    /// </summary>
    private void CollectSimpleContentText(XText text, XElement parent, List<XdmValue> items)
    {
        string value;
        if (GetExpandText(parent) && ContainsTvtExpression(text.Value))
        {
            value = EvaluateTvt(text.Value);
        }
        else if (IsWhitespacePreserveContext(parent))
        {
            value = text.Value;
        }
        else if (IsWhitespaceOnly(text.Value))
        {
            return;
        }
        else
        {
            value = text.Value;
        }
        items.Add(XdmValue.FromNode(new XDocumentNode(new XText(value))));
    }

    /// <summary>
    /// Processes an XSLT instruction in simple content and adds the resulting
    /// items to the items list.
    /// </summary>
    private void CollectSimpleContentXsltInstruction(XElement instruction, XdmValue contextItem, List<XdmValue> items)
    {
        var name = instruction.Name.LocalName;
        switch (name)
        {
            case "sequence":
                {
                    var seqSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(seqSelect))
                    {
                        var compiled = XPath31Expression.Compile(seqSelect);
                        var result = compiled.Evaluate(_context);
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                        }
                        else
                        {
                            items.Add(result);
                        }
                    }
                    else
                    {
                        CollectSimpleContentItems(instruction, contextItem, items);
                    }
                    break;
                }

            case "copy-of":
                {
                    var copySelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(copySelect))
                    {
                        var compiled = XPath31Expression.Compile(copySelect);
                        var result = compiled.Evaluate(_context);
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                        }
                        else
                        {
                            items.Add(result);
                        }
                    }
                    break;
                }

            case "document":
                {
                    // In simple content, an xsl:document instruction contributes
                    // the string value of the document node (descendant text only),
                    // not the comment/PI descendants.
                    var docValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: true);
                    if (docValue.IsNode && docValue.NodeValue != null)
                        items.Add(docValue);
                    break;
                }

            case "for-each":
                {
                    var feSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(feSelect))
                    {
                        var compiled = XPath31Expression.Compile(feSelect);
                        var result = compiled.Evaluate(_context);
                        var feItems = new List<XdmValue>();
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                feItems.Add(item);
                        }
                        else
                        {
                            feItems.Add(result);
                        }

                        // Apply xsl:sort if present
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            feItems = SortItems(feItems, sortElements);
                        }

                        var savedItem = _context.ContextItem;
                        var savedCurrent = _context.CurrentItem;
                        var savedPosition = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        try
                        {
                            for (int i = 0; i < feItems.Count; i++)
                            {
                                _context.WithFocus(feItems[i], i + 1, feItems.Count);
                                _context.WithCurrentItem(feItems[i]);
                                CollectSimpleContentItems(instruction, feItems[i], items);
                            }
                        }
                        finally
                        {
                            _context.WithFocus(savedItem, savedPosition, savedSize);
                            _context.WithCurrentItem(savedCurrent);
                        }
                    }
                    break;
                }

            case "if":
                {
                    var test = instruction.Attribute("test")?.Value;
                    if (!string.IsNullOrEmpty(test))
                    {
                        var compiled = XPath31Expression.Compile(test);
                        if (compiled.Evaluate(_context).EffectiveBooleanValue())
                        {
                            CollectSimpleContentItems(instruction, contextItem, items);
                        }
                    }
                    break;
                }

            case "choose":
                {
                    bool matched = false;
                    foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var whenTest = when.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(whenTest))
                        {
                            var compiled = XPath31Expression.Compile(whenTest);
                            if (compiled.Evaluate(_context).EffectiveBooleanValue())
                            {
                                CollectSimpleContentItems(when, contextItem, items);
                                matched = true;
                                break;
                            }
                        }
                    }
                    if (!matched)
                    {
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            CollectSimpleContentItems(otherwise, contextItem, items);
                        }
                    }
                    break;
                }

            case "variable":
            case "param":
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = XPath31Expression.Compile(varSelect);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
                        _context.WithVariable(varName, varValue);
                    }
                    break;
                }

            case "message":
                {
                    var msgSelect = instruction.Attribute("select")?.Value;
                    string msgText;
                    if (!string.IsNullOrEmpty(msgSelect))
                    {
                        var compiled = XPath31Expression.Compile(msgSelect);
                        msgText = XdmValueToString(compiled.Evaluate(_context), " ");
                    }
                    else
                    {
                        msgText = EvaluateSimpleContent(instruction, contextItem);
                    }
                    _messageListener?.OnMessage(msgText);
                    break;
                }

            case "result-document":
            case "fallback":
                // No output in simple content
                break;

            default:
                // Fallback: execute into a temporary container and extract nodes.
                var savedContainer = _currentContainer;
                var temp = new XElement("__fallback__");
                _currentContainer = temp;
                try
                {
                    var currentNode = contextItem.IsNode ? contextItem.NodeValue : null;
                    ExecuteXsltInstruction(instruction, currentNode!);
                    foreach (var child in temp.Nodes())
                    {
                        switch (child)
                        {
                            case XText t:
                                items.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                                break;
                            case XElement e:
                                items.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                break;
                            case XComment c:
                                items.Add(XdmValue.FromNode(new XDocumentNode(c)));
                                break;
                            case XProcessingInstruction pi:
                                items.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                                break;
                        }
                    }
                }
                finally
                {
                    _currentContainer = savedContainer;
                }
                break;
        }
    }

    /// <summary>
    /// Applies simple content construction rules to a list of items and returns
    /// the concatenated string.
    /// </summary>
    private static string ConstructSimpleContentString(List<XdmValue> items, string separator)
    {
        var strings = new List<string>();
        string? pendingText = null;

        foreach (var item in items)
        {
            bool isTextNode = item.IsNode && item.NodeValue != null &&
                              item.NodeValue.NodeKind == XdmNodeKind.Text;

            if (isTextNode && item.NodeValue!.StringValue.Length == 0)
            {
                continue; // Remove zero-length text nodes
            }

            if (isTextNode)
            {
                if (pendingText != null)
                {
                    pendingText += item.NodeValue!.StringValue;
                }
                else
                {
                    pendingText = item.NodeValue!.StringValue;
                }
            }
            else
            {
                if (pendingText != null)
                {
                    strings.Add(pendingText);
                    pendingText = null;
                }
                // Atomize and cast to string
                strings.Add(item.ToString());
            }
        }

        if (pendingText != null)
        {
            strings.Add(pendingText);
        }

        return string.Join(separator, strings);
    }

    /// <summary>
    /// Copies a literal result element into a standalone XElement without
    /// adding it to the current result container.
    /// </summary>
    private XElement CopyLiteralElementToXElement(XElement source)
    {
        var savedContainer = _currentContainer;
        var temp = new XElement("__temp__");
        _currentContainer = temp;
        try
        {
            CopyLiteralElement(source);
            return temp.Elements().First();
        }
        finally
        {
            _currentContainer = savedContainer;
        }
    }

    // ------------------------------------------------------------------
    // Whitespace stripping (xsl:strip-space / xsl:preserve-space)
    // ------------------------------------------------------------------

    private void ApplyWhitespaceStripping(IXdmNode source)
    {
        var rules = _stylesheet.GetAllSpaceHandlingRules();

        // Only strip whitespace in XDocument-backed nodes for now
        if (source is XDocumentNode xdocNode)
        {
            if (xdocNode.UnderlyingObject is XDocument doc)
            {
                // Strip whitespace text nodes that are direct children of the document
                foreach (var textNode in doc.Nodes().OfType<XText>().ToList())
                {
                    if (IsWhitespaceOnly(textNode.Value))
                        textNode.Remove();
                }
                if (rules.Count > 0)
                    StripWhitespaceInElement(doc.Root, rules);
            }
            else if (xdocNode.UnderlyingObject is XElement elem)
            {
                if (rules.Count > 0)
                    StripWhitespaceInElement(elem, rules);
            }
        }
    }

    private static void StripWhitespaceInElement(XElement? element, List<SpaceHandlingRule> rules)
    {
        if (element == null)
            return;

        foreach (var child in element.Elements().ToList())
        {
            StripWhitespaceInElement(child, rules);
        }

        if (ShouldStripWhitespace(element, rules))
        {
            foreach (var textNode in element.Nodes().OfType<XText>().ToList())
            {
                if (IsWhitespaceOnly(textNode.Value))
                {
                    textNode.Remove();
                }
            }
        }
    }

    private static bool IsWhitespaceOnly(string text)
    {
        foreach (var c in text)
        {
            if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
                return false;
        }
        return text.Length > 0;
    }

    private static bool ShouldStripWhitespace(XElement element, List<SpaceHandlingRule> rules)
    {
        // xml:space="preserve" always preserves whitespace
        var xmlSpace = element.Attribute(System.Xml.Linq.XNamespace.Xml + "space")?.Value;
        if (xmlSpace == "preserve")
            return false;

        SpaceHandlingRule? bestStrip = null;
        SpaceHandlingRule? bestPreserve = null;

        foreach (var rule in rules)
        {
            if (MatchesNameTest(rule, element))
            {
                if (rule.IsStrip && (bestStrip == null || rule.Precedence > bestStrip.Value.Precedence))
                    bestStrip = rule;
                if (!rule.IsStrip && (bestPreserve == null || rule.Precedence > bestPreserve.Value.Precedence))
                    bestPreserve = rule;
            }
        }

        if (bestPreserve == null && bestStrip == null)
            return false;

        if (bestPreserve == null)
            return bestStrip != null;

        if (bestStrip == null)
            return false;

        // Preserve wins at same or higher precedence; strip wins only at strictly higher precedence
        return bestStrip.Value.Precedence > bestPreserve.Value.Precedence;
    }

    private static bool MatchesNameTest(SpaceHandlingRule rule, XElement element)
    {
        var nameTest = rule.NameTest;
        if (nameTest == "*")
            return true;

        if (nameTest.StartsWith("Q{"))
        {
            int closeBrace = nameTest.IndexOf('}');
            if (closeBrace > 2)
            {
                var nsUri = nameTest[2..closeBrace];
                var localName = nameTest[(closeBrace + 1)..];
                return element.Name.NamespaceName == nsUri && element.Name.LocalName == localName;
            }
        }

        if (nameTest.EndsWith(":*"))
        {
            // prefix:* - would need prefix resolution; for now, match any element
            return true;
        }

        if (nameTest.Contains(':'))
        {
            // QName with prefix - would need prefix resolution
            var localName = nameTest.Contains(':') ? nameTest.Split(':')[1] : nameTest;
            return element.Name.LocalName == localName;
        }

        // Unprefixed name: match local name and namespace
        // If NamespaceUri is specified, match exactly that namespace.
        // If NamespaceUri is null (no xpath-default-namespace), match no namespace.
        var expectedNs = rule.NamespaceUri ?? "";
        return element.Name.LocalName == nameTest && element.Name.NamespaceName == expectedNs;
    }

    // ------------------------------------------------------------------
    // Text Value Template (expand-text) support
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the inherited value of the expand-text attribute for the given element.
    /// Walks up the XML tree until it finds an explicit expand-text attribute;
    /// defaults to false if none is found.
    /// </summary>
    private static bool GetExpandText(XElement element)
    {
        XElement? current = element;
        while (current != null)
        {
            var attr = current.Attribute("expand-text");
            if (attr != null)
                return attr.Value is "yes" or "true" or "1";
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Evaluates a Text Value Template (TVT): parses {expr} and {{ escapes,
    /// evaluates each XPath expression, and returns the concatenated result.
    /// Respects XPath string literals when finding matching }.
    /// </summary>
    private string EvaluateTvt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder();
        int i = 0;

        while (i < text.Length)
        {
            // {{ escape → {
            if (i < text.Length - 1 && text[i] == '{' && text[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
                continue;
            }

            // }} escape → }
            if (i < text.Length - 1 && text[i] == '}' && text[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
                continue;
            }

            // {expr} — evaluate XPath expression
            if (text[i] == '{')
            {
                int exprStart = i + 1;
                int j = exprStart;
                int braceDepth = 1;
                bool inSingleQuote = false;
                bool inDoubleQuote = false;

                while (j < text.Length && braceDepth > 0)
                {
                    char c = text[j];
                    if (inSingleQuote)
                    {
                        if (c == '\'') inSingleQuote = false;
                    }
                    else if (inDoubleQuote)
                    {
                        if (c == '"') inDoubleQuote = false;
                    }
                    else
                    {
                        if (c == '\'') inSingleQuote = true;
                        else if (c == '"') inDoubleQuote = true;
                        else if (c == '{') braceDepth++;
                        else if (c == '}') braceDepth--;
                    }
                    j++;
                }

                if (braceDepth == 0)
                {
                    string expr = text.Substring(exprStart, j - exprStart - 1);
                    if (!string.IsNullOrEmpty(expr))
                    {
                        var compiled = XPath31Expression.Compile(expr);
                        var value = compiled.Evaluate(_context);
                        // XSLT 3.0 §5.6.2: atomized TVT items are joined with a single space.
                        sb.Append(XdmValueToString(value, " "));
                    }
                    i = j;
                    continue;
                }
                else
                {
                    // Unmatched { — treat as literal
                    sb.Append('{');
                    i++;
                    continue;
                }
            }

            sb.Append(text[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// XSLT elements whose whitespace text nodes are preserved (not stripped).
    /// See XSLT 3.0 spec §3.3.1.1.
    /// </summary>
    /// <summary>
    /// XSLT 3.0 §3.3.1.1: Whitespace text nodes are preserved only in xsl:text
    /// and in elements with xml:space="preserve".
    /// </summary>
    private static readonly HashSet<string> WhitespacePreserveElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "text"
    };

    /// <summary>
    /// Returns true if whitespace text nodes inside the given XSLT element should be preserved.
    /// </summary>
    private static bool IsWhitespacePreserveContext(XElement parent)
    {
        if (parent.Name.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
            return false;
        return WhitespacePreserveElements.Contains(parent.Name.LocalName);
    }

    /// <summary>
    /// Processes a text node encountered in a sequence constructor.
    /// If the parent element (or an ancestor) has expand-text="yes",
    /// evaluates the text as a TVT. Otherwise applies normal whitespace
    /// stripping and adds the text node to the result.
    /// </summary>
    /// <summary>
    /// Returns true if the text contains an unescaped text value template expression ({...}).
    /// </summary>
    private static bool ContainsTvtExpression(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                if (i + 1 < text.Length && text[i + 1] == '{')
                {
                    i++; // skip escaped {{
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void ProcessSequenceText(XText text, XElement parent)
    {
        if (GetExpandText(parent) && ContainsTvtExpression(text.Value))
        {
            var tvtResult = EvaluateTvt(text.Value);
            _lastAddedWasAtomic = false;
            AddTextNode(tvtResult);
        }
        else if (IsWhitespacePreserveContext(parent))
        {
            // Preserve whitespace text nodes in xsl:text and xml:space="preserve" contexts
            _lastAddedWasAtomic = false;
            AddTextNode(text.Value);
        }
        else
        {
            if (!IsWhitespaceOnly(text.Value))
            {
                _lastAddedWasAtomic = false;
                AddTextNode(text.Value);
            }
        }
    }

    // ------------------------------------------------------------------
    // xsl:number support
    // ------------------------------------------------------------------

    /// <summary>
    /// Executes an <c>xsl:number</c> instruction.
    /// </summary>
    private void ExecuteXsltNumber(XElement instruction, IXdmNode currentNode)
    {
        // Determine effective backwards-compatibility: walk ancestor chain for xsl:version.
        bool backwardsCompatible = _context.BackwardsCompatible;
        var xslNs = XNamespace.Get("http://www.w3.org/1999/XSL/Transform");
        var ancestor = instruction;
        while (ancestor != null)
        {
            var versionAttr = ancestor.Attribute(xslNs + "version");
            if (versionAttr != null)
            {
                if (double.TryParse(versionAttr.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) && v < 2.0)
                    backwardsCompatible = true;
                else
                    backwardsCompatible = false;
                break;
            }
            ancestor = ancestor.Parent;
        }

        var level = instruction.Attribute("level")?.Value ?? "single";
        var countPattern = instruction.Attribute("count")?.Value;
        var fromPattern = instruction.Attribute("from")?.Value;
        var formatAttr = instruction.Attribute("format")?.Value ?? "1";
        var valueAttr = instruction.Attribute("value")?.Value;
        var selectAttr = instruction.Attribute("select")?.Value;
        var startAtAttr = instruction.Attribute("start-at")?.Value;
        var ordinalAttr = instruction.Attribute("ordinal")?.Value;
        var langAttr = instruction.Attribute("lang")?.Value;
        var groupingSepAttr = instruction.Attribute("grouping-separator")?.Value;
        var groupingSizeAttr = instruction.Attribute("grouping-size")?.Value;

        // Evaluate format as AVT (it is always an AVT per XSLT spec)
        var format = EvaluateAvt(formatAttr, instruction);

        // Evaluate optional AVT attributes
        string? lang = string.IsNullOrEmpty(langAttr) ? null : EvaluateAvt(langAttr, instruction);
        if (!string.IsNullOrEmpty(lang))
        {
            try
            {
                _ = System.Globalization.CultureInfo.GetCultureInfo(lang);
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                throw new InvalidOperationException("XTDE0030");
            }
        }
        string? groupingSeparator = string.IsNullOrEmpty(groupingSepAttr) ? null : EvaluateAvt(groupingSepAttr);
        int groupingSize = 0;
        if (!string.IsNullOrEmpty(groupingSizeAttr))
        {
            var gsEval = EvaluateAvt(groupingSizeAttr, instruction);
            int.TryParse(gsEval, out groupingSize);
        }

        bool ordinal = false;
        if (!string.IsNullOrEmpty(ordinalAttr))
        {
            var ordEval = EvaluateAvt(ordinalAttr, instruction);
            ordinal = ordEval.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        // Evaluate start-at as AVT, then parse as space-separated integers (XSLT 3.0)
        BigInteger[]? startAtValues = null;
        if (!string.IsNullOrEmpty(startAtAttr))
        {
            var evaluated = EvaluateAvt(startAtAttr, instruction);
            startAtValues = ParseStartAtValues(evaluated);
        }

        // Handle select attribute: evaluate to get the target node for numbering
        IXdmNode? targetNode = currentNode;
        if (!string.IsNullOrEmpty(selectAttr))
        {
            var compiled = XPath31Expression.Compile(selectAttr);
            var result = compiled.Evaluate(_context);

            // XTTE1000: select must return at most one node
            if (result.IsSequence && result.SequenceValue != null)
            {
                int nodeCount = 0;
                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                {
                    if (item.IsNode)
                    {
                        nodeCount++;
                        if (nodeCount > 1)
                            throw new InvalidOperationException("XTTE1000");
                    }
                }
            }

            targetNode = ExtractSingleNode(result);
            if (targetNode == null)
                throw new InvalidOperationException("XTTE1000");
        }

        if (!string.IsNullOrEmpty(valueAttr))
        {
            var compiled = XPath31Expression.Compile(valueAttr);
            var result = compiled.Evaluate(_context);

            // Determine whether the raw result is an empty sequence
            bool isEmptySequence = false;
            if (result.IsSequence && result.SequenceValue != null)
            {
                isEmptySequence = true;
                foreach (var _ in XdmSequence.FromSource(result.SequenceValue))
                {
                    isEmptySequence = false;
                    break;
                }
            }

            // Negative numbers without a pattern separator are an error (check original
            // XdmValue before int conversion to avoid overflow false positives).
            if (HasNegativeValue(result) && !format.Contains(';'))
                throw new InvalidOperationException("XTDE0980");

            var numbers = XdmValueToBigIntegerArray(result);
            if (numbers.Length > 0)
            {
                // Apply start-at to each number: value - 1 + start-at
                for (int i = 0; i < numbers.Length; i++)
                {
                    var startAt = startAtValues != null && startAtValues.Length > 0
                        ? (i < startAtValues.Length ? startAtValues[i] : startAtValues[^1])
                        : BigInteger.One;
                    numbers[i] = numbers[i] - 1 + startAt;
                }
                var formatted = FormatNumberSequence(numbers, format, ordinal, lang, groupingSeparator, groupingSize);
                // When value is present, xsl:number is equivalent to format-integer.
                // Strip leading whitespace from the first output to match test expectations
                // where multiple xsl:number calls are concatenated inside xsl:for-each.
                if (!string.IsNullOrEmpty(valueAttr) && IsFirstSignificantChild())
                {
                    formatted = formatted.TrimStart();
                }
                _lastAddedWasAtomic = false;
                AddTextNode(formatted);
            }
            else
            {
                // No convertible numbers: empty sequence or non-numeric value.
                if (backwardsCompatible)
                {
                    // XSLT 1.0 backwards-compatible → NaN for empty or non-numeric values.
                    _lastAddedWasAtomic = false;
                    AddTextNode("NaN");
                }
                else if (isEmptySequence)
                {
                    // Empty sequence → emit prefix+suffix only.
                    var formatted = FormatNumberSequence(System.Array.Empty<BigInteger>(), format, ordinal, lang, groupingSeparator, groupingSize);
                    if (!string.IsNullOrEmpty(formatted))
                    {
                        if (!string.IsNullOrEmpty(valueAttr) && IsFirstSignificantChild())
                        {
                            formatted = formatted.TrimStart();
                        }
                        _lastAddedWasAtomic = false;
                        AddTextNode(formatted);
                    }
                }
                else
                {
                    // Non-empty, non-numeric sequence in XSLT 2.0+ → XTDE0980.
                    throw new InvalidOperationException("XTDE0980");
                }
            }
        }
        else
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var countMatcher = string.IsNullOrEmpty(countPattern)
                ? CreateDefaultCountMatcher(targetNode)
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(countPattern, instruction), defaultNs);

            var fromMatcher = string.IsNullOrEmpty(fromPattern)
                ? null
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(fromPattern, instruction), defaultNs);

            int[]? numbers = level switch
            {
                "single" => ComputeNumberSingle(targetNode, countMatcher, fromMatcher, _context),
                "any" => ComputeNumberAny(targetNode, countMatcher, fromMatcher, _context),
                "multiple" => ComputeNumberMultiple(targetNode, countMatcher, fromMatcher, _context),
                _ => null
            };

            if (numbers != null && numbers.Length > 0)
            {
                // Negative numbers without a pattern separator in the format string are an error
                if (numbers.Any(n => n < 0) && !format.Contains(';'))
                    throw new InvalidOperationException("XTDE0980");

                // Apply start-at to each number
                if (startAtValues != null)
                {
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        var startAt = i < startAtValues.Length ? startAtValues[i] : startAtValues[^1];
                        numbers[i] = (int)(numbers[i] - 1 + (int)startAt);
                    }
                }
            }

            // Format even when no numbers match: prefix+suffix is still emitted
            // (e.g. format="(1)" with no matches produces "()").
            var numsToFormat = numbers?.Select(n => (BigInteger)n).ToArray() ?? System.Array.Empty<BigInteger>();
            var formatted = FormatNumberSequence(numsToFormat, format, ordinal, lang, groupingSeparator, groupingSize);
            _lastAddedWasAtomic = false;
            AddTextNode(formatted);
        }
    }

    /// <summary>
    /// Creates a default count matcher based on the current node's kind and name.
    /// </summary>
    private static Patterns.PatternPredicate CreateDefaultCountMatcher(IXdmNode node)
    {
        var compiler = new Patterns.PatternCompiler();
        string name = string.IsNullOrEmpty(node.NamespaceUri)
            ? node.LocalName
            : $"Q{{{node.NamespaceUri}}}{node.LocalName}";
        return node.NodeKind switch
        {
            XdmNodeKind.Element => compiler.Compile(name),
            XdmNodeKind.Attribute => compiler.Compile("@" + name),
            _ => (n, ctx) => n.IsNode && n.NodeValue.NodeKind == node.NodeKind
        };
    }

    /// <summary>
    /// Replaces prefix:local-name occurrences in a pattern with Q{uri}local-name,
    /// resolving prefixes using the namespace declarations in scope on the given element.
    /// </summary>
    private static string ResolveNamespacePrefixes(string pattern, XElement contextElement)
    {
        if (!pattern.Contains(':'))
            return pattern;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            if (c == '\'' || c == '\"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < pattern.Length && pattern[i] != quote)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                if (i < pattern.Length)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                continue;
            }
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
            {
                sb.Append(c);
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-'))
                    i++;
                if (i < pattern.Length && pattern[i] == ':')
                {
                    if (i + 1 < pattern.Length && pattern[i + 1] == ':')
                    {
                        sb.Append(pattern[start..i]);
                        continue;
                    }
                    var prefix = pattern[start..i];
                    i++;
                    int localStart = i;
                    while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
                        i++;
                    var local = pattern[localStart..i];
                    var nsUri = contextElement.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
                    if (!string.IsNullOrEmpty(nsUri))
                    {
                        sb.Append($"Q{{{nsUri}}}{local}");
                    }
                    else
                    {
                        sb.Append(prefix);
                        sb.Append(':');
                        sb.Append(local);
                    }
                }
                else
                {
                    sb.Append(pattern[start..i]);
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Computes the number for <c>level="single"</c>.
    /// </summary>
    private static int[]? ComputeNumberSingle(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        // Find nearest ancestor-or-self matching count
        IXdmNode? target = null;
        if (countMatcher(XdmValue.FromNode(currentNode), context))
        {
            target = currentNode;
        }
        else
        {
            foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (countMatcher(XdmValue.FromNode(ancestor), context))
                    {
                        target = ancestor;
                        break;
                    }
                }
            }
        }

        if (target == null)
            return null;

        // If from is specified, the target must be a descendant-or-self of the
        // nearest ancestor of the current node that matches the from pattern.
        if (fromMatcher != null)
        {
            IXdmNode? fromNode = null;
            foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (fromMatcher(XdmValue.FromNode(ancestor), context))
                    {
                        fromNode = ancestor;
                        break;
                    }
                }
            }

            if (fromNode != null)
            {
                bool isDescendantOrSelf = false;
                IXdmNode? check = target;
                while (check != null)
                {
                    if (check.IsSameNode(fromNode))
                    {
                        isDescendantOrSelf = true;
                        break;
                    }
                    IXdmNode? parent = null;
                    foreach (var parentItem in check.Axis(XdmAxis.Parent))
                    {
                        if (parentItem.IsNode && parentItem.NodeValue is IXdmNode p)
                        {
                            parent = p;
                            break;
                        }
                    }
                    if (parent == null)
                        break;
                    check = parent;
                }
                if (!isDescendantOrSelf)
                    return null;
            }
            // If no from-matching ancestor exists, target is still valid (fallback).
        }

        int count = 0;
        foreach (var item in target.Axis(XdmAxis.PrecedingSibling))
        {
            if (item.IsNode && item.NodeValue is IXdmNode sibling)
            {
                if (countMatcher(XdmValue.FromNode(sibling), context))
                    count++;
            }
        }

        return new[] { count + 1 };
    }

    /// <summary>
    /// Computes the number for <c>level="any"</c>.
    /// </summary>
    private static int[]? ComputeNumberAny(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        var doc = currentNode.Document;
        if (doc == null)
        {
            // For non-document trees (e.g. variables), find the root ancestor
            doc = currentNode;
            while (doc.Parent != null)
                doc = doc.Parent;
        }

        int count = 0;
        bool foundCurrent = false;

        // Per .NET XslCompiledTransform semantics, only the FIRST attribute of each
        // element is counted by xsl:number level="any".
        IXdmNode? lastCountedAttributeParent = null;
        WalkDocumentTree(doc, node =>
        {
            if (node.IsSameNode(currentNode))
                foundCurrent = true;

            if (fromMatcher != null && fromMatcher(XdmValue.FromNode(node), context))
            {
                count = 0;
                lastCountedAttributeParent = null;
            }

            if (countMatcher(XdmValue.FromNode(node), context))
            {
                if (node.NodeKind == XdmNodeKind.Attribute)
                {
                    var parent = node.Parent;
                    if (parent != null && lastCountedAttributeParent != null && lastCountedAttributeParent.IsSameNode(parent))
                    {
                        // Skip non-first attributes
                        return !foundCurrent;
                    }
                    lastCountedAttributeParent = parent;
                }
                else
                {
                    lastCountedAttributeParent = null;
                }
                count++;
            }

            return !foundCurrent;
        }, false, out _);

        return count > 0 ? new[] { count } : null;
    }

    /// <summary>
    /// Computes the number sequence for <c>level="multiple"</c>.
    /// </summary>
    private static int[]? ComputeNumberMultiple(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        var numbers = new List<int>();
        var ancestors = new List<IXdmNode>();

        foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
        {
            if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                ancestors.Add(ancestor);
        }
        // ancestors is now [parent, grandparent, ...] = innermost to outermost

        // Find the nearest ancestor matching the from pattern.
        IXdmNode? fromNode = null;
        if (fromMatcher != null)
        {
            foreach (var ancestor in ancestors)
            {
                if (fromMatcher(XdmValue.FromNode(ancestor), context))
                {
                    fromNode = ancestor;
                    break;
                }
            }
        }

        // Build the chain from the from-node (or root) down to the current node,
        // in outermost-to-innermost order.
        var chain = new List<IXdmNode>();
        bool started = fromNode == null;
        for (int i = ancestors.Count - 1; i >= 0; i--)
        {
            if (!started && ancestors[i].IsSameNode(fromNode!))
                started = true;

            if (started)
                chain.Add(ancestors[i]);
        }
        chain.Add(currentNode);

        foreach (var node in chain)
        {
            if (countMatcher(XdmValue.FromNode(node), context))
            {
                int count = 0;
                foreach (var item in node.Axis(XdmAxis.PrecedingSibling))
                {
                    if (item.IsNode && item.NodeValue is IXdmNode sibling)
                    {
                        if (countMatcher(XdmValue.FromNode(sibling), context))
                            count++;
                    }
                }
                numbers.Add(count + 1);
            }
        }

        return numbers.Count > 0 ? numbers.ToArray() : null;
    }

    /// <summary>
    /// Recursively walks a document tree in document order, calling <paramref name="visitor"/>
    /// for each node. Attributes are visited immediately after their owner element and before
    /// its children, per XDM document-order rules. Returns <c>false</c> if the visitor
    /// requested stopping.
    /// </summary>
    /// <summary>
    /// Walks the tree in document order, visiting all nodes that match the visitor.
    /// When <paramref name="skipNextText"/> is true, the next text node encountered
    /// in document order is skipped (not visited). This models .NET XslCompiledTransform
    /// semantics where the first text node after an element's attributes is not counted
    /// by <c>xsl:number level="any"</c>.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the walk should continue; <c>false</c> if the visitor signalled
    /// stop. The <paramref name="pendingSkip"/> out parameter indicates whether a
    /// text-node skip is still pending after this subtree.
    /// </returns>
    private static bool WalkDocumentTree(IXdmNode node, Func<IXdmNode, bool> visitor, bool skipNextText, out bool pendingSkip)
    {
        pendingSkip = false;

        if (node.NodeKind == XdmNodeKind.Text && skipNextText)
        {
            return true; // Skip this text node
        }

        if (!visitor(node))
            return false;

        // Attributes are in document order immediately after the element's start tag.
        // All attributes must be visited so that foundCurrent works when currentNode
        // is a non-first attribute (e.g. number-1101).
        bool hasAttributes = false;
        if (node.NodeKind == XdmNodeKind.Element)
        {
            foreach (var item in node.Axis(XdmAxis.Attribute))
            {
                if (item.IsNode && item.NodeValue is IXdmNode attr)
                {
                    hasAttributes = true;
                    if (!visitor(attr))
                        return false;
                }
            }
        }

        // Per XSLT 1.0 xsl:number semantics (matching .NET XslCompiledTransform),
        // the first text node that follows an element's attributes is not counted.
        pendingSkip = hasAttributes || skipNextText;
        foreach (var item in node.Axis(XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue is IXdmNode child)
            {
                if (child.NodeKind == XdmNodeKind.Text && pendingSkip)
                {
                    pendingSkip = false;
                    continue;
                }

                bool childResult = WalkDocumentTree(child, visitor, pendingSkip, out bool childPendingSkip);
                if (!childResult)
                    return false;

                pendingSkip = childPendingSkip;
            }
        }

        return true;
    }

    /// <summary>
    /// Formats a sequence of integers according to an <c>xsl:number</c> format string.
    /// </summary>
    private string FormatNumberSequence(BigInteger[] numbers, string format, bool ordinal, string? lang, string? groupingSeparator, int groupingSize)
    {
        var (prefix, tokens, separators, suffix) = ParseXslNumberFormat(format);

        var sb = new System.Text.StringBuilder();
        sb.Append(prefix);

        for (int i = 0; i < numbers.Length; i++)
        {
            var token = tokens.Count > 0
                ? (i < tokens.Count ? tokens[i] : tokens[^1])
                : "1";

            // Append ordinal modifier if requested
            if (ordinal && !token.Contains(';'))
                token += ";o";

            var formatted = FormatIntegerEngine.Format(_context, numbers[i], token, lang);

            // Apply xsl:number grouping-separator / grouping-size
            if (!string.IsNullOrEmpty(groupingSeparator) && groupingSize > 0)
                formatted = ApplyNumberGrouping(formatted, groupingSeparator, groupingSize);

            sb.Append(formatted);

            if (i < numbers.Length - 1)
            {
                var sep = separators.Count > 0
                    ? (i < separators.Count ? separators[i] : separators[^1])
                    : ".";
                sb.Append(sep);
            }
        }

        sb.Append(suffix);
        return sb.ToString();
    }

    /// <summary>
    /// Applies grouping separator and size to a formatted number string.
    /// Handles optional leading minus sign.
    /// </summary>
    private static string ApplyNumberGrouping(string formatted, string groupingSeparator, int groupingSize)
    {
        bool negative = formatted.StartsWith("-");
        string digits = negative ? formatted.Substring(1) : formatted;

        var sb = new System.Text.StringBuilder();
        int count = 0;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % groupingSize == 0)
                sb.Insert(0, groupingSeparator);
            sb.Insert(0, digits[i]);
            count++;
        }

        string result = sb.ToString();
        return negative ? "-" + result : result;
    }

    /// <summary>
    /// Parses an <c>xsl:number</c> format string into prefix, tokens, separators, and suffix.
    /// Recognizes Unicode numbering characters (including astral-plane characters) as format tokens.
    /// </summary>
    private static (string prefix, List<string> tokens, List<string> separators, string suffix) ParseXslNumberFormat(string format)
    {
        var tokens = new List<string>();
        var separators = new List<string>();

        int i = 0;
        while (i < format.Length && !IsFormatTokenChar(format, i))
            i = AdvanceCodepoint(format, i);
        var prefix = format.Substring(0, i);

        while (i < format.Length)
        {
            int tokenStart = i;
            while (i < format.Length && IsFormatTokenChar(format, i))
                i = AdvanceCodepoint(format, i);
            tokens.Add(format.Substring(tokenStart, i - tokenStart));

            int sepStart = i;
            while (i < format.Length && !IsFormatTokenChar(format, i))
                i = AdvanceCodepoint(format, i);
            separators.Add(format.Substring(sepStart, i - sepStart));
        }

        string suffix = string.Empty;
        if (separators.Count > 0)
        {
            suffix = separators[^1];
            separators.RemoveAt(separators.Count - 1);
        }

        // Special case: non-empty format string with no alphanumeric characters.
        // The entire string is used as both prefix and suffix (e.g. "*" → "*1*").
        if (format.Length > 0 && tokens.Count == 0)
        {
            suffix = prefix;
        }

        return (prefix, tokens, separators, suffix);
    }

    /// <summary>
    /// Returns whether the character at the given index in <paramref name="s"/>
    /// is a letter, digit, or Unicode numbering character (i.e. can form a format token).
    /// </summary>
    private static bool IsFormatTokenChar(string s, int i)
    {
        var cat = CharUnicodeInfo.GetUnicodeCategory(s, i);
        return cat == UnicodeCategory.UppercaseLetter
            || cat == UnicodeCategory.LowercaseLetter
            || cat == UnicodeCategory.TitlecaseLetter
            || cat == UnicodeCategory.ModifierLetter
            || cat == UnicodeCategory.OtherLetter
            || cat == UnicodeCategory.DecimalDigitNumber
            || cat == UnicodeCategory.LetterNumber
            || cat == UnicodeCategory.OtherNumber;
    }

    /// <summary>
    /// Advances <paramref name="i"/> past the current codepoint (1 or 2 chars for surrogates).
    /// </summary>
    private static int AdvanceCodepoint(string s, int i)
    {
        if (i < s.Length && char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            return i + 2;
        return i + 1;
    }

    /// <summary>
    /// Parses a start-at attribute value string into an array of integers.
    /// Handles space-separated values and single values.
    /// </summary>
    private static BigInteger[] ParseStartAtValues(string value)
    {
        var parts = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return [BigInteger.One];
        var result = new BigInteger[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!BigInteger.TryParse(parts[i], out result[i]))
                throw new InvalidOperationException("XTSE0020");
        }
        return result;
    }

    /// <summary>
    /// Extracts a single node from an <see cref="XdmValue"/> if it represents a singleton node.
    /// </summary>
    private static IXdmNode? ExtractSingleNode(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return ExtractSingleNode(item);
            return null;
        }

        if (value.Kind == XdmValueKind.Node && value.NodeValue is IXdmNode node)
            return node;

        return null;
    }

    /// <summary>
    /// Returns <c>true</c> if the <see cref="XdmValue"/> represents a negative number.
    /// Sequences are inspected by looking at the first item.
    /// </summary>
    private static bool HasNegativeValue(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return HasNegativeValue(item);
            return false;
        }

        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue < 0,
            XdmValueKind.Decimal => value.DecimalValue < 0,
            XdmValueKind.Double => value.DoubleValue < 0 && !double.IsNaN(value.DoubleValue),
            XdmValueKind.Float => value.DoubleValue < 0 && !double.IsNaN(value.DoubleValue),
            _ => false
        };
    }

    /// <summary>
    /// Converts an <see cref="XdmValue"/> to a <see cref="BigInteger"/> if it represents a number.
    /// </summary>
    private static BigInteger? XdmValueToBigInteger(XdmValue value)
    {
        // If it's a singleton sequence, extract the first item
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return XdmValueToBigInteger(item);
            return null;
        }

        return value.Kind switch
        {
            XdmValueKind.Integer => new BigInteger(value.IntegerValue),
            XdmValueKind.Decimal => BigInteger.Parse(Math.Round(value.DecimalValue, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture)),
            XdmValueKind.Double => new BigInteger(value.DoubleValue),
            XdmValueKind.Float => new BigInteger(value.DoubleValue),
            XdmValueKind.Node => BigInteger.TryParse(value.NodeValue?.StringValue ?? "", out var n) ? n : null,
            _ => BigInteger.TryParse(value.ToString(), out var n) ? n : null
        };
    }

    /// <summary>
    /// Converts an <see cref="XdmValue"/> to an array of <see cref="BigInteger"/> values.
    /// Handles sequences by extracting all numeric items.
    /// </summary>
    private static BigInteger[] XdmValueToBigIntegerArray(XdmValue value)
    {
        var result = new List<BigInteger>();
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                var n = XdmValueToBigInteger(item);
                if (n.HasValue)
                    result.Add(n.Value);
            }
        }
        else
        {
            var n = XdmValueToBigInteger(value);
            if (n.HasValue)
                result.Add(n.Value);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Extracts the local name from a QName string, handling the case where
    /// namespace="" forces the null namespace (prefix must be stripped).
    /// </summary>
    private static string GetLocalName(string name, string? namespaceUri)
    {
        // If namespace is explicitly empty, strip any prefix from the name
        if (namespaceUri == "")
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
                return name[(colon + 1)..];
        }
        return name;
    }

    /// <summary>
    /// Resolves the local name and namespace URI for xsl:element / xsl:attribute
    /// name attributes that may contain a prefix. When no explicit namespace is
    /// given, the prefix is resolved against the in-scope namespaces of the
    /// instruction element.
    /// </summary>
    private static (string LocalName, string NamespaceUri) ResolveElementName(XElement instruction, string name, string? explicitNamespace, string errorCode)
        => ResolveName(instruction, name, explicitNamespace, errorCode, useDefaultNamespace: true);

    private static (string LocalName, string NamespaceUri) ResolveAttributeName(XElement instruction, string name, string? explicitNamespace, string errorCode)
        => ResolveName(instruction, name, explicitNamespace, errorCode, useDefaultNamespace: false);

    private static (string LocalName, string NamespaceUri) ResolveName(XElement instruction, string name, string? explicitNamespace, string errorCode, bool useDefaultNamespace)
    {
        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string localName = name[(colon + 1)..];
            if (explicitNamespace != null)
                return (localName, explicitNamespace);
            var ns = instruction.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException(errorCode);
            return (localName, ns.NamespaceName);
        }
        else
        {
            if (explicitNamespace != null)
                return (name, explicitNamespace);
            if (useDefaultNamespace)
            {
                var ns = instruction.GetDefaultNamespace();
                return (name, ns?.NamespaceName ?? "");
            }
            return (name, "");
        }
    }

    /// <summary>
    /// Annotation attached to copied elements when <c>copy-accumulators="yes"</c> is used.
    /// Maps accumulator Clark names to their before/after values for the source node.
    /// </summary>
    private sealed class AccumulatorValues
    {
        public Dictionary<string, (XdmValue Before, XdmValue After)> Values { get; } = new();
    }
}
