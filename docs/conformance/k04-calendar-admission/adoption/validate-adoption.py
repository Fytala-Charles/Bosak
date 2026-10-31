"""Verify the owner-approved exact CB admission patch and fresh normal acceptance evidence."""
from pathlib import Path
import subprocess,json,re,hashlib
A=Path(__file__).resolve().parent;P=A.parent;E=P/'evidence';R=A.parents[3]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
def check_git_hashes(rows):
 names=list(rows)
 if not names:return
 actual=subprocess.check_output(['git','hash-object','--stdin-paths'],input=('\n'.join(names)+'\n').encode(),cwd=R).decode().splitlines()
 assert actual==[rows[n] for n in names]
def skips(p):
 reason=None;items={}
 for line in read(p).splitlines():
  if line.startswith('=== '):reason=line.split(' skipped: ',1)[1].rsplit(' ===',1)[0]
  elif line.strip():assert reason and line not in items;items[line]=reason
 return items
def totals(p):
 text=read(p);return [int(re.search(r'^'+k+r':\s+(\d+)',text,re.M)[1]) for k in ['Passed','Failed','Skipped']]
for name,h in load(A/'proposal-identities.json').items():assert sha(R/name)==h,name
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
assert load(P/'RESULTS.json')['ownerApproved'] is False # Preserve the historical proposal receipt.
approval=load(A/'approval.json');assert approval['answer']=='Approve exact two-case CB adoption (Recommended)'
assert approval['proposalRevision']==git('rev-parse','6df9a58b') and approval['decisionId']=='K04-CB-calendar-adoption-v1'
assert approval['releaseApproved'] is False
base=approval['proposalRevision'];names=[]
for section in read(P/'PROPOSED-ADOPTION.patch').split('--- a/')[1:]:
 name=section.splitlines()[0];names.append(name);expected=subprocess.check_output(['git','show',base+':'+name],cwd=R).decode('utf-8').replace('\r\n','\n')
 for hunk in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
  lines=hunk.splitlines(True);old=''.join(l[1:] for l in lines if l.startswith((' ','-')));new=''.join(l[1:] for l in lines if l.startswith((' ','+')));assert old in expected;expected=expected.replace(old,new,1)
 assert read(R/name)==expected,name
assert set(names)=={'AGENTS.md','tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'}
assert set(git('diff',base,'--name-only','--','src','tests','AGENTS.md').splitlines())==set(names)
check_git_hashes({n:h for n,h in load(E/'normal-source-identities.json').items() if n not in names})
check_git_hashes(load(R/'docs/conformance/k04-calendar-fix/evidence/source-identities.json'))
assert '<Version>1.28.1</Version>' in read(R/'src/Directory.Build.props')
inventory=load(E/'calendar-inventory.json')
for name,h in inventory['fixtureHashes'].items():assert sha(R/name)==h,name
pin=load(E/'REVIEW-PIN.json')
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');assert git('-C',str(root),'rev-parse','HEAD')==pin['fixtureRevisions'][suite]
 assert not git('-C',str(root),'status','--porcelain')
for name,h in pin['catalogueHashes'].items():assert sha(R/name)==h,name
for suite,expected in [('qt3',[31164,0,657]),('qt4',[8587,0,1892])]:
 assert totals(A/(suite+'.log'))==expected
 new=skips(A/(suite+'-skips.txt'));rehearsal=skips(E/(suite+'-rehearsal-skips.txt'));assert new==rehearsal
 old=skips(R/'docs/conformance/k04-calendar-fix/evidence'/(suite+'-skips.txt'))
 assert not set(new)-set(old) and all(old[n]==r for n,r in new.items())
 assert set(old)-set(new)==({'format-date-en152','format-dateTime-en152'} if suite=='qt3' else set())
 for kind in ['filter','projection']:
  actual=[json.loads(line) for line in read(A/(suite+'-'+kind+'.jsonl')).splitlines()]
  expectedRows=[json.loads(line) for line in read(E/(suite+('-prototype.jsonl' if kind=='filter' else '-projected.jsonl'))).splitlines()]
  assert actual==expectedRows
assert 'Experimental-level (XPath40Experimental) passes: 11' in read(A/'qt4.log')
for scope,expected in [('fn-format-date',[208,6,18]),('fn-format-dateTime',[77,2,15])]:
 assert totals(A/(scope+'.log'))==expected
 assert skips(A/(scope+'-skips.txt'))==skips(E/('qt4-'+scope+'-prototype-skips.txt'))
 failures=lambda p:[line for line in read(p).splitlines() if line.startswith('  FAIL ')]
 assert failures(A/(scope+'.log'))==failures(E/('qt4-'+scope+'-prototype.log'))==failures(E/('qt4-'+scope+'-current.log'))
 assert len(failures(A/(scope+'.log')))==expected[1]
for row in load(E/'requests.json'):
 text=read(A/(row['Suite']+'-'+row['Name']+'.log'));m=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',text)
 assert list(map(int,m.groups()))==[1,1,0,0]
checks=load(A/'check-receipts.json');gates=load(A/'gate-receipts.json')
assert len(checks)==10 and len(gates)==2 and all(r['exitCode']==0 and not r['timeout'] for r in checks+gates)
assert '0 Error(s)' in read(A/'build.log')
units=read(A/'solution-tests.log');counts=list(map(int,re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+)',units)));assert len(counts)==10 and sum(counts)==22579 and 'Failed!' not in units
assert re.search(r'Passed:\s+8196,',units)
for name,h in load(A/'binary-identities.json').items():assert sha(R/name)==h,name
validation=load(A/'validation.json');assert validation['ownerApproved'] is True and validation['solutionPassed']==22579 and validation['releaseApproved'] is False
manifest=load(A/'manifest.json')
for name,h in manifest['files'].items():assert sha(A/name)==h,name
check_git_hashes(manifest['documents'])
renders=load(A/'render-receipts.json');assert len(renders['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
print('PASS: exact approved3file CB patch;normalQT3 31164/0/657;qt4 8587/0/1892;exploratory208/6/18 with6failures retained;364controls/40projections/4originals;22579solution tests;historical evidence intact.')
