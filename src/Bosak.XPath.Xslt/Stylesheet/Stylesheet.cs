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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.IO;
using System.Xml.Linq;
using Bosak.XPath.Xslt.Api;

namespace Bosak.XPath.Xslt.Stylesheet;

/// <summary>
/// Represents a loaded XSLT stylesheet, including all imported and included modules.
/// </summary>
public sealed class Stylesheet
{
    private readonly XDocument _document;
    private readonly string? _baseUri;
    private readonly IXsltUriResolver _resolver;
    private readonly HashSet<string> _resolvedUris;
    private readonly List<TemplateRule> _templateRules = new();
    private readonly Dictionary<string, TemplateRule> _namedTemplates = new();
    private readonly List<Stylesheet> _imports = new();
    private readonly List<Stylesheet> _includes = new();
    private readonly List<KeyDefinition> _keyDefinitions = new();
    private OutputProperties? _outputProperties;

    public Stylesheet(XDocument document, string? baseUri, IXsltUriResolver resolver, int importPrecedence = 0, HashSet<string>? resolvedUris = null)
    {
        _document = document;
        _baseUri = baseUri;
        _resolver = resolver;
        ImportPrecedence = importPrecedence;
        _resolvedUris = resolvedUris ?? new HashSet<string>();
        Load();
    }

    /// <summary>The root element of the stylesheet (xsl:stylesheet or xsl:transform).</summary>
    public XElement Root => _document.Root!;

    /// <summary>All template rules defined in this stylesheet, ordered by priority.</summary>
    public IReadOnlyList<TemplateRule> TemplateRules => _templateRules;

    /// <summary>Named templates indexed by name.</summary>
    public IReadOnlyDictionary<string, TemplateRule> NamedTemplates => _namedTemplates;

    /// <summary>Stylesheets imported via xsl:import (lower precedence).</summary>
    public IReadOnlyList<Stylesheet> Imports => _imports;

    /// <summary>Stylesheets included via xsl:include (same precedence).</summary>
    public IReadOnlyList<Stylesheet> Includes => _includes;

    /// <summary>The import precedence of this stylesheet (0 = main, higher = deeper import).</summary>
    public int ImportPrecedence { get; }

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

        // Process xsl:import elements (must come first per spec)
        foreach (var import in root.Elements(XName.Get("import", XslNamespace)))
        {
            var href = import.Attribute("href")?.Value;
            if (!string.IsNullOrEmpty(href))
                ResolveImport(href);
        }

        // Process xsl:include elements
        foreach (var include in root.Elements(XName.Get("include", XslNamespace)))
        {
            var href = include.Attribute("href")?.Value;
            if (!string.IsNullOrEmpty(href))
                ResolveInclude(href);
        }

        // Parse xsl:key declarations
        foreach (var key in root.Elements(XName.Get("key", XslNamespace)))
        {
            var def = KeyDefinition.FromElement(key, this);
            if (def != null)
                _keyDefinitions.Add(def);
        }

        // Parse xsl:output (first one wins per spec)
        var outputElem = root.Element(XName.Get("output", XslNamespace));
        if (outputElem != null)
            _outputProperties = OutputProperties.FromElement(outputElem);

        // Collect template rules from this stylesheet
        foreach (var template in root.Elements(XName.Get("template", XslNamespace)))
        {
            var rule = TemplateRule.FromElement(template, this);
            if (rule != null)
            {
                if (!string.IsNullOrEmpty(rule.Match))
                    _templateRules.Add(rule);
                if (!string.IsNullOrEmpty(rule.Name))
                    _namedTemplates[rule.Name] = rule;
            }
        }
    }

    private void ResolveImport(string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        _resolvedUris.Add(resolvedUri);

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            _imports.Add(new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence + 1, _resolvedUris));
        }
        catch (FileNotFoundException)
        {
            // Silently ignore missing imports per common XSLT processor behavior.
            _resolvedUris.Remove(resolvedUri);
        }
    }

    private void ResolveInclude(string href)
    {
        var resolvedUri = ResolveAbsoluteUri(href, _baseUri);

        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        _resolvedUris.Add(resolvedUri);

        try
        {
            var doc = _resolver.Resolve(href, _baseUri);
            _includes.Add(new Stylesheet(doc, resolvedUri, _resolver, ImportPrecedence, _resolvedUris));
        }
        catch (FileNotFoundException)
        {
            // Silently ignore missing includes per common XSLT processor behavior.
            _resolvedUris.Remove(resolvedUri);
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

    /// <summary>All key definitions defined in this stylesheet.</summary>
    public IReadOnlyList<KeyDefinition> KeyDefinitions => _keyDefinitions;

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

    /// <summary>The XSLT namespace URI.</summary>
    public const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";
}
