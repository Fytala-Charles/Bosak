"""Offline replay of retained timing evidence; never launches qualification or modifies policy."""
from pathlib import Path
import hashlib
import importlib.util
import json
import zipfile
import xml.etree.ElementTree as ET

here = Path(__file__).resolve().parent
root = here.parents[1]
manifest = json.loads((here / 'MANIFEST.json').read_text(encoding='utf-8'))


def check(data, expected):
    assert len(data) == expected['bytes'] and hashlib.sha256(data).hexdigest() == expected['sha256']


for name, expected in manifest['files'].items():
    check((here / name).read_bytes(), expected)
spec = importlib.util.spec_from_file_location('analysis', here / 'analyze.py')
analysis = importlib.util.module_from_spec(spec)
spec.loader.exec_module(analysis)
with zipfile.ZipFile(here / 'EVIDENCE.zip') as archive:
    assert set(archive.namelist()) == set(manifest['archiveEntries'])
    for name, expected in manifest['archiveEntries'].items():
        check(archive.read(name), expected)
    sources = json.loads((here / 'SOURCES.json').read_text(encoding='utf-8'))
    actual = analysis.analyze(root, sources, archive.read)
    assert actual == json.loads((here / 'REPORT.json').read_text(encoding='utf-8'))
    assert actual['completeRuns'] == 4 and actual['timeouts'] == 3 and actual['newDiagnosticExecutions'] == 0
    assert not actual['rootCauseEstablished'] and not actual['policyChanged']
    controls = json.loads((here / 'CONTROLS.json').read_text(encoding='utf-8'))
    assert controls['success'] and controls['count'] == len(controls['controls']) == 13
    for row in controls['controls']:
        assert row['success']
        if 'expected' in row:
            assert analysis.comparison(row['left'], row['right']) == row['expected'] == row['actual']
        else:
            try:
                if 'text' in row:
                    analysis.progress(row['text'])
                else:
                    analysis.comparison(row['left'], row['right'])
            except (ValueError, AssertionError) as error:
                assert type(error).__name__ == row['expectedException']
            else:
                raise AssertionError('Expected refusal: ' + row['name'])
    fixture = archive.read('fixtures/date-020.xsl')
    policy = json.loads((root / 'docs/xslt30-gate/BASELINE.json').read_text(encoding='utf-8'))
    with zipfile.ZipFile(root / policy['evidence']) as baseline:
        entry = json.loads(baseline.read('CORPUS-BEFORE.json'))['tests/type/date/date-020.xsl']
    check(fixture, entry)
    ns = {'x': 'http://www.w3.org/1999/XSL/Transform'}
    document = ET.fromstring(fixture)
    assert any(e.get('select') == '1 to 1000000' for e in document.findall('.//x:for-each', ns))
    assert sum(e.get('select') == 'current-dateTime()' for e in document.findall('.//x:variable', ns)) == 2
    scope = json.loads(archive.read('SCOPE.json'))
    assert scope['analysisOnly'] and scope['newDiagnosticExecutions'] == 0 and not scope['mutatedGateSettings']
print('Verified seven original attempts, four complete 14,601-case replays, three preserved timeouts, two exact-payload comparisons and thirteen refusal/comparison controls.')
print('Root cause remains open; no new diagnostic execution, engine, gate, fixture, exclusion or budget change.')
