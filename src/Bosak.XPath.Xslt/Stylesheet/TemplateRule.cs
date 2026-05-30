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
using Bosak.XPath.Runtime.Vm;

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
    public Patterns.PatternPredicate? CompiledMatch { get; private set; }

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
                CompiledMatch = (node, ctx) => node.NodeKind == XdmNodeKind.Document;
            else
            {
                // Resolve namespace prefixes in the pattern to Q{uri}local syntax
                // so the pattern compiler can match namespaced elements correctly.
                var resolved = ResolveNamespacePrefixes(Match);
                CompiledMatch = compiler.Compile(resolved);
            }
        }
    }

    /// <summary>
    /// Replaces prefix:local-name occurrences in a pattern with Q{uri}local-name,
    /// resolving prefixes using the namespace declarations in scope on the
    /// xsl:template element.
    /// </summary>
    private string ResolveNamespacePrefixes(string pattern)
    {
        // Quick exit if no colon present (no prefixes to resolve)
        if (!pattern.Contains(':'))
            return pattern;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            // Skip string literals
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
            // Check for Q{…} syntax — already resolved, copy through
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
            {
                sb.Append(c);
                i++;
                continue;
            }
            // Look for prefix:local-name
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-'))
                    i++;
                if (i < pattern.Length && pattern[i] == ':')
                {
                    var prefix = pattern[start..i];
                    i++; // skip ':'
                    int localStart = i;
                    while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
                        i++;
                    var local = pattern[localStart..i];
                    // Resolve prefix; if not found, keep original (will fail to match, which is correct)
                    var nsUri = Element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
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
    /// Computes the default priority for a match pattern per the XSLT 2.0/3.0 spec.
    /// </summary>
    private static double ComputeDefaultPriority(string? match)
    {
        if (string.IsNullOrEmpty(match))
            return 0.5;

        var trimmed = match.Trim();

        // Document patterns and root pattern: +0.5
        if (trimmed == "/" || trimmed.StartsWith("doc(") || trimmed.StartsWith("document("))
            return 0.5;

        // Path patterns (contain /): +0.5
        if (trimmed.Contains('/'))
            return 0.5;

        // Strip leading @ for attribute tests
        bool isAttribute = trimmed.StartsWith('@');
        if (isAttribute)
            trimmed = trimmed[1..].Trim();

        // Wildcards: * or node() or text() or comment() or processing-instruction() or .
        if (trimmed == "*" || trimmed == "node()" || trimmed == "text()"
            || trimmed == "comment()" || trimmed == "processing-instruction()"
            || trimmed == ".")
        {
            return -0.5;
        }

        // Namespace wildcards: prefix:* or *:local
        if (trimmed.EndsWith(":*") || trimmed.StartsWith("*:"))
            return -0.25;

        // Predicate on a simple pattern: use the base pattern's priority
        if (trimmed.Contains('['))
        {
            int bracket = trimmed.IndexOf('[');
            var basePat = trimmed[..bracket].Trim();
            return ComputeDefaultPriority(basePat);
        }

        // QName: no wildcards, no parentheses
        if (!trimmed.Contains('*') && !trimmed.Contains('(') && !trimmed.Contains(')'))
            return 0.0;

        // Fallback
        return 0.0;
    }
}
