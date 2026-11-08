"""Launch only pinned overlay layouts through the scoped watchdog, with fresh receipts."""
from pathlib import Path
import argparse,hashlib,json,os,subprocess,sys

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
parser=argparse.ArgumentParser();parser.add_argument('--lane',choices=['units','database','gates'],required=True);parser.add_argument('--resume',action='store_true');args=parser.parse_args()
plan=json.loads((P/'PLAN.json').read_text());cohort=json.loads((R/'docs/foundation-workload-1811/COHORT.json').read_text())
hook=Path(cohort['hook']['path']);assert sha(hook)==cohort['hook']['sha256']
executions=[];result_path=E/(args.lane+('-resume' if args.resume else '')+'-executions.json');assert not result_path.exists()
completed={r['key'] for r in json.loads((E/(args.lane+'-executions.json')).read_text()) if r['exitCode']==0} if args.resume else set()
for key,layout in plan['layouts'].items():
    db='database.tests' in key;gate=key in ['qt3','qt4']
    if not ((args.lane=='database' and db) or (args.lane=='gates' and gate) or (args.lane=='units' and not db and not gate)):continue
    if key in completed:continue
    root=Path(layout['path']);assert layout['files']=={p.relative_to(root).as_posix():sha(p) for p in root.rglob('*') if p.is_file()},key
    slug=key.replace('/','-')+('-resume' if args.resume else '');env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(hook)
    env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings'/slug)
    command=['dotnet'];extra=[]
    if gate:
        env['BOSAK_QT'+('3' if key=='qt3' else '4')+'_DUMP_SKIPS']=str(E/(key+'-skips.txt'))
        if key=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
        command += [layout['binary'],str(R/'tests'/('qt3tests' if key=='qt3' else 'qt4tests'))]
    else:
        command += ['vstest',layout['binary'],'--logger:trx','--ResultsDirectory:'+str(E/'trx'/slug)]
    log=R/f'docs/foundation-r2-closure/evidence/execution/preview-qualification-{slug}.log'
    print('Running',key,'expected',layout['expected'],flush=True)
    code=subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),
        '--timeout-seconds','900','--',*command],cwd=R,env=env).returncode
    executions.append(dict(key=key,exitCode=code,log=str(log.relative_to(R)),binary=layout['binary'],
        trxDirectory=str(E/'trx'/slug),bindingsDirectory=str(E/'bindings'/slug)))
    result_path.write_text(json.dumps(executions,indent=2)+'\n',encoding='utf-8')
    if code:raise SystemExit(code)
print('Completed',args.lane,'lanes.',flush=True)
