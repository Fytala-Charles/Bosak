"""Validate owner-approved exact patch, normal gate delta and preserved evidence."""
from pathlib import Path
import subprocess,json,hashlib,re
A=Path(__file__).resolve().parent;R=A.parents[3];C=A.parent;E=C/'evidence'
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(read(p))
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
for n,h in load(A/'historical-identities.json').items():assert sha(R/n)==h,n
m=load(E/'manifest.json')
for n,h in m['files'].items():assert sha(E/n)==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
assert sha(C/'RESULTS.json')==m['reviewSha256']
# Reconstruct precisely the approved patch from the committed proposal base.
patch=read(C/'PROPOSED-ADOPTION.patch')
for section in patch.split('--- a/')[1:]:
 name=section.splitlines()[0];expected=git('show','d0ca0741:'+name).decode('utf-8')
 for h in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
  lines=h.splitlines(True);old=''.join(x[1:] for x in lines if x.startswith((' ','-')));new=''.join(x[1:] for x in lines if x.startswith((' ','+')))
  assert old in expected;expected=expected.replace(old,new,1)
 assert read(R/name)==expected,name
assert set(git('diff','--name-only','d0ca0741','--','src','tests','AGENTS.md').decode().splitlines())=={'AGENTS.md','tests/Bosak.XPath.Conformance/TestExecutor.cs'}
before=skips(R/'docs/conformance/k03-name-storage/evidence/qt3-skips.txt');after=skips(A/'qt3-skips.txt')
assert len(after)==660 and all(before.get(k)==v for k,v in after.items())
assert {k:v for k,v in before.items() if k not in after}==load(E/'skip-delta.json')['removed']
assert re.search(r'Passed:\s+31161\s+Failed:\s+0\s+Skipped:\s+660',read(A/'qt3.log'))
assert re.search(r'Passed:\s+8437\s+Failed:\s+0\s+Skipped:\s+1893',read(A/'qt4.log'))
tests=read(A/'tests-unrestricted.log');counts=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',tests)
assert len(counts)==10 and sum(map(int,counts))==22007 and 'Failed!' not in tests,counts
assert '0 Error(s)' in read(A/'build.log')
v=load(A/'validation.json');assert v['ownerApproved'] and v['normalQt3']==[31161,0,660] and v['solutionPassed']==22007
fixture=load(R/'docs/conformance/k04-dependency-audit/evidence/AUDIT-PIN.json')
for n,h in fixture['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in fixture['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
manifest=load(A/'manifest.json')
for n,h in manifest['files'].items():assert sha(A/n)==h,n
for n,h in manifest['documents'].items():assert git('hash-object',n).decode().strip()==h,n
render=load(A/'render-receipts.json');assert len(render['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
print('PASS: exact approved patch;normal QT3 31161/0/660;16 removals/660 retained reasons;22007 solution tests;qt4 8437/0/1893;prior evidence and fixtures preserved.')
