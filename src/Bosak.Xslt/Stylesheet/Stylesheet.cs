// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : In-memory representation of a loaded XSLT stylesheet with template rules and imports.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added named template dictionary for call-template dispatch                               |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added import/include resolution, import precedence, flattened rule collection            |
//                      | Charles Korthout | 0.4   | 26-05-2026     | Added global variable and parameter collection with import/include precedence            |
//                      | Charles Korthout | 0.5   | 26-05-2026     | Added xsl:strip-space and xsl:preserve-space parsing with SpaceHandlingRule              |
//                      | Charles Korthout | 0.6   | 27-05-2026     | Added xsl:function parsing and GetAllFunctionDefinitions / GetAllNamespaces              |
//                      | Charles Korthout | 0.7   | 27-05-2026     | Added required version attribute validation (XTSE0010)                                   |
//                      | Charles Korthout | 0.8   | 27-05-2026     | Exposed Version property for runtime backwards-compatibility checks                    |
//                      | Charles Korthout | 0.9   | 31-05-2026     | Decimal-format merging from imports/includes; descendant namespace collection            |
//                      | Charles Korthout | 1.0   | 31-05-2026     | Added exclude-result-prefixes parsing and GetAllExcludedResultPrefixes                   |
//                      | Charles Korthout | 1.1   | 31-05-2026     | Added literal result element stylesheet support (WrapLiteralResultElement)               |
//                      | Charles Korthout | 1.2   | 07-06-2026     | Fix import+include same file: separate _includedUris, copy _resolvedUris to children     |
//                      | Charles Korthout | 1.3   | 10-06-2026     | Added ValidateInstructionTree for xsl:copy-of static validation (XTSE0090/0260)        |
//                      | Charles Korthout | 1.4   | 10-06-2026     | ValidateInstructionTree checks xsl:copy attributes and copy-namespaces values (XTSE0020) |
//                      | Charles Korthout | 1.5   | 11-06-2026     | Made GetXPathDefaultNamespace internal for xsl:key index building                       |
//                      | Charles Korthout | 1.6   | 11-06-2026     | Added DeclaredModes parsing                                                             |
//                      | Charles Korthout | 1.7   | 11-06-2026     | XTSE0130 for no-namespace top-level elements; fixes accumulator-078                     |
//                      | Charles Korthout | 1.8   | 13-06-2026     | Empty-URI EQName support for variable/param names (Q{}local)                             |
//                      | Charles Korthout | 1.9   | 13-06-2026     | Parse xsl:global-context-item; XTSE3089 for use=absent with as                         |
//                      | Charles Korthout | 2.0   | 13-06-2026     | XTSE0710 validation for xsl:attribute-set/@use-attribute-sets                          |
//                      | Charles Korthout | 2.1   | 13-06-2026     | xsl:function static validation: attributes, duplicate names, required params           |
//                      | Charles Korthout | 2.2   | 13-06-2026     | Added xsl:merge static validation (required children, merge-key placement)             |
//                      | Charles Korthout | 2.3   | 13-06-2026     | XTSE1650 import-schema; merge-source validation/type and sort-before-merge checks      |
//                      | Charles Korthout | 2.4   | 24-06-2026     | expand-text on xsl:message; package-version XTSE0090; XTSE1660 for strict/lax/type    |
//                      | Charles Korthout | 2.5   | 24-06-2026     | Restrict XTSE1660 to strict validation; allow lax on basic processors                  |
//                      | Charles Korthout | 2.6   | 24-06-2026     | Reject extension-element-prefixes bound to reserved namespaces (XTSE0800)              |
//                      | Charles Korthout | 2.7   | 24-06-2026     | use-when walks ancestor namespace declarations; propagates XTDE1400/1410 errors        |
//                      | Charles Korthout | 2.8   | 25-06-2026     | XTSE0080 validation for xsl:template/@name; added xs/fn namespace constants             |
//                      | Charles Korthout | 2.9   | 26-06-2026     | Whitelist use-attribute-sets on literal result elements (XTSE0805)                    |
//                      | Charles Korthout | 2.9   | 25-06-2026     | Static function-available hides dynamic XSLT functions; skip descendants of false use-when |
//                      | Charles Korthout | 2.10  | 26-06-2026     | Guard XTSE1660 check so it does not fire on literal result elements                     |
//                      | Charles Korthout | 2.11  | 26-06-2026     | Static variable validation and default-value handling for static cluster              |
//                      | Charles Korthout | 2.12  | 26-06-2026     | XTSE0090 for non-global static vars/params; XTSE0090 visibility; XTSE3450 var/param   |
//                      | Charles Korthout | 2.13  | 26-06-2026     | Document-order use-when evaluation; precedence-aware static variable conflict detection |
//                      | Charles Korthout | 2.14  | 26-06-2026     | Added xsl:namespace-alias parsing and effective alias mapping                           |
//                      | Charles Korthout | 2.16  | 07-07-2026     | XTSE0680 validation uses the root stylesheet's named-template set so imports see overrides |
//                      | Charles Korthout | 2.15  | 29-06-2026     | Static validation for xsl:variable/param/with-param attributes; forwards-compatible mode |
//                      | Charles Korthout | 2.16  | 26-06-2026     | Shadow attribute support for version, href, use-when, and xpath-default-namespace        |
//                      | Charles Korthout | 2.17  | 26-06-2026     | Static context hides XSLT dynamic functions such as fn:current-output-uri               |
//                      | Charles Korthout | 2.18  | 03-07-2026     | Validate expand-text values (XTSE0020); allow expand-text on xsl:function               |
//                      | Charles Korthout | 2.19  | 03-07-2026     | Import/include precedence, apply-imports context, and duplicate includes; clears import |
//                      | Charles Korthout | 2.20  | 05-07-2026     | Version cluster: per-element version, known-element set, forwards-compat skip         |
//                      | Charles Korthout | 2.21  | 26-06-2026     | Added assert to known XSLT element set                                                  |
//                      | Charles Korthout | 2.22  | 06-07-2026     | Merge multiple xsl:output declarations instead of using only the first                 |
//                      | Charles Korthout | 2.23  | 08-07-2026     | Forward-compatible handling for unknown elements, attributes, and use-when             |
//                      | Charles Korthout | 2.24  | 26-06-2026     | TransitiveImports now includes modules included by imported modules (apply-imports)   |
//                      | Charles Korthout | 2.25  | 11-07-2026     | Parse xsl:character-map declarations and resolve effective character maps.             |
//                      | Charles Korthout | 2.26  | 11-07-2026     | Character-map resolution now uses first-wins across the effective map list.            |
//                      | Charles Korthout | 2.27  | 11-07-2026     | Named-output import-precedence merge now puts the importing stylesheet last            |
//                      | Charles Korthout | 2.28  | 11-07-2026     | Load xsl:output parameter-document defaults and merge them with explicit attributes.    |
//                      | Charles Korthout | 2.29  | 12-07-2026     | Last map in use-character-maps list wins for duplicate characters.                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Runtime;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a loaded XSLT stylesheet, including all imported and included modules.
/// </summary>
public sealed class Stylesheet
{
    private XDocument _document;
    private readonly string? _baseUri;
    private readonly IXsltUriResolver _resolver;
    private readonly HashSet<string> _resolvedUris;

    private readonly List<TemplateRule> _templateRules = new();
    private readonly Dictionary<string, TemplateRule> _namedTemplates = new();
    private readonly List<Stylesheet> _imports = new();
    private readonly List<Stylesheet> _includes = new();
    private readonly List<KeyDefinition> _keyDefinitions = new();
    private readonly List<AccumulatorDefinition> _accumulators = new();
    private readonly List<XElement> _globalVariables = new();
    private readonly List<XElement> _globalParameters = new();
    private readonly List<SpaceHandlingRule> _spaceRules = new();
    private readonly Dictionary<string, ModeDefinition> _modeDefinitions = new();
    private readonly List<XsltFunctionDefinition> _functionDefinitions = new();
    private readonly List<DecimalFormatDefinition> _decimalFormats = new();
    private readonly List<AttributeSetDefinition> _attributeSets = new();
    private readonly HashSet<string> _excludedResultPrefixes = new();
    private readonly List<NamespaceAliasDefinition> _namespaceAliases = new();
    private OutputProperties? _outputProperties;
    private readonly Dictionary<string, OutputProperties> _namedOutputProperties = new();
    private readonly Dictionary<string, CharacterMapDefinition> _characterMaps = new();
    private readonly bool _isRootStylesheet;
    private readonly Stylesheet _rootStylesheet;
    private readonly StaticContext _staticContext = new();
    private readonly IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> _externalStaticParameters;

    /// <summary>
    /// Empty dictionary used when no external static parameters are supplied.
    /// </summary>
    private static readonly IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> EmptyExternalStaticParameters =
        new Dictionary<(string LocalName, string NamespaceUri), XdmValue>();

    /// <summary>
    /// Holds the static variables and parameters that are in scope for evaluating
    /// <c>use-when</c> expressions in this stylesheet module.
    /// </summary>
    private sealed class StaticContext
    {
        /// <summary>Static variable bindings keyed by (local-name, namespace-uri).</summary>
        public Dictionary<(string LocalName, string NamespaceUri), (XdmValue Value, bool IsParam, int Precedence)> Variables { get; } = new();

        /// <summary>
        /// Same-precedence declarations with different values. A later higher-precedence
        /// declaration resolves the conflict; otherwise it is reported as XTSE3450.
        /// </summary>
        public Dictionary<(string LocalName, string NamespaceUri), (XdmValue First, XdmValue Second)> PendingConflicts { get; } = new();
    }

    /// <summary>
    /// Builds the evaluation context used for static <c>use-when</c> expressions.
    /// </summary>
    private EvaluationContext CreateUseWhenContext(XElement elem, string? explicitBaseUri = null)
    {
        var ctx = new EvaluationContext();

        // The static base URI for use-when is the base URI of the element's
        // containing stylesheet module, taking xml:base into account.
        ctx.BaseUri = explicitBaseUri ?? GetEffectiveBaseUri(elem);
        ctx.IsStaticEvaluation = true;
        Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(ctx);

        // Add in-scope namespace declarations so prefixes in use-when resolve correctly.
        var currentNs = elem;
        while (currentNs != null)
        {
            foreach (var attr in currentNs.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName;
                if (prefix == "xmlns") prefix = "";
                // Inner declarations take precedence; only add if not already present.
                if (!ctx.TryResolveNamespace(prefix, out _))
                    ctx.WithNamespace(prefix, attr.Value);
            }
            currentNs = currentNs.Parent;
        }

        // Apply the effective xpath-default-namespace for element/type names.
        var defaultNs = GetXPathDefaultNamespace(elem);
        if (!string.IsNullOrEmpty(defaultNs))
            ctx.DefaultElementNamespace = defaultNs;

        // Make static variables/parameters visible to the expression.
        foreach (var (key, entry) in _staticContext.Variables)
            ctx.WithVariable(key.LocalName, entry.Value, key.NamespaceUri);

        return ctx;
    }

    /// <summary>
    /// Evaluates the <c>use-when</c> attribute on the given element.
    /// Returns <c>true</c> if the attribute is absent or evaluates to true.
    /// Any error while evaluating the expression is propagated as a static error.
    /// A shadow attribute <c>_use-when</c> is expanded to <c>use-when</c> first.
    /// </summary>
    private bool UseWhen(XElement elem, string? explicitBaseUri = null)
    {
        // In forward-compatible mode, an unknown XSLT element whose use-when expression
        // references a future function is treated as excluded; elements without use-when
        // are kept so that their xsl:fallback children can be processed.
        if (elem.Name.NamespaceName == XslNamespace &&
            !KnownXsltElementNames.Contains(elem.Name.LocalName) &&
            IsForwardsCompatibleElement(elem) &&
            elem.Attribute("use-when") != null)
        {
            return false;
        }

        // Expand a shadow use-when attribute before evaluation.
        ExpandShadowAttribute(elem, "use-when");

        string? useWhen = null;
        bool isXsltElement = elem.Name.NamespaceName == XslNamespace;

        if (isXsltElement)
        {
            // On XSLT elements the controlling use-when is the no-namespace attribute.
            var noNsAttr = elem.Attribute("use-when");
            if (noNsAttr != null)
            {
                useWhen = noNsAttr.Value;
            }
            else
            {
                // A use-when attribute in the XSLT namespace is never permitted on
                // XSLT elements.
                if (elem.Attribute(XName.Get("use-when", XslNamespace)) != null)
                    throw new InvalidOperationException("XTSE0090: use-when must not be in the XSLT namespace on XSLT elements.");
                return true;
            }
        }
        else
        {
            // On literal result elements (and data elements) only the XSLT-namespace
            // form is recognized; the no-namespace form is a normal output attribute.
            var xslAttr = elem.Attribute(XName.Get("use-when", XslNamespace));
            if (xslAttr != null)
                useWhen = xslAttr.Value;
            else
                return true;
        }

        if (string.IsNullOrEmpty(useWhen))
            return true;

        var compiled = XPath31Expression.Compile(useWhen);
        var ctx = CreateUseWhenContext(elem, explicitBaseUri);
        var result = compiled.Evaluate(ctx);
        bool include = result.EffectiveBooleanValue();

        if (isXsltElement)
        {
            // If the no-namespace use-when excludes the element, the (erroneous)
            // XSLT-namespace use-when attribute is ignored. Otherwise it is an error.
            if (include && elem.Attribute(XName.Get("use-when", XslNamespace)) != null)
                throw new InvalidOperationException("XTSE0090: use-when must not be in the XSLT namespace on XSLT elements.");
        }

        return include;
    }

    /// <summary>
    /// Expands a single shadow attribute <c>_{baseName}</c> to <c>{baseName}</c> by
    /// evaluating its value as an attribute value template in the current static context.
    /// </summary>
    private void ExpandShadowAttribute(XElement elem, string baseName)
    {
        var shadow = elem.Attribute("_" + baseName);
        if (shadow == null)
            return;

        var expanded = EvaluateStaticAvt(shadow.Value, elem);
        shadow.Remove();
        elem.SetAttributeValue(baseName, expanded);
    }

