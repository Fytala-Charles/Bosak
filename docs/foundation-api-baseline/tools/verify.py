"""Verify the sealed F140-01 metadata baseline without changing it."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[3]
evidence = Path(__file__).resolve().parents[1] / 'evidence'
baseline = json.loads((evidence / 'baseline.json').read_text(encoding='utf-8'))
api_bytes = (evidence / 'public-api.json').read_bytes()
assert hashlib.sha256(api_bytes).hexdigest() == baseline['inventorySha256'], 'API receipt changed'
for relative, digest in baseline['auditArtifactHashes'].items():
    assert hashlib.sha256((evidence.parent / relative).read_bytes()).hexdigest() == digest, relative
api = json.loads(api_bytes)
for entry in baseline['sourceFiles']:
    assert hashlib.sha256((root / entry['path']).read_bytes()).hexdigest() == entry['sha256'], entry['path']
expected = {(a['assembly'], row[2:].split(':')[0]) for a in api['assemblies'] for row in a['declarations'] if row.startswith('T ')}
scope = json.loads((evidence / 'proposed-type-scope.json').read_text(encoding='utf-8'))
actual = {(t['assembly'], t['type']) for t in scope['types']}
assert expected == actual and len(scope['types']) == len(expected), 'Incomplete/duplicate type dispositions'
assert all(t['frozen'] is False and t['ownerDisposition'] == 'Pending' for t in scope['types'])
assert all(baseline['checks'].values()), 'Capture checks failed'
core = next(a for a in api['assemblies'] if a['assembly'] == 'Bosak.XPath.Core')['declarations']
assert any(r.startswith('M Bosak.XPath.Core.Xdm.IXdmNode.get_NodeKind:') and 'Abstract' in r for r in core)
assert any(r.startswith('M Bosak.XPath.Core.Xdm.IXdmNode.get_HasNoTypedValue:') and 'Abstract' not in r for r in core)
assert any(r.startswith('F Bosak.XPath.Core.Xdm.XdmNodeKind.Element:') and ':2:' in r for r in core)
assert any(r.startswith('L Bosak.XPath.Core.Xdm.XdmValue._reference:') for r in core)
assert any('Nullable' in r for r in core)
assert not any(r.startswith('M Bosak.XPath.Core.Xdm.IXdmSequenceEnumerator.Dispose:') for r in core)
print(f"Verified {len(baseline['sourceFiles'])} source/build hashes, {len(expected)} type dispositions and API receipt/boundaries; freeze remains pending.")
