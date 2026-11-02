"""Verify retained revised dispositions; never infer native or human acceptance."""
from pathlib import Path
import json,hashlib,importlib.util
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('capture_response_validator',R/'docs/foundation-bcap-owner-review/tools/returns.py');validator=importlib.util.module_from_spec(spec);spec.loader.exec_module(validator)
i=json.loads((P/'INPUTS.json').read_text());s=json.loads((P/'STATUS.json').read_text())
assert i['proposalManifestSha256']==s['proposalManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json')
assert i['preparedPacketManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/MANIFEST.json')
for id,v in i['responses'].items():
 f=P/'inputs'/(id+'.json');assert sha(f)==v['sha256'];response=json.loads(f.read_text());assert validator.validate(response) and response['decision']==v['decision']
 assert not response['runtimeQualification'] and not response['c45Accepted'] and not response['apiFrozen'] and response['acceptedPerformanceBudgets'] is None
assert s['decisions']=={'BCAP-O3-Braid':'AcceptWithScope','BCAP-O1-XmlDb':'AcceptWithScope','BCAP-O4-XmlDb':'Revise'}
assert s['submitted']==3 and s['scopedDesignAccepts']==2 and s['revise']==1 and len(s['missing'])==4
x=P/'inputs/XmlDb-INPUTS.json';assert sha(x)==i['xmlDbReceiptSha256'];owner=json.loads(x.read_text());assert owner['workingSourceHashes']==i['xmlDbWorkingHashesVerified']
assert sha(P/'inputs/XmlDb-membership-results.json')==owner['workingSourceHashes']['docs/foundation-requirements/membership/results.json']
c=json.loads((P/'inputs/Schema-status-comment.json').read_text());assert c['id']==6570464706 and 'NotSubmitted' in c['body'] and 'not accepted' in c['body']
assert not any(s[k] for k in ['r2Closed','apiFrozen','humanSignaturesAuthenticated','nativeConsumersQualified','productionChanged'])
if (P/'MANIFEST.json').exists():
 for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/n)==h,n
print('Verified three revised returns: Braid/XmlDb O1 scoped design accepts; XmlDb O4 Revise; Schema/Core four returns missing. No native/freeze acceptance.')
