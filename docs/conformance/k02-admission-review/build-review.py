"""Build a read-only guarded QT3 adoption review from full-runner receipts."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def skips(path):
 result={};reason=None
 for line in path.read_text(encoding='utf-8-sig').splitlines():
  match=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
  if match:reason=match.group(2)
  elif line.strip():assert reason is not None and line not in result;result[line]=reason
 return result
rows=load(E/'full-run-receipts.json');assert len(rows)==2
byMode={r['mode']:r for r in rows}
for mode,expected in [('current',(31144,0,677)),('guarded',(31145,0,676))]:
 r=byMode[mode];assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==300
 a=r['result'];assert a['total']==31821 and (a['passed'],a['failed'],a['skipped'])==expected
 assert not a['defaultGuarded'] and a['selectedGuarded']==(mode=='guarded')
 assert a==load(E/(mode+'-summary.json'))
 b=(E/r['log']).read_bytes();s=b.decode('utf-8-sig');assert json.loads(s.splitlines()[-1])==a
current=skips(E/'current-skips.txt');guarded=skips(E/'guarded-skips.txt')
approved=load(C.parent/'k01-schema-list/baseline-adoption/skip-delta.json')['current']
assert current==approved and len(current)==677 and len(guarded)==676
assert set(current)-set(guarded)=={'validateexpr-28'} and not set(guarded)-set(current)
assert all(current[n]==reason for n,reason in guarded.items())
prior=load(C.parent/'k02-relative-schema-uri/RESULTS.json');assert prior['summary']['solutionPassed']==21842 and prior['summary']['pairedCases']==324 and prior['summary']['remainingRegressions']==0
pin=load(E/'REVIEW-PIN.json')
summary=dict(fullRunnerProcesses=2,casesPerRun=31821,currentQt3=[31144,0,677],guardedRehearsalQt3=[31145,0,676],removedSkips=['validateexpr-28'],addedSkips=[],retainedSkipNamesReasons=676,failures=0,timeouts=0,priorPairedCases=324,priorBoundaryCases=17,inheritedSolutionPasses=21842,inheritedQt4=[8436,0,1894],candidate='1.26.8',published='1.26.3',normalRunnerChanged=False,baselineChanged=False,ownerApproved=False)
out=dict(date='2026-10-27',sourceBase=pin['sourceBase'],status='Proposal ready; owner approval pending; normal runner and baseline unchanged',summary=summary,fullRunnerReceipts=rows,skipDelta=dict(removed={n:current[n] for n in set(current)-set(guarded)},retained=guarded),scope='Enable the existing guarded schema ownership switch on the normal QT3 runner only. No KnownSkips entry exists for validateexpr-28; its environment exception would no longer be counted as a harness skip.',acceptance=dict(id='K02-guarded-runner-adoption-v1',proposal='BASELINE-UPDATE-PROPOSAL.md',patch='PROPOSED-ADOPTION.patch',requiresOwnerApproval='AGENTS.md mandates exact QT3 31144/0/677; proposed guarded-runner adoption changes it to31145/0/676.'),limits=['The one new assertion pass uses the existing XQST0012 error alternative; it does not prove validation against duration-derived XSD types.','Current .NET schema backend and explicit profile exclusions remain unchanged. Bosak.Schema REQ-009 owns staged XSD1.1.','324 eligible schema cases are directly paired; the full runner retains dependency/heavy-set skips. Do not infer support for the 108 excluded schema cases.','No new unit/qt4 suite claim for this documentation-only review; their complete successful receipts are inherited from source-identical K02-C02.','Approval does not publish, tag, push, merge externally, relax assertions, bypass profile guards or admit any other skipped test.'])
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
