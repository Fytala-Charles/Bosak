"""Rebuild C05 bounded type/collection/pattern review with independent unmet controls."""
from pathlib import Path
import collections,json
C=Path(__file__).resolve().parent;E=C/'evidence/c05-collection-pattern-review'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
decisions=load(C/'C05-REVIEW-DECISIONS.json');sources=load(E/'production-sources.json');requests=load(E/'probe-requests.json');byid={v['id']:v for v in requests}
obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()];hostrequests=load(E/'host-requests.json');hosts={v['id']:v for v in hostrequests};hostobs=[json.loads(v) for v in (E/'host-results.jsonl').read_text().splitlines()]
controls=[];unmet=[]
for v in obs:
 q=byid[v['id']];passed=q['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if q['error'] else v['error'] is None and v['result']==q['expected']
 if q['requiredPhase']:passed=passed and v['phase']==q['requiredPhase']
 item=dict(id=v['id'],profile=v['profile'],passed=passed,ticket=q['ticket']);controls.append(item)
 if not passed:unmet.append(item)
expected={(name,p) for name in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first','destructure-record-undeclared','jnode-generic-node','jnode-identity','xnode-xml'] for p in ['XPath40','XPath40Experimental']}
assert len(requests)==111 and len(obs)==219 and {(v['id'],v['profile']) for v in unmet}==expected
hostcontrols=[];hostunmet=[]
for v in hostobs:
 q=hosts[v['id']];passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result']==q['expected']
 item=dict(id=v['id'],host=v['host'],passed=passed);hostcontrols.append(item)
 if not passed:hostunmet.append(item)
assert len(hostrequests)==28 and len(hostobs)==56 and {(v['id'],v['host']) for v in hostunmet}=={('jnode-pattern','Legacy'),('jnode-pattern','Xslt40Draft')}
rows=[]
for key,decision in decisions['rows'].items():
 rows.append(dict(key=key,**decision,retained=sources[key]['retained'],linkedPages=sources[key]['linkedPages'],grammar=sources[key]['grammar'],normativeSections=sources[key]['sections'],xpathObservations=[v for v in obs if byid[v['id']]['row']==key],hostObservations=[v for v in hostobs if hosts[v['id']]['row']==key],unmetControls=[v for v in unmet if byid[v['id']]['row']==key],unmetHostControls=[v for v in hostunmet if hosts[v['id']]['row']==key]))
summary=dict(grammarRows=9,uniqueProductionOrTokenNames=len({v['name'] for row in sources.values() for v in row['grammar']}),retainedPages=16,linkedPages=5,structuredEntries=17,xpathExpressions=111,xpathObservations=219,xpathMatchingControls=205,xpathUnmetControls=14,hostStylesheets=28,hostObservations=56,hostMatchingControls=54,hostUnmetControls=2,totalObservations=275,matchingControls=259,unmetControls=16,knownModelBoundaryControls=8,choiceCoercionControls=6,recordDestructuringControls=2,freshTests=dict(api=323,jnodeMembers=89,patterns=2045,total=2457),inheritedSolutionTests=21798,inheritedCatalogExamples=3148)
out=dict(reviewDate='2026-10-26',workpackage='C05',status='Complete bounded reconciliation; defects/model limits and acceptance remain open',documentationBase='2d8a7005',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',engineVersion='1.26.3',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',summary=summary,rows=rows,followups=decisions['tickets'],namedTests=load(E/'named-test-evidence.json'),newNameAssignments={'array:members':dict(entry='data-model/jnode-navigation.md',role='One ordered member JNode per array member, preserving sequence boundaries'),'array:of-members':dict(entry='data-model/jnode-navigation.md',role='One array member per supplied JNode, preserving its wrapped value')},policy='Keep all independent expectations. Eight unmet generic-model controls record known unsupported scope; six choice and two record-destructuring controls are reproduced current-draft mismatches. No conformance promotion, reconstructed history, full-model adoption or semantic acceptance follows. C02/C04 defects remain open; C06/C09-C13 retain host/final acceptance work.')
(C/'TYPE-COLLECTION-PATTERN-RECONCILIATION.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
