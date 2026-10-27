"""Validate K02's public diagnostic correction without claiming schema capability."""
from pathlib import Path
import json,hashlib,re,subprocess,sys,xml.etree.ElementTree as ET
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
adoption=load(C/'ADOPTION.json');assert adoption['recordedBeforeImplementation'] and adoption['revision']=='W3C Recommendation 21 March 2017' and 'REC-xquery-31-20170321' in adoption['immutableSpecification']
for n,p,f in [('baseline.trx',5,4),('fixed.trx',9,0)]:
 rows=[v for v in ET.parse(E/n).getroot().iter() if v.tag.endswith('UnitTestResult')];assert len(rows)==9 and sum(v.get('outcome')=='Passed' for v in rows)==p and sum(v.get('outcome')=='Failed' for v in rows)==f
assert '0 Error(s)' in log('build.log') and '<Version>1.26.5</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
runs=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',log('solution-tests.log'))
assert len(runs)==10 and len({n for f,p,s,n in runs})==10 and all(int(f)==int(s)==0 for f,p,s,n in runs) and sum(int(p) for f,p,s,n in runs)==21825
for n,p,s in [('qt3-gate.log',31144,677),('qt4-gate.log',8436,1894)]:
 text=log(n)
 for key,count in [('Passed',p),('Failed',0),('Skipped',s)]:assert re.search(r'^'+key+r':\s+'+str(count)+r'\s*$',text,re.M)
assert 'Experimental-level (XPath40Experimental) passes: 11' in log('qt4-gate.log')
fixture=load(E/'fixture.json')
for path,key in [('setFile','setSha256'),('schemaFile','schemaSha256')]:assert sha((R/fixture[path]).read_bytes())==fixture[key]
assert 'XQST0012' in fixture['case'] and 'assert-xml' in fixture['case']
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==adoption['qt3FixtureRevision']
assert not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
before=load(E/'public-baseline.json');after=load(E/'public-fixed.json')
assert before['phase']==after['phase']=='evaluate' and before['result'] is after['result'] is None
assert 'XQST0012' not in before['error'] and 'XQST0012: Schema compilation failed:' in after['error']
assert after['error'].replace('XQST0012: ','',1)==before['error']
executorBefore=json.loads(log('executor-baseline.log').strip().splitlines()[-1]);executorAfter=json.loads(log('executor-fixed.log').strip().splitlines()[-1])
assert executorBefore==executorAfter and executorAfter['kind']=='HarnessException' and executorAfter['exceptionType']=='System.Xml.Schema.XmlSchemaException'
old=load(R/'docs/conformance/k01-schema-list/baseline-adoption/skip-delta.json')['current'];fresh={};reason=None
for line in (E/'qt3-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 match=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if match:reason=match.group(2)
 elif line.strip():assert line not in fresh and reason is not None;fresh[line]=reason
assert fresh==old and len(fresh)==677 and 'validateexpr-28' in fresh
for n in ['AGENTS.md','tests/Bosak.XPath.Conformance/ConformanceRunner.cs','tests/Bosak.XPath.Conformance/TestEnvironment.cs','tests/Bosak.XPath.Conformance/TestExecutor.cs','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs']:
 assert git('rev-parse',m['sourceBase']+':'+n).decode().strip()==git('hash-object',n).decode().strip(),n
changed=set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines())|set(git('ls-files','--others','--exclude-standard','--','tests').decode().splitlines())
assert changed=={'src/Bosak.XQuery/Api/XQueryExecutable.cs','src/Directory.Build.props','tests/Bosak.XQuery.Tests/SchemaImportErrorTests.cs'},changed
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
for v in load(E/'binary-identities.json'):assert sha(Path(v['path']).read_bytes())==v['sha256']
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h
render=load(E/'render-receipts.json');assert len(render['pages'])==8 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;t=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',t) if n=='docs/conformance/k02-schema-duration/README.md' else re.findall(r'\[Duration-schema investigation\]\(([^)]+)\)',t)
 for target in targets:
  if not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target) and not target.startswith('#'):assert (p.parent/target.split('#')[0]).exists(),(n,target)
print('K02 bounded correction verified:4 baseline failures fixed;9 regressions and21,825 solution passes;public XQST0012 with details retained;normal case still skipped;all677 skip reasons and both gates unchanged.')
