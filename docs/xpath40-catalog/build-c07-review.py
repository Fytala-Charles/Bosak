"""Rebuild C07 lifecycle dispositions without promoting support or accepting contracts."""
from pathlib import Path
import collections,json,re

C=Path(__file__).resolve().parent
E=C/'evidence/c07-lifecycle-review'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))

decisions=load(C/'C07-REVIEW-DECISIONS.json')
sources=load(E/'entry-sources.json')
requests=load(E/'probe-requests.json');byid={v['id']:v for v in requests}
observed=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
controls=[]
for v in observed:
    q=byid[v['id']]
    passed=q['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if q['error'] else v['error'] is None and v['result']==q['expected']
    controls.append(dict(**v,passed=passed))
assert len(requests)==36 and len(observed)==113 and all(v['passed'] for v in controls)
assert set(sources)==set(decisions['entries']) and len(sources)==76
entries=[]
for name,d in decisions['entries'].items():
    s=sources[name]
    text=(C/name).read_text(encoding='utf-8')
    current=re.search(r'^## 1\. Name and Classification\s*(.*?)(?=^## \d+\.|\Z)',text,re.M|re.S).group(1).strip()
    entries.append(dict(entry=name,**d,currentClassificationText=current,previousClassificationText=s['classification'],
                        baselineEntryGitBlob=s['gitBlob'],manifestIds=s['manifestIds'],codeFiles=s['localCodeFiles'],
                        deliveryOrDispositionRefs=s['deliveryOrDispositionRefs'],priorReviews=s['priorReviews'],
                        probeControls=[v for v in controls if byid[v['id']]['entry']==name]))
commits=load(E/'commit-evidence.json')
summary=dict(entries=76,classificationCounts=dict(collections.Counter(v['classification'] for v in entries)),
             originalAndWiderGroups=19,originalMovingBullets=6,originalRejectedGroups=5,widerGroups=9,
             repositoryRevisionRecords=len(commits),mainlineAncestorRecords=sum(v['ancestorOfDocumentationBase'] for v in commits.values()),
             historicalBranchRecords=sum(not v['ancestorOfDocumentationBase'] for v in commits.values()),
             mappedCodeFiles=len(load(E/'source-files.json')),linkedManifestCases=len({n for v in entries for n in v['manifestIds']}),
             xpathExpressions=36,profileControls=113,matchingControls=113,unmetControls=0,
             freshTests=dict(api=296,host=75,catalog=3148,total=3519,failed=0,skipped=0),
             inheritedSolutionTests=21798,missingAdoptionPins=28)
out=dict(reviewDate='2026-10-26',workpackage='C07',status='Complete bounded lifecycle reconciliation; acceptance remains partial',
         documentationBase='d00a8fb0',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',summary=summary,
         entries=entries,originalGroups=decisions['originalGroups'],followups=decisions['followups'],
         documentationCorrections=load(E/'documentation-corrections.json'),controls=controls,
         policy='Directory names, gate membership and a current review pin do not determine adoption. Preserve historical URLs, original references and C00-C06 evidence. No entry or AC01-04 is accepted here; C08/C13 retain adoption-policy blockers and C09-C13 retain semantic review. No deferred implementation commitment.')
(C/'LIFECYCLE-RECONCILIATION.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(summary))
