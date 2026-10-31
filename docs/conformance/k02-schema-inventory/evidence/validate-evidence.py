"""Verify the bounded wider audit, unchanged source and preserved raw evidence."""
from pathlib import Path
import json,hashlib,subprocess,sys,zipfile,collections,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,n
for n,h in m['documents'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('hash-object',n).strip()==git('rev-parse',':'+n).strip(),n
for n,h in load(E/'source-identities.json').items():assert git('hash-object',n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha((R/n).read_bytes())==h,n
assert not git('diff','--name-only',m['sourceBase'],'--','src','tests','AGENTS.md','assets','.crossnote').strip()
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(E/'inventory-scope.json')['fixtureRevision']
assert not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
for n in ['fixture-files.json','trace-resource-identities.json']:
 for p,h in load(E/n).items():assert sha((R/p).read_bytes())==h,p
for p,v in load(E/'resource-identities.json').items():assert sha((R/p).read_bytes())==v['sha256'],p
allcases=load(E/'all-cases.json');cases=load(E/'schema-cases.json')
assert len(allcases)==31821 and len({r['name'] for r in allcases})==31821
assert len(cases)==1010 and collections.Counter(r['baseline'] for r in cases)=={'Passed':895,'Skipped':115}
assert sum(bool(r['inspectPolicy']) for r in cases)==902
rows=[]
for n in ['metadata-results.json','comparison-results.json','boundary-results.json']:rows+=load(E/n)
index=load(E/'raw-log-index.json');assert len(rows)==len(index)==1584
with zipfile.ZipFile(E/'raw-process-logs.zip') as z:
 assert set(z.namelist())==set(index)
 for row in rows:
  b=z.read(row['log']);assert sha(b)==index[row['log']]
  assert json.loads(b.decode('utf-8-sig').splitlines()[-1])==row['result'],row['log']
  assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20 and not row.get('budgetExceeded',False)
before=sha((C/'RESULTS.json').read_bytes());subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before==sha((C/'RESULTS.json').read_bytes())==m['reviewSha256']
r=load(C/'RESULTS.json');assert len(r['regressions'])==72 and r['summary']['totalWatchdogProcesses']==1584
for name in ['driver-build-fixed.log','driver-final-build.log','driver-normalized-build.log']:
 b=(E/name).read_bytes();s=b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig');assert '0 Error(s)' in s,name
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for page in render['pages']:
 p=R/page['source'];assert p.exists()
 if '--local-artifacts' in sys.argv:assert sha((R/page['html']).read_bytes())==page['sha256']
for p in [C/'README.md']:
 for href in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),href
if '--local-artifacts' in sys.argv:
 for v in load(E/'binary-identities.json'):assert sha(Path(v['path']).read_bytes())==v['sha256'],v['path']
print('PASS: 1584 process receipts; 72 preserved regressions; 830 prior artifacts; unchanged source/gates; deterministic review; nine HTML renders.')
