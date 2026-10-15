// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Immutable targets, proposals, candidates and refusals for value-of insertion.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>Classifies a refused structural insertion without modifying the input.</summary>
public enum AuthoringInsertionFailureKind
{
    /// <summary>The engine does not offer this capability.</summary>
    CapabilityUnavailable,
    /// <summary>The descriptor or target belongs to another snapshot.</summary>
    ForeignSnapshot,
    /// <summary>The target cannot be resolved.</summary>
    UnknownTarget,
    /// <summary>Only a single principal stylesheet module is supported.</summary>
    UnsupportedModule,
    /// <summary>The effective stylesheet profile is unsupported.</summary>
    UnsupportedProfile,
    /// <summary>Static validation cannot cover the supplied context.</summary>
    UnsupportedCoverage,
    /// <summary>The encoding or its declaration is unsupported.</summary>
    UnsupportedEncoding,
    /// <summary>The original or candidate exceeds the byte limit.</summary>
    SizeLimit,
    /// <summary>The parent is outside the supported constructor chain.</summary>
    IneligibleParent,
    /// <summary>The parent has no explicit closing tag.</summary>
    SelfClosingParent,
    /// <summary>The parent contains significant text, CDATA or preserved whitespace.</summary>
    MixedContent,
    /// <summary>The original stylesheet fails static validation.</summary>
    InvalidOriginal,
    /// <summary>The proposed select expression is invalid.</summary>
    InvalidExpression,
    /// <summary>The emitted stylesheet fails static validation.</summary>
    InvalidCandidate,
    /// <summary>Static processing requested a denied resource.</summary>
    ResourceDenied,
    /// <summary>Source provenance could not establish complete node correspondence.</summary>
    SourceAlignmentFailure,
    /// <summary>Cooperative cancellation was observed before returning a candidate.</summary>
    Cancelled,
}

/// <summary>An immutable classified refusal; expression errors do not fabricate original-source ranges.</summary>
public sealed class AuthoringInsertionFailure
{
    internal AuthoringInsertionFailure(AuthoringInsertionFailureKind kind, string message, string? errorCode = null)
    { Kind = kind; Message = message; ErrorCode = errorCode; }
    /// <summary>Gets the refusal classification.</summary>
    public AuthoringInsertionFailureKind Kind { get; }
    /// <summary>Gets the diagnostic description.</summary>
    public string Message { get; }
    /// <summary>Gets the underlying static error code when available.</summary>
    public string? ErrorCode { get; }
    /// <inheritdoc />
    public override string ToString() => $"{Kind}: {Message}";
}

/// <summary>An engine-issued append position bound by reference to one immutable input snapshot.</summary>
public sealed class AuthoringValueOfInsertionTarget
{
    internal AuthoringValueOfInsertionTarget(AuthoringSnapshot owner, AuthoringNodeDescriptor parent, int offset)
    { Owner = owner; Parent = parent; CharacterOffset = offset; }
    internal AuthoringSnapshot Owner { get; }
    internal int CharacterOffset { get; }
    /// <summary>Gets the original parent descriptor; integer identities alone cannot recreate a target.</summary>
    public AuthoringNodeDescriptor Parent { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Owner.PrincipalModule.ModuleUri;
}

/// <summary>The mutually exclusive target or refusal from target discovery.</summary>
public sealed class AuthoringValueOfInsertionTargetResult
{
    internal AuthoringValueOfInsertionTargetResult(AuthoringValueOfInsertionTarget? target, AuthoringInsertionFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets whether an eligible target was issued.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the engine-issued target on success.</summary>
    public AuthoringValueOfInsertionTarget? Target { get; }
    /// <summary>Gets the refusal on failure.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>A request to append one value-of instruction using expanded XPath expression text.</summary>
public sealed class AuthoringValueOfInsertionProposal
{
    /// <summary>Creates a proposal; empty or incomplete expressions are classified during proposal processing.</summary>
    /// <param name="target">The engine-issued target.</param>
    /// <param name="selectExpressionText">Expanded expression text, before XML escaping.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringValueOfInsertionProposal(AuthoringValueOfInsertionTarget target, string selectExpressionText)
    { Target = target ?? throw new ArgumentNullException(nameof(target)); SelectExpressionText = selectExpressionText ?? throw new ArgumentNullException(nameof(selectExpressionText)); }
    /// <summary>Gets the opaque insertion target.</summary>
    public AuthoringValueOfInsertionTarget Target { get; }
    /// <summary>Gets the exact expanded expression text.</summary>
    public string SelectExpressionText { get; }
}

/// <summary>The mutually exclusive isolated candidate or classified refusal from a proposal.</summary>
public sealed class AuthoringValueOfInsertionResult
{
    internal AuthoringValueOfInsertionResult(AuthoringValueOfInsertionCandidate? candidate, AuthoringInsertionFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets whether a fully checked candidate is available.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets the isolated candidate on success.</summary>
    public AuthoringValueOfInsertionCandidate? Candidate { get; }
    /// <summary>Gets the refusal on failure.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>A source-preserving, statically checked insertion with complete surviving-node provenance.</summary>
public sealed class AuthoringValueOfInsertionCandidate
{
    internal AuthoringValueOfInsertionCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot,
        SourceRange originalRange, SourceRange insertedRange, AuthoringNodeDescriptor insertedNode,
        AuthoringAttributeDescriptor selectSlot, IReadOnlyDictionary<int, int> correspondence)
    { InputSnapshot = input; Snapshot = snapshot; OriginalRange = originalRange; InsertedRange = insertedRange;
      InsertedNode = insertedNode; SelectSlot = selectSlot; NodeCorrespondence = correspondence; }
    /// <summary>Gets the exact input snapshot against which this candidate is valid.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the isolated candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the zero-length insertion range in the original source.</summary>
    public SourceRange OriginalRange { get; }
    /// <summary>Gets the exact inserted instruction range in candidate source.</summary>
    public SourceRange InsertedRange { get; }
    /// <summary>Gets the new instruction in the candidate identity space.</summary>
    public AuthoringNodeDescriptor InsertedNode { get; }
    /// <summary>Gets the actual new Expression slot, including its captured static context.</summary>
    public AuthoringAttributeDescriptor SelectSlot { get; }
    /// <summary>Gets a total injective map of all original node identities into candidate identities.</summary>
    public IReadOnlyDictionary<int, int> NodeCorrespondence { get; }
    /// <summary>Exports an independent copy of the complete candidate source, including its original BOM.</summary>
    /// <returns>The source bytes; callers may mutate the returned array.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
