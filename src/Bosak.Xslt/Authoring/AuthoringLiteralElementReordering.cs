// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Direction-bound element reordering authority and exhaustive immutable exchange evidence.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>The direction bound to an adjacent sibling exchange.</summary>
public enum AuthoringLiteralElementMoveDirection
{
    /// <summary>Exchange the selected element with its previous supported sibling.</summary>
    Up,
    /// <summary>Exchange the selected element with its next supported sibling.</summary>
    Down,
}

/// <summary>The exclusive reorder outcome.</summary>
public enum AuthoringLiteralElementReorderOutcome
{
    /// <summary>A complete changed candidate is available.</summary>
    Changed,
    /// <summary>Eligible exchange produces identical bytes and retains original selection.</summary>
    NoChange,
    /// <summary>No mutation authority is available.</summary>
    Refused,
}

/// <summary>Classified refusal of a bounded reorder.</summary>
public enum AuthoringLiteralElementReorderFailureKind
{
    /// <summary>The direction is not defined.</summary>
    InvalidDirection,
    /// <summary>Actual snapshot-owned authority is required.</summary>
    ForeignSnapshot,
    /// <summary>The selected element or parent is unsupported.</summary>
    IneligibleTarget,
    /// <summary>No adjacent supported neighbor exists in the requested direction.</summary>
    UnavailableNeighbor,
    /// <summary>One dependency-free principal module is required.</summary>
    UnsupportedModule,
    /// <summary>The captured host/version is unsupported.</summary>
    UnsupportedProfile,
    /// <summary>Complete restricted coverage cannot be established.</summary>
    UnsupportedCoverage,
    /// <summary>Strict supported mutation encoding is required.</summary>
    UnsupportedEncoding,
    /// <summary>The complete original does not validate.</summary>
    InvalidOriginal,
    /// <summary>The complete candidate does not validate.</summary>
    InvalidCandidate,
    /// <summary>Retained source and actual ranges do not align.</summary>
    SourceAlignmentFailure,
    /// <summary>Complete reordered graph correspondence cannot be proved.</summary>
    UnsupportedCorrespondence,
    /// <summary>An independent work or complete-evidence limit was exceeded.</summary>
    SizeLimit,
    /// <summary>Ambient resource acquisition is denied.</summary>
    ResourceDenied,
    /// <summary>Cancellation was observed before an outcome was published.</summary>
    Cancelled,
}

/// <summary>A refusal without partial target, candidate or NoChange authority.</summary>
public sealed class AuthoringLiteralElementReorderFailure
{
    internal AuthoringLiteralElementReorderFailure(AuthoringLiteralElementReorderFailureKind kind, string message, Uri moduleUri, string? code = null)
    { Kind = kind; Message = message; ModuleUri = moduleUri; Code = code; }
    /// <summary>Gets the refusal classification.</summary>
    public AuthoringLiteralElementReorderFailureKind Kind { get; }
    /// <summary>Gets the refusal explanation.</summary>
    public string Message { get; }
    /// <summary>Gets the applicable original or candidate module.</summary>
    public Uri ModuleUri { get; }
    /// <summary>Gets an available underlying static diagnostic code.</summary>
    public string? Code { get; }
}

internal sealed record LiteralElementReorderData(AuthoringSnapshot Snapshot, AuthoringNodeDescriptor Node,
    AuthoringNodeDescriptor Neighbor, AuthoringNodeDescriptor Parent, AuthoringNodeDescriptor Earlier,
    AuthoringNodeDescriptor Later, AuthoringNodeDescriptor? Separator, AuthoringLiteralElementMoveDirection Direction,
    int Start, int EarlierEnd, int LaterStart, int End);

/// <summary>Opaque snapshot-owned, direction-bound adjacent exchange authority.</summary>
public sealed class AuthoringLiteralElementReorderTarget
{
    internal AuthoringLiteralElementReorderTarget(LiteralElementReorderData data) => Data = data;
    internal LiteralElementReorderData Data { get; }
    /// <summary>Gets the actual selected original element.</summary>
    public AuthoringNodeDescriptor Node => Data.Node;
    /// <summary>Gets the actual adjacent original element.</summary>
    public AuthoringNodeDescriptor Neighbor => Data.Neighbor;
    /// <summary>Gets the actual common original parent.</summary>
    public AuthoringNodeDescriptor Parent => Data.Parent;
    /// <summary>Gets the optional ordinary whitespace-only separator.</summary>
    public AuthoringNodeDescriptor? Separator => Data.Separator;
    /// <summary>Gets the direction captured at discovery.</summary>
    public AuthoringLiteralElementMoveDirection Direction => Data.Direction;
    /// <summary>Gets the owning module URI.</summary>
    public Uri ModuleUri => Data.Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the exact bounded coverage profile identifier.</summary>
    public string Profile => AuthoringCapabilities.LiteralElementReorderProfile;
    /// <summary>Gets the issuing engine build identity.</summary>
    public string EngineVersion => AuthoringCapabilities.EngineVersion;
}

