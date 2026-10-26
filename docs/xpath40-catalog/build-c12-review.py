"""Rebuild C12's bounded type, pattern and XSLT review from preserved receipts."""
from pathlib import Path
import json
C=Path(__file__).resolve().parent;E=C/'evidence/c12-semantic-batch4'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
d=load(C/'C12-REVIEW-DECISIONS.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
controls=[];unmet=[]
for family,profile in [('probe','profile'),('host','host')]:
 q={v['id']:v for v in load(E/(family+'-requests.json'))}
 obs=[json.loads(v) for v in (E/(family+'-results.jsonl')).read_text(encoding='utf-8-sig').splitlines()]
 assert {(v['id'],v[profile]) for v in obs}=={(v['id'],p) for v in q.values() for p in v['profiles']}
 for v in obs:
  r=q[v['id']];passed=r['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if r['error'] else v['error'] is None and v['result']==r['expected']
  item=dict(**v,family=family,entry=r['entry'],passed=passed,kind=r['kind'],ticket=r['ticket']);controls.append(item)
  if not passed:unmet.append(item)
assert {(v['id'],v.get('profile',v.get('host'))) for v in unmet}=={(id,p) for id in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first'] for p in ['XPath40','XPath40Experimental']}|{('switch-not-boolean-guard',p) for p in ['Legacy','Xslt40Draft']}
scope=next(v for v in load(C/'CLOSURE-PLAN.json')['workpackages'] if v['id']=='C12')['scope']['entries'];assert len(scope)==19 and set(scope)==set(d['entries'])==set(samples)
legacy={v['entry']:v for v in load(C/'ADOPTION-DISPOSITIONS.json')['entries']};entries=[]
for n in scope:
 row=d['entries'][n];sample=samples[n]
 entries.append(dict(entry=n,**row,caseIds=sample['caseIds'],caseCount=len(sample['caseIds']),sampleBlocks=sample['blocks'],controls=[v for v in controls if v['entry']==n],
  context='Exact kind/mode/input/expected/error/baseline metadata and current compiled asset identities are retained. Dedicated walkthrough input blocks and family commands are audited separately.',
  adoptionDisposition=legacy.get(n,dict(status='Outside the exact28-record legacy exception; do not extend it.'))))
walk=load(E/'walkthrough-audit.json')
summary=dict(entries=19,contractGroups=sum(len(v['contracts']) for v in entries),scopedUniqueCatalogCases=len(cases),exactCompiledAssets=len(load(E/'compiled-assets.json')['files']),sampleBlocks=0,dedicatedWalkthroughs=len(walk),walkthroughBlocks=sum(len(v['blocks']) for v in walk.values()),apiExpressions=47,hostRequests=52,supplementalObservations=len(controls),matchingControls=len(controls)-len(unmet),unmetControls=len(unmet),freshTests=dict(catalog=3148,api=296,xslt=2151,total=5595,failed=0,skipped=0))
assert (summary['contractGroups'],summary['scopedUniqueCatalogCases'],summary['exactCompiledAssets'],summary['walkthroughBlocks'])==(57,369,264,15)
out=dict(reviewDate='2026-10-26',workpackage='C12',status='Complete bounded19-entry review with explicit limitations and blockers; final owner acceptance remains C13',documentationBase='22caf4ed',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',summary=summary,entries=entries,controls=controls,unmetControls=unmet,documentationCorrections=load(E/'documentation-corrections.json'),policy='All 76 entries now have bounded C09-C12 reviews. Preserve earlier requirements, all eight current unmet controls and exact C08 approval. No engine, fixture, profile, gate, version or final acceptance change; C13 remains open.')
(C/'SEMANTIC-REVIEW-BATCH-4.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
lines=['# REQ-126 C12: final semantic and evidence batch','','**Complete bounded review — 26 October 2026.** All 76 catalog entries now have C09-C12 batch receipts. C13 remains open for blocker disposition and explicit owner acceptance; REQ-126 is still In Progress.','','This final batch reviews 19 entries and 57 contract groups. Fresh catalog 3,148 + API 296 + XSLT 2,151 = **5,595 passing tests**, zero failures/skips. Exact scope: 369 distinct catalog cases and 264 compiled assets. Six linked walkthroughs contain 15 blocks: 12 input/fixture blocks and three family commands whose selected cases are covered by the full catalog run. The 19 Sample Code sections contain no inline fences.','','The independent 47 XPath expressions and 52 host requests retain their original C05/C06 expectations. Six choice-coercion controls and two switch-comparison controls remain unmet. Passing membership, explicit cast or ordinary string dispatch does not resolve those gaps. Detailed observations and per-entry context are retained in [the review JSON](SEMANTIC-REVIEW-BATCH-4.json), [decisions](C12-REVIEW-DECISIONS.json) and [evidence manifest](evidence/c12-semantic-batch4/manifest.json).','','| Entry | Bounded decision | Cases |','|---|---|---:|']
for v in entries:lines.append(f'| [{v["entry"]}]({v["entry"]}) | {v["status"]} | {v["caseCount"]} |')
lines+=['','Current review source is immutable `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`; it is not a recovered adoption pin. Original adoption fields and C08 dispositions are unchanged. Legacy version policy, frozen Draft XPath, generic JNode/model limitations, nominal context, complete static-detection mode and package/override/transitive graph boundaries remain explicit. Delivered union-priority and type-version-gate corrections remain closed.','','Two prose corrections distinguish matching choice coercion from ordered cast, and ordinary switch dispatch from incomparable general comparison. Original entry text is preserved. No engine or executable fixture changed. Source-identical C02 full-solution 21,798 passes and frozen QT3/qt4 gate receipts are inherited; no new full-suite or gate execution is claimed.','','Reproduce validation: `python docs/xpath40-catalog/evidence/c12-semantic-batch4/validate-evidence.py --local-artifacts`. Renderer evidence is HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.','','Next: [C13 final closure review](CLOSURE-PLAN.md). Consolidate and resolve or explicitly dispose blocking receipts, then obtain the required owner acceptance of AC-01 through AC-04. Batch completion alone closes none of those criteria.','']
(C/'SEMANTIC-REVIEW-BATCH-4.md').write_text('\n'.join(lines),encoding='utf-8');print(json.dumps(summary))
