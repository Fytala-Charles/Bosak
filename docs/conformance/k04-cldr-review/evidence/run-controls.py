from pathlib import Path
import subprocess,json,time,concurrent.futures
E=Path(__file__).resolve().parent;rows=json.loads((E/'control-requests.json').read_text())
def run(row):
 name=row['Suite']+'-'+row['Name'];request=E/(name+'-request.json');request.write_text(json.dumps(row),encoding='utf-8');log=E/(name+'.log');command=['dotnet','.guard-tmp/k04-collation-driver/bin/Release/net10.0/Probe.dll','execute',str(request)];start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,result=result,log=log.name)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,rows))
(E/'control-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8');print(json.dumps({r['name']:r['result'] for r in receipts}))
