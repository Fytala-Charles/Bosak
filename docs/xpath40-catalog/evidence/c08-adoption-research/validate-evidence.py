"""Validate C08 historical research and the explicitly approved, bounded legacy policy."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,tempfile

E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def normalized(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json')
for n,expected in m['files'].items():
    assert sha((E/n).read_bytes())==expected,n
    if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==expected,'index '+n
for n,expected in m['tools'].items():assert git('hash-object',n).decode().strip()==expected,n
for n,expected in m['priorCheckpoints'].items():assert sha((C/n).read_bytes())==expected,n
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
context=load(E/'research-context.json');ledger=load(C/'provenance.json');inventory=load(C/'evidence/closure-triage/catalog-inventory.json')
assert sha((C/'provenance.json').read_bytes())==context['originalLedgerSha256']
assert git('hash-object',str(C/'provenance.json')).decode().strip()==context['originalLedgerGitBlob']
assert sha((C/'evidence/closure-triage/catalog-inventory.json').read_bytes())==context['originalInventorySha256']
assert len(ledger['entries'])==28 and all(v['adoptedSpecRevision'] is None for v in ledger['entries'])
assert len(inventory['missingAdoptedSpecPins'])==28
approval=load(E/'owner-approval.json');assert approval['decision']=='Approved' and approval['answer']=='Approve the scoped legacy policy'
assert approval['policyAsPresentedSha256']==sha((E/'policy-as-presented.md').read_bytes())
assert 'proposal only; approval pending' in (E/'policy-as-presented.md').read_text(encoding='utf-8')
assert 'explicitly approved by the owner' in (C/'LEGACY-ADOPTION-POLICY-PROPOSAL.md').read_text(encoding='utf-8')
sources=load(E/'entry-sources.json');assert len(sources)==28
assert set(approval['exactScope'])==set(sources)=={v['entry'] for v in inventory['missingAdoptedSpecPins']}
for n,item in sources.items():
    assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==item['gitBlob']
    assert item['originalRecord']==next(v for v in inventory['missingAdoptedSpecPins'] if v['entry']==n)
prs={v['number']:v for v in [load(p) for p in E.glob('pr-*.json')]};assert len(prs)==16
assert {v['bosakPr'] for v in inventory['missingAdoptedSpecPins']}==set(prs)
for v in inventory['missingAdoptedSpecPins']:assert prs[v['bosakPr']]['mergeCommit']['oid']==v['bosakRevision']
assert sum(len(v['comments'])+len(v['reviews']) for v in prs.values())==0
pattern='qtspecs|qt4cg.org/specifications|adoptedSpec|[Ss]pec.{0,40}([Rr]evision|[Ss]napshot|[Pp]in)'
deliveries=load(E/'delivery-research.json')['records'];heads=load(E/'head-research.json');assert len(deliveries)==len(heads)==16
for v in deliveries:
    rev=v['bosakRevision']
    assert (E/v['commitReceipt']).read_bytes()==git('show','-s','--format=%H%n%aI%n%cI%n%B',rev)
    assert (E/v['fixtureTreeReceipt']).read_bytes()==git('ls-tree',rev,'tests/qt4tests','tests/qt3tests')
    result=subprocess.run(['git','grep','-n','-I','-E',pattern,rev,'--','docs','src','tests'],cwd=R,capture_output=True)
    assert result.returncode==v['sourceSearchExitCode'] and result.stdout==(E/v['sourceSearch']).read_bytes()
for v in heads:
    rev=v['headRevision'];assert v['objectRetained'] and rev==prs[v['pr']]['headRefOid']
    assert (E/f"pr-{v['pr']}-head-commit.txt").read_bytes()==git('show','-s','--format=%H%n%aI%n%cI%n%B',rev)
    result=subprocess.run(['git','grep','-n','-I','-E',pattern,rev,'--','docs','src','tests'],cwd=R,capture_output=True)
    assert result.returncode==v['sourceSearchExitCode'] and result.stdout==(E/f"pr-{v['pr']}-head-source-search.txt").read_bytes()
expanded=load(E/'expanded-source-search.json');assert len(expanded['records'])==32
for v in expanded['records']:
    result=subprocess.run(['git','grep','-n','-I','-E',expanded['pattern'],v['revision'],'--',*expanded['scope']],cwd=R,capture_output=True)
    assert result.returncode==v['exitCode'] and result.stdout.decode('utf-8')==v['output']
history=load(E/'historical-change-receipts.json');assert len(history)==57
command=load(E/'history-search-command.json');result=subprocess.run(command['arguments'],cwd=R,capture_output=True)
assert result.returncode==0 and result.stdout==(E/'preledger-all-head-history.txt').read_bytes()
assert [v.split(' ')[0] for v in result.stdout.decode('utf-8').splitlines()]==[v['revision'] for v in history]
historicalPattern=re.compile(expanded['pattern'])
candidate=re.compile(r'qtspecs/(?:blob|commit)/[0-9a-f]{8,40}|adoptedSpecRevision|[Ss]pec.{0,40}(?:[Rr]evision|[Ss]napshot|[Cc]ommit|[Hh]ash).{0,20}\b[0-9a-f]{40}\b')
for v in history:
    raw=git('show','--format=','--unified=0',v['revision'],'--','docs','src','tests')
    assert sha(raw)==v['diffSha256']
    assert [line for line in raw.decode('utf-8').splitlines() if line.startswith(('+','-')) and historicalPattern.search(line)]==v['matchingChangedLines']
    assert not any(candidate.search(line) for line in raw.decode('utf-8').splitlines() if line.startswith(('+','-')))
alltrees=load(E/'all-tracked-source-search.json');assert len(alltrees['records'])==32
for v in alltrees['records']:
    result=subprocess.run(['git','grep','-n','-I','-E',alltrees['pattern'],v['revision']],cwd=R,capture_output=True)
    assert result.returncode==v['exitCode'] and result.stdout.decode('utf-8')==v['output']
    assert not any(candidate.search(line.split(':',3)[-1]) for line in v['output'].splitlines())
allhistory=load(E/'all-tracked-history-receipts.json');assert len(allhistory['records'])==58
result=subprocess.run(allhistory['arguments'],cwd=R,capture_output=True)
assert result.returncode==0 and result.stdout.decode('utf-8')==allhistory['logOutput']
assert [line.split(' ')[0] for line in allhistory['logOutput'].splitlines()]==[v['revision'] for v in allhistory['records']]
for v in allhistory['records']:
    raw=git('show','--format=','--unified=0',v['revision']);assert sha(raw)==v['diffSha256']
    lines=[line for line in raw.decode('utf-8').splitlines() if line.startswith(('+','-'))]
    assert [line for line in lines if re.search(alltrees['pattern'],line)]==v['matchingChangedLines']
    assert not any(candidate.search(line) for line in lines)
before={n:sha(normalized(C/n)) for n in m['outputs']};assert before==m['outputs']
for n in ['build-c08-research.py','build-closure-plan.py']:subprocess.run([sys.executable,str(C/n)],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(normalized(C/n)) for n in m['outputs']},'deterministic rebuild'
report=load(C/'ADOPTION-HISTORY-RESEARCH.json');dispositions=load(C/'ADOPTION-DISPOSITIONS.json');plan=load(C/'CLOSURE-PLAN.json')
assert report['summary']['recoveredAdoptionPins']==0 and report['summary']['ownerDispositionsApplied']==28
assert report['ownerDecision']==approval and report['summary']['legacyExceptions']==26 and report['summary']['nonAdoptions']==2
assert len(dispositions['entries'])==28 and all(v['adoptedSpecRevision'] is None for v in dispositions['entries'])
nonadoptions={v['entry'] for v in dispositions['entries'] if v['disposition'].startswith('Non-adoption;')}
assert nonadoptions=={'operators-and-syntax/method-call-operator.md','rejected/mapping-arrow-gtgt.md'}
assert plan['openClosureWorkpackages']==5
assert [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==[f'C{i:02d}' for i in range(1,9)]
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
assert 'C01-C08 complete and 5 open' in (R/'docs/FEATURE_REQUESTS.md').read_text(encoding='utf-8')
render=load(E/'render-receipts.json');assert len(render['pages'])==13 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
fullpages={'docs/xpath40-catalog/'+n for n in ['ADOPTION-HISTORY-RESEARCH.md','LEGACY-ADOPTION-POLICY-PROPOSAL.md','CLOSURE-PLAN.md']}
for page in render['pages']:
    p=R/page['source'];text=p.read_text(encoding='utf-8')
    targets=re.findall(r'\]\(([^)]+)\)',text) if page['source'] in fullpages else re.findall(r'\[Historical adoption research\]\(([^)]+)\)',text)
    for target in targets:
        if not target.startswith('#') and not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target):assert (p.parent/target.split('#')[0]).exists(),(page['source'],target)
for folder in ['closure-triage','c01-inventory','c02-function-review','c03-type-review','c04-expression-review','c05-collection-pattern-review','c06-host-profile-review','c07-lifecycle-review']:
    prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
    for name,item in previous['files'].items():assert sha((prior/name).read_bytes())==(item['sha256'] if isinstance(item,dict) else item),folder+'/'+name
    builderBlob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles'])['docs/xpath40-catalog/build-closure-plan.py']
    with tempfile.TemporaryDirectory(prefix='bosak-prior-closure-') as temporary:
        historical=Path(temporary);shutil.copytree(C/'evidence/closure-triage',historical/'evidence/closure-triage');builder=historical/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',builderBlob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
        for name,expected in previous['outputs'].items():
            if name in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:actual=sha(normalized(historical/name))
            elif name=='GRAMMAR-COVERAGE-MATRIX.json':
                checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9','c06-host-profile-review':'d00a8fb0','c07-lifecycle-review':'c854e9bb'}[folder]
                actual=sha(git('show',checkpoint+':docs/xpath40-catalog/'+name).replace(b'\r\n',b'\n'))
            else:actual=sha(normalized(C/name))
            assert actual==expected,folder+' checkpoint '+name
if '--local-artifacts' in sys.argv:
    for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256'],v['source']
print('C08 verified:28 original nulls preserved;16 delivery/16 head trees and58 all-path historical changes reconstructed; explicit approval applies26 legacy exceptions/two non-adoptions. Five packages open; C00-C07 preserved.')
