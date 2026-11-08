"""Validate C12's exact samples, profile boundaries and preserved grammar requirements."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def norm(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def logtext(p):
    b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def blocks(t):return [v['code'].strip() for v in re.finditer(r'^(?P<f>`{3,}|~{3,})[^\n]*\n(?P<code>.*?)(?:\n)(?P=f)[ \t]*$',t,re.M|re.S)]
def shape(e):return(e.tag,sorted(e.attrib.items()),None if not (e.text or '').strip() else e.text,[(shape(c),None if not (c.tail or '').strip() else c.tail) for c in e])
m=load(E/'manifest.json')
for n,h in m['files'].items():
    assert sha((E/n).read_bytes())==h,n
    if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,'index '+n
for n,h in {**m['tools'],**load(E/'source-files.json')}.items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['priorCheckpoints'].items():assert sha((C/n).read_bytes())==h,n
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={n:sha(norm(C/n)) for n in m['outputs']};assert before==m['outputs']
for n in ['build-c12-review.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/n)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(norm(C/n)) for n in m['outputs']}
plan=load(C/'CLOSURE-PLAN.json');review=load(C/'SEMANTIC-REVIEW-BATCH-4.json');decisions=load(C/'C12-REVIEW-DECISIONS.json');sources=load(E/'entry-sources.json');samples=load(E/'sample-audit.json');cases=load(E/'applicable-cases.json')
assert plan['openClosureWorkpackages']==1 and [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==[f'C{i:02d}' for i in range(1,13)]
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
scope=next(v for v in plan['workpackages'] if v['id']=='C12')['scope']['entries'];assert len(scope)==19 and set(scope)==set(sources)==set(samples)==set(decisions['entries'])=={v['entry'] for v in review['entries']}
assert sum(len(v['contracts']) for v in decisions['entries'].values())==57 and len(cases)==369
q={v['id']:v for family in ['probe','host'] for v in load(E/(family+'-requests.json'))}
observations={family:[json.loads(v) for v in (E/(family+'-results.jsonl')).read_text(encoding='utf-8-sig').splitlines()] for family in ['probe','host']}
assert len(q)==99 and sum(len(v) for v in observations.values())==175
unmet=[]
for family,obs in observations.items():
    profile='profile' if family=='probe' else 'host'
    requests=load(E/(family+'-requests.json'))
    assert {(v['id'],v[profile]) for v in obs}=={(v['id'],p) for v in requests for p in v['profiles']}
    for v in obs:
        r=q[v['id']];ok=r['error'] in ((v['error'] or '')+(v.get('errorCode') or '')) if r['error'] else v['error'] is None and v['result']==r['expected']
        if not ok:unmet.append((v['id'],v[profile]));assert r['kind']=='current-draft-control'
assert set(unmet)=={(n,p) for n in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first'] for p in ['XPath40','XPath40Experimental']}|{('switch-not-boolean-guard',p) for p in ['Legacy','Xslt40Draft']}
assert all(q[n]['expected']=='true' for n in ['choice-coerce-matches-first','choice-argument-matches-first','choice-return-matches-first']) and q['switch-not-boolean-guard']['error']=='XPTY0004'
for family,old,fields in [('probe','c05-collection-pattern-review',['expression','expected','error','profiles']),('host','c06-host-profile-review',['stylesheet','source','expected','error','profiles'])]:
    retained={v['id']:v for v in load(C/'evidence'/old/(('probe' if family=='probe' else 'host')+'-requests.json'))}
    for v in load(E/(family+'-requests.json')):
        assert all(v[field]==retained[v['id']][field] for field in fields),'changed original requirement '+v['id']
byid={v['id']:v for v in load(C/'examples/cases.json')};trx=[v for v in ET.parse(E/'catalog.trx').getroot().iter() if v.tag.endswith('UnitTestResult')]
assert len(trx)==len(byid)==3148 and len({v.get('testId') for v in trx})==3148 and all(v.get('outcome')=='Passed' for v in trx)
for id,item in cases.items():
    assert item['case']==byid[id] and item['passedRows']
    for v in item['passedRows']:
        actual=next(r for r in trx if r.get('testId')==v['testId']);assert actual.get('testName')==v['display'] and actual.get('outcome')==v['outcome']=='Passed'
        match=re.search(r'id: "([^"]+)"',v['display']);display=match.group(1);truncated=v['display'][match.end():].startswith(('\u00b7','\u2026','.'))
        assert v['caseCandidates']==([n for n in byid if n.startswith(display)] if truncated else [display]) and id in v['caseCandidates']
gate=load(E/'gated-sets.json');text=(R/gate['source']).read_text(encoding='utf-8');gates=sorted(re.findall(r'"([^"\n]+)"',text.split('private static readonly HashSet<string> GatedSets',1)[1].split('};',1)[0]));assert gates==gate['sets'] and len(gates)==224
c02=load(C/'FUNCTION-OPTIONS-RECONCILIATION.json')
grammarReviews={n:load(C/n) for n in ['EXPRESSION-CALL-FOCUS-RECONCILIATION.json','TYPE-COLLECTION-PATTERN-RECONCILIATION.json','HOST-PROFILE-RECONCILIATION.json']}
for n,item in sources.items():
    assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==item['gitBlob']
    assert git('show',m['documentationBase']+':docs/xpath40-catalog/'+n).decode('utf-8').replace('\r\n','\n')==item['text']
    current=(C/n).read_text(encoding='utf-8');assert 'C12 contract/evidence review' in current and len(re.findall(r'^## \d+\.',current,re.M))==16
    sample=re.search(r'^## 13\.[^\n]*\n(.*?)(?=^## \d+\.|\Z)',current,re.M|re.S).group(1);assert blocks(sample)==[v['code'] for v in samples[n]['blocks']]
    prior=next((v for v in c02['entries'] if v['entry']==n),None);extra={id for v in prior['evidence'] for field in ['normal','empty','error'] for id in v[field] if id in byid} if prior else set()
    assert set(samples[n]['c02ContractCaseIds'])==extra and set(samples[n]['caseIds'])==set(samples[n]['directCaseIds'])|extra
    row=decisions['entries'][n];assert set(row['gates']['assignedSets'])<=set(gates)
    inherited=[]
    for reportName,grammarReview in grammarReviews.items():
        for v in grammarReview['rows']:
            if n in v['retained']['entries']+v.get('linkedPages',[]):inherited.append(dict(report=reportName,key=v['key'],assessment=v['assessment'],remaining=v['remaining'],namedTests=v['retained']['testEvidence']))
    for contract in row['contracts']:
        assert contract['functionSpecificC02Evidence']==(prior['evidence'] if prior else [])
        assert contract['inheritedGrammarEvidence']==inherited
        assert set(contract['caseIds'])<=set(samples[n]['caseIds'])<=set(cases) and set(contract['probeIds'])<=set(q)
    for v in row['namedTests']:
        assert git('hash-object',v['file']).decode().strip()==v['gitBlob']
        assert all(method+'(' in (R/v['file']).read_text(encoding='utf-8-sig') for method in v['methods'])
for row in samples.values():
    for b in row['blocks']:
        assert any(b.get(k) for k in ['exactFiles','containedInFiles','expectedOutputCases','verbatimProbeIds'])
        code=b['code']
        for f in b['exactFiles']:assert (R/f).read_text(encoding='utf-8-sig').strip()==code
        for f in b['containedInFiles']:assert code in (R/f).read_text(encoding='utf-8-sig').strip()
        for id in b['expectedOutputCases']:assert str(byid[id]['expected']).strip()==code
        for id in b['verbatimProbeIds']:assert q[id]['expression'].strip()==code
assert sum(len(v['blocks']) for v in samples.values())==0
for clarification in load(E/'documentation-corrections.json'):
    assert clarification['before'] in sources[clarification['entry']]['text'] and clarification['after'] in (C/clarification['entry']).read_text(encoding='utf-8')
assets=load(E/'compiled-assets.json');assert len(assets['files'])==264 and sha((C/'examples/cases.json').read_bytes())==assets['manifestSha256']
for n,v in assets['files'].items():assert sha((R/n).read_bytes())==v['sha256']
for n,count in [('catalog',3148),('api',296),('xslt',2151)]:assert re.search(r'Failed:\s+0, Passed:\s+'+str(count)+r', Skipped:\s+0',logtext(E/(n+'-tests.log')))
spec=load(E/'review-sources.json')
for n,h in spec['files'].items():assert sha((C/n).read_bytes())==h
fo=ET.parse(C/'evidence/c01-inventory/current-function-catalog.xml').getroot()
for v in spec['functions']:assert any(n.tag.rsplit('}',1)[-1]=='function' and n.get('name')==v['name'] and n.get('prefix','fn')==v['prefix'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in fo.iter())
grammar=ET.parse(C/'evidence/c03-type-review/current-xpath-grammar.xml').getroot()
for v in spec['productions']:assert any(n.get('name')==v['name'] and n.get('if')==v['condition'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in grammar.iter())
xslt=ET.parse(C/'evidence/c03-type-review/current-xslt.xml').getroot()
for v in spec['xsltSections']:assert any(n.get('id')==v['id'] and ET.tostring(n,encoding='unicode')==v['xml'] for n in xslt.iter())
walk=load(E/'walkthrough-audit.json');assert len(walk)==6 and sum(len(v['blocks']) for v in walk.values())==15
for n,item in walk.items():
    p=R/n;assert sha(p.read_bytes())==item['sha256'] and p.read_text(encoding='utf-8')==item['text']
    assert blocks(item['text'])==[b['code'] for b in item['blocks']]
    for b in item['blocks']:
        assert any(b.get(k) for k in ['exactFiles','containedInFiles','equivalentXmlFiles','commandCaseIds'])
        for f in b['exactFiles']:assert (R/f).read_text(encoding='utf-8-sig').strip()==b['code']
        for f in b['containedInFiles']:assert b['code'] in (R/f).read_text(encoding='utf-8-sig').strip()
        for f in b['equivalentXmlFiles']:assert shape(ET.fromstring(b['code']))==shape(ET.parse(R/f).getroot())
        if b.get('commandCaseIds'):
            assert b['language']=='powershell' and set(b['commandCaseIds'])<=set(cases)
            family=b['code'].split('DisplayName~')[1].split('"')[0]
            assert set(b['commandCaseIds'])=={id for entry in item['entries'] for id in samples[entry]['caseIds'] if family in id}
render=load(E/'render-receipts.json');assert len(render['pages'])==36 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in [*scope,'SEMANTIC-REVIEW-BATCH-4.md','CLOSURE-PLAN.md']}
for page in render['pages']:
    p=R/page['source'];t=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',t) if page['source'] in fullpages else re.findall(r'\[Final semantic/evidence batch\]\(([^)]+)\)',t)
    for target in targets:
        if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review','c05-collection-pattern-review','c06-host-profile-review','c07-lifecycle-review','c08-adoption-research','c09-semantic-batch1','c10-semantic-batch2','c11-semantic-batch3']:
    prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
    for n,v in previous['files'].items():assert sha((prior/n).read_bytes())==(v['sha256'] if isinstance(v,dict) else v),folder+'/'+n
    blob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
    with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
        old=Path(temporary);shutil.copytree(C/'evidence/closure-triage',old/'evidence/closure-triage');builder=old/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
        for n,h in previous['outputs'].items():
            if n in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:got=sha(norm(old/n))
            elif n=='GRAMMAR-COVERAGE-MATRIX.json':
                checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9','c06-host-profile-review':'d00a8fb0','c07-lifecycle-review':'c854e9bb','c08-adoption-research':'34080f04','c09-semantic-batch1':'06b53349','c10-semantic-batch2':'00ee9bf','c11-semantic-batch3':'22caf4ed'}[folder];got=sha(git('show',checkpoint+':docs/xpath40-catalog/'+n).replace(b'\r\n',b'\n'))
            else:got=sha(norm(C/n))
            assert got==h,folder+' checkpoint '+n
if '--local-artifacts' in sys.argv:
    assert sha((R/assets['compiledManifest']).read_bytes())==assets['manifestSha256']
    for v in assets['files'].values():assert sha((R/v['compiledAsset']).read_bytes())==v['sha256']
    for v in m['localBinaryIdentities']:assert sha(Path(v['path']).read_bytes())==v['sha256']
    for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256']
print('C12 verified:19 entries/57 groups;369 cases;6 walkthroughs/15 blocks;5,595 fresh tests;167 matching/8 known unmet controls. C13 remains open; C00-C11 preserved.')
