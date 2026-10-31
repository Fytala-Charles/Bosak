"""Verify the exact unadopted CB proposal, actual controls and complete copied-runner evidence."""
from pathlib import Path
import subprocess,json,re,hashlib,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
def totals(p):
 text=read(p);return [int(re.search(r'^'+k+r':\s+(\d+)',text,re.M)[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 reason=None;items={}
 for line in read(p).splitlines():
  if line.startswith('=== '):reason=line.split(' skipped: ',1)[1].rsplit(' ===',1)[0]
  elif line.strip():assert reason and line not in items;items[line]=reason
 return items
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
pin=load(E/'REVIEW-PIN.json');base=pin['sourceBase'];assert not git('diff',base,'--','src','tests','AGENTS.md')
for name,h in load(E/'normal-source-identities.json').items():assert git('hash-object',name)==h,name
for name,h in load(R/'docs/conformance/k04-calendar-fix/evidence/source-identities.json').items():assert git('hash-object',name)==h,name
assert '<Version>1.28.1</Version>' in read(R/'src/Directory.Build.props')
assert 'exactly **31,162/0/659**' in read(R/'AGENTS.md')
# Reconstruct all approved-on-paper file contents from the immutable normal source base.
sections=read(P/'PROPOSED-ADOPTION.patch').split('--- a/')[1:];names=[]
for section in sections:
 name=section.splitlines()[0];names.append(name);expected=subprocess.check_output(['git','show',base+':'+name],cwd=R).decode('utf-8').replace('\r\n','\n')
 for hunk in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
  lines=hunk.splitlines(True);old=''.join(l[1:] for l in lines if l.startswith((' ','-')));new=''.join(l[1:] for l in lines if l.startswith((' ','+')));assert old in expected;expected=expected.replace(old,new,1)
 if name=='AGENTS.md':assert 'exactly **31,164/0/657**' in expected
 else:
  suite='qt4' if 'Conformance40' in name else 'qt3';assert read(E/(suite+'-prototype-filter.cs.txt'))==expected
assert set(names)=={'AGENTS.md','tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs'}
subprocess.run(['git','apply','--check',str(P/'PROPOSED-ADOPTION.patch')],cwd=R,check=True)
ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'};inventory=load(E/'calendar-inventory.json');counts={};declarations=[]
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');count=0
 assert git('-C',str(root),'rev-parse','HEAD')==pin['fixtureRevisions'][suite]
 assert not git('-C',str(root),'status','--porcelain')
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  path=root/ref.get('file');relative=path.relative_to(R).as_posix();assert sha(path)==inventory['fixtureHashes'][relative];doc=X.parse(path).getroot()
  for case in doc.findall('t:test-case',ns):
   count+=1
   for d in doc.findall('t:dependency',ns)+case.findall('t:dependency',ns):
    if d.get('type')=='calendar':declarations.append(dict(suite=suite,name=case.get('name'),value=d.get('value'),satisfied=d.get('satisfied','true') not in ['false','0'],setFile=relative))
 counts[suite]=count
assert counts==inventory['scannedCases']=={'qt3':31821,'qt4':46985}
assert declarations==inventory['declarations'] and len(declarations)==4
assert all(r['value']=='CB' and not r['satisfied'] for r in declarations)
# Confirm inherited unit validation is source-identical; no fresh unit run is claimed.
inherited=load(R/'docs/conformance/k04-calendar-fix/evidence/validation.json');assert inherited['solutionPassed']==22579 and inherited['regressionRows']==381
controls={r['id']:r for r in load(E/'filter-controls.json')};assert len(controls)==364
changed=0;outcomes=0;projected=0
for suite in ['qt3','qt4']:
 before={r['id']:r for r in map(json.loads,read(E/(suite+'-current.jsonl')).splitlines())};after={r['id']:r for r in map(json.loads,read(E/(suite+'-prototype.jsonl')).splitlines())};assert before.keys()==after.keys()
 for key,row in before.items():
  for field in ['xpathSupported','xquerySupported']:
   if row[field] is not None:outcomes+=1
   if row[field]!=after[key][field]:
    changed+=1;assert any(d['type']=='calendar' and d['value']=='CB' for d in controls[key]['dependencies'])
 for row in map(json.loads,read(E/(suite+'-projected.jsonl')).splitlines()):
  projected+=1;control=controls[row['id']];positive=any(d['type']=='calendar' and d['value']=='CB' and d['satisfied'] for d in control['dependencies'])
  for field in ['xpathSupported','xquerySupported']:
   if row[field] is not None:assert after[row['id']][field]==(not positive and row[field])
assert (changed,outcomes,projected)==(28,538,40)
originals=load(E/'original-receipts.json');assert len(originals)==12
for row in originals:
 if row['mode']=='direct':assert row['result']['kind']=='Passed'
 else:assert row['counts']==([1,0,0,1] if row['mode']=='current' else [1,1,0,0])
baseE=R/'docs/conformance/k04-calendar-fix/evidence'
for suite,expected in [('qt3',[31164,0,657]),('qt4',[8587,0,1892])]:
 assert totals(E/(suite+'-rehearsal.log'))==expected
 old=skips(baseE/(suite+'-skips.txt'));new=skips(E/(suite+'-rehearsal-skips.txt'));assert not set(new)-set(old);assert all(old[n]==v for n,v in new.items())
 assert set(old)-set(new)==({'format-date-en152','format-dateTime-en152'} if suite=='qt3' else set())
assert 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-rehearsal.log')
for scope,oldExpected,newExpected in [('fn-format-date',[206,6,20],[208,6,18]),('fn-format-dateTime',[76,2,16],[77,2,15])]:
 before=E/('qt4-'+scope+'-current.log');after=E/('qt4-'+scope+'-prototype.log');assert totals(before)==oldExpected and totals(after)==newExpected
 failures=lambda p:[line for line in read(p).splitlines() if line.startswith('  FAIL ')]
 assert failures(before)==failures(after) and len(failures(after))==newExpected[1]
 old=skips(E/('qt4-'+scope+'-current-skips.txt'));new=skips(E/('qt4-'+scope+'-prototype-skips.txt'));assert not set(new)-set(old) and all(old[n]==v for n,v in new.items())
 assert set(old)-set(new)==({'format-date-en152','format-dateTime-en152'} if scope=='fn-format-date' else {'format-dateTime-en152'})
mechanism=[json.loads(line) for line in read(E/'mechanism.jsonl').splitlines()];assert len(mechanism)==16
assert all(r['fallbackRequired'] is True for r in mechanism if r['calendar']=='CB')
assert all(r['fallbackRequired'] is False for r in mechanism if r['calendar'] in ['AD','ISO'])
receipts=originals+load(E/'filter-receipts.json')+load(E/'projection-receipts.json')+load(E/'rehearsal-receipts.json')+load(E/'exploratory-receipts.json')+[load(E/'mechanism-receipt.json')]
assert len(receipts)==25 and all(r['exitCode']==0 and not r['timeout'] for r in receipts)
for suite in ['qt3','qt4']:
 for name,row in load(E/'prototype-source-identities.json')[suite].items():
  assert sha(R/row['normal'])==row['normalSha256']
  assert row['executableChange']==(name in ['DependencyFilter.cs','DependencyFilter40.cs'])
  assert (row['normalSha256']!=row['prototypeSha256'])==row['executableChange']
for name,h in load(E/'binary-identities.json').items():assert sha(R/name)==h,name
results=load(P/'RESULTS.json');assert results['ownerApproved'] is False and results['baselineChanged'] is False and results['processes']==25
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
for name,h in manifest['documents'].items():assert git('hash-object',name)==h,name
renders=load(E/'render-receipts.json');assert len(renders['pages'])==10 and renders['cssSha256']==sha(R/'assets/css/fytala-markdown.css')
print('PASS: exact unadopted3file CB patch;78806cases/4declarations;364controls/538outcomes;25bounded processes;QT3 31164/0/657;qt4gate unchanged;6exploratory failures preserved;historical identities intact.')
