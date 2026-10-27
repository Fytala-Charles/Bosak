"""Build the owner-approved K02 adoption record from the actual normal gate."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'baseline-adoption'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def skips(p):
 out={};reason=None
 for line in p.read_text(encoding='utf-8-sig').splitlines():
  m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
  if m:reason=m.group(2)
  elif line.strip():assert reason is not None and line not in out;out[line]=reason
 return out
approval=load(E/'owner-approval.json');assert approval['answer']=='Approve scoped K02 adoption (Recommended)' and approval['proposalCommit']=='628f169e'
gate=load(E/'gate-receipt.json');assert gate['exitCode']==0 and not gate['timeout'] and gate['watchdogSeconds']==300
for label,n in [('Total',31821),('Passed',31145),('Failed',0),('Skipped',676)]:assert re.search(r'^'+label+r':\s+'+str(n)+r'\s*$',log('qt3-normal-gate.log'),re.M)
assert '0 Error(s)' in log('harness-build.log') and '0 Warning(s)' in log('harness-build.log')
before=skips(C/'evidence/current-skips.txt');after=skips(E/'qt3-skips.txt');rehearsal=skips(C/'evidence/guarded-skips.txt')
assert len(before)==677 and len(after)==676 and after==rehearsal
assert set(before)-set(after)=={'validateexpr-28'} and not set(after)-set(before)
assert all(before[k]==v for k,v in after.items())
delta=dict(previous=before,current=after,removed={n:before[n] for n in before if n not in after},added={},retainedNamesReasons=676)
(E/'skip-delta.json').write_text(json.dumps(delta,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
old=load(C.parent/'k02-relative-schema-uri/RESULTS.json');assert old['summary']['solutionPassed']==21842
out=dict(date='2026-10-27',decisionId='K02-guarded-runner-adoption-v1',status='Complete',ownerApproved=True,sourceBase=approval['sourceBase'],approvedProposalCommit='628f169e',summary=dict(qt3=[31145,0,676],totalCases=31821,removedSkip='validateexpr-28',retainedSkipNamesReasons=676,addedSkips=0,normalRunnerGuarded=True,testExecutorDefaultGuarded=False,inheritedSolutionPasses=21842,inheritedQt4=[8436,0,1894],candidate='1.26.8',published='1.26.3',releaseApproved=False),freshGate=gate,policy='Approved exact normal-runner initializer and AGENTS baseline update only; TestExecutor default and all guards unchanged. validateexpr-28 passes via its unchanged XQST0012 alternative. No fixture, assertion, dependency, retry/timeout, engine, version, release or REQ-126 acceptance change. Bosak.Schema owns XSD1.1.',validation='Fresh harness build and complete actual normal QT3 gate with exact skip delta. Unit21842 and qt4 gate8436/0/1894 inherited from unchanged engine; no fresh full-suite/qt4 claim.')
(C/'BASELINE-ADOPTION.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8');print(json.dumps(out['summary']))
