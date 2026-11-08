from pathlib import Path
import subprocess,time,json
E=Path(__file__).resolve().parent;R=E.parents[3];receipts=[]
for phase,directory in [('normal','tests/Bosak.XPath.Conformance/bin/Release/net10.0'),('prototype','.guard-tmp/k04-pragma-fallback-harness/bin/Release/net10.0')]:
 command=['dotnet',str(R/'.guard-tmp/k04-pragma-fallback-inventory/bin/Release/net10.0/Probe.dll'),directory,str(E/phase),phase,str(E/'lexical-controls.json')];start=time.monotonic()
 with (E/(phase+'-inventory.log')).open('wb') as f:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=60)
 receipts.append(dict(phase=phase,command=command,exitCode=p.returncode,timeout=False,watchdogSeconds=60,wallSeconds=time.monotonic()-start))
(E/'inventory-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(x['exitCode']==0 for x in receipts)
rows=[json.loads(l)['row'] for l in (E/'normal-originals.jsonl').read_text(encoding='utf-8').splitlines()]
requests=[dict(Suite='qt3',Name=x['Name'],SuiteRoot=str(R/'tests/qt3tests'),SetFile=str(R/'tests/qt3tests'/x['File'])) for x in rows]
(E/'selected-originals.json').write_text(json.dumps(requests,indent=2)+'\n',encoding='utf-8')
print('Complete inventory and selected original requests recorded.')
