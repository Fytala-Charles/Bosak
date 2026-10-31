from pathlib import Path
import subprocess,json,time,os,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def run(row):
 env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env['BOSAK_QT4_DUMP_SKIPS']=str(E/(row['scope']+'-skips.txt'));start=time.monotonic()
 with (E/(row['scope']+'.log')).open('wb') as f:
  try:p=subprocess.run(row['command'],cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=120);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(**row,exitCode=code,timeout=timeout,watchdogSeconds=120,wallSeconds=time.monotonic()-start)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:receipts=list(pool.map(run,json.loads((E/'normal-set-requests.json').read_text())))
(E/'normal-set-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts);print('Two complete normal exploratory scopes finished.')
