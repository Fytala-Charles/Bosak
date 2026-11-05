"""Verify the finite review inventory and pinned read-only evidence."""
from pathlib import Path
import hashlib
import json
import subprocess

ROOT = Path(__file__).resolve().parents[3]
DOSSIER = Path(__file__).resolve().parents[1]

def digest(data):
    return hashlib.sha256(data).hexdigest()

def read(name):
    return json.loads((DOSSIER / name).read_text(encoding='utf-8-sig'))

matrix = read('DECISION-MATRIX.json')
source = ROOT / matrix['prototypeInput']['path']
assert digest(source.read_bytes()) == matrix['prototypeInput']['sha256']
prototype = json.loads(source.read_text(encoding='utf-8-sig'))['exportedTypes']
actual = [t for g in matrix['groups'] for t in g['prototypeTypes']]
assert len(matrix['groups']) == 6
assert len(actual) == len({t['type'] for t in actual}) == len(prototype) == 32
assert {t['type']: t['members'] for t in actual} == {t['type']: t['members'] for t in prototype}
assert sum(len(t['members']) for t in actual) == matrix['prototypeInput']['memberEntries'] == 377
assert all(t['productionTypeAccepted'] is False for t in actual)
assert matrix['decisions'] == dict(acceptedProductionTypes=[], acceptedProductionSignatures=[], ownerResponse=None, c45ImplementationAuthorized=False, c45Accepted=False, apiFrozen=False, releaseAuthorized=False)
for item in read('OWNER-INPUTS.json')['observations']:
    assert item['ownerCandidateAcceptanceReturned'] is False
    for record in item['inputs']:
        data = subprocess.check_output(['git', '-C', item['repositoryPath'], 'show', item['revision'] + ':' + record['path']])
        assert digest(data) == record['sha256'], record['path']
disposition = read('DISPOSITION.template.json')
for key, value in disposition.items():
    if key in ('recordFormatVersion', 'status'):
        continue
    assert value is (False if key in ('implementationAuthorized', 'freezeAccepted', 'releaseAuthorized') else None), key
manifest = DOSSIER / 'MANIFEST.json'
if manifest.exists():
    for record in read('MANIFEST.json')['files']:
        assert digest((DOSSIER / record['path']).read_bytes()) == record['sha256'], record['path']
print('Verified: 6 groups, 32 exact types, 377 member entries; pinned committed owner evidence and pending decisions.')
