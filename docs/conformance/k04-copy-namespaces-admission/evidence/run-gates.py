from pathlib import Path
import subprocess,json,os,time,concurrent.futures
E=Path(__file__).resolve().parent;R=E.parents[3]
def run(job):
 name,project,custom=job
 directory=R/custom if custom else R/'tests'/project/'bin/Release/net10.0'
 command=['dotnet',str(directory/(project+'.dll'))]+(['tests/qt4tests'] if name=='qt4-normal' else [])
 env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='1';env['BOSAK_'+('QT4' if name=='qt4-normal' else 'QT3')+'_DUMP_SKIPS']=str(E/(name+'-skips.txt'));start=time.monotonic()
 with (E/(name+'.log')).open('wb') as output:
  try:p=subprocess.run(command,cwd=R,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-start,log=name+'.log')
jobs=[('qt3-normal','Bosak.XPath.Conformance',None),('qt3-prototype','Bosak.XPath.Conformance','.guard-tmp/k04-copy-admission-harness/bin/Release/net10.0'),('qt4-normal','Bosak.XPath.Conformance40',None)]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,jobs))
(E/'gate-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
assert all(not r['timeout'] and r['exitCode'] in [0,2] for r in rows)
print('Complete normal/prototype gate receipts retained.')
