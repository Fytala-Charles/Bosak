"""Run the immutable basic XSLT 3.0 gate; retain every attempt and refuse drift."""
import argparse
import collections
import csv
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import shutil
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parents[1]
BASELINE = ROOT / 'docs/xslt30-gate/BASELINE.json'


def load_module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')


def baseline():
    policy = json.loads(BASELINE.read_text(encoding='utf-8'))
    evidence = ROOT / policy['evidence']
    if digest(evidence) != policy['evidenceSha256']:
        raise ValueError('Qualified baseline archive changed')
    with zipfile.ZipFile(evidence) as archive:
        plan = json.loads(archive.read('PLAN.json'))
        inputs = json.loads(archive.read('CORPUS-BEFORE.json'))
        skips = list(csv.DictReader(io.StringIO(archive.read('skips.csv').decode('utf-8'))))
    if plan['corpusCommit'] != policy['corpusCommit'] or len(skips) != policy['counts']['Skipped']:
        raise ValueError('Baseline identity/count mismatch')
    return policy, plan, inputs, skips


def inventory(directory):
    # Include ignored/untracked generated files as well; .git alone is metadata.
    result = {}
    for path in directory.rglob('*'):
        name = path.relative_to(directory)
        if '.git' in name.parts:
            continue
        if path.is_symlink():
            raise ValueError('Unexpected symbolic link: ' + str(path))
        if path.is_file():
            result[name.as_posix()] = dict(bytes=path.stat().st_size, sha256=digest(path))
    return result


def runtime_hashes(directory):
    return {p.relative_to(directory).as_posix(): digest(p) for p in directory.rglob('*') if p.is_file()}


def reconcile(plan, run, text, skips, policy):
    if not run.get('success') or run.get('status') != 'Completed' or not run.get('completeCatalogCounts'):
        raise ValueError('Failed, timed-out or incomplete runner attempt')
    reference = load_module('xslt30_reference', ROOT / 'docs/xslt30-qualification/report.py')
    rows = reference.parse(plan, text, True)
    counts = dict(collections.Counter(row['status'] for row in rows))
    actual = {key: counts.get(key, 0) for key in ['Passed', 'Failed', 'Skipped']}
    if actual != policy['counts'] or any(run['counts'].get(key) != value for key, value in actual.items()):
        raise ValueError('Exact gate counts changed')
    expected = {(s['test_set'], s['test']): s for s in skips}
    observed = {(r['test_set'], r['test']) for r in rows if r['status'] == 'Skipped'}
    if observed != set(expected):
        raise ValueError('Named exclusions changed; separate adoption required')
    for row in rows:
        if row['status'] != 'Skipped':
            row['recorded_reason'] = row['reason']
            continue
        original = expected[(row['test_set'], row['test'])]
        if row['reason_source'] in {'RunnerCaseLine', 'RunnerSetSkip'} and row['reason'] != original['recorded_reason']:
            raise ValueError('Printed exclusion reason changed: ' + row['test'])
        for key in ['reason', 'recorded_reason', 'reason_source']:
            row[key] = original[key]
    return rows


def prepare(packet, corpus, assembly):
    policy, plan, inputs, skips = baseline()
    if packet.exists():
        raise ValueError('Existing packet: use a fresh path, never overwrite receipts')
    if not assembly.is_file() or assembly.name != 'Bosak.Xslt.Conformance.dll':
        raise ValueError('Build the conformance runner first')
    revision = subprocess.check_output(['git', '-C', str(corpus), 'rev-parse', 'HEAD'], text=True).strip()
    if revision != policy['corpusCommit'] or inventory(corpus) != inputs:
        raise ValueError('Corpus revision or full LF byte inventory differs from qualified baseline')
    status = subprocess.check_output(['git', '-C', str(ROOT), 'status', '--porcelain', '--untracked-files=all'], text=True)
    if status.strip():
        raise ValueError('Source checkout must be clean; commit the review candidate first')
    source = subprocess.check_output(['git', '-C', str(ROOT), 'rev-parse', 'HEAD'], text=True).strip()
    packet.mkdir(parents=True)
    # Own a disposable copy; never execute against caller-supplied fixtures.
    shutil.copytree(corpus, packet / 'corpus')
    shutil.copytree(assembly.parent, packet / 'runtime')
    plan.update(sourceCommit=source, sourceBaseCommit=source,
                sourceIdentity='Clean committed checkout; current gate source identity',
                wallTimeoutSeconds=policy['wallTimeoutSeconds'], memoryLimitBytes=policy['memoryLimitBytes'])
    for name, value in [('PLAN.json',plan), ('CORPUS-BEFORE.json',inputs),
                        ('RUNTIME-HASHES.json',runtime_hashes(packet/'runtime')),
                        ('GATE-INPUT.json',dict(policy=policy,sourceCommit=source,
                          sourceCorpus=str(corpus),sourceCorpusInventory=inputs))]:
        write_json(packet/name,value)
    return policy, plan, inputs, skips


