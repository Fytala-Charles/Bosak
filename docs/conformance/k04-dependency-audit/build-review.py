"""Rebuild the read-only K04 applicability audit from actual current filter receipts."""
from pathlib import Path
import json,collections,re
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def lines(p):return [json.loads(s) for s in p.read_text(encoding='utf-8-sig').splitlines()]
filterReceipts=load(E/'filter-process-receipts.json');assert len(filterReceipts)==4
for r in filterReceipts:assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==30 and len(lines(E/r['output']))==r['rows']
qt3=lines(E/'qt3-catalog.jsonl');qt4=lines(E/'qt4-catalog.jsonl');assert len(qt3)==619 and len(qt4)==1893 and not any(r['normalAdmitted'] for r in qt3+qt4)
assert len({(r['suite'],r['name']) for r in qt3+qt4})==2512
synthetic=lines(E/'qt3-synthetic.jsonl')+lines(E/'qt4-synthetic.jsonl');assert len(synthetic)==62
routing=[r for r in qt3 if r['xquerySupported'] and not r['xquerySyntaxEligible']];assert len(routing)==246
quotes=[r for r in routing if chr(34)*3 in r['expression']];assert len(quotes)==16
(E/'quote-candidates.json').write_text(json.dumps(quotes,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
axes=collections.Counter((r['xquerySupported'],r['xquerySyntaxEligible']) for r in qt3);assert axes=={(False,True):367,(True,False):246,(False,False):6}
executions=load(E/'execution-results.json');assert len(executions)==32
for r in executions:assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 and r['result']
pairs=collections.defaultdict(dict)
for r in executions:pairs[r['name']][r['mode']]=r['result']
assert len(pairs)==16 and all(set(p)=={'actual','public-xquery'} for p in pairs.values())
assert collections.Counter(p['actual']['kind'] for p in pairs.values())=={'Passed':2,'Failed':2,'Skipped':12}
assert collections.Counter(p['public-xquery']['kind'] for p in pairs.values())=={'Passed':6,'Failed':10}
quoteProof=['K2-DirectConElemAttr-8','Constr-attr-syntax-5','DirectConElemAttr-1','K2-DirectConElemAttr-29','Literals069']
assert all(pairs[n]['public-xquery']['kind']=='Passed' for n in quoteProof)
assert all(pairs[n]['public-xquery']['kind']=='Failed' for n in pairs if n.startswith('contextDecl-'))
summary=dict(qt3DependencySkips=619,qt4GatedDependencySkips=1893,totalCatalogAssessments=2512,syntheticAssessments=62,filterProcesses=4,routingOnlyQt3Cases=246,tripleQuoteRoutingCandidates=16,executionCases=16,executionProcesses=32,publicXqueryPassed=6,publicXqueryFailed=10,quoteCasesPublicPassed=5,timeouts=0,engineChanged=False,filterPolicyChanged=False,baselineChanged=False,inheritedSolutionPasses=22007,inheritedQt3=[31145,0,676],inheritedQt4=[8437,0,1893],candidate='1.26.11',published='1.26.3')
rejects={}
for suite,rows in [('qt3',qt3),('qt4',qt4)]:
 c=collections.Counter(a['type']+':'+a['value']+(' [negative]' if not a['satisfied'] else '') for r in rows for a in r['assessments'] if not a['xpath'] and (suite=='qt4' or not a['xquery']))
 rejects[suite]=dict(c.most_common())
findings=[dict(id='K04-F01',status='Open',scope='Quote-aware routing',evidence='Sixteen dependency-admissible QT3 cases are refused by a raw triple-double-quote test. Five original quote cases pass through public XQuery, including four positive assertions and one malformed-constructor error assertion.',next='K04-C01: isolate quote-aware routing, verify all16 candidates and negative controls, then rehearse the full runner before any exact QT3 baseline proposal.'),dict(id='K04-F02',status='Open',scope='Unsupported prolog/pragma boundaries',evidence='All seven sampled context declarations, construction strip, valid copy-namespaces and pragma controls fail original assertions through public XQuery. Invalid copy-namespaces syntax also passes an expected parse error, which is not implementation support.',next='Keep broader guards until each construct has independently verified engine semantics.'),dict(id='K04-F03',status='Open',scope='Polarity and explicit profiles',evidence='Synthetic negative mixed XP/XQ spec dependencies are admitted despite matching XPath tokens; contradictory positive/negative XQ31 predicates pass the admitted-XQuery filter. Negative fr-CA is rejected despite positive fr-CA rejection. Unknown positive features default to supported.',next='Define/profile-test polarity, known/unknown capabilities and language routing before policy changes; synthetic observations are not legal unskip evidence.'),dict(id='K04-F04',status='Open',scope='Schema ownership mismatch',evidence='Both filters admit xsd-version1.1 and contain comments asserting XSD1.1 implementation. The current core representation is XmlSchemaSet; Bosak.Schema owns staged XSD1.1.',next='Correct capability wording and coordinate explicit schema profiles with Bosak.Schema; do not convert the existing predicate into a schema compliance claim.'),dict(id='K04-F05',status='Open',scope='qt4 XQuery/profile exclusions',evidence='Most gatedqt4 dependency skips mention XQuery-only versions;1608 mention XQ10+ among their rejected predicates. The qt4 harness is XPath-only. Feature and version rejection counts overlap.',next='Keep XQuery4/version/profile exclusions separate from core implementation bugs; no blanket admission or broader full-catalog execution.')]
out=dict(date='2026-10-27',sourceBase=load(E/'AUDIT-PIN.json')['sourceBase'],status='K04 initial applicability audit complete; five bounded follow-ups remain open',summary=summary,qt3RoutingAxes=[dict(xqueryDependencies=xq,syntaxEligible=syntax,cases=n) for (xq,syntax),n in axes.items()],effectiveRejectedMentions=rejects,countPolicy='Dependency mentions overlap. QT3 counts use actual admitted-XQuery single-predicate results; routing axes are separate. CanHandleAsXQuery is syntax eligibility, not proof of semantic support.',findings=findings,executionPairs=pairs,synthetic=synthetic,next='K04-C01 quote-aware routing correction and complete candidate/rehearsal evidence before any baseline-policy proposal.',policy='Read-only audit. Direct executor/public API probes bypass admission diagnostically but retain original assertions and constrained resource-free environments. Public XQuery comparison uses no output-option overrides for this selected sample. No engine, harness, fixture, assertion, filter, baseline, package, release or REQ-126 acceptance change. All10 unmet public observations remain failures.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
