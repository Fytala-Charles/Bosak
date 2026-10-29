from pathlib import Path
import subprocess,time,json,concurrent.futures,re
E=Path(__file__).resolve().parent;R=E.parents[3];rows=json.loads((E/'xslt-originals.json').read_text(encoding='utf-8'));dll=R/'tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll'
def run(row):
 name=row['Name'];command=['dotnet',str(dll),'tests/xslt30-test/catalog.xml',row['Set'],name,'--schema-aware'];start=time.monotonic();log=name+'.log'
 with (E/log).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=60);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 text=(E/log).read_text(encoding='utf-8-sig');observations=[dict(outcome=m[1],name=m[2],detail=m[3] or '') for m in re.finditer(r'^\s*(PASS|FAIL|SKIP)\s+([^:\s]+)(?::\s*(.*))?$',text,re.M)]
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=60,wallSeconds=time.monotonic()-start,log=log,observations=observations)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:results=list(pool.map(run,rows))
(E/'xslt-receipts.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8');assert all(not x['timeout'] and x['exitCode']==0 for x in results)
for r in results:
 target=[x for x in r['observations'] if x['name']==r['name']];print(r['name'],target)
