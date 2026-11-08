from pathlib import Path
import json,subprocess,time,concurrent.futures
E=Path('docs/conformance/k02-relative-schema-uri/evidence');P=Path('.guard-tmp/k02-wide/requests')
P.mkdir(parents=True,exist_ok=True)
previous=Path('docs/conformance/k02-schema-inventory/evidence')
eligible=[x for x in json.loads((previous/'metadata-results.json').read_text(encoding='utf-8')) if x['result']['deferred']]
byid={x['id']:x for x in json.loads((previous/'schema-cases.json').read_text(encoding='utf-8'))}
for v in eligible:(P/(str(v['index'])+'.json')).write_text(json.dumps([byid[v['id']]],ensure_ascii=False)+'\n',encoding='utf-8')
started=time.monotonic();jobs=[(v,mode) for v in eligible for mode in ['current','guarded']]
def run(item):
 meta,mode=item;index=meta['index'];q=byid[meta['id']];request=P/(str(index)+'.json');log=E/('compare-'+str(index)+'-'+mode+'.log');command=['dotnet','.guard-tmp/k02-c02-driver/bin/Release/net10.0/K02WideDiagnostic.dll',str(request),q['id'],mode];start=time.monotonic()
 if start-started>240:return dict(index=index,id=q['id'],mode=mode,exitCode=None,timeout=False,budgetExceeded=True,result=None)
 with log.open('wb') as output:
  try:p=subprocess.run(command,stdout=output,stderr=subprocess.STDOUT,timeout=20);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 result=None
 if code==0:
  try:result=json.loads(log.read_text(encoding='utf-8-sig').splitlines()[-1])
  except(ValueError,IndexError):pass
 return dict(index=index,id=q['id'],mode=mode,exitCode=code,timeout=timeout,budgetExceeded=False,watchdogSeconds=20,wallSeconds=time.monotonic()-start,command=command,log=log.name,result=result)
out=[]
with (E/'comparison-results.jsonl').open('w',encoding='utf-8') as checkpoint,concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
 for v in pool.map(run,jobs):
  out.append(v);checkpoint.write(json.dumps(v,ensure_ascii=False)+'\n');checkpoint.flush()
  if len(out)%50==0 or v['exitCode']!=0:print(len(out),'of',len(jobs),'elapsed',round(time.monotonic()-started,1),v['id'],v['mode'],v['result']['kind'] if v['result'] else v['exitCode'],flush=True)
(E/'comparison-results.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print('Finished',len(out),'comparison processes',flush=True)
