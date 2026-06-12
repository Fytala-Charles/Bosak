// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 juni 2026
// PURPOSE              : Represents an xsl:accumulator declaration and its rules.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a single <c>xsl:accumulator</c> declaration.
/// </summary>
public sealed class AccumulatorDefinition
{
    /// <summary>The original XElement of the xsl:accumulator declaration.</summary>
    public XElement Element { get; }

    /// <summary>The local name of the accumulator.</summary>
    public string LocalName { get; }

    /// <summary>The namespace URI of the accumulator (empty if unprefixed).</summary>
    public string NamespaceUri { get; }

    /// <summary>The accumulator name in Clark notation.</summary>
    public string ClarkName => string.IsNullOrEmpty(NamespaceUri) ? LocalName : $"{{{NamespaceUri}}}{LocalName}";

    /// <summary>The declared type of the accumulator value.</summary>
    public string? As { get; }

    /// <summary>The initial-value expression.</summary>
    public string InitialValue { get; }

    /// <summary>The accumulator rules, in declaration order.</summary>
    public IReadOnlyList<AccumulatorRule> Rules { get; }

    private AccumulatorDefinition(XElement element, string localName, string namespaceUri, string? asType, string initialValue, IReadOnlyList<AccumulatorRule> rules)
    {
        Element = element;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        As = asType;
        InitialValue = initialValue;
        Rules = rules;
    }

    /// <summary>
    /// Parses an <c>xsl:accumulator</c> element into an <see cref="AccumulatorDefinition"/>.
    /// </summary>
    public static AccumulatorDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(nameAttr))
            return null;

        var (localName, namespaceUri) = ResolveAccumulatorName(nameAttr, element);
        var initialValue = element.Attribute("initial-value")?.Value ?? "()";
        var asType = element.Attribute("as")?.Value;

        var rules = element.Elements(XName.Get("accumulator-rule", Stylesheet.XslNamespace))
            .Select(r => AccumulatorRule.FromElement(r, stylesheet))
            .Where(r => r != null)
            .Cast<AccumulatorRule>()
            .ToList();

        return new AccumulatorDefinition(element, localName, namespaceUri, asType, initialValue, rules);
    }

    private static (string LocalName, string NamespaceUri) ResolveAccumulatorName(string name, XElement element)
    {
        var colon = name.IndexOf(':');
        if (colon < 0)
            return (name, string.Empty);

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        var ns = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
        return (local, ns);
    }
}

/// <summary>
/// Represents a single <c>xsl:accumulator-rule</c> within an accumulator.
/// </summary>
public sealed class AccumulatorRule
{
    /// <summary>The original XElement of the accumulator rule.</summary>
    public XElement Element { get; }

    /// <summary>The match pattern.</summary>
    public string Match { get; }

    /// <summary>The select expression that computes the new accumulator value.</summary>
    public string? Select { get; }

    /// <summary>The rule phase ("start" or "end"), if specified.</summary>
    public string? Phase { get; }

    private AccumulatorRule(XElement element, string match, string? select, string? phase)
    {
        Element = element;
        Match = match;
        Select = select;
        Phase = phase;
    }

    /// <summary>
    /// Parses an <c>xsl:accumulator-rule</c> element.
    /// </summary>
    public static AccumulatorRule? FromElement(XElement element, Stylesheet stylesheet)
    {
        var match = element.Attribute("match")?.Value;
        if (string.IsNullOrEmpty(match))
            return null;

        var select = element.Attribute("select")?.Value;
        var phase = element.Attribute("phase")?.Value;
        return new AccumulatorRule(element, match, select, phase);
    }
}
