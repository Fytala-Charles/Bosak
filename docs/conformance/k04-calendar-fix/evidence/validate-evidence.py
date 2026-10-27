"""Verify bounded calendar correction results, exact skips and retained historical evidence."""
from pathlib import Path
import subprocess,json,hashlib,re
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
for name,h in load(R/'docs/conformance/k04-negative-capability-review/manifest.json').items():assert sha(R/name)==h,name
for name,h in load(E/'fixture-hashes.json').items():assert sha(R/name)==h,name
pin=load(E/'IMPLEMENTATION-PIN.json');verified=load(E/'specification-verification.json')
assert verified['datedRecommendationSha256']==pin['specifications'][0]['sha256']
assert verified['qt4SourceSha256']==pin['specifications'][1]['sha256']
assert len(pin['listedDesignators'])==28
for name,h in load(E/'source-identities.json').items():assert git('hash-object',name)==h,name
changed=set(git('diff',pin['sourceBase'],'--name-only','--','src','tests').splitlines())
assert changed<={'src/Bosak.XPath.Standard/Functions/FormatDateTimeEngine.cs','src/Directory.Build.props','tests/Bosak.XPath.Standard.Tests/CalendarFallbackTests.cs'},changed
for name in ['AGENTS.md','tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/DependencyFilter40.cs','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs']:
 assert not git('diff',pin['sourceBase'],'--',name),name
assert '<Version>1.28.1</Version>' in read(R/'src/Directory.Build.props')
assert '0 Error(s)' in read(E/'build.log')
units=read(E/'solution-tests.log');counts=list(map(int,re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+)',units)));assert len(counts)==10 and sum(counts)==22579 and 'Failed!' not in units
assert re.search(r'Passed:\s+8196,',units)
assert re.search(r'Failed:\s+249, Passed:\s+132,',read(E/'regressions-before.log'))
assert re.search(r'Failed:\s+9, Passed:\s+372,',read(E/'regressions-after.log'))
assert re.search(r'Failed:\s+0, Passed:\s+381,',read(E/'regressions-final.log'))
initial=load(R/'docs/conformance/k04-calendar-review/execution.json');final=load(E/'execution.json');assert len(final)==58
assert {r['name'] for r in initial}=={r['name'] for r in final}
assert all(r['exitCode']==0 and not r['timeout'] and r['watchdogSeconds']==20 and r['result']['kind']=='Passed' for r in final)
for row in final:assert json.loads(read(E/(row['name']+'-output.txt')).splitlines()[-1])==row['result']
def skips(p):
 reason=None;items={}
 for line in read(p).splitlines():
  if line.startswith('=== '):reason=line.split(' skipped: ',1)[1].rsplit(' ===',1)[0]
  elif line.strip():assert reason and line not in items;items[line]=reason
 return items
for suite,expected in [('qt3',[31162,0,659]),('qt4',[8587,0,1892])]:
 assert skips(E/(suite+'-skips.txt'))==skips(R/'docs/conformance/k04-sequence-admission/adoption'/(suite+'-skips.txt'))
 text=read(E/(suite+'.log'));assert [int(re.search(r'^'+k+r':\s+(\d+)',text,re.M)[1]) for k in ['Passed','Failed','Skipped']]==expected
assert 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4.log')
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
for name,h in manifest['documents'].items():assert git('hash-object',name)==h,name
renders=load(E/'render-receipts.json');assert len(renders['pages'])==9 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
assert load(E/'validation.json')['solutionPassed']==22579
print('PASS:381regressions/58diagnostics;22579solution tests;QT3 31162/0/659 and qt4 8587/0/1892;exact skips and historical evidence preserved.')
