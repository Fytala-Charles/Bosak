// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable validation result: outcome, ordered diagnostics, coverage gaps, declared coverage.
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

namespace Bosak.Xslt.Validation;

/// <summary>
/// The immutable, never-throwing result of one static validation run: the classified outcome, the
/// ordered diagnostics collected across all walked modules, the explicitly deferred coverage, and
/// the declared checked coverage of this engine build. A failing expression never aborts the walk:
/// all slots are checked, diagnostics are collected in module-then-document order, and the outcome
/// is decided from the whole set.
/// </summary>
/// <remarks>
/// <para>
/// Outcome precedence (highest first): <see cref="XsltValidationOutcome.InvalidSource"/> (a module
/// could not be decoded or parsed as XML at all), <see cref="XsltValidationOutcome.Invalid"/>
/// (expression/pattern/AVT diagnostics), <see cref="XsltValidationOutcome.Refused"/> (a module
/// reference was refused), <see cref="XsltValidationOutcome.UnsupportedCoverage"/> (deferred
/// constructs present, no diagnostics), <see cref="XsltValidationOutcome.Valid"/>.
/// <see cref="XsltValidationOutcome.Cancelled"/> is reserved for REQ-125 Slice D and is never
/// produced by this build.
/// </para>
/// <para>
/// Validation performs no transformation and no result-document writes; the validated snapshot and
/// every module envelope are observed read-only, so principal and dependency bytes are unchanged.
/// The result is pure managed state: it holds no unmanaged resources, needs no disposal and is safe
/// to share across threads.
/// </para>
/// </remarks>
public sealed class XsltValidationResult
{
    private static readonly XsltValidationCoverage[] DeclaredCoverageBacking =
    {
        XsltValidationCoverage.ExpressionSlots,
        XsltValidationCoverage.PatternSlots,
        XsltValidationCoverage.AvtSlots,
        XsltValidationCoverage.StaticVariableScope,
    };

    private readonly IReadOnlyList<XsltValidationDiagnostic> _diagnostics;
    private readonly IReadOnlyList<XsltValidationCoverageGap> _unsupportedCoverage;

    internal XsltValidationResult(
        XsltValidationOutcome outcome,
        IReadOnlyList<XsltValidationDiagnostic> diagnostics,
        IReadOnlyList<XsltValidationCoverageGap> unsupportedCoverage)
    {
        Outcome = outcome;
        _diagnostics = diagnostics;
        _unsupportedCoverage = unsupportedCoverage;
    }

    /// <summary>Gets the classified outcome of the validation run.</summary>
    public XsltValidationOutcome Outcome { get; }

    /// <summary>Gets whether the outcome is <see cref="XsltValidationOutcome.Valid"/>.</summary>
    public bool IsValid => Outcome == XsltValidationOutcome.Valid;

    /// <summary>
    /// Gets the diagnostics collected across all walked modules, in module processing order and
    /// document order within one module. Empty when nothing failed.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<XsltValidationDiagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// Gets the constructs present in the validated stylesheet whose static analysis is explicitly
    /// deferred. Empty when every construct present falls inside the declared coverage.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<XsltValidationCoverageGap> UnsupportedCoverage => _unsupportedCoverage;

    /// <summary>
    /// Gets the static checks this engine build performs — the declared supported coverage. The
    /// collection is identical for every result of this build; consumers can rely on it to
    /// interpret <see cref="Outcome"/> without probing behavior.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<XsltValidationCoverage> DeclaredCoverage { get; } =
        Array.AsReadOnly(DeclaredCoverageBacking);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Outcome}: {_diagnostics.Count} diagnostic(s), {_unsupportedCoverage.Count} deferred coverage item(s)";
}
