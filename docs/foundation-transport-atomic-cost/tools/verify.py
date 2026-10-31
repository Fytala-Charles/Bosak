"""Read-only verification of the isolated atomic/cost candidate and historical receipts."""
from pathlib import Path
import json,hashlib,zipfile,statistics,difflib,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
pins={'runtimeArchiveSha256':'docs/foundation-native-kernel-carrier/evidence/runtime-source.zip','coreArchiveSha256':'docs/foundation-carrier-equality/evidence/core-source.zip','productArchiveSha256':'docs/foundation-carrier-equality/evidence/product-dlls.zip','nativeArchiveSha256':'docs/foundation-native-adapter/evidence/native-source.zip','parentCarrierSha256':'docs/foundation-carrier-field-equality/evidence/field-selective-XdmValue.cs.txt','controlProbeSha256':'docs/foundation-native-kernel-carrier/tools/Probe.cs.txt','controlRuntimeSha256':'docs/foundation-native-kernel-carrier/evidence/SchemaQuerySession-test-accessors.cs.txt'}
prior=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text())
with zipfile.ZipFile(ROOT/pins['productArchiveSha256']) as z:product={n:sha(z.read(n)) for n in z.namelist() if n.endswith('.dll')}
expectedFailures={f'{name}-{operation}' for name in ['double-zero-sign','float-zero-sign','nan-bits','duration-lexical','qname-prefix','dateTime-offset'] for operation in ['rebind','conversion']}
data={};baseline=None
for phase,total,bad in [('red',363,True),('initial-before',373,True),('initial-fast-candidate',373,False),('before',376,True),('final',376,False)]:
 E=P/'evidence'/phase;i=json.loads((E/'INPUTS.json').read_text());data[phase]={}
 for key,path in pins.items():assert sha((ROOT/path).read_bytes())==i[key],(phase,key)
 source=P/'candidate' if phase=='final' else E/'candidate' if phase=='initial-fast-candidate' else ROOT/'docs/foundation-private-transport/evidence'
 for name,h in i['candidateSources'].items():assert sha((source/name).read_bytes())==h,(phase,name)
 host=P/'tools/AtomicProbe.cs.txt' if phase in {'before','final'} else E/'AtomicProbe.cs.txt'
 assert sha(host.read_bytes())==i['probeSha256']
 if '--check-current' in sys.argv:assert sha((ROOT/'tools/run-retained-foundation-caller.py').read_bytes())==i['checkedRunnerSha256']
 assert i['productionModified'] is False and len(i['variants'])==2
 builds=list(E.glob('*-build.txt'));assert len(builds)==7
 for f in builds:
  s=f.read_text();assert '0 Warning(s)' in s and '0 Error(s)' in s
 for v in i['variants']:
  mode=v['mode'];layout=v['layout'];assert len(layout)==7
  assert {n for n in layout if layout[n]!=product[n]}==({'Bosak.XPath.Runtime.dll'} if mode=='control' else {'Bosak.XPath.Core.dll','Bosak.XPath.Runtime.dll'})
  old=json.loads((E/f'{mode}-legacy.json').read_text())
  assert old['passed']==20 and old['failed']==0 and old['cases']==prior['cases'] and old['callerSha256']==prior['callerSha256']
  assert {a['name']+'.dll':a['sha256'] for a in old['loadedAssemblies']}==layout
  data[phase][mode]=[];previous=None
  for j in range(2):
   native=json.loads((E/f'round-{j}-{mode}.json').read_text())
   assert native['checkCount']==len(native['checks'])==(121 if mode=='carrier' else 103) and all(c['passed'] for c in native['checks']) and len(native['observations'])==14
   r=json.loads((E/f'atomic-{j}-{mode}.json').read_text());data[phase][mode].append(r)
   for bound in [native,r]:
    assert {a['name']+'.dll':a['sha256'] for a in bound['bindings']}==layout
    assert all(a['context']=='Default' and not a['collectible'] for a in bound['bindings'])
   assert r['fixtureCount']==39 and r['kindCount']==11 and r['xdmValueSize']==48
   assert r['checkCount']==len(r['checks'])==len({c['id'] for c in r['checks']})==total
   assert {c['id'] for c in r['checks'] if not c['passed']}==(expectedFailures if bad and mode=='carrier' else set())
   assert len(r['observations'])==39 and len(r['changesObserved'])==6
   plain=[(o['id'],o['kind'],o['payload'],[v['payload'] for v in o['items']]) for o in r['observations']]
   if baseline is None:baseline=plain
   assert plain==baseline
   assert all(len(o['items'])==3 and all(v['origin']==(mode=='carrier') for v in o['items']) for o in r['observations'])
   stable=(r['checks'],r['observations'],r['changesObserved'])
   if previous is not None:assert stable==previous
   previous=stable
   assert len(r['costs'])==30
   for g in r['costs']:
    assert g['count'] in {1000,20000} and len(g['samples'])==7
    expected=g['count']*(5 if g['kind']=='string' else 42)
    assert all(s['checksum']==expected and s['allocatedBytes']>=0 and s['nanoseconds']>0 for s in g['samples'])
