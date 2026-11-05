"""Run bounded read-only library context-declaration parser controls."""
from pathlib import Path
import json
import subprocess
import time

E = Path(__file__).resolve().parent
R = E.parents[3]
receipts = []
for item in json.loads((E/'library-control-requests.json').read_text(encoding='utf-8')):
    name = item['Name']
    request = E/(name+'-request.json')
    request.write_text(json.dumps(item)+'\n',encoding='utf-8')
    command = ['dotnet',str(R/'.guard-tmp/k04-context-driver/bin/Release/net10.0/Probe.dll'),'library-parse',str(request)]
    start = time.monotonic()
    with (E/(name+'.log')).open('wb') as output:
        try:
            p = subprocess.run(command,cwd=R,stdout=output,stderr=subprocess.STDOUT,timeout=20)
            code, timeout = p.returncode, False
        except subprocess.TimeoutExpired:
            code, timeout = None, True
    result = json.loads((E/(name+'.log')).read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
    receipts.append(dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,result=result,log=name+'.log'))
(E/'library-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
print(json.dumps([r['result'] for r in receipts]))
