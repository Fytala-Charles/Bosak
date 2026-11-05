"""Capture full production declarations, versions and actual assembly-level friend grants."""
from pathlib import Path
import json,hashlib,zipfile,subprocess,sys
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-context-promotion';A=W/'Audit';A.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
source=ROOT/'docs/foundation-c45-revised-shape/tools/Inventory.cs.txt';s=source.read_text(encoding='utf-8')
s=s.replace('args.Length != 2','args.Length != 3').replace('Usage: inventory <repository-root> <output-json>','Usage: inventory <repository-root> <output-json> <baseline-xslt-dll>')
s=s.replace('publicTypes = declarations.Count','friendAssemblies = Friends(path), publicTypes = declarations.Count')
s=s.replace('assemblies }, new JsonSerializerOptions','baselineXsltFriends = Friends(args[2]), assemblies }, new JsonSerializerOptions')
s=s.replace('static string[] References(string file)', '''static string[] Friends(string file) {
 var context = new ApiContext(Path.GetDirectoryName(file)!);
 try { return context.LoadFromAssemblyPath(Path.GetFullPath(file)).GetCustomAttributesData()
  .Where(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.InternalsVisibleToAttribute")
  .Select(a => (string)a.ConstructorArguments[0].Value!).Order(StringComparer.Ordinal).ToArray(); }
 finally { context.Unload(); }
}
static string[] References(string file)''')
(P/'tools/Inventory.cs.txt').write_text(s,encoding='utf-8');(A/'Program.cs').write_bytes((P/'tools/Inventory.cs.txt').read_bytes())
(A/'Audit.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
prior=A/'Prior/Bosak.Xslt.dll';prior.parent.mkdir(exist_ok=True)
with zipfile.ZipFile(ROOT/'docs/foundation-xslt-context-qualification/evidence/candidate-binaries.zip') as z:prior.write_bytes(z.read('corrected/Bosak.Xslt.dll'))
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,name
run(['dotnet','build',A/'Audit.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'audit-build.txt')
run(['dotnet','build',ROOT/'src/Bosak.LanguageServer/Bosak.LanguageServer.csproj','-c','Release','--no-dependencies','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'language-server-build.txt')
for n in range(2):run(['dotnet',A/'bin/Release/net10.0/Audit.dll',ROOT,E/f'public-api-{n}.json',prior],f'audit-{n}.txt')
a=json.loads((E/'public-api-0.json').read_text());assert a==json.loads((E/'public-api-1.json').read_text())
baseline=ROOT/'docs/foundation-c45-delta136/evidence/public-api.json';old={a['assembly']:a for a in json.loads(baseline.read_text())['assemblies']}
assert len(a['assemblies'])==11
for assembly in a['assemblies']:
 assert assembly['declarations']==old[assembly['assembly']]['declarations'],assembly['assembly']
 assert 'Version=1.37.0.0' in assembly['identity'],assembly['identity']
xslt=next(x for x in a['assemblies'] if x['assembly']=='Bosak.Xslt');assert xslt['friendAssemblies']==a['baselineXsltFriends']==['Bosak.Xslt.Conformance','Bosak.Xslt.Tests']
(E/'API-COMPARISON.json').write_text(json.dumps({'baselineSha256':sha(baseline),'inventoryBaseSourceSha256':sha(source),'inventorySourceSha256':sha(P/'tools/Inventory.cs.txt'),'assemblies':11,'exportedTypes':sum(x['publicTypes'] for x in a['assemblies']),'declarationRows':sum(x['declarationCount'] for x in a['assemblies']),'declarationsIdentical':True,'assemblyVersions':'1.37.0.0','xsltFriendsIdentical':True,'baselineXsltSha256':sha(prior)},indent=2)+'\n')
print('Full production declarations unchanged; all eleven versions 1.37.0.0; actual XSLT friend grants retained.')
