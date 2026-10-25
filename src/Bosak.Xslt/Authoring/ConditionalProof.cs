// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Bounded conditional wrapper lifecycle and complete source provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

// Match the transformed ordered graph rather than guessing identities from names or old ids.
internal sealed class ConditionalProof(AuthoringSnapshot input, AuthoringSnapshot output, ConditionalTargetData target,
    bool wrap, IReadOnlyList<ConditionalChange> changes, CancellationToken token)
{
    internal readonly Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> Map = new();
    internal readonly List<AuthoringLiteralAttributeCorrespondence> Attributes = new();
    internal readonly List<AuthoringNodeDescriptor> CreatedNodes = new(), RemovedNodes = new();
    internal readonly List<AuthoringAttributeDescriptor> CreatedAttributes = new(), RemovedAttributes = new();
    internal readonly List<AuthoringTextMergeGroup> Groups = new();
    internal readonly List<AuthoringConditionalReparenting> Reparenting = new();
    private readonly HashSet<AuthoringNodeDescriptor> used = new();
    private readonly AuthoringSource original = input.SourceFor(input.PrincipalModule.ModuleUri);
    private readonly AuthoringSource candidate = output.SourceFor(output.PrincipalModule.ModuleUri);

    internal bool Prove()
    {
        token.ThrowIfCancellationRequested();
        if (!wrap) { RemovedNodes.Add(target.Node); RemovedAttributes.AddRange(target.Node.Attributes); }
        if (!Match(input.PrincipalModule.Root, output.PrincipalModule.Root)) return false;
        var old = ConditionalWrapperPipeline.Nodes(input.PrincipalModule.Root, token).ToArray();
        var fresh = ConditionalWrapperPipeline.Nodes(output.PrincipalModule.Root, token).ToArray();
        var fragments = Groups.SelectMany(g => g.Fragments.Select(f => f.OriginalNode)).ToArray();
        if (fragments.Distinct().Count() != fragments.Length || fragments.Any(Map.ContainsKey)
            || Map.Count + RemovedNodes.Count + fragments.Length != old.Length
            || Map.Count + CreatedNodes.Count + Groups.Count != fresh.Length || used.Count != fresh.Length
            || CreatedNodes.Count != (wrap ? 1 : 0) || CreatedAttributes.Count != (wrap ? 1 : 0)
            || RemovedNodes.Count != (wrap ? 0 : 1) || RemovedAttributes.Count != (wrap ? 0 : 1) || Groups.Count > 2) return false;
        var oldDepths = ConditionalWrapperPipeline.Depths(input.PrincipalModule.Root, token);
        var newDepths = ConditionalWrapperPipeline.Depths(output.PrincipalModule.Root, token);
        foreach (var pair in Map)
        {
            token.ThrowIfCancellationRequested();
            var inElement = Contains(target.Element.Range, pair.Key.Range);
            var innerWhitespace = !wrap && target.Node.Children.Contains(pair.Key) && pair.Key.Kind == AuthoringNodeKind.Text;
            var expected = inElement || innerWhitespace ? (wrap ? 1 : -1) : 0;
            if (newDepths[pair.Value] - oldDepths[pair.Key] != expected) return false;
        }
        var candidateParent = Map[target.Parent];
        if (wrap)
        {
            var wrapper = CreatedNodes.Single();
            Reparenting.Add(new(target.Element, Map[target.Element], target.Parent, wrapper, 1));
        }
        else
            foreach (var child in target.Node.Children)
            {
                token.ThrowIfCancellationRequested();
                if (Map.TryGetValue(child, out var freshChild)) Reparenting.Add(new(child, freshChild, target.Node, candidateParent, -1));
                // Merged children have explicit ordered fragment provenance instead of a false one-to-one edge.
            }
        return true;
    }

    private static bool Contains(SourceRange outer, SourceRange inner) => outer.StartByteOffset <= inner.StartByteOffset
        && inner.StartByteOffset + inner.ByteLength <= outer.StartByteOffset + outer.ByteLength;

    private int Translate(int offset)
    {
        var delta = 0;
        foreach (var c in changes)
        {
            if (offset < c.Start) break;
            if (offset < c.End) return checked(c.Start + delta);
            delta = checked(delta + c.Replacement.Length - (c.End - c.Start));
        }
        return checked(offset + delta);
    }
    private SourceRange Range(SourceRange range)
    {
        var a = ConditionalWrapperPipeline.Offset(original, range, false); var b = ConditionalWrapperPipeline.Offset(original, range, true);
        if (!wrap) return candidate.CoordinateMap.RangeFromCharOffsets(Translate(a), Translate(b));
        // Ownership resolves endpoints shared by E and the suffix (and by E and its ancestors).
        var opening = changes[0].Replacement.Length; var total = opening + changes[1].Replacement.Length;
        if (Contains(target.Element.Range, range)) { a += opening; b += opening; }
        else if (b <= target.Start) { }
        else if (a >= target.End) { a += total; b += total; }
        else { if (a > target.Start || b < target.End) throw new InvalidOperationException("Unexpected partially overlapping descriptor extent."); b += total; }
        return candidate.CoordinateMap.RangeFromCharOffsets(a, b);
    }
    private bool Match(AuthoringNodeDescriptor old, AuthoringNodeDescriptor fresh)
    {
        token.ThrowIfCancellationRequested();
        if (old.Kind != fresh.Kind || old.ElementName != fresh.ElementName || old.Text != fresh.Text || Range(old.Range) != fresh.Range
            || old.Attributes.Count != fresh.Attributes.Count || !used.Add(fresh) || !Map.TryAdd(old, fresh)) return false;
        var attrs = new Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>();
        for (var i = 0; i < old.Attributes.Count; i++)
        {
            token.ThrowIfCancellationRequested(); var a = old.Attributes[i]; var b = fresh.Attributes[i];
            if ((a.Name, a.LocalName, a.NamespaceUri, a.Prefix, a.RawLiteral, a.ExpandedValue, a.SlotKind)
                != (b.Name, b.LocalName, b.NamespaceUri, b.Prefix, b.RawLiteral, b.ExpandedValue, b.SlotKind)
                || Range(a.FullRange) != b.FullRange || Range(a.ValueRange) != b.ValueRange || !Bytes(a.FullRange, b.FullRange)) return false;
            attrs.Add(a, b);
        }
        if (old.Kind == AuthoringNodeKind.Element) Attributes.Add(new(old, fresh, attrs));
        // Every unchanged leaf/subtree byte is checked, while containing ancestors legitimately include the edits.
        if (!Contains(old.Range, target.Node.Range) && !Bytes(old.Range, fresh.Range)) return false;
        if (wrap)
        {
            if (old.Children.Count != fresh.Children.Count) return false;
            for (var i = 0; i < old.Children.Count; i++)
            {
                var child = old.Children[i]; var newChild = fresh.Children[i];
                if (ReferenceEquals(child, target.Element))
                {
                    if (newChild.ElementName != new AuthoringQName(target.Prefix!, ConditionalWrapperPipeline.Xsl, "if")
                        || newChild.Children.Count != 1 || newChild.Attributes.Count != 1 || !used.Add(newChild)) return false;
                    var test = newChild.Attributes[0];
                    if (test.Name != "test" || test.NamespaceUri != "" || test.SlotKind != AuthoringAttributeSlotKind.Expression) return false;
                    var expected = candidate.CoordinateMap.RangeFromCharOffsets(target.Start, target.End + changes.Sum(c => c.Replacement.Length));
                    if (newChild.Range != expected) return false;
                    CreatedNodes.Add(newChild); CreatedAttributes.Add(test);
                    if (!Match(child, newChild.Children[0])) return false;
                }
                else if (!Match(child, newChild)) return false;
            }
            return true;
        }
        var expectedChildren = old.Children.SelectMany(n => ReferenceEquals(n, target.Node) ? target.Node.Children : new[] { n }).ToArray();
        var at = 0;
        for (var i = 0; i < expectedChildren.Length;)
        {
            token.ThrowIfCancellationRequested();
            if (at >= fresh.Children.Count) return false;
            var child = expectedChildren[i];
            if (i + 1 < expectedChildren.Length && child.Kind == AuthoringNodeKind.Text && expectedChildren[i + 1].Kind == AuthoringNodeKind.Text)
            {
                if (!Merge(child, expectedChildren[i + 1], fresh.Children[at])) return false;
                i += 2;
            }
            else { if (!Match(child, fresh.Children[at])) return false; i++; }
            at++;
        }
        return at == fresh.Children.Count;
    }
    private bool Merge(AuthoringNodeDescriptor left, AuthoringNodeDescriptor right, AuthoringNodeDescriptor fresh)
    {
        token.ThrowIfCancellationRequested();
        if (Groups.Count >= 2 || !ConditionalWrapperPipeline.PlainWhitespace(original, left, token)
            || !ConditionalWrapperPipeline.PlainWhitespace(original, right, token)
            || !ConditionalWrapperPipeline.PlainWhitespace(candidate, fresh, token) || !used.Add(fresh)) return false;
        var l = Range(left.Range); var r = Range(right.Range);
        if (l.StartByteOffset != fresh.Range.StartByteOffset || l.StartByteOffset + l.ByteLength != r.StartByteOffset
            || r.StartByteOffset + r.ByteLength != fresh.Range.StartByteOffset + fresh.Range.ByteLength
            || !Bytes(left.Range, l) || !Bytes(right.Range, r)) return false;
        var a = ConditionalWrapperPipeline.Offset(candidate, fresh.Range, false); var b = ConditionalWrapperPipeline.Offset(candidate, fresh.Range, true);
        // Normalize the complete concatenated raw text, including a newly adjacent CR+LF seam.
        var logical = new System.Text.StringBuilder();
        for (var i = a; i < b; i++)
        {
            if ((i & 8191) == 0) token.ThrowIfCancellationRequested();
            var c = candidate.Text[i];
            if (c == '\r') { if (i + 1 < b && candidate.Text[i + 1] == '\n') i++; logical.Append('\n'); }
            else logical.Append(c);
        }
        if (logical.ToString() != fresh.Text) return false;
        Groups.Add(new(fresh, new[] { new AuthoringTextMergeFragment(left, left.Range, l), new AuthoringTextMergeFragment(right, right.Range, r) }));
        return true;
    }
    private bool Bytes(SourceRange a, SourceRange b)
    {
        if (a.ByteLength != b.ByteLength) return false;
        var old = original.OriginalBytes.Span.Slice(checked((int)a.StartByteOffset), checked((int)a.ByteLength));
        var fresh = candidate.OriginalBytes.Span.Slice(checked((int)b.StartByteOffset), checked((int)b.ByteLength));
        for (var i = 0; i < old.Length; i += Math.Min(65536, old.Length - i))
        { token.ThrowIfCancellationRequested(); var count = Math.Min(65536, old.Length - i); if (!old.Slice(i, count).SequenceEqual(fresh.Slice(i, count))) return false; }
        return true;
    }
}
