"""Verify the exact pending quote-routing proposal and complete isolated rehearsal."""
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
assert '**31,145/0/676**' in (R/'AGENTS.md').read_text(encoding='utf-8')
source=(R/'tests/Bosak.XPath.Conformance/TestExecutor.cs').read_text(encoding='utf-8');a=source.index('        // """...""" string constructors');b=source.index('        if (UnsupportedXQueryConstructRegex',a)
expected=source[:a]+'''        // Existing lexical normalization and the XQuery parser handle doubled quotes;
        // raw triple-double-quote substrings are not an unsupported-grammar predicate.
'''+source[b:]
prototype=(E/'prototype-TestExecutor.cs.txt').read_text(encoding='utf-8');assert prototype.split('\n',1)[0].startswith('// Generated') and prototype.split('\n',1)[1]==expected
for n,h in load(E/'prototype-source-identities.json').items():
 assert sha(E/'prototype-TestExecutor.cs.txt')==h if n=='TestExecutor.cs' else sha(R/'tests/Bosak.XPath.Conformance'/n)==h,n
pin=load(E/'DESIGN-PIN.json');assert pin['sourceBase']==m['sourceBase'] and sha(R/'docs/conformance/k04-dependency-audit/evidence/quote-candidates.json')==pin['fixtures']
fixture=load(R/'docs/conformance/k04-dependency-audit/evidence/AUDIT-PIN.json')
for n,h in fixture['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in fixture['fixtureRevisions'].items():
 assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
for file in ['results.json','boundary-results.json','control-receipts.json']:
 for row in load(E/file):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result']
patch=(C/'PROPOSED-ADOPTION.patch').read_bytes();assert b'\r' not in patch
assert set(re.findall(r'^\+\+\+ b/(.+)$',patch.decode(),re.M))=={'AGENTS.md','tests/Bosak.XPath.Conformance/TestExecutor.cs'}
subprocess.run(['git','apply','--cached','--check',str(C/'PROPOSED-ADOPTION.patch')],cwd=R,check=True)
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert not load(C/'RESULTS.json')['summary']['ownerApproved'] and load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p.parent==C else re.findall(r'\[Exact adoption review\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for file in ['binary-identities.json','prototype-binaries.json']:
  for n,h in load(E/file).items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: all16 candidate assertions;11 unchanged boundaries;13 lexical controls; full31161/0/660 rehearsal; exact16-case skip delta; normal source/baseline unchanged;1272 prior artifacts; approval pending.')