def finish(packet, policy, plan, inputs, skips, source_corpus):
    receipt = dict(success=False, scope='Pinned basic/non-schema-aware Legacy/Xslt30; exclusions are not passes')
    try:
        after = inventory(packet/'corpus')
        write_json(packet/'CORPUS-AFTER.json',after)
        changes = {kind: sorted(names) for kind,names in [
            ('added',after.keys()-inputs.keys()), ('removed',inputs.keys()-after.keys()),
            ('changed',{n for n in inputs.keys() & after.keys() if inputs[n]!=after[n]})]}
        with zipfile.ZipFile(packet/'GENERATED-OUTPUTS.zip','w',compression=zipfile.ZIP_DEFLATED) as archive:
            for name in changes['added']+changes['changed']:
                archive.write(packet/'corpus'/name,name)
        receipt.update(corpusDifferences=changes, sourceCorpusUnchanged=inventory(source_corpus)==inputs,
            runtimeUnchanged=runtime_hashes(packet/'runtime')==json.loads((packet/'RUNTIME-HASHES.json').read_text()))
        if any(changes.values()) or not receipt['sourceCorpusUnchanged'] or not receipt['runtimeUnchanged']:
            raise ValueError('Fixture or runtime integrity changed')
        run = json.loads((packet/'RUN.json').read_text(encoding='utf-8'))
        text = (packet/'stdout.log').read_text(encoding='utf-8-sig')
        for name in ['stdout.log','stderr.log']:
            if digest(packet/name) != run[name+'Sha256']:
                raise ValueError('Process transcript hash mismatch')
        rows = reconcile(plan,run,text,skips,policy)
        with (packet/'outcomes.csv').open('w',encoding='utf-8',newline='') as stream:
            writer=csv.DictWriter(stream,fieldnames=['test_set','test','status','reason','recorded_reason','reason_source','evidence_line','catalog_file'],lineterminator='\n')
            writer.writeheader()
            writer.writerows(rows)
        receipt.update(success=True,counts=policy['counts'],totalCases=len(rows),sourceCommit=plan['sourceCommit'])
    except Exception as error:
        receipt['error']=repr(error)
    write_json(packet/'GATE.json',receipt)
    return receipt


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--corpus',type=Path,required=True)
    parser.add_argument('--output',type=Path,required=True)
    parser.add_argument('--assembly',type=Path,default=ROOT/'tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll')
    args=parser.parse_args()
    if sys.platform != 'win32':
        parser.error('This gate requires Windows Job memory and process-error-mode controls')
    packet=args.output.resolve()
    policy,plan,inputs,skips=prepare(packet,args.corpus.resolve(),args.assembly.resolve())
    try:
        subprocess.run([sys.executable,str(ROOT/'docs/xslt30-qualification/run.py'),'--packet',str(packet)],check=False)
    finally:
        receipt=finish(packet,policy,plan,inputs,skips,args.corpus.resolve())
        print(json.dumps(receipt,indent=2),flush=True)
    return 0 if receipt['success'] else 1


if __name__=='__main__':
    try:
        raise SystemExit(main())
    except Exception as error:
        print('Gate refused: '+repr(error),file=sys.stderr)
        raise SystemExit(1)
