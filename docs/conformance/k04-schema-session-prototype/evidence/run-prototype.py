from pathlib import Path
import subprocess,json,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3];dll=R/'.guard-tmp/k04-schema-session-prototype/bin/Release/net10.0/Prototype.dll'
def run(mode):
 command=['dotnet',str(dll),mode];start=time.monotonic();log='prototype-'+mode+'.log'
 with (E/log).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=45);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads((E/log).read_text(encoding='utf-8-sig')) if code in [0,2] else None
 if result:(E/('prototype-'+mode+'-results.json')).write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
 return dict(mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=45,wallSeconds=time.monotonic()-start,log=log,passed=result['passed'] if result else None,failed=result['failed'] if result else None)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,['none','fallback','qname','partial','unwrap']))
(E/'prototype-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
for x in rows:print(x['mode'],x['exitCode'],x['passed'],x['failed'])
assert all(not x['timeout'] for x in rows) and rows[0]['exitCode']==0 and all(x['exitCode']==2 for x in rows[1:])
