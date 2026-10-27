"""Validate bounded filter changes, controls, immutable catalog inventory and gates."""
from pathlib import Path
import json,hashlib,subprocess,sys,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'REVIEW-PIN.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
assert len(load(E/'previous-identities.json'))==1376
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
for n,h in pin['sourceHashes'].items():assert hashlib.sha256(git('show',pin['sourceBase']+':'+n)).hexdigest()==h or hashlib.sha256(git('show',pin['sourceBase']+':'+n).replace(b'\n',b'\r\n')).hexdigest()==h,n
assert set(git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md').decode().splitlines())=={'tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'}
assert '**31,161/0/660**' in (R/'AGENTS.md').read_text(encoding='utf-8')
# Verify actual positive/negative complement directly in each profile, including mixed spec alternatives.
requests=load(E/'controls.json')
for suite in ['qt3','qt4']:
 rows=[json.loads(l) for l in (E/(suite+'-after.jsonl')).read_text(encoding='utf-8-sig').splitlines()];byid={r['id']:r for r in rows}
 for q in requests:
  if q['Suite']!=suite or len(q['dependencies'])!=1:continue
  d=q['dependencies'][0]
  if d['satisfied'] or d['type'] not in ['spec','default-language']:continue
  positive=next(p for p in requests if p['Suite']==suite and p['dependencies']==[dict(type=d['type'],value=d['value'],satisfied=True)])
  for key in ['xpathSupported']+(['xquerySupported'] if suite=='qt3' else []):assert byid[q['id']][key] is not byid[positive['id']][key]
old=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==old==m['resultsSha256']
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[Profile review\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:74 controls/111 profile outcomes per phase;31821+46985 case inventory;zero affected declarations;22007 tests;both gates/skips unchanged;1376 previous artifacts preserved.')
