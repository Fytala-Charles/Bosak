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

namespace Bosak.Xslt.Authoring;

/// <summary>The structural operation bound to conditional-wrapper evidence.</summary>
public enum AuthoringConditionalOperation
{
    /// <summary>Insert an if wrapper around one literal element.</summary>
    Wrap,
    /// <summary>Remove only the if markup, retaining its contents.</summary>
    Unwrap,
}

/// <summary>Expected conditional-wrapper refusal categories; no refusal grants candidate authority.</summary>
public enum AuthoringConditionalFailureKind
{
    /// <summary>The descriptor or target belongs to another snapshot.</summary>
    ForeignSnapshot,
    /// <summary>The selected shape or ancestry is outside the profile.</summary>
    IneligibleTarget,
    /// <summary>A dependency-free principal module is required.</summary>
    UnsupportedModule,
    /// <summary>The captured authoring host or version is unsupported.</summary>
    UnsupportedProfile,
    /// <summary>The strict encoding is unsupported.</summary>
    UnsupportedEncoding,
    /// <summary>The module exceeds closed static or instruction coverage.</summary>
    UnsupportedCoverage,
    /// <summary>No bounded nonempty parent-context XSLT alias is available.</summary>
    NoSafeXsltPrefix,
    /// <summary>Namespace or inherited context cannot be preserved.</summary>
    UnsupportedNamespaceContext,
    /// <summary>The supplied condition is invalid in its actual slot context.</summary>
    InvalidCondition,
    /// <summary>The original module does not validate.</summary>
    InvalidOriginal,
    /// <summary>The candidate module does not validate.</summary>
    InvalidCandidate,
    /// <summary>Retained syntax and physical coordinates do not align.</summary>
    SourceAlignmentFailure,
    /// <summary>Complete node, attribute or merge provenance could not be proved.</summary>
    UnsupportedCorrespondence,
    /// <summary>An independent resource bound was exceeded.</summary>
    SizeLimit,
    /// <summary>Cooperative cancellation was requested.</summary>
    Cancelled,
    /// <summary>Restricted compilation refused resource acquisition.</summary>
    ResourceDenied,
}

/// <summary>A classified, immutable refusal without partial candidate authority.</summary>
public sealed class AuthoringConditionalFailure
{
    internal AuthoringConditionalFailure(AuthoringConditionalFailureKind kind, string message) { Kind = kind; Message = message; }
    /// <summary>Gets the refusal category.</summary>
    public AuthoringConditionalFailureKind Kind { get; }
    /// <summary>Gets its explanatory message.</summary>
    public string Message { get; }
}

internal sealed record ConditionalTargetData(AuthoringSnapshot Snapshot, AuthoringNodeDescriptor Node,
    AuthoringNodeDescriptor Element, AuthoringNodeDescriptor Parent, int Start, int End, int OpeningEnd, int ClosingStart, string? Prefix);

/// <summary>Opaque snapshot-owned wrap authority; only discovery constructs it.</summary>
public sealed class AuthoringConditionalWrapTarget
{
    internal AuthoringConditionalWrapTarget(ConditionalTargetData data) => Data = data;
    internal ConditionalTargetData Data { get; }
    /// <summary>Gets the selected actual original descriptor.</summary>
    public AuthoringNodeDescriptor Node => Data.Node;
    /// <summary>Gets the original containing parent.</summary>
    public AuthoringNodeDescriptor Parent => Data.Parent;
    /// <summary>Gets the owning principal module.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
}

/// <summary>Exactly one owned wrap target or typed refusal.</summary>
public sealed class AuthoringConditionalWrapTargetResult
{
    internal AuthoringConditionalWrapTargetResult(ConditionalTargetData? data, AuthoringConditionalFailure? failure)
    { Target = data is null ? null : new(data); Failure = failure; }
    /// <summary>Gets whether discovery granted authority.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the opaque target, when eligible.</summary>
    public AuthoringConditionalWrapTarget? Target { get; }
    /// <summary>Gets the refusal, when ineligible.</summary>
    public AuthoringConditionalFailure? Failure { get; }
}

