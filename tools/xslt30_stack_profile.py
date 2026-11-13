"""Bound one sampled-thread-stack diagnostic of a sealed copied caller; never qualify a gate."""
import argparse
import collections
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DOSSIER = ROOT / 'docs/xslt30-stack-profile'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, value):
    temporary = path.with_suffix(path.suffix+'.tmp')
    temporary.write_bytes((json.dumps(value,indent=2)+'\n').encode());temporary.replace(path)


def module(name, path):
    spec=importlib.util.spec_from_file_location(name,path);result=importlib.util.module_from_spec(spec);spec.loader.exec_module(result);return result


def inventory(folder):
    return {f.relative_to(folder).as_posix():sha(f) for f in sorted(folder.rglob('*')) if f.is_file()}


def baseline():
    policy=json.loads((DOSSIER/'BASELINE.json').read_text())
    if sha(DOSSIER/'REFERENCE.zip')!=policy['referenceSha256']:
        raise ValueError('Sealed copied-caller reference changed')
    return policy


def profiler_files(path):
    expected=json.loads((DOSSIER/'PROFILER-FILES.json').read_text())
    if path.name!='dotnet-trace.dll' or any(not (path.parent/name).is_file() or sha(path.parent/name)!=value for name,value in expected.items()):
        raise ValueError('Profiler package closure differs from reviewed version')
    return expected


def native_files(pin):
    for name,value in pin['nativeFiles'].items():
        if sha(Path(value['path']))!=value['sha256']:
            raise ValueError('Native module pin drift: '+name)
    current=inventory(Path(pin['frameworkDirectory']))
    if current!=pin['frameworkFiles']:
        raise ValueError('Target framework inventory drift')


def prepare(retained, corpus, profiler, output):
    if output.exists():raise ValueError('Existing output; preserve original attempts')
    if output.is_relative_to(retained) or output.is_relative_to(corpus):raise ValueError('Output overlaps retained caller/corpus')
    policy=baseline();tool=profiler_files(profiler)
    gate=module('gate',ROOT/'tools/xslt30_gate.py');_,_,inputs,_=gate.baseline()
    if gate.inventory(corpus)!=inputs:raise ValueError('Corpus differs from pinned complete inventory')
    with zipfile.ZipFile(DOSSIER/'REFERENCE.zip') as z:
        hashes=json.loads(z.read('diagnostic/RUNTIME-HASHES.json'))
        pin=json.loads(z.read('diagnostic/CASE-COST-PIN.json'))
        identity=json.loads(z.read('diagnostic/IDENTITY.json'))
    if inventory(retained/'runtime')!=hashes:raise ValueError('Retained copied caller changed')
    native_files(pin)
    output.mkdir(parents=True)
    shutil.copytree(retained/'runtime',output/'runtime');shutil.copytree(corpus,output/'corpus')
    write(output/'CASE-COST-PIN.json',pin);write(output/'RUNTIME-BEFORE.json',hashes);write(output/'CORPUS-BEFORE.json',inputs)
    collector=Path(pin['nativeFiles']['dotnet.exe']['path']).parent/'shared/Microsoft.NETCore.App/8.0.28'
    if not collector.is_dir():raise ValueError('Reviewed collector framework 8.0.28 is required')
    write(output/'COLLECTOR-FRAMEWORK.json',dict(path=str(collector),files=inventory(collector),frameworkVersion='8.0.28',observedLoaded=False))
    write(output/'PLAN.json',dict(policy=policy,profiler=str(profiler),profilerFiles=tool,engineHashes=identity['engineHashes'],
        sourceCommit=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
        diagnosticOnly=True,qualification=False,toolSourceSha256=sha(Path(__file__))))


def validate(packet):
    plan=json.loads((packet/'PLAN.json').read_text());policy=baseline()
    if plan['policy']!=policy:raise ValueError('Diagnostic selection/bounds changed')
    if policy['wallTimeoutSeconds']!=180 or policy['memoryLimitBytes']!=8589934592 or policy['targetSet']['name']!='sx-GeneralComp-gt':
        raise ValueError('Unexpected diagnostic scope')
    profiler_files(Path(plan['profiler']))
    pin=json.loads((packet/'CASE-COST-PIN.json').read_text());native_files(pin)
    collector=json.loads((packet/'COLLECTOR-FRAMEWORK.json').read_text())
    if inventory(Path(collector['path']))!=collector['files']:raise ValueError('Collector framework changed')
    if inventory(packet/'runtime')!=json.loads((packet/'RUNTIME-BEFORE.json').read_text()):raise ValueError('Application changed')
    return plan,pin


