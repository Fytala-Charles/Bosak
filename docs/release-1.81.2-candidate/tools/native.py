"""Replay unchanged native/prototype/consumer tests against the new package bytes."""
from pathlib import Path
import json,os,hashlib,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1];rows=json.loads((P/'NATIVE-LAYOUTS.json').read_text());env=os.environ.copy();hook=json.loads((R/'docs/foundation-package-workload/COHORT.json').read_text())['hook'];assert hashlib.sha256(Path(hook['path']).read_bytes()).hexdigest()==hook['sha256'];env['DOTNET_STARTUP_HOOKS']=hook['path']
for row in rows:
 env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(P/'evidence/bindings'/row['key']);cmd=['dotnet','vstest',row['binary'],'--logger:trx','--ResultsDirectory:'+str(P/'evidence/native-trx'/row['key'])];log=R/'docs/foundation-r2-closure/evidence/execution'/('candidate-1812-'+row['key']+'.log');code=subprocess.run([os.sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),'--timeout-seconds','300','--',*cmd],env=env,cwd=R).returncode
 (P/'evidence'/(row['key']+'-execution.json')).write_text(json.dumps({'command':cmd,'exitCode':code,'log':log.relative_to(R).as_posix()},indent=2)+'\n');print(row['key'],code,flush=True)
 if code:raise SystemExit(code)
