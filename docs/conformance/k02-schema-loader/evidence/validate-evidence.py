"""Validate the default-off schema diagnostic mode and preserved experimental evidence."""
from pathlib import Path
import json,hashlib,re,subprocess,sys
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,n
for n,h in m['sourceFiles'].items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['previousFiles'].items():assert sha((R/n).read_bytes())==h,n
for n,v in load(E/'source-identities.json').items():assert git('rev-parse',m['sourceBase']+':'+n).decode().strip()==v['gitBlob']
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['recordedBeforeImplementation'] and pin['revision']=='W3C Recommendation 21 March 2017'
assert git('rev-parse',pin['sourceBase']).decode().strip()==m['sourceBase']
before=sha((C/'RESULTS.json').read_bytes());subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha((C/'RESULTS.json').read_bytes())==m['reviewSha256']
review=load(C/'RESULTS.json');s=review['summary'];assert s['implementedProcesses']==66 and s['implementedExecutorCalls']==68 and s['allDiagnosticProcesses']==196 and not s['normalAdmissionChanged']
for n in ['implementation-build-fixed.log','final-harness-build.log','driver-build.log']:assert '0 Error(s)' in log(n)
runs=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
assert len(runs)==10 and len({n for f,p,s,n in runs})==10 and all(int(f)==int(s)==0 for f,p,s,n in runs) and sum(int(p) for f,p,s,n in runs)==21825
for n,passed,skipped in [('qt3-final-gate.log',31144,677),('qt4-gate.log',8436,1894)]:
 for key,count in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+key+r':\s+'+str(count)+r'\s*$',log(n),re.M)
assert 'Experimental-level (XPath40Experimental) passes: 11' in log('qt4-gate.log')
fresh={};reason=None
for line in (E/'qt3-final-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 match=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if match:reason=match.group(2)
 elif line.strip():assert line not in fresh and reason is not None;fresh[line]=reason
assert fresh==load(R/'docs/conformance/k01-schema-list/baseline-adoption/skip-delta.json')['current'] and len(fresh)==677
for n,h in load(E/'fixture-identities.json').items():assert sha((R/n).read_bytes())==h,n
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(E/'DESIGN-PIN.json')['fixtureRevision'] and not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
for resultFile in ['v1-results.json','results.json','actual-results.json','implementation-results.json']:
 for v in load(E/resultFile):
  path=E/(('v1-' if resultFile=='v1-results.json' else '')+v['log']);actual=json.loads(path.read_text(encoding='utf-8-sig').splitlines()[-1]);assert actual==v['result']
  assert v['exitCode']==0 and not v['timeout'] and v['watchdogSeconds']==20
expected={'src/Bosak.XQuery/Api/XQueryExecutable.cs','src/Bosak.XQuery/Bosak.XQuery.csproj','src/Directory.Build.props','tests/Bosak.XPath.Conformance/TestExecutor.cs','tests/Bosak.XPath.Conformance/TestEnvironment.cs'}
assert set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines())==expected
for n in ['AGENTS.md','tests/Bosak.XPath.Conformance/ConformanceRunner.cs','tests/Bosak.XPath.Conformance/ResultComparer.cs','tests/Bosak.XPath.Conformance/DependencyFilter.cs','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs']:
 assert git('rev-parse',m['sourceBase']+':'+n).decode().strip()==git('hash-object',n).decode().strip(),n
executor=(R/'tests/Bosak.XPath.Conformance/TestExecutor.cs').read_text(encoding='utf-8')
assert 'internal bool UseQueryOwnedSchemasForDiagnostics { get; init; }' in executor and 'if (!UseQueryOwnedSchemasForDiagnostics) return (false, "CurrentMode");' in executor
assert 'unsafe-control' not in executor and 'GetField(' not in executor and 'validateexpr-28' not in executor
environment=(R/'tests/Bosak.XPath.Conformance/TestEnvironment.cs').read_text(encoding='utf-8');assert 'bool deferSchemaCompilation = false' in environment and 'string? XsdVersion = null' in environment
assert '<Version>1.26.6</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
for n in ['src/Bosak.XQuery/Api/XQueryExecutable.cs','tests/Bosak.XPath.Conformance/TestExecutor.cs','tests/Bosak.XPath.Conformance/TestEnvironment.cs']:
 text=(R/n).read_text(encoding='utf-8');assert text.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in text and '27-10-2026' in text
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h,n
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;t=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',t) if n=='docs/conformance/k02-schema-loader/README.md' else re.findall(r'\[Implemented guarded mode\]\(([^)]+)\)',t)
 for target in targets:
  if not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target) and not target.startswith('#'):assert (p.parent/target.split('#')[0]).exists(),(n,target)
if '--local-artifacts' in sys.argv:
 for v in load(E/'binary-identities.json'):assert sha(Path(v['path']).read_bytes())==v['sha256']
 for v in load(E/'synthetic-files.json').values():assert sha(v['text'].encode('utf-8'))==v['sha256']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256']
print('Guarded loader verified:66 actual diagnostic processes/68 calls;all prototype expectations preserved;21,825 solution passes;normal QT3 31,144/0/677 and qt4 8,436/0/1,894 unchanged;mode remains default off.')
