// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Proposal, result, failure taxonomy, changed-slot marker and candidate types for expression edits.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-124 Slice C: lifecycle, staleness and sharing remarks                                |
//                      | Charles Korthout | 0.3   | 10-10-2026     | REQ-124 acceptance F1: candidate compilation routed through the snapshot's resolver      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.Xslt.Api;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// A caller-built proposal to replace the value of one owned attribute slot with a new XPath
/// expression. The proposal is validated: <see cref="OwningNodeId"/> must be a node id of the
/// snapshot the proposal is submitted to, <see cref="AttributeName"/> an attribute of that node,
/// and <see cref="NewExpressionText"/> a non-empty expression. Instances are immutable and may be
/// reused; a proposal stays attached to its input snapshot even when that snapshot has since been
/// superseded by a candidate (a late proposal remains valid data the consumer may discard).
/// </summary>
public sealed class AuthoringEditProposal
{
    /// <summary>Initializes a new expression-edit proposal.</summary>
    /// <param name="owningNodeId">The snapshot-scoped descriptor id of the element owning the slot.</param>
    /// <param name="attributeName">The attribute name as written in source (for example <c>select</c>).</param>
    /// <param name="newExpressionText">The replacement XPath expression text.</param>
    /// <exception cref="ArgumentNullException"><paramref name="attributeName"/> or <paramref name="newExpressionText"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="attributeName"/> or <paramref name="newExpressionText"/> is empty.</exception>
    public AuthoringEditProposal(int owningNodeId, string attributeName, string newExpressionText)
    {
        ArgumentNullException.ThrowIfNull(attributeName);
        ArgumentNullException.ThrowIfNull(newExpressionText);
        if (attributeName.Length == 0)
        {
            throw new ArgumentException("The attribute name must not be empty.", nameof(attributeName));
        }

        if (newExpressionText.Length == 0)
        {
            throw new ArgumentException("The new expression text must not be empty.", nameof(newExpressionText));
        }

        OwningNodeId = owningNodeId;
        AttributeName = attributeName;
        NewExpressionText = newExpressionText;
    }

    /// <summary>Gets the snapshot-scoped descriptor id of the element owning the slot.</summary>
    public int OwningNodeId { get; }

    /// <summary>Gets the attribute name as written in source.</summary>
    public string AttributeName { get; }

    /// <summary>Gets the replacement XPath expression text.</summary>
    public string NewExpressionText { get; }
}

/// <summary>
/// Classifies why an expression edit was refused. Programmer misuse (null proposals, empty
/// proposal fields) is reported as <see cref="ArgumentException"/> from the proposal constructor
/// and is deliberately distinct from these source/edit diagnostics.
/// </summary>
public enum AuthoringEditFailureKind
{
    /// <summary>The proposal's node id does not belong to the snapshot it was submitted to.</summary>
    UnknownNode,

    /// <summary>The attribute is missing, or its slot kind is not <see cref="AuthoringAttributeSlotKind.Expression"/>; other slot kinds are a documented capability absence in this version, never an exception.</summary>
    SlotNotEditable,

    /// <summary>The new text does not parse or compile as an XPath expression in the slot's static context.</summary>
    ExpressionParseError,

    /// <summary>The edit would require a namespace binding or start-tag change outside the attribute value; the expanded affected ownership (the owning element's start tag) is reported and the edit is refused.</summary>
    RequiresParentChange,

    /// <summary>The escaped edit cannot be encoded in the module's declared encoding.</summary>
    NotRepresentableInEncoding,
}

/// <summary>
/// An immutable, classified refusal of an <see cref="AuthoringEditProposal"/>. Carries the source
/// range the failure relates to where available; <see cref="RequiresParentChange"/> additionally
/// fills <see cref="ExpandedRange"/> with the owning element's full start-tag range.
/// </summary>
/// <remarks>
/// A failure is immutable, short-lived reporting data: it holds no unmanaged resources, needs no
/// disposal and is safe to share across threads. It carries no snapshot references and stays
/// meaningful after the snapshot that produced it is discarded.
/// </remarks>
public sealed class AuthoringEditFailure
{
    internal AuthoringEditFailure(AuthoringEditFailureKind kind, string message, SourceRange? range = null, SourceRange? expandedRange = null)
    {
        Kind = kind;
        Message = message;
        Range = range;
        ExpandedRange = expandedRange;
    }

