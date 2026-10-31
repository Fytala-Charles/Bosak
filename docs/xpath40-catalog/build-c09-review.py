"""Rebuild C09's per-entry semantic/evidence receipts without accepting unmet contracts."""
from pathlib import Path
import json

C=Path(__file__).resolve().parent;E=C/'evidence/c09-semantic-batch1'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
decisions=load(C/'C09-REVIEW-DECISIONS.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
q={v['id']:v for v in load(E/'probe-requests.json')};observed=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
controls=[];unmet=[]
for v in observed:
    req=q[v['id']];passed=req['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if req['error'] else v['error'] is None and v['result']==req['expected']
    row=dict(**v,passed=passed,kind=req['kind'],ticket=req['ticket']);controls.append(row)
    if not passed:unmet.append(row)
expected={(id,p) for id in ['jnode-generic-required','extrema-required-mixed','extrema-identity-required','partial-nonpositive-draft'] for p in ['XPath40','XPath40Experimental']}
assert len(q)==28 and len(observed)==54 and {(v['id'],v['profile']) for v in unmet}==expected
scope=next(v for v in load(C/'CLOSURE-PLAN.json')['workpackages'] if v['id']=='C09')['scope']['entries']
assert len(scope)==19 and set(scope)==set(decisions['entries'])==set(samples)
legacy={v['entry']:v for v in load(C/'ADOPTION-DISPOSITIONS.json')['entries']}
entries=[]
for name in scope:
    d=decisions['entries'][name];sample=samples[name]
    for contract in d['contracts']:
        assert contract['caseIds'] or contract['probeIds'] or contract['inheritedEvidence']
        assert set(contract['caseIds'])<=set(sample['caseIds']) and set(contract['probeIds'])<=set(q)
    entries.append(dict(entry=name,**d,caseIds=sample['caseIds'],caseCount=len(sample['caseIds']),
                        sampleBlocks=sample['blocks'],context='Exact manifest kind/mode/input/resource/profile/baseline fields are preserved in applicable-cases.json; fixtures match compiled assets byte-for-byte.',
                        supplementalControls=[v for v in controls if q[v['id']]['entry']==name],
                        adoptionDisposition=legacy.get(name,dict(status='Outside the original28-record legacy exception; do not extend that policy. Current review and original provenance remain distinct.'))))
summary=dict(entries=19,contractGroups=sum(len(v['contracts']) for v in entries),scopedUniqueCatalogCases=len(cases),
             sampleBlocks=sum(len(v['sampleBlocks']) for v in entries),walkthroughBlocks=len(load(E/'sample-walkthrough.json')['blocks']),
             exactCompiledAssets=len(load(E/'compiled-assets.json')['files']),supplementalExpressions=28,supplementalObservations=54,
             matchingControls=46,unmetControls=8,freshTests=dict(catalog=3148,standard=182,host=117,total=3447,failed=0,skipped=0))
assert (summary['contractGroups'],summary['scopedUniqueCatalogCases'],summary['sampleBlocks'],summary['walkthroughBlocks'])==(67,536,116,8)
out=dict(reviewDate='2026-10-26',workpackage='C09',status='Complete19-entry review with explicit limitations/blockers; final acceptance remains open',
         documentationBase='34080f04',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',
         summary=summary,entries=entries,controls=controls,unmetControls=unmet,documentationCorrections=load(E/'documentation-corrections.json'),
         policy='A passing observed/refusal/example result is not draft conformance. Preserve all8 independent unmet controls, prior defects and frozen/adoption limits. C08 approval is only the original28-record historical policy. C10-C13 remain open; no entry is unconditionally owner-accepted here.')
(C/'SEMANTIC-REVIEW-BATCH-1.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
