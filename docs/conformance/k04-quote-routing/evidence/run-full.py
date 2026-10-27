from pathlib import Path
import subprocess,os,json,time
E=Path(__file__).resolve().parent;env=dict(os.environ);env['BOSAK_QT3_DUMP_SKIPS']=str(E/'rehearsal-skips.txt');command=['dotnet','.guard-tmp/k04-quote-harness/bin/Release/net10.0/Bosak.XPath.Conformance.dll'];started=time.monotonic()
with (E/'rehearsal-full-run.log').open('wb') as f:
 try:p=subprocess.run(command,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
receipt=dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-started,log='rehearsal-full-run.log',skipDump='rehearsal-skips.txt',mode='Isolated copied complete harness with quote predicate corrected; normal tracked source unchanged')
(E/'rehearsal-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print(json.dumps(receipt))
