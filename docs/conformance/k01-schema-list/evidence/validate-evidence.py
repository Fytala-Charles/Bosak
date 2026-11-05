"""Validate the scoped K01 correction without changing frozen skip admission."""
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
for n in m['previousFiles']:
 if n.endswith('/manifest.json'):
  prior=R/n
  for file,value in load(prior)['files'].items():
   expected=value['sha256'] if isinstance(value,dict) else value
   assert sha((prior.parent/file).read_bytes())==expected,str(prior.parent/file)
adoption=load(C/'ADOPTION.json');assert adoption['recordedBeforeImplementation'] is True and adoption['revision']=='W3C Recommendation 21 March 2017' and 'REC-xquery-31-20170321' in adoption['immutableSpecification']
fixture=load(E/'fixture.json');assert fixture['revision']==adoption['qt3FixtureRevision']==git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()
assert sha((R/fixture['file']).read_bytes())==fixture['sha256'] and set(fixture['cases'])=={'CastAs-ListType-20','CastAs-ListType-28'}
assert not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
for n,passed,failed in [('baseline.trx',12,6),('fixed.trx',18,0)]:
 rows=[x for x in ET.parse(E/n).getroot().iter() if x.tag.endswith('UnitTestResult')]
 assert len(rows)==18 and sum(x.get('outcome')=='Passed' for x in rows)==passed and sum(x.get('outcome')=='Failed' for x in rows)==failed
assert '0 Error(s)' in log('build.log') and '<Version>1.26.4</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
suite=log('solution-tests.log');runs=re.findall(r'Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+).*?- (Bosak\.[^ ]+\.dll)',suite)
assert len(runs)==10 and all(int(f)==int(s)==0 for f,p,s,n in runs) and sum(int(p) for f,p,s,n in runs)==21816
assert len({n for f,p,s,n in runs})==10
for n,p,s in [('qt3-gate.log',31142,679),('qt4-gate.log',8436,1894)]:
 text=log(n);assert re.search(r'Passed:\s+'+str(p)+r'\b',text) and re.search(r'Failed:\s+0\b',text) and re.search(r'Skipped:\s+'+str(s)+r'\b',text)
assert 'Experimental-level (XPath40Experimental) passes: 11' in log('qt4-gate.log')
results=load(E/'executor-results.json');assert len(results)==4 and all(not v['timeout'] and v['exitCode']==0 for v in results)
assert {(v['binary'],v['name'],v['result']['kind']) for v in results}=={('released','CastAs-ListType-20','Failed'),('released','CastAs-ListType-28','Passed'),('candidate','CastAs-ListType-20','Passed'),('candidate','CastAs-ListType-28','Passed')}
# Preserve policy and admission source exactly; proposed unskip totals are not executed totals.
for n in ['AGENTS.md','tests/Bosak.XPath.Conformance/ConformanceRunner.cs','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs']:
 assert git('rev-parse',m['sourceBase']+':'+n).decode().strip()==git('hash-object',n).decode().strip(),n
assert load(C/'RESULTS.json')['baselinePolicyApproval'] is None
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
for v in load(E/'binary-identities.json'):assert sha(Path(v['path']).read_bytes())==v['sha256']
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h
for n in m['markdownFiles']:
 p=R/n;t=p.read_text(encoding='utf-8')
 targets=re.findall(r'\]\(([^)]+)\)',t) if n.startswith('docs/conformance/k01-schema-list/') else re.findall(r'\[Correction and evidence\]\(([^)]+)\)',t)
 for target in targets:
  if not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target) and not target.startswith('#'):assert (p.parent/target.split('#')[0]).exists(),(n,target)
print('K01 verified:6 baseline failures corrected;18 focused and21,816 solution passes;both original probes pass;QT3 31,142/0/679 and qt4 8,436/0/1,894 unchanged;two-skip baseline approval pending.')
