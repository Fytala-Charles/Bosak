// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Parses named record patterns and their predicate suffixes.
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

/// <summary>Parses named record patterns and their predicate suffixes.</summary>
internal sealed record RecordPattern(string Name, string Predicates)
{
    internal static bool StartsRecordPattern(string text) => Regex.IsMatch(text, @"^record\s*\(");
    internal double DefaultPriority => Predicates.Length > 0 ? 0.5 : 0;

    internal static RecordPattern Parse(string text)
    {
        var opening = text.IndexOf('(');
        var close = PatternCompiler.FindMatchingDelimiter(text, opening, '(', ')');
        if (close < 0) throw new InvalidOperationException("XTSE0340: Unclosed record pattern.");
        var name = text[(opening + 1)..close].Trim();
        if (!Regex.IsMatch(name, @"^(?:Q\{[^}]*\}[\p{L}_][\p{L}\p{M}\p{N}_.-]*|[\p{L}_][\p{L}\p{M}\p{N}_.-]*(?::[\p{L}_][\p{L}\p{M}\p{N}_.-]*)?)$"))
            throw new InvalidOperationException("XTSE0340: A record pattern requires one type EQName.");
        var predicates = text[(close + 1)..].Trim();
        var rest = predicates;
        while (rest.StartsWith('['))
        {
            var end = PatternCompiler.FindMatchingDelimiter(rest, 0, '[', ']');
            if (end < 0) throw new InvalidOperationException("XTSE0340: Unclosed record-pattern predicate.");
            rest = rest[(end + 1)..].TrimStart();
        }
        if (rest.Length != 0) throw new InvalidOperationException("XTSE0340: Unexpected text after record pattern.");
        return new(name, predicates);
    }
}
