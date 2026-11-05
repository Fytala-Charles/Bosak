"""Apply the proposal only in an ignored materialization and run affected tests."""
from pathlib import Path
import os,sys,json,hashlib,subprocess,zipfile,shutil,runpy
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/transport-promotion-preflight'
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(args,name,cwd=None,check=True):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=cwd or W,stdout=f,stderr=subprocess.STDOUT,timeout=300)
 if check and r.returncode:raise RuntimeError(name+' failed')
 return r.returncode
def write(n,v):(E/n).write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
if W.exists():raise ValueError('Use a fresh preflight directory.')
W.mkdir(parents=True)
archive=ROOT/'docs/foundation-transport-full-gates/evidence/source-baseline.zip'
with zipfile.ZipFile(archive) as z:
 for n in z.namelist():
  target=(W/n).resolve();assert target.is_relative_to(W.resolve())
  if n.endswith('/'):target.mkdir(parents=True,exist_ok=True)
  else:target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
relative=W.relative_to(ROOT).as_posix();patch=P/'PRODUCTION.patch'
run(['git','apply','--check','--directory='+relative,patch],'apply-check.txt',cwd=ROOT)
run(['git','apply','--directory='+relative,patch],'apply.txt',cwd=ROOT)
proposal=json.loads((P/'PATCH.json').read_text())
for n,v in proposal['files'].items():assert sha((W/n).read_bytes().replace(b'\r\n',b'\n'))==v['afterSha256'],n
write('MATERIALIZED.json',{'lineEndingPolicy':'Patch/files use LF; git apply may materialize CRLF according to repository attributes. Compare normalized LF and retain exact materialized hashes.','files':{n:{'sha256':sha((W/n).read_bytes()),'lfSha256':sha((W/n).read_bytes().replace(b'\r\n',b'\n'))} for n in proposal['files']}})
for corpus in ['qt3tests','qt4tests']:
 path=W/'tests'/corpus
 if path.exists():path.rmdir()
 command="New-Item -ItemType Junction -Path '"+str(path).replace("'","''")+"' -Value '"+str(ROOT/'tests'/corpus).replace("'","''")+"' | Out-Null"
 subprocess.run(['powershell','-NoProfile','-Command',command],check=True)
(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
results=W/'trx';results.mkdir();units=[]
for name in ['Bosak.XPath.Core.Tests','Bosak.XPath.Runtime.Tests']:
 project=W/'tests'/name/(name+'.csproj')
 run(['dotnet','build',project,'-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false','-m:1'],name+'-build.txt')
 output=project.parent/'bin/Release/net10.0'
 code=run(['dotnet','vstest',output/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(results)],name+'.txt',cwd=output,check=False)
 units.append({'name':name,'exitCode':code,'assemblies':{f.name:sha(f.read_bytes()) for f in output.glob('Bosak.*.dll')}})
write('UNIT-RUNS.json',units)
with zipfile.ZipFile(E/'preflight-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
 for f in results.glob('*.trx'):z.write(f,f.name)
if any(v['exitCode'] for v in units):raise RuntimeError('Affected tests failed; original receipts retained.')
layout=W/'lib';layout.mkdir()
names=json.loads((ROOT/'docs/foundation-transport-full-gates/evidence/LAYOUT.json').read_text())['seven']
assemblies={}
for name in names:
 path=W/'src'/Path(name).stem/'bin/Release/net10.0'/name;shutil.copyfile(path,layout/name);assemblies[name]=sha(path.read_bytes())
write('LAYOUT.json',{'seven':assemblies})
metadata=W/'Metadata';metadata.mkdir();(metadata/'Program.cs').write_bytes((ROOT/'docs/foundation-transport-full-gates/tools/MetadataProbe.cs.txt').read_bytes())
(metadata/'Metadata.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
run(['dotnet','build',metadata/'Metadata.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'metadata-build.txt')
run(['dotnet',metadata/'bin/Release/net10.0/Metadata.dll',layout],'metadata.json')
runpy.run_path(str(P/'tools/finish.py'),run_name='__main__')