    /// <summary>
    /// Expands all shadow attributes on the given XSLT element and its XSLT descendants.
    /// Shadow attributes on literal result elements and data elements are ignored.
    /// </summary>
    private void ExpandAllShadowAttributes(XElement root)
    {
        foreach (var elem in root.DescendantsAndSelf())
        {
            // Shadow attributes only apply to XSLT instructions.
            if (elem.Name.NamespaceName != XslNamespace)
                continue;

            var shadows = elem.Attributes()
                .Where(a => string.IsNullOrEmpty(a.Name.NamespaceName) && a.Name.LocalName.StartsWith("_"))
                .ToList();

            foreach (var attr in shadows)
            {
                var baseName = attr.Name.LocalName.Substring(1);
                if (string.IsNullOrEmpty(baseName))
                    continue;

                var expanded = EvaluateStaticAvt(attr.Value, elem);
                attr.Remove();
                elem.SetAttributeValue(baseName, expanded);
            }
        }
    }

    /// <summary>
    /// Evaluates a static attribute value template using the current static context.
    /// </summary>
    private string EvaluateStaticAvt(string avt, XElement element)
    {
        if (string.IsNullOrEmpty(avt) || !avt.Contains('{'))
            return avt;

        var ctx = CreateUseWhenContext(element);
        var nsMap = ExtractInScopeNamespaces(element);
        var sb = new StringBuilder();

        int i = 0;
        while (i < avt.Length)
        {
            if (i + 1 < avt.Length && avt[i] == '{' && avt[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
                continue;
            }
            if (i + 1 < avt.Length && avt[i] == '}' && avt[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
                continue;
            }
            if (avt[i] == '{')
            {
                int end = FindMatchingAvtBrace(avt, i + 1);
                if (end < 0)
                {
                    sb.Append(avt[i]);
                    i++;
                    continue;
                }

                var expr = avt.Substring(i + 1, end - i - 1);
                if (!string.IsNullOrEmpty(expr))
                {
                    var compiled = XPath31Expression.Compile(expr, new CompileOptions { Namespaces = nsMap });
                    var result = compiled.Evaluate(ctx);
                    sb.Append(AtomizedAvtString(result));
                }
                i = end + 1;
                continue;
            }

            sb.Append(avt[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Finds the matching closing brace for an AVT expression, skipping string literals.
    /// </summary>
    private static int FindMatchingAvtBrace(string value, int start)
    {
        char inString = '\0';
        int braceDepth = 1;
        for (int i = start; i < value.Length; i++)
        {
            char c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    // Handle doubled quote characters inside string literals.
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i++;
                        continue;
                    }
                    inString = '\0';
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inString = c;
                continue;
            }

            if (c == '{')
            {
                braceDepth++;
                continue;
            }

            if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                    return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Atomizes a value for AVT concatenation, returning the string value of each item
    /// without separators.
    /// </summary>
    private static string AtomizedAvtString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsSequence && value.SequenceValue != null)
        {
            var sb = new StringBuilder();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                sb.Append(item.ToString());
            return sb.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Collects the in-scope namespace declarations for an element and its ancestors.
    /// </summary>
    private static Dictionary<string, string> ExtractInScopeNamespaces(XElement element)
    {
        var dict = new Dictionary<string, string>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName;
                if (prefix == "xmlns")
                    prefix = "";
                if (!string.IsNullOrEmpty(prefix) && !dict.ContainsKey(prefix))
                    dict[prefix] = attr.Value;
            }
            current = current.Parent;
        }
        return dict;
    }

    public Stylesheet(XDocument document, string? baseUri, IXsltUriResolver resolver, int importPrecedence = 0, HashSet<string>? resolvedUris = null, object? inheritedStaticContext = null, IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue>? externalStaticParameters = null, Stylesheet? rootStylesheet = null)
    {
        _document = document;
        _baseUri = baseUri;
        _resolver = resolver;
        ImportPrecedence = importPrecedence;
        ApplyImportsContextModule = this;
        _resolvedUris = resolvedUris ?? new HashSet<string>();
        _isRootStylesheet = _resolvedUris.Count == 0;
        _externalStaticParameters = externalStaticParameters ?? EmptyExternalStaticParameters;
        _rootStylesheet = rootStylesheet ?? this;

        // Add this stylesheet's own URI to the resolved set for circular-reference detection
        if (!string.IsNullOrEmpty(baseUri))
            _resolvedUris.Add(baseUri);

        // Initialise the static context from the including module (for xsl:include).
        if (inheritedStaticContext is StaticContext inherited)
        {
            foreach (var kv in inherited.Variables)
                _staticContext.Variables[kv.Key] = (kv.Value.Value, kv.Value.IsParam, kv.Value.Precedence);
        }

        // Handle literal result element stylesheets before loading
        var root = _document.Root;
        if (root != null && root.Name.NamespaceName != XslNamespace)
        {
            var xslVersion = root.Attributes()
                .FirstOrDefault(a => a.Name.NamespaceName == XslNamespace && a.Name.LocalName == "version");
            if (xslVersion != null)
            {
                _document = WrapLiteralResultElement(root, xslVersion.Value);
            }
        }

        Load();
    }

    /// <summary>The root element of the stylesheet (xsl:stylesheet or xsl:transform).</summary>
    public XElement Root => _document.Root!;

    /// <summary>The base URI of this stylesheet, used for resolving relative xml:base values.</summary>
    public string? BaseUri => _baseUri;

    /// <summary>All template rules defined in this stylesheet, ordered by priority.</summary>
    public IReadOnlyList<TemplateRule> TemplateRules => _templateRules;

    /// <summary>Named templates indexed by name.</summary>
    public IReadOnlyDictionary<string, TemplateRule> NamedTemplates => _namedTemplates;

    /// <summary>Accumulator declarations defined in this stylesheet.</summary>
    public IReadOnlyList<AccumulatorDefinition> Accumulators => _accumulators;

    /// <summary>Stylesheets imported via xsl:import (lower precedence).</summary>
    public IReadOnlyList<Stylesheet> Imports => _imports;

    /// <summary>Stylesheets included via xsl:include (same precedence).</summary>
    public IReadOnlyList<Stylesheet> Includes => _includes;

    /// <summary>The import precedence of this stylesheet (0 = main, higher = deeper import).</summary>
    public int ImportPrecedence { get; private set; }

    /// <summary>
    /// The stylesheet module whose import tree is used by <c>xsl:apply-imports</c>
    /// inside templates in this module. For the root and imported modules this is
    /// the module itself; for included modules it is the module that included it
    /// (propagated through nested includes).
    /// </summary>
    public Stylesheet ApplyImportsContextModule { get; private set; } = null!;

    /// <summary>The default mode for xsl:apply-templates within this stylesheet (empty string = unnamed mode).</summary>
    public string DefaultMode { get; private set; } = "";

    /// <summary>
    /// Whether every mode used in the stylesheet must be explicitly declared.
    /// A value of <c>false</c> (from xsl:package/@declared-modes="no") means implicit
    /// mode declarations are allowed.
    /// </summary>
    public bool DeclaredModes { get; private set; } = true;

    /// <summary>The value of the xsl:global-context-item/@use attribute, or null if absent.</summary>
    public string? GlobalContextItemUse { get; private set; }

    /// <summary>The value of the xsl:global-context-item/@as attribute, or null if absent.</summary>
    public string? GlobalContextItemAs { get; private set; }

    /// <summary>
    /// Recursively collects all template rules from this stylesheet, its includes, and its imports.
    /// Order: local first, then includes (same precedence), then imports (lower precedence).
    /// </summary>
    public IEnumerable<TemplateRule> GetAllTemplateRules()
    {
        // Group local template rules by their source element so we can emit them
        // in true document order while interleaving imported/included modules.
        var rulesByElement = new Dictionary<XElement, List<TemplateRule>>();
        foreach (var rule in _templateRules)
        {
            if (!rulesByElement.TryGetValue(rule.Element, out var list))
            {
                list = new List<TemplateRule>();
                rulesByElement[rule.Element] = list;
            }
            list.Add(rule);
        }

        foreach (var element in Root.Elements())
        {
            var ns = element.Name.NamespaceName;
            var localName = element.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } imported })
                {
                    foreach (var rule in imported.GetAllTemplateRules())
                        yield return rule;
                }
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } included })
                {
                    foreach (var rule in included.GetAllTemplateRules())
                        yield return rule;
                }
            }
            else if (ns == XslNamespace && localName == "template" && rulesByElement.TryGetValue(element, out var rules))
            {
                foreach (var rule in rules)
                    yield return rule;
            }
        }
    }

    /// <summary>
    /// Recursively collects all accumulator declarations from this stylesheet, its includes, and its imports.
    /// </summary>
    public IEnumerable<AccumulatorDefinition> GetAllAccumulators()
    {
        foreach (var acc in _accumulators)
            yield return acc;

        foreach (var included in _includes)
        {
            foreach (var acc in included.GetAllAccumulators())
                yield return acc;
        }

        foreach (var imported in _imports)
        {
            foreach (var acc in imported.GetAllAccumulators())
                yield return acc;
        }
    }

    /// <summary>
    /// Recursively collects all named templates from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, TemplateRule> GetAllNamedTemplates()
    {
        var result = new Dictionary<string, TemplateRule>();

        // Imported first (lowest precedence, can be overridden)
        foreach (var imported in _imports)
        {
            foreach (var (name, rule) in imported.GetAllNamedTemplates())
                result[name] = rule;
        }

        // Included next (same precedence)
        foreach (var included in _includes)
        {
            foreach (var (name, rule) in included.GetAllNamedTemplates())
                result[name] = rule;
        }

        // Local last (highest precedence)
        foreach (var (name, rule) in _namedTemplates)
            result[name] = rule;

        return result;
    }

    private void Load()
    {
        var root = _document.Root;
        if (root == null)
            throw new InvalidOperationException("Stylesheet document has no root element.");

        var rootName = root.Name;

        if (rootName.NamespaceName != XslNamespace)
            throw new InvalidOperationException($"Expected xsl:stylesheet or xsl:transform, got {rootName}.");

        if (rootName.LocalName != "stylesheet" && rootName.LocalName != "transform")
            throw new InvalidOperationException($"Expected xsl:stylesheet or xsl:transform, got {rootName}.");

        // Expand a shadow version attribute before the effective version is determined.
        ExpandShadowAttribute(root, "version");

        // Required version attribute (XTSE0010)
        var versionAttr = root.Attribute("version");
        if (versionAttr == null || string.IsNullOrWhiteSpace(versionAttr.Value))
            throw new InvalidOperationException("XTSE0010: The version attribute is required on xsl:stylesheet or xsl:transform.");

        var versionValue = versionAttr.Value.Trim();
        if (!decimal.TryParse(versionValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
            throw new InvalidOperationException("XTSE0110: The version attribute must be a valid decimal number.");

        Version = versionValue;

        // Parse xsl:declared-modes on stylesheet/package root
        var declaredModesAttr = root.Attribute("declared-modes")?.Value?.Trim()?.ToLowerInvariant();
        DeclaredModes = declaredModesAttr != "no";

        // Parse xsl:default-mode on stylesheet root
        var defaultModeAttr = root.Attribute("default-mode")?.Value?.Trim() ?? "";
        DefaultMode = defaultModeAttr;
        if (defaultModeAttr == "#unnamed" || defaultModeAttr == "#default")
        {
            DefaultMode = ""; // the unnamed mode
        }
        else if (!string.IsNullOrEmpty(defaultModeAttr) && defaultModeAttr != "#current" && defaultModeAttr != "#all")
        {
            int colon = defaultModeAttr.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = defaultModeAttr.Substring(0, colon);
                var local = defaultModeAttr.Substring(colon + 1);
                var current = root;
                while (current != null)
                {
                    foreach (var attr in current.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                        {
                            DefaultMode = $"{{{attr.Value}}}{local}";
                            break;
                        }
                    }
                    if (DefaultMode != defaultModeAttr)
                        break;
                    current = current.Parent;
                }
            }
        }

        // Parse exclude-result-prefixes
        var excludePrefixesAttr = root.Attribute("exclude-result-prefixes")?.Value;
        if (!string.IsNullOrWhiteSpace(excludePrefixesAttr))
        {
            foreach (var token in excludePrefixesAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _excludedResultPrefixes.Add(token.Trim());
            }
        }

        // Validate extension-element-prefixes: reserved namespaces are not permitted.
        var extensionPrefixesAttr = root.Attribute("extension-element-prefixes")?.Value;
        if (!string.IsNullOrWhiteSpace(extensionPrefixesAttr))
        {
            foreach (var token in extensionPrefixesAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var prefix = token.Trim();
                string nsUri;
                if (prefix == "#default")
                    nsUri = root.GetDefaultNamespace().NamespaceName;
                else
                    nsUri = root.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;

                if (nsUri == XslNamespace ||
                    nsUri == System.Xml.Linq.XNamespace.Xml.NamespaceName ||
                    nsUri == "http://www.w3.org/2001/XMLSchema" ||
                    nsUri == "http://www.w3.org/2001/XMLSchema-instance")
                {
                    throw new InvalidOperationException("XTSE0800");
                }
            }
        }

        /// <summary>
        /// Recursively strips elements whose <c>use-when</c> attribute evaluates to <c>false()</c>.
        /// This is applied to the entire stylesheet tree, including nested elements inside
        /// template bodies, so that <c>use-when</c> on instructions and LREs is respected.
        /// Descendants of an excluded element are not evaluated.
        /// </summary>
        void StripUseWhenElements(XElement parent)
        {
            var children = parent.Elements().ToList();
            foreach (var child in children)
            {
                if (UseWhen(child))
                {
                    StripUseWhenElements(child);
                }
                else
                {
                    child.Remove();
                }
            }
        }

        // Build the static context (static variables/parameters, imports and includes)
        // before evaluating any use-when expressions.
        BuildStaticContext();

        // Apply use-when stripping to the entire tree (nested elements inside templates,
        // literal result elements, etc.) after imports/includes are resolved.
        StripUseWhenElements(root);

        // Expand any remaining shadow attributes (e.g. _xpath-default-namespace on
        // xsl:template) now that the static context is fully built.
        ExpandAllShadowAttributes(root);

        // Parse top-level xsl:param declarations
        foreach (var param in root.Elements(XName.Get("param", XslNamespace)))
        {
            if (!UseWhen(param)) continue;
            _globalParameters.Add(param);
        }

        // Parse top-level xsl:variable declarations
        foreach (var variable in root.Elements(XName.Get("variable", XslNamespace)))
        {
            if (!UseWhen(variable)) continue;
            _globalVariables.Add(variable);
        }

        // Parse xsl:key declarations
        foreach (var key in root.Elements(XName.Get("key", XslNamespace)))
        {
            if (!UseWhen(key)) continue;
            var def = KeyDefinition.FromElement(key, this);
            _keyDefinitions.Add(def);
        }

        // Parse xsl:accumulator declarations
        foreach (var acc in root.Elements(XName.Get("accumulator", XslNamespace)))
        {
            if (!UseWhen(acc)) continue;
            var def = AccumulatorDefinition.FromElement(acc, this);
            if (def != null)
                _accumulators.Add(def);
        }

        // Parse xsl:strip-space and xsl:preserve-space declarations.
        // Rules are collected in document order so that same-precedence conflicts
        // are resolved by last-match-wins.
        foreach (var decl in root.Elements()
            .Where(e => e.Name.NamespaceName == XslNamespace
                     && (e.Name.LocalName == "strip-space" || e.Name.LocalName == "preserve-space")))
        {
            if (!UseWhen(decl)) continue;
            bool isStrip = decl.Name.LocalName == "strip-space";
            var elements = decl.Attribute("elements")?.Value;
            var defaultNs = GetXPathDefaultNamespace(decl);
            if (!string.IsNullOrEmpty(elements))
            {
                foreach (var nameTest in elements.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var rule = SpaceHandlingRule.FromNameTest(nameTest, decl, this, defaultNs, isStrip, ImportPrecedence);
                    _spaceRules.Add(rule);
                }
            }
        }

        // Parse xsl:mode declarations. Collect all local declarations first so that
        // duplicate or conflicting declarations at the same import precedence can be
        // detected before the dictionary overwrites earlier entries.
        var localModes = new List<ModeDefinition>();
        foreach (var mode in root.Elements(XName.Get("mode", XslNamespace)))
        {
            if (!UseWhen(mode)) continue;
            var def = ModeDefinition.FromElement(mode);
            if (def != null)
                localModes.Add(def);
        }

        // Check for duplicate/conflicting local declarations in this stylesheet module.
        // For non-root modules this is deferred to the root-level validation, because
        // conflicts in imported modules may be overridden by a higher-precedence declaration.
        if (_isRootStylesheet)
        {
            var seenLocalModes = new Dictionary<string, ModeDefinition>();
            foreach (var def in localModes)
            {
                if (seenLocalModes.TryGetValue(def.Name, out var existing))
                {
                    if (!AreModesEquivalent(existing, def))
                        throw new InvalidOperationException($"XTSE0545: Conflicting xsl:mode declarations for mode '{def.Name}' at the same import precedence.");
                }
                else
                {
                    seenLocalModes[def.Name] = def;
                }
            }
        }

        foreach (var def in localModes)
        {
            _modeDefinitions[def.Name] = def;
        }

        // Validate mode definitions across the complete stylesheet tree. Conflicting
        // declarations at the winning import precedence are a static error; declarations
        // overridden by a higher-precedence declaration are ignored.
        if (_isRootStylesheet)
            ValidateModeDefinitions();

        // Parse xsl:global-context-item declaration (XSLT 3.0)
        foreach (var gci in root.Elements(XName.Get("global-context-item", XslNamespace)))
        {
            if (!UseWhen(gci)) continue;
            var use = gci.Attribute("use")?.Value?.Trim();
            var asType = gci.Attribute("as")?.Value?.Trim();
            // XTSE3089: use="absent" and as must not both be present.
            if (use == "absent" && !string.IsNullOrEmpty(asType))
                throw new InvalidOperationException("XTSE3089: xsl:global-context-item must not have an as attribute when use is absent.");
            GlobalContextItemUse = use;
            GlobalContextItemAs = asType;
        }

        // Parse xsl:output properties. Multiple xsl:output declarations are merged,
        // with later declarations overriding earlier ones for the same property.
        // Named outputs are stored separately by expanded QName and are used by
        // xsl:result-document via its @format attribute.
        var outputElems = root.Elements(XName.Get("output", XslNamespace)).Where(e => UseWhen(e)).ToList();
        foreach (var oe in outputElems)
        {
            var props = OutputProperties.FromElement(oe);

            // A parameter document supplies default values; explicit xsl:output attributes
            // override values from the parameter document.
            var paramDocAttr = oe.Attribute("parameter-document")?.Value;
            if (!string.IsNullOrEmpty(paramDocAttr))
            {
                var paramDoc = _resolver.Resolve(paramDocAttr, _baseUri);
                var paramProps = OutputProperties.FromSerializationParameters(paramDoc);
                var merged = paramProps.Clone();
                OutputProperties.Merge(merged, props);
                props = merged;
            }

            var nameAttr = oe.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                var expandedName = ExpandQName(oe, nameAttr);
                if (_namedOutputProperties.TryGetValue(expandedName, out var existing))
                    OutputProperties.Merge(existing, props);
                else
                    _namedOutputProperties[expandedName] = props.Clone();
            }
            else
            {
                if (_outputProperties == null)
                    _outputProperties = new OutputProperties();
                OutputProperties.Merge(_outputProperties, props);
            }
        }

        // Parse xsl:character-map declarations.
        foreach (var cm in root.Elements(XName.Get("character-map", XslNamespace)))
        {
            if (!UseWhen(cm)) continue;
            var def = CharacterMapDefinition.FromElement(cm, this);
            _characterMaps[def.ExpandedName] = def;
        }

        // Parse xsl:namespace-alias declarations
        foreach (var alias in root.Elements(XName.Get("namespace-alias", XslNamespace)))
        {
            if (!UseWhen(alias)) continue;
            _namespaceAliases.Add(NamespaceAliasDefinition.FromElement(alias, this));
        }

        // XTSE0130: top-level elements in no namespace are not permitted unless they are
        // XSLT instructions. Data elements in other namespaces are allowed.
        foreach (var topLevel in root.Elements())
        {
            if (topLevel.Name.NamespaceName != XslNamespace && string.IsNullOrEmpty(topLevel.Name.NamespaceName))
                throw new InvalidOperationException($"XTSE0130: Top-level element '{topLevel.Name.LocalName}' is not permitted in no namespace.");
        }

        // Collect template rules from this stylesheet
        foreach (var template in root.Elements(XName.Get("template", XslNamespace)))
        {
            if (!UseWhen(template)) continue;
            var rules = TemplateRule.FromElement(template, this);
            if (rules.Count > 0)
            {
                foreach (var rule in rules)
                {
                    if (!string.IsNullOrEmpty(rule.Match))
                        _templateRules.Add(rule);
                }
                // Named templates: register only the first rule (all share the same body)
                var firstNamed = rules.FirstOrDefault(r => !string.IsNullOrEmpty(r.Name));
                if (firstNamed != null && firstNamed.Name != null)
                {
                    // XTSE0080: template names must not use a reserved namespace.
                    ValidateNamedQName(firstNamed.Element, firstNamed.Name, "xsl:template");
                    _namedTemplates[firstNamed.Name] = firstNamed;
                }
            }
        }

        // Parse xsl:function declarations
        foreach (var func in root.Elements(XName.Get("function", XslNamespace)))
        {
            if (!UseWhen(func)) continue;
            var def = XsltFunctionDefinition.FromElement(func, this);
            if (def != null)
                _functionDefinitions.Add(def);
        }

        // XTSE0770: duplicate xsl:function declarations with the same expanded QName and arity
        {
            var seenFunctions = new HashSet<(string ns, string local, int arity)>();
            foreach (var def in _functionDefinitions)
            {
                var key = (def.NamespaceUri, def.LocalName, def.Arity);
                if (!seenFunctions.Add(key))
                    throw new InvalidOperationException($"XTSE0770: Duplicate function declaration '{{{def.NamespaceUri}}}{def.LocalName}#{def.Arity}'.");
            }
        }

        // Parse xsl:decimal-format declarations
        foreach (var df in root.Elements(XName.Get("decimal-format", XslNamespace)))
        {
            if (!UseWhen(df)) continue;
            var def = DecimalFormatDefinition.FromElement(df, this);
            if (def != null)
                _decimalFormats.Add(def);
        }

        // Parse xsl:attribute-set declarations
        foreach (var attrSet in root.Elements(XName.Get("attribute-set", XslNamespace)))
        {
            if (!UseWhen(attrSet)) continue;
            var def = AttributeSetDefinition.FromElement(attrSet, this);
            if (def != null)
                _attributeSets.Add(def);
        }

        // Static validation: xsl:attribute-set/@use-attribute-sets must reference
        // existing attribute sets (XTSE0710).
        var allAttrSets = GetAllAttributeSets();
        foreach (var attrSet in _attributeSets)
        {
            if (string.IsNullOrWhiteSpace(attrSet.UseAttributeSets))
                continue;
            foreach (var name in attrSet.UseAttributeSets.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = name.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;
                string localName;
                string nsUri;
                int colon = trimmed.IndexOf(':');
                if (colon >= 0)
                {
                    var prefix = trimmed.Substring(0, colon);
                    localName = trimmed.Substring(colon + 1);
                    nsUri = attrSet.Element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
                }
                else
                {
                    localName = trimmed;
                    nsUri = "";
                }
                if (!allAttrSets.ContainsKey((localName, nsUri)))
                    throw new InvalidOperationException($"XTSE0710: Attribute set '{trimmed}' is not defined.");
            }
        }

        // Static validation: check for disallowed attributes and children on XSLT instructions
        ValidateInstructionTree(root);
    }

    /// <summary>
    /// Builds the static context for this module by resolving imports/includes and
    /// evaluating static variables/parameters in document order. Top-level
    /// <c>use-when</c> attributes are evaluated as each element is encountered so
    /// that forward references to imported static variables are detected.
    /// </summary>
    private void BuildStaticContext()
    {
        var root = _document.Root;
        if (root == null) return;

        foreach (var child in root.Elements().ToList())
        {
            // Expand shadow attributes on XSLT elements before any of their real
            // attribute values are consumed (e.g. _static on xsl:variable, _href on
            // xsl:include, _use-when on any top-level element).
            if (child.Name.NamespaceName == XslNamespace)
                ExpandAllShadowAttributes(child);

            var ns = child.Name.NamespaceName;
            var localName = child.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (UseWhen(child))
                {
                    var href = child.Attribute("href")?.Value;
                    if (string.IsNullOrEmpty(href))
                        throw new InvalidOperationException("XTSE0010: Missing required href attribute on xsl:import.");
                    ResolveImport(child, href);
                }
                else
                {
                    child.Remove();
                }
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (UseWhen(child))
                {
                    var href = child.Attribute("href")?.Value;
                    if (string.IsNullOrEmpty(href))
                        throw new InvalidOperationException("XTSE0010: Missing required href attribute on xsl:include.");
                    ResolveInclude(child, href);
                }
                else
                {
                    child.Remove();
                }
            }
            else if (ns == XslNamespace && (localName == "variable" || localName == "param"))
            {
                ProcessStaticVariable(child);
            }
            else
            {
                // Evaluate use-when on all other top-level elements immediately, using
                // only the static context established up to this point.
                if (!UseWhen(child))
                    child.Remove();
            }
        }

        // Any same-precedence conflicts not resolved by a higher-precedence declaration
        // are reported now.
        if (_staticContext.PendingConflicts.Count > 0)
        {
            var first = _staticContext.PendingConflicts.First();
            throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: conflicting values for '{{{first.Key.NamespaceUri}}}{first.Key.LocalName}' at the same import precedence.");
        }

        // Recompute import precedence numbers so that later sibling imports have higher
        // precedence than earlier ones, and imported modules are always lower than their
        // importer. This produces the total order required by XSLT §6.4.
        AssignImportPrecedences();
    }

    /// <summary>
    /// Processes a single static variable or parameter declaration, validating its
    /// attributes, evaluating its select expression, and adding it to the static context.
    /// </summary>
    private void ProcessStaticVariable(XElement elem)
    {
        var staticAttr = elem.Attribute("static")?.Value;
        if (staticAttr is null)
            return;

        var trimmed = staticAttr.Trim();
        if (IsStaticYes(trimmed))
        {
            // Static declaration: evaluate if not excluded by its own use-when.
            if (!UseWhen(elem))
                return;

            if (!IsStaticBodyEmpty(elem))
                throw new InvalidOperationException("XTSE0620: Static variable or parameter must not have a sequence constructor.");

            // XTSE0090: tunnel is not permitted on static variables/parameters.
            if (elem.Attribute("tunnel") != null)
                throw new InvalidOperationException("XTSE0090: The tunnel attribute is not permitted on a static variable or parameter.");

            // XTSE0090: visibility is not permitted on static variables/parameters.
            if (elem.Attribute("visibility") != null)
                throw new InvalidOperationException("XTSE0090: The visibility attribute is not permitted on a static variable or parameter.");

            var select = elem.Attribute("select")?.Value;
            var requiredAttr = elem.Attribute("required")?.Value;
            bool isRequired = requiredAttr != null && IsStaticYes(requiredAttr.Trim());

            // XTSE0010: a required static parameter must not have a select attribute.
            if (isRequired && !string.IsNullOrEmpty(select))
                throw new InvalidOperationException("XTSE0010: A required static parameter must not have a select attribute.");

            bool isParam = elem.Name.LocalName == "param";

            // Determine the effective value. Externally supplied parameter values override
            // the default select expression, which avoids forward-reference errors when a
            // static param is referenced before it is declared.
            var name = elem.Attribute("name")?.Value;
            var (local, ns) = string.IsNullOrEmpty(name) ? (string.Empty, string.Empty) : ExpandVariableName(elem, name);
            var key = (local, ns);

            XdmValue value;
            if (isParam && _externalStaticParameters.TryGetValue(key, out var externalValue))
            {
                value = externalValue;
            }
            else if (string.IsNullOrEmpty(select))
            {
                // No select attribute: required parameters have no default value;
                // all other static declarations default to the empty sequence.
                value = isRequired ? XdmValue.Undefined : XdmValue.FromSequence(XdmSequence.Empty);
            }
            else
            {
                value = EvaluateStaticExpression(select, elem);
            }

            // Validate externally supplied parameter values against the declared @as type
            // at compile time (XTTE0590). Default values are validated at runtime so that
            // empty-sequence defaults for optional types are preserved correctly.
            var asType = elem.Attribute("as")?.Value;
            if (isParam && _externalStaticParameters.ContainsKey(key) && !string.IsNullOrEmpty(asType))
                value = TransformEngine.ConvertVariableValue(value, asType, isParam);

            AddStaticVariable(elem, value, ImportPrecedence);
        }
        else if (IsStaticNo(trimmed))
        {
            // Non-static declaration: not part of the static context.
        }
        else
        {
            throw new InvalidOperationException("XTSE0020: Invalid value for static attribute.");
        }
    }

    /// <summary>
    /// Returns true if the given static attribute value means "static".
    /// </summary>
    private static bool IsStaticYes(string value)
        => value.Trim() is "yes" or "true" or "1";

    /// <summary>
    /// Returns true if the given static attribute value means "non-static".
    /// </summary>
    private static bool IsStaticNo(string value)
        => value.Trim() is "no" or "false" or "0";

    /// <summary>
    /// Evaluates an XPath expression in the current static context.
    /// </summary>
    private XdmValue EvaluateStaticExpression(string expression, XElement elem)
    {
        var ctx = CreateUseWhenContext(elem);
        var compiled = XPath31Expression.Compile(expression);
        return compiled.Evaluate(ctx);
    }

    /// <summary>
    /// Returns true if the content of a static variable or parameter is empty after
    /// applying use-when to its children.
    /// </summary>
    private bool IsStaticBodyEmpty(XElement elem)
    {
        foreach (var node in elem.Nodes())
        {
            if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
                continue;
            if (node is XComment or XProcessingInstruction)
                continue;
            if (node is XElement child)
            {
                // If the child is excluded by use-when, it contributes no content.
                if (UseWhen(child))
                    return false;
                continue;
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Adds a static variable or parameter to the static context, detecting conflicts
    /// with existing declarations (XTSE3450).
    /// </summary>
    private void AddStaticVariable(XElement elem, XdmValue value, int precedence)
    {
        var name = elem.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(name))
            return;

        var (local, ns) = ExpandVariableName(elem, name);
        AddStaticVariable((local, ns), value, elem.Name.LocalName == "param", precedence);
    }

    /// <summary>
    /// Adds a static variable or parameter to the static context, detecting conflicts
    /// with existing declarations (XTSE3450).
    /// </summary>
    private void AddStaticVariable((string LocalName, string NamespaceUri) key, XdmValue value, bool isParam, int precedence)
    {
        if (_staticContext.Variables.TryGetValue(key, out var existing))
        {
            if (existing.IsParam != isParam)
            {
                // XTSE3450: a static variable and static parameter with the same expanded
                // name are inconsistent unless the lower-precedence declaration is
                // overridden by a higher-precedence declaration that was processed first.
                if (precedence > existing.Precedence)
                    return; // new declaration has lower precedence: overridden

                // New declaration has same or higher precedence: conflicting kinds.
                throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: variable and parameter have the same name '{{{key.NamespaceUri}}}{key.LocalName}'.");
            }

            if (precedence > existing.Precedence)
            {
                // New declaration has lower import precedence: it is overridden.
                return;
            }

            if (precedence < existing.Precedence)
            {
                // New declaration has higher import precedence. If it resolves a
                // pending same-precedence conflict, the conflict is settled. Otherwise
                // a change in the effective value is inconsistent.
                if (!_staticContext.PendingConflicts.ContainsKey(key) &&
                    !XdmValueEqualityComparer.Instance.Equals(existing.Value, value))
                {
                    throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: conflicting values for '{{{key.NamespaceUri}}}{key.LocalName}'.");
                }

                _staticContext.Variables[key] = (value, isParam, precedence);
                _staticContext.PendingConflicts.Remove(key);
                return;
            }

            // Same import precedence: values must be identical.
            if (XdmValueEqualityComparer.Instance.Equals(existing.Value, value))
                return;

            _staticContext.PendingConflicts[key] = (existing.Value, value);
            return;
        }

        _staticContext.Variables[key] = (value, isParam, precedence);
    }

    /// <summary>
    /// Merges the static context of an imported or included child module into this
    /// module's static context.
    /// </summary>
    private void MergeChildStaticContext(StaticContext child)
    {
        foreach (var (key, entry) in child.Variables)
            AddStaticVariable(key, entry.Value, entry.IsParam, entry.Precedence);
    }

    /// <summary>
    /// Performs static validation of the stylesheet tree, checking for disallowed
    /// attributes and children on XSLT instructions.
    /// </summary>
    /// <summary>
    /// Returns false when the element is inside an unknown XSLT element that is in
    /// forwards-compatible mode, unless the element is a descendant of an
    /// <c>xsl:fallback</c> child of that unknown element.
    /// </summary>
    private bool ShouldValidateElement(XElement element)
    {
        var current = element.Parent;
        while (current != null)
        {
            if (current.Name.NamespaceName == XslNamespace &&
                !KnownXsltElementNames.Contains(current.Name.LocalName) &&
                IsForwardsCompatibleElement(current))
            {
                // Walk up from the element to the unknown ancestor to find the
                // immediate child of the unknown ancestor on that path.
                var childOnPath = element;
                while (childOnPath.Parent != null && childOnPath.Parent != current)
                    childOnPath = childOnPath.Parent;

                if (childOnPath.Name.NamespaceName == XslNamespace && childOnPath.Name.LocalName == "fallback")
                    return true;

                return false;
            }
            current = current.Parent;
        }
        return true;
    }

    private void ValidateInstructionTree(XElement root)
    {
        foreach (var elem in root.DescendantsAndSelf())
        {
            if (!ShouldValidateElement(elem))
                continue;

            bool isXsltElement = elem.Name.NamespaceName == XslNamespace;
            var localName = elem.Name.LocalName;

            // XTSE0090: static variables and parameters must be declared at the top level.
            if (isXsltElement && localName is "param" or "variable")
            {
                var staticAttr = elem.Attribute("static")?.Value;
                if (!string.IsNullOrEmpty(staticAttr) && IsStaticYes(staticAttr.Trim()))
                {
                    var parent = elem.Parent;
                    bool isTopLevel = parent != null &&
                        parent.Name.NamespaceName == XslNamespace &&
                        parent.Name.LocalName is "transform" or "stylesheet";
                    if (!isTopLevel)
                        throw new InvalidOperationException("XTSE0090: A static variable or parameter must be declared at the top level of the stylesheet.");
                }
            }

            // XTSE0805 / XTSE0090: validate attributes in the XSLT namespace.
            foreach (var attr in elem.Attributes())
            {
                if (attr.Name.NamespaceName != XslNamespace)
                    continue;

                var attrLocal = attr.Name.LocalName;
                if (isXsltElement)
                {
                    // XSLT-namespaced attributes are not permitted on XSLT elements.
                    throw new InvalidOperationException("XTSE0090");
                }
                else
                {
                    // On literal result elements only the defined XSLT attributes are allowed.
                    var allowed = attrLocal is "use-when" or "expand-text" or "type" or "validation"
                        or "default-mode" or "default-collation" or "default-validation"
                        or "exclude-result-prefixes" or "extension-element-prefixes"
                        or "version" or "xpath-default-namespace"
                        or "use-attribute-sets"
                        or "inherit-namespaces";
                    if (!allowed && !IsForwardsCompatibleElement(elem))
                        throw new InvalidOperationException("XTSE0805");
                }
            }

            // XTSE0020: validate expand-text attribute values. The attribute may
            // appear as no-namespace expand-text on XSLT elements or as
            // xsl:expand-text on literal result elements; it must not be an AVT.
            foreach (var attr in elem.Attributes())
            {
                if (attr.Name.LocalName != "expand-text")
                    continue;
                if (attr.IsNamespaceDeclaration)
                    continue;
                var val = attr.Value;
                if (IsAvtValue(val) || !IsYesNoValue(val))
                    throw new InvalidOperationException("XTSE0020");
            }

            // XTSE0010 / XTSE0090: xsl:element does not allow a select attribute.
            if (isXsltElement && localName == "element" && elem.Attribute("select") != null)
                throw new InvalidOperationException("XTSE0090");

            if (isXsltElement && localName == "element")
            {
                var allowedElementAttributes = new HashSet<string>(StringComparer.Ordinal)
                {
                    "name", "namespace", "inherit-namespaces", "use-attribute-sets",
                    "type", "validation", "select", "use-when"
                };
                foreach (var attr in elem.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                        continue;
                    if (!string.IsNullOrEmpty(attr.Name.NamespaceName))
                        continue;
                    var baseName = attr.Name.LocalName;
                    if (baseName.StartsWith("_"))
                        baseName = baseName.Substring(1);
                    if (!allowedElementAttributes.Contains(baseName))
                        throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:element.");
                }
            }

            // XTSE0010 / XTSE0090 / XTSE0020: validate xsl:variable, xsl:param and xsl:with-param.
            if (isXsltElement && localName is "variable" or "param" or "with-param")
            {
                var nameAttr = elem.Attribute("name") ?? elem.Attribute("_name");
                if (nameAttr == null || string.IsNullOrWhiteSpace(nameAttr.Value))
                    throw new InvalidOperationException($"XTSE0010: Missing name attribute on xsl:{localName}.");

                var allowedAttributes = localName switch
                {
                    "variable" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "static", "use-when", "visibility", "version", "expand-text"
                    },
                    "param" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "required", "tunnel", "static", "use-when", "version", "expand-text"
                    },
                    "with-param" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "tunnel", "use-when"
                    },
                    _ => new HashSet<string>(StringComparer.Ordinal)
                };

                // In forwards-compatible mode unknown attributes on XSLT elements are ignored.
                if (!IsForwardsCompatible)
                {
                    foreach (var attr in elem.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration)
                            continue;
                        if (!string.IsNullOrEmpty(attr.Name.NamespaceName))
                            continue;

                        var baseName = attr.Name.LocalName;
                        if (baseName.StartsWith("_"))
                            baseName = baseName.Substring(1);

                        if (!allowedAttributes.Contains(baseName))
                            throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:{localName}.");
                    }
                }

                if (localName == "param")
                {
                    if (elem.Attribute("visibility") != null || elem.Attribute("_visibility") != null)
                        throw new InvalidOperationException("XTSE0090: visibility is not permitted on xsl:param.");

                    var requiredAttr = elem.Attribute("required") ?? elem.Attribute("_required");
                    if (requiredAttr != null)
                    {
                        var reqVal = requiredAttr.Value.Trim();
                        if (reqVal != "yes" && reqVal != "no" &&
                            reqVal != "true" && reqVal != "false" &&
                            reqVal != "1" && reqVal != "0")
                        {
                            throw new InvalidOperationException("XTSE0020: Invalid value for required attribute on xsl:param.");
                        }

                        if (reqVal == "yes" && (elem.Attribute("select") != null || elem.Attribute("_select") != null))
                            throw new InvalidOperationException("XTSE0010: A required xsl:param must not have a select attribute.");
                    }
                }

                if (localName == "with-param")
                {
                    if (elem.Attribute("required") != null || elem.Attribute("_required") != null)
                        throw new InvalidOperationException("XTSE0090: required is not permitted on xsl:with-param.");
                }
            }

            // XTSE0580: duplicate parameter names within a template or function
            if (localName is "template" or "function")
            {
                var seenParams = new HashSet<string>();
                foreach (var param in elem.Elements(XName.Get("param", XslNamespace)))
                {
                    var paramName = param.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(paramName))
                        continue;
                    var (pLocal, pNs) = ExpandVariableName(param, paramName);
                    var key = string.IsNullOrEmpty(pNs) ? pLocal : $"{{{pNs}}}{pLocal}";
                    if (!seenParams.Add(key))
                        throw new InvalidOperationException($"XTSE0580: Duplicate parameter name '{paramName}'.");
                }
            }

            // XTSE0680: xsl:call-template with a parameter not declared by the named template.
            // This is only an error in XSLT 2.0 and later; XSLT 1.0 backwards-compatible
            // stylesheets silently ignore unknown parameters.
            if (localName == "call-template" && !IsVersion10(root))
            {
                var calledName = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(calledName))
                {
                    var allNamed = _rootStylesheet.GetAllNamedTemplates();
                    if (allNamed.TryGetValue(calledName, out var rule))
                    {
                        var declaredParams = new HashSet<string>();
                        foreach (var param in rule.Element.Elements(XName.Get("param", XslNamespace)))
                        {
                            var paramName = param.Attribute("name")?.Value;
                            if (!string.IsNullOrEmpty(paramName))
                            {
                                var (pLocal, pNs) = ExpandVariableName(param, paramName);
                                var key = string.IsNullOrEmpty(pNs) ? pLocal : $"{{{pNs}}}{pLocal}";
                                declaredParams.Add(key);
                            }
                        }
                        foreach (var wp in elem.Elements(XName.Get("with-param", XslNamespace)))
                        {
                            if (wp.Attribute("tunnel")?.Value == "yes")
                                continue;
                            var wpName = wp.Attribute("name")?.Value;
                            if (string.IsNullOrEmpty(wpName))
                                continue;
                            var (wpLocal, wpNs) = ExpandVariableName(wp, wpName);
                            var wpKey = string.IsNullOrEmpty(wpNs) ? wpLocal : $"{{{wpNs}}}{wpLocal}";
                            if (!declaredParams.Contains(wpKey))
                                throw new InvalidOperationException($"XTSE0680: Parameter '{wpName}' is not declared by template '{calledName}'.");
                        }
                    }
                }
            }

            // xsl:copy-of must be empty (no children)
            if (localName == "copy-of")
            {
                // XTSE0260: xsl:copy-of must not have children
                if (elem.Elements().Any())
                    throw new InvalidOperationException("XTSE0260");

                // XTSE0090: xsl:copy-of does not allow invalid attributes
                foreach (var attr in elem.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    // Strip leading underscore for AVT forms (e.g. _copy-namespaces)
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (attr.Name.NamespaceName == "" &&
                        baseName != "select" &&
                        baseName != "copy-accumulators" &&
                        baseName != "copy-namespaces" &&
                        baseName != "type" &&
                        baseName != "validation")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: validate copy-namespaces value if it's a literal (not AVT)
                    if (baseName == "copy-namespaces" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value.Trim();
                        if (val != "yes" && val != "no" && val != "true" && val != "false" && val != "1" && val != "0")
                        {
                            throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }
            }

            // XTSE0010: xsl:on-empty must be the last significant child of its sequence constructor
            if (localName == "on-empty")
            {
                var parent = elem.Parent;
                if (parent != null)
                {
                    bool hasSignificantFollowing = false;
                    bool seen = false;
                    foreach (var node in parent.Nodes())
                    {
                        if (!seen)
                        {
                            if (node == elem) seen = true;
                            continue;
                        }
                        if (node is XElement || node is XComment || node is XProcessingInstruction)
                        {
                            hasSignificantFollowing = true;
                            break;
                        }
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                        {
                            hasSignificantFollowing = true;
                            break;
                        }
                    }
                    if (hasSignificantFollowing)
                        throw new InvalidOperationException("XTSE0010: xsl:on-empty must be the last child of its sequence constructor");
                }
            }

            // xsl:copy attribute validation
            if (localName == "copy")
            {
                // XTSE0090: xsl:copy does not allow invalid attributes
                foreach (var attr in elem.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (attr.Name.NamespaceName == "" &&
                        baseName != "select" &&
                        baseName != "copy-namespaces" &&
                        baseName != "inherit-namespaces" &&
                        baseName != "use-attribute-sets" &&
                        baseName != "type" &&
                        baseName != "validation")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: validate copy-namespaces value if it's a literal (not AVT)
                    if (baseName == "copy-namespaces" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value.Trim();
                        if (val != "yes" && val != "no" && val != "true" && val != "false" && val != "1" && val != "0")
                        {
                            throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }
            }

            // xsl:function static validation
            if (localName == "function")
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "name" &&
                        baseName != "as" &&
                        baseName != "visibility" &&
                        baseName != "override" &&
                        baseName != "override-extension-function" &&
                        baseName != "new-each-time" &&
                        baseName != "identity-sensitive" &&
                        baseName != "expand-text")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    if (!attrName.StartsWith("_"))
                    {
                        if (baseName == "override" || baseName == "override-extension-function" ||
                            baseName == "identity-sensitive" || baseName == "expand-text")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020");
                        }
                        else if (baseName == "new-each-time")
                        {
                            if (!IsNewEachTimeValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }

                var overrideAttr = elem.Attribute("override");
                var overrideExtAttr = elem.Attribute("override-extension-function");
                if (overrideAttr != null && overrideExtAttr != null)
                {
                    var o1 = TryParseYesNo(overrideAttr.Value);
                    var o2 = TryParseYesNo(overrideExtAttr.Value);
                    if (o1.HasValue && o2.HasValue && o1.Value != o2.Value)
                        throw new InvalidOperationException("XTSE0020");
                }

                // xsl:param children of xsl:function may have @required='yes' but not @required='no'
                foreach (var param in elem.Elements(XName.Get("param", XslNamespace)))
                {
                    var requiredVal = param.Attribute("required")?.Value.Trim();
                    if (requiredVal == "no")
                        throw new InvalidOperationException("XTSE0020");
                }
            }

            // xsl:message static validation
            if (localName == "message")
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "select" &&
                        baseName != "terminate" &&
                        baseName != "error-code" &&
                        baseName != "expand-text" &&
                        baseName != "use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: literal terminate value must be a valid boolean/yes-no.
                    // Attribute value templates (containing unescaped '{') are evaluated at
                    // runtime and are not validated here.
                    if (baseName == "terminate" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value;
                        if (!IsAvtValue(val) && !IsYesNoValue(val))
                            throw new InvalidOperationException("XTSE0020");
                    }
                }
            }

            // xsl:sequence does not allow @as
            if (localName == "sequence" && elem.Attribute("as") != null)
                throw new InvalidOperationException("XTSE0090");

            // xsl:import-schema is only supported by schema-aware processors
            if (localName == "import-schema")
                throw new InvalidOperationException("XTSE1650: xsl:import-schema requires a schema-aware processor");

            // XTSE0090: package-version is only permitted on xsl:package
            if ((localName == "stylesheet" || localName == "transform") && elem.Attribute("package-version") != null && !IsForwardsCompatibleElement(elem))
                throw new InvalidOperationException("XTSE0090");

            // XTSE1660: non-schema-aware processors do not support validation/type attributes
            // that require schema awareness. Only strict requires a schema-aware processor;
            // lax is permitted (it behaves like skip on a basic processor).
            var validationAttr = elem.Attribute("validation") ?? elem.Attribute(XName.Get("validation", XslNamespace));
            if (validationAttr != null)
            {
                var val = validationAttr.Value.Trim();
                if (val == "strict")
                    throw new InvalidOperationException("XTSE1660");
            }
            if (localName is "stylesheet" or "transform" or "package")
            {
                var defaultValidationAttr = elem.Attribute("default-validation") ?? elem.Attribute(XName.Get("default-validation", XslNamespace));
                if (defaultValidationAttr != null)
                {
                    var val = defaultValidationAttr.Value.Trim();
                    if (val == "strict")
                        throw new InvalidOperationException("XTSE1660");
                }
            }
            var typeAttr = elem.Attribute("type") ?? elem.Attribute(XName.Get("type", XslNamespace));
            if (isXsltElement && typeAttr != null && localName != "merge-source")
                throw new InvalidOperationException("XTSE1660");

            // xsl:merge validation
            if (localName == "merge")
            {
                var mergeSources = elem.Elements(XName.Get("merge-source", XslNamespace)).ToList();
                var mergeActions = elem.Elements(XName.Get("merge-action", XslNamespace)).ToList();

                // XTSE0010: at least one merge-source and exactly one merge-action
                if (mergeSources.Count == 0)
                    throw new InvalidOperationException("XTSE0010: xsl:merge must contain at least one xsl:merge-source");
                if (mergeActions.Count != 1)
                    throw new InvalidOperationException("XTSE0010: xsl:merge must contain exactly one xsl:merge-action");

                // XTSE0090: xsl:merge allows only use-when (and xml:*)
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName == "" &&
                        attr.Name.LocalName != "use-when" &&
                        attr.Name.LocalName != "_use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }

                // All merge-sources must specify the same number of merge keys
                int? keyCount = null;
                foreach (var source in mergeSources)
                {
                    var count = source.Elements(XName.Get("merge-key", XslNamespace)).Count();
                    if (keyCount == null)
                        keyCount = count;
                    else if (keyCount != count)
                        throw new InvalidOperationException("XTSE0010: all xsl:merge-source elements must have the same number of xsl:merge-key children");
                }

                // Validate child order: merge-source*, merge-action, fallback*
                bool actionSeen = false;
                foreach (var child in elem.Elements())
                {
                    if (child.Name.NamespaceName != XslNamespace)
                        throw new InvalidOperationException("XTSE0010: xsl:merge may only contain XSLT namespace children");

                    var childName = child.Name.LocalName;
                    if (childName == "merge-source")
                    {
                        if (actionSeen)
                            throw new InvalidOperationException("XTSE0010: xsl:merge-source must appear before xsl:merge-action");
                    }
                    else if (childName == "merge-action")
                    {
                        actionSeen = true;
                    }
                    else if (childName == "fallback")
                    {
                        if (!actionSeen)
                            throw new InvalidOperationException("XTSE0010: xsl:fallback must follow xsl:merge-action");
                    }
                    else
                    {
                        throw new InvalidOperationException("XTSE0010: xsl:merge may only contain xsl:merge-source, xsl:merge-action, and xsl:fallback children");
                    }
                }
            }

            // xsl:merge-source validation
            if (localName == "merge-source")
            {
                // Must be child of xsl:merge
                if (elem.Parent?.Name.LocalName != "merge" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must be a child of xsl:merge");

                var hasSelect = elem.Attribute("select") != null || elem.Attribute("_select") != null;
                var hasForEachItem = elem.Attribute("for-each-item") != null || elem.Attribute("_for-each-item") != null;
                var hasForEachSource = elem.Attribute("for-each-source") != null || elem.Attribute("_for-each-source") != null;

                // XTSE3195: @for-each-item and @for-each-source are mutually exclusive
                if (hasForEachItem && hasForEachSource)
                    throw new InvalidOperationException("XTSE3195: xsl:merge-source cannot have both for-each-item and for-each-source");

                // Must have @select or @for-each-item or @for-each-source
                if (!hasSelect && !hasForEachItem && !hasForEachSource)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must have a select, for-each-item, or for-each-source attribute");

                // Must contain at least one xsl:merge-key
                var mergeKeys = elem.Elements(XName.Get("merge-key", XslNamespace)).ToList();
                if (mergeKeys.Count == 0)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must contain at least one xsl:merge-key");

                // XTSE0010: only xsl:merge-key children are permitted (whitespace text is ignored)
                foreach (var child in elem.Elements())
                {
                    if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "merge-key")
                        throw new InvalidOperationException("XTSE0010: xsl:merge-source may only contain xsl:merge-key children");
                }

                // XTSE0090 / XTSE0020: attribute validation
                var hasValidation = false;
                var hasType = false;
                string? validationValue = null;
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (!IsMergeSourceAttribute(baseName))
                        throw new InvalidOperationException("XTSE0090");

                    if (!attrName.StartsWith("_"))
                    {
                        if (baseName == "streamable")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020: invalid value for streamable");
                        }
                        else if (baseName == "sort-before-merge")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020: invalid value for sort-before-merge");
                        }
                        else if (baseName == "validation")
                        {
                            hasValidation = true;
                            validationValue = attr.Value.Trim();
                            if (validationValue is not "strict" and not "lax" and not "preserve" and not "strip")
                                throw new InvalidOperationException("XTSE0020: invalid value for validation");
                        }
                        else if (baseName == "type")
                        {
                            hasType = true;
                        }
                    }
                }

                // XTSE1505: validation and type are mutually exclusive
                if (hasValidation && hasType)
                    throw new InvalidOperationException("XTSE1505: xsl:merge-source cannot have both validation and type attributes");

                // XTSE1660: non-schema-aware processors do not support the type attribute
                if (hasType)
                    throw new InvalidOperationException("XTSE1660: xsl:merge-source/@type requires a schema-aware processor");

                // Validate @name is a valid NCName/EQName if present
                var nameAttr = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                {
                    if (!IsValidMergeSourceName(nameAttr))
                        throw new InvalidOperationException("XTSE0020: invalid xsl:merge-source name");
                }
            }

            // xsl:merge-key validation
            if (localName == "merge-key")
            {
                // Must be child of xsl:merge-source
                if (elem.Parent?.Name.LocalName != "merge-source" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-key must be a child of xsl:merge-source");

                // XTSE0090: disallowed attributes
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "select" &&
                        baseName != "order" &&
                        baseName != "data-type" &&
                        baseName != "lang" &&
                        baseName != "case-order" &&
                        baseName != "collation" &&
                        baseName != "use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }

                // XTSE3200: select attribute and sequence-constructor content are mutually exclusive
                if (elem.Attribute("select") != null || elem.Attribute("_select") != null)
                {
                    bool hasNonWhitespaceContent = false;
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XElement)
                        {
                            hasNonWhitespaceContent = true;
                            break;
                        }
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                        {
                            hasNonWhitespaceContent = true;
                            break;
                        }
                    }
                    if (hasNonWhitespaceContent)
                        throw new InvalidOperationException("XTSE3200: xsl:merge-key cannot have both a select attribute and content");
                }
            }

            // xsl:merge-action validation
            if (localName == "merge-action")
            {
                // Must be child of xsl:merge
                if (elem.Parent?.Name.LocalName != "merge" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-action must be a child of xsl:merge");

                // XTSE0090: xsl:merge-action does not allow attributes
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName == "" &&
                        attr.Name.LocalName != "use-when" &&
                        attr.Name.LocalName != "_use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }
            }
        }
    }

    private static bool IsMergeSourceAttribute(string baseName)
    {
        return baseName is "select" or "for-each-item" or "for-each-source"
                   or "name" or "streamable" or "sort-before-merge"
                   or "use-accumulators" or "validation" or "type" or "use-when";
    }

    private static bool IsValidMergeSourceName(string name)
    {
        // Allow Q{uri}local EQNames and simple NCNames
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            return closeBrace >= 2 && closeBrace < name.Length - 1;
        }
        // Simple NCName check: no colons, not starting with digit
        if (string.IsNullOrEmpty(name) || name.Contains(':'))
            return false;
        var first = name[0];
        return first == '_' || char.IsLetter(first);
    }

    private static bool IsYesNoValue(string value)
    {
        var v = value.Trim();
        return v == "yes" || v == "no" || v == "true" || v == "false" || v == "1" || v == "0";
    }

    private static bool IsAvtValue(string value)
    {
        // An attribute value template contains an unescaped '{'.
        for (int i = 0; i < value.Length - 1; i++)
        {
            if (value[i] == '{' && value[i + 1] != '{')
                return true;
        }
        return false;
    }

    private static bool IsNewEachTimeValue(string value)
    {
        var v = value.Trim();
        return v == "yes" || v == "no" || v == "true" || v == "false" || v == "1" || v == "0" ||
               v == "maybe" || v == "probably";
    }

    private static bool? TryParseYesNo(string value)
    {
        var v = value.Trim();
        if (v == "yes" || v == "true" || v == "1")
            return true;
        if (v == "no" || v == "false" || v == "0")
            return false;
        return null;
    }

    /// <summary>
    /// Wraps a literal result element stylesheet into an implicit xsl:stylesheet/xsl:template document.
    /// </summary>
    private static XDocument WrapLiteralResultElement(XElement literalRoot, string version)
    {
        var xsl = XNamespace.Get("http://www.w3.org/1999/XSL/Transform");
        var wrapper = new XElement(xsl + "stylesheet",
            new XAttribute("version", version),
            new XElement(xsl + "template",
                new XAttribute("match", "/"),
                literalRoot));

        // Copy all namespace declarations from the literal root to the wrapper
        foreach (var attr in literalRoot.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                if (attr.Name.LocalName == "xmlns")
                    wrapper.SetAttributeValue("xmlns", attr.Value);
                else
                    wrapper.SetAttributeValue(XNamespace.Xmlns + attr.Name.LocalName, attr.Value);
            }
        }

        return new XDocument(wrapper);
    }

    /// <summary>
    /// Resolves a lexical variable or parameter name to an expanded QName.
    /// Handles <c>Q{uri}local</c> EQNames and prefixed QNames using the namespaces
    /// in scope on the declaring element.
    /// </summary>
    internal static (string LocalName, string NamespaceUri) ExpandVariableName(XElement element, string name)
    {
        if (string.IsNullOrEmpty(name))
            return ("", "");

        // The empty URI form Q{}local is permitted and means "no namespace".
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace >= 2)
            {
                string uri = name[2..closeBrace];
                string rest = name[(closeBrace + 1)..];
                int restColon = rest.IndexOf(':');
                string local = restColon < 0 ? rest : rest[(restColon + 1)..];
                return (local, uri);
            }
        }

        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string local = name[(colon + 1)..];
            if (prefix == "xml")
                return (local, "http://www.w3.org/XML/1998/namespace");

            var ns = element.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}'");
            return (local, ns.NamespaceName);
        }

        return (name, "");
    }

    /// <summary>
    /// Returns true if the effective stylesheet version is 1.0 (backwards-compatible mode).
    /// </summary>
    private static bool IsVersion10(XElement root)
    {
        var version = root.Attribute("version")?.Value?.Trim();
        return version is "1.0" or "1";
    }

    /// <summary>
    /// Annotation attached to an <c>xsl:import</c> or <c>xsl:include</c> element
    /// so that document-order flattening methods can locate the resolved child
    /// stylesheet (if any).
    /// </summary>
    private sealed class ResolvedModuleAnnotation
    {
        public Stylesheet? Module { get; init; }
    }

    private void ResolveImport(XElement importElement, string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            var root = doc.Root;
            // use-when on the root element of an imported module excludes the whole module.
            if (root != null && !UseWhen(root, resolvedUri))
                return;
            var child = new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence + 1, childResolvedUris, null, _externalStaticParameters, _rootStylesheet);
            child.ApplyImportsContextModule = child;
            _imports.Add(child);
            importElement.AddAnnotation(new ResolvedModuleAnnotation { Module = child });
            MergeChildStaticContext(child._staticContext);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to resolve xsl:import href '{href}'.", ex);
        }
    }

    private void ResolveInclude(XElement includeElement, string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        // Circular reference detection: if this URI is already in the ancestor chain,
        // including it would create a cycle.
        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            var root = doc.Root;
            // use-when on the root element of an included module excludes the whole module.
            if (root != null && !UseWhen(root, resolvedUri))
                return;
            var child = new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence, childResolvedUris, _staticContext, _externalStaticParameters, _rootStylesheet);
            child.ApplyImportsContextModule = ApplyImportsContextModule;
            _includes.Add(child);
            includeElement.AddAnnotation(new ResolvedModuleAnnotation { Module = child });
            MergeChildStaticContext(child._staticContext);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to resolve xsl:include href '{href}'.", ex);
        }
    }

    private static string ResolveAbsoluteUri(string href, string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
        {
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                return href;
            return Path.GetFullPath(href);
        }

        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
            return href;

        var baseUriObj = new Uri(baseUri);
        var resolved = new Uri(baseUriObj, href);
        return resolved.AbsoluteUri;
    }

    /// <summary>
    /// Returns the effective base URI for the given element, resolving any
    /// <c>xml:base</c> attributes in the ancestor chain against the module base URI.
    /// </summary>
    private string? GetEffectiveBaseUri(XElement elem)
    {
        // Start from the document/entity base URI when available, otherwise the
        // module URI supplied by the resolver.
        var currentBase = !string.IsNullOrEmpty(elem.BaseUri) ? elem.BaseUri : _baseUri;

        // Apply xml:base attributes from the root down to the element.
        foreach (var ancestor in elem.AncestorsAndSelf().Reverse())
        {
            var xmlBase = ancestor.Attribute(XName.Get("base", "http://www.w3.org/XML/1998/namespace"))?.Value;
            if (string.IsNullOrEmpty(xmlBase))
                continue;

            currentBase = string.IsNullOrEmpty(currentBase)
                ? ResolveAbsoluteUri(xmlBase, null)
                : ResolveAbsoluteUri(xmlBase, currentBase);
        }

        return currentBase;
    }

    /// <summary>The parsed xsl:output properties, or null if not specified.</summary>
    public OutputProperties? OutputProperties => _outputProperties;

    /// <summary>Named xsl:output definitions keyed by expanded QName (Clark notation).</summary>
    public IReadOnlyDictionary<string, OutputProperties> NamedOutputProperties => _namedOutputProperties;

    /// <summary>
    /// Returns the effective xsl:output properties for a named output declaration,
    /// merging definitions from imported, included, and local modules in ascending
    /// order of import precedence. Scalar attributes are overridden by higher-precedence
    /// declarations; list-valued attributes (cdata-section-elements, etc.) are combined.
    /// </summary>
    public OutputProperties? GetEffectiveNamedOutput(string expandedName)
    {
        var definitions = new List<(int Precedence, OutputProperties Props)>();
        CollectNamedOutputDefinitions(this, expandedName, definitions);
        if (definitions.Count == 0)
            return null;

        // Lower numeric precedence means higher XSLT import precedence, so merge
        // lowest-precedence (imported) definitions first and the main stylesheet last.
        definitions.Sort((a, b) => b.Precedence.CompareTo(a.Precedence));

        var result = new OutputProperties();
        foreach (var (_, props) in definitions)
            OutputProperties.Merge(result, props);

        return result;
    }

    private static void CollectNamedOutputDefinitions(Stylesheet sheet, string expandedName, List<(int, OutputProperties)> definitions)
    {
        foreach (var imported in sheet._imports)
            CollectNamedOutputDefinitions(imported, expandedName, definitions);
        foreach (var included in sheet._includes)
            CollectNamedOutputDefinitions(included, expandedName, definitions);

        if (sheet._namedOutputProperties.TryGetValue(expandedName, out var props))
            definitions.Add((sheet.ImportPrecedence, props));
    }

    /// <summary>
    /// Looks up a character-map definition by expanded QName, searching this stylesheet
    /// module and its imports/includes.
    /// </summary>
    public CharacterMapDefinition? GetCharacterMap(string expandedName)
    {
        if (_characterMaps.TryGetValue(expandedName, out var def))
            return def;

        foreach (var import in _imports)
        {
            var imported = import.GetCharacterMap(expandedName);
            if (imported != null)
                return imported;
        }

        foreach (var include in _includes)
        {
            var included = include.GetCharacterMap(expandedName);
            if (included != null)
                return included;
        }

        return null;
    }

    /// <summary>
    /// Resolves a list of character-map names into an effective character-to-string map.
    /// The maps are processed in the order supplied; for duplicate characters, the first
    /// map in the list wins. Within a single character map, explicit
    /// <c>xsl:output-character</c> mappings override mappings inherited via
    /// <c>use-character-maps</c>.
    /// </summary>
    public Dictionary<char, string> ResolveCharacterMap(IEnumerable<string> expandedNames)
    {
        var result = new Dictionary<char, string>();
        var expanded = new Dictionary<string, Dictionary<char, string>>();
        foreach (var name in expandedNames)
        {
            var map = ExpandCharacterMap(name, expanded);
            foreach (var (ch, str) in map)
            {
                // Later maps in the supplied list override earlier ones; explicit mappings
                // within a single map already override its used maps in ExpandCharacterMap.
                result[ch] = str;
            }
        }
        return result;
    }

    private Dictionary<char, string> ExpandCharacterMap(string expandedName, Dictionary<string, Dictionary<char, string>> expanded)
    {
        if (string.IsNullOrEmpty(expandedName))
            return new Dictionary<char, string>();

        if (expanded.TryGetValue(expandedName, out var cached))
            return cached;

        var result = new Dictionary<char, string>();
        var def = GetCharacterMap(expandedName);
        if (def == null)
        {
            expanded[expandedName] = result;
            return result;
        }

        // Place an empty entry before recursing so cycles terminate without revisiting.
        expanded[expandedName] = result;

        foreach (var used in def.UseCharacterMaps)
        {
            var usedMap = ExpandCharacterMap(used, expanded);
            foreach (var (ch, str) in usedMap)
                result[ch] = str;
        }

        // Explicit mappings in this character map override its used maps.
        foreach (var (ch, str) in def.Mappings)
            result[ch] = str;

        return result;
    }

    /// <summary>Top-level xsl:param elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalParameters => _globalParameters;

    /// <summary>Top-level xsl:variable elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalVariables => _globalVariables;

    /// <summary>Static variables and parameters evaluated during stylesheet loading.</summary>
    internal IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> StaticVariables =>
        _staticContext.Variables.ToDictionary(kv => kv.Key, kv => kv.Value.Value);

    /// <summary>All key definitions defined in this stylesheet.</summary>
    public IReadOnlyList<KeyDefinition> KeyDefinitions => _keyDefinitions;

    private HashSet<Stylesheet>? _transitiveImports;

    /// <summary>
    /// All modules that are imported (directly or indirectly, including via includes)
    /// by this stylesheet. Used by <c>xsl:apply-imports</c> to restrict the search
    /// to rules imported into the stylesheet containing the current template rule.
    /// </summary>
    public IReadOnlySet<Stylesheet> TransitiveImports
        => _transitiveImports ??= ComputeTransitiveImports();

    private HashSet<Stylesheet> ComputeTransitiveImports()
    {
        var set = new HashSet<Stylesheet>();
        foreach (var imported in _imports)
            CollectReachableImportsAndIncludes(imported, set);
        foreach (var included in _includes)
            CollectReachableImportsAndIncludes(included, set);
        return set;
    }

    /// <summary>
    /// Recursively collects a module and every module reachable from it through
    /// <c>xsl:import</c> or <c>xsl:include</c> edges. Included modules are transparent
    /// for <c>xsl:apply-imports</c>, so rules declared in modules included by an imported
    /// module must be visible to the importer.
    /// </summary>
    private static void CollectReachableImportsAndIncludes(Stylesheet module, HashSet<Stylesheet> set)
    {
        if (!set.Add(module))
            return;
        foreach (var imported in module._imports)
            CollectReachableImportsAndIncludes(imported, set);
        foreach (var included in module._includes)
            CollectReachableImportsAndIncludes(included, set);
    }

    /// <summary>
    /// Assigns import-precedence values to all imported modules. The main stylesheet
    /// has precedence 0; imported modules are numbered so that later sibling imports
    /// have lower numbers (higher XSLT precedence) than earlier ones. Includes keep
    /// the same precedence as their parent.
    /// </summary>
    private void AssignImportPrecedences()
    {
        var order = new List<Stylesheet>();
        CollectImportedModulesHighToLow(this, order);
        int rank = 1;
        foreach (var module in order)
            module.ImportPrecedence = rank++;
    }

    private static void CollectImportedModulesHighToLow(Stylesheet module, List<Stylesheet> order)
    {
        // Traverse top-level elements in reverse document order so that later
        // imports (which have higher XSLT precedence) are emitted first.
        foreach (var element in module.Root.Elements().Reverse())
        {
            if (element.Name.NamespaceName != XslNamespace)
                continue;

            if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } child })
            {
                if (element.Name.LocalName == "import")
                {
                    // The imported module itself is higher than its own imports.
                    order.Add(child);
                }
                // Includes share the parent's precedence; their local content is not
                // added, but their imports are still lower than the parent.
                CollectImportedModulesHighToLow(child, order);
            }
        }
    }

    /// <summary>
    /// Collects global parameters and variables in document order, recursing into
    /// imported and included modules at the point where their <c>xsl:import</c> or
    /// <c>xsl:include</c> element occurs. Each declaration is tagged with its
    /// import precedence and a monotonic document-order index so callers can sort
    /// by precedence (lower first) and then by document order, matching XSLT
    /// import-precedence / last-wins semantics.
    /// </summary>
    public void CollectGlobalsInDocumentOrder(List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam)> globals, ref int order)
    {
        foreach (var element in Root.Elements())
        {
            var ns = element.Name.NamespaceName;
            var localName = element.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } imported })
                    imported.CollectGlobalsInDocumentOrder(globals, ref order);
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } included })
                    included.CollectGlobalsInDocumentOrder(globals, ref order);
            }
            else if (ns == XslNamespace && localName == "param")
            {
                var name = ExpandVariableName(element, element.Attribute("name")?.Value ?? "");
                globals.Add((ImportPrecedence, order++, name, element, true));
            }
            else if (ns == XslNamespace && localName == "variable")
            {
                var name = ExpandVariableName(element, element.Attribute("name")?.Value ?? "");
                globals.Add((ImportPrecedence, order++, name, element, false));
            }
        }
    }

    /// <summary>
    /// Recursively collects all global parameters from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, XElement> GetAllGlobalParameters()
    {
        var result = new Dictionary<string, XElement>();

        foreach (var imported in _imports)
        {
            foreach (var (name, elem) in imported.GetAllGlobalParameters())
                result[name] = elem;
        }

        foreach (var included in _includes)
        {
            foreach (var (name, elem) in included.GetAllGlobalParameters())
                result[name] = elem;
        }

        foreach (var param in _globalParameters)
        {
            var name = param.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
                result[name] = param;
        }

        return result;
    }

    /// <summary>
    /// Recursively collects all global variables from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, XElement> GetAllGlobalVariables()
    {
        var result = new Dictionary<string, XElement>();

        foreach (var imported in _imports)
        {
            foreach (var (name, elem) in imported.GetAllGlobalVariables())
                result[name] = elem;
        }

        foreach (var included in _includes)
        {
            foreach (var (name, elem) in included.GetAllGlobalVariables())
                result[name] = elem;
        }

        foreach (var variable in _globalVariables)
        {
            var name = variable.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
                result[name] = variable;
        }

        return result;
    }

    /// <summary>
    /// Recursively collects all key definitions from this stylesheet, its includes, and its imports.
    /// </summary>
    public IReadOnlyList<KeyDefinition> GetAllKeyDefinitions()
    {
        var result = new List<KeyDefinition>(_keyDefinitions);
        foreach (var included in _includes)
            result.AddRange(included.GetAllKeyDefinitions());
        foreach (var imported in _imports)
            result.AddRange(imported.GetAllKeyDefinitions());
        return result;
    }

    /// <summary>Function definitions declared in this stylesheet.</summary>
    public IReadOnlyList<XsltFunctionDefinition> FunctionDefinitions => _functionDefinitions;

    /// <summary>Decimal format definitions declared in this stylesheet.</summary>
    public IReadOnlyList<DecimalFormatDefinition> DecimalFormats => _decimalFormats;

    /// <summary>
    /// Recursively collects all function definitions from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// The key is a tuple of (namespaceUri, localName, arity).
    /// </summary>
    public Dictionary<(string ns, string name, int arity), XsltFunctionDefinition> GetAllFunctionDefinitions()
    {
        var result = new Dictionary<(string, string, int), XsltFunctionDefinition>();

        foreach (var imported in _imports)
        {
            foreach (var (key, def) in imported.GetAllFunctionDefinitions())
                result[key] = def;
        }

        foreach (var included in _includes)
        {
            foreach (var (key, def) in included.GetAllFunctionDefinitions())
                result[key] = def;
        }

        foreach (var def in _functionDefinitions)
        {
            result[(def.NamespaceUri, def.LocalName, def.Arity)] = def;
        }

        return result;
    }

    /// <summary>
    /// Recursively collects all whitespace handling rules from this stylesheet, its includes, and its imports.
    /// Rules are returned in precedence order (local highest, then included, then imported).
    /// </summary>
    public List<SpaceHandlingRule> GetAllSpaceHandlingRules()
    {
        var result = new List<SpaceHandlingRule>();
        // Imported first (lowest precedence)
        foreach (var imported in _imports)
            result.AddRange(imported.GetAllSpaceHandlingRules());
        // Included next
        foreach (var included in _includes)
            result.AddRange(included.GetAllSpaceHandlingRules());
        // Local last (highest precedence)
        result.AddRange(_spaceRules);
        return result;
    }

    /// <summary>
    /// Looks up a mode definition by name, considering import/include precedence.
    /// Local definitions override included, which override imported.
    /// </summary>
    private void ValidateModeDefinitions()
    {
        var all = new Dictionary<string, List<(int Precedence, ModeDefinition Def)>>();
        CollectModeDefinitions(this, all);

        foreach (var (name, list) in all)
        {
            var minPrecedence = list.Min(x => x.Precedence);
            var top = list.Where(x => x.Precedence == minPrecedence).Select(x => x.Def).ToList();
            if (top.Count <= 1)
                continue;

            var first = top[0];
            for (int i = 1; i < top.Count; i++)
            {
                if (!AreModesEquivalent(first, top[i]))
                {
                    var details = string.Join(", ", list.Select(x => $"(p={x.Precedence},on={x.Def.OnNoMatch},vis={x.Def.Visibility},acc={string.Join("|", x.Def.UseAccumulators)})"));
                    throw new InvalidOperationException($"XTSE0545: Conflicting xsl:mode declarations for mode '{name}' at the same import precedence. [{details}]");
                }
            }
        }
    }

    private static void CollectModeDefinitions(Stylesheet stylesheet, Dictionary<string, List<(int Precedence, ModeDefinition Def)>> map)
    {
        foreach (var kv in stylesheet._modeDefinitions)
        {
            if (!map.TryGetValue(kv.Key, out var list))
            {
                list = new List<(int, ModeDefinition)>();
                map[kv.Key] = list;
            }
            list.Add((stylesheet.ImportPrecedence, kv.Value));
        }
        foreach (var included in stylesheet._includes)
            CollectModeDefinitions(included, map);
        foreach (var imported in stylesheet._imports)
            CollectModeDefinitions(imported, map);
    }

    private static bool HasSamePrecedenceMode(Stylesheet stylesheet, string name)
    {
        if (stylesheet._modeDefinitions.ContainsKey(name))
            return true;
        foreach (var included in stylesheet._includes)
        {
            if (included.ImportPrecedence == stylesheet.ImportPrecedence && HasSamePrecedenceMode(included, name))
                return true;
        }
        return false;
    }

    private static bool AreModesEquivalent(ModeDefinition a, ModeDefinition b)
    {
        return a.OnNoMatch == b.OnNoMatch
            && a.OnMultipleMatch == b.OnMultipleMatch
            && a.Visibility == b.Visibility
            && a.Typed == b.Typed
            && a.WarningOnNoMatch == b.WarningOnNoMatch
            && a.WarningOnMultipleMatch == b.WarningOnMultipleMatch
            && a.Streamable == b.Streamable
            && a.UseAllAccumulators == b.UseAllAccumulators
            && a.UseAccumulators.SetEquals(b.UseAccumulators);
    }

    public ModeDefinition? GetModeDefinition(string name)
    {
        // Local first (highest precedence)
        if (_modeDefinitions.TryGetValue(name, out var local))
            return local;

        // Included next
        foreach (var included in _includes)
        {
            var def = included.GetModeDefinition(name);
            if (def != null)
                return def;
        }

        // Imported last (lowest precedence)
        foreach (var imported in _imports)
        {
            var def = imported.GetModeDefinition(name);
            if (def != null)
                return def;
        }

        return null;
    }

    /// <summary>The root element of the parsed stylesheet document.</summary>
    public XElement RootElement => _document.Root!;

    /// <summary>
    /// Collects all namespace prefix declarations from this stylesheet and its imports/includes.
    /// </summary>
    public Dictionary<string, string> GetAllNamespaces()
    {
        var result = new Dictionary<string, string>();
        foreach (var imported in _imports)
        {
            foreach (var (prefix, ns) in imported.GetAllNamespaces())
                result[prefix] = ns;
        }
        foreach (var included in _includes)
        {
            foreach (var (prefix, ns) in included.GetAllNamespaces())
                result[prefix] = ns;
        }
        if (_document.Root != null)
        {
            foreach (var attr in _document.Root.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    var prefix = attr.Name.LocalName;
                    if (prefix == "xmlns")
                        prefix = string.Empty;
                    result[prefix] = attr.Value;
                }
            }
            // Also collect namespace declarations from descendant elements,
            // but only add new prefixes — do not override root declarations.
            // This prevents literal result elements from shadowing stylesheet
            // prefixes while still making locally-declared prefixes available.
            foreach (var elem in _document.Root.Descendants())
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        var prefix = attr.Name.LocalName;
                        if (prefix == "xmlns")
                            prefix = string.Empty;
                        if (!result.ContainsKey(prefix))
                            result[prefix] = attr.Value;
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Collects all excluded result prefixes from this stylesheet, its imports, and its includes.
    /// Imported first, then included, then local.
    /// </summary>
    public HashSet<string> GetAllExcludedResultPrefixes()
    {
        var result = new HashSet<string>();
        foreach (var imported in _imports)
        {
            foreach (var prefix in imported.GetAllExcludedResultPrefixes())
                result.Add(prefix);
        }
        foreach (var included in _includes)
        {
            foreach (var prefix in included.GetAllExcludedResultPrefixes())
                result.Add(prefix);
        }
        foreach (var prefix in _excludedResultPrefixes)
            result.Add(prefix);
        return result;
    }

    /// <summary>
    /// Returns the effective <c>xsl:namespace-alias</c> mapping for this stylesheet.
    /// Higher-precedence declarations override lower-precedence ones; conflicting
    /// declarations at the same import precedence are reported as <c>XTSE0813</c>.
    /// </summary>
    public Dictionary<string, NamespaceAliasDefinition> GetEffectiveNamespaceAliases()
    {
        var all = new List<NamespaceAliasDefinition>();
        foreach (var imported in _imports)
            all.AddRange(imported.GetAllNamespaceAliases());
        foreach (var included in _includes)
            all.AddRange(included.GetAllNamespaceAliases());
        all.AddRange(_namespaceAliases);

        var result = new Dictionary<string, NamespaceAliasDefinition>();
        foreach (var group in all.GroupBy(a => a.SourceUri))
        {
            var ordered = group.OrderBy(a => a.ImportPrecedence).ToList();
            var best = ordered[0];
            foreach (var other in ordered)
            {
                if (other.ImportPrecedence == best.ImportPrecedence &&
                    (other.ResultPrefix != best.ResultPrefix || other.ResultUri != best.ResultUri))
                {
                    throw new InvalidOperationException("XTSE0813: Conflicting xsl:namespace-alias declarations for the same namespace URI at the same import precedence.");
                }
            }
            result[group.Key] = best;
        }
        return result;
    }

    /// <summary>
    /// Collects the raw <c>xsl:namespace-alias</c> definitions from this stylesheet module only.
    /// </summary>
    public IReadOnlyList<NamespaceAliasDefinition> GetAllNamespaceAliases()
    {
        return _namespaceAliases;
    }

    /// <summary>
    /// Recursively collects all decimal format definitions from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<(string localName, string nsUri), DecimalFormatDefinition> GetAllDecimalFormats()
    {
        var result = new Dictionary<(string, string), DecimalFormat>();

        // Imported first (lowest precedence)
        foreach (var imported in _imports)
        {
            foreach (var (key, def) in imported.GetAllDecimalFormats())
                MergeDecimalFormat(result, key, def);
        }

        // Included next
        foreach (var included in _includes)
        {
            foreach (var (key, def) in included.GetAllDecimalFormats())
                MergeDecimalFormat(result, key, def);
        }

        // Local last (highest precedence)
        foreach (var def in _decimalFormats)
        {
            MergeDecimalFormat(result, (def.LocalName, def.NamespaceUri), def);
        }

        // Convert back to definitions
        var defs = new Dictionary<(string, string), DecimalFormatDefinition>();
        foreach (var (key, format) in result)
        {
            defs[key] = new DecimalFormatDefinition
            {
                LocalName = key.Item1,
                NamespaceUri = key.Item2,
                Format = format
            };
        }
        return defs;
    }

    private static void MergeDecimalFormat(Dictionary<(string, string), DecimalFormat> result, (string, string) key, DecimalFormatDefinition def)
    {
        if (!result.TryGetValue(key, out var existing))
        {
            result[key] = new DecimalFormat
            {
                DecimalSeparator = def.Format.DecimalSeparator,
                GroupingSeparator = def.Format.GroupingSeparator,
                Digit = def.Format.Digit,
                ZeroDigit = def.Format.ZeroDigit,
                PatternSeparator = def.Format.PatternSeparator,
                MinusSign = def.Format.MinusSign,
                Percent = def.Format.Percent,
                PerMille = def.Format.PerMille,
                Infinity = def.Format.Infinity,
                NaN = def.Format.NaN,
                ExponentSeparator = def.Format.ExponentSeparator
            };
            return;
        }

        // Merge explicitly-set attributes from the new definition
        foreach (var attr in def.ExplicitAttributes)
        {
            switch (attr)
            {
                case "decimal-separator": existing.DecimalSeparator = def.Format.DecimalSeparator; break;
                case "grouping-separator": existing.GroupingSeparator = def.Format.GroupingSeparator; break;
                case "infinity": existing.Infinity = def.Format.Infinity; break;
                case "minus-sign": existing.MinusSign = def.Format.MinusSign; break;
                case "NaN": existing.NaN = def.Format.NaN; break;
                case "percent": existing.Percent = def.Format.Percent; break;
                case "per-mille": existing.PerMille = def.Format.PerMille; break;
                case "zero-digit": existing.ZeroDigit = def.Format.ZeroDigit; break;
                case "digit": existing.Digit = def.Format.Digit; break;
                case "pattern-separator": existing.PatternSeparator = def.Format.PatternSeparator; break;
                case "exponent-separator": existing.ExponentSeparator = def.Format.ExponentSeparator; break;
            }
        }
    }

    /// <summary>
    /// Collects all attribute-set definitions from this stylesheet, its includes, and its imports.
    /// Attribute sets accumulate (merge) across modules: imported first, then included, then local.
    /// </summary>
    public Dictionary<(string LocalName, string NamespaceUri), List<AttributeSetDefinition>> GetAllAttributeSets()
    {
        var result = new Dictionary<(string, string), List<AttributeSetDefinition>>();

        // Imported first (lowest precedence)
        foreach (var imported in _imports)
        {
            foreach (var (key, list) in imported.GetAllAttributeSets())
            {
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();
                existing.AddRange(list);
            }
        }

        // Included next
        foreach (var included in _includes)
        {
            foreach (var (key, list) in included.GetAllAttributeSets())
            {
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();
                existing.AddRange(list);
            }
        }

        // Local last (highest precedence)
        foreach (var def in _attributeSets)
        {
            var key = (def.LocalName, def.NamespaceUri);
            if (!result.TryGetValue(key, out var existing))
                result[key] = existing = new List<AttributeSetDefinition>();
            existing.Add(def);
        }

        return result;
    }

    /// <summary>The XSLT namespace URI.</summary>
    public const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>The XML Schema namespace URI.</summary>
    public const string XsNamespace = "http://www.w3.org/2001/XMLSchema";

    /// <summary>The XPath functions namespace URI.</summary>
    public const string FnNamespace = "http://www.w3.org/2005/xpath-functions";

    /// <summary>The version attribute of the stylesheet root element.</summary>
    public string? Version { get; private set; }

    /// <summary>
    /// Whether the stylesheet is in forwards-compatible mode (declared version greater
    /// than the implementation supports). In this mode unknown attributes on XSLT
    /// elements are ignored rather than rejected.
    /// </summary>
    public bool IsForwardsCompatible =>
        !string.IsNullOrEmpty(Version) &&
        decimal.TryParse(Version, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var v) &&
        v > 3.0m;

    /// <summary>
    /// Returns the effective XSLT version for the given element, walking ancestors for
    /// an explicit <c>version</c> (XSLT elements) or <c>xsl:version</c> (literal result
    /// elements) attribute and falling back to the global stylesheet version.
    /// </summary>
    public double GetEffectiveVersion(XElement element)
    {
        var ancestor = element;
        while (ancestor != null)
        {
            XAttribute? versionAttr = null;
            if (ancestor.Name.NamespaceName == XslNamespace)
                versionAttr = ancestor.Attribute("version");
            if (versionAttr == null)
                versionAttr = ancestor.Attribute(XName.Get("version", XslNamespace));
            if (versionAttr != null)
            {
                if (double.TryParse(versionAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    return v;
                break;
            }
            ancestor = ancestor.Parent;
        }
        if (double.TryParse(Version, NumberStyles.Any, CultureInfo.InvariantCulture, out var sv))
            return sv;
        return 3.0;
    }

    /// <summary>
    /// Determines whether the given element is in XSLT forwards-compatible mode.
    /// </summary>
    public bool IsForwardsCompatibleElement(XElement element) => GetEffectiveVersion(element) > 3.0;

    /// <summary>
    /// The set of known XSLT 3.0 element names. Used during static validation to
    /// distinguish unknown XSLT elements (whose descendants may be skipped in
    /// forwards-compatible mode) from recognized ones.
    /// </summary>
    public static readonly HashSet<string> KnownXsltElementNames = new(StringComparer.Ordinal)
    {
        "stylesheet", "transform", "include", "import", "strip-space", "preserve-space",
        "output", "namespace-alias", "attribute-set", "decimal-format", "key", "mode",
        "accumulator", "variable", "param", "with-param", "template", "function",
        "global-context-item", "context-item", "use-package", "package", "expose",
        "import-schema",
        "apply-templates", "apply-imports", "call-template", "next-match",
        "value-of", "text", "element", "attribute", "namespace", "copy", "copy-of",
        "comment", "processing-instruction", "document", "result-document",
        "for-each", "for-each-group", "sort", "if", "choose", "when", "otherwise",
        "fallback", "message", "number", "sequence", "perform-sort",
        "analyze-string", "matching-substring", "non-matching-substring",
        "merge", "merge-source", "merge-key", "merge-action",
        "map", "map-entry", "array",
        "try", "catch", "evaluate", "source-document",
        "iterate", "break", "next-iteration", "on-completion",
        "where-populated", "on-empty", "on-non-empty", "assert"
    };

    /// <summary>
    /// Throws <c>XTSE0080</c> if the given lexical QName, when expanded in the context of
    /// <paramref name="element"/>, uses a reserved namespace (XSLT, XML Schema, or XPath
    /// functions). Used for names of named templates, attribute sets, and similar constructs.
    /// </summary>
    internal static void ValidateNamedQName(XElement element, string name, string construct)
    {
        if (string.IsNullOrEmpty(name))
            return;

        string? nsUri = null;
        string localName = name.Trim();
        var trimmed = localName;
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace >= 2)
            {
                nsUri = trimmed[2..closeBrace];
                localName = trimmed[(closeBrace + 1)..];
            }
        }
        else
        {
            int colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = trimmed[..colon];
                localName = trimmed[(colon + 1)..];
                if (prefix == "xml")
                    nsUri = "http://www.w3.org/XML/1998/namespace";
                else
                    nsUri = element.GetNamespaceOfPrefix(prefix)?.NamespaceName;
            }
        }

        // xsl:initial-template is the one XSLT-namespace name permitted for a named template.
        if ((nsUri == XsNamespace || nsUri == FnNamespace ||
             (nsUri == XslNamespace && localName != "initial-template")))
            throw new InvalidOperationException($"XTSE0080: The name '{name}' used in {construct} is in a reserved namespace.");
    }

    /// <summary>
    /// Expands an <see cref="XsQName"/> into Clark notation (<c>Q{namespace}local</c>).
    /// </summary>
    public static string ExpandQName(XsQName qname)
        => $"Q{{{qname.NamespaceUri}}}{qname.LocalName}";

    /// <summary>
    /// Expands a lexical QName in the context of <paramref name="element"/> and returns
    /// it in Clark notation (<c>Q{namespace}local</c>). Supports <c>Q{{uri}}local</c>,
    /// <c>prefix:local</c>, and unprefixed names.
    /// </summary>
    internal static string ExpandQName(XElement element, string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var trimmed = name.Trim();
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace >= 2)
            {
                var nsUri = trimmed[2..closeBrace];
                var localName = trimmed[(closeBrace + 1)..];
                return $"Q{{{nsUri}}}{localName}";
            }
        }

        int colon = trimmed.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = trimmed[..colon];
            var localName = trimmed[(colon + 1)..];
            if (prefix == "xml")
                return $"Q{{http://www.w3.org/XML/1998/namespace}}{localName}";

            var nsUri = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
            return $"Q{{{nsUri}}}{localName}";
        }

        return $"Q{{}}{trimmed}";
    }

    /// <summary>
    /// Returns the effective xpath-default-namespace for the given element by walking
    /// the ancestor chain and finding the nearest xpath-default-namespace attribute.
    /// </summary>
    internal static string? GetXPathDefaultNamespace(XElement element)
    {
        var current = element;
        while (current != null)
        {
            var attr = current.Attribute(XName.Get("xpath-default-namespace", XslNamespace));
            if (attr != null)
            {
                // XTSE0090: xsl:xpath-default-namespace is not allowed on XSLT elements
                if (current.Name.NamespaceName == XslNamespace)
                    throw new InvalidOperationException("XTSE0090");
                return attr.Value;
            }
            if (current.Name.NamespaceName == XslNamespace)
            {
                attr = current.Attribute("xpath-default-namespace");
                if (attr != null) return attr.Value;
            }
            current = current.Parent;
        }
        return null;
    }

}