/// <summary>Opaque snapshot-owned unwrap authority; only discovery constructs it.</summary>
public sealed class AuthoringConditionalUnwrapTarget
{
    internal AuthoringConditionalUnwrapTarget(ConditionalTargetData data) => Data = data;
    internal ConditionalTargetData Data { get; }
    /// <summary>Gets the selected actual original descriptor.</summary>
    public AuthoringNodeDescriptor Node => Data.Node;
    /// <summary>Gets the original containing parent.</summary>
    public AuthoringNodeDescriptor Parent => Data.Parent;
    /// <summary>Gets the owning principal module.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
}

/// <summary>Exactly one owned unwrap target or typed refusal.</summary>
public sealed class AuthoringConditionalUnwrapTargetResult
{
    internal AuthoringConditionalUnwrapTargetResult(ConditionalTargetData? data, AuthoringConditionalFailure? failure)
    { Target = data is null ? null : new(data); Failure = failure; }
    /// <summary>Gets whether discovery granted authority.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the opaque target, when eligible.</summary>
    public AuthoringConditionalUnwrapTarget? Target { get; }
    /// <summary>Gets the refusal, when ineligible.</summary>
    public AuthoringConditionalFailure? Failure { get; }
}

/// <summary>Explicit owned Wrap intent and logical XPath condition.</summary>
public sealed class AuthoringConditionalWrapProposal
{
    /// <summary>Captures a discovered target and unescaped condition.</summary>
    /// <param name="target">The exact discovered Wrap target.</param>
    /// <param name="conditionText">The logical XPath expression; the engine escapes XML.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringConditionalWrapProposal(AuthoringConditionalWrapTarget target, string conditionText)
    { ArgumentNullException.ThrowIfNull(target); ArgumentNullException.ThrowIfNull(conditionText); Target = target; ConditionText = conditionText; }
    /// <summary>Gets the operation-bound authority.</summary>
    public AuthoringConditionalWrapTarget Target { get; }
    /// <summary>Gets the unescaped XPath condition.</summary>
    public string ConditionText { get; }
}

/// <summary>Explicit owned Unwrap intent; no markup or range is caller supplied.</summary>
public sealed class AuthoringConditionalUnwrapProposal
{
    /// <summary>Captures an independently discovered Unwrap target.</summary>
    /// <param name="target">The exact discovered Unwrap target.</param>
    /// <exception cref="ArgumentNullException">The target is null.</exception>
    public AuthoringConditionalUnwrapProposal(AuthoringConditionalUnwrapTarget target)
    { ArgumentNullException.ThrowIfNull(target); Target = target; }
    /// <summary>Gets the operation-bound authority.</summary>
    public AuthoringConditionalUnwrapTarget Target { get; }
}

