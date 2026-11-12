"""Read-only verification of the pinned gate's fresh operational qualification."""
import csv
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import re
import zipfile

here=Path(__file__).resolve().parent
root=here.parents[1]
manifest=json.loads((here/'MANIFEST.json').read_text(encoding='utf-8'))
def check(data,expected):
    if len(data)!=expected['bytes'] or hashlib.sha256(data).hexdigest()!=expected['sha256']:
        raise ValueError('Evidence length/hash mismatch')
for name,expected in manifest['files'].items():
    check((here/name).read_bytes(),expected)
spec=importlib.util.spec_from_file_location('gate',root/'tools/xslt30_gate.py')
gate=importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)
policy,_,inputs,skips=gate.baseline()
with zipfile.ZipFile(here/'EVIDENCE.zip') as archive:
    if set(archive.namelist())!=set(manifest['archiveEntries']):
        raise ValueError('Archive entry set changed')
    for name,expected in manifest['archiveEntries'].items():
        check(archive.read(name),expected)
        if name.startswith('source/'):
            current=(root/name.removeprefix('source/')).read_bytes().replace(b'\r\n',b'\n')
            if current!=archive.read(name): raise ValueError('Gate code/policy differs from tested source')
    read=lambda name:json.loads(archive.read(name))
    run,result,plan=read('RUN.json'),read('GATE.json'),read('PLAN.json')
    if not result['success'] or result['counts']!=policy['counts'] or result['totalCases']!=14601:
        raise ValueError('Fresh gate result changed')
    if plan['sourceCommit']!=run['sourceCommit'] or result['sourceCommit']!=run['sourceCommit']:
        raise ValueError('Tested commit identities disagree')
    if any(result['corpusDifferences'].values()) or not result['sourceCorpusUnchanged'] or not result['runtimeUnchanged']:
        raise ValueError('Integrity audit failed')
    if read('CORPUS-BEFORE.json')!=inputs or read('CORPUS-AFTER.json')!=inputs:
        raise ValueError('Fixture inventory changed')
    for name in ['stdout.log','stderr.log']:
        if hashlib.sha256(archive.read(name)).hexdigest()!=run[name+'Sha256']:
            raise ValueError('Process transcript changed')
    rows=gate.reconcile(plan,run,archive.read('stdout.log').decode('utf-8-sig'),skips,policy)
    recorded=list(csv.DictReader(io.StringIO(archive.read('outcomes.csv').decode('utf-8'))))
    keys=['test_set','test','status','reason','recorded_reason','reason_source','catalog_file']
    if [{k:r[k] for k in keys} for r in rows]!=[{k:r[k] for k in keys} for r in recorded]:
        raise ValueError('Case ledger replay changed')
    if not read('HOST-AWAKE.json')['reservationReleased']:
        raise ValueError('Temporary idle reservation was not released')
    if not all(read(name)['conclusion']=='success' for name in ['PARENT-CI.json','PARENT-SCHEMA-CI.json']):
        raise ValueError('Parent hosted verification failed')
    control=read('CONTROL-RUN.json')
    if control['exitCode']!=0 or control['sourceCommit']!=run['sourceCommit'] or hashlib.sha256(archive.read('CONTROLS-RAW.log')).hexdigest()!=control['stdoutSha256']:
        raise ValueError('Gate refusal control receipt changed')
    if not re.search(r'Ran 14 tests.*?OK',archive.read('CONTROLS-RAW.log').decode('utf-8-sig'),re.S):
        raise ValueError('Gate refusal controls did not pass')
print('Verified fresh gate: 10,221 passed / 0 failed / 4,380 excluded; exact 14,601-case replay, fixtures, source and refusal controls.')
print('Operational CI proposal only; branch protection, publication and full conformance are not asserted.')
