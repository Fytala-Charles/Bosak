// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Prove and emit exact adjacent sibling exchanges with total piecewise correspondence.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

internal static class LiteralElementReorderPipeline
{
    private static AuthoringLiteralElementReorderFailure Fail(AuthoringSnapshot s, AuthoringLiteralElementReorderFailureKind kind, string message, string? code = null) =>
        new(kind, message, s.PrincipalModule.ModuleUri, code);

    private static AuthoringLiteralElementReorderFailure Convert(AuthoringSnapshot s, AuthoringLiteralElementFailure f) =>
        Fail(s, Enum.TryParse<AuthoringLiteralElementReorderFailureKind>(f.Kind.ToString(), out var kind) ? kind : AuthoringLiteralElementReorderFailureKind.UnsupportedCoverage, f.Message, f.Code);

    private static AuthoringLiteralElementReorderFailure Convert(AuthoringSnapshot s, AuthoringLiteralAttributeFailure f) =>
        Fail(s, Enum.TryParse<AuthoringLiteralElementReorderFailureKind>(f.Kind.ToString(), out var kind) ? kind : AuthoringLiteralElementReorderFailureKind.UnsupportedCoverage, f.Message, f.Code);

    internal static AuthoringLiteralElementReorderTargetResult Discover(AuthoringSnapshot s, AuthoringNodeDescriptor node, AuthoringLiteralElementMoveDirection direction, CancellationToken token)
    {
        AuthoringLiteralElementReorderTargetResult Refuse(AuthoringLiteralElementReorderFailureKind k, string message) => new(null, Fail(s, k, message));
        try
        {
            token.ThrowIfCancellationRequested();
            if (direction is not (AuthoringLiteralElementMoveDirection.Up or AuthoringLiteralElementMoveDirection.Down))
                return Refuse(AuthoringLiteralElementReorderFailureKind.InvalidDirection, "An explicit Up or Down direction is required.");
            var bounded = Bounds(s, token);
            if (bounded is not null) return new(null, bounded);
            var original = LiteralElementLifecyclePipeline.Discover(s, node, token);
            if (original.Failure is not null) return new(null, Convert(s, original.Failure));
            var parent = original.Data!.Parent;
            if (parent.Kind != AuthoringNodeKind.Element || parent.ElementName?.NamespaceUri != "")
                return Refuse(AuthoringLiteralElementReorderFailureKind.IneligibleTarget, "A supported no-namespace literal parent is required.");
            var parentCheck = LiteralElementLifecyclePipeline.Discover(s, parent, token);
            if (parentCheck.Failure is not null) return new(null, Convert(s, parentCheck.Failure));
            var index = -1;
            for (var i = 0; i < parent.Children.Count; i++)
            { token.ThrowIfCancellationRequested(); if (ReferenceEquals(parent.Children[i], node)) index = i; }
            if (index < 0) return Refuse(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot, "The actual selected descriptor is not owned by its parent.");
            var step = direction == AuthoringLiteralElementMoveDirection.Up ? -1 : 1;
            var neighborIndex = index + step;
            AuthoringNodeDescriptor? separator = null;
            if (neighborIndex >= 0 && neighborIndex < parent.Children.Count && parent.Children[neighborIndex].Kind == AuthoringNodeKind.Text)
            {
                separator = parent.Children[neighborIndex];
                if (!White(separator.Text, token)) return Refuse(AuthoringLiteralElementReorderFailureKind.UnavailableNeighbor, "Significant text cannot be crossed.");
                neighborIndex += step;
            }
            if (neighborIndex < 0 || neighborIndex >= parent.Children.Count)
                return Refuse(AuthoringLiteralElementReorderFailureKind.UnavailableNeighbor, "No adjacent element exists in this direction; movement does not wrap.");
            var neighbor = parent.Children[neighborIndex];
            if (neighbor.Kind != AuthoringNodeKind.Element || neighbor.ElementName?.NamespaceUri != "")
                return Refuse(AuthoringLiteralElementReorderFailureKind.UnavailableNeighbor, "The immediate neighbor is unsupported; movement does not skip content.");
            var next = LiteralElementLifecyclePipeline.Discover(s, neighbor, token);
            if (next.Failure is not null) return new(null, Convert(s, next.Failure));
            if (!ReferenceEquals(next.Data!.Parent, parent))
                return Refuse(AuthoringLiteralElementReorderFailureKind.UnsupportedCorrespondence, "Both actual elements must have the same parent.");
            var a = step < 0 ? next.Data : original.Data;
            var b = step < 0 ? original.Data : next.Data;
            var source = s.SourceFor(s.PrincipalModule.ModuleUri);
            if (a.End > b.Start || separator is null && a.End != b.Start
                || separator is not null && (source.CoordinateMap.RangeFromCharOffsets(a.End, b.Start) != separator.Range || !LexicalWhite(source.Text.AsSpan(a.End, b.Start - a.End), token)))
                return Refuse(AuthoringLiteralElementReorderFailureKind.SourceAlignmentFailure, "Only absent or exact ordinary lexical whitespace between aligned element slices is supported.");
            var pairRange = source.CoordinateMap.RangeFromCharOffsets(a.Start, b.End);
            if (pairRange.ByteLength > AuthoringCapabilities.MaxLiteralElementReorderExchangeBytes || Evidence(source.OriginalBytes.Length, pairRange.ByteLength) > AuthoringCapabilities.MaxLiteralElementReorderEvidenceBytes)
                return Refuse(AuthoringLiteralElementReorderFailureKind.SizeLimit, "The complete exchange and copied review evidence must fit their independent limits.");
            token.ThrowIfCancellationRequested();
            return new(new(new(s, node, neighbor, parent, a.Node, b.Node, separator, direction, a.Start, a.End, b.Start, b.End)), null);
        }
        catch (OperationCanceledException) { return Refuse(AuthoringLiteralElementReorderFailureKind.Cancelled, "Cancelled without target authority."); }
    }

