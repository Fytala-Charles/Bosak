"""Verify retained qualification consistency without running .NET or modifying evidence."""
import collections
import csv
import hashlib
import io
import json
from pathlib import Path
import re
import zipfile

ROOT = Path(__file__).resolve().parent


def verify():
    manifest = json.loads((ROOT / 'MANIFEST.json').read_text(encoding='utf-8'))
    for name, expected in manifest.items():
        path = (ROOT / name).resolve()
        if not path.is_relative_to(ROOT):
            raise ValueError('Manifest path outside dossier')
        data = path.read_bytes()
        if len(data) != expected['bytes'] or hashlib.sha256(data).hexdigest() != expected['sha256']:
            raise ValueError('Artifact changed: ' + name)
    result = json.loads((ROOT / 'RESULTS.json').read_text(encoding='utf-8'))
    with zipfile.ZipFile(ROOT / 'EVIDENCE.zip') as archive:
        load = lambda name: json.loads(archive.read(name).decode('utf-8'))
        plan, run, report, audit = [load(name) for name in ['PLAN.json', 'RUN.json', 'REPORT.json', 'AUDIT.json']]
        if archive.read('PLAN.json') != (ROOT / 'PLAN.json').read_bytes():
            raise ValueError('Plan copy mismatch')
        for key in ['sourceCommit', 'corpusCommit']:
            if len({item[key] for item in [result, plan, run, report]}) != 1:
                raise ValueError('Input identity mismatch: ' + key)
        if run['status'] != 'Completed' or not run['completeCatalogCounts'] or run['runnerExitCode'] != 0 or run['success']:
            raise ValueError('Legacy-exit/failing-qualification distinction lost')
        expected = dict(Passed=10199, Failed=22, Skipped=4380)
        text = archive.read('stdout.log').decode('utf-8-sig')
        for key, count in expected.items():
            value = int(re.search(r'^' + key + r':\s*(\d+)\s*$', text, re.M)[1])
            if value != count or run['counts'][key] != count or result['counts'][key.lower()] != count:
                raise ValueError('Aggregate mismatch: ' + key)
        if run['counts']['Unselected'] or run['counts']['Missing test sets']:
            raise ValueError('Incomplete catalog selection')
        for name in ['stdout.log', 'stderr.log']:
            if hashlib.sha256(archive.read(name)).hexdigest() != run[name + 'Sha256']:
                raise ValueError('Process output hash mismatch: ' + name)
        rows = list(csv.DictReader(io.StringIO(archive.read('outcomes.csv').decode('utf-8'))))
        identities = [(r['test_set'], r['test']) for r in rows]
        catalog = [(s['name'], case) for s in plan['sets'] for case in s['cases']]
        if len(rows) != 14601 or len(set(identities)) != len(rows) or set(identities) != set(catalog):
            raise ValueError('Lost, added or duplicate catalog outcome')
        if dict(collections.Counter(r['status'] for r in rows)) != expected:
            raise ValueError('Case ledger does not reconcile with summary')
        for name, status in [('failures.csv', 'Failed'), ('skips.csv', 'Skipped')]:
            data = archive.read(name)
            if data != (ROOT / name).read_bytes():
                raise ValueError('Derived inventory copy mismatch: ' + name)
            selected = list(csv.DictReader(io.StringIO(data.decode('utf-8'))))
            if selected != [r for r in rows if r['status'] == status] or any(not r['reason'] for r in selected):
                raise ValueError('Incomplete named inventory/reasons: ' + name)
        if report['unresolvedSkipReasons']:
            raise ValueError('Unresolved skip reasons')
        before, after = load('CORPUS-BEFORE.json'), load('CORPUS-AFTER.json')
        alias = json.loads((ROOT / 'WRITE-ALIAS-REVIEW.json').read_text(encoding='utf-8'))
        name = alias['actualAndExpectedResolveToSameTrackedRelativePath']
        case_row = next(r for r in rows if r['test_set'] == 'xslt-compat' and r['test'] == alias['case'])
        if (alias['secondaryUri'] != alias['fileAssertion'] or case_row['status'] != alias['observedStatus']
                or alias['beforeSha256'] != before[name]['sha256'] or alias['afterSha256'] != after[name]['sha256']):
            raise ValueError('Expected-file path review does not match the recorded case/bytes')
        for item in plan['sets']:
            if before[item['file']]['sha256'] != item['sha256'] or after[item['file']]['sha256'] != item['sha256']:
                raise ValueError('Test-set definition changed: ' + item['file'])
        changes = dict(added=sorted(after.keys() - before.keys()), removed=sorted(before.keys() - after.keys()),
                       changed=sorted(k for k in before.keys() & after.keys() if before[k] != after[k]))
        if changes != audit['disposableCorpusDifferences'] or len(changes['changed']) != 4 or len(changes['added']) != 250:
            raise ValueError('Corpus-write audit mismatch')
        outputs = archive.read('GENERATED-OUTPUTS.zip')
        if hashlib.sha256(outputs).hexdigest() != audit['generatedOutputsSha256']:
            raise ValueError('Generated bytes archive hash mismatch')
        with zipfile.ZipFile(io.BytesIO(outputs)) as generated:
            if set(generated.namelist()) != set(changes['changed'] + changes['added']):
                raise ValueError('Missing generated/changed output')
            for name in generated.namelist():
                if hashlib.sha256(generated.read(name)).hexdigest() != after[name]['sha256']:
                    raise ValueError('Generated byte mismatch: ' + name)
        if not audit['originalCorpusUnchanged'] or any(audit['originalCorpusDifferences'].values()):
            raise ValueError('Original edited corpus was changed')
        if len(load('ORIGINAL-CORPUS-BEFORE.json')) != 13229 or not audit['runtimeLayoutUnchanged']:
            raise ValueError('Preservation/runtime audit mismatch')
    print('Verified 14,601 named outcomes: 10,199 passed / 22 failed / 4,380 skipped; all reasons, original receipts and write-audit caveats retained.')


if __name__ == '__main__':
    verify()
