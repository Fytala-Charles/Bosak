"""Retain repository-required build/test command output for the editor-only fix."""
from pathlib import Path
import subprocess,json,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1]
for name,cmd in [('solution-build',['dotnet','build','Bosak.sln','-c','Release','--no-restore']),('solution-tests',['dotnet','test','Bosak.sln','-c','Release','--no-build','--logger:trx','--results-directory',str(P/'solution-trx')])]:
 f=P/(name+'.txt');assert not f.exists()
 with f.open('x',encoding='utf-8') as out:result=subprocess.run(cmd,cwd=R,stdout=out,stderr=subprocess.STDOUT,timeout=900)
 (P/(name+'.json')).write_text(json.dumps({'command':cmd,'exitCode':result.returncode,'logSha256':hashlib.sha256(f.read_bytes()).hexdigest()},indent=2)+'\n');print(name,result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
