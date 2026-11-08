"""Build and package an isolated unpublished extension candidate."""
from pathlib import Path
import json,subprocess,sys,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];plan=json.loads((P/'PLAN.json').read_text());w=Path(plan['work']);ext=Path(plan['extension'])
steps=[('server',['dotnet','publish',plan['project'],'-c','Release','-p:UseAppHost=false','-p:Version='+plan['serverVersion'],'-p:SourceRevisionId='+plan['sourceCandidate'],'-p:RepositoryCommit='+plan['sourceCandidate'],'-p:NuGetAudit=false','--configfile',str(w/'NuGet.Config'),'-o',str(ext/'server')],R),('compile',['npm.cmd','run','compile'],ext),('tests',['npm.cmd','test'],ext),('package',['node',plan['packager']['path'],'package','--out',str(P/'vscode-bosak-0.2.6-uri-fix.vsix')],ext)]
for name,cmd,cwd in steps:
 log=P/(name+'.txt');assert not log.exists(),'Preserve previous execution receipts'
 with log.open('x',encoding='utf-8') as out:
  result=subprocess.run(cmd,cwd=cwd,stdout=out,stderr=subprocess.STDOUT,timeout=240)
 receipt={'command':cmd,'cwd':str(cwd),'exitCode':result.returncode,'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()};(P/(name+'-execution.json')).write_text(json.dumps(receipt,indent=2)+'\n')
 print(name, result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
