// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies neutral retained-source admission, revocation, transaction rollback and actual drain.
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
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies mechanical retention without creating schema authority or a public compatible snapshot.</summary>
public sealed class RetainedSourceAdmissionTests
{
    private static QuerySourceSession Session(int limit=1)=>new(new(ImmutableDictionary<string,string>.Empty,"retained-source"),_=>XdmValue.FromInteger(1),limit);
    private sealed class Source: IQuerySourceInput,IDisposable
    {
        internal readonly FileStream File=new(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        internal int Entries,Releases,RootReads;internal Action? CheckAction,ReleaseAction;
        IXdmNode IQuerySourceInput.Root{get{RootReads++;throw new Exception("root poison");}}
        void IQuerySourceInput.ThrowIfUnavailable(){CheckAction?.Invoke();}
        bool IQuerySourceInput.OwnsNode(IXdmNode node)=>false;
        internal IDisposable Enter(){Entries++;return new Lease(this);}
        private sealed class Lease(Source source):IDisposable
        {
            /// <inheritdoc/>
            public void Dispose(){source.Releases++;source.File.Dispose();source.ReleaseAction?.Invoke();}
        }
        /// <inheritdoc/>
        public void Dispose()=>File.Dispose();
    }
    /// <summary>MC08/MC15 target admission preserves exact input identity and no schema state; source close revokes before getters.</summary>
    [Fact] public async Task MC08_MC15_SourceCloseRevokesWithoutEarlyRelease()
    {
        using var source=Session();using var target=Session();using var input=new Source();
        var original=source.OpenSnapshot(source.Environment,input,input.Enter);var retained=target.OpenRetainedSourceSnapshot(target.Environment,original);
        Assert.Same(input,retained.Input);Assert.Same(original,retained.RetainedSource);Assert.Null(retained.Schema);Assert.Equal(0,retained.TypedScalarCount);Assert.Equal(1,input.Entries);
        source.Dispose();Assert.True(input.File.CanWrite);Assert.Equal(0,input.Releases);Assert.False(source.DrainAsync().IsCompleted);
        Assert.Throws<ObjectDisposedException>(()=>target.Run(target.Environment,[retained],op=>op.GetRoot(retained)));Assert.Equal(0,input.RootReads);
        retained.Dispose();retained.Dispose();await source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(1,input.Releases);Assert.False(input.File.CanWrite);
    }
    /// <summary>MC08 target close does not transfer/dispose the original host's still-live source ownership.</summary>
    [Fact] public void MC08_TargetClosePreservesOriginalOwner()
    {
        using var source=Session();using var target=Session();using var input=new Source();var original=source.OpenSnapshot(source.Environment,input,input.Enter);
        target.OpenRetainedSourceSnapshot(target.Environment,original);target.Dispose();Assert.True(input.File.CanWrite);Assert.Equal(0,input.Releases);original.CheckSource();
        source.Dispose();Assert.Equal(1,input.Releases);Assert.False(input.File.CanWrite);
    }
    /// <summary>MC08/MC12/MC15 raw target work retains the real original file through independent source and target close.</summary>
    [Fact] public async Task MC08_MC12_MC15_RawWorkRetainsOriginalFile()
    {
        using var source=Session();using var target=Session();using var input=new Source();var original=source.OpenSnapshot(source.Environment,input,input.Enter);var retained=target.OpenRetainedSourceSnapshot(target.Environment,original);
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=target.RunAsync(target.Environment,[retained],async op=>{entered.SetResult();await resume.Task;op.Check();return 1;});await entered.Task;
        source.Dispose();target.Dispose();Assert.True(input.File.CanWrite);input.File.WriteByte(1);Assert.Equal(0,input.Releases);Assert.False(source.DrainAsync().IsCompleted);Assert.False(target.DrainAsync().IsCompleted);
        resume.SetResult();await Assert.ThrowsAsync<ObjectDisposedException>(()=>raw);await source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));await target.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(1,input.Releases);Assert.False(input.File.CanWrite);
    }
    /// <summary>MC15 duplicates/quotas and non-transitive source rules refuse without extra owner acquisition.</summary>
    [Fact] public void MC15_MembershipAndQuota()
    {
        using var source=Session(2);using var target=Session();using var third=Session();using var input=new Source();using var other=new Source();
        var original=source.OpenSnapshot(source.Environment,input,input.Enter);var second=source.OpenSnapshot(source.Environment,other,other.Enter);
        Assert.Throws<NotSupportedException>(()=>source.OpenRetainedSourceSnapshot(source.Environment,original));
        var retained=target.OpenRetainedSourceSnapshot(target.Environment,original);
        Assert.Throws<ArgumentException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original));Assert.Throws<InvalidOperationException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,second));
        Assert.Throws<NotSupportedException>(()=>third.OpenRetainedSourceSnapshot(third.Environment,retained));
        retained.Dispose();var replacement=target.OpenRetainedSourceSnapshot(target.Environment,second);Assert.Same(other,replacement.Input);replacement.Dispose();Assert.Equal(1,input.Entries);Assert.Equal(1,other.Entries);Assert.Equal(0,input.Releases);Assert.Equal(0,other.Releases);
    }
    /// <summary>MC15 a blocked source input check already holds its real lifetime, and failed publication returns all obligations.</summary>
    /// <param name="closeTarget">Close target as well as the original source while the callback is blocked.</param>
    [Theory][InlineData(false)][InlineData(true)] public async Task MC15_OpeningCallbackRetainsActualResource(bool closeTarget)
    {
        using var source=Session();using var target=Session();using var input=new Source();var original=source.OpenSnapshot(source.Environment,input,input.Enter);
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        input.CheckAction=()=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));Assert.True(input.File.CanWrite);input.File.WriteByte(4);};
        var opening=Task.Run(()=>target.OpenRetainedSourceSnapshot(target.Environment,original));
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));source.Dispose();if(closeTarget)target.Dispose();
            Assert.True(input.File.CanWrite);Assert.Equal(0,input.Releases);Assert.False(source.DrainAsync().IsCompleted);Assert.False(target.DrainAsync().IsCompleted);
        }
        finally {resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await opening.WaitAsync(TimeSpan.FromSeconds(10)));await source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));await target.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(1,input.Entries);Assert.Equal(1,input.Releases);Assert.False(input.File.CanWrite);
        if(!closeTarget)
        {using var freshOwner=Session();using var fresh=new Source();var next=freshOwner.OpenSnapshot(freshOwner.Environment,fresh,fresh.Enter);var admitted=target.OpenRetainedSourceSnapshot(target.Environment,next);admitted.Dispose();}
    }
    /// <summary>MC15 cancellation/closure during post-retain availability releases the reservation and source obligation.</summary>
    /// <param name="cancel">Cancellation rather than source snapshot close.</param>
    [Theory][InlineData(false)][InlineData(true)] public void MC15_RollbackAndQuotaRefund(bool cancel)
    {
        using var source=Session(2);using var target=Session();using var input=new Source();using var cancellation=new CancellationTokenSource();var original=source.OpenSnapshot(source.Environment,input,input.Enter);
        input.CheckAction=()=>{if(cancel)cancellation.Cancel();else original.Dispose();Assert.True(input.File.CanWrite);};
        if(cancel)Assert.ThrowsAny<OperationCanceledException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original,cancellation.Token));else Assert.Throws<ObjectDisposedException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original));
        Assert.Equal(cancel?0:1,input.Releases);input.CheckAction=null;
        using var fresh=new Source();var next=source.OpenSnapshot(source.Environment,fresh,fresh.Enter);var replacement=target.OpenRetainedSourceSnapshot(target.Environment,next);replacement.Dispose();
        original.Dispose();Assert.Equal(1,input.Releases);Assert.Equal(1,input.Entries);
    }
    /// <summary>MC09/MC15 failed source retention cleanup preserves primary then original cleanup failure and no target slot.</summary>
    [Fact] public void MC09_MC15_FailedAdmissionCleanup()
    {
        using var source=Session(2);using var target=Session();using var input=new Source();var original=source.OpenSnapshot(source.Environment,input,input.Enter);var cleanup=new ApplicationException("original owner cleanup");
        input.ReleaseAction=()=>throw cleanup;input.CheckAction=()=>original.Dispose();
        var error=Assert.Throws<AggregateException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original));Assert.IsType<ObjectDisposedException>(error.InnerExceptions[0]);Assert.Same(cleanup,error.InnerExceptions[1]);Assert.Equal(1,input.Releases);
        using var fresh=new Source();var next=source.OpenSnapshot(source.Environment,fresh,fresh.Enter);target.OpenRetainedSourceSnapshot(target.Environment,next).Dispose();
    }
    /// <summary>MC15 invalid source/environment/cancellation refuses before source callbacks or new ownership.</summary>
    [Fact] public void MC15_EarlyValidation()
    {
        using var source=Session();using var target=Session();using var input=new Source();var original=source.OpenSnapshot(source.Environment,input,input.Enter);
        input.CheckAction=()=>throw new Exception("source callback poison");
        Assert.Throws<ArgumentNullException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,null!));
        Assert.Throws<ArgumentException>(()=>target.OpenRetainedSourceSnapshot(source.Environment,original));
        using var c=new CancellationTokenSource();c.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original,c.Token));
        target.Dispose();Assert.Throws<ObjectDisposedException>(()=>target.OpenRetainedSourceSnapshot(target.Environment,original));Assert.Equal(1,input.Entries);
    }
    private sealed class Advisor(FileStream file,string name,List<string> events):IQueryOperationLifetime
    {
        internal int Entries,Releases;
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){Entries++;return new Lease(this);}
        private void Release(){Releases++;file.Dispose();events.Add(name);}
        private sealed class Lease(Advisor owner):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token)=>token.ThrowIfCancellationRequested();
            /// <inheritdoc/>
            public void Dispose()=>owner.Release();
        }
    }
    /// <summary>MC06/MC12/MC15 actual paired calls compose with the retained source and release providers before original ownership.</summary>
    [Fact] public async Task MC06_MC12_MC15_PairAndRetainerCompose()
    {
        using var source=Session();using var target=Session();using var input=new Source();var events=new List<string>();input.ReleaseAction=()=>events.Add("original-owner");
        using var sourceFile=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        using var targetFile=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        var a=new Advisor(sourceFile,"source-advisor",events);var b=new Advisor(targetFile,"target-advisor",events);
        var original=source.OpenSnapshot(source.Environment,input,input.Enter);var retained=target.OpenRetainedSourceSnapshot(target.Environment,original);
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();var done=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);Task<int>? tail=null;
        var raw=target.RunAsync(target.Environment,[retained],async op=>
        {tail=Task.Run(()=>op.CallProviderPair(a,b,_=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));Assert.True(input.File.CanWrite);Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);return 1;}));Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));await done.Task;return 9;});
        try
        {
            source.Dispose();target.Dispose();done.SetResult();Assert.True(input.File.CanWrite);Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);
            Assert.False(source.DrainAsync().IsCompleted);Assert.False(target.DrainAsync().IsCompleted);Assert.Empty(events);
        }
        finally{resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await tail!.WaitAsync(TimeSpan.FromSeconds(10)));Assert.Equal(9,await raw.WaitAsync(TimeSpan.FromSeconds(10)));
        await source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));await target.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(new[]{"target-advisor","source-advisor","original-owner"},events);Assert.Equal(1,a.Releases);Assert.Equal(1,b.Releases);Assert.Equal(1,input.Releases);
        Assert.False(input.File.CanWrite);Assert.False(sourceFile.CanWrite);Assert.False(targetFile.CanWrite);
    }
}
