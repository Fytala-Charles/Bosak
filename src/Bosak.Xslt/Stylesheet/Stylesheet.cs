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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.IO;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;

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
    private readonly HashSet<string> _includedUris = new();
    private readonly List<TemplateRule> _templateRules = new();
    private readonly Dictionary<string, TemplateRule> _namedTemplates = new();
    private readonly List<Stylesheet> _imports = new();
    private readonly List<Stylesheet> _includes = new();
    private readonly List<KeyDefinition> _keyDefinitions = new();
    private readonly List<AccumulatorDefinition> _accumulators = new();
    private readonly List<XElement> _globalVariables = new();
    private readonly List<XElement> _globalParameters = new();
    private readonly List<SpaceHandlingRule> _stripSpaceRules = new();
    private readonly List<SpaceHandlingRule> _preserveSpaceRules = new();
    private readonly Dictionary<string, ModeDefinition> _modeDefinitions = new();
    private readonly List<XsltFunctionDefinition> _functionDefinitions = new();
    private readonly List<DecimalFormatDefinition> _decimalFormats = new();
    private readonly List<AttributeSetDefinition> _attributeSets = new();
    private readonly HashSet<string> _excludedResultPrefixes = new();
    private OutputProperties? _outputProperties;
    private readonly bool _isRootStylesheet;

    public Stylesheet(XDocument document, string? baseUri, IXsltUriResolver resolver, int importPrecedence = 0, HashSet<string>? resolvedUris = null)
    {
        _document = document;
        _baseUri = baseUri;
        _resolver = resolver;
        ImportPrecedence = importPrecedence;
        _resolvedUris = resolvedUris ?? new HashSet<string>();
        _isRootStylesheet = _resolvedUris.Count == 0;

        // Add this stylesheet's own URI to the resolved set for circular-reference detection
        if (!string.IsNullOrEmpty(baseUri))
            _resolvedUris.Add(baseUri);

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
    public int ImportPrecedence { get; }

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
        foreach (var rule in _templateRules)
            yield return rule;

        foreach (var included in _includes)
        {
            foreach (var rule in included.GetAllTemplateRules())
                yield return rule;
        }

        foreach (var imported in _imports)
        {
            foreach (var rule in imported.GetAllTemplateRules())
                yield return rule;
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

        // Required version attribute (XTSE0010)
        var versionAttr = root.Attribute("version");
        if (versionAttr == null || string.IsNullOrWhiteSpace(versionAttr.Value))
            throw new InvalidOperationException("XTSE0010: The version attribute is required on xsl:stylesheet or xsl:transform.");

        Version = versionAttr.Value;

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

        // Helper to evaluate use-when on top-level elements
        bool UseWhen(XElement elem)
        {
            var useWhen = GetUseWhenAttribute(elem);
            if (string.IsNullOrEmpty(useWhen))
                return true;
            try
            {
                var compiled = XPath31Expression.Compile(useWhen);
                var ctx = new Bosak.XPath.Runtime.Vm.EvaluationContext();
                Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(ctx);
                // Add in-scope namespace declarations so prefixes in use-when resolve correctly
                foreach (var attr in elem.Attributes().Where(a => a.IsNamespaceDeclaration))
                {
                    var prefix = attr.Name.LocalName;
                    if (prefix == "xmlns") prefix = "";
                    ctx.WithNamespace(prefix, attr.Value);
                }
                var result = compiled.Evaluate(ctx);
                return result.EffectiveBooleanValue();
            }
            catch
            {
                return true; // If evaluation fails, include the element (fail-safe)
            }
        }

        /// <summary>
        /// Gets the value of the <c>use-when</c> attribute, checking both the no-namespace
        /// form (used on XSLT elements) and the <c>xsl:use-when</c> form (used on LREs).
        /// </summary>
        static string? GetUseWhenAttribute(XElement elem)
        {
            // On XSLT elements, use-when has no namespace
            var attr = elem.Attribute("use-when");
            if (attr != null)
                return attr.Value;
            // On literal result elements, use-when must be in the XSLT namespace
            attr = elem.Attribute(XName.Get("use-when", XslNamespace));
            if (attr != null)
                return attr.Value;
            return null;
        }

        /// <summary>
        /// Recursively strips elements whose <c>use-when</c> attribute evaluates to <c>false()</c>.
        /// This is applied to the entire stylesheet tree, including nested elements inside
        /// template bodies, so that <c>use-when</c> on instructions and LREs is respected.
        /// </summary>
        void StripUseWhenElements(XElement parent)
        {
            // Process children first (depth-first) so we strip descendants before
            // deciding whether to strip the parent. Then process the parent's children
            // in a separate pass to avoid modifying a collection while iterating.
            var children = parent.Elements().ToList();
            foreach (var child in children)
            {
                StripUseWhenElements(child);
            }

            // Now remove any direct children whose use-when is false
            foreach (var child in children)
            {
                if (!UseWhen(child))
                {
                    child.Remove();
                }
            }
        }

        // Process xsl:import elements (must come first per spec)
        foreach (var import in root.Elements(XName.Get("import", XslNamespace)))
        {
            if (!UseWhen(import)) continue;
            var href = import.Attribute("href")?.Value;
            if (!string.IsNullOrEmpty(href))
                ResolveImport(href);
        }

        // Process xsl:include elements
        foreach (var include in root.Elements(XName.Get("include", XslNamespace)))
        {
            if (!UseWhen(include)) continue;
            var href = include.Attribute("href")?.Value;
            if (!string.IsNullOrEmpty(href))
                ResolveInclude(href);
        }

        // Apply use-when stripping to the entire tree (nested elements inside templates,
        // literal result elements, etc.) after imports/includes are resolved.
        StripUseWhenElements(root);

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

        // Parse xsl:strip-space and xsl:preserve-space declarations
        foreach (var strip in root.Elements(XName.Get("strip-space", XslNamespace)))
        {
            if (!UseWhen(strip)) continue;
            var elements = strip.Attribute("elements")?.Value;
            var stripDefaultNs = GetXPathDefaultNamespace(strip);
            if (!string.IsNullOrEmpty(elements))
            {
                foreach (var nameTest in elements.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var (resolvedName, nsUri) = ResolveNameTest(nameTest, stripDefaultNs);
                    _stripSpaceRules.Add(new SpaceHandlingRule(resolvedName, isStrip: true, ImportPrecedence, nsUri));
                }
            }
        }
        foreach (var preserve in root.Elements(XName.Get("preserve-space", XslNamespace)))
        {
            if (!UseWhen(preserve)) continue;
            var elements = preserve.Attribute("elements")?.Value;
            var preserveDefaultNs = GetXPathDefaultNamespace(preserve);
            if (!string.IsNullOrEmpty(elements))
            {
                foreach (var nameTest in elements.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var (resolvedName, nsUri) = ResolveNameTest(nameTest, preserveDefaultNs);
                    _preserveSpaceRules.Add(new SpaceHandlingRule(resolvedName, isStrip: false, ImportPrecedence, nsUri));
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

        // Parse xsl:output (first one wins per spec)
        var outputElem = root.Elements(XName.Get("output", XslNamespace)).FirstOrDefault(UseWhen);
        if (outputElem != null)
            _outputProperties = OutputProperties.FromElement(outputElem);

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
                    _namedTemplates[firstNamed.Name] = firstNamed;
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
    /// Performs static validation of the stylesheet tree, checking for disallowed
    /// attributes and children on XSLT instructions.
    /// </summary>
    private void ValidateInstructionTree(XElement root)
    {
        foreach (var elem in root.DescendantsAndSelf())
        {
            if (elem.Name.NamespaceName != XslNamespace)
                continue;

            var localName = elem.Name.LocalName;

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
                    var allNamed = GetAllNamedTemplates();
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
                        baseName != "identity-sensitive")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    if (!attrName.StartsWith("_"))
                    {
                        if (baseName == "override" || baseName == "override-extension-function" ||
                            baseName == "identity-sensitive")
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
    private static (string LocalName, string NamespaceUri) ExpandVariableName(XElement element, string name)
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

    private void ResolveImport(string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            _imports.Add(new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence + 1, childResolvedUris));
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to resolve xsl:import href '{href}'.", ex);
        }
    }

    private void ResolveInclude(string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        // XSLT allows the same stylesheet to be included multiple times;
        // subsequent includes are silently ignored.
        if (_includedUris.Contains(resolvedUri))
            return;

        // Circular reference detection: if this URI is already in the ancestor chain,
        // including it would create a cycle.
        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        _includedUris.Add(resolvedUri);
        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            _includes.Add(new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence, childResolvedUris));
        }
        catch (FileNotFoundException ex)
        {
            _includedUris.Remove(resolvedUri);
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

    /// <summary>The parsed xsl:output properties, or null if not specified.</summary>
    public OutputProperties? OutputProperties => _outputProperties;

    /// <summary>Top-level xsl:param elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalParameters => _globalParameters;

    /// <summary>Top-level xsl:variable elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalVariables => _globalVariables;

    /// <summary>All key definitions defined in this stylesheet.</summary>
    public IReadOnlyList<KeyDefinition> KeyDefinitions => _keyDefinitions;

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
        result.AddRange(_stripSpaceRules);
        result.AddRange(_preserveSpaceRules);
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

    /// <summary>The version attribute of the stylesheet root element.</summary>
    public string? Version { get; private set; }

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

    /// <summary>
    /// Resolves a name test string using the given default namespace.
    /// Returns the resolved name test and the namespace URI (if any).
    /// </summary>
    private static (string ResolvedName, string? NamespaceUri) ResolveNameTest(string nameTest, string? defaultNamespace)
    {
        if (nameTest == "*" || nameTest.Contains(':'))
            return (nameTest, null);
        if (!string.IsNullOrEmpty(defaultNamespace))
            return ($"Q{{{defaultNamespace}}}{nameTest}", defaultNamespace);
        return (nameTest, null);
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
/// Represents a single xsl:strip-space or xsl:preserve-space rule.
/// </summary>
public readonly struct SpaceHandlingRule
{
    public string NameTest { get; }
    public string? NamespaceUri { get; }
    public bool IsStrip { get; }
    public int Precedence { get; }

    public SpaceHandlingRule(string nameTest, bool isStrip, int precedence, string? namespaceUri = null)
    {
        NameTest = nameTest;
        IsStrip = isStrip;
        Precedence = precedence;
        NamespaceUri = namespaceUri;
    }
}
