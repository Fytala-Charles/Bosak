"""Run rebuilt owner suites with bounded scoped receipts and package-load observations."""
from pathlib import Path
import hashlib,json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';p=json.loads((P/'PLAN.json').read_text());retry=sys.argv[1:]==['--retry']
review=P/'PROCESS-REVIEW.json'
if review.exists() and not json.loads(review.read_text())['explicitUserAuthorizationReceived']:
    raise SystemExit('Full suite includes process-termination probes rejected by automatic review. Use the approved in-process scope; explicit authorization is required before revisiting Integration.')
sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
source=Path(p['source']);cohort=json.loads((R/'docs/foundation-package-workload/COHORT.json').read_text())
assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['NUGET_PACKAGES']=p['cache']
rows=[];output=E/('TEST-EXECUTIONS-RETRY.json' if retry else 'TEST-EXECUTIONS.json');assert not output.exists()
for project,count in [('Bosak.XmlDb.Xdm.Tests',47),('Bosak.XmlDb.Storage.Tests',3572),('Bosak.XmlDb.IntegrationTests',863)]:
    if retry and project=='Bosak.XmlDb.Xdm.Tests' and (E/'INPROCESS-EXECUTIONS.json').exists():
        passed=json.loads((E/'INPROCESS-EXECUTIONS.json').read_text())
        if any(r['project']==project and r['exitCode']==0 for r in passed):continue
    folder=source/'tests'/project/'bin/Release/net10.0';binary=folder/(project+'.dll')
    slug=project+('-retry' if retry else '')
    env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings'/slug)
    files={f.relative_to(folder).as_posix():sha(f) for f in sorted(folder.rglob('*')) if f.is_file()}
    print('Running',project,'expected',count,flush=True)
    log=R/f'docs/foundation-r2-closure/evidence/execution/xmldb-source-test-{slug}.log'
    code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),
        '--timeout-seconds','7200' if project=='Bosak.XmlDb.Storage.Tests' else '1800','--','dotnet','vstest',str(binary),'--logger:trx','--ResultsDirectory:'+str(E/'trx'/slug)],
        cwd=R,env=env).returncode
    rows.append(dict(project=project,expected=count,path=str(folder),binarySha256=sha(binary),files=files,exitCode=code,log=log.relative_to(R).as_posix(),slug=slug))
    output.write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
    if code:raise SystemExit(code)
print('Completed all three rebuilt XmlDb suites.',flush=True)
