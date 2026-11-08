"""Rebuild K03-C01 validation outcomes while retaining open provider boundaries."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
assert re.search(r'Failed:\s+18, Passed:\s+60, Skipped:\s+0, Total:\s+78',log('baseline-tests.log'))
assert re.search(r'Failed:\s+0, Passed:\s+78, Skipped:\s+0, Total:\s+78',log('fixed-tests.log'))
assert 'duplicate ID' not in log('baseline-tests.log')+log('fixed-tests.log')
rows=load(E/'results.json');old=load(C.parent/'k03-unicode-names/evidence/results.json');assert len(rows)==len(old)==30
p={r['id']:r['result'] for r in rows};before={r['id']:r['result'] for r in old};changed={'map-name-0x3001','map-name-0x200c','map-name-0xf0000','map-name-0x10ffff'}
for r in rows:
 assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20
 if r['id'] not in changed:assert all(r['result'][k]==before[r['id']][k] for k in ['detail','error','exceptionType']),r['id']
for n in ['map-name-0x3001','map-name-0x200c']:assert p[n]['exceptionType']=='XmlException' and before[n]['error'].startswith('FOJS0009:')
for n in ['map-name-0xf0000','map-name-0x10ffff']:assert p[n]['exceptionType']=='InvalidOperationException' and p[n]['error'].startswith('FOJS0009:')
assert p['original-map-to-element-053']['detail']['kind']=='Skipped'
units=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'));assert len(units)==10 and len({n for f,p,s,n in units})==10
assert all(int(f)==int(s)==0 for f,p,s,n in units) and sum(int(p) for f,p,s,n in units)==21920
for n,passed,skipped in [('qt3-gate.log',31145,676),('qt4-gated.log',8436,1894)]:
 for label,total in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+label+r':\s+'+str(total)+r'\s*$',log(n),re.M)
assert '0 Error(s)' in log('solution-build.log')
skips={};reason=None
for line in (E/'qt3-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if m:reason=m.group(2)
 elif line.strip():assert reason is not None and line not in skips;skips[line]=reason
assert skips==load(C.parent/'k02-admission-review/baseline-adoption/skip-delta.json')['current'] and len(skips)==676
summary=dict(focusedTests=78,baselineFailed=18,baselinePassed=60,solutionPassed=21920,diagnosticProcesses=30,timeouts=0,qt3=[31145,0,676],retainedQt3SkipNamesReasons=676,qt4Gated=[8436,0,1894],experimentalPasses=11,originalUnicodeSkipRetained=True,candidate='1.26.9',published='1.26.3')
out=dict(date='2026-10-27',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],status='K03-C01 complete; provider representation and serialization remain open',summary=summary,changedDiagnosticIds=sorted(changed),probes=rows,unitProjects={n:int(p) for f,p,s,n in units},implementation='Decode UTF16 scalars with Rune.DecodeFromUtf16 and reject non-Done status; use XML NameStartChar/NameChar scalar ranges, excluding colon. Preserve all name-format and FOJS0009 paths. No provider storage/serializer changes.',qualification='Valid names outside current XName support are tested via the existing empty-attribute omission path after parsing. Their materialized construction remains unsupported. Initial malformed-string MemberData caused two xUnit duplicate IDs; final tests construct malformed UTF16 inside uniquely indexed theory cases, with78/78 executed. Initial authoring receipt retained.',next='K03-C02 serialization: decode name tokens without modifying literal node content; then integrate reversible map/provider name storage before any qt4 admission proposal.',policy='No public API, harness policy, fixture, assertion, dependency, skip, baseline, XSD1.1 ownership or release change. REQ-126 accepted bounded catalog unchanged.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
