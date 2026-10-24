// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : The opaque authoring snapshot handle: per-module envelopes, graph and identities.
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-124 Slice B: edit proposals (resolver/options retention, node lookup, entry point)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 13-10-2026     | Retain immutable captured function keyword signatures.                                   |
//                      | Charles Korthout | 0.4   | 15-10-2026     | REQ-128: bounded insertion and immutable candidate descriptors.                           |
//                      | Charles Korthout | 0.5   | 17-10-2026     | REQ-130: snapshot-owned deletion and merged-text provenance.                            |
//                      | Charles Korthout | 0.6   | 17-10-2026     | REQ-131: typed snapshot-owned literal-text insertion entry points.                     |
//                      | Charles Korthout | 0.7   | 18-10-2026     | REQ-132: additive literal element insertion with exact candidate provenance.             |
// ===========================================================================================================================================================
//                      | Charles Korthout | 0.8   | 18-10-2026     | REQ-133: snapshot-owned literal text body replacement.                                  |

//                      | Charles Korthout | 0.9   | 19-10-2026     | REQ-134: separately bounded literal text deletion.                                      |

//                      | Charles Korthout | 1.0   | 20-10-2026     | REQ-135: operation-bound literal attribute lifecycle.                                    |

//                      | Charles Korthout | 1.1   | 23-10-2026     | REQ-136: element rename and subtree deletion. |

//                      | Charles Korthout | 1.2   | 24-10-2026     | REQ-137: direction-bound sibling reordering. |

