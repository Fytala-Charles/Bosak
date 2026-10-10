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
// ===========================================================================================================================================================

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
        AuthoringInspectionOptions inspectionOptions)
    {
        _modules = modules;
        _modulesByUri = modulesByUri;
        _sourcesByUri = sourcesByUri;
        _nodeIds = nodeIds;
        IsCompilable = isCompilable;
        CompilationDiagnostics = compilationDiagnostics;
        ModuleResolver = moduleResolver;
        InspectionOptions = inspectionOptions;
    }

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
