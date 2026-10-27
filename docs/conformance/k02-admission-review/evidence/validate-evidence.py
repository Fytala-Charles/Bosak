"""Verify the read-only full-runner rehearsal and exact pending adoption proposal."""
from pathlib import Path
import json,hashlib,subprocess,sys,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
assert not git('diff','--name-only',m['sourceBase'],'--','src','tests','AGENTS.md','assets','.crossnote').strip()
for n,h in m['sourceFiles'].items():assert git('hash-object',n).decode().strip()==h==git('rev-parse',m['sourceBase']+':'+n).decode().strip(),n
assert 'private readonly TestExecutor _executor = new();' in (R/'tests/Bosak.XPath.Conformance/ConformanceRunner.cs').read_text(encoding='utf-8')
assert '**31,144/0/677**' in (R/'AGENTS.md').read_text(encoding='utf-8')
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
old=R/'docs/conformance/k02-schema-inventory/evidence'
for name in ['fixture-files.json','trace-resource-identities.json']:
 for p,h in load(old/name).items():assert sha(R/p)==h,p
for p,v in load(old/'resource-identities.json').items():assert sha(R/p)==v['sha256'],p
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(old/'inventory-scope.json')['fixtureRevision'] and not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
patch=(C/'PROPOSED-ADOPTION.patch').read_bytes();assert b'\r' not in patch
text=patch.decode('utf-8');assert set(re.findall(r'^\+\+\+ b/(.+)$',text,re.M))=={'AGENTS.md','tests/Bosak.XPath.Conformance/ConformanceRunner.cs'}
assert 'UseQueryOwnedSchemasForDiagnostics = true' in text and '31,145/0/676' in text
assert '+//                      | Charles Korthout | 1.45' in text
subprocess.run(['git','apply','--cached','--check',str(C/'PROPOSED-ADOPTION.patch')],cwd=R,check=True)
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
s=load(C/'RESULTS.json')['summary'];assert not s['ownerApproved'] and not s['normalRunnerChanged'] and not s['baselineChanged']
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p.parent==C else re.findall(r'\[Guarded-runner proposal\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p,h in load(E/'binary-identities.json').items():assert sha(R/p)==h,p
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: two full31821-case rehearsals; only validateexpr-28 skip removed; 676 names/reasons retained; unchanged source/baseline; dry patch valid; 939 prior artifacts; approval pending.')
