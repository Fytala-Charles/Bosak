"""Verify optional date calls, qualified promotion, live example updates and historical preservation."""
from pathlib import Path
import json,hashlib,subprocess,re,collections
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3];B=R/'docs/conformance/k04-date-empty-fix/evidence'
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
def hashes(rows):
 actual=subprocess.check_output(['git','hash-object','--stdin-paths'],input=('\n'.join(rows)+'\n').encode(),cwd=R).decode().splitlines();assert actual==list(rows.values())
def totals(p):
 s=read(p);return [int(re.search(r'^'+n+r':\s+(\d+)',s,re.M)[1]) for n in ['Passed','Failed','Skipped']]
def skips(p):
 reason=None;items={}
 for line in read(p).splitlines():
  if line.startswith('=== '):reason=line.split(' skipped: ',1)[1].rsplit(' ===',1)[0]
  elif line.strip():assert reason and line not in items;items[line]=reason
 return items
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
for name,h in load(E/'historical-catalog-identities.json').items():assert sha(R/name)==h,name
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['date']=='2026-10-28' and pin['specification']['revision']=='e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e'
assert pin['specification']['sha256']=='409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5'
allowed={'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','src/Directory.Build.props','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs','tests/Bosak.XPath.Standard.Tests/DateFormattingOptionalTests.cs'}
changed=set(git('diff',pin['sourceBase'],'--name-only','--','src','tests','AGENTS.md').splitlines());assert changed<=allowed and len(changed)>=3
assert not git('diff',pin['sourceBase'],'--','AGENTS.md','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs')
hashes(load(E/'source-identities.json'));assert '<Version>1.29.0</Version>' in read(R/'src/Directory.Build.props')
frozen=load(R/'docs/conformance/k04-calendar-admission/evidence/REVIEW-PIN.json')
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');assert git('-C',str(root),'rev-parse','HEAD')==frozen['fixtureRevisions'][suite]
 assert not git('-C',str(root),'status','--porcelain')
for name,h in load(R/'docs/conformance/k04-date-arity-review/evidence/fixture-hashes.json').items():assert sha(R/name)==h,name
source=read(R/'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs')
for name in ['format-date','format-dateTime','format-time']:
 arities=re.findall(r'\[\(Namespaces.Fn, "'+name+r'", (\d+)\)\] = new',source);assert arities==['2','3','4','5'],(name,arities)
 for arity in [3,4]:
  a=source.index('[(Namespaces.Fn, "'+name+'", '+str(arity)+')] = new');b=source.index('            },',a);assert 'IsXPath40Only = true' in source[a:b] and 'ReturnTypeName = "xs:string?"' in source[a:b]
 assert '[(Namespaces.Fn, "'+name+'")] = new(["value", "picture", "language", "calendar", "place"], [null, null, "()", "()", "()"])' in source
runnerPath='tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs'
def gates(s):
 a=s.index('private static readonly HashSet<string> GatedSets');b=s.index('    };',a);return set(re.findall(r'^\s*"([^"]+)"',s[a:b],re.M))
old=gates(subprocess.check_output(['git','show',pin['sourceBase']+':'+runnerPath],cwd=R).decode());new=gates(read(R/runnerPath));added={'fn-format-date','fn-format-dateTime','fn-format-time'}
assert len(old)==226 and len(new)==229 and new-old==added and not old-new
qualification=load(E/'promotion-qualification.json');assert {r['name'] for r in qualification['sets']}==added and all(r['failed']==0 and r['passed']>0 for r in qualification['sets'])
assert re.search(r'Failed:\s+276, Passed:\s+162,.*Total:\s+438,',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+0, Passed:\s+1167,.*Total:\s+1167,',read(E/'regressions-after.log'))
assert 'duplicate ID' not in read(E/'regressions-before.log') and 'duplicate ID' not in read(E/'regressions-after.log')
assert re.search(r'Failed:\s+0, Passed:\s+18,',read(E/'catalog-current-after.log'))
for name in ['prepromotion-build.log','build.log','final-build.log','driver-build.log']:assert '0 Error(s)' in read(E/name)
units=read(E/'solution-tests.log');counts=list(map(int,re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+)',units)));assert len(counts)==10 and sum(counts)==23365 and 'Failed!' not in units
assert re.search(r'Passed:\s+8196,',units)
assert totals(E/'qt3.log')==[31164,0,657] and totals(E/'qt4.log')==[8895,0,1914]
oldSkips=skips(B/'qt3-skips.txt');assert oldSkips==skips(E/'qt3-skips.txt')
oldSkips=skips(B/'qt4-skips.txt');newSkips=skips(E/'qt4-skips.txt');assert not set(oldSkips)-set(newSkips) and all(newSkips[n]==v for n,v in oldSkips.items())
expectedAdded=skips(E/'fn-format-date-skips.txt')|skips(E/'fn-format-time-skips.txt');assert len(expectedAdded)==22 and {n:v for n,v in newSkips.items() if n not in oldSkips}==expectedAdded
assert 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4.log')
for scope,expected in [('fn-format-date',[214,0,18]),('fn-format-time',[94,0,4])]:
 assert totals(E/(scope+'.log'))==expected and totals(E/('prepromotion-'+scope+'.log'))==expected
 assert skips(E/(scope+'-skips.txt'))==skips(B/(scope+'-skips.txt'))==skips(E/('prepromotion-'+scope+'-skips.txt'))
rows=load(E/'probe-receipts.json');assert len(rows)==104 and all(r['result']['kind']=='Passed' for r in rows)
for row in rows:assert json.loads(read(E/row['log']).splitlines()[-1])==row['result']
receipts=rows+load(E/'normal-set-receipts.json')+load(E/'gate-receipts.json');assert len(receipts)==108 and all(r['exitCode']==0 and not r['timeout'] for r in receipts)
# Only the twelve live result rows change; expressions, 3.1 boundaries and dated acceptance stay fixed.
update=load(E/'catalog-expectation-update.json');before=json.loads(subprocess.check_output(['git','show',update['sourceBase']+':'+update['manifest']],cwd=R).decode('utf-8-sig'));after=load(R/update['manifest']);changes={r['id']:r for r in update['updates']};assert len(changes)==12 and len(before)==len(after)
for oldRow,newRow in zip(before,after):
 assert oldRow['id']==newRow['id']
 if oldRow['id'] not in changes:assert oldRow==newRow;continue
 record=changes[oldRow['id']];assert oldRow==record['before'] and newRow==record['after']
 file=R/'docs/xpath40-catalog/examples'/newRow['file'];assert sha(file)==record['fileSha256']
 if newRow['id'].startswith('changed31-inventory-'):assert newRow['expected']=='1' and newRow['baselineExpected']=='0'
 else:assert newRow['expected'] in ['3','4'] and newRow['baselineError']=='XPST0017' and 'error' not in newRow
for name,h in load(E/'binary-identities.json').items():assert sha(R/name)==h,name
validation=load(E/'validation.json');assert validation['solutionPassed']==23365 and validation['gatedSets']==229 and validation['releaseApproved'] is False
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
hashes(manifest['documents'])
renders=load(E/'render-receipts.json');assert len(renders['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
print('PASS:438new/1167focused;8originals/96controls;23365solution tests;QT3 31164/0/657;qt4 8895/0/1914 with229sets;12live rows updated;old skips and historical acceptance preserved.')
