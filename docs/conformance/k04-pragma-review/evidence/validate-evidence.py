"""Verify read-only C36 skip classification, pragma failures and immutable scope."""
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
for name in ['previous-identities.json','normal-source-identities.json','fixture-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','1b5554b6','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'no production/baseline/fixture change')
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821 and len({x['Set'] for x in normal})==428,'complete 31821 cases / 428 sets')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'actual predicates '+x['Name'])
 for row in [x,y]:
  check(row['xquerySupported']==all(d['xquerySupported'] for d in row['Dependencies']) and row['xpathSupported']==all(d['xpathSupported'] for d in row['Dependencies']),'singleton dependency reconciliation '+row['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'catalog identity '+x['Name'])
affected=lines(E/'normal-affected.jsonl');originals=lines(E/'normal-originals.jsonl');names={x['row']['Name'] for x in originals};deltaNames={x['row']['Name'] for x in affected}
check(len(affected)==57 and len(originals)==60 and collections.Counter(x['row']['Set'] for x in originals)=={'prod-ExtensionExpr':57,'prod-EQName':1,'prod-ValidateExpr':1,'misc-CombinedErrorCodes':1},'57 routing deltas / 60 complete originals')
check(lines(E/'prototype-originals.jsonl')==[dict(x,row=dict(x['row'],actual=True)) for x in originals],'prototype original identities retained')
excluded={x['row']['Name'] for x in affected if not x['row']['xquerySupported']};check(excluded=={'K-ExtensionExpression-4','XQST0079'},'two independent exact-version exclusions')
for name,expected in [('qt3-normal',[31340,0,481]),('qt3-prototype',[31348,47,426]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,'exact '+name)
a=skips(E/'qt3-normal-skips.txt');b=skips(E/'qt3-prototype-skips.txt');q=skips(E/'qt4-normal-skips.txt');A=R/'docs/conformance/k04-construction-admission/adoption'
check(a==skips(A/'qt3-normal-skips.txt') and q==skips(A/'qt4-normal-skips.txt'),'normal approved maps unchanged')
check(len(b)==426 and set(a)-set(b)==deltaNames-excluded and set(b)<=set(a) and all(a[k]==v for k,v in b.items()),'exact 55 isolated removals / 426 reasons retained')
ledger=load(E/'remaining-skips.json');check(len(ledger)==481 and {x['Name']:x['Reason'] for x in ledger}==a,'complete named 481 skip ledger')
index={x['Name']:x for x in normal}
for x in ledger:
 row=index[x['Name']];check(x['row']==row and x['rejectedXQueryDependencies']==[d for d in row['Dependencies'] if not d['xquerySupported']],'actual dependency witness '+x['Name'])
 if x['Reason']=='Unsupported dependency':
  category=('routing-only' if row['xquerySupported'] else 'routing-and-independent') if not row['before'] else 'independent'
  check(x['Category']==category and not x['runnerAdmitted'],'runner rejection '+x['Name'])
check(collections.Counter(x['Category'] for x in ledger if x['Reason']=='Unsupported dependency')=={'independent':368,'routing-only':55,'routing-and-independent':2},'425 dependency skips partitioned exactly')
check(sum(x['Reason'].startswith('Harness error: XmlSchemaException') for x in ledger)==6,'six schema harness errors explicitly retained')
receipts=load(E/'original-receipts.json');check(len(receipts)==180,'180 original/resource receipts')
for phase,expected in [('normal',{'Passed':11,'Failed':9,'Skipped':40}),('prototype',{'Passed':11,'Failed':49})]:
 r=[x for x in receipts if x['phase']==phase and x['mode']=='actual'];check({x['case'] for x in r}==names and collections.Counter(x['result']['kind'] for x in r)==expected,'direct '+phase+' assertions')
failed={x['case'] for x in receipts if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'}
check(len(failed-excluded)==47 and len((deltaNames-excluded)-failed)==8,'47 eligible failures / eight new passes reconcile with gate totals')
visible=set(re.findall(r'^  FAIL ([^:]+):',read(E/'qt3-prototype.log'),re.M));check(len(visible)==47 and visible==failed-excluded,'all 47 full gate failure names reconcile exactly with eligible direct failures')
for x in originals:
 if x['row']['Name'] not in failed:check('XPST0003' in x['Assertion'],'eleven passes accept parse error '+x['row']['Name'])
controls=load(E/'control-receipts.json');check(len(controls)==24 and collections.Counter(x['result']['kind'] for x in controls)=={'Failed':16,'Passed':8},'24 independent controls / 16 semantic failures')
check(next(x for x in controls if x['name']=='C36-control-literal')['result']['kind']=='Passed','literal control corrected to seven characters')
for phase in ['normal','prototype']:
 c=lines(E/(phase+'-controls.jsonl'));check(len(c)==16 and all(x['passed'] for x in c),'16 lexical controls '+phase)
 check(all(x['actual']==(x['before'] if phase=='normal' else x['after']) for x in c),'real lexical routing '+phase)
gates=load(E/'gate-receipts.json');check({x['name']:x['exitCode'] for x in gates}=={'qt3-normal':0,'qt3-prototype':2,'qt4-normal':0},'failed isolated gate exit two retained')
processes=receipts+controls+gates+load(E/'inventory-receipts.json');check(len(processes)==209 and all(not x['timeout'] and x['exitCode']==(2 if x.get('name')=='qt3-prototype' else 0) for x in processes),'209 final bounded processes / no timeout')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'unchanged harness source '+p)
 else:
  clean=''.join(s for s in read(R/p).splitlines(True) if 'Isolated C36 pragma review' not in s)
  check(clean==read(R/v['source']).replace(r'\(#\s*[A-Za-z_]',r'(?!)'),'only isolated pragma regex/history changed')
for p in (R/'.guard-tmp/k04-pragma-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'identical isolated engine '+p.name)
check(load(E/'control-correction.json')['productionChanged']==False and load(E/'public-control-correction.json')['productionChanged']==False,'both diagnostic expectation corrections preserved')
render=load(E/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders / unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'remainingSkips':481,'dependencySkips':425,'pragmaOriginals':60,'isolatedAssertionFailures':47,'admissionQualified':False}))
