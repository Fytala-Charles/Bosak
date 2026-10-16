// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Parses array member sequence patterns and predicate suffixes.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================

using System.Text.RegularExpressions;

namespace Bosak.Xslt.Patterns;

/// <summary>Parses array member sequence patterns and predicate suffixes.</summary>
internal sealed record ArrayPattern(string Members, string Predicates)
{
    internal static bool StartsArrayPattern(string text) => Regex.IsMatch(text, @"^array\s*\(");
    internal static bool IsEmptySequence(string text) => Regex.IsMatch(text,
        @"\Aempty-sequence[ \t\r\n]*\([ \t\r\n]*\)\z", RegexOptions.CultureInvariant);

    internal string ItemPattern
    {
        get
        {
            var text = Members;
            if (text.Length > 1 && (text[^1] is '?' or '+' ||
                (text[^1] == '*' && !text.EndsWith(":*", StringComparison.Ordinal) && !text.EndsWith("}*", StringComparison.Ordinal))))
                text = text[..^1].TrimEnd();
            return text;
        }
    }

    internal bool HasPredicates
    {
        get
        {
            if (Predicates.Length > 0) return true;
            char quote = '\0';
            bool uri = false;
            for (int i = 0; i < Members.Length; i++)
            {
                char c = Members[i];
                if (quote != '\0')
                {
                    if (c == quote)
                    {
                        if (i + 1 < Members.Length && Members[i + 1] == quote) i++;
                        else quote = '\0';
                    }
                    continue;
                }
                if (uri) { if (c == '}') uri = false; continue; }
                if (c == 'Q' && i + 1 < Members.Length && Members[i + 1] == '{') { uri = true; i++; continue; }
                if (c is '\'' or '"') { quote = c; continue; }
                if (c == '[') return true;
            }
            return false;
        }
    }

    internal static ArrayPattern Parse(string text)
    {
        var opening = text.IndexOf('(');
        var close = PatternCompiler.FindMatchingDelimiter(text, opening, '(', ')');
        if (close < 0) throw new InvalidOperationException("XTSE0340: Unclosed array pattern.");
        var members = text[(opening + 1)..close].Trim();
        if (members.Length == 0 || members.StartsWith(',') || members.EndsWith(',') ||
            PatternCompiler.SplitPatternTopLevel(members, ",").Length > 1)
            throw new InvalidOperationException("XTSE0340: An array pattern requires one member sequence pattern.");
        var predicates = text[(close + 1)..].Trim();
        var rest = predicates;
        while (rest.StartsWith('['))
        {
            var end = PatternCompiler.FindMatchingDelimiter(rest, 0, '[', ']');
            if (end < 0) throw new InvalidOperationException("XTSE0340: Unclosed array-pattern predicate.");
            rest = rest[(end + 1)..].TrimStart();
        }
        if (rest.Length != 0) throw new InvalidOperationException("XTSE0340: Unexpected text after array pattern.");
        return new(members, predicates);
    }
}