    internal static AuthoringLiteralElementReorderResult Propose(AuthoringSnapshot s, AuthoringLiteralElementReorderProposal proposal, CancellationToken token)
    {
        AuthoringLiteralElementReorderResult Refuse(AuthoringLiteralElementReorderFailureKind kind, string message) => new(null, null, Fail(s, kind, message));
        try
        {
            token.ThrowIfCancellationRequested();
            var target = proposal.Target; var data = target.Data;
            if (!ReferenceEquals(data.Snapshot, s)) return Refuse(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot, "The direction-bound target belongs to another snapshot.");
            var discovery = Discover(s, data.Node, data.Direction, token);
            if (discovery.Failure is not null) return new(null, null, discovery.Failure);
            if (data != discovery.Target!.Data)
                return Refuse(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot, "Retained adjacency, coverage and actual descriptor authority must still match.");
            var source = s.SourceFor(s.PrincipalModule.ModuleUri);
            var a = data.Earlier.Range; var b = data.Later.Range;
            var at = checked((int)a.StartByteOffset); var ae = checked(at + (int)a.ByteLength);
            var bt = checked((int)b.StartByteOffset); var end = checked(bt + (int)b.ByteLength);
            var spanLength = checked(end - at);
            if (source.OriginalBytes.Length > AuthoringCapabilities.MaxLiteralElementReorderSourceBytes || spanLength > AuthoringCapabilities.MaxLiteralElementReorderExchangeBytes
                || Evidence(source.OriginalBytes.Length, spanLength) > AuthoringCapabilities.MaxLiteralElementReorderEvidenceBytes)
                return Refuse(AuthoringLiteralElementReorderFailureKind.SizeLimit, "Independent candidate/source/exchange/evidence limits were exceeded.");
            // Equal-length exchange: copy P, B, W, A, S directly from retained strict bytes.
            var bytes = new byte[source.OriginalBytes.Length];
            var original = source.OriginalBytes.Span;
            Copy(original[..at], bytes, token);
            var write = at;
            Copy(original[bt..end], bytes.AsSpan(write), token); write = checked(write + end - bt);
            Copy(original[ae..bt], bytes.AsSpan(write), token); write = checked(write + bt - ae);
            Copy(original[at..ae], bytes.AsSpan(write), token); write = checked(write + ae - at);
            if (write != end) return Refuse(AuthoringLiteralElementReorderFailureKind.SourceAlignmentFailure, "Exchange arithmetic does not exhaust the original pair span.");
            Copy(original[end..], bytes.AsSpan(end), token);
            if (Equal(original, bytes, token))
            { token.ThrowIfCancellationRequested(); return new(null, new(target), null); }
            if (!AuthoringSource.TryCreate(bytes, source.BaseUri, out var freshSource, out var sourceFailure))
                return Refuse(AuthoringLiteralElementReorderFailureKind.InvalidCandidate, sourceFailure!.Message);
            var xml = LiteralElementLifecyclePipeline.CheckXml(freshSource!, token);
            if (xml is not null)
                return Refuse(xml.Value.Kind == AuthoringLiteralElementFailureKind.InvalidOriginal ? AuthoringLiteralElementReorderFailureKind.InvalidCandidate
                    : Enum.Parse<AuthoringLiteralElementReorderFailureKind>(xml.Value.Kind.ToString()), xml.Value.Message);
            var inspection = XsltAuthoring.Inspect(freshSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspection.IsSuccess) return Refuse(AuthoringLiteralElementReorderFailureKind.InvalidCandidate, inspection.Failure!.Message);
            var fresh = inspection.Snapshot!;
            var bounded = Bounds(fresh, token);
            if (bounded is not null) return new(null, null, bounded);
            var preflight = LiteralElementLifecyclePipeline.Preflight(fresh, token);
            if (preflight is not null) return new(null, null, Convert(fresh, preflight));
            var scope = LiteralAttributeLifecyclePipeline.Scope(fresh, null, token, out _);
            if (scope is not null) return new(null, null, Convert(fresh, scope));
            var validation = LiteralAttributeLifecyclePipeline.Validate(fresh, true, token);
            if (validation is not null) return new(null, null, Convert(fresh, validation));
            var proof = new Proof(source, freshSource!, data, token);
            if (!proof.Match(s.PrincipalModule.Root, fresh.PrincipalModule.Root))
                return Refuse(AuthoringLiteralElementReorderFailureKind.UnsupportedCorrespondence, "A total actual node/attribute bijection with exactly the declared sibling exchange could not be proven.");
            var oldRange = source.CoordinateMap.RangeFromCharOffsets(data.Start, data.End);
            var newRange = freshSource!.CoordinateMap.RangeFromCharOffsets(data.Start, data.End);
            if (oldRange.StartByteOffset != newRange.StartByteOffset || oldRange.ByteLength != newRange.ByteLength
                || !Equal(original[..at], bytes.AsSpan(0, at), token) || !Equal(original[end..], bytes.AsSpan(end), token)
                || !Equal(original[bt..end], bytes.AsSpan(at, end - bt), token)
                || !Equal(original[ae..bt], bytes.AsSpan(at + end - bt, bt - ae), token)
                || !Equal(original[at..ae], bytes.AsSpan(end - (ae - at), ae - at), token))
                return Refuse(AuthoringLiteralElementReorderFailureKind.SourceAlignmentFailure, "Full same-original one-edit replay and component byte proof failed.");
            var result = new AuthoringLiteralElementReorderCandidate(s, fresh.WithSuccessfulCompilation(), data, proof.Map, proof.Attributes, new(oldRange, newRange));
            token.ThrowIfCancellationRequested();
            return new(result, null, null);
        }
        catch (OperationCanceledException) { return Refuse(AuthoringLiteralElementReorderFailureKind.Cancelled, "Cancelled without candidate or NoChange authority."); }
    }

