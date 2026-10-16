// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Parses and matches the string-key subset of XSLT 4.0 map patterns.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Xslt.Patterns;

/// <summary>Parses and matches the string-key subset of XSLT 4.0 map patterns.</summary>
internal sealed class StringKeyMapPattern(string[] keys, bool open)
{
    // Count distinct keys: the grammar permits repeated requirements for the same entry.
    private readonly XdmValue[] _keys = keys.Distinct(StringComparer.Ordinal).Select(XdmValue.FromString).ToArray();

    internal double DefaultPriority => keys.Length == 0 ? open ? -0.25 : 0.5 : 0;

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
        int position = 0;
        void WhiteSpace() { while (position < text.Length && char.IsWhiteSpace(text[position])) position++; }
        bool Take(char c)
        {
            WhiteSpace();
            if (position >= text.Length || text[position] != c) return false;
            position++;
            return true;
        }
        static InvalidOperationException Unsupported() => new(
            "XTSE0340: Supported map patterns contain string keys, optionally followed by a final wildcard; entry-value constraints and suffixes remain unsupported.");
        if (!Take('{')) throw Unsupported();
        var keys = new List<string>();
        bool open = false;
        if (!Take('}'))
        {
            while (true)
            {
                if (Take('*'))
                {
                    open = true;
                    if (!Take('}')) throw Unsupported();
                    break;
                }
                WhiteSpace();
                if (position >= text.Length || text[position] is not ('\'' or '"')) throw Unsupported();
                char quote = text[position++];
                var key = new StringBuilder();
                bool closed = false;
                while (position < text.Length)
                {
                    char c = text[position++];
                    if (c != quote) { key.Append(c); continue; }
                    if (position < text.Length && text[position] == quote) { key.Append(quote); position++; continue; }
                    closed = true;
                    break;
                }
                if (!closed) throw Unsupported();
                keys.Add(key.ToString());
                if (Take('}')) break;
                if (!Take(',')) throw Unsupported();
            }
        }
        WhiteSpace();
        if (position != text.Length) throw Unsupported();
        return new(keys.ToArray(), open);
    }
}
