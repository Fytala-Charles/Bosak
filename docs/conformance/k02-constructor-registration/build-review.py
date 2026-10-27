"""Rebuild K02-C01 outcomes from the original paired assertion receipts."""
from pathlib import Path
import json,collections,re
C=Path(__file__).resolve().parent;E=C/'evidence';P=C.parent/'k02-schema-inventory'/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def pairs(rows):
 out=collections.defaultdict(dict)
 for r in rows:
  assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 and not r.get('budgetExceeded',False)
  out[r['id']][r['mode']]=r['result']
 return out
before=pairs(load(P/'comparison-results.json'));after=pairs(load(E/'comparison-results.json'));boundary=pairs(load(E/'boundary-results.json'))
assert set(before)==set(after) and len(after)==324 and len(boundary)==17
fixed=[];remaining=[]
for name,a in after.items():
 b=before[name]
 assert all(a['current'][k]==b['current'][k] for k in ['kind','message','exceptionType','expression','assertion']),name
 assert all(a['guarded'][k]==b['guarded'][k] for k in ['expression','assertion','decision','deferred']),name
 if b['guarded']['kind']=='Failed' and a['guarded']['kind']=='Passed':fixed.append(name)
 elif a['guarded']['kind']=='Failed':remaining.append(name)
 else:assert a['guarded']['kind']==b['guarded']['kind'],name
assert len(fixed)==68 and set(remaining)=={'qischema032','qischema032a','qischema032b','qischema032c'}
oldbound=pairs(load(P/'boundary-results.json'))
for name,p in boundary.items():
 for mode,r in p.items():assert r==oldbound[name][mode],(name,mode)
traces=load(E/'mechanism-traces.json')
for t in traces[:2]:assert t['error1'] is None and t['error2'] is None and t['typeAfterFirst'] and t['functionAfterFirst'] and t['functionAfterSecond']
assert not traces[2]['compiled'] and traces[3]['compiled']
assert re.search(r'Failed:\s+0, Passed:\s+11, Skipped:\s+0',log('final-focused-tests.log'))
# The restricted run could not bind the database provider's local listener; substitute only its separate complete green project run.
unitRows=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
units={n:dict(failed=int(f),passed=int(p),skipped=int(s),receipt='solution-tests.log') for f,p,s,n in unitRows}
assert len(units)==10,units.keys()
assert units['Bosak.XPath.Providers.Database.Tests.dll']['failed']==45
assert re.search(r'Failed:\s+0, Passed:\s+47, Skipped:\s+0',log('database-tests-unrestricted.log'))
units['Bosak.XPath.Providers.Database.Tests.dll']=dict(failed=0,passed=47,skipped=0,receipt='database-tests-unrestricted.log')
assert all(v['failed']==v['skipped']==0 for v in units.values())
assert sum(v['passed'] for v in units.values())==21836
for name,passed,skipped in [('qt3-gate.log',31144,677),('qt4-gated.log',8436,1894)]:
 for label,total in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+label+r':\s+'+str(total)+r'\s*$',log(name),re.M)
assert '0 Error(s)' in log('solution-isolated-build.log')
summary=dict(focusedTests=11,baselineFocusedFailed=9,baselineFocusedPassed=2,solutionPassed=21836,pairedCases=324,pairedProcesses=648,boundaryProcesses=34,totalWatchdogProcesses=682,timeouts=0,fixedRegressions=68,remainingRegressions=4,newRegressions=0,qt3=[31144,0,677],qt4Gated=[8436,0,1894],experimentalPasses=11,candidate='1.26.7',published='1.26.3')
out=dict(date='2026-10-27',status='K02-C01 complete; K02-C02 and normal-adoption review remain open',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],summary=summary,fixed=fixed,remaining=remaining,transitions=dict(collections.Counter(p['current']['kind']+' -> '+p['guarded']['kind'] for p in after.values())),pairs=after,boundaries=boundary,mechanismTraces=traces,unitProjects=units,qualification='Initial fixed test run had one assertion-authoring error: XdmValue.ToString() displays a list as (sequence). The corrected test checks deep-equal against (1,2,3); final 11/11 pass. Restricted database run had 45 HttpListener setup failures, then all 47 pass outside sandbox. Normal solution build encountered an existing XSLT conformance process file lock; isolated solution build succeeds. Full exploratory qt4 run was interrupted; only the complete 224-set gated run is accepted.',policy='No default-mode, fixture, assertion, dependency, skip, gate baseline, public API, XSD1.1 capability or release change. Bosak.Schema REQ-009 owns XSD1.1; any future new seam needs its separate compatibility ADR. Do not enable guarded mode normally while four URI regressions remain.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
