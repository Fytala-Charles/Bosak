"""Run unchanged probe bytes with legacy and isolated candidate XSLT binaries."""
from pathlib import Path
import sys,json,hashlib,subprocess
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-explicit-context';H=W/'Probe';H.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());layout=i['qualifiedEngines']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
(H/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
refs=''.join('<Reference Include="'+Path(v['path']).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for v in layout.values())
(H/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
run(['dotnet','build',H/'Probe.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'probe-build.txt')
records={}
for mode in ['legacy','candidate']:
 folder=W/'Run'/mode;folder.mkdir(parents=True,exist_ok=True)
 for f in (H/'bin/Release/net10.0').iterdir():
  if f.is_file():(folder/f.name).write_bytes(f.read_bytes())
 if mode=='candidate':(folder/'Bosak.Xslt.dll').write_bytes(Path(i['candidatePath']).read_bytes())
 assert sha(folder/'Probe.dll')==sha(H/'bin/Release/net10.0/Probe.dll')
 for n in range(2):run(['dotnet',folder/'Probe.dll'],f'{mode}-{n}.json')
 a=json.loads((E/f'{mode}-0.json').read_text());assert a==json.loads((E/f'{mode}-1.json').read_text())
 records[mode]=a
engines={Path(v['path']).stem:v for v in layout.values()}
for mode,a in records.items():
 for b in a['bindings']:assert b['sha256']==(i['candidateSha256'] if mode=='candidate' and b['name']=='Bosak.Xslt' else engines[b['name']]['sha256'])
legacy=records['legacy']['cases'];candidate=records['candidate']['cases'];assert len(legacy)==7 and len(candidate)==22
assert candidate[:7]==legacy and all(x['passed'] for x in legacy)
(E/'RESULTS.json').write_text(json.dumps({'legacyCases':len(legacy),'candidateCases':len(candidate),'candidatePassed':sum(x['passed'] for x in candidate),'legacyIdentical':True,'repeatIdentical':True,'probeSha256':sha(H/'bin/Release/net10.0/Probe.dll'),'sourceSha256':sha(P/'tools/Probe.cs.txt')},indent=2)+'\n')
assert all(x['passed'] for x in candidate),'Candidate observations differ; receipts retained'
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
print('Captured legacy/candidate results; '+str(sum(x['passed'] for x in candidate))+'/'+str(len(candidate))+' candidate cases match references.')
