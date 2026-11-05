"""Rebuild bounded parse-integer and evidence-based gate promotion results."""
from pathlib import Path
import json,re,hashlib
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
assert re.search(r'Failed:\s+81, Passed:\s+6, Skipped:\s+0, Total:\s+87',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+2, Passed:\s+85, Skipped:\s+0, Total:\s+87',read(E/'regressions-after-initial.log'))
assert re.search(r'Failed:\s+0, Passed:\s+87, Skipped:\s+0, Total:\s+87',read(E/'regressions-after.log'))
probes=load(E/'probe-receipts.json');controls=load(E/'control-receipts.json');assert len(probes)==48 and len(controls)==46
for r in probes+controls:assert r['exitCode']==0 and not r['timeout'] and r['result']['kind']=='Passed'
for kind,totals in [('parse',[47,47,0,0]),('format',[102,101,0,1])]:
 r=load(E/('format-integer-implementation-'+kind+'-receipt.json'));assert r['exitCode']==0 and not r['timeout'] and r['totals']==totals
promotion=load(E/'GATE-PROMOTION.json');assert promotion['beforeCount']==224 and promotion['afterCount']==226
for n,h in promotion['prerequisites'].items():assert hashlib.sha256((E/n).read_bytes()).hexdigest()==h
name='tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs';old=__import__('subprocess').check_output(['git','show',promotion['sourceBase']+':'+name]).decode();source=(R/name).read_text(encoding='utf-8')
def gates(s):
 s=s[s.index('private static readonly HashSet<string> GatedSets'):];return set(re.findall(r'"([^"]+)"',s[:s.index('};')]))
assert len(gates(old))==224 and len(gates(source))==226 and gates(source)-gates(old)=={'fn-parse-integer','fn-format-integer'} and not gates(old)-gates(source)
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
assert skips(E/'qt3-skips.txt')==skips(R/'docs/conformance/k04-radix-fix/evidence/qt3-skips.txt')
before=skips(R/'docs/conformance/k04-radix-fix/evidence/qt4-skips.txt');after=skips(E/'qt4-skips.txt');assert len(after)==1893 and all(after.get(k)==v for k,v in before.items()) and {k:v for k,v in after.items() if k not in before}=={'format-integer-042':'Unsupported dependency'}
for suite,passed,skipped in [('qt3',31161,660),('qt4',8586,1893)]:assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skipped),read(E/(suite+'.log')))
p=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(p)==10 and sum(map(int,p))==22198 and 'Failed!' not in read(E/'solution-tests.log');assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C11',status='Bounded parse-integer complete;both integer sets promoted after zero-failure full runs',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],specificationAdoption='evidence/SPECIFICATION-ADOPTION.json',summary=dict(regressionRows=87,beforeRegressionsPassed=6,beforeRegressionsFailed=81,initialPostFixPassed=85,initialEmptyDisplayExpectationMistakes=2,finalRegressionsPassed=87,originalParseAssertionsPassed=47,originalRoundTripPassed=1,profileControlsPassed=46,normalParseSet=[47,0,0],normalFormatSet=[101,0,1],smallWatchdogProcesses=96,timeouts=0,solutionPassed=22198,normalQt3=[31161,0,660],qt4Gated=[8586,0,1893],gatedSetsBefore=224,gatedSetsAfter=226,newlyGatedPasses=148,qt3RetainedNamesAndReasons=660,qt4PriorRetainedNamesAndReasons=1892,qt4AddedExistingSkip='format-integer-042',candidateVersion='1.28.0',publishedVersion='1.26.3'),policies=load(E/'SPECIFICATION-ADOPTION.json')['implementationPolicies'],limits=['Int64 output limit;oversized valid lexical input raisesFOCA0003','No general arbitrary-precision integer representation or latest-draft certification','Two initial test display expectations corrected to semantic empty assertions;original logs/source retained','Seven original any-of size alternatives preserved,not weakened','No fixture/assertion/filter/schema/QT3 baseline or REQ-126 disposition change','Bosak.Schema owns XSD1.1;no release or upstream contribution'],next='Review the remaining format-integer-042 negative numbering-sequence dependency in both profiles before any exact QT3 baseline proposal.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
