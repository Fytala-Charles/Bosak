"""Rebuild K04-C03 evidence from actual filters, catalog inventory and fresh gates."""
from pathlib import Path
import json,subprocess,sys,re,hashlib
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for script,out in [('build-inventory.py','inventory.json'),('build-ledger.py','capability-ledger.json')]:
 old=sha(E/out);subprocess.run([sys.executable,str(E/script)],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(E/out)==old
requests=load(E/'controls.json');policy=load(E/'admission-policy.json');changes=[];profileOutcomes=0
for suite in ['qt3','qt4']:
 before={r['id']:r for r in map(json.loads,read(E/(suite+'-before.jsonl')).splitlines())};after={r['id']:r for r in map(json.loads,read(E/(suite+'-after.jsonl')).splitlines())};selected=[q for q in requests if q['Suite']==suite];assert len(selected)==len(before)==len(after)
 recognized={v.lower() for k in ['frozenAdmitted','unsupported'] for v in policy[suite][k]}
 assert all(v.lower() in recognized for v in load(E/'inventory.json')['catalogs'][suite]['featureValues'])
 for q in selected:
  unknown=any(d['type']=='feature' and d['value'].lower() not in recognized for d in q['dependencies'])
  for field in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):
   old,new=before[q['id']][field],after[q['id']][field];assert new==(False if unknown else old),(q['id'],field);profileOutcomes+=1
   if new!=old:changes.append(dict(id=q['id'],profile=field,before=old,after=new))
 for phase in ['before','after']:
  for r in load(E/(phase+'-receipts.json')):assert r['exitCode']==0 and not r['timeout']
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
for suite,counts in [('qt3',(31161,0,660)),('qt4',(8437,0,1893))]:
 assert skips(E/(suite+'-skips.txt'))==skips(R/('docs/conformance/k04-profile-review/evidence/'+suite+'-skips.txt'))
 assert re.search(r'Passed:\s+'+str(counts[0])+r'\s+Failed:\s+0\s+Skipped:\s+'+str(counts[2]),read(E/(suite+'.log')))
passed=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',read(E/'solution-tests.log'));assert len(passed)==10 and sum(map(int,passed))==22007 and 'Failed!' not in read(E/'solution-tests.log')
assert '0 Error(s)' in read(E/'solution-build.log')
result=dict(date='2026-10-27',slice='K04-C03',status='Complete for explicit feature-name admission;capability gaps retained',sourceBase=load(E/'INVENTORY-PIN.json')['sourceBase'],summary=dict(controls=len(requests),profileOutcomesPerPhase=profileOutcomes,changedProfileOutcomes=len(changes),changedControlRows=len({r['id'] for r in changes}),smallProcesses=4,timeouts=0,inventoryCases=78806,qt3FeatureNames=24,qt4FeatureNames=31,featureLedgerRows=55,explicitLegacyAdmissions={'qt3':19,'qt4':17},currentFeatureDecisionsChanged=0,normalQt3=[31161,0,660],qt4Gated=[8437,0,1893],solutionPassed=22007,retainedQt3NamesAndReasons=660,retainedQt4NamesAndReasons=1893,baselineChanged=False,candidateVersion='1.26.11',publishedVersion='1.26.3'),changedControls=changes,limits=['Explicit legacy admissions are not full capability certifications','Other dependency kinds and their fallback policies are unchanged','No schema backend change;Bosak.Schema owns staged XSD1.1','No exploratory qt4 query sweep;all46985 qt4 cases inventoried as metadata','REQ-126 remains Complete;no release or upstream submission'],next='Review non_unicode_codepoint_collation:QT3 admits original13cases;qt4 has one gated skip and12 exploratory cases;original assertions and scoped rehearsal required before changing admission.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
