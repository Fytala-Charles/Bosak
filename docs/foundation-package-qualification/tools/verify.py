"""Verify the retained package feed, restore isolation, API metadata and caller bindings."""
import base64
import hashlib
import io
import json
from pathlib import Path, PureWindowsPath, PurePosixPath
import xml.etree.ElementTree as ET
import zipfile

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def load(path):
    return json.loads(path.read_bytes())


def inputs():
    return load(HERE / 'evidence/packages.json'), load(HERE / 'evidence/project.assets.json'), {m: load(HERE / 'evidence' / (m + '.json')) for m in ['package-source-run', 'unchanged-binary-run']}


def validate(report, assets, runs):
    scope = load(HERE / 'SCOPE.json')
    assert hashlib.sha256((HERE / 'evidence/caller-source.txt').read_bytes()).hexdigest() == load(HERE / 'evidence/execution.json')['callerSourceSha256']
    expected = set(scope['packages'])
    assert len(expected) == len(report['packages']) == 7
    assert report['sourceRevision'] == scope['sourceRevision']
    assert report['apiFreezeAccepted'] is False and report['signingOrPublicationPerformed'] is False
    assert set(assets['libraries']) == {n + '/1.34.2' for n in expected}, 'Unexpected restore dependency'
    assert all(v.get('type') == 'package' for v in assets['libraries'].values()), 'Non-package restore input'
    assert len(assets['packageFolders']) == len(assets['project']['restore']['sources']) == 1, 'Restore isolation lost'
    config = ET.parse(HERE / 'evidence/NuGet.Config').getroot()
    assert config.find('packageSources/clear') is not None and config.find('fallbackPackageFolders/clear') is not None
    assert [a.get('key') for a in config.findall('packageSources/add')] == ['candidate-local']
    # Inspect historical paths lexically; the original cache need not still exist.
    def recorded_path(value):
        return PureWindowsPath(value) if '\\' in value or ':' in value[:3] else PurePosixPath(value)
    feed = recorded_path(config.find('packageSources/add').get('value'))
    cache = recorded_path(config.find('config/add').get('value'))
    assert feed.is_absolute() and cache.is_absolute() and feed.parent == cache.parent
    assert '.guard-tmp' in feed.parts and feed.name == 'feed' and cache.name == 'cache'
    assert recorded_path(next(iter(assets['project']['restore']['sources']))) == feed
    assert recorded_path(next(iter(assets['packageFolders']))) == cache
    feed_file = HERE / 'evidence' / report['feedArchive']['filename']
    assert hashlib.sha256(feed_file.read_bytes()).hexdigest() == report['feedArchive']['sha256']
    source_assets = {f['path']: f['sha256'] for f in load(HERE / 'evidence/source-pins.json')['files']}
    baseline = load(ROOT / 'docs/foundation-legacy-compatibility/evidence/baseline.json')
    for run in runs.values():
        assert run['passed'] == 20 and run['failed'] == 0
        assert run['cases'] == baseline['cases'], 'Caller observations changed'
        assert {a['name'] for a in run['loadedAssemblies']} == expected
        assert all('Version=1.34.2.0,' in a['identity'] for a in run['loadedAssemblies']), 'Wrong assembly version'
        assert all(a['informationalVersion'] == '1.34.2+' + scope['sourceRevision'] for a in run['loadedAssemblies']), 'Wrong loaded source revision'
    assert runs['unchanged-binary-run']['callerSha256'] == baseline['callerSha256']
    assert runs['unchanged-binary-run']['requestedBosakReferences'] == baseline['requestedBosakReferences']
    assert all('Version=1.34.2.0,' in r for r in runs['package-source-run']['requestedBosakReferences'])
    old_api = {a['assembly']: a for a in load(ROOT / 'docs/foundation-api-baseline/evidence/public-api.json')['assemblies']}
    package_api = {a['assembly']: a for a in load(HERE / 'evidence/public-api.json')['assemblies']}
    assert set(package_api) == expected
    assert sum(a['declarationCount'] for a in package_api.values()) == 2205
    with zipfile.ZipFile(feed_file) as feedzip:
        assert set(feedzip.namelist()) == {p['filename'] for p in report['packages']}
        for package in report['packages']:
            raw = feedzip.read(package['filename'])
            assert hashlib.sha256(raw).hexdigest() == package['sha256'], 'Package hash mismatch'
            sha512 = base64.b64encode(hashlib.sha512(raw).digest()).decode()
            assert sha512 == package['sha512'] == assets['libraries'][package['id']+'/1.34.2']['sha512'], 'Restore package hash mismatch'
            with zipfile.ZipFile(io.BytesIO(raw)) as z:
                tree = ET.fromstring(z.read(next(n for n in z.namelist() if n.endswith('.nuspec'))))
                ns = {'n': tree.tag.split('}')[0][1:]}
                assert tree.find('.//n:id', ns).text == package['id']
                assert tree.find('.//n:version', ns).text == scope['version']
                assert tree.find('.//n:repository', ns).get('commit') == package['repositoryCommit'] == scope['sourceRevision'], 'Package source pin mismatch'
                assert 'NOTICE' in z.namelist() and 'license.md' in z.namelist() and 'README.md' in z.namelist()
                assert tree.find('.//n:license', ns).get('type') == 'file'
                for name, digest in package['payloadHashes'].items():
                    assert hashlib.sha256(z.read(name)).hexdigest() == digest == source_assets[name]
                assert '.signature.p7s' not in z.namelist()
                dll = z.read('lib/net10.0/' + package['id'] + '.dll')
                dllhash = hashlib.sha256(dll).hexdigest()
                assert dllhash == package['assemblySha256'] == package_api[package['id']]['sha256'].lower()
                for run in runs.values():
                    assert next(a for a in run['loadedAssemblies'] if a['name'] == package['id'])['sha256'] == dllhash
                for d in tree.findall('.//n:dependency', ns):
                    assert d.get('id') in expected and d.get('version') == '1.34.2'
                if package['id'] == 'Bosak.XPath.Standard':
                    assert hashlib.sha256(z.read('licenses/Unicode17-LICENSE.txt')).hexdigest() == source_assets['src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt']
            assert package_api[package['id']]['declarations'] == old_api[package['id']]['declarations']
    with zipfile.ZipFile(ROOT / 'docs/foundation-legacy-compatibility/evidence/baseline-runtime.zip') as z:
        assert all(hashlib.sha256(z.read(name)).hexdigest() == digest for name, digest in report['retainedCallerFiles'].items())
    return 7, 20, 2, 2205


if __name__ == '__main__':
    result = validate(*inputs())
    manifest = HERE / 'MANIFEST.json'
    if manifest.exists():
        for entry in load(manifest)['files']:
            assert hashlib.sha256((HERE / entry['path']).read_bytes()).hexdigest() == entry['sha256'], entry['path']
    print('Verified packages/cases/modes/API entries:', result, '; publication and freeze pending.')
