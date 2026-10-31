from pathlib import Path
import json,subprocess,time,concurrent.futures,collections
E=Path(__file__).resolve().parent;R=E.parents[3];driver=R/'.guard-tmp/k04-independent-profile-driver/bin/Release/net10.0/Probe.dll'
originals=json.loads((E/'diagnostic-originals.json').read_text(encoding='utf-8'));controls=json.loads((E/'control-requests.json').read_text(encoding='utf-8'))
jobs=[(x,mode) for x in originals for mode in ['resources','actual']]+[(x,'public-xquery') for x in controls]
def run(job):
 row,mode=job;name=row['Suite']+'-'+row['Name']+'-'+mode;request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n',encoding='utf-8');command=['dotnet',str(driver),mode,str(request)];start=time.monotonic()
 with (E/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads((E/(name+'.log')).read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 return dict(name=name,suite=row['Suite'],case=row['Name'],mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,result=result,log=name+'.log')
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,jobs))
(E/'diagnostic-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(x['exitCode']==0 and not x['timeout'] for x in receipts)
print(json.dumps({mode:dict(collections.Counter(x['result'].get('kind') for x in receipts if x['mode']==mode)) for mode in ['actual','public-xquery']}))
