"""Rebuild the bounded C06 host/profile review from preserved observations."""
from pathlib import Path
import json

C = Path(__file__).resolve().parent
E = C / 'evidence/c06-host-profile-review'

def load(p):
    return json.loads(p.read_text(encoding='utf-8-sig'))

decisions = load(C / 'C06-REVIEW-DECISIONS.json')
sources = load(E / 'production-sources.json')
results = {}
for lane in ['host', 'registration']:
    requests = load(E / (lane + '-requests.json'))
    observed = [json.loads(v) for v in (E / (lane + '-results.jsonl')).read_text(encoding='utf-8-sig').splitlines()]
    controls, diagnostics, unmet = [], [], []
    for v in observed:
        q = next(q for q in requests if q['id'] == v['id'] and (lane == 'host' or q['profile'] == v['profile']))
        if q.get('kind') == 'context-observation':
            diagnostics.append(v)
            continue
        passed = q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result'] == q['expected']
        item = dict(**v, passed=passed)
        controls.append(item)
        if not passed:
            unmet.append(item)
    results[lane] = dict(requestCount=len(requests), observationCount=len(observed), controls=controls, diagnostics=diagnostics, unmet=unmet)

assert (results['host']['requestCount'], results['host']['observationCount']) == (60, 100)
assert {(v['id'], v['host']) for v in results['host']['unmet']} == {('switch-not-boolean-guard', p) for p in ['Legacy', 'Xslt40Draft']}
assert results['registration']['observationCount'] == 54 and not results['registration']['unmet']
summary = dict(grammarRows=3, retainedPages=9, mappedForms=sum(len(v['forms']) for v in sources.values()), hostStylesheets=60,
               hostObservations=100, registrationObservations=54, totalObservations=154, assertedControls=135,
               matchingControls=133, unmetControls=2, contextObservations=19,
               freshFocusedTests=376, freshCatalogExamples=3148, freshTestTotal=3524, inheritedSolutionTests=21798)
assert sum(len(v['controls']) for v in results.values()) == 135
assert sum(len(v['diagnostics']) for v in results.values()) == 19
out = dict(reviewDate='2026-10-26', workpackage='C06', status='Complete bounded reconciliation; acceptance remains partial',
           documentationBase='19d9e1a9', engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',
           reviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c', summary=summary,
           rows=[dict(key=k, **d, retained=sources[k]['retained'], normativeSections=sources[k]['sections']) for k, d in decisions['rows'].items()],
           observations=results, followups=decisions['tickets'], namedTests=load(E / 'named-test-evidence.json'),
           policy='All 18 grammar rows now have bounded reviews, not exhaustive grammar coverage or semantic acceptance. Preserve original rows, earlier reviews, gates and defects. C07-C13 remain open; no historical adoption waiver or XQuery4/Experimental promotion.')
(C / 'HOST-PROFILE-RECONCILIATION.json').write_text(json.dumps(out, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
print(json.dumps(summary))
