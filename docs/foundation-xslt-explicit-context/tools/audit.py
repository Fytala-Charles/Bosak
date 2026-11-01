"""Inventory candidate public metadata without executing its proposed bridge."""
from pathlib import Path
import subprocess,sys,json,hashlib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-explicit-context';A=W/'Inventory';A.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
source=ROOT/'docs/foundation-c45-revised-shape/tools/Inventory.cs.txt'
(A/'Program.cs').write_bytes(source.read_bytes())
(A/'Inventory.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
run(['dotnet','build',A/'Inventory.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'audit-build.txt')
run(['dotnet',A/'bin/Release/net10.0/Inventory.dll',W,E/'candidate-api.json'],'audit.txt')
baseline=ROOT/'docs/foundation-c45-delta136/evidence/public-api.json'
old=next(a for a in json.loads(baseline.read_text())['assemblies'] if a['assembly']=='Bosak.Xslt')
new=json.loads((E/'candidate-api.json').read_text())['assemblies'];assert len(new)==1;new=new[0]
assert new['declarations']==old['declarations']
(E/'API-COMPARISON.json').write_text(json.dumps({'baselineSource':baseline.relative_to(ROOT).as_posix(),'baselineSha256':hashlib.sha256(baseline.read_bytes()).hexdigest(),'inventoryToolSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'publicTypes':new['publicTypes'],'declarationRows':new['declarationCount'],'declarationsIdentical':True,'newPublicExports':0},indent=2)+'\n')
print('Candidate public metadata equals qualified full-inventory baseline.')
