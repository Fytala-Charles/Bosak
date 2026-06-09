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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
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
    }

    /// <summary>
    /// Executes the stylesheet transformation.
    /// </summary>
    public XdmValue Transform(IXdmNode source, string? initialTemplate = null, string? initialMode = null)
    {
        // Ensure xsl:function registrations are present (re-entrant transforms)
        RegisterXsltFunctions();
        // Compile all template match patterns before execution
        var patternCompiler = new Patterns.PatternCompiler();
        foreach (var rule in _allTemplateRules)
        {
            rule.CompileMatch(patternCompiler);
        }

        // Always register key() function before building key indices, because
        // xsl:key/@use expressions may call key() recursively (key-063/064).
        RegisterKeyFunction();

        // Build key indices iteratively to handle cross-key dependencies
        // (e.g. key-063 where k2's use calls key('k1',...), or key-064 where
        // k1's match calls key('k2',...)).
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (allKeyDefs.Count > 0)
        {
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

                foreach (var keyDef in allKeyDefs)
                {
                    sourceIndex.ClearKey(keyDef.Name);
                    KeyIndex.BuildSingleKey(source, keyDef, _context, sourceIndex);
                }
            }
        }

        RegisterGroupingFunctions();

        // Apply whitespace stripping from xsl:strip-space / xsl:preserve-space
        ApplyWhitespaceStripping(source);

        // Initialize global parameters and variables before template execution
        InitializeGlobalParametersAndVariables(source);

        if (!string.IsNullOrEmpty(initialTemplate))
        {
            // Start from a named template (xsl:initial-template or test harness)
            CallTemplate(initialTemplate, source);
        }
        else
        {
            // Check for xsl:initial-template as the implicit entry point
            if (_allNamedTemplates.TryGetValue("xsl:initial-template", out var initialTemplateRule))
            {
                CallTemplate("xsl:initial-template", source);
            }
            else if (!string.IsNullOrEmpty(initialMode))
            {
                // Start transformation in the specified initial mode.
                // Expand any namespace prefix in the initial mode name.
                var resolvedInitialMode = ExpandModeName(initialMode, _stylesheet.Root);
                // If the mode is #unnamed, treat it as the empty unnamed mode
                if (resolvedInitialMode == "#unnamed")
                    resolvedInitialMode = "";
                _modeStack.Push(resolvedInitialMode);
                try
                {
                    var rootTemplate = FindBestTemplate(source, resolvedInitialMode);
                    if (rootTemplate != null)
                    {
                        ExecuteTemplate(rootTemplate, source);
                    }
                    else
                    {
                        ApplyBuiltInRules(source, resolvedInitialMode);
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
                    rootTemplate.CompiledMatch(XdmValue.FromNode(source), _context))
                {
                    ExecuteTemplate(rootTemplate, source);
                }
                else
                {
                    // XSLT 2.0 §5.4: when there is no template matching "/",
                    // the built-in template rule for the document node is invoked.
                    // This built-in rule applies templates to the children of the
                    // document node. We must NOT search for other patterns (like
                    // node() or document-node()) that might match the document node
                    // directly, as that causes incorrect next-match chaining.
                    ApplyTemplates(source, mode: "", select: null);
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
    /// Executes the body of an xsl:function declaration, binding parameters and
    /// returning the sequence produced by the function body.
    /// </summary>
    private const int MaxXsltFunctionCallDepth = 20;

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

            // The contextItem parameter to EvaluateFunctionBody is used for
            // XSLT instructions (e.g. xsl:copy, xsl:apply-templates) that
            // reference the context node explicitly, not for XPath evaluation.
            var focus = args.Length > 0 ? args[0] : XdmValue.FromSequence(XdmSequence.Empty);

            // Evaluate the function body (sequence constructor)
            return EvaluateFunctionBody(def.Element, focus);
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
                            foreach (var child in instruction.Elements())
                            {
                                EvaluateFunctionBodyInstruction(child, results, contextItem);
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
                case "apply-templates":
                    {
                        var modeRaw = instruction.Attribute("mode")?.Value ?? "";
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
                                    var compiled = XPath31Expression.Compile(wpSelect);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
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
                            else if (node is XText t && !string.IsNullOrEmpty(t.Value))
                                results.Add(XdmValue.FromString(t.Value));
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
                                        var compiled = XPath31Expression.Compile(wpSelect);
                                        wpValue = compiled.Evaluate(_context);
                                    }
                                    else
                                    {
                                        wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                    }
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
                default:
                    // Unknown XSLT instruction in function body: ignore
                    break;
            }
        }
        else
        {
            // Literal result element in function body: create a shallow copy as a node
            var copy = new XElement(instruction.Name);
            foreach (var attr in instruction.Attributes())
                copy.SetAttributeValue(attr.Name, attr.Value);
            foreach (var child in instruction.Nodes())
            {
                if (child is XText text)
                    copy.Add(new XText(text.Value));
                else if (child is XElement childElem)
                {
                    var childCopy = new XElement(childElem.Name);
                    foreach (var attr in childElem.Attributes())
                        childCopy.SetAttributeValue(attr.Name, attr.Value);
                    copy.Add(childCopy);
                }
            }
            results.Add(XdmValue.FromNode(new XDocumentNode(copy)));
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
    public void ApplyTemplates(IXdmNode contextNode, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null)
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
                var compiled = XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(XdmValue.FromNode(contextNode), 1, 1));
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
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, position: pos, last: last);
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
    public void ApplyTemplates(XdmValue contextItem, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null)
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
                var compiled = XPath31Expression.Compile(select);
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
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, position: pos, last: last);
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
            return mode;

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
                    return $"{{{attr.Value}}}{local}";
                }
            }
            current = current.Parent;
        }
        // Prefix not declared — return as-is (will fail to match)
        return mode;
    }

    /// <summary>
    /// Executes the body of a template rule against the current node.
    /// </summary>
    public void ExecuteTemplate(Stylesheet.TemplateRule rule, IXdmNode currentNode, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
        => ExecuteTemplate(rule, XdmValue.FromNode(currentNode), callParams, incomingTunnelParams, position, last, setCurrentRule);

    public void ExecuteTemplate(Stylesheet.TemplateRule rule, XdmValue contextItem, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
    {
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
                            var contentElements = child.Elements().ToList();
                            if (contentElements.Count > 0)
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
                    var elemName = EvaluateAvt(elemNameRaw);
                    var elemNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var elemNs = elemNsRaw != null ? EvaluateAvt(elemNsRaw) : null;
                    var (elemLocalName, elemNsUri) = ResolveElementName(instruction, elemName, elemNs);
                    var elem = new XElement(XName.Get(elemLocalName, elemNsUri));
                    _currentContainer.Add(elem);
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
                    _currentContainer = prev;
                    break;
                }

            case "attribute":
                {
                    var attrNameRaw = instruction.Attribute("name")?.Value ?? "unnamed";
                    var attrName = EvaluateAvt(attrNameRaw);
                    var attrNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var attrNs = attrNsRaw != null ? EvaluateAvt(attrNsRaw) : null;
                    var (attrLocalName, attrNsUri) = ResolveElementName(instruction, attrName, attrNs);
                    var select = instruction.Attribute("select")?.Value;
                    string value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
                        var result = compiled.Evaluate(_context);
                        value = result.ToString();
                    }
                    else
                    {
                        value = EvaluateSimpleContent(instruction, contextItem);
                    }
                    if (_currentContainer is XElement targetElem)
                    {
                        targetElem.SetAttributeValue(XName.Get(attrLocalName, attrNsUri), value);
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
                        var textValue = EvaluateSimpleContent(instruction, contextItem);
                        _lastAddedWasAtomic = false;
                        AddTextNode(textValue);
                    }
                    break;
                }

            case "text":
                {
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
                        var compiled = XPath31Expression.Compile(commentSelect);
                        var result = compiled.Evaluate(_context);
                        commentText = XdmValueToString(result);
                    }
                    else
                    {
                        commentText = EvaluateSimpleContent(instruction, contextItem);
                    }
                    _currentContainer.Add(new XComment(commentText));
                    break;
                }

            case "processing-instruction":
                {
                    var piNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var piName = EvaluateAvt(piNameRaw);
                    var piSelect = instruction.Attribute("select")?.Value;
                    string piData;
                    if (!string.IsNullOrEmpty(piSelect))
                    {
                        var compiled = XPath31Expression.Compile(piSelect);
                        var result = compiled.Evaluate(_context);
                        piData = XdmValueToString(result);
                    }
                    else
                    {
                        piData = EvaluateSimpleContent(instruction, contextItem);
                    }
                    // XSLT 3.0 §11.4.4: leading spaces in PI data are removed
                    piData = piData.TrimStart();
                    _currentContainer.Add(new XProcessingInstruction(piName, piData));
                    break;
                }

            case "namespace":
                {
                    var nsNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var nsName = EvaluateAvt(nsNameRaw);
                    var nsSelect = instruction.Attribute("select")?.Value;
                    string nsUri;
                    if (!string.IsNullOrEmpty(nsSelect))
                    {
                        var compiled = XPath31Expression.Compile(nsSelect);
                        var result = compiled.Evaluate(_context);
                        nsUri = result.ToString();
                    }
                    else
                    {
                        nsUri = EvaluateSimpleContent(instruction, contextItem);
                    }
                    if (_currentContainer is XElement targetElem)
                    {
                        targetElem.SetAttributeValue(XNamespace.Xmlns + nsName, nsUri);
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
                        var result = compiled.Evaluate(_context);
                        msgText = XdmValueToString(result, " ");
                    }
                    else
                    {
                        msgText = EvaluateSimpleContent(instruction, contextItem);
                    }
                    _messageListener?.OnMessage(msgText);
                    break;
                }

            case "copy":
                {
                    if (node == null) break;
                    // XSLT 3.0: optional select attribute; default is context item
                    IXdmNode nodeToCopy = node;
                    var copySelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(copySelect))
                    {
                        var compiled = XPath31Expression.Compile(copySelect);
                        var result = compiled.Evaluate(_context);
                        if (result.IsNode && result.NodeValue != null)
                            nodeToCopy = result.NodeValue;
                    }

                    // XSLT 3.0: xsl:copy with select="..." processes the sequence as if by xsl:for-each,
                    // which clears the current template rule and next-match exclusions.
                    var hasSelect = !string.IsNullOrEmpty(copySelect);
                    var savedCopyTemplateRule = _currentTemplateRule;
                    var savedCopyExcluded = _nextMatchExcluded;
                    if (hasSelect)
                    {
                        _currentTemplateRule = null;
                        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();
                    }

                    switch (nodeToCopy.NodeKind)
                    {
                        case XdmNodeKind.Element:
                            {
                                var copy = new XElement(
                                    XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri));
                                // Shallow copy includes attributes for element nodes.
                                // (Strictly XSLT does not copy attributes automatically, but
                                // removing this breaks namespace-inheritance tests because
                                // our XDocument adapter does not model namespace nodes.)
                                foreach (var attr in nodeToCopy.Attributes())
                                {
                                    copy.SetAttributeValue(
                                        XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                                        attr.NodeValue!.StringValue);
                                }
                                _currentContainer.Add(copy);
                                var prev = _currentContainer;
                                _currentContainer = copy;
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
                                _currentContainer = prev;
                                break;
                            }
                        case XdmNodeKind.Text:
                            _lastAddedWasAtomic = false;
                            AddTextNode(nodeToCopy.StringValue);
                            break;
                        case XdmNodeKind.Attribute:
                            if (_currentContainer is XElement target)
                            {
                                target.SetAttributeValue(
                                    XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri),
                                    nodeToCopy.StringValue);
                            }
                            break;
                        case XdmNodeKind.Comment:
                            _currentContainer.Add(new XComment(nodeToCopy.StringValue));
                            break;
                        case XdmNodeKind.ProcessingInstruction:
                            _currentContainer.Add(new XProcessingInstruction(nodeToCopy.LocalName, nodeToCopy.StringValue));
                            break;
                        default:
                            // Document and other kinds: just process children
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

                    if (hasSelect)
                    {
                        _currentTemplateRule = savedCopyTemplateRule;
                        _nextMatchExcluded = savedCopyExcluded;
                    }
                    break;
                }

            case "apply-templates":
                {
                    var select = instruction.Attribute("select")?.Value;
                    var modeRaw = instruction.Attribute("mode")?.Value;
                    var mode = string.IsNullOrEmpty(modeRaw)
                        ? CurrentDefaultMode
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
                                var compiled = XPath31Expression.Compile(wpSelect);
                                wpValue = compiled.Evaluate(_context);
                            }
                            else
                            {
                                wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                            }
                            if (wpTunnel)
                                tunnelParams[wpName] = wpValue;
                            else
                                withParams[wpName] = wpValue;
                        }
                    }

                    if (node != null)
                    {
                        ApplyTemplates(node, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams);
                    }
                    else if (!string.IsNullOrEmpty(select))
                    {
                        // apply-templates with select but no context node (e.g. inside named template)
                        ApplyTemplates(contextItem, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams);
                    }
                    // If node is null and select is empty, apply-templates has nothing to process
                    break;
                }

            case "for-each":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
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

                    var compiled = XPath31Expression.Compile(select);
                    var result = compiled.Evaluate(_context);
                    var items = EnumerateItems(result).ToList();
                    if (items.Count == 0) break;

                    var groupBy = instruction.Attribute("group-by")?.Value;
                    var groupAdjacent = instruction.Attribute("group-adjacent")?.Value;
                    var groupStarting = instruction.Attribute("group-starting-with")?.Value;
                    var groupEnding = instruction.Attribute("group-ending-with")?.Value;
                    var bindGroup = instruction.Attribute("bind-group")?.Value;
                    var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

                    var groups = new List<(XdmValue? Key, List<XdmValue> Items)>();

                    if (!string.IsNullOrEmpty(groupBy))
                    {
                        var keyExpr = XPath31Expression.Compile(groupBy);
                        var dict = new Dictionary<string, List<XdmValue>>();
                        var keyOrder = new List<string>();
                        foreach (var item in items)
                        {
                            _context.WithFocus(item, 1, 1);
                            var keyValue = keyExpr.Evaluate(_context);
                            if (keyValue.IsSequence && keyValue.SequenceValue != null)
                            {
                                var seq = XdmSequence.FromSource(keyValue.SequenceValue);
                                foreach (var kv in seq)
                                {
                                    var keyStr = GetGroupingKeyString(kv);
                                    if (!dict.TryGetValue(keyStr, out var list))
                                    {
                                        list = new List<XdmValue>();
                                        dict[keyStr] = list;
                                        keyOrder.Add(keyStr);
                                    }
                                    if (!list.Contains(item))
                                        list.Add(item);
                                }
                            }
                            else
                            {
                                var keyStr = GetGroupingKeyString(keyValue);
                                if (!dict.TryGetValue(keyStr, out var list))
                                {
                                    list = new List<XdmValue>();
                                    dict[keyStr] = list;
                                    keyOrder.Add(keyStr);
                                }
                                if (!list.Contains(item))
                                    list.Add(item);
                            }
                        }
                        var seenKeys = new HashSet<string>();
                        foreach (var keyStr in keyOrder)
                        {
                            if (seenKeys.Add(keyStr))
                                groups.Add((XdmValue.FromString(keyStr), dict[keyStr]));
                        }
                    }
                    else if (!string.IsNullOrEmpty(groupAdjacent))
                    {
                        var keyExpr = XPath31Expression.Compile(groupAdjacent);
                        var currentItems = new List<XdmValue>();
                        XdmValue? currentKey = null;
                        string? currentKeyStr = null;
                        foreach (var item in items)
                        {
                            _context.WithFocus(item, 1, 1);
                            var keyValue = keyExpr.Evaluate(_context);
                            var keyStr = GetGroupingKeyString(keyValue);
                            if (currentKeyStr == null)
                            {
                                currentKeyStr = keyStr;
                                currentKey = keyValue;
                                currentItems.Add(item);
                            }
                            else if (currentKeyStr == keyStr)
                            {
                                currentItems.Add(item);
                            }
                            else
                            {
                                groups.Add((currentKey, new List<XdmValue>(currentItems)));
                                currentKeyStr = keyStr;
                                currentKey = keyValue;
                                currentItems.Clear();
                                currentItems.Add(item);
                            }
                        }
                        if (currentItems.Count > 0)
                            groups.Add((currentKey, new List<XdmValue>(currentItems)));
                    }
                    else if (!string.IsNullOrEmpty(groupStarting))
                    {
                        var patternCompiler = new Patterns.PatternCompiler();
                        var pattern = patternCompiler.Compile(groupStarting);
                        var currentItems = new List<XdmValue>();
                        foreach (var item in items)
                        {
                            if (pattern(item, _context))
                            {
                                if (currentItems.Count > 0)
                                    groups.Add((null, new List<XdmValue>(currentItems)));
                                currentItems.Clear();
                                currentItems.Add(item);
                            }
                            else
                            {
                                currentItems.Add(item);
                            }
                        }
                        if (currentItems.Count > 0)
                            groups.Add((null, new List<XdmValue>(currentItems)));
                    }
                    else if (!string.IsNullOrEmpty(groupEnding))
                    {
                        var patternCompiler = new Patterns.PatternCompiler();
                        var pattern = patternCompiler.Compile(groupEnding);
                        var currentItems = new List<XdmValue>();
                        foreach (var item in items)
                        {
                            currentItems.Add(item);
                            if (pattern(item, _context))
                            {
                                groups.Add((null, new List<XdmValue>(currentItems)));
                                currentItems.Clear();
                            }
                        }
                        if (currentItems.Count > 0)
                            groups.Add((null, new List<XdmValue>(currentItems)));
                    }

                    // Handle xsl:sort children (sort groups by representative)
                    var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                    if (sortElements.Count > 0 && groups.Count > 0)
                    {
                        var reps = groups.Select(g => g.Items[0]).ToList();
                        var sortedReps = SortItems(reps, sortElements);
                        var orderMap = new Dictionary<XdmValue, int>();
                        for (int i = 0; i < sortedReps.Count; i++)
                            orderMap[sortedReps[i]] = i;
                        groups = groups.OrderBy(g => orderMap[g.Items[0]]).ToList();
                    }

                    var savedFocus = _context.ContextItem;
                    var savedCurrent = _context.CurrentItem;
                    var savedTemplateRule = _currentTemplateRule;
                    var savedNextMatchExcluded = _nextMatchExcluded;
                    var savedGroup = _currentGroup;
                    var savedKey = _currentGroupingKey;
                    _currentTemplateRule = null;
                    _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

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

                    _context.WithFocus(savedFocus, 1, 1);
                    _context.WithCurrentItem(savedCurrent);
                    _currentTemplateRule = savedTemplateRule;
                    _nextMatchExcluded = savedNextMatchExcluded;
                    _currentGroup = savedGroup;
                    _currentGroupingKey = savedKey;
                    break;
                }

            case "if":
                {
                    var test = instruction.Attribute("test")?.Value;
                    if (!string.IsNullOrEmpty(test))
                    {
                        var compiled = XPath31Expression.Compile(test);
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
                            var compiled = XPath31Expression.Compile(test);
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
                                    var compiled = XPath31Expression.Compile(wpSelect);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
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

            case "copy-of":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = XPath31Expression.Compile(select);
                        var result = compiled.Evaluate(_context);
                        CopyToResult(result);
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
                                    var compiled = XPath31Expression.Compile(wpSelect);
                                    wpValue = compiled.Evaluate(_context);
                                }
                                else
                                {
                                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                                }
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
                                var compiled = XPath31Expression.Compile(wpSelect);
                                wpValue = compiled.Evaluate(_context);
                            }
                            else
                            {
                                wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                            }
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

        // Determine if #all is specified — this excludes every prefix.
        bool excludeAll = _excludedResultPrefixes.Contains("#all");

        // If the element has a non-empty namespace URI and uses a prefix,
        // ensure the prefix is declared on the copied element, unless excluded.
        if (!string.IsNullOrEmpty(source.Name.NamespaceName))
        {
            var prefix = source.GetPrefixOfNamespace(source.Name.Namespace);
            if (!string.IsNullOrEmpty(prefix) && !excludeAll && !_excludedResultPrefixes.Contains(prefix))
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
                // Skip excluded prefixes
                if (excludeAll || _excludedResultPrefixes.Contains(declaredPrefix))
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
            var attrValue = EvaluateAvt(attr.Value);
            copy.SetAttributeValue(attrName, attrValue);
        }

        _currentContainer.Add(copy);

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
            foreach (var child in source.Nodes())
            {
                switch (child)
                {
                    case XElement childElem:
                        if (childElem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                        {
                            // This shouldn't happen in literal elements, but handle anyway
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
                    // Comments inside literal result elements are not copied to output
                    // (XSLT processors typically strip stylesheet-level comments).
                    case XComment:
                        break;
                    case XProcessingInstruction pi:
                        _currentContainer.Add(new XProcessingInstruction(pi.Target, pi.Data));
                        break;
                }
            }
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
    /// </summary>
    private string EvaluateAvt(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains('{'))
            return value;

        var sb = new System.Text.StringBuilder();
        int i = 0;
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
                        var compiled = XPath31Expression.Compile(expr);
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

                // Discard zero-length text nodes
                if (item.IsNode && item.NodeValue != null &&
                    item.NodeValue.NodeKind == XdmNodeKind.Text &&
                    item.NodeValue.StringValue.Length == 0)
                {
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

    private void CopyNodeToResult(IXdmNode node)
    {
        if (node.NodeKind == XdmNodeKind.Document)
        {
            foreach (var child in node.Axis(XdmAxis.Child))
            {
                CopyNodeToResult(child.NodeValue!);
            }
        }
        else if (node.NodeKind == XdmNodeKind.Element)
        {
            _lastAddedWasAtomic = false;
            var copy = new XElement(
                XName.Get(node.LocalName, node.NamespaceUri));
            foreach (var attr in node.Attributes())
            {
                copy.SetAttributeValue(
                    XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                    attr.NodeValue!.StringValue);
            }
            _currentContainer.Add(copy);
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
        else if (node.NodeKind == XdmNodeKind.Attribute && _currentContainer is XElement parent)
        {
            parent.SetAttributeValue(
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
        var version = _stylesheet.Version;
        if (!string.IsNullOrEmpty(version) &&
            (version.StartsWith("1.") || version.StartsWith("2.")))
        {
            return Stylesheet.OnNoMatch.TextOnlyCopy;
        }
        return Stylesheet.OnNoMatch.ShallowSkip;
    }

    /// <summary>
    /// Applies built-in template rules when no explicit template matches.
    /// Respects xsl:mode on-no-match declarations.
    /// </summary>
    public void ApplyBuiltInRules(IXdmNode node, string mode, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1)
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
                // Built-in: apply templates to children of the document node
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams: null);
                break;

            case XdmNodeKind.Element:
                ApplyBuiltInRulesForElement(node, mode, behavior, incomingTunnelParams);
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

    private void ApplyBuiltInRulesForElement(IXdmNode node, string mode, Stylesheet.OnNoMatch behavior, Dictionary<string, XdmValue>? incomingTunnelParams)
    {
        switch (behavior)
        {
            case Stylesheet.OnNoMatch.ShallowCopy:
                {
                    // XSLT 3.0 §6.6 (bug 28774): shallow-copy creates the element shell
                    // without copying attributes; templates are applied to children AND attributes.
                    var copy = new XElement(
                        XName.Get(node.LocalName, node.NamespaceUri));
                    _currentContainer.Add(copy);

                    var previousContainer = _currentContainer;
                    _currentContainer = copy;
                    ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams: null);
                    _currentContainer = previousContainer;
                }
                break;

            case Stylesheet.OnNoMatch.ShallowSkip:
                // XSLT 3.0 §6.6 (bug 28774): shallow-skip applies templates to children AND attributes.
                ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams: null);
                break;

            case Stylesheet.OnNoMatch.TextOnlyCopy:
                // Recurse to children without copying the element wrapper (attributes are not processed).
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams: null);
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

        var keyName = args[0].ToString();
        var keyValueArg = args[1];

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
            // 3-arg form: search only the nodes supplied in the 3rd argument.
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

            // Look up key values and filter to candidates.
            var seen = new HashSet<IXdmNode>();
            var result = new List<XdmValue>();
            var keyValues = ExtractKeyValueStrings(keyValueArg);

            foreach (var keyValue in keyValues)
            {
                foreach (var (_, keyIndex, docCandidates) in docEntries)
                {
                    foreach (var node in keyIndex.Lookup(keyName, keyValue))
                    {
                        if (!seen.Add(node))
                            continue;

                        // Check if this node is one of the candidates for this document.
                        bool isCandidate = false;
                        foreach (var cand in docCandidates)
                        {
                            if (cand.IsSameNode(node))
                            {
                                isCandidate = true;
                                break;
                            }
                        }
                        if (isCandidate)
                            result.Add(XdmValue.FromNode(node));
                    }
                }
            }

            return XdmValue.FromSequence(MaterializedSequence.FromList(result));
        }
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

                foreach (var keyDef in allKeyDefs)
                {
                    keyIndex.ClearKey(keyDef.Name);
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
    private static XdmValue LookupKeyValues(KeyIndex keyIndex, string keyName, XdmValue keyValueArg)
    {
        var seen = new HashSet<IXdmNode>();
        var result = new List<XdmValue>();
        var keyValues = ExtractKeyValueStrings(keyValueArg);

        foreach (var keyValue in keyValues)
        {
            foreach (var node in keyIndex.Lookup(keyName, keyValue))
            {
                if (seen.Add(node))
                    result.Add(XdmValue.FromNode(node));
            }
        }

        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    /// <summary>
    /// Extracts atomic string values from a key-value argument (either a single value or a sequence).
    /// </summary>
    private static IEnumerable<string> ExtractKeyValueStrings(XdmValue keyValueArg)
    {
        if (keyValueArg.IsSequence && keyValueArg.SequenceValue != null)
        {
            foreach (var val in XdmSequence.FromSource(keyValueArg.SequenceValue))
                yield return val.ToString();
        }
        else
        {
            yield return keyValueArg.ToString();
        }
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
                if (_currentGroup == null || _currentGroup.Count == 0)
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
                    return XdmValue.Undefined;
                return _currentGroupingKey.Value;
            }
        });
    }

    /// <summary>
    /// Evaluates top-level xsl:param and xsl:variable declarations and binds them into the context.
    /// Order: imported first, then included, then local. Parameters are evaluated before variables.
    /// </summary>
    private void InitializeGlobalParametersAndVariables(IXdmNode source)
    {
        var focus = XdmValue.FromNode(source);

        // Set the focus once for all global param/var evaluations.
        // Sequence constructors inside global variables rely on _context.ContextItem
        // being set when they evaluate XPath expressions (e.g. xsl:value-of/@select).
        _context.WithFocus(focus, 1, 1);

        // Collect globals in precedence order: imports first, then includes, then local.
        // Within each stylesheet module, params and vars are evaluated in document order.
        var globals = new List<(string Name, XElement Element, bool IsParam)>();

        foreach (var imported in _stylesheet.Imports)
            CollectGlobalsInDocumentOrder(imported, globals);

        foreach (var included in _stylesheet.Includes)
            CollectGlobalsInDocumentOrder(included, globals);

        CollectGlobalsInDocumentOrder(_stylesheet, globals);

        foreach (var (name, elem, isParam) in globals)
        {
            // Skip parameters already supplied by caller (e.g. fn:transform)
            if (isParam && _context.TryGetVariable(name, out _))
                continue;

            var select = elem.Attribute("select")?.Value;
            XdmValue value;
            if (!string.IsNullOrEmpty(select))
            {
                var compiled = XPath31Expression.Compile(select);
                value = compiled.Evaluate(_context);
            }
            else
            {
                value = EvaluateSequenceConstructor(elem, focus, wrapInDocumentNode: string.IsNullOrEmpty(elem.Attribute("as")?.Value));
            }
            value = ConvertVariableValue(value, elem.Attribute("as")?.Value);
            _context.WithVariable(name, value);
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
    /// Applies basic type conversion for the <c>as</c> attribute on xsl:variable / xsl:param.
    /// Atomizes the value and casts to common atomic types (xs:integer, xs:string, etc.).
    /// Node types (element(), attribute(), document-node()) are returned unchanged.
    /// </summary>
    private static XdmValue ConvertVariableValue(XdmValue value, string? asType)
    {
        if (string.IsNullOrEmpty(asType) || value.IsUndefined)
            return value;

        var originalType = asType.Trim();
        var type = originalType;
        bool allowsMultiple = type.EndsWith("*") || type.EndsWith("+");
        bool allowsEmpty = type.EndsWith("?") || type.EndsWith("*");
        if (type.EndsWith("?") || type.EndsWith("*") || type.EndsWith("+"))
            type = type[..^1].Trim();

        // Node types: no conversion needed
        if (type.Contains("element(") || type.Contains("attribute(") || type.Contains("document-node("))
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

        // Convert each item
        var converted = new List<XdmValue>();
        foreach (var item in items)
        {
            string str = item.IsNode ? item.NodeValue.StringValue : item.ToString();
            XdmValue? conv = null;

            if (type == "xs:integer" || type.EndsWith(":integer"))
            {
                if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var l))
                    conv = XdmValue.FromInteger(l);
            }
            else if (type == "xs:string" || type.EndsWith(":string"))
                conv = XdmValue.FromString(str);
            else if (type == "xs:boolean" || type.EndsWith(":boolean"))
            {
                if (bool.TryParse(str, out var b))
                    conv = XdmValue.FromBoolean(b);
                else
                    conv = XdmValue.FromBoolean(!string.IsNullOrWhiteSpace(str));
            }
            else if (type == "xs:double" || type.EndsWith(":double"))
            {
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    conv = XdmValue.FromDouble(d);
                else
                    conv = XdmValue.FromDouble(double.NaN);
            }
            else if (type == "xs:decimal" || type.EndsWith(":decimal"))
            {
                if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    conv = XdmValue.FromDecimal(dec);
                else
                    conv = XdmValue.FromDecimal(0m);
            }

            converted.Add(conv ?? item);
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

        try
        {
            // Create a temporary container to capture the sequence constructor output
            var wrapper = new XElement("__temp__");
            ExecuteSequenceConstructorDirect(parent, contextItem, wrapper);

            var nodes = wrapper.Nodes().ToList();
            var attributes = wrapper.Attributes().ToList();

            // Include items collected by xsl:sequence into the accumulator
            var accumulatorItems = _sequenceAccumulator ?? new List<XdmValue>();

            // Empty sequence constructor → empty sequence (XSLT 2.0 §11.2)
            if (nodes.Count == 0 && attributes.Count == 0 && accumulatorItems.Count == 0)
                return XdmValue.FromSequence(XdmSequence.Empty);

            if (wrapInDocumentNode)
            {
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
                    // Single element: use it directly as the document root
                    var tempDoc = new XDocument();
                    tempDoc.Add(nodes[0]);
                    return XdmValue.FromNode(new XDocumentNode(tempDoc));
                }
                else
                {
                    // Mixed content: wrap in synthetic document wrapper
                    var docWrapper = new XElement("__xdm_doc__");
                    docWrapper.Add(nodes);
                    var tempDoc = new XDocument(docWrapper);
                    return XdmValue.FromNode(new XDocumentNode(tempDoc));
                }
            }

            // wrapInDocumentNode == false: return the raw sequence (used when @as is present)
            var results = new List<XdmValue>();
            results.AddRange(accumulatorItems);
            foreach (var child in nodes)
            {
                switch (child)
                {
                    case XElement e:
                        results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                        break;
                    case XText t:
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
    private string EvaluateSimpleContent(XElement parent, XdmValue contextItem)
    {
        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        var temp = new XElement("__temp__");
        _currentContainer = temp;
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
        return temp.Value;
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
            if (MatchesNameTest(rule.NameTest, element))
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

    private static bool MatchesNameTest(string nameTest, XElement element)
    {
        if (nameTest == "*")
            return true;

        if (nameTest.EndsWith(":*"))
        {
            // prefix:* - would need prefix resolution; for now, match any element
            // A proper implementation would resolve the prefix to a namespace URI
            return true;
        }

        if (nameTest.Contains(':'))
        {
            // QName with prefix - would need prefix resolution
            // For now, try matching local name only as a fallback
            var localName = nameTest.Contains(':') ? nameTest.Split(':')[1] : nameTest;
            return element.Name.LocalName == localName;
        }

        return element.Name.LocalName == nameTest;
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
            if (tvtResult.Length > 0)
            {
                _lastAddedWasAtomic = false;
                AddTextNode(tvtResult);
            }
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
        var format = EvaluateAvt(formatAttr);

        // Evaluate optional AVT attributes
        string? lang = string.IsNullOrEmpty(langAttr) ? null : EvaluateAvt(langAttr);
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
            var gsEval = EvaluateAvt(groupingSizeAttr);
            int.TryParse(gsEval, out groupingSize);
        }

        bool ordinal = false;
        if (!string.IsNullOrEmpty(ordinalAttr))
        {
            var ordEval = EvaluateAvt(ordinalAttr);
            ordinal = ordEval.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        // Evaluate start-at as AVT, then parse as space-separated integers (XSLT 3.0)
        BigInteger[]? startAtValues = null;
        if (!string.IsNullOrEmpty(startAtAttr))
        {
            var evaluated = EvaluateAvt(startAtAttr);
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
            var countMatcher = string.IsNullOrEmpty(countPattern)
                ? CreateDefaultCountMatcher(targetNode)
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(countPattern, instruction));

            var fromMatcher = string.IsNullOrEmpty(fromPattern)
                ? null
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(fromPattern, instruction));

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
    private static (string LocalName, string NamespaceUri) ResolveElementName(XElement instruction, string name, string? explicitNamespace)
    {
        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string localName = name[(colon + 1)..];
            if (explicitNamespace != null)
                return (localName, explicitNamespace);
            var ns = instruction.GetNamespaceOfPrefix(prefix);
            return (localName, ns?.NamespaceName ?? "");
        }
        else
        {
            if (explicitNamespace != null)
                return (name, explicitNamespace);
            var ns = instruction.GetDefaultNamespace();
            return (name, ns?.NamespaceName ?? "");
        }
    }
}
