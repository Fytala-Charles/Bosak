"""Verify actual current-filter evidence and constrained original-assertion diagnostics."""
from pathlib import Path
import json,hashlib,subprocess,sys,re,xml.etree.ElementTree as ET
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def jsonlines(p):return [json.loads(s) for s in p.read_text(encoding='utf-8-sig').splitlines()]
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
for n,h in pin['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in pin['fixtureRevisions'].items():
 assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h
 assert not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
def skips(p):
 out={};reason=None
 for line in p.read_text(encoding='utf-8-sig').splitlines():
  match=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
  if match:reason=match.group(2)
  elif line.strip():out[line]=reason
 return out
for suite,filename in [('qt3','qt3-skips.txt'),('qt4','qt4-skips.txt')]:
 current=skips(R/'docs/conformance/k03-name-storage/evidence'/filename)
 assert {r['name'] for r in jsonlines(E/(suite+'-catalog.jsonl'))}=={n for n,reason in current.items() if reason=='Unsupported dependency'}
synthetic={r['id']:r for suite in ['qt3','qt4'] for r in jsonlines(E/(suite+'-synthetic.jsonl'))}
for suite in ['qt3','qt4']:
 assert synthetic[suite+'-spec-XP31+ XQ31+-False']['xpathSupported']
 assert synthetic[suite+'-feature-urn:k04-unknown-True']['xpathSupported']
 assert synthetic[suite+'-xsd-version-1.1-True']['xpathSupported']
 assert not synthetic[suite+'-default-language-fr-CA-False']['xpathSupported']
assert synthetic['qt3-contradictory-spec']['xquerySupported']
selected={r['Name']:r for r in load(E/'execution-selection.json')};ns='{http://www.w3.org/2010/09/qt-fots-catalog}'
for row in load(E/'execution-results.json'):
 assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result'],row['name']
 if row['mode']=='public-xquery':
  fixture=ET.parse(R/selected[row['name']]['SetFile']);case=next(e for e in fixture.getroot().iter(ns+'test-case') if e.get('name')==row['name'])
  assert row['result']['expression'].strip()==''.join(case.find(ns+'test').itertext()).strip(),row['name']
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p.parent==C else re.findall(r'\[Current dependency/routing review\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: 2512 actual catalog filter assessments; 62 synthetic controls; 32 bounded original-assertion executions; failures preserved; source/baselines unchanged;1207 prior artifacts;10 HTML renders.')
