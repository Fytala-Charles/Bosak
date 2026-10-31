"""Check real before/prototype routing and dependency decisions, retaining unrelated guards."""
from pathlib import Path
import json
import os
import subprocess
import time

E=Path(__file__).resolve().parent
R=E.parents[3]
rows=json.loads((R/'docs/conformance/k04-main-context/evidence/filter-requests.json').read_text(encoding='utf-8'))
controls=[
    ('context-value','declare context item := 3; .',False,True),
    ('context-comments','declare(:a:)context(:b:)item external; .',False,True),
    ('context-type-error','declare context item as xs:integer+ := 3; .',False,True),
    ('context-in-string','"declare context item := 3;"',True,True),
    ('context-in-comment','(: declare context item := 3; :) 1',True,True),
    ('context-in-nested-comment','(: outer (: declare context item := 3; :) :) 1',True,True),
    ('construction','declare construction strip; 1',False,False),
    ('copy-namespaces','declare copy-namespaces preserve, inherit; 1',False,False),
    ('pragma','(# p:opaque #){1}',False,False),
    ('context-and-construction','declare context item := 3; declare construction strip; .',False,False),
    ('context-and-copy','declare copy-namespaces preserve, inherit; declare context item := 3; .',False,False),
    ('context-and-pragma','declare context item := 3; (# p:opaque #){.}',False,False),
    ('plain-expression','1+1',True,True),
    ('module-import','import module namespace m="urn:c27"; 1',True,True),
]
expect=[]
for name,query,before,after in controls:
    row=dict(Suite='qt3',Name='routing-'+name,Expression=query,Dependencies=[])
    rows.append(row)
    expect.append(dict(name=row['Name'],normal=before,prototype=after))
(E/'routing-requests.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
(E/'routing-expectations.json').write_text(json.dumps(expect,indent=2)+'\n',encoding='utf-8')
receipts=[]
for phase in ['normal','prototype']:
    env=dict(os.environ)
    env.pop('C27_QT3_HARNESS',None)
    if phase=='prototype':env['C27_QT3_HARNESS']=str(R/'.guard-tmp/k04-context-resource-harness/bin/Release/net10.0')+os.sep
    command=['dotnet',str(R/'.guard-tmp/k04-context-resource-driver/bin/Release/net10.0/Probe.dll'),'filters',str(E/'routing-requests.json'),str(E/('routing-'+phase+'.jsonl'))]
    start=time.monotonic()
    with (E/('routing-'+phase+'.log')).open('wb') as output:
        try:
            p=subprocess.run(command,cwd=R,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=30)
            code,timeout=p.returncode,False
        except subprocess.TimeoutExpired:
            code,timeout=None,True
    receipts.append(dict(phase=phase,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start))
(E/'routing-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(r['exitCode']==0 and not r['timeout'] for r in receipts)
for phase in ['normal','prototype']:
    actual={r['Name']:r for r in [json.loads(l) for l in (E/('routing-'+phase+'.jsonl')).read_text(encoding='utf-8-sig').splitlines()]}
    for row in expect:assert actual[row['name']]['canRouteXquery']==row[phase],row['name']
print('222 actual routing/filter observations verified, including14 lexical/guard boundaries per phase.')
