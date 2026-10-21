// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Typed literal element targets, proposals and complete immutable candidate evidence.
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

/// <summary>An opaque literal element append position owned by exactly one immutable snapshot.</summary>
public sealed class AuthoringLiteralElementInsertionTarget
{
    internal AuthoringLiteralElementInsertionTarget(AuthoringSnapshot owner, AuthoringValueOfInsertionTarget position)
    { Owner = owner; Position = position; }
    internal AuthoringSnapshot Owner { get; }
    internal AuthoringValueOfInsertionTarget Position { get; }
    /// <summary>Gets the exact original constructor parent.</summary>
    public AuthoringNodeDescriptor Parent => Position.Parent;
    /// <summary>Gets the principal module URI.</summary>
    public Uri ModuleUri => Position.ModuleUri;
}

/// <summary>An exclusive typed target or classified insertion refusal.</summary>
public sealed class AuthoringLiteralElementInsertionTargetResult
{
    internal AuthoringLiteralElementInsertionTargetResult(AuthoringLiteralElementInsertionTarget? target, AuthoringInsertionFailure? failure)
    { Target = target; Failure = failure; }
    /// <summary>Gets whether a fully checked target is available.</summary>
    public bool IsSuccess => Target is not null;
    /// <summary>Gets the target on success.</summary>
    public AuthoringLiteralElementInsertionTarget? Target { get; }
    /// <summary>Gets the refusal otherwise.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>An immutable literal element name and typed append target.</summary>
public sealed class AuthoringLiteralElementInsertionProposal
{
    /// <summary>Creates a literal element request using PreserveSource layout.</summary>
    /// <param name="target">The engine-issued literal element target.</param>
    /// <param name="localName">An exact no-namespace local name.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AuthoringLiteralElementInsertionProposal(AuthoringLiteralElementInsertionTarget target, string localName)
        : this(target, localName, AuthoringInsertionFormattingMode.PreserveSource) { }
    /// <summary>Creates a literal element request with an explicit layout mode.</summary>
    /// <param name="target">The engine-issued literal element target.</param>
    /// <param name="localName">An exact local name; never a QName or markup.</param>
    /// <param name="formattingMode">PreserveSource or explicit Readable.</param>
    /// <exception cref="ArgumentNullException">Either reference argument is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The layout mode is undefined.</exception>
    public AuthoringLiteralElementInsertionProposal(AuthoringLiteralElementInsertionTarget target, string localName,
        AuthoringInsertionFormattingMode formattingMode)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
        if (!Enum.IsDefined(formattingMode)) throw new ArgumentOutOfRangeException(nameof(formattingMode));
        FormattingMode = formattingMode;
    }
    /// <summary>Gets the engine-issued typed target.</summary>
    public AuthoringLiteralElementInsertionTarget Target { get; }
    /// <summary>Gets the exact unnormalized local name.</summary>
    public string LocalName { get; }
    /// <summary>Gets the explicitly selected immutable layout mode.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
}

/// <summary>An exclusive complete literal element candidate or classified insertion refusal.</summary>
public sealed class AuthoringLiteralElementInsertionResult
{
    internal AuthoringLiteralElementInsertionResult(AuthoringLiteralElementInsertionCandidate? candidate, AuthoringInsertionFailure? failure)
    { Candidate = candidate; Failure = failure; }
    /// <summary>Gets whether a fully checked candidate is available.</summary>
    public bool IsSuccess => Candidate is not null;
    /// <summary>Gets the isolated candidate on success.</summary>
    public AuthoringLiteralElementInsertionCandidate? Candidate { get; }
    /// <summary>Gets the refusal otherwise.</summary>
    public AuthoringInsertionFailure? Failure { get; }
}

/// <summary>A validated empty literal element and complete source/descriptor provenance.</summary>
public sealed class AuthoringLiteralElementInsertionCandidate
{
    internal AuthoringLiteralElementInsertionCandidate(AuthoringSnapshot input, AuthoringSnapshot snapshot,
        SourceRange anchor, SourceRange insertedRange, SourceRange bodyRange, AuthoringNodeDescriptor inserted,
        AuthoringNodeDescriptor parent, string name, AuthoringInsertionFormattingMode mode,
        IReadOnlyDictionary<int, int> map, IEnumerable<AuthoringInsertionSourceEdit> edits,
        IEnumerable<AuthoringNodeDescriptor> newNodes)
    {
        InputSnapshot = input; Snapshot = snapshot; OriginalRange = anchor; InsertedRange = insertedRange;
        BodyRange = bodyRange; InsertedNode = inserted; InsertedParent = parent; LocalName = name;
        FormattingMode = mode; NodeCorrespondence = new System.Collections.ObjectModel.ReadOnlyDictionary<int, int>(
            map.ToDictionary(p => p.Key, p => p.Value));
        SourceEdits = Array.AsReadOnly(edits.ToArray()); NewNodes = Array.AsReadOnly(newNodes.ToArray());
    }
    /// <summary>Gets the exact immutable input owner.</summary>
    public AuthoringSnapshot InputSnapshot { get; }
    /// <summary>Gets the fresh ordinary candidate snapshot.</summary>
    public AuthoringSnapshot Snapshot { get; }
    /// <summary>Gets the principal source URI.</summary>
    public Uri ModuleUri => Snapshot.PrincipalModule.ModuleUri;
    /// <summary>Gets the exact accepted unnormalized local name.</summary>
    public string LocalName { get; }
    /// <summary>Gets the actual inserted expanded name, with empty namespace URI.</summary>
    public AuthoringQName ElementName => InsertedNode.ElementName!;
    /// <summary>Gets the original zero-length append anchor; edits describe the entire footprint.</summary>
    public SourceRange OriginalRange { get; }
    /// <summary>Gets the paired markup range, including a generated namespace reset if required.</summary>
    public SourceRange InsertedRange { get; }
    /// <summary>Gets the zero-byte empty-body caret in candidate coordinates, with no synthetic child.</summary>
    public SourceRange BodyRange { get; }
    /// <summary>Gets the actual fresh literal element descriptor.</summary>
    public AuthoringNodeDescriptor InsertedNode { get; }
    /// <summary>Gets the actual fresh counterpart of the original target parent.</summary>
    public AuthoringNodeDescriptor InsertedParent { get; }
    /// <summary>Gets the explicitly selected layout mode.</summary>
    public AuthoringInsertionFormattingMode FormattingMode { get; }
    /// <summary>Gets the complete injective old-to-candidate ID map scoped to the two snapshots.</summary>
    public IReadOnlyDictionary<int, int> NodeCorrespondence { get; }
    /// <summary>Gets all actual candidate nodes outside the old mapping, including new layout text.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> NewNodes { get; }
    /// <summary>Gets the complete immutable ordered source edits.</summary>
    public IReadOnlyList<AuthoringInsertionSourceEdit> SourceEdits { get; }
    /// <summary>Exports a defensive copy of exact candidate bytes and retained BOM.</summary>
    /// <returns>Caller-owned source bytes.</returns>
    public byte[] ExportSource() => Snapshot.ExportOriginal(ModuleUri);
}
