"""Verify C02 contract reconciliation, known unmet controls and immutable earlier checkpoints."""
from pathlib import Path
import collections,hashlib,json,re,shutil,subprocess,sys,tempfile
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(data):return hashlib.sha256(data).hexdigest()
def normalized(p):return p.read_bytes().replace(bytes([13,10]),bytes([10]))
def logtext(p):
 data=p.read_bytes();return data.decode('utf-16' if data.startswith(bytes([255,254])) else 'utf-8-sig')
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
m=load(E/'manifest.json')
for name,expected in m['files'].items():
 assert sha((E/name).read_bytes())==expected,name
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/name).relative_to(R).as_posix()))==expected,'index '+name
for name,expected in m['sourceFiles'].items():assert git('hash-object',name).decode().strip()==expected,name
for name,expected in m['tools'].items():assert git('hash-object',name).decode().strip()==expected,name
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={name:sha(normalized(C/name)) for name in m['outputs']}
assert before==m['outputs']
for name in ['build-c02-review.py','build-closure-plan.py']:
 subprocess.run([sys.executable,str(C/name)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert {name:sha(normalized(C/name)) for name in m['outputs']}==before,'deterministic outputs'
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'FUNCTION-OPTIONS-RECONCILIATION.json');decisions=load(C/'C02-REVIEW-DECISIONS.json')
assert plan['openClosureWorkpackages']==11 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C02')['scope']['entries']
assert set(scope)==set(decisions['entries'])=={v['entry'] for v in review['entries']}
assert len(review['entries'])==30 and len(review['functions'])==74 and len(review['followups'])==14
s=review['summary'];assert (s['profileObservations'],s['assertedControls'],s['assertedControlPasses'],s['assertedControlFailures'],s['diagnosticObservations'])==(195,120,118,2,75)
requests=load(E/'probe-requests.json');observations=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()]
assert len(requests)==94 and len({v['id'] for v in requests})==94
assert {(v['id'],v['profile']) for v in observations}=={(q['id'],p) for q in requests for p in q['profiles']} and len(observations)==195
unmet=[];controlcount=0
for q in requests:
 for v in [v for v in observations if v['id']==q['id']]:
  if q['expected'] is None and q['error'] is None:continue
  controlcount+=1
  passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result']==q['expected']
  if not passed:unmet.append((q['id'],v['profile']))
assert controlcount==120 and set(unmet)=={('atomic-annotation-empty','XPath40'),('atomic-annotation-empty','XPath40Experimental')}
assert all(v['result']=='(map)' and v['error'] is None for v in observations if v['id']=='atomic-annotation-empty')
assert requests[[v['id'] for v in requests].index('atomic-annotation-empty')]['error']=='XPTY0004','independent unmet expectation retained'
assert decisions['tickets']['F08']['boundedCorrections'][0]['id']=='F08-atomic-annotation-empty'
assert all('probe:atomic-annotation-empty' not in v['errorBoundaryEvidence'] for v in review['functions']),'do not count the failure as supported refusal'
caseindex=load(E/'catalog-case-inventory.json');assert len(caseindex)==3148
tree=git('ls-tree','-r',m['documentationBase'],'--','docs/xpath40-catalog/examples').decode().splitlines()
blobs={line.split('\t',1)[1]:line.split('\t',1)[0].split()[2] for line in tree}
for v in caseindex:assert blobs['docs/xpath40-catalog/examples/'+v['case']['file']]==v['sourceGitBlob'],v['case']['id']
for row in review['entries']:
 text=(C/row['entry']).read_text(encoding='utf-8');assert 'C02 reconciliation' in text
 assert len(re.findall(r'^## \d+\.',text,re.M))==16,row['entry']
 assert row['documentedContracts'] and row['limitations'] and row['evidence']
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+row['entry']).decode().strip()==row['sourceGitBlob']
for fn in review['functions']:assert fn['normalEvidence'] and fn['unsupportedRemainder']
text=logtext(E/'solution-tests.log');counts=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+)',text)
assert len(counts)==10 and tuple(sum(int(v[i]) for v in counts) for i in range(3))==(0,21798,0)
assert re.search(r'0 Error\(s\)',logtext(E/'build.log'))
assert re.search(r'Passed:\s+3148',logtext(E/'catalog-tests.log'))
render=load(E/'render-receipts.json');assert len(render['pages'])==43
assert sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for name in ['FUNCTION-OPTIONS-RECONCILIATION.md','CLOSURE-PLAN.md',*scope]:
 for target in re.findall(r'\]\(([^)]+)\)',(C/name).read_text(encoding='utf-8')):
  if not target.startswith(('http:','https:','#')):assert ((C/name).parent/target.split('#')[0]).exists(),(name,target)
# Historical validators expect their original closure counts. Preserve all their
# evidence bytes and reproduce original plans with exact historical builders in isolation.
for folder in ['closure-triage','c01-inventory']:
 prior=C/'evidence'/folder;original=load(prior/'manifest.json')
 for name,item in original['files'].items():
  expected=item['sha256'] if isinstance(item,dict) else item
  assert sha((prior/name).read_bytes())==expected,folder+'/'+name
 blob=original.get('builderGitBlob') or original['sourceFiles']['docs/xpath40-catalog/build-closure-plan.py']
 with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
  historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage')
  builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob))
  subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
  for name,expected in original['outputs'].items():
   if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:assert sha(normalized(historical/name))==expected,folder+' original '+name
 if folder=='c01-inventory':assert sha(normalized(C/'CURRENT-FUNCTION-INVENTORY.json'))==original['outputs']['CURRENT-FUNCTION-INVENTORY.json']
if '--local-artifacts' in sys.argv:
 for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C02 verified: 30 entries/74 functions; 21,798 solution and 3,148 catalog passes; 118 matching controls/2 retained unmet controls/75 diagnostics; 11 packages open. Prior evidence preserved; no semantic acceptance.')
