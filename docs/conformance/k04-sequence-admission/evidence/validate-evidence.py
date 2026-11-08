"""Validate exact pending sequence proposal, copied-source identity and full gate deltas."""
from pathlib import Path
import json,hashlib,subprocess,sys,re,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json');pin=load(E/'PROPOSAL-PIN.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
prior=load(E/'previous-identities.json');assert len(prior)==2520
for n,h in prior.items():assert sha(R/n)==h,n
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
for n,h in pin['sourceHashes'].items():
 assert sha(R/n)==h,n;b=git('show',pin['sourceBase']+':'+n);assert hashlib.sha256(b).hexdigest()==h or hashlib.sha256(b.replace(b'\n',b'\r\n')).hexdigest()==h,n
assert not git('diff','--name-only',pin['sourceBase'],'--','src','tests','AGENTS.md','assets','.crossnote').strip()
assert '**31,161/0/660**' in (R/'AGENTS.md').read_text(encoding='utf-8')
patch=(C/'PROPOSED-ADOPTION.patch').read_bytes();assert b'\r' not in patch
expected={}
for section in patch.decode().split('--- a/')[1:]:
 name=section.splitlines()[0];s=git('show',pin['sourceBase']+':'+name).decode('utf-8')
 for h in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
  lines=h.splitlines(True);old=''.join(l[1:] for l in lines if l.startswith((' ','-')));new=''.join(l[1:] for l in lines if l.startswith((' ','+')));assert old in s;s=s.replace(old,new,1)
 expected[name]=s
assert set(expected)=={'tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs','AGENTS.md'} and '**31,162/0/659**' in expected['AGENTS.md']
subprocess.run(['git','apply','--cached','--check',str(C/'PROPOSED-ADOPTION.patch')],cwd=R,check=True)
identities=load(E/'prototype-source-identities.json')
for suite,items in identities.items():
 for n,h in items.items():
  p=R/('.guard-tmp/k04-sequence-harness-'+suite)/Path(n).name
  if '--local-artifacts' in sys.argv:assert sha(p)==h,n
  if n in expected:
   snapshot=E/(suite+'-prototype-filter.cs.txt');assert sha(snapshot)==h;assert snapshot.read_text(encoding='utf-8').split('\n',1)[1]==expected[n]
  elif '--local-artifacts' in sys.argv:assert p.read_text(encoding='utf-8').split('\n',1)[1]==(R/n).read_text(encoding='utf-8'),n
subprocess.run([sys.executable,str(E/'build-inventory.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
for row in load(E/'requests.json'):
 doc=X.parse(R/row['SetFile']).getroot();ns={'t':doc.tag.split('}')[0][1:]};case=next(c for c in doc.findall('t:test-case',ns) if c.get('name')==row['Name']);assert case.find('t:test',ns).text==row['expression'];assert X.tostring(case.find('t:result',ns),encoding='unicode')==row['assertion']
for file in ['probe-receipts.json','expression-control-receipts.json']:
 for row in load(E/file):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result']
mechanism=load(E/'mechanism-receipt.json');assert [json.loads(l) for l in (E/mechanism['log']).read_text(encoding='utf-8-sig').splitlines()]==mechanism['results']
old=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert sha(C/'RESULTS.json')==old==m['resultsSha256'] and not load(C/'RESULTS.json')['summary']['ownerApproved']
render=load(E/'render-receipts.json');assert len(render['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['documents']:
 if not n.endswith('.md'):continue
 p=R/n;s=p.read_text(encoding='utf-8');targets=re.findall(r'\[Exact baseline review\]\(([^)]+)\)',s) if p.parent!=C else re.findall(r'\]\(([^)]+)\)',s)
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS:exact three-file dry patch;288filter/16expression controls;2original passes;rehearsals31162/0/659 and8587/0/1892;only042removed each;normal source/binaries unchanged;2520prior artifacts;approval pending.')
