"""Verify exact source pins, prior-shape retention and compiled dependency direction."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());a=json.loads((E/'api-0.json').read_text())
assert a==json.loads((E/'api-1.json').read_text())
assert len(a['assemblies'])==4 and sum(x['publicTypes'] for x in a['assemblies'])==45
assert sum(x['declarationCount'] for x in a['assemblies'])==615
assert not i['proposalBodiesExecuted'] and i['productionExportsAdded']==0
for name,digest in i['declarationSources'].items():assert sha(P/'proposal'/name)==digest
for v in i['qualifiedEngines'].values():assert sha(Path(v['path']))==v['sha256']
assert sha(P/'tools/Inventory.cs.txt')==i['inventorySourceSha256']
assert sha(ROOT/'docs/foundation-c45-delta136/tools/Inventory.cs.txt')==i['inventoryBaseSourceSha256']
for name in ['Provider.cs.txt','OperationLifetime.cs.txt']:assert (P/'proposal'/name).read_bytes()==(ROOT/'docs/foundation-c45-delta136/proposal'/name).read_bytes()
old=json.loads((ROOT/'docs/foundation-c45-delta136/evidence/proposed-api.json').read_text())
now={x['assembly']:x for x in a['assemblies']}
for assembly in old['assemblies']:
 before=set(assembly['declarations']);after=set(now[assembly['assembly']]['declarations'])
 removed=before-after
 if assembly['assembly']=='Bosak.XPath.Core.C45Proposal':assert not removed
 else:
  assert len(removed)==2 and all('SchemaQueryLimits..ctor' in row for row in removed)
 for row in before:
  if row.startswith('T '):assert row in after
for assembly in a['assemblies']:
 name=assembly['assembly'];graph=i['dependencyGraph'][name]
 actual={r for r in assembly['references'] if r.startswith('Bosak.')}
 assert actual<=set(graph['proposalReferences']+graph['engineReferences'])
 if name=='Bosak.XPath.Core.C45Proposal':assert actual=={'Bosak.XPath.Core'}
 if name=='Bosak.XPath.Api.C45Proposal':assert not any('XQuery' in r or 'Xslt' in r for r in actual)
ledger=json.loads((P/'NUMERIC-MEMBERS.json').read_text());assert len(ledger['members'])==6 and not ledger['productionMembersAdded']
witness=(P/'proposal/ExactNumeric.cs.txt').read_text().split('internal interface IXdmValueNumericMemberWitness')[1]
assert witness.count('static abstract')==4 and witness.count('{ get; }')==2
cases=json.loads((P/'ACCEPTANCE.json').read_text());assert len(cases['cases'])==10 and all(c['status']=='NotRun' for c in cases['cases']) and not cases['accepted']
if (P/'MANIFEST.json').exists():
 manifest=json.loads((P/'MANIFEST.json').read_text())
 assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in manifest.items():assert sha(P/name)==digest,name
print('Verified 45 types/615 rows, prior provider/type retention, the sole proposed constructor replacement, compiled reference direction, six numeric witnesses and pending acceptance.')
