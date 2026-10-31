from pathlib import Path
import subprocess,time,json,concurrent.futures
E=Path(__file__).resolve().parent;Q=Path('.guard-tmp/k04-executor/requests');Q.mkdir(exist_ok=True)
rows=json.loads((E/'execution-selection.json').read_text(encoding='utf-8'))
def run(job):
 q,mode=job;request=Q/(q['Name']+'.json');request.write_text(json.dumps(q,ensure_ascii=False),encoding='utf-8');log=E/(q['Name']+'-'+mode+'.log');command=['dotnet','.guard-tmp/k04-executor/bin/Release/net10.0/Probe.dll',mode,str(request)];start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=None
 if code==0:
  try:result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1])
  except (ValueError,IndexError):pass
 return dict(name=q['Name'],mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,result=result,log=log.name)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:out=list(pool.map(run,[(q,m) for q in rows for m in ['actual','public-xquery']]))
(E/'execution-results.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
for r in out:print(r['name'],r['mode'],r['exitCode'],r['result']['kind'] if r['result'] else None,flush=True)
