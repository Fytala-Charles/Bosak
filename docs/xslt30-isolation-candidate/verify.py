"""Read-only verification of the sealed local output-isolation qualification evidence."""
import collections
import csv
import hashlib
import io
import json
from pathlib import Path
import re
import zipfile


here = Path(__file__).resolve().parent
manifest = json.loads((here / 'MANIFEST.json').read_text(encoding='utf-8'))
def check(content, expected):
    if len(content) != expected['bytes'] or hashlib.sha256(content).hexdigest() != expected['sha256']:
        raise ValueError('Length/hash mismatch')
for name, expected in manifest['files'].items():
    check((here / name).read_bytes(), expected)
with zipfile.ZipFile(here / 'EVIDENCE.zip') as archive:
    if set(archive.namelist()) != set(manifest['archiveEntries']):
        raise ValueError('Archive entry mismatch')
    for name, expected in manifest['archiveEntries'].items():
        check(archive.read(name), expected)
    read_json = lambda name: json.loads(archive.read(name))
    result = json.loads((here / 'RESULTS.json').read_text(encoding='utf-8'))
    run, report, audit = [read_json(name) for name in ['RUN.json', 'REPORT.json', 'AUDIT.json']]
    rows = list(csv.DictReader(io.StringIO(archive.read('outcomes.csv').decode('utf-8'))))
    identities = {(r['test_set'], r['test']) for r in rows}
    plan = read_json('PLAN.json')
    planned = {(s['name'], case) for s in plan['sets'] for case in s['cases']}
    if identities != planned:
        raise ValueError('Case identities differ from the pinned catalog plan')
    counts = dict(collections.Counter(r['status'] for r in rows))
    if len(rows) != 14601 or len(identities) != 14601 or counts != report['counts'] or counts != result['counts']:
        raise ValueError('Case reconciliation failed')
    if run['sourceCommit'] is not None or run['status'] != 'Completed' or not run['completeCatalogCounts']:
        raise ValueError('Candidate identity/completeness mismatch')
    reuse = read_json('VERIFICATION-REUSE.json')
    if not reuse['sourceHashesIdentical'] or not reuse['runtimeLayoutHashesIdentical']:
        raise ValueError('Verification evidence reused over different candidate bytes')
    if read_json('host-paused-attempt/SOURCE-HASHES.json') != read_json('SOURCE-HASHES.json'):
        raise ValueError('Source identity changed between interrupted and complete attempts')
    if read_json('host-paused-attempt/RUNTIME-HASHES.json') != read_json('RUNTIME-HASHES.json'):
        raise ValueError('Runtime identity changed between interrupted and complete attempts')
    if not read_json('HOST-AWAKE.json')['reservationReleased']:
        raise ValueError('Temporary host reservation was not released')
    for name in ['Passed', 'Failed', 'Skipped']:
        if counts.get(name, 0) != run['counts'][name]:
            raise ValueError('Run count mismatch')
    if run['success'] != (counts.get('Failed', 0) == 0):
        raise ValueError('False qualification-success claim')
    for name in ['stdout.log', 'stderr.log']:
        if hashlib.sha256(archive.read(name)).hexdigest() != run[name + 'Sha256']:
            raise ValueError('Run transcript identity mismatch')
    original_skips = {(r['test_set'], r['test']): r['reason'] for r in csv.DictReader(
        (here.parent / 'xslt30-qualification/skips.csv').open(encoding='utf-8', newline=''))}
    current_skips = {(r['test_set'], r['test']): r['reason'] for r in rows if r['status'] == 'Skipped'}
    if current_skips != original_skips:
        raise ValueError('Skip identities or explanations changed')
    if any(audit['disposableCorpusDifferences'].values()) or not audit['originalCorpusUnchanged'] or not audit['runtimeLayoutUnchanged']:
        raise ValueError('Input/runtime isolation failed')
    if read_json('CORPUS-BEFORE.json') != read_json('CORPUS-AFTER.json'):
        raise ValueError('Corpus bytes changed')
    controls = read_json('controls/CONTROLS.json')
    if not controls['success'] or not controls['unchangedInputs'] or controls['actual'] != controls['expected']:
        raise ValueError('Isolation controls failed')
    if sum(not passed for passed in controls['expected'].values()) != 3:
        raise ValueError('Missing negative assertion controls')
    strict = read_json('controls-strict/CONTROLS.json')
    if not strict['success'] or not strict['wrongResultsRefused']:
        raise ValueError('Negative controls did not produce explicit assertion mismatches')
    baseline = read_json('baseline-controls/CONTROLS.json')
    if not baseline['actual']['mismatch'] or not baseline['actual']['missing'] or baseline['unchangedInputs']:
        raise ValueError('Missing original-runner false-acceptance regression proof')
    sources = read_json('SOURCE-HASHES.json')
    with zipfile.ZipFile(io.BytesIO(archive.read('SOURCE.zip'))) as source_zip:
        for name, digest in sources.items():
            if hashlib.sha256(source_zip.read(name)).hexdigest() != digest:
                raise ValueError('Source snapshot mismatch')
    for name, expected in [('QT3.log', (31397, 0, 424)), ('QT4.log', (8941, 0, 1918))]:
        text = archive.read(name).decode('utf-8-sig')
        actual = tuple(int(re.search(r'^' + label + r':\s+(\d+)\s*$', text, re.M)[1]) for label in ['Passed', 'Failed', 'Skipped'])
        if actual != expected:
            raise ValueError('Frozen gate changed: ' + name)
    tests = archive.read('verification/SOLUTION-TEST.log').decode('utf-8-sig')
    totals = [int(n) for n in re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0', tests)]
    if len(totals) != 10 or sum(totals) != 26349:
        raise ValueError('Solution implementation-test totals changed')
    server = archive.read('LANGUAGE-SERVER-TEST.log').decode('utf-8-sig')
    if not re.search(r'Passed!\s+- Failed:\s+0, Passed:\s+90, Skipped:\s+0', server):
        raise ValueError('Separate language-server tests failed')
print('Verified 14,601 case identities, unchanged fixture bytes, source/runtime seals, negative controls and frozen gates.')
print('Standalone counts:', counts, '; qualification successful:', run['success'])
