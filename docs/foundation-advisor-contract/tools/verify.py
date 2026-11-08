"""Data-only verification of V3 declarations and explicitly synthetic protocol controls."""
from pathlib import Path
import hashlib,io,json,subprocess,zipfile
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
def read(f):return json.loads((P/f).read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
i=read('evidence/INPUTS.json');r=read('RESULTS.json');surface=read('evidence/SURFACE.json')
assert sha((P/'SIGNATURES-V3.cs.txt').read_bytes())==i['draftSourceSha256']
assert sha((ROOT/i['parentDraftPath']).read_bytes())==i['parentDraftSha256']
assert sha((ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip').read_bytes())==i['feedSha256']
with zipfile.ZipFile(ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip') as z:
    package=z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
with zipfile.ZipFile(io.BytesIO(package)) as z:
    assert sha(z.read('lib/net10.0/Bosak.XPath.Core.dll'))==i['coreSha256']
assert len(surface)==35 and all(t['name'].startswith('C13901.Draft.') for t in surface)
log=(P/'evidence/build.txt').read_text(encoding='utf-8');assert 'Build succeeded.' in log and '0 Warning(s)' in log and '0 Error(s)' in log
for n in range(3):
    d=read(f'evidence/run-{n}.json')
    assert d['checkCount']==len(d['checks'])==54
    assert {c['id'].split('-')[0] for c in d['checks']}=={f'C{x:02}' for x in range(1,55)}
    assert all(c['passed'] is True for c in d['checks'])
    assert [c['id'] for c in d['checks']]==r['checkIds']
    assert d['surface']==surface and d['coreSha256']==i['coreSha256']
    assert d['originalEvidencePreserved'] is True and d['proofDispatches']==4
    assert (P/f'evidence/run-{n}.stderr.txt').read_bytes()==b''
need=read('REQUIREMENT-MAP.json')
assert {x['id'] for x in need['schemaNeeds']}=={f'N{n}' for n in range(1,9)}
assert {x['type'] for x in need['surface']}=={x['name'] for x in surface}
assert need['acceptedSignatures']==[] and need['productionImplementationAuthorized'] is need['apiFrozen'] is False
assert r['publicDraftTypes']==35 and r['distinctControls']==54
assert r['realProviderQualified'] is r['performanceAccepted'] is r['apiFrozen'] is False
assert read('SPEC-BASIS.json')['sources'][0]['immutableRevision']=='REC-xmlschema11-1-20120405'
for f in read('evidence/SOURCE-INPUTS.json')['files']:
    assert sha(subprocess.check_output(['git','-C',str(ROOT),'show',f['revision']+':'+f['path']]))==f['sha256']
if (P/'MANIFEST.json').exists():
    for f in read('MANIFEST.json')['files']:assert sha((P/f['path']).read_bytes())==f['sha256'],f['path']
print('PASS: V3 35-type surface; three repeats of 54 synthetic controls; N1–N8 trace; no real-provider, performance or API acceptance inferred.')
