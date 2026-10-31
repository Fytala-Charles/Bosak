"""Rebuild the content-preserving K03-C02 review from complete test and gate receipts."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
assert re.search(r'Failed:\s+39, Passed:\s+1, Skipped:\s+0, Total:\s+40',log('baseline-tests.log'))
assert re.search(r'Failed:\s+0, Passed:\s+40, Skipped:\s+0, Total:\s+40',log('fixed-tests.log'))
rows=load(E/'results.json');old=load(C.parent/'k03-name-validation/evidence/results.json');assert len(rows)==len(old)==30
before={r['id']:r['result'] for r in old};changed={'provider-65-True','provider-119070-True'}
literal=chr(0x1c0)+'0041'+chr(0x1c1)
for row in rows:
 assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20
 a=row['result'];b=before[row['id']]
 if row['id'] in changed:
  assert a['detail']['stringValue']==b['detail']['stringValue']==literal
  assert a['detail']['localName']==b['detail']['localName']
  assert literal in a['detail']['serialized'] and literal not in b['detail']['serialized']
 else:assert all(a[k]==b[k] for k in ['detail','error','exceptionType']),row['id']
assert next(r for r in rows if r['id']=='original-map-to-element-053')['result']['detail']['kind']=='Skipped'
units=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
assert len(units)==10 and len({n for f,p,s,n in units})==10 and all(int(f)==int(s)==0 for f,p,s,n in units) and sum(int(p) for f,p,s,n in units)==21960
for n,passed,skipped in [('qt3-gate.log',31145,676),('qt4-gated.log',8436,1894)]:
 for label,total in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+label+r':\s+'+str(total)+r'\s*$',log(n),re.M)
assert '0 Error(s)' in log('solution-build.log')
skips={};reason=None
for line in (E/'qt3-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if m:reason=m.group(2)
 elif line.strip():assert reason is not None and line not in skips;skips[line]=reason
assert skips==load(C.parent/'k02-admission-review/baseline-adoption/skip-delta.json')['current'] and len(skips)==676
summary=dict(focusedTests=40,baselineFailed=39,baselinePassed=1,solutionPassed=21960,diagnosticProcesses=30,timeouts=0,qt3=[31145,0,676],retainedQt3SkipNamesReasons=676,qt4Gated=[8436,0,1894],experimentalPasses=11,originalUnicodeSkipRetained=True,candidate='1.26.10',published='1.26.3')
out=dict(date='2026-10-27',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],status='K03-C02 complete for serialization of existing stored node content; name-storage integration remains open',summary=summary,changedDiagnosticIds=sorted(changed),probes=rows,unitProjects={n:int(p) for f,p,s,n in units},implementation='Replace whole-output sentinel decoding with lazy token-only decoding of element and attribute names in emitted XML. Skip quoted attribute values, namespace URIs, comments, CDATA, PIs and full declarations including quoted/internal DTD subsets. Preserve inherited-namespace fixup and unannotated-tree behavior.',limits='Existing XObject content is preserved. XML loader decoding, XDM value-accessor behavior, name-storage representation and current XML1.1 control-character support are unchanged; this is not an end-to-end parse/copy/serialization guarantee.',next='Integrate reversible map/provider name storage with end-to-end name/content/namespace/copy tests; review loader/accessor value preservation as part of that boundary before any qt4 admission proposal.',policy='No public API, fixture, assertion, harness/dependency/skip/baseline change, XSD1.1 backend, REQ-126 acceptance change or release.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
