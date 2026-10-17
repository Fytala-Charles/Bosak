// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Remove one instruction and prove exact byte, graph and bounded whitespace-merge provenance.
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

using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class ValueOfDeletionPipeline
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";

    internal static AuthoringValueOfDeletionTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor node, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(snapshot.FindNodeById(node.Id), node))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.ForeignTarget, "The descriptor belongs to another snapshot."));
        if (snapshot.Modules.Count != 1 || snapshot.PrincipalModule.Edges.Count != 0)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.UnsupportedModule, "Only a dependency-free principal module is supported."));
        var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
        if (source.OriginalBytes.Length > AuthoringCapabilities.MaxValueOfDeletionSourceBytes)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.SizeLimit, "The original exceeds the deletion byte bound."));
        if (node.Kind != AuthoringNodeKind.Element || node.ElementName?.NamespaceUri != Xsl
            || node.ElementName.LocalName != "value-of" || node.Children.Count != 0
            || !node.Attributes.Any(a => a.NamespaceUri.Length == 0 && a.LocalName == "select"))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.IneligibleNode, "An empty-body value-of with select is required."));
        var parent = Parent(snapshot.PrincipalModule.Root, node, token);
        if (parent is null)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.IneligibleNode, "A supported principal constructor parent is required."));
        // Reuse source-derived scope and static execution exclusions before original compilation.
        var scopeFailure = ValueOfInsertionPipeline.CheckScope(snapshot, parent, token, out _, out var element);
        token.ThrowIfCancellationRequested();
        if (scopeFailure is not null) return new(null, Translate(snapshot, scopeFailure, false));
        if (element!.AncestorsAndSelf().Any(e => (string?)e.Attribute(XNamespace.Xml + "space") == "preserve"))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.MixedContent, "An ancestor preserves whitespace."));
        var start = Offset(source, node.Range, false);
        var end = SourceTagScanner.FindElementEnd(source.Text, start, out _, out _);
        if (source.CoordinateMap.RangeFromCharOffsets(start, end) != node.Range)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.SourceAlignmentFailure, "The instruction extent is not aligned."));
        var index = parent.Children.ToList().IndexOf(node);
        if (index > 0 && index + 1 < parent.Children.Count
            && parent.Children[index - 1].Kind == AuthoringNodeKind.Text && parent.Children[index + 1].Kind == AuthoringNodeKind.Text
            && (!Whitespace(parent.Children[index - 1]) || !Whitespace(parent.Children[index + 1])))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.UnsupportedCorrespondence, "Only ordinary whitespace merges are supported."));
        var validation = ValueOfInsertionPipeline.Validate(snapshot, false, token);
        if (validation is not null) return new(null, Translate(snapshot, validation, false));
        token.ThrowIfCancellationRequested();
        return new(new(snapshot, node, parent), null);
    }

    internal static AuthoringValueOfDeletionResult Run(AuthoringSnapshot snapshot,
        AuthoringValueOfDeletionProposal proposal, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(proposal.Target.Owner, snapshot))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.ForeignTarget, "The target belongs to another snapshot."));
        var checkedTarget = Target(snapshot, proposal.Target.Node, token);
        if (!checkedTarget.IsSuccess) return new(null, checkedTarget.Failure);
        var target = checkedTarget.Target!;
        var source = snapshot.SourceFor(target.ModuleUri);
        var bytes = source.OriginalBytes;
        var a = checked((int)target.Node.Range.StartByteOffset);
        var b = checked(a + (int)target.Node.Range.ByteLength);
        if (a < source.ByteOffsetBias || b > bytes.Length || a >= b)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.SourceAlignmentFailure, "The removal extent is invalid."));
        var emitted = new byte[bytes.Length - (b - a)];
        bytes.Span[..a].CopyTo(emitted);
        bytes.Span[b..].CopyTo(emitted.AsSpan(a));
        token.ThrowIfCancellationRequested();
        if (!AuthoringSource.TryCreate(emitted, target.ModuleUri, out var candidateSource, out var sourceFailure))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.InvalidCandidate, sourceFailure!.Message));
        var inspection = XsltAuthoring.Inspect(candidateSource!, new DenyResolver(),
            new AuthoringInspectionOptions { AttemptCompilation = false, MaxModuleCount = 1 });
        token.ThrowIfCancellationRequested();
        if (!inspection.IsSuccess)
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.InvalidCandidate, inspection.Failure!.Message));
        var candidate = inspection.Snapshot!;
        var proof = new Proof(source, candidateSource!, target, token);
        if (!proof.Match(snapshot.PrincipalModule.Root, candidate.PrincipalModule.Root)
            || !proof.Map.TryGetValue(target.Parent, out var newParent))
            return new(null, Failure(snapshot, AuthoringDeletionFailureKind.UnsupportedCorrespondence, "The complete survivor and fragment relation could not be proven."));
        var validation = ValueOfInsertionPipeline.Validate(candidate, true, token);
        if (validation is not null) return new(null, Translate(snapshot, validation, true));
        var offset = Offset(source, target.Node.Range, false);
        var anchor = candidateSource!.CoordinateMap.RangeFromCharOffsets(offset, offset);
        token.ThrowIfCancellationRequested();
        return new(new(snapshot, candidate.WithSuccessfulCompilation(), target.Node, newParent, anchor,
            new ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(proof.Map), proof.Groups), null);
    }

    private static AuthoringNodeDescriptor? Parent(AuthoringNodeDescriptor root, AuthoringNodeDescriptor node, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        foreach (var child in root.Children)
        {
            if (ReferenceEquals(child, node)) return root;
            var found = Parent(child, node, token);
            if (found is not null) return found;
        }
        return null;
    }

    private static bool Whitespace(AuthoringNodeDescriptor n) => n.Kind == AuthoringNodeKind.Text
        && n.Range.ByteLength > 0 && n.Text is not null && n.Text.All(c => c is ' ' or '\t' or '\r' or '\n')
        && n.BackingObject is XText and not XCData;

    private static int Offset(AuthoringSource source, SourceRange range, bool end) => source.CoordinateMap.CharOffsetFromLineColumn(
        end ? range.EndLine : range.StartLine, end ? range.EndColumn : range.StartColumn);

    internal sealed class Proof(AuthoringSource original, AuthoringSource candidate,
        AuthoringValueOfDeletionTarget target, CancellationToken token)
    {
        internal Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> Map { get; } = new();
        internal List<AuthoringTextMergeGroup> Groups { get; } = new();
        private readonly HashSet<AuthoringNodeDescriptor> _used = new();
        private int A => Offset(original, target.Node.Range, false);
        private int B => Offset(original, target.Node.Range, true);
        private int Translate(int offset) => offset <= A ? offset : offset >= B ? offset - (B - A) : -1;
        private SourceRange? Range(SourceRange old)
        {
            var start = Translate(Offset(original, old, false));
            var end = Translate(Offset(original, old, true));
            return start < 0 || end < start || end > candidate.Text.Length
                ? null : candidate.CoordinateMap.RangeFromCharOffsets(start, end);
        }

        internal bool Match(AuthoringNodeDescriptor old, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(old, target.Node) || old.Kind != fresh.Kind || old.ElementName != fresh.ElementName
                || old.Text != fresh.Text || Range(old.Range) != fresh.Range || old.Attributes.Count != fresh.Attributes.Count
                || !_used.Add(fresh) || !Map.TryAdd(old, fresh)) return false;
            for (var i = 0; i < old.Attributes.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var x = old.Attributes[i]; var y = fresh.Attributes[i];
                if (x.Name != y.Name || x.NamespaceUri != y.NamespaceUri || x.RawLiteral != y.RawLiteral
                    || x.ExpandedValue != y.ExpandedValue || x.SlotKind != y.SlotKind
                    || Range(x.FullRange) != y.FullRange || Range(x.ValueRange) != y.ValueRange) return false;
            }
            var oi = 0; var ni = 0;
            while (oi < old.Children.Count)
            {
                token.ThrowIfCancellationRequested();
                var child = old.Children[oi];
                if (ReferenceEquals(child, target.Node)) { oi++; continue; }
                if (ReferenceEquals(old, target.Parent) && oi + 2 < old.Children.Count
                    && ReferenceEquals(old.Children[oi + 1], target.Node)
                    && old.Children[oi].Kind == AuthoringNodeKind.Text && old.Children[oi + 2].Kind == AuthoringNodeKind.Text)
                {
                    if (ni >= fresh.Children.Count || !Merge(child, old.Children[oi + 2], fresh.Children[ni])) return false;
                    oi += 3; ni++; continue;
                }
                if (ni >= fresh.Children.Count || !Match(child, fresh.Children[ni])) return false;
                oi++; ni++;
            }
            return ni == fresh.Children.Count;
        }

        private bool Merge(AuthoringNodeDescriptor left, AuthoringNodeDescriptor right, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            if (Groups.Count != 0 || !Whitespace(left) || !Whitespace(right) || !Whitespace(fresh)
                || !_used.Add(fresh) || Offset(original, left.Range, true) != A || Offset(original, right.Range, false) != B) return false;
            var l = Range(left.Range); var r = Range(right.Range);
            if (l is null || r is null || l.StartByteOffset != fresh.Range.StartByteOffset
                || l.StartByteOffset + l.ByteLength != r.StartByteOffset
                || r.StartByteOffset + r.ByteLength != fresh.Range.StartByteOffset + fresh.Range.ByteLength) return false;
            foreach (var pair in new[] { (left.Range, l), (right.Range, r) })
            {
                token.ThrowIfCancellationRequested();
                if (!original.OriginalBytes.Span.Slice((int)pair.Item1.StartByteOffset, (int)pair.Item1.ByteLength)
                    .SequenceEqual(candidate.OriginalBytes.Span.Slice((int)pair.Item2.StartByteOffset, (int)pair.Item2.ByteLength))) return false;
            }
            // Fresh ordinary inspection owns parsed Text, including CR/LF and character-reference semantics.
            Groups.Add(new(fresh, new[] { new AuthoringTextMergeFragment(left, left.Range, l), new AuthoringTextMergeFragment(right, right.Range, r) }));
            return true;
        }
    }

    private static AuthoringDeletionFailure Failure(AuthoringSnapshot snapshot, AuthoringDeletionFailureKind kind,
        string message, string? code = null) => new(kind, message, code, snapshot.PrincipalModule.ModuleUri, null);

    private static AuthoringDeletionFailure Translate(AuthoringSnapshot snapshot, AuthoringInsertionFailure failure, bool candidate)
    {
        var kind = failure.Kind switch
        {
            AuthoringInsertionFailureKind.ForeignSnapshot => AuthoringDeletionFailureKind.ForeignTarget,
            AuthoringInsertionFailureKind.UnsupportedModule => AuthoringDeletionFailureKind.UnsupportedModule,
            AuthoringInsertionFailureKind.UnsupportedProfile => AuthoringDeletionFailureKind.UnsupportedProfile,
            AuthoringInsertionFailureKind.UnsupportedEncoding => AuthoringDeletionFailureKind.UnsupportedEncoding,
            AuthoringInsertionFailureKind.SizeLimit => AuthoringDeletionFailureKind.SizeLimit,
            AuthoringInsertionFailureKind.MixedContent => AuthoringDeletionFailureKind.MixedContent,
            AuthoringInsertionFailureKind.IneligibleParent or AuthoringInsertionFailureKind.SelfClosingParent => AuthoringDeletionFailureKind.IneligibleNode,
            AuthoringInsertionFailureKind.InvalidOriginal or AuthoringInsertionFailureKind.InvalidCandidate => candidate ? AuthoringDeletionFailureKind.InvalidCandidate : AuthoringDeletionFailureKind.InvalidOriginal,
            AuthoringInsertionFailureKind.SourceAlignmentFailure => AuthoringDeletionFailureKind.SourceAlignmentFailure,
            _ => AuthoringDeletionFailureKind.UnsupportedCoverage,
        };
        return Failure(snapshot, kind, failure.Message, failure.ErrorCode);
    }

    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
