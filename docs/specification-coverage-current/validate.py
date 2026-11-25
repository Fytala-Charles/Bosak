from pathlib import Path
import json,hashlib,subprocess
D=Path(__file__).resolve().parent;R=D.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
a=load(D/'CURRENT-MATRIX.json');assert len(a['rows'])==24 and len({x['id'] for x in a['rows']})==24
assert not a['inventoryExhaustive'] and a['normativeDenominator'] is None and a['conformancePercentage'] is None and not a['qualification']['freshRun']
assert a['qualification']['expandedCombined']==dict(cases=129,match=19,unsupported=110,mismatch=0,harnessError=0)
assert all(not x['fullyConformant'] and not x['normativeRequirementEnumerationComplete'] and x['remaining'] and x['evidence'] for x in a['rows'])
for n,h in load(D/'HISTORICAL-PRESERVATION.json').items():assert sha(R/n)==h,n
section=load(D/'SECTION-REVIEW.json');assert not section['semanticAdoption'] and all(x['sectionHeadingNumberVerified'] for x in section['sectionReferences'] if not x['reference'].startswith('xdm31:'))
for row in a['rows']:assert all(key in a['evidence'] for key in row['evidence'])
# Verify local evidence for this repository. Cross-repository evidence is checked by the paired audit orchestration.
repository='schema' if (R/'Bosak.Schema.slnx').is_file() else 'core'
for e in a['evidence'].values():
 if e['repository']==repository:assert sha(R/e['path'])==e['sha256'],e['path']
print('24bounded areas; reviewed section references and exact local evidence verify; historical matrix preserved; no percentage/conformance claim.')
