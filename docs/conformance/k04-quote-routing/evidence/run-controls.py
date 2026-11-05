from pathlib import Path
import json,subprocess,time
E=Path(__file__).resolve().parent;rows=[]
for mode in ['current','prototype']:
 command=['dotnet','.guard-tmp/k04-quote-driver/bin/Release/net10.0/Probe.dll','can-handle',str(E/'controls.json'),mode];log=E/('controls-'+mode+'.log');started=time.monotonic()
 with log.open('wb') as output:
  try:p=subprocess.run(command,stdout=output,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1]) if code==0 else None
 rows.append(dict(mode=mode,command=command,exitCode=code,timeout=timeout,watchdogSeconds=20,wallSeconds=time.monotonic()-started,result=result,log=log.name))
(E/'control-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');print('Completed lexical controls')
