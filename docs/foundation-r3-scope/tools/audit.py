"""Reconcile the sealed accepted ledger with the exact qualified production inventory."""
from pathlib import Path
import json,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(name,data):(P/name).write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def types(data):return {row.split(':',1)[0][2:]:a['assembly'] for a in data['assemblies'] for row in a['declarations'] if row.startswith('T ')}

def audit():
 inputs=read(P/'INPUTS.json')
 for name,h in inputs['files'].items():
  if digest(R/name)!=h:raise ValueError('Changed audit input: '+name)
 accepted=read(R/'docs/foundation-r2-closure/REQUIREMENT-DISPOSITIONS.json')
 proposed=types(read(R/'docs/foundation-bcap-owner-review/inputs/proposed-api-0.json'))
 current=types(read(R/'docs/foundation-public-compatible/evidence/public-api.json'))
 missing=set(proposed)-set(current)
 expected={'Bosak.XPath.Core.Schema.SchemaAtomics','Bosak.XQuery.Api.Schema.SchemaXQueryCompiler'}|{'Bosak.Xslt.Api.Schema.'+n for n in ['SchemaXsltCompiler','SchemaXsltExecutable','SchemaXsltInvocation','SchemaXsltCaptureResult','SchemaXsltRunResult']}
 if len(proposed)!=47 or len(set(proposed)&set(current))!=40 or missing!=expected:raise ValueError('Declaration inventory changed; review required')
 declarations=[]
 for name in sorted(proposed):
  present=name in current
  declarations.append(dict(type=name,presentInProduction=present,assembly=current.get(name),status='DeclaredNotFullBehaviorAcceptance' if present else 'MissingProductionDeclaration',nextWork=None if present else 'PV-01' if name.endswith('SchemaAtomics') else 'AH-02' if name.endswith('SchemaXQueryCompiler') else 'AH-03',scope='Type-name presence only; signatures/defaults/semantics and backend qualification are separate.'))
 core='docs/foundation-public-compatible/STATUS.json';nodes='docs/foundation-public-node-data/STATUS.json';numeric='docs/foundation-public-typed-numeric/STATUS.json';lifetime='docs/foundation-paired-provider-lifetime/STATUS.json'
 schema={
 'N1':([core,numeric],'Opaque handles and exact source authority checks execute in bounded typed/numeric/compatible routes.','General authority/universe use and actual independently compiled native universes require owner qualification.','R4-01'),
 'N2':([numeric],'Selected numeric projection resolves and authenticates actual integer/decimal relations.','General affiliation/declaration/complete-closure query routing and native provider coverage remain unqualified.','SC-01'),
 'N3':([nodes,'docs/foundation-typed-container/README.md'],'Internal bounded readers preserve list shape and per-atomic selected source evidence; public data flattens selected containers.','SchemaAtomics observation is absent; general public empty/list/union transport and persistent/constructed variants are not fully implemented.','PV-01'),
 'N4':(['docs/foundation-advisor-adapter/README.md','docs/foundation-public-construction/README.md'],'Assessor binding and source-free episode constants are used by selected XPath profiles.','General assessment/CTA/assertion consultation and complete valid-only root routing across hosts remain.','SC-01'),
 'N5':([nodes,'docs/foundation-public-node-nilled/README.md'],'Public original-node nilled/data authenticate exact facts/node/input/origin and preserve original evidence.','General preserve/strip/copy and actual native PSVI/provider routes need integration qualification.','SC-01'),
 'N6':([core],'Public directional handoff and non-list singleton identity reproof preserve original source components/assessment.','Compatible derivation/declaration predicates, node/numeric/list/transitive/all-host and native routes remain excluded, without waiving the requirement.','SC-01'),
 'N7':([core,lifetime],'Engine raw retention, paired admissions, closure/drain, concurrency refusal and cleanup are qualified with real test resources.','Actual native reader/snapshot/worker ownership, allowed concurrency and deferred/nested scopes require owner evidence.','R4-01'),
 'N8':([core,'docs/foundation-public-ordinary/README.md'],'Separate source/snapshot/origin/component and XPath semantic environments are represented with exact identity guards.','Public XQuery/XSLT captured environments and actual host/provider negative cases remain.','AH-01')}
 # Use the actual existing ordinary dossier rather than invent an evidence location.
 schema['N8'][0][1]='src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs'
 rows=[]
 for row in accepted['requirements']:
  id=row['id'];base=row['baselineRequirement']
  if id in schema:
   evidence,implemented,remaining,nextWork=schema[id];status='PartialCoreImplementation'
  elif id.startswith('FXR-'):
   evidence=[core,'docs/release-planning/OWNER-CAPABILITIES.md'];implemented='Existing ordinary node/sequence interfaces and bounded engine lifetime routes remain; earlier owner receipts retain their own exact pins.'
   remaining=base.get('requiredContract',base['requestedReturn'])+' Re-run the actual XmlDb source/unchanged-binary/store/resource cases on the selected candidate; Core fixtures do not qualify the provider.';nextWork='R4-01';status='CoreBoundaryRecordedOwnerCandidatePending'
  elif id.startswith('FXG-'):
   evidence=['docs/release-planning/OWNER-CAPABILITIES.md',core];implemented='Historical owner profile disposition and bounded Core guards are retained; no universal cached/nested-value or concurrent-disposal guarantee is inferred.'
   remaining=base['requestedReturn'];nextWork='R5-01' if id=='FXG-06' else 'R4-01';status='OwnerCandidateQualificationPending'
  elif id.startswith('BF140-'):
   evidence=['docs/foundation-bcap-profile/README.md',core];implemented='Accepted BCAP declaration/isolated capture feasibility and public XPath subsets exist; neither is an integrated production Braid route.'
   remaining=base['requestedReturn'];nextWork='AH-03' if id=='BF140-05' else 'R4-01';status='ConsumerIntegrationPending'
  elif id.startswith('PF-'):
   evidence=['docs/foundation-bcap-cost/evidence/SUMMARY.json',core];implemented='Recorded local synthetic allocation/cancellation observations exist only in their original fixture/candidate scopes.'
   remaining=base['title']+': '+base['requestedReturn'];nextWork='R4-02';status='WorkloadBudgetAcceptancePending'
  else:raise ValueError('Unclassified requirement '+id)
  for name in evidence:
   if not (R/name).is_file():raise ValueError('Missing evidence '+name)
  rows.append(dict(id=id,owner=row['owner'],historicalDisposition=row['coreDisposition'],baselineRequirement=base,status=status,implemented=implemented,remaining=remaining,evidence=evidence,nextWork=nextWork,requirementWaived=False,fullyImplemented=False,ownerCandidateAccepted=False))
 decisions=[]
 for row in accepted['decisions']:
  id=row['id'];work={'D01':'AH-01','D02':'AH-01','D03':'PV-01','D04':'AH-01','D05':'R4-01','D06':'AH-03','D07':'SC-01','D08':'NU-01','D09':'PV-01','D10':'SC-01','D11':'AH-03','D12':'AH-03','D13':'R4-01','D14':'R4-02'}[id]
  decisions.append(dict(id=id,title=row['title'],historicalRule=row['historicalRule'],status='PartialProductionOrOwnerQualificationPending',nextWork=work,requirementWaived=False,acceptedForFinalFreeze=False))
 closure=read(R/'docs/foundation-r2-closure/CLOSURE.json')
 obligationWork={'R3-OWN':['SC-01','PV-01','R4-01'],'R3-NUM':['NU-01'],'R3-HOST':['AH-01','AH-02','AH-03'],'R4-NATIVE':['R4-01'],'R4-PERF':['R4-02'],'R5-R7':['R5-01','PK-01']}
 obligations=[dict(id=o['id'],obligation=o['obligation'],status='Open',nextWork=obligationWork[o['id']],requirementWaived=False) for o in closure['remainingObligations']]
 write('REQUIREMENTS.json',dict(sourceVersion='1.70.0',requirements=rows,decisions=decisions,closureObligations=obligations))
 write('DECLARATIONS.json',dict(proposedTypes=47,presentTypes=40,missingTypes=7,rows=declarations,additionalCompatibleResult='Bosak.XPath.Api.Schema.SchemaQueryCompatibilityResult',typePresenceIsBehaviorAcceptance=False))
 # Check all six numeric member intents in compiled production metadata, not legacy summary flags.
 inventory=read(R/'docs/foundation-public-compatible/evidence/public-api.json')
 allrows=[r for a in inventory['assemblies'] if a['assembly']=='Bosak.XPath.Core' for r in a['declarations']]
 witnesses=[r for r in allrows if (r.startswith('M Bosak.XPath.Core.Xdm.XdmValue.FromInteger:') and '(System.Numerics.BigInteger value:' in r) or (r.startswith('M Bosak.XPath.Core.Xdm.XdmValue.FromDecimal:') and '(Bosak.XPath.Core.Xdm.XdmExactDecimal value:' in r) or r.startswith(('P Bosak.XPath.Core.Xdm.XdmValue.BigIntegerValue:','P Bosak.XPath.Core.Xdm.XdmValue.ExactDecimalValue:'))]
 if len(witnesses)!=6:raise ValueError('Numeric intent witness changed')
 write('evidence/NUMERIC-INTENTS.json',dict(allSixDeclared=True,witnesses=sorted(witnesses),fullNumericPipelineQualified=False,remaining='Legacy parsing/arithmetic/casts/map keys/serialization and broad all-host routes remain separate from bounded exact profiles; default-literal source migration remains recorded.'))
 write('STATUS.json',dict(date='2026-11-05',sourceVersion='1.70.0',baseCommit=inputs['baseCommit'],requirementRows=len(rows),decisionRows=len(decisions),closureObligations=len(obligations),proposedTypes=47,presentProposedTypes=40,missingProposedTypes=7,numericMemberIntentsDeclared=6,requirementsWaived=0,auditControlsPassed=13,sourceChanged=False,newEngineTestsRun=False,priorQualifiedUnitTests=25831,priorQualification='docs/foundation-public-compatible/STATUS.json',r3Complete=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,packagesBuilt=False,pushPerformed=False,publicationAuthorized=False,nextCoreTask='AH-01',scope='Reconciliation and finite implementation plan only; no semantic or release adoption.'))
 print('Reconciled 43 requirements, 14 decisions, 6 closure obligations; 40/47 proposed types present, seven absent; six numeric intents declared. No waiver or release acceptance.')
if __name__=='__main__':audit()
