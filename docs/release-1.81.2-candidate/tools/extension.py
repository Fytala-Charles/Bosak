"""Prepare a fresh-lock local VSIX against the approved1.81.2 server."""
from pathlib import Path
import json,subprocess,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());ext=Path(p['work'])/'extension'
vsce='C:/Users/charl/AppData/Local/npm-cache/_npx/66fbc91407e86cd3/node_modules/@vscode/vsce/vsce'
for name,cmd in [('npm-ci',['npm.cmd','ci','--offline','--no-audit','--no-fund']),('extension-compile',['npm.cmd','run','compile']),('extension-tests',['npm.cmd','test']),('vsix-package',['node',vsce,'package','--out',str(P/'evidence/vscode-bosak-0.2.6-engine-1.81.2.vsix')])]:
 log=P/'evidence'/(name+'.txt');assert not log.exists()
 with log.open('x',encoding='utf-8') as out:result=subprocess.run(cmd,cwd=ext,stdout=out,stderr=subprocess.STDOUT,timeout=180)
 (P/'evidence'/(name+'.json')).write_text(json.dumps({'command':cmd,'exitCode':result.returncode,'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()},indent=2)+'\n');print(name,result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
