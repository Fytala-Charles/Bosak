"""Rebuild the bounded K03 name-storage and value-integrity completion review."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def skips(path):
 out={};reason=None
 for line in path.read_text(encoding='utf-8-sig').splitlines():
  m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
  if m:reason=m.group(2)
  elif line.strip():assert reason is not None and line not in out;out[line]=reason
 return out
for name,failed,passed in [('baseline-map-tests.log',22,13),('baseline-provider-tests.log',7,0),('free-attribute-before.log',1,0),('namespace-before.log',2,1),('final-map-tests.log',0,39),('final-provider-tests.log',0,8),('final-all-provider-tests.log',0,201)]:assert re.search(r'Failed:\s+'+str(failed)+r', Passed:\s+'+str(passed)+r', Skipped:\s+0',log(name)),name
rows=load(E/'results.json');old=load(C.parent/'k03-serialization/evidence/results.json');assert len(rows)==len(old)==30
before={r['id']:r['result'] for r in old};changed=[]
for row in rows:
 assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20
 if any(row['result'][k]!=before[row['id']][k] for k in ['detail','error','exceptionType']):changed.append(row['id'])
assert set(changed)=={'original-map-to-element-053','map-name-0x3001','map-name-0x200c','map-name-0x10000','map-name-0x1d11e','map-name-0xeffff','supplementary-child','supplementary-attribute','supplementary-qname','provider-119070-False'}
p={r['id']:r['result'] for r in rows};assert p['original-map-to-element-053']['detail']['kind']=='Passed' and '119070' in p['original-map-to-element-053']['detail']['assertion']
for name in changed:
 if name.startswith('map-name') or name.startswith('supplementary'):assert p[name]['error'] is None,name
assert p['provider-119070-False']['detail']['localName']==chr(119070)
units=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
assert len(units)==10 and len({n for f,p,s,n in units})==10 and all(int(f)==int(s)==0 for f,p,s,n in units) and sum(int(p) for f,p,s,n in units)==22007
for name,passed,skipped in [('qt3-gate.log',31145,676),('qt4-gated.log',8437,1893)]:
 for label,n in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+label+r':\s+'+str(n)+r'\s*$',log(name),re.M)
assert '0 Error(s)' in log('solution-final-build.log')
qt3=skips(E/'qt3-skips.txt');assert qt3==load(C.parent/'k02-admission-review/baseline-adoption/skip-delta.json')['current'] and len(qt3)==676
qt4before=skips(E/'qt4-prior-skips.txt');qt4after=skips(E/'qt4-skips.txt');assert len(qt4before)==1894 and len(qt4after)==1893
assert set(qt4before)-set(qt4after)=={'map-to-element-053'} and not set(qt4after)-set(qt4before) and all(qt4before[k]==v for k,v in qt4after.items())
(E/'qt4-skip-delta.json').write_text(json.dumps(dict(removed={'map-to-element-053':qt4before['map-to-element-053']},added={},retained=qt4after),indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
reader=json.loads(log('reader-control.log'));assert len(reader)==8
for point in [1,11,133]:assert any(not r['check'] and r['point']==point and r['attribute']==r['text']==point and r['error'] is None for r in reader)
summary=dict(focusedMapTests=39,focusedProviderTests=8,newRegressions=47,initialBaselineMapFailed=22,initialBaselineProviderFailed=7,solutionPassed=22007,diagnosticProcesses=30,timeouts=0,qt3=[31145,0,676],retainedQt3SkipNamesReasons=676,qt4Gated=[8437,0,1893],removedQt4Skip='map-to-element-053',retainedQt4SkipNamesReasons=1893,experimentalPasses=11,candidate='1.26.11',published='1.26.3')
out=dict(date='2026-10-27',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],status='K03 bounded core correction complete; original supplementary-name case passes normally',summary=summary,changedDiagnosticIds=changed,probes=rows,readerControls=reader,unitProjects={n:int(p) for f,p,s,n in units},implementation=['Encode map local names, attributes and namespace prefixes; mark encoded trees without changing XML-version feature validation.','Normalize copied element/document names exactly once and preserve prefix/namespace metadata across ancestor/document boundaries.','Use existing reader numeric-reference expansion; remove redundant loader/attribute value decoding so literal sentinel text survives entity expansion and serialization.','Decode virtual namespace prefixes using owner metadata, then encode only once for their storage/cache.'],qualification='Initial compile/API mistakes and the two-stage namespace regression are retained, including interrupted intermediate test logs and file-lock build attempt. Only the final complete solution and gate receipts are accepted. Free-attribute and namespace controls were added during the integration review.',limits=['This verifies bounded map/provider round trips, not complete current-draft F&O behavior or every XML parser edition/input.','XML-layout parsing retains its existing well-formedness/parser restrictions; current-draft escape-names remains unsupported.','Existing XML reader/resolver settings, including character-checking policy, are unchanged; native controls are mechanism evidence, not full XML1.1 validity certification.','Bosak.Schema owns XSD1.1; no commercial seam, licensing or public API change.'],policy='AGENTS exact QT3 expectation stays31145/0/676. qt4 keeps its existing zero-gated-failures rule and224 gated sets; the existing case now passes via the core fix, without any fixture/assertion/filter or admission change. REQ-126 accepted bounded catalog unchanged. No release/push/tag/upstream/Braid action.',next='K04 dependency/routing applicability audit; keep current-draft and XSD1.1 ownership limits explicit.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
