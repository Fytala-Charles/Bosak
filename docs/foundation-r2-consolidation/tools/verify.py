"""Verify consolidated return decisions, owner source evidence and finite pending plan."""
from pathlib import Path
import hashlib,json,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
sys.path.insert(0,str(R/'docs/foundation-r2-owner-review/tools'))
from returns import validate
i=json.loads((P/'INPUTS.json').read_text());s=json.loads((P/'STATUS.json').read_text());plan=json.loads((P/'PLAN.json').read_text());cases=json.loads((P/'FOLLOWUP-CASES.json').read_text())
assert not i['productionChanged'] and s['productionVersion']=='1.42.0' and s['submittedRoleReturns']==5 and s['scopedDesignAccepts']==3 and s['revisionReturns']==2
assert s['missingRoleReturns']==['O1-Schema','O2-Schema'] and not any(s[k] for k in ['productionChanged','schemaGuidanceIsFormalReturn','r2Complete','r3Started','c45Accepted','apiFrozen','publicationAuthorized'])
decisions={}
for id,v in i['responses'].items():
 assert sha(P/v['copy'])==v['sha256'];response=json.loads((P/v['copy']).read_text());assert validate(response) and response['returnId']==id
 assert not response['runtimeQualification'] and not response['c45Accepted'] and not response['apiFrozen'];decisions[id]=response['decision']
assert decisions=={'O2-Core':'AcceptWithScope','O4-Core':'AcceptWithScope','O1-XmlDb':'AcceptWithScope','O3-Braid':'Revise','O4-XmlDb':'Revise'}
assert {x['id']:x['decision'] for x in s['rows'] if x['decision']!='Missing'}==decisions
braid=json.loads((P/'inputs/O3-Braid.json').read_text());xml=json.loads((P/'inputs/O4-XmlDb.json').read_text());assert len(braid['unresolvedRequiredSignatureGaps'])==1 and xml['unresolvedRequiredSignatureGaps']==[]
assert sha(P/'inputs/Schema-alignment-comment.json')==i['schemaCommentSha256'] and sha(P/'inputs/issue-468-comments.json')==i['issueCommentsSha256']
comment=json.loads((P/'inputs/Schema-alignment-comment.json').read_text());assert comment['id']==6563123351 and 'proposed work/acceptance checklist' in comment['body'] and 'not a claim' in comment['body']
seen=set()
for task in plan['tasks']:assert set(task['dependsOn'])<=seen and task['exitEvidence'];seen.add(task['id'])
assert len(seen)==10 and plan['nextCoreTask']=='P09' and {x['id'] for x in plan['openItems']}=={'BCAP-01','XDB-LIFE-01'}
assert next(t for t in plan['tasks'] if t['id']=='P03')['status']=='CompleteBoundedCoreFeasibility'
assert next(t for t in plan['tasks'] if t['id']=='P02')['status']=='CompleteIsolatedDeclaration'
assert next(t for t in plan['tasks'] if t['id']=='P08')['status']=='ClosedWithScope'
assert next(t for t in plan['tasks'] if t['id']=='P09')['status']=='InProgress'
subprocess.run(['python',str(R/'docs/foundation-r2-closure/tools/verify.py')],check=True,capture_output=True)
assert len(cases['cases'])==18 and len({x['id'] for x in cases['cases']})==18 and all(x['status']=='NotRun' for x in cases['cases'])
current=json.loads((R/'docs/foundation-node-facts-cost/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for name,digest in i['reviewInputs'].items():assert sha(R/name)==current['sources'].get(name,digest),name
audit=i['xmlDbSourceAudit'];assert sha(P/'inputs/XmlDb-owner-INPUTS.json')==audit['ownerReceiptSha256']
owner=json.loads((P/'inputs/XmlDb-owner-INPUTS.json').read_text());assert owner['sourceRevision']==audit['revision']
for name,v in audit['files'].items():
 assert owner['sourceHashes'][name]==v['ownerRecordedSha256'];data=subprocess.check_output(['git','show',audit['revision']+':'+name],cwd=audit['root']);assert hashlib.sha256(data).hexdigest()==v['committedBytesSha256']
for id in ['O1-XmlDb','O4-XmlDb']:assert sha(P/'inputs'/(id+'.json'))==owner['responses'][id+'.response.json']
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
current=json.loads((R/'docs/foundation-node-facts-cost/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==current['sources'].get(name,digest),name
engines=json.loads((R/'docs/foundation-xslt-source-membership/evidence/INPUTS.json').read_text())['engines']
for value in current['engines'].values():assert sha(Path(value['path']))==value['sha256']
subprocess.run(['python',str(R/'docs/foundation-node-facts-cost/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified historical response snapshot and pinned evidence, current task dependencies and typed-container overlay. Historical receipts remain unchanged.')
