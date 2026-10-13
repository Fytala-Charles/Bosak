// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Expand aliases within supported structural sequence-type syntax
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
//                      | Charles Korthout | 0.2 | 13-10-2026     | Expand function signatures and ordered choices while preserving return/outer occurrences
//                      | Charles Korthout | 0.3 | 13-10-2026     | Expand aliases in structural record fields while preserving literal field names
//                      | Charles Korthout | 0.4 | 13-10-2026     | Check choice cast eligibility with quote and EQName-aware type traversal
//                      | Charles Korthout | 0.5 | 13-10-2026     | Classify known nominative names as non-atomic choice alternatives
//                      | Charles Korthout | 0.6 | 13-10-2026     | Traverse name-free node-kind types within aliases and structural sequence types
//                      | Charles Korthout | 0.7 | 13-10-2026     | Resolve names in element, attribute and document node type aliases
// ===========================================================================================================================================================
using System.Text;
using System.Text.RegularExpressions;

namespace Bosak.XPath.Api;

/// <summary>Expands complete type-name tokens inside supported structural types.</summary>
internal static class ItemTypeAliasExpansion
{
    internal static string? Expand(string type, Func<string, string?> resolveName, Func<string, bool, string>? resolveNodeName = null)
    {
        var text = RemoveComments(type).Trim();
        var function = Regex.Match(text, @"^function\s*\(");
        if (function.Success)
        {
            var close = MatchingClose(text, function.Length - 1);
            if (close < 0) throw new InvalidOperationException("XPST0003: Unclosed function type.");
            var arguments = text[function.Length..close].Trim();
            var tail = text[(close + 1)..].Trim();
            if (arguments == "*" && tail is "" or "?" or "*" or "+") return "function(*)" + tail;
            var resultMarker = Regex.Match(tail, @"^as\s+");
            if (!resultMarker.Success) throw new InvalidOperationException("XPST0003: A typed function requires an as return type.");
            var parameters = arguments.Length == 0 ? new List<string>() : SplitArguments(arguments);
            var expandedParameters = new List<string>(parameters.Count);
            foreach (var parameter in parameters)
            {
                var expanded = Expand(parameter, resolveName, resolveNodeName);
                if (expanded is null) return null;
                expandedParameters.Add(expanded);
            }
            var result = Expand(tail[resultMarker.Length..], resolveName, resolveNodeName);
            return result is null ? null : "function(" + string.Join(",", expandedParameters) + ") as " + result;
        }
        var suffix = text.Length > 0 && text[^1] is '?' or '*' or '+' ? text[^1].ToString() : string.Empty;
        if (suffix.Length > 0) text = text[..^1].TrimEnd();
        if (text.StartsWith('(') && MatchingClose(text, 0) == text.Length - 1)
        {
            var alternatives = SplitArguments(text[1..^1], '|');
            var expandedAlternatives = new List<string>(alternatives.Count);
            foreach (var alternative in alternatives)
            {
                var expanded = Expand(alternative, resolveName, resolveNodeName);
                if (expanded is null) return null;
                expandedAlternatives.Add(expanded);
            }
            return "(" + string.Join("|", expandedAlternatives) + ")" + suffix;
        }
        var enumeration = Regex.Match(text, @"^enum\s*\(");
        if (enumeration.Success && text.EndsWith(')')) return "enum(" + text[enumeration.Length..] + suffix;
        var emptyTest = Regex.Match(text, @"^(item|node|empty-sequence|element|attribute|text|comment|namespace-node|document-node|processing-instruction)\s*\(\s*\)$");
        if (emptyTest.Success) return emptyTest.Groups[1].Value + "()" + suffix;
        var namedNode = Regex.Match(text, @"^(element|attribute)\s*\(\s*(.*?)\s*\)$");
        if (namedNode.Success)
        {
            var name = namedNode.Groups[2].Value;
            // Schema annotations and partial name wildcards use a separate grammar.
            if (name != "*" && !Regex.IsMatch(name, @"^(?:Q\{[^}]*\})?[\p{L}_][\p{L}\p{M}\p{N}_.-]*(?::[\p{L}_][\p{L}\p{M}\p{N}_.-]*)?$")) return null;
            var kind = namedNode.Groups[1].Value;
            return kind + "(" + (name == "*" ? name : resolveNodeName?.Invoke(name, kind == "attribute") ?? name) + ")" + suffix;
        }
        var document = Regex.Match(text, @"^document-node\s*\(\s*(element\s*\(.*\))\s*\)$");
        if (document.Success)
        {
            var inner = Expand(document.Groups[1].Value, resolveName, resolveNodeName);
            return inner is null ? null : "document-node(" + inner + ")" + suffix;
        }
        var record = Regex.Match(text, @"^record\s*\(");
        if (record.Success && MatchingClose(text, record.Length - 1) == text.Length - 1)
        {
            var inner = text[record.Length..^1].Trim();
            if (inner is "" or "*") return "record(" + inner + ")" + suffix;
            var fields = SplitArguments(inner);
            var expandedFields = new List<string>();
            foreach (var raw in fields)
            {
                var field = raw.Trim();
                if (field.Length == 0) { expandedFields.Add(field); continue; }
                int end = 0;
                if (field[0] is '\'' or '"')
                {
                    var quote = field[end++];
                    while (end < field.Length)
                    {
                        if (field[end++] != quote) continue;
                        if (end < field.Length && field[end] == quote) { end++; continue; }
                        break;
                    }
                }
                else while (end < field.Length && !char.IsWhiteSpace(field[end])) end++;
                var tail = field[end..].TrimStart();
                if (tail.Length == 0) { expandedFields.Add(field); continue; }
                var marker = Regex.Match(tail, @"^as\s+");
                if (!marker.Success) return null;
                var expanded = Expand(tail[marker.Length..], resolveName, resolveNodeName);
                if (expanded is null) return null;
                expandedFields.Add(field[..end] + " as " + expanded);
            }
            return "record(" + string.Join(",", expandedFields) + ")" + suffix;
        }
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
                var value = Expand(part, resolveName, resolveNodeName);
                if (value is null) return null;
                expanded.Add(value);
            }
            if (kind == "map" && expanded[0].Length > 0 && expanded[0][^1] is '?' or '*' or '+')
                throw new InvalidOperationException("XPST0003: A map key type cannot have an occurrence indicator.");
            return kind + "(" + string.Join(",", expanded) + ")" + suffix;
        }
        if (!Regex.IsMatch(text, @"^(?:Q\{[^}]*\})?[\p{L}_][\p{L}\p{M}\p{N}_.-]*(?::[\p{L}_][\p{L}\p{M}\p{N}_.-]*)?$")) return null;
        var replacement = resolveName(text);
        return replacement is null ? null : (replacement.StartsWith("function(", StringComparison.Ordinal) ? "(" + replacement + ")" : replacement) + suffix;
    }

    // Unknown QNames remain for schema/name validation; this checks syntactically
    // non-atomic alternatives without interpreting separators inside strings or EQNames.
    internal static bool HasIneligibleCastTarget(string type, bool choiceAlternative = false, Func<string, bool>? isNonAtomicName = null)
    {
        var text = RemoveComments(type).Trim();
        if (text.StartsWith('(') && MatchingClose(text, 0) == text.Length - 1)
        {
            var alternatives = SplitArguments(text[1..^1], '|');
            return alternatives.Any(alternative => HasIneligibleCastTarget(alternative,
                choiceAlternative || alternatives.Count > 1, isNonAtomicName));
        }
        var opening = text.IndexOf('(');
        if (opening >= 0 && !text.StartsWith("Q{", StringComparison.Ordinal))
        {
            var kind = text[..opening].Trim();
            if (kind == "enum") return false;
            if (kind is "array" or "map" or "record") return choiceAlternative;
            return true;
        }
        return choiceAlternative && (isNonAtomicName?.Invoke(text) == true ||
            text is "xs:anyAtomicType" or "xs:anySimpleType" or "xs:NOTATION" or "xs:anyType" or "xs:untyped");
    }

    private static List<string> SplitArguments(string text, char separator = ',')
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
            else if (c == separator && depth == 0) { result.Add(text[start..i]); start = i + 1; }
        }
        result.Add(text[start..]);
        return result;
    }

    private static int MatchingClose(string text, int opening)
    {
        int depth = 0, braces = 0;
        char quote = '\0';
        for (int i = opening; i < text.Length; i++)
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
            else if (c == ')' && --depth == 0) return i;
        }
        return -1;
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
