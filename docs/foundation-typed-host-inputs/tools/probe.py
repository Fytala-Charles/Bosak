"""Replay the 75 real-session controls using production typed paths and direct evaluators."""
from pathlib import Path
import json,hashlib,subprocess,sys
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/typed-host-inputs/Probe';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
origin=ROOT/'docs/foundation-real-host-session/tools/Probe.cs.txt';s=origin.read_text(encoding='utf-8')
start=s.index('sealed class Inputs {');end=s.index('sealed class Result(',start)
baseline=s[start:end].replace('sealed class Inputs {','sealed class BaselineInputs {').replace('internal Inputs(','internal BaselineInputs(')
typed='''sealed class Inputs {
 internal readonly SchemaOperationInputs Value;
 internal Inputs(EvaluationContext caller,Bundle bundle){Value=caller.CaptureSchemaInputs(bundle.Owns,16,16);}
 internal Inputs(SchemaOperationInputs value){Value=value;}
 internal EvaluationContext Create()=>Value.CreateContext();
 internal static string Observe(EvaluationContext caller)=>BaselineInputs.Observe(caller);
 internal static object[] Policy=>BaselineInputs.Policy;
}
'''
s=s[:start]+baseline+typed+s[end:]
start=s.index('sealed class Result(');end=s.index('static class Fixture',start)
s=s[:start]+'''sealed class Result(SchemaAtomicResult result) {
 internal string Drain(){var values=new List<string>();result.ForEachAtomic(v=>values.Add(v.ToString()));return string.Join("|",values);}
}
'''+s[end:]
old='static readonly MethodInfo Bridge=typeof(XsltExecutable).GetMethod("TransformWithExplicitContext",BindingFlags.Instance|BindingFlags.NonPublic)!;'
new='''static readonly Type HostBridge=typeof(XsltExecutable).Assembly.GetType("Bosak.Xslt.Api.SchemaHostExecution")!;
 static readonly MethodInfo Bridge=HostBridge.GetMethod("BindXslt",BindingFlags.Static|BindingFlags.NonPublic)!;
 static readonly Func<XQueryExecutable,Func<EvaluationContext,XdmValue>> BindQuery=HostBridge.GetMethod("BindXQuery",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<Func<XQueryExecutable,Func<EvaluationContext,XdmValue>>>();
 static readonly Func<XsltExecutable,IXdmNode?,IXdmNode?,string?,Func<IXdmNode,bool>,Func<EvaluationContext,XdmValue>> BindTransform=Bridge.CreateDelegate<Func<XsltExecutable,IXdmNode?,IXdmNode?,string?,Func<IXdmNode,bool>,Func<EvaluationContext,XdmValue>>>();
 static SchemaOperationInputs? typedSink;'''
assert old in s;s=s.replace(old,new)
s=s.replace('string host,Issuer issuer,Inputs inputs,Bundle bundle','string host,Issuer issuer,Bundle bundle').replace('Compile(host,issuer,admitted,bundle','Compile(host,issuer,bundle')
s=s.replace('new(environment,ctx=>executable.Evaluate(Inputs.CreateQuery(ctx)),issuer,4,16)','new(environment,BindQuery(executable),issuer,4,16)')
old='return new(environment,ctx=>{try{return (XdmValue)Bridge.Invoke(xslt,new object?[]{bundle.Match,bundle.Global,"main",ctx})!;}catch(TargetInvocationException error)when(error.InnerException!=null){throw error.InnerException;}},issuer,4,16);'
assert old in s;s=s.replace(old,'return new(environment,BindTransform(xslt,bundle.Match,bundle.Global,"main",bundle.Owns),issuer,4,16);')
s=s.replace('new Result(op,op.Evaluate(admitted.Create()))','new Result(op.EvaluateAtomic(admitted.Value,16))').replace('op=>op.Evaluate(admitted.Create())','op=>op.EvaluateAtomic(admitted.Value,16)')
needle='await Refuse<ArgumentException>(prefix+"foreign-snapshot",()=>Task.FromResult(session.Run(session.Environment,other,_=>0)));'
assert needle in s;s=s.replace(needle,needle+'await Refuse<ArgumentException>(prefix+"foreign-session-input-capture",()=>Task.FromResult(foreign.Run(foreign.Environment,other,op=>op.EvaluateAtomic(admitted.Value,16))));')
needle='await Refuse<ArgumentException>(prefix+"foreign-environment",()=>Task.FromResult(session.Run(new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"foreign"),snapshot,_=>0)));'
assert needle in s;s=s.replace(needle,needle+'using(var second=session.OpenSnapshot(session.Environment,new object(),issuer.Origin,()=>{})){await Refuse<ArgumentException>(prefix+"foreign-snapshot-input-capture",()=>Task.FromResult(session.Run(session.Environment,second,op=>op.EvaluateAtomic(admitted.Value,16))));}')
old='''var admitted=new Inputs(caller,bundle);caller.WithVariable("v",XdmValue.FromInteger(99));caller.WithNamespace("p","urn:after");var before=Inputs.Observe(caller);
   var issuer=new Issuer();using var session=Compile(host,issuer,bundle);int releases=0;using var snapshot=session.OpenSnapshot(session.Environment,bundle,issuer.Origin,()=>Interlocked.Increment(ref releases));'''