    // Retained before/after envelopes plus complete old/new spans and full component review payloads.
    private static long Evidence(long source, long exchange) => checked(2 * source + 4 * exchange);

    private static AuthoringLiteralElementReorderFailure? Bounds(AuthoringSnapshot s, CancellationToken token)
    {
        if (s.Modules.Count != 1 || s.PrincipalModule.Edges.Count != 0)
            return Fail(s, AuthoringLiteralElementReorderFailureKind.UnsupportedModule, "One dependency-free principal module is required.");
        if (s.SourceFor(s.PrincipalModule.ModuleUri).OriginalBytes.Length > AuthoringCapabilities.MaxLiteralElementReorderSourceBytes)
            return Fail(s, AuthoringLiteralElementReorderFailureKind.SizeLimit, "Original/candidate physical source exceeds its independent byte bound.");
        var stack = new Stack<(AuthoringNodeDescriptor Node, int Depth)>(); stack.Push((s.PrincipalModule.Root, 0));
        var nodes = 0; var attrs = 0;
        while (stack.TryPop(out var item))
        {
            token.ThrowIfCancellationRequested(); nodes++; attrs = checked(attrs + item.Node.Attributes.Count);
            if (nodes > AuthoringCapabilities.MaxLiteralElementReorderNodes || attrs > AuthoringCapabilities.MaxLiteralElementReorderAttributes || item.Depth > AuthoringCapabilities.MaxLiteralElementReorderDepth)
                return Fail(s, AuthoringLiteralElementReorderFailureKind.SizeLimit, "Independent traversal node/attribute/depth limit exceeded.");
            if (item.Node.Children.Count > AuthoringCapabilities.MaxLiteralElementReorderNodes - nodes - stack.Count)
                return Fail(s, AuthoringLiteralElementReorderFailureKind.SizeLimit, "The pending traversal would exceed the node bound before stack allocation.");
            foreach (var child in item.Node.Children) stack.Push((child, item.Depth + 1));
        }
        return null;
    }

