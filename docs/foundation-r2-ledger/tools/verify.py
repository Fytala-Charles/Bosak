"""Verify complete Core disposition coverage and isolated declaration evidence."""
from pathlib import Path
import json,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';B=R/'docs/foundation-c45-revised-shape';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
l=json.loads((P/'LEDGER.json').read_text());i=json.loads((E/'INPUTS.json').read_text());a=json.loads((E/'api-0.json').read_text());old=json.loads((B/'evidence/api-0.json').read_text())
assert l['coreDecisionRecorded'] and not l['ownerAccepted'] and not l['r2Complete'] and not l['proposalBodiesExecuted']
assert a==json.loads((E/'api-1.json').read_text()) and len(a['assemblies'])==4 and sum(x['publicTypes'] for x in a['assemblies'])==45
types=lambda api:{d[2:].split(':',1)[0] for x in api['assemblies'] for d in x['declarations'] if d.startswith('T ')}
assert types(a)==types(old)=={x['type'] for x in l['types']} and len(l['types'])==45
rows=lambda api:{(x['assembly'],d) for x in api['assemblies'] for d in x['declarations']}
assert {(x['assembly'],x['baselineRow']) for x in l['baselineMetadataRows']}==rows(old) and len(l['baselineMetadataRows'])==615
assert len(rows(a))==619 and len(rows(a)-rows(old))==8 and len(rows(old)-rows(a))==4
added=rows(a)-rows(old);removed=rows(old)-rows(a)
methods=lambda rows:{d[2:].split(':',1)[0] for _,d in rows if d.startswith('M ')}
assert methods(added)=={'Bosak.XPath.Core.Schema.ISchemaAssessmentInput.OwnsNode','Bosak.XPath.Core.Schema.ISchemaAssessmentAdvisor.OwnsNode','Bosak.XPath.Core.Schema.ISchemaAssessmentAdvisor.OwnsTypedValue','Bosak.XPath.Api.Schema.SchemaQueryOperation.ReadTypedValue'}
assert methods(removed)=={'Bosak.XPath.Core.Schema.ISchemaAssessmentAdvisor.OwnsTypedValue','Bosak.XPath.Api.Schema.SchemaQueryOperation.ReadTypedValue'}
delta=json.loads((E/'DELTA.json').read_text());assert {(x['assembly'],x['row']) for x in delta['added']}==added and {(x['assembly'],x['row']) for x in delta['removed']}==removed and not delta['productionChanged']
assert sum(x['coreDisposition']=='Revise' for x in l['types'])==3 and all(not x['ownerAccepted'] and not x['productionImplemented'] for x in l['types'])
assert [x['member'] for x in l['numericMembers']]==json.loads((B/'NUMERIC-MEMBERS.json').read_text())['members'] and len(l['numericMembers'])==6
requirements=json.loads((R/'docs/foundation-c45-handoff136/MATRIX.json').read_text())['rows']
assert [x['baselineRequirement'] for x in l['requirements']]==requirements and len(l['requirements'])==43 and all(x['ownerAcceptance']=='Pending' for x in l['requirements'])
ids={x['id'] for x in l['decisions']};assert ids=={'D'+str(n).zfill(2) for n in range(1,15)}
for x in l['types']+l['requirements']:assert x['decisions'] and set(x['decisions'])<=ids
for name,digest in json.loads((P/'REVIEW-INPUTS.json').read_text()).items():assert sha(R/name)==digest,name
for name,digest in i['declarationSources'].items():assert sha(P/'proposal'/name)==digest,name
for v in i['qualifiedEngines'].values():assert sha(Path(v['path']))==v['sha256']
assert not i['proposalBodiesExecuted'] and i['productionExportsAdded']==0
for name in ['Bosak.XQuery.C45Proposal-build.txt','Bosak.Xslt.C45Proposal-build.txt']:assert '0 Warning(s)' in (E/name).read_text() and '0 Error(s)' in (E/name).read_text()
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 45 type dispositions, 615 baseline rows, six numeric intents, 43 requirements, 619 revised declaration rows and unchanged production; owner acceptance pending.')