using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// An opaque, immutable handle over the inspected state of one principal module and the modules it
/// includes or imports. The snapshot retains every module's original bytes (the only emission source
/// for untouched regions) plus derived, source-backed descriptor trees. Snapshot-scoped node
/// identities (see <see cref="AuthoringNodeDescriptor.Id"/>) are stable for the lifetime of the
/// snapshot and must not be reused against other revisions. The snapshot is pure managed state: it
/// holds no unmanaged resources and needs no disposal; keep it alive as long as the descriptors or
/// the original bytes are needed.
/// </summary>
public sealed class AuthoringSnapshot
{
    /// <summary>Discovers direction-bound adjacent literal element exchange authority.</summary>
    /// <param name="node">The actual selected snapshot-owned element.</param>
    /// <param name="direction">The requested adjacent sibling direction, captured in the target.</param>
    /// <param name="cancellationToken">Cancellation observed before authority is published.</param>
    /// <returns>An owned target or typed refusal.</returns>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    public AuthoringLiteralElementReorderTargetResult GetLiteralElementReorderTarget(AuthoringNodeDescriptor node,
        AuthoringLiteralElementMoveDirection direction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        return LiteralElementReorderPipeline.Discover(this, node, direction, cancellationToken);
    }
    /// <summary>Proposes one pure exact-source adjacent sibling exchange.</summary>
    /// <param name="proposal">The snapshot-owned, direction-bound operation intent.</param>
    /// <param name="cancellationToken">Cancellation observed before Changed or NoChange is published.</param>
    /// <returns>Exactly one complete candidate, original-selection NoChange or typed refusal.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralElementReorderResult ProposeLiteralElementReorder(AuthoringLiteralElementReorderProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralElementReorderPipeline.Propose(this, proposal, cancellationToken);
    }

    /// <summary>Discovers opaque literal element rename authority.</summary>
    /// <param name="node">The actual snapshot-owned element.</param>
    /// <param name="cancellationToken">Cancellation observed before authority is published.</param>
    /// <returns>An owned target or typed refusal.</returns>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    public AuthoringLiteralElementRenameTargetResult GetLiteralElementRenameTarget(AuthoringNodeDescriptor node, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        var r = LiteralElementLifecyclePipeline.Discover(this, node, cancellationToken);
        return new(r.Data is null ? null : new(r.Data), r.Failure);
    }
    /// <summary>Proposes a pure source-preserving literal element rename.</summary>
    /// <param name="proposal">The owned target and explicit operation intent.</param>
    /// <param name="cancellationToken">Cancellation observed before success.</param>
    /// <returns>An exclusive result without partial authority.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralElementRenameResult ProposeLiteralElementRename(AuthoringLiteralElementRenameProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralElementLifecyclePipeline.Rename(this, proposal, cancellationToken);
    }
    /// <summary>Discovers opaque literal element deletion authority.</summary>
    /// <param name="node">The actual snapshot-owned element.</param>
    /// <param name="cancellationToken">Cancellation observed before authority is published.</param>
    /// <returns>An owned target or typed refusal.</returns>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    public AuthoringLiteralElementDeletionTargetResult GetLiteralElementDeletionTarget(AuthoringNodeDescriptor node, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        var r = LiteralElementLifecyclePipeline.Discover(this, node, cancellationToken);
        return new(r.Data is null ? null : new(r.Data), r.Failure);
    }
    /// <summary>Proposes a pure source-preserving literal element deletion.</summary>
    /// <param name="proposal">The owned target and explicit operation intent.</param>
    /// <param name="cancellationToken">Cancellation observed before success.</param>
    /// <returns>An exclusive result without partial authority.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralElementDeletionResult ProposeLiteralElementDeletion(AuthoringLiteralElementDeletionProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralElementLifecyclePipeline.Deletion(this, proposal, cancellationToken);
    }

    /// <summary>Discovers owned literal attribute Creation authority.</summary>
    /// <param name="owner">The actual owned literal element.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive target or refusal.</returns>
    /// <exception cref="ArgumentNullException">An owner or attribute argument is null.</exception>
    public AuthoringLiteralAttributeCreationTargetResult GetLiteralAttributeCreationTarget(AuthoringNodeDescriptor owner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        var (data, failure) = LiteralAttributeLifecyclePipeline.Discover(this, owner, null, cancellationToken);
        return new(data is null ? null : new(data), failure);
    }
    /// <summary>Proposes an exact literal attribute Creation.</summary>
    /// <param name="proposal">Owned operation intent.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive operation result.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralAttributeCreationResult ProposeLiteralAttributeCreation(AuthoringLiteralAttributeCreationProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralAttributeLifecyclePipeline.Creation(this, proposal, cancellationToken);
    }

    /// <summary>Discovers owned literal attribute Update authority.</summary>
    /// <param name="owner">The actual owned literal element.</param>
    /// <param name="attribute">The actual attribute within that owner.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive target or refusal.</returns>
    /// <exception cref="ArgumentNullException">An owner or attribute argument is null.</exception>
    public AuthoringLiteralAttributeUpdateTargetResult GetLiteralAttributeUpdateTarget(AuthoringNodeDescriptor owner, AuthoringAttributeDescriptor attribute, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(attribute);
        var (data, failure) = LiteralAttributeLifecyclePipeline.Discover(this, owner, attribute, cancellationToken);
        return new(data is null ? null : new(data), failure);
    }
    /// <summary>Proposes an exact literal attribute Update.</summary>
    /// <param name="proposal">Owned operation intent.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive operation result.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralAttributeUpdateResult ProposeLiteralAttributeUpdate(AuthoringLiteralAttributeUpdateProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralAttributeLifecyclePipeline.Update(this, proposal, cancellationToken);
    }

    /// <summary>Discovers owned literal attribute Deletion authority.</summary>
    /// <param name="owner">The actual owned literal element.</param>
    /// <param name="attribute">The actual attribute within that owner.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive target or refusal.</returns>
    /// <exception cref="ArgumentNullException">An owner or attribute argument is null.</exception>
    public AuthoringLiteralAttributeDeletionTargetResult GetLiteralAttributeDeletionTarget(AuthoringNodeDescriptor owner, AuthoringAttributeDescriptor attribute, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(attribute);
        var (data, failure) = LiteralAttributeLifecyclePipeline.Discover(this, owner, attribute, cancellationToken);
        return new(data is null ? null : new(data), failure);
    }
    /// <summary>Proposes an exact literal attribute Deletion.</summary>
    /// <param name="proposal">Owned operation intent.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An exclusive operation result.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralAttributeDeletionResult ProposeLiteralAttributeDeletion(AuthoringLiteralAttributeDeletionProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralAttributeLifecyclePipeline.Deletion(this, proposal, cancellationToken);
    }

    private readonly IReadOnlyList<AuthoringModuleDescriptor> _modules;
    private readonly Dictionary<Uri, AuthoringModuleDescriptor> _modulesByUri;
    private readonly Dictionary<Uri, AuthoringSource> _sourcesByUri;
    private readonly Dictionary<XObject, int> _nodeIds;

    internal AuthoringSnapshot(
        IReadOnlyList<AuthoringModuleDescriptor> modules,
        Dictionary<Uri, AuthoringModuleDescriptor> modulesByUri,
        Dictionary<Uri, AuthoringSource> sourcesByUri,
        Dictionary<XObject, int> nodeIds,
        bool isCompilable,
        IReadOnlyList<string> compilationDiagnostics,
        IAuthoringModuleResolver moduleResolver,
        AuthoringInspectionOptions inspectionOptions,
        AuthoringFunctionSignatures? functionSignatures = null)
    {
        _modules = Array.AsReadOnly(modules.ToArray());
        _modulesByUri = modulesByUri;
        _sourcesByUri = sourcesByUri;
        _nodeIds = nodeIds;
        IsCompilable = isCompilable;
        CompilationDiagnostics = Array.AsReadOnly(compilationDiagnostics.ToArray());
        ModuleResolver = moduleResolver;
        InspectionOptions = inspectionOptions;
        FunctionSignatures = functionSignatures;
    }

    internal AuthoringFunctionSignatures? FunctionSignatures { get; }

    /// <summary>Gets the effective module resolver of this snapshot's inspection.</summary>
    internal IAuthoringModuleResolver ModuleResolver { get; }

    /// <summary>Gets the inspection options this snapshot was produced with.</summary>
    internal AuthoringInspectionOptions InspectionOptions { get; }

    /// <summary>Gets all successfully inspected modules. The principal is always first.</summary>
    public IReadOnlyList<AuthoringModuleDescriptor> Modules => _modules;

    /// <summary>Gets the principal (entry) module.</summary>
    public AuthoringModuleDescriptor PrincipalModule => _modules[0];

    /// <summary>
    /// Gets whether the principal module compiled without exceptions on the derived document. Semantic
    /// errors never block inspection; they surface here and in <see cref="CompilationDiagnostics"/>.
    /// </summary>
    public bool IsCompilable { get; }

    /// <summary>Gets the recorded compilation diagnostics of the principal module (empty when compilable).</summary>
    public IReadOnlyList<string> CompilationDiagnostics { get; }

    /// <summary>
    /// Exports the exact original bytes of one module — byte-for-byte, byte-order mark included (AC-01).
    /// This is the lossless emission path: untouched regions are always produced from the retained
    /// envelope, never re-serialized.
    /// </summary>
    /// <param name="moduleUri">The absolute URI of an inspected module.</param>
    /// <returns>A copy of the original module bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="moduleUri"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">The URI does not identify a module of this snapshot.</exception>
    public byte[] ExportOriginal(Uri moduleUri)
    {
        ArgumentNullException.ThrowIfNull(moduleUri);
        if (!_sourcesByUri.TryGetValue(moduleUri, out var source))
        {
            throw new KeyNotFoundException($"The URI '{moduleUri}' does not identify a module of this snapshot.");
        }

        return source.OriginalBytes.ToArray();
    }

    /// <summary>
    /// Attempts to resolve an absolute URI to an inspected module of this snapshot.
    /// </summary>
    /// <param name="moduleUri">The absolute module URI.</param>
    /// <param name="module">The module descriptor, when found.</param>
    /// <returns><see langword="true"/> when the module is part of this snapshot.</returns>
    public bool TryGetModule(Uri moduleUri, out AuthoringModuleDescriptor? module)
    {
        ArgumentNullException.ThrowIfNull(moduleUri);
        return _modulesByUri.TryGetValue(moduleUri, out module);
    }

    /// <summary>
    /// Resolves the snapshot-scoped identity of a derived node. Reliable only within this snapshot.
    /// </summary>
    /// <param name="node">An <see cref="XObject"/> from the derived document of one of this snapshot's modules.</param>
    /// <param name="id">The stable identity, when the node belongs to this snapshot.</param>
    /// <returns><see langword="true"/> when the node belongs to this snapshot.</returns>
    public bool TryGetNodeId(XObject node, out int id)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _nodeIds.TryGetValue(node, out id);
    }

    /// <summary>
    /// Proposes a replacement of one owned expression attribute slot's value. The proposal is
    /// validated against this snapshot (never against any candidate derived from it), the edit is
    /// compiled in the slot's real static context, and on acceptance an isolated, source-preserving
    /// candidate is returned. The input snapshot is never mutated; a proposal built against an old
    /// revision still resolves against this snapshot.
    /// </summary>
    /// <param name="proposal">The caller-built edit proposal.</param>
    /// <returns>The classified outcome: a candidate on success, a refusal otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="proposal"/> is null.</exception>
    public AuthoringEditResult ProposeExpressionEdit(AuthoringEditProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return ExpressionEditPipeline.Run(this, proposal);
    }

    /// <summary>Discovers a snapshot-owned append target beneath a supported template constructor.</summary>
    /// <param name="parent">The exact descriptor issued by this snapshot.</param>
    /// <param name="cancellationToken">Cooperative cancellation, observed between processing phases.</param>
    /// <returns>An opaque target or classified refusal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parent"/> is null.</exception>
    public AuthoringValueOfInsertionTargetResult GetValueOfInsertionTarget(AuthoringNodeDescriptor parent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);
        return ValueOfInsertionPipeline.Target(this, parent, cancellationToken);
    }

    /// <summary>Proposes one source-preserving value-of insertion without mutating this snapshot.</summary>
    /// <param name="proposal">An engine-issued target and expanded select expression.</param>
    /// <param name="cancellationToken">Cooperative cancellation; no candidate is returned once observed.</param>
    /// <returns>A statically checked isolated candidate or classified refusal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="proposal"/> is null.</exception>
    public AuthoringValueOfInsertionResult ProposeValueOfInsertion(AuthoringValueOfInsertionProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return ValueOfInsertionPipeline.Run(this, proposal, cancellationToken);
    }

    /// <summary>Discovers an owned empty-body value-of deletion target in the bounded profile.</summary>
    /// <param name="node">The actual instruction descriptor issued by this snapshot.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>An opaque target or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is observed.</exception>
    public AuthoringValueOfDeletionTargetResult GetValueOfDeletionTarget(AuthoringNodeDescriptor node,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        return ValueOfDeletionPipeline.Target(this, node, cancellationToken);
    }

    /// <summary>Removes one instruction privately and proves complete source and graph provenance.</summary>
    /// <param name="proposal">The engine-issued target to remove.</param>
    /// <param name="cancellationToken">Cooperative cancellation; no partial candidate is returned.</param>
    /// <returns>A complete isolated candidate or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is observed.</exception>
    public AuthoringValueOfDeletionResult ProposeValueOfDeletion(AuthoringValueOfDeletionProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return ValueOfDeletionPipeline.Run(this, proposal, cancellationToken);
    }

    /// <summary>Discovers a typed snapshot-owned target for appending one literal text instruction.</summary>
    /// <param name="parent">The exact parent descriptor issued by this snapshot.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>A target or classified insertion refusal, including Cancelled.</returns>
    /// <exception cref="ArgumentNullException">The parent is null.</exception>
    public AuthoringTextInsertionTargetResult GetTextInsertionTarget(AuthoringNodeDescriptor parent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);
        return TextInsertionPipeline.Target(this, parent, cancellationToken);
    }

    /// <summary>Appends one literal text instruction privately with complete source and node evidence.</summary>
    /// <param name="proposal">A typed engine-issued target and literal Unicode payload.</param>
    /// <param name="cancellationToken">Cooperative cancellation; cancellation returns no candidate.</param>
    /// <returns>An isolated validated candidate or classified insertion refusal, including Cancelled.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringTextInsertionResult ProposeTextInsertion(AuthoringTextInsertionProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return TextInsertionPipeline.Run(this, proposal, cancellationToken);
    }

    /// <summary>Discovers an opaque append target for one empty no-namespace literal element.</summary>
    /// <param name="parent">The exact constructor descriptor from this snapshot.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>A typed target or classified refusal, including cancellation.</returns>
    /// <exception cref="ArgumentNullException">The parent is null.</exception>
    public AuthoringLiteralElementInsertionTargetResult GetLiteralElementInsertionTarget(
        AuthoringNodeDescriptor parent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);
        return LiteralElementInsertionPipeline.Target(this, parent, cancellationToken);
    }

    /// <summary>Privately proposes one paired empty literal element with complete source evidence.</summary>
    /// <param name="proposal">The immutable target, local name and formatting mode.</param>
    /// <param name="cancellationToken">Cooperative cancellation; no partial candidate is returned.</param>
    /// <returns>A fresh validated candidate or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringLiteralElementInsertionResult ProposeLiteralElementInsertion(
        AuthoringLiteralElementInsertionProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return LiteralElementInsertionPipeline.Run(this, proposal, cancellationToken);
    }

    /// <summary>Discovers opaque authority to remove one literal text instruction and its payload.</summary>
    /// <param name="instruction">The exact owned instruction descriptor.</param>
    /// <param name="cancellationToken">Cooperative cancellation, returned as a typed refusal.</param>
    /// <returns>A complete target or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The instruction is null.</exception>
    public AuthoringTextDeletionTargetResult GetTextDeletionTarget(AuthoringNodeDescriptor instruction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        return TextDeletionPipeline.Target(this, instruction, cancellationToken);
    }

    /// <summary>Proposes exact removal without mutating this snapshot.</summary>
    /// <param name="proposal">The engine-issued target authority.</param>
    /// <param name="cancellationToken">Cooperative cancellation, returned as a typed refusal.</param>
    /// <returns>A fresh validated candidate or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringTextDeletionResult ProposeTextDeletion(AuthoringTextDeletionProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return TextDeletionPipeline.Run(this, proposal, cancellationToken);
    }

    /// <summary>Discovers authority for one paired ordinary literal text body.</summary>
    /// <param name="instruction">The actual instruction descriptor from this snapshot.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    /// <returns>A checked target or classified refusal.</returns>
    /// <exception cref="ArgumentNullException">The instruction is null.</exception>
    public AuthoringTextBodyReplacementTargetResult GetTextBodyReplacementTarget(AuthoringNodeDescriptor instruction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        return TextBodyReplacementPipeline.Target(this, instruction, cancellationToken);
    }

    /// <summary>Privately replaces one literal body or reports validated equal intent.</summary>
    /// <param name="proposal">An opaque target and exact logical Unicode intent.</param>
    /// <param name="cancellationToken">Cooperative cancellation; no partial candidate is published.</param>
    /// <returns>Exclusive Changed, NoChange or Refused evidence.</returns>
    /// <exception cref="ArgumentNullException">The proposal is null.</exception>
    public AuthoringTextBodyReplacementResult ProposeTextBodyReplacement(AuthoringTextBodyReplacementProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return TextBodyReplacementPipeline.Run(this, proposal, cancellationToken);
    }

    internal AuthoringSnapshot WithSuccessfulCompilation() => new(_modules, _modulesByUri, _sourcesByUri,
        _nodeIds, true, Array.Empty<string>(), ModuleResolver, InspectionOptions, FunctionSignatures);

    internal AuthoringSource SourceFor(Uri moduleUri)
    {
        if (!_sourcesByUri.TryGetValue(moduleUri, out var source))
        {
            throw new KeyNotFoundException($"The URI '{moduleUri}' does not identify a module of this snapshot.");
        }

        return source;
    }

    internal AuthoringNodeDescriptor? FindNodeById(int id)
    {
        foreach (var module in _modules)
        {
            var found = module.Root.FindDescendant(node => node.Id == id);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
