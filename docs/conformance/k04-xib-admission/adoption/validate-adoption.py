"""Verify the exact approved C40 adoption and fresh normal execution evidence."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
A=Path(__file__).resolve().parent;D=A.parent;E=D/'evidence';R=A.parents[3];checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def lines(p):return [json.loads(x) for x in read(p).splitlines()]
def check(ok,label):
 assert ok,label
 checks.append(label)
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:check(s.strip() not in out,'unique skip '+s.strip());out[s.strip()]=reason
 return out
for p,h in load(A/'previous-identities.json').items():check(sha(R/p)==h,'historical qualification/proposal preserved '+p)
check(load(A/'approval.json')['answer']=='Approve exact xib adoption in both suites (Recommended)','explicit exact owner approval')
proposal=load(E/'proposal-identities.json');approved=proposal['paths']
for p in approved:check((R/p).read_bytes()==(E/('proposed-'+Path(p).name+'.txt')).read_bytes(),'byte-exact approved snapshot '+p)
changed=subprocess.check_output(['git','diff','7593efa7','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines();check(set(changed)==set(approved),'only three approved production paths changed')
for name in ['normal-source-identities.json','fixture-identities.json','diagnostic-fixture-identities.json']:
 for p,h in load(E/name).items():
  if p not in approved:check(sha(R/p)==h,'retained source/fixture '+p)
pin=load(R/'docs/conformance/k04-pragma-review/evidence/REVIEW-PIN.json')['fixtureRevisions']
for suite,revision in pin.items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
names={'format-date-en151','format-dateTime-en151'}
for suite,expected,count,sets in [('qt3',[31397,0,424],31821,428),('qt4',[8941,0,1918],46985,738)]:
 check(totals(A/(suite+'-normal.log'))==expected,'exact fresh gate '+suite)
 old=skips(E/(suite+'-normal-skips.txt'));new=skips(A/(suite+'-normal-skips.txt'))
 check(new==skips(E/(suite+'-prototype-skips.txt')) and set(old)-set(new)==names and set(new)<=set(old) and all(old[k]==v for k,v in new.items()),'exact two removals / every retained reason unchanged '+suite)
 catalog=lines(A/(suite+'-adopted-catalog.jsonl'));check(len(catalog)==count and len({x['Set'] for x in catalog})==sets and catalog==lines(E/(suite+'-prototype-catalog.jsonl')),'complete actual catalog equals qualified rehearsal '+suite)
 check(lines(A/(suite+'-adopted-originals.jsonl'))==lines(E/(suite+'-prototype-originals.jsonl')),'all original queries/assertions/resources equal '+suite)
 check(lines(A/(suite+'-adopted-polarity.jsonl'))==lines(E/(suite+'-prototype-polarity.jsonl')),'all actual polarity observations equal '+suite)
 if suite=='qt4':
  s=read(A/'qt4-normal.log');check(len(re.findall(r'^  Starting:',s,re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in s,'230 qt4 gated sets / 11 experimental passes unchanged')
matrix=lines(A/'adopted-filters.jsonl');check(len(matrix)==854 and matrix==lines(E/'prototype-filters.jsonl'),'all 854 adopted actual filter requests equal qualified outputs')
diag=load(A/'diagnostic-receipts.json');qualified=load(E/'diagnostic-receipts.json')
lookup={(x['suite'],x['case'],x['mode']):x['result'] for x in qualified if x['phase']=='prototype' or x['mode'] in ['resources','public-xquery']}
check(len(diag)==30 and all(x['result']==lookup[x['suite'],x['case'],x['mode']] for x in diag),'all original/resource/public diagnostic outcomes exactly replayed')
actual=[x for x in diag if x['mode']=='actual'];check(collections.Counter(x['result']['kind'] for x in actual)=={'Passed':4,'Failed':5},'four xib passes and five excluded counter-profile failures preserved')
check(all(x['result']['kind']=='Passed' for x in actual if x['case'] in names),'all four unchanged target assertions pass')
check(len([x for x in diag if x['mode']=='public-xquery'])==12 and all(x['result']['kind']=='Passed' for x in diag if x['mode']=='public-xquery'),'all twelve public fallback controls pass')
processes=diag+load(A/'inventory-receipts.json')+load(A/'gate-receipts.json')+[load(A/'filter-receipt.json')];check(len(processes)==35 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'35 bounded successful harness/matrix/diagnostic process exits')
testRows=re.findall(r'Passed!\s+- Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+)',read(A/'solution-tests.log'));check(len(testRows)==10 and sum(int(x[1]) for x in testRows)==24164 and all(x[0]==x[2]=='0' for x in testRows),'24164 fresh solution passes / ten projects')
check(load(A/'solution-receipt.json')['exitCode']==0 and '0 Error(s)' in read(A/'build.log'),'full solution test process/build pass')
comparison=load(A/'binary-comparison.json');check(len(comparison)==9 and all(x['review']['methodAndSignatureSha256']==x['adopted']['methodAndSignatureSha256'] and x['review']['assemblyVersion']==x['adopted']['assemblyVersion'] for x in comparison),'nine engine method/signature/assembly-version identities unchanged')
for p,h in load(A/'binary-identities.json').items():check(sha(R/p)==h,'final rebuilt binary '+p)
for p,h in load(A/'adopted-source-identities.json').items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'adopted source '+p)
qt3=lines(A/'qt3-adopted-catalog.jsonl');index={x['Name']:x for x in qt3};remaining=[index[n] for n,r in skips(A/'qt3-normal-skips.txt').items() if r=='Unsupported dependency']
check(len(remaining)==368 and all(x['routingAllowed'] and not x['xpathSupported'] and not x['xquerySupported'] for x in remaining),'368 remaining independent exclusions / routing unchanged')
for x in load(E/'schema-context.json')['documents']:check(sha(R/x['snapshot'])==x['sha256'],'inherited Schema context '+x['source'])
check(read(E/'core-req139-snapshot.txt') in read(R/'docs/FEATURE_REQUESTS.md'),'REQ139 ownership/acceptance boundary unchanged')
render=load(A/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders / unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (A/'MANIFEST.json').exists():
 m=load(A/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'solutionPassed':24164,'qt3':[31397,0,424],'qt4':[8941,0,1918],'removedSkipsEachSuite':2}))
