"""Execute explicitly authorized, hash-pinned full lanes through scoped watchdogs."""
from pathlib import Path
import argparse,hashlib,json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
parser=argparse.ArgumentParser();parser.add_argument('--lane',choices=['storage','integration'],required=True);a=parser.parse_args()
p=read(P/'PLAN.json');auth=read(P/'AUTHORIZATION.json');assert auth['explicitUserAuthorizationReceived'] and sha(P/'AUTHORIZATION.json')==p['authorizationSha256']
if a.lane=='integration' and (P/'INTEGRATION-RESTRICTION.json').exists():
    raise SystemExit('Full Integration was rejected by automatic review despite user authorization. Do not retry or bypass this method; only separately approved in-process scope is available.')
assert auth['unexpectedManagedFailuresMustBeCaught'] and not auth['machineWideErrorSettingsAllowed']
layout=p['storageLayout'] if a.lane=='storage' else read(E/'GUARDED-INTEGRATION.json')['layout']
folder=Path(layout['path']);assert layout['files']=={f.relative_to(folder).as_posix():sha(f) for f in folder.rglob('*') if f.is_file()}
project='Bosak.XmlDb.'+('Storage' if a.lane=='storage' else 'Integration')+'.Tests'
cohort=read(R/'docs/foundation-package-workload/COHORT.json');assert sha(Path(cohort['hook']['path']))==cohort['hook']['sha256']
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=cohort['hook']['path'];env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings'/a.lane)
log=R/f'docs/foundation-r2-closure/evidence/execution/xmldb-full-{a.lane}.log'
code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),
    '--timeout-seconds','7200' if a.lane=='storage' else '1800','--','dotnet','vstest',str(folder/(project+'.dll')),
    '--logger:trx','--logger:console;verbosity=normal','--ResultsDirectory:'+str(E/'trx'/a.lane)],cwd=R,env=env).returncode
(E/(a.lane+'-execution.json')).write_text(json.dumps(dict(lane=a.lane,exitCode=code,log=log.relative_to(R).as_posix(),layout=layout),indent=2)+'\n')
raise SystemExit(code)