assert json.loads((P/'evidence/before/INPUTS.json').read_text())['probeSha256']==json.loads((P/'evidence/final/INPUTS.json').read_text())['probeSha256']
results=json.loads((P/'RESULTS.json').read_text())
for phase in ['before','final']:
 for mode in ['control','carrier']:
  for key,saved in results['costs'][phase][mode].items():
   kind,op,n=key.split('|');n=int(n)
   samples=[s for r in data[phase][mode] for g in r['costs'] if (g['kind'],g['operation'],g['count'])==(kind,op,n) for s in g['samples']]
   assert len(samples)==saved['samples']==14
   for suffix,metric in [('BytesPerOp','allocatedBytes'),('NsPerOp','nanoseconds')]:
    vals=[s[metric]/n for s in samples]
    assert saved['median'+suffix]==statistics.median(vals) and saved['min'+suffix]==min(vals) and saved['max'+suffix]==max(vals)
   assert saved['gcEvents']=={f'gen{k}':sum(s[f'gen{k}'] for s in samples) for k in range(3)}
for kind in ['integer','string','decimal']:
 for op in ['payload-access','cached-kernel-read','issued-value-read','evidence-read','exact-noop']:
  before={'payload-access':0,'cached-kernel-read':176,'issued-value-read':176 if kind=='integer' else 208,'evidence-read':64 if kind=='decimal' else 0,'exact-noop':32 if kind=='integer' else 64 if kind=='string' else 192}[op]
  after={'payload-access':0,'cached-kernel-read':0,'issued-value-read':0 if kind=='integer' else 32,'evidence-read':0,'exact-noop':32 if kind=='integer' else 64}[op]
  for phase,wanted in [('before',before),('final',after)]:
   for n in [1000,20000]:assert results['costs'][phase]['carrier'][f'{kind}|{op}|{n}']['medianBytesPerOp']==wanted
diff=[]
for name in ['XdmValue.cs.txt','SchemaEvidenceKernel.cs.txt','SchemaQuerySession.cs.txt']:
 a=ROOT/'docs/foundation-private-transport/evidence'/name;b=P/'candidate'/name
 diff.extend(difflib.unified_diff(a.read_text(encoding='utf-8').splitlines(True),b.read_text(encoding='utf-8').splitlines(True),fromfile='private-transport/'+name,tofile='corrected/'+name))
assert (P/'evidence/SOURCE-DIFF.txt').read_text(encoding='utf-8')==''.join(diff)
assert (P/'candidate/SchemaQuerySession.cs.txt').read_bytes()==(ROOT/'docs/foundation-private-transport/evidence/SchemaQuerySession.cs.txt').read_bytes()
kernel=(P/'candidate/SchemaEvidenceKernel.cs.txt').read_text();assert 'internal ref struct ReadLease' in kernel and 'var lease = snapshot.EnterRead();' in kernel
assert not results['productionModified'] and not results['budgetAccepted'] and not results['publicApiAccepted']
print('PASS: 39 fixtures/11 factory kinds; final376 checks x4, native121/103 x2, exact seven DLL bindings, legacy20/0; historical12 failures, lease controls and allocation/timing distributions verified.')
