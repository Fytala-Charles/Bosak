"""Rebuild bounded K04-C02 results from actual-filter controls and fresh gates."""
from pathlib import Path
import json,re,subprocess,sys
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
subprocess.run([sys.executable,str(E/'build-inventory.py')],check=True,stdout=subprocess.DEVNULL)
controls=load(E/'controls.json');changed=[];outcomes=0
for suite in ['qt3','qt4']:
 before=[json.loads(l) for l in read(E/(suite+'-before.jsonl')).splitlines()];after=[json.loads(l) for l in read(E/(suite+'-after.jsonl')).splitlines()];assert len(before)==len(after)==37
 for b,a in zip(before,after):
  assert b['id']==a['id'] and a['xpathSupported']==b['revisedXPath'] and a['xquerySupported']==b['revisedXQuery']
  assert a['xpathSupported']==a['revisedXPath'] and a['xquerySupported']==a['revisedXQuery']
  request=next(q for q in controls if q['id']==a['id'])
  if 'contradiction-' in a['id']:assert not a['xpathSupported'] and not a['xquerySupported']
  if all(d['satisfied'] for d in request['dependencies']) or all(d['type'] not in ['spec','default-language'] for d in request['dependencies']):assert (b['xpathSupported'],b['xquerySupported'])==(a['xpathSupported'],a['xquerySupported'])
  for field in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):
   outcomes+=1
   if b[field]!=a[field]:changed.append(dict(id=a['id'],profile=field,before=b[field],after=a[field]))
 for phase in ['before','after']:
  for r in load(E/(phase+'-receipts.json')):assert r['exitCode']==0 and not r['timeout']
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
R=C.parents[2]
assert skips(E/'qt3-skips.txt')==skips(R/'docs/conformance/k04-quote-routing/adoption/qt3-skips.txt')
assert skips(E/'qt4-skips.txt')==skips(R/'docs/conformance/k03-name-storage/evidence/qt4-skips.txt')
for file,counts in [('qt3.log',(31161,0,660)),('qt4.log',(8437,0,1893))]:
 s=read(E/file);assert re.search(r'Passed:\s+'+str(counts[0])+r'\s+Failed:\s+0\s+Skipped:\s+'+str(counts[2]),s)
tests=read(E/'solution-tests.log');passed=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',tests);assert len(passed)==10 and sum(map(int,passed))==22007 and 'Failed!' not in tests
assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C02',status='Complete for bounded spec/default-language polarity;broader profile policy remains open',sourceBase=load(E/'REVIEW-PIN.json')['sourceBase'],summary=dict(controls=74,profileOutcomesPerPhase=outcomes,changedProfileOutcomes=len(changed),smallProcesses=4,timeouts=0,inventory=load(E/'inventory.json'),normalQt3=[31161,0,660],qt4Gated=[8437,0,1893],solutionPassed=22007,retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1893,baselineChanged=False,candidateVersion='1.26.11',publishedVersion='1.26.3'),changedControls=changed,limits=['Positive predicates and routing policy unchanged','Other negative dependency types and unknown positive feature policy unchanged','No XSD1.1 capability added;Bosak.Schema owns staged implementation','All46985 qt4 cases inventoried as metadata;only10330 gated cases executed','REQ-126 remains Complete for accepted bounded catalog','No release or upstream submission'])
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
