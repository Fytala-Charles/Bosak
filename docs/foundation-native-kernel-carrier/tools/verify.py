from pathlib import Path
import hashlib,json,zipfile
P=Path(__file__).resolve().parents[1]; ROOT=P.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():
 assert sha((P/n).read_bytes())==h,n
i=json.loads((P/'evidence/INPUTS.json').read_text())
for key,path in [('runtimeArchiveSha256',P/'evidence/runtime-source.zip'),('coreArchiveSha256',ROOT/'docs/foundation-carrier-equality/evidence/core-source.zip'),('productArchiveSha256',ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip'),('nativeArchiveSha256',ROOT/'docs/foundation-native-adapter/evidence/native-source.zip'),('parentCarrierSha256',ROOT/'docs/foundation-carrier-field-equality/evidence/field-selective-XdmValue.cs.txt'),('probeSha256',P/'tools/Probe.cs.txt')]:
 assert sha(path.read_bytes())==i[key],key
with zipfile.ZipFile(ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip') as z:
 product={n:sha(z.read(n)) for n in z.namelist() if n.endswith('.dll')}
prior=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text())
baseline=None
for v in i['variants']:
 mode=v['mode'];layout=v['layout'];assert len(layout)==7
 assert set(n for n in layout if layout[n]!=product[n])==({'Bosak.XPath.Runtime.dll'} if mode=='control' else {'Bosak.XPath.Core.dll','Bosak.XPath.Runtime.dll'})
 old=json.loads((P/f'evidence/{mode}-legacy.json').read_text())
 assert old['passed']==20 and old['failed']==0 and old['cases']==prior['cases'] and old['callerSha256']==prior['callerSha256']
 assert {a['name']+'.dll':a['sha256'] for a in old['loadedAssemblies']}==layout
 previous=None
 for j in range(2):
  r=json.loads((P/f'evidence/round-{j}-{mode}.json').read_text())
  assert r['mode']==mode and r['checkCount']==len(r['checks'])==(103 if mode=='control' else 109)
  assert all(c['passed'] for c in r['checks']) and len({c['id'] for c in r['checks']})==r['checkCount']
  assert {a['name']+'.dll':a['sha256'] for a in r['bindings']}==layout
  assert all(a['context']=='Default' and not a['collectible'] for a in r['bindings'])
  assert len(r['observations'])==14
  plain=[(o['id'],o['expr'],[(e['value'],e['kind']) for e in o['entries']]) for o in r['observations']]
  if baseline is None:baseline=plain
  assert plain==baseline
  stable=(r['checks'],r['observations'])
  if previous is not None:assert stable==previous
  previous=stable
for f in P.glob('evidence/*-build.txt'):
 s=f.read_text();assert '0 Warning(s)' in s and '0 Error(s)' in s
with zipfile.ZipFile(P/'evidence/runtime-source.zip') as z:
 original=z.read('src/Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs').decode('utf-8-sig')
 expected='// Generated isolated validation accessors; original implementation bodies are unchanged.\n'+original.replace('    internal SchemaQueryEnvironment Environment { get; }','    internal SchemaQueryEnvironment Environment { get; }\n    internal void ValidateEvidenceForCarrier(SchemaEvidenceKernel.Evidence evidence) => _kernel.CheckEvidence(evidence);',1).replace('        internal XdmValue Evaluate(EvaluationContext context)','        internal void ValidateAccessForCarrier() => Check();\n        internal XdmValue Evaluate(EvaluationContext context)',1)
 assert (P/'evidence/SchemaQuerySession-test-accessors.cs.txt').read_bytes()==expected.encode('utf-8')
print('PASS: four native/kernel runs, 103/109 checks, 14 VM expressions, seven exact bindings, two legacy20/0 replays, five clean builds; isolated scope only.')
