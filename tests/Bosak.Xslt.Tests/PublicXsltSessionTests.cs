// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies public ordinary XSLT invocation, bounded capture and owned outcomes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Api.Schema;
using Bosak.Xslt.Capture;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
namespace Bosak.Xslt.Tests;

/// <summary>Exercises the actual public XSLT compiler, transform, capture and owned result.</summary>
public sealed class PublicXsltSessionTests
{
    private readonly ITestOutputHelper _output;
    /// <summary>Receives the bounded cost-fixture diagnostic sink.</summary>
    /// <param name="output">Captured test diagnostics.</param>
    public PublicXsltSessionTests(ITestOutputHelper output) { _output=output; }
    private sealed class Source : ISchemaSourceBinding;
    private sealed class Stamp : ISchemaSnapshotBinding;
    private sealed class Input(string text="A") : ISchemaAssessmentInput,IQueryOperationLifetime,IDisposable
    {
        internal readonly IXdmNode Node=new XDocumentNode(XDocument.Parse("<n>"+text+"</n>"));
        internal IXdmNode? OverrideRoot;internal bool Available=true,All;internal int Roots,Releases,Active;internal Action? OnRelease;internal FileStream? Resource;
        private readonly Source _source=new();private readonly Stamp _stamp=new();
        IXdmNode ISchemaAssessmentInput.Root{get{Roots++;return OverrideRoot??Node;}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_stamp;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>All||ReferenceEquals(node,Node)||ReferenceEquals(node,OverrideRoot);
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){token.ThrowIfCancellationRequested();Active++;return new Lease(this);}
        private sealed class Lease(Input input):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!input.Available,input);}
            /// <inheritdoc/>
            public void Dispose(){input.Releases++;input.Active--;if(!input.Available&&input.Active==0)input.Resource?.Dispose();input.OnRelease?.Invoke();}
        }
        /// <inheritdoc/>
        public void Dispose(){Available=false;if(Active==0)Resource?.Dispose();}
    }
    private sealed class PoisonAssessor:ISchemaAssessor
    {
        ISchemaAuthority ISchemaAssessor.Authority=>throw new Exception("authority poison");
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("schema poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment e,CancellationToken t)=>throw new Exception("bind poison");
    }
    private static SchemaQueryLimits Limits=>new(4,8,16,128,128);
    private static string Sheet(string body="Hello",string declarations="")=>
        "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:output method='text' encoding='UTF-8' byte-order-mark='no'/>"+declarations+"<xsl:template name='main'>"+body+"</xsl:template></xsl:stylesheet>";
    private static XsltCaptureOptions Options()=>new(){MaxSecondaryOutputCount=4,MaxBytesPerOutput=4096,MaxTotalBytes=8192,MaxMessages=4,TimeLimit=TimeSpan.FromSeconds(10),MaxEngineOperations=100000};
    private static SchemaXsltInvocation Main=>new(initialTemplate:"main");
    private static SchemaXsltExecutable Compile(string? source=null,XsltCompiler? compiler=null)=>SchemaXsltCompiler.Compile(source??Sheet(),Limits,compiler).Value;
    private static string Text(SchemaXsltCaptureResult result)=>Encoding.UTF8.GetString(result.Capture.Principal!.Bytes.ToArray());
    private static SchemaXsltExecutable Instrument(string source,Action<long> observer)
    {
        Assert.True(SchemaHostSessionCompilation.TryCaptureXslt(source,new(),null,out var executable,out var environment,out _));
        return new(executable!,environment!,Limits,observer);
    }

    /// <summary>PX01 source-free owned capture exposes detached bytes and the public descriptor.</summary>
    [Fact] public void PX01_PublicCompileAndCapture()
    {
        var executable=Compile();using var session=executable.Session;
        var result=executable.RunCaptureTransform([],Main,Options());
        Assert.True(result.IsSuccess);Assert.Equal("Hello",Text(result.Result!));Assert.Empty(result.Result!.ResourceReceipts);Assert.Empty(result.Result.ExecutionErrors);
        Assert.Equal(QueryHostLanguage.Xslt,session.Environment.HostLanguage);Assert.Equal("3.0",session.Environment.HostVersion);
        Assert.Equal("bosak-xslt-ordinary-v1",session.Environment.BehaviorProfile);
        Assert.Equal("bosak-xslt-ordinary-descriptor-v1",session.Environment.DescriptorFormatVersion);
        session.Dispose();Assert.Equal("Hello",Text(result.Result));
    }

    /// <summary>PX02 borrowed transform and direct capture use this exact public operation.</summary>
    [Fact] public void PX02_BorrowedRoutes()
    {
        var executable=Compile(Sheet("<xsl:sequence select='42'/>"));using var session=executable.Session;
        session.Run([],op=>
        {
            Assert.Equal(42,executable.Transform(op,new(initialTemplate:"main",rawResult:true)).Value.IntegerValue);
            Assert.Equal("42",Text(executable.CaptureTransform(op,Main,Options()).Value));
            Assert.Throws<NotSupportedException>(()=>op.Evaluate(new()));return 0;
        });
    }

    /// <summary>PX03 document match/global roles and named-template focus are independent.</summary>
    /// <param name="mode">Invocation arrangement.</param>
    [Theory][InlineData("distinct")][InlineData("shared")][InlineData("template")]
    public void PX03_IndependentRoles(string mode)
    {
        var source=Sheet("<xsl:value-of select=\"concat(string(/*),'|',$g)\"/>","<xsl:variable name='g' select='string(/*)'/><xsl:template match='/'><xsl:value-of select=\"concat(string(/*),'|',$g)\"/></xsl:template>");
        var executable=Compile(source);using var session=executable.Session;using var a=new Input("A");using var b=new Input("B");
        using var sa=session.OpenSnapshot(a,a).Value;using var sb=session.OpenSnapshot(b,b).Value;
        var invocation=new SchemaXsltInvocation(sa,mode=="shared"?sa:sb,mode=="template"?"main":null);
        var result=executable.RunCaptureTransform(mode=="shared"?[sa]:[sa,sb],invocation,Options());
        Assert.True(result.IsSuccess);Assert.Equal(mode=="shared"?"A|A":mode=="template"?"B|B":"A|B",Text(result.Result!));
    }

    /// <summary>PX04 absence of global context never falls back to the match input.</summary>
    [Fact] public void PX04_AbsentGlobalContext()
    {
        var executable=Compile(Sheet("<xsl:value-of select='$g'/>","<xsl:variable name='g' select='string(/*)'/>"));using var session=executable.Session;
        using var input=new Input();using var snapshot=session.OpenSnapshot(input,input).Value;
        var result=executable.RunCaptureTransform([snapshot],new(snapshot,initialTemplate:"main"),Options());
        Assert.False(result.IsSuccess);Assert.NotEmpty(result.Result!.ExecutionErrors);Assert.Null(result.WorkError);
    }

    /// <summary>PX05 named and default modes retain the existing host convention.</summary>
    /// <param name="mode">Initial mode name.</param>
    [Theory][InlineData("named")][InlineData("#default")][InlineData("#unnamed")]
    public void PX05_InitialModes(string mode)
    {
        var executable=Compile(Sheet("unused","<xsl:mode name='named'/><xsl:template match='/'>default</xsl:template><xsl:template match='/' mode='named'>named</xsl:template>"));using var session=executable.Session;
        using var input=new Input();using var snapshot=session.OpenSnapshot(input,input).Value;
        var result=executable.RunCaptureTransform([snapshot],new(snapshot,initialMode:mode),Options());
        Assert.True(result.IsSuccess);Assert.Equal(mode=="named"?"named":"default",Text(result.Result!));
    }

    /// <summary>PX06 unsupported compiler/assessor profiles refuse before borrowed assessor callbacks.</summary>
    /// <param name="mode">Unsupported static profile.</param>
    [Theory][InlineData("schema")][InlineData("include")][InlineData("assessor")][InlineData("root")]
    public void PX06_CompilerRefusal(string mode)
    {
        var compiler=new XsltCompiler();if(mode=="schema")compiler.SchemaAware=true;
        var source=mode=="include"?Sheet("Hello","<xsl:include href='file:///never-read.xsl'/>"):mode=="root"?"<n/>":Sheet();
        var result=SchemaXsltCompiler.Compile(source,Limits,compiler,mode=="assessor"?new PoisonAssessor():null);
        Assert.Equal(SchemaResultKind.Unsupported,result.Kind);
    }

    /// <summary>PX07 effective static values are frozen and override same-name dynamic bindings.</summary>
    [Fact] public void PX07_FrozenStatics()
    {
        var compiler=new XsltCompiler{StaticParameters=new Dictionary<(string,string),XdmValue>{[("v","")]=XdmValue.FromString("before")}};
        var executable=Compile(Sheet("<xsl:value-of select='$v'/>","<xsl:param name='v' static='yes' select=\"'default'\"/>"),compiler);using var session=executable.Session;
        compiler.StaticParameters[("v","")]=XdmValue.FromString("after");compiler.SchemaAware=true;
        var result=executable.RunCaptureTransform([],Main,Options(),new EvaluationContext().WithVariable("v",XdmValue.FromString("dynamic")));
        Assert.True(result.IsSuccess);Assert.Equal("before",Text(result.Result!));
    }

    /// <summary>PX08 all controls are explicit and duplicate selection/context sources refuse.</summary>
    /// <param name="mode">Invalid or missing option.</param>
    [Theory][InlineData("count")][InlineData("per")][InlineData("total")][InlineData("messages")][InlineData("time")][InlineData("operations")]
    [InlineData("zero-operations")][InlineData("context")][InlineData("template")][InlineData("mode")][InlineData("base")][InlineData("raw")]
    public void PX08_OptionsRefusal(string mode)
    {
        var executable=Compile();using var session=executable.Session;var options=Options();
        switch(mode){case "count":options.MaxSecondaryOutputCount=null;break;case "per":options.MaxBytesPerOutput=null;break;
        case "total":options.MaxTotalBytes=null;break;case "messages":options.MaxMessages=null;break;case "time":options.TimeLimit=null;break;
        case "operations":options.MaxEngineOperations=null;break;case "zero-operations":options.MaxEngineOperations=0;break;
        case "context":options.Context=new();break;case "template":options.InitialTemplate="main";break;case "mode":options.InitialMode="#default";break;
        case "base":options.BaseOutputUri="file:///ignored";break;}
        Assert.Throws<ArgumentException>(()=>executable.RunCaptureTransform([],new(initialTemplate:"main",rawResult:mode=="raw"),options));
    }

    /// <summary>PX09 zero count/byte/message caps are meaningful for empty text output.</summary>
    [Fact] public void PX09_ZeroCaps()
    {
        var executable=Compile(Sheet(""));using var session=executable.Session;var options=Options();
        options.MaxSecondaryOutputCount=0;options.MaxBytesPerOutput=0;options.MaxTotalBytes=0;options.MaxMessages=0;
        var result=executable.RunCaptureTransform([],Main,options);Assert.True(result.IsSuccess);Assert.Empty(result.Result!.Capture.Principal!.Bytes);
    }

    /// <summary>PX10 output/message caps classify failures and never imply overall success.</summary>
    /// <param name="mode">Capture limit channel.</param>
    [Theory][InlineData("per")][InlineData("total")][InlineData("count")][InlineData("messages")][InlineData("operations")]
    public void PX10_CaptureLimits(string mode)
    {
        var executable=Compile(Sheet(mode=="count"?"<xsl:result-document href='second'>secondary</xsl:result-document>Hello":mode=="messages"?"<xsl:message>one</xsl:message>Hello":"Hello"));using var session=executable.Session;var options=Options();
        if(mode=="per")options.MaxBytesPerOutput=1;else if(mode=="total")options.MaxTotalBytes=1;else if(mode=="count")options.MaxSecondaryOutputCount=0;
        else if(mode=="messages")options.MaxMessages=0;else options.MaxEngineOperations=1;
        var result=executable.RunCaptureTransform([],new(initialTemplate:"main",baseOutputUri:"file:///capture/principal"),options);
        Assert.False(result.IsSuccess);Assert.NotEmpty(result.Result!.Capture.Failures);Assert.Null(result.WorkError);Assert.Null(result.AdmissionError);
        if(mode!="messages")Assert.NotEmpty(result.Result.ExecutionErrors);
    }

    /// <summary>PX11 inherited and local controls both apply without weakening either.</summary>
    /// <param name="mode">Tighter gate source.</param>
    [Theory][InlineData("inherited-operations")][InlineData("local-operations")][InlineData("inherited-cancel")][InlineData("local-cancel")][InlineData("time")]
    public void PX11_ComposedControls(string mode)
    {
        var executable=Compile();using var session=executable.Session;using var cancellation=new CancellationTokenSource();cancellation.Cancel();
        var context=new EvaluationContext();var options=Options();
        if(mode=="inherited-operations")context.ExecutionGate=ExecutionGate.Create(default,null,0);
        else if(mode=="local-operations"){context.ExecutionGate=ExecutionGate.Create(default,null,100000);options.MaxEngineOperations=1;}
        else if(mode=="inherited-cancel")context.ExecutionGate=ExecutionGate.Create(cancellation.Token,null,null);
        else if(mode=="local-cancel")options.CancellationToken=cancellation.Token;
        else{context.ExecutionGate=ExecutionGate.Create(default,TimeSpan.FromMilliseconds(1),null);Thread.Sleep(20);}
        var result=executable.RunCaptureTransform([],Main,options,context);
        Assert.False(result.IsSuccess);Assert.NotEmpty(result.Result!.Capture.Failures);Assert.NotEmpty(result.Result.ExecutionErrors);
        Assert.Null(result.WorkError);
        if(mode.Contains("cancel"))Assert.Equal(XsltCaptureOutcome.Cancelled,result.Result.Capture.Outcome);
    }

    /// <summary>PX12 same-descriptor foreign executable operations cannot invoke this stylesheet.</summary>
    [Fact] public void PX12_ForeignExecutable()
    {
        var a=Compile();var b=Compile();using var first=a.Session;using var second=b.Session;
        Assert.Equal(first.Environment.StaticContextFingerprint,second.Environment.StaticContextFingerprint);
        second.Run([],op=>{Assert.Throws<ArgumentException>(()=>a.CaptureTransform(op,Main,Options()));return 0;});
    }

    /// <summary>PX13 membership and invocation-role failures precede root getters.</summary>
    /// <param name="mode">Wrong membership arrangement.</param>
    [Theory][InlineData("default")][InlineData("duplicate")][InlineData("foreign")][InlineData("missing-role")]
    public void PX13_SourceAdmission(string mode)
    {
        var executable=Compile();var other=Compile();using var session=executable.Session;using var foreign=other.Session;using var input=new Input();
        using var own=session.OpenSnapshot(input,input).Value;using var theirs=foreign.OpenSnapshot(input,input).Value;
        ImmutableArray<SchemaQuerySnapshot> members=mode switch{"default"=>default,"duplicate"=>[own,own],"foreign"=>[theirs],_=>[]};
        var result=executable.RunCaptureTransform(members,new(own,initialTemplate:"main"),Options());
        Assert.False(result.IsSuccess);Assert.Null(result.Result);Assert.Equal(0,input.Roots);
        if(mode=="missing-role")Assert.IsType<ArgumentException>(result.WorkError);else Assert.IsAssignableFrom<ArgumentException>(result.AdmissionError);
    }

    /// <summary>PX14 custom dynamic services, structured data and unadmitted nodes refuse.</summary>
    /// <param name="mode">Unsupported dynamic input.</param>
    [Theory][InlineData("loader")][InlineData("map")][InlineData("node")]
    public void PX14_ContextRefusal(string mode)
    {
        var executable=Compile();using var session=executable.Session;using var input=new Input();var context=new EvaluationContext();
        if(mode=="loader")context.DocumentLoader=_=>throw new Exception("loader poison");
        else context.WithVariable("v",mode=="map"?XdmValue.FromMap(new XdmMap()):XdmValue.FromNode(input.Node));
        var result=executable.RunCaptureTransform([],Main,Options(),context);Assert.False(result.IsSuccess);Assert.Null(result.Result);Assert.IsType<NotSupportedException>(result.WorkError);
    }

    /// <summary>PX15 public copying uses actual binding counts instead of private sixteen-item fixtures.</summary>
    [Fact] public void PX15_ActualCardinalities()
    {
        var executable=Compile();using var session=executable.Session;var context=new EvaluationContext();
        for(int i=0;i<40;i++){context.WithVariable("v"+i,XdmValue.FromString("value"));context.WithNamespace("p"+i,"urn:"+i);}
        var result=executable.RunCaptureTransform([],Main,Options(),context);Assert.True(result.IsSuccess);
    }

    /// <summary>PX16 secondary bytes are captured with output identities without writing files.</summary>
    [Fact] public void PX16_SecondaryOutputs()
    {
        var directory=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));var uri=new Uri(Path.Combine(directory,"principal.txt")).AbsoluteUri;
        var executable=Compile(Sheet("<xsl:result-document href='second.txt' method='text'>secondary</xsl:result-document>Hello"));using var session=executable.Session;
        var result=executable.RunCaptureTransform([],new(initialTemplate:"main",baseOutputUri:uri),Options());
        Assert.True(result.IsSuccess);Assert.Equal(uri,result.Result!.Capture.Principal!.ResolvedUri);
        Assert.Equal("secondary",Encoding.UTF8.GetString(Assert.Single(result.Result.Capture.SecondaryOutputs).Bytes.ToArray()));Assert.False(Directory.Exists(directory));
    }

    /// <summary>PX17 original swallowed principal serialization errors retain identity and nesting.</summary>
    [Fact] public void PX17_OriginalSerializationError()
    {
        var error=new AggregateException(new Exception("serializer observer"));var executable=Instrument(Sheet(),_=>throw error);using var session=executable.Session;
        var result=executable.RunCaptureTransform([],Main,Options());
        Assert.False(result.IsSuccess);Assert.Same(error,Assert.Single(result.Result!.ExecutionErrors));Assert.Null(result.WorkError);Assert.Single(error.InnerExceptions);
    }

    /// <summary>PX18 capture options are copied before the actual serializer runs.</summary>
    [Fact] public void PX18_CopiedOptions()
    {
        var options=Options();var executable=Instrument(Sheet(),_=>{options.MaxBytesPerOutput=0;options.MaxTotalBytes=0;options.MaxEngineOperations=0;});using var session=executable.Session;
        var result=executable.RunCaptureTransform([],Main,options);Assert.True(result.IsSuccess);Assert.Equal("Hello",Text(result.Result!));
        Assert.Throws<ArgumentException>(()=>executable.RunCaptureTransform([],Main,options));
    }

    /// <summary>PX19 resource attempts are actual fresh per-run closed-policy receipts.</summary>
    [Fact] public void PX19_PerRunReceipts()
    {
        var executable=Compile(Sheet("<xsl:value-of select=\"if($load) then string(doc('file:///never-read-public-xslt.xml')) else 'clean'\"/>","<xsl:param name='load' select='false()'/>"));using var session=executable.Session;
        var failed=executable.RunCaptureTransform([],Main,Options(),new EvaluationContext().WithVariable("load",XdmValue.FromBoolean(true)));
        Assert.False(failed.IsSuccess);Assert.NotEmpty(failed.Result!.ResourceReceipts);
        var clean=executable.RunCaptureTransform([],Main,Options());Assert.True(clean.IsSuccess);Assert.Empty(clean.Result!.ResourceReceipts);Assert.Equal("clean",Text(clean.Result));
    }

    /// <summary>PX20 raw cancellation keeps staged diagnostics but rejects overall delivery.</summary>
    [Fact] public void PX20_RawCancellation()
    {
        using var cancellation=new CancellationTokenSource();var executable=Instrument(Sheet(),_=>cancellation.Cancel());using var session=executable.Session;
        var result=executable.RunCaptureTransform([],Main,Options(),cancellationToken:cancellation.Token);
        Assert.False(result.IsSuccess);Assert.NotNull(result.Result);Assert.IsAssignableFrom<OperationCanceledException>(result.WorkError);
        Assert.Equal(XsltCaptureOutcome.Cancelled,result.Result!.Capture.Outcome);
    }

    /// <summary>PX21 real serialization retains source leases and detached output through close/cleanup failure.</summary>
    /// <returns>The completed serializer-tail ownership check.</returns>
    [Fact] public async Task PX21_SerializerTailAndCleanup()
    {
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        using var file=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        var executable=Instrument(Sheet(),_=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));Assert.True(file.CanWrite);file.WriteByte(42);});using var session=executable.Session;
        using var input=new Input(){Resource=file};using var snapshot=session.OpenSnapshot(input,input).Value;
        var cleanup=new AggregateException(new Exception("owner cleanup"));input.OnRelease=()=>throw cleanup;
        var task=Task.Run(()=>executable.RunCaptureTransform([snapshot],Main,Options()));
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));session.Dispose();input.Dispose();Assert.True(file.CanWrite);Assert.Equal(0,input.Releases);
            using var cancellation=new CancellationTokenSource();cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task.WaitAsync(cancellation.Token));Assert.False(task.IsCompleted);
            resume.Set();var result=await task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(result.IsSuccess);Assert.Equal("Hello",Text(result.Result!));Assert.IsType<ObjectDisposedException>(result.WorkError);
            Assert.Same(cleanup,Assert.Single(result.CleanupErrors));Assert.Equal(1,input.Releases);Assert.False(file.CanWrite);
        }
        finally{resume.Set();await task.WaitAsync(TimeSpan.FromSeconds(10));input.OnRelease=null;}
    }

    /// <summary>PX22 the public route retains exact existing serializer bytes across supported methods.</summary>
    /// <param name="method">Supported serializer method.</param>
    [Theory][InlineData("xml")][InlineData("html")][InlineData("xhtml")][InlineData("text")]
    public void PX22_SerializerFidelity(string method)
    {
        var source=Sheet("<out a='&amp;'>?</out>","<xsl:output method='"+method+"' encoding='UTF-16' byte-order-mark='yes' omit-xml-declaration='yes'/>")
            .Replace("<xsl:output method='text' encoding='UTF-8' byte-order-mark='no'/>","");
        var legacy=new XsltCompiler().Compile(source);var priorOptions=Options();priorOptions.InitialTemplate="main";
        var prior=legacy.CaptureTransform(null,priorOptions);
        var executable=Compile(source);using var session=executable.Session;var result=executable.RunCaptureTransform([],Main,Options());
        Assert.True(prior.IsSuccess);Assert.True(result.IsSuccess);Assert.Equal(prior.Principal!.Bytes,result.Result!.Capture.Principal!.Bytes);
    }

    /// <summary>PX23 document-root roles refuse admitted element roots without starting capture.</summary>
    /// <param name="global">Whether the element is the global context role.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void PX23_DocumentRootRoles(bool global)
    {
        var executable=Compile();using var session=executable.Session;using var input=new Input();
        var children=input.Node.Children().GetEnumerator();Assert.True(children.MoveNext());input.OverrideRoot=children.Current.NodeValue;
        using var snapshot=session.OpenSnapshot(input,input).Value;
        var result=executable.RunCaptureTransform([snapshot],global?new(globalContextInput:snapshot,initialTemplate:"main"):new(snapshot,initialTemplate:"main"),Options());
        Assert.False(result.IsSuccess);Assert.Null(result.Result);Assert.IsType<NotSupportedException>(result.WorkError);
    }

    /// <summary>PX24 ambiguous dynamic node ownership refuses before invocation root getters.</summary>
    [Fact] public void PX24_AmbiguousOwnership()
    {
        var executable=Compile();using var session=executable.Session;using var a=new Input(){All=true};using var b=new Input(){All=true};
        using var first=session.OpenSnapshot(a,a).Value;using var second=session.OpenSnapshot(b,b).Value;
        var result=executable.RunCaptureTransform([first,second],Main,Options(),new EvaluationContext().WithVariable("v",XdmValue.FromNode(a.Node)));
        Assert.False(result.IsSuccess);Assert.Null(result.Result);Assert.IsType<NotSupportedException>(result.WorkError);Assert.Equal(0,a.Roots);Assert.Equal(0,b.Roots);
    }

    /// <summary>PX25 compilation observes pre-cancellation before touching invalid stylesheet syntax.</summary>
    [Fact] public void PX25_CompileCancellation()
    {
        using var cancellation=new CancellationTokenSource();cancellation.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(()=>SchemaXsltCompiler.Compile("not XML",Limits,cancellationToken:cancellation.Token));
    }

    /// <summary>PX26 closed and pre-canceled owned runs report admission failures without a result.</summary>
    /// <param name="cancel">Whether to cancel rather than close.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void PX26_OwnedAdmissionRefusal(bool cancel)
    {
        var executable=Compile();using var session=executable.Session;using var cancellation=new CancellationTokenSource();
        if(cancel)cancellation.Cancel();else session.Dispose();
        var result=executable.RunCaptureTransform([],Main,Options(),cancellationToken:cancellation.Token);
        Assert.False(result.IsSuccess);Assert.Null(result.Result);Assert.Null(result.WorkError);
        if(cancel)Assert.IsAssignableFrom<OperationCanceledException>(result.AdmissionError);else Assert.IsType<ObjectDisposedException>(result.AdmissionError);
    }

    /// <summary>PX27 direct capture cannot replay an expired borrowed operation.</summary>
    [Fact] public void PX27_ExpiredOperation()
    {
        var executable=Compile();using var session=executable.Session;SchemaQueryOperation? retained=null;
        session.Run([],op=>{retained=op;Assert.Equal("Hello",Text(executable.CaptureTransform(op,Main,Options()).Value));return 0;});
        Assert.Throws<ObjectDisposedException>(()=>executable.CaptureTransform(retained!,Main,Options()));
    }

    /// <summary>PX28 the inherited-gate exception does not broaden public XQuery context policy.</summary>
    [Fact] public void PX28_QueryPolicyUnchanged()
    {
        using var session=Bosak.XQuery.Api.Schema.SchemaXQueryCompiler.Compile("1",Limits).Value;
        var context=new EvaluationContext(){ExecutionGate=ExecutionGate.Create(default,null,1000)};
        Assert.Throws<NotSupportedException>(()=>session.Run([],op=>op.Evaluate(context)));
    }

    /// <summary>PX29 completed secondary bytes survive a subsequent transform failure.</summary>
    [Fact] public void PX29_PartialSecondaryOutput()
    {
        var executable=Compile(Sheet("<xsl:result-document href='first.txt' method='text'>retained</xsl:result-document><xsl:sequence select=\"error(QName('urn:test','failure'),'after-output')\"/>"));using var session=executable.Session;
        var result=executable.RunCaptureTransform([],new(initialTemplate:"main",baseOutputUri:"file:///capture/main"),Options());
        Assert.False(result.IsSuccess);Assert.Equal(XsltCaptureOutcome.Partial,result.Result!.Capture.Outcome);
        Assert.Equal("retained",Encoding.UTF8.GetString(Assert.Single(result.Result.Capture.SecondaryOutputs).Bytes.ToArray()));
        Assert.NotEmpty(result.Result.ExecutionErrors);Assert.Null(result.WorkError);
    }

    /// <summary>PX30 serialization and multiple owner cleanups retain distinct original identities/order.</summary>
    [Fact] public void PX30_SerializationAndMultipleCleanupErrors()
    {
        SchemaQuerySnapshot? a=null,b=null;
        var serialization=new AggregateException(new Exception("serialization"));
        var first=new AggregateException(new Exception("first cleanup"));var second=new AggregateException(new Exception("second cleanup"));
        var executable=Instrument(Sheet(),_=>{a!.Dispose();b!.Dispose();throw serialization;});using var session=executable.Session;
        using var one=new Input();using var two=new Input();a=session.OpenSnapshot(one,one).Value;b=session.OpenSnapshot(two,two).Value;
        using var closeA=a;using var closeB=b;one.OnRelease=()=>throw first;two.OnRelease=()=>throw second;
        var result=executable.RunCaptureTransform([a,b],Main,Options());
        Assert.False(result.IsSuccess);Assert.Same(serialization,Assert.Single(result.Result!.ExecutionErrors));
        Assert.IsType<ObjectDisposedException>(result.WorkError);Assert.Null(result.AdmissionError);
        Assert.Equal(new Exception[]{second,first},result.CleanupErrors);Assert.Equal(1,one.Releases);Assert.Equal(1,two.Releases);
    }

    /// <summary>PX31 records bounded cold/warm caller allocations and latency without selecting budgets.</summary>
    [Fact] public void PX31_BoundedCostObservation()
    {
        long coldBytes=GC.GetAllocatedBytesForCurrentThread(),coldStart=Stopwatch.GetTimestamp();
        var executable=Compile();using var session=executable.Session;
        Assert.True(executable.RunCaptureTransform([],Main,Options()).IsSuccess);
        long coldTicks=Stopwatch.GetTimestamp()-coldStart;coldBytes=GC.GetAllocatedBytesForCurrentThread()-coldBytes;
        const int runs=8;long warmBytes=GC.GetAllocatedBytesForCurrentThread(),warmStart=Stopwatch.GetTimestamp();
        for(int i=0;i<runs;i++)Assert.True(executable.RunCaptureTransform([],Main,Options()).IsSuccess);
        long warmTicks=Stopwatch.GetTimestamp()-warmStart;warmBytes=GC.GetAllocatedBytesForCurrentThread()-warmBytes;
        _output.WriteLine($"Public XSLT cost: cold caller bytes {coldBytes}; cold ticks {coldTicks}; warm runs {runs}; warm caller bytes {warmBytes}; warm ticks {warmTicks}; frequency {Stopwatch.Frequency}");
        Assert.True(coldBytes>0);Assert.True(warmBytes>0);Assert.True(coldTicks>0);Assert.True(warmTicks>0);
    }
}
