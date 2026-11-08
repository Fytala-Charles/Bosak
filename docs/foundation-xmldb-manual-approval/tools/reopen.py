"""Run only the layout-dependent reopen case with unchanged pinned payloads."""
from pathlib import Path
import hashlib,json,os,subprocess,sys
R=Path(__file__).resolve().parents[3];P=R/'docs/foundation-xmldb-manual-approval'
read=lambda p:json.loads(p.read_text());sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
plan=read(P/'REOPEN-LAYOUT.json');root=Path(plan['root'])
assert plan['files']=={f.relative_to(root).as_posix():sha(f) for f in root.rglob('*') if f.is_file()}
cohort=read(R/'docs/foundation-package-workload/COHORT.json');assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(P/'bindings-reopen')
command=[sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(R/'docs/foundation-r2-closure/evidence/execution/xmldb-reopen-layout-20261108.log'),'--timeout-seconds','120','--','dotnet','vstest',plan['testDll'],'--TestCaseFilter:'+plan['filter'],'--logger:trx','--logger:console;verbosity=normal','--ResultsDirectory:'+str(P/'trx-reopen')]
raise SystemExit(subprocess.run(command,cwd=R,env=env).returncode)
