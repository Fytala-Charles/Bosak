"""Verify retained-caller execution, exact dependencies and cross-version case equality."""
import hashlib
import json
from pathlib import Path
import zipfile

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]
MODES = ['baseline', 'unchanged-binary', 'recompiled-source']
CALLER_FILES = ['LegacyCaller.dll', 'LegacyCaller.deps.json', 'LegacyCaller.runtimeconfig.json']


def load(path):
    return json.loads(path.read_bytes())


def inputs():
    return {mode: load(HERE / 'evidence' / (mode + '.json')) for mode in MODES}, load(HERE / 'evidence/bundles.json')


def validate(runs, bundles):
    qualified = {a['assembly']: a['candidateSha256'].lower() for a in load(ROOT / 'docs/streaming-completion-cleanup/evidence/api-comparison.json')['assemblies']}
    scope = load(HERE / 'SCOPE.json')
    expected_ids = ['LC-' + str(i).zfill(2) for i in range(1, 21)]
    members_by_mode = {}
    assert len(bundles['bundles']) == 3
    for bundle in bundles['bundles']:
        mode = bundle['mode']
        archive = HERE / 'evidence' / bundle['archive']
        assert hashlib.sha256(archive.read_bytes()).hexdigest() == bundle['sha256'], 'Bundle hash mismatch'
        with zipfile.ZipFile(archive) as z:
            assert set(z.namelist()) == {e['path'] for e in bundle['members']}
            assert 'license.md' in z.namelist() and 'NOTICE' in z.namelist()
            members_by_mode[mode] = {n: z.read(n) for n in z.namelist()}
        members = members_by_mode[mode]
        for entry in bundle['members']:
            assert hashlib.sha256(members[entry['path']]).hexdigest() == entry['sha256']
        run = runs[mode]
        assert run['passed'] == 20 and run['failed'] == 0
        assert [c['id'] for c in run['cases']] == expected_ids
        assert all(c['passed'] and c['error'] is None for c in run['cases'])
        assert run['callerSha256'] == hashlib.sha256(members['LegacyCaller.dll']).hexdigest(), 'Caller hash mismatch'
        assert len(run['loadedAssemblies']) == 7
        version = '1.34.1.0' if mode == 'baseline' else '1.34.2.0'
        for assembly in run['loadedAssemblies']:
            assert 'Version=' + version + ',' in assembly['identity'], 'Wrong loaded version'
            assert hashlib.sha256(members[assembly['name'] + '.dll']).hexdigest() == assembly['sha256']
            if mode != 'baseline':
                assert assembly['sha256'] == qualified[assembly['name']], 'Wrong qualified dependency'
            else:
                assert scope['baseline']['sourceRevision'] in assembly['informationalVersion']
    assert runs['baseline']['cases'] == runs['unchanged-binary']['cases'] == runs['recompiled-source']['cases'], 'Case observations differ'
    for filename in CALLER_FILES:
        assert members_by_mode['baseline'][filename] == members_by_mode['unchanged-binary'][filename], 'Caller artifact changed'
    assert runs['baseline']['requestedBosakReferences'] == runs['unchanged-binary']['requestedBosakReferences']
    assert all('Version=1.34.1.0,' in r for r in runs['baseline']['requestedBosakReferences'])
    assert all('Version=1.34.2.0,' in r for r in runs['recompiled-source']['requestedBosakReferences'])
    assert len(runs['baseline']['requestedBosakReferences']) == 4
    values = {c['id']: c['value'] for c in runs['baseline']['cases']}
    assert values['LC-07']['code'] == values['LC-08']['code'] == 'XPST0008'
    assert values['LC-09']['code'] == 'XPST0081'
    assert values['LC-13']['IsValid'] is False and values['LC-13']['errors'] == 1
    assert values['LC-17']['kind'] == 'Decimal' and values['LC-17']['value'] == 12.5
    assert values['LC-19']['annotationStripped'] is True
    return 20, 3, 7


def main():
    result = validate(*inputs())
    manifest = HERE / 'MANIFEST.json'
    if manifest.exists():
        for entry in load(manifest)['files']:
            assert hashlib.sha256((HERE / entry['path']).read_bytes()).hexdigest() == entry['sha256'], entry['path']
    print('Verified cases/modes/dependencies:', result, '; actual consumer and freeze acceptance remain pending.')


if __name__ == '__main__':
    main()
