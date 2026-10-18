// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Literal text body replacement authority, outcomes and immutable source evidence.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>The exclusive outcome of a literal text body proposal.</summary>
public enum AuthoringTextBodyReplacementOutcome
{
    /// <summary>A validated fresh candidate is available.</summary>
    Changed,
    /// <summary>The validated logical intent already equals the original.</summary>
    NoChange,
    /// <summary>The operation was refused without a candidate.</summary>
    Refused,
}

/// <summary>Operation-specific literal text replacement refusal classifications.</summary>
public enum AuthoringTextBodyReplacementFailureKind
{
    /// <summary>CapabilityUnavailable: no replacement candidate is returned.</summary>
    CapabilityUnavailable,
    /// <summary>ForeignSnapshot: no replacement candidate is returned.</summary>
    ForeignSnapshot,
    /// <summary>UnknownTarget: no replacement candidate is returned.</summary>
    UnknownTarget,
    /// <summary>UnsupportedModule: no replacement candidate is returned.</summary>
    UnsupportedModule,
    /// <summary>UnsupportedProfile: no replacement candidate is returned.</summary>
    UnsupportedProfile,
    /// <summary>UnsupportedCoverage: no replacement candidate is returned.</summary>
    UnsupportedCoverage,
    /// <summary>UnsupportedEncoding: no replacement candidate is returned.</summary>
    UnsupportedEncoding,
    /// <summary>SizeLimit: no replacement candidate is returned.</summary>
    SizeLimit,
    /// <summary>IneligibleInstruction: no replacement candidate is returned.</summary>
    IneligibleInstruction,
    /// <summary>SelfClosingInstruction: no replacement candidate is returned.</summary>
    SelfClosingInstruction,
    /// <summary>InvalidOriginal: no replacement candidate is returned.</summary>
    InvalidOriginal,
    /// <summary>InvalidPayload: no replacement candidate is returned.</summary>
    InvalidPayload,
    /// <summary>InvalidCandidate: no replacement candidate is returned.</summary>
    InvalidCandidate,
    /// <summary>ResourceDenied: no replacement candidate is returned.</summary>
    ResourceDenied,
    /// <summary>SourceAlignmentFailure: no replacement candidate is returned.</summary>
    SourceAlignmentFailure,
    /// <summary>Cancelled: no replacement candidate is returned.</summary>
    Cancelled,
}

/// <summary>A classified refusal without replacement authority.</summary>
public sealed class AuthoringTextBodyReplacementFailure
{
    internal AuthoringTextBodyReplacementFailure(AuthoringTextBodyReplacementFailureKind kind, string message, string? code = null)
    { Kind = kind; Message = message; Code = code; }
    /// <summary>Gets the operation-specific classification.</summary>
    public AuthoringTextBodyReplacementFailureKind Kind { get; }
    /// <summary>Gets the diagnostic message.</summary>
    public string Message { get; }
    /// <summary>Gets an optional static diagnostic code.</summary>
    public string? Code { get; }
}

/// <summary>Opaque authority for one paired ordinary text body in one immutable snapshot.</summary>
public sealed class AuthoringTextBodyReplacementTarget
{
    internal AuthoringTextBodyReplacementTarget(AuthoringSnapshot owner, AuthoringNodeDescriptor instruction,
        AuthoringNodeDescriptor parent, SourceRange body, AuthoringNodeDescriptor? payload, int start, int end)
    { Owner = owner; Instruction = instruction; Parent = parent; OriginalBodyRange = body;
      OriginalPayloadNode = payload; Start = start; End = end; }
    internal AuthoringSnapshot Owner { get; }
    internal int Start { get; }
    internal int End { get; }
    /// <summary>Gets the actual original instruction.</summary>
    public AuthoringNodeDescriptor Instruction { get; }
    /// <summary>Gets the actual original constructor parent.</summary>
    public AuthoringNodeDescriptor Parent { get; }
    /// <summary>Gets the principal source URI.</summary>
    public Uri ModuleUri => Owner.PrincipalModule.ModuleUri;
    /// <summary>Gets the complete original lexical body range, possibly empty.</summary>
    public SourceRange OriginalBodyRange { get; }
    /// <summary>Gets the original decoded logical value.</summary>
    public string OriginalTextValue => OriginalPayloadNode?.Text ?? "";
    /// <summary>Gets the actual original payload, or null for an empty body.</summary>
    public AuthoringNodeDescriptor? OriginalPayloadNode { get; }
}

