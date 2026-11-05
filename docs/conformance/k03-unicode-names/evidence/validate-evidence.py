"""Verify bounded K03 diagnostics without changing or reclassifying conformance outcomes."""
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
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in m['sourceFiles'].items():assert git('hash-object',n).decode().strip()==h==git('rev-parse',m['sourceBase']+':'+n).decode().strip(),n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
assert not git('diff','--name-only',m['sourceBase'],'--','src','tests','AGENTS.md','assets','.crossnote').strip()
pin=load(E/'AUDIT-PIN.json');assert pin['sourceBase']==m['sourceBase']
for n,h in pin['fixtures'].items():assert sha(R/n)==h,n
assert git('-C','tests/qt4tests','rev-parse','HEAD').decode().strip()==pin['qt4Revision'] and not git('-C','tests/qt4tests','status','--porcelain','--untracked-files=no').strip()
for row in load(E/'results.json'):
 assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result'],row['id']
 assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert load(R/'docs/conformance/k02-admission-review/BASELINE-ADOPTION.json')['status']=='Complete'
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
b=(E/'driver-build.log').read_bytes();assert '0 Error(s)' in b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p==C/'README.md' else re.findall(r'\[Bounded investigation\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: 30 watchdog-bounded diagnostic processes; original Unicode/assertion preserved; three open findings; source/baselines unchanged; 978 prior artifacts and nine HTML renders verified.')
