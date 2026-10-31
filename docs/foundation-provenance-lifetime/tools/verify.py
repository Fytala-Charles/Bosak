"""Verify bounded lifetime receipts, actual draft nullability, and proposed policy state."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
def read(f):return json.loads((P/f).read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
i=read('evidence/INPUTS.json');r=read('RESULTS.json')
assert sha((ROOT/i['draftPath']).read_bytes())==i['draftSha256']
assert sha((ROOT/i['parentDraftPath']).read_bytes())==i['parentDraftSha256']
assert sha((ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip').read_bytes())==i['feedSha256']
assert i['fixtureDirectoriesRemoved'] is True and i['productionModified'] is False
log=(P/'evidence/build.txt').read_text(encoding='utf-8')
assert 'Build succeeded.' in log and '0 Warning(s)' in log and '0 Error(s)' in log
for n in range(3):
    d=read(f'evidence/run-{n}.json')
    assert d['scenarios']==12 and d['checkCount']==len(d['checks'])==46
    assert len({c['id'] for c in d['checks']})==46 and all(c['passed'] is True for c in d['checks'])
    assert [c['id'] for c in d['checks']]==r['checkIds']
    assert d['coreSha256']==i['coreSha256']
    assert (P/f'evidence/run-{n}.stderr.txt').read_bytes()==b''
    assert {p['name']:p['nullability'] for p in d['atomicEvidenceProperties']}=={'DeclaredType':'NotNull','ActualType':'NotNull','OriginalAssessment':'Nullable'}
    assert d['nodeOriginNullability']=='NotNull'
    assert len(d['ownedFileFixtures'])==8
    assert all(f['expected']==f['actual']==sha(bytes([42])) and f['closeCount']==1 for f in d['ownedFileFixtures'])
    assert all(o['active']==0 for o in d['owners'])
    assert d['owners'][2]['events']==['enter','begin-close','exit','operation-release-requested']
    assert d['owners'][9]['events']==['enter','exit','begin-close','operation-release-requested']
    assert d['negativeWaiterPatternDemonstrated'] is True
laws=read('PROPOSED-LAWS.json')
assert {x['id'] for x in laws['laws']}=={f'PL-{n:02}' for n in range(1,11)}
assert laws['accepted'] is laws['productionImplemented'] is False
basis=read('SPEC-BASIS.json')
assert {s['immutableRevision'] for s in basis['sources']}=={'REC-xpath-31-20170321','REC-xpath-datamodel-31-20170321'}
assert all('/TR/2017/REC-' in s['url'] and s['sections'] for s in basis['sources'])
assert r['normativeProductionAdoption'] is r['productionImplemented'] is r['apiFrozen'] is r['fullE04Complete'] is False
if (P/'MANIFEST.json').exists():
    for x in read('MANIFEST.json')['files']:assert sha((P/x['path']).read_bytes())==x['sha256'],x['path']
print('PASS: V2 atomic/node nullability, three repeated 12-scenario/46-check file-lifetime runs, unsafe ownership witness, immutable proposal basis and pending acceptance.')
