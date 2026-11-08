"""Validate C05 bounded evidence, current-draft mismatches and immutable prior checkpoints."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET
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
for name,expected in m['specFiles'].items():assert sha((C/'evidence/c03-type-review'/name).read_bytes())==expected,name
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={name:sha(normalized(C/name)) for name in m['outputs']};assert before==m['outputs']
for name in ['build-c05-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/name)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert {name:sha(normalized(C/name)) for name in m['outputs']}==before,'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'TYPE-COLLECTION-PATTERN-RECONCILIATION.json');decisions=load(C/'C05-REVIEW-DECISIONS.json')
assert plan['openClosureWorkpackages']==8 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02','C03','C04','C05']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C05')['scope']['grammarRows'];assert set(scope)==set(decisions['rows'])=={v['key'] for v in review['rows']} and len(scope)==9
q={v['id']:v for v in load(E/'probe-requests.json')};obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()]
assert len(q)==111 and len(obs)==219 and {(v['id'],v['profile']) for v in obs}=={(v['id'],p) for v in q.values() for p in v['profiles']}
unmet=[]
for v in obs:
 req=q[v['id']];passed=req['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if req['error'] else v['error'] is None and v['result']==req['expected']
 if req['requiredPhase']:passed=passed and req['requiredPhase']==v['phase']
 if not passed:unmet.append((v['id'],v['profile']))
expected={(name,p) for name in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first','destructure-record-undeclared','jnode-generic-node','jnode-identity','xnode-xml'] for p in ['XPath40','XPath40Experimental']}
assert len(unmet)==14 and set(unmet)==expected and all(q[name]['kind']=='current-draft-control' for name,_ in unmet)
assert all(q[name]['expected']=='true' for name in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first'])
assert q['destructure-record-undeclared']['error']=='XPTY0004'
assert all(v['errorCode']=='FOER0000' for v in obs if v['id']=='destructure-body-error')
for name in ['members-roundtrip','members-empty','of-members-empty','jnode-root-type','jnode-key-type']:
 assert any(v['id']==name and v['phase']=='success' for v in obs),name
h={v['id']:v for v in load(E/'host-requests.json')};hostobs=[json.loads(v) for v in (E/'host-results.jsonl').read_text().splitlines()];hostunmet=[]
assert len(h)==28 and len(hostobs)==56
for v in hostobs:
 req=h[v['id']];passed=req['error'] in (v['error'] or '') if req['error'] else v['error'] is None and v['result']==req['expected']
 if not passed:hostunmet.append((v['id'],v['host']));assert 'XTSE0340' in v['error'] and v['phase']=='compile'
assert set(hostunmet)=={('jnode-pattern','Legacy'),('jnode-pattern','Xslt40Draft')}
assert {(v['id'],v['host']) for v in hostobs}=={(req['id'],p) for req in h.values() for p in ['Legacy','Xslt40Draft']}
s=review['summary'];assert (s['totalObservations'],s['matchingControls'],s['unmetControls'],s['knownModelBoundaryControls'],s['freshTests']['total'])==(275,259,16,8,2457)
sources=load(E/'production-sources.json');grammar=ET.parse(C/'evidence/c03-type-review/current-xpath-grammar.xml').getroot();expr=ET.parse(C/'evidence/c03-type-review/current-expressions.xml').getroot();xslt=ET.parse(C/'evidence/c03-type-review/current-xslt.xml').getroot()
actual={(v.tag.rsplit('}',1)[-1],v.get('name'),v.get('if'),ET.tostring(v,encoding='unicode')) for v in grammar.iter() if v.tag.rsplit('}',1)[-1] in ['production','token']}
for key,row in sources.items():
 assert row['retained']['key']==key and row['grammar']
 for v in row['grammar']:assert (v['kind'],v['name'],v['condition'],v['xml']) in actual
 for v in row['sections']:
  root=expr if v['source']=='expressions' else xslt
  assert any(node.get('id')==v['id'] and ET.tostring(node,encoding='unicode')==v['xml'] for node in root.iter())
assert len({v['name'] for row in sources.values() for v in row['grammar']})==49
matrix=load(C/'GRAMMAR-COVERAGE-MATRIX.json');original=json.loads(git('show',m['documentationBase']+':docs/xpath40-catalog/GRAMMAR-COVERAGE-MATRIX.json'));oldrows={v['key']:v for v in original['rows']}
assert len(matrix['rows'])==18 and matrix['currentReview']['reviewedRowCount']==15 and matrix['currentReview']['otherRows']==3
assert {v['key'] for v in matrix['rows'] if v.get('currentReview',{}).get('status','').startswith('C05')}==set(scope)
for row in matrix['rows']:
 old=oldrows[row['key']];a=dict(row);b=dict(old);a.pop('currentReview',None);b.pop('currentReview',None);assert a==b
 if 'currentReview' in old:assert row['currentReview']==old['currentReview'],'preserve C04 annotation'
entries=load(E/'entry-sources.json');assert len(entries)==21;structured=0
for name,item in entries.items():
 text=(C/name).read_text(encoding='utf-8');assert 'C05 grammar review' in text
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+name).decode().strip()==item['gitBlob']
 if '## 12. Implementation' in text:assert len(re.findall(r'^## \d+\.',text,re.M))==16;structured+=1
assert structured==17
for name,count in [('api',323),('jnode',89),('pattern',2045)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(name+'-tests.log')))
for name,item in review['namedTests'].items():
 text=(R/name).read_text(encoding='utf-8-sig');assert git('hash-object',name).decode().strip()==item['gitBlob'];assert all(method+'(' in text for method in item['methods'])
render=load(E/'render-receipts.json');assert len(render['pages'])==34 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*entries,'TYPE-COLLECTION-PATTERN-RECONCILIATION.md','CLOSURE-PLAN.md']}
for page in render['pages']:
 p=R/page['source'];text=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[Type/collection/pattern review\]\(([^)]+)\)',text)
 for target in targets:
  if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review']:
 prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
 for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
 blob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
 with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
  historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage');builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
  for name,expectedHash in previous['outputs'].items():
   if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:assert sha(normalized(historical/name))==expectedHash,folder+' historical '+name
   elif name=='GRAMMAR-COVERAGE-MATRIX.json':assert sha(git('show',m['documentationBase']+':docs/xpath40-catalog/'+name).replace(bytes([13,10]),bytes([10])))==expectedHash,'historical grammar matrix'
   else:assert sha(normalized(C/name))==expectedHash,folder+' unchanged '+name
if '--local-artifacts' in sys.argv:
 for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C05 verified: nine rows/49 productions; 2,457 focused passes; 259 matching/16 retained unmet controls; eight packages open. Prior evidence and C04 annotations preserved; no semantic acceptance.')
