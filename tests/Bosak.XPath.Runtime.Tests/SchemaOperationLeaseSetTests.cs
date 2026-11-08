// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Verifies private operation lease acquisition, rollback and completion.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Verify separate admission/rollback outcomes and unchanged throwing entry (1.73.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises deterministic admission failures and raw ownership boundaries.</summary>
public sealed class SchemaOperationLeaseSetTests
{
    private sealed class Lease(Action release) : IDisposable
    {
        /// <summary>Invokes the injected release callback.</summary>
        public void Dispose() => release();
    }
    private sealed class Fixture
    {
        internal readonly object Session = new();
        internal readonly List<string> Events = [];
        internal bool Available = true;
        internal void Check() { if (!Available) throw new ObjectDisposedException("session"); }
        internal SchemaOperationLeaseSet.Member Member(string id, Action? enter = null, Action? release = null, Action? check = null) =>
            new(Session, check ?? (() => { }), () => { Events.Add("enter" + id); enter?.Invoke(); return new Lease(() => { Events.Add("release" + id); release?.Invoke(); }); });
        internal SchemaOperationLeaseSet Open(params SchemaOperationLeaseSet.Member[] members) => SchemaOperationLeaseSet.Acquire(Session, [.. members], 8, Check);
    }
    /// <summary>Acquires in order and releases once in reverse, after invalidating access.</summary>
    [Fact] public void OrderedExactlyOnceCompletion()
    {
        var f = new Fixture(); SchemaOperationLeaseSet? set = null;
        set = f.Open(f.Member("1", release: () => Assert.Throws<ObjectDisposedException>(() => set!.Check())), f.Member("2"));
        set.Check(); set.Complete(); set.Complete(); Assert.Equal(new[]{"enter1","enter2","release2","release1"},f.Events);
    }
    /// <summary>Empty explicit membership is valid and still checks session lifetime.</summary>
    [Fact] public void EmptySetStillChecksSession()
    { var f=new Fixture();var set=f.Open();set.Check();f.Available=false;Assert.Throws<ObjectDisposedException>(set.Check);set.Complete();Assert.Empty(f.Events); }
    /// <summary>Invalid lists refuse before any acquisition callback.</summary>
    /// <param name="kind">Invalid membership shape.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public void InvalidMembershipDoesNotAcquire(int kind)
    {
        var f=new Fixture();var one=f.Member("1");ImmutableArray<SchemaOperationLeaseSet.Member> members=kind switch {0=>default,1=>[null!],2=>[one,one],3=>[new Fixture().Member("x")],_=>[one,f.Member("2")]};
        Assert.ThrowsAny<ArgumentException>(()=>SchemaOperationLeaseSet.Acquire(f.Session,members,kind==4?1:8,f.Check));Assert.Empty(f.Events);
    }
    /// <summary>Nonpositive bounds refuse before acquisition.</summary>
    [Fact] public void InvalidBoundDoesNotAcquire()
    {var f=new Fixture();Assert.Throws<ArgumentOutOfRangeException>(()=>SchemaOperationLeaseSet.Acquire(f.Session,[],0,f.Check));Assert.Empty(f.Events);}
    /// <summary>Only successfully acquired leases roll back.</summary>
    [Fact] public void PartialEntryRollsBack()
    {var f=new Fixture();var error=new ApplicationException("entry");Assert.Same(error,Assert.Throws<ApplicationException>(()=>f.Open(f.Member("1"),f.Member("2",enter:()=>throw error))));Assert.Equal(new[]{"enter1","enter2","release1"},f.Events);}
    /// <summary>Rollback preserves all exception objects in attempted release order.</summary>
    [Fact] public void RollbackRetainsAllFailures()
    {
        var f=new Fixture();var primary=new ApplicationException("entry");var a=new AggregateException(new Exception("nested"));var b=new Exception("b");
        var error=Assert.Throws<AggregateException>(()=>f.Open(f.Member("1",release:()=>throw b),f.Member("2",release:()=>throw a),f.Member("3",enter:()=>throw primary)));
        Assert.Equal(new Exception[]{primary,a,b},error.InnerExceptions);Assert.Equal(new[]{"enter1","enter2","enter3","release2","release1"},f.Events);
    }
    /// <summary>Cancellation before entry avoids all leases.</summary>
    [Fact] public void PreCanceledEntry()
    {var f=new Fixture();using var c=new CancellationTokenSource();c.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>SchemaOperationLeaseSet.Acquire(f.Session,[f.Member("1")],8,f.Check,c.Token));Assert.Empty(f.Events);}
    /// <summary>Cancellation after the final lease rolls it back before publication.</summary>
    [Fact] public void CancellationAfterAcquisition()
    {var f=new Fixture();using var c=new CancellationTokenSource();Assert.ThrowsAny<OperationCanceledException>(()=>SchemaOperationLeaseSet.Acquire(f.Session,[f.Member("1",enter:c.Cancel)],8,f.Check,c.Token));Assert.Equal(new[]{"enter1","release1"},f.Events);}
    /// <summary>Session closure at final entry recheck rolls back acquired leases.</summary>
    [Fact] public void CloseDuringEntry()
    {var f=new Fixture();Assert.Throws<ObjectDisposedException>(()=>f.Open(f.Member("1",enter:()=>f.Available=false)));Assert.Equal(new[]{"enter1","release1"},f.Events);}
    /// <summary>One revoked member blocks the complete set while unrelated sets survive.</summary>
    [Fact] public void RevocationDoesNotCompleteLeases()
    {
        var f=new Fixture();bool live=true;var a=f.Member("1",check:()=>{if(!live)throw new ObjectDisposedException("member");});var b=f.Member("2");var group=f.Open(a,b);var independent=f.Open(b);
        live=false;Assert.Throws<ObjectDisposedException>(group.Check);independent.Check();Assert.DoesNotContain(f.Events,x=>x.StartsWith("release"));group.Complete();independent.Complete();
    }
    /// <summary>A single cleanup failure is rethrown unchanged and never retried.</summary>
    [Fact] public void SingleCleanupFailure()
    {var f=new Fixture();var error=new ApplicationException("cleanup");var set=f.Open(f.Member("1",release:()=>throw error));Assert.Same(error,Assert.Throws<ApplicationException>(()=>set.Complete()));set.Complete();Assert.Single(f.Events,x=>x=="release1");}
    /// <summary>Multiple cleanup failures are ordered without flattening.</summary>
    [Fact] public void MultipleCleanupFailures()
    {var f=new Fixture();var a=new Exception("a");var b=new AggregateException(new Exception("b"));var set=f.Open(f.Member("1",release:()=>throw a),f.Member("2",release:()=>throw b));Assert.Equal(new Exception[]{b,a},Assert.Throws<AggregateException>(()=>set.Complete()).InnerExceptions);}
    /// <summary>Work failure precedes every cleanup failure.</summary>
    [Fact] public void WorkAndCleanupFailures()
    {var f=new Fixture();var primary=new OperationCanceledException();var a=new Exception("a");var b=new Exception("b");var set=f.Open(f.Member("1",release:()=>throw a),f.Member("2",release:()=>throw b));Assert.Equal(new Exception[]{primary,b,a},Assert.Throws<AggregateException>(()=>set.Complete(primary)).InnerExceptions);set.Complete();}
    /// <summary>Work failure without cleanup error preserves its identity.</summary>
    [Fact] public void WorkFailureAlone()
    {var f=new Fixture();var primary=new ApplicationException("work");var set=f.Open(f.Member("1"));Assert.Same(primary,Assert.Throws<ApplicationException>(()=>set.Complete(primary)));}
    /// <summary>Concurrent completion invalidates once and releases once.</summary>
    /// <returns>The completed concurrency check.</returns>
    [Fact] public async Task ConcurrentCompletion()
    {var f=new Fixture();int count=0;var set=f.Open(f.Member("1",release:()=>Interlocked.Increment(ref count)));await Task.WhenAll(Enumerable.Range(0,16).Select(_=>Task.Run(()=>set.Complete())));Assert.Equal(1,count);Assert.Throws<ObjectDisposedException>(set.Check);}
    /// <summary>Nested lease sets release independently.</summary>
    [Fact] public void NestedSets()
    {var f=new Fixture();var member=f.Member("1");var outer=f.Open(member);var inner=f.Open(member);inner.Complete();outer.Check();Assert.Single(f.Events,x=>x=="release1");outer.Complete();Assert.Equal(2,f.Events.Count(x=>x=="release1"));}
    /// <summary>A canceled observer does not complete the raw task's lease set.</summary>
    /// <returns>The raw callback completion.</returns>
    [Fact] public async Task CanceledWaitRetainsRawOwnership()
    {
        var f=new Fixture();var set=f.Open(f.Member("1"));var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task Raw(){try{entered.SetResult();await resume.Task;set.Check();}finally{set.Complete();}}
        var raw=Raw();await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));using var c=new CancellationTokenSource();c.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(c.Token));Assert.DoesNotContain("release1",f.Events);resume.SetResult();await raw.WaitAsync(TimeSpan.FromSeconds(10));Assert.Single(f.Events,x=>x=="release1");
    }

    /// <summary>Successful reports transfer the real leases without releasing them.</summary>
    [Fact] public void OutcomeTransfersSuccessfulLeases()
    {
        var f = new Fixture();
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session, [f.Member("1"), f.Member("2")], 8, f.Check);
        Assert.Null(outcome.AdmissionError); Assert.Empty(outcome.CleanupErrors);
        var leases = Assert.IsType<SchemaOperationLeaseSet>(outcome.Leases);
        Assert.Equal(new[] { "enter1", "enter2" }, f.Events);
        leases.Check(); leases.Complete(); leases.Complete();
        Assert.Equal(new[] { "enter1", "enter2", "release2", "release1" }, f.Events);
    }

    /// <summary>Admission and rollback errors retain identity, nesting, duplicates and order.</summary>
    [Fact] public void OutcomeKeepsOriginalErrorsSeparate()
    {
        var f = new Fixture(); var primary = new AggregateException(new Exception("entry"));
        var nested = new AggregateException(new Exception("cleanup"));
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session,
            [f.Member("1", release: () => throw nested), f.Member("2", release: () => throw nested),
             f.Member("3", enter: () => throw primary)], 8, f.Check);
        Assert.Null(outcome.Leases); Assert.Same(primary, outcome.AdmissionError);
        Assert.Equal(2, outcome.CleanupErrors.Length);
        Assert.All(outcome.CleanupErrors, error => Assert.Same(nested, error));
        Assert.Single(nested.InnerExceptions);
        Assert.Equal(new[] { "enter1", "enter2", "enter3", "release2", "release1" }, f.Events);
    }

    /// <summary>Invalid membership is reported before any acquisition.</summary>
    /// <param name="kind">Invalid membership shape.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public void OutcomeInvalidMembership(int kind)
    {
        var f = new Fixture(); var one = f.Member("1");
        ImmutableArray<SchemaOperationLeaseSet.Member> members = kind switch
        { 0 => default, 1 => [null!], 2 => [one, one], 3 => [new Fixture().Member("x")], _ => [one, f.Member("2")] };
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session, members, kind == 4 ? 1 : 8, f.Check);
        Assert.Null(outcome.Leases); Assert.IsAssignableFrom<ArgumentException>(outcome.AdmissionError);
        Assert.Empty(outcome.CleanupErrors); Assert.Empty(f.Events);
    }

    /// <summary>Pre-cancellation remains admission failure and acquires nothing.</summary>
    [Fact] public void OutcomePreCancellation()
    {
        var f = new Fixture(); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session, [f.Member("1")], 8, f.Check, cancellation.Token);
        var error = Assert.IsAssignableFrom<OperationCanceledException>(outcome.AdmissionError);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Null(outcome.Leases); Assert.Empty(outcome.CleanupErrors); Assert.Empty(f.Events);
    }

    /// <summary>Final cancellation reports rollback failure separately after release.</summary>
    [Fact] public void OutcomeFinalCancellationWithCleanupFailure()
    {
        var f = new Fixture(); using var cancellation = new CancellationTokenSource(); var cleanup = new Exception("cleanup");
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session,
            [f.Member("1", enter: cancellation.Cancel, release: () => throw cleanup)], 8, f.Check, cancellation.Token);
        Assert.IsAssignableFrom<OperationCanceledException>(outcome.AdmissionError);
        Assert.Null(outcome.Leases); Assert.Same(cleanup, Assert.Single(outcome.CleanupErrors));
        Assert.Equal(new[] { "enter1", "release1" }, f.Events);
    }

    /// <summary>A failed final lifetime recheck releases every acquired lease in reverse.</summary>
    [Fact] public void OutcomeFinalRevocationWithMultipleCleanupFailures()
    {
        var f = new Fixture(); var first = new Exception("first"); var second = new Exception("second");
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session,
            [f.Member("1", release: () => throw first), f.Member("2", enter: () => f.Available = false, release: () => throw second)], 8, f.Check);
        Assert.IsType<ObjectDisposedException>(outcome.AdmissionError); Assert.Null(outcome.Leases);
        Assert.Equal(new[] { second, first }, outcome.CleanupErrors);
        Assert.Equal(new[] { "enter1", "enter2", "release2", "release1" }, f.Events);
    }

    /// <summary>A null lease refuses and rolls back only the prior successful lease.</summary>
    [Fact] public void OutcomeNullLeaseRollsBackPriorEntry()
    {
        var f = new Fixture(); var invalid = new SchemaOperationLeaseSet.Member(f.Session, () => { }, () => null!);
        var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session, [f.Member("1"), invalid], 8, f.Check);
        Assert.IsType<InvalidOperationException>(outcome.AdmissionError); Assert.Null(outcome.Leases);
        Assert.Empty(outcome.CleanupErrors); Assert.Equal(new[] { "enter1", "release1" }, f.Events);
    }

    /// <summary>Empty membership succeeds while still retaining session revocation checks.</summary>
    [Fact] public void OutcomeEmptyMembershipChecksSession()
    {
        var f = new Fixture(); var outcome = SchemaOperationLeaseSet.AcquireOutcome(f.Session, [], 8, f.Check);
        Assert.Null(outcome.AdmissionError); Assert.Empty(outcome.CleanupErrors);
        var leases = Assert.IsType<SchemaOperationLeaseSet>(outcome.Leases);
        f.Available = false; Assert.Throws<ObjectDisposedException>(leases.Check); leases.Complete(); Assert.Empty(f.Events);
    }
}
