"""Qualify exact candidate bytes with retained unit binaries and frozen gate runners."""
from pathlib import Path
import json,sys,subprocess,hashlib,os,tempfile,shutil,zipfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/xslt-context-qualification';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(name,value):(E/name).write_text(json.dumps(value,indent=2)+'\n',encoding='utf-8')
def run(args,name,cwd=ROOT,env=None,timeout=900):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:
  r=subprocess.run([str(a) for a in args],cwd=cwd,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=timeout)
 return r.returncode
def corpus():return {n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/n,text=True)} for n in ['qt3tests','qt4tests']}
def prepare(corrected=False):
 if corrected:
  old=json.loads((E/'INPUTS.json').read_text());attempt=E/'attempt-1';assert not attempt.exists();attempt.mkdir()
  names=['INPUTS.json','UNITS.json','unit-trx.zip']+[r['name']+suffix for r in old['records'] if r['kind']=='unit' for suffix in ['.txt','-bindings']]
  for name in names:
   source=E/name
   if source.is_dir():shutil.copytree(source,attempt/name)
   elif source.exists():shutil.copyfile(source,attempt/name)
 else:assert not (E/'INPUTS.json').exists(),'Fresh qualification only'
 prior=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/INPUTS.json').read_text());engines=prior['qualifiedEngines'];candidate=Path(prior['candidatePath']);assert sha(candidate)==prior['candidateSha256']
 if corrected:
  correction=json.loads((E/'CORRECTION.json').read_text());candidate=Path(correction['correctedCandidatePath']);assert sha(candidate)==correction['correctedCandidateSha256']
 for v in engines.values():assert sha(Path(v['path']))==v['sha256']
 temp=Path(tempfile.mkdtemp(prefix='BosakXsltContextQualification-'));(temp/'tests').mkdir()
 for link,target in [(temp/'tests/qt3tests',ROOT/'tests/qt3tests'),(temp/'tests/qt4tests',ROOT/'tests/qt4tests'),(temp/'docs',ROOT/'docs')]:
  env=os.environ.copy();env['BOSAK_FIXTURE_LINK']=str(link);env['BOSAK_FIXTURE_TARGET']=str(target)
  subprocess.run(['powershell','-NoProfile','-Command','New-Item -ItemType Junction -Path $env:BOSAK_FIXTURE_LINK -Target $env:BOSAK_FIXTURE_TARGET | Out-Null'],env=env,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
 baseline=json.loads((ROOT/'docs/foundation-transport136/evidence/UNIT-FINAL.json').read_text());records=[]
 for row in baseline:
  name=row['name'];source=ROOT/'tests'/name/'bin/Release/net10.0';assert sha(source/(name+'.dll'))==row['assemblies'][name+'.dll']
  records.append({'name':name,'kind':'unit','source':str(source)})
 for name in ['Bosak.XPath.Conformance','Bosak.XPath.Conformance40']:records.append({'name':name,'kind':'gate','source':str(ROOT/'tests'/name/'bin/Release/net10.0')})
 for row in records:
  source=Path(row['source']);dest=temp/'tests'/row['name']/'bin/Release/net10.0';shutil.copytree(source,dest)
  original={f.name:sha(f) for f in source.glob('Bosak.*.dll')}
  for n,v in engines.items():
   if (dest/n).exists():assert sha(dest/n)==v['sha256'],row['name']+': '+n
  if (dest/'Bosak.Xslt.dll').exists():shutil.copyfile(candidate,dest/'Bosak.Xslt.dll')
  if row['name']=='Bosak.XPath.Providers.Database.Tests':(dest/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
  row.update(directory=str(dest),sourceAssemblies=original,assemblies={f.name:sha(f) for f in dest.glob('Bosak.*.dll')})
 observer=W/'Observer';observer.mkdir(exist_ok=True);source=ROOT/'docs/foundation-transport-full-gates/tools/StartupHook.cs.txt';(observer/'Program.cs').write_bytes(source.read_bytes())
 (observer/'Observer.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
 (W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
 assert run(['dotnet','build',observer/'Observer.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'observer-build.txt')==0
 hook=observer/'bin/Release/net10.0/Observer.dll'
 write('INPUTS.json',{'candidatePath':str(candidate),'candidateSha256':sha(candidate),'projectMetadataCorrected':corrected,'originalCandidateSha256':prior['candidateSha256'],'engines':engines,'temporaryRoot':str(temp),'records':records,'observer':str(hook),'observerSha256':sha(hook),'observerSourceSha256':sha(source),'corporaBefore':corpus(),'canonicalChanged':False,'productionPromoted':False})
 print('Prepared fresh temporary qualification tree; pinned ten test assemblies and two gate runners.',flush=True)
def inputs():return json.loads((E/'INPUTS.json').read_text())
def observed(label):
 i=inputs();folder=E/(label+('-corrected-bindings' if i['projectMetadataCorrected'] else '-bindings'));folder.mkdir(exist_ok=True);env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=i['observer'];env['BOSAK_BINDING_DIRECTORY']=str(folder);return env
def units(skip_database=False):
 i=inputs();trx=W/('corrected-trx' if i['projectMetadataCorrected'] else 'trx');trx.mkdir(exist_ok=True);runs=[]
 for row in i['records']:
  if row['kind']!='unit':continue
  if skip_database and row['name']=='Bosak.XPath.Providers.Database.Tests':continue
  folder=Path(row['directory']);name=row['name'];code=run(['dotnet','vstest',folder/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(trx)],name+'.txt',cwd=folder,env=observed(name))
  assert row['assemblies']=={f.name:sha(f) for f in folder.glob('Bosak.*.dll')}
  result=ET.parse(trx/(name+'.trx')).find('{*}ResultSummary/{*}Counters').attrib
  runs.append({'name':name,'exitCode':code,'counters':result});write('UNITS.json',runs)
  print(name+': '+str(code)+'; '+result['passed']+' passed / '+result['failed']+' failed',flush=True)
 with zipfile.ZipFile(E/'unit-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for f in sorted(trx.glob('*.trx')):z.write(f,f.name)
 assert len(runs)==(9 if skip_database else 10) and all(r['exitCode']==0 for r in runs),'Unit failure retained'
def database():
 i=inputs();row=next(r for r in i['records'] if r['name']=='Bosak.XPath.Providers.Database.Tests');folder=W/'DatabaseLoopback';assert not folder.exists();shutil.copytree(Path(row['source']),folder)
 for n,v in i['engines'].items():
  if (folder/n).exists():assert sha(folder/n)==v['sha256']
 if (folder/'Bosak.Xslt.dll').exists():shutil.copyfile(i['candidatePath'],folder/'Bosak.Xslt.dll')
 (folder/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
 row['directory']=str(folder);row['assemblies']={f.name:sha(f) for f in folder.glob('Bosak.*.dll')};write('INPUTS.json',i)
 name=row['name'];trx=W/'corrected-trx';code=run(['dotnet','vstest',folder/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(trx)],name+'.txt',cwd=folder,env=observed(name))
 counters=ET.parse(trx/(name+'.trx')).find('{*}ResultSummary/{*}Counters').attrib
 runs=json.loads((E/'UNITS.json').read_text());assert len(runs)==9;runs.append({'name':name,'exitCode':code,'counters':counters});write('UNITS.json',runs)
 with zipfile.ZipFile(E/'unit-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for f in sorted(trx.glob('*.trx')):z.write(f,f.name)
 assert code==0;print('Database loopback qualification: '+counters['passed']+' passed.',flush=True)
def gates():
 i=inputs();runs={}
 for label,name,corpusName in [('qt3','Bosak.XPath.Conformance','qt3tests'),('qt4','Bosak.XPath.Conformance40','qt4tests')]:
  row=next(r for r in i['records'] if r['name']==name);env=observed(label);env['BOSAK_'+label.upper()+'_DUMP_SKIPS']=str(E/(label+'-skips.txt'))
  if label=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
  runs[label]=run(['dotnet',Path(row['directory'])/(name+'.dll'),ROOT/'tests'/corpusName],label+'.txt',cwd=Path(i['temporaryRoot']),env=env)
  write('GATES.json',runs);print(label+': exit '+str(runs[label]),flush=True)
 write('CORPORA-AFTER.json',corpus());assert all(v==0 for v in runs.values()),'Gate failure retained'
action=sys.argv[1] if len(sys.argv)>1 else 'prepare'
{'prepare':prepare,'prepare-corrected':lambda:prepare(True),'units':units,'units-no-database':lambda:units(True),'database':database,'gates':gates}[action]()
