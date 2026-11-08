"""Validate the bounded C07 lifecycle ledger and immutable prior checkpoints."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET

E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def normalized(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def section(t,n):
    return re.search(r'^## '+str(n)+r'\..*?\n(.*?)(?=^## \d+\.|\Z)',t,re.M|re.S).group(1).strip()
def logtext(p):
    b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
m=load(E/'manifest.json')
for n,expected in m['files'].items():
    assert sha((E/n).read_bytes())==expected,n
    if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==expected,'index '+n
for n,expected in {**m['sourceFiles'],**m['tools']}.items():assert git('hash-object',n).decode().strip()==expected,n
for n,expected in m['priorCheckpoints'].items():assert sha((C/n).read_bytes())==expected,n
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={n:sha(normalized(C/n)) for n in m['outputs']};assert before==m['outputs']
for n in ['build-c07-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/n)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(normalized(C/n)) for n in m['outputs']},'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'LIFECYCLE-RECONCILIATION.json');decisions=load(C/'C07-REVIEW-DECISIONS.json');sources=load(E/'entry-sources.json')
assert plan['openClosureWorkpackages']==6
assert [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01','C02','C03','C04','C05','C06','C07']
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C07')['scope']['entries']
assert len(scope)==len(sources)==len(review['entries'])==76 and set(scope)==set(sources)==set(decisions['entries'])=={v['entry'] for v in review['entries']}
assert len(decisions['originalGroups'])==19 and {v['id'] for v in decisions['followups']}=={'L01','L02','L03'}
cases=load(C/'examples/cases.json');ids={v['id'] for v in cases}
assert all(re.fullmatch(r'[A-Za-z0-9_-]+',v) for v in ids)
casepaths={str((C/'examples'/v['file']).resolve()):v['id'] for v in cases}
for row in review['entries']:
    n=row['entry'];snapshot=sources[n];p=C/n;text=p.read_text(encoding='utf-8')
    baseline=git('show',m['documentationBase']+':docs/xpath40-catalog/'+n).decode('utf-8').replace('\r\n','\n')
    assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==snapshot['gitBlob']
    for number,field in [(1,'classification'),(4,'history'),(12,'implementation'),(15,'knownUses')]:assert section(baseline,number)==snapshot[field],n+' '+field
    assert section(text,1)==row['currentClassificationText'] and row['previousClassificationText']==snapshot['classification']
    assert len(re.findall(r'^## \d+\.',text,re.M))==16,n
    assert row['manifestIds'] and set(row['manifestIds'])<=ids,n
    exact=set(re.findall(r'[A-Za-z0-9_-]+',baseline))&ids
    for target in re.findall(r'\]\(([^)]+)\)',baseline):
        if not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target) and not target.startswith('#'):
            identifier=casepaths.get(str((p.parent/target.split('#')[0]).resolve()))
            if identifier:exact.add(identifier)
    assert set(row['manifestIds'])==exact,n+' exact case mapping'
    assert row['acceptance']=='Pending; lifecycle review only'
    for f in row['codeFiles']:assert (R/f).is_file() and f in m['sourceFiles'],f
assert review['summary']['linkedManifestCases']==len({v for row in review['entries'] for v in row['manifestIds']})==819
initial=load(E/'initial-entry-sources.json');correction=load(E/'mapping-corrections.json')
assert initial[correction['entry']]['manifestIds']==correction['before']
initial[correction['entry']]['manifestIds']=correction['after'];assert initial==sources
for n,item in load(E/'original-dossier-sources.json').items():
    assert git('rev-parse',m['documentationBase']+':'+n).decode().strip()==item['gitBlob']
    assert git('show',m['documentationBase']+':'+n).decode('utf-8').replace('\r\n','\n')==item['text']
commits=load(E/'commit-evidence.json');assert len(commits)==37
for requested,item in commits.items():
    resolved=item['correction'] or requested
    assert git('rev-parse',resolved+'^{commit}').decode().strip()==item['commit']
    ancestor=subprocess.run(['git','merge-base','--is-ancestor',item['commit'],m['documentationBase']],cwd=R,capture_output=True).returncode
    assert (ancestor==0)==item['ancestorOfDocumentationBase']
    assert git('show','-s','--format=%s',item['commit']).decode().strip()==item['subject']
    assert git('diff-tree','--no-commit-id','--name-only','-r',item['commit']).decode().strip().splitlines()==item['changedPaths']
assert sum(v['ancestorOfDocumentationBase'] for v in commits.values())==33
corrections=load(E/'documentation-corrections.json')['corrections'];assert len(corrections)==6
for v in corrections:
    assert v['after'] in (C/v['entry']).read_text(encoding='utf-8') and v['before'] not in (C/v['entry']).read_text(encoding='utf-8')
    assert v['before'] in git('show',m['documentationBase']+':docs/xpath40-catalog/'+v['entry']).decode('utf-8')
q={v['id']:v for v in load(E/'probe-requests.json')};obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
assert len(q)==36 and len(obs)==113
assert {(v['id'],v['profile']) for v in obs}=={(v['id'],p) for v in q.values() for p in v['profiles']}
for v in obs:
    req=q[v['id']];passed=req['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if req['error'] else v['error'] is None and v['result']==req['expected'];assert passed,(v['id'],v['profile'])
assert all(v['passed'] for v in review['controls']) and len(review['controls'])==113
for n,count in [('api',296),('host',75),('catalog',3148)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(n+'-tests.log')))
for n,item in load(E/'named-test-evidence.json').items():
    assert git('hash-object',n).decode().strip()==item['gitBlob']
    assert all(method+'(' in (R/n).read_text(encoding='utf-8-sig') for method in item['methods'])
assert review['summary']['freshTests']['total']==3519 and review['summary']['missingAdoptionPins']==28
assert len(load(C/'evidence/closure-triage/catalog-inventory.json')['missingAdoptedSpecPins'])==28
matrix=load(C/'GRAMMAR-COVERAGE-MATRIX.json');assert matrix['currentReview']['reviewedRowCount']==18
assert normalized(C/'GRAMMAR-COVERAGE-MATRIX.json')==git('show',m['documentationBase']+':docs/xpath40-catalog/GRAMMAR-COVERAGE-MATRIX.json').replace(b'\r\n',b'\n')
assert 'C01-C07 complete and 6 open' in (R/'docs/FEATURE_REQUESTS.md').read_text(encoding='utf-8')
render=load(E/'render-receipts.json');assert len(render['pages'])==90 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*sources,'LIFECYCLE-RECONCILIATION.md','CLOSURE-PLAN.md']}
for page in render['pages']:
    p=R/page['source'];text=p.read_text(encoding='utf-8')
    targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[Lifecycle/disposition review\]\(([^)]+)\)',text)
    for target in targets:
        if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review','c05-collection-pattern-review','c06-host-profile-review']:
    prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
    for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
    builderBlob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
    with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
        historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage');builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',builderBlob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
        for name,expected in previous['outputs'].items():
            if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:actual=sha(normalized(historical/name))
            elif name=='GRAMMAR-COVERAGE-MATRIX.json':
                checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9','c06-host-profile-review':'d00a8fb0'}[folder]
                actual=sha(git('show',checkpoint+':docs/xpath40-catalog/'+name).replace(b'\r\n',b'\n'))
            else:actual=sha(normalized(C/name))
            assert actual==expected,folder+' checkpoint '+name
if '--local-artifacts' in sys.argv:
    for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
    for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C07 verified: 76 classifications/19 groups; 3,519 fresh tests and 113 profile controls pass; six packages open. C00-C06 evidence, historical URLs and 28 adoption gaps preserved.')
