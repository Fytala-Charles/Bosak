// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies actual captured XQuery and XSLT execution on public session operations.
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
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;
using Bosak.Xslt.Api;
using Xunit;
namespace Bosak.Xslt.Tests;

/// <summary>Qualifies the private bridge using real compiled query/stylesheet engines.</summary>
public sealed class CapturedHostExecutionTests
{
    private sealed class Source:ISchemaSourceBinding;
    private sealed class Stamp:ISchemaSnapshotBinding;
    private sealed class Input(string text):ISchemaAssessmentInput,IQueryOperationLifetime,IDisposable
    {
        internal readonly IXdmNode Node=new XDocumentNode(XDocument.Parse("<n>"+text+"</n>"));
        internal bool Available=true;internal int Entries,Releases,Roots;
        private readonly Source _source=new();private readonly Stamp _stamp=new();
        IXdmNode ISchemaAssessmentInput.Root{get{Roots++;return Node;}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_stamp;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode n)=>ReferenceEquals(n,Node);
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){Entries++;return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!owner.Available,this);}
            /// <inheritdoc/>
            public void Dispose()=>owner.Releases++;
        }
        /// <inheritdoc/>
        public void Dispose()=>Available=false;
    }
    private static SchemaQueryLimits Limits=>new(4,8,16,128,128);
    private static string Sheet=>"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='g' select='string(/*)'/><xsl:template match='/'><xsl:sequence select=\"concat(string(/*),'|',$g)\"/></xsl:template><xsl:template name='main'><xsl:sequence select=\"concat(string(/*),'|',$g)\"/></xsl:template></xsl:stylesheet>";
    private static CapturedTransformHost Transform(string? source=null,XsltCompiler? compiler=null)=>CapturedTransformHost.Compile(source??Sheet,compiler??new(),Limits,16,16)!;
    private static string Text(XdmValue v,Action check)
    {check();if(!v.IsSequence)return v.ToString();var result=new List<string>();foreach(var item in XdmSequence.FromSource(v.SequenceValue!)){check();result.Add(item.ToString());}check();return string.Join("|",result);}
    /// <summary>HC01 actual XQuery compilation uses copied variables and a separate child context.</summary>
    [Fact] public void HC01_ActualQuery()
    {var host=CapturedTransformHost.Query.Compile("declare variable $v external;concat($v,'!')",new(),Limits,16,16)!;using var session=host.Session;var caller=new EvaluationContext().WithVariable("v",XdmValue.FromString("query"));Assert.Equal(QueryHostLanguage.XQuery,session.Environment.HostLanguage);Assert.Equal("query!",session.Run([],op=>host.Use(op,caller,Text)));Assert.True(caller.TryGetVariable("v",out var value));Assert.Equal("query",value.StringValue);}
    /// <summary>HC02 equal descriptors do not admit another compiled executable operation.</summary>
    [Fact] public void HC02_ActualForeignExecutable()
    {var first=Transform();var other=Transform();using var a=first.Session;using var b=other.Session;Assert.Equal(a.Environment.StaticContextFingerprint,b.Environment.StaticContextFingerprint);Assert.Throws<ArgumentException>(()=>b.Run([],op=>first.Use(op,null,null,"main",null,Text)));}
    /// <summary>HC03 a foreign dynamic node refuses before query execution.</summary>
    [Fact] public void HC03_QueryForeignNode()
    {var host=CapturedTransformHost.Query.Compile("declare variable $v external;string($v)",new(),Limits,16,16)!;using var session=host.Session;using var input=new Input("A");using var snapshot=session.OpenSnapshot(input,input).Value;using var foreign=new Input("B");Assert.Throws<NotSupportedException>(()=>session.Run([snapshot],op=>host.Use(op,new EvaluationContext().WithVariable("v",XdmValue.FromNode(foreign.Node)),Text)));Assert.Equal(0,foreign.Roots);}
    /// <summary>HC04 actual transforms distinguish shared/separate match and global context and named-template focus.</summary>
    /// <param name="mode">Selected source-role arrangement.</param>
    [Theory][InlineData("distinct")][InlineData("shared")][InlineData("template")]
    public void HC04_ActualRoles(string mode)
    {var host=Transform();using var session=host.Session;using var a=new Input("A");using var b=new Input("B");using var sa=session.OpenSnapshot(a,a).Value;using var sb=session.OpenSnapshot(b,b).Value;var result=session.Run(mode=="shared"?[sa]:[sa,sb],op=>host.Use(op,sa,mode=="shared"?sa:sb,mode=="template"?"main":null,null,Text));Assert.Equal(mode=="shared"?"A|A":mode=="template"?"B|B":"A|B",result);Assert.Equal(1,a.Entries);Assert.Equal(1,b.Entries);}
    /// <summary>HC05 a real source-free named template uses explicit empty membership.</summary>
    [Fact] public void HC05_ActualSourceFree()
    {var host=Transform("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template name='main'><xsl:sequence select=\"'free'\"/></xsl:template></xsl:stylesheet>");using var session=host.Session;Assert.Equal("free",session.Run([],op=>host.Use(op,null,null,"main",null,Text)));}
    /// <summary>HC05 absent global context never silently falls back to the match source.</summary>
    [Fact] public void HC05_NoGlobalFallback()
    {var host=Transform();using var session=host.Session;using var input=new Input("A");using var snapshot=session.OpenSnapshot(input,input).Value;Assert.NotNull(Record.Exception(()=>session.Run([snapshot],op=>host.Use(op,snapshot,null,null,null,Text))));}
    /// <summary>HC06 caller compiler namespace mutation cannot change captured query behavior.</summary>
    [Fact] public void HC06_FrozenQueryCompiler()
    {var compiler=new XQueryCompiler().WithNamespace("p","urn:one");var host=CapturedTransformHost.Query.Compile("namespace-uri-from-QName(xs:QName('p:x'))",compiler,Limits,16,16)!;using var session=host.Session;compiler.WithNamespace("p","urn:two");Assert.Equal("urn:one",session.Run([],op=>host.Use(op,new(),Text)));}
    /// <summary>HC06 static parameter mutation cannot change a captured stylesheet.</summary>
    [Fact] public void HC06_FrozenStylesheetCompiler()
    {var values=new Dictionary<(string,string),XdmValue>{{("p",""),XdmValue.FromString("one")}};var compiler=new XsltCompiler{StaticParameters=values};var host=Transform("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:param name='p' static='yes'/><xsl:template name='main'><xsl:sequence select='$p'/></xsl:template></xsl:stylesheet>",compiler);using var session=host.Session;values[("p","")]=XdmValue.FromString("two");Assert.Equal("one",session.Run([],op=>host.Use(op,null,null,"main",new EvaluationContext().WithVariable("p",XdmValue.FromString("dynamic")),Text)));}
    /// <summary>HC06 uncaptured schema/modules/conditional compilation are honest refusals.</summary>
    /// <param name="mode">Unsupported compilation capability.</param>
    [Theory][InlineData("include")][InlineData("schema")][InlineData("conditional")]
    public void HC06_UnsupportedStylesheet(string mode)
    {var content=mode=="include"?"<xsl:include href='never.xsl'/>":mode=="schema"?"<xsl:import-schema namespace='urn:s'/>":"<xsl:template name='main' use-when='true()'/>";Assert.Null(CapturedTransformHost.Compile("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"+content+"</xsl:stylesheet>",new(),Limits,16,16));}
    /// <summary>HC06 schema-aware XQuery imports refuse before external resolution.</summary>
    [Fact] public void HC06_UnsupportedQuerySchema()
    {Assert.Null(CapturedTransformHost.Query.Compile("import schema namespace s='urn:s';1",new(),Limits,16,16));}
    /// <summary>HC07 actual host producer rejects snapshot closure after evaluation during consumption.</summary>
    [Fact] public void HC07_ClosureDuringConsumption()
    {var host=Transform();using var session=host.Session;using var input=new Input("A");using var snapshot=session.OpenSnapshot(input,input).Value;Assert.Throws<ObjectDisposedException>(()=>session.Run([snapshot],op=>host.Use(op,snapshot,snapshot,null,null,(v,check)=>{snapshot.Dispose();return Text(v,check);})) );Assert.Equal(1,input.Releases);}
    /// <summary>HC08 actual query work can follow an await while a canceled caller wait retains the raw operation.</summary>
    /// <returns>A task completing after actual query work and raw cleanup.</returns>
    [Fact] public async Task HC08_QueryAsyncWait()
    {var host=CapturedTransformHost.Query.Compile("'query'",new(),Limits,16,16)!;using var session=host.Session;var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var raw=session.RunAsync([],async op=>{await resume.Task;return host.Use(op,new(),Text);});using var cancel=new CancellationTokenSource();cancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(cancel.Token));Assert.False(session.DrainAsync().IsCompleted);resume.SetResult();Assert.Equal("query",await raw);await session.DrainAsync();}
    /// <summary>HC09 original consumer exceptions keep object identity through real transform cleanup.</summary>
    [Fact] public void HC09_ActualConsumerFailure()
    {var host=Transform();using var session=host.Session;using var input=new Input("A");using var snapshot=session.OpenSnapshot(input,input).Value;var expected=new ApplicationException("consumer");Assert.Same(expected,Assert.Throws<ApplicationException>(()=>session.Run([snapshot],op=>host.Use<string>(op,snapshot,snapshot,null,null,(_,_)=>throw expected))));}
    /// <summary>HC10 actual output consumption remains in the host window and its guard expires immediately.</summary>
    [Fact] public void HC10_SerializedConsumptionWindow()
    {var host=Transform();using var session=host.Session;using var input=new Input("A");using var snapshot=session.OpenSnapshot(input,input).Value;Action? guard=null;Assert.Equal("A|A",session.Run([snapshot],op=>host.Use(op,snapshot,snapshot,null,null,(v,check)=>{guard=check;Assert.Throws<InvalidOperationException>(()=>host.Use(op,snapshot,snapshot,null,null,Text));return Text(v,check);})));Assert.Throws<ObjectDisposedException>(()=>guard!());}
    /// <summary>HC13 ordinary XPath facade remains the selected public host; no all-host entry has been added.</summary>
    [Fact] public void HC13_NoPublicFacadeActivation()
    {Assert.Null(typeof(Bosak.XPath.Api.Schema.SchemaQuerySession).Assembly.GetType("Bosak.XPath.Api.Schema.CapturedHostBinding")?.GetConstructors().FirstOrDefault());Assert.DoesNotContain(typeof(Bosak.XPath.Api.Schema.SchemaQuerySession).GetMethods(),m=>m.Name=="CompileXQuery");}
}
