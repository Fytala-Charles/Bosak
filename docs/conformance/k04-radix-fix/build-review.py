"""Rebuild bounded radix implementation results while retaining parse-integer failure."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
assert re.search(r'Failed:\s+53, Passed:\s+15, Skipped:\s+0, Total:\s+68',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+0, Passed:\s+68, Skipped:\s+0, Total:\s+68',read(E/'regressions-after.log'))
original=load(E/'probe-receipts.json');controls=load(E/'control-receipts.json');full=load(E/'format-integer-implementation-receipt.json');assert len(original)==17 and len(controls)==34
for r in original+controls:assert r['exitCode']==0 and not r['timeout']
assert full['exitCode']==0 and not full['timeout'] and full['totals']==[102,100,1,1]
assert sum(r['result']['kind']=='Passed' for r in original)==16
failed=[r for r in original if r['result']['kind']=='Failed'];assert len(failed)==1 and failed[0]['result']['Name']=='format-integer-40-012' and 'XPST0017' in failed[0]['result']['message']
qt3=[r for r in controls if r['result']['Suite']=='qt3'];qt4=[r for r in controls if r['result']['Suite']=='qt4'];assert all(r['result']['kind']=='Passed' for r in qt3) and len(qt3)==17
assert sum(r['result']['kind']=='Passed' for r in qt4)==14 and {r['result']['Name'] for r in qt4 if r['result']['kind']=='Failed'}=={'control-qt4-parse-hex','control-qt4-parse-binary','control-qt4-lookup-parse'}
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
assert skips(E/'format-integer-implementation-skips.txt')=={'format-integer-042':'Unsupported dependency'}
for suite,passed,skipped in [('qt3',31161,660),('qt4',8438,1892)]:
 assert skips(E/(suite+'-skips.txt'))==skips(R/('docs/conformance/k04-cldr-admission/evidence/'+suite+'-skips.txt'))
 assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skipped),read(E/(suite+'.log')))
p=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(p)==10 and sum(map(int,p))==22111 and 'Failed!' not in read(E/'solution-tests.log');assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C09',status='Bounded radix formatter complete;parse-integer dependency remains open',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],specificationAdoption='evidence/SPECIFICATION-ADOPTION.json',summary=dict(regressionRows=68,beforeRegressionsPassed=15,beforeRegressionsFailed=53,afterRegressionsPassed=68,originalRadixPassed=16,originalRadixFailed=1,formatterOnlyFailuresFixed=14,qt3ControlsPassed=17,qt4ControlsPassed=14,qt4DependencyControlsFailed=3,exploratorySet=[102,100,1,1],smallWatchdogProcesses=52,timeouts=0,solutionPassed=22111,normalQt3=[31161,0,660],qt4Gated=[8438,0,1892],retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1892,baselineChanged=False,gatedSetsChanged=False,candidateVersion='1.27.0',publishedVersion='1.26.3'),retainedOriginalFailure=failed,limits=load(E/'SPECIFICATION-ADOPTION.json')['intentionalLimits']+['Numeric radix modifiers use existing literal/English suffix conventions;traditional flag does not title-case digits','Regular/irregular grouping reuses the existing template mechanism','No full current-draft certification or public arbitrary-precision input claim','Bosak.Schema owns XSD1.1;REQ-126 remains Complete;no release or upstream submission'],next='Review and implement the separate parse-integer contract,including profile/arity/radix/lexical errors,before attempting case012 or set promotion.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
