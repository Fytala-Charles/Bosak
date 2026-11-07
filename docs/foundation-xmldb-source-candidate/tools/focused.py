"""Run only the focused Storage cleanup suite; exclude all Integration process probes."""
from pathlib import Path
import hashlib,json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';p=json.loads((P/'PLAN.json').read_text())
source=Path(p['source']);sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
cohort=json.loads((R/'docs/foundation-package-workload/COHORT.json').read_text())
assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['NUGET_PACKAGES']=p['cache']
output=E/'FOCUSED-EXECUTIONS.json';assert not output.exists();rows=[]
for project,count in [('Bosak.XmlDb.Storage.Tests',139)]:
    for file in (source/'tests'/project).glob('*.cs'):
        text=file.read_text(encoding='utf-8-sig')
        assert not any(token in text for token in ['Process.Start','ProcessStartInfo','.Kill(']),file
    folder=source/'tests'/project/'bin/Release/net10.0';binary=folder/(project+'.dll');slug=project+'-focused'
    env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings'/slug)
    files={f.relative_to(folder).as_posix():sha(f) for f in sorted(folder.rglob('*')) if f.is_file()}
    log=R/f'docs/foundation-r2-closure/evidence/execution/xmldb-source-test-{slug}.log'
    print('Running in-process suite',project,flush=True)
    code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),
        '--timeout-seconds','900','--','dotnet','vstest',str(binary),'--logger:trx','--TestCaseFilter:FullyQualifiedName~NativeSnapshotCleanupTests','--ResultsDirectory:'+str(E/'trx'/slug)],
        cwd=R,env=env).returncode
    rows.append(dict(project=project,expected=count,path=str(folder),files=files,exitCode=code,slug=slug,log=log.relative_to(R).as_posix()))
    output.write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
    if code:raise SystemExit(code)
print('Completed focused cleanup only. Integration process probes were not launched.',flush=True)
