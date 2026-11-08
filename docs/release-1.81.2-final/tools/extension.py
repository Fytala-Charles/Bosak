"""Package the committed extension and seal its exact stock artifact."""
from pathlib import Path
import json,subprocess,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());s=Path(p['source']);ext=s/'vscode-bosak';out=P/'evidence/vsix-out';out.mkdir();vsce='C:/Users/charl/AppData/Local/npm-cache/_npx/66fbc91407e86cd3/node_modules/@vscode/vsce/vsce'
steps=[('npm-ci',['npm.cmd','ci','--offline','--no-audit','--no-fund'],ext),('extension-compile',['npm.cmd','run','compile'],ext),('extension-tests',['npm.cmd','test'],ext),('vsix-package',['node',vsce,'package','--out',str(out/'vscode-bosak-0.2.6.vsix')],ext),('seal-vscode',['python',str(s/'.github/scripts/release_artifacts.py'),'seal','vscode',str(out)],s)]
for name,cmd,cwd in steps:
 log=P/'evidence'/(name+'.txt');assert not log.exists()
 with log.open('x',encoding='utf-8') as writer:result=subprocess.run(cmd,cwd=cwd,stdout=writer,stderr=subprocess.STDOUT,timeout=240)
 (P/'evidence'/(name+'.json')).write_text(json.dumps({'command':cmd,'exitCode':result.returncode,'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()},indent=2)+'\n');print(name,result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
