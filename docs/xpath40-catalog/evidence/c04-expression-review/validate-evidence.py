"""Verify C04 mappings, retained unmet expectations and earlier immutable checkpoints."""
from pathlib import Path
import collections,hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET
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
for name in ['build-c04-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/name)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert {name:sha(normalized(C/name)) for name in m['outputs']}==before,'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'EXPRESSION-CALL-FOCUS-RECONCILIATION.json');decisions=load(C/'C04-REVIEW-DECISIONS.json')
assert plan['openClosureWorkpackages']==9 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02','C03','C04']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C04')['scope']['grammarRows']
assert set(scope)==set(decisions['rows'])=={v['key'] for v in review['rows']} and len(scope)==6
requests=load(E/'probe-requests.json');obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()];byid={v['id']:v for v in requests}
assert len(requests)==len(byid)==108 and len(obs)==227
assert {(v['id'],v['profile']) for v in obs}=={(v['id'],p) for v in requests for p in v['profiles']}
controls=[];unmet=[]
for v in obs:
 q=byid[v['id']]
 if q['kind']=='context-observation':continue
 passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and q['expected']==v['result']
 if q['requiredPhase']:passed=passed and v['phase']==q['requiredPhase']
 controls.append((q['id'],v['profile'],passed))
 if not passed:unmet.append((q['id'],v['profile']))
expected={(name,p) for name in ['otherwise-current-spelling','otherwise-current-guard','keyword-partial','mapping-containing-focus','mapping-containing-position'] for p in ['XPath40','XPath40Experimental']}
assert len(controls)==219 and sum(v[2] for v in controls)==209 and set(unmet)==expected
assert all(byid[name]['kind']=='current-draft-control' for name,_ in unmet)
assert len([v for v in obs if byid[v['id']]['kind']=='context-observation'])==8
assert {v['result'] for v in obs if v['id']=='mapping-containing-focus'}=={'1:1|2:2'}
assert byid['mapping-containing-focus']['expected']=='1:100|2:100'
assert {v['result'] for v in obs if v['id']=='mapping-for-focus'}=={'1:100|2:100'}
assert byid['keyword-partial']['expected']=='BC' and all(v['result']=='BC' for v in obs if v['id']=='keyword-positional-placeholder-alternative')
assert all(v['result']=='7' for v in obs if v['id'] in ['context-supplied-variable','context-supplied-function'])
assert dict(collections.Counter(v['phase'] for v in obs))==review['summary']['phaseCounts']
sources=load(E/'production-sources.json');grammar=ET.parse(C/'evidence/c03-type-review/current-xpath-grammar.xml').getroot()
actual={(v.tag.rsplit('}',1)[-1],v.get('name'),v.get('if'),ET.tostring(v,encoding='unicode')) for v in grammar.iter() if v.tag.rsplit('}',1)[-1] in ['production','token']}
for key,row in sources.items():
 assert row['grammar'] and row['retained']['key']==key
 for fragment in row['grammar']:assert (fragment['kind'],fragment['name'],fragment['condition'],fragment['xml']) in actual
assert len({v['name'] for row in sources.values() for v in row['grammar']})==33
matrix=load(C/'GRAMMAR-COVERAGE-MATRIX.json');original=json.loads(git('show',m['documentationBase']+':docs/xpath40-catalog/GRAMMAR-COVERAGE-MATRIX.json'))
assert len(matrix['rows'])==18 and {v['key'] for v in matrix['rows'] if 'currentReview' in v}==set(scope)
stripped=json.loads(json.dumps(matrix));stripped.pop('currentReview')
for row in stripped['rows']:row.pop('currentReview',None)
assert stripped==original,'original grammar baseline preserved'
entries=load(E/'entry-sources.json');assert len(entries)==11
structured=[]
for name,item in entries.items():
 p=C/name;text=p.read_text(encoding='utf-8');assert 'C04 grammar review' in text
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+name).decode().strip()==item['gitBlob']
 if '## 12. Implementation' in text:assert len(re.findall(r'^## \d+\.',text,re.M))==16;structured.append(name)
assert len(structured)==9
for name,count in [('api',289),('parser',202),('host-catalog',389)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(name+'-tests.log')))
for name,item in review['namedTests'].items():
 text=(R/name).read_text(encoding='utf-8-sig');assert git('hash-object',name).decode().strip()==item['gitBlob']
 assert all(method+'(' in text for method in item['methods'])
render=load(E/'render-receipts.json');assert len(render['pages'])==24 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*entries,'EXPRESSION-CALL-FOCUS-RECONCILIATION.md','CLOSURE-PLAN.md']}
for page in render['pages']:
 p=R/page['source'];text=p.read_text(encoding='utf-8')
 targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[Expression/call/focus review\]\(([^)]+)\)',text)
 for target in targets:
  if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
encoding=load(E/'documentation-encoding-correction.json');assert encoding['beforeGitBlob']==git('rev-parse',m['documentationBase']+':'+encoding['file']).decode().strip()
assert '\u00e2\u20ac\u201d' not in (R/encoding['file']).read_text(encoding='utf-8')
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review']:
 prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
 for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
 blob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
 with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
  historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage')
  builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
  for name,expectedHash in previous['outputs'].items():
   if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:assert sha(normalized(historical/name))==expectedHash,folder+' historical '+name
   else:assert sha(normalized(C/name))==expectedHash,folder+' unchanged '+name
if '--local-artifacts' in sys.argv:
 for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C04 verified: six rows/33 productions; 880 focused passes; 209 matching controls/10 retained unmet controls/8 context observations; nine packages open. Earlier evidence preserved; no semantic acceptance.')
