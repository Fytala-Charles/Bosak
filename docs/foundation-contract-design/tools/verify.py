"""Verify exact draft inputs and shape evidence; never infer runtime acceptance."""
from pathlib import Path
import hashlib
import io
import json
import subprocess
import zipfile

PACKET = Path(__file__).resolve().parents[1]
ROOT = PACKET.parents[1]
def read(path):
    return json.loads((PACKET / path).read_text(encoding='utf-8-sig'))
def sha(data):
    return hashlib.sha256(data).hexdigest()
r = read('evidence/INPUTS.json')
assert sha((PACKET / 'SIGNATURES.cs.txt').read_bytes()) == r['draftSourceSha256']
assert sha((PACKET / 'evidence/Draft.csproj.txt').read_bytes()) == r['draftProjectSha256']
feed = ROOT / 'docs/foundation-package-qualification/evidence/candidate-feed.zip'
assert sha(feed.read_bytes()) == r['feedSha256']
with zipfile.ZipFile(feed) as z:
    package = z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
assert sha(package) == r['corePackageSha256']
with zipfile.ZipFile(io.BytesIO(package)) as z:
    assert sha(z.read('lib/net10.0/Bosak.XPath.Core.dll')) == r['coreAssemblySha256']
surface = read('evidence/SURFACE.json')
assert surface['loadedCoreSha256'] == r['coreAssemblySha256']
assert len(surface['types']) == 13
assert all(t['type'].startswith('C13901.Draft.') for t in surface['types'])
assert surface['pointerSize'] == 8 and surface['currentXdmValueSize'] == 48
for item in read('evidence/SOURCE-INPUTS.json')['files']:
    assert sha(subprocess.check_output(['git', '-C', str(ROOT), 'show', item['revision'] + ':' + item['path']])) == item['sha256']
plan = read('EXPERIMENTS.json')
assert {e['id'] for e in plan['experiments']} == {'E-01','E-02','E-03','E-04'}
assert all(e['status'] == 'NotRun' and e['results'] is None and e['workloadBudget'] is None for e in plan['experiments'])
assert plan['acceptedSignatures'] == [] and plan['carrierSelected'] is None
assert plan['productionImplementationAuthorized'] is plan['apiFrozen'] is False
if (PACKET / 'MANIFEST.json').exists():
    for item in read('MANIFEST.json')['files']:
        assert sha((PACKET / item['path']).read_bytes()) == item['sha256'], item['path']
print('PASS: exact 13-type draft/source/package metadata, current carrier observation and four unrun experiments; no API acceptance inferred.')
