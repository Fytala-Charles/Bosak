// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Plan bounded readable insertion edits and translate retained source provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>Plans exact source edits without reserializing any existing markup.</summary>
internal sealed class ValueOfInsertionLayout
{
    internal sealed record Edit(int Start, int End, string Replacement, int GapLength);
    internal List<Edit> Edits { get; } = new();
    internal int InstructionOffset { get; private set; }

    internal static ValueOfInsertionLayout? Create(AuthoringSource source, AuthoringValueOfInsertionTarget target,
        string instruction, AuthoringInsertionFormattingMode mode, CancellationToken token, out bool sizeLimit)
    {
        sizeLimit = false;
        var plan = new ValueOfInsertionLayout();
        if (mode == AuthoringInsertionFormattingMode.PreserveSource)
        {
            plan.Edits.Add(new(target.CharacterOffset, target.CharacterOffset, instruction, 0));
            plan.InstructionOffset = target.CharacterOffset;
            return plan;
        }
        // Runtime preservation can remain observable below an ancestor preserve declaration,
        // even after a nearer default reset. Refuse rather than introduce result whitespace.
        if (target.ContextElement.AncestorsAndSelf().Any(e =>
            (string?)e.Attribute(System.Xml.Linq.XNamespace.Xml + "space") == "preserve")) return null;
        var text = source.Text;
        string? newline = null;
        for (var i = 0; i < text.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            if (text[i] is not ('\r' or '\n')) continue;
            var current = text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? "\r\n" : text[i].ToString();
            if (current.Length == 2) i++;
            if (newline is not null && newline != current) return null;
            newline = current;
        }
        newline ??= "\n";
        var map = source.CoordinateMap;
        int Start(AuthoringNodeDescriptor n) => map.CharOffsetFromLineColumn(n.Range.StartLine, n.Range.StartColumn);
        int End(AuthoringNodeDescriptor n) => map.CharOffsetFromLineColumn(n.Range.EndLine, n.Range.EndColumn);
        var parentStart = Start(target.Parent);
        SourceTagScanner.FindElementEnd(text, parentStart, out var openingEnd, out _);
        var children = target.Parent.Children.Where(n => n.Kind != AuthoringNodeKind.Text).ToArray();
        var gaps = new List<(int Start, int End)>();
        var cursor = openingEnd;
        foreach (var child in children)
        {
            token.ThrowIfCancellationRequested();
            gaps.Add((cursor, Start(child)));
            cursor = End(child);
        }
        gaps.Add((cursor, target.CharacterOffset));
        foreach (var gap in gaps)
            for (var i = gap.Start; i < gap.End; i++)
            {
                token.ThrowIfCancellationRequested();
                if (text[i] is not (' ' or '\t' or '\r' or '\n')) return null;
            }
        var multiline = gaps.Any(g => text.AsSpan(g.Start, g.End - g.Start).IndexOfAny('\r', '\n') >= 0);
        string baseline;
        string childIndent;
        if (multiline)
        {
            var closing = LinePrefix(text, target.CharacterOffset);
            if (closing is null || !Uniform(closing)) return null;
            baseline = closing;
            if (children.Length != 0)
            {
                var prefixes = children.Select(c => LinePrefix(text, Start(c))).ToArray();
                var first = prefixes[0];
                if (first is null || !first.StartsWith(baseline, StringComparison.Ordinal) || first.Length <= baseline.Length
                    || !Uniform(first) || prefixes.Any(p => p != first)) return null;
                // Each direct unit must actually follow a newline, not only share a line prefix.
                if (gaps.Take(children.Length).Any(g => text.AsSpan(g.Start, g.End - g.Start).IndexOfAny('\r', '\n') < 0)) return null;
                childIndent = first;
            }
            else
            {
                var opening = LinePrefix(text, parentStart);
                if (opening is null || opening != baseline) return null;
                childIndent = baseline + InferUnit(target.Parent, text, baseline);
            }
        }
        else
        {
            var lineStart = parentStart;
            while (lineStart > 0 && text[lineStart - 1] is not ('\r' or '\n')) lineStart--;
            var end = lineStart;
            while (end < parentStart && text[end] is ' ' or '\t') end++;
            baseline = text[lineStart..end];
            if (!Uniform(baseline)) return null;
            childIndent = baseline + "  ";
        }
        if (!Uniform(childIndent)) return null;
        // Count the entire footprint before allocating replacement strings and the emitted buffer.
        long finalLength = source.OriginalBytes.Length;
        var prefixBytes = source.Encoding.GetByteCount(newline) + source.Encoding.GetByteCount(childIndent);
        foreach (var gap in gaps)
        {
            token.ThrowIfCancellationRequested();
            finalLength -= source.Encoding.GetByteCount(text.AsSpan(gap.Start, gap.End - gap.Start));
            finalLength += prefixBytes;
        }
        finalLength += source.Encoding.GetByteCount(instruction) + source.Encoding.GetByteCount(newline) + source.Encoding.GetByteCount(baseline);
        if (finalLength > AuthoringCapabilities.MaxValueOfInsertionSourceBytes)
        {
            sizeLimit = true;
            return null;
        }
        var delta = 0;
        foreach (var gap in gaps.Take(gaps.Count - 1))
        {
            token.ThrowIfCancellationRequested();
            var replacement = newline + childIndent;
            if (text.AsSpan(gap.Start, gap.End - gap.Start).SequenceEqual(replacement)) continue;
            plan.Edits.Add(new(gap.Start, gap.End, replacement, replacement.Length));
            delta = checked(delta + replacement.Length - (gap.End - gap.Start));
        }
        var last = gaps[^1];
        var leading = newline + childIndent;
        plan.InstructionOffset = checked(last.Start + delta + leading.Length);
        plan.Edits.Add(new(last.Start, last.End, leading + instruction + newline + baseline, leading.Length));
        return plan;
    }

