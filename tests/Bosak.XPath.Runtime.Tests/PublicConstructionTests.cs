// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies source-free public construction, exact receipt consumption and provider lifetime.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; source-free operation-local construction (1.66.0).                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 06-11-2026     | Verify bounded public provenance observation and expiry (1.77.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies the exact public source-free construction route without synthetic source ownership.</summary>
/// <param name="output">Records local allocation observations.</param>
public sealed class PublicConstructionTests(ITestOutputHelper output)
{
    private sealed class Outcome<T>(SchemaResultKind kind,T value,Action<string>? hit=null):SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind{get{hit?.Invoke("kind");return kind;}}
        /// <inheritdoc/>
        public override T Value{get{hit?.Invoke("value");return kind==SchemaResultKind.Supported?value:throw new Exception("outcome Value poison");}}
        /// <inheritdoc/>
        public override string DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    private sealed class Authority:ISchemaAuthority
    {
        internal readonly Dictionary<ISchemaComponentHandle,object> Owned=new(ReferenceEqualityComparer.Instance);
        internal Action<string>? Hook;
        internal SchemaResultKind IdentityKind=SchemaResultKind.Supported;
        internal bool Wrong,NullIdentity;
        bool ISchemaAuthority.Owns(ISchemaComponentHandle h){Hook?.Invoke("owns");return Owned.ContainsKey(h);}
        SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)
        {Hook?.Invoke("same");return NullIdentity?null!:new Outcome<bool>(IdentityKind,!Wrong&&ReferenceEquals(Owned[a],Owned[b]),s=>Hook?.Invoke("same-"+s));}
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup poison");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup poison");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup poison");
        SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("derivation poison");
        SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("affiliation poison");
        SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle h,CancellationToken t)=>throw new Exception("closure poison");
    }
    private sealed class Episode:ISchemaAssessmentEpisode
    {
        internal DateTimeOffset Time=new(2026,11,5,9,8,7,TimeSpan.FromHours(1));internal int Zone=60,TimeReads,ZoneReads;
        internal Action<string>? Hook;
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime{get{TimeReads++;Hook?.Invoke("time");return Time;}}
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes{get{ZoneReads++;Hook?.Invoke("zone");return Zone;}}
    }
    private sealed class Provider:ISchemaAssessor
    {
        internal readonly Authority Authority=new();internal readonly Handle Target=new(),Actual=new();
        internal bool Lifetime=true,Available=true,LeaseAvailable=true,NullOutcome,NullValue,NullBinding,NullAdvisor,NullLease;
        internal int Binds,Entries,Releases,Constructs;internal bool Record=true;
        internal readonly List<string> Calls=[];internal Action<string>? Hook;internal Action? Release;
        internal SchemaResultKind ResultKind=SchemaResultKind.Supported,BindKind=SchemaResultKind.Supported;
        internal Container Value;internal XdmValue LastInput;internal ISchemaAssessmentEpisode? LastEpisode;
        internal Provider()
        {
            Authority.Owned.Add(Target,new());Authority.Owned.Add(Actual,new());Authority.Hook=Hit;
            Value=new Container{Governing=Target,Fast=true};Value.Items=[new(XdmValue.FromInteger(42),new Provenance(Value,Target,Actual,null))];
        }
        internal void Hit(string s){if(Record)Calls.Add(s);Hook?.Invoke(s);}
        ISchemaAuthority ISchemaAssessor.Authority{get{Hit("authority");return Authority;}}
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("language poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment e,CancellationToken t)
        {Binds++;return NullBinding?null!:new Outcome<ISchemaAssessmentAdvisor>(BindKind,NullAdvisor?null!:Lifetime?new Advisor(this,e):new BareAdvisor(this,e),s=>Hit("bind-"+s));}
        private class BareAdvisor(Provider p,IQuerySemanticEnvironment e):ISchemaAssessmentAdvisor
        {
            ISchemaAssessor ISchemaAssessmentAdvisor.Assessor{get{p.Hit("endpoint");return p;}}
            IQuerySemanticEnvironment ISchemaAssessmentAdvisor.QueryEnvironment{get{p.Hit("environment");return e;}}
            void ISchemaAssessmentAdvisor.ThrowIfUnavailable(CancellationToken t){p.Hit("available");t.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!p.Available,this);}
            bool ISchemaAssessmentAdvisor.OwnsAssessment(ISchemaAssessmentOrigin o)=>throw new Exception("origin issuance poison");
            bool ISchemaAssessmentAdvisor.OwnsNodeFacts(ISchemaNodeFacts f)=>throw new Exception("facts issuance poison");
            bool ISchemaAssessmentAdvisor.OwnsNode(IXdmNode n,ISchemaNodeFacts f,ISchemaAssessmentInput i,ISchemaAssessmentOrigin o)=>throw new Exception("node issuance poison");
            bool ISchemaAssessmentAdvisor.OwnsTypedValue(ISchemaTypedValue v,ISchemaAssessmentInput i,ISchemaAssessmentOrigin? o)=>throw new Exception("source-bound typed issuance poison");
            ISchemaAssessmentResult ISchemaAssessmentAdvisor.Assess(SchemaAssessmentRequest r,CancellationToken t)=>throw new Exception("assess poison");
            SchemaResult<ISchemaTypedValue> ISchemaAssessmentAdvisor.ConstructSimpleValue(XdmValue input,ISchemaComponentHandle target,ISchemaAssessmentEpisode ep,CancellationToken t)
            {p.Constructs++;p.LastInput=input;p.LastEpisode=ep;p.Hit("construct");return p.NullOutcome?null!:new Outcome<ISchemaTypedValue>(p.ResultKind,p.NullValue?null!:p.Value,s=>p.Hit("result-"+s));}
        }
        private sealed class Advisor:BareAdvisor,IQueryOperationLifetime
        {
            private readonly Provider _provider;
            internal Advisor(Provider provider,IQuerySemanticEnvironment environment):base(provider,environment){_provider=provider;}
            IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken t)
            {_provider.Entries++;_provider.Hit("enter");return _provider.NullLease?null!:new Lease(_provider);}
            private sealed class Lease(Provider p):IQueryOperationLease
            {
                /// <inheritdoc/>
                public void ThrowIfUnavailable(CancellationToken t){p.Hit("lease");t.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!p.LeaseAvailable,this);}
                /// <inheritdoc/>
                public void Dispose(){p.Releases++;p.Hit("release");p.Release?.Invoke();}
            }
        }
    }
    private sealed class Fixture:IDisposable
    {
        internal readonly Provider Provider;internal readonly Session Session;internal readonly Episode Episode=new();
        internal Fixture(SchemaQueryLimits? limits=null,string expression="$constructed",CompileOptions? options=null,Provider? provider=null)
        {Provider=provider??new();Session=Session.CompileXPath(expression,limits??new(2,1,4,128,128),options,Provider).Value;Provider.Calls.Clear();}
        internal SchemaResult<XdmValue> Construct(SchemaQueryOperation op,XdmValue? input=null)=>op.ConstructSimpleValue(input??XdmValue.FromString("42"),Provider.Target,Episode);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    private static EvaluationContext Context(XdmValue value)=>EvaluationContext.CreateIsolated().WithVariable("constructed",value);

    /// <summary>SC01-SC02, SC13 and SC19 exact selector uses only actual provider construction and fresh receipts.</summary>
    /// <param name="kind">Supported atomic carrier.</param>
    [Theory][InlineData("string")][InlineData("boolean")][InlineData("integer")][InlineData("decimal")]
    public void SC01_SC02_SC13_SC19_Construct(string kind)
    {
        using var f=new Fixture(expression:"  $constructed  ");var p=f.Provider;
        var value=kind switch {"string"=>XdmValue.FromString("hello"),"boolean"=>XdmValue.FromBoolean(true),"decimal"=>XdmValue.FromDecimal(1.2300m),_=>XdmValue.FromInteger(BigInteger.Pow(10,60))};
        p.Value.Items=[new(value,new Provenance(p.Value,p.Target,p.Actual,null))];
        Assert.Equal("bosak-constructed-atomic-v1",f.Session.Environment.BehaviorProfile);Assert.Equal("bosak-constructed-atomic-descriptor-v1",f.Session.Environment.DescriptorFormatVersion);
        f.Session.Run([],op=>{var constructed=f.Construct(op).Value;Assert.True(constructed.SchemaTransportMatches(value));Assert.NotNull(XdmValue.SchemaTransportMarker(constructed));Assert.Same(XdmValue.SchemaTransportMarker(constructed),XdmValue.SchemaTransportMarker(op.Evaluate(Context(constructed))));return 0;});
        Assert.Equal(1,p.Entries);Assert.Equal(1,p.Releases);Assert.Equal(1,p.Constructs);Assert.Null(XdmValue.SchemaTransportMarker(p.LastInput));Assert.Equal("42",p.LastInput.StringValue);
    }
    /// <summary>SC03 capability is optional on the exact advisor; unsupported bind does not expose payload.</summary>
    /// <param name="missing">Missing lifetime rather than Unsupported binding.</param>
    [Theory][InlineData(true)][InlineData(false)] public void SC03_UnsupportedBinding(bool missing)
    {var p=new Provider{Lifetime=!missing,BindKind=missing?SchemaResultKind.Supported:SchemaResultKind.Unsupported};var r=Session.CompileXPath("$constructed",new(2,1,4,128,128),assessor:p);Assert.Equal(SchemaResultKind.Unsupported,r.Kind);Assert.Equal(0,p.Entries);Assert.Equal(0,p.Constructs);Assert.Equal(0,p.Releases);}
    /// <summary>SC04 malformed binding does not acquire a provider operation.</summary>
    /// <param name="mode">Binding failure.</param>
    [Theory][InlineData("null")][InlineData("payload")][InlineData("invalid")][InlineData("notfound")][InlineData("unknown")]
    public void SC04_BindingErrors(string mode)
    {var p=new Provider();if(mode=="null")p.NullBinding=true;else if(mode=="payload")p.NullAdvisor=true;else p.BindKind=mode switch{"invalid"=>SchemaResultKind.Invalid,"notfound"=>SchemaResultKind.NotFound,_=>(SchemaResultKind)99};Assert.Throws<ArgumentException>(()=>Session.CompileXPath("$constructed",new(2,1,4,128,128),assessor:p));Assert.Equal(0,p.Entries);}
    /// <summary>SC05 and SC29 repeated calls share admission but produce fresh distinct receipts without source quotas.</summary>
    [Fact] public async Task SC05_SC29_RepeatedNoCache()
    {using var f=new Fixture();await f.Session.RunAsync([],async op=>{var receipts=new HashSet<object>(ReferenceEqualityComparer.Instance);for(int i=0;i<20;i++)receipts.Add(XdmValue.SchemaTransportMarker(f.Construct(op).Value)!);Assert.Equal(20,receipts.Count);await Task.Yield();return 0;});Assert.Equal(1,f.Provider.Entries);Assert.Equal(1,f.Provider.Releases);Assert.Equal(20,f.Provider.Constructs);}
    /// <summary>SC14 unsupported input never acquires or touches targets/episode/constructor.</summary>
    /// <param name="kind">Unsupported input shape.</param>
    [Theory][InlineData("undefined")][InlineData("integer")][InlineData("boolean")][InlineData("decimal")][InlineData("node")][InlineData("sequence")][InlineData("map")][InlineData("array")][InlineData("function")][InlineData("user")][InlineData("marker")]
    public void SC14_InputScreen(string kind)
    {
        using var f=new Fixture();var v=kind switch{"integer"=>XdmValue.FromInteger(1),"boolean"=>XdmValue.FromBoolean(true),"decimal"=>XdmValue.FromDecimal(1m),"node"=>XdmValue.FromNode(new PublicOriginalNilledTests.Node()),"sequence"=>XdmValue.FromSequence(XdmSequence.Empty),"map"=>XdmValue.FromMap(new XdmMap()),"array"=>XdmValue.FromArray(new XdmArray()),"function"=>XdmValue.FromFunction(new object()),"user"=>XdmValue.FromString("a","string","Q{u}t"),"marker"=>XdmValue.SchemaTransportAttach(XdmValue.FromString("a"),new object()),_=>XdmValue.Undefined};
        f.Provider.Hook=_=>throw new Exception("provider screen poison");f.Episode.Hook=_=>throw new Exception("episode poison");
        f.Session.Run([],op=>{Assert.Equal(SchemaResultKind.Unsupported,f.Construct(op,v).Kind);return 0;});Assert.Equal(0,f.Provider.Entries);Assert.Empty(f.Provider.Calls);
    }
    /// <summary>SC15 null arguments retain precedence; foreign target refuses before construction.</summary>
    [Fact] public void SC15_Arguments()
    {using var f=new Fixture();f.Session.Run([],op=>{Assert.Throws<ArgumentNullException>(()=>op.ConstructSimpleValue(XdmValue.Undefined,null!,f.Episode));Assert.Throws<ArgumentNullException>(()=>op.ConstructSimpleValue(XdmValue.Undefined,f.Provider.Target,null!));Assert.Throws<ArgumentException>(()=>op.ConstructSimpleValue(XdmValue.FromString("x"),new Handle(),f.Episode));return 0;});Assert.Equal(0,f.Provider.Constructs);}
    /// <summary>SC16-SC17 caller episode is read once and immutable constants fix the entire raw operation.</summary>
    [Fact] public void SC16_SC17_FrozenEpisode()
    {
        using var f=new Fixture();f.Session.Run([],op=>
        {f.Construct(op);var copy=f.Provider.LastEpisode!;Assert.NotSame(f.Episode,copy);Assert.Equal(f.Episode.Time,copy.CurrentDateTime);f.Construct(op);Assert.Same(copy,f.Provider.LastEpisode);Assert.Equal(2,f.Episode.TimeReads);Assert.Equal(2,f.Episode.ZoneReads);f.Episode.Time=f.Episode.Time.ToOffset(TimeSpan.Zero);Assert.Throws<ArgumentException>(()=>f.Construct(op));Assert.Equal(2,f.Provider.Constructs);return 0;});
    }
    /// <summary>SC18 implicit timezone has explicit inclusive host limits.</summary>
    /// <param name="zone">Timezone minutes.</param>
    [Theory][InlineData(-841)][InlineData(-840)][InlineData(840)][InlineData(841)] public void SC18_Timezone(int zone)
    {using var f=new Fixture();f.Episode.Zone=zone;f.Session.Run([],op=>{if(Math.Abs(zone)>840)Assert.Throws<ArgumentException>(()=>f.Construct(op));else Assert.Equal(SchemaResultKind.Supported,f.Construct(op).Kind);return 0;});Assert.Equal(Math.Abs(zone)>840?0:1,f.Provider.Constructs);}
    /// <summary>SC20 non-supported outcomes never read provider payload or diagnostics.</summary>
    /// <param name="kind">Non-supported constructor outcome.</param>
    [Theory][InlineData(SchemaResultKind.Invalid)][InlineData(SchemaResultKind.Unsupported)] public void SC20_Outcome(SchemaResultKind kind)
    {using var f=new Fixture();f.Provider.ResultKind=kind;f.Session.Run([],op=>{var r=f.Construct(op);Assert.Equal(kind,r.Kind);Assert.NotNull(r.DiagnosticCode);Assert.Throws<InvalidOperationException>(()=>r.Value);return 0;});Assert.DoesNotContain("result-value",f.Provider.Calls);Assert.Empty(f.Provider.Value.Reads);}
    /// <summary>SC21 malformed outcomes are not Unsupported or Invalid.</summary>
    /// <param name="mode">Malformed outcome.</param>
    [Theory][InlineData("null")][InlineData("payload")][InlineData("notfound")][InlineData("unknown")] public void SC21_MalformedOutcomes(string mode)
    {using var f=new Fixture();if(mode=="null")f.Provider.NullOutcome=true;else if(mode=="payload")f.Provider.NullValue=true;else f.Provider.ResultKind=mode=="notfound"?SchemaResultKind.NotFound:(SchemaResultKind)99;Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op)));}
    /// <summary>SC22 shape support does not flatten lists or turn emptiness into successful construction.</summary>
    /// <param name="count">Atomic count.</param><param name="list">List metadata.</param>
    [Theory][InlineData(0,false)][InlineData(1,true)][InlineData(2,false)][InlineData(2,true)] public void SC22_Shapes(int count,bool list)
    {using var f=new Fixture();var c=f.Provider.Value;c.Fast=false;c.List=list;c.Items=Enumerable.Repeat(c.Items[0],count).ToArray();f.Session.Run([],op=>{Assert.Equal(SchemaResultKind.Unsupported,f.Construct(op).Kind);return 0;});Assert.Equal(1,c.Disposals);}
    /// <summary>SC23 unsupported primitive families differ from malformed/default/nonplain carriers.</summary>
    /// <param name="kind">Carrier selector.</param>
    [Theory][InlineData("double")][InlineData("float")][InlineData("default")][InlineData("node")][InlineData("marker")][InlineData("annotation")][InlineData("subtype")]
    public void SC23_Carriers(string kind)
    {using var f=new Fixture();var c=f.Provider.Value;if(kind=="default")c.Items=[default];else{var v=kind switch{"double"=>XdmValue.FromDouble(1),"float"=>XdmValue.FromFloat(1),"node"=>XdmValue.FromNode(new PublicOriginalNilledTests.Node()),"marker"=>XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object()),"annotation"=>XdmValue.FromString("x","string","Q{u}t"),_=>XdmValue.FromString("x","token")};c.Items=[new(v,new Provenance(c,f.Provider.Target,f.Provider.Actual,null))];}f.Session.Run([],op=>{if(kind is "double" or "float")Assert.Equal(SchemaResultKind.Unsupported,f.Construct(op).Kind);else Assert.Throws<ArgumentException>(()=>f.Construct(op));return 0;});}
    /// <summary>SC24 output limits refuse excess before Current and keep exact numeric capacity errors.</summary>
    /// <param name="kind">Capacity boundary.</param>
    [Theory][InlineData("atoms")][InlineData("digits")][InlineData("scale")]
    public void SC24_Capacity(string kind)
    {using var f=new Fixture(new(2,1,1,4,2));var c=f.Provider.Value;if(kind=="atoms"){c.Fast=false;c.Items=[c.Items[0],c.Items[0]];c.ForbiddenCurrent=1;}else c.Items=[new(kind=="digits"?XdmValue.FromInteger(10000):XdmValue.FromDecimal(1.001m),new Provenance(c,f.Provider.Target,f.Provider.Actual,null))];Assert.Throws<NotSupportedException>(()=>f.Session.Run([],op=>f.Construct(op)));if(kind=="atoms"){Assert.Equal(1,c.Currents);Assert.Equal(1,c.Disposals);}}
    /// <summary>SC25 and SC27 authoritative aliases need no handle interning; actual union member remains owned.</summary>
    [Fact] public void SC25_SC27_AliasesAndActualMember()
    {using var f=new Fixture();var alias=new Handle();f.Provider.Authority.Owned[alias]=f.Provider.Authority.Owned[f.Provider.Target];f.Provider.Value.Governing=alias;((Provenance)f.Provider.Value.Items[0].TypeEvidence).Declared=alias;f.Session.Run([],op=>{var v=f.Construct(op).Value;Assert.Equal(42,op.Evaluate(Context(v)).IntegerValue);return 0;});Assert.DoesNotContain("derivation",f.Provider.Calls);}
    /// <summary>SC26 ownership and identity cannot be inferred from labels or supported false proofs.</summary>
    /// <param name="mode">Ownership/identity fault.</param>
    [Theory][InlineData("target")][InlineData("governing")][InlineData("declared")][InlineData("actual")][InlineData("false")][InlineData("unsupported")][InlineData("null")][InlineData("invalid")][InlineData("notfound")][InlineData("unknown")]
    public void SC26_TypeProof(string mode)
    {using var f=new Fixture();var p=f.Provider;switch(mode){case "target":p.Authority.Owned.Remove(p.Target);break;case "governing":p.Value.Governing=new Handle();break;case "declared":((Provenance)p.Value.Items[0].TypeEvidence).Declared=new Handle();break;case "actual":p.Authority.Owned.Remove(p.Actual);break;case "false":p.Authority.Wrong=true;break;case "null":p.Authority.NullIdentity=true;break;default:p.Authority.IdentityKind=mode switch{"unsupported"=>SchemaResultKind.Unsupported,"invalid"=>SchemaResultKind.Invalid,"notfound"=>SchemaResultKind.NotFound,_=>(SchemaResultKind)99};break;}f.Session.Run([],op=>{if(mode=="unsupported")Assert.Equal(SchemaResultKind.Unsupported,f.Construct(op).Kind);else Assert.Throws<ArgumentException>(()=>f.Construct(op));return 0;});}
    /// <summary>SC28 construction refuses inherited original assessment evidence.</summary>
    [Fact] public void SC28_OriginalOriginRefuses()
    {using var f=new Fixture();((Provenance)f.Provider.Value.Items[0].TypeEvidence).Origin=new QueryTypedContainerTests.Origin();Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op)));}
    /// <summary>SC30 all five limits and actual compiler options are frozen into the descriptor.</summary>
    [Fact] public void SC30_FrozenDescriptor()
    {var ns=new Dictionary<string,string>{{"p","urn:one"}};using var f=new Fixture(options:new CompileOptions{Namespaces=ns});ns["p"]="urn:two";Assert.Equal("urn:one",f.Session.Environment.StaticContext.Namespaces["p"]);foreach(var l in new[]{new SchemaQueryLimits(3,1,4,128,128),new(2,2,4,128,128),new(2,1,5,128,128),new(2,1,4,129,128),new(2,1,4,128,127)}){using var g=new Fixture(l,options:new CompileOptions{Namespaces=new Dictionary<string,string>{{"p","urn:one"}}});Assert.NotEqual(f.Session.Environment.StaticContextFingerprint,g.Session.Environment.StaticContextFingerprint);}}
    /// <summary>SC31 saved immutable payload is consumed without re-reading any constructor result getters.</summary>
    [Fact] public void SC31_NoGetterReplay()
    {using var f=new Fixture();f.Session.Run([],op=>{var v=f.Construct(op).Value;f.Provider.Value.OnRead=_=>throw new Exception("result replay poison");Assert.Equal(42,op.Evaluate(Context(v)).IntegerValue);return 0;});}
    /// <summary>SC32 closed context capture refuses before provider admission.</summary>
    /// <param name="mode">Unsupported context state.</param>
    [Theory][InlineData("missing")][InlineData("extra")][InlineData("prefixed")][InlineData("focus")][InlineData("namespace")][InlineData("service")][InlineData("time")]
    public void SC32_ClosedCapture(string mode)
    {using var f=new Fixture();var c=Context(XdmValue.FromInteger(1));switch(mode){case "missing":c=new();break;case "extra":c.WithVariable("extra",XdmValue.Undefined);break;case "prefixed":c=new EvaluationContext().WithVariable("constructed",XdmValue.FromInteger(1),"u");break;case "focus":c.WithFocus(XdmValue.FromInteger(1),1,1);break;case "namespace":c.WithNamespace("p","u");break;case "service":c.DocumentLoader=_=>throw new Exception("loader poison");break;default:c.ImplicitTimezoneOffsetMinutes=1;break;}Assert.Throws<NotSupportedException>(()=>f.Session.Run([],op=>op.Evaluate(c)));Assert.Equal(0,f.Provider.Entries);}
    /// <summary>SC33 forged marker and altered representations cannot authorize consumption.</summary>
    /// <param name="mode">Forgery shape.</param>
    [Theory][InlineData("plain")][InlineData("foreign")][InlineData("tampered")]
    public void SC33_Forgery(string mode)
    {using var f=new Fixture();f.Session.Run([],op=>{var v=mode=="tampered"?XdmValue.SchemaTransportAttach(XdmValue.FromInteger(43),XdmValue.SchemaTransportMarker(f.Construct(op).Value)!):mode=="plain"?XdmValue.FromInteger(42):XdmValue.SchemaTransportAttach(XdmValue.FromInteger(42),new object());Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(v)));return 0;});}
    /// <summary>SC34-SC35 copied payload remains readable but cannot revive another operation or session admission.</summary>
    [Fact] public void SC34_SC35_NoImport()
    {using var f=new Fixture();SchemaQueryOperation? retained=null;var v=f.Session.Run([],op=>{retained=op;return f.Construct(op).Value;});object box=v;Assert.Equal(42,((XdmValue)box).IntegerValue);Assert.Throws<ObjectDisposedException>(()=>retained!.Evaluate(Context(v)));Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>op.Evaluate(Context(v))));using var other=new Fixture(provider:f.Provider);Assert.Throws<ArgumentException>(()=>other.Session.Run([],op=>op.Evaluate(Context(v))));Assert.Equal(1,f.Provider.Entries);Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC36 target or actual type revocation before consumption prevents result authorization.</summary>
    /// <param name="actual">Revoke actual rather than requested target.</param>
    [Theory][InlineData(false)][InlineData(true)] public void SC36_ConsumptionCurrentness(bool actual)
    {using var f=new Fixture();f.Session.Run([],op=>{var v=f.Construct(op).Value;f.Provider.Authority.Owned.Remove(actual?f.Provider.Actual:f.Provider.Target);Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(v)));return 0;});}
    /// <summary>SC37 injected pre-publication failure publishes no marker and still releases actual admission.</summary>
    [Fact] public void SC37_UnpublishedFailure()
    {using var f=new Fixture();Assert.Throws<InvalidOperationException>(()=>f.Session.Run([],op=>{var plan=typeof(SchemaQueryOperation).GetField("_construction",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(op)!;plan.GetType().GetField("_publicationFault",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(plan,0);return f.Construct(op);}));Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC38 exact selector never expands general grammar or binds unsupported options.</summary>
    /// <param name="source">Unselected grammar.</param>
    [Theory][InlineData("$constructed + 1")][InlineData("($constructed)")][InlineData("$p:constructed")][InlineData("xs:integer($constructed)")][InlineData("$constructed cast as xs:string")]
    public void SC38_Grammar(string source)
    {var p=new Provider();Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,new(2,1,4,128,128),assessor:p).Kind);Assert.Equal(0,p.Binds);}
    /// <summary>SC39 this source-free profile refuses snapshots before source or lifetime callbacks.</summary>
    [Fact] public void SC39_NoSnapshots()
    {using var f=new Fixture();var input=new QueryTypedContainerTests.Input();var lifetime=new PublicOriginalNilledTests.Source{EnterFails=true};Assert.Equal(SchemaResultKind.Unsupported,f.Session.OpenSnapshot(input,lifetime).Kind);using var ordinary=new PublicOriginalNilledTests.Fixture();Assert.Throws<ArgumentException>(()=>f.Session.Run([ordinary.Snapshot!],_=>0));Assert.Equal(0,f.Provider.Entries);}
    /// <summary>SC40 synthetic cold/warm observations do not imply native budget acceptance.</summary>
    [Fact] public void SC40_AllocationObservation()
    {using var f=new Fixture();f.Provider.Record=false;f.Provider.Value.Record=false;f.Session.Run([],op=>{var v=f.Construct(op).Value;var context=Context(v);for(int i=0;i<10;i++)op.Evaluate(context);long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)op.Evaluate(context);long bytes=GC.GetAllocatedBytesForCurrentThread()-start;output.WriteLine($"Constructed warm identity: {bytes/100} bytes/query; constructions: {f.Provider.Constructs}; admissions: {f.Provider.Entries}");return 0;});}
    /// <summary>SC06-SC07 and SC11 actual constructor/evidence tails retain a real file through raw closure and drain.</summary>
    /// <param name="phase">Provider callback to block.</param><param name="stop">Close/cancellation mode.</param>
    [Theory][InlineData("enter","callback")][InlineData("construct","callback")][InlineData("actual","callback")][InlineData("dispose","callback")]
    [InlineData("construct","close")][InlineData("actual","close")][InlineData("construct","cancel")][InlineData("actual","cancel")]
    public async Task SC06_SC07_SC11_ActualProviderTail(string phase,string stop)
    {
        using var f=new Fixture();using var cancellation=new CancellationTokenSource();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        var done=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var path=Path.GetTempFileName();using var stream=new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        f.Provider.Release=stream.Dispose;f.Provider.Value.Fast=false;
        void Block(){entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));Assert.True(stream.CanWrite);stream.WriteByte(9);}
        if(phase is "enter" or "construct")f.Provider.Hook=name=>{if(name==phase)Block();};
        else f.Provider.Value.OnRead=name=>{if(name==phase)Block();};
        Task<SchemaResult<XdmValue>>? tail=null;SchemaQueryOperation? retained=null;
        var raw=f.Session.RunAsync([],async op=>
        {retained=op;tail=Task.Run(()=>f.Construct(op));Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));await done.Task;return 9;},cancellation.Token);
        try
        {
            Assert.True(entered.IsSet);if(stop=="close")f.Session.Dispose();if(stop=="cancel")cancellation.Cancel();done.SetResult();
            var deadline=DateTime.UtcNow.AddSeconds(10);
            while(Record.Exception(()=>f.Construct(retained!,XdmValue.Undefined)) is null&&DateTime.UtcNow<deadline)await Task.Delay(1);
            Assert.ThrowsAny<Exception>(()=>f.Construct(retained!,XdmValue.Undefined));
            Assert.False(raw.IsCompleted);Assert.Equal(0,f.Provider.Releases);Assert.True(stream.CanWrite);
            var drain=f.Session.DrainAsync();Assert.False(drain.IsCompleted);
            using var waitCancellation=new CancellationTokenSource();waitCancellation.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>f.Session.DrainAsync(waitCancellation.Token));
            Assert.False(drain.IsCompleted);Assert.True(stream.CanWrite);
        }
        finally {resume.Set();}
        await Assert.ThrowsAnyAsync<Exception>(async()=>await tail!.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal(9,await raw.WaitAsync(TimeSpan.FromSeconds(10)));await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(1,f.Provider.Entries);Assert.Equal(1,f.Provider.Releases);Assert.False(stream.CanWrite);
    }
    /// <summary>SC08 failed entry/retention does not publish receipts or keep phantom admissions.</summary>
    /// <param name="mode">Acquisition failure mode.</param>
    [Theory][InlineData("throw")][InlineData("null")][InlineData("revoke")]
    public async Task SC08_AcquisitionFailure(string mode)
    {using var f=new Fixture();var failure=new ApplicationException("enter");f.Provider.NullLease=mode=="null";f.Provider.Hook=name=>{if(name!="enter")return;if(mode=="throw")throw failure;if(mode=="revoke")f.Provider.LeaseAvailable=false;};var error=Record.Exception(()=>f.Session.Run([],op=>f.Construct(op)));if(mode=="throw")Assert.Same(failure,error);else if(mode=="null")Assert.IsType<InvalidOperationException>(error);else Assert.IsType<ObjectDisposedException>(error);Assert.Equal(0,f.Provider.Constructs);Assert.Equal(mode=="revoke"?1:0,f.Provider.Releases);f.Session.Dispose();await f.Session.DrainAsync();}
    /// <summary>SC09 guards each provider and evidence callback before acting on its returned result.</summary>
    /// <param name="phase">Revocation callback.</param>
    [Theory][InlineData("available")][InlineData("endpoint")][InlineData("environment")][InlineData("authority")][InlineData("lease")][InlineData("owns")][InlineData("same")][InlineData("same-kind")][InlineData("same-value")]
    [InlineData("construct")][InlineData("result-kind")][InlineData("result-value")][InlineData("time")][InlineData("zone")][InlineData("governing")][InlineData("list")][InlineData("fast")][InlineData("items")][InlineData("enumerator")][InlineData("move")][InlineData("current")][InlineData("declared")][InlineData("actual")][InlineData("origin")][InlineData("dispose")]
    public void SC09_CallbackRevocation(string phase)
    {
        using var f=new Fixture();f.Provider.Value.Fast=false;
        void Revoke(string name){if(name==phase){if(phase=="lease")f.Provider.LeaseAvailable=false;else f.Provider.Available=false;if(phase=="owns")f.Provider.Authority.Owned.Remove(f.Provider.Target);}}
        f.Provider.Hook=Revoke;f.Provider.Value.OnRead=Revoke;f.Episode.Hook=Revoke;
        Assert.Throws<ObjectDisposedException>(()=>f.Session.Run([],op=>f.Construct(op)));Assert.Equal(f.Provider.Entries,f.Provider.Releases);
    }
    /// <summary>SC10 actual constructor overlap/reentrancy refuses and independent raw operations admit separately.</summary>
    [Fact] public async Task SC10_SerializedCallsAndIndependentOperations()
    {
        using var f=new Fixture();
        await f.Session.RunAsync([],async op=>
        {
            f.Provider.Hook=name=>{if(name=="construct")Assert.Throws<InvalidOperationException>(()=>f.Construct(op));};f.Construct(op);f.Provider.Hook=null;
            using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
            f.Provider.Hook=name=>{if(name=="construct"){entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));}};
            var tail=Task.Run(()=>f.Construct(op));
            try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.Throws<InvalidOperationException>(()=>f.Construct(op));}finally{resume.Set();}
            await tail;f.Provider.Hook=null;
            await f.Session.RunAsync([],inner=>Task.FromResult(f.Construct(inner).Kind));Assert.Equal(1,f.Provider.Releases);return 0;
        });
        Assert.Equal(2,f.Provider.Entries);Assert.Equal(2,f.Provider.Releases);Assert.Equal(3,f.Provider.Constructs);
    }
    /// <summary>SC12 constructor/work and admission cleanup failures retain primary object order.</summary>
    /// <param name="work">Whether callback fails after successful construction.</param>
    [Theory][InlineData(false)][InlineData(true)] public void SC12_ConstructorAndCleanupFailures(bool work)
    {using var f=new Fixture();var primary=new ApplicationException("primary");var cleanup=new ApplicationException("admission cleanup");f.Provider.Release=()=>throw cleanup;if(!work)f.Provider.Hook=name=>{if(name=="construct")throw primary;};var error=Assert.Throws<AggregateException>(()=>f.Session.Run<int>([],op=>{f.Construct(op);throw primary;}));Assert.Equal(new Exception[]{primary,cleanup},error.InnerExceptions);Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC12/SC37 iterator primary and cleanup exceptions preserve nested boundaries through admission cleanup.</summary>
    [Fact] public void SC12_SC37_IteratorCleanupFailures()
    {using var f=new Fixture();var c=f.Provider.Value;c.Fast=false;c.Fail="current";var iteratorCleanup=new ApplicationException("iterator cleanup");var admissionCleanup=new ApplicationException("admission cleanup");c.OnRead=name=>{if(name=="dispose")throw iteratorCleanup;};f.Provider.Release=()=>throw admissionCleanup;var error=Assert.Throws<AggregateException>(()=>f.Session.Run([],op=>f.Construct(op)));var inner=Assert.IsType<AggregateException>(error.InnerExceptions[0]);Assert.IsType<InvalidOperationException>(inner.InnerExceptions[0]);Assert.Same(iteratorCleanup,inner.InnerExceptions[1]);Assert.Same(admissionCleanup,error.InnerExceptions[1]);Assert.Equal(1,c.Disposals);Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC16 episode exceptions are preserved and later constants cannot drift.</summary>
    /// <param name="phase">First/later episode failure.</param>
    [Theory][InlineData("time")][InlineData("zone")][InlineData("later-ticks")][InlineData("later-zone")]
    public void SC16_SC17_EpisodeFailure(string phase)
    {using var f=new Fixture();var failure=new ApplicationException("episode");f.Session.Run([],op=>{if(phase.StartsWith("later")){f.Construct(op);if(phase=="later-ticks")f.Episode.Time=f.Episode.Time.AddTicks(1);else f.Episode.Zone++;Assert.Throws<ArgumentException>(()=>f.Construct(op));Assert.Equal(1,f.Provider.Constructs);}else{f.Episode.Hook=name=>{if(name==phase)throw failure;};Assert.Same(failure,Assert.Throws<ApplicationException>(()=>f.Construct(op)));Assert.Equal(0,f.Provider.Constructs);}return 0;});Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC14 a previously constructed lexical string is not a new lexical input.</summary>
    [Fact] public void SC14_ConstructedInputRefuses()
    {using var f=new Fixture();f.Provider.Value.Items=[new(XdmValue.FromString("42"),new Provenance(f.Provider.Value,f.Provider.Target,f.Provider.Actual,null))];f.Session.Run([],op=>{var value=f.Construct(op).Value;Assert.Equal(SchemaResultKind.Unsupported,f.Construct(op,value).Kind);Assert.Equal(1,f.Provider.Constructs);return 0;});}
    /// <summary>SC36 revocation during iterator cleanup prevents publication of otherwise valid evidence.</summary>
    [Fact] public void SC36_LateTypeRevocation()
    {using var f=new Fixture();f.Provider.Value.Fast=false;f.Provider.Value.OnRead=name=>{if(name=="dispose")f.Provider.Authority.Owned.Remove(f.Provider.Actual);};Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op)));Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC16 the provider receives immutable copied constants, never replayable caller getters.</summary>
    [Fact] public void SC16_ProviderUsesFrozenCopy()
    {using var f=new Fixture();var original=f.Episode.Time;f.Provider.Hook=name=>{if(name!="construct")return;f.Episode.Time=f.Episode.Time.AddDays(1);f.Episode.Hook=_=>throw new Exception("caller episode replay poison");Assert.Equal(original,f.Provider.LastEpisode!.CurrentDateTime);Assert.Equal(original,f.Provider.LastEpisode.CurrentDateTime);Assert.Equal(60,f.Provider.LastEpisode.ImplicitTimezoneOffsetMinutes);};f.Session.Run([],op=>{Assert.Equal(SchemaResultKind.Supported,f.Construct(op).Kind);return 0;});Assert.Equal(1,f.Episode.TimeReads);Assert.Equal(1,f.Episode.ZoneReads);}
    /// <summary>SC23 malformed iterator boundaries and lying scalar cells cannot produce receipts.</summary>
    /// <param name="phase">Malformed boundary.</param>
    [Theory][InlineData("items")][InlineData("enumerator")][InlineData("fast-default")]
    public void SC23_MalformedContainers(string phase)
    {using var f=new Fixture();if(phase=="fast-default")f.Provider.Value.InvalidOut=true;else{f.Provider.Value.Fast=false;f.Provider.Value.NullAt=phase;}Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op)));Assert.Equal(1,f.Provider.Releases);}
    /// <summary>SC26 owned handles still need positive target equality for governing and declared types.</summary>
    /// <param name="governing">Change governing rather than declared target.</param>
    [Theory][InlineData(true)][InlineData(false)] public void SC26_OwnedWrongTarget(bool governing)
    {using var f=new Fixture();if(governing)f.Provider.Value.Governing=f.Provider.Actual;else((Provenance)f.Provider.Value.Items[0].TypeEvidence).Declared=f.Provider.Actual;Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op)));}
    /// <summary>SC38 unavailable compiler options refuse before provider binding.</summary>
    [Fact] public void SC38_Options()
    {var p=new Provider();Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("$constructed",new(2,1,4,128,128),new CompileOptions{SchemaSet=new System.Xml.Schema.XmlSchemaSet()},p).Kind);Assert.Equal(0,p.Binds);}
    /// <summary>SC14/SC23 runtime null strings cannot cross lexical or constructed payload boundaries.</summary>
    /// <param name="input">Malformed lexical input rather than provider result.</param>
    [Theory][InlineData(true)][InlineData(false)] public void SC14_SC23_NullString(bool input)
    {using var f=new Fixture();if(!input)f.Provider.Value.Items=[new(XdmValue.FromString(null!),new Provenance(f.Provider.Value,f.Provider.Target,f.Provider.Actual,null))];Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>f.Construct(op,input?XdmValue.FromString(null!):null)));Assert.Equal(input?0:1,f.Provider.Entries);Assert.Equal(input?0:1,f.Provider.Releases);}

    /// <summary>PC01 original construction receipts expose cached types only during their raw operation.</summary>
    /// <param name="kind">Constructed primitive carrier.</param>
    [Theory][InlineData("string")][InlineData("integer")][InlineData("decimal")][InlineData("boolean")]
    public void PC01_ObserveConstructed(string kind)
    {
        using var f=new Fixture();var p=f.Provider;var plain=kind switch{"string"=>XdmValue.FromString("x"),"decimal"=>XdmValue.FromDecimal(2.30m),"boolean"=>XdmValue.FromBoolean(true),_=>XdmValue.FromInteger(42)};
        p.Value.Items=[new(plain,new Provenance(p.Value,p.Target,p.Actual,null))];ISchemaAtomicProvenance? retained=null;
        var value=f.Session.Run([],op=>
        {
            var result=f.Construct(op).Value;p.Hook=_=>throw new Exception("provider replay");p.Value.OnRead=_=>throw new Exception("result replay");
            try{Assert.True(SchemaAtomics.TryGetProvenance(in result,out retained));Assert.Same(p.Target,retained!.DeclaredType);Assert.Same(p.Actual,retained.ActualType);Assert.Null(retained.OriginalAssessment);}
            finally{p.Hook=null;p.Value.OnRead=null;}
            return result;
        });
        Assert.False(SchemaAtomics.TryGetProvenance(in value,out var expired));Assert.Null(expired);
        Assert.Throws<ObjectDisposedException>(()=>retained!.ActualType);Assert.Equal(1,p.Entries);Assert.Equal(1,p.Releases);
    }

    /// <summary>PC02 raw cancellation/closure invalidates try results and every constructed evidence getter.</summary>
    /// <param name="cancel">Whether raw cancellation rather than session closure triggers expiry.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void PC02_ConstructedExpiry(bool cancel)
    {
        using var f=new Fixture();using var cancellation=new CancellationTokenSource();
        f.Session.Run([],op=>
        {
            var value=f.Construct(op).Value;Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));
            if(cancel)cancellation.Cancel();else f.Session.Dispose();
            Assert.False(SchemaAtomics.TryGetProvenance(in value,out var expired));Assert.Null(expired);
            foreach(var getter in new Func<object?>[]{()=>evidence!.DeclaredType,()=>evidence!.ActualType,()=>evidence!.OriginalAssessment})
            {var error=Record.Exception(()=>getter());if(cancel)Assert.IsAssignableFrom<OperationCanceledException>(error);else Assert.IsType<ObjectDisposedException>(error);}
            return 0;
        },cancellation.Token);
    }

    /// <summary>PC03 retained/forged/rebound constructed transport cannot manufacture observations.</summary>
    /// <param name="mode">Invalid carrier shape.</param>
    [Theory][InlineData("tampered")][InlineData("forged")][InlineData("plain")]
    public void PC03_ConstructedForgery(string mode)
    {
        using var f=new Fixture();var saved=f.Session.Run([],op=>
        {
            var actual=f.Construct(op).Value;
            var value=mode=="plain"?XdmValue.FromInteger(42):XdmValue.SchemaTransportAttach(XdmValue.FromInteger(mode=="tampered"?43:42),mode=="forged"?new object():XdmValue.SchemaTransportMarker(actual)!);
            Assert.False(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Null(evidence);return actual;
        });
        f.Session.Run([],op=>{Assert.False(SchemaAtomics.TryGetProvenance(in saved,out _));Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(saved)));return 0;});
    }

}
