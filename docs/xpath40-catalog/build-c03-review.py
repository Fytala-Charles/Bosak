"""Rebuild C03 constructor/type/coercion evidence without changing earlier checkpoints."""
from pathlib import Path
import collections,json
C=Path(__file__).resolve().parent;E=C/'evidence/c03-type-review'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
decisions=load(C/'C03-REVIEW-DECISIONS.json');sources=load(E/'entry-contract-sources.json');requests=load(E/'probe-requests.json')
observations=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()];byid={v['id']:v for v in requests}
hostrequests=load(E/'host-requests.json');hostobservations=[json.loads(v) for v in (E/'host-results.jsonl').read_text().splitlines()]
controls=[]
for v in observations:
 q=byid[v['id']]
 if q['expected'] is None and q['error'] is None:continue
 passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result']==q['expected']
 controls.append(dict(id=v['id'],profile=v['profile'],passed=passed))
assert len(observations)==1337 and len(controls)==1315 and all(v['passed'] for v in controls)
hostcontrols=[]
for q in hostrequests:
 for v in [v for v in hostobservations if v['id']==q['id']]:
  if q['expected'] is not None:hostcontrols.append(dict(id=q['id'],host=v['host'],passed=v['error'] is None and v['result']==q['expected']))
assert len(hostobservations)==20 and len(hostcontrols)==18 and all(v['passed'] for v in hostcontrols)
old=load(E/'retained-focus-audit.json');inventory=load(C/'CURRENT-FUNCTION-INVENTORY.json');live={v['name']:v for v in inventory['functions']}
constructors=[]
for row in old['constructors']:
 name=row['name'];prefix='focus-'+name[3:]+'-';checks=[v for v in observations if v['id'].startswith(prefix)]
 assert len(checks)==23
 constructors.append(dict(name=name,lexicalInput=row['lexicalInput'],retainedRepresentativeCase=row['validCase'],liveSignatures=live[name]['registeredSignatures'],observations=checks,uninhabited=name=='xs:error',assessment='Representative value/type/invalid/context/ref/lookup/node checks pass; no exhaustive facet/cast or nonempty xs:error construction claim.'))
assert len(constructors)==49
entries=[dict(entry=name,**v,sourceGitBlob=sources[name]['gitBlob'],acceptance='Bounded reconciliation only; C09-C12 retain per-contract applicable-evidence review and C13 retains owner acceptance.') for name,v in sorted(decisions['entries'].items())]
summary=dict(entries=8,focusConstructorNames=49,focusMatrixObservations=1127,xpathExpressions=len(requests),xpathObservations=1337,xpathAssertedControls=1315,xpathDiagnostics=22,hostStylesheets=len(hostrequests),hostObservations=20,hostAssertedControls=18,hostDiagnostics=2,totalObservations=1357,totalAssertedControls=1333,unmetControls=0,freshUnitTests=dict(schema=70,api=119,host=56,total=245),inheritedSolutionTests=21798,inheritedCatalogExamples=3148)
out=dict(reviewDate='2026-10-26',workpackage='C03',status='Complete bounded review; no acceptance criterion closed',documentationBase='cc6495c3',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',engineVersion='1.26.3',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',historicalAdoption='28 missing historical revisions remain C08 work; current review is not adoption evidence.',summary=summary,entries=entries,constructors=constructors,nonConstructorChecks=[dict(request=q,observations=[v for v in observations if v['id']==q['id']]) for q in requests if q['group']!='focus'],nominalHostChecks=[dict(request=q,observations=[v for v in hostobservations if v['id']==q['id']]) for q in hostrequests],boundaries=decisions['boundaries'],namedTests=load(E/'named-test-evidence.json'),deliveredCorrections=['Floating signed/unsigned overflow thresholds','Unsigned decimal/floating truncation','Integer arithmetic/unary/modulo identity fixes','Repeated/empty unary conversion','Inline record-signature old-profile gate','Published abs/floor/ceiling integer preservation'],evidencePolicy='Controls retain their exact expected values/error markers. Fresh probes and 245 focused unit tests supplement unchanged C02 full-solution/catalog results. Diagnostics, type membership and example passage do not establish arbitrary precision, full schema/host/type conversion or entry/criterion acceptance.')
(C/'CONSTRUCTOR-TYPE-RECONCILIATION.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(summary))
