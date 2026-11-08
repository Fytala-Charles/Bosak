from pathlib import Path
import subprocess,json,time
E=Path(__file__).resolve().parent;R=E.parents[3];command=['dotnet',str(R/'.guard-tmp/k04-normalization-corpus/bin/Release/net10.0/Probe.dll')];start=time.monotonic()
with (E/'corpus.log').open('wb') as f:
 try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=120);code=p.returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
(E/'corpus-receipt.json').write_text(json.dumps(dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=120,wallSeconds=time.monotonic()-start),indent=2)+'\n');assert code==0 and not timeout
result=json.loads((E/'corpus.log').read_text(encoding='utf-8-sig').splitlines()[-1]);(E/'corpus-results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
