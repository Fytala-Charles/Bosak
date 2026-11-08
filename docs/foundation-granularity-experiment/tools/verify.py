"""Data-only verification of bounded E-03 comparisons and exact V4 Core surface reduction."""
from pathlib import Path
import hashlib,io,json,statistics,zipfile
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
def read(f):return json.loads((P/f).read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
i=read('evidence/INPUTS.json');r=read('RESULTS.json');delta=read('SURFACE-DELTA.json')
assert sha((ROOT/i['draftPath']).read_bytes())==i['draftSha256']
assert sha((ROOT/i['parentDraftPath']).read_bytes())==i['parentDraftSha256']
feed=ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip';assert sha(feed.read_bytes())==i['feedSha256']
with zipfile.ZipFile(feed) as z:pkg=z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
with zipfile.ZipFile(io.BytesIO(pkg)) as z:assert sha(z.read('lib/net10.0/Bosak.XPath.Core.dll'))==i['coreSha256']
parent=read('../foundation-advisor-contract/evidence/SURFACE.json');surface=read('evidence/CORE-SURFACE.json');old={x['name']:x for x in parent};new={x['name']:x for x in surface}
assert len(old)==35 and len(new)==33
assert sorted(set(old)-set(new))==delta['removedFromCoreCandidate']==['C13901.Draft.IQueryOperationLease','C13901.Draft.IQueryOperationLifetime']
assert all(new[n]==old[n] for n in new)
parent_source=(ROOT/i['parentDraftPath']).read_text(encoding='utf-8');start=parent_source.index('/// <summary>Optional host/provider operation admission;');end=parent_source.index('/// <summary>Proposed atomic channel;',start)
expected=(parent_source[:start]+parent_source[end:]).replace('// Generated V3 isolated','// Generated V4 Core-candidate isolated',1)
assert (ROOT/i['draftPath']).read_text(encoding='utf-8')==expected
runs=[read(f'evidence/run-{n}.json') for n in range(3)]
for n,d in enumerate(runs):
    assert d['checkCount']==len(d['checks'])==38 and all(c['passed'] for c in d['checks'])
    assert [c['id'] for c in d['checks']]==r['checkIds']
    assert d['coreCandidateSurface']==surface and d['coreSha256']==i['coreSha256']
    assert d['pointerSize']==8 and d['allRootsReferenceProjectionBytes']==65536*65536*8
    assert (P/f'evidence/run-{n}.stderr.txt').read_bytes()==b''
    assert len(d['groups'])==33
    modes={'scalar-full','bulk-cold-full','bulk-cached-full','cursor-preflight-full','cursor-certified-full','cursor-certified-prefix-1','cursor-certified-prefix-8','lookup-scalar-diverse','lookup-batch-diverse','lookup-scalar-repeated','lookup-batch-repeated'}
    assert {(g['size'],g['mode']) for g in d['groups']}=={(size,mode) for size in [64,4096,65536] for mode in modes}
    for g in d['groups']:
        size=g['size'];mode=g['mode'];assert size in [64,4096,65536] and len(g['samples'])==7
        count=size;found=size;complete=True;total=size*(size+1)//2
        if mode.startswith('lookup'):
            calls=1 if 'batch' in mode else size;nodes=0;found=size*7//8
            missing=size//8;total=126*(size//16) if mode.endswith('repeated') else total-(4*missing*(missing-1)+missing)
        elif mode.startswith('cursor'):
            count=1 if mode.endswith('prefix-1') else 8 if mode.endswith('prefix-8') else size
            calls=1+(count+63)//64;nodes=count+(size if 'preflight' in mode else 0);found=count;complete=count==size
            total=1 if count==1 else 102 if count==8 else total
        else:calls=size if mode=='scalar-full' else 1;nodes=0 if mode=='bulk-cached-full' else size
        for s in g['samples']:
            assert s['calls']==calls and s['nodeReads']==nodes
            assert s['result']=={'Sum':total,'Count':count,'Complete':complete,'Found':found}
            assert s['allocatedBytes']>=0 and s['nanoseconds']>=0
            if mode=='bulk-cached-full':assert s['allocatedBytes']==0
    assert all(p['nodeReads']==p['size'] and p['calls']==1 for p in d['preparations'])
for row in r['rows']:
    groups=[next(g for g in d['groups'] if g['size']==row['size'] and g['mode']==row['mode']) for d in runs]
    assert row['allocatedBytes']==sorted({s['allocatedBytes'] for g in groups for s in g['samples']})
    assert row['providerCalls']==sorted({s['calls'] for g in groups for s in g['samples']})
    assert row['nodeReads']==sorted({s['nodeReads'] for g in groups for s in g['samples']})
    assert row['perProcessMedianNanoseconds']==[statistics.median(s['nanoseconds'] for s in g['samples']) for g in groups]
replay=read('evidence/protocol-replay.json');assert replay['checkCount']==54 and all(c['passed'] for c in replay['checks']) and replay['surface']==surface
assert (P/'evidence/protocol-replay.stderr.txt').read_bytes()==b''
for name in ['build.txt','protocol-build.txt']:
    text=(P/'evidence'/name).read_text(encoding='utf-8');assert 'Build succeeded.' in text and '0 Warning(s)' in text and '0 Error(s)' in text
assert r['apiAccepted'] is r['productionModified'] is r['performanceBudgetsAccepted'] is r['E03CompleteForRealProviders'] is False
if (P/'MANIFEST.json').exists():
    for x in read('MANIFEST.json')['files']:assert sha((P/x['path']).read_bytes())==x['sha256'],x['path']
print('PASS: 38 repeated controls, 99 size/mode groups, 54 old protocol controls; dispatch/allocation/completeness checks; exact 35-to-33 Core candidate reduction, no acceptance inferred.')
