from pathlib import Path
import subprocess,json,os,time,concurrent.futures
A=Path(__file__).resolve().parent;R=A.parents[3]
def run(suite):
 project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';command=['dotnet',str(R/'tests'/project/'bin/Release/net10.0'/(project+'.dll'))]+([] if suite=='qt3' else ['tests/qt4tests']);env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='1';env['BOSAK_'+suite.upper()+'_DUMP_SKIPS']=str(A/(suite+'-skips.txt'));start=time.monotonic()
 with (A/(suite+'.log')).open('wb') as f:
  try:process=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=300);code=process.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-start)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:receipts=list(pool.map(run,['qt3','qt4']))
(A/'gate-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts);print('Fresh complete normal gates finished successfully.')
