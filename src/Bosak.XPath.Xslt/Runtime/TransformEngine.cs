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
//                      | Charles Korthout | 0.8   | 26-05-2026     | Added xsl:copy, fixed for-each variable scoping, AVT evaluation in literal elements      |
//                      | Charles Korthout | 0.9   | 26-05-2026     | Added initial-template support, fixed xsl:copy to copy attributes                       |
//                      | Charles Korthout | 1.0   | 26-05-2026     | Added xsl:mode on-no-match support; atomic for-each (EnumerateItems); keyword var names  |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added global variable and parameter initialization from stylesheet/includes/imports      |
//                      | Charles Korthout | 1.4   | 27-05-2026     | Fixed AVT sequence atomization, version-aware built-in rules, pattern // support         |
//                      | Charles Korthout | 1.5   | 27-05-2026     | Process text nodes in sequence constructors; strip document-level whitespace            |
//                      | Charles Korthout | 1.7   | 28-05-2026     | Added xsl:next-match with excluded-rule chain; call-template clears current template rule |
//                      | Charles Korthout | 1.8   | 29-05-2026     | Reduced MaxXsltFunctionCallDepth to 32 to prevent .NET stack overflow crashes             |
//                      | Charles Korthout | 1.9   | 29-05-2026     | Added expand-text / Text Value Template support with XPath string literal awareness       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XPath.Xslt.Api;
using Bosak.XPath.Xslt.Stylesheet;

