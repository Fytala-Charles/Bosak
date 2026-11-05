"""Qualify independent owners against the sealed production factory cohort."""
from pathlib import Path
import sys,json,hashlib,subprocess
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/independent-owners/Probe';W.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
layout=json.loads((ROOT/'docs/foundation-private-host-factories/evidence/INPUTS.json').read_text())['engines']
for n,v in layout.items():assert sha(Path(v['path']))==v['sha256'],n
(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
node=ROOT/'docs/foundation-real-host-session/tools/TypedNode.cs.txt';(W/'TypedNode.cs').write_bytes(node.read_bytes())
refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in layout.items())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',ROOT/'.guard-tmp/private-host-factories/NuGet.Config','-p:NuGetAudit=false'],'build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'probe-{n}.json')
a=json.loads((E/'probe-0.json').read_text());assert a==json.loads((E/'probe-1.json').read_text()) and all(c['passed'] for c in a['rows'])
for b in a['bindings']:assert b['sha256']==(sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll') if b['name']=='Bosak.XPath.Runtime.Tests' else layout[b['name']+'.dll']['sha256'])
(E/'INPUTS.json').write_text(json.dumps({'sourceRevision':'f72db224','productionVersion':'1.39.0','engineChanges':False,'sourceSha256':sha(P/'tools/Probe.cs.txt'),'nodeSourceSha256':sha(node),'probeAssemblySha256':sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'),'baseProbeSha256':sha(ROOT/'docs/foundation-private-host-factories/tools/Probe.cs.txt'),'checks':a['checks'],'independentChecks':len([c for c in a['rows'] if c['id'].startswith('independent:')]),'engines':layout},indent=2)+'\n')
print('Independent owners: '+str(a['checks'])+' checks passed twice.')
