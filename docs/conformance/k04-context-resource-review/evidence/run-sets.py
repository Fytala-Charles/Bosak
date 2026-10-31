"""Record complete normal/prototype context sets, retaining expected prototype assertion failures."""
from pathlib import Path
import json
import os
import subprocess
import time

E=Path(__file__).resolve().parent
R=E.parents[3]
rows=[]
for suite,phase,project in [('qt3','normal','Bosak.XPath.Conformance'),('qt3','prototype','Bosak.XPath.Conformance'),('qt4','normal','Bosak.XPath.Conformance40')]:
    directory=R/'.guard-tmp/k04-context-resource-harness/bin/Release/net10.0' if phase=='prototype' else R/'tests'/project/'bin/Release/net10.0'
    name=suite+'-set-'+phase
    command=['dotnet',str(directory/(project+'.dll')),'tests/'+suite+'tests','prod-ContextItemDecl']
    env=dict(os.environ)
    env['BOSAK_QT4_GATE_ONLY']='0'
    env['BOSAK_'+suite.upper()+'_DUMP_SKIPS']=str(E/(name+'-skips.txt'))
    start=time.monotonic()
    with (E/(name+'.log')).open('wb') as output:
        try:
            p=subprocess.run(command,cwd=R,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=120)
            code,timeout=p.returncode,False
        except subprocess.TimeoutExpired:
            code,timeout=None,True
    rows.append(dict(suite=suite,phase=phase,command=command,exitCode=code,timeout=timeout,watchdogSeconds=120,wallSeconds=time.monotonic()-start,log=name+'.log',skipDump=name+'-skips.txt'))
(E/'set-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
assert [r['exitCode'] for r in rows]==[0,2,0] and not any(r['timeout'] for r in rows)
print('Complete sets recorded: expected prototype failure exit2 retained; normal sources/policies unchanged.')
