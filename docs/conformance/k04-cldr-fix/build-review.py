"""Rebuild bounded formatter-fix validation from fresh tests and unchanged gates."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
probes=load(E/'probe-receipts.json');controls=load(E/'control-receipts.json');normal=load(E/'normal-case-receipts.json');assert len(probes)==len(normal)==6 and len(controls)==22
for p in probes+controls+normal:assert p['exitCode']==0 and not p['timeout']
assert all(p['result']['kind']=='Passed' for p in probes+controls)
assert all(p['totals']==([1,1,0,0] if p['suite']=='qt3' else [1,0,0,1]) for p in normal)
assert re.search(r'Failed:\s+21, Passed:\s+15, Skipped:\s+0, Total:\s+36',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+0, Passed:\s+36, Skipped:\s+0, Total:\s+36',read(E/'regressions-after.log'))
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
for suite,passed,skipped in [('qt3',31161,660),('qt4',8438,1892)]:
 assert skips(E/(suite+'-skips.txt'))==skips(R/('docs/conformance/k04-collation-admission/evidence/'+suite+'-skips.txt'))
 assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skipped),read(E/(suite+'.log')))
p=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(p)==10 and sum(map(int,p))==22043 and 'Failed!' not in read(E/'solution-tests.log');assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C06',status='Bounded formatter correction complete;qt4 CLDR admission unchanged',sourceBase=load(E/'IMPLEMENTATION-PIN.json')['sourceBase'],specificationAdoption='evidence/SPECIFICATION-ADOPTION.json',summary=dict(regressionRows=36,regressionCompatibilityPaths=['XPath31','XPath40'],beforeRegressionPassed=15,beforeRegressionFailed=21,afterRegressionPassed=36,originalAssertionsPassed=6,modifierControlsPassed=22,normalOriginalQt3Passed=2,normalOriginalQt4Skipped=4,smallWatchdogProcesses=34,timeouts=0,solutionPassed=22043,normalQt3=[31161,0,660],qt4Gated=[8438,0,1892],retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1892,admissionChanged=False,baselineChanged=False,candidateVersion='1.26.12',publishedVersion='1.26.3'),limits=load(E/'SPECIFICATION-ADOPTION.json')['intentionalLimits']+['No complete CLDR/RBNF implementation','No harness,fixture,assertion,GatedSets,REQ-126 status or release change','Bosak.Schema retains staged XSD1.1 ownership'],next='Review qt4 CLDR admission separately with the original four cases and relevant profile/language controls;no admission included in this implementation slice.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
