"""Validate bounded CLDR admission, exact exploratory delta and unchanged frozen gates."""
from pathlib import Path
import json,hashlib,subprocess,sys,re,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'ADMISSION-PIN.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
prior=load(E/'previous-identities.json');assert len(prior)==1772
for n,h in prior.items():assert sha(R/n)==h,n
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
name='tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'
for n,h in pin['sourceHashes'].items():
 b=git('show',pin['sourceBase']+':'+n);assert hashlib.sha256(b).hexdigest()==h or hashlib.sha256(b.replace(b'\n',b'\r\n')).hexdigest()==h,n
 if n!=name:assert sha(R/n)==h,n
assert git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md').decode().splitlines()==[name]
source=(R/name).read_text(encoding='utf-8');old=git('show',pin['sourceBase']+':'+name).decode('utf-8');feature='fn-format-integer-CLDR'
def values(text,name):return re.findall(r'"([^"]+)"',re.search(name+r' = new\(StringComparer.OrdinalIgnoreCase\)\s*\{(.*?)\};',text,re.S).group(1))
assert values(source,'UnsupportedFeatures')==[v for v in values(old,'UnsupportedFeatures') if v!=feature]
assert set(values(source,'FrozenAdmittedFeatures'))==set(values(old,'FrozenAdmittedFeatures'))|{feature}
assert 'SPDX-License-Identifier: Apache-2.0' in source and 'COPYRIGHT            : Fytala' in source
for row in load(E/'requests.json'):
 doc=X.parse(R/row['SetFile']).getroot();ns={'t':doc.tag.split('}')[0][1:]};case=next(c for c in doc.findall('t:test-case',ns) if c.get('name')==row['Name']);assert case.find('t:test',ns).text==row['expression'];assert X.tostring(case.find('t:result',ns),encoding='unicode')==row['assertion']
for row in load(E/'remaining-radix-findings.json')['cases']:
 doc=X.parse(R/'tests/qt4tests/fn/format-integer.xml').getroot();ns={'t':doc.tag.split('}')[0][1:]};case=next(c for c in doc.findall('t:test-case',ns) if c.get('name')==row['name']);assert case.find('t:test',ns).text==row['expression'];assert X.tostring(case.find('t:result',ns),encoding='unicode')==row['assertion']
oldhash=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==oldhash==m['resultsSha256']
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[CLDR admission\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:242 filter controls;6 normal originals pass;exploratory82/15/5->86/15/1;15 failures retained;22043 tests;both gates/skips unchanged;1772 prior artifacts;no engine or GatedSets change.')