/// <summary>An exclusive discovery target or refusal.</summary>
public sealed class AuthoringLiteralElementReorderTargetResult
{
    internal AuthoringLiteralElementReorderTargetResult(AuthoringLiteralElementReorderTarget? target, AuthoringLiteralElementReorderFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets whether complete target authority is available.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the owned target, if discovery succeeded.</summary>
    public AuthoringLiteralElementReorderTarget? Target { get; }
    /// <summary>Gets the refusal, if discovery failed.</summary>
    public AuthoringLiteralElementReorderFailure? Failure { get; }
}

/// <summary>An immutable exchange intent without a caller destination or direction override.</summary>
public sealed class AuthoringLiteralElementReorderProposal
{
    /// <summary>Creates an explicit reorder intent.</summary>
    /// <param name="target">The direction-bound, snapshot-owned target.</param>
    /// <exception cref="ArgumentNullException">The target is null.</exception>
    public AuthoringLiteralElementReorderProposal(AuthoringLiteralElementReorderTarget target)
    { ArgumentNullException.ThrowIfNull(target); Target = target; }
    /// <summary>Gets the complete owned intent.</summary>
    public AuthoringLiteralElementReorderTarget Target { get; }
}

/// <summary>Eligibility-checked byte equality without a candidate, fresh ports or Apply authority.</summary>
public sealed class AuthoringLiteralElementReorderNoChange
{
    internal AuthoringLiteralElementReorderNoChange(AuthoringLiteralElementReorderTarget target) => Target = target;
    /// <summary>Gets the original owned target; its original selection is retained.</summary>
    public AuthoringLiteralElementReorderTarget Target { get; }
}

/// <summary>Exactly one changed candidate, NoChange or refusal.</summary>
public sealed class AuthoringLiteralElementReorderResult
{
    internal AuthoringLiteralElementReorderResult(AuthoringLiteralElementReorderCandidate? candidate, AuthoringLiteralElementReorderNoChange? noChange, AuthoringLiteralElementReorderFailure? failure)
    { Candidate = candidate; NoChange = noChange; Failure = failure; }
    /// <summary>Gets the exclusive outcome.</summary>
    public AuthoringLiteralElementReorderOutcome Outcome => Candidate is not null ? AuthoringLiteralElementReorderOutcome.Changed : NoChange is not null ? AuthoringLiteralElementReorderOutcome.NoChange : AuthoringLiteralElementReorderOutcome.Refused;
    /// <summary>Gets complete changed evidence, when available.</summary>
    public AuthoringLiteralElementReorderCandidate? Candidate { get; }
    /// <summary>Gets original-selection equality evidence, when available.</summary>
    public AuthoringLiteralElementReorderNoChange? NoChange { get; }
    /// <summary>Gets the typed refusal, when available.</summary>
    public AuthoringLiteralElementReorderFailure? Failure { get; }
}

/// <summary>Explicit immutable sibling-order exception alongside exhaustive node and attribute bijections.</summary>
public sealed class AuthoringLiteralElementExchange
{
    internal AuthoringLiteralElementExchange(LiteralElementReorderData data, IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> map)
    {
        OriginalEarlier = data.Earlier; OriginalLater = data.Later; OriginalParent = data.Parent; OriginalSeparator = data.Separator;
        CandidateOfEarlier = map[data.Earlier]; CandidateOfLater = map[data.Later]; CandidateParent = map[data.Parent];
        CandidateSeparator = data.Separator is null ? null : map[data.Separator];
    }
    /// <summary>Gets original A, the earlier subtree.</summary>
    public AuthoringNodeDescriptor OriginalEarlier { get; }
    /// <summary>Gets original B, the later subtree.</summary>
    public AuthoringNodeDescriptor OriginalLater { get; }
    /// <summary>Gets moved A, which is now later in the candidate.</summary>
    public AuthoringNodeDescriptor CandidateOfEarlier { get; }
    /// <summary>Gets moved B, which is now earlier in the candidate.</summary>
    public AuthoringNodeDescriptor CandidateOfLater { get; }
    /// <summary>Gets the original common parent.</summary>
    public AuthoringNodeDescriptor OriginalParent { get; }
    /// <summary>Gets the candidate common parent.</summary>
    public AuthoringNodeDescriptor CandidateParent { get; }
    /// <summary>Gets original W, if present.</summary>
    public AuthoringNodeDescriptor? OriginalSeparator { get; }
    /// <summary>Gets the actual candidate W, if present.</summary>
    public AuthoringNodeDescriptor? CandidateSeparator { get; }
}

/// <summary>A validated exact exchange with total actual node and per-owner attribute correspondence.</summary>
public sealed class AuthoringLiteralElementReorderCandidate
{
    internal AuthoringLiteralElementReorderCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot, LiteralElementReorderData data,
        Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> map, List<AuthoringLiteralAttributeCorrespondence> attributes, AuthoringInsertionSourceEdit edit)
    {
        InputSnapshot = input; Snapshot = snapshot; Direction = data.Direction; OriginalNode = data.Node; OriginalNeighbor = data.Neighbor;
        CandidateSelection = map[data.Node]; CandidateNeighbor = map[data.Neighbor]; Exchange = new(data, map);
        NodeCorrespondence = new System.Collections.ObjectModel.ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(new Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(map));
        AttributeCorrespondence = Array.AsReadOnly(attributes.ToArray()); SourceEdits = Array.AsReadOnly(new[] { edit });
    }
    /// <summary>Gets the immutable original snapshot and its engine/profile provenance.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the fresh, successfully validated ordinary inspection snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the owning principal module.</summary>
    public Uri ModuleUri => InputSnapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the exact bounded coverage profile identifier.</summary>
    public string Profile => AuthoringCapabilities.LiteralElementReorderProfile;
    /// <summary>Gets the engine build identity that produced this candidate.</summary>
    public string EngineVersion => AuthoringCapabilities.EngineVersion;
    /// <summary>Gets the captured exchange direction.</summary>
    public AuthoringLiteralElementMoveDirection Direction { get; }
    /// <summary>Gets the selected original element.</summary>
    public AuthoringNodeDescriptor OriginalNode { get; }
    /// <summary>Gets the original adjacent element.</summary>
    public AuthoringNodeDescriptor OriginalNeighbor { get; }
    /// <summary>Gets the actual moved selected element in the candidate.</summary>
    public AuthoringNodeDescriptor CandidateSelection { get; }
    /// <summary>Gets the actual moved neighbor in the candidate.</summary>
    public AuthoringNodeDescriptor CandidateNeighbor { get; }
    /// <summary>Gets the explicit sibling-order exception and separator evidence.</summary>
    public AuthoringLiteralElementExchange Exchange { get; }
    /// <summary>Gets the total original-to-candidate node bijection.</summary>
    public IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> NodeCorrespondence { get; }
    /// <summary>Gets complete per-element attribute bijections, including namespace declarations.</summary>
    public IReadOnlyList<AuthoringLiteralAttributeCorrespondence> AttributeCorrespondence { get; }
    /// <summary>Gets the one replacement, owned by InputSnapshot and Snapshot respectively.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Exports complete independent candidate bytes, including any BOM.</summary>
    /// <returns>A caller-owned copy of the complete candidate.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
    /// <summary>Exports independent bytes for an owned original range.</summary>
    /// <param name="range">A range within the original module.</param>
    /// <returns>A caller-owned byte array.</returns>
    /// <exception cref="ArgumentNullException">The range is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The range exceeds the original module.</exception>
    public byte[] ExportOriginalRange(SourceRange range)
    {
        ArgumentNullException.ThrowIfNull(range);
        var bytes = InputSnapshot.SourceFor(ModuleUri).OriginalBytes;
        if (range.StartByteOffset < 0 || range.ByteLength < 0 || range.StartByteOffset > bytes.Length || range.ByteLength > bytes.Length - range.StartByteOffset)
            throw new ArgumentOutOfRangeException(nameof(range));
        return bytes.Span.Slice((int)range.StartByteOffset, (int)range.ByteLength).ToArray();
    }
}
