"""Rebuild bounded CLDR admission results, preserving exploratory failures."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
normal=load(E/'normal-case-receipts.json');assert len(normal)==6 and all(r['totals']==[1,1,0,0] and r['exitCode']==0 and not r['timeout'] for r in normal)
assert load(E/'format-integer-before-receipt.json')['totals']==[102,82,15,5] and load(E/'format-integer-after-receipt.json')['totals']==[102,86,15,1]
for phase in ['before','after']:
 r=load(E/('format-integer-'+phase+'-receipt.json'));assert r['exitCode']==0 and not r['timeout']
failures=re.findall(r'^  FAIL ([^:]+): (.*)$',read(E/'format-integer-before.log'),re.M);assert failures==re.findall(r'^  FAIL ([^:]+): (.*)$',read(E/'format-integer-after.log'),re.M) and len(failures)==15
assert [r['name'] for r in load(E/'remaining-radix-findings.json')['cases']]==[name for name,msg in failures]
changes=[];outcomes=0
for suite in ['qt3','qt4']:
 before={r['id']:r for r in map(json.loads,read(E/(suite+'-before.jsonl')).splitlines())};after={r['id']:r for r in map(json.loads,read(E/(suite+'-after.jsonl')).splitlines())};assert len(before)==len(after)==(113 if suite=='qt3' else 129)
 expected={'feature-qt4-fn-format-integer-CLDR-True','feature-qt4-fn-format-integer-CLDR-False'}|{'qt4-format-integer-'+n+'-full' for n in ['065','066','076','077']}
 for key in before:
  for field in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):
   old,new=before[key][field],after[key][field];outcomes+=1
   if old!=new:assert suite=='qt4' and key in expected;changes.append(dict(id=key,before=old,after=new))
   else:assert suite=='qt3' or key not in expected
 for phase in ['before','after']:
  for r in load(E/(phase+'-receipts.json')):assert r['exitCode']==0 and not r['timeout']
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
before=skips(E/'format-integer-before-skips.txt');after=skips(E/'format-integer-after-skips.txt');assert after=={'format-integer-042':'Unsupported dependency'} and {k:v for k,v in before.items() if k not in after}=={'format-integer-'+n:'Unsupported dependency' for n in ['065','066','076','077']}
for suite,passed,skipped in [('qt3',31161,660),('qt4',8438,1892)]:
 assert skips(E/(suite+'-skips.txt'))==skips(R/('docs/conformance/k04-cldr-fix/evidence/'+suite+'-skips.txt'))
 assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skipped),read(E/(suite+'.log')))
p=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(p)==10 and sum(map(int,p))==22043 and 'Failed!' not in read(E/'solution-tests.log');assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C07',status='Bounded qt4 CLDR admission complete;15 exploratory radix errors retained',sourceBase=load(E/'ADMISSION-PIN.json')['sourceBase'],summary=dict(filterControls=242,profileOutcomesPerPhase=outcomes,changedFilterOutcomes=len(changes),normalOriginalAssertionsPassed=6,newQt4ExploratoryPasses=4,exploratorySetBefore=[102,82,15,5],exploratorySetAfter=[102,86,15,1],exploratoryFailuresRetained=15,remainingSetDependencySkips=1,smallWatchdogProcesses=12,timeouts=0,solutionPassed=22043,normalQt3=[31161,0,660],qt4Gated=[8438,0,1892],retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1892,gatedSetsChanged=False,baselineChanged=False,candidateVersion='1.26.12',publishedVersion='1.26.3'),changedFilterAssessments=changes,removedExploratorySkips=['format-integer-'+n for n in ['065','066','076','077']],limits=['Admission covers reviewed bounded word-numbering behavior;not complete CLDR/RBNF','Existing language type admission is unchanged;default-language guard remains distinct from full locale support','No engine,fixture,assertion,GatedSets,schema,QT3 baseline or version change','The exploratory set remains ungated because15 failures persist','No complete exploratory qt4 sweep;only the102-case format-integer set executed','Bosak.Schema owns XSD1.1;REQ-126 remains Complete;no release or upstream contribution'],next='Review the15 retained XPath4 radix-picture failures against immutable specification evidence before implementing a bounded radix-formatting slice.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