def bounded(command, cwd, prefix, timeout, trace=None):
    if prefix.with_suffix('.json').exists():raise ValueError('Existing launch receipt')
    control=module('bounded_control',ROOT/'docs/xslt30-qualification/run.py')
    job=control.BoundedJob(8589934592);process=None;start=time.monotonic()
    receipt=dict(status='Prepared',success=False,command=command,timeoutSeconds=timeout,memoryLimitBytes=8589934592,
        diagnosticOnly=True,qualification=False,memoryScope='Aggregate collector and owned target process tree')
    write(prefix.with_suffix('.json'),receipt)
    try:
        with prefix.with_suffix('.stdout.log').open('xb') as out,prefix.with_suffix('.stderr.log').open('xb') as err:
            with control.process_error_mode():
                process=subprocess.Popen(command,cwd=cwd,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
            job.assign(process);receipt.update(status='Running',processId=process.pid);write(prefix.with_suffix('.json'),receipt)
            while process.poll() is None:
                if time.monotonic()-start>=timeout:
                    receipt['status']='TimedOut';job.terminate();process.wait(timeout=15);break
                if trace and trace.exists() and trace.stat().st_size>268435456:
                    receipt['status']='TraceSizeExceeded';job.terminate();process.wait(timeout=15);break
                time.sleep(0.1)
            receipt['exitCode']=process.returncode
            if receipt['status']=='Running':receipt['status']='Completed'
            receipt['success']=receipt['status']=='Completed' and process.returncode==0
    except Exception as error:receipt.update(status='ControllerFailure',error=repr(error))
    finally:
        job.close()
        if process and process.poll() is None:process.wait(timeout=15)
        receipt['elapsedSeconds']=time.monotonic()-start
        write(prefix.with_suffix('.json'),receipt)
    return receipt


def commands(packet, plan, pin):
    host=pin['nativeFiles']['dotnet.exe']['path']
    tool=[host,'exec','--fx-version','8.0.28','--roll-forward','Disable',plan['profiler']]
    target=[host,'exec','--fx-version','10.0.11','--roll-forward','Disable',str(packet/'runtime/Bosak.Xslt.Conformance.dll'),
        '--processor-profile','legacy',str(packet/'corpus/catalog.xml'),'sx-GeneralComp-gt']
    return tool,tool+['collect','--profile','dotnet-common,dotnet-sampled-thread-time','--buffersize','64',
        '--output',str(packet/'raw.nettrace'),'--format','NetTrace','--show-child-io','--',*target]


def validate_cases(rows, expected):
    seen=set()
    for index,row in enumerate(rows,1):
        name=row['test']
        if row['testSet']!='sx-GeneralComp-gt' or name not in expected or name in seen or row['ordinal']!=index:
            raise ValueError('Unexpected/duplicate/reordered selected case')
        if row['result']!=expected[name]:raise ValueError('Original selected outcome changed')
        for key in ['wallSeconds','cpuSeconds']:
            if row[key] is None or not math.isfinite(row[key]) or row[key]<0:raise ValueError('Invalid case counter')
        seen.add(name)
    if seen!=set(expected):raise ValueError('Incomplete selected case inventory')


def stack_report(data):
    frames=data['shared']['frames'];exclusive=collections.Counter();inclusive=collections.Counter();total=0.;case_total=0.;case_exclusive=collections.Counter();case_inclusive=collections.Counter()
    def interval(stack,weight):
        nonlocal total,case_total
        if not math.isfinite(weight) or weight<0:raise ValueError('Invalid sampled duration')
        if any(type(i)!=int or i<0 or i>=len(frames) for i in stack):raise ValueError('Invalid stack frame')
        if not stack:return
        total+=weight;exclusive[stack[-1]]+=weight
        for i in set(stack):inclusive[i]+=weight
        if any('RunTestCase' in frames[i]['name'] for i in stack):
            case_total+=weight;case_exclusive[stack[-1]]+=weight
            for i in set(stack):case_inclusive[i]+=weight
    units=set()
    for profile in data['profiles']:
        units.add(profile['unit'])
        if profile['type']=='sampled':
            if len(profile['samples'])!=len(profile['weights']):raise ValueError('Sample/weight mismatch')
            for stack,weight in zip(profile['samples'],profile['weights']):interval(stack,weight)
        elif profile['type']=='evented':
            stack=[];previous=profile['startValue']
            for event in profile['events']:
                if event['at']<previous:raise ValueError('Event time reversal')
                interval(stack,event['at']-previous);previous=event['at']
                if event['type']=='O':stack.append(event['frame'])
                elif event['type']=='C':
                    if not stack or stack.pop()!=event['frame']:raise ValueError('Unbalanced event stack')
                else:raise ValueError('Unknown event type')
            if stack:raise ValueError('Unclosed event stack')
        else:raise ValueError('Unknown stack profile type')
    if len(units)!=1 or not units.issubset({'seconds','milliseconds','microseconds','nanoseconds'}) or not total or not case_total:
        raise ValueError('Missing/mixed-unit resolved case-stack samples')
    def top(counter,denominator):
        return [dict(frame=frames[i]['name'],weight=value,share=value/denominator) for i,value in counter.most_common(30)]
    return dict(unit=next(iter(units)),profiles=len(data['profiles']),frames=len(frames),sampledThreadWeight=total,
        caseStackWeight=case_total,allExclusive=top(exclusive,total),caseExclusive=top(case_exclusive,case_total),
        caseInclusive=top(case_inclusive,case_total),interpretation='Sampled thread wall-time weights, not on-CPU percentages. Case stacks require a resolved RunTestCase ancestor. Inclusive methods deduplicate recursion and overlap; do not sum their shares.')


def run(packet):
    if (packet/'COLLECT.json').exists():raise ValueError('Existing original attempt; no automatic retry')
    plan,pin=validate(packet);tool,command=commands(packet,plan,pin)
    work=packet/'work';work.mkdir()
    receipt=bounded(command,work,packet/'COLLECT',180,packet/'raw.nettrace')
    gate=module('gate_after',ROOT/'tools/xslt30_gate.py')
    write(packet/'CORPUS-AFTER.json',gate.inventory(packet/'corpus'));write(packet/'RUNTIME-AFTER.json',inventory(packet/'runtime'))
    if json.loads((packet/'CORPUS-BEFORE.json').read_text())!=json.loads((packet/'CORPUS-AFTER.json').read_text()):raise ValueError('Corpus changed')
    validate(packet)
    if not receipt['success']:raise ValueError('Original profiler attempt failed; retained without retry')
    rows=[json.loads(line) for line in (work/'case-costs.jsonl').read_text().splitlines()]
    validate_cases(rows,plan['policy']['expectedResults'])
    text=(packet/'COLLECT.stdout.log').read_text(encoding='utf-8-sig')
    text=re.sub(r'\x1b\[[0-9;]*[A-Za-z]','',text)
    counts={key:int(value) for key,value in re.findall(r'^(Total|Passed|Failed|Skipped):\s*(\d+)\s*$',text,re.M)}
    expected=collections.Counter(plan['policy']['expectedResults'].values())
    if counts!=dict(Total=len(rows),Passed=expected['Pass'],Failed=0,Skipped=expected['Skip']):raise ValueError('Printed selected counts differ')
    end=json.loads((work/'case-cost-end.json').read_text());native=json.loads((work/'case-cost-runtime.json').read_text())
    if end['rows']!=len(rows) or end['errors'] or native['frameworkVersion']!='10.0.11':raise ValueError('Incomplete caller capture')
    for name,value in pin['nativeFiles'].items():
        if native['modules'][name]['sha256']!=value['sha256']:raise ValueError('Loaded target module changed')
    if not bounded(tool+['convert',str(packet/'raw.nettrace'),'--format','Speedscope','--output',str(packet/'stack')],work,packet/'CONVERT',180)['success']:raise ValueError('Trace conversion failed')
    for label,args in [('TOP-EXCLUSIVE',[]),('TOP-INCLUSIVE',['--inclusive'])]:
        if not bounded(tool+['report',str(packet/'raw.nettrace'),'topN','--number','30','--verbose',*args],work,packet/label,180)['success']:raise ValueError('Trace report failed')
    speedscope=list(packet.glob('stack*.speedscope.json'))
    if len(speedscope)!=1:raise ValueError('Expected one converted trace')
    result=stack_report(json.loads(speedscope[0].read_text(encoding='utf-8-sig')))
    result.update(diagnosticOnly=True,qualification=False,selectedCases=len(rows),outsideSelection=14601-len(rows),
        selectedOutcomes=dict(collections.Counter(r['result'] for r in rows)),caseWallSeconds=sum(r['wallSeconds'] for r in rows),
        caseCpuSeconds=sum(r['cpuSeconds'] for r in rows),elapsedSeconds=receipt['elapsedSeconds'],traceSha256=sha(packet/'raw.nettrace'))
    validate(packet)
    write(packet/'TARGET-FRAMEWORK-AFTER.json',inventory(Path(pin['frameworkDirectory'])))
    collector=json.loads((packet/'COLLECTOR-FRAMEWORK.json').read_text())
    write(packet/'COLLECTOR-FRAMEWORK-AFTER.json',inventory(Path(collector['path'])))
    write(packet/'PROFILER-AFTER.json',{name:sha(Path(plan['profiler']).parent/name) for name in plan['profilerFiles']})
    write(packet/'REPORT.json',result);print(json.dumps({k:v for k,v in result.items() if not isinstance(v,list)},indent=2))


def main():
    parser=argparse.ArgumentParser(description=__doc__);sub=parser.add_subparsers(dest='mode',required=True)
    p=sub.add_parser('prepare')
    for name in ['retained','corpus','profiler','output']:p.add_argument('--'+name,type=Path,required=True)
    p=sub.add_parser('run');p.add_argument('--packet',type=Path,required=True)
    args=parser.parse_args()
    if args.mode=='prepare':prepare(args.retained.resolve(),args.corpus.resolve(),args.profiler.resolve(),args.output.resolve())
    else:run(args.packet.resolve())


if __name__=='__main__':
    try:main()
    except Exception as error:print('Stack diagnostic refused: '+repr(error),file=sys.stderr);raise SystemExit(2)
