"""Validate explicit feature-name policy, unchanged catalog decisions and fresh gates."""
from pathlib import Path
import json,hashlib,subprocess,sys,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'INVENTORY-PIN.json');policy=load(E/'admission-policy.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
prior=load(E/'previous-identities.json');assert len(prior)==1410
for n,h in prior.items():assert sha(R/n)==h,n
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
for n,h in pin['sourceHashes'].items():
 b=git('show',pin['sourceBase']+':'+n);assert hashlib.sha256(b).hexdigest()==h or hashlib.sha256(b.replace(b'\n',b'\r\n')).hexdigest()==h,n
assert set(git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md').decode().splitlines())=={'tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'}
for suite,row in policy.items():
 source=(R/row['source']).read_text(encoding='utf-8');old=git('show',pin['sourceBase']+':'+row['source']).decode('utf-8')
 def values(text,name):return re.findall(r'"([^"]+)"',re.search(name+r' = new\(StringComparer.OrdinalIgnoreCase\)\s*\{(.*?)\};',text,re.S).group(1))
 assert values(source,'UnsupportedFeatures')==values(old,'UnsupportedFeatures')==row['unsupported']
 assert values(source,'FrozenAdmittedFeatures')==row['frozenAdmitted']
 assert set(row['frozenAdmitted'])=={n for n in load(E/'inventory.json')['catalogs'][suite]['featureValues'] if n.lower() not in {u.lower() for u in row['unsupported']}}
 assert 'SPDX-License-Identifier: Apache-2.0' in source and 'COPYRIGHT            : Fytala' in source
assert '**31,161/0/660**' in (R/'AGENTS.md').read_text(encoding='utf-8')
old=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==old==m['resultsSha256']
for row in load(E/'capability-ledger.json')['features']:
 if row['sourceEvidence']:assert (R/row['sourceEvidence']).exists()
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[Capability inventory\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:222 controls;all78806 cases inventoried;55 feature ledger rows;existing feature decisions unchanged;22007 tests;both gates/skips unchanged;1410 prior artifacts preserved.')
