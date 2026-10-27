from pathlib import Path
import subprocess,json,time,concurrent.futures
E=Path(__file__).resolve().parent
rows=json.loads((E/'requests.json').read_text(encoding='utf-8'))
def run(q):
 command=['dotnet','.guard-tmp/k03-driver/bin/Release/net10.0/Probe.dll',str(E/'requests.json'),q['id']];start=time.monotonic();log=E/(q['id']+'.log')
 with log.open('wb') as output:
  try:p=subprocess.run(command,stdout=output,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 return dict(id=q['id'],request=q,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,log=log.name,result=result)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:out=list(pool.map(run,rows))
(E/'results.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print('Completed',len(out),'processes')
for r in out:print(r['id'],json.dumps(r['result'],ensure_ascii=True)[:430])
