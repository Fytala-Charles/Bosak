from pathlib import Path
import subprocess,time,json
E=Path(__file__).resolve().parent;R=E.parents[3];receipts=[]
for suite,project in [('qt3','Bosak.XPath.Conformance'),('qt4','Bosak.XPath.Conformance40')]:
 command=['dotnet',str(R/'.guard-tmp/k04-independent-profile-inventory/bin/Release/net10.0/Probe.dll'),str(R/'tests'/project/'bin/Release/net10.0'),suite,str(E/suite),str(E/'review-names.json'),str(E/'polarity-requests.json')];start=time.monotonic()
 with (E/(suite+'-inventory.log')).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=60);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 receipts.append(dict(suite=suite,command=command,exitCode=code,timeout=timeout,watchdogSeconds=60,wallSeconds=time.monotonic()-start))
(E/'inventory-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(x['exitCode']==0 and not x['timeout'] for x in receipts)
print('Both complete actual dependency inventories recorded.')
