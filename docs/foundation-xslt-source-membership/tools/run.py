"""Build and qualify the actual 1.42.0 production-source cohort."""
from pathlib import Path
import json,hashlib,subprocess,sys,os,tempfile,shutil,zipfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/xslt-source-membership';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(name,v):(E/name).write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
def run(args,name,cwd=ROOT,env=None,timeout=900):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=cwd,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=timeout)
 return r.returncode
def corpus():return {n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/n,text=True)} for n in ['qt3tests','qt4tests']}
def archive_source():
 targets=['src/Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs', 'src/Bosak.XPath.Runtime/Vm/SchemaOperationInputs.cs', 'src/Bosak.XPath.Runtime/Vm/SchemaProviderSessionBinding.cs', 'src/Bosak.Xslt/Api/SchemaHostExecution.cs', 'src/Bosak.Xslt/Api/SchemaHostSessionCompilation.cs', 'src/Directory.Build.props', 'tests/Bosak.XPath.Runtime.Tests/SchemaCompositeAdmissionTests.cs']
 names=set(subprocess.check_output(['git','ls-files','src','tests','Bosak.sln','license.md','NOTICE'],cwd=ROOT,text=True).splitlines())|set(targets)
 files={}
 with zipfile.ZipFile(E/'source.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for n in sorted(names):
   f=ROOT/n
   if not f.is_file():continue
   z.write(f,n);files[n]=sha(f)
 before=corpus();assert all(not c['status'] for c in before.values())
 write('SOURCE.json',{'baseRevision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),'version':'1.42.0','sourceArchiveSha256':sha(E/'source.zip'),'files':files,'changedFiles':targets,'corporaBefore':before,'projectSha256':sha(ROOT/'src/Bosak.Xslt/Bosak.Xslt.csproj')})
def build():
 archive_source()
 (W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
 assert run(['dotnet','build',ROOT/'Bosak.sln','-c','Release','-t:Rebuild','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'solution-build.txt')==0,'Build failure retained'
 old=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines'];engines={name:{'path':v['path'],'sha256':sha(Path(v['path']))} for name,v in old.items()}
 observer=W/'Observer';observer.mkdir(exist_ok=True);hook=ROOT/'docs/foundation-transport-full-gates/tools/StartupHook.cs.txt';(observer/'Program.cs').write_bytes(hook.read_bytes())
 (observer/'Observer.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
 assert run(['dotnet','build',observer/'Observer.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'observer-build.txt')==0
 temp=Path(tempfile.mkdtemp(prefix='BosakXsltContext142-'));(temp/'tests').mkdir()
 for link,target in [(temp/'tests/qt3tests',ROOT/'tests/qt3tests'),(temp/'tests/qt4tests',ROOT/'tests/qt4tests'),(temp/'docs',ROOT/'docs')]:
  env=os.environ.copy();env['BOSAK_FIXTURE_LINK']=str(link);env['BOSAK_FIXTURE_TARGET']=str(target)
  subprocess.run(['powershell','-NoProfile','-Command','New-Item -ItemType Junction -Path $env:BOSAK_FIXTURE_LINK -Target $env:BOSAK_FIXTURE_TARGET | Out-Null'],env=env,stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
 baseline=json.loads((ROOT/'docs/foundation-xslt-context-qualification/evidence/INPUTS.json').read_text());records=[]
 for row in baseline['records']:
  name=row['name'];source=ROOT/'tests'/name/'bin/Release/net10.0'
  dest=W/'DatabaseLoopback' if name=='Bosak.XPath.Providers.Database.Tests' else temp/'tests'/name/'bin/Release/net10.0'
  assert not dest.exists();shutil.copytree(source,dest)
  if name=='Bosak.XPath.Providers.Database.Tests':(dest/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
  assemblies={f.name:sha(f) for f in dest.glob('Bosak.*.dll')}
  for n,v in engines.items():
   if n in assemblies:assert assemblies[n]==v['sha256'],name+': '+n
  records.append({'name':name,'kind':row['kind'],'directory':str(dest),'source':str(source),'assemblies':assemblies})
 write('INPUTS.json',{'version':'1.42.0','engines':engines,'records':records,'temporaryRoot':str(temp),'observer':str(observer/'bin/Release/net10.0/Observer.dll'),'observerSourceSha256':sha(hook),'productionSourceApplied':True,'publicFacadeImplemented':False})
 print('Built actual solution and prepared isolated execution layouts for the 1.42.0 outputs.',flush=True)
def inputs():return json.loads((E/'INPUTS.json').read_text())
def rebuild():
 archive_source()
 assert run(['dotnet','build',ROOT/'Bosak.sln','-c','Release','-t:Rebuild','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'solution-build.txt')==0
 i=inputs()
 for v in i['engines'].values():v['sha256']=sha(Path(v['path']))
 for row in i['records']:
  folder=Path(row['directory']);shutil.copytree(row['source'],folder,dirs_exist_ok=True)
  if row['name']=='Bosak.XPath.Providers.Database.Tests':(folder/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
  row['assemblies']={f.name:sha(f) for f in folder.glob('Bosak.*.dll')}
 write('INPUTS.json',i);print('Full rebuild complete; refreshed execution copies before testing.',flush=True)
def observed(label):
 i=inputs();folder=E/(label+'-final-bindings');folder.mkdir(exist_ok=True);env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=i['observer'];env['BOSAK_BINDING_DIRECTORY']=str(folder);return env
def unit(row):
 folder=Path(row['directory']);name=row['name'];trx=W/'trx';trx.mkdir(exist_ok=True)
 code=run(['dotnet','vstest',folder/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(trx)],name+'.txt',cwd=folder,env=observed(name))
 counters=ET.parse(trx/(name+'.trx')).find('{*}ResultSummary/{*}Counters').attrib
 assert row['assemblies']=={f.name:sha(f) for f in folder.glob('Bosak.*.dll')}
 print(name+': '+counters['passed']+' passed, '+counters['failed']+' failed',flush=True)
 return {'name':name,'exitCode':code,'counters':counters}
def units():
 records=[]
 for row in inputs()['records']:
  if row['kind']=='unit' and row['name']!='Bosak.XPath.Providers.Database.Tests':records.append(unit(row));write('UNITS.json',records)
 assert len(records)==9 and all(r['exitCode']==0 for r in records),'Unit failure retained'
def database():
 records=json.loads((E/'UNITS.json').read_text());assert len(records)==9
 records.append(unit(next(r for r in inputs()['records'] if r['name']=='Bosak.XPath.Providers.Database.Tests')));write('UNITS.json',records)
 with zipfile.ZipFile(E/'unit-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for f in sorted((W/'trx').glob('*.trx')):z.write(f,f.name)
 assert all(r['exitCode']==0 for r in records)
def gates():
 i=inputs();results={}
 for label,name,corpusName in [('qt3','Bosak.XPath.Conformance','qt3tests'),('qt4','Bosak.XPath.Conformance40','qt4tests')]:
  row=next(r for r in i['records'] if r['name']==name);env=observed(label);env['BOSAK_'+label.upper()+'_DUMP_SKIPS']=str(E/(label+'-skips.txt'))
  if label=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
  results[label]=run(['dotnet',Path(row['directory'])/(name+'.dll'),ROOT/'tests'/corpusName],label+'.txt',cwd=Path(i['temporaryRoot']),env=env);write('GATES.json',results);print(label+': exit '+str(results[label]),flush=True)
 write('CORPORA-AFTER.json',corpus());assert all(v==0 for v in results.values())
{'build':build,'rebuild':rebuild,'units':units,'database':database,'gates':gates}[sys.argv[1]]()
