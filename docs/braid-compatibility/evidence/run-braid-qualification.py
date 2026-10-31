from pathlib import Path
import subprocess,json,time,concurrent.futures
r=Path.cwd()
def run(mode):
 cmd=['dotnet','test',f'.guard-tmp/braid-{mode}/Qualification.slnx','-c','Release','-p:MinVerSkip=true','--logger','trx','--results-directory',str(r/f'.guard-tmp/braid-{mode}-results')]
 start=time.monotonic()
 with (r/f'.guard-tmp/braid-{mode}-tests.log').open('w',encoding='utf-8') as log:
  try:
   p=subprocess.run(cmd,stdout=log,stderr=subprocess.STDOUT,timeout=300);row={'mode':mode,'exitCode':p.returncode,'timedOut':False}
  except subprocess.TimeoutExpired:row={'mode':mode,'timedOut':True}
 row.update(elapsedSeconds=round(time.monotonic()-start,3),externalTimeoutSeconds=300);print(row,flush=True);return row
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(run,['baseline','candidate']))
(r/'.guard-tmp/braid-processes.json').write_text(json.dumps(results,indent=2)+'\n')
