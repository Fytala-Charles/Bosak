"""Full isolated solution build, unit projects and frozen conformance gates."""
from pathlib import Path
import subprocess,sys,json,hashlib,tempfile,shutil,os,xml.etree.ElementTree as ET,zipfile
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-profile';E.mkdir(exist_ok=True)
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(E/n).write_text(json.dumps(v,indent=2)+'\n')
def run(args,n,cwd=W,env=None):
 if n.endswith('.Tests.txt') or n in ['qt3.txt','qt4.txt']:
  i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());env=(env or os.environ.copy()).copy()
  dest=E/(n.removesuffix('.txt')+'-bindings');dest.mkdir(exist_ok=True)
  env['DOTNET_STARTUP_HOOKS']=i['observer'];env['BOSAK_BINDING_DIRECTORY']=str(dest)
 with process_error_mode(),(E/n).open('w',encoding='utf-8') as log:
  r=subprocess.run([str(a) for a in args],cwd=cwd,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=900)
 if r.returncode:raise RuntimeError('Failure retained: '+n)
def build():
 run(['dotnet','build',W/'Bosak.sln','-c','Release','-p:NuGetAudit=false','-v:q'],'solution-build.txt')
 # Freeze one solution cohort before copying it to Application-Control-safe execution layouts.
 engines={p.stem:{'path':str(p),'sha256':sha(p)} for p in (W/'src').glob('*/bin/Release/net10.0/Bosak.*.dll') if p.parent.parent.parent.parent.name==p.stem}
 temp=Path(tempfile.mkdtemp(prefix='BosakBcapFull-'));(temp/'tests').mkdir()
 for link,target in [(temp/'tests/qt3tests',R/'tests/qt3tests'),(temp/'tests/qt4tests',R/'tests/qt4tests'),(temp/'docs',R/'docs')]:
  env=os.environ.copy();env['BOSAK_FIXTURE_LINK']=str(link);env['BOSAK_FIXTURE_TARGET']=str(target)
  subprocess.run(['powershell','-NoProfile','-Command','New-Item -ItemType Junction -Path $env:BOSAK_FIXTURE_LINK -Target $env:BOSAK_FIXTURE_TARGET | Out-Null'],env=env,stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
 records=[]
 for project in sorted((W/'tests').glob('*/bin/Release/net10.0')):
  name=project.parent.parent.parent.name
  if not(name.endswith('.Tests') or name in ['Bosak.XPath.Conformance','Bosak.XPath.Conformance40']):continue
  dest=W/'DatabaseLoopback' if name=='Bosak.XPath.Providers.Database.Tests' else temp/'tests'/name/'bin/Release/net10.0'
  shutil.copytree(project,dest)
  if name=='Bosak.XPath.Providers.Database.Tests':(dest/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
  hashes={f.name:sha(f) for f in dest.glob('Bosak.*.dll')}
  for key,v in engines.items():
   if key+'.dll' in hashes:assert hashes[key+'.dll']==v['sha256'],name+': '+key
  records.append({'name':name,'path':str(dest),'kind':'unit' if name.endswith('.Tests') else 'gate','hashes':hashes})
 write('QUALIFICATION-INPUTS.json',{'engines':engines,'records':records,'temporaryRoot':str(temp),'candidateEnginePromoted':False})
 print('Full isolated solution build passed; frozen '+str(len(engines))+' engines and '+str(len(records))+' runners.',flush=True)
def observer():
 folder=W/'Observer';folder.mkdir(exist_ok=True);source=R/'docs/foundation-transport-full-gates/tools/StartupHook.cs.txt';shutil.copyfile(source,P/'tools/StartupHook.cs.txt');shutil.copyfile(source,folder/'Program.cs')
 (folder/'Observer.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
 run(['dotnet','build',folder/'Observer.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'observer-build.txt')
 i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());i['observer']=str(folder/'bin/Release/net10.0/Observer.dll');i['observerSha256']=sha(Path(i['observer']));i['observerSourceSha256']=sha(source)
 i['corpora']={name:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True)} for name in ['qt3tests','qt4tests']};write('QUALIFICATION-INPUTS.json',i)
def unit(row):
 folder=Path(row['path']);trx=W/'trx';trx.mkdir(exist_ok=True);name=row['name']
 run(['dotnet','vstest',folder/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(trx)],name+'.txt',cwd=folder)
 c=ET.parse(trx/(name+'.trx')).find('{*}ResultSummary/{*}Counters').attrib
 assert c['failed']=='0';assert row['hashes']=={f.name:sha(f) for f in folder.glob('Bosak.*.dll')}
 print(name+': '+c['passed']+' passed',flush=True);return {'name':name,'counters':c}
def units(database=False):
 i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());rows=json.loads((E/'UNITS.json').read_text()) if database else []
 for row in i['records']:
  if row['kind']=='unit' and (row['name']=='Bosak.XPath.Providers.Database.Tests')==database:rows.append(unit(row));write('UNITS.json',rows)
 if database:
  assert len(rows)==10 and sum(int(r['counters']['passed']) for r in rows)==24390
  with zipfile.ZipFile(E/'unit-trx.zip','w',zipfile.ZIP_DEFLATED) as z:
   for f in (W/'trx').glob('*.trx'):z.write(f,f.name)
def database_only():
 i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());write('DATABASE.json',unit(next(r for r in i['records'] if r['name']=='Bosak.XPath.Providers.Database.Tests')))
def finish():
 rows=json.loads((E/'UNITS.json').read_text());assert len(rows)==9
 rows.append(json.loads((E/'DATABASE.json').read_text()));assert len(rows)==10 and sum(int(r['counters']['passed']) for r in rows)==24390
 write('ALL-UNITS.json',rows)
 with zipfile.ZipFile(E/'unit-trx.zip','w',zipfile.ZIP_DEFLATED) as z:
  for f in (W/'trx').glob('*.trx'):z.write(f,f.name)
def gates():
 i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());results={}
 for label,name,corpus in [('qt3','Bosak.XPath.Conformance','qt3tests'),('qt4','Bosak.XPath.Conformance40','qt4tests')]:
  row=next(r for r in i['records'] if r['name']==name);env=os.environ.copy();env['BOSAK_'+label.upper()+'_DUMP_SKIPS']=str(E/(label+'-skips.txt'))
  if label=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
  run(['dotnet',Path(row['path'])/(name+'.dll'),R/'tests'/corpus],label+'.txt',cwd=Path(i['temporaryRoot']),env=env)
  results[label]={'exitCode':0};write('GATES.json',results);print(label+' gate passed.',flush=True)
{'build':build,'observer':observer,'units':units,'database':database_only,'finish':finish,'gates':gates}[sys.argv[1]]()
