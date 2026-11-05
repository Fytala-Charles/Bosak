"""Validate C06 controls and preserve the immutable C00-C05 review checkpoints."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET

E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def normalized(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
def logtext(p):
    b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
m=load(E/'manifest.json')
for name,expected in m['files'].items():
    assert sha((E/name).read_bytes())==expected,name
    if '--staged' in sys.argv:assert sha(git('show',':'+(E/name).relative_to(R).as_posix()))==expected,'index '+name
for name,expected in {**m['sourceFiles'],**m['tools']}.items():assert git('hash-object',name).decode().strip()==expected,name
for name,expected in m['specFiles'].items():assert sha((C/name).read_bytes())==expected,name
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={n:sha(normalized(C/n)) for n in m['outputs']};assert before==m['outputs']
for n in ['build-c06-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/n)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(normalized(C/n)) for n in m['outputs']},'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');report=load(C/'HOST-PROFILE-RECONCILIATION.json');decisions=load(C/'C06-REVIEW-DECISIONS.json')
assert plan['openClosureWorkpackages']==7
assert [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02','C03','C04','C05','C06']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C06')['scope']['grammarRows']
assert set(scope)==set(decisions['rows'])=={v['key'] for v in report['rows']}=={'S0','S7','S8-dispatch'}
for lane in ['host','registration']:
    requests=load(E/(lane+'-requests.json'));obs=[json.loads(v) for v in (E/(lane+'-results.jsonl')).read_text(encoding='utf-8-sig').splitlines()]
    if lane=='host':
        assert {(v['id'],v['host']) for v in obs}=={(q['id'],p) for q in requests for p in q['profiles']}
    else:assert {(v['id'],v['profile']) for v in obs}=={(q['id'],q['profile']) for q in requests}
    controls=[];diagnostics=[];unmet=[]
    for v in obs:
        q=next(q for q in requests if q['id']==v['id'] and (lane=='host' or q['profile']==v['profile']))
        if q.get('kind')=='context-observation':diagnostics.append(v);continue
        passed=q['error'] in (v['error'] or '') if q['error'] else v['error'] is None and v['result']==q['expected']
        item=dict(**v,passed=passed);controls.append(item)
        if not passed:unmet.append(item)
    assert report['observations'][lane]==dict(requestCount=len(requests),observationCount=len(obs),controls=controls,diagnostics=diagnostics,unmet=unmet)
host=report['observations']['host'];registration=report['observations']['registration']
assert {(v['id'],v['host']) for v in host['unmet']}=={('switch-not-boolean-guard',p) for p in ['Legacy','Xslt40Draft']}
assert not registration['unmet']
assert all(v['phase']=='evaluate' and 'XPTY0004' in v['error'] for v in host['controls'] if v['id']=='switch-direct-comparison')
assert all(v['error'] is None and v['result']=='12' for v in host['controls'] if v['id']=='stylesheet-loader-invocation')
assert all(v['phase']=='compile' and 'FOQM0005' in v['error'] for v in host['diagnostics'] if v['id']=='stylesheet-loader-external-coercion')
s=report['summary'];assert (s['totalObservations'],s['assertedControls'],s['matchingControls'],s['unmetControls'],s['contextObservations'],s['freshTestTotal'])==(154,135,133,2,19,3524)
sources=load(E/'production-sources.json');xslt=ET.parse(C/'evidence/c03-type-review/current-xslt.xml').getroot()
for key,row in sources.items():
    assert row['retained']['key']==key and row['forms']==decisions['rows'][key]['forms']
    for v in row['sections']:assert any(n.get('id')==v['id'] and n.get('if')==v['condition'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in xslt.iter())
fo=ET.parse(C/'evidence/c01-inventory/current-function-catalog.xml').getroot()
for v in load(E/'host-function-source-fragments.json'):assert any(n.tag.rsplit('}',1)[-1]=='function' and n.get('name')==v['name'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in fo.iter())
matrix=load(C/'GRAMMAR-COVERAGE-MATRIX.json');original=json.loads(git('show',m['documentationBase']+':docs/xpath40-catalog/GRAMMAR-COVERAGE-MATRIX.json'));oldrows={v['key']:v for v in original['rows']}
assert len(matrix['rows'])==18 and matrix['currentReview']['reviewedRowCount']==18 and matrix['currentReview']['otherRows']==0
assert set(matrix['currentReview']['reviewedRows'])=={v['key'] for v in matrix['rows']}
for row in matrix['rows']:
    old=oldrows[row['key']];a=dict(row);b=dict(old);a.pop('currentReview',None);b.pop('currentReview',None);assert a==b
    if 'currentReview' in old:assert row['currentReview']==old['currentReview'],'preserve earlier review'
    else:assert row['currentReview']['status']=='C06 bounded review complete; not accepted'
entries=load(E/'entry-sources.json');assert len(entries)==9
for name,item in entries.items():
    p=C/name;assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+name).decode().strip()==item['gitBlob']
    assert sha(git('show',m['documentationBase']+':docs/xpath40-catalog/'+name).replace(b'\r\n',b'\n'))==sha(item['text'].encode().replace(b'\r\n',b'\n'))
    text=p.read_text(encoding='utf-8');assert ('C06 host/profile review' if name!='README.md' else 'REQ-126 C06 complete') in text
    if '## 12. Implementation' in text:assert len(re.findall(r'^## \d+\.',text,re.M))==16
for name,count in [('host',276),('registration',94),('module',6),('catalog',3148)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(name+'-tests.log')))
for name,item in report['namedTests'].items():
    assert git('hash-object',name).decode().strip()==item['gitBlob']
    assert all(method+'(' in (R/name).read_text(encoding='utf-8-sig') for method in item['methods'])
render=load(E/'render-receipts.json');assert len(render['pages'])==19 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*entries,'HOST-PROFILE-RECONCILIATION.md','CLOSURE-PLAN.md']}
for page in render['pages']:
    p=R/page['source'];text=p.read_text(encoding='utf-8')
    targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[Host/profile review\]\(([^)]+)\)',text)
    for target in targets:
        if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review','c05-collection-pattern-review']:
    prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
    for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
    builderBlob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
    with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
        historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage');builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',builderBlob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
        for name,expected in previous['outputs'].items():
            if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:actual=sha(normalized(historical/name))
            elif name=='GRAMMAR-COVERAGE-MATRIX.json':
                checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9'}[folder]
                actual=sha(git('show',checkpoint+':docs/xpath40-catalog/'+name).replace(b'\r\n',b'\n'))
            else:actual=sha(normalized(C/name))
            assert actual==expected,folder+' checkpoint '+name
if '--local-artifacts' in sys.argv:
    for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
    for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C06 verified: three rows; 3,524 fresh test passes; 133 matching/two unmet controls; 19 context observations; seven packages open. C00-C05 raw evidence and historical plans preserved.')
