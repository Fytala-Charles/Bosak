from pathlib import Path
import subprocess,json,os,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def run(job):
 suite,phase=job;project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';name=suite+'-'+phase
 directory=R/'tests'/project/'bin/Release/net10.0' if phase=='normal' else R/('.guard-tmp/k04-xib-harness-'+suite)/'bin/Release/net10.0'
 command=['dotnet',str(directory/(project+'.dll'))]+(['tests/qt4tests'] if suite=='qt4' else []);env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='1';env['BOSAK_'+suite.upper()+'_DUMP_SKIPS']=str(E/(name+'-skips.txt'));start=time.monotonic()
 with (E/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,phase=phase,name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-start,log=name+'.log')
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:rows=list(pool.map(run,[(s,p) for s in ['qt3','qt4'] for p in ['normal','prototype']]))
(E/'gate-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in rows)
print('All four complete normal/prototype gate receipts retained.')
