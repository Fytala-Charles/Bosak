"""Reconcile candidate outcomes without attributing uncommitted code to its base commit."""
import collections
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import zipfile


root = Path(__file__).resolve().parents[2]
packet = Path(sys.argv[1]).resolve()
spec = importlib.util.spec_from_file_location('reference', root / 'docs/xslt30-qualification/report.py')
reference = importlib.util.module_from_spec(spec)
spec.loader.exec_module(reference)
plan = json.loads((packet / 'PLAN.json').read_text(encoding='utf-8'))
run = json.loads((packet / 'RUN.json').read_text(encoding='utf-8'))
if run['status'] != 'Completed' or not run['completeCatalogCounts']:
    raise ValueError('Incomplete candidate attempt cannot qualify')
rows = reference.parse(plan, (packet / 'stdout.log').read_text(encoding='utf-8-sig'), True)
old = {(r['test_set'], r['test']): r for r in csv.DictReader((root / 'docs/xslt30-qualification/skips.csv').open(encoding='utf-8', newline=''))}
new_skip_names = {(r['test_set'], r['test']) for r in rows if r['status'] == 'Skipped'}
if new_skip_names != set(old):
    raise ValueError('Skip identities changed; separately review rather than inherit explanations')
for row in rows:
    if row['status'] == 'Skipped':
        original = old[(row['test_set'], row['test'])]
        if row['reason_source'] in {'RunnerCaseLine', 'RunnerSetSkip'} and row['reason'] != original['recorded_reason']:
            raise ValueError('Printed skip reason changed: ' + row['test'])
        for key in ['reason', 'recorded_reason', 'reason_source']:
            row[key] = original[key]
    else:
        row['recorded_reason'] = row['reason']
for item in plan['sets']:
    if hashlib.sha256((packet / 'corpus' / item['file']).read_bytes()).hexdigest() != item['sha256']:
        raise ValueError('Pinned test-set bytes changed: ' + item['file'])
hashes = json.loads((packet / 'SOURCE-HASHES.json').read_text(encoding='utf-8'))
with zipfile.ZipFile(packet / 'SOURCE.zip') as archive:
    for name, digest in hashes.items():
        if hashlib.sha256(archive.read(name)).hexdigest() != digest:
            raise ValueError('Source snapshot hash mismatch: ' + name)
        if hashlib.sha256((root / name).read_bytes()).hexdigest() != digest:
            raise ValueError('Candidate source changed since execution: ' + name)
fields = ['test_set', 'test', 'status', 'reason', 'recorded_reason', 'reason_source', 'evidence_line', 'catalog_file']
for name, selected in [('outcomes.csv', rows), ('failures.csv', [r for r in rows if r['status'] == 'Failed']),
                       ('skips.csv', [r for r in rows if r['status'] == 'Skipped'])]:
    with (packet / name).open('w', encoding='utf-8', newline='') as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, lineterminator='\n')
        writer.writeheader()
        writer.writerows(selected)
counts = dict(collections.Counter(r['status'] for r in rows))
for name in ['Passed', 'Failed', 'Skipped']:
    if counts.get(name, 0) != run['counts'][name]:
        raise ValueError('Aggregate mismatch: ' + name)
result = dict(sourceCommit=None, sourceBaseCommit=plan['sourceBaseCommit'], sourceHashes=hashes,
              corpusCommit=plan['corpusCommit'], totalCases=len(rows), counts=counts,
              exactTestSetHashes=True, unchangedSkipNamesAndReasons=True,
              failureSets=dict(collections.Counter(r['test_set'] for r in rows if r['status'] == 'Failed')),
              qualificationSuccessful=counts.get('Failed', 0) == 0)
(packet / 'REPORT.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(json.dumps(result, indent=2))