    private sealed class Proof(AuthoringSource original, AuthoringSource candidate, LiteralElementReorderData data, CancellationToken token)
    {
        internal Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> Map { get; } = new();
        internal List<AuthoringLiteralAttributeCorrespondence> Attributes { get; } = new();
        private SourceRange? Translate(SourceRange range, int delta)
        {
            var start = original.CoordinateMap.CharOffsetFromLineColumn(range.StartLine, range.StartColumn);
            var end = original.CoordinateMap.CharOffsetFromLineColumn(range.EndLine, range.EndColumn);
            if (original.CoordinateMap.RangeFromCharOffsets(start, end) != range)
                return null;
            if ((long)start + delta < 0 || (long)end + delta > candidate.Text.Length || end < start) return null;
            return candidate.CoordinateMap.RangeFromCharOffsets(checked(start + delta), checked(end + delta));
        }
        private bool Raw(SourceRange a, SourceRange b) => a.ByteLength == b.ByteLength && Equal(
            original.OriginalBytes.Span.Slice(checked((int)a.StartByteOffset), checked((int)a.ByteLength)),
            candidate.OriginalBytes.Span.Slice(checked((int)b.StartByteOffset), checked((int)b.ByteLength)), token);

        internal bool Match(AuthoringNodeDescriptor oldRoot, AuthoringNodeDescriptor freshRoot)
        {
            if (original.OriginalBytes.Length != candidate.OriginalBytes.Length || original.Text.Length != candidate.Text.Length) return false;
            var used = new HashSet<AuthoringNodeDescriptor>();
            var stack = new Stack<(AuthoringNodeDescriptor Old, AuthoringNodeDescriptor Fresh, int Delta)>();
            stack.Push((oldRoot, freshRoot, 0));
            var exchanged = false; var attributes = 0;
            while (stack.TryPop(out var entry))
            {
                token.ThrowIfCancellationRequested(); var old = entry.Old; var fresh = entry.Fresh;
                if (old.Kind != fresh.Kind || old.ElementName != fresh.ElementName || old.Text != fresh.Text
                    || Translate(old.Range, entry.Delta) != fresh.Range || old.Attributes.Count != fresh.Attributes.Count || old.Children.Count != fresh.Children.Count
                    || !Map.TryAdd(old, fresh) || !used.Add(fresh)) return false;
                // Containers of the exchanged pair have changed child order/content bytes, but fixed extents.
                var containsPair = old.Range.StartByteOffset <= data.Earlier.Range.StartByteOffset
                    && checked(old.Range.StartByteOffset + old.Range.ByteLength) >= checked(data.Later.Range.StartByteOffset + data.Later.Range.ByteLength);
                if (!containsPair && !Raw(old.Range, fresh.Range)) return false;
                var attrs = new Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>();
                var usedAttrs = new HashSet<AuthoringAttributeDescriptor>();
                for (var i = 0; i < old.Attributes.Count; i++)
                {
                    token.ThrowIfCancellationRequested(); var a = old.Attributes[i]; var b = fresh.Attributes[i]; attributes++;
                    if ((a.Name, a.LocalName, a.NamespaceUri, a.Prefix, a.RawLiteral, a.ExpandedValue, a.SlotKind)
                        != (b.Name, b.LocalName, b.NamespaceUri, b.Prefix, b.RawLiteral, b.ExpandedValue, b.SlotKind)
                        || Translate(a.FullRange, entry.Delta) != b.FullRange || Translate(a.ValueRange, entry.Delta) != b.ValueRange
                        || !Raw(a.FullRange, b.FullRange) || !Raw(a.ValueRange, b.ValueRange) || !attrs.TryAdd(a, b) || !usedAttrs.Add(b)) return false;
                }
                if (old.Kind == AuthoringNodeKind.Element) Attributes.Add(new(old, fresh, attrs));
                var first = -1; var last = -1;
                if (ReferenceEquals(old, data.Parent))
                {
                    if (exchanged) return false; exchanged = true;
                    for (var i = 0; i < old.Children.Count; i++)
                    { if (ReferenceEquals(old.Children[i], data.Earlier)) first = i; if (ReferenceEquals(old.Children[i], data.Later)) last = i; }
                    if (first < 0 || last != first + (data.Separator is null ? 1 : 2)
                        || data.Separator is not null && !ReferenceEquals(old.Children[first + 1], data.Separator)) return false;
                }
                for (var i = old.Children.Count - 1; i >= 0; i--)
                {
                    token.ThrowIfCancellationRequested(); var destination = i; var delta = entry.Delta;
                    if (first >= 0)
                    {
                        if (i == first) { destination = last; delta = checked(data.End - data.EarlierEnd); }
                        else if (i == last) { destination = first; delta = checked(data.Start - data.LaterStart); }
                        else if (data.Separator is not null && i == first + 1)
                            delta = checked((data.End - data.LaterStart) - (data.EarlierEnd - data.Start));
                    }
                    stack.Push((old.Children[i], fresh.Children[destination], delta));
                }
            }
            // Match visits every child on both sides exactly once; independent inventory confirms exhaustion.
            return exchanged && Map.Count == Count(oldRoot, token) && used.Count == Count(freshRoot, token)
                && attributes == Attributes.Sum(a => a.Survivors.Count);
        }
    }
    private static int Count(AuthoringNodeDescriptor root, CancellationToken token)
    {
        var stack = new Stack<AuthoringNodeDescriptor>(); stack.Push(root); var count = 0;
        while (stack.TryPop(out var n)) { token.ThrowIfCancellationRequested(); count++; foreach (var child in n.Children) stack.Push(child); }
        return count;
    }
    private static bool White(string? text, CancellationToken token) => text is not null && White(text.AsSpan(), token);
    // Ordinary XML text can spell whitespace using numeric references. CDATA/DTD/general
    // entities have already been excluded by closed preflight; retain each reference byte-exactly.
    private static bool LexicalWhite(ReadOnlySpan<char> text, CancellationToken token)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if ((i & 4095) == 0) token.ThrowIfCancellationRequested();
            if (text[i] is ' ' or '\t' or '\r' or '\n') continue;
            if (text[i] != '&' || i + 2 >= text.Length || text[i + 1] != '#') return false;
            var digits = i + 2; var hex = text[digits] == 'x'; if (hex) digits++;
            var end = digits;
            while (end < text.Length && text[end] != ';') { if ((end & 4095) == 0) token.ThrowIfCancellationRequested(); end++; }
            if (end == text.Length || !int.TryParse(text[digits..end], hex ? System.Globalization.NumberStyles.AllowHexSpecifier : System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var value) || value is not (9 or 10 or 13 or 32)) return false;
            i = end;
        }
        return true;
    }
    private static bool White(ReadOnlySpan<char> text, CancellationToken token)
    { for (var i = 0; i < text.Length; i++) { if ((i & 4095) == 0) token.ThrowIfCancellationRequested(); if (text[i] is not (' ' or '\t' or '\r' or '\n')) return false; } return true; }
    private static bool Equal(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, CancellationToken token)
    {
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i += Math.Min(65536, a.Length - i))
        { token.ThrowIfCancellationRequested(); var length = Math.Min(65536, a.Length - i); if (!a.Slice(i, length).SequenceEqual(b.Slice(i, length))) return false; }
        return true;
    }
    private static void Copy(ReadOnlySpan<byte> source, Span<byte> destination, CancellationToken token)
    {
        for (var i = 0; i < source.Length; i += Math.Min(65536, source.Length - i))
        { token.ThrowIfCancellationRequested(); var length = Math.Min(65536, source.Length - i); source.Slice(i, length).CopyTo(destination.Slice(i, length)); }
    }
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
