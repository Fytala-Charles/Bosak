"""Measure lease-set bookkeeping against the rebuilt runtime; no workload budget selected."""
from pathlib import Path
import json,subprocess,sys,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/operation-lease-set/Cost';W.mkdir(exist_ok=True)
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in i['engines'].items())
(W/'Program.cs').write_bytes((P/'tools/Cost.cs.txt').read_bytes())
(W/'Cost.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'Cost.csproj','-c','Release','--configfile',W.parent/'NuGet.Config','-p:NuGetAudit=false'],'cost-build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'cost-{n}.json')
a=json.loads((E/'cost-0.json').read_text());assert a==json.loads((E/'cost-1.json').read_text()) and a['runtimeSha256']==i['engines']['Bosak.XPath.Runtime.dll']['sha256']
(E/'COST-INPUTS.json').write_text(json.dumps({'sourceSha256':sha(P/'tools/Cost.cs.txt'),'assemblySha256':sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll')},indent=2)+'\n')
print(json.dumps(a['rows']))