/// <summary>An exclusive checked target or classified refusal.</summary>
public sealed class AuthoringTextBodyReplacementTargetResult
{
    internal AuthoringTextBodyReplacementTargetResult(AuthoringTextBodyReplacementTarget? target, AuthoringTextBodyReplacementFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets whether an eligible target is present.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the opaque target, on success.</summary>
    public AuthoringTextBodyReplacementTarget? Target { get; }
    /// <summary>Gets the refusal otherwise.</summary>
    public AuthoringTextBodyReplacementFailure? Failure { get; }
}

/// <summary>An immutable literal Unicode intent, never markup or an expression.</summary>
public sealed class AuthoringTextBodyReplacementProposal
{
    /// <summary>Creates a body replacement request.</summary>
    /// <param name="target">Engine-issued authority owned by one snapshot.</param>
    /// <param name="textValue">Exact logical Unicode text; empty is legal.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringTextBodyReplacementProposal(AuthoringTextBodyReplacementTarget target, string textValue)
    { Target = target ?? throw new ArgumentNullException(nameof(target)); TextValue = textValue ?? throw new ArgumentNullException(nameof(textValue)); }
    /// <summary>Gets the opaque target.</summary>
    public AuthoringTextBodyReplacementTarget Target { get; }
    /// <summary>Gets the exact unnormalized intent.</summary>
    public string TextValue { get; }
}

/// <summary>A validated equal intent, with no candidate, edits or new snapshot.</summary>
public sealed class AuthoringTextBodyReplacementNoChange
{
    internal AuthoringTextBodyReplacementNoChange(AuthoringTextBodyReplacementTarget target, string text)
    { Target = target; TextValue = text; }
    /// <summary>Gets the exact original immutable owner.</summary>
    public AuthoringSnapshot InputSnapshot => Target.Owner;
    /// <summary>Gets the original authority; it does not assert consumer currentness.</summary>
    public AuthoringTextBodyReplacementTarget Target { get; }
    /// <summary>Gets the unchanged principal URI.</summary>
    public Uri ModuleUri => Target.ModuleUri;
    /// <summary>Gets the equal logical intent.</summary>
    public string TextValue { get; }
}

/// <summary>Three exclusive outcomes; NoChange never implies a candidate.</summary>
public sealed class AuthoringTextBodyReplacementResult
{
    internal AuthoringTextBodyReplacementResult(AuthoringTextBodyReplacementCandidate? candidate,
        AuthoringTextBodyReplacementNoChange? noChange, AuthoringTextBodyReplacementFailure? failure)
    { Candidate = candidate; NoChange = noChange; Failure = failure; }
    /// <summary>Gets the exclusive result discriminator.</summary>
    public AuthoringTextBodyReplacementOutcome Outcome => Candidate is not null ? AuthoringTextBodyReplacementOutcome.Changed
        : NoChange is not null ? AuthoringTextBodyReplacementOutcome.NoChange : AuthoringTextBodyReplacementOutcome.Refused;
    /// <summary>Gets the complete candidate only for Changed.</summary>
    public AuthoringTextBodyReplacementCandidate? Candidate { get; }
    /// <summary>Gets original-owner evidence only for NoChange.</summary>
    public AuthoringTextBodyReplacementNoChange? NoChange { get; }
    /// <summary>Gets the classified failure only for Refused.</summary>
    public AuthoringTextBodyReplacementFailure? Failure { get; }
}

/// <summary>A validated one-body edit with complete disjoint payload and survivor evidence.</summary>
public sealed class AuthoringTextBodyReplacementCandidate
{
    internal AuthoringTextBodyReplacementCandidate(AuthoringTextBodyReplacementTarget target, AuthoringSnapshot snapshot,
        string text, SourceRange replacement, AuthoringNodeDescriptor edited, AuthoringNodeDescriptor parent,
        AuthoringNodeDescriptor? payload, IReadOnlyDictionary<int, int> map)
    {
        InputSnapshot = target.Owner; Snapshot = snapshot; OriginalTextValue = target.OriginalTextValue;
        TextValue = text; OriginalBodyRange = target.OriginalBodyRange; ReplacementBodyRange = replacement;
        EditedNode = edited; EditedParent = parent; OriginalPayloadNode = target.OriginalPayloadNode; PayloadNode = payload;
        SurvivorNodeCorrespondence = new System.Collections.ObjectModel.ReadOnlyDictionary<int, int>(map.ToDictionary(p => p.Key, p => p.Value));
        ReplacedNodes = Array.AsReadOnly(target.OriginalPayloadNode is null ? Array.Empty<AuthoringNodeDescriptor>() : new[] { target.OriginalPayloadNode });
        NewNodes = Array.AsReadOnly(payload is null ? Array.Empty<AuthoringNodeDescriptor>() : new[] { payload });
        SourceEdits = Array.AsReadOnly(new[] { new AuthoringInsertionSourceEdit(OriginalBodyRange, replacement) });
    }
    /// <summary>Gets the exact original owner.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the fresh validated candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the original logical value.</summary>
    public string OriginalTextValue { get; }
    /// <summary>Gets the exact replacement logical value.</summary>
    public string TextValue { get; }
    /// <summary>Gets the complete original body range.</summary>
    public SourceRange OriginalBodyRange { get; }
    /// <summary>Gets the complete encoded replacement body or empty caret.</summary>
    public SourceRange ReplacementBodyRange { get; }
    /// <summary>Gets the actual candidate instruction.</summary>
    public AuthoringNodeDescriptor EditedNode { get; }
    /// <summary>Gets the actual candidate parent.</summary>
    public AuthoringNodeDescriptor EditedParent { get; }
    /// <summary>Gets the actual old payload or null.</summary>
    public AuthoringNodeDescriptor? OriginalPayloadNode { get; }
    /// <summary>Gets the actual new payload or null.</summary>
    public AuthoringNodeDescriptor? PayloadNode { get; }
    /// <summary>Gets the total injective survivor map excluding replaced payload.</summary>
    public IReadOnlyDictionary<int, int> SurvivorNodeCorrespondence { get; }
    /// <summary>Gets the zero-or-one actual old payload partition.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> ReplacedNodes { get; }
    /// <summary>Gets the zero-or-one actual new payload partition.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> NewNodes { get; }
    /// <summary>Gets the exactly one immutable body edit.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Gets the principal source URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Exports a defensive copy of the exact candidate source bytes.</summary>
    /// <returns>Caller-owned bytes including the original BOM.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
