// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies bounded public original-node nilled execution and refusal boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; public original-node nilled controls (1.63.0).                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Attribute guard fanout and qualify adjacent proof sharing (1.63.1). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Share guarded fixture and retain separate data-profile refusal boundaries (1.64.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;
using Authority=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Authority;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Origin=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Origin;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies bounded public original-node nilled execution and refusal boundaries.</summary>
/// <param name="output">Records local allocation/callback observations.</param>
public sealed class PublicOriginalNilledTests(ITestOutputHelper output)
{
    internal sealed class Source:ISchemaAssessmentInput,IQueryOperationLifetime
    {
        internal IXdmNode Node=null!;internal IXdmNode? AdditionalNode;internal bool Available=true,Owned=true,EnterFails;internal int Releases;
        internal Action<string>? Hook;
        IXdmNode ISchemaAssessmentInput.Root=>throw new Exception("root poison");
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>BindingSource;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>BindingSnapshot;
        private readonly SourceBinding BindingSource=new();private readonly SnapshotBinding BindingSnapshot=new();
        internal sealed class SourceBinding:ISchemaSourceBinding { }
        private sealed class SnapshotBinding:ISchemaSnapshotBinding { }
        internal bool ClaimsNode(IXdmNode node)=>ReferenceEquals(node,Node)||ReferenceEquals(node,AdditionalNode);
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node){Hook?.Invoke("owns");return Owned&&ClaimsNode(node);}
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){if(EnterFails)throw new InvalidOperationException("enter");return new Lease(this);}
        private sealed class Lease(Source owner):IQueryOperationLease
        {
            private bool _closed;
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(_closed,this);}
            /// <inheritdoc/>
            public void Dispose(){if(!_closed){_closed=true;owner.Releases++;}}
        }
    }
    internal sealed class Facts:ISchemaNodeFacts
    {
        internal ISchemaComponentHandle Type=null!;internal ISchemaComponentHandle? Declaration;
        internal ISchemaAssessmentOrigin Origin=null!;internal bool Absent,Nilled,TypedAllowed,NullOutcome;
        internal SchemaResultKind TypedKind=SchemaResultKind.Supported;internal ISchemaTypedValue? Value;internal Func<ISchemaTypedValue>? ValueFactory;internal Action<string>? OutcomeHook;
        internal readonly List<string> Reads=[];internal bool Record=true;internal Action<string>? Hook;
        private void Hit(string name){if(Record)Reads.Add(name);Hook?.Invoke(name);}
        ISchemaComponentHandle ISchemaNodeFacts.Type{get{Hit("type");return Type;}}
        ISchemaComponentHandle? ISchemaNodeFacts.Declaration{get{Hit("declaration");return Declaration;}}
        ISchemaAssessmentOrigin ISchemaNodeFacts.Origin{get{Hit("origin");return Origin;}}
        bool ISchemaNodeFacts.HasNoTypedValue{get{Hit("absent");return Absent;}}
        bool ISchemaNodeFacts.IsNilled{get{Hit("nilled");return Nilled;}}
        SchemaResult<ISchemaTypedValue> ISchemaNodeFacts.ReadTypedValue(CancellationToken token)
        {if(!TypedAllowed)throw new Exception("typed poison");Hit("typed");token.ThrowIfCancellationRequested();return NullOutcome?null!:new Outcome<ISchemaTypedValue>(TypedKind,ValueFactory is null?Value!:ValueFactory()){Hook=OutcomeHook};}
    }
    internal sealed class Node:ISchemaAnnotatedNode
    {
        internal XdmNodeKind Kind=XdmNodeKind.Element;internal ISchemaNodeFacts? Facts;
        internal Action<string>? Hook;internal readonly List<string> Reads=[];internal bool Record=true;
        private void Hit(string name){if(Record)Reads.Add(name);Hook?.Invoke(name);}
        XdmNodeKind IXdmNode.NodeKind{get{Hit("node-kind");return Kind;}}
        ISchemaNodeFacts ISchemaAnnotatedNode.OriginalSchemaFacts{get{Hit("facts");return Facts!;}}
        string IXdmNode.LocalName=>throw new Exception("name poison");
        string IXdmNode.NamespaceUri=>throw new Exception("namespace poison");
        string IXdmNode.Prefix=>throw new Exception("prefix poison");
        string IXdmNode.StringValue=>throw new Exception("string poison");
        XdmValue IXdmNode.TypedValue=>throw new Exception("legacy typed poison");
        bool IXdmNode.HasNoTypedValue=>throw new Exception("legacy absent poison");
        bool IXdmNode.IsNilled=>throw new Exception("legacy nilled poison");
        (string,string)? IXdmNode.SchemaTypeAnnotation=>throw new Exception("legacy type poison");
        IXdmNode? IXdmNode.Parent=>throw new Exception("parent poison");
        IXdmNode? IXdmNode.Document=>throw new Exception("document poison");
        XdmSequence IXdmNode.Children(XdmNodeKind kind)=>throw new Exception("children poison");
        XdmSequence IXdmNode.Attributes(string? localName,string? namespaceUri)=>throw new Exception("attributes poison");
        XdmSequence IXdmNode.Axis(XdmAxis axis)=>throw new Exception("axis poison");
        bool IXdmNode.IsSameNode(IXdmNode other)=>throw new Exception("identity poison");
        long IXdmNode.DocumentOrder=>throw new Exception("order poison");
        string IXdmNode.BaseUri=>throw new Exception("base poison");
        string IXdmNode.DocumentUri=>throw new Exception("uri poison");
        string IXdmNode.ToXmlString()=>throw new Exception("serialize poison");
    }
    internal sealed class Outcome<T>(SchemaResultKind kind,T value):SchemaResult<T>
    {
        /// <inheritdoc/>
        internal Action<string>? Hook;
        /// <inheritdoc/>
        public override SchemaResultKind Kind{get{Hook?.Invoke("kind");return kind;}}
        /// <inheritdoc/>
        public override T Value{get{Hook?.Invoke("payload");return kind==SchemaResultKind.Supported?value:throw new Exception("unsupported payload poison");}}
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    internal sealed class Provider:ISchemaAssessor,ISchemaAssessmentAdvisor
    {
        internal readonly Authority Authority=new();internal ISchemaAuthority Selected;
        internal ISchemaAssessor Endpoint;internal IQuerySemanticEnvironment Environment=null!;
        internal bool Available=true,FactsOwned=true,TupleOwned=true,OriginOwned=true,TypedAllowed;
        internal readonly HashSet<ISchemaTypedValue> Values=new(ReferenceEqualityComparer.Instance);
        internal Facts Issued=null!;internal Source Input=null!;internal Origin Origin=null!;
        internal int Binds,Calls;internal Action<string>? Hook;
        internal Func<SchemaResult<ISchemaAssessmentAdvisor>>? BindingOutcome;
        internal Provider(){Selected=Authority;Endpoint=this;}
        private void Hit(string name){Calls++;Hook?.Invoke(name);}
        ISchemaAuthority ISchemaAssessor.Authority{get{Hit("authority");return Selected;}}
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("language poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment,CancellationToken token)
        {Binds++;Environment=environment;return BindingOutcome is null?new Outcome<ISchemaAssessmentAdvisor>(SchemaResultKind.Supported,this):BindingOutcome();}
        ISchemaAssessor ISchemaAssessmentAdvisor.Assessor{get{Hit("endpoint");return Endpoint;}}
        IQuerySemanticEnvironment ISchemaAssessmentAdvisor.QueryEnvironment{get{Hit("environment");return Environment;}}
        void ISchemaAssessmentAdvisor.ThrowIfUnavailable(CancellationToken token){Calls++;token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        bool ISchemaAssessmentAdvisor.OwnsAssessment(ISchemaAssessmentOrigin origin){Hit("owns-origin");return OriginOwned&&ReferenceEquals(origin,Origin);}
        bool ISchemaAssessmentAdvisor.OwnsNodeFacts(ISchemaNodeFacts facts){Hit("owns-facts");return FactsOwned&&ReferenceEquals(facts,Issued);}
        bool ISchemaAssessmentAdvisor.OwnsNode(IXdmNode node,ISchemaNodeFacts facts,ISchemaAssessmentInput input,ISchemaAssessmentOrigin origin)
        {Hit("owns-tuple");return TupleOwned&&Input.ClaimsNode(node)&&ReferenceEquals(facts,Issued)&&ReferenceEquals(input,Input)&&ReferenceEquals(origin,Origin);}
        bool ISchemaAssessmentAdvisor.OwnsTypedValue(ISchemaTypedValue value,ISchemaAssessmentInput input,ISchemaAssessmentOrigin? origin)
        {if(!TypedAllowed)throw new Exception("typed issuance poison");Hit("owns-value");return Values.Contains(value)&&ReferenceEquals(input,Input)&&ReferenceEquals(origin,Origin);}
        ISchemaAssessmentResult ISchemaAssessmentAdvisor.Assess(SchemaAssessmentRequest request,CancellationToken token)=>throw new Exception("assess poison");
        SchemaResult<ISchemaTypedValue> ISchemaAssessmentAdvisor.ConstructSimpleValue(XdmValue input,ISchemaComponentHandle type,ISchemaAssessmentEpisode episode,CancellationToken token)=>throw new Exception("construct poison");
    }
    internal sealed class Fixture:IDisposable
    {
        internal readonly Source Input=new();internal readonly Node Node=new();internal readonly Facts Facts=new();
        internal readonly Provider Provider=new();internal readonly Origin Origin=new();internal readonly Handle Type=new(),Declaration=new();
        internal readonly Session Session;internal readonly SchemaQuerySnapshot? Snapshot;
        internal Fixture(string expression="nilled($node)",bool open=true,bool original=true,CompileOptions? options=null,SchemaQueryLimits? limits=null)
        {
            Input.Node=Node;Node.Facts=Facts;Facts.Type=Type;Facts.Declaration=Declaration;Facts.Origin=Origin;
            Provider.Input=Input;Provider.Issued=Facts;Provider.Origin=Origin;Provider.Authority.Types.UnionWith([Type,Declaration]);
            Session=Session.CompileXPath(expression,limits??new(4,8,8,128,128),options,Provider).Value;
            if(open)Snapshot=Session.OpenSnapshot(Input,Input,original?Origin:null).Value;
        }
        internal XdmValue Evaluate(XdmValue? value=null)=>Session.Run(Snapshot is null?[]:[Snapshot],op=>op.Evaluate(Context(value??XdmValue.FromNode(Input.Node))));
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    private static EvaluationContext Context(XdmValue node)=>EvaluationContext.CreateIsolated().WithVariable("node",node);
    private sealed class PoisonSequence:IXdmSequence
    {
        bool IXdmSequence.TryGetLength(out int length)=>throw new Exception("length poison");
        IXdmSequenceEnumerator IXdmSequence.GetEnumerator()=>throw new Exception("iterator poison");
    }
    /// <summary>NP01-NP02 returns copied Boolean from both canonical spellings without typed/legacy callbacks.</summary>
    /// <param name="expression">Selected spelling.</param><param name="nilled">Original nilled value.</param>
    [Theory][InlineData("nilled($node)",true)][InlineData("fn:nilled($node)",false)]
    public void NP01_NP02_OriginalBoolean(string expression,bool nilled)
    {using var f=new Fixture(expression);f.Facts.Nilled=nilled;var result=f.Evaluate();Assert.Equal(nilled,result.BooleanValue);Assert.Null(XdmValue.SchemaTransportMarker(result));Assert.Equal(OriginalNodeNilledPlan.Profile,f.Session.Environment.BehaviorProfile);Assert.Equal(OriginalNodeNilledPlan.Descriptor,f.Session.Environment.DescriptorFormatVersion);Assert.Equal(new[]{"type","declaration","origin","absent","nilled"},f.Facts.Reads);}
    /// <summary>NP03 empty input requires live advisor even without snapshots.</summary>
    [Fact] public void NP03_EmptyChecksAdvisor()
    {using var f=new Fixture(open:false);Assert.True(f.Evaluate(XdmValue.Undefined).IsUndefined);f.Provider.Available=false;Assert.Throws<ObjectDisposedException>(()=>f.Evaluate(XdmValue.Undefined));Assert.Empty(f.Node.Reads);}
    /// <summary>NP04 known non-elements require ownership and kind only.</summary>
    /// <param name="kind">Owned non-element kind.</param>
    [Theory][InlineData(XdmNodeKind.Document)][InlineData(XdmNodeKind.Attribute)][InlineData(XdmNodeKind.Text)][InlineData(XdmNodeKind.Comment)][InlineData(XdmNodeKind.ProcessingInstruction)][InlineData(XdmNodeKind.Namespace)]
    public void NP04_NonElements(XdmNodeKind kind)
    {using var f=new Fixture(original:false);f.Node.Kind=kind;Assert.True(f.Evaluate().IsUndefined);Assert.Equal(new[]{"node-kind"},f.Node.Reads);Assert.Empty(f.Facts.Reads);}
    /// <summary>NP05-NP07 closed capture refuses bindings, shapes, focus, namespaces and services before node getters.</summary>
    /// <param name="mode">Unsupported context state.</param>
    [Theory][InlineData("missing")][InlineData("extra")][InlineData("prefixed")][InlineData("integer")][InlineData("sequence")][InlineData("focus")][InlineData("namespace")][InlineData("service")][InlineData("function")]
    public void NP05_NP07_CaptureRefusal(string mode)
    {
        using var f=new Fixture();var c=Context(XdmValue.FromNode(f.Node));
        switch(mode){case "missing":c=EvaluationContext.CreateIsolated();break;case "extra":c.WithVariable("extra",XdmValue.Undefined);break;
            case "prefixed":c=EvaluationContext.CreateIsolated().WithVariable("node",XdmValue.FromNode(f.Node),"urn:n");break;
            case "integer":c=Context(XdmValue.FromInteger(1));break;case "sequence":c=Context(XdmValue.FromSequence(XdmSequence.FromSource(new PoisonSequence())));break;
            case "focus":c.WithFocus(XdmValue.FromNode(f.Node),1,1);break;case "namespace":c.WithNamespace("p","urn:p");break;
            case "service":c.DocumentLoader=_=>throw new Exception("loader poison");break;default:c.SkipStandardFunctionPopulation=true;break;}
        f.Session.Run([f.Snapshot!],op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(c));return 0;});Assert.Empty(f.Node.Reads);
    }
    /// <summary>NP08 foreign and ambiguously owned nodes refuse before NodeKind.</summary>
    /// <param name="ambiguous">Two admitted sources claim the node.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NP08_ForeignOwnership(bool ambiguous)
    {using var f=new Fixture();var other=new Source{Node=f.Node};if(!ambiguous)f.Input.Owned=false;SchemaQuerySnapshot? second=ambiguous?f.Session.OpenSnapshot(other,other).Value:null;f.Session.Run(second is null?[f.Snapshot!]:[f.Snapshot!,second],op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Node))));return 0;});Assert.Empty(f.Node.Reads);}
    /// <summary>NP09 missing element annotation/original assessment refuses without legacy fallback.</summary>
    /// <param name="annotation">Remove annotation instead of origin.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NP09_MissingOriginal(bool annotation)
    {using var f=new Fixture(original:annotation);if(annotation)f.Input.Node=new XDocumentNode(XElement.Parse("<n/>"));Assert.Throws<NotSupportedException>(()=>f.Evaluate());Assert.Empty(f.Facts.Reads);}
    /// <summary>NP10 unissued, null or copied facts refuse before metadata access.</summary>
    /// <param name="mode">Invalid facts channel.</param>
    [Theory][InlineData("null")][InlineData("unissued")][InlineData("copy")][InlineData("tuple")]
    public void NP10_FactsIssuance(string mode)
    {using var f=new Fixture();switch(mode){case "null":f.Node.Facts=null;break;case "unissued":f.Provider.FactsOwned=false;break;case "copy":f.Node.Facts=new Facts{Type=f.Type,Origin=f.Origin};break;default:f.Provider.TupleOwned=false;break;}Assert.Throws<ArgumentException>(()=>f.Evaluate());Assert.Empty(f.Facts.Reads);}
    /// <summary>NP11 original component/origin and bound authority/environment remain exact.</summary>
    /// <param name="mode">Foreign evidence.</param>
    [Theory][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("authority")][InlineData("environment")]
    public void NP11_ForeignEvidence(string mode)
    {using var f=new Fixture();switch(mode){case "type":f.Facts.Type=new Handle();break;case "declaration":f.Facts.Declaration=new Handle();break;case "origin":f.Facts.Origin=new Origin();break;case "authority":f.Provider.Selected=new Authority();break;default:using(var other=new Fixture())f.Provider.Environment=other.Session.Environment;break;}Assert.Throws<ArgumentException>(()=>f.Evaluate());}
    /// <summary>NP12-NP13 callback cancellation/revocation prevents delivery and defers cleanup until raw completion.</summary>
    /// <param name="stage">Interrupted callback.</param><param name="mode">Cancellation, close or advisor revocation.</param>
    /// <returns>A task completing after raw work drains.</returns>
    [Theory][InlineData("node-kind","cancel")][InlineData("facts","cancel")][InlineData("type","cancel")][InlineData("declaration","cancel")][InlineData("origin","cancel")][InlineData("absent","cancel")][InlineData("nilled","cancel")]
    [InlineData("node-kind","revoke")][InlineData("facts","revoke")][InlineData("nilled","revoke")][InlineData("owns","close")][InlineData("owns-false","close")][InlineData("owns-tuple","cancel")][InlineData("owns-facts","close")]
    public async Task NP12_NP13_CallbackInterruption(string stage,string mode)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();
        void Interrupt(string current){if(current!=stage&&!(current=="owns"&&stage=="owns-false"))return;if(stage=="owns-false")f.Input.Owned=false;if(mode=="cancel")cancel.Cancel();else if(mode=="close")f.Session.Dispose();else f.Provider.Available=false;}
        f.Node.Hook=Interrupt;f.Facts.Hook=Interrupt;f.Input.Hook=Interrupt;f.Provider.Hook=Interrupt;
        f.Session.Run([f.Snapshot!],op=>{if(mode=="cancel")Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Node))));else Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(Context(XdmValue.FromNode(f.Node))));Assert.Equal(0,f.Input.Releases);Assert.False(f.Session.DrainAsync().IsCompleted);return 0;},cancel.Token);
        await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(5));if(mode=="close")Assert.Equal(1,f.Input.Releases);
        if(stage is "node-kind" or "facts")Assert.Equal(stage,f.Node.Reads[^1]);
        if(stage is "type" or "declaration" or "origin" or "absent" or "nilled")Assert.Equal(stage,f.Facts.Reads[^1]);
    }
    /// <summary>NP14 poisoned typed/legacy getters and type-family lookup are unnecessary even for absent typed values.</summary>
    [Fact] public void NP14_TypedPoison()
    {using var f=new Fixture();f.Facts.Absent=true;Assert.False(f.Evaluate().BooleanValue);Assert.DoesNotContain("typed",f.Facts.Reads);}
    /// <summary>NP15 metadata-only prerequisite remains explicit and complete reads keep their original default.</summary>
    [Fact] public void NP15_MetadataState()
    {Assert.Equal(3,(int)QueryNodeTypedValueKind.NotRequested);Assert.Equal(0,(int)QueryNodeTypedValueKind.Absent);Assert.Equal(1,(int)QueryNodeTypedValueKind.Unsupported);Assert.Equal(2,(int)QueryNodeTypedValueKind.Supported);using var f=new Fixture();Assert.False(f.Evaluate().BooleanValue);}
    /// <summary>NP16-NP17/NP24 unsupported syntax and foreign fn bindings refuse before Bind; data has a separate profile.</summary>
    /// <param name="expression">Unsupported syntax.</param><param name="foreign">Foreign fn binding.</param>
    [Theory][InlineData("nilled()",false)][InlineData("nilled(.)",false)][InlineData("data($node)/x",false)][InlineData("$node/n",false)][InlineData("nilled($node) or true()",false)][InlineData("f:nilled($node)",false)][InlineData("Q{http://www.w3.org/2005/xpath-functions}nilled($node)",false)][InlineData("fn:nilled($node)",true)]
    public void NP16_NP17_NP24_CompileRefusal(string expression,bool foreign)
    {var provider=new Provider();var opts=foreign?new CompileOptions{Namespaces=new Dictionary<string,string>{{"fn","urn:foreign"}}}:null;var result=Session.CompileXPath(expression,new(4,8,8,128,128),opts,provider);Assert.Equal(SchemaResultKind.Unsupported,result.Kind);Assert.Equal(0,provider.Binds);}
    /// <summary>NP18 canonical spelling shares a descriptor; each limit and frozen option mutation is distinguished.</summary>
    [Fact] public void NP18_DescriptorsAndFrozenOptions()
    {
        using var a=new Fixture();using var b=new Fixture("fn:nilled($node)");Assert.Equal(a.Session.Environment.StaticContextFingerprint,b.Session.Environment.StaticContextFingerprint);
        foreach(var limits in new[]{new SchemaQueryLimits(5,8,8,128,128),new(4,9,8,128,128),new(4,8,9,128,128),new(4,8,8,129,128),new(4,8,8,128,127)})
        {using var f=new Fixture(limits:limits);Assert.NotEqual(a.Session.Environment.StaticContextFingerprint,f.Session.Environment.StaticContextFingerprint);}
        var ns=new Dictionary<string,string>{{"p","urn:one"}};using var frozen=new Fixture(options:new CompileOptions{Namespaces=ns});string fingerprint=frozen.Session.Environment.StaticContextFingerprint;ns["p"]="urn:two";Assert.Equal(fingerprint,frozen.Session.Environment.StaticContextFingerprint);Assert.Equal("urn:one",frozen.Session.Environment.StaticContext.Namespaces["p"]);Assert.False(frozen.Evaluate().BooleanValue);
    }
    /// <summary>NP19 copied results survive close and asynchronous evaluation retains no provider view.</summary>
    /// <returns>A task completing after the public asynchronous run.</returns>
    [Fact] public async Task NP19_PlainResultsAsync()
    {using var f=new Fixture();f.Facts.Nilled=true;var result=await f.Session.RunAsync([f.Snapshot!],async op=>{await Task.Yield();return op.Evaluate(Context(XdmValue.FromNode(f.Node)));});f.Session.Dispose();f.Provider.Available=false;Assert.True(result.BooleanValue);Assert.Null(XdmValue.SchemaTransportMarker(result));using var empty=new Fixture(open:false);var missing=empty.Evaluate(XdmValue.Undefined);empty.Session.Dispose();Assert.True(missing.IsUndefined);}
    /// <summary>NP20 binding outcomes preserve unsupported/malformed distinctions and admission rollback permits retry.</summary>
    /// <param name="kind">Selected provider binding outcome.</param>
    [Theory][InlineData(SchemaResultKind.Unsupported)][InlineData(SchemaResultKind.Invalid)][InlineData(SchemaResultKind.Supported)]
    public void NP20_BindingAndRollback(SchemaResultKind kind)
    {
        var provider=new Provider{BindingOutcome=()=>new Outcome<ISchemaAssessmentAdvisor>(kind,null!)};
        if(kind==SchemaResultKind.Unsupported)Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("nilled($node)",new(4,8,8,128,128),assessor:provider).Kind);
        else Assert.Throws<ArgumentException>(()=>Session.CompileXPath("nilled($node)",new(4,8,8,128,128),assessor:provider));
        using var f=new Fixture(open:false);f.Input.EnterFails=true;Assert.Equal("enter",Assert.Throws<InvalidOperationException>(()=>f.Session.OpenSnapshot(f.Input,f.Input,f.Origin)).Message);f.Input.EnterFails=false;using var snapshot=f.Session.OpenSnapshot(f.Input,f.Input,f.Origin).Value;Assert.False(f.Session.Run([snapshot],op=>op.Evaluate(Context(XdmValue.FromNode(f.Node)))).BooleanValue);
    }
    /// <summary>NP20 provider binding null/throw retains the original failure without acquiring an input.</summary>
    /// <param name="throwing">Throw rather than return a missing outcome.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NP20_NullAndThrowingBinding(bool throwing)
    {var provider=new Provider{BindingOutcome=()=>throwing?throw new InvalidOperationException("bind"):null!};if(throwing)Assert.Equal("bind",Assert.Throws<InvalidOperationException>(()=>Session.CompileXPath("nilled($node)",new(4,8,8,128,128),assessor:provider)).Message);else Assert.Throws<ArgumentException>(()=>Session.CompileXPath("nilled($node)",new(4,8,8,128,128),assessor:provider));Assert.Equal(1,provider.Binds);}
    /// <summary>NP03-NP04 early returns still honor raw cancellation and advisor callback revocation.</summary>
    /// <param name="empty">Evaluate empty rather than a non-element.</param>
    [Theory][InlineData(false)][InlineData(true)] public void NP03_NP04_EarlyReturnCancellation(bool empty)
    {using var f=new Fixture();using var cancel=new CancellationTokenSource();f.Node.Kind=XdmNodeKind.Text;f.Provider.Hook=_=>cancel.Cancel();f.Session.Run([f.Snapshot!],op=>{Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(empty?XdmValue.Undefined:XdmValue.FromNode(f.Node))));return 0;},cancel.Token);Assert.Empty(f.Facts.Reads);}
    /// <summary>NP12 original metadata getter failures propagate and leave no cached partial facts on retry.</summary>
    /// <param name="stage">Throwing original getter.</param>
    [Theory][InlineData("node-kind")][InlineData("facts")][InlineData("type")][InlineData("declaration")][InlineData("origin")][InlineData("absent")][InlineData("nilled")]
    public void NP12_GetterFailureRetry(string stage)
    {using var f=new Fixture();void Fail(string name){if(name==stage)throw new InvalidOperationException(name);}f.Node.Hook=Fail;f.Facts.Hook=Fail;Assert.Equal(stage,Assert.Throws<InvalidOperationException>(()=>f.Evaluate()).Message);f.Node.Hook=null;f.Facts.Hook=null;Assert.False(f.Evaluate().BooleanValue);}
    /// <summary>NP21 malformed nilled/absence combinations and invalid kind are rejected.</summary>
    /// <param name="mode">Malformed metadata shape.</param>
    [Theory][InlineData("absent-nilled")][InlineData("none")][InlineData("all")]
    public void NP21_MalformedMetadata(string mode)
    {using var f=new Fixture();if(mode=="absent-nilled"){f.Facts.Absent=true;f.Facts.Nilled=true;}else f.Node.Kind=mode=="none"?XdmNodeKind.None:XdmNodeKind.All;Assert.Throws<ArgumentException>(()=>f.Evaluate());}
    /// <summary>NP22 other profile descriptors/refusals remain distinct; nilled does not admit typed scalar registration.</summary>
    [Fact] public void NP22_OtherProfiles()
    {using var f=new Fixture();using var ordinary=Session.CompileXPath("true()",new(4,8,8,128,128)).Value;Assert.NotEqual(ordinary.Environment.BehaviorProfile,f.Session.Environment.BehaviorProfile);f.Session.Run([f.Snapshot!],op=>{Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(f.Snapshot!,new QueryTypedContainerTests.Container()).Kind);return 0;});}
    /// <summary>NP23 public cold/warm observations retain no facts cache and record allocation/callback cost.</summary>
    [Fact] public void NP23_AllocationAndCallbacks()
    {
        using var f=new Fixture();var c=Context(XdmValue.FromNode(f.Node));f.Session.Run([f.Snapshot!],op=>{
            for(int i=0;i<10;i++)_=op.Evaluate(c);long before=GC.GetAllocatedBytesForCurrentThread();int reads=f.Facts.Reads.Count,calls=f.Provider.Calls;
            for(int i=0;i<100;i++)Assert.False(op.Evaluate(c).BooleanValue);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/100;
            output.WriteLine($"Public nilled warm: {bytes} bytes/query; provider callbacks: {(f.Provider.Calls-calls)/100}; metadata reads: {(f.Facts.Reads.Count-reads)/100}");Assert.Equal(500,f.Facts.Reads.Count-reads);Assert.InRange(bytes,1,8192);Assert.InRange((f.Provider.Calls-calls)/100,1,10000);return 0;});
        long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<20;i++)_=f.Evaluate();output.WriteLine($"Public nilled cold operation: {(GC.GetAllocatedBytesForCurrentThread()-start)/20} bytes/query");
    }
    /// <summary>Attributes public guard fanout without caching provider answers across queries.</summary>
    [Fact] public void GuardStageAttribution()
    {
        using var f=new Fixture();var counts=new Dictionary<string,int>{{"endpoint",0},{"environment",0},{"authority",0},{"owns-origin",0},{"owns-facts",0},{"owns-tuple",0}};
        f.Provider.Hook=name=>counts[name]++;var context=Context(XdmValue.FromNode(f.Node));
        f.Session.Run([f.Snapshot!],op=>{for(int i=0;i<10;i++)_=op.Evaluate(context);foreach(var name in counts.Keys.ToArray())counts[name]=0;int calls=f.Provider.Calls;
            for(int i=0;i<100;i++)_=op.Evaluate(context);
            int availability=f.Provider.Calls-calls-counts.Values.Sum();
            output.WriteLine("Public nilled stages: "+string.Join(",",counts.Select(pair=>$"{pair.Key}={pair.Value/100}"))+$",availability={availability/100}");
            Assert.All(counts.Values,count=>Assert.True(count>0));Assert.True(availability>0);return 0;});
    }
    /// <summary>Shared live boundaries still stop every binding/issuance callback on revocation or cancellation.</summary>
    /// <param name="stage">Provider callback.</param><param name="mode">Interruption or callback failure.</param>
    [Theory]
    [InlineData("endpoint","cancel")][InlineData("environment","cancel")][InlineData("authority","cancel")][InlineData("owns-origin","cancel")][InlineData("owns-facts","cancel")][InlineData("owns-tuple","cancel")]
    [InlineData("endpoint","revoke")][InlineData("environment","revoke")][InlineData("authority","revoke")][InlineData("owns-origin","revoke")][InlineData("owns-facts","revoke")][InlineData("owns-tuple","revoke")]
    [InlineData("endpoint","throw")][InlineData("environment","throw")][InlineData("authority","throw")][InlineData("owns-origin","throw")][InlineData("owns-facts","throw")][InlineData("owns-tuple","throw")]
    [InlineData("owns-origin","false-revoke")][InlineData("owns-facts","false-revoke")][InlineData("owns-tuple","false-revoke")]
    public void SharedBindingBoundaryInterruption(string stage,string mode)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();
        f.Provider.Hook=name=>{if(name!=stage)return;if(mode=="throw")throw new InvalidOperationException(stage);
            if(mode=="cancel")cancel.Cancel();else f.Provider.Available=false;
            if(mode=="false-revoke"){if(stage=="owns-origin")f.Provider.OriginOwned=false;else if(stage=="owns-facts")f.Provider.FactsOwned=false;else f.Provider.TupleOwned=false;}};
        f.Session.Run([f.Snapshot!],op=>{var context=Context(XdmValue.FromNode(f.Node));
            if(mode=="cancel")Assert.Throws<OperationCanceledException>(()=>op.Evaluate(context));
            else if(mode=="throw")Assert.Equal(stage,Assert.Throws<InvalidOperationException>(()=>op.Evaluate(context)).Message);
            else Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(context));return 0;},cancel.Token);
        Assert.Empty(f.Facts.Reads);
    }
    /// <summary>Original type/declaration callbacks cannot revoke evidence inside a shared live boundary.</summary>
    /// <param name="declaration">Target the declaration instead of type.</param><param name="mode">Revoked capability.</param>
    [Theory][InlineData(false,"cancel")][InlineData(true,"cancel")][InlineData(false,"tuple")][InlineData(true,"tuple")]
    [InlineData(false,"source")][InlineData(true,"source")][InlineData(false,"false-source")][InlineData(true,"false-source")]
    public void SharedAuthorityBoundaryInterruption(bool declaration,string mode)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();var target=declaration?f.Declaration:f.Type;
        f.Provider.Authority.OnOwn=type=>{if(!ReferenceEquals(type,target))return;
            if(mode=="cancel")cancel.Cancel();else if(mode=="tuple")f.Provider.TupleOwned=false;
            else{f.Input.Available=false;if(mode=="false-source")f.Provider.Authority.Types.Remove(type);}};
        f.Session.Run([f.Snapshot!],op=>{var context=Context(XdmValue.FromNode(f.Node));
            if(mode=="cancel")Assert.Throws<OperationCanceledException>(()=>op.Evaluate(context));
            else if(mode=="tuple")Assert.Throws<ArgumentException>(()=>op.Evaluate(context));
            else Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(context));return 0;},cancel.Token);
        Assert.DoesNotContain("nilled",f.Facts.Reads);
    }
}
