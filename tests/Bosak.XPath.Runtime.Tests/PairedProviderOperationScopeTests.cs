// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies bounded paired advisor admission, rollback, real resources and raw drain.
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

/// <summary>Exercises explicit paired membership on actual raw operations without activating compatibility.</summary>
public sealed class PairedProviderOperationScopeTests
{
    private static QuerySourceSession Session()=>new(new(ImmutableDictionary<string,string>.Empty,"pair-tail"),_=>XdmValue.FromInteger(1),4);
    private sealed class Provider(string name,List<string> events):IQueryOperationLifetime
    {
        internal int Entries,Releases;internal bool Available=true,NullLease;internal Action? EnterAction,ReleaseAction,CheckAction;
        /// <inheritdoc/>
        public IQueryOperationLease Enter(CancellationToken token)
        {Entries++;events.Add("enter-"+name);EnterAction?.Invoke();return NullLease?null!:new Lease(this);}
        private void Release(){Releases++;events.Add("release-"+name);ReleaseAction?.Invoke();}
        private sealed class Lease(Provider p):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){p.CheckAction?.Invoke();token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!p.Available,p);}
            /// <inheritdoc/>
            public void Dispose()=>p.Release();
        }
    }
    /// <summary>MC01/MC02 immutable pair membership acquires in order, deduplicates and releases in reverse.</summary>
    /// <param name="same">Whether both endpoint references are identical.</param>
    [Theory][InlineData(false)][InlineData(true)] public void MC01_MC02_OrderedPair(bool same)
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=same?a:new Provider("b",events);
        session.Run(session.Environment,[],op=>{Assert.Equal(1,op.CallProviderPair(a,b,_=>1));Assert.Equal(2,op.CallProviderPair(a,b,_=>2));return 0;});
        Assert.Equal(same?new[]{"enter-a","release-a"}:new[]{"enter-a","enter-b","release-b","release-a"},events);
        Assert.Equal(1,a.Entries);Assert.Equal(1,a.Releases);Assert.Equal(1,b.Entries);Assert.Equal(1,b.Releases);
    }
    /// <summary>MC03 failed acquisition rolls back before returning to user code, with no phantom retained lease.</summary>
    /// <param name="second">Failure in target rather than source.</param><param name="nullLease">Null result rather than exception.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public async Task MC03_AcquisitionRollback(bool second,bool nullLease)
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);var failure=new ApplicationException("enter");var failing=second?b:a;
        failing.NullLease=nullLease;if(!nullLease)failing.EnterAction=()=>throw failure;
        session.Run(session.Environment,[],op=>
        {var error=Record.Exception(()=>op.CallProviderPair(a,b,_=>1));if(nullLease)Assert.IsType<InvalidOperationException>(error);else Assert.Same(failure,error);Assert.Equal(second?1:0,a.Releases);Assert.Equal(0,b.Releases);return 0;});
        await session.DrainAsync();Assert.Equal(second?1:0,a.Releases);Assert.Equal(0,b.Releases);
    }
    /// <summary>MC03 failed paired admission cannot reopen within the raw operation; a new raw operation has fresh state.</summary>
    [Fact] public void MC03_FailedPairCannotBeReopened()
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events){NullLease=true};
        session.Run(session.Environment,[],op=>{Assert.Throws<InvalidOperationException>(()=>op.CallProviderPair(a,b,_=>1));Assert.Equal(1,a.Releases);b.NullLease=false;Assert.Throws<InvalidOperationException>(()=>op.CallProviderPair(a,b,_=>3));Assert.Equal(1,a.Entries);Assert.Equal(1,b.Entries);return 0;});
        session.Run(session.Environment,[],op=>op.CallProviderPair(a,b,_=>3));
        Assert.Equal(2,a.Entries);Assert.Equal(2,a.Releases);Assert.Equal(2,b.Entries);Assert.Equal(1,b.Releases);
    }
    /// <summary>MC04-MC08/MC12 actual entry and work tails retain separately owned source/target files through completion.</summary>
    /// <param name="phase">Actual provider callback to block.</param><param name="stop">Revocation mode.</param>
    [Theory][InlineData("source-enter","callback")][InlineData("target-enter","callback")][InlineData("source-call","callback")][InlineData("target-call","callback")]
    [InlineData("proof-call","callback")][InlineData("source-enter","cancel")][InlineData("target-enter","cancel")][InlineData("source-call","cancel")]
    [InlineData("target-enter","close")][InlineData("target-call","close")]
    public async Task MC04_MC05_MC06_MC07_MC08_MC12_ActualTail(string phase,string stop)
    {
        using var session=Session();using var cancel=new CancellationTokenSource();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        using var sourceFile=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        using var targetFile=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        var done=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var events=new List<string>();
        var a=new Provider("a",events){ReleaseAction=sourceFile.Dispose};var b=new Provider("b",events){ReleaseAction=targetFile.Dispose};
        void Block(){entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);sourceFile.WriteByte(1);targetFile.WriteByte(2);}
        if(phase=="source-enter")a.EnterAction=Block;if(phase=="target-enter")b.EnterAction=Block;
        Task<int>? tail=null;QuerySourceSession.Operation? retained=null;
        var raw=session.RunAsync(session.Environment,[],async op=>
        {retained=op;tail=Task.Run(()=>op.CallProviderPair(a,b,check=>{Block();check();return 7;}));Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));await done.Task;return 9;},cancel.Token);
        try
        {
            if(stop=="cancel")cancel.Cancel();if(stop=="close")session.Dispose();done.SetResult();
            var deadline=DateTime.UtcNow.AddSeconds(10);while(Record.Exception(retained!.Check) is null&&DateTime.UtcNow<deadline)await Task.Delay(1);
            Assert.ThrowsAny<Exception>(retained!.Check);Assert.False(raw.IsCompleted);Assert.Equal(0,a.Releases);Assert.Equal(0,b.Releases);
            var drain=session.DrainAsync();Assert.False(drain.IsCompleted);using var waitCancel=new CancellationTokenSource();waitCancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>drain.WaitAsync(waitCancel.Token));
            Assert.False(drain.IsCompleted);Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);
        }
        finally {resume.Set();}
        await Assert.ThrowsAnyAsync<Exception>(async()=>await tail!.WaitAsync(TimeSpan.FromSeconds(10)));Assert.Equal(9,await raw.WaitAsync(TimeSpan.FromSeconds(10)));await session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(1,a.Releases);Assert.False(sourceFile.CanWrite);
        Assert.Equal(phase=="source-enter"?0:1,b.Releases);Assert.Equal(phase=="source-enter",targetFile.CanWrite);
    }
    /// <summary>MC09 retention failure rolls back both entries and preserves every original cleanup object in reverse order.</summary>
    [Fact] public void MC09_RetentionFailureAndRollbackErrors()
    {
        using var session=Session();var events=new List<string>();var targetCleanup=new AggregateException(new ApplicationException("target cleanup"));var sourceCleanup=new ApplicationException("source cleanup");
        var a=new Provider("a",events){ReleaseAction=()=>throw sourceCleanup};var b=new Provider("b",events){ReleaseAction=()=>throw targetCleanup,EnterAction=()=>{}};
        b.EnterAction=()=>b.Available=false;
        session.Run(session.Environment,[],op=>{var error=Assert.Throws<AggregateException>(()=>op.CallProviderPair(a,b,_=>1));Assert.IsType<ObjectDisposedException>(error.InnerExceptions[0]);Assert.Same(targetCleanup,error.InnerExceptions[1]);Assert.Same(sourceCleanup,error.InnerExceptions[2]);Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);return 0;});
        Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);
    }
    /// <summary>MC09 work and cleanup failures preserve primary/target/source/raw ordering without flattening supplied aggregates.</summary>
    [Fact] public async Task MC09_WorkAndCleanupOrdering()
    {
        var events=new List<string>();var primary=new ApplicationException("work");var targetCleanup=new AggregateException(new ApplicationException("target"));var sourceCleanup=new ApplicationException("source");var rawCleanup=new ApplicationException("raw");
        var a=new Provider("a",events){ReleaseAction=()=>throw sourceCleanup};var b=new Provider("b",events){ReleaseAction=()=>throw targetCleanup};var scope=new QueryProviderOperationScope(()=>{},default);
        scope.CallPair(a,b,_=>1);var error=await Assert.ThrowsAsync<AggregateException>(()=>scope.CompleteAsync(()=>[rawCleanup],primary));Assert.Equal(new Exception[]{primary,targetCleanup,sourceCleanup,rawCleanup},error.InnerExceptions);
    }
    /// <summary>MC10 reentry from acquisition and work refuses before provider callbacks; overlap has the same gate.</summary>
    [Fact] public async Task MC10_ReentryAndOverlap()
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);
        await session.RunAsync(session.Environment,[],async op=>
        {
            a.EnterAction=()=>Assert.Throws<InvalidOperationException>(()=>op.CallProviderPair(a,b,_=>1));b.EnterAction=a.EnterAction;
            op.CallProviderPair(a,b,_=>{Assert.Throws<InvalidOperationException>(()=>op.CallProvider(a,_=>1));return 0;});
            using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();var tail=Task.Run(()=>op.CallProviderPair(a,b,_=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));return 0;}));
            try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.Throws<InvalidOperationException>(()=>op.CallProviderPair(a,b,_=>1));}finally{resume.Set();}
            await tail;return 0;
        });Assert.Equal(1,a.Entries);Assert.Equal(1,b.Entries);
    }
    /// <summary>MC11 independent raw operations each retain distinct real obligations over the same selected endpoints.</summary>
    [Fact] public async Task MC11_IndependentOperations()
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);
        await session.RunAsync(session.Environment,[],async outer=>{outer.CallProviderPair(a,b,_=>1);await session.RunAsync(session.Environment,[],inner=>Task.FromResult(inner.CallProviderPair(a,b,_=>2)));Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);return 0;});
        Assert.Equal(2,a.Entries);Assert.Equal(2,b.Entries);Assert.Equal(2,a.Releases);Assert.Equal(2,b.Releases);
    }
    /// <summary>MC13 retained raw operation references cannot extend or revive either admission.</summary>
    [Fact] public void MC13_RetainedOperationRefuses()
    {using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);QuerySourceSession.Operation? retained=null;session.Run(session.Environment,[],op=>{retained=op;return op.CallProviderPair(a,b,_=>1);});Assert.Throws<ObjectDisposedException>(()=>retained!.CallProviderPair(a,b,_=>1));Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);}
    /// <summary>MC14 completion retains raw drain through blocked target cleanup and attempts source cleanup after failure.</summary>
    [Fact] public async Task MC14_BlockedCleanupAndRepeatedCompletion()
    {
        using var session=Session();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();var events=new List<string>();var failure=new ApplicationException("target cleanup");
        var a=new Provider("a",events);var b=new Provider("b",events){ReleaseAction=()=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));throw failure;}};
        var raw=Task.Run(()=>session.Run(session.Environment,[],op=>op.CallProviderPair(a,b,_=>1)));
        try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.False(raw.IsCompleted);Assert.False(session.DrainAsync().IsCompleted);Assert.Equal(0,a.Releases);session.Dispose();session.Dispose();Assert.False(session.DrainAsync().IsCompleted);}finally{resume.Set();}
        Assert.Same(failure,await Assert.ThrowsAsync<ApplicationException>(async()=>await raw.WaitAsync(TimeSpan.FromSeconds(10))));await session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);
    }
    /// <summary>MC16 single versus paired mode and exact ordered pair membership never expand implicitly.</summary>
    /// <param name="paired">Start with explicit paired membership.</param>
    [Theory][InlineData(false)][InlineData(true)] public void MC16_ImmutableModeAndMembership(bool paired)
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);var c=new Provider("c",events);
        session.Run(session.Environment,[],op=>{if(paired){op.CallProviderPair(a,b,_=>1);Assert.Throws<ArgumentException>(()=>op.CallProviderPair(b,a,_=>1));Assert.Throws<ArgumentException>(()=>op.CallProviderPair(a,c,_=>1));Assert.Throws<ArgumentException>(()=>op.CallProvider(a,_=>1));}else{op.CallProvider(a,_=>1);Assert.Throws<ArgumentException>(()=>op.CallProvider(b,_=>1));Assert.Throws<ArgumentException>(()=>op.CallProviderPair(a,b,_=>1));Assert.Throws<ArgumentException>(()=>op.CallProviderPair(a,a,_=>1));}return 0;});Assert.Equal(0,c.Entries);Assert.Equal(paired?1:0,b.Entries);
    }
    /// <summary>MC10 callback-free validation rejects missing members/work before acquisition.</summary>
    [Fact] public void MC10_InvalidArgumentsDoNotEnter()
    {using var session=Session();var events=new List<string>();var a=new Provider("a",events);session.Run(session.Environment,[],op=>{Assert.Throws<ArgumentNullException>(()=>op.CallProviderPair<int>(a,null!,_=>1));Assert.Throws<ArgumentNullException>(()=>op.CallProviderPair<int>(null!,a,_=>1));Assert.Throws<ArgumentNullException>(()=>op.CallProviderPair<int>(a,a,null!));return 0;});Assert.Equal(0,a.Entries);}
    /// <summary>MC08 source callback closure stops the target lease callback before any further provider work.</summary>
    [Fact] public void MC08_GuardBetweenLeaseCallbacks()
    {
        using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);
        Assert.Throws<ObjectDisposedException>(()=>session.Run(session.Environment,[],op=>op.CallProviderPair(a,b,_=>{a.CheckAction=session.Dispose;b.CheckAction=()=>throw new Exception("late target check poison");return 0;})));
        Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);
    }
    /// <summary>MC08 revocation from either endpoint prevents false result delivery and cleans both admissions.</summary>
    /// <param name="source">Revoke source rather than target admission.</param>
    [Theory][InlineData(false)][InlineData(true)] public void MC08_EitherEndpointRevocation(bool source)
    {using var session=Session();var events=new List<string>();var a=new Provider("a",events);var b=new Provider("b",events);Assert.Throws<ObjectDisposedException>(()=>session.Run(session.Environment,[],op=>op.CallProviderPair(a,b,_=>{(source?a:b).Available=false;return false;})));Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);}
}
