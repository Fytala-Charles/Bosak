// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies private compatible cache residency and fresh operation authorization.
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
//                      | Charles Korthout | 0.2 | 06-11-2026     | Verify bounded public provenance observation and expiry (1.77.0). |
// ===========================================================================================================================================================
using System.Numerics;
using System.Reflection;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Pair=Bosak.XPath.Runtime.Tests.CompatibleAdmissionTests.Fixture;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies private payload reuse under fresh paired compatible operations.</summary>
public sealed class CompatibleCacheTests
{
    private static Container Value(Pair f,XdmValue? scalar=null,int count=1,bool list=false,bool fast=true)
    {
        var governing=new Handle();var actual=new Handle();f.SourceProvider.Authority.Types.UnionWith([governing,actual]);
        var c=new Container{Governing=governing,List=list,Fast=fast};
        c.Items=Enumerable.Range(0,count).Select(_=>new SchemaAtomicItem(scalar??XdmValue.FromInteger(7),new Provenance(c,governing,actual,f.SourceProvider.Origin))).ToArray();
        f.SourceProvider.TypedAllowed=true;f.SourceProvider.Values.Add(c);return c;
    }
    private static XdmValue Read(Pair f,CompatibleAdmissionPlan.Admission a,Container c)
        =>f.Target.RunCompatibleIdentityCandidate(a,op=>op.Read(c).Value);
    /// <summary>CI01 preserves legacy singleton-list registration on the original public route.</summary>
    [Fact] public void CI01_LegacySingletonList()
    {using var f=new Pair(identity:true);var c=Value(f,list:true);var result=f.Source.Run([f.Original],op=>op.ReadTypedValue(f.Original,c));Assert.Equal(SchemaResultKind.Supported,result.Kind);Assert.Equal(1,f.Original.Runtime.TypedScalarCount);}
    /// <summary>CI02/03/04/12 stable cache payload survives distinct current issuers without retaining them.</summary>
    [Fact] public void CI02_FreshIssuerStablePayload()
    {
        using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);CompatibleAdmissionPlan.TypedOperation? old=null;IQueryOperationBoundTypedIssuer? first=null;
        var value=f.Target.RunCompatibleIdentityCandidate(a,op=>{old=op;first=op.Issuer;return op.Read(c).Value;});
        var cache=a.Snapshot.RequireTypedScalars();Assert.Null(typeof(QueryTypedScalarCache).GetField("_issuer",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(cache));
        Assert.IsType<object>(typeof(QueryTypedScalarCache).GetField("_issuerIdentity",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(cache));
        int callbacks=f.SourceProvider.Calls.Count;Assert.Throws<ObjectDisposedException>(()=>old!.Evaluate(value));Assert.Throws<ObjectDisposedException>(()=>first!.ThrowIfUnavailable(default));Assert.Equal(callbacks,f.SourceProvider.Calls.Count);
        c.OnRead=_=>throw new Exception("cached getter replay");
        f.Target.RunCompatibleIdentityCandidate(a,op=>{Assert.NotSame(first,op.Issuer);Assert.Same(first!.ScopeIdentity,op.Issuer.ScopeIdentity);Assert.Same(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(op.Read(c).Value));Assert.Equal(value,op.Evaluate(value));Assert.Throws<NotSupportedException>(()=>op.RequireNoGenericFallback(value));return 0;});
        Assert.Equal(3,f.TargetProvider.Establishes);Assert.Equal(1,a.Snapshot.TypedScalarCount);Assert.Equal(0,f.Original.Runtime.TypedScalarCount);
    }
    /// <summary>CI06/07 selected carrier representations and source evidence survive unchanged.</summary>
    /// <param name="kind">Selected primitive carrier.</param>
    [Theory][InlineData("string")][InlineData("boolean")][InlineData("integer")][InlineData("decimal")]
    public void CI07_ExactCarriers(string kind)
    {
        using var f=new Pair(identity:true);using var a=f.Open().Value!;
        var plain=kind switch{"string"=>XdmValue.FromString("source","xs:string","urn:source:type"),"boolean"=>XdmValue.FromBoolean(true),"integer"=>XdmValue.FromInteger(BigInteger.Parse("1234567890123456789012345678901234567890")),_=>XdmValue.FromDecimal(new XdmExactDecimal(12345600,5,128,128))};
        var c=Value(f,plain);var p=(Provenance)c.Items[0].TypeEvidence;
        f.Target.RunCompatibleIdentityCandidate(a,op=>{var v=op.Read(c).Value;Assert.True(v.SchemaTransportMatches(plain));var stored=op.Validate(v);Assert.Same(c.Governing,stored.GoverningType);var atom=stored.GetAtomic(0);Assert.Same(p.Declared,atom.DeclaredType);Assert.Same(p.Actual,atom.ActualType);Assert.Same(f.SourceProvider.Origin,atom.OriginalAssessment);Assert.True(op.Evaluate(v).SchemaTransportMatches(plain));return 0;});
    }
    /// <summary>CI05/06 stored types and exact source issuance are checked again on every raw operation.</summary>
    /// <param name="reason">Revoked source claim.</param>
    [Theory][InlineData("container")][InlineData("governing")][InlineData("actual")][InlineData("declared")][InlineData("origin")]
    public void CI05_RevokeSourceEvidence(string reason)
    {
        using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);var v=Read(f,a,c);c.OnRead=_=>throw new Exception("getter replay");
        if(reason=="container")f.SourceProvider.Values.Clear();else if(reason=="governing")f.SourceProvider.Authority.Types.Remove(c.Governing);else if(reason=="declared")f.SourceProvider.Authority.Types.Remove(((Provenance)c.Items[0].TypeEvidence).Declared);else if(reason=="actual")f.SourceProvider.Authority.Types.Remove(((Provenance)c.Items[0].TypeEvidence).Actual);else f.SourceProvider.OriginOwned=false;
        Assert.Throws<ArgumentException>(()=>f.Target.RunCompatibleIdentityCandidate(a,op=>op.Evaluate(v)));
    }
    /// <summary>CI08 eligibility is decided before any scalar installation.</summary>
    /// <param name="mode">Unsupported shape or family.</param>
    [Theory][InlineData("empty")][InlineData("list-one")][InlineData("list-many")][InlineData("many")][InlineData("double")]
    public void CI08_UnsupportedBeforeCommit(string mode)
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f,mode=="double"?XdmValue.FromDouble(1):null,mode=="empty"?0:mode is "many" or "list-many"?2:1,mode.StartsWith("list"),false);Assert.Equal(SchemaResultKind.Unsupported,f.Target.RunCompatibleIdentityCandidate(a,op=>op.Read(c)).Kind);Assert.Equal(0,a.Snapshot.TypedScalarCount);}
    /// <summary>CI09 numeric and atom bounds deny before commit, and never read an excess Current.</summary>
    /// <param name="mode">Capacity exceeded.</param>
    [Theory][InlineData("atoms")][InlineData("digits")][InlineData("scale")]
    public void CI09_Capacities(string mode)
    {using var f=new Pair(identity:true,limits:new(1,4,1,3,2));using var a=f.Open().Value!;var c=Value(f,mode=="digits"?XdmValue.FromInteger(1000):mode=="scale"?XdmValue.FromDecimal(0.001m):null,mode=="atoms"?2:1,fast:false);c.ForbiddenCurrent=1;Assert.Throws<NotSupportedException>(()=>Read(f,a,c));Assert.Equal(1,c.Currents);Assert.Equal(0,a.Snapshot.TypedScalarCount);Assert.Equal(1,c.Disposals);}
    /// <summary>CI10 fixed insertion and pre-delivery seams leave no prefix and allow a clean retry.</summary>
    /// <param name="fault">Fixed internal fault boundary.</param>
    [Theory][InlineData(-2)][InlineData(0)][InlineData(1)]
    public void CI10_Rollback(int fault)
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);Assert.Throws<InvalidOperationException>(()=>f.Target.RunCompatibleIdentityCandidate(a,op=>op.Read(c,fault)));Assert.Equal(0,a.Snapshot.TypedScalarCount);Read(f,a,c);Assert.Equal(1,a.Snapshot.TypedScalarCount);}
    /// <summary>CI11 original, foreign, absent and tampered markers cannot enter a target cache.</summary>
    /// <param name="mode">Foreign or altered transport.</param>
    [Theory][InlineData("source")][InlineData("foreign")][InlineData("plain")][InlineData("tampered")][InlineData("forged")]
    public void CI11_Markers(string mode)
    {
        using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);var good=Read(f,a,c);XdmValue bad;
        if(mode=="source")bad=f.Source.Run([f.Original],op=>op.ReadTypedValue(f.Original,c).Value);
        else if(mode=="foreign"){using var other=new Pair(identity:true);using var b=other.Open().Value!;bad=Read(other,b,Value(other));}
        else bad=mode=="plain"?XdmValue.FromInteger(7):XdmValue.SchemaTransportAttach(XdmValue.FromInteger(mode=="tampered"?8:7),mode=="forged"?new object():XdmValue.SchemaTransportMarker(good)!);
        Assert.Throws<ArgumentException>(()=>f.Target.RunCompatibleIdentityCandidate(a,op=>op.Evaluate(bad)));
    }
    /// <summary>CI13 later negative proof prevents the typed work callback and all container getters.</summary>
    /// <param name="decision">Current negative proof.</param>
    [Theory][InlineData(SchemaCompatibilityDecision.Incompatible)][InlineData(SchemaCompatibilityDecision.Unsupported)]
    public void CI13_NegativeProof(SchemaCompatibilityDecision decision)
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);f.TargetProvider.Decision=decision;bool work=false;Assert.Throws<NotSupportedException>(()=>f.Target.RunCompatibleIdentityCandidate(a,op=>{work=true;return op.Read(c);}));Assert.False(work);Assert.Empty(c.Reads);}
    /// <summary>CI13 current cancellation/revocation closes every path before delivery.</summary>
    /// <param name="mode">Revocation point.</param>
    [Theory][InlineData("source")][InlineData("target")][InlineData("input")][InlineData("cancel")][InlineData("view")]
    public void CI13_CallbackRevocation(string mode)
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);using var cancel=new CancellationTokenSource();c.OnRead=stage=>{if(stage!="governing")return;switch(mode){case "source":f.Source.Dispose();break;case "target":f.Target.Dispose();break;case "input":f.Input.Dispose();break;case "cancel":cancel.Cancel();break;default:f.TargetProvider.LastView!.Source=null!;break;}};var error=Record.Exception(()=>f.Target.RunCompatibleIdentityCandidate(a,op=>op.Read(c),cancel.Token));Assert.NotNull(error);Assert.False(error is Xunit.Sdk.XunitException);Assert.Equal(0,a.Snapshot.TypedScalarCount);Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);}
    /// <summary>CI14 provider callbacks can inspect cache count on another thread without the cache gate being held.</summary>
    [Fact] public void CI14_NoProviderUnderCacheGate()
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f);Read(f,a,c);int probe=0;f.SourceProvider.Hook=s=>{if(s!="typed-issuance"||Interlocked.Exchange(ref probe,1)!=0)return;Assert.True(Task.Run(()=>a.Snapshot.TypedScalarCount).Wait(TimeSpan.FromSeconds(5)));};f.Target.RunCompatibleIdentityCandidate(a,op=>op.Evaluate(Read(f,a,c)));Assert.Equal(1,probe);}
    /// <summary>CI15 target quota counts one scalar independently from original source residency.</summary>
    [Fact] public void CI15_IndependentQuota()
    {using var f=new Pair(identity:true,limits:new(1,1,4,128,128));using var a=f.Open().Value!;var c=Value(f);var v=Read(f,a,c);Assert.Throws<NotSupportedException>(()=>Read(f,a,Value(f)));Assert.Equal(1,a.Snapshot.TypedScalarCount);Assert.Equal(0,f.Original.Runtime.TypedScalarCount);f.Target.RunCompatibleIdentityCandidate(a,op=>op.Evaluate(v));Assert.Throws<ArgumentException>(()=>f.Open());}

    /// <summary>CI10 malformed cells fail before commit, including enumerator cleanup errors.</summary>
    /// <param name="mode">Malformed source cell.</param>
    [Theory][InlineData("default")][InlineData("marked")][InlineData("null-string")][InlineData("dispose")]
    public void CI10_MalformedCells(string mode)
    {using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f,fast:false);if(mode=="default")c.Items=[default];else if(mode=="dispose")c.Fail="dispose";else c.Items=[new(mode=="marked"?XdmValue.SchemaTransportAttach(XdmValue.FromInteger(7),new object()):XdmValue.FromString(null!),c.Items[0].TypeEvidence)];Assert.NotNull(Record.Exception(()=>Read(f,a,c)));Assert.Equal(0,a.Snapshot.TypedScalarCount);Assert.Equal(1,c.Disposals);}
    /// <summary>CI14 blocked typed enumeration retains actual input and both backends through close and drain.</summary>
    [Fact] public async Task CI14_PairedTypedTailResources()
    {
        using var f=new Pair(identity:true);using var a=f.Open().Value!;var c=Value(f,fast:false);
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        using var input=NewFile();using var source=NewFile();using var target=NewFile();f.Input.Resource=input;f.SourceProvider.Resource=source;f.TargetProvider.Resource=target;
        c.OnRead=stage=>{if(stage!="current")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);source.WriteByte(1);target.WriteByte(2);};
        var pending=Task.Run(()=>Read(f,a,c));
        try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));f.Source.Dispose();f.Target.Dispose();f.Input.Dispose();f.SourceProvider.Dispose();f.TargetProvider.Dispose();Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);Assert.False(f.Source.DrainAsync().IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);}
        finally{resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await pending.WaitAsync(TimeSpan.FromSeconds(10)));await f.Source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));await f.Target.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(input.CanWrite);Assert.False(source.CanWrite);Assert.False(target.CanWrite);Assert.Equal(1,c.Disposals);
    }
    private static FileStream NewFile()=>new(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);

    /// <summary>PK01 compatible stable entries never expose unleased current proof through observation.</summary>
    [Fact] public void PK01_CompatibleObservationRefuses()
    {
        using var f=new Pair(identity:true);using var admission=f.Open().Value!;var c=Value(f);
        var first=f.Target.RunCompatibleIdentityCandidate(admission,op=>
        {var value=op.Read(c).Value;int callbacks=f.SourceProvider.Calls.Count;Assert.False(SchemaAtomics.TryGetProvenance(in value,out var p));Assert.Null(p);Assert.Equal(callbacks,f.SourceProvider.Calls.Count);return value;});
        Assert.False(SchemaAtomics.TryGetProvenance(in first,out _));
        f.Target.RunCompatibleIdentityCandidate(admission,op=>
        {var again=op.Read(c).Value;Assert.Same(XdmValue.SchemaTransportMarker(first),XdmValue.SchemaTransportMarker(again));Assert.False(SchemaAtomics.TryGetProvenance(in again,out _));Assert.Equal(first,op.Evaluate(again));return 0;});
        Assert.Null(typeof(QueryTypedScalarCache).GetField("_issuer",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(admission.Snapshot.RequireTypedScalars()));
    }

    /// <summary>PK02 concurrent context-free observers cannot select a latest compatible proof or mutate cache.</summary>
    [Fact] public void PK02_ParallelCompatibleRefusal()
    {
        using var f=new Pair(identity:true);using var admission=f.Open().Value!;var c=Value(f);var value=Read(f,admission,c);
        int callbacks=f.SourceProvider.Calls.Count;
        Parallel.For(0,32,_=>{Assert.False(SchemaAtomics.TryGetProvenance(in value,out var p));Assert.Null(p);});
        Assert.Equal(callbacks,f.SourceProvider.Calls.Count);Assert.Equal(1,admission.Snapshot.TypedScalarCount);
    }

}
