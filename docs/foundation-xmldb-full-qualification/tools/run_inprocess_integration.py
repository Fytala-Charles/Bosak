"""Run only NativeSnapshotWorkTests; no Integration kill/fault-process cases selected."""
from pathlib import Path
import hashlib,json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');layout=read(E/'GUARDED-INTEGRATION.json')['layout'];folder=Path(layout['path'])
assert layout['files']=={f.relative_to(folder).as_posix():sha(f) for f in folder.rglob('*') if f.is_file()}
source=Path(p['source'])/'tests/Bosak.XmlDb.IntegrationTests/NativeSnapshotWorkTests.cs'
text=source.read_text(encoding='utf-8-sig')
assert not any(token in text for token in ['Process.Start','ProcessStartInfo','.Kill(','FailFast'])
cohort=read(R/'docs/foundation-package-workload/COHORT.json');assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings/integration-inprocess')
log=R/'docs/foundation-r2-closure/evidence/execution/xmldb-full-integration-inprocess.log'
code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),'--timeout-seconds','300','--',
    'dotnet','vstest',str(folder/'Bosak.XmlDb.IntegrationTests.dll'),'--logger:trx','--logger:console;verbosity=normal',
    '--TestCaseFilter:FullyQualifiedName~Bosak.XmlDb.IntegrationTests.NativeSnapshotWorkTests',
    '--ResultsDirectory:'+str(E/'trx/integration-inprocess')],cwd=R,env=env).returncode
(E/'integration-inprocess-execution.json').write_text(json.dumps(dict(exitCode=code,log=log.relative_to(R).as_posix(),
    layout=layout,filter='FullyQualifiedName~Bosak.XmlDb.IntegrationTests.NativeSnapshotWorkTests',
    expected=39,processProbesSelected=False,reviewedSourceSha256=sha(source)),indent=2)+'\n')
raise SystemExit(code)
