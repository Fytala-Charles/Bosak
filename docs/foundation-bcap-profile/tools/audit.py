"""Compare the unchanged shipping public declarations and existing friend grants."""
from pathlib import Path
import json,subprocess,hashlib,sys
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-profile';A=W/'Audit';A.mkdir(exist_ok=True)
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
source=R/'docs/foundation-xslt-source-membership/tools/Inventory.cs.txt';(P/'tools/Inventory.cs.txt').write_bytes(source.read_bytes());(A/'Program.cs').write_bytes(source.read_bytes())
(A/'Audit.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
def run(args,n):
 with process_error_mode(),(E/n).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=W,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,n
run(['dotnet','build',W/'src/Bosak.LanguageServer/Bosak.LanguageServer.csproj','-c','Release','--no-dependencies','-p:NuGetAudit=false','-v:q'],'language-server-build.txt')
run(['dotnet','build',A/'Audit.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'audit-build.txt')
for n in range(2):run(['dotnet',A/'bin/Release/net10.0/Audit.dll',W,E/f'public-api-{n}.json',R/'src/Bosak.Xslt/bin/Release/net10.0/Bosak.Xslt.dll'],f'audit-{n}.txt')
a=json.loads((E/'public-api-0.json').read_text());assert a==json.loads((E/'public-api-1.json').read_text())
base=json.loads((R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json').read_text());old={v['assembly']:v for v in base['assemblies']}
assert len(a['assemblies'])==11
for v in a['assemblies']:
 assert v['declarations']==old[v['assembly']]['declarations'] and v['friendAssemblies']==old[v['assembly']]['friendAssemblies'],v['assembly']
(E/'API-COMPARISON.json').write_text(json.dumps({'assemblies':11,'exportedTypes':sum(x['publicTypes'] for x in a['assemblies']),'declarationRows':sum(x['declarationCount'] for x in a['assemblies']),'declarationsIdentical':True,'friendsIdentical':True,'productionBaselineSha256':sha(R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json'),'inventorySourceSha256':sha(source)},indent=2)+'\n')
print('All eleven public declaration inventories and original friend grants match production.')
