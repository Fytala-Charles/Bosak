"""Verify C42's bounded existing-contract correction and unchanged admission policy."""
from pathlib import Path
import json,hashlib,re,subprocess,collections,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];P=R/'docs/conformance/k04-schema-profile-review/evidence';checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def lines(p):return [json.loads(s) for s in read(p).splitlines()]
def check(ok,label):
 assert ok,label
 checks.append(label)
def gitobject(p):return subprocess.check_output(['git','hash-object','--',p],cwd=R,text=True).strip()
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 result={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:
   assert s.strip() not in result
   result[s.strip()]=reason
 return result
def trx(p):
 root=X.parse(p).getroot();n={'t':root.tag.split('}')[0][1:]};return root,n,root.find('t:ResultSummary/t:Counters',n).attrib
pin=load(E/'IMPLEMENTATION-PIN.json');check(pin['baseCommit'].startswith('2e364d66') and pin['candidateVersion']=='1.33.1' and len(pin['sources'])==2,'pre-implementation immutable adoption and policy pin')
for p,h in load(E/'previous-identities.json').items():check(sha(R/p)==h,'preserved previous evidence '+p)
for p,h in load(P/'reviewed-resource-identities.json').items():check(sha(R/p)==h,'unchanged fixture resource '+p)
for p,h in load(E/'before-source-identities.json').items():
 if p not in ['src/Bosak.XPath.Runtime/Vm/VmEngine.cs','src/Directory.Build.props']:check(gitobject(p)==h,'unchanged harness/policy '+p)
allowed={'src/Bosak.XPath.Runtime/Vm/VmEngine.cs','src/Directory.Build.props'}
changed=set(subprocess.check_output(['git','diff',pin['baseCommit'],'--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines())
check(changed<=allowed|{'tests/Bosak.XQuery.Tests/SchemaElementMembershipTests.cs'},'implementation scope limited to runtime, version and regression tests')
check('<Version>1.33.1</Version>' in read(R/'src/Directory.Build.props'),'unpublished patch candidate')
for suite,rev in pin['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests');check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==rev,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
for name in ['cohorts.json','diagnostic-originals.json','review-names.json','polarity-requests.json']:
 check(load(E/name)==load(P/name),'identical cohort request '+name)
check(lines(E/'qt3-catalog.jsonl')==lines(P/'qt3-catalog.jsonl') and len(lines(E/'qt3-catalog.jsonl'))==31821,'complete 31,821-case actual admission inventory unchanged')
check(lines(E/'qt3-originals.jsonl')==lines(P/'qt3-originals.jsonl'),'all 52 original queries/assertions/environments/resources unchanged')
c=load(E/'cohorts.json');legacy=set(c['legacy']);errors=set(c['schemaPreparationErrors']);companions={x['companion'] for x in c['pairs']}
diag=load(E/'diagnostic-receipts.json');oldDiag=load(P/'diagnostic-receipts.json');actual={x['case']:x['result'] for x in diag if x['mode']=='actual'};old={x['case']:x['result'] for x in oldDiag if x['mode']=='actual'}
changed={n for n in actual if actual[n]!=old[n]};repaired={'substitution-003','substitution-010','substitution-011','substitution-030'}
check(changed==repaired and all(old[n]['kind']=='Failed' and actual[n]['kind']=='Passed' for n in repaired),'exact four repaired originals; other 48 outcomes identical')
check(collections.Counter(actual[n]['kind'] for n in legacy)=={'Passed':19,'Failed':18},'legacy cohort 19 direct passes / 18 retained counter-profile failures')
check(all(actual[n]['kind']=='Passed' for n in companions),'all nine exact-query companions pass')
check(all(actual[n]['kind']=='HarnessException' and actual[n]['exceptionType']=='System.Xml.Schema.XmlSchemaException' and 's:H1 s:H2' in actual[n]['message'] for n in errors),'all six multi-affiliation preparation boundaries retained')
check({x['case']:x['result'] for x in diag if x['mode']=='resources'}=={x['case']:x['result'] for x in oldDiag if x['mode']=='resources'},'all actual environment/resource observations unchanged')
ledger=load(E/'cohort-ledger.json');check(len(ledger)==52 and {x['Name'] for x in ledger}==set(actual),'complete per-case before/after disposition ledger')
for x in ledger:check(x['Before']==old[x['Name']] and x['After']==actual[x['Name']],'original result delta '+x['Name'])
check(load(E/'framework-results.json')==load(P/'framework-results.json'),'unchanged framework single-head/preparation contrast')
matching=load(E/'matching-results.json');check(len(matching['nodeControls'])==10 and all(x['passed'] and x['error'] is None and x['schemaValidated'] for x in matching['nodeControls']) and matching['functionControl']['passed'],'all eleven unchanged independent controls pass')
for n in ['matching-program.cs.txt','framework-program.cs.txt','executor.cs.txt','inventory-program.cs.txt']:check(read(E/n)==read(P/n),'same independent executor/probe '+n)
processes=diag+[load(E/'inventory-receipt.json'),load(E/'framework-receipt.json'),load(E/'matching-receipt.json')]+load(E/'gate-receipts.json');check(len(processes)==109 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'109 final bounded processes exit successfully without timeout')
check(totals(E/'qt3-normal.log')==[31397,0,424] and totals(E/'qt4-normal.log')==[8941,0,1918],'fresh exact complete normal gates')
for n in ['qt3','qt4']:check(skips(E/(n+'-normal-skips.txt'))==skips(P/(n+'-normal-skips.txt')),'all '+n+' retained skip names/reasons identical')
a=skips(E/'qt3-normal-skips.txt');check(all(a[n]=='Unsupported dependency' for n in legacy) and all(a[n].startswith('Harness error: XmlSchemaException') for n in errors),'profile exclusions remain distinct from preparation errors')
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'unchanged 230 gated sets and eleven experimental passes')
check(read(R/'tests/Bosak.XQuery.Tests/SchemaElementMembershipTests.cs')==read(R/'.guard-tmp/k04-schema-element-baseline/SchemaElementMembershipTests.cs'),'identical final regression source before/after')
for name,expected in [('baseline-results/baseline-final.trx',(39,22,17)),('focused-results/focused-final.trx',(39,39,0))]:
 root,n,counters=trx(E/name);check(tuple(int(counters[k]) for k in ['total','passed','failed'])==expected,'exact test outcomes '+name)
before,ns,_=trx(E/'baseline-results/baseline-final.trx');after,ans,_=trx(E/'focused-results/focused-final.trx')
check({x.attrib['testName'] for x in before.findall('t:Results/t:UnitTestResult',ns)}=={x.attrib['testName'] for x in after.findall('t:Results/t:UnitTestResult',ans)},'all 39 identical public regression cases before/after')
clean,cleanNs,cleanCounters=trx(E/'baseline-results/baseline-clean.trx');check(tuple(int(cleanCounters[k]) for k in ['total','passed','failed'])==(39,22,17),'cleaned identical source still has 17 failures on preserved libraries')
current,currentNs,currentCounters=trx(E/'xquery-results/xquery-final.trx');check(currentCounters['passed']=='804' and currentCounters['failed']=='0','final full XQuery project confirms all 804 tests after insignificant XML whitespace cleanup')
regressions=[x for x in current.findall('t:Results/t:UnitTestResult',currentNs) if 'SchemaElementMembershipTests.' in x.attrib['testName']];check(len(regressions)==39 and all(x.attrib['outcome']=='Passed' for x in regressions),'all 39 final source regressions independently pass')
source=load(E/'regression-source-identities.json');check(gitobject(source['currentSource']['path'])==source['currentSource']['gitObject'] and sha(R/source['baselineCopy']['path'])==source['baselineCopy']['sha256'],'same final regression source identities')
for p,h in load(E/'baseline-binary-identities.json').items():check(sha(R/p)==h,'preserved old binary '+p)
for p,h in load(E/'binary-identities.json').items():check(sha(R/p)==h,'executed current binary '+p)
solution=load(E/'solution-receipt.json');check(solution['exitCode']==0 and solution['projects']==10 and solution['passed']==24203 and solution['failed']==0,'fresh complete final solution: 24,203 passes / ten projects')
check(len(re.findall(r'Passed!\s+- Failed:\s+0, Passed:',read(E/'solution-tests.log')))==10 and sum(map(int,re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+)',read(E/'solution-tests.log'))))==24203,'solution log reconciles to receipt')
check('0 Error(s)' in read(E/'solution-build.log') and 'Build succeeded.' in read(E/'solution-build.log'),'full Release build succeeds')
snapshot=read(P/'core-req139-snapshot.txt');check(snapshot in read(R/'docs/FEATURE_REQUESTS.md') and '- [ ] ADR-006 accepted' in snapshot,'REQ139 acceptance draft remains unchanged/pending')
for name in ['README.md','docs/ARCHITECTURE.md','docs/FEATURE_REQUESTS.md','docs/INTEGRATION.md','docs/AGENT_HANDOVER.md','docs/DOCUMENTATION_RENDERER_TEST.md','docs/xpath40-catalog/CONFORMANCE-SKIP-TRIAGE.md','docs/conformance/k04-dependency-audit/README.md']:check('K04-C42 schema-element repair' in read(R/name),'canonical documentation sync '+name)
render=load(E/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders with unchanged Fytala CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw evidence '+p)
 for p,h in m['gitObjects'].items():check(gitobject(p)==h,'normalized '+p)
print(json.dumps(dict(status='passed',checks=len(checks),regressions=39,previouslyFailing=17,legacyDirectPasses=19,legacyRetainedFailures=18,currentProfileControls=11,solutionPassed=24203,normalBaselineChanged=False)))
