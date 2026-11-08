from pathlib import Path
import subprocess,json,hashlib,os,time,shutil,xml.etree.ElementTree as ET
area=Path('.tools/fx009-02').resolve(); results=area/'results'; selection=json.loads((area/'selection.json').read_text(encoding='utf-8')); packages=selection['packages']
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def layout(path):return {p.relative_to(path).as_posix():sha(p) for p in sorted(path.rglob('*')) if p.is_file()}
def store(path):
 return {'files':layout(path),'directories':sorted(p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_dir())}
def run(label,cmd,timeout=180):
 env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(area/'observer/bin/Release/net10.0/PackageBindingHook.dll');env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(results/(label+'-bindings'))
 start=time.monotonic();p=subprocess.Popen(cmd,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,env=env)
 try:out,err=p.communicate(timeout=timeout)
 except subprocess.TimeoutExpired:
  subprocess.run(['taskkill','/PID',str(p.pid),'/T','/F'],capture_output=True);out,err=p.communicate();(results/(label+'-timeout.txt')).write_text(out+err,encoding='utf-8');raise
 (results/(label+'.txt')).write_text(out+'\nSTDERR:\n'+err,encoding='utf-8');(results/(label+'-command.json')).write_text(json.dumps({'command':cmd,'exitCode':p.returncode,'seconds':time.monotonic()-start,'timeoutSeconds':timeout,'observer':env['DOTNET_STARTUP_HOOKS']},indent=2)+'\n',encoding='utf-8')
 print(label,'exit',p.returncode,out[-450:],err[-300:],flush=True);assert p.returncode==0,label
for package in packages:
 cached=area/'candidate-cache'/package['id'].lower()/'1.34.2'/(package['id'].lower()+'.1.34.2.nupkg')
 assert sha(cached)==package['sha256'],str(cached)
for name in ['tests/Bosak.XmlDb.Xdm.Tests','qualification/Probe']:
 assets=json.loads((area/'candidate/source'/name/'obj/project.assets.json').read_text(encoding='utf-8'))
 declared={k for k in assets['libraries'] if k.startswith('Bosak.XPath.')}
 assert declared=={p['id']+'/1.34.2' for p in packages},declared
for variant in ['baseline','candidate']:
 for typ,rel in [('tests','tests/Bosak.XmlDb.Xdm.Tests'),('probe','qualification/Probe')]:
  output=area/variant/'source'/rel/'bin/Release/net10.0'
  for package in packages:
   assembly=output/(package['id']+'.dll')
   if variant=='candidate': assert sha(assembly)==package['assemblySha256'],str(assembly)
# Freeze complete baseline layouts, then replace exactly the seven allowlisted DLLs.
frozen=area/'frozen';frozen.mkdir()
for mode in ['control','replacement']:
 for typ,rel in [('tests','tests/Bosak.XmlDb.Xdm.Tests'),('probe','qualification/Probe')]:
  src=area/'baseline/source'/rel/'bin/Release/net10.0';dest=frozen/mode/typ;shutil.copytree(src,dest)
  if mode=='replacement':
   for package in packages:shutil.copyfile(area/'candidate-engine'/(package['id']+'.dll'),dest/(package['id']+'.dll'))
for typ in ['tests','probe']:
 a=layout(frozen/'control'/typ);b=layout(frozen/'replacement'/typ)
 assert a.keys()==b.keys()
 assert {k for k in a if a[k]!=b[k]}=={p['id']+'.dll' for p in packages}
pre={mode:{typ:layout(frozen/mode/typ) for typ in ['tests','probe']} for mode in ['control','replacement']}
(results/'frozen-layouts-before.json').write_text(json.dumps(pre,indent=2)+'\n',encoding='utf-8')
shared=area/'fixtures/retained-existing'
run('final-seed',['dotnet',str(frozen/'control/probe/Fx009Probe.dll'),'seed',str(shared)])
before=store(shared);(results/'existing-store-before.json').write_text(json.dumps(before,indent=2)+'\n',encoding='utf-8')
vstest=Path('C:/Program Files/dotnet/sdk/10.0.301/vstest.console.dll')
roles=[('source-baseline',area/'baseline/source/tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0',area/'baseline/source/qualification/Probe/bin/Release/net10.0'),('source-candidate',area/'candidate/source/tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0',area/'candidate/source/qualification/Probe/bin/Release/net10.0'),('binary-control',frozen/'control/tests',frozen/'control/probe'),('binary-replacement',frozen/'replacement/tests',frozen/'replacement/probe')]
for label,tests,probe in roles:
 trxdir=results/(label+'-tests');trxdir.mkdir()
 run(label+'-tests',['dotnet',str(vstest),str(tests/'Bosak.XmlDb.Xdm.Tests.dll'),'--Framework:.NETCoreApp,Version=v10.0','--logger:trx;LogFileName=results.trx','--ResultsDirectory:'+str(trxdir)],timeout=240)
 run(label+'-probe',['dotnet',str(probe/'Fx009Probe.dll'),'qualify',str(shared),str(results/(label+'-observations.json')),str(area/'fixtures'/(label+'-scratch'))])
 after=store(shared);assert after==before,label
 (results/(label+'-store-after.json')).write_text(json.dumps(after,indent=2)+'\n',encoding='utf-8')
post={mode:{typ:layout(frozen/mode/typ) for typ in ['tests','probe']} for mode in ['control','replacement']};assert post==pre
(results/'frozen-layouts-after.json').write_text(json.dumps(post,indent=2)+'\n',encoding='utf-8')
obs=[json.loads((results/(label+'-observations.json')).read_text(encoding='utf-8'))['observations'] for label,_,_ in roles]
assert all(o==obs[0] for o in obs), 'Behavior differs; preserve for review'
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'};sets=[]
for label,_,_ in roles:
 x=ET.parse(results/(label+'-tests/results.trx'));counts=x.find('.//t:Counters',ns).attrib;assert int(counts['passed'])==int(counts['total'])==27 and int(counts['failed'])==0
 sets.append({a.get('testId') for a in x.findall('.//t:UnitTestResult',ns)})
assert all(s==sets[0] for s in sets)
summary={'scope':'Windows sequential ordinary XML characterization; not joint contract acceptance','sourceBaselineTests':27,'sourceCandidateTests':27,'binaryControlTests':27,'binaryReplacementTests':27,'sameTestIds':True,'observationsPerRole':len(obs[0]),'allFourObservationSetsEqual':True,'frozenLayoutsUnchangedAfterExecution':True,'exactSevenDllDifferencesBeforeExecution':True,'existingStoreBytesAndInventoryUnchanged':True,'currentRepublishMayOccur':'Legacy Open can republish byte-identical CURRENT in these owned fixtures; not write-free I/O proof','candidateQualifiedAsStableApi':False}
(results/'windows-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8');print(json.dumps(summary),flush=True)
