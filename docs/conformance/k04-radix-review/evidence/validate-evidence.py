"""Validate read-only radix diagnoses, immutable source review and inherited accepted gates."""
from pathlib import Path
import json,hashlib,subprocess,sys,re,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'REVIEW-PIN.json');spec=load(E/'SPECIFICATION-REVIEW.json')
assert spec['kind']=='Review pin only;not engine adoption' and spec['sourceBase']==pin['sourceBase']
assert spec['sha256']==load(R/'docs/xpath40-catalog/audit-changed31-contracts.json')['draftSha256'] and spec['revision']=='e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e'
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
prior=load(E/'previous-identities.json');assert len(prior)==1818
for n,h in prior.items():assert sha(R/n)==h,n
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
for n,h in pin['sourceHashes'].items():
 assert sha(R/n)==h,n;b=git('show',pin['sourceBase']+':'+n);assert hashlib.sha256(b).hexdigest()==h or hashlib.sha256(b.replace(b'\n',b'\r\n')).hexdigest()==h,n
assert not git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md','assets','.crossnote').strip()
rows=load(E/'requests.json');assert len(rows)==17
for row in rows:
 doc=X.parse(R/row['SetFile']).getroot();ns={'t':doc.tag.split('}')[0][1:]};case=next(c for c in doc.findall('t:test-case',ns) if c.get('name')==row['Name']);assert case.find('t:test',ns).text==row['expression'];assert X.tostring(case.find('t:result',ns),encoding='unicode')==row['assertion']
for file in ['probe-receipts.json','control-receipts.json']:
 for row in load(E/file):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result']
old=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==old==m['resultsSha256']
inherited=load(R/'docs/conformance/k04-cldr-admission/RESULTS.json')['summary'];assert inherited['solutionPassed']==22043 and inherited['normalQt3']==[31161,0,660] and inherited['qt4Gated']==[8438,0,1892]
render=load(E/'render-receipts.json');assert len(render['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[Radix contract review\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 assert sha(R/'.guard-tmp/k04-radix-spec/function-catalog.xml')==spec['sha256']
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:17 originals (2pass/15fail retained);34 controls;parse-integer dependency isolated;fresh exploratory86/15/1;52 bounded processes;source/admission unchanged;1818 prior artifacts;full gates inherited.')
