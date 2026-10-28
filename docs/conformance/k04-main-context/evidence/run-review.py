"""Run actual filter assessments, unchanged normal sets and bounded original assertions."""
from pathlib import Path
import concurrent.futures
import json
import os
import subprocess
import time

E = Path(__file__).resolve().parent
R = E.parents[3]
DRIVER = str(R/'.guard-tmp/k04-main-context-driver/bin/Release/net10.0/Probe.dll')

def execute(name, command, seconds=20, environment=None):
    start = time.monotonic()
    with (E/(name+'.log')).open('wb') as output:
        try:
            p = subprocess.run(command,cwd=R,env=environment,stdout=output,stderr=subprocess.STDOUT,timeout=seconds)
            code, timeout = p.returncode, False
        except subprocess.TimeoutExpired:
            code, timeout = None, True
    return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=seconds,wallSeconds=time.monotonic()-start,log=name+'.log')

meta = execute('actual-filters',['dotnet',DRIVER,'filters',str(E/'filter-requests.json'),str(E/'filter-results.jsonl')],30)
assert meta['exitCode']==0 and not meta['timeout']
(E/'filter-receipt.json').write_text(json.dumps(meta,indent=2)+'\n',encoding='utf-8')

def run(job):
    row, kind, mode = job
    name = kind+'-'+row['Suite']+'-'+row['Name']+'-'+mode
    request = E/(name+'-request.json')
    request.write_text(json.dumps(row)+'\n',encoding='utf-8')
    receipt = execute(name,['dotnet',DRIVER,mode,str(request)])
    receipt.update(kind=kind,mode=mode,suite=row['Suite'],case=row['Name'])
    receipt['result'] = json.loads((E/receipt['log']).read_text(encoding='utf-8-sig').splitlines()[-1]) if receipt['exitCode']==0 else None
    return receipt

originals = json.loads((E/'selected-originals.json').read_text(encoding='utf-8'))
controls = json.loads((E/'control-requests.json').read_text(encoding='utf-8'))
jobs = [(row,'original',mode) for row in originals for mode in ['actual','public-xquery']]+[(row,'control','public-xquery') for row in controls]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    results = list(pool.map(run,jobs))
(E/'execution-receipts.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in results)
normal = []
for suite,project in [('qt3','Bosak.XPath.Conformance'),('qt4','Bosak.XPath.Conformance40')]:
    env = dict(os.environ)
    env['BOSAK_QT4_GATE_ONLY']='0'
    env['BOSAK_'+suite.upper()+'_DUMP_SKIPS']=str(E/(suite+'-normal-skips.txt'))
    normal.append(execute(suite+'-normal',['dotnet',str(R/'tests'/project/'bin/Release/net10.0'/(project+'.dll')),'tests/'+suite+'tests','prod-ContextItemDecl'],120,env))
(E/'normal-receipts.json').write_text(json.dumps(normal,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in normal)
from collections import Counter
print(json.dumps({kind+'-'+mode:dict(Counter(r['result']['kind'] for r in results if r['kind']==kind and r['mode']==mode)) for kind,mode in [('original','actual'),('original','public-xquery'),('control','public-xquery')]}))
