"""Verify review coverage and pins without granting owner acceptance."""
from pathlib import Path
import hashlib, json, re, subprocess

P = Path(__file__).resolve().parents[1]
R = P.parents[1]
read = lambda p: json.loads(p.read_text(encoding='utf-8'))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
review = read(P/'SOURCE-REVIEW.json')
gaps = read(P/'GAPS.json')
assert review['readOnly'] and review['sourceVersion'] == gaps['sourceVersion'] == '1.81.1'
assert review['candidateCommit'] == gaps['candidateCommit'] == 'c0b926de34824ceb2010d5f814a4450b28b31dcc'
for name, digest in review['inputs'].items():
    assert sha(R/name) == digest, name
assert {r['name'] for r in review['records']} == {'schema-assessment', 'schema-public', 'xmldb', 'braid'}
for row in review['records']:
    root = Path(row['root'])
    head = subprocess.check_output(['git', '-C', str(root), 'rev-parse', 'HEAD'], text=True).strip()
    assert head == row['head'] == row['priorHead'], row['name']
    files = {p.relative_to(root).as_posix(): sha(p) for p in (root/'src').rglob('*.cs')
             if not {'bin', 'obj'}.intersection(p.relative_to(root).parts)}
    assert files == row['productionSourceHashes'] and not row['changedSinceReadiness'], row['name']
    for token, names in row['tokenOccurrences'].items():
        assert sorted(n for n in files if token in (root/n).read_text(encoding='utf-8-sig')) == names
        assert not names
assert sum(len(r['productionSourceHashes']) for r in review['records']) == 263
pf = read(R/'docs/foundation-provider-qualification/PF-MATRIX.json')
budget = read(R/'docs/foundation-owner-workload-protocol/BUDGET-PROPOSAL.json')
assert len(gaps['scenarios']) == gaps['scenarioCount'] == 9
assert gaps['acceptedBudgetCount'] == gaps['acceptedBaselineCount'] == gaps['requirementsWaived'] == 0
assert gaps['reviewCompleteWithinScope']
for key in ['ownerAccepted', 'apiFrozen', 'packagesBuilt', 'productionProviderAdoptionAccepted',
            'publicationAuthorized', 'historicalProtocolReusedAsCurrentReceipt']:
    assert gaps[key] is False, key
for row, source, proposal in zip(gaps['scenarios'], pf['scenarios'], budget['scenarios'], strict=True):
    assert row['id'] == source['id'] == proposal['id']
    for key in ['owners', 'boundary', 'workloads', 'metrics', 'controls']:
        assert row[key] == source[key], (row['id'], key)
    for key in ['ownerWorkloadAccepted', 'baselineAccepted', 'candidateAccepted']:
        assert row[key] == proposal[key] is False
    assert row['ownerBudget'] == proposal['ownerBudget'] is None
    assert row['currentCohortPerformanceReceipt'] is None and len(row['nextEvidence']) == 4
assert read(R/'docs/foundation-owner-workload-protocol/PROTOCOL.json')['sourceVersion'] == gaps['historicalProtocolVersion'] == '1.81.0'
assert read(R/'docs/foundation-owner-workload-protocol/BASELINE-ELIGIBILITY.json')['selectedBaseline'] is None
links = 0
for file in [P/'README.md', P/'OWNER-DECISIONS.md', R/'docs/CHECKPOINT-1.81.1.md']:
    source = re.sub(r'`[^`]*`', '', file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)', source):
        target = link.strip('<>').split('#')[0]
        if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:', target):
            assert (file.parent/target).exists(), (file, target)
            links += 1
if (P/'MANIFEST.json').exists():
    manifest = read(P/'MANIFEST.json')
    files = {p.relative_to(P).as_posix(): sha(p) for p in P.rglob('*') if p.is_file()
             and p.name != 'MANIFEST.json' and '__pycache__' not in p.parts}
    assert files == manifest
print(f'PV-Q05 verified: 4 owner inventories/263 C# files, 9 unaccepted scenarios, historical protocol distinction, {links} local links; no acceptance or fresh engine runs claimed.')
