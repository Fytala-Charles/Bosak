"""Exercise actual private compiler factories, provider binding and real operation lifetimes."""
from pathlib import Path
import json,hashlib,subprocess,sys
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/private-host-factories/Probe';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
origin=ROOT/'docs/foundation-typed-host-inputs/tools/Probe.cs.txt';s=origin.read_text(encoding='utf-8')
start=s.index('sealed class Issuer:');end=s.index('sealed class Bundle',start)
s=s[:start]+'''sealed class Issuer:ISchemaEvidenceIssuer,ISchemaSessionProvider {
 internal readonly object Origin=new();internal bool Available=true;internal int Binds,IssuerReads;
 public bool OwnsValue(ISchemaIssuedScalar value)=>false;public bool OwnsType(object type)=>false;
 public bool OwnsAssessment(object origin)=>ReferenceEquals(origin,Origin);public bool IsSameType(object a,object b)=>ReferenceEquals(a,b);
 sealed class Binding(Issuer owner,SchemaQueryEnvironment environment):ISchemaSessionBinding {
  public SchemaQueryEnvironment Environment=>environment;
  public ISchemaEvidenceIssuer Issuer {get{owner.IssuerReads++;return owner;}}
  public void ThrowIfUnavailable(){if(!owner.Available)throw new ObjectDisposedException("provider");}
 }
 public ISchemaSessionBinding Bind(SchemaQueryEnvironment environment){Binds++;return new Binding(this,environment);}
}
'''+s[end:]
start=s.index('static readonly Type HostBridge=');end=s.index('static SchemaOperationInputs? typedSink;',start)
s=s[:start]+'''delegate bool XPathFactory(string source,CompileOptions options,ISchemaSessionProvider provider,int snapshots,int values,out SchemaQuerySession? session,out string? unsupported,CancellationToken cancellation);
 delegate bool XQueryFactory(string source,XQueryCompiler compiler,bool xml11,ISchemaSessionProvider provider,int snapshots,int values,out SchemaQuerySession? session,out string? unsupported,CancellationToken cancellation);
 delegate bool XsltFactory(string source,XsltCompiler compiler,string? baseUri,IXdmNode? match,IXdmNode? global,string? template,Func<IXdmNode,bool> owns,ISchemaSessionProvider provider,int snapshots,int values,out SchemaQuerySession? session,out string? unsupported,CancellationToken cancellation);
 static readonly Type HostBridge=typeof(XsltExecutable).Assembly.GetType("Bosak.Xslt.Api.SchemaHostSessionCompilation")!;
 static readonly MethodInfo Bridge=HostBridge.GetMethod("TryCompileXslt",BindingFlags.Static|BindingFlags.NonPublic)!;
 static readonly XPathFactory CompileXPath=HostBridge.GetMethod("TryCompileXPath",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<XPathFactory>();
 static readonly XQueryFactory CompileXQuery=HostBridge.GetMethod("TryCompileXQuery",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<XQueryFactory>();
 static readonly XsltFactory CompileXslt=Bridge.CreateDelegate<XsltFactory>();
 static Bosak.XPath.Runtime.Resources.ControlledResourcePolicy? resourceSink;
 '''+s[end:]
start=s.index('static SchemaQuerySession Compile(');end=s.index('internal static async Task Run()',start)
s=s[:start]+'''static SchemaQuerySession Compile(string host,Issuer issuer,Bundle bundle,bool failure=false,bool structured=false) {
  SchemaQuerySession? session;string? reason;bool supported;
  if(host=="XPath")supported=CompileXPath(failure?"error()":structured?"$n":"($v,data($n),string(/*))",new CompileOptions(),issuer,4,16,out session,out reason,default);
  else if(host=="XQuery")supported=CompileXQuery("declare variable $v external;declare variable $n external;"+(failure?"error()":structured?"$n":"($v,data($n),string(/*))"),new XQueryCompiler(),false,issuer,4,16,out session,out reason,default);
  else supported=CompileXslt(Sheet(failure?"error()":structured?"$n":"($v,data($n),string(/*),$g)"),new XsltCompiler(),null,bundle.Match,bundle.Global,"main",bundle.Owns,issuer,4,16,out session,out reason,default);
  if(!supported)throw new Exception("Unexpected factory refusal: "+reason);return session!;
 }
 static string Read(SchemaQuerySession session,Issuer issuer){using var snapshot=session.OpenSnapshot(session.Environment,new object(),issuer.Origin,()=>{});return session.Run(session.Environment,snapshot,op=>{var result=new Result(op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),_=>false,16,16),16));return result.Drain();});}
 '''+s[end:]
