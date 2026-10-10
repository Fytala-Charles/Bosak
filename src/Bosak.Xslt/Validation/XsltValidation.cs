// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Static entry point of the controlled static validation boundary (REQ-125 Slice A).
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.Xslt.Authoring;

namespace Bosak.Xslt.Validation;

/// <summary>
/// The static entry point of the controlled validation boundary (REQ-125). Validation walks the
/// full declared surface of a stylesheet — every expression, match-pattern and attribute value
/// template slot in the principal module and every module reachable through the caller-controlled
/// module resolver — compiles each in its real static context (in-scope namespaces, default element
/// namespace, base URI, effective XPath version and in-scope variable declarations), and reports a
/// classified, immutable result. It performs no transformation and no result-document writes, and it
/// never mutates the validated source: principal and dependency bytes, and any authoring snapshot
/// supplied, are observed read-only.
/// </summary>
/// <remarks>
/// <para>
/// The overloads compose with the REQ-124 authoring surface:
/// <see cref="Validate(AuthoringSnapshot, XsltValidationOptions?)"/> validates an existing snapshot
/// without any further module resolution (dependency bytes cannot change), while
/// <see cref="Validate(AuthoringSource, IAuthoringModuleResolver?, XsltValidationOptions?)"/> and
/// <see cref="Validate(byte[], Uri, IAuthoringModuleResolver?, XsltValidationOptions?)"/> create the
/// snapshot for the caller through the same inspection pipeline. All overloads share identical
/// validation semantics and byte-preservation guarantees.
/// </para>
/// <para>
/// What is checked — the declared coverage — is fixed per engine build and listed on
/// <see cref="XsltValidationResult.DeclaredCoverage"/>: expression slots, pattern slots, AVT slots
/// and static variable scoping. Constructs whose analysis is deferred (schema imports, packages)
/// are reported explicitly on <see cref="XsltValidationResult.UnsupportedCoverage"/> and downgrade
/// an otherwise clean result to <see cref="XsltValidationOutcome.UnsupportedCoverage"/>; they are
/// never silently skipped. Known limits of the checked set: expressions inside subtrees excluded by
/// a literal <c>use-when="false()'"</c> are not analyzed (mirroring XSLT removal semantics); QName
/// slots (for example <c>xsl:call-template/@name</c>) are classified but not resolved against their
/// declarations; variable references inside match patterns are not scope-checked; schema-aware
/// expression checks require schema support that validation does not acquire.
/// </para>
/// </remarks>
public static class XsltValidation
{
    /// <summary>
    /// Validates an existing authoring snapshot without performing any further module resolution.
    /// The snapshot's module envelopes and descriptors are observed read-only: the original bytes of
    /// every module (and the snapshot itself) are unchanged by validation.
    /// </summary>
    /// <param name="snapshot">The inspected authoring snapshot to validate.</param>
    /// <param name="options">Optional validation controls.</param>
    /// <returns>The immutable validation result; never <see langword="null"/> and never throws for bad source.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="snapshot"/> is null.</exception>
    public static XsltValidationResult Validate(AuthoringSnapshot snapshot, XsltValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new XsltValidationEngine(options).Validate(snapshot);
    }

    /// <summary>
    /// Inspects the principal module — and, through the supplied resolver, the modules it includes
    /// or imports — and validates the resulting snapshot. Module resolution is caller-controlled:
    /// the default resolver reads from the file system; supply an
    /// <see cref="IAuthoringModuleResolver"/> to validate against approved immutable module bytes.
    /// Inspection performs no transformation and no implicit resource acquisition beyond what the
    /// resolver supplies.
    /// </summary>
    /// <param name="principal">The retained source envelope of the principal module.</param>
    /// <param name="resolver">The module resolver; <see langword="null"/> uses <see cref="FileSystemAuthoringModuleResolver"/>.</param>
    /// <param name="options">Optional validation controls.</param>
    /// <returns>The immutable validation result; never <see langword="null"/> and never throws for bad source.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is null.</exception>
    public static XsltValidationResult Validate(
        AuthoringSource principal,
        IAuthoringModuleResolver? resolver = null,
        XsltValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        // Inspection is the module-graph + byte-retention front end of validation; the compile
        // attempt is disabled because the validation walk subsumes it with full cross-module
        // coverage and a real per-slot static context.
        var inspection = XsltAuthoring.Inspect(
            principal,
            resolver,
            new AuthoringInspectionOptions { AttemptCompilation = false });
        if (!inspection.IsSuccess)
        {
            return XsltValidationEngine.SourceFailureResult(inspection.Failure!);
        }

        return new XsltValidationEngine(options).Validate(inspection.Snapshot!);
    }

    /// <summary>
    /// Convenience overload validating principal module bytes directly: wraps them in a retained
    /// source envelope, inspects the module graph through the supplied resolver and validates the
    /// snapshot. The caller's array is defensively copied by
    /// <see cref="AuthoringSource.TryCreate(byte[], Uri, out AuthoringSource?, out AuthoringFailure?)"/>;
    /// byte preservation holds exactly as for the other overloads.
    /// </summary>
    /// <param name="principalBytes">The exact original bytes of the principal module, including any byte-order mark.</param>
    /// <param name="baseUri">The absolute base URI of the principal module.</param>
    /// <param name="resolver">The module resolver; <see langword="null"/> uses <see cref="FileSystemAuthoringModuleResolver"/>.</param>
    /// <param name="options">Optional validation controls.</param>
    /// <returns>The immutable validation result; never <see langword="null"/> and never throws for bad source.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principalBytes"/> or <paramref name="baseUri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseUri"/> is not absolute.</exception>
    public static XsltValidationResult Validate(
        byte[] principalBytes,
        Uri baseUri,
        IAuthoringModuleResolver? resolver = null,
        XsltValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(principalBytes);
        ArgumentNullException.ThrowIfNull(baseUri);

        if (!AuthoringSource.TryCreate(principalBytes, baseUri, out var source, out var failure))
        {
            return XsltValidationEngine.SourceFailureResult(failure!);
        }

        return Validate(source!, resolver, options);
    }
}
