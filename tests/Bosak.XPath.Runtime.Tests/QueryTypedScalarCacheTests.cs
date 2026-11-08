// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies transactional typed scalar accounting, reuse, races and raw-lifetime guards.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; transactional scalar controls (1.54.0)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Numerics;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Fixture=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Fixture;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises internal scalar transactions inside real source-session raw operations.</summary>
/// <param name="output">Records scoped registration/read allocations.</param>
public sealed class QueryTypedScalarCacheTests(ITestOutputHelper output)
{
    private static QueryTypedScalarRegistration Prepare(Fixture f,QuerySourceSession.Operation op,ISchemaTypedValue value,int limit=8,int atoms=8,int digits=128,int scale=128,bool original=true)
        =>op.PrepareTypedRegistration(f.Snapshot,f.Issuer,f.Input,original?f.Origin:null,value,limit,atoms,digits,scale);
    private static QueryRegisteredTypedContainer Read(Fixture f,QuerySourceSession.Operation op,ISchemaTypedValue value,int limit=8,int atoms=8,int digits=128,int scale=128,bool original=true)
        =>Prepare(f,op,value,limit,atoms,digits,scale,original).Commit();
    /// <summary>Zero/one/many atomic cells retain list shape and consume exactly one slot per cell.</summary>
    /// <param name="count">Atomic count.</param>
    /// <param name="list">Whether governing type is a list.</param>
    [Theory][InlineData(0,false)][InlineData(0,true)][InlineData(1,false)][InlineData(1,true)][InlineData(3,true)]
    public void ScalarAccountingAndShape(int count,bool list)
    {
        using var f=new Fixture();var c=f.Value(count,list,count==1);
        f.Run(op=>{var v=Read(f,op,c);Assert.Equal(count,v.Count);Assert.Equal(list,v.IsList);Assert.Same(f.Governing,v.GoverningType);
            for(int i=0;i<count;i++){Assert.Equal(i+1,v.GetAtomic(i).Value.IntegerValue);Assert.Same(f.Actual,v.GetAtomic(i).ActualType);Assert.Same(f.Origin,v.GetAtomic(i).OriginalAssessment);}return 0;});
        Assert.Equal(count,f.Snapshot.TypedScalarCount);Assert.Equal(0,f.Input.Roots);
    }
    /// <summary>Warm raw operations reuse stable identities without re-reading container/provenance getters.</summary>
    [Fact] public void WarmReuseDoesNotReplayGetters()
    {
        using var f=new Fixture();var c=f.Value(3,true,false);object[] identities=[];
        f.Run(op=>{var v=Read(f,op,c,limit:3);identities=Enumerable.Range(0,3).Select(v.GetIdentity).ToArray();return 0;});int reads=c.Reads.Count;
        c.Fail="governing";
        for(int run=0;run<20;run++)f.Run(op=>{var v=Read(f,op,c,limit:3);for(int i=0;i<3;i++)Assert.Same(identities[i],v.GetIdentity(i));return 0;});
        Assert.Equal(reads,c.Reads.Count);Assert.Equal(3,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Empty results create no resident record even across many distinct containers.</summary>
    [Fact] public void EmptyResultsDoNotFillCache()
    {
        using var f=new Fixture();for(int i=0;i<1000;i++){var c=f.Value(0,i%2==0,false);f.Run(op=>{Assert.Equal(0,Read(f,op,c,limit:1).Count);return 0;});f.Issuer.Values.Remove(c);}
        Assert.Equal(0,f.Snapshot.TypedScalarCount);var scalar=f.Value();f.Run(op=>{Assert.Equal(1,Read(f,op,scalar,limit:1).Count);return 0;});Assert.Equal(1,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Capacity refusal changes no prior entry and does not publish part of a multi-cell result.</summary>
    [Fact] public void WholeBatchCapacityRefusal()
    {
        using var f=new Fixture();var a=f.Value();var b=f.Value(2,true,false);object? first=null;
        f.Run(op=>{first=Read(f,op,a,limit:2).GetIdentity(0);return 0;});
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>Read(f,op,b,limit:2)));Assert.Equal(1,f.Snapshot.TypedScalarCount);
        var c=f.Value();f.Run(op=>{Assert.Same(first,Read(f,op,a,limit:2).GetIdentity(0));Read(f,op,c,limit:2);return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Fixed allocation/insertion faults roll back new cells while preserving a prior winner and retry capacity.</summary>
    /// <param name="point">Zero for pre-insertion fault or insertion ordinal.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void FaultRollbackAndRetry(int point)
    {
        using var f=new Fixture();var old=f.Value();var c=f.Value(3,true,false);object? identity=null;
        f.Run(op=>{identity=Read(f,op,old,limit:4).GetIdentity(0);var tx=Prepare(f,op,c,limit:4);
            Assert.Throws<InvalidOperationException>(()=>tx.Commit(point));Assert.Equal(1,f.Snapshot.TypedScalarCount);
            var v=tx.Commit();Assert.Equal(3,v.Count);Assert.Same(identity,Read(f,op,old,limit:4).GetIdentity(0));return 0;});Assert.Equal(4,f.Snapshot.TypedScalarCount);
    }
    private sealed class EqualClaim(ISchemaTypedValue inner):ISchemaTypedValue
    {
        /// <inheritdoc/>
        public override bool Equals(object? other)=>throw new Exception("caller equality");
        /// <inheritdoc/>
        public override int GetHashCode()=>throw new Exception("caller hash");
        ISchemaComponentHandle ISchemaTypedValue.GoverningType=>inner.GoverningType;
        bool ISchemaTypedValue.IsList=>inner.IsList;
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems=>inner.AtomicItems;
        bool ISchemaTypedValue.TryGetSingleAtomic(out SchemaAtomicItem item)=>inner.TryGetSingleAtomic(out item);
    }
    /// <summary>Caller equality/hash code never participates in cache identity, including equal-valued wrappers.</summary>
    [Fact] public void ReferenceKeysIgnoreCallerEquality()
    {
        using var f=new Fixture();var c=f.Value();var a=new EqualClaim(c);var b=new EqualClaim(c);f.Issuer.Values.Add(a);f.Issuer.Values.Add(b);
        f.Run(op=>{var first=Read(f,op,a).GetIdentity(0);Assert.NotSame(first,Read(f,op,b).GetIdentity(0));Assert.Same(first,Read(f,op,a).GetIdentity(0));return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Warm entries still enforce raw, issuer/type and per-call atom/numeric limits.</summary>
    [Fact] public void WarmValidationAndLimits()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);f.Run(op=>{Read(f,op,c);return 0;});int reads=c.Reads.Count;
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>Read(f,op,c,atoms:1)));Assert.Equal(reads,c.Reads.Count);
        f.Authority.Types.Remove(f.Actual);Assert.Throws<ArgumentException>(()=>f.Run(op=>Read(f,op,c)));Assert.Equal(reads,c.Reads.Count);f.Authority.Types.Add(f.Actual);
        f.Issuer.Values.Remove(c);Assert.Throws<ArgumentException>(()=>f.Run(op=>Read(f,op,c)));Assert.Equal(reads,c.Reads.Count);f.Issuer.Values.Add(c);
        Assert.Throws<ArgumentException>(()=>f.Run(op=>Read(f,op,c,limit:9)));Assert.Equal(reads,c.Reads.Count);
        Assert.Throws<ArgumentOutOfRangeException>(()=>f.Run(op=>Read(f,op,c,limit:0)));
    }
    /// <summary>Warm cached wide payloads cannot bypass a smaller later numeric admission ceiling.</summary>
    [Fact] public void WideWarmValuesRemainBounded()
    {
        using var f=new Fixture();var c=f.Value();var n=BigInteger.Pow(10,1000)+7;c.Items=[new(XdmValue.FromInteger(n),c.Items[0].TypeEvidence)];
        f.Run(op=>{Assert.Equal(n,Read(f,op,c,digits:1010).GetAtomic(0).Value.BigIntegerValue);return 0;});int reads=c.Reads.Count;
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>Read(f,op,c,digits:8)));Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Provider revalidation runs outside the registry lock, so a concurrent count probe can complete.</summary>
    [Fact] public void NoProviderCallbacksUnderRegistryLock()
    {
        using var f=new Fixture();var c=f.Value();f.Issuer.OnOwn=()=>{var probe=Task.Run(()=>f.Snapshot.TypedScalarCount);Assert.True(probe.Wait(TimeSpan.FromSeconds(3)));};
        f.Run(op=>{Read(f,op,c);Read(f,op,c);return 0;});f.Issuer.OnOwn=null;
    }
    /// <summary>Two raw operations staging the same container reuse the complete first winner.</summary>
    /// <returns>Completion of both real raw callbacks.</returns>
    [Fact] public async Task SameKeyInterleavingReusesWinner()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);var staged=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);object? first=null,second=null;
        var a=f.Session.RunAsync(f.Session.Environment,[f.Snapshot],async op=>{var tx=Prepare(f,op,c,limit:2);staged.SetResult(true);await release.Task.WaitAsync(TimeSpan.FromSeconds(10));first=tx.Commit().GetIdentity(0);return 0;});
        await staged.Task.WaitAsync(TimeSpan.FromSeconds(10));f.Run(op=>{second=Read(f,op,c,limit:2).GetIdentity(0);return 0;});release.SetResult(true);await a;
        Assert.Same(first,second);Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Distinct interleaved batches recheck capacity at commit without damaging the completed winner.</summary>
    /// <returns>Completion of the losing raw callback.</returns>
    [Fact] public async Task CapacityRacePreservesWinner()
    {
        using var f=new Fixture();var a=f.Value(2,true,false);var b=f.Value(2,true,false);var staged=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var loser=f.Session.RunAsync(f.Session.Environment,[f.Snapshot],async op=>{var tx=Prepare(f,op,a,limit:3);staged.SetResult(true);await release.Task.WaitAsync(TimeSpan.FromSeconds(10));Assert.Throws<NotSupportedException>(()=>tx.Commit());return 0;});
        await staged.Task.WaitAsync(TimeSpan.FromSeconds(10));object? winner=null;f.Run(op=>{winner=Read(f,op,b,limit:3).GetIdentity(0);return 0;});release.SetResult(true);await loser;
        f.Run(op=>{Assert.Same(winner,Read(f,op,b,limit:3).GetIdentity(0));return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Closing after staging clears the registry and retains actual source release until raw completion.</summary>
    /// <returns>The completed raw callback and source drain.</returns>
    [Fact] public async Task CloseBetweenStageAndCommit()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);var staged=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=f.Session.RunAsync(f.Session.Environment,[f.Snapshot],async op=>{var tx=Prepare(f,op,c);staged.SetResult(true);await release.Task.WaitAsync(TimeSpan.FromSeconds(10));Assert.Throws<ObjectDisposedException>(()=>tx.Commit());Assert.Equal(0,f.Input.Releases);return 0;});
        await staged.Task.WaitAsync(TimeSpan.FromSeconds(10));f.Session.Dispose();Assert.Equal(0,f.Snapshot.TypedScalarCount);Assert.Equal(0,f.Input.Releases);release.SetResult(true);await raw;await f.Session.DrainAsync();Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>Cancellation before commit adds no entries; cancellation observed after commit retains a whole reusable batch.</summary>
    /// <param name="after">Whether cancellation occurs only during postcommit revalidation.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void CancellationBoundary(bool after)
    {
        using var f=new Fixture();var c=f.Value(2,true,false);using var cancel=new CancellationTokenSource();
        Assert.ThrowsAny<OperationCanceledException>(()=>f.Run(op=>{var tx=Prepare(f,op,c);if(!after)cancel.Cancel();else f.Issuer.OnOwn=()=>{if(f.Snapshot.TypedScalarCount==2)cancel.Cancel();};return tx.Commit();},cancel.Token));
        f.Issuer.OnOwn=null;Assert.Equal(after?2:0,f.Snapshot.TypedScalarCount);f.Run(op=>{Assert.Equal(2,Read(f,op,c).Count);return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Source close from postcommit validation clears all entries without releasing an active owner early.</summary>
    [Fact] public void CloseDuringDeliveryClearsBatch()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);f.Issuer.OnOwn=()=>{if(f.Snapshot.TypedScalarCount==2){f.Session.Dispose();Assert.Equal(0,f.Input.Releases);}};
        Assert.Throws<ObjectDisposedException>(()=>f.Run(op=>Read(f,op,c)));f.Issuer.OnOwn=null;Assert.Equal(0,f.Snapshot.TypedScalarCount);Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>Raw result views expire after completion while a new operation can reuse the same resident identities.</summary>
    [Fact] public void ViewsAreRawBoundButEntriesAreNot()
    {
        using var f=new Fixture();var c=f.Value();QueryRegisteredTypedContainer? old=null;QueryRegisteredTypedContainer.AtomicView atom=default;object? identity=null;
        f.Run(op=>{old=Read(f,op,c);atom=old.GetAtomic(0);identity=old.GetIdentity(0);return 0;});
        Assert.Throws<ObjectDisposedException>(()=>old!.Count);Assert.Throws<ObjectDisposedException>(()=>atom.Value);Assert.Throws<InvalidOperationException>(()=>default(QueryRegisteredTypedContainer.AtomicView).Value);
        f.Run(op=>{Assert.Same(identity,Read(f,op,c).GetIdentity(0));return 0;});
    }
    /// <summary>Null-origin and original-source containers retain separate evidence and do not manufacture an assessment.</summary>
    [Fact] public void OriginlessAndOriginalRemainDistinct()
    {
        using var f=new Fixture();var original=f.Value();var constructed=f.Value(original:false);
        f.Run(op=>{var a=Read(f,op,original);var b=Read(f,op,constructed,original:false);Assert.NotSame(a.GetIdentity(0),b.GetIdentity(0));Assert.Same(f.Origin,a.GetAtomic(0).OriginalAssessment);Assert.Null(b.GetAtomic(0).OriginalAssessment);return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Shared owned containers and original assessments still acquire distinct identities in distinct snapshot scopes.</summary>
    /// <param name="original">Whether original provenance is retained.</param>
    [Theory][InlineData(true)][InlineData(false)]
    public void SnapshotScopesStaySeparate(bool original)
    {
        using var a=new Fixture();using var b=new Fixture();var c=a.Value(original:original);
        var issuer=new QueryTypedContainerTests.Issuer(b.Session.Environment,b.Input,a.Authority,a.Origin);issuer.Values.Add(c);object? first=null,second=null;
        a.Run(op=>{first=Read(a,op,c,original:original).GetIdentity(0);return 0;});
        b.Run(op=>{second=op.ReadRegisteredTypedContainer(b.Snapshot,issuer,b.Input,original?a.Origin:null,c,8,8,128,128).GetIdentity(0);return 0;});
        Assert.NotSame(first,second);Assert.Equal(1,a.Snapshot.TypedScalarCount);Assert.Equal(1,b.Snapshot.TypedScalarCount);
    }
    /// <summary>Callback-free commit state checks still reject a foreign raw operation before returning a winner.</summary>
    [Fact] public void ManagedCommitRejectsForeignOperation()
    {
        using var a=new Fixture();using var b=new Fixture();var c=a.Value();a.Run(op=>{Read(a,op,c);return 0;});
        var cache=a.Snapshot.GetTypedScalars(a.Issuer,a.Input,a.Authority,8);var group=cache.Find(c,a.Origin)!;
        b.Run(op=>{Assert.Throws<ArgumentException>(()=>cache.Install(op,group,-1));return 0;});Assert.Equal(1,a.Snapshot.TypedScalarCount);
    }
    private sealed class LegacyAdvisor(SchemaQueryEnvironment environment,object origin):IQuerySchemaAdvisor
    {
        SchemaQueryEnvironment IQuerySchemaAdvisor.Environment=>environment;
        void IQuerySchemaAdvisor.ThrowIfUnavailable(){ }
        bool IQuerySchemaAdvisor.OwnsNodeFacts(object facts)=>false;
        bool IQuerySchemaAdvisor.OwnsNode(IXdmNode node,object facts,IQuerySourceInput input,object original)=>false;
        bool IQuerySchemaAdvisor.OwnsTypedValue(ISchemaIssuedScalar value,IQuerySourceInput input,object original)=>false;
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value)=>false;
        bool ISchemaEvidenceIssuer.OwnsType(object type)=>true;
        bool ISchemaEvidenceIssuer.OwnsAssessment(object value)=>ReferenceEquals(origin,value);
        bool ISchemaEvidenceIssuer.IsSameType(object a,object b)=>ReferenceEquals(a,b);
    }
    /// <summary>A legacy scalar association cannot be combined with a second independent registry ceiling.</summary>
    [Fact] public void LegacyAssociationCannotDoubleAccount()
    {
        using var f=new Fixture();var c=f.Value();var advisor=new LegacyAdvisor(f.Session.Environment,f.Origin);
        f.Snapshot.Schema=new QuerySchemaAssociation(f.Session.Environment,f.Input,advisor,f.Origin,8,f.Input.Check,default);
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>Read(f,op,c)));Assert.Empty(c.Reads);Assert.Equal(0,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Reverse-order legacy binding refuses after transactional registration without altering resident identities.</summary>
    [Fact] public void RegistryPreventsLaterLegacyAssociation()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{Read(f,op,c);return 0;});
        using var association=new QuerySchemaAssociation(f.Session.Environment,f.Input,new LegacyAdvisor(f.Session.Environment,f.Origin),f.Origin,8,f.Input.Check,default);
        Assert.Throws<NotSupportedException>(()=>f.Snapshot.Schema=association);Assert.Null(f.Snapshot.Schema);Assert.Equal(1,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Racing legacy and transactional route selection can choose only one registry for the snapshot.</summary>
    /// <returns>Completion of both independent selection attempts.</returns>
    [Fact] public async Task ScalarRouteSelectionIsAtomic()
    {
        using var f=new Fixture();var c=f.Value();using var association=new QuerySchemaAssociation(f.Session.Environment,f.Input,new LegacyAdvisor(f.Session.Environment,f.Origin),f.Origin,8,f.Input.Check,default);
        using var start=new ManualResetEventSlim();
        var typed=Task.Run(()=>{Assert.True(start.Wait(TimeSpan.FromSeconds(10)));try{f.Run(op=>Read(f,op,c));return true;}catch(NotSupportedException){return false;}});
        var legacy=Task.Run(()=>{Assert.True(start.Wait(TimeSpan.FromSeconds(10)));try{f.Snapshot.Schema=association;return true;}catch(NotSupportedException){return false;}});
        start.Set();var results=await Task.WhenAll(typed,legacy).WaitAsync(TimeSpan.FromSeconds(10));Assert.NotEqual(results[0],results[1]);Assert.Equal(results[0]?1:0,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Mixed selected types and governing list shape survive both registration and warm reuse.</summary>
    [Fact] public void MixedMetadataSurvivesWarmReuse()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);var alternate=new QueryTypedContainerTests.Handle();f.Authority.Types.Add(alternate);
        c.Items[1]=new(XdmValue.FromString("selected"),new Provenance(c,f.Governing,alternate,f.Origin));
        for(int i=0;i<2;i++)f.Run(op=>{var v=Read(f,op,c);Assert.True(v.IsList);Assert.Same(f.Governing,v.GoverningType);Assert.Same(f.Actual,v.GetAtomic(0).ActualType);Assert.Same(alternate,v.GetAtomic(1).ActualType);Assert.Equal("selected",v.GetAtomic(1).Value.StringValue);return 0;});Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>A staged transaction cannot register after its originating raw callback has completed.</summary>
    [Fact] public void StagedTransactionExpiresWithRawOperation()
    {
        using var f=new Fixture();var c=f.Value();QueryTypedScalarRegistration? tx=null;f.Run(op=>{tx=Prepare(f,op,c);return 0;});
        Assert.Throws<ObjectDisposedException>(()=>tx!.Commit());Assert.Equal(0,f.Snapshot.TypedScalarCount);f.Run(op=>{Read(f,op,c);return 0;});
    }
    /// <summary>A real file handle survives logical close until the actual raw operation has left.</summary>
    /// <returns>The completed callback and owner drain verification.</returns>
    [Fact] public async Task RealOwnedFileSurvivesUntilRawCompletion()
    {
        using var f=new Fixture();var path=Path.Combine(Path.GetTempPath(),"Bosak.TypedCache-"+Guid.NewGuid().ToString("N")+".tmp");
        var file=new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.Read,4096,FileOptions.DeleteOnClose);f.Input.Resource=file;
        try
        {
            file.WriteByte(42);file.Position=0;var c=f.Value();var active=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var raw=f.Session.RunAsync(f.Session.Environment,[f.Snapshot],async op=>{Read(f,op,c);active.SetResult(true);await finish.Task.WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(42,file.ReadByte());Assert.False(file.SafeFileHandle.IsClosed);return 0;});
            await active.Task.WaitAsync(TimeSpan.FromSeconds(10));f.Session.Dispose();Assert.False(file.SafeFileHandle.IsClosed);Assert.Equal(0,f.Input.Releases);finish.SetResult(true);await raw;await f.Session.DrainAsync();Assert.True(file.SafeFileHandle.IsClosed);Assert.Equal(1,f.Input.Releases);
        }
        finally{file.Dispose();}
    }
    private sealed class BrokenStream:MemoryStream
    {
        /// <inheritdoc/>
        protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)throw new InvalidOperationException("owner-cleanup");}
    }
    /// <summary>Cache closure does not mask a raw failure or an actual owner-disposal failure.</summary>
    /// <returns>The observed faulted drain after release.</returns>
    [Fact] public async Task WorkAndOwnerCleanupFailuresRemainOrdered()
    {
        using var f=new Fixture();f.Input.Resource=new BrokenStream();var c=f.Value();
        var ex=Assert.Throws<AggregateException>(()=>f.Run<int>(op=>{Read(f,op,c);f.Session.Dispose();throw new InvalidOperationException("work");}));
        Assert.Equal(new[]{"work","owner-cleanup"},ex.InnerExceptions.Select(e=>e.Message));Assert.Equal(1,f.Input.Releases);Assert.Equal(0,f.Snapshot.TypedScalarCount);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Session.DrainAsync());
    }
    /// <summary>Cold registration, warm checked reuse and capacity refusal report local allocation without an owner budget.</summary>
    [Fact] public void RegistrationAllocationObservations()
    {
        using var f=new Fixture();var c=f.Value(3,true,false);f.Run(op=>{Read(f,op,c,limit:3);return 0;});int reads=c.Reads.Count;
        f.Run(op=>{for(int i=0;i<100;i++)Read(f,op,c,limit:3);long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Read(f,op,c,limit:3);long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;output.WriteLine($"Typed cache warm-list-three: {bytes} bytes/read");Assert.InRange(bytes,1,32768);return 0;});Assert.Equal(reads,c.Reads.Count);
        var refused=f.Value();refused.Record=false;f.Run(op=>{void Refuse(){try{Read(f,op,refused,limit:3);}catch(NotSupportedException){return;}throw new Exception("Expected capacity refusal");}
            for(int i=0;i<100;i++)Refuse();long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Refuse();long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;output.WriteLine($"Typed cache capacity-refusal: {bytes} bytes/read");Assert.InRange(bytes,1,32768);return 0;});
        using var cold=new Fixture();var values=Enumerable.Range(0,1100).Select(_=>cold.Value()).ToArray();foreach(var v in values)v.Record=false;
        cold.Run(op=>{for(int i=0;i<100;i++)Read(cold,op,values[i],limit:1100);long start=GC.GetAllocatedBytesForCurrentThread();for(int i=100;i<1100;i++)Read(cold,op,values[i],limit:1100);long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;output.WriteLine($"Typed cache cold-scalar: {bytes} bytes/read");Assert.InRange(bytes,1,32768);return 0;});Assert.Equal(1100,cold.Snapshot.TypedScalarCount);
    }
}
