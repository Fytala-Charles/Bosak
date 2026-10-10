// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable captured-transformation result: outcome, principal/secondary outputs, failures, messages.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice C)                                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: Cancelled outcome + checkpoint/coverage remarks                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// The immutable, never-throwing result of one captured transformation (REQ-125 Slice C): the
/// classified outcome, the captured principal output (when retained), the captured secondary outputs
/// in production order, the classified failure entries, and the bounded <c>xsl:message</c> channel.
/// A transformation that aborts mid-run never loses its already-captured outputs silently: they are
/// present here together with a failure entry describing the abort.
/// </summary>
/// <remarks>
/// <para>
/// Outcome precedence: <see cref="XsltCaptureOutcome.Success"/> only when the transformation completed
/// and every produced output was captured with no failure entries. Any classified failure downgrades
/// the outcome — <see cref="XsltCaptureOutcome.Partial"/> when at least one output was captured (or,
/// for a message bound breach, the transformation completed), <see cref="XsltCaptureOutcome.Failed"/>
/// when the transformation failed before any output was captured. Host cancellation produces the
/// distinct <see cref="XsltCaptureOutcome.Cancelled"/> outcome (outputs completed before the
/// observing checkpoint are retained). A partial or cancelled result is diagnostic evidence, never a
/// successful run.
/// </para>
/// <para>
/// Coverage: capture covers the principal output and every secondary
/// <c>xsl:result-document</c> output the transformation completes, including raw
/// (<c>method="json"</c>, <c>method="adaptive"</c>, <c>build-tree="no"</c>) outputs. Exclusions:
/// the capture entry point does not accept an initial match selection or a raw-result delivery
/// format (the existing <c>Transform</c> family remains the surface for those); messages are a
/// separate bounded channel and are never part of output bytes; cancellation and execution limits
/// are cooperative and observed at engine checkpoints — a single XPath expression between
/// checkpoints is not interrupted mid-evaluation (see <see cref="XsltCaptureOptions.MaxEngineOperations"/>).
/// </para>
/// <para>
/// Capture performs no filesystem or network writes for any output. The result is pure managed state:
/// it holds no unmanaged resources, needs no disposal and is safe to share across threads.
/// </para>
/// </remarks>
public sealed class XsltCaptureResult
{
    private readonly IReadOnlyList<XsltCapturedOutput> _secondaryOutputs;
    private readonly IReadOnlyList<XsltCaptureFailure> _failures;
    private readonly IReadOnlyList<string> _messages;

    internal XsltCaptureResult(
        XsltCaptureOutcome outcome,
        XsltCapturedOutput? principal,
        IReadOnlyList<XsltCapturedOutput> secondaryOutputs,
        IReadOnlyList<XsltCaptureFailure> failures,
        IReadOnlyList<string> messages)
    {
        Outcome = outcome;
        Principal = principal;
        _secondaryOutputs = secondaryOutputs;
        _failures = failures;
        _messages = messages;
    }

    /// <summary>Gets the classified outcome of the captured transformation.</summary>
    public XsltCaptureOutcome Outcome { get; }

    /// <summary>Gets whether the outcome is <see cref="XsltCaptureOutcome.Success"/>.</summary>
    public bool IsSuccess => Outcome == XsltCaptureOutcome.Success;

    /// <summary>
    /// Gets the captured principal output, or <see langword="null"/> when it was not retained (the
    /// transformation failed before producing it, or it breached a configured byte bound).
    /// </summary>
    public XsltCapturedOutput? Principal { get; }

    /// <summary>
    /// Gets the captured secondary (<c>xsl:result-document</c>) outputs in production order. Outputs
    /// dropped by a bound breach or lost with an abort are not present; their absence is classified in
    /// <see cref="Failures"/>.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<XsltCapturedOutput> SecondaryOutputs => _secondaryOutputs;

    /// <summary>
    /// Gets the classified failure entries of the run. Empty exactly when the outcome is
    /// <see cref="XsltCaptureOutcome.Success"/>.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<XsltCaptureFailure> Failures => _failures;

    /// <summary>
    /// Gets the <c>xsl:message</c> texts emitted during the transformation, in emission order, retained
    /// up to <see cref="XsltCaptureOptions.MaxMessages"/>. The message channel is separate from result
    /// output: these texts are never part of any captured output's bytes.
    /// </summary>
    /// <remarks>The collection is immutable: a read-only wrapper over privately retained storage.</remarks>
    public IReadOnlyList<string> Messages => _messages;

    /// <inheritdoc />
    public override string ToString() =>
        $"{Outcome}: principal {(Principal is null ? "not captured" : "captured")}, {_secondaryOutputs.Count} secondary output(s), {_failures.Count} failure(s), {_messages.Count} message(s)";
}
