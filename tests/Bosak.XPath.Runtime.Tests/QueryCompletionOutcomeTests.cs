// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies structured raw completion and original ordered lifetime errors.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Tests owned internal run outcomes and completion barriers on actual source leases.</summary>
public sealed class QueryCompletionOutcomeTests
{
    private static QuerySourceSession Session() => new(new(ImmutableDictionary<string,string>.Empty,"completion-report"), _ => XdmValue.FromInteger(1), 4);
    private sealed class Lease(Action release) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => release();
    }
    private sealed class Source : IQuerySourceInput
    {
        internal Action? CheckAction, ReleaseAction;
        internal int Releases;
        IXdmNode IQuerySourceInput.Root => throw new InvalidOperationException("No root reads expected.");
        bool IQuerySourceInput.OwnsNode(IXdmNode node) => false;
        void IQuerySourceInput.ThrowIfUnavailable() => CheckAction?.Invoke();
        internal IDisposable Enter() => new Lease(() => { Releases++; ReleaseAction?.Invoke(); });
    }
    private sealed class Provider(Action release) : IQueryOperationLifetime
    {
        /// <inheritdoc/>
        public IQueryOperationLease Enter(CancellationToken cancellationToken) => new ProviderLease(release);
        private sealed class ProviderLease(Action release) : IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken cancellationToken) => cancellationToken.ThrowIfCancellationRequested();
            /// <inheritdoc/>
            public void Dispose() => release();
        }
    }

    /// <summary>Successful empty membership distinguishes a returned null from no result.</summary>
    [Fact] public void CO01_SuccessfulNullResult()
    {
        using var session = Session(); QuerySourceSession.Operation? retained = null;
        var result = session.RunOutcome<string?>(session.Environment, [], operation => { retained = operation; return null; });
        Assert.True(result.IsSuccess); Assert.True(result.HasResult); Assert.Null(result.Result);
        Assert.Null(result.AdmissionError); Assert.Null(result.WorkError); Assert.Empty(result.CleanupErrors);
        Assert.Throws<ObjectDisposedException>(retained!.Check);
    }

    /// <summary>Membership refusal never executes work.</summary>
    /// <param name="kind">The invalid membership shape.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void CO02_InvalidMembership(int kind)
    {
        using var session = Session(); using var other = Session(); var input = new Source();
        var own = session.OpenSnapshot(session.Environment, input, input.Enter);
        var foreign = other.OpenSnapshot(other.Environment, input, input.Enter);
        ImmutableArray<QuerySourceSession.Snapshot> members = kind switch
        { 0 => default, 1 => [null!], 2 => [own, own], _ => [foreign] };
        var result = session.RunOutcome<int>(session.Environment, members, _ => throw new Exception("work poison"));
        Assert.False(result.IsSuccess); Assert.False(result.HasResult);
        Assert.IsAssignableFrom<ArgumentException>(result.AdmissionError); Assert.Null(result.WorkError); Assert.Empty(result.CleanupErrors);
    }

    /// <summary>Raw pre-cancellation is admission failure with the original token.</summary>
    [Fact] public void CO03_PreCancellation()
    {
        using var session = Session(); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var result = session.RunOutcome<int>(session.Environment, [], _ => throw new Exception("work poison"), cancellation.Token);
        Assert.Equal(cancellation.Token, Assert.IsAssignableFrom<OperationCanceledException>(result.AdmissionError).CancellationToken);
        Assert.False(result.HasResult); Assert.Null(result.WorkError); Assert.Empty(result.CleanupErrors);
    }

    /// <summary>Admission rollback retains its original nested aggregate separately from cleanup.</summary>
    [Fact] public void CO04_AdmissionRollbackErrors()
    {
        using var session = Session(); var first = new Source(); var second = new Source();
        var a = session.OpenSnapshot(session.Environment, first, first.Enter);
        var b = session.OpenSnapshot(session.Environment, second, second.Enter);
        var primary = new AggregateException(new Exception("entry")); var cleanup = new AggregateException(new Exception("release"));
        first.ReleaseAction = () => throw cleanup;
        second.CheckAction = () => { a.Dispose(); throw primary; };
        var result = session.RunOutcome<int>(session.Environment, [a,b], _ => throw new Exception("work poison"));
        Assert.Same(primary, result.AdmissionError); Assert.Null(result.WorkError); Assert.False(result.HasResult);
        Assert.Same(cleanup, Assert.Single(result.CleanupErrors)); Assert.Equal(1, first.Releases);
        second.CheckAction = null;
    }

    /// <summary>Work errors preserve original aggregate boundaries and do not become admission errors.</summary>
    [Fact] public void CO05_WorkErrorIdentity()
    {
        using var session = Session(); var error = new AggregateException(new Exception("work"));
        var result = session.RunOutcome<int>(session.Environment, [], _ => throw error);
        Assert.Same(error, result.WorkError); Assert.Null(result.AdmissionError); Assert.Empty(result.CleanupErrors);
        Assert.False(result.IsSuccess); Assert.False(result.HasResult);
    }

    /// <summary>A produced result remains diagnostic when the final cancellation guard refuses delivery.</summary>
    [Fact] public void CO06_ResultAndFinalCancellation()
    {
        using var session = Session(); using var cancellation = new CancellationTokenSource(); var value = new object();
        var result = session.RunOutcome(session.Environment, [], _ => { cancellation.Cancel(); return value; }, cancellation.Token);
        Assert.True(result.HasResult); Assert.Same(value, result.Result); Assert.False(result.IsSuccess);
        Assert.IsAssignableFrom<OperationCanceledException>(result.WorkError); Assert.Null(result.AdmissionError);
    }

    /// <summary>Source closure keeps a produced value but reports work and every reverse cleanup error.</summary>
    [Fact] public void CO07_ClosedSourcesReleaseInReverse()
    {
        using var session = Session(); var events = new List<string>(); var first = new Source(); var second = new Source();
        var a = session.OpenSnapshot(session.Environment, first, first.Enter);
        var b = session.OpenSnapshot(session.Environment, second, second.Enter);
        var one = new AggregateException(new Exception("first")); var two = new Exception("second");
        first.ReleaseAction = () => { events.Add("first"); throw one; };
        second.ReleaseAction = () => { events.Add("second"); throw two; };
        var result = session.RunOutcome(session.Environment, [a,b], _ => { a.Dispose(); b.Dispose(); return 42; });
        Assert.True(result.HasResult); Assert.Equal(42, result.Result); Assert.False(result.IsSuccess);
        Assert.IsType<ObjectDisposedException>(result.WorkError); Assert.Null(result.AdmissionError);
        Assert.Equal(new Exception[] { two,one }, result.CleanupErrors); Assert.Equal(new[] { "second","first" }, events);
        Assert.Equal(1, first.Releases); Assert.Equal(1, second.Releases);
    }

    /// <summary>Provider cleanup failure alone cannot mark a successful callback as successful overall.</summary>
    [Fact] public void CO08_ResultWithCleanupFailure()
    {
        using var session = Session(); var cleanup = new Exception("cleanup"); var provider = new Provider(() => throw cleanup);
        var result = session.RunOutcome(session.Environment, [], operation => operation.CallProvider(provider, _ => 7));
        Assert.True(result.HasResult); Assert.Equal(7, result.Result); Assert.False(result.IsSuccess);
        Assert.Null(result.AdmissionError); Assert.Null(result.WorkError); Assert.Same(cleanup, Assert.Single(result.CleanupErrors));
    }

    /// <summary>Paired providers release target then source before raw cleanup, preserving duplicates.</summary>
    /// <returns>The repeated completion check.</returns>
    [Fact] public async Task CO09_OrderedCompletionAndRepeatedObservers()
    {
        var events = new List<string>(); var error = new AggregateException(new Exception("shared"));
        var source = new Provider(() => { events.Add("source"); throw error; });
        var target = new Provider(() => { events.Add("target"); throw error; });
        var scope = new QueryProviderOperationScope(() => { }, default);
        scope.CallPair(source,target,_ => 1); var work = new AggregateException(new Exception("work"));
        List<Exception> Raw() { events.Add("raw"); return [error]; }
        var first = scope.CompleteOutcomeAsync(Raw,work);
        var second = scope.CompleteOutcomeAsync(() => throw new Exception("repeat poison"),new Exception("ignored"));
        Assert.Same(first,second); var report = await first;
        Assert.Same(work, report.Primary); Assert.Equal(3, report.CleanupErrors.Length);
        Assert.All(report.CleanupErrors, e => Assert.Same(error,e));
        Assert.Equal(new[] { "target","source","raw" }, events);
        var thrown = await Assert.ThrowsAsync<AggregateException>(() => scope.CompleteAsync(Raw));
        Assert.Equal(new Exception[] { work,error,error,error },thrown.InnerExceptions);
        Assert.Equal(3,events.Count);
    }

    /// <summary>Thrown raw cleanup callbacks remain one original cleanup item.</summary>
    /// <returns>The raw cleanup check.</returns>
    [Fact] public async Task CO10_RawCallbackFailure()
    {
        var error = new AggregateException(new Exception("raw")); var scope = new QueryProviderOperationScope(() => { },default);
        var report = await scope.CompleteOutcomeAsync(() => throw error);
        Assert.Null(report.Primary); Assert.Same(error,Assert.Single(report.CleanupErrors));
        Assert.Same(error,await Assert.ThrowsAsync<AggregateException>(() => scope.CompleteAsync(() => [])));
    }

    /// <summary>Actual host tails retain an exclusive file through closure and abandoned observation.</summary>
    /// <returns>The completion barrier check.</returns>
    [Fact] public async Task CO11_RealTailAndCanceledWait()
    {
        var path = Path.GetTempFileName();
        using var file = new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        using var entered = new ManualResetEventSlim(); using var resume = new ManualResetEventSlim();
        var scope = new QueryProviderOperationScope(() => { },default); int released = 0;
        var call = Task.Run(() => Record.Exception(() => scope.CallHost(new object(),_ =>
        { entered.Set(); Assert.True(resume.Wait(TimeSpan.FromSeconds(20))); file.WriteByte(42); return 1; })));
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));
            var completion = scope.CompleteOutcomeAsync(() => { Interlocked.Increment(ref released); file.Dispose(); return []; });
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => completion.WaitAsync(cancellation.Token));
            Assert.False(completion.IsCompleted); Assert.Equal(0,released); Assert.True(file.CanWrite);
            resume.Set(); Assert.IsType<ObjectDisposedException>(await call.WaitAsync(TimeSpan.FromSeconds(10)));
            var report = await completion.WaitAsync(TimeSpan.FromSeconds(10)); Assert.Empty(report.CleanupErrors);
            Assert.Equal(1,released); Assert.False(file.CanWrite);
        }
        finally { resume.Set(); await call.WaitAsync(TimeSpan.FromSeconds(10)); }
    }

    /// <summary>A provider work error and cleanup errors remain distinct in the actual source run.</summary>
    [Fact] public void CO12_WorkAndProviderCleanup()
    {
        using var session = Session(); var work = new AggregateException(new Exception("work")); var cleanup = new Exception("cleanup");
        var provider = new Provider(() => throw cleanup);
        var report = session.RunOutcome<int>(session.Environment, [], op => op.CallProvider<int>(provider,_ => throw work));
        Assert.Same(work,report.WorkError); Assert.Same(cleanup,Assert.Single(report.CleanupErrors));
        Assert.Null(report.AdmissionError); Assert.False(report.HasResult); Assert.False(report.IsSuccess);
    }

    /// <summary>The throwing adapter retains its shared faulted task even for cancellation exceptions.</summary>
    /// <returns>The legacy task-state check.</returns>
    [Fact] public async Task CO13_ThrowingObserversKeepFaultState()
    {
        using var cancellation = new CancellationTokenSource();
        var error = new OperationCanceledException(cancellation.Token);
        var scope = new QueryProviderOperationScope(() => { },default);
        var first = scope.CompleteAsync(() => [],error);
        var second = scope.CompleteAsync(() => throw new Exception("repeat poison"));
        Assert.Same(first,second);
        Assert.Same(error,await Assert.ThrowsAsync<OperationCanceledException>(() => first));
        Assert.True(first.IsFaulted); Assert.False(first.IsCanceled);
        Assert.Same(error,(await scope.CompleteOutcomeAsync(() => [])).Primary);
    }
}
