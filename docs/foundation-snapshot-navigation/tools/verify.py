from pathlib import Path
import json,hashlib,zipfile
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
m=json.loads((P/'MANIFEST.json').read_text(encoding='utf-8'))
for n,h in m.items():assert sha((P/n).read_bytes())==h,n
i=json.loads((P/'evidence/INPUTS.json').read_text(encoding='utf-8'));source=json.loads((ROOT/'docs/foundation-native-adapter/evidence/SOURCE.json').read_text(encoding='utf-8'))
assert source['revision']==i['nativeSourceRevision']=='c3624998eca8c83018e121fa84af611f834a0501'
assert sha((ROOT/'docs/foundation-native-adapter/evidence/native-source.zip').read_bytes())==i['nativeSourceArchiveSha256']
with zipfile.ZipFile(ROOT/'docs/foundation-native-adapter/evidence/native-source.zip') as z:
 for f in source['files']:assert sha(z.read(f['path']))==f['sha256']
assert sha((P/'tools/Probe.cs.txt').read_bytes())==i['probeSha256']
assert sha((ROOT/i['draftPath']).read_bytes())==i['draftSha256']
assert sha((ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip').read_bytes())==i['feedSha256']
assert sha((ROOT/i['priorSourceArchive']).read_bytes())==i['sourceArchiveSha256']
layouts={v['mode']:v['layout'] for v in i['variants']};assert set(layouts)=={'baseline','holder'}
assert layouts['baseline']==i['packageDlls']
assert [n for n in layouts['baseline'] if layouts['baseline'][n]!=layouts['holder'][n]]==['Bosak.XPath.Core.dll']
previous=None
for mode in ['baseline','holder']:
 runs=[]
 for index in range(2):
  r=json.loads((P/f'evidence/round-{index}-{mode}.json').read_text(encoding='utf-8'));assert r['mode']==mode
  assert r['checkCount']==len(r['checks'])==162 and all(c['passed'] for c in r['checks'])
  assert len({c['id'] for c in r['checks']})==162
  assert (P/f'evidence/round-{index}-{mode}.stderr.txt').read_bytes()==b''
  assert len(r['bindings'])==7
  assert {b['name']+'.dll':b['sha256'] for b in r['bindings']}==layouts[mode]
  assert all(b['context']=='Default' and not b['collectible'] for b in r['bindings'])
  observations=r['observations'];assert len(observations)==11
  for o in observations:
   expected=mode=='holder' and o['id'] not in ['arithmetic','integer-cast','sum']
   assert o['markerPreserved']==o['typePreserved']==expected
  assert len(r['navigation'])==23
  assert sum(c['id'].startswith('Axis-') for c in r['checks'])==13
  assert sum(c['id'].startswith('R0') for c in r['checks'])==6
  expectedPaths={'child','descendant','following-sibling','preceding-sibling','following','preceding','parent','self','ancestor-or-self','descendant-or-self','copy-data','same-type-constructor'}
  for path in r['navigation']:
   assert path['originPreserved']==path['typePreserved']==(mode=='holder' and path['id'] in expectedPaths)
  assert r['snapshotMetrics']=={'original':{'elementFacts':3,'regularWrappers':9,'namespaceWrappers':6},'copy':{'elementFacts':3,'regularWrappers':7,'namespaceWrappers':0}}
  runs.append(observations)
  nav=r['navigation']
  if index==0:navPrevious=nav
  else:assert nav==navPrevious
 assert runs[0]==runs[1]
 plain=[(o['id'],o['value']) for o in runs[0]]
 if previous is not None:assert plain==previous
 previous=plain
 navPlain=[(o['id'],o['values']) for o in nav]
 if mode=='baseline':baselineNavigation=navPlain
 else:assert navPlain==baselineNavigation
print('PASS: pinned native/V5/cohort hashes; 162 controls x 4; all 13 axes, 23 navigation expressions and 6 candidate rule guards; no production acceptance inferred.')
