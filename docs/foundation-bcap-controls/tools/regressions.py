"""Run existing Runtime and focused XSLT regressions with isolated candidate binaries."""
from pathlib import Path
import tempfile,shutil,json,subprocess,sys,hashlib,os,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-controls-v3'
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
temp=Path(tempfile.mkdtemp(prefix='BosakBcapFeasibility-'));results=[]
(temp/'tests').mkdir()
for link,target in [(temp/'tests/qt3tests',R/'tests/qt3tests'),(temp/'docs',R/'docs')]:
 env=os.environ.copy();env['BOSAK_FIXTURE_LINK']=str(link);env['BOSAK_FIXTURE_TARGET']=str(target)
 subprocess.run(['powershell','-NoProfile','-Command','New-Item -ItemType Junction -Path $env:BOSAK_FIXTURE_LINK -Target $env:BOSAK_FIXTURE_TARGET | Out-Null'],env=env,stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
for name,filter in [('Bosak.XPath.Runtime.Tests',None),('Bosak.Xslt.Tests','FullyQualifiedName~Capture|FullyQualifiedName~ExplicitContext')]:
 source=R/'tests'/name/'bin/Release/net10.0';dest=temp/'tests'/name/'bin/Release/net10.0';shutil.copytree(source,dest)
 for f in (W/'src/Bosak.Xslt/bin/Release/net10.0').glob('Bosak.*.dll'):
  if (dest/f.name).exists():shutil.copyfile(f,dest/f.name)
 args=['dotnet','vstest',str(dest/(name+'.dll')),'/Logger:trx;LogFileName=results.trx','/ResultsDirectory:'+str(dest/'results')]
 if filter:args+=['/TestCaseFilter:'+filter]
 with process_error_mode(),(E/(name+'.txt')).open('w',encoding='utf-8') as log:
  code=subprocess.run(args,cwd=dest,stdout=log,stderr=subprocess.STDOUT,timeout=180).returncode
 if code:raise RuntimeError('Regression failure retained: '+name)
 counters=ET.parse(dest/'results/results.trx').find('{*}ResultSummary/{*}Counters').attrib
 assert int(counters['passed'])>0 and counters['failed']=='0'
 shutil.copyfile(dest/'results/results.trx',E/(name+'.trx'))
 results.append({'name':name,'filter':filter,'counters':counters,'testAssemblySha256':hashlib.sha256((dest/(name+'.dll')).read_bytes()).hexdigest(),'layout':str(dest),'candidateBinaries':{f.name:hashlib.sha256(f.read_bytes()).hexdigest() for f in dest.glob('Bosak.*.dll') if f.name!=name+'.dll'}})
(E/'REGRESSIONS.json').write_text(json.dumps(results,indent=2)+'\n')
print('Runtime and focused XSLT regressions passed: '+', '.join(r['counters']['passed'] for r in results))
