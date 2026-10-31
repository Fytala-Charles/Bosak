"""Read-only checks of the sealed isolated private transport trial."""
from pathlib import Path
import hashlib,json,zipfile,difflib,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
i=json.loads((P/'evidence/INPUTS.json').read_text())
pins={'runtimeArchiveSha256':'docs/foundation-native-kernel-carrier/evidence/runtime-source.zip','coreArchiveSha256':'docs/foundation-carrier-equality/evidence/core-source.zip','productArchiveSha256':'docs/foundation-carrier-equality/evidence/product-dlls.zip','nativeArchiveSha256':'docs/foundation-native-adapter/evidence/native-source.zip','parentCarrierSha256':'docs/foundation-carrier-field-equality/evidence/field-selective-XdmValue.cs.txt','controlProbeSha256':'docs/foundation-native-kernel-carrier/tools/Probe.cs.txt','controlRuntimeSha256':'docs/foundation-native-kernel-carrier/evidence/SchemaQuerySession-test-accessors.cs.txt'}
for key,path in pins.items():assert sha((ROOT/path).read_bytes())==i[key],key
assert sha((P/'tools/Probe.cs.txt').read_bytes())==i['probeSha256']
for n,h in i['candidateSources'].items():assert sha((P/'evidence'/n).read_bytes())==h
if '--check-current' in sys.argv:assert sha((ROOT/'tools/run-retained-foundation-caller.py').read_bytes())==i['checkedRunnerSha256']
assert i['productionModified'] is False and i['friendHostIdentity']=='Bosak.XPath.Runtime.Tests'
with zipfile.ZipFile(ROOT/pins['productArchiveSha256']) as z:product={n:sha(z.read(n)) for n in z.namelist() if n.endswith('.dll')}
prior=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text())
baseline=None
assert {v['mode'] for v in i['variants']}=={'control','carrier'}
for v in i['variants']:
 mode=v['mode'];layout=v['layout'];assert len(layout)==7
 assert {n for n in layout if layout[n]!=product[n]}==({'Bosak.XPath.Runtime.dll'} if mode=='control' else {'Bosak.XPath.Core.dll','Bosak.XPath.Runtime.dll'})
 old=json.loads((P/f'evidence/{mode}-legacy.json').read_text())
 assert old['passed']==20 and old['failed']==0 and old['cases']==prior['cases'] and old['callerSha256']==prior['callerSha256']
 assert {a['name']+'.dll':a['sha256'] for a in old['loadedAssemblies']}==layout
 assert {a['name']+'.dll':a['sha256'] for a in json.loads((P/f'evidence/{mode}-qualification.json').read_text())['assemblies']}==layout
 previous=None
 for j in range(2):
  r=json.loads((P/f'evidence/round-{j}-{mode}.json').read_text())
  assert r['mode']==mode and r['checkCount']==len(r['checks'])==(103 if mode=='control' else 121)
  assert all(c['passed'] for c in r['checks']) and len({c['id'] for c in r['checks']})==r['checkCount']
  assert {a['name']+'.dll':a['sha256'] for a in r['bindings']}==layout
  assert all(a['context']=='Default' and not a['collectible'] for a in r['bindings'])
  assert len(r['observations'])==14
  plain=[(o['id'],o['expr'],[(e['value'],e['kind']) for e in o['entries']]) for o in r['observations']]
  if baseline is None:baseline=plain
  assert plain==baseline
  for o in r['observations']:
   expected=mode=='carrier' and o['id'] not in {'sum','arithmetic','integer-raw'}
   assert all(e['origin']==expected for e in o['entries'])
  stable=(r['checks'],r['observations'])
  if previous is not None:assert stable==previous
  previous=stable
builds=list(P.glob('evidence/*-build.txt'));assert len(builds)==5
for f in builds:
 s=f.read_text();assert '0 Warning(s)' in s and '0 Error(s)' in s
for n in ['attempt-1','attempt-2']:
 assert 'Build FAILED.' in (P/f'evidence/{n}/carrier-host-build.txt').read_text()
 assert not list((P/f'evidence/{n}').glob('round-*.json'))
initial=P/'evidence/initial-success'
assert sha((initial/'Probe.cs.txt').read_bytes())==json.loads((initial/'INPUTS.json').read_text())['probeSha256']
for f in initial.glob('round-*-carrier.json'):
 r=json.loads(f.read_text());assert r['checkCount']==118 and all(c['passed'] for c in r['checks'])
diff=[]
with zipfile.ZipFile(ROOT/pins['coreArchiveSha256']) as c,zipfile.ZipFile(ROOT/pins['runtimeArchiveSha256']) as r:
 for name,old in [('XdmValue.cs.txt',(ROOT/pins['parentCarrierSha256']).read_text(encoding='utf-8')),('SchemaEvidenceKernel.cs.txt',c.read('src/Bosak.XPath.Core/Xdm/SchemaEvidenceKernel.cs').decode('utf-8-sig')),('SchemaQuerySession.cs.txt',r.read('src/Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs').decode('utf-8-sig'))]:
  diff.extend(difflib.unified_diff(old.splitlines(True),(P/'evidence'/name).read_text(encoding='utf-8').splitlines(True),fromfile='baseline/'+name,tofile='candidate/'+name))
assert (P/'evidence/SOURCE-DIFF.txt').read_text(encoding='utf-8')==''.join(diff)
host=(P/'tools/Probe.cs.txt').read_text();assert 'Bridge.' not in host and 'sealed record Frame' not in host
runtime=(P/'evidence/SchemaQuerySession.cs.txt').read_text();assert 'ValidateEvidenceForCarrier' not in runtime and 'ValidateAccessForCarrier' not in runtime
assert 'ReadValue(ISchemaIssuedScalar value)' in runtime and 'AdmitConversionValue(XdmValue source' in runtime
print('PASS: candidate121/control103 checks twice, 14 expressions, seven exact bindings, two legacy20/0 replays, five clean final builds; historical failures and source diffs verified.')
