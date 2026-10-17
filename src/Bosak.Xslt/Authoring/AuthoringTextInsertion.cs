// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Typed literal-text targets, proposals and immutable source-preserving candidates.
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

/// <summary>An opaque text append position owned by exactly one immutable snapshot.</summary>
public sealed class AuthoringTextInsertionTarget
{
    internal AuthoringTextInsertionTarget(AuthoringSnapshot owner, AuthoringValueOfInsertionTarget position)
    { Owner = owner; Position = position; }
    internal AuthoringSnapshot Owner { get; }
    internal AuthoringValueOfInsertionTarget Position { get; }
    /// <summary>Gets the exact original constructor parent.</summary>
    public AuthoringNodeDescriptor Parent => Position.Parent;
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Position.ModuleUri;
}

/// <summary>An exclusive typed target or classified insertion refusal.</summary>
public sealed class AuthoringTextInsertionTargetResult
{
    internal AuthoringTextInsertionTargetResult(AuthoringTextInsertionTarget? target, AuthoringInsertionFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets whether a fully checked target is available.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the target on success.</summary>
    public AuthoringTextInsertionTarget? Target { get; }
    /// <summary>Gets the refusal otherwise.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>An immutable literal Unicode payload and typed append target.</summary>
public sealed class AuthoringTextInsertionProposal
{
    /// <summary>Creates a literal text request using PreserveSource layout.</summary>
    /// <param name="target">The engine-issued text target.</param>
    /// <param name="textValue">Literal Unicode text, including an empty string.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringTextInsertionProposal(AuthoringTextInsertionTarget target, string textValue)
        : this(target, textValue, AuthoringInsertionFormattingMode.PreserveSource) { }
    /// <summary>Creates a literal text request with an explicit layout mode.</summary>
    /// <param name="target">The engine-issued text target.</param>
    /// <param name="textValue">Literal Unicode text; never an XPath expression or markup.</param>
    /// <param name="formattingMode">PreserveSource or explicit Readable.</param>
    /// <exception cref="ArgumentNullException">Either reference argument is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The layout mode is undefined.</exception>
    public AuthoringTextInsertionProposal(AuthoringTextInsertionTarget target, string textValue,
        AuthoringInsertionFormattingMode formattingMode)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        TextValue = textValue ?? throw new ArgumentNullException(nameof(textValue));
        if (!Enum.IsDefined(formattingMode)) throw new ArgumentOutOfRangeException(nameof(formattingMode));
        FormattingMode = formattingMode;
    }
    /// <summary>Gets the engine-issued typed target.</summary>
    public AuthoringTextInsertionTarget Target { get; }
    /// <summary>Gets the exact unnormalized logical payload.</summary>
    public string TextValue { get; }
    /// <summary>Gets the explicitly selected immutable layout mode.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
}

/// <summary>An exclusive complete text candidate or classified insertion refusal.</summary>
public sealed class AuthoringTextInsertionResult
{
    internal AuthoringTextInsertionResult(AuthoringTextInsertionCandidate? candidate, AuthoringInsertionFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets whether a fully checked candidate is available.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets the isolated candidate on success.</summary>
    public AuthoringTextInsertionCandidate? Candidate { get; }
    /// <summary>Gets the refusal otherwise.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>A fresh ordinary candidate with literal payload and complete source/node evidence.</summary>
public sealed class AuthoringTextInsertionCandidate
{
    internal AuthoringTextInsertionCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot,
        SourceRange anchor, SourceRange instruction, SourceRange payload, AuthoringNodeDescriptor inserted,
        AuthoringNodeDescriptor? payloadNode, string text, AuthoringInsertionFormattingMode mode,
        IReadOnlyDictionary<int, int> map, IEnumerable<AuthoringInsertionSourceEdit> edits)
    {
        InputSnapshot = input; Snapshot = snapshot; OriginalRange = anchor; InsertedRange = instruction;
        TextPayloadRange = payload; InsertedNode = inserted; PayloadNode = payloadNode; TextValue = text;
        FormattingMode = mode; NodeCorrespondence = map; SourceEdits = Array.AsReadOnly(edits.ToArray());
    }
    /// <summary>Gets the exact immutable input snapshot.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the fresh validated ordinary candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the zero-length original append anchor; SourceEdits describes the complete footprint.</summary>
    public SourceRange OriginalRange { get; }
    /// <summary>Gets the entire inserted instruction markup range.</summary>
    public SourceRange InsertedRange { get; }
    /// <summary>Gets the encoded payload range, empty immediately after the opening tag for empty text.</summary>
    public SourceRange TextPayloadRange { get; }
    /// <summary>Gets the actual new instruction descriptor.</summary>
    public AuthoringNodeDescriptor InsertedNode { get; }
    /// <summary>Gets the actual source payload text descriptor, or null for empty text.</summary>
    public AuthoringNodeDescriptor? PayloadNode { get; }
    /// <summary>Gets the exact logical Unicode payload.</summary>
    public string TextValue { get; }
    /// <summary>Gets the chosen layout; there is no implicit downgrade.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
    /// <summary>Gets the total injective old-to-candidate ID map, scoped to InputSnapshot and Snapshot.</summary>
    public IReadOnlyDictionary<int, int> NodeCorrespondence { get; }
    /// <summary>Gets immutable ordered edits in original and candidate coordinates.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Exports a defensive copy of exact candidate bytes, including the original BOM.</summary>
    /// <returns>Caller-owned source bytes.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