/// <summary>Exactly one changed structural candidate or refusal; no structural NoChange exists.</summary>
public sealed class AuthoringConditionalResult
{
    internal AuthoringConditionalResult(AuthoringConditionalCandidate? candidate, AuthoringConditionalFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets whether a changed candidate is available.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets the complete changed candidate.</summary>
    public AuthoringConditionalCandidate? Candidate { get; }
    /// <summary>Gets the typed refusal.</summary>
    public AuthoringConditionalFailure? Failure { get; }
}

/// <summary>Explicit approved parent-edge change; descendant order is retained.</summary>
public sealed class AuthoringConditionalReparenting
{
    internal AuthoringConditionalReparenting(AuthoringNodeDescriptor original, AuthoringNodeDescriptor candidate,
        AuthoringNodeDescriptor originalParent, AuthoringNodeDescriptor candidateParent, int depthDelta)
    { OriginalNode = original; CandidateNode = candidate; OriginalParent = originalParent; CandidateParent = candidateParent; DepthDelta = depthDelta; }
    /// <summary>Gets the actual original node.</summary>
    public AuthoringNodeDescriptor OriginalNode { get; }
    /// <summary>Gets the actual retained candidate node.</summary>
    public AuthoringNodeDescriptor CandidateNode { get; }
    /// <summary>Gets the original containing node.</summary>
    public AuthoringNodeDescriptor OriginalParent { get; }
    /// <summary>Gets the new containing node.</summary>
    public AuthoringNodeDescriptor CandidateParent { get; }
    /// <summary>Gets the depth change for this retained subtree.</summary>
    public int DepthDelta { get; }
}

/// <summary>Immutable exact-source edits and exhaustive ordinary-inspection graph provenance.</summary>
public sealed class AuthoringConditionalCandidate
{
    internal AuthoringConditionalCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot, AuthoringConditionalOperation operation,
        AuthoringNodeDescriptor selection, AuthoringAttributeDescriptor? test, ConditionalProof proof, IReadOnlyList<AuthoringInsertionSourceEdit> edits)
    {
        InputSnapshot = input; Snapshot = snapshot; Operation = operation; CandidateSelection = selection; CandidateTest = test;
        NodeCorrespondence = new ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(new Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(proof.Map));
        AttributeCorrespondence = Array.AsReadOnly(proof.Attributes.ToArray());
        CreatedNodes = Array.AsReadOnly(proof.CreatedNodes.ToArray()); CreatedAttributes = Array.AsReadOnly(proof.CreatedAttributes.ToArray());
        RemovedNodes = Array.AsReadOnly(proof.RemovedNodes.ToArray()); RemovedAttributes = Array.AsReadOnly(proof.RemovedAttributes.ToArray());
        MergedTextGroups = Array.AsReadOnly(proof.Groups.ToArray()); Reparenting = Array.AsReadOnly(proof.Reparenting.ToArray());
        SourceEdits = Array.AsReadOnly(edits.ToArray());
    }
    /// <summary>Gets the exact original snapshot.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the fresh validated ordinary inspection.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the operation proved by this evidence.</summary>
    public AuthoringConditionalOperation Operation { get; }
    /// <summary>Gets actual I after Wrap or retained E after Unwrap.</summary>
    public AuthoringNodeDescriptor CandidateSelection { get; }
    /// <summary>Gets the actual owned test slot after Wrap; null after Unwrap.</summary>
    public AuthoringAttributeDescriptor? CandidateTest { get; }
    /// <summary>Gets all simple surviving descriptor pairs, excluding merge participants.</summary>
    public IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> NodeCorrespondence { get; }
    /// <summary>Gets complete per-owner surviving attribute pairs.</summary>
    public IReadOnlyList<AuthoringLiteralAttributeCorrespondence> AttributeCorrespondence { get; }
    /// <summary>Gets created descriptors; exactly I for Wrap.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> CreatedNodes { get; }
    /// <summary>Gets created attributes; exactly test for Wrap.</summary>
    public IReadOnlyList<AuthoringAttributeDescriptor> CreatedAttributes { get; }
    /// <summary>Gets removed descriptors; exactly I for Unwrap.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> RemovedNodes { get; }
    /// <summary>Gets removed attributes; exactly test for Unwrap.</summary>
    public IReadOnlyList<AuthoringAttributeDescriptor> RemovedAttributes { get; }
    /// <summary>Gets up to two complete ordered boundary merges.</summary>
    public IReadOnlyList<AuthoringTextMergeGroup> MergedTextGroups { get; }
    /// <summary>Gets explicit approved parent and depth changes for retained nodes.</summary>
    public IReadOnlyList<AuthoringConditionalReparenting> Reparenting { get; }
    /// <summary>Gets exactly two edits in ascending same-original coordinates.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }

    /// <summary>Gets the owning principal module.</summary>
    public Uri ModuleUri => InputSnapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the closed conditional-wrapper profile.</summary>
    public string Profile => AuthoringCapabilities.ConditionalWrapperProfile;
    /// <summary>Gets the exact engine build identity.</summary>
    public string EngineVersion => AuthoringCapabilities.EngineVersion;
    /// <summary>Exports an independent complete candidate including BOM.</summary>
    /// <returns>Caller-owned candidate bytes.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
    /// <summary>Exports complete original payload for an evidence range.</summary>
    /// <param name="range">The original physical range.</param>
    /// <returns>Caller-owned original bytes.</returns>
    /// <exception cref="ArgumentNullException">The range is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The range exceeds the original source.</exception>
    public byte[] ExportOriginalRange(SourceRange range)
    {
        ArgumentNullException.ThrowIfNull(range);
        var bytes = InputSnapshot.SourceFor(ModuleUri).OriginalBytes;
        if (range.StartByteOffset < 0 || range.ByteLength < 0 || range.StartByteOffset > bytes.Length || range.ByteLength > bytes.Length - range.StartByteOffset)
            throw new ArgumentOutOfRangeException(nameof(range));
        return bytes.Span.Slice((int)range.StartByteOffset, (int)range.ByteLength).ToArray();
    }
}
