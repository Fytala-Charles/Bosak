from pathlib import Path
import json,subprocess,time,os,concurrent.futures,collections
E=Path(__file__).resolve().parent;R=E.parents[3];driver=R/'.guard-tmp/k04-xib-driver/bin/Release/net10.0/Probe.dll'
def env(phase):
 value=dict(os.environ)
 for suite in ['qt3','qt4']:
  key='C40_'+suite.upper()+'_HARNESS';value.pop(key,None)
  if phase=='prototype':value[key]=str(R/('.guard-tmp/k04-xib-harness-'+suite)/'bin/Release/net10.0')+os.sep
 return value
def execute(name,command,phase):
 start=time.monotonic()
 with (E/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,env=env(phase),stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(name=name,phase=phase,command=command,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start,log=name+'.log')
filters=[]
for phase in ['normal','prototype']:
 command=['dotnet',str(driver),'filters',str(E/'filter-requests.json'),str(E/(phase+'-filters.jsonl'))];filters.append(execute(phase+'-filters',command,phase))
(E/'filter-receipts.json').write_text(json.dumps(filters,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in filters)
originals=json.loads((E/'diagnostic-originals.json').read_text(encoding='utf-8'));controls=json.loads((E/'control-requests.json').read_text(encoding='utf-8'))
jobs=[(x,'resources','normal') for x in originals]+[(x,'actual',p) for x in originals for p in ['normal','prototype']]+[(x,'public-xquery','normal') for x in controls]
def run(job):
 row,mode,phase=job;name=row['Suite']+'-'+row['Name']+'-'+mode+'-'+phase;request=E/(name+'-request.json');request.write_text(json.dumps(row)+'\n',encoding='utf-8');receipt=execute(name,['dotnet',str(driver),mode,str(request)],phase)
 receipt.update(suite=row['Suite'],case=row['Name'],mode=mode);receipt['result']=json.loads((E/receipt['log']).read_text(encoding='utf-8-sig').splitlines()[-1]) if receipt['exitCode']==0 else None;return receipt
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,jobs))
(E/'diagnostic-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in rows)
print(json.dumps({phase:dict(collections.Counter(x['result'].get('kind') for x in rows if x['mode']=='actual' and x['phase']==phase)) for phase in ['normal','prototype']}))
