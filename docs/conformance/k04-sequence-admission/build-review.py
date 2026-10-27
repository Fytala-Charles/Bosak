"""Rebuild exact known-unsupported sequence proposal and isolated gate deltas."""
from pathlib import Path
import json,re
C=Path(__file__).resolve().parent;E=C/'evidence';R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
probes=load(E/'probe-receipts.json');expressions=load(E/'expression-control-receipts.json');normal=load(E/'current-normal-receipts.json');mechanism=load(E/'mechanism-receipt.json')
assert len(probes)==2 and len(expressions)==16 and len(normal)==2
for row in probes+expressions:assert row['exitCode']==0 and not row['timeout'] and row['result']['kind']=='Passed'
for row in normal:assert row['exitCode']==0 and not row['timeout'] and row['totals']==[1,0,0,1]
assert mechanism['exitCode']==0 and not mechanism['timeout'] and mechanism['results'][0]==dict(codepoint=64500,value=1,supported=False,result='') and mechanism['results'][1]['supported']
inv=load(E/'sequence-inventory.json');assert inv['scannedCases']=={'qt3':31821,'qt4':46985} and len(inv['declarations'])==18
assert all(not r['satisfied'] and r['name']=='format-integer-042' for r in inv['declarations'] if r['value']=='\ufbf4')
changes=[];outcomes=0
for suite in ['qt3','qt4']:
 before={r['id']:r for r in map(json.loads,read(E/(suite+'-current.jsonl')).splitlines())};after={r['id']:r for r in map(json.loads,read(E/(suite+'-prototype.jsonl')).splitlines())};assert len(before)==len(after)==(136 if suite=='qt3' else 152)
 for key in before:
  for field in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):
   old,new=before[key][field],after[key][field];outcomes+=1
   if old!=new:
    assert key in [suite+'-sequence-\ufbf4-True',suite+'-sequence-\ufbf4-False',suite+'-original042'];changes.append(dict(id=key,profile=field,before=old,after=new))
 for key in [suite+'-sequence-\ufbf4-True',suite+'-sequence-\ufbf4-False']:
  assert after[key]['xpathSupported']==key.endswith('False')
assert len(changes)==8
for r in load(E/'filter-receipts.json')+load(E/'rehearsal-receipts.json'):assert r['exitCode']==0 and not r['timeout']
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
for suite,passed,skip in [('qt3',31162,659),('qt4',8587,1892)]:
 before=skips(R/('docs/conformance/k04-parse-integer-fix/evidence/'+suite+'-skips.txt'));after=skips(E/(suite+'-rehearsal-skips.txt'));assert len(after)==skip and all(before.get(k)==v for k,v in after.items()) and {k:v for k,v in before.items() if k not in after}=={'format-integer-042':'Unsupported dependency'}
 assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skip),read(E/(suite+'-rehearsal.log')))
result=dict(date='2026-10-27',slice='K04-C12',status='Exact baseline proposal ready;owner approval pending',sourceBase=load(E/'PROPOSAL-PIN.json')['sourceBase'],decisionId='K04-sequence-adoption-v1',summary=dict(originalAssertionsPassed=2,currentNormalOriginalSkipped=2,expressionControlsPassed=16,mechanismControls=2,filterControls=288,filterProfileOutcomesPerMode=outcomes,changedFilterOutcomes=len(changes),smallWatchdogProcesses=25,completeRehearsals=2,totalWatchdogProcesses=27,timeouts=0,currentQt3=[31161,0,660],proposedQt3=[31162,0,659],currentQt4=[8586,0,1893],rehearsalQt4=[8587,0,1892],removedSkipsEach=['format-integer-042'],retainedQt3NamesAndReasons=659,retainedQt4NamesAndReasons=1892,normalSourceChanged=False,ownerApproved=False,inheritedSolutionPassed=22198,candidateVersion='1.28.0',publishedVersion='1.26.3'),changedPredicates=changes,limits=['Recognizes only the reviewed unsupported FBF4 sequence;other names/kinds retain existing policy','No engine semantics,fixture,assertion,version,schema or gate-membership change','Normal source and AGENTS remain unchanged pending explicit exact-baseline approval','Inherited unit evidence is source-identical;fresh complete normal gates required after adoption','REQ-126 remains Complete;Bosak.Schema owns staged XSD1.1','No release or upstream submission'])
(C/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print(json.dumps(result['summary']))
