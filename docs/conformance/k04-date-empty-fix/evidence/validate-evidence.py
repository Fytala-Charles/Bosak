"""Verify bounded empty-result semantics, complete gates and retained optional-call failures."""
from pathlib import Path
import json,subprocess,hashlib,re,collections
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3];B=R/'docs/conformance/k04-date-arity-review/evidence'
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
for filename in ['previous-identities.json','adoption-identities.json']:
 for name,h in load(E/filename).items():assert sha(R/name)==h,name
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['date']=='2026-10-28' and pin['kind'].startswith('Pre-implementation semantic adoption')
review=load(B/'SPECIFICATION-REVIEW.json');assert pin['specifications'][0]['sha256']==review['xpath31Review']['sha256']
assert pin['specifications'][1]['sha256']==review['mainSource']['sha256'] and pin['specifications'][1]['revision']==review['revision']
allowed={'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','src/Directory.Build.props','tests/Bosak.XPath.Standard.Tests/CalendarFallbackTests.cs','tests/Bosak.XPath.Standard.Tests/DateFormattingEmptyTests.cs'}
changed=set(git('diff',pin['sourceBase'],'--name-only','--','src','tests','AGENTS.md').splitlines());assert changed<=allowed and len(changed)>=3
assert not git('diff',pin['sourceBase'],'--','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40','AGENTS.md')
hashes(load(E/'source-identities.json'))
assert '<Version>1.28.2</Version>' in read(R/'src/Directory.Build.props')
source=read(R/'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs')
for name in ['format-date','format-dateTime','format-time']:
 arities=re.findall(r'\[\(Namespaces.Fn, "'+name+r'", (\d+)\)\] = new',source);assert arities==['2','5'],(name,arities)
a=source.index('            [(Namespaces.Fn, "format-date", 2)]');b=source.index('            // ----- fn:adjust-date-to-timezone',a);assert source[a:b].count('ReturnTypeName = "xs:string?"')==6
assert 'if (value.IsUndefined || IsEmptySequence(value))\n            return XdmValue.Undefined;' in source
for name,h in load(E/'binary-identities.json').items():assert sha(R/name)==h,name
old=load(R/'docs/conformance/k04-calendar-admission/evidence/REVIEW-PIN.json')
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');assert git('-C',str(root),'rev-parse','HEAD')==old['fixtureRevisions'][suite]
 assert not git('-C',str(root),'status','--porcelain')
assert re.search(r'Failed:\s+240, Passed:\s+108,.*Total:\s+348,',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+0, Passed:\s+729,.*Total:\s+729,',read(E/'regressions-after.log'))
assert '0 Error(s)' in read(E/'build.log') and '0 Error(s)' in read(E/'driver-build.log')
units=read(E/'solution-tests.log');counts=list(map(int,re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+)',units)));assert len(counts)==10 and sum(counts)==22927 and 'Failed!' not in units
assert re.search(r'Passed:\s+8196,',units)
for suite,expected in [('qt3',[31164,0,657]),('qt4',[8587,0,1892])]:
 assert totals(E/(suite+'.log'))==expected
 assert skips(E/(suite+'-skips.txt'))==skips(R/'docs/conformance/k04-calendar-admission/adoption'/(suite+'-skips.txt'))
assert 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4.log')
before={r['name']:r for r in load(B/'probe-receipts.json')};after=load(E/'probe-receipts.json');assert len(after)==104
changes=[]
for row in after:
 assert json.loads(read(E/row['log']).splitlines()[-1])==row['result']
 if row['result']!=before[row['name']]['result']:
  assert before[row['name']]['result']['message']=='Unexpected error: XPTY0004' and row['kind']=='control' and row['result']['kind']=='Passed';changes.append(row['name'])
assert len(changes)==30
assert collections.Counter(r['result']['kind'] for r in after if r['kind']=='control')=={'Passed':75,'Failed':21}
assert all(r['result']['kind']=='Failed' and 'XPST0017' in r['result']['message'] for r in after if r['kind']=='original')
for scope,expected in [('fn-format-date',[208,6,18]),('fn-format-time',[92,2,4])]:
 assert totals(E/(scope+'.log'))==expected and skips(E/(scope+'-skips.txt'))==skips(B/(scope+'-skips.txt'))
 failures=lambda p:[l for l in read(p).splitlines() if l.startswith('  FAIL ')]
 assert failures(E/(scope+'.log'))==failures(B/(scope+'.log')) and len(failures(E/(scope+'.log')))==expected[1]
receipts=after+load(E/'normal-set-receipts.json')+load(E/'gate-receipts.json');assert len(receipts)==108 and all(r['exitCode']==0 and not r['timeout'] for r in receipts)
validation=load(E/'validation.json');assert validation['solutionPassed']==22927 and validation['newRegressionRows']==348 and validation['releaseApproved'] is False
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
hashes(manifest['documents'])
renders=load(E/'render-receipts.json');assert len(renders['pages'])==9 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
print('PASS:348new regressions/729focused;22927solution tests;30empty controls corrected/21call gaps retained;QT3 31164/0/657;qt4 8587/0/1892;all skips/history preserved.')
