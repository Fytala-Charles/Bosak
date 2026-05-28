// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : A single template rule with its match pattern, priority, mode, and compiled body.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added ImportPrecedence for xsl:import priority resolution                              |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added multi-mode support (Modes array, #all, #current, #default)                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Xslt.Stylesheet;

/// <summary>
/// Represents a single xsl:template rule within a stylesheet.
/// </summary>
public sealed class TemplateRule
{
    /// <summary>The original XElement of the xsl:template.</summary>
    public XElement Element { get; }

    /// <summary>The match pattern string (e.g. "foo[bar]"), or null for named templates.</summary>
    public string? Match { get; }

    /// <summary>The template name, or null for match-only templates.</summary>
    public string? Name { get; }

    /// <summary>The modes this template participates in (default is empty string for the default mode).</summary>
    public IReadOnlyList<string> Modes { get; }

    /// <summary>True if this template matches all modes.</summary>
    public bool MatchesAllModes { get; }

    /// <summary>The explicit or computed priority.</summary>
    public double Priority { get; }

    /// <summary>The import precedence (0 = main stylesheet, higher = deeper import).</summary>
    public int ImportPrecedence { get; }

    /// <summary>The compiled match predicate, or null for named-only templates.</summary>
    public Func<IXdmNode, bool>? CompiledMatch { get; private set; }

    /// <summary>The parent stylesheet.</summary>
    public Stylesheet Stylesheet { get; }

    private TemplateRule(XElement element, string? match, string? name, IReadOnlyList<string> modes, double priority, Stylesheet stylesheet)
    {
        Element = element;
        Match = match;
        Name = name;
        Modes = modes;
        MatchesAllModes = modes.Contains("#all");
        Priority = priority;
        Stylesheet = stylesheet;
        ImportPrecedence = stylesheet.ImportPrecedence;
    }

    /// <summary>
    /// Creates a <see cref="TemplateRule"/> from an xsl:template element.
    /// </summary>
    public static TemplateRule? FromElement(XElement element, Stylesheet stylesheet)
    {
        var match = element.Attribute("match")?.Value;
        var name = element.Attribute("name")?.Value;
        var modeAttr = element.Attribute("mode")?.Value;
        var modes = ParseModes(modeAttr);

        if (string.IsNullOrEmpty(match) && string.IsNullOrEmpty(name))
            return null; // Invalid template (no match and no name)

        var priorityAttr = element.Attribute("priority");
        double priority = priorityAttr != null && double.TryParse(priorityAttr.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p)
            ? p
            : ComputeDefaultPriority(match);

        return new TemplateRule(element, match, name, modes, priority, stylesheet);
    }

    private static IReadOnlyList<string> ParseModes(string? modeAttr)
    {
        if (string.IsNullOrEmpty(modeAttr))
            return new[] { "" }; // default mode

        var modes = modeAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return modes.Length > 0 ? modes : new[] { "" };
    }

    /// <summary>
    /// Compiles the match pattern into an executable predicate.
    /// </summary>
    public void CompileMatch(Patterns.PatternCompiler compiler)
    {
        if (!string.IsNullOrEmpty(Match))
        {
            if (Match.Trim() == "/")
                CompiledMatch = node => node.NodeKind == XdmNodeKind.Document;
            else
                CompiledMatch = compiler.Compile(Match);
        }
    }

    /// <summary>
    /// Computes the default priority for a match pattern per the XSLT spec.
    /// </summary>
    private static double ComputeDefaultPriority(string? match)
    {
        if (string.IsNullOrEmpty(match))
            return 0.5;

        var trimmed = match.Trim();

        // Document patterns (doc(...), document(...)): +0.5
        if (trimmed.StartsWith("doc(") || trimmed.StartsWith("document("))
            return 0.5;

        // QName: +0.0
        // Namespace test (prefix:*): -0.25
        // Local name test (*:local): -0.25
        // Wildcard (*, @*, node(), etc.): -0.5
        // TODO: Implement full default priority computation
        return 0.0;
    }
}
