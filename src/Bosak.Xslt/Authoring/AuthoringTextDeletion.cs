// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 October 2026
// PURPOSE              : Immutable targets, results and complete provenance for bounded literal text deletion.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>Classifies a refused deletion without changing its input.</summary>
public enum AuthoringTextDeletionFailureKind
{
    /// <summary>The capability is unavailable.</summary>
    CapabilityUnavailable,
    /// <summary>The target authority is not recognized.</summary>
    UnknownTarget,
    /// <summary>The target or descriptor belongs to another snapshot.</summary>
    ForeignSnapshot,
    /// <summary>The node or body is outside the supported deletion scope.</summary>
    IneligibleInstruction,
    /// <summary>Only one dependency-free principal module is supported.</summary>
    UnsupportedModule,
    /// <summary>The effective stylesheet profile is unsupported.</summary>
    UnsupportedProfile,
    /// <summary>Static coverage is incomplete or a resource was requested.</summary>
    UnsupportedCoverage,
    /// <summary>Significant text, CDATA or preserved context is unsupported.</summary>
    MixedContent,
    /// <summary>The source encoding is unsupported.</summary>
    UnsupportedEncoding,
    /// <summary>The original or candidate exceeds the bound.</summary>
    SizeLimit,
    /// <summary>The original fails restricted static validation.</summary>
    InvalidOriginal,
    /// <summary>The candidate fails restricted static validation.</summary>
    InvalidCandidate,
    /// <summary>The instruction does not align to retained source.</summary>
    SourceAlignmentFailure,
    /// <summary>Complete graph and fragment provenance could not be proven.</summary>
    UnsupportedCorrespondence,
    /// <summary>Restricted resource acquisition was denied.</summary>
    ResourceDenied,
    /// <summary>Cancellation was observed without returning authority.</summary>
    Cancelled,
}

