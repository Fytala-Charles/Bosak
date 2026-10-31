from pathlib import Path
import json,hashlib,statistics,zipfile,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text(encoding='utf-8')).items():assert sha((P/n).read_bytes())==h,n
i=json.loads((P/'evidence/INPUTS.json').read_text(encoding='utf-8'))
assert sha((P/'evidence/core-source.zip').read_bytes())==i['sourceSha256']
assert sha((P/'evidence/product-dlls.zip').read_bytes())==i['productSha256']
assert sha((P/'tools/Probe.cs.txt').read_bytes())==i['probeSha256']
assert sha((P/'evidence/checked-runner.py.txt').read_bytes())==i['checkedRunnerSha256']
if '--check-current' in sys.argv:assert sha((ROOT/i['checkedRunnerPath']).read_bytes())==i['checkedRunnerSha256']
layouts={v['mode']:v['layout'] for v in i['variants']};assert set(layouts)=={'product','source','holder','equal-holder','equal-selective'}
with zipfile.ZipFile(P/'evidence/product-dlls.zip') as z:assert {n:sha(z.read(n)) for n in z.namelist() if n.endswith('.dll')}==layouts['product']
baseline=None;caller=i['frozenFiles']['Probe.dll']
for mode,layout in layouts.items():
 assert [n for n in layout if layout[n]!=layouts['product'][n]]==([] if mode=='product' else ['Bosak.XPath.Core.dll'])
 legacy=json.loads((P/f'evidence/{mode}-legacy.json').read_text(encoding='utf-8'))
 prior=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text(encoding='utf-8'))
 assert legacy['passed']==20 and legacy['failed']==0 and legacy['cases']==prior['cases'] and legacy['callerSha256']==prior['callerSha256']
 assert {a['name']+'.dll':a['sha256'] for a in legacy['loadedAssemblies']}==layout
 previous=None
 for index in range(2):
  r=json.loads((P/f'evidence/round-{index}-{mode}.json').read_text(encoding='utf-8'));assert r['mode']==mode and r['callerSha256']==caller and r['xdmValueSize']==48
  assert {a['name']+'.dll':a['sha256'] for a in r['bindings']}==layout
  assert all(a['context']=='Default' and not a['collectible'] for a in r['bindings'])
  eq=r['equality'];assert eq['atomicFixtures']==23 and eq['controls']==len(eq['checks'])==(253 if mode=='holder' else 828)
  assert all(c['passed'] and c['observed']==c['expected'] for c in eq['checks'])
  assert len(eq['plainPairs'])==len(eq['decoratedPairs'])==529
  if mode!='holder':assert eq['plainPairs']==eq['decoratedPairs']
  assert r['clrEqualsToPlainInteger']==(mode!='holder')
  if baseline is None:baseline=eq['plainPairs']
  else:assert eq['plainPairs']==baseline
  plain=[(o['id'],o['items'] and [(v['kind'],v['value']) for v in o['items']],o['error']) for o in r['observations']];assert len(plain)==27
  if mode=='product' and index==0:plainBaseline=plain
  else:assert plain==plainBaseline
  comparable=(eq['plainPairs'],eq['decoratedPairs'],[(o['id'],[v['origin'] for v in o['items']]) for o in r['observations']])
  if previous is not None:assert comparable==previous
  previous=comparable
  assert len(r['allocations'])==6 and len(r['equalityCosts'])==8
  for group in r['allocations']+r['equalityCosts']:assert len(group['samples'])==7
  def cost(annotated,operation):
   g=next(g for g in r['equalityCosts'] if g['count']==100000 and g['annotated']==annotated and g['operation']==operation)
   return statistics.median(s['allocatedBytes']/g['count'] for s in g['samples'])
  assert cost(False,'equals')==344 and cost(False,'hash')==64
  assert cost(True,'equals')==(408 if mode.startswith('equal-') else 344) and cost(True,'hash')==64
  for g in r['allocations']:
   expected=48*g['items']+24+(32*g['items'] if mode in ['holder','equal-holder'] or mode=='equal-selective' and g['kind']!='integer' else 0)
   allocated=[s['allocatedBytes'] for s in g['samples']]
   assert statistics.median(allocated)==expected
   assert all(x-expected in {0,8,16,24} for x in allocated)
print('PASS: unchanged frozen caller, five checked legacy20/0 cohorts, 23 fixtures/529 pairs, ten VM/equality/allocation runs; 64-byte annotated Equals penalty retained; no production acceptance inferred.')
