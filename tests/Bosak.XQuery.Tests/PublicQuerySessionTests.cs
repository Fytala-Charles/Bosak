// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies public XQuery capture, borrowed results, ownership and raw lifecycle.
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
using System.Xml.Linq;
using Bosak.XQuery.Api;
using Bosak.XQuery.Api.Schema;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XQuery.Tests;

/// <summary>Exercises only public APIs for the captured ordinary XQuery session.</summary>
public sealed class PublicQuerySessionTests
{
    private sealed class Source:ISchemaSourceBinding;
    private sealed class Stamp:ISchemaSnapshotBinding;
    private sealed class Input:ISchemaAssessmentInput,IQueryOperationLifetime,IDisposable
    {
        private readonly Source _source=new();private readonly Stamp _stamp=new();
        private readonly HashSet<IXdmNode> _known=new(ReferenceEqualityComparer.Instance);
        internal readonly IXdmNode Node;
        internal bool Available=true,All;internal int Entries,Releases,Active,Roots,Reads;internal FileStream? Resource;internal Action<string>? Hook;internal Action? OnRelease;
        internal Input(string text="data")=>Node=Wrap(new XDocumentNode(XDocument.Parse("<n a='x'>"+text+"</n>")));
        private IXdmNode Wrap(IXdmNode n){var node=new NodeView(this,n);_known.Add(node);return node;}
        internal void Check(CancellationToken token=default){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IXdmNode ISchemaAssessmentInput.Root{get{Roots++;Check();return Node;}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_stamp;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode n){Check();return All||_known.Contains(n);}
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token)=>Check(token);
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){Check(token);Entries++;Active++;return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token)=>owner.Check(token);
            /// <inheritdoc/>
            public void Dispose(){owner.Releases++;owner.Active--;if(!owner.Available&&owner.Active==0)owner.Resource?.Dispose();owner.OnRelease?.Invoke();}
        }
        /// <inheritdoc/>
        public void Dispose(){Available=false;if(Active==0)Resource?.Dispose();}
        private sealed class NodeView(Input owner,IXdmNode raw):IXdmNode
        {
            private T Read<T>(string name,Func<T> read){owner.Reads++;owner.Check();owner.Hook?.Invoke(name);owner.Check();return read();}
            XdmNodeKind IXdmNode.NodeKind=>Read("kind",()=>raw.NodeKind);
            string IXdmNode.LocalName=>Read("name",()=>raw.LocalName);string IXdmNode.NamespaceUri=>Read("namespace",()=>raw.NamespaceUri);string IXdmNode.Prefix=>Read("prefix",()=>raw.Prefix);
            string IXdmNode.StringValue=>Read("string",()=>raw.StringValue);XdmValue IXdmNode.TypedValue=>Read("typed",()=>raw.TypedValue);
            IXdmNode? IXdmNode.Parent=>Read("parent",()=>raw.Parent is{} n?owner.Wrap(n):null);
            IXdmNode? IXdmNode.Document=>Read("document",()=>raw.Document is{} n?owner.Wrap(n):null);
            XdmSequence IXdmNode.Children(XdmNodeKind kind)=>Read("children",()=>Wrap(raw.Children(kind)));
            XdmSequence IXdmNode.Attributes(string? localName,string? namespaceUri)=>Read("attributes",()=>Wrap(raw.Attributes(localName,namespaceUri)));
            XdmSequence IXdmNode.Axis(XdmAxis axis)=>Read("axis",()=>Wrap(raw.Axis(axis)));
            bool IXdmNode.IsSameNode(IXdmNode other)=>Read("identity",()=>other is NodeView n&&ReferenceEquals(owner,n.Owner)&&raw.IsSameNode(n.Raw));
            private Input Owner=>owner;private IXdmNode Raw=>raw;
            long IXdmNode.DocumentOrder=>Read("order",()=>raw.DocumentOrder);string IXdmNode.BaseUri=>Read("base",()=>raw.BaseUri);string IXdmNode.DocumentUri=>Read("uri",()=>raw.DocumentUri);
            string IXdmNode.ToXmlString()=>Read("xml",raw.ToXmlString);
            private XdmSequence Wrap(XdmSequence values)=>XdmSequence.FromSource(new Projected(owner,values));
        }
        private sealed class Projected(Input owner,XdmSequence values):IXdmSequence
        {
            bool IXdmSequence.TryGetLength(out int n){owner.Check();return values.TryGetLength(out n);}
            IXdmSequenceEnumerator IXdmSequence.GetEnumerator(){owner.Check();return new Enumerator(owner,values.GetEnumerator());}
            private sealed class Enumerator(Input owner,XdmSequence.Enumerator inner):IXdmSequenceEnumerator
            {
                private XdmSequence.Enumerator _inner=inner;
                XdmValue IXdmSequenceEnumerator.Current{get{owner.Check();var v=_inner.Current;return v.IsNode?XdmValue.FromNode(owner.Wrap(v.NodeValue!)):v;}}
                bool IXdmSequenceEnumerator.MoveNext(){owner.Check();return _inner.MoveNext();}
            }
        }
    }
    private sealed class PoisonAssessor:ISchemaAssessor
    {
        ISchemaAuthority ISchemaAssessor.Authority=>throw new Exception("authority poison");SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("schema poison");string ISchemaAssessor.AssessmentProfile=>throw new Exception("profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment e,CancellationToken t)=>throw new Exception("bind poison");
    }
    private static SchemaQueryLimits Limits(int snapshots=4)=>new(snapshots,8,16,128,128);
    private static Session Compile(string query,XQueryCompiler? compiler=null,SchemaQueryLimits? limits=null)=>SchemaXQueryCompiler.Compile(query,limits??Limits(),compiler).Value;
    private static string Text(XdmValue v)
    {if(v.IsUndefined)return "";if(!v.IsSequence)return v.IsNode?v.NodeValue!.StringValue:v.ToString();var items=new List<string>();foreach(var x in XdmSequence.FromSource(v.SequenceValue!))items.Add(x.IsNode?x.NodeValue!.StringValue:x.ToString());return string.Join("|",items);}
    private static string Evaluate(Session s,EvaluationContext? c=null)=>s.Run([],op=>Text(op.Evaluate(c??new())));
    /// <summary>XQ01 reviewed public entry exposes its distinct real captured XQuery environment.</summary>
    [Fact] public void XQ01_PublicEntry()
    {using var s=Compile("'query'");Assert.Equal(QueryHostLanguage.XQuery,s.Environment.HostLanguage);Assert.Equal("3.1",s.Environment.HostVersion);Assert.Equal("bosak-xquery-ordinary-v1",s.Environment.BehaviorProfile);Assert.Equal("bosak-xquery-ordinary-descriptor-v1",s.Environment.DescriptorFormatVersion);Assert.Null(s.Advisor);Assert.Equal("query",Evaluate(s));}
    /// <summary>XQ02 valid source-free ordinary queries exercise the actual engine.</summary>
    /// <param name="query">Query source.</param><param name="expected">Expected flat result.</param>
    [Theory][InlineData("()","")][InlineData("1 + 2","3")][InlineData("for $i in 1 to 3 return $i * 2","2|4|6")][InlineData("<n>constructed</n>","constructed")][InlineData("xs:hexBinary('AB')","AB")]
    public void XQ02_ActualQueries(string query,string expected)
    {using var s=Compile(query);Assert.Equal(expected,Evaluate(s));}
    /// <summary>XQ03 compile arguments/default limits and pre-cancellation validate before dispatch.</summary>
    [Fact] public void XQ03_Arguments()
    {Assert.Throws<ArgumentNullException>(()=>SchemaXQueryCompiler.Compile(null!,Limits()));Assert.ThrowsAny<ArgumentException>(()=>SchemaXQueryCompiler.Compile("1",default));using var c=new CancellationTokenSource();c.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>SchemaXQueryCompiler.Compile("1",Limits(),assessor:new PoisonAssessor(),cancellationToken:c.Token));}
    /// <summary>XQ04 assessor and schema import profiles return Unsupported without touching provider authority.</summary>
    [Fact] public void XQ04_UnsupportedSchema()
    {Assert.Equal(SchemaResultKind.Unsupported,SchemaXQueryCompiler.Compile("1",Limits(),assessor:new PoisonAssessor()).Kind);Assert.Equal(SchemaResultKind.Unsupported,SchemaXQueryCompiler.Compile("import schema namespace s='urn:s';1",Limits()).Kind);}
    /// <summary>XQ05 ordinary source focus and variables require the exact admitted input and remain unchanged.</summary>
    [Fact] public void XQ05_SourceInput()
    {using var s=Compile("declare variable $v external;concat(string(.),'|',$v)");using var input=new Input();using var snapshot=s.OpenSnapshot(input,input).Value;var c=new EvaluationContext().WithFocus(XdmValue.FromNode(input.Node),1,1).WithVariable("v",XdmValue.FromString("value"));Assert.Equal("data|value",s.Run([snapshot],op=>Text(op.Evaluate(c))));Assert.Equal(1,input.Entries);Assert.Equal(0,input.Releases);Assert.True(c.TryGetVariable("v",out var v));Assert.Equal("value",v.StringValue);}
    /// <summary>XQ06 foreign and multiply-owned dynamic input nodes refuse before node data getters.</summary>
    /// <param name="ambiguous">Multiple owners instead of no owner.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void XQ06_ExactlyOneOwner(bool ambiguous)
    {using var s=Compile("declare variable $v external;string($v)");using var a=new Input();using var b=new Input();using var foreign=new Input();using var sa=s.OpenSnapshot(a,a).Value;using var sb=s.OpenSnapshot(b,b).Value;a.All=b.All=ambiguous;var c=new EvaluationContext().WithVariable("v",XdmValue.FromNode(ambiguous?a.Node:foreign.Node));Assert.Throws<NotSupportedException>(()=>s.Run([sa,sb],op=>op.Evaluate(c)));Assert.Equal(0,a.Reads);Assert.Equal(0,b.Reads);Assert.Equal(0,foreign.Reads);}
    /// <summary>XQ07 frozen namespace/module source survives later compiler mutation.</summary>
    [Fact] public void XQ07_CompilerSnapshot()
    {var c=new XQueryCompiler().WithNamespace("p","urn:one").WithModule("urn:m","module namespace m='urn:m';declare function m:f(){'old'};");using var s=Compile("import module namespace m='urn:m';concat(m:f(),'|',namespace-uri-from-QName(xs:QName('p:n')))",c);c.WithNamespace("p","urn:two").WithModule("urn:m","module namespace m='urn:m';declare function m:f(){'new'};");Assert.Equal("old|urn:one",Evaluate(s));}
    /// <summary>XQ08 deferred loops retain the compiled base URI and namespaces on their owned child.</summary>
    [Fact] public void XQ08_DeferredStaticContext()
    {using var s=Compile("declare base-uri 'https://query.invalid/base/';declare namespace p='urn:query';for $i in 1 to 2 return concat(resolve-uri(string($i)),'|',namespace-uri-from-QName(xs:QName('p:n')))");var c=new EvaluationContext{BaseUri="https://caller.invalid/"};c.WithNamespace("p","urn:caller");Assert.Equal("https://query.invalid/base/1|urn:query|https://query.invalid/base/2|urn:query",Evaluate(s,c));Assert.Equal("https://caller.invalid/",c.BaseUri);}
    /// <summary>XQ09 real parser/compiler and execution failures propagate rather than become Unsupported.</summary>
    /// <param name="query">Failing query.</param><param name="compile">Failure occurs at compilation.</param>
    [Theory][InlineData("for $x in",true)][InlineData("error()",false)]
    public void XQ09_ErrorPropagation(string query,bool compile)
    {if(compile)Assert.NotNull(Record.Exception(()=>Compile(query)));else{using var s=Compile(query);Assert.NotNull(Record.Exception(()=>Evaluate(s)));}}
    /// <summary>XQ10 actual context cardinalities permit more than the private fixture's sixteen bindings.</summary>
    [Fact] public void XQ10_ActualCaptureCounts()
    {using var s=Compile("declare variable $v external;$v");var c=new EvaluationContext().WithVariable("v",XdmValue.FromString("value"));for(int i=0;i<32;i++){c.WithVariable("extra"+i,XdmValue.FromString("x"));c.WithNamespace("p"+i,"urn:"+i);}Assert.Equal("value",Evaluate(s,c));}
    /// <summary>XQ11 unsupported runtime context services and structured inputs refuse before engine use.</summary>
    /// <param name="mode">Unsupported context capability.</param>
    [Theory][InlineData("loader")][InlineData("lazy")][InlineData("map")][InlineData("array")][InlineData("function")][InlineData("focus")]
    public void XQ11_ClosedContext(string mode)
    {using var s=Compile("declare variable $v external;$v");var c=new EvaluationContext().WithVariable("v",XdmValue.FromString("value"));if(mode=="loader")c.DocumentLoader=_=>throw new Exception("loader poison");if(mode=="lazy")c.LazyVariableResolver=(_,_)=>throw new Exception("resolver poison");if(mode=="map")c.WithVariable("v",XdmValue.FromMap(new XdmMap()));if(mode=="array")c.WithVariable("v",XdmValue.FromArray(new XdmArray()));if(mode=="function")c.WithVariable("v",XdmValue.FromFunction(new object()));if(mode=="focus")c.WithFocus(XdmValue.FromString("focus"),1,1);Assert.Throws<NotSupportedException>(()=>Evaluate(s,c));}
    /// <summary>XQ12 maps/arrays/functions are outside this flat ordinary result profile.</summary>
    /// <param name="query">Unsupported result expression.</param>
    [Theory][InlineData("map{'a':1}")][InlineData("[1,2]")][InlineData("function(){1}")]
    public void XQ12_StructuredResults(string query)
    {using var s=Compile(query);Assert.Throws<NotSupportedException>(()=>Evaluate(s));}
    /// <summary>XQ13 deferred outer sequence length/enumeration requires the live exact raw operation.</summary>
    [Fact] public void XQ13_SequenceExpiryAndCachedValue()
    {using var s=Compile("for $i in 1 to 3 return $i");XdmValue result=default,cached=default;XdmSequence.Enumerator enumerator=default;s.Run([],op=>{result=op.Evaluate(new());Assert.True(result.IsSequence);enumerator=XdmSequence.FromSource(result.SequenceValue!).GetEnumerator();Assert.True(enumerator.MoveNext());cached=enumerator.Current;return 0;});Assert.Equal(1,cached.IntegerValue);Assert.Equal(1,enumerator.Current.IntegerValue);Assert.Throws<ObjectDisposedException>(()=>enumerator.MoveNext());Assert.Throws<ObjectDisposedException>(()=>result.SequenceValue!.TryGetLength(out _));}
    /// <summary>XQ14 late cell refusal does not pretend a streamed prefix was rolled back.</summary>
    [Fact] public void XQ14_PrefixThenRefusal()
    {using var s=Compile("(1,map{'a':2})");s.Run([],op=>{var v=op.Evaluate(new());var e=XdmSequence.FromSource(v.SequenceValue!).GetEnumerator();Assert.True(e.MoveNext());Assert.Equal(1,e.Current.IntegerValue);Assert.Throws<NotSupportedException>(()=>e.MoveNext());return 0;});}
    /// <summary>XQ15 independent raw evaluations and async callbacks retain ordinary borrowed source ownership.</summary>
    /// <returns>A task completing after actual query work and cleanup.</returns>
    [Fact] public async Task XQ15_AsyncAndCanceledWait()
    {using var s=Compile("string(.)");using var input=new Input();using var snapshot=s.OpenSnapshot(input,input).Value;var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var raw=s.RunAsync([snapshot],async op=>{await resume.Task;return Text(op.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(input.Node),1,1)));});using var cancel=new CancellationTokenSource();cancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(cancel.Token));Assert.False(s.DrainAsync().IsCompleted);Assert.Equal(0,input.Releases);resume.SetResult();Assert.Equal("data",await raw);await s.DrainAsync();}
    /// <summary>XQ16 source close retains a real file while an actual engine node getter is blocked.</summary>
    /// <returns>A task completing after raw execution and actual resource release.</returns>
    [Fact] public async Task XQ16_ActualResourceTail()
    {using var s=Compile("string(.)");using var input=new Input();using var file=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);input.Resource=file;using var snapshot=s.OpenSnapshot(input,input).Value;using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();input.Hook=stage=>{if(stage!="string")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));Assert.True(file.CanWrite);file.WriteByte(1);};var task=Task.Run(()=>s.Run([snapshot],op=>Text(op.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(input.Node),1,1)))));try{Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));s.Dispose();input.Dispose();Assert.True(file.CanWrite);Assert.False(s.DrainAsync().IsCompleted);}finally{resume.Set();}await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await task);await s.DrainAsync();Assert.False(file.CanWrite);Assert.Equal(1,input.Releases);}
    /// <summary>XQ17 reentrant engine work on the same raw operation refuses before a second evaluation.</summary>
    [Fact] public void XQ17_Reentry()
    {using var s=Compile("string(.)");using var input=new Input();using var snapshot=s.OpenSnapshot(input,input).Value;s.Run([snapshot],op=>{bool seen=false;var c=new EvaluationContext().WithFocus(XdmValue.FromNode(input.Node),1,1);input.Hook=stage=>{if(stage!="string"||seen)return;seen=true;Assert.Throws<InvalidOperationException>(()=>op.Evaluate(c));};Assert.Equal("data",Text(op.Evaluate(c)));Assert.True(seen);return 0;});}
    /// <summary>XQ18 source quotas, duplicate membership and foreign admission rules remain unchanged.</summary>
    [Fact] public void XQ18_AdmissionLimits()
    {using var s=Compile("1",limits:Limits(1));using var a=new Input();using var b=new Input();using var first=s.OpenSnapshot(a,a).Value;Assert.Throws<InvalidOperationException>(()=>s.OpenSnapshot(b,b));Assert.Throws<ArgumentException>(()=>s.Run([first,first],_=>1));first.Dispose();using var retry=s.OpenSnapshot(b,b).Value;}
    /// <summary>XQ19 all five limits distinguish the public semantic descriptor without imposing a numeric pipeline claim.</summary>
    [Fact] public void XQ19_DescriptorLimits()
    {using var s=Compile("1");foreach(var l in new[]{new SchemaQueryLimits(5,8,16,128,128),new(4,9,16,128,128),new(4,8,17,128,128),new(4,8,16,129,128),new(4,8,16,128,127)}){using var other=Compile("1",limits:l);Assert.NotEqual(s.Environment.StaticContextFingerprint,other.Environment.StaticContextFingerprint);}}
    /// <summary>XQ20 the line-ending option is captured and existing legacy execution remains available.</summary>
    /// <param name="xml11">Select XML 1.1 line endings.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void XQ20_LineEndings(bool xml11)
    {const string query="contains('a\u0085b',codepoints-to-string(10))";using var s=SchemaXQueryCompiler.Compile(query,Limits(),xml11LineEndings:xml11).Value;var old=new XQueryCompiler().Compile(query,xml11).Evaluate(new XQueryContext());Assert.Equal(old.ToString(),Evaluate(s));}
    /// <summary>XQ21 original work and cleanup failures preserve their identities and aggregate boundaries.</summary>
    [Fact] public void XQ21_CleanupFailures()
    {using var s=Compile("1");using var input=new Input();using var snapshot=s.OpenSnapshot(input,input).Value;var primary=new ApplicationException("work");var cleanup=new AggregateException(new Exception("cleanup"));input.OnRelease=()=>throw cleanup;var error=Assert.Throws<AggregateException>(()=>s.Run<int>([snapshot],op=>{op.Evaluate(new());snapshot.Dispose();throw primary;}));Assert.Equal(new Exception[]{primary,cleanup},error.InnerExceptions);Assert.Equal(1,input.Releases);}
    /// <summary>XQ22 cancellation during an actual engine getter denies delivery and retains the raw token.</summary>
    [Fact] public void XQ22_ActualCancellation()
    {using var s=Compile("string(.)");using var input=new Input();using var snapshot=s.OpenSnapshot(input,input).Value;using var cancel=new CancellationTokenSource();input.Hook=stage=>{if(stage=="string")cancel.Cancel();};var error=Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([snapshot],op=>op.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(input.Node),1,1)),cancel.Token));Assert.Equal(cancel.Token,error.CancellationToken);}
    /// <summary>XQ23 independent async operations on one executable keep their child contexts separate.</summary>
    /// <returns>A task completing after both independent raw query operations.</returns>
    [Fact] public async Task XQ23_IndependentQueries()
    {using var s=Compile("declare variable $v external;concat($v,'!')");async Task<string> Run(string value)=>await s.RunAsync([],async op=>{await Task.Yield();return Text(op.Evaluate(new EvaluationContext().WithVariable("v",XdmValue.FromString(value))));});Assert.Equal(new[]{"a!","b!"},await Task.WhenAll(Run("a"),Run("b")));}
}
