from pathlib import Path
import json,time,subprocess,concurrent.futures
E=Path('docs/conformance/k02-admission-review/evidence')
def run(mode):
 command=['dotnet','.guard-tmp/k02-admission-driver/bin/Release/net10.0/Probe.dll',mode,str(E/(mode+'-skips.txt')),str(E/(mode+'-summary.json'))];start=time.monotonic()
 with (E/(mode+'-full-run.log')).open('wb') as output:
  try:p=subprocess.run(command,stdout=output,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads((E/(mode+'-summary.json')).read_text(encoding='utf-8')) if (E/(mode+'-summary.json')).exists() else None
 return dict(mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-start,result=result,log=mode+'-full-run.log')
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:rows=list(pool.map(run,['current','guarded']))
(E/'full-run-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
print(json.dumps(rows))