new='''var issuer=new Issuer();using var session=Compile(host,issuer,bundle);int releases=0;using var snapshot=session.OpenSnapshot(session.Environment,bundle,issuer.Origin,()=>Interlocked.Increment(ref releases));
   var admitted=session.Run(session.Environment,snapshot,op=>new Inputs(op.CaptureInputs(caller,bundle.Owns,16,16)));caller.WithVariable("v",XdmValue.FromInteger(99));caller.WithNamespace("p","urn:after");var before=Inputs.Observe(caller);'''
assert old in s;s=s.replace(old,new)
needle='failed.Run(failed.Environment,s,op=>op.EvaluateAtomic(admitted.Value,16))'
assert needle in s;s=s.replace(needle,'failed.Run(failed.Environment,s,op=>op.EvaluateAtomic(op.CaptureInputs(admitted.Create(),bundle.Owns,16,16),16))')
needle='structured.Run(structured.Environment,nodeSnapshot,op=>new Result(op.EvaluateAtomic(admitted.Value,16)).Drain())'
assert needle in s;s=s.replace(needle,'structured.Run(structured.Environment,nodeSnapshot,op=>new Result(op.EvaluateAtomic(op.CaptureInputs(admitted.Create(),bundle.Owns,16,16),16)).Drain())')
needle='cleanupSession.Run<int>(cleanupSession.Environment,cleanup,op=>{_ = new Result(op.EvaluateAtomic(admitted.Value,16)).Drain();'
assert needle in s;s=s.replace(needle,'cleanupSession.Run<int>(cleanupSession.Environment,cleanup,op=>{_ = new Result(op.EvaluateAtomic(op.CaptureInputs(admitted.Create(),bundle.Owns,16,16),16)).Drain();')
old='var bytesPerOperation=new Dictionary<string,long>{["fixture-reflective-input-capture"]=Cost(()=>inputSink=new Inputs(costCaller,costBundle)),["fresh-evaluation-context"]=Cost(()=>contextSink=captured.Create()),["xquery-context-wrapper-copy"]=Cost(()=>querySink=Inputs.CreateQuery(child))};'
new='''Func<IXdmNode,bool> costOwner=costBundle.Owns;
  BaselineInputs? baselineSink=null;
  var bytesPerOperation=new Dictionary<string,long>{["fixture-reflective-input-capture"]=Cost(()=>baselineSink=new BaselineInputs(costCaller,costBundle)),["typed-production-input-capture"]=Cost(()=>typedSink=costCaller.CaptureSchemaInputs(costOwner,16,16)),["fresh-evaluation-context"]=Cost(()=>contextSink=captured.Create()),["old-xquery-context-wrapper-copy"]=Cost(()=>querySink=BaselineInputs.CreateQuery(child))};
  var costIssuer=new Issuer();if(!SchemaSessionCompilation.TryCompile("$v",new CompileOptions(),costIssuer,2,16,out var costSession,out var why))throw new Exception(why);
  using(costSession!){using var costSnapshot=costSession!.OpenSnapshot(costSession.Environment,new object(),costIssuer.Origin,()=>{});costSession.Run(costSession.Environment,costSnapshot,op=>{bytesPerOperation["typed-production-operation-capture"]=Cost(()=>typedSink=op.CaptureInputs(costCaller,costOwner,16,16));var result=op.EvaluateAtomic(op.CaptureInputs(costCaller,costOwner,16,16),16);bytesPerOperation["guarded-single-atomic-visitation"]=Cost(()=>result.ForEachAtomic(static _=>{}));return 0;});}
  GC.KeepAlive(baselineSink);'''
assert old in s;s=s.replace(old,new)
s=s.replace('Actual private session/kernel; fixture host producers, composite snapshot, bounded fresh-context input/result adapters and test issuer; no production facade.','Production typed capture/result paths and direct bound hosts; fixture static producers, composite snapshot and test issuer; setup reflection only, no production facade.')
s=s.replace('static Inputs? inputSink;','').replace('GC.KeepAlive(inputSink);','GC.KeepAlive(typedSink);')
assert 'Bridge.Invoke(' not in s and 'op.Evaluate(admitted.Create())' not in s
s='\n'.join(line.rstrip() for line in s.splitlines())+'\n'
(P/'tools/Probe.cs.txt').write_text(s,encoding='utf-8');(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
typedNode=ROOT/'docs/foundation-real-host-session/tools/TypedNode.cs.txt';(W/'TypedNode.cs').write_bytes(typedNode.read_bytes())
layout=json.loads((E/'INPUTS.json').read_text())['engines'];refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in layout.items())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W.parent/'NuGet.Config','-p:NuGetAudit=false'],'probe-build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'probe-{n}.json')
a=json.loads((E/'probe-0.json').read_text());assert a==json.loads((E/'probe-1.json').read_text()) and a['checks']==81 and all(r['passed'] for r in a['rows'])
engines={Path(n).stem:v for n,v in layout.items()}
for b in a['bindings']:assert b['sha256']==(sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll') if b['name']=='Bosak.XPath.Runtime.Tests' else engines[b['name']]['sha256'])
(E/'PROBE-INPUTS.json').write_text(json.dumps({'baseFixtureSha256':sha(origin),'probeSourceSha256':sha(P/'tools/Probe.cs.txt'),'typedNodeSourceSha256':sha(typedNode),'probeAssemblySha256':sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'),'setupReflectionOnly':True,'perEvaluationReflection':False,'typedProductionCaptureAndResult':True},indent=2)+'\n')
print('Production typed paths: 81 checks twice, direct host delegates and repeated allocation costs verified.')
