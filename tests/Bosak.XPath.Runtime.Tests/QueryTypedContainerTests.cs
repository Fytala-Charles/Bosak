// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies uncached typed-container issuer, enumeration and raw-lifetime boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; typed-container controls (1.53.0)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections;
using System.Collections.Immutable;
using System.Numerics;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises the internal container reader through actual source-session raw leases.</summary>
/// <param name="output">Records local cold-read allocation observations.</param>
public sealed class QueryTypedContainerTests(ITestOutputHelper output)
{
    private sealed class Input : IQuerySourceInput,ISchemaAssessmentInput
    {
        internal bool Available=true;
        internal int Roots,Releases;
        IXdmNode IQuerySourceInput.Root {get{Roots++;throw new Exception("root");}}
        IXdmNode ISchemaAssessmentInput.Root {get{Roots++;throw new Exception("public root");}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>throw new Exception("source getter");
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>throw new Exception("snapshot getter");
        void IQuerySourceInput.ThrowIfUnavailable()=>Check();
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();Check();}
        bool IQuerySourceInput.OwnsNode(IXdmNode node)=>false;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>false;
        internal void Check()=>ObjectDisposedException.ThrowIf(!Available,this);
        internal IDisposable Enter()=>new Lease(this);
        private sealed class Lease(Input input):IDisposable
        {
            private int _closed;
            /// <inheritdoc/>
            public void Dispose(){if(Interlocked.Exchange(ref _closed,1)==0)input.Releases++;}
        }
    }
    private sealed class Origin : ISchemaAssessmentOrigin
    {ISchemaSourceBinding ISchemaAssessmentOrigin.Source=>throw new Exception("origin source getter");}
    private sealed class Handle : ISchemaComponentHandle
    {ISchemaAuthority ISchemaComponentHandle.Authority=>throw new Exception("self-claimed authority getter");}
    private sealed class Authority : ISchemaAuthority
    {
        internal bool Available=true;
        internal readonly HashSet<ISchemaComponentHandle> Types=new(ReferenceEqualityComparer.Instance);
        bool ISchemaAuthority.Owns(ISchemaComponentHandle type)=>Types.Contains(type);
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle a,CancellationToken t)=>throw new Exception("lookup");
    }
    private sealed class Issuer(SchemaQueryEnvironment environment,Input input,Authority authority,Origin origin) : IQueryTypedContainerIssuer
    {
        internal SchemaQueryEnvironment Env=environment;
        internal ISchemaAuthority SelectedAuthority=authority;
        internal bool Available=true,InputOwned=true,OriginOwned=true;
        internal readonly HashSet<ISchemaTypedValue> Values=new(ReferenceEqualityComparer.Instance);
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment=>Env;
        ISchemaAuthority IQueryTypedContainerIssuer.Authority=>SelectedAuthority;
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available||!authority.Available,this);}
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput original,IQuerySourceInput current)=>InputOwned&&ReferenceEquals(original,input)&&ReferenceEquals(current,input);
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin o,ISchemaAssessmentInput original,IQuerySourceInput current)=>OriginOwned&&ReferenceEquals(o,origin)&&ReferenceEquals(original,input)&&ReferenceEquals(current,input);
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput original,IQuerySourceInput current,ISchemaAssessmentOrigin? o)
            =>Values.Contains(value)&&ReferenceEquals(original,input)&&ReferenceEquals(current,input)&&(o is null||ReferenceEquals(o,origin));
    }
    private sealed class Provenance(Container container,ISchemaComponentHandle declared,ISchemaComponentHandle actual,ISchemaAssessmentOrigin? origin):ISchemaAtomicProvenance
    {
        internal ISchemaComponentHandle Declared=declared,Actual=actual;
        internal ISchemaAssessmentOrigin? Origin=origin;
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType {get{container.Trace("declared");return Declared;}}
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType {get{container.Trace("actual");return Actual;}}
        ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment {get{container.Trace("origin");return Origin;}}
    }
    private sealed class Container : ISchemaTypedValue,IEnumerable<SchemaAtomicItem>
    {
        internal ISchemaComponentHandle Governing=null!;
        internal bool List,Fast,InvalidOut,Record=true;
        internal SchemaAtomicItem[] Items=[];
        internal readonly List<string> Reads=[];
        internal string? Fail,NullAt;
        internal Action<string>? OnRead;
        internal int Currents,Disposals,ForbiddenCurrent=int.MaxValue;
        internal void Trace(string stage){if(Record)Reads.Add(stage);OnRead?.Invoke(stage);if(Fail==stage)throw new InvalidOperationException(stage);}
        ISchemaComponentHandle ISchemaTypedValue.GoverningType {get{Trace("governing");return Governing;}}
        bool ISchemaTypedValue.IsList {get{Trace("list");return List;}}
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems {get{Trace("items");return NullAt=="items"?null!:this;}}
        bool ISchemaTypedValue.TryGetSingleAtomic(out SchemaAtomicItem item){Trace("fast");item=InvalidOut||Items.Length==0?default:Items[0];return Fast;}
        IEnumerator<SchemaAtomicItem> IEnumerable<SchemaAtomicItem>.GetEnumerator(){Trace("enumerator");return NullAt=="enumerator"?null!:new Enumerator(this);}
        IEnumerator IEnumerable.GetEnumerator()=>((IEnumerable<SchemaAtomicItem>)this).GetEnumerator();
        private sealed class Enumerator(Container owner):IEnumerator<SchemaAtomicItem>
        {
            private int _index=-1;
            /// <inheritdoc/>
            public SchemaAtomicItem Current {get{owner.Currents++;owner.Trace("current");if(_index>=owner.ForbiddenCurrent)throw new Exception("forbidden Current");return owner.Items[_index];}}
            object IEnumerator.Current=>Current;
            /// <inheritdoc/>
            public bool MoveNext(){owner.Trace("move");return ++_index<owner.Items.Length;}
            /// <inheritdoc/>
            public void Reset()=>throw new NotSupportedException();
            /// <inheritdoc/>
            public void Dispose(){owner.Disposals++;owner.Trace("dispose");}
        }
    }
    private sealed class Fixture : IDisposable
    {
        internal readonly QuerySourceSession Session;
        internal readonly Input Input=new();
        internal readonly Authority Authority=new();
        internal readonly Origin Origin=new();
        internal readonly Handle Governing=new(),Actual=new();
        internal readonly Issuer Issuer;
        internal readonly QuerySourceSession.Snapshot Snapshot;
        internal Fixture()
        {
            var env=new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"typed-container-controls");
            Session=new(env,_=>XdmValue.FromString("unused"),4);Issuer=new(env,Input,Authority,Origin);
            Authority.Types.Add(Governing);Authority.Types.Add(Actual);Snapshot=Session.OpenSnapshot(env,Input,Input.Enter);
        }
        internal Container Value(int count=1,bool list=false,bool fast=true,bool original=true)
        {
            var c=new Container{Governing=Governing,List=list,Fast=fast};c.Items=Enumerable.Range(0,count).Select(i=>new SchemaAtomicItem(XdmValue.FromInteger(i+1),new Provenance(c,Governing,Actual,original?Origin:null))).ToArray();Issuer.Values.Add(c);return c;
        }
        internal T Run<T>(Func<QuerySourceSession.Operation,T> work,CancellationToken token=default)=>Session.Run(Session.Environment,[Snapshot],work,token);
        internal QueryTypedContainer Read(QuerySourceSession.Operation op,Container c,int max=8,int digits=128,int scale=128,bool original=true)
            =>op.ReadTypedContainer(Snapshot,Issuer,Input,original?Origin:null,c,max,digits,scale);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    /// <summary>Atomic/list shape, selected actual types and optional original assessment survive scalar and enumerated reads.</summary>
    /// <param name="list">Whether this is a list container.</param>
    /// <param name="count">Number of atomics.</param>
    /// <param name="fast">Whether scalar fast path is supplied.</param>
    /// <param name="original">Whether original assessment is present.</param>
    [Theory]
    [InlineData(false,0,false,false)][InlineData(true,0,false,true)][InlineData(false,1,true,true)]
    [InlineData(true,1,true,false)][InlineData(true,1,false,true)][InlineData(true,3,false,true)][InlineData(false,1,false,false)]
    public void ShapeAndEvidence(bool list,int count,bool fast,bool original)
    {
        using var f=new Fixture();var c=f.Value(count,list,fast,original);
        f.Run(op=>{var value=f.Read(op,c,original:original);Assert.Same(f.Governing,value.GoverningType);Assert.Equal(list,value.IsList);Assert.Equal(count,value.Count);
            for(int i=0;i<count;i++){var atom=value.GetAtomic(i);Assert.Equal(i+1,atom.Value.IntegerValue);Assert.Same(f.Governing,atom.DeclaredType);Assert.Same(f.Actual,atom.ActualType);Assert.Same(original?f.Origin:null,atom.OriginalAssessment);}return 0;});
        Assert.Equal(fast?0:1,c.Disposals);Assert.Equal(0,f.Input.Roots);if(fast)Assert.DoesNotContain("items",c.Reads);
    }
    /// <summary>Foreign containers, origins, input mappings and issuer environments refuse before every provider getter.</summary>
    /// <param name="reason">The rejected binding.</param>
    [Theory][InlineData("container")][InlineData("origin")][InlineData("input")][InlineData("ambiguous")][InlineData("environment")][InlineData("issuer")]
    public void ForeignBindingBeforeGetters(string reason)
    {
        using var f=new Fixture();var c=f.Value();
        if(reason=="container"||reason=="issuer")f.Issuer.Values.Clear();if(reason=="origin")f.Issuer.OriginOwned=false;
        if(reason=="input"||reason=="ambiguous")f.Issuer.InputOwned=false;if(reason=="environment")f.Issuer.Env=new(ImmutableDictionary<string,string>.Empty,"other");
        Assert.Throws<ArgumentException>(()=>f.Run(op=>f.Read(op,c)));Assert.Empty(c.Reads);
    }
    /// <summary>A snapshot not in the operation is rejected before the typed issuer or container is used.</summary>
    [Fact] public void NonmemberSnapshotAndSameOriginInputRefuse()
    {
        using var f=new Fixture();using var other=new Fixture();var c=f.Value();
        Assert.Throws<ArgumentException>(()=>other.Run(op=>op.ReadTypedContainer(f.Snapshot,f.Issuer,f.Input,f.Origin,c,8,128,128)));Assert.Empty(c.Reads);
        var input=new Input();var b=f.Session.OpenSnapshot(f.Session.Environment,input,input.Enter);
        Assert.Throws<ArgumentException>(()=>f.Session.Run(f.Session.Environment,[b],op=>op.ReadTypedContainer(b,f.Issuer,f.Input,f.Origin,c,8,128,128)));Assert.Empty(c.Reads);
    }
    /// <summary>Invalid default cells, typed transport markers and non-atomic payloads refuse, without reserving cache state.</summary>
    /// <param name="kind">Invalid cell/payload kind.</param>
    [Theory][InlineData("default")][InlineData("marker")][InlineData("empty")][InlineData("node")][InlineData("map")]
    public void InvalidCellsThenValidRetry(string kind)
    {
        using var f=new Fixture();var c=f.Value();
        if(kind=="default")c.InvalidOut=true;
        else{var p=new Provenance(c,f.Governing,f.Actual,f.Origin);c.Items=[new(kind=="marker"?XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object()):kind=="node"?XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<n/>"))):kind=="map"?XdmValue.FromMap(new XdmMap()):default,p)];}
        Assert.Throws<ArgumentException>(()=>f.Run(op=>f.Read(op,c)));
        c.InvalidOut=false;c.Items=[new(XdmValue.FromInteger(2),new Provenance(c,f.Governing,f.Actual,f.Origin))];
        f.Run(op=>{Assert.Equal(2,f.Read(op,c).GetAtomic(0).Value.IntegerValue);return 0;});
    }
    /// <summary>Unowned governing or atomic types and wrong original metadata cannot become admitted evidence.</summary>
    /// <param name="kind">The unowned or mismatched metadata.</param>
    [Theory][InlineData("governing")][InlineData("declared")][InlineData("actual")][InlineData("origin")]
    public void InvalidTypeOrOrigin(string kind)
    {
        using var f=new Fixture();var c=f.Value();var p=(Provenance)c.Items[0].TypeEvidence;
        if(kind=="governing")c.Governing=new Handle();if(kind=="declared")p.Declared=new Handle();if(kind=="actual")p.Actual=new Handle();if(kind=="origin")p.Origin=new Origin();
        Assert.Throws<ArgumentException>(()=>f.Run(op=>f.Read(op,c)));
    }
    /// <summary>Owned scalar payloads retain wide values but supplied digit/scale capacities still apply.</summary>
    [Fact] public void NumericCapacityAndWidePayload()
    {
        using var f=new Fixture();var c=f.Value();var p=c.Items[0].TypeEvidence;var n=BigInteger.Pow(10,1000)+1;
        c.Items=[new(XdmValue.FromInteger(n),p)];f.Run(op=>{Assert.Equal(n,f.Read(op,c,digits:1010).GetAtomic(0).Value.BigIntegerValue);return 0;});
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>f.Read(op,c,digits:8)));
        c.Items=[new(XdmValue.FromDecimal(1.000m),p)];Assert.Throws<NotSupportedException>(()=>f.Run(op=>f.Read(op,c,scale:2)));
    }
    /// <summary>Atom limits reject an extra member before Current, and negative/default limits precede provider access.</summary>
    [Fact] public void AtomLimitAndDefaultLimits()
    {
        using var f=new Fixture();var c=f.Value(3,true,false);c.ForbiddenCurrent=2;
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>f.Read(op,c,max:2)));Assert.Equal(2,c.Currents);Assert.Equal(1,c.Disposals);
        var invalid=f.Value();Assert.Throws<ArgumentOutOfRangeException>(()=>f.Run(op=>f.Read(op,invalid,max:0)));Assert.Empty(invalid.Reads);
    }
    /// <summary>False scalar fast path ignores its invalid out cell and enumerates the actual ordered payload.</summary>
    [Fact] public void FalseFastPathIgnoresOutCell()
    {using var f=new Fixture();var c=f.Value(2,true,false);c.InvalidOut=true;f.Run(op=>{Assert.Equal(2,f.Read(op,c).Count);return 0;});Assert.Equal(1,c.Disposals);}
    /// <summary>Getter/enumerator exceptions propagate, and acquired enumerators are disposed exactly once.</summary>
    /// <param name="stage">The throwing provider stage.</param>
    [Theory][InlineData("governing")][InlineData("list")][InlineData("fast")][InlineData("items")][InlineData("enumerator")][InlineData("move")][InlineData("current")][InlineData("declared")][InlineData("actual")][InlineData("origin")][InlineData("dispose")]
    public void ProviderFailuresAndDisposal(string stage)
    {
        using var f=new Fixture();var c=f.Value(1,true,false);c.Fail=stage;var ex=Assert.Throws<InvalidOperationException>(()=>f.Run(op=>f.Read(op,c)));Assert.Equal(stage,ex.Message);
        Assert.Equal(new[]{"move","current","declared","actual","origin","dispose"}.Contains(stage)?1:0,c.Disposals);
        c.Fail=null;f.Run(op=>{Assert.Equal(1,f.Read(op,c).Count);return 0;});
    }
    /// <summary>Primary read failures and disposal failures retain their original order without a cleanup retry.</summary>
    [Fact] public void PrimaryAndCleanupFailuresRemainOrdered()
    {
        using var f=new Fixture();var c=f.Value(1,true,false);c.OnRead=stage=>{if(stage=="move"||stage=="dispose")throw new InvalidOperationException(stage);};
        var ex=Assert.Throws<AggregateException>(()=>f.Run(op=>f.Read(op,c)));Assert.Equal(new[]{"move","dispose"},ex.InnerExceptions.Select(e=>e.Message));Assert.Equal(1,c.Disposals);
    }
    /// <summary>Every provider read stage observes issuer, source, authority or container revocation before another read.</summary>
    /// <returns>The complete dependency and getter-stage matrix.</returns>
    public static IEnumerable<object[]> Revocations()
    {
        foreach(var resource in new[]{"issuer","source","authority","container"})
            foreach(var stage in new[]{"governing","list","fast","items","enumerator","move","current","declared","actual","origin","dispose"})yield return [stage,resource];
    }
    /// <summary>Revocation stops following reads while required enumerator cleanup still occurs.</summary>
    /// <param name="stage">The revoking stage.</param>
    /// <param name="resource">The revoked dependency.</param>
    [Theory][MemberData(nameof(Revocations))]
    public void GetterRevocationStopsFurtherReads(string stage,string resource)
    {
        using var f=new Fixture();var c=f.Value(1,true,false);c.OnRead=s=>{if(s!=stage)return;if(resource=="issuer")f.Issuer.Available=false;if(resource=="source")f.Input.Available=false;if(resource=="authority")f.Authority.Available=false;if(resource=="container")f.Issuer.Values.Clear();};
        var error=Record.Exception(()=>f.Run(op=>f.Read(op,c)));Assert.NotNull(error);Assert.True(error is ArgumentException or ObjectDisposedException);
        int at=c.Reads.IndexOf(stage);Assert.True(at>=0);Assert.All(c.Reads.Skip(at+1),s=>Assert.Equal("dispose",s));
    }
    /// <summary>Container, source and selected authority loss are rechecked at every provider boundary.</summary>
    /// <param name="resource">The revoked dependency.</param>
    [Theory][InlineData("container")][InlineData("source")][InlineData("authority")][InlineData("authority-change")]
    public void DependencyRevocation(string resource)
    {
        using var f=new Fixture();var c=f.Value(1,true,false);c.OnRead=stage=>{if(stage!="current")return;if(resource=="container")f.Issuer.Values.Clear();if(resource=="source")f.Input.Available=false;if(resource=="authority")f.Authority.Available=false;if(resource=="authority-change")f.Issuer.SelectedAuthority=new Authority();};
        var ex=Record.Exception(()=>f.Run(op=>f.Read(op,c)));Assert.NotNull(ex);Assert.True(ex is ArgumentException or ObjectDisposedException);Assert.Equal(1,c.Disposals);Assert.DoesNotContain("declared",c.Reads);
    }
    /// <summary>Cancellation between MoveNext and Current never consumes the cell but disposes the held enumerator.</summary>
    [Fact] public void CancellationDuringEnumeration()
    {
        using var f=new Fixture();var c=f.Value(1,true,false);using var cancel=new CancellationTokenSource();c.OnRead=stage=>{if(stage=="move")cancel.Cancel();};
        Assert.ThrowsAny<OperationCanceledException>(()=>f.Run(op=>f.Read(op,c),cancel.Token));Assert.Equal(0,c.Currents);Assert.Equal(1,c.Disposals);
        c.OnRead=null;f.Run(op=>{Assert.Equal(1,f.Read(op,c).Count);return 0;});
    }
    /// <summary>Close during Current retains the actual owner until raw completion and drain, even during cleanup.</summary>
    /// <returns>The completed drain verification.</returns>
    [Fact] public async Task CloseDuringEnumerationDefersRelease()
    {
        using var f=new Fixture();var c=f.Value(1,true,false);c.OnRead=stage=>{if(stage=="current")f.Session.Dispose();if(stage=="current"||stage=="dispose")Assert.Equal(0,f.Input.Releases);};
        Assert.Throws<ObjectDisposedException>(()=>f.Run(op=>f.Read(op,c)));await f.Session.DrainAsync();Assert.Equal(1,f.Input.Releases);Assert.Equal(1,c.Disposals);
    }
    /// <summary>Escaped container and atomic views cannot read metadata after the original raw operation ends.</summary>
    [Fact] public void EscapedViewsAreRevokedWithoutMoreProviderGetters()
    {
        using var f=new Fixture();var c=f.Value();QueryTypedContainer? leaked=null;QueryTypedContainer.AtomicView atom=default;
        f.Run(op=>{leaked=f.Read(op,c);atom=leaked.GetAtomic(0);return 0;});int reads=c.Reads.Count;
        Assert.Throws<ObjectDisposedException>(()=>leaked!.Count);Assert.Throws<ObjectDisposedException>(()=>atom.Value);Assert.Throws<ObjectDisposedException>(()=>atom.ActualType);Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Distinct selected union-member types remain ordered under their separate governing list type.</summary>
    [Fact] public void MixedSelectedTypesStayOrdered()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);var other=new Handle();f.Authority.Types.Add(other);
        c.Items[1]=new(XdmValue.FromString("second"),new Provenance(c,f.Governing,other,f.Origin));
        f.Run(op=>{var v=f.Read(op,c);Assert.True(v.IsList);Assert.Same(f.Actual,v.GetAtomic(0).ActualType);Assert.Same(other,v.GetAtomic(1).ActualType);Assert.Equal("second",v.GetAtomic(1).Value.StringValue);return 0;});
    }
    /// <summary>Null provider enumerable/enumerator results refuse without fabricating an empty result.</summary>
    /// <param name="stage">The null returning stage.</param>
    [Theory][InlineData("items")][InlineData("enumerator")]
    public void NullEnumerationRefuses(string stage)
    {using var f=new Fixture();var c=f.Value(1,true,false);c.NullAt=stage;Assert.Throws<ArgumentException>(()=>f.Run(op=>f.Read(op,c)));Assert.Equal(0,c.Disposals);}
    /// <summary>Cancellation before raw entry invokes no provider getter and the admission remains reusable.</summary>
    [Fact] public void PreEntryCancellationAndDefaultView()
    {
        using var f=new Fixture();var c=f.Value();using var cancel=new CancellationTokenSource();cancel.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(()=>f.Run(op=>f.Read(op,c),cancel.Token));Assert.Empty(c.Reads);
        Assert.Throws<InvalidOperationException>(()=>default(QueryTypedContainer.AtomicView).Value);
        f.Run(op=>{Assert.Equal(1,f.Read(op,c).Count);return 0;});
    }
    /// <summary>Uncached cold reads report owned-input allocation and getter counts without a retained-cache budget claim.</summary>
    [Fact] public void ColdReadAllocationAndGetterObservations()
    {
        using var f=new Fixture();foreach(bool fast in new[]{true,false})
        {
            var c=f.Value(fast?1:3,true,fast);f.Run(op=>{f.Read(op,c);return 0;});Assert.Equal(fast?0:1,c.Disposals);Assert.Contains("fast",c.Reads);
            output.WriteLine($"Typed container getters {(fast?"scalar":"list-three")}: {c.Reads.Count} reads");c.Record=false;
            f.Run(op=>{for(int i=0;i<100;i++)f.Read(op,c);long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)f.Read(op,c);long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;output.WriteLine($"Typed container cold {(fast?"scalar":"list-three")}: {bytes} bytes/read");Assert.InRange(bytes,1,fast?2048:4096);return 0;});
        }
        var refused=f.Value(3,true,false);refused.Record=false;refused.ForbiddenCurrent=2;
        f.Run(op=>{void Refuse(){try{f.Read(op,refused,max:2);}catch(NotSupportedException){return;}throw new Exception("Expected refusal");}
            for(int i=0;i<100;i++)Refuse();long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Refuse();long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;
            output.WriteLine($"Typed container cold limit-refusal: {bytes} bytes/read");Assert.InRange(bytes,1,4096);return 0;});
        Assert.Equal(0,f.Input.Roots);
    }
}
