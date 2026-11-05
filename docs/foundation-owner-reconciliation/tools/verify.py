"""Verify the sealed owner returns without relying on moving owner worktrees."""
from pathlib import Path
import hashlib
import json

PACKET = Path(__file__).resolve().parents[1]
ROOT = PACKET.parents[1]
def read(path):
    return json.loads((PACKET / path).read_text(encoding='utf-8-sig'))
def sha(data):
    return hashlib.sha256(data).hexdigest()
r = read('INPUTS.json')
for entry in r['inputs']:
    assert sha((PACKET / entry['snapshot']).read_bytes()) == entry['sha256'], entry['snapshot']
x = read('inputs/xmldb-matrix.json')
assert {row['id'] for row in x['matrix']} == {f'FXR-{i:02}' for i in range(1,16)}
assert {row['id'] for row in x['openItems']} == {f'FXG-{i:02}' for i in range(1,7)}
assert len({m for row in x['matrix'] for m in row['manifestEntryIds']}) == 176
assert sha((ROOT / x['manifest']['path']).read_bytes()) == x['manifest']['sha256']
s = read('inputs/schema-fixture-identities.json')
assert {f['id'] for f in s['fixtures']} == {f'F-{i:02}' for i in range(1,7)}
needs = (PACKET / 'inputs/schema-foundation-needs.md').read_text(encoding='utf-8-sig')
assert all(f'| N{i} |' in needs for i in range(1,9))
returned = read('inputs/xmldb-characterization-OWNER-RETURN.json')
assert all(i['finalAccepted'] is False for i in returned['requiredContractGaps'])
assert r['requirementsReturned'] is True
assert r['ownerAcceptanceReturned'] is r['implementationAuthorized'] is r['apiFrozen'] is False
if (PACKET / 'MANIFEST.json').exists():
    for entry in read('MANIFEST.json')['files']:
        assert sha((PACKET / entry['path']).read_bytes()) == entry['sha256'], entry['path']
print('PASS: owner snapshots, 15 XmlDb areas / 176 IDs / six gaps, eight Schema needs / six fixtures; no acceptance inferred.')
