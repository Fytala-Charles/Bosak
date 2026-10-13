// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Capture linked named types for source-preserving authoring validation.
// SPECIAL NOTES        : Part of the source-preserving XSLT authoring pipeline.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-10-2026     | Link captured named types using include and import precedence.                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;
using Bosak.Xslt.Api;
using XsltStylesheet = Bosak.Xslt.Stylesheet.Stylesheet;

namespace Bosak.Xslt.Authoring;

/// <summary>Immutable named-type bindings captured without compiling the stylesheet.</summary>
internal sealed class AuthoringNamedTypes
{
    internal IReadOnlyDictionary<string, string>? Aliases { get; private init; }
    internal IReadOnlySet<string>? Records { get; private init; }
    internal string? Error { get; private init; }
    internal XElement? ErrorElement { get; private init; }
    internal string? CoverageGap { get; private init; }

    internal bool ModuleLinkageSupported { get; private init; }

    internal static AuthoringNamedTypes Capture(
        IReadOnlyList<(XElement Root, string BaseUri, int Precedence)> modules,
        IXsltUriResolver resolver, string? graphGap)
    {
        if (graphGap is not null) return new() { CoverageGap = modules.Any(m => m.Root.Elements().Any(e => e.Name.NamespaceName == XsltStylesheet.XslNamespace && e.Name.LocalName is "item-type" or "record-type")) ? graphGap : null };
        if (modules.Count == 0) return new() { ModuleLinkageSupported = true };
        var elements = modules.SelectMany(m => m.Root.Elements().Where(e => e.Name.NamespaceName == XsltStylesheet.XslNamespace
            && e.Name.LocalName is "item-type" or "record-type").Select(e => (Element: e, m.BaseUri, m.Precedence))).ToList();
        if (elements.Count == 0) return new() { ModuleLinkageSupported = true };
        foreach (var module in modules)
        {
            var rootUseWhen = module.Root.Attribute("use-when")?.Value.Trim();
            if (rootUseWhen is not null && rootUseWhen is not ("true()" or "true"))
                return new() { CoverageGap = "Non-literal module-root static exclusion remains outside linked named-type capture." };
            if (module.Root.Name.LocalName == "package" || module.Root.Elements().Any(e => e.Name.NamespaceName == XsltStylesheet.XslNamespace && e.Name.LocalName is "use-package" or "import-schema"))
                return new() { CoverageGap = "Named-type package and schema linkage remains outside authoring coverage." };
        }
        var declarations = new Dictionary<string, XElement>(StringComparer.Ordinal);
        var candidates = new Dictionary<string, List<(int Precedence, XElement Element)>>(StringComparer.Ordinal);
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        var records = new HashSet<string>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        XElement current = modules[0].Root;
        try
        {
            foreach (var (element, baseUri, precedence) in elements)
            {
                current = element;
                if (!XsltStylesheet.UsesXPath40Namespaces(element)) continue;
                var useWhen = element.Attribute("use-when")?.Value.Trim();
                if (useWhen is "false()" or "false") continue;
                if (useWhen is not null && useWhen is not ("true()" or "true"))
                    return new() { CoverageGap = "Named-type registration with non-literal use-when is outside authoring coverage." };
                if (element.Attributes().Any(a => a.Name.NamespaceName.Length == 0 && a.Name.LocalName.StartsWith('_')))
                    return new() { CoverageGap = "Shadow named-type declarations are not evaluated during authoring inspection." };
                _ = XsltStylesheet.GetApplicableStaticNamespaces(element, resolver, baseUri);
                var name = element.Attribute("name")?.Value.Trim();
                if (string.IsNullOrEmpty(name) || (element.Name.LocalName == "item-type" && string.IsNullOrWhiteSpace(element.Attribute("as")?.Value)))
                    throw new InvalidOperationException("XTSE0010: Named types require a name; item-type also requires as.");
                if (element.Attribute("visibility") is { } visibility && visibility.Value is not "public" and not "private")
                    throw new InvalidOperationException("XTSE0020: Named type visibility must be public or private.");
                var key = XsltStylesheet.ExpandItemTypeName(name, element);
                var uri = key[2..key.IndexOf('}')];
                if (uri is XsltStylesheet.XslNamespace or "http://www.w3.org/2001/XMLSchema" or "http://www.w3.org/2005/xpath-functions")
                    throw new InvalidOperationException("XTSE4030: Reserved namespace in item-type name.");
                if (!candidates.TryGetValue(key, out var definitions)) candidates[key] = definitions = new();
                definitions.Add((precedence, element));
            }
            foreach (var (name, definitions) in candidates)
            {
                int precedence = definitions.Min(d => d.Precedence);
                var winners = definitions.Where(d => d.Precedence == precedence).ToList();
                current = winners[0].Element;
                if (winners.Count != 1)
                    throw new InvalidOperationException("XTSE4030: Duplicate item-type declarations at the same import precedence.");
                declarations[name] = current;
                if (current.Name.LocalName == "record-type") records.Add(name);
            }
            string? Expand(string name)
            {
                if (aliases.TryGetValue(name, out var cached)) return cached;
                if (!declarations.TryGetValue(name, out var declaration) || records.Contains(name)) return null;
                current = declaration;
                if (!active.Add(name)) throw new InvalidOperationException("XTSE4035: Circular item-type alias definition.");
                try
                {
                    var expanded = ItemTypeAliasExpansion.Expand(declaration.Attribute("as")!.Value.Trim(), lexical =>
                    {
                        var key = XsltStylesheet.ExpandItemTypeName(lexical, declaration);
                        if (key.StartsWith("Q{http://www.w3.org/2001/XMLSchema}", StringComparison.Ordinal)) return "xs:" + key[(key.IndexOf('}') + 1)..];
                        if (!declarations.ContainsKey(key)) throw new InvalidOperationException($"XPST0051: Unknown item type '{lexical}' in an item-type definition.");
                        return records.Contains(key) ? key : Expand(key);
                    }, (nodeName, attribute) => XsltStylesheet.ExpandNodeTypeName(nodeName, attribute, declaration),
                        schemaName => ItemTypeAliasExpansion.ValidateSchemaTypeName(XsltStylesheet.ExpandSchemaTypeName(schemaName, declaration), null),
                        (schemaName, attribute) => ItemTypeAliasExpansion.ValidateSchemaDeclarationName(XsltStylesheet.ExpandSchemaDeclarationName(schemaName, attribute, declaration), attribute, null));
                    current = declaration;
                    if (expanded is null) throw new NotSupportedException("The item-type grammar is outside authoring registration coverage.");
                    if (XPathParser.Parse("() instance of " + expanded, xpath40: true) is not InstanceOfNode { Occurrence: OccurrenceIndicator.One })
                        throw new InvalidOperationException("XTSE0020: An item-type definition cannot have an occurrence indicator.");
                    XPath31Expression.Compile("() instance of " + expanded, new CompileOptions { Compatibility = XPathCompatibility.XPath40, NamedRecordTypeNames = records });
                    return aliases[name] = expanded;
                }
                finally { active.Remove(name); }
            }
            foreach (var name in declarations.Keys) Expand(name);
            return new() { Aliases = new ReadOnlyDictionary<string, string>(aliases), Records = records.ToFrozenSet(StringComparer.Ordinal), ModuleLinkageSupported = true };
        }
        catch (NotSupportedException ex) { return new() { CoverageGap = ex.Message }; }
        catch (Exception ex)
        {
            return new() { Error = ex is System.Xml.XmlException ? "XTSE0020: Invalid named-type QName." : ex.Message, ErrorElement = current };
        }
    }
}