namespace Bosak.XPath.Xslt.Runtime;

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

    // Flattened template rules and named templates from the entire stylesheet tree
    private readonly List<Stylesheet.TemplateRule> _allTemplateRules;
    private readonly Dictionary<string, Stylesheet.TemplateRule> _allNamedTemplates;

    // Variable scope stack for proper lexical scoping across call-template
    private readonly Stack<Dictionary<(string LocalName, string NamespaceUri), XdmValue?>> _varScopes = new();

    // Mode stack for #current resolution
    private readonly Stack<string> _modeStack = new();

    // Tunnel parameter stack: each frame is the tunnel params visible at that call depth
    private readonly Stack<Dictionary<string, XdmValue>> _tunnelParamStack = new();

    // Current template rule for xsl:next-match
    private Stylesheet.TemplateRule? _currentTemplateRule;

    // Accumulated excluded rules for the current xsl:next-match chain
    private HashSet<Stylesheet.TemplateRule> _nextMatchExcluded = new();

    // Key index for key() function lookups
    private KeyIndex? _keyIndex;

    // Recursion depth guard for xsl:function and xsl:call-template calls
    private int _xsltFunctionCallDepth;
    private int _callTemplateDepth;

    // Recursion depth guard for xsl:apply-templates
    private int _applyTemplatesDepth;
    private const int MaxApplyTemplatesDepth = 256;

    /// <summary>The parsed xsl:output serialization properties.</summary>
    public Stylesheet.OutputProperties? OutputProperties => _stylesheet.OutputProperties;

    public TransformEngine(Stylesheet.Stylesheet stylesheet, EvaluationContext? context = null)
    {
        _stylesheet = stylesheet;
        _context = context ?? new EvaluationContext();
        FunctionLibrary.Populate(_context);
        XsltFunctionLibrary.Populate(_context);

        _resultDocument = new XDocument();
        _currentContainer = _resultDocument;

        _allTemplateRules = _stylesheet.GetAllTemplateRules().ToList();
        _allNamedTemplates = _stylesheet.GetAllNamedTemplates();

        // Register namespace prefixes declared on the stylesheet root(s)
        foreach (var (prefix, nsUri) in _stylesheet.GetAllNamespaces())
        {
            _context.WithNamespace(prefix, nsUri);
        }

        // Register decimal-format declarations from the stylesheet
        RegisterDecimalFormats();

        // Register xsl:function declarations as callable XPath functions
        RegisterXsltFunctions();
    }

    /// <summary>
    /// Executes the stylesheet transformation.
    /// </summary>
    public XdmValue Transform(IXdmNode source, string? initialTemplate = null)
    {
        // Ensure xsl:function registrations are present (re-entrant transforms)
        RegisterXsltFunctions();
        // Compile all template match patterns before execution
        var patternCompiler = new Patterns.PatternCompiler();
        foreach (var rule in _allTemplateRules)
        {
            rule.CompileMatch(patternCompiler);
        }

        // Build key index if the stylesheet defines xsl:key
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (allKeyDefs.Count > 0)
        {
            _keyIndex = KeyIndex.Build(source, allKeyDefs, _context);
            RegisterKeyFunction();
        }

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
            else
            {
                // Look for a template matching "/" (document root)
                var rootTemplate = FindRootTemplate();
                if (rootTemplate != null)
                {
                    ExecuteTemplate(rootTemplate, source);
                }
                else
                {
                    // Apply templates to the source node in default mode
                    ApplyTemplates(source, mode: "", select: null);
                }
            }
        }

        // Return the result document, or document-level text if no root element was produced
        if (_documentLevelText.Length > 0 && _resultDocument.Root == null)
        {
            return XdmValue.FromString(_documentLevelText.ToString());
        }
        return XdmValue.FromNode(new Providers.Xml.XDocumentNode(_resultDocument));
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
    private const int MaxXsltFunctionCallDepth = 32;

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

            // Set focus to the first argument if present, otherwise empty sequence
            var focus = args.Length > 0 ? args[0] : XdmValue.FromSequence(XdmSequence.Empty);
            _context.WithFocus(focus, 1, 1);
            _context.WithCurrentItem(focus);

            // Evaluate the function body (sequence constructor)
            return EvaluateFunctionBody(def.Element, focus);
        }
        finally
        {
            _xsltFunctionCallDepth--;
            _context.RestoreVariables(snapshot);
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithFocus(_context.ContextItem, savedPosition, savedSize);
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
                            // CallTemplate produces a result tree fragment, not a sequence.
                            // For function bodies, we ignore call-template output.
                            CallTemplate(calledName, contextItem, withParams, tunnelParams);
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
            results.Add(XdmValue.FromNode(new Providers.Xml.XDocumentNode(copy)));
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
            if (rule.Match != null && rule.Match.Trim() == "/")
                return rule;
        }
        return null;
    }

    /// <summary>
    /// Implements xsl:apply-templates: selects nodes and processes each with the best-matching template.
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
            List<IXdmNode> nodes;
            if (string.IsNullOrEmpty(select))
            {
                // Default: child nodes
                nodes = EnumerateNodes(contextNode.Axis(XdmAxis.Child)).ToList();
            }
            else
            {
                // Evaluate select expression
                var compiled = XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(XdmValue.FromNode(contextNode), 1, 1));
                nodes = EnumerateNodes(result).ToList();
                // XSLT apply-templates processes nodes in document order
                nodes.Sort((a, b) => a.DocumentOrder.CompareTo(b.DocumentOrder));
            }

            // Apply xsl:sort if present
            if (sortKeys != null && sortKeys.Count > 0)
            {
                nodes = SortNodes(nodes, sortKeys);
            }

            int pos = 1;
            int last = nodes.Count;
            foreach (var node in nodes)
            {
                var rule = FindBestTemplate(node, resolvedMode);
                if (rule != null)
                {
                    ExecuteTemplate(rule, node, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                }
                else
                {
                    ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, position: pos, last: last);
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
            return "";
        }
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
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        _context.WithFocus(contextItem, position, last);
        _context.WithCurrentItem(contextItem);

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
            _context.WithFocus(_context.ContextItem, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _tunnelParamStack.Pop();
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
                        value = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
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
                        var sep = instruction.Attribute("separator")?.Value ?? " ";
                        var textValue = XdmValueToString(result, sep);
                        AddTextNode(textValue);
                    }
                    else if (GetExpandText(instruction))
                    {
                        var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                        var tvtResult = EvaluateTvt(text);
                        AddTextNode(tvtResult);
                    }
                    break;
                }

            case "text":
                {
                    var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                    if (GetExpandText(instruction))
                    {
                        var tvtResult = EvaluateTvt(text);
                        AddTextNode(tvtResult);
                    }
                    else
                    {
                        AddTextNode(text);
                    }
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
                        commentText = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                    }
                    _currentContainer.Add(new XComment(commentText));
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

                    switch (nodeToCopy.NodeKind)
                    {
                        case XdmNodeKind.Element:
                            {
                                var copy = new XElement(
                                    XName.Get(nodeToCopy.LocalName, nodeToCopy.NamespaceUri));
                                // Shallow copy includes attributes for element nodes
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
                    break;
                }

            case "apply-templates":
                {
                    if (node == null) break;
                    var select = instruction.Attribute("select")?.Value;
                    var mode = instruction.Attribute("mode")?.Value ?? "";
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

                    ApplyTemplates(node, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams);
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
                        CopyToResult(result);
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
                    if (node == null) break;
                    if (_currentTemplateRule == null)
                    {
                        // xsl:next-match is only valid within a template invoked by apply-templates or next-match
                        // If called from a named template, for-each, or other context where the current
                        // template rule is absent, raise XTDE0560.
                        throw new InvalidOperationException("XTDE0560: xsl:next-match evaluated when the current template rule is absent.");
                    }

                    var nextMatchMode = _modeStack.Count > 0 ? _modeStack.Peek() : "";
                    _nextMatchExcluded.Add(_currentTemplateRule);
                    try
                    {
                        var nextRule = FindBestTemplate(node, nextMatchMode, _nextMatchExcluded);

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
                            ExecuteTemplate(nextRule, node, callParams: nextMatchParams, incomingTunnelParams: mergedTunnelParams);
                        }
                        else
                        {
                            ApplyBuiltInRules(node, nextMatchMode, mergedTunnelParams);
                        }
                    }
                    finally
                    {
                        _nextMatchExcluded.Remove(_currentTemplateRule);
                    }
                    break;
                }

            case "number":
                {
                    if (node != null)
                        ExecuteXsltNumber(instruction, node);
                    break;
                }

            default:
                // Unknown instruction: ignore for now
                break;
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

        // If the element has a non-empty namespace URI and uses a prefix,
        // ensure the prefix is declared on the copied element.
        if (!string.IsNullOrEmpty(source.Name.NamespaceName))
        {
            var prefix = source.GetPrefixOfNamespace(source.Name.Namespace);
            if (!string.IsNullOrEmpty(prefix))
            {
                copy.SetAttributeValue(XNamespace.Xmlns + prefix, source.Name.NamespaceName);
            }
        }

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
                copy.SetAttributeValue(attr.Name, attr.Value);
                continue;
            }

            var attrName = XName.Get(attr.Name.LocalName, attr.Name.NamespaceName);
            // AVTs are not evaluated in XSLT-namespace attributes
            var attrValue = attr.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
                ? attr.Value
                : EvaluateAvt(attr.Value);
            copy.SetAttributeValue(attrName, attrValue);
        }

        _currentContainer.Add(copy);

        var prev = _currentContainer;
        _currentContainer = copy;

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
                case XComment comment:
                    _currentContainer.Add(new XComment(comment.Value));
                    break;
                case XProcessingInstruction pi:
                    _currentContainer.Add(new XProcessingInstruction(pi.Target, pi.Data));
                    break;
            }
        }

        _currentContainer = prev;
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
                int end = value.IndexOf('}', i + 1);
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
    /// Copies an XDM value (node or sequence) into the result tree.
    /// </summary>
    private void CopyToResult(XdmValue value)
    {
        if (value.IsUndefined)
            return;

        if (value.IsNode && value.NodeValue != null)
        {
            CopyNodeToResult(value.NodeValue);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            // XSLT 2.0: consecutive atomic values in complex content are joined
            // with a single space (#x20) separator before becoming a text node.
            var atomics = new List<string>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode && item.NodeValue != null)
                {
                    if (atomics.Count > 0)
                    {
                        AddTextNode(string.Join(" ", atomics));
                        atomics.Clear();
                    }
                    CopyToResult(item);
                }
                else
                {
                    atomics.Add(item.ToString());
                }
            }
            if (atomics.Count > 0)
            {
                AddTextNode(string.Join(" ", atomics));
            }
        }
        else
        {
            AddTextNode(value.ToString());
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
            AddTextNode(node.StringValue);
        }
        else if (node.NodeKind == XdmNodeKind.Comment)
        {
            _currentContainer.Add(new XComment(node.StringValue));
        }
        else if (node.NodeKind == XdmNodeKind.ProcessingInstruction)
        {
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
    /// All versions default to ShallowSkip (apply templates to children) for
    /// compatibility with the XSLT test suite expectations.
    /// </summary>
    private Stylesheet.OnNoMatch GetDefaultOnNoMatch()
    {
        return Stylesheet.OnNoMatch.ShallowSkip;
    }

    /// <summary>
    /// Applies built-in template rules when no explicit template matches.
    /// Respects xsl:mode on-no-match declarations.
    /// </summary>
    public void ApplyBuiltInRules(IXdmNode node, string mode, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1)
    {
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        _context.WithFocus(XdmValue.FromNode(node), position, last);
        _context.WithCurrentItem(XdmValue.FromNode(node));
        try
        {
            var modeDef = _stylesheet.GetModeDefinition(mode);
            var behavior = modeDef?.OnNoMatch ?? GetDefaultOnNoMatch();

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
                if (_currentContainer is XElement && behavior != Stylesheet.OnNoMatch.Fail)
                {
                    AddTextNode(node.StringValue);
                }
                break;

            case XdmNodeKind.Attribute:
                // Built-in: copy attribute to current element
                if (_currentContainer is XElement elem && behavior != Stylesheet.OnNoMatch.Fail)
                {
                    elem.SetAttributeValue(
                        XName.Get(node.LocalName, node.NamespaceUri),
                        node.StringValue);
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
            }
        }
        finally
        {
            _context.WithCurrentItem(savedCurrent);
        }
    }

    private void ApplyBuiltInRulesForElement(IXdmNode node, string mode, Stylesheet.OnNoMatch behavior, Dictionary<string, XdmValue>? incomingTunnelParams)
    {
        switch (behavior)
        {
            case Stylesheet.OnNoMatch.ShallowCopy:
                {
                    var copy = new XElement(
                        XName.Get(node.LocalName, node.NamespaceUri));
                    foreach (var attr in node.Attributes())
                    {
                        copy.SetAttributeValue(
                            XName.Get(attr.NodeValue!.LocalName, attr.NodeValue!.NamespaceUri),
                            attr.NodeValue!.StringValue);
                    }
                    _currentContainer.Add(copy);

                    var previousContainer = _currentContainer;
                    _currentContainer = copy;
                    ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams: null);
                    _currentContainer = previousContainer;
                }
                break;

            case Stylesheet.OnNoMatch.ShallowSkip:
                // Skip element node, only apply-templates to children
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams: null);
                break;

            case Stylesheet.OnNoMatch.TextOnlyCopy:
                // Recurse to children without copying the element wrapper
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams: null);
                break;

            case Stylesheet.OnNoMatch.DeepCopy:
                CopyNodeToResult(node);
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
        if (_keyIndex == null)
            return XdmValue.Undefined;

        var keyName = args[0].ToString();
        var keyValueArg = args[1];

        var seen = new HashSet<IXdmNode>();
        var result = new List<XdmValue>();

        if (keyValueArg.IsSequence && keyValueArg.SequenceValue != null)
        {
            foreach (var val in XdmSequence.FromSource(keyValueArg.SequenceValue))
            {
                var keyValue = val.ToString();
                foreach (var node in _keyIndex.Lookup(keyName, keyValue))
                {
                    if (seen.Add(node))
                        result.Add(XdmValue.FromNode(node));
                }
            }
        }
        else
        {
            var keyValue = keyValueArg.ToString();
            foreach (var node in _keyIndex.Lookup(keyName, keyValue))
            {
                if (seen.Add(node))
                    result.Add(XdmValue.FromNode(node));
            }
        }

        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    /// <summary>
    /// Evaluates top-level xsl:param and xsl:variable declarations and binds them into the context.
    /// Order: imported first, then included, then local. Parameters are evaluated before variables.
    /// </summary>
    private void InitializeGlobalParametersAndVariables(IXdmNode source)
    {
        var focus = XdmValue.FromNode(source);

        // Evaluate global parameters first
        var allParams = _stylesheet.GetAllGlobalParameters();
        foreach (var (name, paramElem) in allParams)
        {
            // If already supplied by caller (e.g. fn:transform), keep the supplied value
            if (_context.TryGetVariable(name, out _))
                continue;

            var select = paramElem.Attribute("select")?.Value;
            XdmValue value;
            if (!string.IsNullOrEmpty(select))
            {
                var compiled = XPath31Expression.Compile(select);
                value = compiled.Evaluate(_context.WithFocus(focus, 1, 1));
            }
            else
            {
                value = EvaluateSequenceConstructor(paramElem, focus, wrapInDocumentNode: string.IsNullOrEmpty(paramElem.Attribute("as")?.Value));
            }
            _context.WithVariable(name, value);
        }

        // Then evaluate global variables
        var allVars = _stylesheet.GetAllGlobalVariables();
        foreach (var (name, varElem) in allVars)
        {
            var select = varElem.Attribute("select")?.Value;
            XdmValue value;
            if (!string.IsNullOrEmpty(select))
            {
                var compiled = XPath31Expression.Compile(select);
                value = compiled.Evaluate(_context.WithFocus(focus, 1, 1));
            }
            else
            {
                value = EvaluateSequenceConstructor(varElem, focus, wrapInDocumentNode: string.IsNullOrEmpty(varElem.Attribute("as")?.Value));
            }
            _context.WithVariable(name, value);
        }
    }

    /// <summary>
    /// Finds the highest-priority template rule that matches the given node in the given mode.
    /// </summary>
    private Stylesheet.TemplateRule? FindBestTemplate(IXdmNode node, string mode, HashSet<Stylesheet.TemplateRule>? excludedRules = null)
    {
        Stylesheet.TemplateRule? best = null;
        double bestPriority = double.NegativeInfinity;
        int bestImportPrecedence = -1;

        foreach (var rule in _allTemplateRules)
        {
            if (excludedRules != null && excludedRules.Contains(rule))
                continue;
            if (!MatchesMode(rule, mode))
                continue;
            if (rule.CompiledMatch == null)
                continue;
            if (rule.CompiledMatch(node, _context))
            {
                if (rule.Priority > bestPriority)
                {
                    best = rule;
                    bestPriority = rule.Priority;
                    bestImportPrecedence = rule.ImportPrecedence;
                }
                else if (rule.Priority == bestPriority && rule.ImportPrecedence < bestImportPrecedence)
                {
                    best = rule;
                    bestImportPrecedence = rule.ImportPrecedence;
                }
                else if (rule.Priority == bestPriority && rule.ImportPrecedence == bestImportPrecedence)
                {
                    // Tie-breaker: for document nodes, prefer more specific document patterns
                    // (e.g., doc('uri') over /) to avoid infinite recursion when doc() patterns
                    // are compiled to match any document node.
                    if (node.NodeKind == XdmNodeKind.Document && best != null)
                    {
                        if (IsMoreSpecificDocumentPattern(rule.Match, best.Match))
                        {
                            best = rule;
                            bestImportPrecedence = rule.ImportPrecedence;
                        }
                    }
                }
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
    /// Evaluates a sequence constructor (child nodes of an xsl:variable, xsl:param, etc.)
    /// and returns the resulting XDM value.
    /// </summary>
    private XdmValue EvaluateSequenceConstructor(XElement parent, XdmValue contextItem, bool wrapInDocumentNode = true)
    {
        // Create a temporary container to capture the sequence constructor output
        var wrapper = new XElement("__temp__");
        ExecuteSequenceConstructorDirect(parent, contextItem, wrapper);

        var nodes = wrapper.Nodes().ToList();

        // Empty sequence constructor → empty sequence (XSLT 2.0 §11.2)
        if (nodes.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);

        if (wrapInDocumentNode)
        {
            var elementCount = nodes.OfType<XElement>().Count();

            // XSLT 2.0+: non-empty sequence constructor content produces a document node.
            // LINQ-to-XML XDocument requires exactly one root element and does not
            // allow non-whitespace text nodes outside the root, so we can only
            // create a proper document node for single-element content.
            if (elementCount == 1 && nodes.Count == 1)
            {
                var tempDoc = new XDocument();
                tempDoc.Add(nodes[0]);
                return XdmValue.FromNode(new Providers.Xml.XDocumentNode(tempDoc));
            }
        }

        // Fall back: return the raw sequence
        var results = new List<XdmValue>();
        foreach (var child in nodes)
        {
            switch (child)
            {
                case XElement e:
                    results.Add(XdmValue.FromNode(new Providers.Xml.XDocumentNode(e)));
                    break;
                case XText t:
                    results.Add(XdmValue.FromString(t.Value));
                    break;
                case XComment c:
                    results.Add(XdmValue.FromNode(new Providers.Xml.XDocumentNode(c)));
                    break;
                case XProcessingInstruction pi:
                    results.Add(XdmValue.FromNode(new Providers.Xml.XDocumentNode(pi)));
                    break;
            }
        }
        if (results.Count == 1)
            return results[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(results));
    }

    /// <summary>
    /// Executes a sequence constructor directly into the specified container,
    /// handling text nodes, XSLT instructions, and literal result elements.
    /// </summary>
    private void ExecuteSequenceConstructorDirect(XElement parent, XdmValue contextItem, XContainer outputContainer)
    {
        var savedContainer = _currentContainer;
        _currentContainer = outputContainer;
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
        }
    }

    // ------------------------------------------------------------------
    // Whitespace stripping (xsl:strip-space / xsl:preserve-space)
    // ------------------------------------------------------------------

    private void ApplyWhitespaceStripping(IXdmNode source)
    {
        var rules = _stylesheet.GetAllSpaceHandlingRules();

        // Only strip whitespace in XDocument-backed nodes for now
        if (source is Providers.Xml.XDocumentNode xdocNode)
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
                return attr.Value == "yes";
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
    private static readonly HashSet<string> WhitespacePreserveElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "analyze-string", "attribute", "comment", "copy", "document", "element", "eval",
        "for-each", "for-each-group", "if", "key", "matching-substring", "message",
        "non-matching-substring", "otherwise", "param", "processing-instruction",
        "strip-space", "template", "text", "value-of", "variable", "when", "with-param"
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
    private void ProcessSequenceText(XText text, XElement parent)
    {
        if (GetExpandText(parent))
        {
            var tvtResult = EvaluateTvt(text.Value);
            if (tvtResult.Length > 0)
                AddTextNode(tvtResult);
        }
        else
        {
            if (!IsWhitespaceOnly(text.Value))
                AddTextNode(text.Value);
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
        var level = instruction.Attribute("level")?.Value ?? "single";
        var countPattern = instruction.Attribute("count")?.Value;
        var fromPattern = instruction.Attribute("from")?.Value;
        var formatAttr = instruction.Attribute("format")?.Value ?? "1";
        var valueAttr = instruction.Attribute("value")?.Value;
        var selectAttr = instruction.Attribute("select")?.Value;
        var startAtAttr = instruction.Attribute("start-at")?.Value;

        // Evaluate format as AVT (it is always an AVT per XSLT spec)
        var format = EvaluateAvt(formatAttr);

        // Evaluate start-at as AVT, then parse as space-separated integers (XSLT 3.0)
        long[]? startAtValues = null;
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
            targetNode = ExtractSingleNode(result);
            if (targetNode == null)
                return;
        }

        if (!string.IsNullOrEmpty(valueAttr))
        {
            var compiled = XPath31Expression.Compile(valueAttr);
            var result = compiled.Evaluate(_context);
            var numbers = XdmValueToLongArray(result);
            if (numbers.Length > 0)
            {
                // Apply start-at to each number: value - 1 + start-at
                for (int i = 0; i < numbers.Length; i++)
                {
                    var startAt = startAtValues != null && startAtValues.Length > 0
                        ? (i < startAtValues.Length ? startAtValues[i] : startAtValues[^1])
                        : 1;
                    numbers[i] = (int)(numbers[i] - 1 + startAt);
                }
                var formatted = FormatNumberSequence(numbers, format);
                AddTextNode(formatted);
            }
        }
        else
        {
            var countMatcher = string.IsNullOrEmpty(countPattern)
                ? CreateDefaultCountMatcher(targetNode)
                : new Patterns.PatternCompiler().Compile(countPattern);

            var fromMatcher = string.IsNullOrEmpty(fromPattern)
                ? null
                : new Patterns.PatternCompiler().Compile(fromPattern);

            int[]? numbers = level switch
            {
                "single" => ComputeNumberSingle(targetNode, countMatcher, fromMatcher, _context),
                "any" => ComputeNumberAny(targetNode, countMatcher, fromMatcher, _context),
                "multiple" => ComputeNumberMultiple(targetNode, countMatcher, fromMatcher, _context),
                _ => null
            };

            if (numbers != null && numbers.Length > 0)
            {
                // Apply start-at to each number
                if (startAtValues != null)
                {
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        var startAt = i < startAtValues.Length ? startAtValues[i] : startAtValues[^1];
                        numbers[i] = (int)(numbers[i] - 1 + startAt);
                    }
                }
                var formatted = FormatNumberSequence(numbers, format);
                AddTextNode(formatted);
            }
        }
    }

    /// <summary>
    /// Creates a default count matcher based on the current node's kind and name.
    /// </summary>
    private static Patterns.PatternPredicate CreateDefaultCountMatcher(IXdmNode node)
    {
        var compiler = new Patterns.PatternCompiler();
        return node.NodeKind switch
        {
            XdmNodeKind.Element => compiler.Compile(node.LocalName),
            XdmNodeKind.Attribute => compiler.Compile("@" + node.LocalName),
            _ => (n, ctx) => n.NodeKind == node.NodeKind
        };
    }

    /// <summary>
    /// Computes the number for <c>level="single"</c>.
    /// </summary>
    private static int[]? ComputeNumberSingle(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        // Find nearest ancestor-or-self matching count
        IXdmNode? target = null;
        if (countMatcher(currentNode, context))
        {
            target = currentNode;
        }
        else
        {
            foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (countMatcher(ancestor, context))
                    {
                        target = ancestor;
                        break;
                    }
                }
            }
        }

        if (target == null)
            return null;

        // If from is specified, target must have a from-matching ancestor
        if (fromMatcher != null)
        {
            bool hasFromAncestor = false;
            foreach (var item in target.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (fromMatcher(ancestor, context))
                    {
                        hasFromAncestor = true;
                        break;
                    }
                }
            }
            if (!hasFromAncestor)
                return null;
        }

        int count = 0;
        foreach (var item in target.Axis(XdmAxis.PrecedingSibling))
        {
            if (item.IsNode && item.NodeValue is IXdmNode sibling)
            {
                if (countMatcher(sibling, context))
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
            return null;

        int count = 0;
        bool foundCurrent = false;

        WalkDocumentTree(doc, node =>
        {
            if (node.IsSameNode(currentNode))
                foundCurrent = true;

            if (fromMatcher != null && fromMatcher(node, context))
                count = 0;

            if (countMatcher(node, context))
                count++;

            return !foundCurrent;
        });

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
        ancestors.Reverse(); // outermost first
        ancestors.Add(currentNode);

        foreach (var ancestor in ancestors)
        {
            if (fromMatcher != null && fromMatcher(ancestor, context))
                break;

            if (countMatcher(ancestor, context))
            {
                int count = 0;
                foreach (var item in ancestor.Axis(XdmAxis.PrecedingSibling))
                {
                    if (item.IsNode && item.NodeValue is IXdmNode sibling)
                    {
                        if (countMatcher(sibling, context))
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
    /// for each node. Returns <c>false</c> if the visitor requested stopping.
    /// </summary>
    private static bool WalkDocumentTree(IXdmNode node, Func<IXdmNode, bool> visitor)
    {
        if (!visitor(node))
            return false;

        foreach (var item in node.Axis(XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue is IXdmNode child)
            {
                if (!WalkDocumentTree(child, visitor))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Formats a sequence of integers according to an <c>xsl:number</c> format string.
    /// </summary>
    private string FormatNumberSequence(int[] numbers, string format)
    {
        if (numbers.Length == 0)
            return string.Empty;

        var (prefix, tokens, separators, suffix) = ParseXslNumberFormat(format);

        var sb = new System.Text.StringBuilder();
        sb.Append(prefix);

        for (int i = 0; i < numbers.Length; i++)
        {
            var token = tokens.Count > 0
                ? (i < tokens.Count ? tokens[i] : tokens[^1])
                : "1";
            sb.Append(FormatIntegerEngine.Format(_context, numbers[i], token, null));

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
    /// Parses an <c>xsl:number</c> format string into prefix, tokens, separators, and suffix.
    /// </summary>
    private static (string prefix, List<string> tokens, List<string> separators, string suffix) ParseXslNumberFormat(string format)
    {
        var tokens = new List<string>();
        var separators = new List<string>();

        int i = 0;
        while (i < format.Length && !char.IsLetterOrDigit(format[i]))
            i++;
        var prefix = format.Substring(0, i);

        while (i < format.Length)
        {
            int tokenStart = i;
            while (i < format.Length && char.IsLetterOrDigit(format[i]))
                i++;
            tokens.Add(format.Substring(tokenStart, i - tokenStart));

            int sepStart = i;
            while (i < format.Length && !char.IsLetterOrDigit(format[i]))
                i++;
            separators.Add(format.Substring(sepStart, i - sepStart));
        }

        string suffix = string.Empty;
        if (separators.Count > 0)
        {
            suffix = separators[^1];
            separators.RemoveAt(separators.Count - 1);
        }

        return (prefix, tokens, separators, suffix);
    }

    /// <summary>
    /// Parses a start-at attribute value string into an array of integers.
    /// Handles space-separated values and single values.
    /// </summary>
    private static long[] ParseStartAtValues(string value)
    {
        var parts = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new long[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!long.TryParse(parts[i], out result[i]))
                result[i] = 1;
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
    /// Converts an <see cref="XdmValue"/> to a <see cref="long"/> if it represents a number.
    /// </summary>
    private static long? XdmValueToLong(XdmValue value)
    {
        // If it's a singleton sequence, extract the first item
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return XdmValueToLong(item);
            return null;
        }

        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (long)Math.Round(value.DecimalValue),
            XdmValueKind.Double => (long)Math.Round(value.DoubleValue),
            XdmValueKind.Float => (long)Math.Round(value.DoubleValue),
            XdmValueKind.Node => long.TryParse(value.NodeValue?.StringValue ?? "", out var n) ? n : null,
            _ => long.TryParse(value.ToString(), out var n) ? n : null
        };
    }

    /// <summary>
    /// Converts an <see cref="XdmValue"/> to an array of <see cref="long"/> values.
    /// Handles sequences by extracting all numeric items.
    /// </summary>
    private static int[] XdmValueToLongArray(XdmValue value)
    {
        var result = new List<int>();
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                var n = XdmValueToLong(item);
                if (n.HasValue)
                    result.Add((int)n.Value);
            }
        }
        else
        {
            var n = XdmValueToLong(value);
            if (n.HasValue)
                result.Add((int)n.Value);
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
