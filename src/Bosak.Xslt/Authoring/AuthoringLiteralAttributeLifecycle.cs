// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Literal attribute lifecycle authority, outcomes and complete immutable provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>The explicit intent of a literal attribute candidate.</summary>
public enum AuthoringLiteralAttributeOperation
{
    /// <summary>Create one literal attribute.</summary>
    Create,
    /// <summary>Update one literal attribute.</summary>
    Update,
    /// <summary>Delete one literal attribute.</summary>
    Delete,
}

/// <summary>Exclusive Update outcome.</summary>
public enum AuthoringLiteralAttributeUpdateOutcome
{
    /// <summary>Changed outcome without partial evidence.</summary>
    Changed,
    /// <summary>NoChange outcome without partial evidence.</summary>
    NoChange,
    /// <summary>Refused outcome without partial evidence.</summary>
    Refused,
}

/// <summary>Typed literal attribute lifecycle refusals.</summary>
public enum AuthoringLiteralAttributeFailureKind
{
    /// <summary>CapabilityUnavailable refusal.</summary>
    CapabilityUnavailable,
    /// <summary>ForeignSnapshot refusal.</summary>
    ForeignSnapshot,
    /// <summary>UnknownTarget refusal.</summary>
    UnknownTarget,
    /// <summary>IneligibleOwner refusal.</summary>
    IneligibleOwner,
    /// <summary>IneligibleAttribute refusal.</summary>
    IneligibleAttribute,
    /// <summary>InvalidName refusal.</summary>
    InvalidName,
    /// <summary>InvalidValue refusal.</summary>
    InvalidValue,
    /// <summary>DuplicateAttribute refusal.</summary>
    DuplicateAttribute,
    /// <summary>UnsupportedModule refusal.</summary>
    UnsupportedModule,
    /// <summary>UnsupportedProfile refusal.</summary>
    UnsupportedProfile,
    /// <summary>UnsupportedCoverage refusal.</summary>
    UnsupportedCoverage,
    /// <summary>UnsupportedEncoding refusal.</summary>
    UnsupportedEncoding,
    /// <summary>SizeLimit refusal.</summary>
    SizeLimit,
    /// <summary>InvalidOriginal refusal.</summary>
    InvalidOriginal,
    /// <summary>InvalidCandidate refusal.</summary>
    InvalidCandidate,
    /// <summary>ResourceDenied refusal.</summary>
    ResourceDenied,
    /// <summary>SourceAlignmentFailure refusal.</summary>
    SourceAlignmentFailure,
    /// <summary>UnsupportedCorrespondence refusal.</summary>
    UnsupportedCorrespondence,
    /// <summary>Cancelled refusal.</summary>
    Cancelled,
}

/// <summary>A classified refusal with only input-owned diagnostic coordinates.</summary>
public sealed class AuthoringLiteralAttributeFailure
{
    internal AuthoringLiteralAttributeFailure(AuthoringLiteralAttributeFailureKind kind, string message, Uri moduleUri, string? code = null)
    { Kind = kind; Message = message; ModuleUri = moduleUri; Code = code; }
    /// <summary>Gets Kind evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailureKind Kind { get; }
    /// <summary>Gets Message evidence for this operation.</summary>
    public string Message { get; }
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri { get; }
    /// <summary>Gets Code evidence for this operation.</summary>
    public string? Code { get; }
    /// <summary>Gets Range evidence for this operation.</summary>
    public SourceRange? Range => null;
}

internal sealed record LiteralAttributeTargetData(AuthoringSnapshot Snapshot, AuthoringNodeDescriptor Owner, AuthoringAttributeDescriptor? Attribute, SourceRange OpeningTag, bool SelfClosing, char Quote, string? LogicalValue, int NameEnd);

/// <summary>Opaque snapshot-owned Creation authority.</summary>
public sealed class AuthoringLiteralAttributeCreationTarget
{
    internal AuthoringLiteralAttributeCreationTarget(LiteralAttributeTargetData data) => Data = data;
    internal LiteralAttributeTargetData Data { get; }
    /// <summary>Gets Owner evidence for this operation.</summary>
    public AuthoringNodeDescriptor Owner => Data.Owner;
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets OriginalOpeningTagRange evidence for this operation.</summary>
    public SourceRange OriginalOpeningTagRange => Data.OpeningTag;
    /// <summary>Gets IsSelfClosing evidence for this operation.</summary>
    public bool IsSelfClosing => Data.SelfClosing;
}

