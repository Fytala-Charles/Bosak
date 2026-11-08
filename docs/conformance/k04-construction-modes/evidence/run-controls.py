"""Run independent namespace controls through the same isolated complete executor."""
from pathlib import Path
import concurrent.futures
import json
import os
import subprocess
import time

E=Path(__file__).resolve().parent
R=E.parents[3]

def run(row):
    request=E/(row['Name']+'-request.json')
    request.write_text(json.dumps(row)+'\n',encoding='utf-8')
    command=['dotnet',str(R/'.guard-tmp/k04-construction-modes-driver/bin/Release/net10.0/Probe.dll'),'public-xquery',str(request)]
    env=dict(os.environ)
    env['C34_QT3_HARNESS']=str(R/'.guard-tmp/k04-construction-modes-harness/bin/Release/net10.0')+os.sep
    started=time.monotonic()
    with (E/(row['Name']+'.log')).open('wb') as output:
        try:
            p=subprocess.run(command,cwd=R,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=30)
            code,timeout=p.returncode,False
        except subprocess.TimeoutExpired:
            code,timeout=None,True
    result=json.loads((E/(row['Name']+'.log')).read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
    return dict(name=row['Name'],command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-started,result=result,log=row['Name']+'.log')

with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    receipts=list(pool.map(run,json.loads((E/'control-requests.json').read_text(encoding='utf-8'))))
(E/'control-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
from collections import Counter
print(json.dumps(dict(Counter(r['result']['kind'] for r in receipts))))
