"""Check bounded VM experiment claims against raw results and actual assembly bindings."""
from pathlib import Path
import hashlib,json,statistics,subprocess
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
def read(f):return json.loads((P/f).read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def payload(o):return (o['error'],[(v['kind'],v['value']) for v in o['items']])
i=read('evidence/INPUTS.json');r=read('RESULTS.json')
assert sha((ROOT/i['priorSourceArchive']).read_bytes())==i['sourceArchiveSha256']
assert sha((ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip').read_bytes())==i['feedSha256']
assert len(i['packageDlls'])==7
layouts={v['mode']:v['layout'] for v in i['variants']}
base={o['id']:o for o in read('evidence/round-0-baseline.json')['observations']}
assert len(base)==27
assert {k:v['error'] for k,v in base.items() if v['error']}=={'no-typed-value':'FOTY0012','invalid-cast':'FORG0001','cardinality-error':'FORG0005'}
for mode in ['baseline','holder','selective']:
    for name,h in layouts[mode].items():
        if mode=='baseline' or name!='Bosak.XPath.Core.dll':assert h==i['packageDlls'][name]
    for round_index in range(2):
        d=read(f'evidence/round-{round_index}-{mode}.json')
        assert len(d['observations'])==27 and d['xdmValueSize']==48
        assert d['clrEqualsToPlainInteger']==(mode=='baseline')
        assert d['xpathEqualsToPlainInteger'] and d['xpathHashToPlainInteger']
        assert (P/f'evidence/round-{round_index}-{mode}.stderr.txt').read_bytes()==b''
        assert {b['name']+'.dll' for b in d['bindings']}==set(layouts[mode])
        for b in d['bindings']:
            assert b['sha256']==layouts[mode][b['name']+'.dll']
            assert b['context']=='Default' and b['collectible'] is False
            assert Path(b['path']).parent.resolve()==(ROOT/'.guard-tmp/vm-carrier-probe'/mode/'Runner/bin/Release/net10.0').resolve()
        for o in d['observations']:assert payload(o)==payload(base[o['id']])
        observed={o['id']:o for o in d['observations']}
        expected='A' if mode!='baseline' else None
        for name in ['identity','identity-string','let','filter','simple-map','array','map-value','map-nested','function','data-atomic','data-node','distinct']:
            assert observed[name]['items'][0]['origin']==expected
        assert [x['origin'] for x in observed['data-sequence-node']['items']]==(['A','B'] if mode!='baseline' else [None,None])
        assert all(v['origin'] is None for name in ['cast-identity','cast-decimal','arithmetic','comparison','string-conversion','upper-case','string-join','serialization','data-untyped-node'] for v in observed[name]['items'])
        for a in d['allocations']:
            assert len(a['samples'])==7
            per=80 if mode=='holder' or (mode=='selective' and a['kind']=='string') else 48
            assert min(s['allocatedBytes'] for s in a['samples'])==a['items']*per+24
            assert all(s['count']==a['items'] for s in a['samples'])
for row in r['observations']:
    assert row['payloadAndErrorMatchBaseline'] is True
    for mode in ['baseline','holder','selective']:
        assert row['variants'][mode]==next(o for o in read(f'evidence/round-0-{mode}.json')['observations'] if o['id']==row['id'])
for row in r['allocationSummary']:
    ds=[read(f"evidence/round-{n}-{row['mode']}.json") for n in range(2)]
    samples=[next(a for a in d['allocations'] if a['kind']==row['kind'] and a['items']==row['items'])['samples'] for d in ds]
    assert row['allocatedBytes']==sorted({s['allocatedBytes'] for block in samples for s in block})
    assert row['perProcessMedianNanoseconds']==[statistics.median(s['nanoseconds'] for s in block) for block in samples]
for source in read('evidence/SOURCE-INPUTS.json')['files']:
    assert sha(subprocess.check_output(['git','-C',str(ROOT),'show',source['revision']+':'+source['path']]))==source['sha256']
assert r['carrierSelected'] is None and r['apiFrozen'] is r['performanceBudgetsAccepted'] is r['fullE02Completed'] is False
if (P/'MANIFEST.json').exists():
    for x in read('MANIFEST.json')['files']:assert sha((P/x['path']).read_bytes())==x['sha256'],x['path']
print('PASS: six runs / 27 repeated VM cases; exact donor/modified-Core binding, typed/untyped controls, allocation/equality trade-offs; no acceptance inferred.')
