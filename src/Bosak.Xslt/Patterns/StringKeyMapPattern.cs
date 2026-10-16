// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Parses string-key map patterns and composes entry-value constraints.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Parse entry sequences and outer predicates; preserve repeated constraints.               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Xslt.Patterns;

/// <summary>Parses string-key map patterns and composes entry-value constraints.</summary>
internal sealed class StringKeyMapPattern(StringKeyMapPattern.Entry[] entries, bool open, string predicates)
{
    internal readonly record struct Entry(string Key, string? SequencePattern);

    // Count distinct keys: the grammar permits repeated requirements for the same entry.
    private readonly XdmValue[] _keys = entries.Select(e => e.Key).Distinct(StringComparer.Ordinal).Select(XdmValue.FromString).ToArray();

    internal string Predicates => predicates;

    internal double DefaultPriority => predicates.Length > 0 ? 0.5
        : entries.Any(e => e.SequencePattern is not null) ? 0.25
        : entries.Length == 0 ? open ? -0.25 : 0.5 : 0;

    internal PatternPredicate Compile(Func<string, PatternPredicate> compileSequence, PatternPredicate? outer)
    {
        var constraints = entries.Where(e => e.SequencePattern is not null)
            .Select(e => (Key: XdmValue.FromString(e.Key), Match: compileSequence(e.SequencePattern!))).ToArray();
        return (item, context) =>
        {
            if (!Matches(item)) return false;
            foreach (var constraint in constraints)
            {
                if (!item.MapValue.TryGetValue(constraint.Key, out var value) || !constraint.Match(value, context)) return false;
            }
            return outer is null || outer(item, context);
        };
    }

    internal bool Matches(XdmValue item)
    {
        if (!item.IsMap || item.IsSequence) return false;
        var map = item.MapValue;
        if (!open && map.Count != _keys.Length) return false;
        foreach (var key in _keys)
            if (!map.ContainsKey(key)) return false;
        return true;
    }

    internal static bool StartsMapPattern(string text) => text.StartsWith('{') ||
        (text.StartsWith("map", StringComparison.Ordinal) && text[3..].TrimStart().StartsWith('{'));

    internal static StringKeyMapPattern Parse(string text)
    {
        text = text.Trim();
        if (text.StartsWith("map", StringComparison.Ordinal)) text = text[3..].TrimStart();
        static InvalidOperationException Unsupported() => new(
            "XTSE0340: A supported map pattern requires quoted string keys, optional entry sequence patterns, a final wildcard and optional predicates; paths and non-string keys remain unsupported.");
        int close = PatternCompiler.FindMatchingDelimiter(text, 0, '{', '}');
        if (close < 0) throw Unsupported();
        var predicates = text[(close + 1)..].Trim();
        int suffix = 0;
        while (suffix < predicates.Length)
        {
            while (suffix < predicates.Length && char.IsWhiteSpace(predicates[suffix])) suffix++;
            if (suffix == predicates.Length) break;
            int end = PatternCompiler.FindMatchingDelimiter(predicates, suffix, '[', ']');
            if (end < 0) throw Unsupported();
            suffix = end + 1;
        }
        var body = text[1..close].Trim();
        if (body.EndsWith(',')) throw Unsupported();
        var entries = new List<Entry>();
        bool open = false;
        if (body.Length > 0)
        {
            var parts = PatternCompiler.SplitPatternTopLevel(body, ",");
            for (int index = 0; index < parts.Length; index++)
            {
                var part = parts[index].Trim();
                if (part == "*")
                {
                    if (index != parts.Length - 1) throw Unsupported();
                    open = true;
                    break;
                }
                if (part.Length == 0 || part[0] is not ('\'' or '"')) throw Unsupported();
                int position = 1;
                char quote = part[0];
                var key = new StringBuilder();
                bool closed = false;
                while (position < part.Length)
                {
                    char c = part[position++];
                    if (c != quote) { key.Append(c); continue; }
                    if (position < part.Length && part[position] == quote) { key.Append(quote); position++; continue; }
                    closed = true;
                    break;
                }
                if (!closed) throw Unsupported();
                string remaining = part[position..].Trim();
                string? sequence = null;
                if (remaining.Length > 0)
                {
                    if (!remaining.StartsWith(':') || remaining.Length == 1) throw Unsupported();
                    sequence = remaining[1..].Trim();
                    if (sequence.Length == 0) throw Unsupported();
                }
                entries.Add(new(key.ToString(), sequence));
            }
        }
        return new(entries.ToArray(), open, predicates);
    }
}
