"""Verify the frozen date-arity contract review and preserved diagnostic failures."""
from pathlib import Path
import json,subprocess,hashlib,re,collections,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
def hashes(rows):
 actual=subprocess.check_output(['git','hash-object','--stdin-paths'],input=('\n'.join(rows)+'\n').encode(),cwd=R).decode().splitlines();assert actual==list(rows.values())
def totals(p):
 s=read(p);return [int(re.search(r'^'+name+r':\s+(\d+)',s,re.M)[1]) for name in ['Passed','Failed','Skipped']]
pin=load(E/'SPECIFICATION-REVIEW.json');assert not git('diff',pin['sourceBase'],'--','src','tests','AGENTS.md')
hashes(load(E/'normal-source-identities.json'))
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
for name,h in load(E/'fixture-hashes.json').items():assert sha(R/name)==h,name
for name,h in load(E/'binary-identities.json').items():assert sha(R/name)==h,name
assert pin['revision']=='e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e' and pin['sha256']=='409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5'
assert pin['kind']=='Review only; no semantic adoption'
for sig in pin['signatures']:
 assert sig['returnType']=='xs:string?' and len(sig['arguments'])==5
 assert [a['name'] for a in sig['arguments']]==['value','picture','language','calendar','place']
 assert [a.get('default') for a in sig['arguments']]==[None,None,'()','()','()']
 assert all(c['PR']=='901' for c in sig['changes'])
assert git('-C',str(R/'tests/qt4tests'),'rev-parse','HEAD')==pin['fixtureRevision']
assert not git('-C',str(R/'tests/qt4tests'),'status','--porcelain')
# Original requests remain exact projections of the unmodified frozen fixtures.
ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'};originals=load(E/'original-requests.json');assert len(originals)==8
for row in originals:
 case=next(c for c in X.parse(R/row['SetFile']).getroot().findall('t:test-case',ns) if c.get('name')==row['Name'])
 assert case.get('covers-40')=='PR901' and row['expression']==case.find('t:test',ns).text
assert len(load(E/'control-contract.json'))==96
receipts=load(E/'probe-receipts.json');assert len(receipts)==104
assert all(r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 for r in receipts)
for row in receipts:assert json.loads(read(E/row['log']).splitlines()[-1])==row['result']
orig=[r for r in receipts if r['kind']=='original'];controls=[r for r in receipts if r['kind']=='control']
assert len(orig)==8 and all(r['result']['kind']=='Failed' and 'XPST0017' in r['result']['message'] for r in orig)
assert collections.Counter(r['result']['kind'] for r in controls)=={'Passed':45,'Failed':51}
assert sum(r['result']['message']=='Unexpected error: XPTY0004' for r in controls)==30
sets=load(E/'normal-set-receipts.json');assert len(sets)==2 and all(r['exitCode']==0 and not r['timeout'] for r in sets)
assert totals(E/'fn-format-date.log')==[208,6,18] and totals(E/'fn-format-time.log')==[92,2,4]
failed=[]
for name in ['fn-format-date','fn-format-time']:
 failed.extend(re.findall(r'^  FAIL ([^:]+):',read(E/(name+'.log')),re.M))
assert set(failed)=={r['Name'] for r in originals} and len(failed)==8
inherited=load(R/'docs/conformance/k04-calendar-admission/adoption/validation.json');assert inherited['solutionPassed']==22579 and inherited['normalQt3']==[31164,0,657]
results=load(P/'RESULTS.json');assert results['boundedProcesses']==106 and results['semanticAdoption'] is False
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
hashes(manifest['documents'])
renders=load(E/'render-receipts.json');assert len(renders['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
print('PASS:8original missing-arity failures;96controls45passed/51failed;30empty-value findings;106bounded processes;unchanged engine/gates;historical evidence preserved.')
