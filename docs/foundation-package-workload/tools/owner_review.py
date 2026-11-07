"""Record concrete evidence pointers and still-unfilled owner acceptance slots."""
from pathlib import Path
import copy,hashlib,json
from owner_preflight import check_current

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
owners={'Core':['PF-01','PF-02','PF-03','PF-04','PF-05','PF-06','PF-07','PF-08','PF-09'],
    'Schema':['PF-02','PF-03','PF-06','PF-08','PF-09'],'XmlDb':['PF-04','PF-05','PF-06','PF-08','PF-09'],
    'Braid':['PF-06','PF-07','PF-08','PF-09']}
refusals={};stale={}
for owner in owners:
    template=read(P/'returns'/(owner+'.template.json'));result=check_current(template)
    assert not result['structureReady'] and not result['qualificationAccepted'] and not result['ownerAcceptanceVerified']
    assert not any(e.startswith('candidate:') for e in result['errors'])
    refusals[owner]=result
    prior=read(R/'docs/foundation-provider-adoption/returns'/(owner+'.template.json'))
    result=check_current(prior);assert 'candidate: exact source/binary manifest mismatch' in result['errors']
    assert 'candidate: complete engine hash cohort mismatch' in result['errors']
    stale[owner]=result
(P/'evidence/OWNER-REFUSALS.json').write_text(json.dumps(dict(currentUnfilled=refusals,priorProductionCohort=stale),indent=2)+'\n')
budget=read(R/'docs/foundation-owner-workload-protocol/BUDGET-PROPOSAL.json')
assert len(budget['scenarios'])==9 and all(r['ownerBudget'] is None and not r['ownerWorkloadAccepted'] and not r['baselineAccepted'] and not r['candidateAccepted'] for r in budget['scenarios'])
files=[R/'docs/foundation-preview-packages/evidence/PACKAGES.json',R/'docs/foundation-preview-qualification/MANIFEST.json',
    R/'docs/foundation-preview-qualification/evidence/RESULTS.json',P/'CANDIDATE-PINS.json',P/'evidence/SUMMARY.json',
    R/'docs/foundation-provider-adoption/SCHEMA-ADOPTION.md',R/'docs/foundation-owner-workload-protocol/BUDGET-PROPOSAL.json']
review=dict(date='2026-11-07',previewVersion=read(P/'COHORT.json')['previewVersion'],
    candidateCommit=read(P/'COHORT.json')['candidateCommit'],
    evidence={f.relative_to(R).as_posix():sha(f) for f in files},
    owners=[dict(role=owner,scenarios=scenarios,decision=None,ownerIdentity=None,decisionDate=None,
        workloadApproved=False,baselineApproved=False,budgetApproved=False,productionAdoptionAccepted=False)
        for owner,scenarios in owners.items()],
    scenarios=copy.deepcopy(budget['scenarios']),requirementsWaived=0,ownerAccepted=False,
    apiFrozen=False,publicationAuthorized=False,messagesSent=False,
    nextAction='Review the concrete exact-package evidence and provide dated owner workload/baseline/budget/adoption dispositions; source/pin changes require separate qualification.')
(P/'OWNER-REVIEW.json').write_text(json.dumps(review,indent=2)+'\n',encoding='utf-8')
print('Prepared exact-package owner review; four unfilled templates and four prior-cohort templates refuse as expected. All nine acceptance slots remain open.')
