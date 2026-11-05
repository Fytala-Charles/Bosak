"""Replay explicit controls and public metadata against corrected candidate bytes."""
from pathlib import Path
import json,hashlib,sys,subprocess,shutil
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-context-qualification';C=json.loads((E/'CORRECTION.json').read_text())
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
probe=W/'ExplicitProbe';assert not probe.exists();shutil.copytree(ROOT/'.guard-tmp/xslt-explicit-context/Run/candidate',probe)
prior=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/RESULTS.json').read_text());assert sha(probe/'Probe.dll')==prior['probeSha256']
shutil.copyfile(C['correctedCandidatePath'],probe/'Bosak.Xslt.dll')
for n in range(2):run(['dotnet',probe/'Probe.dll'],f'explicit-{n}.json')
a=json.loads((E/'explicit-0.json').read_text());assert a==json.loads((E/'explicit-1.json').read_text()) and len(a['cases'])==22 and all(v['passed'] for v in a['cases'])
assert next(b for b in a['bindings'] if b['name']=='Bosak.Xslt')['sha256']==C['correctedCandidateSha256']
audit=W/'Audit';audit.mkdir();source=ROOT/'docs/foundation-c45-revised-shape/tools/Inventory.cs.txt';(audit/'Program.cs').write_bytes(source.read_bytes())
(audit/'Audit.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
run(['dotnet','build',audit/'Audit.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'audit-build.txt')
run(['dotnet',audit/'bin/Release/net10.0/Audit.dll',W/'Corrected',E/'corrected-api.json'],'audit.txt')
old=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/candidate-api.json').read_text())['assemblies'][0]
new=json.loads((E/'corrected-api.json').read_text())['assemblies'][0];assert old['declarations']==new['declarations']
(E/'EXPLICIT-AND-API.json').write_text(json.dumps({'explicitCases':22,'explicitPassed':22,'repeatIdentical':True,'publicDeclarationsIdentical':True,'publicTypes':new['publicTypes'],'declarationRows':new['declarationCount'],'probeSha256':sha(probe/'Probe.dll'),'candidateSha256':C['correctedCandidateSha256'],'inventorySourceSha256':sha(source)},indent=2)+'\n')
print('Corrected candidate: 22 explicit/legacy controls pass twice; public declarations unchanged.')
