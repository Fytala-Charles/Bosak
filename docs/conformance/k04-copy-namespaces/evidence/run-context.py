"""Audit real resources and before/prototype outcomes under bounded complete harness machinery."""
from pathlib import Path
import concurrent.futures
import json
import os
import subprocess
import time

E = Path(__file__).resolve().parent
R = E.parents[3]
DRIVER = R/'.guard-tmp/k04-copy-namespaces-driver/bin/Release/net10.0/Probe.dll'
PROTO = R/'.guard-tmp/k04-copy-namespaces-harness/bin/Release/net10.0'
rows = json.loads((R/'docs/conformance/k04-context-routing-review/evidence/selected-originals.json').read_text(encoding='utf-8'))

def execute(name,command,phase,seconds=30):
    env = dict(os.environ)
    env.pop('C31_QT3_HARNESS',None)
    if phase=='prototype': env['C31_QT3_HARNESS']=str(PROTO)+os.sep
    started = time.monotonic()
    with (E/(name+'.log')).open('wb') as output:
        try:
            p = subprocess.run(command,cwd=R,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=seconds)
            code, timeout = p.returncode, False
        except subprocess.TimeoutExpired:
            code, timeout = None, True
    return dict(name=name,phase=phase,command=command,exitCode=code,timeout=timeout,watchdogSeconds=seconds,wallSeconds=time.monotonic()-started,log=name+'.log')

def run(job):
    row, mode, phase = job
    name='context-'+row['Name']+'-'+phase+'-'+mode
    request=E/(name+'-request.json')
    request.write_text(json.dumps(row)+'\n',encoding='utf-8')
    receipt=execute(name,['dotnet',str(DRIVER),mode,str(request)],phase)
    receipt.update(case=row['Name'],mode=mode)
    receipt['result']=json.loads((E/receipt['log']).read_text(encoding='utf-8-sig').splitlines()[-1]) if receipt['exitCode']==0 else None
    return receipt

jobs=[(r,'actual','normal') for r in rows]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    receipts=list(pool.map(run,jobs))
(E/'context-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
from collections import Counter
print(json.dumps({phase:dict(Counter(r['result']['kind'] for r in receipts if r['mode']=='actual' and r['phase']==phase)) for phase in ['normal','prototype']}))
