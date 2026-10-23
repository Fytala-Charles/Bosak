// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Literal element lifecycle authority, outcomes and complete immutable provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>The source mutation requested by an element candidate.</summary>
public enum AuthoringLiteralElementOperation
{
    /// <summary>Rename outcome.</summary>
    Rename,
    /// <summary>Delete outcome.</summary>
    Delete,
}

/// <summary>Exclusive rename outcome.</summary>
public enum AuthoringLiteralElementRenameOutcome
{
    /// <summary>Changed outcome.</summary>
    Changed,
    /// <summary>NoChange outcome.</summary>
    NoChange,
    /// <summary>Refused outcome.</summary>
    Refused,
}

/// <summary>Typed element lifecycle refusal.</summary>
public enum AuthoringLiteralElementFailureKind
{
    /// <summary>ForeignSnapshot outcome.</summary>
    ForeignSnapshot,
    /// <summary>IneligibleTarget outcome.</summary>
    IneligibleTarget,
    /// <summary>UnsupportedModule outcome.</summary>
    UnsupportedModule,
    /// <summary>UnsupportedProfile outcome.</summary>
    UnsupportedProfile,
    /// <summary>UnsupportedCoverage outcome.</summary>
    UnsupportedCoverage,
    /// <summary>UnsupportedEncoding outcome.</summary>
    UnsupportedEncoding,
    /// <summary>InvalidName outcome.</summary>
    InvalidName,
    /// <summary>InvalidOriginal outcome.</summary>
    InvalidOriginal,
    /// <summary>InvalidCandidate outcome.</summary>
    InvalidCandidate,
    /// <summary>SourceAlignmentFailure outcome.</summary>
    SourceAlignmentFailure,
    /// <summary>UnsupportedCorrespondence outcome.</summary>
    UnsupportedCorrespondence,
    /// <summary>SizeLimit outcome.</summary>
    SizeLimit,
    /// <summary>ResourceDenied outcome.</summary>
    ResourceDenied,
    /// <summary>Cancelled outcome.</summary>
    Cancelled,
}

/// <summary>A refusal without partial mutation authority.</summary>
public sealed class AuthoringLiteralElementFailure
{
    internal AuthoringLiteralElementFailure(AuthoringLiteralElementFailureKind kind, string message, Uri moduleUri, string? code)
    { Kind = kind; Message = message; ModuleUri = moduleUri; Code = code; }
    /// <summary>Gets Kind evidence owned by this operation.</summary>
    public AuthoringLiteralElementFailureKind Kind { get; }
    /// <summary>Gets Message evidence owned by this operation.</summary>
    public string Message { get; }
    /// <summary>Gets ModuleUri evidence owned by this operation.</summary>
    public Uri ModuleUri { get; }
    /// <summary>Gets Code evidence owned by this operation.</summary>
    public string? Code { get; }
}

internal sealed record LiteralElementTargetData(AuthoringSnapshot Snapshot, AuthoringNodeDescriptor Node, AuthoringNodeDescriptor Parent, int Start, int End, int OpeningEnd, bool SelfClosing);

/// <summary>Opaque snapshot-owned rename authority.</summary>
public sealed class AuthoringLiteralElementRenameTarget
{
    internal AuthoringLiteralElementRenameTarget(LiteralElementTargetData data) => Data = data;
    internal LiteralElementTargetData Data { get; }
    /// <summary>Gets the actual owned selected element.</summary>
    public AuthoringNodeDescriptor Node => Data.Node;
    /// <summary>Gets the actual surviving direct parent.</summary>
    public AuthoringNodeDescriptor Parent => Data.Parent;
    /// <summary>Gets the owning module.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
}

