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
//                      | Charles Korthout | 0.2   | 15-10-2026     | Retain private source-derived context independently of earlier inspection compilation.    |
//                      | Charles Korthout | 0.3   | 17-10-2026     | REQ-129: immutable opt-in formatting mode and complete edit evidence.
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 17-10-2026     | REQ-131: append literal InvalidPayload without renumbering existing failures.          |
//                      | Charles Korthout | 0.5   | 18-10-2026     | REQ-132: additive literal element insertion with exact candidate provenance.             |
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
    /// <summary>Readable layout cannot be inferred or proven safe.</summary>
    UnsupportedFormatting,
    /// <summary>A literal payload contains invalid XML characters or unpaired surrogates.</summary>
    InvalidPayload,
    /// <summary>The requested literal element name is not a supported exact NCName.</summary>
    InvalidElementName,
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
    internal AuthoringValueOfInsertionTarget(AuthoringSnapshot owner, AuthoringNodeDescriptor parent, int offset, System.Xml.Linq.XElement contextElement)
    { Owner = owner; Parent = parent; CharacterOffset = offset; ContextElement = contextElement; }
    internal AuthoringSnapshot Owner { get; }
    internal int CharacterOffset { get; }
    internal System.Xml.Linq.XElement ContextElement { get; }
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

/// <summary>Controls optional layout for a bounded insertion proposal.</summary>
public enum AuthoringInsertionFormattingMode
{
    /// <summary>Keep the existing insertion layout and untouched original bytes.</summary>
    PreserveSource = 0,
    /// <summary>Format only proven ignorable direct-child whitespace gaps.</summary>
    Readable = 1,
}

/// <summary>An immutable byte edit linking input source to exact candidate replacement bytes.</summary>
public sealed class AuthoringInsertionSourceEdit
{
    internal AuthoringInsertionSourceEdit(SourceRange originalRange, SourceRange replacementRange)
    { OriginalRange = originalRange; ReplacementRange = replacementRange; }
    /// <summary>Gets the replaced range in the input snapshot; may be empty for insertion.</summary>
    public SourceRange OriginalRange { get; }
    /// <summary>Gets the replacement range in the exported candidate snapshot.</summary>
    public SourceRange ReplacementRange { get; }
}

/// <summary>A request to append one value-of instruction using expanded XPath expression text.</summary>
public sealed class AuthoringValueOfInsertionProposal
{
    /// <summary>Creates a proposal; empty or incomplete expressions are classified during proposal processing.</summary>
    /// <param name="target">The engine-issued target.</param>
    /// <param name="selectExpressionText">Expanded expression text, before XML escaping.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringValueOfInsertionProposal(AuthoringValueOfInsertionTarget target, string selectExpressionText)
        : this(target, selectExpressionText, AuthoringInsertionFormattingMode.PreserveSource) { }
    /// <summary>Creates a proposal with an explicit immutable formatting mode.</summary>
    /// <param name="target">The engine-issued target.</param>
    /// <param name="selectExpressionText">Expanded expression text, before XML escaping.</param>
    /// <param name="formattingMode">The requested source layout.</param>
    /// <exception cref="ArgumentNullException">Either reference argument is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The formatting mode is undefined.</exception>
    public AuthoringValueOfInsertionProposal(AuthoringValueOfInsertionTarget target, string selectExpressionText,
        AuthoringInsertionFormattingMode formattingMode)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        SelectExpressionText = selectExpressionText ?? throw new ArgumentNullException(nameof(selectExpressionText));
        if (!Enum.IsDefined(formattingMode)) throw new ArgumentOutOfRangeException(nameof(formattingMode));
        FormattingMode = formattingMode;
    }
    /// <summary>Gets the explicitly requested layout; omitted options preserve source.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
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
        AuthoringAttributeDescriptor selectSlot, IReadOnlyDictionary<int, int> correspondence,
        AuthoringInsertionFormattingMode formattingMode, IReadOnlyList<AuthoringInsertionSourceEdit> sourceEdits)
    {
        InputSnapshot = input; Snapshot = snapshot; OriginalRange = originalRange; InsertedRange = insertedRange;
        InsertedNode = insertedNode; SelectSlot = selectSlot; NodeCorrespondence = correspondence;
        FormattingMode = formattingMode; SourceEdits = Array.AsReadOnly(sourceEdits.ToArray());
    }
    /// <summary>Gets the applied formatting mode; no implicit downgrade occurs.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
    /// <summary>Gets all edits in input-source order; replacement ranges refer to the exported candidate bytes.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Gets the exact input snapshot against which this candidate is valid.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the isolated candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the zero-length instruction anchor in the input; use SourceEdits for the complete formatted footprint.</summary>
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