needle='Check(prefix+"caller-preserved-success",Inputs.Observe(caller)==before);'
assert needle in s;s=s.replace(needle,needle+'await Refuse<NotSupportedException>(prefix+"factory-raw-context-refused",()=>Task.FromResult(session.Run(session.Environment,snapshot,op=>op.Evaluate(new EvaluationContext()))));')
at=s.index('  var costBundle=')
extras='''  {
   var provider=new Issuer();var namespaces=new Dictionary<string,string>{{"p","urn:before"}};
   if(!CompileXPath("namespace-uri-from-QName(xs:QName('p:x'))",new CompileOptions{Namespaces=namespaces},provider,4,16,out var session,out var reason,default))throw new Exception(reason);
   using(session!){namespaces["p"]="urn:after";Check("capture:XPath-namespace-freeze",Read(session!,provider)=="urn:before"&&session!.Environment.Namespaces["p"]=="urn:before");}
  }
  {
   const string query="declare namespace p='urn:prolog';import module namespace m='urn:m';(m:f(),namespace-uri-from-QName(xs:QName('p:x')))";
   const string module="module namespace m='urn:m';declare function m:f(){7};";
   var compiler=new XQueryCompiler().WithNamespace("p","urn:caller").WithModule("urn:m",module);var provider=new Issuer();
   if(!CompileXQuery(query,compiler,false,provider,4,16,out var session,out var reason,default))throw new Exception(reason);
   using(session!){compiler.WithNamespace("p","urn:after").WithModule("urn:m","module namespace m='urn:m';declare function m:f(){9};");Check("capture:XQuery-module-prolog-freeze",Read(session!,provider)=="7|urn:prolog"&&session!.Environment.Namespaces["p"]=="urn:prolog");
    if(!CompileXQuery(query,new XQueryCompiler().WithNamespace("p","urn:caller").WithModule("urn:m",module),false,provider,4,16,out var same,out reason,default))throw new Exception(reason);
    using(same!){Check("capture:equal-descriptors-distinct-identities",same!.Environment.Fingerprint==session!.Environment.Fingerprint&&!ReferenceEquals(same.Environment,session.Environment));using var snapshot=session.OpenSnapshot(session.Environment,new object(),provider.Origin,()=>{});await Refuse<ArgumentException>("capture:equal-fingerprint-cannot-admit",()=>Task.FromResult(session.Run(same.Environment,snapshot,_=>0)));}
   }
  }
  {
   const string style="<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:param name='v' static='yes' select='0'/><xsl:template name='main'><xsl:sequence select='$v'/></xsl:template></xsl:stylesheet>";
   var provider=new Issuer();var values=new Dictionary<(string,string),XdmValue>{{("v",""),XdmValue.FromInteger(7)}};var compiler=new XsltCompiler{StaticParameters=values};
   if(!CompileXslt(style,compiler,null,null,null,"main",_=>false,provider,4,16,out var session,out var reason,default))throw new Exception(reason);
   using(session!){values[("v","")]=XdmValue.FromInteger(9);compiler.SchemaAware=true;Check("capture:XSLT-static-freeze",Read(session!,provider)=="7");}
   var negative=new XsltCompiler{StaticParameters=new(){{("v",""),XdmValue.FromDouble(-0.0)}}};var positive=new XsltCompiler{StaticParameters=new(){{("v",""),XdmValue.FromDouble(0.0)}}};
   if(!CompileXslt(style,negative,null,null,null,"main",_=>false,provider,4,16,out var minus,out reason,default)||!CompileXslt(style,positive,null,null,null,"main",_=>false,provider,4,16,out var plus,out reason,default))throw new Exception(reason);
   using(minus!)using(plus!)Check("capture:static-double-sign-descriptor",minus!.Environment.Fingerprint!=plus!.Environment.Fingerprint);
  }
  {
   var provider=new Issuer();bool supported=CompileXPath("1",new CompileOptions{SchemaSet=new System.Xml.Schema.XmlSchemaSet()},provider,4,16,out var session,out var reason,default);Check("refusal:XPath-schema-before-bind",!supported&&session is null&&!string.IsNullOrEmpty(reason)&&provider.Binds==0);
   supported=CompileXQuery("import schema namespace s='urn:s';1",new XQueryCompiler(),false,provider,4,16,out session,out reason,default);Check("refusal:XQuery-schema-before-bind",!supported&&session is null&&!string.IsNullOrEmpty(reason)&&provider.Binds==0);
   supported=CompileXslt("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template name='main' use-when='true()'/></xsl:stylesheet>",new XsltCompiler(),null,null,null,"main",_=>false,provider,4,16,out session,out reason,default);Check("refusal:XSLT-static-profile-before-bind",!supported&&session is null&&!string.IsNullOrEmpty(reason)&&provider.Binds==0);
   bool failed=false;try{CompileXPath("(",new CompileOptions(),provider,4,16,out session,out reason,default);}catch{failed=true;}Check("refusal:compile-error-before-bind",failed&&provider.Binds==0);
  }
  foreach(var host in new[]{"XPath","XQuery","XSLT"}){
   var provider=new Issuer();var bundle=new Bundle();using var session=Compile(host,provider,bundle);int releases=0;using var snapshot=session.OpenSnapshot(session.Environment,new object(),provider.Origin,()=>releases++);
   var caller=new EvaluationContext().WithFocus(XdmValue.FromNode(bundle.Match),1,1).WithVariable("v",XdmValue.FromInteger(42)).WithVariable("n",XdmValue.FromNode(bundle.Typed));
   var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
   var raw=session.RunAsync(session.Environment,snapshot,async op=>{var result=new Result(op.EvaluateAtomic(op.CaptureInputs(caller,bundle.Owns,16,16),16));entered.SetResult();await resume.Task;await Refuse<ObjectDisposedException>("owner:"+host+":revoked-result",()=>Task.FromResult(result.Drain()));return 0;});
   await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));provider.Available=false;Check("owner:"+host+":raw-ownership-held",!raw.IsCompleted&&releases==0);resume.SetResult();await raw.WaitAsync(TimeSpan.FromSeconds(10));session.Dispose();Check("owner:"+host+":cleanup-with-revoked-provider",releases==1);
  }
'''
s=s[:at]+extras+s[at:]
old='if(!SchemaSessionCompilation.TryCompile("$v",new CompileOptions(),costIssuer,2,16,out var costSession,out var why))'
assert old in s;s=s.replace(old,'if(!CompileXPath("$v",new CompileOptions(),costIssuer,2,16,out var costSession,out var why,default))')
s=s.replace('GC.KeepAlive(baselineSink);','bytesPerOperation["closed-resource-policy-per-evaluation"]=Cost(()=>resourceSink=SchemaProviderSessionBinding.ClosedResources());GC.KeepAlive(resourceSink);GC.KeepAlive(baselineSink);')
s=s.replace('Production typed capture/result paths and direct bound hosts; fixture static producers, composite snapshot and test issuer; setup reflection only, no production facade.','Actual private compiler/binding factories, typed execution and direct hosts; test provider and composite snapshot; setup reflection only, no public facade.')
assert 'new(environment' not in s and 'Bridge.Invoke(' not in s
s='\n'.join(line.rstrip() for line in s.splitlines())+'\n';(P/'tools/Probe.cs.txt').write_text(s,encoding='utf-8');(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
node=ROOT/'docs/foundation-real-host-session/tools/TypedNode.cs.txt';(W/'TypedNode.cs').write_bytes(node.read_bytes())
layout=json.loads((E/'INPUTS.json').read_text())['engines'];refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in layout.items())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W.parent/'NuGet.Config','-p:NuGetAudit=false'],'probe-build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'probe-{n}.json')
a=json.loads((E/'probe-0.json').read_text());assert a==json.loads((E/'probe-1.json').read_text()) and all(c['passed'] for c in a['rows'])
for b in a['bindings']:assert b['sha256']==(sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll') if b['name']=='Bosak.XPath.Runtime.Tests' else layout[b['name']+'.dll']['sha256'])
(E/'PROBE-INPUTS.json').write_text(json.dumps({'baseFixtureSha256':sha(origin),'sourceSha256':sha(P/'tools/Probe.cs.txt'),'nodeSourceSha256':sha(node),'assemblySha256':sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'),'actualFactories':True,'testProvider':True,'setupReflectionOnly':True,'checks':a['checks']},indent=2)+'\n')
print('Actual factories/provider binding: '+str(a['checks'])+' checks twice; repeated costs and loaded hashes verified.')
