"""Verify C37 implementation scope, identical originals, guarded baselines and new regressions."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def check(ok,label):
 assert ok,label
 checks.append(label)
def lines(p):return [json.loads(x) for x in read(p).splitlines()]
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:check(s.strip() not in out,'unique skip '+s.strip());out[s.strip()]=reason
 return out
C36=R/'docs/conformance/k04-pragma-review/evidence'
for name in ['previous-identities.json','fixture-identities.json','baseline-binary-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
scope=load(E/'source-identities.json')
for p,h in scope.items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'source '+p)
changed=set(subprocess.check_output(['git','diff','5ba5528c','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines())
changed.update(subprocess.check_output(['git','ls-files','--others','--exclude-standard','--','src','tests'],cwd=R,text=True).splitlines())
check(changed==set(scope),'exact eleven-path implementation scope')
for p in ['AGENTS.md','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40']:
 check(not subprocess.check_output(['git','diff','5ba5528c','--name-only','--',p],cwd=R,text=True).strip(),'unchanged normal policy '+p)
for suite,revision in load(C36/'REVIEW-PIN.json')['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
for phase in ['normal','prototype']:
 check(lines(E/(phase+'-catalog.jsonl'))==lines(C36/(phase+'-catalog.jsonl')),'complete identical 31821-case routing/dependency inventory '+phase)
 check(lines(E/(phase+'-originals.jsonl'))==lines(C36/(phase+'-originals.jsonl')),'unchanged original expressions/assertions/resources '+phase)
 check(lines(E/(phase+'-controls.jsonl'))==lines(C36/(phase+'-controls.jsonl')),'sixteen lexical controls '+phase)
for name,expected in [('qt3-normal',[31340,0,481]),('qt3-prototype',[31395,0,426]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,'exact '+name)
a=skips(E/'qt3-normal-skips.txt');b=skips(E/'qt3-prototype-skips.txt');q=skips(E/'qt4-normal-skips.txt')
check(a==skips(C36/'qt3-normal-skips.txt') and q==skips(C36/'qt4-normal-skips.txt'),'both normal maps unchanged')
check(b==skips(C36/'qt3-prototype-skips.txt') and len(a)==481 and len(b)==426 and len(set(a)-set(b))==55,'exact isolated 55 removals / 426 retained reasons unchanged')
rows=load(E/'original-receipts.json');check(len(rows)==180,'60 originals / 180 resource-and-executor processes')
for phase,expected in [('normal',{'Passed':11,'Failed':9,'Skipped':40}),('prototype',{'Passed':59,'Failed':1})]:
 r=[x for x in rows if x['mode']=='actual' and x['phase']==phase];check(len(r)==60 and collections.Counter(x['result']['kind'] for x in r)==expected,'exact original outcomes '+phase)
failed=[x for x in rows if x['mode']=='actual' and x['phase']=='prototype' and x['result']['kind']=='Failed']
check(len(failed)==1 and failed[0]['case']=='K-ExtensionExpression-4' and failed[0]['case'] in b,'only old exact-XQ10 no-namespace expectation remains failed and excluded')
eligible=set(a)-set(b);check(all(x['result']['kind']=='Passed' for x in rows if x['mode']=='actual' and x['phase']=='prototype' and x['case'] in eligible),'all 55 eligible original assertions pass')
controls=load(E/'control-receipts.json');check(len(controls)==24 and all(x['result']['kind']=='Passed' for x in controls),'all 24 original public controls pass')
oldcontrols=load(C36/'control-receipts.json')
check({x['name']:(x['result']['expression'],x['result']['assertion']) for x in controls}=={x['name']:(x['result']['expression'],x['result']['assertion']) for x in oldcontrols},'public control inputs/assertions unchanged')
processes=rows+controls+load(E/'gate-receipts.json')+load(E/'inventory-receipts.json');check(len(processes)==209 and all(not x['timeout'] and x['exitCode']==0 for x in processes),'209 bounded successful diagnostic process exits')
check(totals(E/'regressions-after.log')==[82,0,0] and totals(E/'regressions-before.log')==[27,55,0],'same 82-test old/new cohort')
test=R/'tests/Bosak.XQuery.Tests/PragmaExpressionTests.cs';check(sha(test)==sha(R/'.guard-tmp/k04-pragma-fallback-baseline/PragmaExpressionTests.cs')==sha(E/'qualified-test-source.cs.txt'),'identical qualified test source')
for p,h in load(E/'baseline-binary-identities.json').items():
 output=R/'.guard-tmp/k04-pragma-fallback-baseline/bin/Release/net10.0'/Path(p).name
 check(sha(output)==h,'executed baseline DLL '+output.name)
s=read(E/'solution-tests.log');tests=re.findall(r'Passed!\s+- Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+)',s)
check(len(tests)==10 and sum(int(x[1]) for x in tests)==24164 and all(x[0]==x[2]=='0' for x in tests),'24164 fresh solution tests / ten projects')
check(load(E/'solution-receipt.json')['exitCode']==0 and '0 Error(s)' in read(E/'build.log'),'full solution process/build pass')
check('<Version>1.33.0</Version>' in read(R/'src/Directory.Build.props'),'unpublished feature version 1.33.0')
for p in scope:
 if p.endswith('.cs'):
  s=read(R/p);check(s.startswith('// ====') and 'SPDX-License-Identifier: Apache-2.0' in s and '28-10-2026' in s,'complete source header '+p)
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'normal-identical copied harness source '+p)
for p in (R/'.guard-tmp/k04-pragma-fallback-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'byte-identical isolated engine '+p.name)
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'qt4 gate membership and experimental passes unchanged')
render=load(E/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders / unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'solutionPassed':24164,'regressionsPassed':82,'eligibleOriginalsPassed':55,'publicControlsPassed':24,'normalBaselineChanged':False}))
