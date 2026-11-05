"""Verify inspected owner response evidence and preserve the open capture gap."""
from pathlib import Path
import hashlib,json,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
sys.path.insert(0,str(R/'docs/foundation-r2-owner-review/tools'))
from returns import validate
i=json.loads((P/'INPUTS.json').read_text());r=json.loads((P/'inputs/O3-Braid.response.json').read_text());issue=json.loads((P/'inputs/issue-468.json').read_text());s=json.loads((P/'STATUS.json').read_text())
assert sha(P/'inputs/issue-468.json')==i['issueSha256'] and sha(P/'inputs/O3-Braid.response.json')==i['braidResponseSha256'] and not i['productionChanged']
assert validate(r) and r['returnId']=='O3-Braid' and r['decision']=='Revise' and len(r['unresolvedRequiredSignatureGaps'])==1 and not r['runtimeQualification'] and not r['c45Accepted'] and not r['apiFrozen']
assert issue['number']==468 and issue['url']=='https://github.com/Fytala-Charles/Bosak/issues/468'
latest=max(issue['comments'],key=lambda x:x['createdAt']);assert latest['url'].endswith('6562337494') and 'registered' in latest['body'] and 'No acceptance is implied' in latest['body']
assert s['submittedRoleReturns']==3 and s['scopedCoreDesignReturns']==2 and s['missingRoleReturns']==['O1-Schema','O1-XmlDb','O2-Schema','O4-XmlDb'] and s['openRevisionRequests']==['BCAP-01'] and not s['r2Complete'] and not s['braidAccepted']
for name,digest in i['sourceFiles'].items():assert sha(R/name)==digest,name
assert sha(Path(i['braidExecutorPath']))==i['braidExecutorSha256']
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified issue registration, draft-matched Braid Revise, BCAP-01, four missing role returns and unchanged production; R2 remains open.')
