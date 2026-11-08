"""Guarded complete metadata capture, preserving the qualified engine cohort."""
from pathlib import Path
import sys,json,hashlib,subprocess
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/c45-delta136';W.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(args,log):
 with process_error_mode(),(E/log).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=180)
 if r.returncode:raise RuntimeError(log+' failed')
engines=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines']
for n,v in engines.items():assert sha(Path(v['path']).read_bytes())==v['sha256'],n
config=W/'NuGet.Config';config.write_text('<configuration><packageSources><clear /></packageSources></configuration>')
run(['dotnet','build',ROOT/'src/Bosak.LanguageServer/Bosak.LanguageServer.csproj','-c','Release','--no-dependencies','--configfile',config,'-p:NuGetAudit=false'],'language-server-build.txt')
for n,v in engines.items():assert sha(Path(v['path']).read_bytes())==v['sha256'],n
source=ROOT/'docs/foundation-api-baseline/tools/Program.cs';text=source.read_text(encoding='utf-8')
at=text.index('if (args.Length');helper=text.index('internal sealed class ApiContext')
guarded='// Generated guarded metadata audit; retains original row encoding.\n'+text[:at]+'try {\n'+text[at:helper]+'\n} catch(Exception error) { Console.Error.WriteLine(error); return 1; }\n'+text[helper:]
(P/'tools/Inventory.cs.txt').write_text(guarded,encoding='utf-8');host=W/'Inventory';host.mkdir(exist_ok=True);(host/'Program.cs').write_text(guarded,encoding='utf-8')
(host/'Inventory.csproj').write_bytes((ROOT/'docs/foundation-api-baseline/tools/Inventory.csproj').read_bytes())
run(['dotnet','build',host/'Inventory.csproj','-c','Release','--configfile',config,'-p:NuGetAudit=false'],'inventory-build.txt')
for j in range(2):run(['dotnet',host/'bin/Release/net10.0/Inventory.dll',ROOT,E/f'public-api-{j}.json'],f'inventory-{j}.txt')
a=json.loads((E/'public-api-0.json').read_text());b=json.loads((E/'public-api-1.json').read_text());assert a==b and len(a['assemblies'])==11
for n,v in engines.items():assert sha(Path(v['path']).read_bytes())==v['sha256'],n
(E/'INPUTS.json').write_text(json.dumps({'sourceRevision':subprocess.check_output(['git','rev-parse','0126d492'],cwd=ROOT,text=True).strip(),'originalInventorySha256':sha(source.read_bytes()),'guardedInventorySha256':sha((P/'tools/Inventory.cs.txt').read_bytes()),'engineHashesUnchanged':True,'engines':engines,'inventoryRepeatedIdentically':True,'productionSourceChanged':False,'languageServerRebuilt':True,'runtimeTestsRun':False},indent=2)+'\n',encoding='utf-8')
print('Captured11 assemblies twice; ten qualified engine hashes unchanged.')
