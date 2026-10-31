"""Validate C03 controls, source identities and unchanged earlier closure evidence."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(data):return hashlib.sha256(data).hexdigest()
def normalized(p):return p.read_bytes().replace(bytes([13,10]),bytes([10]))
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
def logtext(p):
 data=p.read_bytes();return data.decode('utf-16' if data.startswith(bytes([255,254])) else 'utf-8-sig')
m=load(E/'manifest.json')
for name,expected in m['files'].items():
 assert sha((E/name).read_bytes())==expected,name
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/name).relative_to(R).as_posix()))==expected,'index '+name
for name,expected in {**m['sourceFiles'],**m['tools']}.items():assert git('hash-object',name).decode().strip()==expected,name
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={name:sha(normalized(C/name)) for name in m['outputs']};assert before==m['outputs']
for name in ['build-c03-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/name)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert {name:sha(normalized(C/name)) for name in m['outputs']}==before,'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'CONSTRUCTOR-TYPE-RECONCILIATION.json');decisions=load(C/'C03-REVIEW-DECISIONS.json')
assert plan['openClosureWorkpackages']==10 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02','C03']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C03')['scope']['entries']
assert set(scope)==set(decisions['entries'])=={v['entry'] for v in review['entries']} and len(scope)==8
requests=load(E/'probe-requests.json');obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()]
assert len(requests)==631 and len({v['id'] for v in requests})==631 and len(obs)==1337
assert {(v['id'],v['profile']) for v in obs}=={(v['id'],p) for v in requests for p in v['profiles']}
controls=0
for q in requests:
 for v in [v for v in obs if v['id']==q['id']]:
  if q['expected'] is None and q['error'] is None:continue
  controls+=1
  assert q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and q['expected']==v['result'],(q['id'],v)
assert controls==1315 and len(review['constructors'])==49
for row in review['constructors']:
 assert len(row['observations'])==23
 assert any(v['id'].endswith('-missing') and 'XPDY0002' in (v['error'] or '') for v in row['observations'])
 assert any(v['id'].endswith('-old-profile') and 'XPST0017' in (v['error'] or '') for v in row['observations'])
 assert sum(v['id'].endswith('-argument-empty') and v['result']=='0' for v in row['observations'])==3
 assert row['uninhabited']==(row['name']=='xs:error')
assert {v['name'] for v in review['constructors']}=={v['name'] for v in load(E/'retained-focus-audit.json')['constructors']}
hostrequests=load(E/'host-requests.json');hostobs=[json.loads(v) for v in (E/'host-results.jsonl').read_text().splitlines()]
assert len(hostrequests)==10 and len(hostobs)==20
hostcontrols=0
for q in hostrequests:
 matches=[v for v in hostobs if v['id']==q['id']];assert {v['host'] for v in matches}=={'Legacy','Xslt40Draft'}
 for v in matches:
  if q['expected'] is not None:hostcontrols+=1;assert v['error'] is None and q['expected']==v['result'],v
  else:assert q['id']=='nominal-static-external-override' and 'XTTE0570' in v['error']
assert hostcontrols==18
s=review['summary'];assert (s['focusMatrixObservations'],s['totalObservations'],s['totalAssertedControls'],s['unmetControls'])==(1127,1357,1333,0)
assert review['boundaries']['F08-atomic-annotation-empty']['status'].startswith('Still open')
for name,count in [('schema',70),('api',119),('host',56)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(name+'-tests.log')))
for name,item in review['namedTests'].items():
 text=(R/name).read_text(encoding='utf-8-sig');assert git('hash-object',name).decode().strip()==item['gitBlob']
 assert all(method+'(' in text for method in item['methods'])
for row in review['entries']:
 text=(C/row['entry']).read_text(encoding='utf-8');assert 'C03 review' in text and len(re.findall(r'^## \d+\.',text,re.M))==16
 assert row['contracts'] and row['boundary'] and row['groups']
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+row['entry']).decode().strip()==row['sourceGitBlob']
record=load(C/'RECORD-CONTRACTS.json')
for name,key in [('current-expressions.xml','expressions.xml'),('current-xpath-grammar.xml','xpath-grammar.xml')]:assert sha((E/name).read_bytes())==record['specification']['files'][key]
render=load(E/'render-receipts.json');assert len(render['pages'])==24 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for page in render['pages']:
 p=R/page['source']
 text=p.read_text(encoding='utf-8')
 # Review complete scoped pages and newly added canonical notices. Historical
 # handover/code examples can contain ](...) that is not a Markdown link.
 targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in {'docs/xpath40-catalog/'+n for n in [*scope,'CONSTRUCTOR-TYPE-RECONCILIATION.md','CLOSURE-PLAN.md']} else re.findall(r'\[Constructor/type review\]\(([^)]+)\)',text)
 for target in targets:
  if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
# Original manifests retain their exact bytes. Their historical builders run in
# isolation so verification does not reset current package progress.
for folder in ['closure-triage','c01-inventory','c02-function-review']:
 prior=C/'evidence'/folder;original=load(prior/'manifest.json')
 for name,item in original['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
 blob=original.get('builderGitBlob') or (original.get('tools') or original['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
 with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
  historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage')
  builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
  for name,expected in original['outputs'].items():
   if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:assert sha(normalized(historical/name))==expected,folder+' historical '+name
   else:assert sha(normalized(C/name))==expected,folder+' unchanged '+name
if '--local-artifacts' in sys.argv:
 for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C03 verified: eight entries/49 constructors; 1,333 matching controls +24 observations; 245 fresh focused tests; 10 packages open. Earlier evidence preserved; C02 defect and AC01-04 remain open.')
