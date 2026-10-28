from pathlib import Path
import subprocess,time,json
E=Path(__file__).resolve().parent;R=E.parents[3];command=['dotnet',str(R/'.guard-tmp/k04-construction-modes-typing-probe/bin/Release/net10.0/Probe.dll')];start=time.monotonic()
p=subprocess.run(command,cwd=R,capture_output=True,timeout=30);(E/'typing.log').write_bytes(p.stdout+p.stderr)
(E/'typing-receipt.json').write_text(json.dumps(dict(command=command,exitCode=p.returncode,timeout=False,watchdogSeconds=30,wallSeconds=time.monotonic()-start),indent=2)+'\n',encoding='utf-8');assert p.returncode==0
x=json.loads(p.stdout);assert x['sourceUnchanged'] and all(r['source']==r['copied'] and not r['sameNode'] for r in x['outcomes'])
(E/'typing-results.json').write_text(json.dumps(x,indent=2)+'\n',encoding='utf-8');print('All three default-mode typed copies exactly preserve the original metadata snapshots.')
