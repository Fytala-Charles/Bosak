from pathlib import Path
import subprocess,json,time,concurrent.futures
E=Path(__file__).resolve().parent
jobs=[(s,m) for s in ['qt3','qt4'] for m in ['current','prototype']]
def run(job):
 suite,mode=job;output=E/(suite+'-'+mode+'.jsonl');log=E/(suite+'-'+mode+'.log');command=['dotnet','.guard-tmp/k04-sequence-filter-driver/bin/Release/net10.0/Probe.dll',suite,str(E/'filter-controls.json'),str(output),'synthetic',mode];start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,output=output.name,log=log.name)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:rows=list(pool.map(run,jobs))
(E/'filter-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');print(json.dumps(rows))
