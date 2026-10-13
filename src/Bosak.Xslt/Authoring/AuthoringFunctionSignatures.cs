// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Capture immutable function parameter identities for authoring keyword validation.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Retain optional ranges by winning arity and defer conditional parameters.                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Frozen;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Standard.Functions;
using XsltStylesheet = Bosak.Xslt.Stylesheet.Stylesheet;

namespace Bosak.Xslt.Authoring;

/// <summary>Immutable function signatures captured from retained declaration contexts.</summary>
internal sealed class AuthoringFunctionSignatures
{
    private readonly FrozenDictionary<(string Namespace, string Name, int Arity), IReadOnlyList<string>> _signatures;
    private readonly FrozenSet<(string Namespace, string Name)> _uncertain;
    private readonly FrozenDictionary<(string Namespace, string Name, int Arity), int> _optional;
    private readonly string? _graphGap;

    private AuthoringFunctionSignatures(
        Dictionary<(string, string, int), IReadOnlyList<string>> signatures,
        HashSet<(string, string)> uncertain,
        Dictionary<(string, string, int), int> optional, string? graphGap)
    {
        _signatures = signatures.ToFrozenDictionary();
        _uncertain = uncertain.ToFrozenSet();
        _optional = optional.ToFrozenDictionary();
        _graphGap = graphGap;
    }

    internal IReadOnlyList<string>? Resolve(string namespaceUri, string localName, int arity)
    {
        // Standard keyword defaults remain independent of stylesheet graph coverage.
        if (FunctionLibrary.TryGetKeywordSignature(namespaceUri, localName, out _)) return null;
        var name = (namespaceUri, localName);
        if (_graphGap is not null || _uncertain.Contains(name))
            throw new AuthoringFunctionSignatureCoverageException(_graphGap ?? "Conditional, shadow or ambiguous function signatures remain outside keyword validation coverage.");
        if (_signatures.TryGetValue((namespaceUri, localName, arity), out var parameters)) return parameters;
        if (_optional.Any(p => p.Key.Namespace == namespaceUri && p.Key.Name == localName && arity >= p.Value && arity < p.Key.Arity))
            throw new AuthoringFunctionSignatureCoverageException("Omitted optional XSLT function arguments remain outside keyword validation coverage.");
        return null;
    }

