"""Verify the Braid registration and pinned source observations."""
from pathlib import Path
import hashlib
import json
import subprocess

PACKET = Path(__file__).resolve().parents[1]
ROOT = PACKET.parents[1]
data = json.loads((PACKET / 'SOURCE-INPUTS.json').read_text(encoding='utf-8'))
for item in data['sourceInputs']:
    blob = subprocess.check_output(['git', '-C', data['braidRepository'], 'show', item['revision'] + ':' + item['path']])
    assert hashlib.sha256(blob).hexdigest() == item['gitBlobSha256']
text = (PACKET / 'README.md').read_text(encoding='utf-8')
owner = Path(data['braidRepository']) / 'docs/SHARED-FOUNDATION-WORK-PACKAGES.md'
assert all(f'| {package} ' in text and f'| {package} ' in owner.read_text(encoding='utf-8') for package in data['packages'])
assert data['implementationAccepted'] is data['freezeAccepted'] is False
freeze = json.loads((ROOT / 'docs/FOUNDATION-API-FREEZE.template.json').read_text(encoding='utf-8'))
assert freeze['status'] == 'Pending'
assert freeze['braidFoundationEvidence'] is freeze['braidOwnerSignoff'] is None
manifest = PACKET / 'MANIFEST.json'
if manifest.exists():
    for item in json.loads(manifest.read_text(encoding='utf-8'))['files']:
        assert hashlib.sha256((PACKET / item['path']).read_bytes()).hexdigest() == item['sha256']
print('PASS: six source pins, five registered Braid packages, pending implementation and four-owner freeze template.')
