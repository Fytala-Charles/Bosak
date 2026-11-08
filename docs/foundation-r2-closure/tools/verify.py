"""Verify exact submitted role bindings, scoped closure and ready gate without executing .NET."""
from pathlib import Path
import sys,json,hashlib,importlib.util
sys.dont_write_bytecode=True
P=Path(__file__).resolve().parents[1];R=P.parents[1];B=R/'docs/foundation-bcap-owner-review'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('r2closure_returns',B/'tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
a=json.loads((P/'ALL-SEVEN-VALIDATION.json').read_text());c=json.loads((P/'CLOSURE.json').read_text());g=json.loads((P/'GATE.json').read_text());q=json.loads((P/'REQUIREMENT-DISPOSITIONS.json').read_text())
assert a['proposalManifestSha256']==c['proposalManifestSha256']==g['proposalManifestSha256']==sha(B/'PROPOSAL-MANIFEST.json')
for key,path in [('validatorSha256',B/'tools/returns.py'),('baseValidatorSha256',B/'inputs/original-returns.py'),('roleMatrixSha256',B/'ROLE-MATRIX.json'),('packetsSha256',B/'PACKETS.json')]:assert a[key]==sha(path)
assert {x['returnId'] for x in a['roles']}=={x['returnId'] for x in json.loads((B/'ROLE-MATRIX.json').read_text())['roles']} and len(a['roles'])==7
for row in a['roles']:
 f=R/row['path'];assert sha(f)==row['sha256'];d=json.loads(f.read_text());assert v.validate(d) and d['decision']=='AcceptWithScope' and d['returnId']==row['returnId']
 if row['owner']=='Schema':
  assert d['r2BlockingItems']==[] and d['responseRevision']==2 and not any(x['r2Blocker'] for x in d['remainingBlockers'])
  old=R/'docs/foundation-ecosystem-status/inputs'/(row['returnId']+'.json');assert sha(old)==d['supersedesResponse']['sha256'] and d['priorResponseSha256'] is None
 assert not d['runtimeQualification'] and not d['c45Accepted'] and not d['apiFrozen'] and d['acceptedPerformanceBudgets'] is None
for row in json.loads((P/'inputs/schema-v2/CHECKS.json').read_text())['checks']:assert sha(P/'inputs/schema-v2'/row['log'])==row['sha256'] and row['exitCode']==0
assert c['status']=='ClosedWithScope' and c['r2Closed'] and c['requiredDesignSignatureGaps']==[] and c['unresolvedR2ApiPolicyOrSourceFeasibilityBlockers']==[]
for key in ['implementationStarted','runtimeQualification','nativeBcapIntegrationQualified','performanceBudgetsAccepted','c45Accepted','apiFrozen','publicationAuthorized']:assert c[key] is False
assert c['targetReleaseVersion'] is None and c['releaseDate'] is None
assert sha(R/c['allSevenRoleValidationReceipt'])==c['allSevenRoleValidationSha256'] and sha(R/c['requirementDispositions'])==c['requirementDispositionsSha256']
old=R/'docs/foundation-r2-ledger/LEDGER.json';assert sha(old)==q['historicalLedgerSha256'];original=json.loads(old.read_text())
assert len(q['requirements'])==43 and len(q['decisions'])==14
for row,prior in zip(q['requirements'],original['requirements']):
 for key in ['id','baselineRequirement','decisions','coreDisposition','nextEvidence']:assert row[key]==prior[key]
 assert set(row['reviewRoles'])<={x['returnId'] for x in a['roles']} and row['qualificationStatus']=='PendingR4' and row['acceptedPerformanceBudget'] is False
assert g['status']=='Ready for implementation' and not g['implementationStarted'] and g['implementationCandidateCommit'] is None and g['semanticAdoptionBeforeCodeRequired']
for row in g['requiredReplacements']:assert sha(R/row['path'])==row['sha256'] and row['validationReceipt']==c['allSevenRoleValidationReceipt']
assert g['scopedR2Closure']['path']=='docs/foundation-r2-closure/CLOSURE.json' and g['scopedR2Closure']['sha256']==sha(P/'CLOSURE.json') and g['scopedR2Closure']['allSevenRoleValidationReceipt']==c['allSevenRoleValidationReceipt']
for key in ['sourceInputs','qualificationReceipt']:assert sha(R/g['baseline'][key])==g['baseline'][key+'Sha256']
for row in json.loads((R/'docs/schema-w1-gate-review/ACCEPTANCE.json').read_text())['cases']:assert row['status']=='NotRun'
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Verified seven scoped role returns, 43 requirement dispositions, exact closure/gate bindings and retained NotRun acceptance. W1-A entry ready; no implementation/runtime/freeze/publication acceptance.')
