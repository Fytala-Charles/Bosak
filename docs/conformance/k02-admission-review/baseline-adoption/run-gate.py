from pathlib import Path
import subprocess,json,os,time
E=Path(__file__).resolve().parent
command=['dotnet','run','--project','tests/Bosak.XPath.Conformance','-c','Release','--no-build']
env=dict(os.environ);env['BOSAK_QT3_DUMP_SKIPS']=str(E/'qt3-skips.txt');started=time.monotonic()
with (E/'qt3-normal-gate.log').open('wb') as output:
 try:p=subprocess.run(command,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
receipt=dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-started,log='qt3-normal-gate.log',skipDump='qt3-skips.txt',mode='Actual normal Program entry point; no reflection or diagnostic flag injection')
(E/'gate-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print(json.dumps(receipt))
if code!=0 or timeout:raise SystemExit(1)
