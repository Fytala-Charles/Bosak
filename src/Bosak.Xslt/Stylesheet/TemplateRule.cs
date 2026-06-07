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
//                      | Charles Korthout | 0.4   | 05-06-2026     | Strip outer parens in priority computation; added FindMatchingParen helper             |
//                      | Charles Korthout | 0.5   | 07-06-2026     | StripXPathComments in ComputeDefaultPriority; fixes comment-stripped PredicatePattern   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Stylesheet;

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
                CompiledMatch = (item, ctx) => item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Document;
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
                    // Check if this is :: (axis separator) rather than : (namespace prefix)
                    if (i + 1 < pattern.Length && pattern[i + 1] == ':')
                    {
                        sb.Append(pattern[start..i]);
                        continue;
                    }
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

        var trimmed = StripXPathComments(match).Trim();

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeDefaultPriority(trimmed[1..close].Trim());
        }

        // Union patterns: take the maximum priority of all branches
        var branches = SplitUnionBranches(trimmed);
        if (branches.Length > 1)
            return branches.Max(ComputeSinglePatternPriority);

        return ComputeSinglePatternPriority(trimmed);
    }

    private static double ComputeSinglePatternPriority(string trimmed)
    {
        trimmed = StripXPathComments(trimmed).Trim();

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeSinglePatternPriority(trimmed[1..close].Trim());
        }

        // Document patterns and root pattern
        if (trimmed == "/" || trimmed.StartsWith("doc(") || trimmed.StartsWith("document("))
            return 0.5;

        // Path patterns (contain /)
        if (trimmed.Contains('/'))
            return 0.5;

        // PredicatePattern: .[expr]
        if (trimmed.StartsWith("."))
            return trimmed.Contains('[') ? 1.0 : -0.5;

        // Explicit axis step
        if (trimmed.Contains("::"))
        {
            int colonIdx = trimmed.IndexOf("::");
            string axis = trimmed[..colonIdx].Trim().ToLowerInvariant();
            string nodeTest = trimmed[(colonIdx + 2)..].Trim();

            // child::QName → 0.0; attribute::QName → 0.5
            if ((axis == "child" || axis == "attribute") && IsQNameNodeTest(nodeTest))
                return axis == "child" ? 0.0 : 0.5;

            // Namespace wildcards: NCName:* or *:NCName → -0.25
            if (nodeTest.EndsWith(":*") || nodeTest.StartsWith("*:"))
                return -0.25;

            // KindTest or * → -0.5
            if (nodeTest == "*" || IsKindTest(nodeTest))
                return -0.5;

            // Any other axis step → 0.5
            return 0.5;
        }

        // @attr is shorthand for attribute::attr → 0.5
        if (trimmed.StartsWith('@'))
        {
            var name = trimmed[1..].Trim();
            if (name == "*")
                return -0.5;
            if (name.EndsWith(":*") || name.StartsWith("*:"))
                return -0.25;
            if (!name.Contains('*') && !name.Contains('('))
                return 0.5;
            return 0.5;
        }

        // Wildcards and kind tests without axis
        if (trimmed == "*" || IsKindTest(trimmed) || trimmed == ".")
            return -0.5;

        // Namespace wildcards without axis
        if (trimmed.EndsWith(":*") || trimmed.StartsWith("*:"))
            return -0.25;

        // Patterns with a predicate
        if (trimmed.Contains('['))
            return 0.5;

        // QName without axis (implicit child::)
        if (IsQNameNodeTest(trimmed))
            return 0.0;

        // Fallback: any other pattern
        return 0.5;
    }

    private static bool IsQNameNodeTest(string s)
    {
        // A QName has no wildcards, no parentheses, no brackets, no operators
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s)
        {
            if (c == '*' || c == '(' || c == ')' || c == '[' || c == ']' || c == '|' || c == '/')
                return false;
        }
        return true;
    }

    private static bool IsKindTest(string s)
    {
        return s switch
        {
            "node()" or "text()" or "comment()" or "processing-instruction()"
            or "element()" or "attribute()" or "schema-element()" or "schema-attribute()"
            or "document-node()" => true,
            _ => false
        };
    }

    private static string[] SplitUnionBranches(string pattern)
    {
        var parts = new List<string>();
        int start = 0;
        int depth = 0;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (c == '(' || c == '[') depth++;
            else if (c == ')' || c == ']') depth--;
            else if (c == '|' && depth == 0)
            {
                parts.Add(pattern[start..i].Trim());
                start = i + 1;
            }
        }
        parts.Add(pattern[start..].Trim());
        return parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
    }

    /// <summary>
    /// Removes XPath comments (: ... :) from the text, preserving string literals.
    /// </summary>
    private static string StripXPathComments(string text)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            // Preserve string literals
            if (c == '\'' || c == '"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < text.Length && text[i] != quote)
                {
                    sb.Append(text[i]);
                    i++;
                }
                if (i < text.Length)
                {
                    sb.Append(text[i]);
                    i++;
                }
                continue;
            }
            // Skip comment
            if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
            {
                i += 2;
                int depth = 1;
                while (i < text.Length && depth > 0)
                {
                    if (i + 1 < text.Length && text[i] == ':' && text[i + 1] == ')')
                    {
                        depth--;
                        i += 2;
                    }
                    else if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
                    {
                        depth++;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Finds the index of the matching closing parenthesis for the paren at startIndex.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindMatchingParen(string text, int startIndex)
    {
        if (text[startIndex] != '(') return -1;
        int depth = 1;
        for (int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }
}
