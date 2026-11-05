"""Check design evidence; does not execute proposed lifetime semantics."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());a=json.loads((P/'ACCEPTANCE.json').read_text())
assert i['productionChanges'] is False and i['productionRevision']=='f72db224' and i['productionVersion']=='1.39.0'
for name,digest in i['files'].items():assert sha(R/name)==digest,name
assert a['productionImplemented'] is False and a['ownerAccepted'] is False
assert a['decision']=='session-owned-operation-leases-host-owned-resources'
assert [c['id'] for c in a['cases']]==['CO-'+str(n).zfill(2) for n in range(1,25)]
assert all(c['status']=='NotRun' and c['trigger'] and c['expected'] for c in a['cases'])
baseline=json.loads((R/'docs/foundation-private-host-factories/evidence/SOURCE.json').read_text())
for name,digest in baseline['files'].items():assert sha(R/name)==digest,name
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified design input pins, unchanged production source and 24 unexecuted acceptance cases; no runtime qualification claimed.')
