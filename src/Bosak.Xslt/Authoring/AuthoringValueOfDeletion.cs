// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Immutable targets, results and complete provenance for bounded value-of deletion.
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

/// <summary>Classifies a refused deletion without changing its input.</summary>
public enum AuthoringDeletionFailureKind
{
    /// <summary>The target or descriptor belongs to another snapshot.</summary>
    ForeignTarget,
    /// <summary>The node or body is outside the supported deletion scope.</summary>
    IneligibleNode,
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
}

/// <summary>An immutable classified deletion refusal.</summary>
public sealed class AuthoringDeletionFailure
{
    internal AuthoringDeletionFailure(AuthoringDeletionFailureKind kind, string message, string? errorCode, Uri moduleUri, SourceRange? range)
    { Kind = kind; Message = message; ErrorCode = errorCode; ModuleUri = moduleUri; Range = range; }
    /// <summary>Gets the classification.</summary>
    public AuthoringDeletionFailureKind Kind { get; }
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
public sealed class AuthoringValueOfDeletionTarget
{
    internal AuthoringValueOfDeletionTarget(AuthoringSnapshot owner, AuthoringNodeDescriptor node, AuthoringNodeDescriptor parent)
    { Owner = owner; Node = node; Parent = parent; }
    internal AuthoringSnapshot Owner { get; }
    internal AuthoringNodeDescriptor Parent { get; }
    /// <summary>Gets the exact original instruction descriptor.</summary>
    public AuthoringNodeDescriptor Node { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Owner.PrincipalModule.ModuleUri;
}

/// <summary>An immutable request to remove exactly one engine-issued instruction extent.</summary>
public sealed class AuthoringValueOfDeletionProposal
{
    /// <summary>Creates a deletion request without caller-supplied source ranges.</summary>
    /// <param name="target">The engine-issued deletion target.</param>
    /// <exception cref="ArgumentNullException">The target is null.</exception>
    public AuthoringValueOfDeletionProposal(AuthoringValueOfDeletionTarget target)
    { Target = target ?? throw new ArgumentNullException(nameof(target)); }
    /// <summary>Gets the snapshot-owned target.</summary>
    public AuthoringValueOfDeletionTarget Target { get; }
}

/// <summary>An exclusive success or classified refusal.</summary>
public sealed class AuthoringValueOfDeletionTargetResult
{
    internal AuthoringValueOfDeletionTargetResult(AuthoringValueOfDeletionTarget? target, AuthoringDeletionFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets the success value, otherwise null.</summary>
    public AuthoringValueOfDeletionTarget? Target { get; }
    /// <summary>Gets the refusal, otherwise null.</summary>
    public AuthoringDeletionFailure? Failure { get; }
    /// <summary>Gets whether the operation succeeded.</summary>
    public bool IsSuccess => Target is not null;
}

/// <summary>An exclusive success or classified refusal.</summary>
public sealed class AuthoringValueOfDeletionResult
{
    internal AuthoringValueOfDeletionResult(AuthoringValueOfDeletionCandidate? candidate, AuthoringDeletionFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets the success value, otherwise null.</summary>
    public AuthoringValueOfDeletionCandidate? Candidate { get; }
    /// <summary>Gets the refusal, otherwise null.</summary>
    public AuthoringDeletionFailure? Failure { get; }
    /// <summary>Gets whether the operation succeeded.</summary>
    public bool IsSuccess => Candidate is not null;
}

/// <summary>One complete surviving original text extent and its exact candidate byte fragment.</summary>
public sealed class AuthoringTextMergeFragment
{
    internal AuthoringTextMergeFragment(AuthoringNodeDescriptor originalNode, SourceRange originalRange, SourceRange candidateRange)
    { OriginalNode = originalNode; OriginalRange = originalRange; CandidateRange = candidateRange; }
    /// <summary>Gets the actual original snapshot text descriptor.</summary>
    public AuthoringNodeDescriptor OriginalNode { get; }
    /// <summary>Gets the full original lexical range.</summary>
    public SourceRange OriginalRange { get; }
    /// <summary>Gets the corresponding candidate lexical fragment, not a node identity.</summary>
    public SourceRange CandidateRange { get; }
}

/// <summary>Provenance of two original whitespace nodes becoming one ordinary inspected text node.</summary>
public sealed class AuthoringTextMergeGroup
{
    internal AuthoringTextMergeGroup(AuthoringNodeDescriptor candidateNode, IEnumerable<AuthoringTextMergeFragment> fragments)
    { CandidateNode = candidateNode; Fragments = Array.AsReadOnly(fragments.ToArray()); }
    /// <summary>Gets the actual fresh candidate text descriptor; XML semantics are engine-owned.</summary>
    public AuthoringNodeDescriptor CandidateNode { get; }
    /// <summary>Gets full original extents and ordered contiguous candidate fragments.</summary>
    public IReadOnlyList<AuthoringTextMergeFragment> Fragments { get; }
}

/// <summary>An isolated ordinary candidate with complete removal, survivor and merged-text evidence.</summary>
/// <remarks>Consumers verify both correspondence and merge groups; XML parsing stays engine-owned.</remarks>
public sealed class AuthoringValueOfDeletionCandidate
{
    internal AuthoringValueOfDeletionCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor removed, AuthoringNodeDescriptor parent, SourceRange anchor,
        IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> map,
        IEnumerable<AuthoringTextMergeGroup> groups)
    {
        InputSnapshot = input; Snapshot = snapshot; RemovedNode = removed; SurvivingParent = parent;
        CandidateRange = anchor; NodeCorrespondence = map;
        RemovedNodes = Array.AsReadOnly(new[] { removed });
        TextMergeGroups = Array.AsReadOnly(groups.ToArray());
        SourceEdits = Array.AsReadOnly(new[] { new AuthoringInsertionSourceEdit(removed.Range, anchor) });
    }
    /// <summary>Gets the exact immutable input snapshot.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the isolated fresh ordinary candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the sole removed original instruction.</summary>
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
