// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Options and result types for authoring inspection, plus the static entry point.
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-124 Slice C: lifecycle and sharing remarks on the inspection result                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Optional controls for <see cref="AuthoringInspector.Inspect"/>. Defaults are conservative: the
/// principal module is compiled for diagnostics only, and module resolution is bounded.
/// </summary>
public sealed class AuthoringInspectionOptions
{
    /// <summary>
    /// Gets or sets whether inspection attempts to compile the principal module on the derived document
    /// to populate <see cref="AuthoringSnapshot.IsCompilable"/> and
    /// <see cref="AuthoringSnapshot.CompilationDiagnostics"/>. Semantic diagnostics are data, never an
    /// inspection failure. The default is <see langword="true"/>.
    /// </summary>
    public bool AttemptCompilation { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of modules (including the principal) one snapshot will inspect.
    /// References beyond the bound are recorded as edges with a diagnostic instead of being fetched.
    /// The default is 256.
    /// </summary>
    public int MaxModuleCount { get; set; } = 256;
}

/// <summary>
/// The outcome of one authoring inspection: either a success carrying an
/// <see cref="AuthoringSnapshot"/>, or a classified <see cref="AuthoringFailure"/>. Inspection never
/// throws for bad source; it classifies. (Programmer misuse — null arguments, relative base URIs —
/// still throws <see cref="ArgumentException"/>.)
/// </summary>
/// <remarks>
/// The result is a short-lived carrier: keep the <see cref="Snapshot"/> (or the
/// <see cref="AuthoringFailure"/>) as long as needed and discard the result. On success the snapshot
/// is immutable, holds no unmanaged resources, needs no disposal and is safe to share across threads;
/// its node identities are snapshot-scoped and must not be reused against later inspections or
/// candidates.
/// </remarks>
public sealed class AuthoringInspectionResult
{
    private AuthoringInspectionResult(AuthoringSnapshot? snapshot, AuthoringFailure? failure)
    {
        Snapshot = snapshot;
        Failure = failure;
    }

    /// <summary>Gets whether inspection succeeded and <see cref="Snapshot"/> is available.</summary>
    public bool IsSuccess => Snapshot is not null;

    /// <summary>Gets the authoring snapshot on success; <see langword="null"/> otherwise.</summary>
    public AuthoringSnapshot? Snapshot { get; }

    /// <summary>Gets the classified failure on failure; <see langword="null"/> otherwise.</summary>
    public AuthoringFailure? Failure { get; }

    /// <summary>Creates a successful inspection result.</summary>
    /// <param name="snapshot">The inspected snapshot.</param>
    /// <returns>The success result.</returns>
    public static AuthoringInspectionResult SuccessResult(AuthoringSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new AuthoringInspectionResult(snapshot, null);
    }

    /// <summary>Creates a failed inspection result.</summary>
    /// <param name="failure">The classified failure.</param>
    /// <returns>The failure result.</returns>
    public static AuthoringInspectionResult FailureResult(AuthoringFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new AuthoringInspectionResult(null, failure);
    }
}

/// <summary>
/// The static entry point of the source-preserving authoring boundary. All capabilities hang off
/// <see cref="Inspect"/>; inspection performs no transformation and no implicit external resource
/// acquisition beyond what the supplied module resolver does.
/// </summary>
public static class XsltAuthoring
{
    /// <summary>
    /// Inspects one principal module and — through the supplied resolver — the modules it includes or
    /// imports, returning a snapshot of immutable, source-backed descriptors or a classified failure.
    /// </summary>
    /// <param name="principal">The retained source envelope of the principal module.</param>
    /// <param name="resolver">The module resolver; <see langword="null"/> uses <see cref="FileSystemAuthoringModuleResolver"/>.</param>
    /// <param name="options">Optional inspection controls.</param>
    /// <returns>The inspection result.</returns>
    public static AuthoringInspectionResult Inspect(
        AuthoringSource principal,
        IAuthoringModuleResolver? resolver = null,
        AuthoringInspectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return new AuthoringInspector().Inspect(principal, resolver, options);
    }
}
