from pathlib import Path
import subprocess,json,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def run(job):
 row,kind=job;name=kind+'-'+row['Suite']+'-'+row['Name'];request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n');command=['dotnet',str(R/'.guard-tmp/k04-normalization-options-driver/bin/Release/net10.0/Probe.dll'),'execute',str(request)];start=time.monotonic()
 with (E/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 text=(E/(name+'.log')).read_text(encoding='utf-8-sig');result=json.loads(text.splitlines()[-1]) if code==0 else None
 return dict(name=name,kind=kind,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,result=result,log=name+'.log')
jobs=[(r,k) for k in ['original','control'] for r in json.loads((R/'docs/conformance/k04-normalization-review/evidence'/(k+'-requests.json')).read_text())]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,jobs))
(E/'probe-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
from collections import Counter
print(json.dumps({k:dict(Counter(r['result']['kind'] for r in receipts if r['kind']==k)) for k in ['original','control']}))