/// <summary>Exclusive Creation target or refusal.</summary>
public sealed class AuthoringLiteralAttributeCreationTargetResult
{
    internal AuthoringLiteralAttributeCreationTargetResult(AuthoringLiteralAttributeCreationTarget? target, AuthoringLiteralAttributeFailure? failure) { Target = target; Failure = failure; }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeCreationTarget? Target { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
    /// <summary>Gets IsSuccess evidence for this operation.</summary>
    public bool IsSuccess => Target is not null;
}

/// <summary>Immutable explicit Creation intent without caller source offsets.</summary>
public sealed class AuthoringLiteralAttributeCreationProposal
{
    /// <summary>Creates an operation-bound proposal.</summary>
    /// <param name="target">The target intent.</param>
    /// <param name="localName">The localName intent.</param>
    /// <param name="value">The value intent.</param>
    public AuthoringLiteralAttributeCreationProposal(AuthoringLiteralAttributeCreationTarget target, string localName, string value)
    {
        ArgumentNullException.ThrowIfNull(target); Target = target;
        ArgumentNullException.ThrowIfNull(localName); LocalName = localName;
        ArgumentNullException.ThrowIfNull(value); Value = value;
    }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeCreationTarget Target { get; }
    /// <summary>Gets LocalName evidence for this operation.</summary>
    public string LocalName { get; }
    /// <summary>Gets Value evidence for this operation.</summary>
    public string Value { get; }
}

/// <summary>Exclusive Creation result with no partial candidate authority.</summary>
public sealed class AuthoringLiteralAttributeCreationResult
{
    internal AuthoringLiteralAttributeCreationResult(AuthoringLiteralAttributeCandidate? candidate, AuthoringLiteralAttributeFailure? failure) { Candidate = candidate; Failure = failure; }
    /// <summary>Gets IsSuccess evidence for this operation.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets Candidate evidence for this operation.</summary>
    public AuthoringLiteralAttributeCandidate? Candidate { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
}

/// <summary>Opaque snapshot-owned Update authority.</summary>
public sealed class AuthoringLiteralAttributeUpdateTarget
{
    internal AuthoringLiteralAttributeUpdateTarget(LiteralAttributeTargetData data) => Data = data;
    internal LiteralAttributeTargetData Data { get; }
    /// <summary>Gets Owner evidence for this operation.</summary>
    public AuthoringNodeDescriptor Owner => Data.Owner;
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets OriginalOpeningTagRange evidence for this operation.</summary>
    public SourceRange OriginalOpeningTagRange => Data.OpeningTag;
    /// <summary>Gets IsSelfClosing evidence for this operation.</summary>
    public bool IsSelfClosing => Data.SelfClosing;
    /// <summary>Gets Attribute evidence for this operation.</summary>
    public AuthoringAttributeDescriptor Attribute => Data.Attribute!;
    /// <summary>Gets AttributeRange evidence for this operation.</summary>
    public SourceRange AttributeRange => Attribute.FullRange;
    /// <summary>Gets ValueRange evidence for this operation.</summary>
    public SourceRange ValueRange => Attribute.ValueRange;
    /// <summary>Gets QuoteDelimiter evidence for this operation.</summary>
    public char QuoteDelimiter => Data.Quote;
    /// <summary>Gets LogicalValue evidence for this operation.</summary>
    public string LogicalValue => Data.LogicalValue!;
}

/// <summary>Exclusive Update target or refusal.</summary>
public sealed class AuthoringLiteralAttributeUpdateTargetResult
{
    internal AuthoringLiteralAttributeUpdateTargetResult(AuthoringLiteralAttributeUpdateTarget? target, AuthoringLiteralAttributeFailure? failure) { Target = target; Failure = failure; }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeUpdateTarget? Target { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
    /// <summary>Gets IsSuccess evidence for this operation.</summary>
    public bool IsSuccess => Target is not null;
}

/// <summary>Immutable explicit Update intent without caller source offsets.</summary>
public sealed class AuthoringLiteralAttributeUpdateProposal
{
    /// <summary>Creates an operation-bound proposal.</summary>
    /// <param name="target">The target intent.</param>
    /// <param name="value">The value intent.</param>
    public AuthoringLiteralAttributeUpdateProposal(AuthoringLiteralAttributeUpdateTarget target, string value)
    {
        ArgumentNullException.ThrowIfNull(target); Target = target;
        ArgumentNullException.ThrowIfNull(value); Value = value;
    }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeUpdateTarget Target { get; }
    /// <summary>Gets Value evidence for this operation.</summary>
    public string Value { get; }
}

/// <summary>Exclusive Update result with no partial candidate authority.</summary>
public sealed class AuthoringLiteralAttributeUpdateResult
{
    internal AuthoringLiteralAttributeUpdateResult(AuthoringLiteralAttributeCandidate? candidate, AuthoringLiteralAttributeUpdateNoChange? noChange, AuthoringLiteralAttributeFailure? failure) { Candidate = candidate; NoChange = noChange; Failure = failure; }
    /// <summary>Gets NoChange evidence for this operation.</summary>
    public AuthoringLiteralAttributeUpdateNoChange? NoChange { get; }
    /// <summary>Gets Outcome evidence for this operation.</summary>
    public AuthoringLiteralAttributeUpdateOutcome Outcome => Candidate is not null ? AuthoringLiteralAttributeUpdateOutcome.Changed : NoChange is not null ? AuthoringLiteralAttributeUpdateOutcome.NoChange : AuthoringLiteralAttributeUpdateOutcome.Refused;
    /// <summary>Gets Candidate evidence for this operation.</summary>
    public AuthoringLiteralAttributeCandidate? Candidate { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
}

/// <summary>Opaque snapshot-owned Deletion authority.</summary>
public sealed class AuthoringLiteralAttributeDeletionTarget
{
    internal AuthoringLiteralAttributeDeletionTarget(LiteralAttributeTargetData data) => Data = data;
    internal LiteralAttributeTargetData Data { get; }
    /// <summary>Gets Owner evidence for this operation.</summary>
    public AuthoringNodeDescriptor Owner => Data.Owner;
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets OriginalOpeningTagRange evidence for this operation.</summary>
    public SourceRange OriginalOpeningTagRange => Data.OpeningTag;
    /// <summary>Gets IsSelfClosing evidence for this operation.</summary>
    public bool IsSelfClosing => Data.SelfClosing;
    /// <summary>Gets Attribute evidence for this operation.</summary>
    public AuthoringAttributeDescriptor Attribute => Data.Attribute!;
    /// <summary>Gets AttributeRange evidence for this operation.</summary>
    public SourceRange AttributeRange => Attribute.FullRange;
    /// <summary>Gets ValueRange evidence for this operation.</summary>
    public SourceRange ValueRange => Attribute.ValueRange;
    /// <summary>Gets QuoteDelimiter evidence for this operation.</summary>
    public char QuoteDelimiter => Data.Quote;
    /// <summary>Gets LogicalValue evidence for this operation.</summary>
    public string LogicalValue => Data.LogicalValue!;
}

/// <summary>Exclusive Deletion target or refusal.</summary>
public sealed class AuthoringLiteralAttributeDeletionTargetResult
{
    internal AuthoringLiteralAttributeDeletionTargetResult(AuthoringLiteralAttributeDeletionTarget? target, AuthoringLiteralAttributeFailure? failure) { Target = target; Failure = failure; }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeDeletionTarget? Target { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
    /// <summary>Gets IsSuccess evidence for this operation.</summary>
    public bool IsSuccess => Target is not null;
}

/// <summary>Immutable explicit Deletion intent without caller source offsets.</summary>
public sealed class AuthoringLiteralAttributeDeletionProposal
{
    /// <summary>Creates an operation-bound proposal.</summary>
    /// <param name="target">The target intent.</param>
    public AuthoringLiteralAttributeDeletionProposal(AuthoringLiteralAttributeDeletionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target); Target = target;
    }
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeDeletionTarget Target { get; }
}

/// <summary>Exclusive Deletion result with no partial candidate authority.</summary>
public sealed class AuthoringLiteralAttributeDeletionResult
{
    internal AuthoringLiteralAttributeDeletionResult(AuthoringLiteralAttributeCandidate? candidate, AuthoringLiteralAttributeFailure? failure) { Candidate = candidate; Failure = failure; }
    /// <summary>Gets IsSuccess evidence for this operation.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets Candidate evidence for this operation.</summary>
    public AuthoringLiteralAttributeCandidate? Candidate { get; }
    /// <summary>Gets Failure evidence for this operation.</summary>
    public AuthoringLiteralAttributeFailure? Failure { get; }
}

/// <summary>Equal Update intent, with no candidate, edit, export or Apply authority.</summary>
public sealed class AuthoringLiteralAttributeUpdateNoChange
{
    internal AuthoringLiteralAttributeUpdateNoChange(AuthoringLiteralAttributeUpdateTarget target) => Target = target;
    /// <summary>Gets Target evidence for this operation.</summary>
    public AuthoringLiteralAttributeUpdateTarget Target { get; }
    /// <summary>Gets InputSnapshot evidence for this operation.</summary>
    public AuthoringSnapshot InputSnapshot => Target.Data.Snapshot;
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri => Target.ModuleUri;
    /// <summary>Gets Value evidence for this operation.</summary>
    public string Value => Target.LogicalValue;
}

/// <summary>Complete ordinary attribute survivors for one actual mapped element.</summary>
public sealed class AuthoringLiteralAttributeCorrespondence
{
    internal AuthoringLiteralAttributeCorrespondence(AuthoringNodeDescriptor original, AuthoringNodeDescriptor candidate, Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor> survivors)
    { OriginalOwner = original; CandidateOwner = candidate; Survivors = new System.Collections.ObjectModel.ReadOnlyDictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>(new Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>(survivors)); }
    /// <summary>Gets OriginalOwner evidence for this operation.</summary>
    public AuthoringNodeDescriptor OriginalOwner { get; }
    /// <summary>Gets CandidateOwner evidence for this operation.</summary>
    public AuthoringNodeDescriptor CandidateOwner { get; }
    /// <summary>Gets Survivors evidence for this operation.</summary>
    public IReadOnlyDictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor> Survivors { get; }
}

/// <summary>A complete immutable attribute edit with actual graph and attribute provenance.</summary>
public sealed class AuthoringLiteralAttributeCandidate
{
    internal AuthoringLiteralAttributeCandidate(AuthoringSnapshot inputSnapshot, AuthoringSnapshot snapshot, AuthoringLiteralAttributeOperation operation, AuthoringNodeDescriptor originalOwner, AuthoringNodeDescriptor candidateOwner, SourceRange originalOpeningTagRange, SourceRange candidateOpeningTagRange, AuthoringQName addressedName, AuthoringAttributeDescriptor? originalAttribute, AuthoringAttributeDescriptor? candidateAttribute, string? originalLogicalValue, string? candidateLogicalValue, IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> nodeCorrespondence, IReadOnlyList<AuthoringLiteralAttributeCorrespondence> attributeCorrespondence, IReadOnlyList<AuthoringInsertionSourceEdit> sourceEdits)
    {
        InputSnapshot = inputSnapshot;
        Snapshot = snapshot;
        Operation = operation;
        OriginalOwner = originalOwner;
        CandidateOwner = candidateOwner;
        OriginalOpeningTagRange = originalOpeningTagRange;
        CandidateOpeningTagRange = candidateOpeningTagRange;
        AddressedName = addressedName;
        OriginalAttribute = originalAttribute;
        CandidateAttribute = candidateAttribute;
        OriginalLogicalValue = originalLogicalValue;
        CandidateLogicalValue = candidateLogicalValue;
        NodeCorrespondence = nodeCorrespondence;
        AttributeCorrespondence = attributeCorrespondence;
        SourceEdits = sourceEdits;
    }
    /// <summary>Gets InputSnapshot evidence for this operation.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets Snapshot evidence for this operation.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets Operation evidence for this operation.</summary>
    public AuthoringLiteralAttributeOperation Operation { get; }
    /// <summary>Gets OriginalOwner evidence for this operation.</summary>
    public AuthoringNodeDescriptor OriginalOwner { get; }
    /// <summary>Gets CandidateOwner evidence for this operation.</summary>
    public AuthoringNodeDescriptor CandidateOwner { get; }
    /// <summary>Gets OriginalOpeningTagRange evidence for this operation.</summary>
    public SourceRange OriginalOpeningTagRange { get; }
    /// <summary>Gets CandidateOpeningTagRange evidence for this operation.</summary>
    public SourceRange CandidateOpeningTagRange { get; }
    /// <summary>Gets AddressedName evidence for this operation.</summary>
    public AuthoringQName AddressedName { get; }
    /// <summary>Gets OriginalAttribute evidence for this operation.</summary>
    public AuthoringAttributeDescriptor? OriginalAttribute { get; }
    /// <summary>Gets CandidateAttribute evidence for this operation.</summary>
    public AuthoringAttributeDescriptor? CandidateAttribute { get; }
    /// <summary>Gets OriginalLogicalValue evidence for this operation.</summary>
    public string? OriginalLogicalValue { get; }
    /// <summary>Gets CandidateLogicalValue evidence for this operation.</summary>
    public string? CandidateLogicalValue { get; }
    /// <summary>Gets NodeCorrespondence evidence for this operation.</summary>
    public IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> NodeCorrespondence { get; }
    /// <summary>Gets AttributeCorrespondence evidence for this operation.</summary>
    public IReadOnlyList<AuthoringLiteralAttributeCorrespondence> AttributeCorrespondence { get; }
    /// <summary>Gets SourceEdits evidence for this operation.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Gets OriginalAttributeRange evidence for this operation.</summary>
    public SourceRange? OriginalAttributeRange => OriginalAttribute?.FullRange;
    /// <summary>Gets OriginalValueRange evidence for this operation.</summary>
    public SourceRange? OriginalValueRange => OriginalAttribute?.ValueRange;
    /// <summary>Gets CandidateAttributeRange evidence for this operation.</summary>
    public SourceRange? CandidateAttributeRange => CandidateAttribute?.FullRange;
    /// <summary>Gets CandidateValueRange evidence for this operation.</summary>
    public SourceRange? CandidateValueRange => CandidateAttribute?.ValueRange;
    /// <summary>Gets ModuleUri evidence for this operation.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Exports defensive exact candidate bytes.</summary>
    /// <returns>A caller-owned copy including the original BOM.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
