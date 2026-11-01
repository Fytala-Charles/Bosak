"""Verify candidate/legacy observations, exact cohort pins and unchanged public metadata."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());r=json.loads((E/'RESULTS.json').read_text())
assert not i['canonicalChanges'] and not i['productionAccepted']
assert r['legacyCases']==7 and r['candidateCases']==r['candidatePassed']==22 and r['legacyIdentical'] and r['repeatIdentical']
assert sha(P/'tools/Probe.cs.txt')==r['sourceSha256']
for mode in ['legacy','candidate']:assert sha(ROOT/'.guard-tmp/xslt-explicit-context/Run'/mode/'Probe.dll')==r['probeSha256']
assert sha(Path(i['candidatePath']))==i['candidateSha256']
engines={Path(v['path']).stem:v for v in i['qualifiedEngines'].values()}
for v in engines.values():assert sha(Path(v['path']))==v['sha256']
for name,digest in i['sourceFiles'].items():assert sha(ROOT/'src/Bosak.Xslt'/name)==digest
for name,digest in i['patchedFiles'].items():assert sha(ROOT/'.guard-tmp/xslt-explicit-context/src/Bosak.Xslt'/name)==digest
receipts={}
for mode,count in [('legacy',7),('candidate',22)]:
 a=json.loads((E/(mode+'-0.json')).read_text());assert a==json.loads((E/(mode+'-1.json')).read_text())
 assert len(a['cases'])==count and len({c['id'] for c in a['cases']})==count and all(c['passed'] for c in a['cases'])
 assert a['bridgeAvailable']==(mode=='candidate')
 for b in a['bindings']:assert b['sha256']==(i['candidateSha256'] if mode=='candidate' and b['name']=='Bosak.Xslt' else engines[b['name']]['sha256'])
 receipts[mode]=a
assert receipts['candidate']['cases'][:7]==receipts['legacy']['cases']
c=json.loads((E/'API-COMPARISON.json').read_text());assert c['declarationsIdentical'] and c['publicTypes']==157 and c['declarationRows']==2961 and c['newPublicExports']==0
assert sha(ROOT/c['baselineSource'])==c['baselineSha256']
assert sha(ROOT/'docs/foundation-c45-revised-shape/tools/Inventory.cs.txt')==c['inventoryToolSha256']
old=next(a for a in json.loads((ROOT/c['baselineSource']).read_text())['assemblies'] if a['assembly']=='Bosak.Xslt')
new=json.loads((E/'candidate-api.json').read_text())['assemblies'][0]
assert old['declarations']==new['declarations'] and new['sha256'].lower()==i['candidateSha256']
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified repeated 7 legacy/22 candidate cases, exact loaded cohorts, source pins and unchanged 157-type/2961-row public metadata.')
