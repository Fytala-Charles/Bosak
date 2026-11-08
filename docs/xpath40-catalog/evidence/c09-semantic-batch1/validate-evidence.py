"""Validate C09 contracts, verbatim samples, actual execution and preserved checkpoints."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET

E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def norm(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def logtext(p):
    b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def blocks(text):return [m['code'].strip() for m in re.finditer(r'^(?P<f>`{3,}|~{3,})[^\n]*\n(?P<code>.*?)(?:\n)(?P=f)[ \t]*$',text,re.M|re.S)]
def shape(e):return(e.tag,sorted(e.attrib.items()),None if not (e.text or '').strip() else e.text,[(shape(c),None if not (c.tail or '').strip() else c.tail) for c in e])
m=load(E/'manifest.json')
for n,expected in m['files'].items():
    assert sha((E/n).read_bytes())==expected,n
    if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==expected,'index '+n
for n,expected in {**m['tools'],**load(E/'source-files.json')}.items():assert git('hash-object',n).decode().strip()==expected,n
for n,expected in m['priorCheckpoints'].items():assert sha((C/n).read_bytes())==expected,n
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={n:sha(norm(C/n)) for n in m['outputs']};assert before==m['outputs']
for n in ['build-c09-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/n)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(norm(C/n)) for n in m['outputs']},'deterministic rebuild'
plan=load(C/'CLOSURE-PLAN.json');report=load(C/'SEMANTIC-REVIEW-BATCH-1.json');decisions=load(C/'C09-REVIEW-DECISIONS.json');sources=load(E/'entry-sources.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
assert plan['openClosureWorkpackages']==4 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==[f'C{i:02d}' for i in range(1,10)]
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C09')['scope']['entries'];assert len(scope)==19 and set(scope)==set(sources)==set(samples)==set(decisions['entries'])=={v['entry'] for v in report['entries']}
assert sum(len(v['contracts']) for v in decisions['entries'].values())==67 and len(cases)==536
q={v['id']:v for v in load(E/'probe-requests.json')};obs=[json.loads(v) for v in (E/'probe-results.jsonl').read_text(encoding='utf-8-sig').splitlines()]
assert len(q)==28 and len(obs)==54 and {(v['id'],v['profile']) for v in obs}=={(v['id'],p) for v in q.values() for p in v['profiles']}
unmet=[]
for v in obs:
    req=q[v['id']];passed=req['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if req['error'] else v['error'] is None and v['result']==req['expected']
    if not passed:unmet.append((v['id'],v['profile']));assert req['kind']=='current-draft-control'
assert set(unmet)=={(id,p) for id in ['jnode-generic-required','extrema-required-mixed','extrema-identity-required','partial-nonpositive-draft'] for p in ['XPath40','XPath40Experimental']}
assert q['extrema-required-mixed']['expected']=='0' and q['extrema-identity-required']['expected']=='true'
assert q['partial-nonpositive-draft']['expected']=='2' and q['csv-invalid-separator']['error']=='FOCV0002'
manifest=load(C/'examples/cases.json');byid={v['id']:v for v in manifest};trx=[v for v in ET.parse(E/'catalog.trx').getroot().iter() if v.tag.endswith('UnitTestResult')]
assert len(trx)==len(manifest)==3148 and len({v.get('testId') for v in trx})==3148 and all(v.get('outcome')=='Passed' for v in trx)
for id,item in cases.items():
    assert item['case']==byid[id] and item['passedRows']
    for v in item['passedRows']:
        actual=next(r for r in trx if r.get('testId')==v['testId']);assert actual.get('testName')==v['display'] and actual.get('outcome')==v['outcome']=='Passed'
        display=re.search(r'id: "([^"]+)"',v['display']).group(1)
        assert v['caseCandidates']==([display] if display in byid else [n for n in byid if n.startswith(display)]) and id in v['caseCandidates']
gate=load(E/'gated-sets.json');text=(R/gate['source']).read_text(encoding='utf-8');actual=sorted(re.findall(r'"([^"\n]+)"',text.split('private static readonly HashSet<string> GatedSets',1)[1].split('};',1)[0]));assert actual==gate['sets'] and len(actual)==224
for n,item in sources.items():
    assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==item['gitBlob']
    assert git('show',m['documentationBase']+':docs/xpath40-catalog/'+n).decode('utf-8').replace('\r\n','\n')==item['text']
    current=(C/n).read_text(encoding='utf-8');assert 'C09 contract/evidence review' in current and len(re.findall(r'^## \d+\.',current,re.M))==16
    sample=re.search(r'^## 13\.[^\n]*\n(.*?)(?=^## \d+\.|\Z)',current,re.M|re.S).group(1)
    assert blocks(sample)==[v['code'] for v in samples[n]['blocks']]
    row=decisions['entries'][n];assert set(row['gates']['assignedSets'])<=set(actual)
    for contract in row['contracts']:
        assert contract['caseIds'] or contract['probeIds'] or contract['inheritedEvidence']
        assert set(contract['caseIds'])<=set(samples[n]['caseIds'])<=set(cases) and set(contract['probeIds'])<=set(q)
    for item in row['namedTests']:
        assert git('hash-object',item['file']).decode().strip()==item['gitBlob']
        assert all(method+'(' in (R/item['file']).read_text(encoding='utf-8-sig') for method in item['methods'])
walk=load(E/'sample-walkthrough.json');assert git('hash-object',walk['source']).decode().strip()==walk['gitBlob']
assert blocks(walk['text'])==[v['code'] for v in walk['blocks']]
for row in [*samples.values(),dict(blocks=walk['blocks'])]:
    for block in row['blocks']:
        assert any(block.get(k) for k in ['exactFiles','containedInFiles','expectedOutputCases','verbatimProbeIds','equivalentEmbeddedXmlInputs'])
        code=block['code']
        for f in block['exactFiles']:assert (R/f).read_text(encoding='utf-8-sig').strip()==code
        for f in block['containedInFiles']:assert code in (R/f).read_text(encoding='utf-8-sig').strip()
        for id in block['expectedOutputCases']:assert str(byid[id]['expected']).strip()==code
        for id in block['verbatimProbeIds']:assert q[id]['expression'].strip()==code
        for f in block.get('equivalentEmbeddedXmlInputs',[]):
            literals=re.findall(r"parse-xml\(\s*'((?:[^']|'')*)'\s*\)",(R/f).read_text(encoding='utf-8-sig'))
            assert any(shape(ET.fromstring(v.replace("''","'")))==shape(ET.fromstring(code)) for v in literals)
assert sum(len(v['blocks']) for v in samples.values())==116 and len(walk['blocks'])==8
correction=load(E/'documentation-corrections.json');assert correction['before'] in sources[correction['entry']]['text'] and correction['after'] in (C/correction['entry']).read_text(encoding='utf-8')
assets=load(E/'compiled-assets.json');assert len(assets['files'])==555 and sha((C/'examples/cases.json').read_bytes())==assets['manifestSha256']
for n,v in assets['files'].items():assert sha((R/n).read_bytes())==v['sha256']
for n,count in [('catalog',3148),('standard',182),('host',117)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(n+'-tests.log')))
spec=load(E/'review-sources.json')
for n,h in spec['files'].items():assert sha((C/n).read_bytes())==h
fo=ET.parse(C/'evidence/c01-inventory/current-function-catalog.xml').getroot();xslt=ET.parse(C/'evidence/c03-type-review/current-xslt.xml').getroot()
for v in spec['functions']:assert any(n.tag.rsplit('}',1)[-1]=='function' and n.get('name')==v['name'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in fo.iter())
for v in spec['xsltSections']:assert any(n.get('id')==v['id'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in xslt.iter())
render=load(E/'render-receipts.json');assert len(render['pages'])==31 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*scope,'SEMANTIC-REVIEW-BATCH-1.md','CLOSURE-PLAN.md','examples/equality-contracts/README.md']}
for page in render['pages']:
    p=R/page['source'];text=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[First semantic/evidence batch\]\(([^)]+)\)',text)
    for target in targets:
        if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review','c05-collection-pattern-review','c06-host-profile-review','c07-lifecycle-review','c08-adoption-research']:
    prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
    for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
    builderBlob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
    with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
        historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage');builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',builderBlob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
        for name,expected in previous['outputs'].items():
            if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:actualHash=sha(norm(historical/name))
            elif name=='GRAMMAR-COVERAGE-MATRIX.json':
                checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9','c06-host-profile-review':'d00a8fb0','c07-lifecycle-review':'c854e9bb','c08-adoption-research':'34080f04'}[folder]
                actualHash=sha(git('show',checkpoint+':docs/xpath40-catalog/'+name).replace(b'\r\n',b'\n'))
            else:actualHash=sha(norm(C/name))
            assert actualHash==expected,folder+' checkpoint '+name
if '--local-artifacts' in sys.argv:
    assert sha((R/assets['compiledManifest']).read_bytes())==assets['manifestSha256']
    for n,v in assets['files'].items():assert sha((R/v['compiledAsset']).read_bytes())==v['sha256']
    for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256']
    for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256']
print('C09 verified:19 entries/67 contract groups;536 scoped cases;124 sample/walkthrough blocks;3,447 fresh test passes;46 matching/eight known unmet controls. Four packages open; C00-C08 preserved.')
