from pathlib import Path
import subprocess,json,time,concurrent.futures,os,re,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;rows=json.loads((E/'requests.json').read_text())
def run(row):
 name=row['Suite']+'-'+row['Name'];setname=X.parse(row['SetFile']).getroot().get('name');log=E/('normal-'+name+'.log');command=['dotnet','tests/'+('Bosak.XPath.Conformance' if row['Suite']=='qt3' else 'Bosak.XPath.Conformance40')+'/bin/Release/net10.0/'+('Bosak.XPath.Conformance' if row['Suite']=='qt3' else 'Bosak.XPath.Conformance40')+'.dll',row['SuiteRoot'],setname,row['Name']];start=time.monotonic();env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env.pop('BOSAK_QT4_DUMP_SKIPS',None)
 with log.open('wb') as f:
  try:p=subprocess.run(command,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 s=log.read_text(encoding='utf-8-sig');match=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',s);totals=list(map(int,match.groups())) if match else None
 return dict(name=name,suite=row['Suite'],command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,totals=totals,log=log.name)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,rows))
(E/'current-normal-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8');print(json.dumps({r['name']:r['totals'] for r in receipts}))
