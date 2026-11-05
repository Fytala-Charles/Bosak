from pathlib import Path
import subprocess,json,os,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def run(suite):
 project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';command=['dotnet',str(R/'tests'/project/'bin/Release/net10.0'/(project+'.dll')),'tests/'+suite+'tests','fn-normalize-unicode'];env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env['BOSAK_'+suite.upper()+'_DUMP_SKIPS']=str(E/(suite+'-normal-skips.txt'));start=time.monotonic()
 with (E/(suite+'-normal.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=120);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,command=command,exitCode=code,timeout=timeout,watchdogSeconds=120,wallSeconds=time.monotonic()-start)
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:receipts=list(pool.map(run,['qt3','qt4']))
(E/'normal-set-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');assert all(r['exitCode']==0 and not r['timeout'] for r in receipts);print('Actual normal normalization sets finished; inspect assertion results separately from exit status.')