/// <summary>Exclusive owned target or refusal.</summary>
public sealed class AuthoringLiteralElementRenameTargetResult
{
    /// <summary>Gets whether checked target authority is available.</summary>
    public bool IsSuccess => Target is not null;
    internal AuthoringLiteralElementRenameTargetResult(AuthoringLiteralElementRenameTarget? target, AuthoringLiteralElementFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets Target evidence owned by this operation.</summary>
    public AuthoringLiteralElementRenameTarget? Target { get; }
    /// <summary>Gets Failure evidence owned by this operation.</summary>
    public AuthoringLiteralElementFailure? Failure { get; }
}

/// <summary>Immutable rename intent without caller source offsets.</summary>
public sealed class AuthoringLiteralElementRenameProposal
{
    /// <summary>Creates an explicit operation intent.</summary>
    /// <param name="target">The owned operation target.</param>
    /// <param name="newLocalName">Exact unqualified NCName; no normalization.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public AuthoringLiteralElementRenameProposal(AuthoringLiteralElementRenameTarget target, string newLocalName)
    {
        ArgumentNullException.ThrowIfNull(target); Target = target;
        ArgumentNullException.ThrowIfNull(newLocalName); NewLocalName = newLocalName;
    }
    /// <summary>Gets the owned operation target.</summary>
    public AuthoringLiteralElementRenameTarget Target { get; }
    /// <summary>Gets the exact requested local name.</summary>
    public string NewLocalName { get; }
}

/// <summary>Opaque snapshot-owned deletion authority.</summary>
public sealed class AuthoringLiteralElementDeletionTarget
{
    internal AuthoringLiteralElementDeletionTarget(LiteralElementTargetData data) => Data = data;
    internal LiteralElementTargetData Data { get; }
    /// <summary>Gets the actual owned selected element.</summary>
    public AuthoringNodeDescriptor Node => Data.Node;
    /// <summary>Gets the actual surviving direct parent.</summary>
    public AuthoringNodeDescriptor Parent => Data.Parent;
    /// <summary>Gets the owning module.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
}

/// <summary>Exclusive owned target or refusal.</summary>
public sealed class AuthoringLiteralElementDeletionTargetResult
{
    /// <summary>Gets whether checked target authority is available.</summary>
    public bool IsSuccess => Target is not null;
    internal AuthoringLiteralElementDeletionTargetResult(AuthoringLiteralElementDeletionTarget? target, AuthoringLiteralElementFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets Target evidence owned by this operation.</summary>
    public AuthoringLiteralElementDeletionTarget? Target { get; }
    /// <summary>Gets Failure evidence owned by this operation.</summary>
    public AuthoringLiteralElementFailure? Failure { get; }
}

/// <summary>Immutable deletion intent without caller source offsets.</summary>
public sealed class AuthoringLiteralElementDeletionProposal
{
    /// <summary>Creates an explicit operation intent.</summary>
    /// <param name="target">The owned operation target.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public AuthoringLiteralElementDeletionProposal(AuthoringLiteralElementDeletionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target); Target = target;
    }
    /// <summary>Gets the owned operation target.</summary>
    public AuthoringLiteralElementDeletionTarget Target { get; }
}

/// <summary>Eligibility-checked equal-name evidence with no candidate or Apply authority.</summary>
public sealed class AuthoringLiteralElementNoChange
{
    internal AuthoringLiteralElementNoChange(AuthoringLiteralElementRenameTarget target)
    { Target = target; }
    /// <summary>Gets Target evidence owned by this operation.</summary>
    public AuthoringLiteralElementRenameTarget Target { get; }
}

/// <summary>Exclusive changed, unchanged or refused result.</summary>
public sealed class AuthoringLiteralElementRenameResult
{
    /// <summary>Gets the exclusive outcome.</summary>
    public AuthoringLiteralElementRenameOutcome Outcome => Candidate is not null ? AuthoringLiteralElementRenameOutcome.Changed : NoChange is not null ? AuthoringLiteralElementRenameOutcome.NoChange : AuthoringLiteralElementRenameOutcome.Refused;
    internal AuthoringLiteralElementRenameResult(AuthoringLiteralElementCandidate? candidate, AuthoringLiteralElementNoChange? noChange, AuthoringLiteralElementFailure? failure)
    { Candidate = candidate; NoChange = noChange; Failure = failure; }
    /// <summary>Gets Candidate evidence owned by this operation.</summary>
    public AuthoringLiteralElementCandidate? Candidate { get; }
    /// <summary>Gets NoChange evidence owned by this operation.</summary>
    public AuthoringLiteralElementNoChange? NoChange { get; }
    /// <summary>Gets Failure evidence owned by this operation.</summary>
    public AuthoringLiteralElementFailure? Failure { get; }
}

/// <summary>Exclusive changed or refused deletion result.</summary>
public sealed class AuthoringLiteralElementDeletionResult
{
    /// <summary>Gets whether a complete candidate is available.</summary>
    public bool IsSuccess => Candidate is not null;
    internal AuthoringLiteralElementDeletionResult(AuthoringLiteralElementCandidate? candidate, AuthoringLiteralElementFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets Candidate evidence owned by this operation.</summary>
    public AuthoringLiteralElementCandidate? Candidate { get; }
    /// <summary>Gets Failure evidence owned by this operation.</summary>
    public AuthoringLiteralElementFailure? Failure { get; }
}

/// <summary>Complete immutable source edit and actual graph provenance.</summary>
public sealed class AuthoringLiteralElementCandidate
{
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Exports independent exact candidate bytes, including any BOM.</summary>
    /// <returns>A caller-owned byte array.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
    /// <summary>Exports independent original bytes for an owned original range.</summary>
    /// <param name="range">A range within the original module.</param>
    /// <returns>A caller-owned byte array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The range is outside the original module.</exception>
    public byte[] ExportOriginalRange(SourceRange range) => InputSnapshot.SourceFor(ModuleUri).OriginalBytes.Span.Slice(checked((int)range.StartByteOffset), checked((int)range.ByteLength)).ToArray();

