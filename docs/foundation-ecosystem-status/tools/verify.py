"""Verify active seven-role decisions and preserve scoped provider acceptance boundaries."""
from pathlib import Path
import json,hashlib,importlib.util
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('ecosystem_forms',R/'docs/foundation-bcap-owner-review/tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
i=json.loads((P/'INPUTS.json').read_text());s=json.loads((P/'STATUS.json').read_text());assert i['proposalManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json')
for id,row in i['activeResponses'].items():
 f=P/row['copy'];assert sha(f)==row['sha256'];data=json.loads(f.read_text());assert v.validate(data) and data['decision']==row['decision']
assert len(i['activeResponses'])==7 and s['submitted']==7 and s['scopedDesignAccepts']==5 and s['revise']==2
assert s['remainingDesignRevisions']==['BCAP-O1-Schema','BCAP-O2-Schema'] and s['decisions']['BCAP-O4-XmlDb']=='AcceptWithScope'
assert not any(s[k] for k in ['nativeBcapCandidateQualified','performanceBudgetsAccepted','r2Closed','apiFrozen','productionChanged'])
receipt=P/'inputs/XmlDb-fix-review-INPUTS.json';assert sha(receipt)==i['xmlDbFixReviewSha256'];x=json.loads(receipt.read_text());assert x['sourceInputs']==i['xmlDbQualifiedSourceInputs'] and len(x['sourceInputs'])==21
assert sha(P/'inputs/XmlDb-native-results.json')==x['qualifiedReports']['docs/foundation-requirements/native-lifetime/results.json']
assert sha(P/'inputs/BCAP-O4-XmlDb.previous-Revise.json')==i['xmlDbPriorReviseSha256']==x['priorBcapResponseSha256']
assert sum(r['passed'] for r in i['xmlDbTrxChecked'] if r['platform']=='Windows')==4482
assert sum(r['passed'] for r in i['xmlDbTrxChecked'] if r['platform']=='Linux')==4482
assert sum(r['passed'] for r in i['xmlDbTrxChecked'] if r['platform']=='Focused')==190
if (P/'MANIFEST.json').exists():
 for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/n)==h,n
print('Verified current ecosystem: five scoped accepts/two Schema revisions; XmlDb finite provider fix accepted with scope, new-Core/native budgets still pending.')