/// <summary>
/// Represents a parsed xsl:decimal-format declaration.
/// </summary>
public sealed class DecimalFormatDefinition
{
    public string LocalName { get; init; } = "";
    public string NamespaceUri { get; init; } = "";
    public DecimalFormat Format { get; init; } = new();
    /// <summary>Attributes explicitly set on this xsl:decimal-format element.</summary>
    public HashSet<string> ExplicitAttributes { get; init; } = new();

    public static DecimalFormatDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var format = new DecimalFormat();
        var explicitAttrs = new HashSet<string>();
        string? localName = null;
        string? nsUri = null;

        foreach (var attr in element.Attributes())
        {
            var name = attr.Name.LocalName;
            var value = attr.Value;

            switch (name)
            {
                case "name":
                    {
                        var nameVal = value.Trim();
                        if (string.IsNullOrEmpty(nameVal)) break;
                        // Resolve QName using element's in-scope namespaces first
                        int colon = nameVal.IndexOf(':');
                        if (colon >= 0)
                        {
                            var prefix = nameVal.Substring(0, colon);
                            localName = nameVal.Substring(colon + 1);
                            nsUri = element.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix) ?? "";
                        }
                        else
                        {
                            localName = nameVal;
                            nsUri = "";
                        }
                        break;
                    }
                case "decimal-separator": format.DecimalSeparator = value; explicitAttrs.Add(name); break;
                case "grouping-separator": format.GroupingSeparator = value; explicitAttrs.Add(name); break;
                case "infinity": format.Infinity = value; explicitAttrs.Add(name); break;
                case "minus-sign": format.MinusSign = value; explicitAttrs.Add(name); break;
                case "NaN": format.NaN = value; explicitAttrs.Add(name); break;
                case "percent": format.Percent = value; explicitAttrs.Add(name); break;
                case "per-mille": format.PerMille = value; explicitAttrs.Add(name); break;
                case "zero-digit": format.ZeroDigit = value; explicitAttrs.Add(name); break;
                case "digit": format.Digit = value; explicitAttrs.Add(name); break;
                case "pattern-separator": format.PatternSeparator = value; explicitAttrs.Add(name); break;
                case "exponent-separator": format.ExponentSeparator = value; explicitAttrs.Add(name); break;
            }
        }

        // Validate symbol uniqueness (XTSE1300): only check attributes explicitly set on THIS element
        var explicitSymbols = new Dictionary<string, string>();
        foreach (var attr in explicitAttrs)
        {
            string? sym = attr switch
            {
                "decimal-separator" => format.DecimalSeparator,
                "grouping-separator" => format.GroupingSeparator,
                "percent" => format.Percent,
                "per-mille" => format.PerMille,
                "zero-digit" => format.ZeroDigit,
                "digit" => format.Digit,
                "pattern-separator" => format.PatternSeparator,
                "minus-sign" => format.MinusSign,
                _ => null
            };
            if (!string.IsNullOrEmpty(sym))
                explicitSymbols[attr] = sym;
        }
        foreach (var (r1, v1) in explicitSymbols)
        {
            foreach (var (r2, v2) in explicitSymbols)
            {
                if (r1 == r2) continue;
                if (v1 == v2)
                    throw new InvalidOperationException("XTSE1300");
            }
        }

        // Validate zero-digit is actually a digit (XTSE1295)
        if (explicitAttrs.Contains("zero-digit") && !string.IsNullOrEmpty(format.ZeroDigit))
        {
            var category = format.ZeroDigit.Length == 1
                ? char.GetUnicodeCategory(format.ZeroDigit[0])
                : char.GetUnicodeCategory(format.ZeroDigit, 0);
            if (category != System.Globalization.UnicodeCategory.DecimalDigitNumber)
                throw new InvalidOperationException("XTSE1295");
        }

        return new DecimalFormatDefinition
        {
            LocalName = localName ?? "",
            NamespaceUri = nsUri ?? "",
            Format = format,
            ExplicitAttributes = explicitAttrs
        };
    }
}

