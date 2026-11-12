"""Read-only verification of final selected-profile qualification and historical gates."""
import collections
import csv
import hashlib
import io
import json
from pathlib import Path
import re
import zipfile

here = Path(__file__).resolve().parent
manifest = json.loads((here / 'MANIFEST.json').read_text())
def check(content, expected):
    if len(content) != expected['bytes'] or hashlib.sha256(content).hexdigest() != expected['sha256']:
        raise ValueError('Length/hash mismatch')
for name, expected in manifest['files'].items():
    check((here / name).read_bytes(), expected)
with zipfile.ZipFile(here / 'EVIDENCE.zip') as archive:
    if set(archive.namelist()) != set(manifest['archiveEntries']):
        raise ValueError('Archive entries changed')
    for name, expected in manifest['archiveEntries'].items():
        check(archive.read(name), expected)
    read_json = lambda name: json.loads(archive.read(name).decode('utf-8-sig'))
    read_text = lambda name: archive.read(name).decode('utf-8-sig')
    plan, run, report, audit = [read_json(name) for name in ['PLAN.json', 'RUN.json', 'REPORT.json', 'AUDIT.json']]
    rows = list(csv.DictReader(io.StringIO(read_text('outcomes.csv'))))
    expected_names = {(s['name'], case) for s in plan['sets'] for case in s['cases']}
    names = {(r['test_set'], r['test']) for r in rows}
    counts = dict(collections.Counter(r['status'] for r in rows))
    if len(rows) != 14601 or len(names) != 14601 or names != expected_names or counts != report['counts']:
        raise ValueError('Pinned case identities/counts changed')
    if run['status'] != 'Completed' or not run['completeCatalogCounts'] or run['sourceCommit'] is not None:
        raise ValueError('Incomplete or incorrectly identified source attempt')
    if report['sourceBaseCommit'] != 'd86fc38e7ee83519e761afa16d960d7c74a4ad55':
        raise ValueError('Wrong parent review base')
    if counts != {'Passed': 10221, 'Skipped': 4380} or not run['success']:
        raise ValueError('Qualification result changed or falsely accepted')
    failures = {r['test'] for r in rows if r['status'] == 'Failed'}
    if failures != set():
        raise ValueError('Unexpected failure identities')
    for key in ['Passed', 'Failed', 'Skipped']:
        if counts.get(key, 0) != run['counts'][key]:
            raise ValueError('Run summary mismatch')
    for name in ['stdout.log', 'stderr.log']:
        if hashlib.sha256(archive.read(name)).hexdigest() != run[name + 'Sha256']:
            raise ValueError('Process transcript identity mismatch')
    if any(audit['disposableCorpusDifferences'].values()) or not audit['originalCorpusUnchanged'] or not audit['runtimeLayoutUnchanged']:
        raise ValueError('Fixture/runtime audit failed')
    if read_json('CORPUS-BEFORE.json') != read_json('CORPUS-AFTER.json'):
        raise ValueError('Corpus bytes changed')
    original_skips = {(r['test_set'], r['test']): r['reason'] for r in csv.DictReader(
        (here.parent / 'xslt30-qualification/skips.csv').open(encoding='utf-8', newline=''))}
    actual_skips = {(r['test_set'], r['test']): r['reason'] for r in rows if r['status'] == 'Skipped'}
    if original_skips != actual_skips:
        raise ValueError('Standalone skip identities/explanations changed')
    for name, expected in [('QT3.log', (31397, 0, 424)), ('QT4.log', (8941, 0, 1918))]:
        actual = tuple(int(re.search(r'^' + key + r':\s+(\d+)\s*$', read_text(name), re.M)[1])
                       for key in ['Passed', 'Failed', 'Skipped'])
        if actual != expected:
            raise ValueError('Frozen gate changed')
    if not all(v['identical'] for v in read_json('FROZEN-SKIP-CHECK.json').values()):
        raise ValueError('Frozen skip maps changed')
    def skip_map(text):
        return {name: reason for _, reason, body in re.findall(
            r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)', text, re.M | re.S)
            for name in body.splitlines() if name.strip()}
    for gate in ['qt3', 'qt4']:
        original = (here.parent / ('conformance/evidence/' + gate + '-skips.txt')).read_text(encoding='utf-8-sig')
        if skip_map(read_text(gate.upper() + '-SKIPS.txt')) != skip_map(original):
            raise ValueError('Frozen named exclusions differ from canonical evidence')
    totals = [int(n) for n in re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0', read_text('SOLUTION-TEST.log'))]
    if len(totals) != 10 or sum(totals) != 26359:
        raise ValueError('Solution test totals changed')
    if not re.search(r'Passed!\s+- Failed:\s+0, Passed:\s+90, Skipped:\s+0', read_text('LANGUAGE-SERVER-TEST.log')):
        raise ValueError('Language-server tests failed')
    if not re.search(r'Failed!\s+- Failed:\s+3, Passed:\s+2, Skipped:\s+0', read_text('diagnostics/BASELINE-TEST-2.log')):
        raise ValueError('Baseline regression proof missing')
    if not re.search(r'Passed!\s+- Failed:\s+0, Passed:\s+5, Skipped:\s+0', read_text('diagnostics/FOCUSED-TEST-4.log')):
        raise ValueError('Focused correction tests failed')
    for name, passed in [('OUTPUT.log',267), ('TRY.log',82)]:
        if not re.search(r'^Passed:\s+' + str(passed) + r'\s*$', read_text(name), re.M):
            raise ValueError('Targeted whole-set verification failed')
    controls = read_json('controls-nested/CONTROLS.json')
    if not controls['success'] or not controls['wrongResultsRefused'] or not controls['unchangedInputs']:
        raise ValueError('Output-isolation controls failed')
    baseline = read_json('baseline-controls-nested/CONTROLS.json')
    if baseline['actual']['relative-match'] or not controls['actual']['relative-match']:
        raise ValueError('Relative lookup regression proof missing')
    if 'Selected-case result: PASS (10221 executed; 4380 excluded)' not in read_text('stdout.log'):
        raise ValueError('Selected-profile summary missing')
    if re.search(r'^Pass rate:',read_text('stdout.log'),re.M):
        raise ValueError('Rounded percentage display returned')
    if not read_json('HOST-AWAKE.json')['reservationReleased']:
        raise ValueError('Temporary idle reservation not released')
    source_hashes = read_json('SOURCE-HASHES.json')
    with zipfile.ZipFile(io.BytesIO(archive.read('SOURCE.zip'))) as source:
        for name, expected in source_hashes.items():
            if hashlib.sha256(source.read(name)).hexdigest() != expected:
                raise ValueError('Source snapshot changed')
        identity = read_json('BASELINE-IDENTITY.json')
        name = 'tests/Bosak.Xslt.Tests/RawAttributeSequenceTests.cs'
        if identity['regressionSourceSha256'] != source_hashes[name]:
            raise ValueError('Baseline and candidate regression sources differ')
    if read_json('PARENT-CI.json')['conclusion'] != 'success':
        raise ValueError('Parent PR CI not successful')
    if read_json('PARENT-SCHEMA-CI.json')['conclusion'] != 'success':
        raise ValueError('Parent Schema CI not successful')
    rejected = read_json('rejected-first-cohort/RUN.json')
    if rejected['success'] or rejected['counts']['Failed'] != 1 or 'FAIL namespace-2602:' not in read_text('rejected-first-cohort/stdout.log'):
        raise ValueError('Rejected attempt was lost or falsely accepted')
    receipts = [read_json(name) for name in archive.namelist() if name.startswith('wrapper-receipts/') and name.endswith('.json')]
    if len(receipts) != 4 or sum(bool(r['success']) for r in receipts) != 1:
        raise ValueError('Wrapper refusal evidence changed')
print('Verified all 14,601 case identities, zero selected-case failures, unchanged fixture/skip maps, source/runtime seals and regression proofs.')
print('Standalone retained basic profile: 10,221 passed / 0 failed / 4,380 excluded; no full-conformance claim or gate promotion.')
