from pathlib import Path
import json,subprocess,shutil,time,os,concurrent.futures
A=Path(__file__).resolve().parent;D=A.parent;E=D/'evidence';R=A.parents[3]
def execute(name,command,seconds=60):
 env=dict(os.environ);env.pop('C40_QT3_HARNESS',None);env.pop('C40_QT4_HARNESS',None);start=time.monotonic()
 with (A/(name+'.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=seconds);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 return dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=seconds,wallSeconds=time.monotonic()-start,log=name+'.log')
inventories=[]
for suite,project in [('qt3','Bosak.XPath.Conformance'),('qt4','Bosak.XPath.Conformance40')]:
 command=['dotnet',str(R/'.guard-tmp/k04-xib-inventory/bin/Release/net10.0/Probe.dll'),str(R/'tests'/project/'bin/Release/net10.0'),suite,str(A/(suite+'-adopted')),str(E/'review-names.json'),str(E/'polarity-requests.json')]
 inventories.append(execute(suite+'-inventory',command))
(A/'inventory-receipts.json').write_text(json.dumps(inventories,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in inventories)
# A distinct driver preserves qualification binaries while referencing the rebuilt normal engine.
T=R/'.guard-tmp/k04-xib-adoption-driver';T.mkdir(exist_ok=True)
shutil.copyfile(E/'executor.cs.txt',T/'Program.cs');shutil.copyfile(E/'executor.csproj.txt',T/'Probe.csproj')
build=execute('driver-build',['dotnet','build',str(T/'Probe.csproj'),'-c','Release']);assert build['exitCode']==0 and not build['timeout']
driver=T/'bin/Release/net10.0/Probe.dll';receipt=execute('adopted-filters',['dotnet',str(driver),'filters',str(E/'filter-requests.json'),str(A/'adopted-filters.jsonl')],30)
(A/'filter-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');assert receipt['exitCode']==0 and not receipt['timeout']
originals=json.loads((E/'diagnostic-originals.json').read_text(encoding='utf-8'));controls=json.loads((E/'control-requests.json').read_text(encoding='utf-8'))
jobs=[(x,mode) for x in originals for mode in ['resources','actual']]+[(x,'public-xquery') for x in controls]
def run(job):
 row,mode=job;name=row['Suite']+'-'+row['Name']+'-'+mode;request=A/(name+'-request.json');request.write_text(json.dumps(row)+'\n',encoding='utf-8');receipt=execute(name,['dotnet',str(driver),mode,str(request)],30)
 receipt.update(suite=row['Suite'],case=row['Name'],mode=mode);receipt['result']=json.loads((A/receipt['log']).read_text(encoding='utf-8-sig').splitlines()[-1]) if receipt['exitCode']==0 else None;return receipt
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,jobs))
(A/'diagnostic-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8');assert all(x['exitCode']==0 and not x['timeout'] for x in receipts)
print('Both adopted catalogs, complete filter matrix and original/public controls replayed.')
