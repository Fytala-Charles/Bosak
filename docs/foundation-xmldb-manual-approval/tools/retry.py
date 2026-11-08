"""Execute explicitly authorized, hash-pinned full lanes through scoped watchdogs."""
from pathlib import Path
import argparse,hashlib,json,os,subprocess,sys
R=Path(__file__).resolve().parents[3];P=R/'docs/foundation-xmldb-full-qualification';E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
parser=argparse.ArgumentParser();parser.add_argument('--lane',choices=['storage','integration'],required=True);a=parser.parse_args()
p=read(P/'PLAN.json');auth=read(P/'AUTHORIZATION.json');assert auth['explicitUserAuthorizationReceived'] and sha(P/'AUTHORIZATION.json')==p['authorizationSha256']
# Fresh tool-level automatic review is required for this separate attempt.
# The consumed historical approval is not reused or reset.
assert a.lane == 'integration'
assert auth['unexpectedManagedFailuresMustBeCaught'] and not auth['machineWideErrorSettingsAllowed']
layout=p['storageLayout'] if a.lane=='storage' else read(E/'GUARDED-INTEGRATION.json')['layout']
folder=Path(layout['path']);assert layout['files']=={f.relative_to(folder).as_posix():sha(f) for f in folder.rglob('*') if f.is_file()}
project='Bosak.XmlDb.Storage.Tests' if a.lane=='storage' else 'Bosak.XmlDb.IntegrationTests'
cohort=read(R/'docs/foundation-package-workload/COHORT.json');assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(R/'docs/foundation-xmldb-manual-approval/bindings-fresh-review')
log=R/f'docs/foundation-r2-closure/evidence/execution/xmldb-full-{a.lane}-fresh-review-20261108.log'
code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),
    '--timeout-seconds','7200' if a.lane=='storage' else '1800','--','dotnet','vstest',str(folder/(project+'.dll')),
    '--logger:trx','--logger:console;verbosity=normal','--ResultsDirectory:'+str(R/'docs/foundation-xmldb-manual-approval/trx-fresh-review')],cwd=R,env=env).returncode
(R/'docs/foundation-xmldb-manual-approval/fresh-review-execution.json').write_text(json.dumps(dict(lane=a.lane,exitCode=code,log=log.relative_to(R).as_posix(),layout=layout),indent=2)+'\n')
raise SystemExit(code)
