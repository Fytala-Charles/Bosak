"""Rebuild C10 with explicit semantic limits and a separate specification-consistency finding."""
from pathlib import Path
import json
C=Path(__file__).resolve().parent;E=C/'evidence/c10-semantic-batch2'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
d=load(C/'C10-REVIEW-DECISIONS.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
requests=load(E/'probe-requests.json');q={v['id']:v for v in requests};obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
controls=[];diagnostics=[];unmet=[]
for v in obs:
    req=q[v['id']]
    if req['kind']=='context-observation':diagnostics.append(v);continue
    passed=req['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if req['error'] else v['error'] is None and v['result']==req['expected']
    item=dict(**v,passed=passed,kind=req['kind'],ticket=req['ticket']);controls.append(item)
    if not passed:unmet.append(item)
names=['cleanup-grapheme-required','compare-draft-keywords','calendar-fraction-required','numeric-annotation-empty-required','map-filter-required-boolean','ranking-published-example']
assert len(requests)==37 and len(obs)==74 and len(diagnostics)==2
assert {(v['id'],v['profile']) for v in unmet}=={(id,p) for id in names for p in ['XPath40','XPath40Experimental']}
scope=next(v for v in load(C/'CLOSURE-PLAN.json')['workpackages'] if v['id']=='C10')['scope']['entries'];assert len(scope)==19 and set(scope)==set(d['entries'])==set(samples)
legacy={v['entry']:v for v in load(C/'ADOPTION-DISPOSITIONS.json')['entries']};entries=[]
for n in scope:
    row=d['entries'][n];sample=samples[n]
    for contract in row['contracts']:assert set(contract['caseIds'])<=set(sample['caseIds'])<=set(cases) and set(contract['probeIds'])<=set(q)
    entries.append(dict(entry=n,**row,caseIds=sample['caseIds'],caseCount=len(sample['caseIds']),sampleBlocks=sample['blocks'],
                        context='Full kind/mode/input/expected/error/baseline metadata and truncated TRX display candidates are preserved; exact source assets match compiled copies.',
                        controls=[v for v in controls if q[v['id']]['entry']==n],contextObservations=[v for v in diagnostics if q[v['id']]['entry']==n],
                        adoptionDisposition=legacy.get(n,dict(status='Outside the exact28-record legacy exception; do not extend it.'))))
summary=dict(entries=19,contractGroups=sum(len(v['contracts']) for v in entries),scopedUniqueCatalogCases=len(cases),exactCompiledAssets=len(load(E/'compiled-assets.json')['files']),
             sampleBlocks=sum(len(v['sampleBlocks']) for v in entries),pythonOracles=1,supplementalExpressions=37,supplementalObservations=74,assertedControls=72,
             matchingControls=60,unmetControls=12,currentDraftUnmetControls=10,disputedSpecExampleControls=2,contextObservations=2,
             freshTests=dict(catalog=3148,standard=321,total=3469,failed=0,skipped=0))
assert (summary['contractGroups'],summary['scopedUniqueCatalogCases'],summary['sampleBlocks'],summary['exactCompiledAssets'])==(55,892,82,893)
out=dict(reviewDate='2026-10-26',workpackage='C10',status='Complete19-entry review; limitations, current-draft mismatches and source/example consistency remain open',documentationBase='06b53349',
         engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',summary=summary,entries=entries,controls=controls,unmetControls=unmet,
         specificationConsistency=load(E/'review-sources.json')['consistencyIssue'],documentationClarification=load(E/'documentation-corrections.json'),
         policy='No observed/ignored/refused result becomes full draft conformance. Retain10 required draft mismatches and2 disputed worked-example mismatches separately; the latter alone do not diagnose an engine defect. C08 remains only its original28-record policy. C11-C13 and final owner acceptance remain open.')
(C/'SEMANTIC-REVIEW-BATCH-2.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
