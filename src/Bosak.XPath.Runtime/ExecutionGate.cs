// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Cooperative execution gate: cancellation token, wall-clock deadline and operation budget checkpoints.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice D)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Runtime;

/// <summary>
/// The reason an <see cref="ExecutionGate"/> checkpoint terminated the run.
/// </summary>
internal enum ExecutionGateInterruptKind
{
    /// <summary>The host-supplied cancellation token was triggered.</summary>
    Cancelled,

    /// <summary>The wall-clock deadline was exceeded.</summary>
    TimeLimitExceeded,

    /// <summary>The engine-operation budget was exhausted.</summary>
    OperationLimitExceeded,
}

/// <summary>
/// Internal control-flow exception thrown by <see cref="ExecutionGate.Checkpoint"/> when a
/// cancellation token, wall-clock deadline or operation budget terminates the run. Carrying the
/// reason as data lets callers classify the termination (for example as the
/// <c>XsltCaptureOutcome.Cancelled</c> outcome) instead of treating it as an engine error. It is
/// never expected to escape a public API surface.
/// </summary>
internal sealed class ExecutionInterruptException : Exception
{
    internal ExecutionInterruptException(ExecutionGateInterruptKind kind, string message)
        : base(message)
    {
        Kind = kind;
    }

    internal ExecutionGateInterruptKind Kind { get; }
}

/// <summary>
/// Cooperative execution control for one controlled run (REQ-125 Slice D). A gate bundles the
/// three host controls — a cancellation token, a wall-clock deadline and an engine-operation
/// budget — into a single checkpoint call cheap enough to sit on hot engine seams. A run and all
/// its nested calls share one gate instance, so nested transforms inherit the *remaining* budget
/// and deadline by construction; separate runs hold separate instances, which isolates
/// cancellation and limits between runs (including concurrent runs — the gate is used from a
/// single transformation thread, so no synchronization is required).
/// </summary>
/// <remarks>
/// <para>
/// Checkpoint semantics: one <see cref="Checkpoint"/> call corresponds to one engine operation
/// (an XSLT instruction dispatch, template invocation, result-document production, message
/// emission or output serialization, as wired by the calling layer). The operation budget counts
/// these checkpoints exactly, so termination against a budget is deterministic for a given
/// stylesheet, input and budget. The token and deadline are observed on every 64th checkpoint
/// (amortized batch), so cancellation and wall-clock latency are bounded by the work of at most
/// 64 checkpoints. A single XPath expression evaluation between checkpoints (for example a long
/// <c>for</c> loop inside one <c>select</c> attribute) is not interrupted mid-evaluation — it is
/// observed at the next checkpoint.
/// </para>
/// <para>
/// Fast path: when no controls are configured the gate is not created at all; call sites key off
/// a null reference (a single branch). When a gate exists, <see cref="Checkpoint"/> costs one
/// decrement and comparison, plus a token read and a tick count when the budget batch expires.
/// </para>
/// </remarks>
internal sealed class ExecutionGate
{
    private readonly CancellationToken _cancellationToken;
    private readonly long _deadlineTick;
    private long _remainingBudgetChecks;
    private long _remainingOperations;

    private ExecutionGate(CancellationToken cancellationToken, long deadlineTick, long operationBudget)
    {
        _cancellationToken = cancellationToken;
        _deadlineTick = deadlineTick;
        _remainingOperations = operationBudget;
    }

    /// <summary>
    /// Creates a gate for the supplied controls. <paramref name="timeLimit"/> is turned into a
    /// wall-clock deadline at creation; <paramref name="operationBudget"/> counts engine
    /// operations (checkpoints).
    /// </summary>
    internal static ExecutionGate Create(CancellationToken cancellationToken, TimeSpan? timeLimit, long? operationBudget)
    {
        long deadlineTick = 0;
        if (timeLimit is { } limit)
            deadlineTick = Environment.TickCount64 + (long)limit.TotalMilliseconds;

        return new ExecutionGate(
            cancellationToken,
            deadlineTick,
            operationBudget ?? long.MaxValue);
    }

    /// <summary>
    /// Checks one engine operation against the gate: decrements the exact operation budget and
    /// amortizes the token/deadline observations over batches of 64 checkpoints. Throws
    /// <see cref="ExecutionInterruptException"/> carrying the reason when a control terminates
    /// the run.
    /// </summary>
    internal void Checkpoint()
    {
        if (--_remainingOperations < 0)
            throw new ExecutionInterruptException(ExecutionGateInterruptKind.OperationLimitExceeded,
                "The engine-operation budget was exhausted.");

        if (--_remainingBudgetChecks > 0)
            return;

        _remainingBudgetChecks = 64;

        if (_cancellationToken.IsCancellationRequested)
            throw new ExecutionInterruptException(ExecutionGateInterruptKind.Cancelled,
                "The run was cancelled by the host-supplied cancellation token.");

        if (_deadlineTick != 0 && Environment.TickCount64 > _deadlineTick)
            throw new ExecutionInterruptException(ExecutionGateInterruptKind.TimeLimitExceeded,
                "The wall-clock time limit was exceeded.");
    }
}
