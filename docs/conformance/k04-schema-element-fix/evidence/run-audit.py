from pathlib import Path
import json,subprocess,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def execute(name,command,seconds=30):
 start=time.monotonic()
 with (E/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=seconds);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=seconds,wallSeconds=time.monotonic()-start,log=name+'.log')
command=['dotnet',str(R/'.guard-tmp/k04-schema-element-inventory/bin/Release/net10.0/Probe.dll'),str(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'),'qt3',str(E/'qt3'),str(E/'review-names.json'),str(E/'polarity-requests.json')]
receipt=execute('qt3-inventory',command,60);(E/'inventory-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');assert receipt['exitCode']==0 and not receipt['timeout']
driver=R/'.guard-tmp/k04-schema-element-driver/bin/Release/net10.0/Probe.dll';requests=json.loads((E/'diagnostic-originals.json').read_text(encoding='utf-8'))
def run(job):
 row,mode=job;name=row['Name']+'-'+mode;request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n',encoding='utf-8');receipt=execute(name,['dotnet',str(driver),mode,str(request)])
 receipt.update(case=row['Name'],mode=mode);receipt['result']=json.loads((E/receipt['log']).read_text(encoding='utf-8-sig').splitlines()[-1]) if receipt['exitCode']==0 else None;return receipt
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,[(x,mode) for x in requests for mode in ['resources','actual']]))
(E/'diagnostic-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in rows)
from collections import Counter
print(json.dumps(dict(Counter(x['result'].get('kind') for x in rows if x['mode']=='actual'))))
