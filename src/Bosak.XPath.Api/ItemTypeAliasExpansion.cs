// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Expand aliases within supported array and map sequence-type syntax
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Text.RegularExpressions;

namespace Bosak.XPath.Api;

/// <summary>Expands complete type-name tokens inside supported map and array types.</summary>
internal static class ItemTypeAliasExpansion
{
    internal static string? Expand(string type, Func<string, string?> resolveName)
    {
        var text = RemoveComments(type).Trim();
        var suffix = text.Length > 0 && text[^1] is '?' or '*' or '+' ? text[^1].ToString() : string.Empty;
        if (suffix.Length > 0) text = text[..^1].TrimEnd();
        var enumeration = Regex.Match(text, @"^enum\s*\(");
        if (enumeration.Success && text.EndsWith(')')) return "enum(" + text[enumeration.Length..] + suffix;
        if (text is "item()" or "node()" or "empty-sequence()") return text + suffix;
        foreach (var kind in new[] { "array", "map" })
        {
            var opening = Regex.Match(text, "^" + kind + @"\s*\(");
            if (!opening.Success || !text.EndsWith(')')) continue;
            var inner = text[opening.Length..^1].Trim();
            if (inner is "" or "*") return kind + "(" + inner + ")" + suffix;
            var parts = SplitArguments(inner);
            if (parts.Count != (kind == "array" ? 1 : 2)) return null;
            var expanded = new List<string>(parts.Count);
            foreach (var part in parts)
            {
                var value = Expand(part, resolveName);
                if (value is null) return null;
                expanded.Add(value);
            }
            if (kind == "map" && expanded[0].Length > 0 && expanded[0][^1] is '?' or '*' or '+')
                throw new InvalidOperationException("XPST0003: A map key type cannot have an occurrence indicator.");
            return kind + "(" + string.Join(",", expanded) + ")" + suffix;
        }
        if (!Regex.IsMatch(text, @"^(?:Q\{[^}]*\})?[\p{L}_][\p{L}\p{M}\p{N}_.-]*(?::[\p{L}_][\p{L}\p{M}\p{N}_.-]*)?$")) return null;
        var replacement = resolveName(text);
        return replacement is null ? null : replacement + suffix;
    }

    private static List<string> SplitArguments(string text)
    {
        var result = new List<string>();
        int depth = 0, braces = 0, start = 0;
        char quote = '\0';
        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    if (i + 1 < text.Length && text[i + 1] == quote) i++;
                    else quote = '\0';
                }
                continue;
            }
            if (braces > 0) { if (c == '}') braces--; continue; }
            if (c is '\'' or '"') quote = c;
            else if (c == '{') braces++;
            else if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (c == ',' && depth == 0) { result.Add(text[start..i]); start = i + 1; }
        }
        result.Add(text[start..]);
        return result;
    }

    private static string RemoveComments(string text)
    {
        var result = new StringBuilder(text.Length);
        char quote = '\0';
        int commentDepth = 0, braces = 0;
        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                result.Append(c);
                if (c == quote)
                {
                    if (i + 1 < text.Length && text[i + 1] == quote) result.Append(text[++i]);
                    else quote = '\0';
                }
                continue;
            }
            if (braces > 0) { result.Append(c); if (c == '}') braces--; continue; }
            if (i + 1 < text.Length && c == '(' && text[i + 1] == ':') { commentDepth++; i++; continue; }
            if (commentDepth > 0)
            {
                if (i + 1 < text.Length && c == ':' && text[i + 1] == ')') { commentDepth--; i++; if (commentDepth == 0) result.Append(' '); }
                continue;
            }
            if (c is '\'' or '"') quote = c;
            if (c == '{') braces++;
            result.Append(c);
        }
        if (commentDepth != 0) throw new InvalidOperationException("XPST0003: Unterminated comment in a sequence type.");
        return result.ToString();
    }
}