/// <summary>
/// Helper methods for stylesheet parsing.
/// </summary>
public static class StylesheetExtensions
{
    /// <summary>
    /// Resolves a namespace prefix in the stylesheet's root element.
    /// </summary>
    public static string? ResolveNamespace(this Stylesheet stylesheet, string prefix)
    {
        var root = stylesheet.RootElement;
        foreach (var attr in root.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                var attrPrefix = attr.Name.LocalName;
                if (attrPrefix == "xmlns" && string.IsNullOrEmpty(prefix))
                    return attr.Value;
                if (attrPrefix == prefix)
                    return attr.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Resolves a namespace prefix using the element's own and ancestor namespace declarations.
    /// </summary>
    public static string? ResolveNamespace(this XElement element, string prefix)
    {
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    var attrPrefix = attr.Name.LocalName;
                    if (attrPrefix == "xmlns" && string.IsNullOrEmpty(prefix))
                        return attr.Value;
                    if (attrPrefix == prefix)
                        return attr.Value;
                }
            }
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Expands a mode name with optional namespace prefix to Clark notation.
    /// </summary>
    private static string ExpandModeName(string mode, XElement element)
    {
        if (mode == "#current" || mode == "#default" || mode == "#all" || mode == "#unnamed")
            return mode;

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return mode;

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        var current = element;
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
        return mode;
    }
}

/// <summary>
/// The kind of name test stored in an <see cref="SpaceHandlingRule"/>.
/// </summary>
public enum SpaceNameTestKind
{
    /// <summary>Matches any element name.</summary>
    Any,
    /// <summary>Matches an exact QName.</summary>
    Exact,
    /// <summary>Matches any element with a specific local name regardless of namespace.</summary>
    WildcardLocal,
    /// <summary>Matches any element within a specific namespace.</summary>
    WildcardNamespace
}

/// <summary>
/// Represents a single xsl:strip-space or xsl:preserve-space rule.
/// </summary>
public readonly struct SpaceHandlingRule
{
    public SpaceNameTestKind Kind { get; }
    public string? LocalName { get; }
    public string? NamespaceUri { get; }
    public bool IsStrip { get; }
    public int Precedence { get; }

    private SpaceHandlingRule(SpaceNameTestKind kind, string? localName, string? namespaceUri, bool isStrip, int precedence)
    {
        Kind = kind;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        IsStrip = isStrip;
        Precedence = precedence;
    }

    /// <summary>
    /// Parses a name test from an <c>@elements</c> value into a <see cref="SpaceHandlingRule"/>.
    /// </summary>
    public static SpaceHandlingRule FromNameTest(string nameTest, XElement declaration, Stylesheet stylesheet, string? defaultNamespace, bool isStrip, int precedence)
    {
        var nt = nameTest.Trim();

        // EQName syntax: Q{namespace-uri}local or Q{namespace-uri}*
        if (nt.StartsWith("Q{"))
        {
            int closeBrace = nt.IndexOf('}');
            if (closeBrace < 2)
                throw new InvalidOperationException($"XTSE0270: Invalid name test '{nameTest}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            var nsUri = nt[2..closeBrace];
            var rest = nt[(closeBrace + 1)..];
            if (rest == "*")
                return new SpaceHandlingRule(SpaceNameTestKind.WildcardNamespace, null, nsUri, isStrip, precedence);
            if (rest.Length == 0)
                throw new InvalidOperationException($"XTSE0270: Invalid name test '{nameTest}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.Exact, rest, nsUri, isStrip, precedence);
        }

        if (nt == "*")
        {
            return new SpaceHandlingRule(SpaceNameTestKind.Any, null, null, isStrip, precedence);
        }

        if (nt.StartsWith("*:"))
        {
            return new SpaceHandlingRule(SpaceNameTestKind.WildcardLocal, nt[2..], null, isStrip, precedence);
        }

        if (nt.EndsWith(":*"))
        {
            var prefix = nt[..^2];
            var nsUri = declaration.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix);
            if (string.IsNullOrEmpty(nsUri))
                throw new InvalidOperationException($"XTSE0280: Undeclared prefix '{prefix}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.WildcardNamespace, null, nsUri, isStrip, precedence);
        }

        int colon = nt.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = nt[..colon];
            var localName = nt[(colon + 1)..];
            var nsUri = declaration.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix);
            if (string.IsNullOrEmpty(nsUri))
                throw new InvalidOperationException($"XTSE0280: Undeclared prefix '{prefix}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.Exact, localName, nsUri, isStrip, precedence);
        }

        // Unprefixed NCName: use the default namespace if one is in scope.
        return new SpaceHandlingRule(SpaceNameTestKind.Exact, nt, defaultNamespace, isStrip, precedence);
    }
}
