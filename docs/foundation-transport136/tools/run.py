"""Qualify the actual promoted 1.36.0 source/build cohort; preserve unrelated edits."""
from pathlib import Path
import os,sys,json,hashlib,subprocess,zipfile,re,tempfile,shutil
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True)
W=ROOT;T=ROOT/'.guard-tmp/transport136-tools';sha=lambda b:hashlib.sha256(b).hexdigest()
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
def run(command,log,cwd=None,env=None,timeout=600,check=True):
 with process_error_mode(),(E/log).open('w',encoding='utf-8') as f:
  r=subprocess.run([str(c) for c in command],cwd=cwd or W,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=timeout)
 if check and r.returncode:raise RuntimeError(log+' failed: '+str(r.returncode))
 return r.returncode
def write(name,value):(E/name).write_text(json.dumps(value,indent=2)+'\n',encoding='utf-8')
def observed(label):
 folder=E/(label+'-bindings');folder.mkdir(exist_ok=True);env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(T/'Observer/bin/Release/net10.0/Observer.dll');env['BOSAK_BINDING_DIRECTORY']=str(folder);return env
def project(folder,name,source,refs=(),assembly=None):
 folder.mkdir(exist_ok=True);(folder/'Program.cs').write_bytes(source)
 references=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+str(T/'lib'/n)+'</HintPath></Reference>' for n in refs)
 props='<OutputType>Exe</OutputType>' if name!='Observer' else ''
 if assembly:props+='<AssemblyName>'+assembly+'</AssemblyName>'
 (folder/(name+'.csproj')).write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'+props+'</PropertyGroup><ItemGroup>'+references+'</ItemGroup></Project>',encoding='utf-8')
 return folder/(name+'.csproj')
