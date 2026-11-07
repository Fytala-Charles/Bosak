// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies original-node facts, exact ownership and guarded typed-value distinctions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; internal original-node facts reader (1.61.0).                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Qualify shared ownership guards and warm metadata allocation reduction (1.61.1).         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Qualify metadata-only reads, poison callbacks and live guards (1.62.0). |
//                      | Charles Korthout | 0.4   | 07-11-2026     | Share fixture with captured-identity patch regression controls (1.81.1).                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using static Bosak.XPath.Runtime.Tests.QueryTypedContainerTests;

namespace Bosak.XPath.Runtime.Tests;
/// <summary>Exercises uncached facts using actual raw source-session leases and the complete container reader.</summary>
/// <param name="output">Records local allocation and callback observations.</param>
public sealed partial class QueryNodeFactsTests(ITestOutputHelper output)
{
    private sealed class Source : IQuerySourceInput,ISchemaAssessmentInput
    {
        internal IXdmNode Node=new XDocumentNode(XElement.Parse("<n>1</n>"));
        internal bool Available=true,Owned=true; internal int Releases,MembershipCalls; internal Action? OnOwn;
        IXdmNode IQuerySourceInput.Root=>throw new Exception("root poison");
        IXdmNode ISchemaAssessmentInput.Root=>throw new Exception("root poison");
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>throw new Exception("source poison");
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>throw new Exception("snapshot poison");
        void IQuerySourceInput.ThrowIfUnavailable()=>Check();
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();Check();}
        bool IQuerySourceInput.OwnsNode(IXdmNode node){MembershipCalls++;OnOwn?.Invoke();return Owned&&ReferenceEquals(Node,node);}
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>Owned&&ReferenceEquals(Node,node);
        internal void Check()=>ObjectDisposedException.ThrowIf(!Available,this);
        internal IDisposable Enter()=>new Release(this);
        private sealed class Release(Source source):IDisposable { private bool _done; /// <inheritdoc/>
            public void Dispose(){if(!_done){_done=true;source.Releases++;}} }
    }
    private sealed class Issuer(SchemaQueryEnvironment env,Source input,Authority authority,Origin origin) : IQueryNodeFactsIssuer
    {
        internal bool Available=true,InputOwned=true,OriginOwned=true,FactsOwned=true,TupleOwned=true;
        internal SchemaQueryEnvironment Env=env; internal ISchemaAuthority Selected=authority;
        internal readonly HashSet<ISchemaTypedValue> Values=new(ReferenceEqualityComparer.Instance);
        internal Action<string>? Hook; internal int Calls; internal ISchemaNodeFacts? IssuedFacts;
        private void Hit(string name){Calls++;Hook?.Invoke(name);}
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment { get {Hit("environment");return Env;} }
        ISchemaAuthority IQueryTypedContainerIssuer.Authority { get {Hit("authority");return Selected;} }
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available||!authority.Available,this);}
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput original,IQuerySourceInput current){Hit("input");return InputOwned&&ReferenceEquals(original,input)&&ReferenceEquals(current,input);}
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin o,ISchemaAssessmentInput original,IQuerySourceInput current){Hit("origin");return OriginOwned&&ReferenceEquals(o,origin)&&ReferenceEquals(original,input)&&ReferenceEquals(current,input);}
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue v,ISchemaAssessmentInput original,IQuerySourceInput current,ISchemaAssessmentOrigin? o){Hit("container");return Values.Contains(v)&&ReferenceEquals(original,input)&&ReferenceEquals(current,input)&&ReferenceEquals(o,origin);}
        bool IQueryNodeFactsIssuer.OwnsFacts(ISchemaNodeFacts facts){Hit("facts");return FactsOwned&&ReferenceEquals(facts,IssuedFacts);}
        bool IQueryNodeFactsIssuer.OwnsNode(IXdmNode node,ISchemaNodeFacts facts,ISchemaAssessmentInput original,ISchemaAssessmentOrigin o){Hit("tuple");return TupleOwned&&ReferenceEquals(node,input.Node)&&ReferenceEquals(original,input)&&ReferenceEquals(o,origin);}
    }
    private sealed class Outcome(Facts owner):SchemaResult<ISchemaTypedValue>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind { get {owner.Hit("kind");return owner.Kind;} }
        /// <inheritdoc/>
        public override ISchemaTypedValue Value { get {owner.Hit("payload");if(owner.Kind!=SchemaResultKind.Supported)throw new Exception("unsupported payload poison");return owner.Value!;} }
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    private sealed class Facts:ISchemaNodeFacts
    {
        internal ISchemaComponentHandle Type=null!; internal ISchemaComponentHandle? Declaration;
        internal ISchemaAssessmentOrigin Origin=null!; internal bool Absent,Nilled,NullOutcome;
        internal SchemaResultKind Kind=SchemaResultKind.Supported; internal Container? Value;
        internal readonly List<string> Reads=[]; internal Action<string>? Hook; internal string? Fail;
        internal void Hit(string name){Reads.Add(name);Hook?.Invoke(name);if(Fail==name)throw new InvalidOperationException(name);}
        ISchemaComponentHandle ISchemaNodeFacts.Type {get{Hit("type");return Type;}}
        ISchemaComponentHandle? ISchemaNodeFacts.Declaration {get{Hit("declaration");return Declaration;}}
        ISchemaAssessmentOrigin ISchemaNodeFacts.Origin {get{Hit("origin");return Origin;}}
        bool ISchemaNodeFacts.HasNoTypedValue {get{Hit("absent");return Absent;}}
        bool ISchemaNodeFacts.IsNilled {get{Hit("nilled");return Nilled;}}
        SchemaResult<ISchemaTypedValue> ISchemaNodeFacts.ReadTypedValue(CancellationToken token){Hit("typed");token.ThrowIfCancellationRequested();return NullOutcome?null!:new Outcome(this);}
    }
    private sealed class Fixture:IDisposable
    {
        internal readonly QuerySourceSession Session; internal readonly Source Input=new(); internal readonly Authority Authority=new();
        internal readonly Origin Origin=new(); internal readonly Handle Type=new(),Actual=new(),Declaration=new();
        internal readonly Issuer Issuer; internal readonly QuerySourceSession.Snapshot Snapshot; internal readonly Facts Facts;
        internal Fixture(int count=1,bool list=false)
        {
            var env=new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"original-facts-controls");Session=new(env,_=>XdmValue.FromString("unused"),4);
            Issuer=new(env,Input,Authority,Origin);Authority.Types.UnionWith([Type,Actual,Declaration]);
            var c=new Container{Governing=Type,List=list,Fast=count==1};c.Items=Enumerable.Range(0,count).Select(i=>new SchemaAtomicItem(XdmValue.FromInteger(i+1),new Provenance(c,Type,Actual,Origin))).ToArray();Issuer.Values.Add(c);
            Facts=new(){Type=Type,Declaration=Declaration,Origin=Origin,Value=c};Issuer.IssuedFacts=Facts;Snapshot=Session.OpenSnapshot(env,Input,Input.Enter);
        }
        internal T Run<T>(Func<QuerySourceSession.Operation,T> work,CancellationToken token=default)=>Session.Run(Session.Environment,[Snapshot],work,token);
        internal QueryNodeFacts Read(QuerySourceSession.Operation op,int atoms=8,bool metadataOnly=false)=>op.ReadNodeFacts(Snapshot,Issuer,Input,Origin,Input.Node,Facts,atoms,128,128,metadataOnly);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    /// <summary>NM01 reads metadata without typed outcomes, container getters or iterators.</summary>
    /// <param name="absent">Whether the original typed value is absent.</param>
    /// <param name="nilled">Whether the element is nilled.</param>
    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)]
    public void MetadataOnlySkipsTypedValues(bool absent,bool nilled)
    {
        using var f=new Fixture(2,true);f.Facts.Absent=absent;f.Facts.Nilled=nilled;
        f.Facts.Fail="typed";f.Facts.Value!.OnRead=_=>throw new Exception("container poison");
        f.Run(op=>{var v=f.Read(op,metadataOnly:true);Assert.Same(f.Type,v.Type);Assert.Same(f.Declaration,v.Declaration);
            Assert.Same(f.Origin,v.Origin);Assert.Equal(absent,v.HasNoTypedValue);Assert.Equal(nilled,v.IsNilled);
            Assert.Equal(XdmNodeKind.Element,v.NodeKind);Assert.Equal(QueryNodeTypedValueKind.NotRequested,v.TypedValueKind);
            Assert.Throws<NotSupportedException>(()=>v.TypedValue);return 0;});
        Assert.Equal(new[]{"type","declaration","origin","absent","nilled"},f.Facts.Reads);Assert.Empty(f.Facts.Value.Reads);
        Assert.Equal(0,(int)QueryNodeTypedValueKind.Absent);Assert.Equal(1,(int)QueryNodeTypedValueKind.Unsupported);
        Assert.Equal(2,(int)QueryNodeTypedValueKind.Supported);Assert.Equal(3,(int)QueryNodeTypedValueKind.NotRequested);
    }
    /// <summary>NM02 metadata staging never examines missing or unsupported typed outcomes.</summary>
    /// <param name="nullOutcome">Return null rather than Unsupported if invoked.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void MetadataOnlyIgnoresTypedOutcome(bool nullOutcome)
    {
        using var f=new Fixture();f.Facts.NullOutcome=nullOutcome;f.Facts.Kind=SchemaResultKind.Unsupported;f.Facts.Value=null;
        f.Run(op=>{Assert.Equal(QueryNodeTypedValueKind.NotRequested,f.Read(op,metadataOnly:true).TypedValueKind);return 0;});
        Assert.DoesNotContain("typed",f.Facts.Reads);
    }
    /// <summary>NM03 metadata-only preserves original tuple, component and nilled consistency checks.</summary>
    /// <param name="stage">Invalid metadata or ownership layer.</param>
    [Theory][InlineData("node")][InlineData("facts")][InlineData("tuple")][InlineData("input")][InlineData("origin-binding")]
    [InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("absent-nilled")][InlineData("attribute-nilled")]
    public void MetadataOnlyRejectsMalformedEvidence(string stage)
    {
        using var f=new Fixture();
        switch(stage){case "node":f.Input.Owned=false;break;case "facts":f.Issuer.FactsOwned=false;break;
            case "tuple":f.Issuer.TupleOwned=false;break;case "input":f.Issuer.InputOwned=false;break;
            case "origin-binding":f.Issuer.OriginOwned=false;break;case "type":f.Facts.Type=new Handle();break;
            case "declaration":f.Facts.Declaration=new Handle();break;case "origin":f.Facts.Origin=new Origin();break;
            case "absent-nilled":f.Facts.Absent=true;f.Facts.Nilled=true;break;
            default:f.Input.Node=new XDocumentNode(new XAttribute("a",""));f.Facts.Nilled=true;break;}
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op,metadataOnly:true));return 0;});
        Assert.DoesNotContain("typed",f.Facts.Reads);
        if(stage is "node" or "facts" or "tuple" or "input" or "origin-binding")Assert.Empty(f.Facts.Reads);
    }
    /// <summary>NM04 every metadata callback stops delivery on cancellation or close, preserving raw ownership.</summary>
    /// <param name="stage">Metadata callback.</param>
    /// <param name="close">Close instead of cancel.</param>
    /// <returns>A task completing after the interrupted operation has drained.</returns>
    [Theory][InlineData("type",false)][InlineData("declaration",false)][InlineData("origin",false)][InlineData("absent",false)][InlineData("nilled",false)]
    [InlineData("type",true)][InlineData("declaration",true)][InlineData("origin",true)][InlineData("absent",true)][InlineData("nilled",true)]
    public async Task MetadataOnlyInterruption(string stage,bool close)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();
        f.Facts.Hook=s=>{if(s==stage){if(close)f.Session.Dispose();else cancel.Cancel();}};
        f.Run(op=>{if(close)Assert.Throws<ObjectDisposedException>(()=>f.Read(op,metadataOnly:true));
            else Assert.Throws<OperationCanceledException>(()=>f.Read(op,metadataOnly:true));
            Assert.Equal(0,f.Input.Releases);Assert.False(f.Session.DrainAsync().IsCompleted);return 0;},cancel.Token);
        Assert.Equal(stage,f.Facts.Reads[^1]);Assert.DoesNotContain("typed",f.Facts.Reads);
        await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(5));if(close)Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>NM05 failed metadata getters preserve the original exception and permit an uncached retry.</summary>
    /// <param name="stage">Throwing metadata getter.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("absent")][InlineData("nilled")]
    public void MetadataOnlyRetry(string stage)
    {
        using var f=new Fixture();f.Facts.Fail=stage;
        f.Run(op=>{Assert.Equal(stage,Assert.Throws<InvalidOperationException>(()=>f.Read(op,metadataOnly:true)).Message);
            f.Facts.Fail="typed";Assert.Equal(QueryNodeTypedValueKind.NotRequested,f.Read(op,metadataOnly:true).TypedValueKind);return 0;});
    }
    /// <summary>NM06 retained metadata refuses every observation after completion without replaying getters.</summary>
    [Fact] public void MetadataOnlyRetainedViewsExpire()
    {
        using var f=new Fixture();var v=f.Run(op=>f.Read(op,metadataOnly:true));int reads=f.Facts.Reads.Count;
        foreach(var read in new Action[]{()=>{_=v.Type;},()=>{_=v.Declaration;},()=>{_=v.Origin;},()=>{_=v.HasNoTypedValue;},
            ()=>{_=v.IsNilled;},()=>{_=v.NodeKind;},()=>{_=v.TypedValueKind;},()=>{_=v.TypedValue;}})
            Assert.Throws<ObjectDisposedException>(read);
        Assert.Equal(reads,f.Facts.Reads.Count);
    }
    /// <summary>NM07 retained metadata rechecks type ownership, tuple issuance and callback revocation.</summary>
    /// <param name="stage">Revoked capability.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("tuple")][InlineData("facts")][InlineData("authority-callback")]
    public void MetadataOnlyRetainedRevocation(string stage)
    {
        using var f=new Fixture();f.Run(op=>{var v=f.Read(op,metadataOnly:true);int reads=f.Facts.Reads.Count;
            switch(stage){case "type":f.Authority.Types.Remove(f.Type);break;case "declaration":f.Authority.Types.Remove(f.Declaration);break;
                case "tuple":f.Issuer.TupleOwned=false;break;case "facts":f.Issuer.FactsOwned=false;break;
                default:f.Authority.OnOwn=_=>f.Issuer.TupleOwned=false;break;}
            Assert.Throws<ArgumentException>(()=>v.IsNilled);Assert.Equal(reads,f.Facts.Reads.Count);return 0;});
    }
    /// <summary>NM08 observes local metadata cold/warm cost and proves no typed provider work.</summary>
    [Fact] public void MetadataOnlyAllocation()
    {
        using var f=new Fixture();f.Facts.Fail="typed";f.Run(op=>{for(int i=0;i<10;i++)f.Read(op,metadataOnly:true);
            long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)f.Read(op,metadataOnly:true);
            long cold=(GC.GetAllocatedBytesForCurrentThread()-before)/100;var v=f.Read(op,metadataOnly:true);int reads=f.Facts.Reads.Count;
            for(int i=0;i<10;i++)_=v.IsNilled;int calls=f.Issuer.Calls;before=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<100;i++)_=v.IsNilled;long warm=(GC.GetAllocatedBytesForCurrentThread()-before)/100;
            output.WriteLine($"Metadata facts cold: {cold} bytes/read; warm: {warm} bytes/read; callbacks: {(f.Issuer.Calls-calls)/100} per observation");
            Assert.InRange(cold,1,2000000);Assert.Equal(0,warm);Assert.Equal(54,(f.Issuer.Calls-calls)/100);
            Assert.Equal(reads,f.Facts.Reads.Count);Assert.Empty(f.Facts.Value!.Reads);return 0;});
    }
    /// <summary>NF01-NF05 preserve ordered atomic/list shapes, original type, declaration and actual provenance.</summary>
    /// <param name="count">Atomic count.</param><param name="list">List shape.</param>
    [Theory][InlineData(1,false)][InlineData(0,false)][InlineData(0,true)][InlineData(1,true)][InlineData(3,true)]
    public void SupportedShapes(int count,bool list)
    {using var f=new Fixture(count,list);f.Run(op=>{var v=f.Read(op);Assert.Same(f.Type,v.Type);Assert.Same(f.Declaration,v.Declaration);Assert.Same(f.Origin,v.Origin);Assert.False(v.HasNoTypedValue);Assert.False(v.IsNilled);Assert.Equal(QueryNodeTypedValueKind.Supported,v.TypedValueKind);Assert.Equal(count,v.TypedValue.Count);Assert.Equal(list,v.TypedValue.IsList);for(int i=0;i<count;i++){var a=v.TypedValue.GetAtomic(i);Assert.Equal(i+1,a.Value.IntegerValue);Assert.Same(f.Actual,a.ActualType);Assert.Same(f.Origin,a.OriginalAssessment);}return 0;});}
    /// <summary>NF06 preserves absent state without requesting typed values.</summary>
    [Fact] public void AbsentDoesNotReadTypedValue()
    {using var f=new Fixture();f.Facts.Absent=true;f.Facts.Fail="typed";f.Run(op=>{var v=f.Read(op);Assert.True(v.HasNoTypedValue);Assert.Equal(QueryNodeTypedValueKind.Absent,v.TypedValueKind);Assert.Throws<NotSupportedException>(()=>v.TypedValue);return 0;});Assert.DoesNotContain("typed",f.Facts.Reads);}
    /// <summary>NF07 and NF22 retain nilled empty-element and non-element distinctions.</summary>
    /// <param name="attribute">Use an attribute node.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NilledAndNodeKind(bool attribute)
    {using var f=new Fixture(0);if(attribute)f.Input.Node=new XDocumentNode(new XAttribute("a",""));else f.Facts.Nilled=true;f.Run(op=>{var v=f.Read(op);Assert.Equal(attribute?XdmNodeKind.Attribute:XdmNodeKind.Element,v.NodeKind);Assert.Equal(!attribute,v.IsNilled);Assert.Equal(0,v.TypedValue.Count);return 0;});}
    /// <summary>NF08 refuses contradictions instead of rewriting facts.</summary>
    /// <param name="mode">Malformed combination.</param>
    [Theory][InlineData("absent")][InlineData("nonempty")][InlineData("attribute")]
    public void MalformedNilledFacts(string mode)
    {using var f=new Fixture(mode=="nonempty"?1:0);f.Facts.Nilled=true;if(mode=="absent")f.Facts.Absent=true;if(mode=="attribute")f.Input.Node=new XDocumentNode(new XAttribute("a",""));f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});}
    /// <summary>NF09-NF10 never read Unsupported/invalid outcome payloads or fabricate empty.</summary>
    /// <param name="kind">Provider kind.</param>
    [Theory][InlineData(SchemaResultKind.Unsupported)][InlineData(SchemaResultKind.NotFound)][InlineData(SchemaResultKind.Invalid)]
    public void OutcomeKinds(SchemaResultKind kind)
    {using var f=new Fixture();f.Facts.Kind=kind;f.Run(op=>{if(kind==SchemaResultKind.Unsupported){var v=f.Read(op);Assert.Equal(QueryNodeTypedValueKind.Unsupported,v.TypedValueKind);Assert.Throws<NotSupportedException>(()=>v.TypedValue);}else Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});Assert.DoesNotContain("payload",f.Facts.Reads);}
    /// <summary>NF10 rejects null outcome and Supported-null payload.</summary>
    /// <param name="outcome">Null outcome instead of payload.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NullOutcomes(bool outcome)
    {using var f=new Fixture();if(outcome)f.Facts.NullOutcome=true;else f.Facts.Value=null;f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});}
    /// <summary>NF11-NF13 reject unowned nodes, facts, tuples, input and origin before fact getters.</summary>
    /// <param name="stage">Ownership layer.</param>
    [Theory][InlineData("node")][InlineData("facts")][InlineData("tuple")][InlineData("input")][InlineData("origin")][InlineData("environment")]
    public void ForeignBeforeGetters(string stage)
    {using var f=new Fixture();switch(stage){case "node":f.Input.Owned=false;break;case "facts":f.Issuer.FactsOwned=false;break;case "tuple":f.Issuer.TupleOwned=false;break;case "input":f.Issuer.InputOwned=false;break;case "origin":f.Issuer.OriginOwned=false;break;default:f.Issuer.Env=new(ImmutableDictionary<string,string>.Empty,"foreign");break;}f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});Assert.Empty(f.Facts.Reads);}
    /// <summary>NF11 rejects ambiguous and wrong selected owners.</summary>
    /// <param name="ambiguous">Both inputs claim the node.</param>
    [Theory][InlineData(false)][InlineData(true)] public void ExactSelectedOwner(bool ambiguous)
    {using var f=new Fixture();var other=new Source{Node=f.Input.Node};var b=f.Session.OpenSnapshot(f.Session.Environment,other,other.Enter);f.Input.Owned=ambiguous;f.Session.Run(f.Session.Environment,[f.Snapshot,b],op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});Assert.Empty(f.Facts.Reads);}
    /// <summary>NF13-NF14 reject foreign origin, type, declaration and mismatched governing container.</summary>
    /// <param name="stage">Invalid evidence.</param>
    [Theory][InlineData("origin")][InlineData("type")][InlineData("declaration")][InlineData("governing")][InlineData("actual")]
    public void InvalidEvidence(string stage)
    {using var f=new Fixture();switch(stage){case "origin":f.Facts.Origin=new Origin();break;case "type":f.Facts.Type=new Handle();break;case "declaration":f.Facts.Declaration=new Handle();break;case "governing":f.Facts.Value!.Governing=f.Actual;break;default:f.Authority.Types.Remove(f.Actual);break;}f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});}
    /// <summary>NF15-NF16 stop after each fact/outcome getter revokes the selected resources.</summary>
    /// <param name="stage">Getter callback.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("absent")][InlineData("nilled")][InlineData("typed")][InlineData("kind")][InlineData("payload")]
    public void GetterRevocation(string stage)
    {using var f=new Fixture();f.Facts.Hook=s=>{if(s==stage)f.Issuer.Available=false;};f.Run(op=>{Assert.Throws<ObjectDisposedException>(()=>f.Read(op));return 0;});Assert.Equal(stage,f.Facts.Reads[^1]);}
    /// <summary>NF15 stops at ownership false answers that also revoke the source.</summary>
    /// <param name="source">Revoke from the neutral owner instead of issuer.</param>
    [Theory][InlineData(false)][InlineData(true)] public void RevocationOnFalseOwnership(bool source)
    {using var f=new Fixture();if(source){f.Input.Owned=false;f.Input.OnOwn=()=>f.Input.Available=false;}else{f.Issuer.FactsOwned=false;f.Issuer.Hook=s=>{if(s=="facts")f.Issuer.Available=false;};}f.Run(op=>{Assert.Throws<ObjectDisposedException>(()=>f.Read(op));return 0;});Assert.Empty(f.Facts.Reads);}
    /// <summary>NF15 revalidates facts during each subordinate container getter and iterator stage.</summary>
    /// <param name="stage">Container callback.</param>
    [Theory][InlineData("governing")][InlineData("list")][InlineData("fast")][InlineData("items")][InlineData("enumerator")][InlineData("move")][InlineData("current")][InlineData("declared")][InlineData("actual")][InlineData("origin")][InlineData("dispose")]
    public void ContainerRevocation(string stage)
    {using var f=new Fixture(2,true);var c=f.Facts.Value!;c.OnRead=s=>{if(s==stage)f.Issuer.FactsOwned=false;};f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op));return 0;});if(c.Reads.Contains("enumerator"))Assert.Equal(1,c.Disposals);}
    /// <summary>NF16 close and cancellation stop delivery but defer release until raw completion.</summary>
    /// <param name="close">Close the session instead of cancellation.</param>
    [Theory][InlineData(false)][InlineData(true)] public void CloseAndCancellation(bool close)
    {using var f=new Fixture();using var token=new CancellationTokenSource();f.Facts.Hook=s=>{if(s=="nilled"){if(close)f.Session.Dispose();else token.Cancel();}};f.Run(op=>{if(close)Assert.Throws<ObjectDisposedException>(()=>f.Read(op));else Assert.Throws<OperationCanceledException>(()=>f.Read(op));Assert.Equal(0,f.Input.Releases);return 0;},token.Token);if(close)Assert.Equal(1,f.Input.Releases);}
    /// <summary>NF17-NF18 preserve getter errors and allow a valid retry after staging failure.</summary>
    /// <param name="stage">Throwing getter.</param>
    [Theory][InlineData("type")][InlineData("typed")][InlineData("kind")][InlineData("payload")]
    public void ThrowAndRetry(string stage)
    {using var f=new Fixture();f.Facts.Fail=stage;f.Run(op=>{Assert.Equal(stage,Assert.Throws<InvalidOperationException>(()=>f.Read(op)).Message);f.Facts.Fail=null;Assert.Equal(1,f.Read(op).TypedValue.Count);return 0;});}
    /// <summary>NF17 retains enumeration primary/cleanup error ordering from the complete reader.</summary>
    [Fact] public void IteratorCleanupFailure()
    {using var f=new Fixture(2,true);f.Facts.Value!.OnRead=s=>{if(s is "current" or "dispose")throw new InvalidOperationException(s);};f.Run(op=>{var error=Assert.Throws<AggregateException>(()=>f.Read(op));Assert.Collection(error.InnerExceptions,e=>Assert.Equal("current",e.Message),e=>Assert.Equal("dispose",e.Message));Assert.Equal(1,f.Facts.Value.Disposals);return 0;});}
    /// <summary>NF18 capacity refusal leaves no persistent cache or partial value registration.</summary>
    [Fact] public void CapacityRetry()
    {using var f=new Fixture(2,true);f.Run(op=>{Assert.Throws<NotSupportedException>(()=>f.Read(op,1));Assert.Equal(2,f.Read(op,2).TypedValue.Count);return 0;});}
    /// <summary>NF19 all retained fact/container/atomic observations refuse after raw completion.</summary>
    [Fact] public void RetainedViewsExpire()
    {using var f=new Fixture();QueryTypedContainer container=null!;QueryTypedContainer.AtomicView atomic=default;var facts=f.Run(op=>{var v=f.Read(op);container=v.TypedValue;atomic=container.GetAtomic(0);return v;});int reads=f.Facts.Reads.Count;foreach(var read in new Action[]{()=>{_=facts.Type;},()=>{_=facts.Declaration;},()=>{_=facts.Origin;},()=>{_=facts.HasNoTypedValue;},()=>{_=facts.IsNilled;},()=>{_=facts.NodeKind;},()=>{_=facts.TypedValueKind;},()=>{_=facts.TypedValue;}})Assert.Throws<ObjectDisposedException>(read);Assert.Throws<ObjectDisposedException>(()=>container.Count);Assert.Throws<ObjectDisposedException>(()=>atomic.Value);Assert.Equal(reads,f.Facts.Reads.Count);}
    /// <summary>NF14/NF19 revalidate actual type and fact issuance without getter replay.</summary>
    [Fact] public void RetainedEvidenceRevocation()
    {using var f=new Fixture();f.Run(op=>{var v=f.Read(op);var c=v.TypedValue;var a=c.GetAtomic(0);int reads=f.Facts.Reads.Count;f.Authority.Types.Remove(f.Actual);Assert.Throws<ArgumentException>(()=>a.Value);f.Authority.Types.Add(f.Actual);f.Issuer.TupleOwned=false;Assert.Throws<ArgumentException>(()=>v.Type);Assert.Throws<ArgumentException>(()=>c.Count);Assert.Equal(reads,f.Facts.Reads.Count);return 0;});}
    /// <summary>NF20 records cold read and warm observation allocation without retained fact caches or payload replay.</summary>
    [Fact] public void AllocationAndGetterObservation()
    {using var f=new Fixture();f.Run(op=>{for(int i=0;i<10;i++)f.Read(op);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)f.Read(op);long cold=(GC.GetAllocatedBytesForCurrentThread()-before)/100;var v=f.Read(op);int reads=f.Facts.Reads.Count, payload=f.Facts.Value!.Reads.Count;for(int i=0;i<10;i++)_ = v.IsNilled;before=GC.GetAllocatedBytesForCurrentThread();int calls=f.Issuer.Calls;for(int i=0;i<100;i++)_ = v.IsNilled;long warm=(GC.GetAllocatedBytesForCurrentThread()-before)/100;output.WriteLine($"Node facts cold: {cold} bytes/read; warm: {warm} bytes/read; callbacks: {(f.Issuer.Calls-calls)/100} per observation");Assert.Equal(reads,f.Facts.Reads.Count);Assert.Equal(payload,f.Facts.Value.Reads.Count);Assert.InRange(cold,1,2000000);Assert.Equal(0,warm);Assert.InRange((f.Issuer.Calls-calls)/100,1,54);return 0;});}

    /// <summary>NF16 cancellation at every fact/outcome getter prevents later provider reads.</summary>
    /// <param name="stage">Cancellation callback.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("absent")][InlineData("nilled")][InlineData("typed")][InlineData("kind")][InlineData("payload")]
    public void GetterCancellation(string stage)
    {using var f=new Fixture();using var cancel=new CancellationTokenSource();f.Facts.Hook=s=>{if(s==stage)cancel.Cancel();};f.Run(op=>{Assert.Throws<OperationCanceledException>(()=>f.Read(op));return 0;},cancel.Token);Assert.Equal(stage,f.Facts.Reads[^1]);}
    /// <summary>NF12 copied fields are insufficient without actual object issuance.</summary>
    [Fact] public void CopiedFactsAreNotIssued()
    {using var f=new Fixture();var copy=new Facts{Type=f.Type,Origin=f.Origin,Value=f.Facts.Value};f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadNodeFacts(f.Snapshot,f.Issuer,f.Input,f.Origin,f.Input.Node,copy,8,128,128));return 0;});Assert.Empty(copy.Reads);}
    /// <summary>NF13 original node facts require a non-null original origin before getter access.</summary>
    [Fact] public void MissingOriginalOrigin()
    {using var f=new Fixture();f.Run(op=>{Assert.Throws<ArgumentNullException>(()=>op.ReadNodeFacts(f.Snapshot,f.Issuer,f.Input,null!,f.Input.Node,f.Facts,8,128,128));return 0;});Assert.Empty(f.Facts.Reads);}

    /// <summary>Authority callbacks cannot revoke required state between the shared metadata proof boundaries.</summary>
    /// <param name="declaration">Interrupt the optional declaration callback rather than the governing type.</param>
    /// <param name="mode">Required state revoked by authority ownership.</param>
    [Theory][InlineData(false,"source")][InlineData(true,"source")][InlineData(false,"cancel")][InlineData(true,"cancel")][InlineData(false,"tuple")][InlineData(true,"tuple")][InlineData(false,"false-source")][InlineData(true,"false-source")]
    public void MetadataAuthorityCallbackRevocation(bool declaration,string mode)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();
        f.Run(op=>{var view=f.Read(op);int reads=f.Facts.Reads.Count;var target=declaration?f.Declaration:f.Type;
            f.Authority.OnOwn=type=>{if(!ReferenceEquals(type,target))return;switch(mode){case "source":f.Input.Available=false;break;case "cancel":cancel.Cancel();break;case "tuple":f.Issuer.TupleOwned=false;break;default:f.Input.Available=false;f.Authority.Types.Remove(type);break;}};
            if(mode=="cancel")Assert.Throws<OperationCanceledException>(()=>view.IsNilled);
            else if(mode=="tuple")Assert.Throws<ArgumentException>(()=>view.IsNilled);
            else Assert.Throws<ObjectDisposedException>(()=>view.IsNilled);
            Assert.Equal(reads,f.Facts.Reads.Count);return 0;
        },cancel.Token);
    }
}
