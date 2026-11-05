"""Validate C40 exact xib metadata qualification without adopting normal policy."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
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
C39=R/'docs/conformance/k04-independent-profile-review/evidence'
for name in ['previous-identities.json','normal-source-identities.json','fixture-identities.json','diagnostic-fixture-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','08fe2e5a','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'normal source, fixtures and baseline unchanged')
for p,h in load(C39/'binary-identities.json').items():
 if p.startswith('tests/'):check(sha(R/p)==h,'same delivered normal binary '+p)
pin=load(R/'docs/conformance/k04-pragma-review/evidence/REVIEW-PIN.json')['fixtureRevisions']
for suite,revision in pin.items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
names={'format-date-en151','format-dateTime-en151'}
for suite,count,sets in [('qt3',31821,428),('qt4',46985,738)]:
 a=lines(E/(suite+'-normal-catalog.jsonl'));b=lines(E/(suite+'-prototype-catalog.jsonl'))
 check(len(a)==len(b)==count and len({x['Set'] for x in a})==sets,'complete '+suite+' catalog')
 check(a==lines(C39/(suite+'-catalog.jsonl')),'actual normal inventory unchanged '+suite)
 changed=[];hits=[]
 for x,y in zip(a,b):
  check(x['Name']==y['Name'],'case order '+suite+' '+x['Name'])
  if x!=y:changed.append(x['Name'])
  xib=[d for d in x['Dependencies'] if d['Type']=='language' and d['Value']=='xib']
  if xib:hits.append(x['Name']);check(len(xib)==1 and not xib[0]['Satisfied'] and x['gated'],'only gated negative xib fixtures '+suite+' '+x['Name'])
  if x['Name'] not in names:check(x==y,'unchanged actual filter observation '+suite+' '+x['Name']);continue
  before=x['xpathSupported'] or (suite=='qt3' and x['routingAllowed'] and x['xquerySupported'])
  after=y['xpathSupported'] or (suite=='qt3' and y['routingAllowed'] and y['xquerySupported'])
  check(not before and after,'negative xib admitted under actual runner profiles '+suite+' '+x['Name'])
  xx=dict(x);yy=dict(y)
  for row in [xx,yy]:
   row.pop('xpathSupported');row.pop('xquerySupported');row['Dependencies']=[{k:v for k,v in d.items() if k not in ['xpathSupported','xquerySupported']} for d in row['Dependencies']]
  check(xx==yy,'only dependency predicates change '+suite+' '+x['Name'])
 check(set(changed)==set(hits)==names and len(changed)==2,'exact two changes / no positive fixture acquired a skip '+suite)
 originalA=lines(E/(suite+'-normal-originals.jsonl'));originalB=lines(E/(suite+'-prototype-originals.jsonl'))
 check(len(originalA)==len(originalB)==2,'two unchanged original snapshots '+suite)
 for x,y in zip(originalA,originalB):
  x=dict(x);y=dict(y);x.pop('row');y.pop('row');check(x==y,'unchanged query/assertion/module/environment/dependencies '+suite)
check(totals(E/'qt3-normal.log')==[31395,0,426] and totals(E/'qt3-prototype.log')==[31397,0,424] and totals(E/'qt4-normal.log')==[8939,0,1920] and totals(E/'qt4-prototype.log')==[8941,0,1918],'all four exact complete gates')
for suite,retained in [('qt3',424),('qt4',1918)]:
 a=skips(E/(suite+'-normal-skips.txt'));b=skips(E/(suite+'-prototype-skips.txt'))
 check(a==skips(C39/(suite+'-normal-skips.txt')),'complete normal approved map unchanged '+suite)
 check(len(b)==retained and set(a)-set(b)==names and set(b)<=set(a) and all(a[k]==v for k,v in b.items()),'exact two removals / retained reasons unchanged '+suite)
 check(load(E/'skip-delta.json')[suite]['removed']==sorted(names),'named removals '+suite)
requests=load(E/'filter-requests.json');oracle=load(E/'filter-projection-oracle.json')
n={(x['Suite'],x['Name']):x for x in lines(E/'normal-filters.jsonl')};p={(x['Suite'],x['Name']):x for x in lines(E/'prototype-filters.jsonl')}
check(len(requests)==len(n)==len(p)==854 and len(oracle)==474,'854 actual filter requests / 474 base controls')
changed=collections.Counter()
for x in oracle:
 key=(x['Suite'],x['Name']);basis=n[x['Suite'],x['ProjectionName']]
 for field in ['xpathSupported']+(['xqueryDependenciesSupported'] if x['Suite']=='qt3' else []):
  expected=False if x['HasPositiveCanonicalXib'] else basis[field]
  check(p[key][field]==expected,'actual normal-filter projection '+str(key)+' '+field)
  changed[x['Suite']]+=n[key][field]!=p[key][field]
 check(n[key]['canRouteXquery']==p[key]['canRouteXquery'],'routing guard unchanged '+str(key))
check(changed=={'qt3':160,'qt4':80},'only canonical-xib control profile outcomes change')
for x in requests:
 if not any(d['Type']=='language' and d['Value']=='xib' for d in x['Dependencies']):check(n[x['Suite'],x['Name']]==p[x['Suite'],x['Name']],'all unrelated filter output retained '+x['Suite']+' '+x['Name'])
diag=load(E/'diagnostic-receipts.json');old=load(C39/'diagnostic-receipts.json');oldIndex={(x['suite'],x['case'],x['mode']):x['result'] for x in old}
check(len(diag)==39,'39 original/resource/public diagnostic receipts')
for x in diag:check(x['result']==oldIndex[x['suite'],x['case'],x['mode']],'exact C39 original/resource/control result '+x['name'])
for phase in ['normal','prototype']:
 actual=[x for x in diag if x['mode']=='actual' and x['phase']==phase]
 check(len(actual)==9 and collections.Counter(x['result']['kind'] for x in actual)=={'Passed':4,'Failed':5},'four original passes / five counter-profile failures preserved '+phase)
check(len([x for x in diag if x['mode']=='public-xquery'])==12 and all(x['result']['kind']=='Passed' for x in diag if x['mode']=='public-xquery'),'twelve original public controls pass')
processes=diag+load(E/'filter-receipts.json')+load(E/'inventory-receipts.json')+load(E/'gate-receipts.json');check(len(processes)==49 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'49 bounded successful process exits / assertions inspected separately')
for p,x in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==x['sha256'],'copied source '+p)
 if not x['changed']:check(sha(R/p)==sha(R/x['source']),'unchanged complete harness machinery '+p)
for suite,project in [('qt3','Bosak.XPath.Conformance'),('qt4','Bosak.XPath.Conformance40')]:
 for p in (R/('.guard-tmp/k04-xib-harness-'+suite)/'bin/Release/net10.0').glob('Bosak*.dll'):
  if p.name!=project+'.dll':check(sha(p)==sha(R/'tests'/project/'bin/Release/net10.0'/p.name),'byte-identical isolated engine '+suite+' '+p.name)
 for phase in ['normal','prototype']:
  s=read(E/(suite+'-'+phase+'.log'))
  if suite=='qt4':check(len(re.findall(r'^  Starting:',s,re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in s,'qt4 gate membership and experimental passes '+phase)
proposal=load(E/'proposal-identities.json')
for p,h in proposal['sha256'].items():check(sha(R/p)==h,'pinned proposal '+p)
for f in ['DependencyFilter.cs','DependencyFilter40.cs']:
 check(read(E/('proposed-'+f+'.txt')).replace('Admit reviewed canonical xib fallback cases with correct dependency polarity.','Isolated C40 canonical xib polarity review; normal policy remains unchanged.')==read(E/('prototype-'+f+'.txt')),'qualified predicate equals proposed predicate '+f)
for x in load(E/'schema-context.json')['documents']:check(sha(R/x['snapshot'])==x['sha256'],'exact schema context snapshot '+x['source'])
check(read(E/'core-req139-snapshot.txt') in read(R/'docs/FEATURE_REQUESTS.md'),'REQ139 boundary unchanged')
r=load(E/'render-receipt.json');check(len(r['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'ten HTML renders / unchanged CSS')
for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'catalogCases':78806,'originalsPassedEachPhase':4,'qt3Prototype':[31397,0,424],'qt4Prototype':[8941,0,1918],'ownerApproval':'pending'}))