def build(proj,log):run(['dotnet','build',proj,'-c','Release','--configfile',T/'NuGet.Config','-p:NuGetAudit=false','-m:1'],log)
def prepare():
 if T.exists():raise ValueError('Use a fresh qualification tools directory.')
 T.mkdir(parents=True)
 revision=subprocess.check_output(['git','rev-parse','a87ec4ac'],cwd=ROOT,text=True).strip()
 paths=['src','tests','Bosak.sln','README.md','license.md','NOTICE','assets','docs/xpath40-catalog/worked-examples','docs/xpath40-catalog/examples']
 baseline=T/'baseline-inputs.zip';subprocess.run(['git','archive','--format=zip','-o',str(baseline),revision,*paths],cwd=ROOT,check=True)
 proposal=json.loads((ROOT/'docs/foundation-transport-promotion/PATCH.json').read_text())
 for n,v in proposal['files'].items():assert sha((ROOT/n).read_bytes().replace(b'\r\n',b'\n'))==v['afterSha256'],n
 sourceFiles={}
 archive=E/'source.zip'
 with zipfile.ZipFile(baseline) as z,zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED) as out:
  for n in z.namelist():
   if n.endswith('/'):
    out.writestr(n,b'');continue
   # The unrelated working README is excluded; it is not a compilation input.
   data=z.read(n) if n=='README.md' else (ROOT/n).read_bytes()
   if n.startswith(('src/','tests/')) and n not in proposal['files']:
    assert data.replace(b'\r\n',b'\n')==z.read(n).replace(b'\r\n',b'\n'),n
   out.writestr(n,data);sourceFiles[n]=sha(data)
  for n in proposal['files']:
   if n not in sourceFiles:
    data=(ROOT/n).read_bytes();out.writestr(n,data);sourceFiles[n]=sha(data)
 corpora={name:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/name,text=True).strip(),'statusBefore':subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/name,text=True)} for name in ['qt3tests','qt4tests']}
 (T/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
 write('SOURCE.json',{'baseRevision':revision,'archiveSha256':sha(archive.read_bytes()),'archivePaths':paths,'files':sourceFiles,'promotedPatchSha256':sha((ROOT/'docs/foundation-transport-promotion/PRODUCTION.patch').read_bytes()),'patchTargets':{n:sha((ROOT/n).read_bytes()) for n in proposal['files']},'corpora':corpora,'productionModified':True,'version':'1.36.0','unrelatedWorkingReadmeExcluded':True})
 run(['dotnet','--info'],'dotnet-info.txt')
 build(W/'Bosak.sln','solution-build.txt')
 sourceProjects=re.findall(r'^Project\([^\n]+? = "([^"]+)", "(src\\[^\"]+\.csproj)"',(W/'Bosak.sln').read_text(encoding='utf-8-sig'),re.M)
 engines={}
 for name,path in sourceProjects:
  output=W/Path(path.replace('\\','/')).parent/'bin/Release/net10.0'/(name+'.dll');engines[name+'.dll']={'path':str(output),'sha256':sha(output.read_bytes())}
 lib=T/'lib';lib.mkdir()
 with zipfile.ZipFile(ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip') as z:names=[n for n in z.namelist() if n.endswith('.dll')]
 for name in names:shutil.copyfile(engines[name]['path'],lib/name)
 write('LAYOUT.json',{'engines':engines,'seven':{n:engines[n]['sha256'] for n in names}})
 build(project(T/'Observer','Observer',(ROOT/'docs/foundation-transport-full-gates/tools/StartupHook.cs.txt').read_bytes()),'observer-build.txt')
 build(project(T/'Metadata','Metadata',(ROOT/'docs/foundation-transport-full-gates/tools/MetadataProbe.cs.txt').read_bytes()),'metadata-build.txt')
def units():
 solution=(W/'Bosak.sln').read_text(encoding='utf-8-sig');projects=re.findall(r'^Project\([^\n]+? = "([^"]+\.Tests)", "(tests\\[^\"]+\.csproj)"',solution,re.M);assert len(projects)==10
 results=T/'trx';results.mkdir(exist_ok=True);records=[]
 for name,path in projects:
  output=W/Path(path.replace('\\','/')).parent/'bin/Release/net10.0'
  if name=='Bosak.Xslt.Tests':
   target=Path(tempfile.mkdtemp(prefix='BosakTransportXslt-'));shutil.copytree(output,target,dirs_exist_ok=True);output=target
  if name=='Bosak.XPath.Providers.Database.Tests':(output/'xunit.runner.json').write_text('{"parallelizeTestCollections":false}')
  layout={f.name:sha(f.read_bytes()) for f in output.glob('Bosak.*.dll')}
  code=run(['dotnet','vstest',output/(name+'.dll'),'/Logger:trx;LogFileName='+name+'.trx','/ResultsDirectory:'+str(results)],name+'.txt',cwd=output,env=observed(name),check=False)
  assert layout=={f.name:sha(f.read_bytes()) for f in output.glob('Bosak.*.dll')}
  records.append({'name':name,'exitCode':code,'directory':str(output),'assemblies':layout})
 write('UNIT-RUNS.json',records)
 if all(row['exitCode']==0 for row in records):write('UNIT-FINAL.json',records)
 with zipfile.ZipFile(E/'unit-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for f in sorted(results.rglob('*.trx')):z.write(f,f.relative_to(results))
 if any(r['exitCode'] for r in records):raise RuntimeError('Unit failures retained; inspect UNIT-RUNS.json.')
def database_retry():
 records=json.loads((E/'UNIT-RUNS.json').read_text());v=next(v for v in records if v['name']=='Bosak.XPath.Providers.Database.Tests');output=Path(v['directory']);results=T/'database-retry-trx';results.mkdir(exist_ok=True)
 assert v['assemblies']=={f.name:sha(f.read_bytes()) for f in output.glob('Bosak.*.dll')}
 code=run(['dotnet','vstest',output/(v['name']+'.dll'),'/Logger:trx;LogFileName='+v['name']+'.trx','/ResultsDirectory:'+str(results)],'database-rerun.txt',cwd=output,env=observed('database-rerun'),check=False)
 retry={**v,'exitCode':code};write('DATABASE-RERUN.json',retry)
 with zipfile.ZipFile(E/'database-rerun-trx.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
  for f in results.glob('*.trx'):z.write(f,f.name)
 write('UNIT-FINAL.json',[retry if row['name']==v['name'] else row for row in records])
 if code:raise RuntimeError('Database retry failed; receipts retained.')
def gates():
 records={}
 for label,projectName,corpus in [('qt3','Bosak.XPath.Conformance','qt3tests'),('qt4','Bosak.XPath.Conformance40','qt4tests')]:
  output=W/'tests'/projectName/'bin/Release/net10.0';env=observed(label);env['BOSAK_'+label.upper()+'_DUMP_SKIPS']=str(E/(label+'-skips.txt'))
  if label=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
  records[label]=run(['dotnet',output/(projectName+'.dll'),ROOT/'tests'/corpus],label+'.txt',cwd=W,env=env,check=False)
 write('GATE-RUNS.json',records)
 source=json.loads((E/'SOURCE.json').read_text())
 write('CORPUS-AFTER.json',{name:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/name,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/name,text=True)} for name in source['corpora']})
 if any(records.values()):raise RuntimeError('Gate failure retained.')
def review():
 baseline=T/'baseline';baseline.mkdir(exist_ok=True)
 with zipfile.ZipFile(ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip') as z:
  for n in z.namelist():
   if n.endswith('.dll'):(baseline/n).write_bytes(z.read(n))
 for name,folder in [('baseline',baseline),('candidate',T/'lib')]:run(['dotnet',T/'Metadata/bin/Release/net10.0/Metadata.dll',folder],name+'-metadata.json')
 run(['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',ROOT/'docs/foundation-transport-promotion/tools/ReadVersions.ps1','-Directory',T/'lib'],'version-info.json')
 layout=json.loads((E/'LAYOUT.json').read_text());write('legacy-qualification.json',{'assemblies':[{'name':Path(n).stem,'sha256':h} for n,h in layout['seven'].items()]})
 legacy=T/'Legacy';legacy.mkdir(exist_ok=True)
 with zipfile.ZipFile(ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as z:
  for n in z.namelist():
   target=(legacy/n).resolve();assert target.is_relative_to(legacy.resolve())
   if not n.endswith('/'):target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
 for n in layout['seven']:shutil.copyfile(T/'lib'/n,legacy/n)
 run([sys.executable,'-B',ROOT/'tools/run-retained-foundation-caller.py','--layout',legacy,'--candidate-evidence',E/'legacy-qualification.json','--output',E/'legacy-caller.json'],'legacy-log.txt')
 for label,source in [('Native',ROOT/'docs/foundation-private-transport/tools/Probe.cs.txt'),('Atomic',ROOT/'docs/foundation-transport-atomic-cost/tools/AtomicProbe.cs.txt')]:
  folder=T/label;proj=project(folder,label,source.read_bytes(),layout['seven'],assembly='Bosak.XPath.Runtime.Tests')
  if label=='Native':
   with zipfile.ZipFile(ROOT/'docs/foundation-native-adapter/evidence/native-source.zip') as z:
    for n in z.namelist():
     if n.endswith('.cs'):(folder/Path(n).name).write_bytes(z.read(n))
  text=proj.read_text();proj.write_text(text.replace('<TargetFramework>','<DefineConstants>CARRIER</DefineConstants><TargetFramework>',1))
  build(proj,label.lower()+'-build.txt')
  run(['dotnet',folder/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll','carrier'],label.lower()+'-candidate.json')
action=sys.argv[1] if len(sys.argv)>1 else 'prepare'
{'prepare':prepare,'units':units,'database-retry':database_retry,'gates':gates,'review':review}[action]()
print(action+' completed; receipts retained in '+str(E))
