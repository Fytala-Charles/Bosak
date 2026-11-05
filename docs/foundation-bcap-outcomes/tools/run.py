"""Build and run bounded actual capture probes; negative controls are caught."""
from pathlib import Path
import subprocess,sys,json,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-outcomes-v3'
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:
  result=subprocess.run([str(a) for a in args],cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if result.returncode:raise RuntimeError('Caught failure; see '+name)
run(['dotnet','build',W/'src/Bosak.Xslt/Bosak.Xslt.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'candidate-build.txt')
H=W/'Host';H.mkdir(exist_ok=True)
(H/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
refs=''.join('<Reference Include="'+p.stem+'"><HintPath>'+str(p)+'</HintPath></Reference>' for p in (W/'src/Bosak.Xslt/bin/Release/net10.0').glob('Bosak.*.dll'))
(H/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
run(['dotnet','build',H/'Host.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'probe-build.txt')
for n in range(2):run(['dotnet',H/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'],f'probe-{n}.json')
a=json.loads((E/'probe-0.json').read_text());b=json.loads((E/'probe-1.json').read_text());assert a['rows']==b['rows'] and a['passed'] and b['passed']
pins={p.name:{'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in (H/'bin/Release/net10.0').glob('Bosak.*.dll')}
for observed in a['bindings']:assert pins[observed['name']+'.dll']['sha256']==observed['sha256']
(E/'BINARY-PINS.json').write_text(json.dumps(pins,indent=2)+'\n')
print('Actual isolated capture controls passed twice; loaded binary hashes verified.')
