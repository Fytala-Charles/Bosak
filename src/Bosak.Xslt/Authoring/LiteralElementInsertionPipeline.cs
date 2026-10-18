// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Append a paired no-namespace literal element with complete source and identity evidence.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class LiteralElementInsertionPipeline
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    internal static AuthoringLiteralElementInsertionTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor parent, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(parent.Id), parent))
                return new(null, Failure(AuthoringInsertionFailureKind.ForeignSnapshot, "The parent belongs to another snapshot."));
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            if (source.OriginalBytes.Length > AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes)
                return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The original exceeds the literal element byte bound."));
            var failure = ValueOfInsertionPipeline.CheckScope(snapshot, parent, token, out var offset, out var context);
            if (failure is not null) return new(null, failure);
            if (context!.Document!.Root!.DescendantsAndSelf().Any(e => e.Name == Xsl + "namespace-alias"))
                return new(null, Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, "Namespace aliases are outside literal element coverage."));
            foreach (var text in context.Document.Root.DescendantsAndSelf().Where(e => e.Name == Xsl + "text"))
            {
                token.ThrowIfCancellationRequested();
                var expand = text.AncestorsAndSelf().Select(e =>
                    (string?)e.Attribute(e.Name.Namespace == Xsl ? XName.Get("expand-text") : Xsl + "expand-text"))
                    .FirstOrDefault(v => v is not null);
                if (expand is not (null or "no" or "false" or "0")
                    || (string?)text.Attribute("disable-output-escaping") is not (null or "no")
                    || text.Nodes().Any(n => n is not XText || n is XCData))
                    return new(null, Failure(AuthoringInsertionFailureKind.UnsupportedCoverage,
                        "Literal element insertion accepts only non-expanding ordinary literal text without output escaping or body markup."));
            }
            failure = ValueOfInsertionPipeline.Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            return failure is null ? new(new(snapshot, new(snapshot, parent, offset, context)), null) : new(null, failure);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    internal static AuthoringLiteralElementInsertionResult Run(AuthoringSnapshot snapshot,
        AuthoringLiteralElementInsertionProposal proposal, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(proposal.Target.Owner, snapshot))
                return new(null, Failure(AuthoringInsertionFailureKind.ForeignSnapshot, "The target belongs to another snapshot."));
            var discovery = Target(snapshot, proposal.Target.Parent, token);
            if (!discovery.IsSuccess) return new(null, discovery.Failure);
            var target = discovery.Target!.Position;
            var source = snapshot.SourceFor(target.ModuleUri);
            var name = proposal.LocalName;
            // Bound the repeated name before calling the synchronous XML name validator or allocating markup.
            if ((long)name.Length * 2 > AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes)
                return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The repeated name exceeds the byte bound."));
            for (var i = 0; i < name.Length; i++) token.ThrowIfCancellationRequested();
            try { XmlConvert.VerifyNCName(name); }
            catch (Exception ex) when (ex is XmlException or ArgumentException)
            { return new(null, Failure(AuthoringInsertionFailureKind.InvalidElementName, "An exact supported NCName is required.")); }
            token.ThrowIfCancellationRequested();
            var reset = target.ContextElement.GetDefaultNamespace().NamespaceName.Length != 0 ? " xmlns=\"\"" : "";
            // Physical markup delimiters must also use the retained encoding.
            var length = 2L * source.Encoding.GetByteCount(name) + source.Encoding.GetByteCount("<></>" + reset);
            if (OverLimit(length, source, proposal.FormattingMode))
                return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The encoded element exceeds the byte bound."));
            var opening = "<" + name + reset + ">";
            var instruction = opening + "</" + name + ">";
            var layout = ValueOfInsertionLayout.Create(source, target, instruction, proposal.FormattingMode, token,
                out var sizeLimit, AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes);
            if (layout is null) return new(null, Failure(sizeLimit ? AuthoringInsertionFailureKind.SizeLimit : AuthoringInsertionFailureKind.UnsupportedFormatting,
                sizeLimit ? "The candidate exceeds the literal element byte bound." : "Readable formatting cannot be proven in this context."));
            var bytes = layout.Emit(source, token, AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes);
            if (bytes is null) return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The candidate exceeds the literal element byte bound."));
            token.ThrowIfCancellationRequested();
            if (!AuthoringSource.TryCreate(bytes, target.ModuleUri, out var newSource, out var sourceFailure))
                return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, sourceFailure!.Message));
            var inspection = XsltAuthoring.Inspect(newSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspection.IsSuccess) return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, inspection.Failure!.Message));
            var candidate = inspection.Snapshot!;
            var range = newSource!.CoordinateMap.RangeFromCharOffsets(layout.InstructionOffset, layout.InstructionOffset + instruction.Length);
            var bodyRange = newSource.CoordinateMap.RangeFromCharOffsets(layout.InstructionOffset + opening.Length,
                layout.InstructionOffset + opening.Length);
            var matches = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root)
                .Where(n => n.Kind == AuthoringNodeKind.Element && n.Range == range).ToArray();
            var inserted = matches.Length == 1 ? matches[0] : null;
            if (inserted?.ElementName?.NamespaceUri != "" || inserted.ElementName.LocalName != name
                || inserted.Children.Count != 0 || inserted.Attributes.Count != (reset.Length == 0 ? 0 : 1)
                || reset.Length != 0 && (inserted.Attributes[0].Name != "xmlns" || inserted.Attributes[0].ExpandedValue != ""))
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "The literal element did not round-trip exactly."));
            var validation = ValueOfInsertionPipeline.Validate(candidate, true, token);
            if (validation is not null) return new(null, validation);
            var map = ValueOfInsertionPipeline.Correspondence(snapshot, candidate, layout, inserted, token, insertedSubtree: true);
            if (map is null) return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "Complete old-node correspondence could not be proven."));
            if (!ProveCorrespondence(snapshot, candidate, map, layout, token))
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "Parent, text or attribute correspondence is incomplete."));
            var nodes = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).ToArray();
            var mapped = map.Values.ToHashSet();
            var added = nodes.Where(n => !mapped.Contains(n.Id)).ToArray();
            var parent = nodes.Single(n => n.Id == map[target.Parent.Id]);
            if (!parent.Children.Contains(inserted) || !added.Contains(inserted))
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "The actual insertion parent is ambiguous."));
            token.ThrowIfCancellationRequested();
            return new(new(snapshot, candidate.WithSuccessfulCompilation(),
                source.CoordinateMap.RangeFromCharOffsets(target.CharacterOffset, target.CharacterOffset), range, bodyRange,
                inserted, parent, name, proposal.FormattingMode, map, layout.Evidence(source, newSource), added), null);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    private static bool OverLimit(long length, AuthoringSource source, AuthoringInsertionFormattingMode mode) =>
        length > AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes
        || mode == AuthoringInsertionFormattingMode.PreserveSource && length + source.OriginalBytes.Length > AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes;
    private static bool ProveCorrespondence(AuthoringSnapshot input, AuthoringSnapshot candidate,
        IReadOnlyDictionary<int, int> map, ValueOfInsertionLayout layout, CancellationToken token)
    {
        var fresh = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).ToDictionary(n => n.Id);
        var reverse = map.Values.ToHashSet();
        var oldSource = input.SourceFor(input.PrincipalModule.ModuleUri);
        var newSource = candidate.SourceFor(candidate.PrincipalModule.ModuleUri);
        SourceRange Translate(SourceRange range)
        {
            var start = oldSource.CoordinateMap.CharOffsetFromLineColumn(range.StartLine, range.StartColumn);
            var end = oldSource.CoordinateMap.CharOffsetFromLineColumn(range.EndLine, range.EndColumn);
            var translated = layout.Translate(start, end, false);
            return newSource.CoordinateMap.RangeFromCharOffsets(translated.Start, translated.End);
        }
        foreach (var old in ValueOfInsertionPipeline.Nodes(input.PrincipalModule.Root))
        {
            token.ThrowIfCancellationRequested();
            var node = fresh[map[old.Id]];
            if (!old.Children.Select(c => map[c.Id]).SequenceEqual(node.Children.Where(c => reverse.Contains(c.Id)).Select(c => c.Id))
                || old.Attributes.Count != node.Attributes.Count) return false;
            for (var i = 0; i < old.Attributes.Count; i++)
            {
                var a = old.Attributes[i]; var b = node.Attributes[i];
                if (a.Name != b.Name || a.NamespaceUri != b.NamespaceUri || a.LocalName != b.LocalName
                    || a.RawLiteral != b.RawLiteral || a.ExpandedValue != b.ExpandedValue || a.SlotKind != b.SlotKind
                    || Translate(a.FullRange) != b.FullRange || Translate(a.ValueRange) != b.ValueRange) return false;
            }
            if (old.Text != node.Text && !layout.Edits.Any(e => e.End > e.Start
                && old.Range == oldSource.CoordinateMap.RangeFromCharOffsets(e.Start, e.End))) return false;
        }
        return true;
    }
    private static AuthoringInsertionFailure Failure(AuthoringInsertionFailureKind kind, string message) => new(kind, message);
    private static AuthoringInsertionFailure Cancelled() => Failure(AuthoringInsertionFailureKind.Cancelled, "Literal element insertion was cancelled; no candidate returned.");
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