    internal AuthoringLiteralElementCandidate(AuthoringSnapshot inputSnapshot, AuthoringSnapshot snapshot, AuthoringLiteralElementOperation operation, AuthoringNodeDescriptor originalNode, AuthoringNodeDescriptor candidateSelection, IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> nodeCorrespondence, IReadOnlyList<AuthoringLiteralAttributeCorrespondence> attributeCorrespondence, IReadOnlyList<AuthoringNodeDescriptor> removedNodes, IReadOnlyList<AuthoringAttributeDescriptor> removedAttributes, IReadOnlyList<AuthoringTextMergeGroup> mergedTextGroups, IReadOnlyList<AuthoringInsertionSourceEdit> sourceEdits)
    { InputSnapshot = inputSnapshot; Snapshot = snapshot; Operation = operation; OriginalNode = originalNode; CandidateSelection = candidateSelection; NodeCorrespondence = nodeCorrespondence; AttributeCorrespondence = attributeCorrespondence; RemovedNodes = removedNodes; RemovedAttributes = removedAttributes; MergedTextGroups = mergedTextGroups; SourceEdits = sourceEdits; }
    /// <summary>Gets InputSnapshot evidence owned by this operation.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets Snapshot evidence owned by this operation.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets Operation evidence owned by this operation.</summary>
    public AuthoringLiteralElementOperation Operation { get; }
    /// <summary>Gets OriginalNode evidence owned by this operation.</summary>
    public AuthoringNodeDescriptor OriginalNode { get; }
    /// <summary>Gets CandidateSelection evidence owned by this operation.</summary>
    public AuthoringNodeDescriptor CandidateSelection { get; }
    /// <summary>Gets NodeCorrespondence evidence owned by this operation.</summary>
    public IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> NodeCorrespondence { get; }
    /// <summary>Gets AttributeCorrespondence evidence owned by this operation.</summary>
    public IReadOnlyList<AuthoringLiteralAttributeCorrespondence> AttributeCorrespondence { get; }
    /// <summary>Gets RemovedNodes evidence owned by this operation.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> RemovedNodes { get; }
    /// <summary>Gets RemovedAttributes evidence owned by this operation.</summary>
    public IReadOnlyList<AuthoringAttributeDescriptor> RemovedAttributes { get; }
    /// <summary>Gets MergedTextGroups evidence owned by this operation.</summary>
    public IReadOnlyList<AuthoringTextMergeGroup> MergedTextGroups { get; }
    /// <summary>Gets SourceEdits evidence owned by this operation.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
}
