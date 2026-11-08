// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies actual provider call tails, borrowed resources and raw operation drain.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises internal provider admission on actual raw operations without activating constructors.</summary>
public sealed class QueryProviderOperationScopeTests
{
    private static QuerySourceSession Session() => new(new(ImmutableDictionary<string,string>.Empty,"provider-tail"), _ => XdmValue.FromInteger(1), 4);
    private sealed class Provider : IQueryOperationLifetime
    {
        internal int Entries, Releases;
        internal bool Available = true, NullLease;
        internal Action? EnterAction, ReleaseAction;
        /// <summary>Acquires the fixture's borrowed admission.</summary>
        /// <param name="cancellationToken">Raw work cancellation.</param>
        /// <returns>The fixture lease.</returns>
        public IQueryOperationLease Enter(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Entries); EnterAction?.Invoke();
            return NullLease ? null! : new Lease(this);
        }
        private sealed class Lease(Provider owner) : IQueryOperationLease
        {
            /// <summary>Checks the fixture resource availability.</summary>
            /// <param name="cancellationToken">Raw work cancellation.</param>
            public void ThrowIfUnavailable(CancellationToken cancellationToken)
            { cancellationToken.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(!owner.Available, owner); }
            /// <summary>Releases the borrowed fixture admission.</summary>
            public void Dispose() { Interlocked.Increment(ref owner.Releases); owner.ReleaseAction?.Invoke(); }
        }
    }
    /// <summary>One real admission serves repeated calls and retained operation references cannot extend it.</summary>
    [Fact] public void LC01_LC15_OneAdmissionAndRetainedOperationRefuses()
    {
        using var session=Session(); var provider=new Provider(); QuerySourceSession.Operation? retained=null;
        session.Run(session.Environment,[],op=> { retained=op; Assert.Equal(1,op.CallProvider(provider,_=>1)); Assert.Equal(2,op.CallProvider(provider,_=>2)); return 0; });
        Assert.Equal(1,provider.Entries); Assert.Equal(1,provider.Releases);
        Assert.Throws<ObjectDisposedException>(()=>retained!.CallProvider(provider,_=>3));
    }
    /// <summary>Entry failure or missing lease leaves no phantom admission and raw drain settles.</summary>
    /// <param name="nullLease">Whether acquisition returns null rather than throws.</param>
    [Theory][InlineData(false)][InlineData(true)] public async Task LC03_EntryFailuresDrain(bool nullLease)
    {
        using var session=Session(); var failure=new ApplicationException("enter");
        var provider=new Provider {NullLease=nullLease,EnterAction=nullLease?null:()=>throw failure};
        var error=Record.Exception(()=>session.Run(session.Environment,[],op=>op.CallProvider(provider,_=>1)));
        if(nullLease) Assert.IsType<InvalidOperationException>(error); else Assert.Same(failure,error);
        Assert.Equal(0,provider.Releases); await session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
    }
    /// <summary>Actual admission/call tails outlive callback closure and canceled waits, with real file ownership.</summary>
    /// <param name="blockEntry">Whether acquisition itself remains on the stack.</param>
    /// <param name="closeSession">Whether session closure revokes the operation.</param>
    /// <param name="cancelRaw">Whether raw cancellation revokes the operation.</param>
    [Theory][InlineData(true,false,false)][InlineData(false,false,false)][InlineData(true,true,false)]
    [InlineData(false,true,false)][InlineData(true,false,true)][InlineData(false,false,true)]
    public async Task LC04_LC09_LC16_ActualCallTail(bool blockEntry,bool closeSession,bool cancelRaw)
    {
        using var session=Session(); using var cancellation=new CancellationTokenSource();
        using var entered=new ManualResetEventSlim(); using var resume=new ManualResetEventSlim();
        var callbackDone=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var path=Path.GetTempFileName();
        using var stream=new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        try
        {
            var provider=new Provider {ReleaseAction=stream.Dispose};
            void Block() { entered.Set(); Assert.True(resume.Wait(TimeSpan.FromSeconds(20))); Assert.True(stream.CanWrite); stream.WriteByte(42); }
            if(blockEntry) provider.EnterAction=Block;
            Task<int>? tail=null; QuerySourceSession.Operation? retained=null;
            var raw=session.RunAsync(session.Environment,[],async op=>
            {
                retained=op; tail=Task.Run(()=>op.CallProvider(provider,_=> { if(!blockEntry)Block(); return 7; }));
                Assert.True(entered.Wait(TimeSpan.FromSeconds(10))); await callbackDone.Task; return 9;
            },cancellation.Token);
            Assert.True(entered.IsSet);
            if(closeSession) session.Dispose();
            if(cancelRaw) cancellation.Cancel();
            callbackDone.SetResult();
            // Completion closes delivery synchronously before awaiting the real provider tail.
            var deadline=DateTime.UtcNow.AddSeconds(10);
            while(Record.Exception(retained!.Check) is null && DateTime.UtcNow<deadline) await Task.Delay(1);
            Assert.ThrowsAny<Exception>(retained!.Check);
            Assert.False(raw.IsCompleted); Assert.Equal(0,provider.Releases); Assert.True(stream.CanWrite);
            var drain=session.DrainAsync(); Assert.False(drain.IsCompleted);
            using(var waitCancellation=new CancellationTokenSource())
            { waitCancellation.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>drain.WaitAsync(waitCancellation.Token)); }
            Assert.False(drain.IsCompleted); Assert.True(stream.CanWrite);
            resume.Set();
            await Assert.ThrowsAnyAsync<Exception>(async()=>await tail!.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(9,await raw.WaitAsync(TimeSpan.FromSeconds(10)));
            await drain.WaitAsync(TimeSpan.FromSeconds(10)); Assert.Equal(1,provider.Releases); Assert.False(stream.CanWrite);
        }
        finally { resume.Set(); }
    }
    /// <summary>Revocation after provider work prevents delivery even when work returns false.</summary>
    [Fact] public void LC10_RevocationPrecedesFalseResult()
    {
        using var session=Session(); var provider=new Provider();
        Assert.Throws<ObjectDisposedException>(()=>session.Run(session.Environment,[],op=>op.CallProvider(provider,_=>{provider.Available=false;return false;})));
        Assert.Equal(1,provider.Releases);
    }
    /// <summary>Reentrancy and overlap refuse before touching another provider.</summary>
    [Fact] public async Task LC11_OverlapAndReentrancyRefuse()
    {
        using var session=Session(); var provider=new Provider();var other=new Provider();
        await session.RunAsync(session.Environment,[],async op=>
        {
            op.CallProvider(provider,_=> { Assert.Throws<InvalidOperationException>(()=>op.CallProvider(provider,_=>1));return 0; });
            using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
            var work=Task.Run(()=>op.CallProvider(provider,_=> {entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));return 1;}));
            try { Assert.True(entered.Wait(TimeSpan.FromSeconds(10))); Assert.Throws<InvalidOperationException>(()=>op.CallProvider(other,_=>1)); }
            finally {resume.Set();}
            await work; return 0;
        });
        Assert.Equal(1,provider.Entries);Assert.Equal(0,other.Entries);Assert.Equal(1,provider.Releases);
    }
    /// <summary>Independent raw operations acquire independent admissions.</summary>
    [Fact] public async Task LC12_SeparateOperations()
    {
        using var session=Session();var provider=new Provider();
        await session.RunAsync(session.Environment,[],async outer=>
        {outer.CallProvider(provider,_=>1);await session.RunAsync(session.Environment,[],inner=>Task.FromResult(inner.CallProvider(provider,_=>2)));Assert.Equal(1,provider.Releases);return 0;});
        Assert.Equal(2,provider.Entries);Assert.Equal(2,provider.Releases);
    }
    /// <summary>Primary failure remains first and cleanup-only failure is not wrapped.</summary>
    /// <param name="failWork">Whether user work also fails.</param>
    [Theory][InlineData(true)][InlineData(false)] public async Task LC13_LC14_FailureOrdering(bool failWork)
    {
        using var session=Session(); var primary=new ApplicationException("work");var cleanup=new ApplicationException("cleanup");
        var provider=new Provider {ReleaseAction=()=>throw cleanup};
        var error=Record.Exception(()=>session.Run(session.Environment,[],op=> {op.CallProvider(provider,_=>1);if(failWork)throw primary;return 0;}));
        if(failWork)Assert.Equal(new Exception[]{primary,cleanup},Assert.IsType<AggregateException>(error).InnerExceptions);else Assert.Same(cleanup,error);
        Assert.Equal(1,provider.Releases);session.Dispose();await session.DrainAsync();
    }
    /// <summary>Repeated completion uses one cleanup outcome and provider identity cannot change.</summary>
    [Fact] public async Task LC14_RepeatedCompletionAndForeignAdmission()
    {
        var provider=new Provider();var scope=new QueryProviderOperationScope(()=>{},default);
        scope.Call(provider,_=>1);Assert.Throws<ArgumentException>(()=>scope.Call(new Provider(),_=>1));
        int releases=0;var first=scope.CompleteAsync(()=>{releases++;return [];});var second=scope.CompleteAsync(()=>{releases++;return [];});
        Assert.Same(first,second);await first;Assert.Equal(1,releases);Assert.Equal(1,provider.Releases);
    }
    /// <summary>Acquisition reentrancy refuses before a second Enter and late revocation still releases the lease.</summary>
    [Fact] public void LC04_LC10_LC11_EntryReentrancyAndFailedRetention()
    {
        using var session=Session();var provider=new Provider();
        session.Run(session.Environment,[],op=>
        {
            provider.EnterAction=()=> { Assert.Throws<InvalidOperationException>(()=>op.CallProvider(provider,_=>1)); provider.Available=false; };
            Assert.Throws<ObjectDisposedException>(()=>op.CallProvider(provider,_=>1));return 0;
        });
        Assert.Equal(1,provider.Entries);Assert.Equal(1,provider.Releases);
    }
    /// <summary>Drain waits for actual cleanup, then cleanup failure reaches the raw task exactly once.</summary>
    [Fact] public async Task LC13_LC14_DrainIncludesBlockedCleanupFailure()
    {
        using var session=Session();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        var failure=new ApplicationException("late cleanup");
        var provider=new Provider {ReleaseAction=()=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));throw failure;}};
        var raw=Task.Run(()=>session.Run(session.Environment,[],op=>op.CallProvider(provider,_=>1)));
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.False(raw.IsCompleted);
            var drain=session.DrainAsync();Assert.False(drain.IsCompleted);session.Dispose();Assert.False(session.DrainAsync().IsCompleted);
        }
        finally {resume.Set();}
        Assert.Same(failure,await Assert.ThrowsAsync<ApplicationException>(async()=>await raw.WaitAsync(TimeSpan.FromSeconds(10))));
        await session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(1,provider.Releases);
    }
    /// <summary>Aggregate objects supplied by cleanup retain their original boundary and order.</summary>
    [Fact] public async Task LC13_OriginalAggregateBoundaries()
    {
        var provider=new Provider();var primary=new ApplicationException("work");
        var cleanup=new AggregateException(new ApplicationException("nested"));var raw=new ApplicationException("raw release");
        provider.ReleaseAction=()=>throw cleanup;var scope=new QueryProviderOperationScope(()=>{},default);
        scope.Call(provider,_=>1);
        var error=await Assert.ThrowsAsync<AggregateException>(()=>scope.CompleteAsync(()=>[raw],primary));
        Assert.Equal(new Exception[]{primary,cleanup,raw},error.InnerExceptions);
    }
}