    internal static AuthoringFunctionSignatures Capture(
        IReadOnlyList<(XElement Root, string BaseUri, int Precedence)> modules,
        IReadOnlyList<AuthoringModuleDescriptor> descriptors, string? graphGap)
    {
        var contexts = new Dictionary<XElement, ExpressionSlotContext>();
        void Visit(AuthoringNodeDescriptor node)
        {
            if (node.BackingObject is XElement element)
            {
                var context = node.Attributes.FirstOrDefault(a => a.LocalName == "name" && a.NamespaceUri.Length == 0)?.DeclarationContext;
                if (context is not null) contexts[element] = context;
            }
            foreach (var child in node.Children) Visit(child);
        }
        foreach (var descriptor in descriptors) Visit(descriptor.Root);
        var signatures = new Dictionary<(string, string, int), IReadOnlyList<string>>();
        var ranks = new Dictionary<(string, string, int), int>();
        var uncertain = new HashSet<(string, string)>();
        var optional = new Dictionary<(string, string, int), int>();
        foreach (var (root, _, precedence) in modules)
        {
            if (root.Name.LocalName == "package" || root.Elements().Any(e => e.Name == XName.Get("use-package", XsltStylesheet.XslNamespace)))
                graphGap ??= "Package function keyword linkage remains outside authoring coverage.";
            var rootUseWhen = root.Attribute("use-when")?.Value.Trim();
            if (rootUseWhen is not null && rootUseWhen is not ("true" or "true()"))
                graphGap ??= "Conditional module signatures remain outside authoring coverage.";
            foreach (var function in root.Elements(XName.Get("function", XsltStylesheet.XslNamespace)))
            {
                var useWhen = function.Attribute("use-when")?.Value.Trim();
                if (useWhen is "false" or "false()") continue;
                if (!contexts.TryGetValue(function, out var context) || function.Attribute("name") is not { } nameAttribute)
                {
                    graphGap ??= "Shadow or missing function names remain outside keyword signature capture.";
                    continue;
                }
                (string Namespace, string Name) name;
                try
                {
                    var text = nameAttribute.Value.Trim();
                    var hash = text.LastIndexOf('#');
                    if (hash > 0 && int.TryParse(text[(hash + 1)..], out var declaredArity) && declaredArity >= 0) text = text[..hash];
                    name = ExpandName(text, context);
                }
                catch (InvalidOperationException) { continue; } // Declaration diagnostics are owned by the validator.
                var parameters = function.Elements(XName.Get("param", XsltStylesheet.XslNamespace))
                    .Where(p => p.Attribute("use-when")?.Value.Trim() is not ("false" or "false()")).ToArray();
                bool HasShadow(XElement element) => element.Attributes().Any(a => a.Name.NamespaceName.Length == 0 && a.Name.LocalName.StartsWith('_'));
                bool HasConditionalParameter(XElement parameter) => parameter.Attribute("use-when") is { } condition && condition.Value.Trim() is not ("true" or "true()");
                if ((useWhen is not null && useWhen is not ("true" or "true()")) || HasShadow(function) || parameters.Any(p => HasShadow(p) || HasConditionalParameter(p)))
                {
                    uncertain.Add(name);
                    continue;
                }
                var key = (name.Namespace, name.Name, parameters.Length);
                if (ranks.TryGetValue(key, out var rank) && rank < precedence) continue;
                if (ranks.TryGetValue(key, out rank) && rank == precedence)
                {
                    uncertain.Add(name);
                    continue;
                }
                try
                {
                    var names = parameters.Select(p =>
                    {
                        if (!contexts.TryGetValue(p, out var parameterContext) || p.Attribute("name") is not { } parameterName)
                            throw new InvalidOperationException("A parameter name is unavailable.");
                        var expanded = ExpandName(parameterName.Value.Trim(), parameterContext);
                        return $"Q{{{expanded.Namespace}}}{expanded.Name}";
                    }).ToArray();
                    signatures[key] = Array.AsReadOnly(names);
                    ranks[key] = precedence;
                    var minimum = parameters.Count(p => p.Attribute("required")?.Value is not ("no" or "false" or "0"));
                    optional.Remove(key);
                    if (minimum < parameters.Length) optional[key] = minimum;
                }
                catch (InvalidOperationException) { uncertain.Add(name); }
            }
        }
        return new(signatures, uncertain, optional, graphGap);
    }

    private static (string Namespace, string Name) ExpandName(string text, ExpressionSlotContext context)
    {
        if (context.StaticNamespaceError is not null) throw new InvalidOperationException(context.StaticNamespaceError);
        if (text.StartsWith("Q{", StringComparison.Ordinal))
        {
            var close = text.IndexOf('}');
            if (close >= 2 && XsltStylesheet.IsValidNCName(text[(close + 1)..])) return (text[2..close], text[(close + 1)..]);
            throw new InvalidOperationException("Invalid EQName.");
        }
        var colon = text.IndexOf(':');
        var local = colon < 0 ? text : text[(colon + 1)..];
        if (!XsltStylesheet.IsValidNCName(local)) throw new InvalidOperationException("Invalid local name.");
        if (colon < 0) return (string.Empty, local);
        var prefix = text[..colon];
        if (!XsltStylesheet.IsValidNCName(prefix)) throw new InvalidOperationException("Invalid prefix.");
        string? uri = Stylesheet.Stylesheet.GetXPathCompatibility(context.EffectiveVersion) >= XPathCompatibility.XPath40
            ? context.StaticNamespaces?.GetValueOrDefault(prefix)
            : context.InScopeNamespaces.LastOrDefault(n => n.Prefix == prefix)?.Uri;
        if (string.IsNullOrEmpty(uri)) throw new InvalidOperationException("Unavailable namespace prefix.");
        return (uri, local);
    }
}

/// <summary>Signals a deferred host keyword signature without reporting an invalid expression.</summary>
internal sealed class AuthoringFunctionSignatureCoverageException(string message) : NotSupportedException(message);
