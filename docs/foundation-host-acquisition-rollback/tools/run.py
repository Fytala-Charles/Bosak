"""Qualify test-host resource acquisition against unchanged production 1.42.0."""
from pathlib import Path
import hashlib,json,subprocess,sys
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=R/'.guard-tmp/host-acquisition-rollback';W.mkdir(exist_ok=True)
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
base=R/'docs/foundation-xslt-source-membership';source=json.loads((base/'evidence/SOURCE.json').read_text());layout=json.loads((base/'evidence/INPUTS.json').read_text())['engines']
for name,digest in source['files'].items():assert sha(R/name)==digest,name
for name,v in layout.items():assert sha(Path(v['path']))==v['sha256'],name
(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes());(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in layout.items())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'probe-{n}.json')
a=json.loads((E/'probe-0.json').read_text());assert a==json.loads((E/'probe-1.json').read_text()) and a['checks']==len(a['rows'])==57 and all(x['passed'] for x in a['rows'])
assembly=sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll')
for b in a['bindings']:assert b['sha256']==(assembly if b['name']=='Bosak.XPath.Runtime.Tests' else layout[b['name']+'.dll']['sha256'])
(E/'INPUTS.json').write_text(json.dumps({'productionVersion':'1.42.0','productionRevision':subprocess.check_output(['git','rev-parse','b2ca55fc']).decode().strip(),'productionChanges':False,'sourceSha256':sha(P/'tools/Probe.cs.txt'),'probeAssemblySha256':assembly,'baselineSourceReceiptSha256':sha(base/'evidence/SOURCE.json'),'baselineResultsSha256':sha(base/'evidence/RESULTS.json'),'baselineApiReceiptSha256':sha(base/'evidence/public-api-0.json'),'engines':layout,'checks':a['checks'],'setupReflectionOnly':True},indent=2)+'\n')
print('57 acquisition/rollback controls passed twice across all three compiled hosts; production source and loaded engine hashes unchanged.')
