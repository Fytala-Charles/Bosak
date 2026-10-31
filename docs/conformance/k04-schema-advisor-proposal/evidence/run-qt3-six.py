from pathlib import Path
import json,subprocess,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3];P=R/'docs/conformance/k04-schema-element-fix/evidence';names={'substitution-0'+str(i) for i in range(20,26)};rows=[x for x in json.loads((P/'diagnostic-originals.json').read_text(encoding='utf-8')) if x['Name'] in names];assert len(rows)==6

def run(row):
 name=row['Name'];request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n',encoding='utf-8');command=['dotnet',str(R/'.guard-tmp/k04-schema-element-driver/bin/Release/net10.0/Probe.dll'),'actual',str(request)];start=time.monotonic();log=name+'.log'
 with (E/log).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads((E/log).read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,log=log,result=result)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:results=list(pool.map(run,rows))
(E/'qt3-six-receipts.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8');assert all(not x['timeout'] and x['exitCode']==0 and x['result']['kind']=='HarnessException' for x in results);print('Six original multi-affiliation preparation exceptions reproduced without a provider.')
