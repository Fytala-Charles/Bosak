"""Verify the pending performance gate without inferring benchmark results."""
from pathlib import Path
import hashlib
import json

PACKET = Path(__file__).resolve().parents[1]
ROOT = PACKET.parents[1]
plan = json.loads((PACKET / 'PLAN.json').read_text(encoding='utf-8'))
assert {s['id'] for s in plan['scenarios']} == {f'PF-{i:02}' for i in range(1,10)}
assert len(plan['scenarios']) == 9
for scenario in plan['scenarios']:
    assert scenario['status'] == 'NotRun'
    assert all(scenario[key] is None for key in ('fixtureHashes','baselineArtifactHashes','candidateArtifactHashes','budget','observations','disposition'))
    assert all(scenario[key] for key in ('owners','boundary','workloads','metrics','controls'))
assert plan['performanceQualified'] is plan['apiFrozen'] is False
assert plan['finalBreakingChangeIntent'] is True
freeze = json.loads((ROOT / 'docs/FOUNDATION-API-FREEZE.template.json').read_text(encoding='utf-8'))
assert freeze['status'] == 'Pending' and freeze['finalBreakingFoundationChangeIntended'] is True
assert all(freeze[key] is None for key in ('performanceBudgetsAndWorkloads','performanceBaselineAndCandidateEvidence','performanceOwnerDispositions','apiLongevityReview'))
if (PACKET / 'MANIFEST.json').exists():
    for item in json.loads((PACKET / 'MANIFEST.json').read_text(encoding='utf-8'))['files']:
        assert hashlib.sha256((PACKET / item['path']).read_bytes()).hexdigest() == item['sha256']
print('PASS: nine unrun performance scenarios, explicit final-breaking-change intent and pending freeze evidence.')
