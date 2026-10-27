from pathlib import Path
import json,subprocess,os,time,concurrent.futures
E=Path(__file__).resolve().parent

def run(row):
 env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env['BOSAK_QT4_DUMP_SKIPS']=row['skipDump'];start=time.monotonic()
 with Path(row['log']).open('wb') as f:
  try:p=subprocess.run(row['command'],stdout=f,stderr=subprocess.STDOUT,env=env,timeout=120);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(**row,exitCode=code,timeout=timeout,watchdogSeconds=120,wallSeconds=time.monotonic()-start)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:receipts=list(pool.map(run,json.loads((E/'exploratory-requests.json').read_text())))
(E/'exploratory-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');print(json.dumps(receipts))
