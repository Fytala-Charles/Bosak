// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies original-node atomization, ordered evidence and transactional lifetime guards.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; public original-node atomization controls (1.64.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Shared=Bosak.XPath.Runtime.Tests.PublicOriginalNilledTests;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Origin=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Origin;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies original-node atomization, ordered evidence and transactional lifetime guards.</summary>
/// <param name="output">Records bounded public allocation observations.</param>
public sealed class PublicOriginalDataTests(ITestOutputHelper output)
{
    private sealed class Fixture:IDisposable
    {
        internal readonly Shared.Fixture Inner;internal readonly Handle Actual=new();internal Container Value;
        internal Fixture(int count=1,bool? list=null,XdmNodeKind kind=XdmNodeKind.Element,int capacity=8,int atoms=8,bool open=true,bool original=true,string expression="data($node)",CompileOptions? options=null)
        {
            Inner=new(expression,open,original,options,new(4,capacity,atoms,128,128));Inner.Node.Kind=kind;
            Inner.Facts.TypedAllowed=true;Inner.Provider.TypedAllowed=true;Inner.Provider.Authority.Types.Add(Actual);
            Value=Create(count,list??count!=1);Select(Value);
        }
        internal Container Create(int count,bool list)
        {var value=new Container{Governing=Inner.Type,List=list,Fast=count==1};value.Items=Enumerable.Range(0,count).Select(i=>new SchemaAtomicItem(XdmValue.FromInteger(i+1),new Provenance(value,Inner.Type,Actual,Inner.Origin))).ToArray();return value;}
        internal void Select(Container value){Value=value;Inner.Facts.Value=value;Inner.Provider.Values.Add(value);}
        internal XdmValue Evaluate()=>Inner.Session.Run([Inner.Snapshot!],op=>op.Evaluate(Context(XdmValue.FromNode(Inner.Input.Node))));
        internal XdmValue Raw(int faultAfter=-1)=>Inner.Snapshot!.Runtime.Owner.Run(Inner.Snapshot.Runtime.Owner.Environment,[Inner.Snapshot.Runtime],op=>op.AtomizeOriginalNode(Inner.Snapshot.Runtime,(IQueryNodeFactsIssuer)Inner.Snapshot.TypedState!.Issuer,Inner.Input,Inner.Origin,Inner.Input.Node,Inner.Facts,Inner.Node.Kind,8,8,128,128,faultAfter));
        internal QueryRegisteredTypedContainer Validate(QuerySourceSession.Operation op,XdmValue value)=>op.ValidateTypedTransport(Inner.Snapshot!.Runtime,value,8,128,128);
        internal int Count=>Inner.Snapshot!.Runtime.TypedScalarCount;
        /// <inheritdoc/>
        public void Dispose()=>Inner.Dispose();
    }
    private static EvaluationContext Context(XdmValue value)=>new EvaluationContext().WithVariable("node",value);
    private static XdmValue[] Cells(XdmValue value)
    {if(value.IsUndefined)return [];if(value.Kind!=XdmValueKind.Sequence)return [value];var result=new List<XdmValue>();var cursor=value.SequenceValue!.GetEnumerator();while(cursor.MoveNext())result.Add(cursor.Current);return result.ToArray();}
    /// <summary>AD01-AD02 original element/attribute returns a direct marked cell through public compilation.</summary>
    /// <param name="kind">Original node kind.</param>
    [Theory][InlineData(XdmNodeKind.Element)][InlineData(XdmNodeKind.Attribute)] public void AD01_AD02_Single(XdmNodeKind kind)
    {
        using var f=new Fixture(kind:kind);var result=f.Evaluate();Assert.Equal(1,result.IntegerValue);Assert.NotNull(XdmValue.SchemaTransportMarker(result));Assert.Equal(1,f.Count);
        Assert.Equal(OriginalNodeDataPlan.Profile,f.Inner.Session.Environment.BehaviorProfile);Assert.Equal(OriginalNodeDataPlan.Descriptor,f.Inner.Session.Environment.DescriptorFormatVersion);
        f.Inner.Snapshot!.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>{var view=f.Validate(op,result);Assert.Same(f.Actual,view.GetAtomic(0).ActualType);Assert.Same(f.Inner.Type,view.GetAtomic(0).DeclaredType);Assert.Same(f.Inner.Origin,view.GetAtomic(0).OriginalAssessment);return 0;});
    }
    /// <summary>AD03-AD04 materialized list order, duplicates and actual union members remain distinct.</summary>
    [Fact] public void AD03_AD04_OrderedMembers()
    {
        using var f=new Fixture(3);var other=new Handle();f.Inner.Provider.Authority.Types.Add(other);
        f.Value.Items=[new(XdmValue.FromInteger(7),new Provenance(f.Value,f.Inner.Type,f.Actual,f.Inner.Origin)),new(XdmValue.FromString("member"),new Provenance(f.Value,f.Inner.Type,other,f.Inner.Origin)),new(XdmValue.FromInteger(7),new Provenance(f.Value,f.Inner.Type,f.Actual,f.Inner.Origin))];
        var result=f.Evaluate();Assert.Equal(XdmValueKind.Sequence,result.Kind);var cells=Cells(result);Assert.Equal(3,f.Count);Assert.Equal(7,cells[0].IntegerValue);Assert.Equal("member",cells[1].StringValue);Assert.Equal(7,cells[2].IntegerValue);Assert.NotSame(XdmValue.SchemaTransportMarker(cells[0]),XdmValue.SchemaTransportMarker(cells[2]));
        f.Inner.Snapshot!.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>{Assert.Same(other,f.Validate(op,cells[1]).GetAtomic(1).ActualType);return 0;});
    }
    /// <summary>AD05 empty input checks current borrowed advisor with no source access.</summary>
    [Fact] public void AD05_EmptyInput()
    {using var f=new Fixture(open:false);Assert.True(f.Inner.Session.Run([],op=>op.Evaluate(Context(XdmValue.Undefined))).IsUndefined);f.Inner.Provider.Available=false;Assert.Throws<ObjectDisposedException>(()=>f.Inner.Session.Run([],op=>op.Evaluate(Context(XdmValue.Undefined))));Assert.Empty(f.Inner.Node.Reads);}
    /// <summary>AD06 absent element raises FOTY0012 without touching poisoned typed getters.</summary>
    [Fact] public void AD06_Absent()
    {using var f=new Fixture();f.Inner.Facts.Absent=true;f.Inner.Facts.TypedAllowed=false;Assert.StartsWith("FOTY0012:",Assert.Throws<InvalidOperationException>(()=>f.Evaluate()).Message);Assert.DoesNotContain("typed",f.Inner.Facts.Reads);Assert.Equal(0,f.Count);}
    /// <summary>AD07-AD08 supported empty and nilled-empty atomic/list results consume no slots.</summary>
    /// <param name="list">List shape.</param><param name="nilled">Original nilled state.</param>
    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)][InlineData(true,true)] public void AD07_AD08_Empty(bool list,bool nilled)
    {using var f=new Fixture(0,list);f.Inner.Facts.Nilled=nilled;Assert.True(f.Evaluate().IsUndefined);Assert.Equal(0,f.Count);Assert.Contains("typed",f.Inner.Facts.Reads);Assert.Equal(1,f.Value.Disposals);}
    /// <summary>AD09 contradictory metadata never becomes an empty result or absent-element error.</summary>
    /// <param name="mode">Malformed state.</param>
    [Theory][InlineData("absent-nilled")][InlineData("attribute-absent")][InlineData("attribute-nilled")][InlineData("nilled-nonempty")]
    public void AD09_Contradictions(string mode)
    {using var f=new Fixture();if(mode.StartsWith("attribute"))f.Inner.Node.Kind=XdmNodeKind.Attribute;f.Inner.Facts.Absent=mode.Contains("absent");f.Inner.Facts.Nilled=mode.Contains("nilled");Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Equal(0,f.Count);}
    /// <summary>AD10 missing/malformed outcomes differ from an unsupported read.</summary>
    /// <param name="mode">Provider outcome selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)] public void AD10_Outcomes(int mode)
    {using var f=new Fixture();if(mode<4)f.Inner.Facts.TypedKind=mode switch{0=>SchemaResultKind.Unsupported,1=>SchemaResultKind.NotFound,2=>SchemaResultKind.Invalid,_=>(SchemaResultKind)99};else if(mode==4)f.Inner.Facts.NullOutcome=true;else f.Inner.Facts.Value=null;if(mode==0)Assert.Throws<NotSupportedException>(()=>f.Evaluate());else Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Empty(f.Value.Reads);Assert.Equal(0,f.Count);}
    /// <summary>AD11 unsupported annotation/origin never falls back to legacy node properties.</summary>
    /// <param name="annotation">Remove channel instead of original assessment.</param>
    [Theory][InlineData(false)][InlineData(true)] public void AD11_MissingCapability(bool annotation)
    {using var f=new Fixture(original:annotation);if(annotation)f.Inner.Input.Node=new XDocumentNode(XElement.Parse("<n/>"));Assert.Throws<NotSupportedException>(()=>f.Evaluate());Assert.Empty(f.Value.Reads);}
    /// <summary>AD12 known out-of-profile kinds refuse before facts; invalid kinds are malformed.</summary>
    /// <param name="kind">Node kind.</param>
    [Theory][InlineData(XdmNodeKind.Document)][InlineData(XdmNodeKind.Text)][InlineData(XdmNodeKind.Comment)][InlineData(XdmNodeKind.ProcessingInstruction)][InlineData(XdmNodeKind.Namespace)][InlineData(XdmNodeKind.None)][InlineData(XdmNodeKind.All)]
    public void AD12_NodeKinds(XdmNodeKind kind)
    {using var f=new Fixture(kind:kind);if(kind is XdmNodeKind.None or XdmNodeKind.All)Assert.Throws<ArgumentException>(()=>f.Evaluate());else Assert.Throws<NotSupportedException>(()=>f.Evaluate());Assert.DoesNotContain("facts",f.Inner.Node.Reads);}
    /// <summary>AD13 foreign/ambiguous sources reject before any node getter.</summary>
    /// <param name="ambiguous">Admit a second claiming source.</param>
    [Theory][InlineData(false)][InlineData(true)] public void AD13_SourceOwnership(bool ambiguous)
    {using var f=new Fixture();var second=new Shared.Source{Node=f.Inner.Node};var snapshot=ambiguous?f.Inner.Session.OpenSnapshot(second,second).Value:null;if(!ambiguous)f.Inner.Input.Owned=false;f.Inner.Session.Run(snapshot is null?[f.Inner.Snapshot!]:[f.Inner.Snapshot!,snapshot],op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node))));return 0;});Assert.Empty(f.Inner.Node.Reads);}
    /// <summary>AD14 original facts/tuple issuance is checked before metadata getters.</summary>
    /// <param name="mode">Malformed facts channel.</param>
    [Theory][InlineData("null")][InlineData("copy")][InlineData("unissued")][InlineData("tuple")] public void AD14_Issuance(string mode)
    {using var f=new Fixture();switch(mode){case "null":f.Inner.Node.Facts=null;break;case "copy":f.Inner.Node.Facts=new Shared.Facts();break;case "unissued":f.Inner.Provider.FactsOwned=false;break;default:f.Inner.Provider.TupleOwned=false;break;}Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Empty(f.Inner.Facts.Reads);}
    /// <summary>AD15 foreign metadata or atomic evidence never registers a partial result.</summary>
    /// <param name="mode">Invalid evidence.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("container")][InlineData("governing")][InlineData("actual")][InlineData("cell-origin")][InlineData("cell-declared")][InlineData("authority")]
    public void AD15_ForeignEvidence(string mode)
    {
        using var f=new Fixture();switch(mode){case "type":f.Inner.Facts.Type=new Handle();break;case "declaration":f.Inner.Facts.Declaration=new Handle();break;case "origin":f.Inner.Facts.Origin=new Origin();break;case "container":f.Inner.Provider.Values.Clear();break;case "governing":f.Value.Governing=f.Actual;break;case "actual":f.Inner.Provider.Authority.Types.Remove(f.Actual);break;case "cell-origin":((Provenance)f.Value.Items[0].TypeEvidence).Origin=new Origin();break;case "cell-declared":((Provenance)f.Value.Items[0].TypeEvidence).Declared=new Handle();break;default:f.Inner.Provider.Selected=new QueryTypedContainerTests.Authority();break;}
        Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Equal(0,f.Count);
    }
    private sealed class PoisonSequence:IXdmSequence
    {bool IXdmSequence.TryGetLength(out int length)=>throw new Exception("length poison");IXdmSequenceEnumerator IXdmSequence.GetEnumerator()=>throw new Exception("iterator poison");}
    /// <summary>AD16-AD17 closed capture refuses unsupported inputs/state before node getters.</summary>
    /// <param name="mode">Unsupported dynamic state.</param>
    [Theory][InlineData("missing")][InlineData("extra")][InlineData("prefixed")][InlineData("atomic")][InlineData("sequence")][InlineData("map")][InlineData("array")][InlineData("function")][InlineData("focus")][InlineData("namespace")][InlineData("service")]
    public void AD16_AD17_Capture(string mode)
    {using var f=new Fixture();var context=Context(XdmValue.FromNode(f.Inner.Node));switch(mode){case "missing":context=new();break;case "extra":context.WithVariable("extra",XdmValue.Undefined);break;case "prefixed":context=new EvaluationContext().WithVariable("node",XdmValue.FromNode(f.Inner.Node),"urn:n");break;case "atomic":context=Context(XdmValue.FromInteger(1));break;case "sequence":context=Context(XdmValue.FromSequence(XdmSequence.FromSource(new PoisonSequence())));break;case "map":context=Context(XdmValue.FromMap(new XdmMap()));break;case "array":context=Context(XdmValue.FromArray(new XdmArray()));break;case "function":context=Context(XdmValue.FromFunction(new object()));break;case "focus":context.WithFocus(XdmValue.FromNode(f.Inner.Node),1,1);break;case "namespace":context.WithNamespace("p","urn:p");break;default:context.DocumentLoader=_=>throw new Exception("loader poison");break;}f.Inner.Session.Run([f.Inner.Snapshot!],op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(context));return 0;});Assert.Empty(f.Inner.Node.Reads);}
    /// <summary>AD18 syntax/options refusal occurs before provider Bind.</summary>
    /// <param name="expression">Unsupported syntax.</param><param name="foreign">Foreign fn namespace.</param>
    [Theory][InlineData("data()",false)][InlineData("data(.)",false)][InlineData("data($node)/x",false)][InlineData("f:data($node)",false)][InlineData("Q{http://www.w3.org/2005/xpath-functions}data($node)",false)][InlineData("data($node) + 1",false)][InlineData("fn:data($node)",true)]
    public void AD18_CompileRefusal(string expression,bool foreign)
    {var provider=new Shared.Provider();var options=foreign?new CompileOptions{Namespaces=new Dictionary<string,string>{{"fn","urn:foreign"}}}:null;Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(expression,new(4,8,8,128,128),options,provider).Kind);Assert.Equal(0,provider.Binds);}
    /// <summary>AD19 descriptor freezes options, canonical spelling and every finite limit.</summary>
    [Fact] public void AD19_Descriptors()
    {using var f=new Fixture();using var other=new Fixture(expression:"fn:data($node)");Assert.Equal(f.Inner.Session.Environment.StaticContextFingerprint,other.Inner.Session.Environment.StaticContextFingerprint);foreach(var limits in new[]{new SchemaQueryLimits(5,8,8,128,128),new(4,9,8,128,128),new(4,8,9,128,128),new(4,8,8,129,128),new(4,8,8,128,127)}){using var candidate=new Shared.Fixture("data($node)",limits:limits);Assert.NotEqual(f.Inner.Session.Environment.StaticContextFingerprint,candidate.Session.Environment.StaticContextFingerprint);}var ns=new Dictionary<string,string>{{"p","urn:one"}};using var frozen=new Fixture(options:new CompileOptions{Namespaces=ns});ns["p"]="urn:two";Assert.Equal("urn:one",frozen.Inner.Session.Environment.StaticContext.Namespaces["p"]);Assert.Equal(1,frozen.Evaluate().IntegerValue);}
    /// <summary>AD20 excess typed atoms refuse before reading an excess Current and dispose exactly once.</summary>
    [Fact] public void AD20_AtomLimit()
    {using var f=new Fixture(3,atoms:2);Assert.Throws<NotSupportedException>(()=>f.Evaluate());Assert.Equal(2,f.Value.Reads.Count(s=>s=="current"));Assert.Equal(1,f.Value.Disposals);Assert.Equal(0,f.Count);}
    /// <summary>AD21 whole-batch cache capacity failure leaves room for a fitting retry.</summary>
    [Fact] public void AD21_CapacityRetry()
    {using var f=new Fixture(3,capacity:2);Assert.Throws<NotSupportedException>(()=>f.Evaluate());Assert.Equal(0,f.Count);f.Select(f.Create(1,false));Assert.Equal(1,f.Evaluate().IntegerValue);Assert.Equal(1,f.Count);}
    /// <summary>AD22 warm nonempty reuse rereads facts/outcome but never container payload getters.</summary>
    [Fact] public void AD22_WarmReuse()
    {using var f=new Fixture(3);var first=Cells(f.Evaluate());int reads=f.Value.Reads.Count;f.Value.OnRead=_=>throw new Exception("payload replay poison");var second=Cells(f.Evaluate());Assert.Equal(reads,f.Value.Reads.Count);Assert.Equal(2,f.Inner.Facts.Reads.Count(s=>s=="typed"));for(int i=0;i<3;i++)Assert.Same(XdmValue.SchemaTransportMarker(first[i]),XdmValue.SchemaTransportMarker(second[i]));Assert.Equal(3,f.Count);}
    /// <summary>AD23-AD24 empty results do not fill an index; equal different container identities stay distinct.</summary>
    [Fact] public void AD23_AD24_ContainerKeys()
    {using var f=new Fixture(0);for(int i=0;i<12;i++){f.Select(f.Create(0,true));Assert.True(f.Evaluate().IsUndefined);Assert.Equal(0,f.Count);}f.Select(f.Create(1,false));var first=f.Evaluate();f.Select(f.Create(1,false));var second=f.Evaluate();Assert.Equal(first.IntegerValue,second.IntegerValue);Assert.NotSame(XdmValue.SchemaTransportMarker(first),XdmValue.SchemaTransportMarker(second));Assert.Equal(2,f.Count);}
    /// <summary>AD25 fixed insertion fault seams roll back the entire node-derived batch before retry.</summary>
    /// <param name="point">Fault after this many insertions.</param>
    [Theory][InlineData(-2)][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void AD25_TransactionFault(int point)
    {using var f=new Fixture(3);Assert.Throws<InvalidOperationException>(()=>f.Raw(point));Assert.Equal(0,f.Count);Assert.Equal(3,Cells(f.Evaluate()).Length);}
    /// <summary>AD26 postcommit cancellation/close returns no result and preserves complete-batch semantics.</summary>
    /// <param name="close">Close instead of cancel.</param>
    [Theory][InlineData(false)][InlineData(true)] public void AD26_Postcommit(bool close)
    {using var f=new Fixture(2);using var cancel=new CancellationTokenSource();f.Inner.Provider.Hook=_=>{if(f.Count==2){if(close)f.Inner.Session.Dispose();else cancel.Cancel();}};f.Inner.Session.Run([f.Inner.Snapshot!],op=>{if(close)Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node))));else Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node))));Assert.Equal(0,f.Inner.Input.Releases);return 0;},cancel.Token);Assert.Equal(close?0:2,f.Count);}
    /// <summary>AD27 same-key staged public operations return the installed winning transport identities.</summary>
    /// <returns>A task completing after both real raw operations.</returns>
    [Fact] public async Task AD27_ConcurrentWinner()
    {using var f=new Fixture(3);f.Inner.Facts.Record=false;f.Inner.Node.Record=false;f.Value.Record=false;using var barrier=new Barrier(2);f.Value.OnRead=s=>{if(s=="governing")Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));};var result=await Task.WhenAll(Task.Run(f.Evaluate),Task.Run(f.Evaluate));var a=Cells(result[0]);var b=Cells(result[1]);for(int i=0;i<3;i++)Assert.Same(XdmValue.SchemaTransportMarker(a[i]),XdmValue.SchemaTransportMarker(b[i]));Assert.Equal(3,f.Count);}
    /// <summary>AD27 distinct issued wrapper identities race for one slot without rolling back the winner.</summary>
    /// <returns>A task completing after both public evaluations.</returns>
    [Fact] public async Task AD27_CapacityRace()
    {
        using var f=new Fixture(capacity:1);f.Inner.Facts.Record=false;f.Inner.Node.Record=false;
        var a=f.Value;var b=f.Create(1,false);f.Inner.Provider.Values.Add(b);a.Record=b.Record=false;
        using var selected=new ThreadLocal<Container>();f.Inner.Facts.ValueFactory=()=>selected.Value!;
        using var barrier=new Barrier(2);void Wait(string stage){if(stage=="governing")Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));}a.OnRead=b.OnRead=Wait;
        (Container Container,bool Passed,XdmValue Value) Evaluate(Container value){selected.Value=value;try{return(value,true,f.Evaluate());}catch(NotSupportedException){return(value,false,default);}}
        var results=await Task.WhenAll(Task.Run(()=>Evaluate(a)),Task.Run(()=>Evaluate(b)));
        var winner=Assert.Single(results,r=>r.Passed);Assert.Single(results,r=>!r.Passed);Assert.Equal(1,f.Count);
        a.OnRead=b.OnRead=null;f.Inner.Facts.ValueFactory=()=>winner.Container;
        Assert.Same(XdmValue.SchemaTransportMarker(winner.Value),XdmValue.SchemaTransportMarker(f.Evaluate()));
    }
    /// <summary>AD28 provider getter/ownership interruptions stop output and retain resources through raw completion.</summary>
    /// <param name="stage">Interrupted callback.</param><param name="mode">Cancel, close or revoke.</param>
    /// <returns>A task completing after raw work drains.</returns>
    [Theory][InlineData("node-kind","cancel")][InlineData("facts","cancel")][InlineData("type","cancel")][InlineData("typed","cancel")][InlineData("kind","cancel")][InlineData("payload","cancel")][InlineData("governing","cancel")][InlineData("list","cancel")][InlineData("current","cancel")][InlineData("actual","cancel")][InlineData("dispose","cancel")]
    [InlineData("owns","close")][InlineData("owns-facts","close")][InlineData("owns-value","close")][InlineData("current","close")][InlineData("typed","revoke")][InlineData("payload","revoke")]
    public async Task AD28_Interruption(string stage,string mode)
    {using var f=new Fixture(2);using var cancel=new CancellationTokenSource();void Interrupt(string name){if(name!=stage)return;if(mode=="cancel")cancel.Cancel();else if(mode=="close")f.Inner.Session.Dispose();else f.Inner.Provider.Available=false;}f.Inner.Node.Hook=Interrupt;f.Inner.Facts.Hook=Interrupt;f.Inner.Facts.OutcomeHook=Interrupt;f.Inner.Provider.Hook=Interrupt;f.Inner.Input.Hook=Interrupt;f.Value.OnRead=Interrupt;f.Inner.Session.Run([f.Inner.Snapshot!],op=>{if(mode=="cancel")Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node))));else Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node))));Assert.Equal(0,f.Inner.Input.Releases);return 0;},cancel.Token);await f.Inner.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(5));Assert.Equal(0,f.Count);}
    /// <summary>AD29 iterator primary/cleanup failures keep their original ordering and no batch.</summary>
    [Fact] public void AD29_CleanupOrder()
    {using var f=new Fixture(2);f.Value.OnRead=s=>{if(s is "current" or "dispose")throw new InvalidOperationException(s);};var e=Assert.Throws<AggregateException>(()=>f.Evaluate());Assert.Equal(new[]{"current","dispose"},e.InnerExceptions.Select(n=>n.Message));Assert.Equal(1,f.Value.Disposals);Assert.Equal(0,f.Count);}
    /// <summary>AD30 the separate nilled route remains metadata-only; data does not export registration helpers.</summary>
    [Fact] public void AD30_ProfileSeparation()
    {using var nilled=new Shared.Fixture();Assert.False(nilled.Evaluate().BooleanValue);Assert.DoesNotContain("typed",nilled.Facts.Reads);using var data=new Fixture();data.Inner.Session.Run([data.Inner.Snapshot!],op=>{Assert.Equal("BOSAK-ORIGINAL-DATA-UNSUPPORTED",op.ReadTypedValue(data.Inner.Snapshot!,data.Value).DiagnosticCode);return 0;});}
    /// <summary>AD31 materialized traversal stays readable after close without provider callbacks or a lease.</summary>
    [Fact] public void AD31_CopiedPayload()
    {using var f=new Fixture(3);var result=f.Evaluate();f.Inner.Session.Dispose();int calls=f.Inner.Provider.Calls;f.Inner.Provider.Available=false;f.Value.OnRead=_=>throw new Exception("provider poison");Assert.True(result.SequenceValue!.TryGetLength(out int count));Assert.Equal(3,count);Assert.Equal(new long[]{1,2,3},Cells(result).Select(v=>v.IntegerValue));Assert.Equal(3,Cells(result).Length);Assert.Equal(calls,f.Inner.Provider.Calls);Assert.Equal(1,f.Inner.Input.Releases);}
    /// <summary>AD32-AD33 extraction tuples and independently issued resident-cell authorization are distinct.</summary>
    [Fact] public void AD32_AD33_LaterCellAuthorization()
    {using var f=new Fixture(2);var cell=Cells(f.Evaluate())[0];QueryRegisteredTypedContainer? retained=null;f.Inner.Snapshot!.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>{retained=f.Validate(op,cell);return 0;});Assert.Throws<ObjectDisposedException>(()=>retained!.Count);f.Inner.Provider.TupleOwned=false;Assert.Throws<ArgumentException>(()=>f.Evaluate());f.Inner.Snapshot.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>{Assert.Equal(2,f.Validate(op,cell).Count);return 0;});}
    /// <summary>AD34 stale, foreign or representation-tampered resident cells never gain authority from payloads.</summary>
    /// <param name="mode">Revoked or invalid evidence.</param>
    [Theory][InlineData("container")][InlineData("origin")][InlineData("actual")][InlineData("source")][InlineData("plain")][InlineData("tampered")]
    public void AD34_RevokedCell(string mode)
    {using var f=new Fixture();var cell=f.Evaluate();switch(mode){case "container":f.Inner.Provider.Values.Remove(f.Value);break;case "origin":f.Inner.Provider.OriginOwned=false;break;case "actual":f.Inner.Provider.Authority.Types.Remove(f.Actual);break;case "source":f.Inner.Input.Available=false;break;case "plain":cell=XdmValue.FromInteger(1);break;default:cell=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(2),XdmValue.SchemaTransportMarker(cell)!);break;}if(mode=="source")Assert.Throws<ObjectDisposedException>(()=>f.Inner.Snapshot!.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>f.Validate(op,cell)));else f.Inner.Snapshot!.Runtime.Owner.Run(f.Inner.Snapshot.Runtime.Owner.Environment,[f.Inner.Snapshot.Runtime],op=>{Assert.Throws<ArgumentException>(()=>f.Validate(op,cell));return 0;});}
    /// <summary>AD35 equal source/assessor labels do not import markers into another compiled session.</summary>
    [Fact] public void AD35_ForeignSession()
    {using var f=new Fixture();var cell=f.Evaluate();using var other=Session.CompileXPath("data($node)",new(4,8,8,128,128),assessor:f.Inner.Provider).Value;using var admitted=other.OpenSnapshot(f.Inner.Input,f.Inner.Input,f.Inner.Origin).Value;admitted.Runtime.Owner.Run(admitted.Runtime.Owner.Environment,[admitted.Runtime],op=>{Assert.Throws<ArgumentException>(()=>op.ValidateTypedTransport(admitted.Runtime,cell,8,128,128));return 0;});other.Run([admitted],op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(cell)));return 0;});}
    /// <summary>AD36 two issued node associations may share the exact container/ordinal keys.</summary>
    [Fact] public void AD36_SharedContainer()
    {using var f=new Fixture();var first=f.Evaluate();var other=new Shared.Node{Facts=f.Inner.Facts};f.Inner.Input.AdditionalNode=other;var second=f.Inner.Session.Run([f.Inner.Snapshot!],op=>op.Evaluate(Context(XdmValue.FromNode(other))));Assert.Same(XdmValue.SchemaTransportMarker(first),XdmValue.SchemaTransportMarker(second));Assert.Equal(1,f.Count);}
    /// <summary>AD37 multi-result publication contains only a complete installed batch and no default prefix.</summary>
    [Fact] public void AD37_CompleteOutput()
    {using var f=new Fixture(3);var result=f.Evaluate();var cells=Cells(result);Assert.Equal(3,cells.Length);Assert.All(cells,cell=>Assert.NotNull(XdmValue.SchemaTransportMarker(cell)));Assert.Equal(3,f.Count);}
    /// <summary>AD38 numeric limits and malformed nonplain cells fail before registration.</summary>
    /// <param name="mode">Unsupported carrier/representation.</param>
    [Theory][InlineData("wide")][InlineData("default")][InlineData("structured")][InlineData("marked")]
    public void AD38_InvalidPayload(string mode)
    {using var f=new Fixture();var plain=mode=="wide"?XdmValue.FromInteger(BigInteger.Pow(10,200)):mode=="structured"?XdmValue.FromNode(f.Inner.Node):XdmValue.FromInteger(1);if(mode=="marked")plain=XdmValue.SchemaTransportAttach(plain,new object());f.Value.Items=mode=="default"?[default]:[new(plain,new Provenance(f.Value,f.Inner.Type,f.Actual,f.Inner.Origin))];if(mode=="wide")Assert.Throws<NotSupportedException>(()=>f.Evaluate());else Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Equal(0,f.Count);}
    /// <summary>AD38 supported exact and reference-backed atomic representations are preserved unchanged.</summary>
    /// <param name="kind">Payload case.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)]
    public void AD38_ExactPayload(int kind)
    {using var f=new Fixture();var plain=kind switch{0=>XdmValue.FromInteger(BigInteger.Pow(10,100)),1=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,128,128)),2=>XdmValue.FromDouble(-0d),3=>XdmValue.FromString("owned","xs:string","urn:original"),4=>XdmValue.FromBoolean(true),_=>XdmValue.FromQName(new XsQName("n","urn:n","p"))};f.Value.Items=[new(plain,new Provenance(f.Value,f.Inner.Type,f.Actual,f.Inner.Origin))];var value=f.Evaluate();Assert.True(value.SchemaTransportMatches(plain));Assert.NotNull(XdmValue.SchemaTransportMarker(value));Assert.Equal(plain.UserSchemaTypeName,value.UserSchemaTypeName);}
    /// <summary>AD39 actual public asynchronous callbacks retain complete raw ownership through detachment.</summary>
    /// <returns>A task completing after public async evaluation.</returns>
    [Fact] public async Task AD39_Async()
    {using var f=new Fixture(2);var result=await f.Inner.Session.RunAsync([f.Inner.Snapshot!],async op=>{await Task.Yield();var value=op.Evaluate(Context(XdmValue.FromNode(f.Inner.Node)));Assert.Equal(0,f.Inner.Input.Releases);return value;});Assert.Equal(2,Cells(result).Length);}
    /// <summary>AD40 records cold/warm public costs and unchanged payload getters/slot accounting.</summary>
    [Fact] public void AD40_Allocation()
    {using var f=new Fixture(3);f.Value.Record=false;f.Inner.Session.Run([f.Inner.Snapshot!],op=>{var context=Context(XdmValue.FromNode(f.Inner.Node));_=op.Evaluate(context);int typed=f.Inner.Facts.Reads.Count(s=>s=="typed");long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10;i++)Assert.Equal(3,Cells(op.Evaluate(context)).Length);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/10;output.WriteLine($"Public data warm three: {bytes} bytes/query; slots: {f.Count}; typed outcomes: {f.Inner.Facts.Reads.Count(s=>s=="typed")-typed}");Assert.Equal(3,f.Count);return 0;});long start=GC.GetAllocatedBytesForCurrentThread();using(var cold=new Fixture(3))Assert.Equal(3,Cells(cold.Evaluate()).Length);output.WriteLine($"Public data cold session three: {GC.GetAllocatedBytesForCurrentThread()-start} bytes");}
}
