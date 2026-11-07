"""Pin the finite execution layout before launching through the scoped watchdog."""
from pathlib import Path
import hashlib, json, os, subprocess, sys

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
cohort=read(P/'COHORT.json')
output=P/'workload/bin/Release/net10.0'
for name,row in cohort['references'].items():
    assert sha(Path(row['path']))==sha(output/(name+'.dll'))==row['sha256'],name
for name,key in [('Bosak.Schema','nativeAssembly'),('Bosak.Schema.ExperimentalProvider','providerAssembly')]:
    assert sha(Path(cohort[key]['path']))==sha(output/(name+'.dll'))==cohort[key]['sha256'],name
assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
layout=dict(path=str(output),files={p.relative_to(output).as_posix():sha(p) for p in sorted(output.rglob('*')) if p.is_file()})
assert not (E/'EXECUTION-LAYOUT.json').exists(),'Do not overwrite an earlier launch cohort.'
(E/'EXECUTION-LAYOUT.json').write_text(json.dumps(layout,indent=2)+'\n',encoding='utf-8')
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path']
env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings')
command=[sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),
    '--log',str(R/'docs/foundation-r2-closure/evidence/execution/workload-1811-measure.log'),
    '--timeout-seconds','180','--','dotnet',str(output/'Bosak.Schema.OwnerWorkload.Protocol.dll'),
    str(P/'PROTOCOL.json'),str(E/'MEASUREMENTS.json')]
raise SystemExit(subprocess.run(command,cwd=R,env=env).returncode)
