"""Run actual compiled host controls under an existing private test identity."""
from pathlib import Path
import json,subprocess,sys,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/xslt-source-membership/HostProbe';W.mkdir(exist_ok=True)
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in i['engines'].items())
(W/'Program.cs').write_bytes((P/'tools/HostProbe.cs.txt').read_bytes())
(W/'HostProbe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',W/'HostProbe.csproj','-c','Release','--configfile',W.parent/'NuGet.Config','-p:NuGetAudit=false'],'host-probe-build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'host-probe-{n}.json')
a=json.loads((E/'host-probe-0.json').read_text());assert a==json.loads((E/'host-probe-1.json').read_text()) and a['passed'] and a['checks']==18
assembly=sha(W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll')
for b in a['bindings']:assert b['sha256']==(assembly if b['name']=='Bosak.XPath.Runtime.Tests' else i['engines'][b['name']+'.dll']['sha256'])
(E/'HOST-PROBE-INPUTS.json').write_text(json.dumps({'sourceSha256':sha(P/'tools/HostProbe.cs.txt'),'probeAssemblySha256':assembly,'setupReflectionOnly':True,'checks':a['checks'],'assertions':a['assertions']},indent=2)+'\n')
print('XSLT membership controls passed twice; '+str(a['assertions'])+' assertions per run; actual loaded hashes verified.')
