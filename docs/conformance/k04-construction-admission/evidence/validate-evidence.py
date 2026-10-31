"""Check C35 catalog completeness, exact admission delta and bounded assertion receipts."""
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
for name in ['previous-identities.json','fixture-identities.json','normal-source-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','994b4e33','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'production and baseline unchanged')
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821 and len({x['Set'] for x in normal})==428,'complete catalog 31821 / 428')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'real predicates '+x['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'catalog identity '+x['Name'])
affected=lines(E/'normal-affected.jsonl');names={x['row']['Name'] for x in affected}
old=lines(R/'docs/conformance/k04-construction-mode-review/evidence/normal-affected.jsonl')
check(len(affected)==87 and len({x['row']['Set'] for x in affected})==10,'87 cases / ten sets')
check(affected==old,'unchanged complete C33 original expressions/assertions/dependencies/resources')
excluded={x['row']['Name'] for x in affected if not x['row']['xquerySupported']}
check(excluded=={'K-VersionProlog-5','XQTY0086_1','XQTY0086_2','XQTY0086_3'},'four exact-XQ10 exclusions unchanged')
for name,expected in [('qt3-normal',[31257,0,564]),('qt3-prototype',[31340,0,481]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,'exact '+name)
a=skips(E/'qt3-normal-skips.txt');b=skips(E/'qt3-prototype-skips.txt');q=skips(E/'qt4-normal-skips.txt');A=R/'docs/conformance/k04-copy-namespaces-admission/adoption'
check(a==skips(A/'qt3-normal-skips.txt') and q==skips(A/'qt4-normal-skips.txt'),'both normal approved maps unchanged')
check(len(a)==564 and len(b)==481 and set(b)<=set(a) and set(a)-set(b)==names-excluded and all(a[k]==v for k,v in b.items()),'exact 83 removals and retained reasons')
check(load(E/'skip-delta.json')['removed']==sorted(names-excluded),'named exact skip delta')
rows=load(E/'original-receipts.json');check(len(rows)==261,'261 original/resource receipts')
for phase,expected in [('normal',{'Passed':3,'Skipped':84}),('prototype',{'Passed':87})]:
 rr=[x for x in rows if x['phase']==phase and x['mode']=='actual'];check(len(rr)==87 and {x['case'] for x in rr}==names and collections.Counter(x['result']['kind'] for x in rr)==expected,'original assertions '+phase)
oldrows=load(R/'docs/conformance/k04-construction-modes/evidence/original-receipts.json')
check({(x['case'],x['phase'],x['mode']):x['result'] for x in rows}=={(x['case'],x['phase'],x['mode']):x['result'] for x in oldrows},'exact C34 original/resource results replayed')
index={x['Name']:x for x in normal}
for name,n in [('copy',49),('context',43)]:
 rr=load(E/(name+'-receipts.json'));rows+=rr
 for phase in ['normal','prototype']:
  r=[x for x in rr if x['phase']==phase]
  check(len(r)==n and all(x['result']['kind']=='Passed' and index[x['case']]['before'] and index[x['case']]['after'] for x in r),name+' admission/assertions '+phase)
 old=load(R/('docs/conformance/k04-construction-modes/evidence/'+name+'-receipts.json'))
 check({x['case']:x['result'] for x in rr if x['phase']=='normal'}=={x['case']:x['result'] for x in old},name+' exact prior results')
for phase in ['normal','prototype']:
 r=lines(E/(phase+'-controls.jsonl'));check(len(r)==19 and all(x['passed'] for x in r),'19 lexical controls '+phase)
 for x,y in zip(load(E/'lexical-controls.json'),r):check(x['Expression']==y['Expression'] and x['Before']==y['before'] and x['After']==y['after'],'lexical identity '+x['Name'])
rows+=load(E/'gate-receipts.json')+load(E/'inventory-receipts.json')
check(len(rows)==450 and all(x['exitCode']==0 and not x['timeout'] for x in rows),'450 bounded successful diagnostic processes')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'isolated harness source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'normal-identical harness source '+p)
for p in (R/'.guard-tmp/k04-construction-admission-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'byte-identical engine '+p.name)
proposal=load(E/'proposal-identities.json')
for p,h in proposal['sha256'].items():check(sha(R/p)==h,'proposal '+p)
check(read(E/'proposed-executor.cs.txt').replace('Admit qualified construction declarations; retain pragma and independent dependency guards.','Isolated C35 construction review; normal source and policy remain unchanged.          ')==read(E/'prototype-executor.cs.txt'),'proposed predicate equals qualified predicate')
r=load(E/'render-receipt.json');check(len(r['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'ten rendered pages / unchanged CSS')
for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'cases':31821,'originalsPassed':87,'eligibleRemovals':83,'processes':len(rows)}))
