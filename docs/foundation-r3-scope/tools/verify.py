"""Verify finite R3 reconciliation without upgrading historical evidence into acceptance."""
from pathlib import Path
import json,hashlib,subprocess,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1]
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def validate(rows,plan,declarations,status,cases,ledger,closure):
 expected=[r['id'] for r in ledger['requirements']]
 assert [r['id'] for r in rows['requirements']]==expected and len(expected)==43
 assert [r['id'] for r in rows['decisions']]==[r['id'] for r in ledger['decisions']] and len(rows['decisions'])==14
 assert [r['id'] for r in rows['closureObligations']]==[r['id'] for r in closure['remainingObligations']] and len(rows['closureObligations'])==6
 assert rows['sourceVersion']==plan['sourceVersion']==status['sourceVersion']=='1.70.0'
 work=plan['work'];ids=[r['id'] for r in work];assert len(ids)==len(set(ids))==10
 assert plan['nextCoreTask']==status['nextCoreTask']=='AH-01' and plan['noRequirementWaivers']
 seen=set()
 for row in work:
  assert set(row['dependsOn'])<=seen and row['id'] not in row['dependsOn']
  assert row['owner'] and row['work'] and row['exitEvidence'];seen.add(row['id'])
 for row,original in zip(rows['requirements'],ledger['requirements']):
  assert row['baselineRequirement']==original['baselineRequirement'] and row['historicalDisposition']==original['coreDisposition']
  assert not row['requirementWaived'] and not row['fullyImplemented'] and not row['ownerCandidateAccepted']
  assert row['nextWork'] in seen and row['implemented'] and row['remaining'] and row['evidence']
 for row in rows['decisions']:
  assert not row['requirementWaived'] and not row['acceptedForFinalFreeze'] and row['nextWork'] in seen
 for row in rows['closureObligations']:assert not row['requirementWaived'] and row['status']=='Open' and set(row['nextWork'])<=seen
 assert len(declarations['rows'])==declarations['proposedTypes']==47
 assert len({r['type'] for r in declarations['rows']})==47
 assert sum(r['presentInProduction'] for r in declarations['rows'])==declarations['presentTypes']==40
 assert sum(not r['presentInProduction'] for r in declarations['rows'])==declarations['missingTypes']==7
 assert not declarations['typePresenceIsBehaviorAcceptance']
 for k in ['sourceChanged','newEngineTestsRun','r3Complete','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed','publicationAuthorized']:assert status[k] is False,k
 assert status['requirementRows']==43 and status['decisionRows']==14 and status['closureObligations']==6 and status['requirementsWaived']==0 and status['numericMemberIntentsDeclared']==6
 assert cases['implementationStatus']=='NotStarted' and [r['id'] for r in cases['cases']]==['HC'+str(n).zfill(2) for n in range(1,17)] and all(r['status']=='NotRun' for r in cases['cases'])

def main():
 inputs=read(P/'INPUTS.json')
 for name,h in inputs['files'].items():assert sha(R/name)==h,name
 validate(read(P/'REQUIREMENTS.json'),read(P/'PLAN.json'),read(P/'DECLARATIONS.json'),read(P/'STATUS.json'),read(P/'HOST-CASES.json'),read(R/'docs/foundation-r2-closure/REQUIREMENT-DISPOSITIONS.json'),read(R/'docs/foundation-r2-closure/CLOSURE.json'))
 prior=R/'docs/foundation-public-compatible'
 for name,h in read(prior/'MANIFEST.json').items():assert sha(prior/name)==h,name
 pins=read(prior/'evidence/SOURCE-AND-BINARY-PINS.json')
 for name,h in pins['sources'].items():assert sha(R/name)==h,name
 for row in pins['engines'].values():assert sha(Path(row['path']))==row['sha256']
 for name,h in pins['configuration'].items():assert sha(R/name)==h,name
 for name,row in pins['corpora'].items():
  assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
  assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
 units=read(prior/'evidence/UNIT-RESULTS.json');assert units['passed']==25831 and len(units['projects'])==10
 for row in units['projects']:
  f=prior/'evidence'/row['trx'];assert sha(f)==row['sha256'];c=ET.parse(f).find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed'];assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
 proof=read(P/'evidence/prior-proof.log.json');assert proof['exitCode']==0 and proof['failure'] is None and sha(P/'evidence/prior-proof.log')==proof['logSha256']
 controls=read(P/'evidence/controls.log.json');assert controls['exitCode']==0 and controls['failure'] is None and sha(P/'evidence/controls.log')==controls['logSha256']
 assert 'Ran 13 tests' in (P/'evidence/controls.log').read_text(encoding='utf-8-sig') and read(P/'STATUS.json')['auditControlsPassed']==13
 assert 'Verified TA-25: 25831 tests' in (P/'evidence/prior-proof.log').read_text(encoding='utf-8-sig')
 inventory=read(prior/'evidence/public-api.json')
 current={r.split(':',1)[0][2:] for a in inventory['assemblies'] for r in a['declarations'] if r.startswith('T ')}
 proposed=read(R/'docs/foundation-bcap-owner-review/inputs/proposed-api-0.json')
 expected={r.split(':',1)[0][2:] for a in proposed['assemblies'] for r in a['declarations'] if r.startswith('T ')}
 assert {r['type'] for r in read(P/'DECLARATIONS.json')['rows']}==expected
 for row in read(P/'DECLARATIONS.json')['rows']:assert row['presentInProduction']==(row['type'] in current)
 witness=read(P/'evidence/NUMERIC-INTENTS.json');assert witness['allSixDeclared'] and not witness['fullNumericPipelineQualified'] and len(witness['witnesses'])==6
 actual={r for a in inventory['assemblies'] for r in a['declarations']};assert set(witness['witnesses'])<=actual
 if (P/'evidence/DOCUMENTATION-CHECKS.json').exists():
  for row in read(P/'evidence/DOCUMENTATION-CHECKS.json')['pages']:assert sha(R/row['source'])==row['sourceSha256'],row['source']
 if (P/'MANIFEST.json').exists():
  manifest=read(P/'MANIFEST.json');assert set(manifest)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
  for name,h in manifest.items():assert sha(P/name)==h,name
 print('Verified TA-26: 43 requirements / 14 decisions / 6 obligations; 40 present and 7 absent proposed types; six numeric declarations. Exact 1.70.0 qualified source/binaries unchanged; AH-01 next, no scope waiver or release acceptance.')
if __name__=='__main__':main()
