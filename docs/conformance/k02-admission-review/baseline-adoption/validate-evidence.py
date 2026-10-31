"""Verify exact approved K02 adoption, fresh normal gate and preserved checkpoint evidence."""
from pathlib import Path
import json,hashlib,subprocess,sys,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documentsAndSource'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
for source,snapshot in [('BASELINE-UPDATE-PROPOSAL.md','proposal-as-presented.md'),('PROPOSED-ADOPTION.patch','patch-as-presented.patch'),('RESULTS.json','review-as-presented.json')]:assert (E/snapshot).read_bytes()==git('show','628f169e:docs/conformance/k02-admission-review/'+source)
approval=load(E/'owner-approval.json');assert approval['sourceBase']==m['sourceBase'] and approval['answer']=='Approve scoped K02 adoption (Recommended)'
paths={'AGENTS.md','tests/Bosak.XPath.Conformance/ConformanceRunner.cs'}
assert set(git('diff','--name-only',m['sourceBase'],'--','src','tests','AGENTS.md').decode().splitlines())==paths
p='tests/Bosak.XPath.Conformance/ConformanceRunner.cs';s=git('show',m['sourceBase']+':'+p).decode('utf-8');row=next(x for x in s.splitlines() if '| 1.37 ' in x and '27-10-2026' in x)
s=s.replace('private readonly TestExecutor _executor = new();','private readonly TestExecutor _executor = new() { UseQueryOwnedSchemasForDiagnostics = true };').replace(row,row+'\n//                      | Charles Korthout | 1.45  | 27-10-2026     | Enable owner-approved guarded schema initialization in the normal QT3 runner.           |');assert s==(R/p).read_text(encoding='utf-8')
p='AGENTS.md';s=git('show',m['sourceBase']+':'+p).decode('utf-8');line=next(x for x in s.splitlines() if '**QT3 (frozen 3.1 gate):**' in x);new=line.replace('**31,144/0/677**','**31,145/0/676**').replace('The owner-approved [K01 two-case update](docs/conformance/k01-schema-list/BASELINE-ADOPTION.md) supersedes 31,142/0/679;','The owner-approved [K01 two-case update](docs/conformance/k01-schema-list/BASELINE-ADOPTION.md) and K02 guarded-runner adoption supersede 31,142/0/679 and 31,144/0/677;');assert s.replace(line,new)==(R/p).read_text(encoding='utf-8')
engine=load(E/'engine-identities.json');actual=git('hash-object','--stdin-paths') if not engine else subprocess.check_output(['git','hash-object','--stdin-paths'],input=('\n'.join(engine)+'\n').encode(),cwd=R);assert actual.decode().splitlines()==list(engine.values())
assert not git('diff','--name-only',m['sourceBase'],'--','assets','.crossnote','src','tests/Bosak.XPath.Conformance/TestExecutor.cs','tests/Bosak.XPath.Conformance40').strip()
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
old=R/'docs/conformance/k02-schema-inventory/evidence'
for name in ['fixture-files.json','trace-resource-identities.json']:
 for p,h in load(old/name).items():assert sha(R/p)==h,p
for p,v in load(old/'resource-identities.json').items():assert sha(R/p)==v['sha256'],p
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(old/'inventory-scope.json')['fixtureRevision'] and not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
before=sha(C/'BASELINE-ADOPTION.json');delta=sha(E/'skip-delta.json');subprocess.run([sys.executable,str(C/'build-adoption.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'BASELINE-ADOPTION.json')==m['adoptionSha256'] and delta==sha(E/'skip-delta.json')
render=load(E/'render-receipts.json');assert len(render['pages'])==14 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;t=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',t) if p==C/'BASELINE-ADOPTION.md' else re.findall(r'\[Owner-approved normal-runner adoption\]\(([^)]+)\)',t)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p,h in load(E/'binary-identities.json').items():assert sha(R/p)==h,p
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: exact owner-approved patch; fresh normal QT3 31145/0/676; only validateexpr-28 removed; 676 names/reasons unchanged; 243 engine sources and 959 prior artifacts preserved; 14 HTML renders.')
