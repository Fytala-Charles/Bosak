// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies compiled typed identity, exact evidence preservation and profile isolation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; compiled typed identity controls (1.57.0)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Numerics;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
using Fixture=Bosak.XPath.Runtime.Tests.SchemaAdvisorAdapterTests.Fixture;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies the actual compiled typed variable identity profile without public activation.</summary>
/// <param name="output">Records local compiled identity allocation observations.</param>
public sealed class TypedIdentityVmTests(ITestOutputHelper output)
{
    private static readonly SchemaQueryLimits Limits=new(4,8,8,2048,2048);
    /// <summary>Actual compiled VM identity preserves primitive and wide exact representation, marker and metadata.</summary>
    /// <param name="kind">Payload fixture selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)][InlineData(11)]
    public void CompiledIdentityPreservesEvidence(int kind)
    {
        XdmValue plain=kind switch{0=>XdmValue.FromInteger(7),1=>XdmValue.FromBoolean(true),2=>XdmValue.FromDouble(-0d),3=>XdmValue.FromFloat(float.NaN),4=>XdmValue.FromString("owned"),5=>XdmValue.FromDecimal(1.2300m),6=>XdmValue.FromInteger(BigInteger.Pow(10,1000)),7=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048)),8=>XdmValue.FromQName(new XsQName("n","urn:test","p")),9=>XdmValue.FromDateTime(new DateTimeOffset(2026,11,4,12,30,0,TimeSpan.FromHours(1))),10=>XdmValue.FromDuration("P2D"),_=>XdmValue.FromString("selected","xs:string","urn:user:derived")};
        using var f=new Fixture(true);var c=f.Value();c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        f.Run(op=>{var view=f.Read(op,c);var marked=view.GetTransport(0);var result=op.EvaluateTypedIdentityCandidate(f.Snapshot,marked);Assert.True(result.SchemaTransportMatches(plain));Assert.Same(view.GetIdentity(0),XdmValue.SchemaTransportMarker(result));Assert.Same(f.Actual,view.GetAtomic(0).ActualType);Assert.Same(f.Origin,view.GetAtomic(0).OriginalAssessment);return 0;});Assert.Equal(0,f.Input.Roots);
    }
    /// <summary>One marked list member remains scalar identity while governing list metadata and null origin survive.</summary>
    [Fact] public void SelectedListMemberAndWarmRawReuse()
    {
        using var f=new Fixture(true);var c=f.Value(3,true,false);var marked=f.Run(op=>f.Read(op,c,false).GetTransport(1));int reads=c.Reads.Count;c.Fail="governing";
        for(int i=0;i<10;i++)f.Run(op=>{var copied=(XdmValue)(object)marked;var result=op.EvaluateTypedIdentityCandidate(f.Snapshot,copied);Assert.Equal(2,result.IntegerValue);Assert.Same(XdmValue.SchemaTransportMarker(marked),XdmValue.SchemaTransportMarker(result));var view=f.Read(op,c,false);Assert.True(view.IsList);Assert.Null(view.GetAtomic(1).OriginalAssessment);return 0;});Assert.Equal(reads,c.Reads.Count);Assert.Equal(3,f.Snapshot.Runtime.TypedScalarCount);
    }
    /// <summary>Typed identity has a distinct descriptor and deterministic frozen context/limit fingerprint.</summary>
    [Fact] public void ProfileAndCapturedFingerprint()
    {
        var namespaces=new Dictionary<string,string>{{"p","urn:before"}};var options=new CompileOptions{Namespaces=namespaces};using var f=new Fixture(true,options);using var same=new Fixture(true,new CompileOptions{Namespaces=new Dictionary<string,string>{{"p","urn:before"}}});
        var environment=f.Session.Environment;Assert.Equal(TypedIdentityPlan.Profile,environment.BehaviorProfile);Assert.Equal(TypedIdentityPlan.Descriptor,environment.DescriptorFormatVersion);Assert.Same(environment,f.Assessor.Seen);Assert.Equal(environment.StaticContextFingerprint,same.Session.Environment.StaticContextFingerprint);
        namespaces["p"]="urn:after";Assert.Equal("urn:before",environment.StaticContext.Namespaces["p"]);using var changed=new Fixture(true,options);Assert.NotEqual(environment.StaticContextFingerprint,changed.Session.Environment.StaticContextFingerprint);
        using var otherLimits=Session.CompileTypedIdentityCandidate("$value",new SchemaQueryLimits(4,7,8,2048,2048)).Value;Assert.NotEqual(environment.StaticContextFingerprint,otherLimits.Environment.StaticContextFingerprint);
        using var ordinary=Session.CompileXPath("true()",Limits).Value;Assert.NotEqual(environment.BehaviorProfile,ordinary.Environment.BehaviorProfile);
    }
    /// <summary>Grammar admission never broadens identity into arithmetic, calls, sequences or another variable.</summary>
    /// <param name="source">Rejected source.</param>
    [Theory][InlineData("$v")][InlineData("$value + 0")][InlineData("string($value)")][InlineData("($value)")][InlineData("($value,$value)")][InlineData("doc('x')")][InlineData(".")][InlineData("")]
    public void UnavailableGrammarRefuses(string source)=>Assert.Equal(SchemaResultKind.Unsupported,Session.CompileTypedIdentityCandidate(source,Limits).Kind);
    /// <summary>Surrounding source whitespace is accepted without a different behavior fingerprint.</summary>
    [Fact] public void WhitespaceIdentityIsSameProfile()
    {using var a=Session.CompileTypedIdentityCandidate("$value",Limits).Value;using var b=Session.CompileTypedIdentityCandidate("  $value \r\n",Limits).Value;Assert.Equal(a.Environment.StaticContextFingerprint,b.Environment.StaticContextFingerprint);}
    /// <summary>Mutable schema callbacks and invalid/canceled compilation retain their explicit refusal boundaries.</summary>
    [Fact] public void OptionsLimitsAndCancellation()
    {
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileTypedIdentityCandidate("$value",Limits,new CompileOptions{SchemaSet=new System.Xml.Schema.XmlSchemaSet()}).Kind);
        Assert.Throws<ArgumentException>(()=>Session.CompileTypedIdentityCandidate("$value",default));using var cancel=new CancellationTokenSource();cancel.Cancel();Assert.Throws<OperationCanceledException>(()=>Session.CompileTypedIdentityCandidate("$value",Limits,token:cancel.Token));
    }
    /// <summary>Ordinary profiles refuse the candidate identity method before provider callbacks.</summary>
    [Fact] public void OrdinaryProfileDoesNotAcquireTypedCapability()
    {
        using var f=new Fixture();var c=f.Value();var marked=f.Run(op=>f.Read(op,c).GetTransport(0));f.Advisor.Hook=_=>throw new Exception("wrong profile provider poison");f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));return 0;});
    }
    /// <summary>Plain, unknown and altered incoming evidence refuses before VM execution.</summary>
    [Fact] public void InvalidEvidenceRefuses()
    {
        using var f=new Fixture(true);var c=f.Value();f.Run(op=>{var marked=f.Read(op,c).GetTransport(0);Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,XdmValue.FromInteger(1)));Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object())));var altered=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(2),XdmValue.SchemaTransportMarker(marked)!);Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,altered));return 0;});
    }
    /// <summary>Foreign snapshot/evidence cannot borrow another raw operation or schema endpoint.</summary>
    [Fact] public void ForeignAdmissionRefuses()
    {
        using var f=new Fixture(true);using var g=new Fixture(true);var marked=f.Run(op=>f.Read(op,f.Value()).GetTransport(0));f.Advisor.Hook=_=>throw new Exception("foreign provider poison");g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(g.Snapshot,marked));return 0;});
    }
    /// <summary>Current advisor/container/type ownership remains required during compiled identity.</summary>
    /// <param name="change">Dependency to revoke.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void RevocationRefuses(int change)
    {
        using var f=new Fixture(true);var c=f.Value();var marked=f.Run(op=>f.Read(op,c).GetTransport(0));int reads=c.Reads.Count;
        if(change==0)f.Advisor.Values.Clear();else if(change==1)((QueryTypedContainerTests.Authority)f.Assessor.Selected).Types.Remove(f.Actual);else f.Advisor.Available=false;
        f.Run(op=>{if(change==2)Assert.Throws<ObjectDisposedException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));else Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));return 0;});Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Raw expiry and close revoke identity without a copied value retaining a resource lease.</summary>
    /// <returns>Completion of source drain.</returns>
    [Fact] public async Task RawExpiryAndClose()
    {
        using var f=new Fixture(true);var c=f.Value();SchemaQueryOperation? escaped=null;var marked=f.Run(op=>{escaped=op;return f.Read(op,c).GetTransport(0);});Assert.Throws<ObjectDisposedException>(()=>escaped!.EvaluateTypedIdentityCandidate(f.Snapshot,marked));f.Session.Dispose();await f.Session.DrainAsync();Assert.Equal(1,f.Input.Releases);Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);Assert.Throws<ObjectDisposedException>(()=>f.Run(op=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked)));
    }
    /// <summary>Cancellation or close from a pre-execution membership call stops identity and retains cleanup ordering.</summary>
    /// <param name="close">Whether the session closes instead of cancellation.</param>
    [Theory][InlineData(false)][InlineData(true)] public void InterruptionBeforeVm(bool close)
    {
        using var f=new Fixture(true);var c=f.Value();var marked=f.Run(op=>f.Read(op,c).GetTransport(0));using var cancel=new CancellationTokenSource();f.Advisor.Hook=s=>{if(s=="value"){if(close)f.Session.Dispose();else cancel.Cancel();}};
        f.Run(op=>{if(close)Assert.Throws<ObjectDisposedException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));else Assert.Throws<OperationCanceledException>(()=>op.EvaluateTypedIdentityCandidate(f.Snapshot,marked));return 0;},cancel.Token);
    }
    /// <summary>Public evaluator and assessor guards do not gain this internal profile implicitly.</summary>
    [Fact] public void PublicBoundariesRemainUnchanged()
    {
        using var f=new Fixture(true);var c=f.Value();f.Run(op=>{var marked=f.Read(op,c).GetTransport(0);Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(f.Snapshot,c).Kind);Assert.Throws<NotSupportedException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",marked)));Assert.Throws<NotSupportedException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",XdmValue.FromString("plain"))));return 0;});Assert.Null(f.Session.Advisor);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("$value",Limits,assessor:f.Assessor).Kind);
    }
    /// <summary>Observes compiled VM identity allocation under real endpoint/input guards with no payload getter replay.</summary>
    [Fact] public void AllocationObservation()
    {
        using var f=new Fixture(true);var c=f.Value(3,true);var marked=f.Run(op=>f.Read(op,c).GetTransport(1));int reads=c.Reads.Count;
        f.Run(op=>{for(int i=0;i<100;i++)op.EvaluateTypedIdentityCandidate(f.Snapshot,marked);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)op.EvaluateTypedIdentityCandidate(f.Snapshot,marked);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Typed identity compiled-vm: {bytes} bytes/read");Assert.InRange(bytes,1,32768);return 0;});Assert.Equal(reads,c.Reads.Count);
    }

    /// <summary>Every captured admission/numeric ceiling contributes to the typed profile fingerprint.</summary>
    /// <param name="field">The ceiling to change.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public void EveryLimitChangesDescriptor(int field)
    {
        using var baseline=Session.CompileTypedIdentityCandidate("$value",Limits).Value;var limits=new SchemaQueryLimits(field==0?3:4,field==1?7:8,field==2?7:8,field==3?2047:2048,field==4?2047:2048);using var changed=Session.CompileTypedIdentityCandidate("$value",limits).Value;Assert.NotEqual(baseline.Environment.StaticContextFingerprint,changed.Environment.StaticContextFingerprint);Assert.Equal(baseline.Environment.DescriptorFormatVersion,changed.Environment.DescriptorFormatVersion);
    }
    /// <summary>Trusted evaluator fault probes cannot deliver stripped, changed, alternate or interrupted results.</summary>
    /// <param name="fault">The evaluator fault injected at return.</param>
    [Theory][InlineData("strip")][InlineData("change")][InlineData("alternate")][InlineData("cancel")][InlineData("close")]
    public void ResultGuardFaults(string fault)
    {
        using var f=new QueryTypedContainerTests.Fixture();using var cancel=new CancellationTokenSource();QuerySourceSession? active=null;XdmValue incoming=default,alternate=default;int calls=0;
        using var runtime=new QuerySourceSession(f.Session.Environment,context=>{calls++;Assert.True(context.TryGetVariable("value",out var provided));Assert.Same(XdmValue.SchemaTransportMarker(incoming),XdmValue.SchemaTransportMarker(provided));if(fault=="cancel")cancel.Cancel();if(fault=="close")active!.Dispose();return fault switch{"strip"=>XdmValue.WithoutSchemaEvidence(incoming),"change"=>XdmValue.SchemaTransportAttach(XdmValue.FromInteger(99),XdmValue.SchemaTransportMarker(incoming)!),"alternate"=>alternate,_=>incoming};},4,typedIdentity:true);active=runtime;
        using var snapshot=runtime.OpenSnapshot(runtime.Environment,f.Input,f.Input.Enter);var a=f.Value();var b=f.Value();
        runtime.Run(runtime.Environment,[snapshot],op=>{incoming=op.ReadRegisteredTypedContainer(snapshot,f.Issuer,f.Input,f.Origin,a,8,8,2048,2048).GetTransport(0);alternate=op.ReadRegisteredTypedContainer(snapshot,f.Issuer,f.Input,f.Origin,b,8,8,2048,2048).GetTransport(0);
            if(fault=="cancel")Assert.Throws<OperationCanceledException>(()=>op.EvaluateTypedIdentity(snapshot,incoming,8,2048,2048));else if(fault=="close")Assert.Throws<ObjectDisposedException>(()=>op.EvaluateTypedIdentity(snapshot,incoming,8,2048,2048));else Assert.Throws<ArgumentException>(()=>op.EvaluateTypedIdentity(snapshot,incoming,8,2048,2048));return 0;},cancel.Token);Assert.Equal(1,calls);Assert.Equal(fault=="close"?0:2,snapshot.TypedScalarCount);
    }

    /// <summary>General capture and evaluation cannot bypass the explicit operand route even with plain/source-free inputs.</summary>
    [Fact] public void GeneralRuntimeEntryPointsRefuse()
    {
        var environment=new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"identity-entry-guard-control");using var runtime=new QuerySourceSession(environment,_=>throw new Exception("general evaluator poison"),4,typedIdentity:true);
        runtime.Run(environment,[],op=>{var caller=new EvaluationContext().WithVariable("value",XdmValue.FromString("plain"));var inputs=caller.CaptureSchemaInputs(_=>false,8,128);Assert.Throws<NotSupportedException>(()=>op.CaptureInputs(caller,8,128));Assert.Throws<NotSupportedException>(()=>op.EvaluateAtomic(inputs));return 0;});
    }
}
