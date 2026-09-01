// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 mei 2026
// PURPOSE              : Represents a parsed xsl:mode declaration with on-no-match behavior.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 05-06-2026     | Added OnMultipleMatch enum and parsing for xsl:mode on-multiple-match                  |
//                      | Charles Korthout | 0.3   | 07-06-2026     | Added DeepSkip to OnNoMatch enum and parsing; fixes next-match-034                     |
//                      | Charles Korthout | 0.4   | 08-06-2026     | FromElement expands mode name QNames to Clark notation                                 |
//                      | Charles Korthout | 0.5   | 09-06-2026     | Added NormalizeModeName; trim whitespace from mode/on-no-match attribute values       |
//                      | Charles Korthout | 0.6   | 11-06-2026     | Added use-accumulators parsing                                                          |
//                      | Charles Korthout | 0.7   | 30-06-2026     | Default on-no-match is text-only-copy per XSLT 3.0 spec; fixes match-241               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Specifies the built-in template rule behavior when no explicit template matches a node.
/// </summary>
public enum OnNoMatch
{
    /// <summary>Shallow-copy the node.</summary>
    ShallowCopy,
    /// <summary>Skip the element node, apply-templates to children only.</summary>
    ShallowSkip,
    /// <summary>Copy only text nodes, ignore elements.</summary>
    TextOnlyCopy,
    /// <summary>Deep-copy the entire subtree.</summary>
    DeepCopy,
    /// <summary>Skip the element node and all descendants.</summary>
    DeepSkip,
    /// <summary>Throw an error when no template matches.</summary>
    Fail
}

/// <summary>
/// Specifies the behavior when multiple template rules match with the same priority.
/// </summary>
public enum OnMultipleMatch
{
    /// <summary>Use the last matching template (default in XSLT 3.0).</summary>
    UseLast,
    /// <summary>Throw an error when multiple templates have the same priority.</summary>
    Fail
}

/// <summary>
/// Specifies the visibility of an <c>xsl:mode</c> declaration.
/// </summary>
public enum ModeVisibility
{
    /// <summary>Visible to stylesheets that import this one.</summary>
    Public,
    /// <summary>Visible only within this stylesheet package.</summary>
    Private,
    /// <summary>Visible but cannot be overridden.</summary>
    Final,
    /// <summary>Must be overridden by an importing stylesheet.</summary>
    Abstract
}

/// <summary>
/// Represents a parsed xsl:mode declaration.
/// </summary>
public sealed class ModeDefinition
{
    /// <summary>The mode name (empty string for the default mode).</summary>
    public string Name { get; }

    /// <summary>The behavior when no template matches a node.</summary>
    public OnNoMatch OnNoMatch { get; }

    /// <summary>The behavior when multiple templates match with the same priority.</summary>
    public OnMultipleMatch OnMultipleMatch { get; }

    /// <summary>The visibility of the mode.</summary>
    public ModeVisibility Visibility { get; }

    /// <summary>Whether the mode requires typed (schema-validated) nodes.</summary>
    public bool Typed { get; }

    /// <summary>Whether to emit a warning when no template matches a node.</summary>
    public bool WarningOnNoMatch { get; }

    /// <summary>Whether to emit a warning when multiple templates match with the same priority.</summary>
    public bool WarningOnMultipleMatch { get; }

    /// <summary>Whether the mode is declared streamable.</summary>
    public bool Streamable { get; }

    /// <summary>The accumulator names (as Clark names) that are applicable to this mode.</summary>
    public IReadOnlySet<string> UseAccumulators { get; }

    /// <summary>Whether this mode uses all accumulators.</summary>
    public bool UseAllAccumulators { get; }

    public ModeDefinition(string name, OnNoMatch onNoMatch, OnMultipleMatch onMultipleMatch = OnMultipleMatch.UseLast)
        : this(name, onNoMatch, onMultipleMatch, ModeVisibility.Private, false, false, false, false, new HashSet<string>(), false)
    {
    }

    public ModeDefinition(string name, OnNoMatch onNoMatch, OnMultipleMatch onMultipleMatch, ModeVisibility visibility, bool typed, bool warningOnNoMatch, bool warningOnMultipleMatch, bool streamable, IReadOnlySet<string> useAccumulators, bool useAllAccumulators)
    {
        Name = name;
        OnNoMatch = onNoMatch;
        OnMultipleMatch = onMultipleMatch;
        Visibility = visibility;
        Typed = typed;
        WarningOnNoMatch = warningOnNoMatch;
        WarningOnMultipleMatch = warningOnMultipleMatch;
        Streamable = streamable;
        UseAccumulators = useAccumulators;
        UseAllAccumulators = useAllAccumulators;
    }

