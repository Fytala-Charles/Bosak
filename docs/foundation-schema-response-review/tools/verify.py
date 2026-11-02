"""Verify Schema submissions and the current five-return ledger without upgrading acceptance."""
from pathlib import Path
import json,hashlib,importlib.util,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('schema_review_forms',R/'docs/foundation-bcap-owner-review/tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
i=json.loads((P/'INPUTS.json').read_text());s=json.loads((P/'STATUS.json').read_text());assert i['proposalManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json')
for id,row in i['responses'].items():
 f=P/'inputs'/(id+'.json');assert sha(f)==row['sha256'];data=json.loads(f.read_text());assert v.validate(data) and data['decision']==row['decision']
 assert not data['runtimeQualification'] and not data['c45Accepted'] and not data['apiFrozen']
c=json.loads((P/'inputs/Schema-submission-comment.json').read_text());assert c['id']==6573672774
published={r['returnId']:r for r in [json.loads(b) for b in re.findall(r'```json\s*(\{.*?\})\s*```',c['body'],re.S)]}
for id in ['BCAP-O1-Schema','BCAP-O2-Schema']:assert published[id]==json.loads((P/'inputs'/(id+'.json')).read_text()) and s['decisions'][id]=='Revise'
assert s['submitted']==5 and s['scopedDesignAccepts']==2 and s['revise']==3 and s['missing']==['BCAP-O2-Core','BCAP-O4-Core']
assert not any(s[k] for k in ['r2Closed','apiFrozen','nativeConsumersQualified','humanSignaturesAuthenticated','productionChanged'])
assert sha(R/'docs/foundation-bcap-owner-review/inputs/proposal/QueryFacade.cs.txt')==i['limitsDeclarationSha256']
if (P/'MANIFEST.json').exists():
 for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/n)==h,n
print('Verified five exact-bound returns: two scoped accepts/three revisions; only Core O2/O4 missing. Schema remains Revise.')
