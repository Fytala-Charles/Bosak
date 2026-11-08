from pathlib import Path
import subprocess,json,time
E=Path(__file__).resolve().parent;command=['dotnet','.guard-tmp/k04-sequence-mechanism/bin/Release/net10.0/Probe.dll'];start=time.monotonic();log=E/'mechanism.log'
with log.open('wb') as f:
 try:p=subprocess.run(command,stdout=f,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
rows=[json.loads(l) for l in log.read_text(encoding='utf-8-sig').splitlines()] if code==0 else []
r=dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-start,log=log.name,results=rows);(E/'mechanism-receipt.json').write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8');print(json.dumps(r))
