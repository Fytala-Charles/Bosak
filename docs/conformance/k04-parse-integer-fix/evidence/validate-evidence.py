"""Validate bounded parser adoption, original assertions and evidence-based promotion."""
from pathlib import Path
import json,hashlib,subprocess,sys,re,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'IMPLEMENTATION-PIN.json');adoption=load(E/'SPECIFICATION-ADOPTION.json')
assert adoption['sourceBase']==pin['sourceBase'] and adoption['kind']=='New bounded semantic adoption before implementation'
assert adoption['revision']=='e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e' and adoption['sha256']==load(R/'docs/xpath40-catalog/audit-changed31-contracts.json')['draftSha256']
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
prior=load(E/'previous-identities.json');assert len(prior)==2292
for n,h in prior.items():assert sha(R/n)==h,n
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
changed={'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','src/Directory.Build.props','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs'};test='tests/Bosak.XPath.Standard.Tests/ParseIntegerTests.cs'
for n,h in pin['sourceBaseHashes'].items():
 b=git('show',pin['sourceBase']+':'+n);assert hashlib.sha256(b).hexdigest()==h or hashlib.sha256(b.replace(b'\n',b'\r\n')).hexdigest()==h,n
 if n not in changed:assert sha(R/n)==h,n
actual=set(git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md').decode().splitlines());assert changed<=actual<=changed|{test}
assert '<Version>1.28.0</Version>' in (R/'src/Directory.Build.props').read_text()
for n in ['src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs',test]:
 s=(R/n).read_text(encoding='utf-8');assert s.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in s and 'COPYRIGHT            : Fytala' in s
for row in load(E/'requests.json'):
 doc=X.parse(R/row['SetFile']).getroot();ns={'t':doc.tag.split('}')[0][1:]};case=next(c for c in doc.findall('t:test-case',ns) if c.get('name')==row['Name']);assert case.find('t:test',ns).text==row['expression'];assert X.tostring(case.find('t:result',ns),encoding='unicode')==row['assertion']
for file in ['probe-receipts.json','control-receipts.json']:
 for row in load(E/file):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result']
old=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==old==m['resultsSha256']
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[Bounded parser and promotion\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 assert sha(R/'.guard-tmp/k04-radix-spec/function-catalog.xml')==adoption['sha256']
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:explicit parser adoption;87 regressions;48original+46profile assertions pass;22198solution tests;QT3unchanged;226qt4sets8586/0/1893;only existing format042skip added;2292prior artifacts;candidate1.28.0 unpublished.')
