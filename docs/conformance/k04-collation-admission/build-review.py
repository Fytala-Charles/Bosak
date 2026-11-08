"""Rebuild the bounded qt4 non-codepoint collation admission evidence."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
requests=load(E/'requests.json');assert len(requests)==13;probes=load(E/'probe-receipts.json');normal=load(E/'normal-case-receipts.json');expressions=load(E/'expression-control-receipts.json');assert len(probes)==len(normal)==13 and len(expressions)==7
for receipt in probes+normal+expressions:assert receipt['exitCode']==0 and not receipt['timeout']
assert sum(p['result']['kind']=='Passed' for p in probes)==5 and sum(p['result']['kind']=='Failed' for p in probes)==8
assert all(p['result']['kind']=='Passed' for p in expressions)
assert sum(p['totals']==[1,1,0,0] for p in normal)==5 and sum(p['totals']==[1,0,0,1] for p in normal)==8
for r in requests:
 p=next(x for x in probes if x['name']==r['Name']);n=next(x for x in normal if x['name']==r['Name']);assert (p['result']['kind']=='Passed')==(n['totals']==[1,1,0,0])
changes=[];outcomes=0
for suite in ['qt3','qt4']:
 before={r['id']:r for r in map(json.loads,read(E/(suite+'-before.jsonl')).splitlines())};after={r['id']:r for r in map(json.loads,read(E/(suite+'-after.jsonl')).splitlines())}
 assert len(before)==len(after)==(105 if suite=='qt3' else 143)
 expected={'feature-qt4-non_unicode_codepoint_collation-True','feature-qt4-non_unicode_codepoint_collation-False'}|{r['Name']+'-full' for r in requests if next(p for p in probes if p['name']==r['Name'])['result']['kind']=='Passed'}
 for key in before:
  for field in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):
   old,new=before[key][field],after[key][field];outcomes+=1
   if old!=new:
    assert suite=='qt4' and key in expected;changes.append(dict(id=key,before=old,after=new))
   else:assert suite=='qt3' or key not in expected
 if suite=='qt4':
  for r in requests:assert after[r['Name']+'-full']['xpathSupported']==before[r['Name']+'-without-feature']['xpathSupported']
 for phase in ['before','after']:
  for p in load(E/(phase+'-receipts.json')):assert p['exitCode']==0 and not p['timeout']
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
assert skips(E/'qt3-skips.txt')==skips(R/'docs/conformance/k04-feature-inventory/evidence/qt3-skips.txt')
before=skips(R/'docs/conformance/k04-feature-inventory/evidence/qt4-skips.txt');after=skips(E/'qt4-skips.txt');assert len(after)==1892 and all(before.get(k)==v for k,v in after.items()) and {k:v for k,v in before.items() if k not in after}=={'compare-010':'Unsupported dependency'}
for suite,passed,skipped in [('qt3',31161,660),('qt4',8438,1892)]:assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skipped),read(E/(suite+'.log')))
p=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(p)==10 and sum(map(int,p))==22007 and 'Failed!' not in read(E/'solution-tests.log');assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C04',status='Complete for bounded existing-collation admission',sourceBase=load(E/'ADMISSION-PIN.json')['sourceBase'],summary=dict(originalCases=13,directDiagnosticPassed=5,directDiagnosticFailedRetained=8,normalCasesPassed=5,normalCasesSkipped=8,expressionControlsPassed=7,filterControls=248,filterProfileOutcomesPerPhase=outcomes,changedFilterOutcomes=len(changes),smallWatchdogProcesses=37,timeouts=0,solutionPassed=22007,normalQt3=[31161,0,660],qt4Gated=[8438,0,1892],qt4ExperimentalPassed=11,removedGatedSkips=['compare-010'],retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1892,exploratoryNewPasses=['K-SeqDeepEqualFunc-56','K-SeqDeepEqualFunc-57','fn-substring-after-24','fn-substring-before-24'],candidateVersion='1.26.11',publishedVersion='1.26.3'),changedFilters=changes,limits=['Eight direct XPath failures retained;their original XQuery dependency guards still exclude them','Only existing HTML ASCII/UCA mechanisms;not support for every collation URI','No engine,fixture,assertion,GatedSets,schema,QT3 baseline or release change','No complete exploratory qt4 sweep;only13 named cases executed separately','REQ-126 remains Complete;Bosak.Schema retains XSD1.1 ownership'],next='Review the four original qt4 fn-format-integer-CLDR declarations against the existing QT3 observations;do not infer complete CLDR support from two passing assertions.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
