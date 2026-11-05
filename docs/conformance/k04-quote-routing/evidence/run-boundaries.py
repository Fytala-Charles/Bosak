from pathlib import Path
import json,subprocess,time,concurrent.futures
E=Path(__file__).resolve().parent;Q=Path('.guard-tmp/k04-quote-driver/requests');Q.mkdir(exist_ok=True)
requests=json.loads((E/'boundary-requests.json').read_text(encoding='utf-8'))
def run(job):
 q,mode=job;name=q['Name'];path=Q/(name+'.json');path.write_text(json.dumps(q,ensure_ascii=False),encoding='utf-8');command=['dotnet','.guard-tmp/k04-quote-driver/bin/Release/net10.0/Probe.dll','actual',str(path),mode];log=E/('boundary-'+name+'-'+mode+'.log');start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 return dict(name=name,mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,log=log.name,result=result)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:out=list(pool.map(run,[(q,m) for q in requests for m in ['current','prototype']]))
(E/'boundary-results.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
for r in out:print(r['name'],r['mode'],r['exitCode'],r['result']['kind'] if r['result'] else None)
