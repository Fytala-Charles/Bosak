from pathlib import Path
import subprocess,json,time,sys,concurrent.futures
E=Path(__file__).resolve().parent;phase=sys.argv[1]
def run(suite):
 out=E/(suite+'-'+phase+'.jsonl');log=E/(suite+'-'+phase+'.log');command=['dotnet','.guard-tmp/k04-profile-driver/bin/Release/net10.0/Probe.dll',suite,str(E/'controls.json'),str(out),'synthetic'];start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,phase=phase,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,output=out.name,log=log.name)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:rows=list(pool.map(run,['qt3','qt4']))
(E/(phase+'-receipts.json')).write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');assert all(r['exitCode']==0 and not r['timeout'] for r in rows);print(json.dumps(rows))