    /// <summary>Gets the classified failure kind.</summary>
    public AuthoringEditFailureKind Kind { get; }

    /// <summary>Gets the human-readable failure description.</summary>
    public string Message { get; }

    /// <summary>Gets the source range the failure relates to (the attribute value range), when known.</summary>
    public SourceRange? Range { get; }

    /// <summary>
    /// Gets the expanded affected ownership — the owning element's full start-tag range — when the
    /// edit would require a change outside the attribute value (<see cref="AuthoringEditFailureKind.RequiresParentChange"/>).
    /// </summary>
    public SourceRange? ExpandedRange { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}: {Message}";
}

/// <summary>
/// The outcome of <see cref="AuthoringSnapshot.ProposeExpressionEdit"/>: exactly one of
/// <see cref="Candidate"/> or <see cref="Failure"/> is non-null. Instances are immutable.
/// </summary>
/// <remarks>
/// The result is a short-lived carrier: adopt the candidate or inspect the failure, then discard it.
/// Both payloads are immutable, hold no unmanaged resources, need no disposal and are safe to share
/// across threads. A candidate is valid only against the snapshot it was derived from; submitting
/// the same proposal to a later revision produces an independent result against that revision.
/// </remarks>
public sealed class AuthoringEditResult
{
    private AuthoringEditResult(AuthoringEditCandidate? candidate, AuthoringEditFailure? failure)
    {
        Candidate = candidate;
        Failure = failure;
    }

    /// <summary>Gets whether the edit was accepted and <see cref="Candidate"/> is available.</summary>
    public bool IsSuccess => Candidate is not null;

    /// <summary>Gets the isolated edit candidate on success; <see langword="null"/> otherwise.</summary>
    public AuthoringEditCandidate? Candidate { get; }

    /// <summary>Gets the classified refusal on failure; <see langword="null"/> otherwise.</summary>
    public AuthoringEditFailure? Failure { get; }

    /// <summary>Creates a successful edit result.</summary>
    /// <param name="candidate">The accepted candidate.</param>
    /// <returns>The success result.</returns>
    public static AuthoringEditResult SuccessResult(AuthoringEditCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return new AuthoringEditResult(candidate, null);
    }

    /// <summary>Creates a failed edit result.</summary>
    /// <param name="failure">The classified refusal.</param>
    /// <returns>The failure result.</returns>
    public static AuthoringEditResult FailureResult(AuthoringEditFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new AuthoringEditResult(null, failure);
    }
}

/// <summary>
/// An immutable marker describing which attribute slot one candidate changed: the owning node id
/// (in the input snapshot's identity space), the attribute name, and the old versus new value
/// ranges and raw literals.
/// </summary>
/// <remarks>
/// The owning node id is in the input snapshot's identity space: it is valid only within that
/// snapshot. The old range is in the input module's source; the new range is in the candidate's
/// emitted source. The marker is immutable, holds no unmanaged resources, needs no disposal and is
/// safe to share across threads.
/// </remarks>
public sealed class AuthoringChangedSlot
{
    internal AuthoringChangedSlot(
        int owningNodeId,
        string attributeName,
        SourceRange oldValueRange,
        SourceRange newValueRange,
        string oldRawLiteral,
        string newRawLiteral)
    {
        OwningNodeId = owningNodeId;
        AttributeName = attributeName;
        OldValueRange = oldValueRange;
        NewValueRange = newValueRange;
        OldRawLiteral = oldRawLiteral;
        NewRawLiteral = newRawLiteral;
    }

    /// <summary>Gets the owning element's node id in the input snapshot's identity space.</summary>
    public int OwningNodeId { get; }

    /// <summary>Gets the attribute name as written in source.</summary>
    public string AttributeName { get; }

    /// <summary>Gets the attribute value range in the input module's source.</summary>
    public SourceRange OldValueRange { get; }

    /// <summary>Gets the attribute value range in the candidate's emitted source.</summary>
    public SourceRange NewValueRange { get; }

    /// <summary>Gets the attribute value exactly as written before the edit.</summary>
    public string OldRawLiteral { get; }

    /// <summary>Gets the attribute value exactly as written after the edit (source spelling, entity references escaped).</summary>
    public string NewRawLiteral { get; }
}

