"""Rebuild quote-routing proposal evidence without adopting the normal baseline."""
from pathlib import Path
import json,re,collections
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def skips(p):
 d={};reason=None
 for line in p.read_text(encoding='utf-8-sig').splitlines():
  m=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
  if m:reason=m.group(2)
  elif line.strip():assert reason is not None and line not in d;d[line]=reason
 return d
rows=load(E/'results.json');boundaries=load(E/'boundary-results.json');assert len(rows)==32 and len(boundaries)==22
for r in rows+boundaries:assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 and r['result']
pairs=collections.defaultdict(dict)
for r in rows:pairs[r['name']][r['mode']]=r['result']
assert len(pairs)==16 and all(p['prototype']['kind']=='Passed' for p in pairs.values())
assert collections.Counter(p['current']['kind'] for p in pairs.values())=={'Skipped':8,'Failed':7,'Passed':1}
bp=collections.defaultdict(dict)
for r in boundaries:bp[r['name']][r['mode']]=r['result']
assert len(bp)==11 and all(p['current']==p['prototype'] for p in bp.values())
controls=load(E/'controls.json');cr=load(E/'control-receipts.json');assert len(controls)==13 and len(cr)==2
byMode={r['mode']:{x['id']:x['accepted'] for x in r['result']} for r in cr}
for r in cr:assert r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20
assert all(byMode['prototype'][c['id']]==c['expected'] for c in controls)
assert sum(byMode['prototype'][c['id']]!=byMode['current'][c['id']] for c in controls)==8
run=load(E/'rehearsal-receipt.json');assert run['exitCode']==0 and not run['timeout'] and run['watchdogSeconds']==300
text=(E/run['log']).read_text(encoding='utf-8-sig')
for label,n in [('Total',31821),('Passed',31161),('Failed',0),('Skipped',660)]:assert re.search(r'^'+label+r':\s+'+str(n)+r'\s*$',text,re.M)
before=skips(C.parent/'k03-name-storage/evidence/qt3-skips.txt');after=skips(E/'rehearsal-skips.txt');assert len(before)==676 and len(after)==660
assert set(before)-set(after)==set(pairs) and not set(after)-set(before) and all(before[n]==v for n,v in after.items())
(E/'skip-delta.json').write_text(json.dumps(dict(removed={n:before[n] for n in sorted(set(before)-set(after))},added={},retained=after),indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
summary=dict(candidateCases=16,candidateProcesses=32,prototypePassed=16,boundaryCases=11,boundaryProcesses=22,boundariesUnchanged=True,lexicalControls=13,lexicalControlProcesses=2,smallWatchdogProcesses=56,fullRehearsals=1,timeouts=0,currentQt3=[31145,0,676],rehearsalQt3=[31161,0,660],removedSkips=16,retainedSkipNamesReasons=660,normalChanged=False,ownerApproved=False,inheritedSolutionPassed=22007,inheritedQt4=[8437,0,1893],candidate='1.26.11',published='1.26.3')
out=dict(date='2026-10-27',sourceBase=load(E/'DESIGN-PIN.json')['sourceBase'],status='K04-C01 correction verified in isolated harness; exact normal adoption awaits owner approval',summary=summary,candidatePairs=pairs,boundaryPairs=bp,lexicalControlReceipts=cr,fullRehearsal=run,removedSkips=sorted(pairs),decision=dict(id='K04-quote-routing-adoption-v1',proposal='BASELINE-UPDATE-PROPOSAL.md',patch='PROPOSED-ADOPTION.patch',requiresOwnerApproval='AGENTS.md requires exactly31145/0/676; proposed normal routing changes the exact result to31161/0/660.'),policy='No tracked engine/harness/filter/AGENTS/fixture/assertion/baseline/version change. Prototype removes only the raw triple-quote refusal; existing lexical guards, schema/profile/dependency predicates and parser error semantics remain. Original assertions and serialization output-option handling are retained. Unit/qt4 receipts inherited from unchanged engine; no new full-suite claim. No release/upstream/Braid action.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
