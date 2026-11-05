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
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; private typed transport controls (1.55.0)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Fixture=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Fixture;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using Xunit;
using Xunit.Abstractions;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies private scalar attachment independently from public schema or VM activation.</summary>
/// <param name="output">Records scoped private transport validation costs.</param>
public sealed class QueryTypedEvidenceTests(ITestOutputHelper output)
{
    private static QueryRegisteredTypedContainer Read(Fixture f,QuerySourceSession.Operation op,ISchemaTypedValue c,bool original=true)
        =>op.ReadRegisteredTypedContainer(f.Snapshot,f.Issuer,f.Input,original?f.Origin:null,c,8,8,2048,2048);
    private static QueryRegisteredTypedContainer Validate(Fixture f,QuerySourceSession.Operation op,XdmValue v,int atoms=8,int digits=2048,int scale=2048)
        =>op.ValidateTypedTransport(f.Snapshot,v,atoms,digits,scale);
    /// <summary>Primitive and exact representations survive copies, object boxing and sequence storage.</summary>
    /// <param name="kind">Payload fixture selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)]
    public void RepresentationAndCopies(int kind)
    {
        XdmValue plain=kind switch{0=>XdmValue.FromInteger(7),1=>XdmValue.FromBoolean(true),2=>XdmValue.FromDouble(-0d),3=>XdmValue.FromFloat(float.NaN),4=>XdmValue.FromString("owned"),5=>XdmValue.FromDecimal(1.2300m),6=>XdmValue.FromInteger(BigInteger.Pow(10,1000)),_=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048))};
        using var f=new Fixture();var c=f.Value();c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        var marked=f.Run(op=>Read(f,op,c).GetTransport(0));Assert.NotNull(XdmValue.SchemaTransportMarker(marked));Assert.True(marked.SchemaTransportMatches(plain));
        object boxed=marked;var copy=(XdmValue)boxed;var seq=XdmSequence.Singleton(copy);
        f.Run(op=>{var cursor=seq.GetEnumerator();Assert.True(cursor.MoveNext());var stored=cursor.Current;var view=Validate(f,op,stored);Assert.Same(f.Actual,view.GetAtomic(0).ActualType);Assert.Same(f.Origin,view.GetAtomic(0).OriginalAssessment);Assert.Same(XdmValue.SchemaTransportMarker(marked),XdmValue.SchemaTransportMarker(view.GetTransport(0)));Assert.True(view.GetAtomic(0).Value.SchemaTransportMatches(plain));return 0;});
    }
    /// <summary>Originless list members retain exact ordinal identities without fabricating an assessment.</summary>
    [Fact] public void OriginlessListAndWarmReuse()
    {
        using var f=new Fixture();var c=f.Value(3,true,false,false);var values=f.Run(op=>{var v=Read(f,op,c,false);return Enumerable.Range(0,3).Select(v.GetTransport).ToArray();});int reads=c.Reads.Count;c.Fail="governing";
        for(int i=0;i<10;i++)f.Run(op=>{for(int j=0;j<3;j++){var v=Validate(f,op,values[j]);Assert.True(v.IsList);Assert.Null(v.GetAtomic(j).OriginalAssessment);Assert.Same(XdmValue.SchemaTransportMarker(values[j]),v.GetIdentity(j));}return 0;});Assert.Equal(reads,c.Reads.Count);Assert.Equal(3,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Plain and fabricated markers cannot borrow registered ownership.</summary>
    [Fact] public void MissingAndForgedEvidenceRefuses()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var v=Read(f,op,c);Assert.Throws<ArgumentException>(()=>Validate(f,op,XdmValue.FromInteger(1)));Assert.Throws<ArgumentException>(()=>Validate(f,op,XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object())));
        var entry=(QueryTypedScalarCache.Entry)v.GetIdentity(0);var counterfeit=new QueryTypedScalarCache.Entry(entry.Group,0);Assert.Throws<ArgumentException>(()=>Validate(f,op,counterfeit.Transport));return 0;});
    }
    /// <summary>Copying a genuine marker onto a changed payload or annotation does not authenticate it.</summary>
    /// <param name="annotation">Whether only the descriptive annotation changes.</param>
    [Theory][InlineData(false)][InlineData(true)] public void TamperedRepresentationRefuses(bool annotation)
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var marked=Read(f,op,c).GetTransport(0);var altered=annotation?XdmValue.FromInteger(1,"xs:integer","local"):XdmValue.FromInteger(2);var forged=XdmValue.SchemaTransportAttach(altered,XdmValue.SchemaTransportMarker(marked)!);Assert.Throws<ArgumentException>(()=>Validate(f,op,forged));return 0;});Assert.Equal(1,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Equal decimal values with different stored scale cannot substitute for exact representation.</summary>
    [Fact] public void EqualDecimalDifferentRepresentationRefuses()
    {
        using var f=new Fixture();var c=f.Value();c.Items=[new(XdmValue.FromDecimal(1.2300m),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{var marked=Read(f,op,c).GetTransport(0);var changed=XdmValue.SchemaTransportAttach(XdmValue.FromDecimal(1.23m),XdmValue.SchemaTransportMarker(marked)!);Assert.Throws<ArgumentException>(()=>Validate(f,op,changed));return 0;});
    }
    /// <summary>Foreign sessions and nonmember snapshots refuse before provider callbacks.</summary>
    [Fact] public void ForeignAdmissionRefuses()
    {
        using var f=new Fixture();using var g=new Fixture();var c=f.Value();var marked=f.Run(op=>Read(f,op,c).GetTransport(0));f.Issuer.OnOwn=()=>throw new Exception("unexpected callback");
        g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ValidateTypedTransport(f.Snapshot,marked,8,2048,2048));Assert.Throws<ArgumentException>(()=>Validate(g,op,marked));return 0;});
    }
    /// <summary>Current membership, selected authority and type ownership are required on every use.</summary>
    /// <param name="change">Dependency to revoke.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void RevokedOwnershipRefuses(int change)
    {
        using var f=new Fixture();var c=f.Value();var marked=f.Run(op=>Read(f,op,c).GetTransport(0));int reads=c.Reads.Count;
        if(change==0)f.Issuer.Values.Remove(c);else if(change==1)f.Authority.Types.Remove(f.Actual);else f.Issuer.SelectedAuthority=new QueryTypedContainerTests.Authority();
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>Validate(f,op,marked));return 0;});Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Raw views expire; retained transport can be revalidated only through a new live raw operation.</summary>
    [Fact] public void RawExpiryAndSnapshotClose()
    {
        using var f=new Fixture();var c=f.Value();QueryRegisteredTypedContainer? escaped=null;var marked=f.Run(op=>{escaped=Read(f,op,c);return escaped.GetTransport(0);});Assert.Throws<ObjectDisposedException>(()=>escaped!.GetTransport(0));
        f.Run(op=>{Assert.NotNull(Validate(f,op,marked));return 0;});f.Snapshot.Dispose();Assert.Equal(0,f.Snapshot.TypedScalarCount);Assert.Throws<ObjectDisposedException>(()=>f.Run(op=>Validate(f,op,marked)));Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>Cancellation revokes validation without removing the already complete registered batch.</summary>
    [Fact] public void CancellationPreservesCommittedBatch()
    {
        using var f=new Fixture();var c=f.Value();var marked=f.Run(op=>Read(f,op,c).GetTransport(0));using var cancel=new CancellationTokenSource();
        f.Run(op=>{cancel.Cancel();Assert.Throws<OperationCanceledException>(()=>Validate(f,op,marked));return 0;},cancel.Token);Assert.Equal(1,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Warm validation applies current result and numeric limits without payload getters.</summary>
    [Fact] public void CurrentQuotasApply()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);c.Items[0]=new(XdmValue.FromInteger(BigInteger.Pow(10,1000)),new Provenance(c,f.Governing,f.Actual,f.Origin));var marked=f.Run(op=>Read(f,op,c).GetTransport(0));int reads=c.Reads.Count;
        f.Run(op=>{Assert.Throws<NotSupportedException>(()=>Validate(f,op,marked,atoms:1));Assert.ThrowsAny<Exception>(()=>Validate(f,op,marked,digits:10));return 0;});Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Staged but unregistered identities cannot pass admission; insertion rollback remains complete.</summary>
    [Fact] public void UncommittedEvidenceRefuses()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var tx=op.PrepareTypedRegistration(f.Snapshot,f.Issuer,f.Input,f.Origin,c,8,8,2048,2048);var data=f.Read(op,c).Capture();var group=new QueryTypedScalarCache.Group(f.Snapshot.RequireTypedScalars(),c,f.Origin,data.Governing,data.List,data.Cells);Assert.Throws<ArgumentException>(()=>Validate(f,op,group.Entries[0].Transport));Assert.Throws<InvalidOperationException>(()=>tx.Commit(1));Assert.Equal(0,f.Snapshot.TypedScalarCount);Assert.NotNull(XdmValue.SchemaTransportMarker(Read(f,op,c).GetTransport(0)));return 0;});
    }
    /// <summary>Private attachment does not authorize the existing VM capture path.</summary>
    [Fact] public void VmCaptureRemainsUnsupported()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var marked=Read(f,op,c).GetTransport(0);Assert.Throws<NotSupportedException>(()=>op.CaptureInputs(new EvaluationContext().WithVariable("v",marked),16,16));return 0;});
    }

    /// <summary>Concurrent actual raw operations return the same committed transport marker.</summary>
    /// <returns>Completion of the staged raw operation.</returns>
    [Fact] public async Task ConcurrentWinnerReusesTransport()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);var staged=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);XdmValue first=default,second=default;
        var pending=f.Session.RunAsync(f.Session.Environment,[f.Snapshot],async op=>{var tx=op.PrepareTypedRegistration(f.Snapshot,f.Issuer,f.Input,f.Origin,c,8,8,2048,2048);staged.SetResult(true);await release.Task.WaitAsync(TimeSpan.FromSeconds(10));first=tx.Commit().GetTransport(0);return 0;});
        await staged.Task.WaitAsync(TimeSpan.FromSeconds(10));second=f.Run(op=>Read(f,op,c).GetTransport(0));release.SetResult(true);await pending;Assert.Same(XdmValue.SchemaTransportMarker(first),XdmValue.SchemaTransportMarker(second));Assert.Equal(2,f.Snapshot.TypedScalarCount);
    }
    /// <summary>Close during marker membership validation revokes delivery while the raw owner is retained.</summary>
    /// <returns>Completion of source drain after raw release.</returns>
    [Fact] public async Task CloseDuringValidation()
    {
        using var f=new Fixture();var c=f.Value();var marked=f.Run(op=>Read(f,op,c).GetTransport(0));
        f.Run(op=>{f.Issuer.OnOwn=()=>{f.Session.Dispose();Assert.Equal(0,f.Input.Releases);};Assert.Throws<ObjectDisposedException>(()=>Validate(f,op,marked));Assert.Equal(0,f.Snapshot.TypedScalarCount);return 0;});Assert.Equal(1,f.Input.Releases);await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
    }
    /// <summary>Current-operation validation has bounded local allocation and never replays provider payload getters.</summary>
    [Fact] public void ValidationAllocationObservation()
    {
        using var f=new Fixture();var c=f.Value(3,true,false);var marked=f.Run(op=>Read(f,op,c).GetTransport(1));int reads=c.Reads.Count;
        f.Run(op=>{for(int i=0;i<100;i++)Validate(f,op,marked);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Validate(f,op,marked);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Typed evidence warm-validation: {bytes} bytes/read");Assert.InRange(bytes,1,32768);return 0;});Assert.Equal(reads,c.Reads.Count);
    }
}
