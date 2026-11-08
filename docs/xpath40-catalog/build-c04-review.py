"""Rebuild C04 bounded production/profile/context review without accepting observed gaps."""
from pathlib import Path
import collections,json
C=Path(__file__).resolve().parent;E=C/'evidence/c04-expression-review'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
decisions=load(C/'C04-REVIEW-DECISIONS.json');sources=load(E/'production-sources.json');requests=load(E/'probe-requests.json');byid={v['id']:v for v in requests}
observations=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()]
controls=[];unmet=[]
for v in observations:
 q=byid[v['id']]
 if q['kind']=='context-observation':continue
 passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result']==q['expected']
 if q['requiredPhase']:passed=passed and v['phase']==q['requiredPhase']
 control=dict(id=v['id'],profile=v['profile'],passed=passed,ticket=q['ticket']);controls.append(control)
 if not passed:unmet.append(control)
expected={(name,p) for name in ['otherwise-current-spelling','otherwise-current-guard','keyword-partial','mapping-containing-focus','mapping-containing-position'] for p in ['XPath40','XPath40Experimental']}
assert len(requests)==108 and len(observations)==227 and len(controls)==219 and {(v['id'],v['profile']) for v in unmet}==expected
prefixes={'S1-S2-S5':['signature-contracts/','changed31-contracts/'],'S3-otherwise':['otherwise-operator/'],'S3-keywords':['keyword-arguments/','signature-contracts/'],'S3-lexical':['numeric-literals/','string-templates/'],'S4-focus':['pipeline-operator/','mapping-arrow/','focus-functions/','focus-function-syntax/'],'REQ123-scan':['fn-scan/']}
catalog=load(C/'evidence/c02-function-review/catalog-case-inventory.json')
rows=[]
for key,decision in decisions['rows'].items():
 checks=[v for v in observations if byid[v['id']]['row']==key]
 rows.append(dict(key=key,**decision,retained=sources[key]['retained'],grammarFragments=sources[key]['grammar'],expressionSections=sources[key]['expressionSections'],observations=checks,unmetControls=[v for v in unmet if byid[v['id']]['row']==key],retainedCaseIds=[v['case']['id'] for v in catalog if any(v['case']['file'].startswith(p) for p in prefixes[key])],casePolicy='Existing examples retained as bounded evidence. Fresh filtered tests and source hashes supplement earlier full-catalog proof; case lists are not full-production semantic acceptance.'))
followups=[]
for id,definition in decisions['tickets'].items():
 followups.append(dict(id=id,**definition,observations=[v for v in observations if byid[v['id']]['ticket']==id],unmetControls=[v for v in unmet if v['ticket']==id]))
summary=dict(grammarRows=6,uniqueProductionOrTokenNames=len({v['name'] for row in sources.values() for v in row['grammar']}),structuredEntries=9,retainedPages=11,probeExpressions=108,profileObservations=227,assertedControls=219,matchingControls=209,unmetControls=10,contextObservations=8,phaseCounts=dict(collections.Counter(v['phase'] for v in observations)),freshTests=dict(api=289,parser=202,hostCatalog=389,total=880),followupGroups=5,inheritedSolutionTests=21798,inheritedCatalogExamples=3148)
out=dict(reviewDate='2026-10-26',workpackage='C04',status='Complete bounded production review; observed defects and acceptance remain open',documentationBase='bdd303d0',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',engineVersion='1.26.3',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',summary=summary,rows=rows,followups=followups,namedTests=load(E/'named-test-evidence.json'),preservationPolicy='Keep C01-C03 semantic evidence and C00 gate baselines. Do not turn draft spelling drift, passing observed limits, current gated membership or an open host static context into acceptance. C02 atomic annotation defect remains open. C08 historical adoption pins are not reconstructed; C09-C12/C13 retain applicable-evidence/owner acceptance.')
(C/'EXPRESSION-CALL-FOCUS-RECONCILIATION.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
