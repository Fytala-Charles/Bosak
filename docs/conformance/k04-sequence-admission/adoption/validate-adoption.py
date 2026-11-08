"""Verify exact approved source patch,fresh normal gates and preserved proposal evidence."""
from pathlib import Path
import subprocess,json,hashlib,re
A=Path(__file__).resolve().parent;C=A.parent;E=C/'evidence';R=C.parents[2]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
for n,h in load(A/'proposal-identities.json').items():assert sha(R/n)==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
assert sha(C/'RESULTS.json')==load(E/'manifest.json')['resultsSha256']
patch=read(C/'PROPOSED-ADOPTION.patch');names=[]
for section in patch.split('--- a/')[1:]:
 name=section.splitlines()[0];names.append(name);expected=git('show','e924a708:'+name).decode('utf-8')
 for h in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
  lines=h.splitlines(True);old=''.join(l[1:] for l in lines if l.startswith((' ','-')));new=''.join(l[1:] for l in lines if l.startswith((' ','+')));assert old in expected;expected=expected.replace(old,new,1)
 assert read(R/name)==expected,name
assert set(git('diff','--name-only','e924a708','--','src','tests','AGENTS.md').decode().splitlines())==set(names)=={'AGENTS.md','tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'}
def skips(p):
 d={};reason=None
 for l in read(p).splitlines():
  if l.startswith('=== '):reason=l.split(' skipped: ',1)[1].removesuffix(' ===')
  elif l.strip():assert l not in d;d[l]=reason
 return d
for suite,passed,skip in [('qt3',31162,659),('qt4',8587,1892)]:
 before=skips(R/('docs/conformance/k04-parse-integer-fix/evidence/'+suite+'-skips.txt'));after=skips(A/(suite+'-skips.txt'));assert after==skips(E/(suite+'-rehearsal-skips.txt')) and len(after)==skip and all(before.get(k)==v for k,v in after.items()) and {k:v for k,v in before.items() if k not in after}=={'format-integer-042':'Unsupported dependency'}
 assert re.search(r'Passed:\s+'+str(passed)+r'\s+Failed:\s+0\s+Skipped:\s+'+str(skip),read(A/(suite+'.log')))
 assert [json.loads(l) for l in read(A/(suite+'-controls.jsonl')).splitlines()]==[json.loads(l) for l in read(E/(suite+'-prototype.jsonl')).splitlines()]
 assert re.search(r'Total:\s+1\s+Passed:\s+1\s+Failed:\s+0\s+Skipped:\s+0',read(A/(suite+'-original.log')))
for row in load(A/'check-receipts.json'):assert row['exitCode']==0 and not row['timeout']
tests=read(A/'solution-tests.log');counts=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',tests);assert len(counts)==10 and sum(map(int,counts))==22198 and 'Failed!' not in tests
assert '0 Error(s)' in read(A/'build.log')
assert load(A/'approval.json')['answer']=='Approve exact one-case adoption (Recommended)'
v=load(A/'validation.json');assert v['ownerApproved'] and v['normalQt3']==[31162,0,659] and v['normalQt4']==[8587,0,1892] and v['solutionPassed']==22198
for n,h in load(E/'PROPOSAL-PIN.json')['fixtureHashes'].items():assert sha(R/n)==h,n
for suite,h in load(E/'PROPOSAL-PIN.json')['fixtureRevisions'].items():assert git('-C','tests/'+suite,'rev-parse','HEAD').decode().strip()==h and not git('-C','tests/'+suite,'status','--porcelain','--untracked-files=no').strip()
m=load(A/'manifest.json')
for n,h in m['files'].items():assert sha(A/n)==h,n
for n,h in m['documents'].items():assert git('hash-object',n).decode().strip()==h,n
render=load(A/'render-receipts.json');assert len(render['pages'])==12 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
print('PASS:exact owner-approved3file patch;normalQT3 31162/0/659;qt4 8587/0/1892;only042removed each;288normal controls/2original passes;22198solution tests;historical evidence intact.')