/// <summary>
/// An isolated, immutable result of a validated expression edit. The emitted bytes are the
/// original module bytes with only the declared affected lexical region (the attribute value
/// range) replaced; every other byte — declaration, encoding/BOM, quotes, comments, PIs, CRLF,
/// entity spelling — is preserved because it is spliced, never re-serialized. The candidate
/// carries a full re-inspected <see cref="Snapshot"/> (the same resolver and options as the input
/// snapshot's inspection, compilation diagnostics on), old-to-new node identity correspondence,
/// and the changed-slot marker. The input snapshot and its envelopes are never mutated, and this
/// type is safe for concurrent reads; a candidate produced late against its input snapshot
/// remains valid attached data. The candidate is pure managed state and needs no disposal.
/// </summary>
public sealed class AuthoringEditCandidate
{
    internal AuthoringEditCandidate(
        ReadOnlyMemory<byte> emittedSource,
        IReadOnlyList<SourceRange> affectedRanges,
        AuthoringSnapshot snapshot,
        IReadOnlyDictionary<int, int> nodeCorrespondence,
        AuthoringChangedSlot changedSlot)
    {
        EmittedSource = emittedSource;
        AffectedRanges = affectedRanges;
        Snapshot = snapshot;
        NodeCorrespondence = nodeCorrespondence;
        ChangedSlot = changedSlot;
    }

    /// <summary>Gets the emitted bytes of the principal module: original bytes with only the affected range spliced.</summary>
    public ReadOnlyMemory<byte> EmittedSource { get; }

    /// <summary>Gets the source ranges affected by the edit (the replaced attribute value range).</summary>
    public IReadOnlyList<SourceRange> AffectedRanges { get; }

    /// <summary>Gets the candidate's own authoring snapshot, produced by re-inspecting the emitted bytes.</summary>
    public AuthoringSnapshot Snapshot { get; }

    /// <summary>
    /// Gets the correspondence from input-snapshot node ids to candidate-snapshot node ids,
    /// computed by a parallel tree walk matching node kind, element qualified name and child
    /// ordinal. Nodes without a counterpart are omitted; an id is never mapped to a different node.
    /// </summary>
    public IReadOnlyDictionary<int, int> NodeCorrespondence { get; }

    /// <summary>Gets the marker of which attribute slot this candidate changed, with old versus new ranges.</summary>
    public AuthoringChangedSlot ChangedSlot { get; }

    /// <summary>Gets whether the candidate's principal module compiles (from the derived snapshot).</summary>
    public bool IsCompilable => Snapshot.IsCompilable;

    /// <summary>Gets the candidate's compilation diagnostics (empty when compilable).</summary>
    public IReadOnlyList<string> CompilationDiagnostics => Snapshot.CompilationDiagnostics;

    /// <summary>
    /// Compiles the emitted source into an executable using the public compiler, decoding the
    /// bytes with the module's own encoding. Compilation is derived state; this method does not
    /// mutate the candidate.
    /// </summary>
    /// <remarks>
    /// Resource policy: include/import resolution during this compilation is routed through the
    /// same <see cref="IAuthoringModuleResolver"/> the candidate's snapshot was inspected with —
    /// never implicitly through the file system. When the snapshot was created with the engine's
    /// default file-system resolver, that deliberate default applies unchanged. There is no retry
    /// and no silent fallback: if the module resolver refuses, returns <see langword="null"/> or
    /// fails, compilation fails with the corresponding exception (for example
    /// <see cref="FileNotFoundException"/> for an unresolvable module). Success or failure leaves
    /// the input and candidate envelopes, source ranges and node correspondence untouched; the
    /// candidate stays immutable and safe to share across threads.
    /// </remarks>
    /// <returns>An executable transform of the emitted source.</returns>
    /// <exception cref="Exception">Compilation fails, including module resolution refused by the snapshot's module resolver.</exception>
    public XsltExecutable Compile()
    {
        var source = Snapshot.SourceFor(Snapshot.PrincipalModule.ModuleUri);
        var text = source.StrictDecode(source.EmittedBytes(EmittedSource));
        var compiler = new XsltCompiler();
        if (Snapshot.ModuleResolver is not FileSystemAuthoringModuleResolver)
        {
            compiler.UriResolver = new AuthoringModuleUriResolverBridge(Snapshot.ModuleResolver);
        }

        return compiler.Compile(text, Snapshot.PrincipalModule.ModuleUri.AbsoluteUri);
    }
}