    /// <summary>
    /// Parses an xsl:mode element into a <see cref="ModeDefinition"/>.
    /// </summary>
    public static ModeDefinition? FromElement(XElement element, bool isPackage)
    {
        var name = ExpandModeName(element.Attribute("name")?.Value?.Trim() ?? "", element);
        var onNoMatch = element.Attribute("on-no-match")?.Value?.Trim()?.ToLowerInvariant() switch
        {
            "shallow-copy" => OnNoMatch.ShallowCopy,
            "shallow-skip" => OnNoMatch.ShallowSkip,
            "text-only-copy" => OnNoMatch.TextOnlyCopy,
            "deep-copy" => OnNoMatch.DeepCopy,
            "deep-skip" => OnNoMatch.DeepSkip,
            "fail" => OnNoMatch.Fail,
            _ => OnNoMatch.TextOnlyCopy
        };
        var onMultipleMatch = element.Attribute("on-multiple-match")?.Value?.Trim()?.ToLowerInvariant() switch
        {
            "fail" => OnMultipleMatch.Fail,
            _ => OnMultipleMatch.UseLast
        };

        var visibilityAttr = element.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant();
        var visibility = visibilityAttr switch
        {
            "public" => ModeVisibility.Public,
            "private" => ModeVisibility.Private,
            "final" => ModeVisibility.Final,
            "abstract" => ModeVisibility.Abstract,
            null or "" => string.IsNullOrEmpty(name)
                ? ModeVisibility.Private
                : isPackage ? ModeVisibility.Private : ModeVisibility.Public,
            _ => throw new InvalidOperationException("XTSE0020")
        };

        // The unnamed mode can only be private; public/final/abstract are not allowed.
        if (string.IsNullOrEmpty(name) && visibility != ModeVisibility.Private)
            throw new InvalidOperationException("XTSE0020");

        // A named mode cannot be abstract.
        if (!string.IsNullOrEmpty(name) && visibility == ModeVisibility.Abstract)
            throw new InvalidOperationException("XTSE0020");

        var typed = ParseYesNoAttribute(element, "typed");

        var warningOnNoMatch = ParseYesNoAttribute(element, "warning-on-no-match");
        var warningOnMultipleMatch = ParseYesNoAttribute(element, "warning-on-multiple-match");
        var streamable = ParseYesNoAttribute(element, "streamable");

        var useAllAccumulators = false;
        var useAccumulators = new HashSet<string>();
        var useAccAttr = element.Attribute("use-accumulators")?.Value;
        if (!string.IsNullOrWhiteSpace(useAccAttr))
        {
            foreach (var token in useAccAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = token.Trim();
                if (t == "#all")
                {
                    useAllAccumulators = true;
                }
                else if (!string.IsNullOrEmpty(t))
                {
                    useAccumulators.Add(ResolveAccumulatorName(t, element));
                }
            }
        }

        return new ModeDefinition(name, onNoMatch, onMultipleMatch, visibility, typed, warningOnNoMatch, warningOnMultipleMatch, streamable, useAccumulators, useAllAccumulators);
    }

    private static bool ParseYesNoAttribute(XElement element, string attributeName)
    {
        var value = element.Attribute(attributeName)?.Value?.Trim();
        if (string.IsNullOrEmpty(value))
            return false;
        // Values are case-sensitive: only the lower-case/standard forms are allowed.
        if (value is "yes" or "true" or "1")
            return true;
        if (value is "no" or "false" or "0")
            return false;
        throw new InvalidOperationException("XTSE0020");
    }

    private static string ExpandModeName(string mode, XElement element)
    {
        // xsl:mode/@name may be absent (the unnamed mode) or a valid QName; the special
        // mode tokens #current, #default, #all, and #unnamed are not permitted here.
        if (mode is "#current" or "#default" or "#all" or "#unnamed")
            throw new InvalidOperationException($"XTSE0020: Invalid mode name '{mode}' in xsl:mode/@name.");

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return NormalizeModeName(mode);

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return NormalizeModeName($"{{{attr.Value}}}{local}");
                }
            }
            current = current.Parent;
        }
        return NormalizeModeName(mode);
    }

    /// <summary>
    /// Resolves an accumulator name (EQName) to Clark notation.
    /// A lexical QName without a prefix is in no namespace; the default namespace is not used.
    /// </summary>
    private static string ResolveAccumulatorName(string name, XElement element)
    {
        if (name.StartsWith("Q{") && name.Length > 2)
        {
            return NormalizeModeName(name);
        }

        int colon = name.IndexOf(':');
        if (colon < 0)
            return NormalizeModeName(name);

        var prefix = name.Substring(0, colon);
        var local = name.Substring(colon + 1);

        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return NormalizeModeName($"{{{attr.Value}}}{local}");
                }
            }
            current = current.Parent;
        }
        return NormalizeModeName(name);
    }

    /// <summary>
    /// Normalizes a mode name so that <c>Q{{}}local</c> or <c>{{}}local</c>
    /// (empty namespace URI) is treated the same as <c>local</c>.
    /// </summary>
    public static string NormalizeModeName(string mode)
    {
        // #unnamed is the unnamed mode
        if (mode == "#unnamed")
            return "";
        // Q{}local → local
        if (mode.Length > 3 && mode.StartsWith("Q{") && mode[2] == '}')
            return mode.Substring(3);
        // {}local → local
        if (mode.Length > 2 && mode[0] == '{' && mode[1] == '}')
            return mode.Substring(2);
        return mode;
    }
}
