"""Rebuild the K02-C02 review from the unchanged original assertion receipts."""
from pathlib import Path
import json,collections,re
C=Path(__file__).resolve().parent;E=C/'evidence';P=C.parent/'k02-constructor-registration'/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def pairs(rows):
 out=collections.defaultdict(dict)
 for r in rows:
  assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 and not r.get('budgetExceeded',False)
  out[r['id']][r['mode']]=r['result']
 return out
before=pairs(load(P/'comparison-results.json'));after=pairs(load(E/'comparison-results.json'));boundary=pairs(load(E/'boundary-results.json'));oldbound=pairs(load(P/'boundary-results.json'))
assert set(before)==set(after) and len(after)==324 and len(boundary)==17
fixed=[]
for name,a in after.items():
 b=before[name]
 assert all(a['current'][k]==b['current'][k] for k in ['kind','message','exceptionType','expression','assertion']),name
 assert all(a['guarded'][k]==b['guarded'][k] for k in ['expression','assertion','decision','deferred']),name
 assert a['guarded']['kind']=='Passed',name
 if b['guarded']['kind']=='Failed':fixed.append(name)
assert set(fixed)=={'qischema032','qischema032a','qischema032b','qischema032c'}
for name,p in boundary.items():
 for mode,r in p.items():assert r==oldbound[name][mode],(name,mode)
assert re.search(r'Failed:\s+3, Passed:\s+3, Skipped:\s+0',log('baseline-tests.log'))
assert re.search(r'Failed:\s+0, Passed:\s+6, Skipped:\s+0',log('fixed-tests.log'))
unitRows=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
assert len(unitRows)==10 and len({n for f,p,s,n in unitRows})==10
assert [(int(f),int(p),int(s)) for f,p,s,n in unitRows if n=='Bosak.XPath.Runtime.Tests.dll']==[(51,218,0)]
assert all(int(f)==int(s)==0 for f,p,s,n in unitRows if n!='Bosak.XPath.Runtime.Tests.dll')
assert re.search(r'Failed:\s+0, Passed:\s+269, Skipped:\s+0',log('runtime-tests-normal-layout.log'))
assert sum(int(p) for f,p,s,n in unitRows if n!='Bosak.XPath.Runtime.Tests.dll')+269==21842
for name,passed,skipped in [('qt3-gate.log',31144,677),('qt4-gated.log',8436,1894)]:
 for label,total in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+label+r':\s+'+str(total)+r'\s*$',log(name),re.M)
assert '0 Error(s)' in log('solution-build.log')
skips={};reason=None
for line in (E/'qt3-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if m:reason=m.group(2)
 elif line.strip():assert reason is not None and line not in skips;skips[line]=reason
assert len(skips)==677 and skips==load(C.parent/'k01-schema-list'/'baseline-adoption'/'skip-delta.json')['current']
summary=dict(focusedTests=6,baselineFocusedFailed=3,baselineFocusedPassed=3,solutionPassed=21842,pairedCases=324,pairedProcesses=648,boundaryProcesses=34,totalWatchdogProcesses=682,timeouts=0,fixedRegressions=4,remainingRegressions=0,newRegressions=0,qt3=[31144,0,677],qt3SkipNamesReasonsUnchanged=677,qt4Gated=[8436,0,1894],experimentalPasses=11,candidate='1.26.8',published='1.26.3')
out=dict(date='2026-10-27',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],status='K02-C02 complete; all 324 bounded eligible guarded cases pass; normal adoption remains separate review',summary=summary,fixed=fixed,transitions=dict(collections.Counter(p['current']['kind']+' -> '+p['guarded']['kind'] for p in after.values())),pairs=after,boundaries=boundary,unitProjects={n:dict(passed=269 if n=='Bosak.XPath.Runtime.Tests.dll' else int(p),receipt='runtime-tests-normal-layout.log' if n=='Bosak.XPath.Runtime.Tests.dll' else 'solution-tests.log') for f,p,s,n in unitRows},qualification='The isolated full-solution attempt had 51 Runtime fixture lookup failures caused by hard-coded paths to .guard-tmp/qt3tests. The full Runtime project rerun in normal layout passes269/269; combine it with the nine other complete green isolated projects. No test excluded or fixture path patched.',contract='FileStream.Name supplies source identity when fully qualified; existing file-location fallback keeps its resolved filename. Non-file streams retain empty source URI; no inference from unselected hints or query base URI. Wrapped/non-file stream identity needs a separate contract.',policy='Default-off guarded mode remains unchanged. No fixture, assertion, dependency, skip, baseline, public API, resolver/network/DTD policy or XSD1.1 backend change. Bosak.Schema REQ-009 owns staged XSD1.1 implementation and its separately governed compatibility seam. No release, push, tag, upstream or Braid action.',next='Review the 324-case bounded applicability and profile exclusions before any normal-mode or baseline-policy proposal. Do not infer full XSD1.1 support or applicability to the 108 excluded skips.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
