"""Validate C38's exact pragma admission qualification and unchanged normal policy."""
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
C37=R/'docs/conformance/k04-pragma-fallback/evidence';C36=R/'docs/conformance/k04-pragma-review/evidence'
for name in ['previous-identities.json','normal-source-identities.json','fixture-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','254d07d7','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'production, fixtures and baseline unchanged')
for p,h in load(C37/'binary-identities.json').items():
 if p.startswith('tests/'):check(sha(R/p)==h,'delivered C37 normal binary '+p)
for suite,revision in load(C36/'REVIEW-PIN.json')['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
for phase in ['normal','prototype']:
 c=lines(E/(phase+'-catalog.jsonl'));check(len(c)==31821 and len({x['Set'] for x in c})==428 and c==lines(C37/(phase+'-catalog.jsonl')),'complete actual routing/dependency inventory '+phase)
 check(lines(E/(phase+'-originals.jsonl'))==lines(C37/(phase+'-originals.jsonl')),'unchanged original expressions/assertions/resources '+phase)
 check(lines(E/(phase+'-controls.jsonl'))==lines(C37/(phase+'-controls.jsonl')),'sixteen actual lexical controls '+phase)
affected=lines(E/'normal-affected.jsonl');originals=lines(E/'normal-originals.jsonl');delta={x['row']['Name'] for x in affected}
check(len(affected)==57 and len(originals)==60,'57 predicates / 60 originals')
excluded={x['row']['Name'] for x in affected if not x['row']['xquerySupported']};check(excluded=={'K-ExtensionExpression-4','XQST0079'},'two independent exact-XQ10 exclusions')
for name,expected in [('qt3-normal',[31340,0,481]),('qt3-prototype',[31395,0,426]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,'exact '+name)
a=skips(E/'qt3-normal-skips.txt');b=skips(E/'qt3-prototype-skips.txt');q=skips(E/'qt4-normal-skips.txt')
check(a==skips(C37/'qt3-normal-skips.txt') and q==skips(C37/'qt4-normal-skips.txt'),'both approved normal skip maps unchanged')
check(b==skips(C37/'qt3-prototype-skips.txt') and len(b)==426 and set(a)-set(b)==delta-excluded and set(b)<=set(a) and all(a[k]==v for k,v in b.items()),'exact 55 removals / 426 retained reasons')
check(load(E/'skip-delta.json')['removed']==sorted(delta-excluded),'55 named exact removals')
rows=load(E/'original-receipts.json');check(len(rows)==180,'180 original/resource receipts')
oldrows=load(C37/'original-receipts.json')
check({(x['case'],x['phase'],x['mode']):x['result'] for x in rows}=={(x['case'],x['phase'],x['mode']):x['result'] for x in oldrows},'exact original/resource outcomes replayed')
for phase,expected in [('normal',{'Passed':11,'Failed':9,'Skipped':40}),('prototype',{'Passed':59,'Failed':1})]:
 r=[x for x in rows if x['mode']=='actual' and x['phase']==phase];check(len(r)==60 and collections.Counter(x['result']['kind'] for x in r)==expected,'original outcomes '+phase)
check({x['case'] for x in rows if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'}=={'K-ExtensionExpression-4'},'only excluded old-version no-namespace expectation fails directly')
controls=load(E/'control-receipts.json');check(len(controls)==24 and all(x['result']['kind']=='Passed' for x in controls),'24 independent public controls pass')
check({x['name']:x['result'] for x in controls}=={x['name']:x['result'] for x in load(C37/'control-receipts.json')},'public controls replayed exactly')
index={x['Name']:x for x in lines(E/'normal-catalog.jsonl')}
for name,n in [('copy',49),('context',43),('construction',87)]:
 r=load(E/(name+'-receipts.json'));rows+=r
 for phase in ['normal','prototype']:
  rr=[x for x in r if x['phase']==phase];check(len(rr)==n and all(x['result']['kind']=='Passed' and index[x['case']]['before'] and index[x['case']]['after'] for x in rr),name+' original assertions and predicates '+phase)
processes=rows+controls+load(E/'gate-receipts.json')+load(E/'inventory-receipts.json');check(len(processes)==567 and all(not x['timeout'] and x['exitCode']==0 for x in processes),'567 bounded successful diagnostic process exits')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied harness source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'normal-identical copied harness source '+p)
for p in (R/'.guard-tmp/k04-pragma-admission-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'identical isolated engine '+p.name)
proposal=load(E/'proposal-identities.json')
for p,h in proposal['sha256'].items():check(sha(R/p)==h,'pinned proposal '+p)
check(read(E/'proposed-executor.cs.txt').replace('Admit qualified ignored-pragma fallback; retain all independent dependency guards.','Isolated C38 pragma review; normal source and policy remain unchanged.          ')==read(E/'prototype-executor.cs.txt'),'proposed predicate equals qualified predicate')
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'qt4 membership unchanged')
r=load(E/'render-receipt.json');check(len(r['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'ten HTML renders / unchanged CSS')
for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'catalogCases':31821,'eligibleRemovals':55,'retainedSkips':426,'processes':567,'ownerApproval':'pending'}))
