from pathlib import Path
import json,subprocess,os,time,concurrent.futures
r=Path.cwd();receipts=[];hook=r/'.guard-tmp/binary-load-hook/bin/Release/net10.0/Hook.dll';assert hook.exists()
def run(consumer,mode,kind):
 root=r/f'.guard-tmp/binary-{consumer}-{mode}';tag=f'{consumer}-{mode}-{kind}';dest=r/f'.guard-tmp/binary-results/{tag}';dest.mkdir(parents=True,exist_ok=True)
 traces=dest/'loads';env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(hook);env['BINARY_QUAL_TRACE_DIR']=str(traces)
 if kind=='probe':cmd=['dotnet',str(root/'probe/bin/Release/net10.0/Probe.dll'),str(dest/'observations.json')];bound=90
 else:
  names=['Core','Application','Infrastructure','Workbench'] if consumer=='braid' else ['Xdm']
  assemblies=[root/f'tests/Bosak.{"Braid" if consumer=="braid" else "XmlDb"}.{name}.Tests/bin/Release/net10.0/Bosak.{"Braid" if consumer=="braid" else "XmlDb"}.{name}.Tests.dll' for name in names]
  cmd=['dotnet','vstest',*[str(p) for p in assemblies],'/Logger:trx',f'/ResultsDirectory:{dest}'];bound=300
 start=time.monotonic()
 with (dest/'process.txt').open('w',encoding='utf-8') as log:
  process=subprocess.Popen(cmd,stdout=log,stderr=subprocess.STDOUT,env=env,cwd=root)
  try:code=process.wait(timeout=bound);row={'tag':tag,'exitCode':code,'timedOut':False}
  except subprocess.TimeoutExpired:
   subprocess.run(['taskkill','/PID',str(process.pid),'/T','/F'],stdout=log,stderr=subprocess.STDOUT);row={'tag':tag,'timedOut':True}
 row.update(elapsedSeconds=round(time.monotonic()-start,3),externalTimeoutSeconds=bound,command=cmd);print(row,flush=True);return row
for mode in ['baseline','replacement']:
 with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(lambda x:run(x[0],mode,x[1]),[('xmldb','tests'),('xmldb','probe'),('braid','tests')]))
 receipts.extend(rows);(r/'.guard-tmp/binary-execution-processes.json').write_text(json.dumps(receipts,indent=2)+'\n')
 if mode=='baseline' and any(row.get('exitCode')!=0 or row['timedOut'] for row in rows):print('Baseline controls incomplete; replacement phase withheld.',flush=True);break
