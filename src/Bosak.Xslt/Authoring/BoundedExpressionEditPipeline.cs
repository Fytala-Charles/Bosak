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

using System.Collections.ObjectModel;
using System.Xml;

namespace Bosak.Xslt.Authoring;

internal static class BoundedExpressionEditPipeline
{
    internal static AuthoringEditResult Run(AuthoringSnapshot snapshot, AuthoringEditProposal proposal, CancellationToken token)
    {
        AuthoringEditResult Refuse(AuthoringEditFailureKind kind, string message) => AuthoringEditResult.FailureResult(new(kind, message));
        try
        {
            token.ThrowIfCancellationRequested();
            if (proposal.NewExpressionText.Length > AuthoringCapabilities.MaxConditionalConditionUnits)
                return Refuse(AuthoringEditFailureKind.SizeLimit, "The replacement exceeds the UTF-16 unit limit.");
            var validation = ConditionalWrapperPipeline.Check(snapshot, false, token);
            if (validation is not null) return Refuse(Convert(validation, false), validation.Message);
            var node = ConditionalWrapperPipeline.Nodes(snapshot.PrincipalModule.Root, token).SingleOrDefault(n => n.Id == proposal.OwningNodeId);
            if (node is null) return Refuse(AuthoringEditFailureKind.UnknownNode, "The actual slot owner is not present.");
            var slot = node.Attributes.SingleOrDefault(a => a.Name == proposal.AttributeName);
            if (slot?.SlotKind != AuthoringAttributeSlotKind.Expression)
                return Refuse(AuthoringEditFailureKind.SlotNotEditable, "An actual owned Expression slot is required.");
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var start = ConditionalWrapperPipeline.Offset(source, slot.ValueRange, false); var end = ConditionalWrapperPipeline.Offset(source, slot.ValueRange, true);
            var quote = source.Text[start - 1];
            var escaped = ConditionalWrapperPipeline.EscapeCondition(proposal.NewExpressionText, source.Encoding, quote, token);
            var compile = ExpressionEditPipeline.ValidateInStaticContext(snapshot, proposal.NewExpressionText, slot, node, source.CoordinateMap, expectedFailuresOnly: true);
            token.ThrowIfCancellationRequested(); if (compile is not null) return compile;
            var changes = new[] { new ConditionalChange(start, end, escaped) };
            var emitted = ConditionalWrapperPipeline.Emit(source, changes, 0, token);
            if (emitted.Failure is not null) return Refuse(Convert(emitted.Failure, true), emitted.Failure.Message);
            var inspection = ConditionalWrapperPipeline.Inspect(emitted.Source!, token);
            if (!inspection.IsSuccess) return Refuse(AuthoringEditFailureKind.InvalidCandidate, inspection.Failure!.Message);
            var fresh = inspection.Snapshot!;
            validation = ConditionalWrapperPipeline.Check(fresh, true, token);
            if (validation is not null) return Refuse(Convert(validation, true), validation.Message);
            var correspondence = new Dictionary<int, int>();
            var delta = escaped.Length - (end - start);
            SourceRange Range(SourceRange range)
            {
                int Translate(int x) => x <= start ? x : x >= end ? checked(x + delta) : throw new InvalidOperationException("Unexpected overlapping descriptor.");
                return emitted.Source!.CoordinateMap.RangeFromCharOffsets(Translate(ConditionalWrapperPipeline.Offset(source, range, false)), Translate(ConditionalWrapperPipeline.Offset(source, range, true)));
            }
            bool Proof(AuthoringNodeDescriptor old, AuthoringNodeDescriptor candidate)
            {
                token.ThrowIfCancellationRequested();
                if (old.Kind != candidate.Kind || old.ElementName != candidate.ElementName || old.Text != candidate.Text
                    || old.Children.Count != candidate.Children.Count || old.Attributes.Count != candidate.Attributes.Count || Range(old.Range) != candidate.Range
                    || !correspondence.TryAdd(old.Id, candidate.Id)) return false;
                for (var i = 0; i < old.Attributes.Count; i++)
                {
                    token.ThrowIfCancellationRequested(); var a = old.Attributes[i]; var b = candidate.Attributes[i]; var changed = ReferenceEquals(a, slot);
                    if ((a.Name,a.Prefix,a.LocalName,a.NamespaceUri,a.SlotKind) != (b.Name,b.Prefix,b.LocalName,b.NamespaceUri,b.SlotKind)
                        || Range(a.FullRange) != b.FullRange || (changed ? b.ExpandedValue != proposal.NewExpressionText || b.RawLiteral != escaped
                            || b.ValueRange != emitted.Edits![0].ReplacementRange : a.RawLiteral != b.RawLiteral || a.ExpandedValue != b.ExpandedValue || Range(a.ValueRange) != b.ValueRange)) return false;
                }
                for (var i = 0; i < old.Children.Count; i++) if (!Proof(old.Children[i], candidate.Children[i])) return false;
                return true;
            }
            if (!Proof(snapshot.PrincipalModule.Root, fresh.PrincipalModule.Root))
                return Refuse(AuthoringEditFailureKind.UnsupportedCorrespondence, "Complete nodes and per-owner attributes with exactly one changed slot were not proved.");
            var newNode = fresh.FindNodeById(correspondence[node.Id])!; var newSlot = newNode.Attributes.Single(a => a.Name == slot.Name);
            var result = new AuthoringEditCandidate(emitted.Source!.OriginalBytes.ToArray(), Array.AsReadOnly(new[] { slot.ValueRange }), fresh.WithSuccessfulCompilation(),
                new ReadOnlyDictionary<int,int>(correspondence), new(node.Id, slot.Name, slot.ValueRange, newSlot.ValueRange, slot.RawLiteral, newSlot.RawLiteral), defensiveExports: true);
            token.ThrowIfCancellationRequested(); return AuthoringEditResult.SuccessResult(result);
        }
        catch (ConditionalLimitException ex) { return Refuse(AuthoringEditFailureKind.SizeLimit, ex.Message); }
        catch (XmlException ex) { return Refuse(AuthoringEditFailureKind.ExpressionParseError, ex.Message); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return Refuse(AuthoringEditFailureKind.Cancelled, "Cancelled without candidate authority."); }
    }
    private static AuthoringEditFailureKind Convert(AuthoringConditionalFailure failure, bool candidate) => failure.Kind switch
    {
        AuthoringConditionalFailureKind.Cancelled => AuthoringEditFailureKind.Cancelled,
        AuthoringConditionalFailureKind.SizeLimit => AuthoringEditFailureKind.SizeLimit,
        AuthoringConditionalFailureKind.InvalidOriginal => AuthoringEditFailureKind.InvalidOriginal,
        AuthoringConditionalFailureKind.InvalidCandidate => AuthoringEditFailureKind.InvalidCandidate,
        _ => candidate ? AuthoringEditFailureKind.InvalidCandidate : AuthoringEditFailureKind.UnsupportedProfile,
    };
}
