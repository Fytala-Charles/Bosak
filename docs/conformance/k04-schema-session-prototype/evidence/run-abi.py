from pathlib import Path
import subprocess,time,json,shutil,hashlib
E=Path(__file__).resolve().parent;R=E.parents[3];T=R/'.guard-tmp/k04-schema-session-abi';receipts=[]
def run(name,command,seconds=60):
 start=time.monotonic();log=name+'.log'
 with (E/log).open('wb') as f:
  try:p=subprocess.run(command,cwd=R,stdout=f,stderr=subprocess.STDOUT,timeout=seconds);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 receipt=dict(name=name,command=command,exitCode=code,timeout=timeout,watchdogSeconds=seconds,wallSeconds=time.monotonic()-start,log=log);receipts.append(receipt);assert code==0 and not timeout,name;return receipt
run('abi-consumer-build',['dotnet','build',str(T/'consumer/OldConsumer.csproj'),'-c','Release']);run('abi-v2-build',['dotnet','build',str(T/'v2/CarrierAbi.csproj'),'-c','Release'])
source=T/'consumer/bin/Release/net10.0';deployment=T/'upgraded';shutil.copytree(source,deployment,dirs_exist_ok=True);shutil.copy2(T/'v2/bin/Release/net10.0/CarrierAbi.dll',deployment/'CarrierAbi.dll')
for name,directory in [('abi-v1',source),('abi-v2',deployment)]:
 receipt=run(name,['dotnet',str(directory/'OldConsumer.dll')],30);result=json.loads((E/receipt['log']).read_text(encoding='utf-8-sig'));(E/(name+'-results.json')).write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
identities={p.relative_to(R).as_posix():sha(p) for directory in [source,deployment] for p in directory.glob('*.dll')}
assert sha(source/'OldConsumer.dll')==sha(deployment/'OldConsumer.dll') and sha(source/'CarrierAbi.dll')!=sha(deployment/'CarrierAbi.dll')
(E/'abi-binary-identities.json').write_text(json.dumps(identities,indent=2)+'\n',encoding='utf-8');(E/'abi-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8');print('Identical v1-compiled consumer passed with both synthetic library revisions.')
