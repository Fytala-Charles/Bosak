// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Optional controls for captured transformation output: bounds, identity and entry-point controls.
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: CancellationToken, TimeLimit and MaxEngineOperations (cooperative       |
//                      |                  |       |                | execution controls; engine-checkpoint semantics documented per option)                   |
//                      | Charles Korthout | 0.3   | 11-10-2026     | C-F1/D-F1: byte-production profiles and terminal control semantics.                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Capture;

/// <summary>
/// Optional controls for <see cref="Bosak.Xslt.Api.XsltExecutable.CaptureTransform"/>. All settings are
/// opt-in; the defaults capture every output produced by the transformation with no bounds. Capture is
/// always IO-free: no output definition — the principal result or any secondary
/// <c>xsl:result-document</c> — is written to the file system or the network in capture mode.
/// </summary>
/// <remarks>
/// <para>
/// Bounds are enforced during production: each secondary output is serialized to its capture bytes as
/// soon as the producing <c>xsl:result-document</c> instruction completes, and the transformation is
/// terminated at the first output that exceeds a configured bound. Outputs completed before the bound
/// was hit are retained on the result; the bound breach itself is reported as a classified failure
/// entry (never as an exception escaping this API). See
/// <see cref="XsltCaptureFailureKind"/> for the taxonomy.
/// </para>
/// <para>
/// Execution control (REQ-125 Slice D): <see cref="CancellationToken"/>, <see cref="TimeLimit"/> and
/// <see cref="MaxEngineOperations"/> terminate a runaway transformation cooperatively at engine
/// checkpoints and are classified as <see cref="XsltCaptureOutcome.Cancelled"/> (token) or as
/// <c>TimeLimitExceeded</c>/<c>OperationLimitExceeded</c> failures (codes <c>XV0011</c>/<c>XV0012</c>).
/// A run and its nested <c>fn:transform</c> calls share one gate: nested transforms inherit the
/// remaining budget, deadline and token. Cancellation and limits never surface as exceptions through
/// the capture API.
/// </para>
/// <para>
/// Message capture (see <see cref="MaxMessages"/>) is a separate, bounded channel:
/// <c>xsl:message</c> output is never part of the captured result bytes.
/// </para>
/// </remarks>
public sealed class XsltCaptureOptions
{
    /// <summary>
    /// Gets or sets the evaluation context (stylesheet parameters) for the transformation.
    /// </summary>
    public EvaluationContext? Context { get; set; }

    /// <summary>
    /// Gets or sets the name of the initial template to execute. <see langword="null"/> (the default)
    /// uses the default initial template for the supplied source.
    /// </summary>
    public string? InitialTemplate { get; set; }

    /// <summary>
    /// Gets or sets the name of the initial mode to use. <see langword="null"/> (the default) uses the
    /// default mode.
    /// </summary>
    public string? InitialMode { get; set; }

    /// <summary>
    /// Gets or sets the base output URI of the transformation. This URI is the resolved identity of the
    /// captured principal output (even when the stylesheet never names it) and the resolution base for
    /// relative <c>xsl:result-document</c> hrefs. When <see langword="null"/> (the default), the engine's
    /// usual base output URI rules apply and the principal identity is the empty string.
    /// </summary>
    public string? BaseOutputUri { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of secondary (<c>xsl:result-document</c>) outputs captured.
    /// When the bound is exceeded, the transformation is terminated at the offending output and the
    /// breach is reported as a classified failure (code <c>XV0006</c>). <see langword="null"/> (the
    /// default) captures every secondary output. Zero captures no secondary output at all.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int? MaxSecondaryOutputCount
    {
        get => _maxSecondaryOutputCount;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxSecondaryOutputCount), value, "The secondary output count bound must not be negative.");
            _maxSecondaryOutputCount = value;
        }
    }

    private int? _maxSecondaryOutputCount;

    /// <summary>
    /// Gets or sets the maximum capture bytes for any single output (principal or secondary). An output
    /// whose serialization exceeds the bound is dropped and the transformation is terminated; the breach
    /// is reported as a classified failure (code <c>XV0007</c>). <see langword="null"/> (the default)
    /// applies no per-output bound. Zero allows only an output with no bytes (including no BOM).
    /// Encoding stops during production on overflow; fixed-size encoding lookahead is discarded.
    /// Byte-bounded capture supports text (including character maps), XML, HTML and XHTML;
    /// JSON/adaptive, text Unicode normalization, non-text character maps and non-XDocument
    /// markup fallbacks are refused as UnsupportedSerializationProfile/XV0014. These limits do
    /// not bound result-tree construction, normalization or XPath working memory.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public long? MaxBytesPerOutput
    {
        get => _maxBytesPerOutput;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxBytesPerOutput), value, "The per-output byte bound must not be negative.");
            _maxBytesPerOutput = value;
        }
    }

    private long? _maxBytesPerOutput;

    /// <summary>
    /// Gets or sets the maximum total capture bytes across the principal and all secondary outputs.
    /// Capturing an output that would raise the total over the bound is refused and the transformation
    /// is terminated; the breach is reported as a classified failure (code <c>XV0008</c>).
    /// <see langword="null"/> (the default) applies no aggregate bound. Zero retains no output bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public long? MaxTotalBytes
    {
        get => _maxTotalBytes;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxTotalBytes), value, "The aggregate byte bound must not be negative.");
            _maxTotalBytes = value;
        }
    }

    private long? _maxTotalBytes;

    /// <summary>
    /// Gets or sets the maximum number of <c>xsl:message</c> texts retained on the result. Messages
    /// beyond the bound are discarded (the message channel stays bounded) and the overflow is reported
    /// as a classified failure (code <c>XV0010</c>). <see langword="null"/> (the default) retains every
    /// message. The bound counts messages; it never truncates result output bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int? MaxMessages
    {
        get => _maxMessages;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxMessages), value, "The message bound must not be negative.");
            _maxMessages = value;
        }
    }

    private int? _maxMessages;

    /// <summary>
    /// Gets or sets the cooperative cancellation token for the captured transformation. The token
    /// is observed at batched engine checkpoints and unconditionally at serialization/completion
    /// boundaries (see <see cref="TimeLimit"/> for the checkpoint
    /// definition); triggering it terminates the run with the classified
    /// <see cref="XsltCaptureOutcome.Cancelled"/> outcome, retaining outputs captured before the
    /// checkpoint observed the trigger. <see langword="null"/> (the default) disables cancellation.
    /// Cancellation never surfaces as <see cref="OperationCanceledException"/> through this API.
    /// </summary>
    public CancellationToken? CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets the wall-clock budget for the captured transformation. The budget starts when
    /// the run starts and is checked at batched engine checkpoints, serialization/completion
    /// boundaries and between bounded encoding chunks; exceeding it terminates the run with a
    /// classified failure (code <c>XV0011</c>), retaining outputs captured before the checkpoint.
    /// <see langword="null"/> (the default) applies no time limit. This is a cooperative bound, not
    /// a hard preemption; Braid's process termination remains the hard wall-clock control.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative or zero.</exception>
    public TimeSpan? TimeLimit
    {
        get => _timeLimit;
        set
        {
            if (value.HasValue && value.Value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(TimeLimit), value, "The time limit must be positive.");
            _timeLimit = value;
        }
    }

    private TimeSpan? _timeLimit;

    /// <summary>
    /// Gets or sets the engine-operation budget for the captured transformation. One engine
    /// operation is one checkpoint firing: an XSLT instruction dispatch, a template invocation
    /// (xsl:call-template / xsl:apply-templates), a secondary result-document production, an
    /// <c>xsl:message</c> emission, or an output serialization. The budget counts these operations
    /// exactly, so for a given stylesheet and input the abort point is deterministic; a single
    /// XPath expression between checkpoints (for example a long loop inside one <c>select</c>
    /// attribute) is not interrupted mid-evaluation. Exceeding the budget terminates the run with
    /// a classified failure (code <c>XV0012</c>). <see langword="null"/> (the default) applies no
    /// operation budget.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public long? MaxEngineOperations
    {
        get => _maxEngineOperations;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxEngineOperations), value, "The engine-operation budget must not be negative.");
            _maxEngineOperations = value;
        }
    }

    private long? _maxEngineOperations;
}
