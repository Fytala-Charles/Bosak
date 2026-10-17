// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Append one literal xsl:text with bounded emission and complete source/node provenance.
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

using System.Buffers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class TextInsertionPipeline
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    internal static AuthoringTextInsertionTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor parent, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(parent.Id), parent))
                return new(null, Failure(AuthoringInsertionFailureKind.ForeignSnapshot, "The parent belongs to another snapshot."));
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            if (source.OriginalBytes.Length > AuthoringCapabilities.MaxTextInsertionSourceBytes)
                return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The original exceeds the text insertion byte bound."));
            var failure = ValueOfInsertionPipeline.CheckScope(snapshot, parent, token, out var offset, out var context);
            if (failure is not null) return new(null, failure);
            foreach (var text in context!.Document!.Root!.DescendantsAndSelf().Where(e => e.Name == Xsl + "text"))
            {
                token.ThrowIfCancellationRequested();
                var expand = text.AncestorsAndSelf().Select(e =>
                    (string?)e.Attribute(e.Name.Namespace == Xsl ? XName.Get("expand-text") : Xsl + "expand-text"))
                    .FirstOrDefault(v => v is not null);
                if (expand is not (null or "no" or "false" or "0")
                    || (string?)text.Attribute("disable-output-escaping") is not (null or "no")
                    || text.Nodes().Any(n => n is not XText || n is XCData))
                    return new(null, Failure(AuthoringInsertionFailureKind.UnsupportedCoverage,
                        "Text insertion accepts only non-expanding ordinary literal text without output escaping or body markup."));
            }
            failure = ValueOfInsertionPipeline.Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            return failure is null ? new(new(snapshot, new(snapshot, parent, offset, context)), null) : new(null, failure);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    internal static AuthoringTextInsertionResult Run(AuthoringSnapshot snapshot,
        AuthoringTextInsertionProposal proposal, CancellationToken token)
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
            var bindings = ValueOfInsertionPipeline.NamespaceBindings(target.ContextElement);
            var prefix = bindings.Where(p => p.Key.Length != 0 && p.Value == Xsl.NamespaceName).Select(p => p.Key)
                .OrderBy(p => p == "xsl" ? 0 : 1).ThenBy(p => p, StringComparer.Ordinal).FirstOrDefault();
            var declaration = "";
            if (prefix is null)
            {
                prefix = "xsl";
                for (var i = 1; bindings.ContainsKey(prefix); i++) prefix = "xsl" + i;
                declaration = $" xmlns:{prefix}=\"{Xsl}\"";
            }
            var opening = $"<{prefix}:text{declaration} expand-text=\"no\">";
            var closing = $"</{prefix}:text>";
            var payload = proposal.TextValue;
            long length = source.Encoding.GetByteCount(opening) + source.Encoding.GetByteCount(closing);
            long chars = 0;
            for (var i = 0; i < payload.Length;)
            {
                token.ThrowIfCancellationRequested();
                if (Rune.DecodeFromUtf16(payload.AsSpan(i), out var rune, out var used) != OperationStatus.Done
                    || rune.IsBmp && !XmlConvert.IsXmlChar((char)rune.Value))
                    return new(null, Failure(AuthoringInsertionFailureKind.InvalidPayload, "Literal payload contains invalid XML characters or unpaired surrogates."));
                var escaped = Escape(rune);
                chars += escaped?.Length ?? used;
                length += escaped is not null ? source.Encoding.GetByteCount(escaped)
                    : source.Encoding.CodePage == 65001 ? rune.Utf8SequenceLength : used * 2;
                if (OverLimit(length, source, proposal.FormattingMode))
                    return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The encoded literal insertion exceeds the byte bound."));
                i += used;
            }
            if (OverLimit(length, source, proposal.FormattingMode))
                return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The literal instruction exceeds the byte bound."));
            var escapedText = new StringBuilder(checked((int)chars));
            for (var i = 0; i < payload.Length;)
            {
                token.ThrowIfCancellationRequested();
                Rune.DecodeFromUtf16(payload.AsSpan(i), out var rune, out var used);
                var escaped = Escape(rune);
                if (escaped is not null) escapedText.Append(escaped); else escapedText.Append(payload.AsSpan(i, used));
                i += used;
            }
            var instruction = opening + escapedText.ToString() + closing;
            var layout = ValueOfInsertionLayout.Create(source, target, instruction, proposal.FormattingMode, token,
                out var sizeLimit, AuthoringCapabilities.MaxTextInsertionSourceBytes);
            if (layout is null) return new(null, Failure(sizeLimit ? AuthoringInsertionFailureKind.SizeLimit : AuthoringInsertionFailureKind.UnsupportedFormatting,
                sizeLimit ? "The candidate exceeds the text byte bound." : "Readable formatting cannot be proven in this context."));
            var bytes = layout.Emit(source, token, AuthoringCapabilities.MaxTextInsertionSourceBytes);
            if (bytes is null) return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The candidate exceeds the text byte bound."));
            token.ThrowIfCancellationRequested();
            if (!AuthoringSource.TryCreate(bytes, target.ModuleUri, out var newSource, out var sourceFailure))
                return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, sourceFailure!.Message));
            var inspection = XsltAuthoring.Inspect(newSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspection.IsSuccess) return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, inspection.Failure!.Message));
            var candidate = inspection.Snapshot!;
            var range = newSource!.CoordinateMap.RangeFromCharOffsets(layout.InstructionOffset, layout.InstructionOffset + instruction.Length);
            var payloadRange = newSource.CoordinateMap.RangeFromCharOffsets(layout.InstructionOffset + opening.Length,
                layout.InstructionOffset + instruction.Length - closing.Length);
            var matches = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).Where(n => n.Kind == AuthoringNodeKind.Element && n.Range == range).ToArray();
            var inserted = matches.Length == 1 ? matches[0] : null;
            var textNode = inserted?.Children.Count == 1 ? inserted.Children[0] : null;
            if (inserted?.ElementName?.NamespaceUri != Xsl.NamespaceName || inserted.ElementName.LocalName != "text"
                || (payload.Length == 0 ? inserted.Children.Count != 0 : textNode?.Kind != AuthoringNodeKind.Text || textNode.Text != payload || textNode.Range != payloadRange))
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "The literal instruction or payload did not round-trip exactly."));
            var validation = ValueOfInsertionPipeline.Validate(candidate, true, token);
            if (validation is not null) return new(null, validation);
            var map = ValueOfInsertionPipeline.Correspondence(snapshot, candidate, layout, inserted, token, insertedSubtree: true);
            if (map is null) return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "Complete old-node correspondence could not be proven."));
            token.ThrowIfCancellationRequested();
            return new(new(snapshot, candidate.WithSuccessfulCompilation(),
                source.CoordinateMap.RangeFromCharOffsets(target.CharacterOffset, target.CharacterOffset), range, payloadRange,
                inserted, textNode, payload, proposal.FormattingMode, map, layout.Evidence(source, newSource)), null);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    private static bool OverLimit(long length, AuthoringSource source, AuthoringInsertionFormattingMode mode) =>
        length > AuthoringCapabilities.MaxTextInsertionSourceBytes
        || mode == AuthoringInsertionFormattingMode.PreserveSource && length + source.OriginalBytes.Length > AuthoringCapabilities.MaxTextInsertionSourceBytes;
    private static string? Escape(Rune rune) => rune.Value switch
    { '&' => "&amp;", '<' => "&lt;", '>' => "&gt;", '\r' => "&#xD;", '\n' => "&#xA;", '\t' => "&#x9;", _ => null };
    private static AuthoringInsertionFailure Failure(AuthoringInsertionFailureKind kind, string message) => new(kind, message);
    private static AuthoringInsertionFailure Cancelled() => Failure(AuthoringInsertionFailureKind.Cancelled, "Text insertion was cancelled; no candidate returned.");
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
