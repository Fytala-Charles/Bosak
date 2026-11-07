"""Validate evidence links, draft arithmetic and the absence of inferred approvals."""
from pathlib import Path
import hashlib,json,math,re

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PROPOSAL.json')
assert p['status']=='DraftForOwnerReview' and p['previewVersion']=='1.81.1-preview.c0b926de'
for name,h in p['inputs'].items():assert sha(R/name)==h,name
assert p['requirementsWaived']==0 and not any(p[k] for k in ['ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized'])
assert [r['id'] for r in p['scenarios']]==[f'PF-{i:02}' for i in range(1,10)]
summary=read(R/'docs/foundation-package-workload/evidence/SUMMARY.json')
for row in p['scenarios']:
    assert row['proposedWorkload'] and row['requiredInputs'] and row['status']=='DraftForOwnerReview'
    assert not row['ownerAccepted'] and not row['baselineAccepted']
    assert row['baseline'] is row['ownerBudget'] is row['relativeRegressionTolerance'] is None
    if row['id']!='PF-03':assert row['draftBudgets']==[];continue
    assert len(row['draftBudgets'])==2
    for draft,measurement in zip(row['draftBudgets'],summary['workloads'],strict=True):
        assert draft['fixture']==measurement['id'] and not draft['accepted']
        for metric,field,quantum in [('latency','repetitionMediansMs',25),('allocation','allocatedBytesPerCycle',65536)]:
            value=draft[metric];assert value['observed']==max(measurement[field])
            assert value['headroomMultiplier']==2 and value['roundUpQuantum']==quantum
            assert value['proposedCeiling']==math.ceil(2*value['observed']/quantum)*quantum
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists()
print('Verified nine draft workload mappings and two evidence-derived fixture guardrails; all owner budgets/baselines/approvals remain open.')
