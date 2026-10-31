from pathlib import Path
import json,subprocess,time,concurrent.futures,os,xml.etree.ElementTree as X,re
E=Path(__file__).resolve().parent;R=E.parents[3]
rows=json.loads((E/'requests.json').read_text())
def run(job):
 row,mode=job;suite=row['Suite'];project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';name=suite+'-'+row['Name']+'-'+mode;request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n')
 if mode=='direct':command=['dotnet',str(R/'.guard-tmp/k04-calendar-driver/bin/Release/net10.0/Probe.dll'),'execute',str(request)]
 else:
  directory=R/('tests/'+project+'/bin/Release/net10.0' if mode=='current' else '.guard-tmp/k04-calendar-harness-'+suite+'/bin/Release/net10.0');setname=X.parse(R/row['SetFile']).getroot().get('name');command=['dotnet',str(directory/(project+'.dll')),row['SuiteRoot'],setname,row['Name']]
 env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env.pop('BOSAK_QT3_DUMP_SKIPS',None);env.pop('BOSAK_QT4_DUMP_SKIPS',None);log=E/(name+'.log');start=time.monotonic()
 with log.open('wb') as f:
  try:process=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=process.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 text=log.read_text(encoding='utf-8-sig');result=json.loads(text.splitlines()[-1]) if mode=='direct' and code==0 else None;match=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',text);counts=list(map(int,match.groups())) if match else None
 return dict(suite=suite,name=row['Name'],mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,log=log.name,result=result,counts=counts)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,[(row,mode) for row in rows for mode in ['direct','current','prototype']]))
(E/'original-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
print(json.dumps([dict(name=r['suite']+'-'+r['name'],mode=r['mode'],counts=r['counts'],result=r['result']) for r in receipts]))
