"""Rebuild C08 research while leaving historical adoption and owner policy unresolved."""
from pathlib import Path
import hashlib,json,re

C=Path(__file__).resolve().parent
E=C/'evidence/c08-adoption-research'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
context=load(E/'research-context.json');sources=load(E/'entry-sources.json')
approval=load(E/'owner-approval.json')
assert approval['decision']=='Approved' and approval['answer']=='Approve the scoped legacy policy'
assert approval['policyAsPresentedSha256']==hashlib.sha256((E/'policy-as-presented.md').read_bytes()).hexdigest()
assert set(approval['exactScope'])==set(sources)
deliveries=load(E/'delivery-research.json')['records'];heads=load(E/'head-research.json')
expanded=load(E/'expanded-source-search.json');history=load(E/'historical-change-receipts.json')
alltrees=load(E/'all-tracked-source-search.json');allhistory=load(E/'all-tracked-history-receipts.json')
prs={v['number']:v for v in [load(p) for p in sorted(E.glob('pr-*.json'))]}
assert len(prs)==16 and len(deliveries)==len(heads)==16 and len(history)==57 and len(sources)==28
assert len(expanded['records'])==32 and all(v['objectRetained'] for v in heads)
assert len(alltrees['records'])==32 and len(allhistory['records'])==58
candidate=re.compile(r'qtspecs/(?:blob|commit)/[0-9a-f]{8,40}|adoptedSpecRevision|[Ss]pec.{0,40}(?:[Rr]evision|[Ss]napshot|[Cc]ommit|[Hh]ash).{0,20}\b[0-9a-f]{40}\b')
assert not any(candidate.search(line.split(':',3)[-1]) for v in expanded['records'] for line in v['output'].splitlines())
assert not any(candidate.search(line) for v in history for line in v['matchingChangedLines'])
assert not any(candidate.search(v['body']) for v in prs.values())
assert not any(candidate.search(line.split(':',3)[-1]) for v in alltrees['records'] for line in v['output'].splitlines())
assert not any(candidate.search(line) for v in allhistory['records'] for line in v['matchingChangedLines'])
entries=[]
for name,s in sources.items():
    original=s['originalRecord'];pr=prs[original['bosakPr']]
    assert pr['mergeCommit']['oid']==original['bosakRevision']
    nonadoption=name in ['operators-and-syntax/method-call-operator.md','rejected/mapping-arrow-gtgt.md']
    entries.append(dict(entry=name,originalRecord=original,sourceEntryGitBlob=s['gitBlob'],
                        finding='No historical implementation adoption for this documented shape' if nonadoption else 'Historical adoption snapshot not recovered from researched records',
                        adoptedSpecRevision=None,ownerDisposition='Approved 2026-10-26; historical null retained',
                        disposition='Non-adoption; implementation-adoption pin not applicable' if nonadoption else 'Legacy provenance exception; retain null and require pinned semantic review',
                        prReceipt=f"evidence/c08-adoption-research/pr-{pr['number']}.json",deliveryRevision=original['bosakRevision'],
                        headRevision=pr['headRefOid'],evidenceLimit='Current PR text, immutable Bosak source and fixture revisions do not establish an adopted specification revision.'))
summary=dict(originalMissingRecords=28,recoveredAdoptionPins=0,ownerDispositionsApplied=28,unresolvedDispositionRecords=0,
             legacyExceptions=26,nonAdoptions=2,originalPullRequests=16,deliveryTrees=16,retainedHeadTrees=16,
             intermediateMatchingCommits=58,sourceScopedMatchingCommits=57,allTrackedTreeSearches=32,
             prComments=sum(len(v['comments']) for v in prs.values()),prReviews=sum(len(v['reviews']) for v in prs.values()))
out=dict(reviewDate='2026-10-26',workpackage='C08',status='Complete research and explicitly approved legacy disposition; historical facts remain missing',
         documentationBase=context['documentationBase'],engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',summary=summary,
         entries=entries,approvedPolicy='LEGACY-ADOPTION-POLICY-PROPOSAL.md',ownerDecision=approval,limits=context['limits'],
         policy='Preserve all28 original null adoption fields and original C00-C07 receipts. Explicit user approval grants only26 legacy provenance exceptions and2 non-adoption dispositions, not recovered facts or semantic acceptance. Five packages remain open; C09-C13 retain per-contract review, defects and final acceptance.')
(C/'ADOPTION-HISTORY-RESEARCH.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
(C/'ADOPTION-DISPOSITIONS.json').write_text(json.dumps(dict(date='2026-10-26',ownerApproval='evidence/c08-adoption-research/owner-approval.json',originalLedger='provenance.json',originalLedgerUnchanged=True,recoveredAdoptionPins=0,legacyExceptions=26,nonAdoptions=2,entries=[dict(entry=v['entry'],adoptedSpecRevision=None,disposition=v['disposition'],conditions=approval['conditions']) for v in entries]),indent=2)+'\n',encoding='utf-8')
print(json.dumps(summary))
