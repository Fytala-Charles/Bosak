"""Verify all seven dispositions and Core's exact-manifest scoped design returns."""
from pathlib import Path
import json,hashlib,importlib.util
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('core_review_validation',R/'docs/foundation-bcap-owner-review/tools/returns.py');validator=importlib.util.module_from_spec(spec);spec.loader.exec_module(validator)
s=json.loads((P/'STATUS.json').read_text());assert s['proposalManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json')
rows=[]
for f in sorted((P/'returns').glob('*.json')):
 data=json.loads(f.read_text());assert validator.validate(data) and data['decision']=='AcceptWithScope' and data['owner']=='Core';rows.append(data)
 for evidence in data['evidence']:assert sha(R/evidence['reference'])==evidence['sha256']
 assert not data['runtimeQualification'] and not data['c45Accepted'] and not data['apiFrozen'] and data['acceptedPerformanceBudgets'] is None
for f in sorted((P/'inputs').glob('*.json')):
 data=json.loads(f.read_text());assert validator.validate(data);rows.append(data)
assert len(rows)==7 and len({r['returnId'] for r in rows})==7
assert sum(r['decision']=='AcceptWithScope' for r in rows)==4 and sum(r['decision']=='Revise' for r in rows)==3
assert s['submitted']==s['required']==7 and not s['missing'] and not any(s[k] for k in ['r2Closed','apiFrozen','nativeConsumersQualified','productionChanged'])
if (P/'MANIFEST.json').exists():
 for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/n)==h,n
print('Verified all seven exact-bound returns: four scoped accepts/three revisions; Core O2/O4 complete, R2 still open.')
