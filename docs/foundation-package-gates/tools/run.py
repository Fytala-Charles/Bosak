"""Execute staged package-backed unit or frozen-gate runners without rebuilding binaries."""
import argparse
import concurrent.futures
import json
import os
from pathlib import Path
import subprocess
import time

HERE=Path(__file__).resolve().parents[1]
ROOT=HERE.parents[1]
WORK=ROOT/'.guard-tmp/foundation-package-gates-2026-10-30'


def main():
    global WORK
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--workspace',default=str(WORK))
    parser.add_argument('--unit')
    parser.add_argument('--all-units',action='store_true')
    parser.add_argument('--remaining',action='store_true',help='Retain successful executions already recorded in this cohort')
    parser.add_argument('--gate',choices=['qt3','qt4'])
    args=parser.parse_args();WORK=Path(args.workspace).resolve();assert WORK.is_relative_to((ROOT/'.guard-tmp').resolve()); manifest=json.loads((WORK/'runners.json').read_bytes())
    selected=[r for r in manifest['runners'] if (args.all_units and r['kind']=='unit') or r['name']==args.unit or (args.gate and r['name']==('Bosak.XPath.Conformance' if args.gate=='qt3' else 'Bosak.XPath.Conformance40'))]
    assert selected,'Select a unit project, all units, or a gate'
    if args.remaining:
        selected=[r for r in selected if not (WORK/(r['name']+('-'+r['attemptId'] if 'attemptId' in r else '')+'-execution.json')).exists()]
    else:
        assert not any((WORK/(r['name']+('-'+r['attemptId'] if 'attemptId' in r else '')+'-execution.json')).exists() for r in selected),'Use a fresh cohort or --remaining; executions are not overwritten'
    assert selected,'No pending runners selected'
    def run(runner):
        name=runner['name']; key=name+('-'+runner['attemptId'] if 'attemptId' in runner else ''); entry=Path(runner['directory'])/runner['entryAssembly']; env=os.environ.copy()
        env['DOTNET_STARTUP_HOOKS']=runner.get('hookPath',manifest['hookPath']);env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=runner['bindingDirectory'];env['DOTNET_CLI_TELEMETRY_OPTOUT']='1'
        if runner['kind']=='unit':
            results=WORK/'unit-results'/key;results.mkdir(parents=True,exist_ok=True)
            argv=['dotnet','vstest',str(entry),'/Logger:trx;LogFileName=results.trx','/ResultsDirectory:'+str(results)]
        else:
            gate='qt3' if name=='Bosak.XPath.Conformance' else 'qt4'
            env['BOSAK_'+gate.upper()+'_DUMP_SKIPS']=str(WORK/(gate+'-skips.txt'))
            if gate=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
            argv=['dotnet',str(entry),str(ROOT/'tests'/('qt3tests' if gate=='qt3' else 'qt4tests'))]
        started=time.monotonic()
        with (WORK/(key+'.txt')).open('wb') as output: completed=subprocess.run(argv,cwd=ROOT,env=env,stdout=output,stderr=subprocess.STDOUT)
        result={'runner':name,'argv':argv,'exitCode':completed.returncode,'elapsedSeconds':round(time.monotonic()-started,2),'hookSha256':runner.get('hookSha256',manifest['hookSha256'])}
        (WORK/(key+'-execution.json')).write_bytes((json.dumps(result,indent=2)+'\n').encode());print(name,completed.returncode,flush=True)
        return completed.returncode
    if args.all_units:
        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool: results=list(pool.map(run,selected))
    else:results=[run(r) for r in selected]
    if any(results):raise SystemExit(1)


if __name__=='__main__':main()
