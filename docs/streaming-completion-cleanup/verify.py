"""Verify the cleanup candidate source, compressed test receipts and frozen skip maps."""
import gzip
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def load(path):
    return json.loads(path.read_bytes())


def main():
    results = load(HERE / 'RESULTS.json')
    candidate = load(HERE / 'evidence/candidate-source.json')
    for entry in candidate['changedProductionSourceFiles']:
        assert hashlib.sha256((ROOT / entry['path']).read_bytes()).hexdigest() == entry['candidateSha256'], entry['path']
    regression = candidate['newRegressionSource']
    assert hashlib.sha256((ROOT / regression['path']).read_bytes()).hexdigest() == regression['sha256']
    assert '<Version>1.34.2</Version>' in (ROOT / 'src/Directory.Build.props').read_text()
    total = 0
    for receipt in results['fullTests']['projects']:
        raw_gzip = (HERE / 'evidence' / receipt['receipt']).read_bytes()
        assert hashlib.sha256(raw_gzip).hexdigest() == receipt['gzipSha256']
        raw = gzip.decompress(raw_gzip)
        assert hashlib.sha256(raw).hexdigest() == receipt['uncompressedSha256']
        tree = ET.fromstring(raw)
        counts = tree.find('.//t:Counters', NS)
        assert tree.find('.//t:ResultSummary', NS).get('outcome') == 'Completed'
        assert int(counts.get('failed')) == 0 and int(counts.get('passed')) == receipt['passed']
        total += receipt['passed']
    assert total == results['fullTests']['passed'] == 24225
    for name, expected in [('before', (5, 6)), ('after', (11, 0)), ('final', (11, 0))]:
        raw = gzip.decompress((HERE / 'evidence' / ('focused-' + name + '.trx.gz')).read_bytes())
        counts = ET.fromstring(raw).find('.//t:Counters', NS)
        assert (int(counts.get('passed')), int(counts.get('failed'))) == expected
    for gate in ['qt3', 'qt4']:
        saved = (HERE / 'evidence' / (gate + '-skips.txt')).read_text(encoding='utf-8-sig')
        baseline = (ROOT / results['gates'][gate]['baseline']).read_text(encoding='utf-8-sig')
        assert saved == baseline, gate
    comparison = load(HERE / 'evidence/api-comparison.json')
    assert len(comparison['assemblies']) == 11
    assert sum(a['declarationEntries'] for a in comparison['assemblies']) == 5766
    assert all(a['metadataUnchanged'] for a in comparison['assemblies'])
    assert results['apiFreezeAccepted'] is False and results['packagePublished'] is False
    manifest = HERE / 'MANIFEST.json'
    if manifest.exists():
        for entry in load(manifest)['files']:
            assert hashlib.sha256((HERE / entry['path']).read_bytes()).hexdigest() == entry['sha256'], entry['path']
    print('Verified 24,225 full tests, before/after regressions, candidate source, API comparison and exact skip maps; release/freeze pending.')


if __name__ == '__main__':
    main()
