"""Build and run the isolated prototype, preserving the qualified engine cohort."""
from pathlib import Path
import sys,subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/exact-numeric-prototype';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
layout=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
core=next(v for v in layout.values() if Path(v['path']).stem=='Bosak.XPath.Core')
(W/'Program.cs').write_bytes((P/'tools/Prototype.cs.txt').read_bytes())
(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup><Reference Include="Bosak.XPath.Core"><HintPath>'+core['path']+'</HintPath></Reference></ItemGroup></Project>')
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Probe.dll'],f'run-{n}.json')
a=json.loads((E/'run-0.json').read_text());b=json.loads((E/'run-1.json').read_text());assert a==b
assert a['checks']==31 and a['carrierBytes']==a['productBytes']==48
assert a['loadedCoreSha256']==core['sha256']
assert a['bytesPerOperation']['big-read']==a['bytesPerOperation']['big-read-evidence']==0
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
(E/'INPUTS.json').write_text(json.dumps({'qualifiedEngines':layout,'prototypeSha256':sha(P/'tools/Prototype.cs.txt'),'engineChanges':False,'repeatIdentical':True,'productionIntegration':False},indent=2)+'\n')
print('Prototype checks, repeat measurements and unchanged qualified engine hashes verified.')
