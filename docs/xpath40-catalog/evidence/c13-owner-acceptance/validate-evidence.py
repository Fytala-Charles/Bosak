"""Validate explicit bounded-catalog acceptance and immutable pre-approval receipts."""
from pathlib import Path
import json,hashlib,subprocess,sys,tempfile,shutil,re
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def norm(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
m=load(E/'manifest.json');approval=load(E/'owner-approval.json');accepted=load(C/'CATALOG-ACCEPTANCE.json')
assert approval['decision']=='Approved' and approval['answer']=='Approve bounded catalog closure (Recommended)'
assert approval['proposalId']==accepted['proposalId']=='REQ-126-C13-bounded-catalog-acceptance-v1'
assert approval['presentedCommit']==m['documentationBase'] and approval['dispositionRecords']==38 and approval['unmetObservationsRetained']==48
for n,v in approval['approvedArtifacts'].items():
 b=git('show',approval['presentedCommit']+':docs/xpath40-catalog/'+n)
 assert b==(E/v['snapshot']).read_bytes() and sha(b)==v['sha256']
 assert git('rev-parse',approval['presentedCommit']+':docs/xpath40-catalog/'+n).decode().strip()==v['gitBlob']
 assert (C/n).read_bytes().replace(b'\r\n',b'\n')==b.replace(b'\r\n',b'\n')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,'index '+n
for n,h in m['tools'].items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['previousCheckpoints'].items():assert sha((C/n).read_bytes())==h,n
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h,n
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
before={n:sha(norm(C/n)) for n in m['outputs']};assert before==m['outputs']
subprocess.run([sys.executable,str(C/'build-closure-plan.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(norm(C/n)) for n in m['outputs']}
plan=load(C/'CLOSURE-PLAN.json');assert plan['status']=='Complete' and plan['openClosureWorkpackages']==0 and all(v['status']=='Complete' for v in plan['workpackages']) and len(plan['workpackages'])==13
assert all(plan['acceptance'][id]=='Accepted bounded catalog' for id in ['AC01','AC02','AC03','AC04'])
assert plan['ownerApproval']==accepted['ownerApproval']=='evidence/c13-owner-acceptance/owner-approval.json'
review=load(E/'approved-FINAL-CLOSURE-REVIEW.json');proposal=load(E/'approved-C13-ACCEPTANCE-PROPOSAL.json')
assert accepted['status']=='Complete' and accepted['technicalFindingsResolved']==0 and accepted['adoptionPinsRecovered']==0
assert accepted['unmetBatchControls']==review['unmetBatchControls'] and len(accepted['unmetBatchControls'])==48
assert all(v['originalObservation']['passed'] is False for v in accepted['unmetBatchControls'])
assert len(accepted['dispositions'])==38 and {v['id'] for v in accepted['dispositions']}==set(proposal['findings'])
for v in accepted['dispositions']:
 original=next(q for q in review['findings'] if q['id']==v['id'])
 assert v['originalTechnicalStatus']==original['technicalStatus'] and v['originalOwner']==original['owner'] and v['acceptedScope']==original['proposedCatalogDisposition'] and v['technicalExitCriteriaWaived'] is False
registry=(R/'docs/FEATURE_REQUESTS.md').read_text(encoding='utf-8');section=registry.split('### REQ-126:',1)[1].split('\n### REQ-',1)[0]
assert '**Status:** Complete' in section and all('- [x] '+id in section for id in ['AC-01','AC-02','AC-03','AC-04','AC-05'])
assert '**Complete**' in next(l for l in registry.splitlines() if l.startswith('| REQ-126 |'))
# Original validators remain original-checkpoint artifacts. Reproduce historical plan
# outputs in temporary directories instead of running their open-state assertions on closure.
for folder in m['priorFolders']:
 prior=C/'evidence'/folder;previous=load(prior/'manifest.json')
 for n,v in previous['files'].items():assert sha((prior/n).read_bytes())==(v['sha256'] if isinstance(v,dict) else v),folder+'/'+n
 if folder=='c13-final-review':
  for n,h in previous['markdownFiles'].items():
   b=git('show',approval['presentedCommit']+':'+n)
   assert h in {sha(b),sha(b.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n'))},n
  for n,v in load(prior/'review-inputs.json').items():assert git('rev-parse',approval['presentedCommit']+':docs/xpath40-catalog/'+n).decode().strip()==v['gitBlob']
 blob=previous.get('builderGitBlob') or (previous.get('tools') or previous['sourceFiles']).get('docs/xpath40-catalog/build-closure-plan.py')
 if blob:
  with tempfile.TemporaryDirectory(prefix='bosak-approved-checkpoint-') as temporary:
   old=Path(temporary);shutil.copytree(C/'evidence/closure-triage',old/'evidence/closure-triage');builder=old/'build-closure-plan.py';builder.write_bytes(git('cat-file','blob',blob));subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
   for n,h in previous['outputs'].items():
    if n in ['CLOSURE-PLAN.json','CONFORMANCE-SKIP-TRIAGE.json']:got=sha(norm(old/n))
    elif n=='GRAMMAR-COVERAGE-MATRIX.json':
     checkpoint={'c04-expression-review':'2d8a7005','c05-collection-pattern-review':'19d9e1a9','c06-host-profile-review':'d00a8fb0','c07-lifecycle-review':'c854e9bb','c08-adoption-research':'34080f04','c09-semantic-batch1':'06b53349','c10-semantic-batch2':'00ee9bf','c11-semantic-batch3':'22caf4ed','c12-semantic-batch4':'1ec2c210'}[folder];got=sha(git('show',checkpoint+':docs/xpath40-catalog/'+n).replace(b'\r\n',b'\n'))
    else:got=sha(norm(C/n))
    assert got==h,folder+' output '+n
 else:
  for n,h in previous['outputs'].items():assert sha(norm(C/n))==h,folder+' output '+n
assets=load(C/'evidence/c13-final-review/compiled-assets.json')
for n,v in assets.items():assert sha((R/n).read_bytes())==v['sha256']
links=load(E/'link-audit.json');assert not links['bad']
for v in links['links']:assert (R/v['target']).exists() and v['anchor'] is not False
render=load(E/'render-receipts.json');assert len(render['pages'])==15 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
if '--local-artifacts' in sys.argv:
 for v in assets.values():assert sha((R/v['compiled']).read_bytes())==v['sha256']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256']
print('REQ-126 accepted:13/13 workpackages complete,0 open;AC-01–04 explicitly accepted;38 dispositions and48 unmet observations preserved;C00-C13 receipts intact.')
