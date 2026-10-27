from pathlib import Path
import subprocess,json,time,os,concurrent.futures
E=Path(__file__).resolve().parent

def run(suite):
 project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';command=['dotnet','.guard-tmp/k04-calendar-harness-'+suite+'/bin/Release/net10.0/'+project+'.dll']+([] if suite=='qt3' else ['tests/qt4tests']);env=dict(os.environ);env['BOSAK_'+('QT3' if suite=='qt3' else 'QT4')+'_DUMP_SKIPS']=str(E/(suite+'-rehearsal-skips.txt'));env['BOSAK_QT4_GATE_ONLY']='1';log=E/(suite+'-rehearsal.log');start=time.monotonic()
 with log.open('wb') as f:
  try:p=subprocess.run(command,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=300);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(suite=suite,command=command,exitCode=code,timeout=timeout,watchdogSeconds=300,wallSeconds=time.monotonic()-start,log=log.name,skipDump=suite+'-rehearsal-skips.txt',mode='Isolated copied complete harness;normal source/AGENTS unchanged')
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:rows=list(pool.map(run,['qt3','qt4']))
(E/'rehearsal-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');print(json.dumps(rows))
