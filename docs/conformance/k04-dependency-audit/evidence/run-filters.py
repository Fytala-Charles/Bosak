from pathlib import Path
import subprocess,time,json,concurrent.futures
E=Path(__file__).resolve().parent
def run(job):
 suite,mode=job;src=E/('requests.json' if mode=='catalog' else 'synthetic-requests.json');out=E/(suite+'-'+mode+'.jsonl');log=E/(suite+'-'+mode+'.log');command=['dotnet','.guard-tmp/k04-driver/bin/Release/net10.0/Probe.dll',suite,str(src),str(out)]+(['synthetic'] if mode=='synthetic' else []);start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,output=out.name,log=log.name,rows=len(out.read_text(encoding='utf-8-sig').splitlines()) if out.exists() else 0)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:rows=list(pool.map(run,[(s,m) for s in ['qt3','qt4'] for m in ['catalog','synthetic']]))
(E/'filter-process-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');print(json.dumps(rows))
