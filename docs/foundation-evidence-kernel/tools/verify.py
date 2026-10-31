from pathlib import Path
import json,hashlib,zipfile
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
m=json.loads((P/'MANIFEST.json').read_text(encoding='utf-8'))
for n,h in m.items():assert sha((P/n).read_bytes())==h,n
i=json.loads((P/'evidence/INPUTS.json').read_text(encoding='utf-8'))
assert sha((P/'tools/Probe.cs.txt').read_bytes())==i['probeSha256']
assert sha((P/'tools/Kernel.cs.txt').read_bytes())==i['kernelSourceSha256']
assert sha((ROOT/i['draftPath']).read_bytes())==i['draftSha256']
assert sha((ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip').read_bytes())==i['feedSha256']
assert sha((ROOT/i['priorSourceArchive']).read_bytes())==i['sourceArchiveSha256']
assert sha((ROOT/'docs/foundation-native-adapter/evidence/native-source.zip').read_bytes())==i['nativeSourceArchiveSha256']
assert sha((P/'evidence/runtime-source.zip').read_bytes())==i['runtimeSourceSha256']
s=json.loads((P/'evidence/RUNTIME-SOURCE.json').read_text(encoding='utf-8'));assert s['revision']==i['sourceRevision']=='c88eea4308c0e49a81e7bfe2f481319f2e4d208c'
with zipfile.ZipFile(P/'evidence/runtime-source.zip') as z:
 assert set(z.namelist())==set(s['files'])
 for n,h in s['files'].items():assert sha(z.read(n))==h
layouts={v['mode']:v['layout'] for v in i['variants']};assert set(layouts)=={'baseline','holder','kernel'}
assert layouts['baseline']==i['packageDlls']
assert sorted(n for n,h in layouts['holder'].items() if h!=i['packageDlls'][n])==['Bosak.XPath.Core.dll']
assert sorted(n for n,h in layouts['kernel'].items() if h!=i['packageDlls'][n])==['Bosak.XPath.Core.dll','Bosak.XPath.Runtime.dll']
parent=json.loads((ROOT/'docs/foundation-snapshot-navigation/evidence/round-0-baseline.json').read_text(encoding='utf-8'));oldIds=[c['id'] for c in parent['checks']]
plainReference=None
for mode in ['baseline','holder','kernel']:
 previous=None
 for index in range(2):
  r=json.loads((P/f'evidence/round-{index}-{mode}.json').read_text(encoding='utf-8'));count=205 if mode=='kernel' else 162
  assert r['mode']==mode and r['checkCount']==len(r['checks'])==count and all(c['passed'] for c in r['checks'])
  assert len({c['id'] for c in r['checks']})==count
  assert [c['id'] for c in r['checks'] if not c['id'].startswith('K')]==oldIds
  if mode=='kernel':
   assert {c['id'].split('-')[0] for c in r['checks'] if c['id'].startswith('K')}=={f'K{n:02}' for n in range(1,44)}
   k=r['kernelChecks'];assert k['enabled'] and k['Released'] and k['CleanupReferenceCleared'] and k['evidenceCacheCleared'] and k['cachesCleared']
   assert k['ConstructorCalls']==14 and k['NoOpReuses']==4 and k['FreshInteger']==4 and k['UnknownTargets']==2 and k['UnissuedMarkers']==1
  else:assert r['kernelChecks']=={'enabled':False}
  assert (P/f'evidence/round-{index}-{mode}.stderr.txt').read_bytes()==b''
  assert len(r['bindings'])==7 and {b['name']+'.dll':b['sha256'] for b in r['bindings']}==layouts[mode]
  assert all(b['context']=='Default' and not b['collectible'] for b in r['bindings'])
  assert len(r['observations'])==11 and len(r['navigation'])==23
  preserve={'child','descendant','following-sibling','preceding-sibling','following','preceding','parent','self','ancestor-or-self','descendant-or-self','copy-data','same-type-constructor'}
  for o in r['navigation']:assert o['originPreserved']==o['typePreserved']==(mode!='baseline' and o['id'] in preserve)
  for o in r['observations']:assert o['markerPreserved']==o['typePreserved']==(mode!='baseline' and o['id'] not in ['arithmetic','integer-cast','sum'])
  content=(r['observations'],r['navigation'],r['kernelChecks'],r['snapshotMetrics'])
  if previous is not None:assert content==previous
  previous=content
  plain=([(o['id'],o['value']) for o in r['observations']],[(o['id'],o['values']) for o in r['navigation']])
  if plainReference is None:plainReference=plain
  else:assert plain==plainReference
for f in (P/'evidence').glob('*-build.txt'):assert '0 Warning(s)' in f.read_text(encoding='utf-8') and '0 Error(s)' in f.read_text(encoding='utf-8')
print('PASS: 162 controls x 4 + 205 x 2; exact source and loaded layouts, constructor bridge, detached targets, issuance, snapshot cleanup/capacity; no production acceptance inferred.')
