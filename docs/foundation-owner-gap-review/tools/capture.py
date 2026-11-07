"""Read-only owner source inventory; never builds or invokes owner binaries."""
from pathlib import Path
import hashlib, json, subprocess

P = Path(__file__).resolve().parents[1]
R = P.parents[1]
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
old = json.loads((R / 'docs/foundation-provider-qualification/evidence/OWNER-SOURCE-SNAPSHOT.json').read_text())
records = []
for prior in old['records']:
    root = Path(prior['root'])
    def git(*args):
        return subprocess.check_output(['git', '-C', str(root), *args], text=True).strip()
    files = {p.relative_to(root).as_posix(): sha(p)
             for p in sorted((root / 'src').rglob('*.cs'))
             if not {'bin', 'obj'}.intersection(p.relative_to(root).parts)}
    tokens = {t: [] for t in ['ISchemaAssessor', 'ISchemaAssessmentAdvisor', 'ReadTypedContainer', 'SchemaAtomics']}
    for name in files:
        source = (root / name).read_text(encoding='utf-8-sig')
        for token in tokens:
            if token in source:
                tokens[token].append(name)
    previous = prior['scanSourceHashes']
    records.append(dict(name=prior['name'], root=str(root), head=git('rev-parse', 'HEAD'),
        branch=git('branch', '--show-current'), status=git('status', '--porcelain'),
        productionSourceHashes=files, tokenOccurrences=tokens,
        changedSinceReadiness=sorted(n for n in set(files) | set(previous) if files.get(n) != previous.get(n)),
        priorHead=prior['head']))
inputs = [
    'docs/foundation-provider-qualification/PF-MATRIX.json',
    'docs/foundation-owner-workload-protocol/BUDGET-PROPOSAL.json',
    'docs/foundation-owner-workload-protocol/BASELINE-ELIGIBILITY.json',
    'docs/foundation-owner-workload-protocol/PROTOCOL.json',
    'docs/foundation-captured-identity-production/STATUS.json',
    'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json',
    'docs/foundation-native-immutable-guard/STATUS.json',
    'docs/foundation-uri-binary-disposition/STATUS.json',
    'docs/foundation-legacy-disposition/STATUS.json',
    'docs/release-planning/GATES.json',
]
result = dict(date='2026-11-07', sourceVersion='1.81.1',
    readOnly=True, records=records, inputs={n: sha(R/n) for n in inputs},
    scope='Literal production C# token and byte inventory only; absence is not proof of no indirect integration. No owner binaries executed or private licensing checkout read.')
result['candidateCommit'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=R, text=True).strip()
(P/'SOURCE-REVIEW.json').write_text(json.dumps(result, indent=2)+'\n', encoding='utf-8')
pf = json.loads((R/'docs/foundation-provider-qualification/PF-MATRIX.json').read_text())
budgets = json.loads((R/'docs/foundation-owner-workload-protocol/BUDGET-PROPOSAL.json').read_text())
rows = []
for scenario, budget in zip(pf['scenarios'], budgets['scenarios'], strict=True):
    assert scenario['id'] == budget['id']
    rows.append(dict(id=scenario['id'], owners=scenario['owners'], boundary=scenario['boundary'],
        workloads=scenario['workloads'], metrics=scenario['metrics'], controls=scenario['controls'],
        ownerWorkloadAccepted=budget['ownerWorkloadAccepted'], ownerBudget=budget['ownerBudget'],
        baselineAccepted=budget['baselineAccepted'], candidateAccepted=budget['candidateAccepted'],
        currentCohortPerformanceReceipt=None,
        diagnosticEvidence=('TA-51 candidate-only 1.81.0 one/eight-child decimal subset; not full PF-03'
                            if scenario['id']=='PF-03' else None),
        nextEvidence=['Owner fixture/profile/limit selection', 'Comparable baseline and loaded bindings',
                      'Unit-bearing budgets and tolerances', 'Exact 1.81.1 source/binary runs and dated disposition']))
gaps = dict(date='2026-11-07', candidateCommit=result['candidateCommit'], sourceVersion='1.81.1',
    scenarioCount=9, acceptedBudgetCount=0, acceptedBaselineCount=0, scenarios=rows,
    requirementsWaived=0, ownerAccepted=False, apiFrozen=False, packagesBuilt=False,
    productionProviderAdoptionAccepted=False, publicationAuthorized=False,
    reviewCompleteWithinScope=True, nextSlice='PV-Q06 exact-1.81.1 workload cohort preparation',
    historicalProtocolVersion='1.81.0', historicalProtocolReusedAsCurrentReceipt=False)
(P/'GAPS.json').write_text(json.dumps(gaps, indent=2)+'\n', encoding='utf-8')
for row in records:
    print(row['name'], row['head'][:12], 'source files:', len(row['productionSourceHashes']),
          'changed:', len(row['changedSinceReadiness']), 'token matches:', sum(map(len, row['tokenOccurrences'].values())))
