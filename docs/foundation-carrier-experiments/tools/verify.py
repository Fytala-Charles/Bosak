"""Data-only verification of sealed, bounded carrier experiment receipts."""
from pathlib import Path
import hashlib,json,zipfile,statistics

PACKET=Path(__file__).resolve().parents[1]
def read(path): return json.loads((PACKET/path).read_text(encoding='utf-8-sig'))
def sha(data): return hashlib.sha256(data).hexdigest()
inputs=read('evidence/INPUTS.json')
assert inputs['sourceRevision']=='c88eea4308c0e49a81e7bfe2f481319f2e4d208c'
assert sha((PACKET/'evidence/core-source.zip').read_bytes())==inputs['coreSourceArchiveSha256']
for mode in ['baseline','naive','overlay','wrapper']:
    log=(PACKET/f'evidence/{mode}-build.txt').read_text(encoding='utf-8')
    assert 'Build succeeded.' in log and '0 Warning(s)' in log and '0 Error(s)' in log
runs=[]
for round_index in range(2):
    for mode in ['baseline','naive','overlay','wrapper']:
        for size in ([1000] if mode=='naive' else [1000,100000]):
            d=read(f'evidence/round-{round_index}-{mode}-{size}.json');runs.append(d)
            assert d['mode']==mode and d['items']==size
            assert d['pointerSize']==8 and d['xdmValueSize']==48 and d['pairSize']==56
            assert len(d['checks'])==d['controlCount']==(21 if mode=='naive' else 49)
            assert len({c['id'] for c in d['checks']})==d['controlCount']
            assert all(c['observed']==c['expected'] for c in d['checks'])
            assert d['intentionalNaiveAccessorFailures']==(4 if mode=='naive' else 0)
            assert len(d['measurements'])==(0 if mode=='naive' else 4)
            for m in d['measurements']:
                assert len(m['samples'])==7
                assert m['checksum']==(42 if m['id']=='copy-create' else size*10*(42 if m['id']=='integer-access' else 5))
                assert all(s['elapsedNanoseconds']>=0 and s['allocatedBytes']>=0 and all(s[k]>=0 for k in ('gen0','gen1','gen2')) for s in m['samples'])
                if m['id']=='copy-create':
                    per={'baseline':48,'overlay':80,'wrapper':56}[mode]
                    assert min(s['allocatedBytes'] for s in m['samples'])==size*per+24
assert len(runs)==14
assert len({c['id'] for d in runs for c in d['checks']})==49
r=read('RESULTS.json')
assert len(r['rows'])==7
for row in r['rows']:
    selected=[read(f"evidence/round-{i}-{row['mode']}-{row['items']}.json") for i in range(2)]
    for index,m in enumerate(row['measurements']):
        assert m['id']==selected[0]['measurements'][index]['id']
        assert m['perProcessMedianNanoseconds']==[statistics.median(x['elapsedNanoseconds'] for x in d['measurements'][index]['samples']) for d in selected]
        assert m['allocatedBytesObserved']==sorted({x['allocatedBytes'] for d in selected for x in d['measurements'][index]['samples']})

assert r['carrierSelected'] is None and r['productionModified'] is r['apiFrozen'] is r['performanceBudgetsAccepted'] is False
if (PACKET/'MANIFEST.json').exists():
    for item in read('MANIFEST.json')['files']:
        assert sha((PACKET/item['path']).read_bytes())==item['sha256'],item['path']
print('PASS: four cohorts, 14 process/size runs, 49 reused control IDs, intentional four-accessor bad variant, layout/allocation records and pending acceptance.')
