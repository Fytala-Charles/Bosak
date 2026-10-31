"""Rebuild C11's exact grammar/function review without relaxing known requirements."""
from pathlib import Path
import json
C=Path(__file__).resolve().parent;E=C/'evidence/c11-semantic-batch3'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
d=load(C/'C11-REVIEW-DECISIONS.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
q={v['id']:v for v in load(E/'probe-requests.json')};obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
controls=[];unmet=[];diagnostics=[]
for v in obs:
    r=q[v['id']]
    if r['kind']=='context-observation':diagnostics.append(v);continue
    passed=r['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if r['error'] else v['error'] is None and v['result']==r['expected']
    item=dict(**v,passed=passed,kind=r['kind'],ticket=r['ticket']);controls.append(item)
    if not passed:unmet.append(item)
names=['uri-empty-query-required','sort-empty-key','array-sort-empty-key','sort-unknown-collation','sort-exact','sort-nan-left','destructure-record-required','keyword-placeholder-required','mapping-containing-required','otherwise-word-required']
assert len(q)==48 and len(obs)==99 and len(diagnostics)==2
assert {(v['id'],v['profile']) for v in unmet}=={(id,p) for id in names for p in ['XPath40','XPath40Experimental']}
scope=next(v for v in load(C/'CLOSURE-PLAN.json')['workpackages'] if v['id']=='C11')['scope']['entries'];assert len(scope)==19 and set(scope)==set(d['entries'])==set(samples)
legacy={v['entry']:v for v in load(C/'ADOPTION-DISPOSITIONS.json')['entries']};entries=[]
for n in scope:
    row=d['entries'][n];sample=samples[n]
    for contract in row['contracts']:assert set(contract['caseIds'])<=set(sample['caseIds'])<=set(cases) and set(contract['probeIds'])<=set(q)
    entries.append(dict(entry=n,**row,caseIds=sample['caseIds'],caseCount=len(sample['caseIds']),sampleBlocks=sample['blocks'],
                        context='Exact manifest kind/mode/input/focus/expected/error/baseline metadata is retained; compiled asset and source identities establish current execution.',
                        controls=[v for v in controls if q[v['id']]['entry']==n],contextObservations=[v for v in diagnostics if q[v['id']]['entry']==n],
                        adoptionDisposition=legacy.get(n,dict(status='Outside the exact28-record legacy exception; do not extend it.'))))
summary=dict(entries=19,contractGroups=sum(len(v['contracts']) for v in entries),scopedUniqueCatalogCases=len(cases),exactCompiledAssets=len(load(E/'compiled-assets.json')['files']),sampleBlocks=sum(len(v['sampleBlocks']) for v in entries),
             verbatimInlineQueries=7,supplementalExpressions=48,supplementalObservations=99,assertedControls=97,matchingControls=77,unmetControls=20,contextObservations=2,
             freshTests=dict(catalog=3148,api=323,standard=285,total=3756,failed=0,skipped=0))
assert (summary['contractGroups'],summary['scopedUniqueCatalogCases'],summary['exactCompiledAssets'],summary['sampleBlocks'])==(56,874,764,26)
out=dict(reviewDate='2026-10-26',workpackage='C11',status='Complete19-entry review with explicit grammar/function limitations and blockers; owner acceptance remains open',documentationBase='00ee9bf',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',
         reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',summary=summary,entries=entries,controls=controls,unmetControls=unmet,documentationCorrections=load(E/'documentation-corrections.json'),
         policy='Preserve all20 current-draft unmet controls, source/profile boundaries, earlier defects and the exact C08 historical policy. Split independent inline examples without changing expressions; correct the flattened result comment and qualify existing maps/epoch/focus limits. C12/C13 remain open; no final semantic acceptance or unsupported implementation.')
(C/'SEMANTIC-REVIEW-BATCH-3.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