    private static bool Uniform(string indent) => !indent.Contains(' ') || !indent.Contains('\t');

    private static string? LinePrefix(string text, int position)
    {
        var start = position;
        while (start > 0 && text[start - 1] is not ('\r' or '\n')) start--;
        for (var i = start; i < position; i++) if (text[i] is not (' ' or '\t')) return null;
        return text[start..position];
    }

    private static string InferUnit(AuthoringNodeDescriptor parent, string text, string baseline)
    {
        // The parent indentation itself can establish a homogeneous unit in an empty block.
        // With no unambiguous ancestor evidence, use the documented two-space fallback.
        if (parent.BackingObject is System.Xml.Linq.XElement element)
        {
            foreach (var ancestor in element.Ancestors())
            {
                if (ancestor is not System.Xml.IXmlLineInfo info || !info.HasLineInfo()) continue;
                var line = 1; var offset = 0;
                while (offset < text.Length && line < info.LineNumber)
                {
                    if (text[offset] == '\r') { if (offset + 1 < text.Length && text[offset + 1] == '\n') offset++; line++; }
                    else if (text[offset] == '\n') line++;
                    offset++;
                }
                // XElement line info points at the name, one character after '<'.
                var prefix = LinePrefix(text, offset + info.LinePosition - 2);
                if (prefix is not null && baseline.StartsWith(prefix, StringComparison.Ordinal) && baseline.Length > prefix.Length)
                    return baseline[prefix.Length..];
            }
        }
        return "  ";
    }

    internal byte[]? Emit(AuthoringSource source, CancellationToken token)
    {
        long length = source.OriginalBytes.Length;
        foreach (var e in Edits)
        {
            token.ThrowIfCancellationRequested();
            length += source.Encoding.GetByteCount(e.Replacement) -
                (source.CoordinateMap.ByteOffsetFromCharOffset(e.End) - source.CoordinateMap.ByteOffsetFromCharOffset(e.Start));
        }
        if (length > AuthoringCapabilities.MaxValueOfInsertionSourceBytes) return null;
        var output = new byte[checked((int)length)];
        var original = source.OriginalBytes.Span; var oldOffset = 0; var newOffset = 0;
        foreach (var e in Edits)
        {
            token.ThrowIfCancellationRequested();
            var start = checked((int)source.CoordinateMap.ByteOffsetFromCharOffset(e.Start));
            var end = checked((int)source.CoordinateMap.ByteOffsetFromCharOffset(e.End));
            original[oldOffset..start].CopyTo(output.AsSpan(newOffset)); newOffset += start - oldOffset;
            newOffset += source.Encoding.GetBytes(e.Replacement.AsSpan(), output.AsSpan(newOffset)); oldOffset = end;
        }
        original[oldOffset..].CopyTo(output.AsSpan(newOffset));
        return output;
    }

    internal IReadOnlyList<AuthoringInsertionSourceEdit> Evidence(AuthoringSource input, AuthoringSource candidate)
    {
        var delta = 0;
        return Edits.Select(e =>
        {
            var start = checked(e.Start + delta);
            delta = checked(delta + e.Replacement.Length - (e.End - e.Start));
            return new AuthoringInsertionSourceEdit(input.CoordinateMap.RangeFromCharOffsets(e.Start, e.End),
                candidate.CoordinateMap.RangeFromCharOffsets(start, start + e.Replacement.Length));
        }).ToArray();
    }

    internal (int Start, int End) Translate(int start, int end, bool text)
    {
        var before = 0; var inside = 0;
        foreach (var e in Edits)
        {
            if (text && start == e.Start && end == e.End && end > start)
                return (checked(start + before), checked(start + before + e.GapLength));
            var delta = e.Replacement.Length - (e.End - e.Start);
            if (e.End <= start) before = checked(before + delta);
            else if (e.Start < end && e.End <= end) inside = checked(inside + delta);
        }
        return (checked(start + before), checked(end + before + inside));
    }
}
