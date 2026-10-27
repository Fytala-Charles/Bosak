from pathlib import Path
import subprocess,json,time,os,xml.etree.ElementTree as X
A=Path(__file__).resolve().parent;E=A.parent/'evidence';receipts=[]
def run(command,logname):
 log=A/logname;start=time.monotonic();env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env.pop('BOSAK_QT3_DUMP_SKIPS',None);env.pop('BOSAK_QT4_DUMP_SKIPS',None)
 with log.open('wb') as f:
  try:p=subprocess.run(command,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,log=logname)
for suite in ['qt3','qt4']:
 output=A/(suite+'-controls.jsonl');command=['dotnet','.guard-tmp/k04-sequence-filter-driver/bin/Release/net10.0/Probe.dll',suite,str(E/'filter-controls.json'),str(output),'synthetic','current'];r=run(command,suite+'-controls.log');r.update(suite=suite,kind='filter',output=output.name);receipts.append(r)
 actual=[json.loads(l) for l in output.read_text(encoding='utf-8-sig').splitlines()];expected=[json.loads(l) for l in (E/(suite+'-prototype.jsonl')).read_text(encoding='utf-8-sig').splitlines()];assert actual==expected
 row=next(r for r in json.loads((E/'requests.json').read_text()) if r['Suite']==suite);setname=X.parse(row['SetFile']).getroot().get('name');project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';command=['dotnet','tests/'+project+'/bin/Release/net10.0/'+project+'.dll',row['SuiteRoot'],setname,row['Name']];r=run(command,suite+'-original.log');r.update(suite=suite,kind='normal-original');receipts.append(r)
(A/'check-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts);print('Normal288filter controls match approved prototype;both original normal cases executed.')