/// <summary>An immutable classified deletion refusal.</summary>
public sealed class AuthoringTextDeletionFailure
{
    internal AuthoringTextDeletionFailure(AuthoringTextDeletionFailureKind kind, string message, string? errorCode, Uri moduleUri, SourceRange? range)
    { Kind = kind; Message = message; ErrorCode = errorCode; ModuleUri = moduleUri; Range = range; }
    /// <summary>Gets the classification.</summary>
    public AuthoringTextDeletionFailureKind Kind { get; }
    /// <summary>Gets a diagnostic description.</summary>
    public string Message { get; }
    /// <summary>Gets an optional underlying static diagnostic code.</summary>
    public string? ErrorCode { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri { get; }
    /// <summary>Gets the related original range when known.</summary>
    public SourceRange? Range { get; }
}

/// <summary>An opaque deletion target owned by exactly one snapshot.</summary>
public sealed class AuthoringTextDeletionTarget
{
    internal AuthoringTextDeletionTarget(AuthoringSnapshot owner, AuthoringNodeDescriptor node, AuthoringNodeDescriptor parent, bool selfClosing, SourceRange? body, AuthoringNodeDescriptor? payload)
    { Owner = owner; Instruction = node; Parent = parent; IsSelfClosing = selfClosing; OriginalBodyRange = body; OriginalPayloadNode = payload; }
    internal AuthoringSnapshot Owner { get; }
    /// <summary>Gets the actual original direct parent.</summary>
    public AuthoringNodeDescriptor Parent { get; }
    /// <summary>Gets the exact original instruction descriptor.</summary>
    public AuthoringNodeDescriptor Instruction { get; }
    /// <summary>Gets the full original instruction extent.</summary>
    public SourceRange OriginalRange => Instruction.Range;
    /// <summary>Gets whether the instruction has self-closing syntax.</summary>
    public bool IsSelfClosing { get; }
    /// <summary>Gets the actual paired body span, or null for self-closing syntax.</summary>
    public SourceRange? OriginalBodyRange { get; }
    /// <summary>Gets the actual original payload, if present.</summary>
    public AuthoringNodeDescriptor? OriginalPayloadNode { get; }
    /// <summary>Gets the original logical payload value.</summary>
    public string OriginalTextValue => OriginalPayloadNode?.Text ?? "";
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Owner.PrincipalModule.ModuleUri;
}

/// <summary>An immutable request to remove exactly one engine-issued instruction extent.</summary>
public sealed class AuthoringTextDeletionProposal
{
    /// <summary>Creates a deletion request without caller-supplied source ranges.</summary>
    /// <param name="target">The engine-issued deletion target.</param>
    /// <exception cref="ArgumentNullException">The target is null.</exception>
    public AuthoringTextDeletionProposal(AuthoringTextDeletionTarget target)
    { Target = target ?? throw new ArgumentNullException(nameof(target)); }
    /// <summary>Gets the snapshot-owned target.</summary>
    public AuthoringTextDeletionTarget Target { get; }
}

/// <summary>An exclusive success or classified refusal.</summary>
public sealed class AuthoringTextDeletionTargetResult
{
    internal AuthoringTextDeletionTargetResult(AuthoringTextDeletionTarget? target, AuthoringTextDeletionFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets the success value, otherwise null.</summary>
    public AuthoringTextDeletionTarget? Target { get; }
    /// <summary>Gets the refusal, otherwise null.</summary>
    public AuthoringTextDeletionFailure? Failure { get; }
    /// <summary>Gets whether the operation succeeded.</summary>
    public bool IsSuccess => Target is not null;
}

/// <summary>An exclusive success or classified refusal.</summary>
public sealed class AuthoringTextDeletionResult
{
    internal AuthoringTextDeletionResult(AuthoringTextDeletionCandidate? candidate, AuthoringTextDeletionFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets the success value, otherwise null.</summary>
    public AuthoringTextDeletionCandidate? Candidate { get; }
    /// <summary>Gets the refusal, otherwise null.</summary>
    public AuthoringTextDeletionFailure? Failure { get; }
    /// <summary>Gets whether the operation succeeded.</summary>
    public bool IsSuccess => Candidate is not null;
}

/// <summary>An isolated ordinary candidate with complete removal, survivor and merged-text evidence.</summary>
/// <remarks>Consumers verify both correspondence and merge groups; XML parsing stays engine-owned.</remarks>
public sealed class AuthoringTextDeletionCandidate
{
    internal AuthoringTextDeletionCandidate(AuthoringTextDeletionTarget target, AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor removed, AuthoringNodeDescriptor parent, SourceRange anchor,
        IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> map,
        IEnumerable<AuthoringTextMergeGroup> groups)
    {
        InputSnapshot = target.Owner; OriginalBodyRange = target.OriginalBodyRange;
        RemovedPayloadNode = target.OriginalPayloadNode; OriginalTextValue = target.OriginalTextValue; Snapshot = snapshot; RemovedNode = removed; SurvivingParent = parent;
        CandidateRange = anchor; NodeCorrespondence = map;
        RemovedNodes = Array.AsReadOnly(target.OriginalPayloadNode is { } payload ? new[] { removed, payload } : new[] { removed });
        TextMergeGroups = Array.AsReadOnly(groups.ToArray());
        SourceEdits = Array.AsReadOnly(new[] { new AuthoringInsertionSourceEdit(removed.Range, anchor) });
    }
    /// <summary>Gets the exact immutable input snapshot.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the isolated fresh ordinary candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the original body span, or null for self-closing syntax.</summary>
    public SourceRange? OriginalBodyRange { get; }
    /// <summary>Gets the actual removed payload, if present.</summary>
    public AuthoringNodeDescriptor? RemovedPayloadNode { get; }
    /// <summary>Gets the original logical payload value.</summary>
    public string OriginalTextValue { get; }
    /// <summary>Gets the removed original instruction.</summary>
    public AuthoringNodeDescriptor RemovedNode { get; }
    /// <summary>Gets the complete removed descriptor set.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> RemovedNodes { get; }
    /// <summary>Gets the proven surviving direct parent for current selection.</summary>
    public AuthoringNodeDescriptor SurvivingParent { get; }
    /// <summary>Gets the complete original instruction extent.</summary>
    public SourceRange OriginalRange => RemovedNode.Range;
    /// <summary>Gets the zero-length replacement range in the candidate.</summary>
    public SourceRange CandidateRange { get; }
    /// <summary>Gets the sole removal edit; its replacement range is empty.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Gets the injective map of actual descriptors outside removed and merged nodes.</summary>
    public IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> NodeCorrespondence { get; }
    /// <summary>Gets complete bounded whitespace merge evidence, excluded from the one-to-one map.</summary>
    public IReadOnlyList<AuthoringTextMergeGroup> TextMergeGroups { get; }
    /// <summary>Exports an independent copy of exact candidate bytes, including the retained BOM.</summary>
    /// <returns>Caller-owned candidate source bytes.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
