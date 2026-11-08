"""Validate C29's bounded catalog, source isolation and exact pending adoption."""
from pathlib import Path
import json,hashlib,re,subprocess,collections,difflib
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label)
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def text(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def lines(p):return [json.loads(l) for l in text(p).splitlines()]
def skipmap(p):
 out={};reason=None
 for line in text(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',line)
  if m:reason=m[1]
  elif line.strip() and reason:
   check(line.strip() not in out,'unique skip '+line.strip());out[line.strip()]=reason
 return out
def totals(p):
 s=text(p);return [int(re.search(k+r':\s+(\d+)',s)[1]) for k in ['Passed','Failed','Skipped']]
for filename in ['previous-identities.json','fixture-identities.json','normal-source-identities.json','binary-identities.json']:
 for p,h in load(E/filename).items():check(sha(R/p)==h,'identity '+p)
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 folder='qt3tests' if suite=='qt3' else 'qt4tests'
 check(subprocess.check_output(['git','-C',str(R/'tests'/folder),'rev-parse','HEAD'],text=True).strip()==revision,'fixture revision '+suite)
 check(not subprocess.check_output(['git','-C',str(R/'tests'/folder),'status','--porcelain'],text=True).strip(),'fixture clean '+suite)
check(not subprocess.check_output(['git','diff','HEAD','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'normal source/fixtures/baseline unchanged')
check(totals(E/'qt3-normal.log')==[31165,0,656],'normal QT3 exact')
check(totals(E/'qt3-prototype.log')==[31208,0,613],'prototype QT3 exact')
check(totals(E/'qt4-normal.log')==[8939,0,1920],'normal qt4 exact')
a=skipmap(E/'qt3-normal-skips.txt');b=skipmap(E/'qt3-prototype-skips.txt');q=skipmap(E/'qt4-normal-skips.txt')
check(a==skipmap(R/'docs/conformance/k04-normalization-admission/adoption/qt3-gate-skips.txt'),'approved QT3 map unchanged')
check(q==skipmap(R/'docs/conformance/k04-normalization-admission/adoption/qt4-gate-skips.txt'),'approved qt4 map unchanged')
check(len(a)==656 and len(b)==613 and not set(b)-set(a),'exact 43 removals / zero additions')
check(all(a[n]==b[n] for n in b),'all 613 retained skip reasons unchanged')
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821,'complete catalog count')
check(len({x['Set'] for x in normal})==428,'all 428 catalog sets')
check(len({(x['Set'],x['Name']) for x in normal})==31821,'unique catalog cases')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'real assembly predicate '+x['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'normal/prototype identity '+x['Name'])
affected=[x for x in normal if x['before']!=x['after']];names={x['Name'] for x in affected}
check(names==set(a)-set(b),'every predicate delta equals a full-gate skip removal')
check(all(not x['before'] and x['after'] and not x['xpathSupported'] and x['xquerySupported'] for x in affected),'all affected cases XQuery admissible only')
check(dict(collections.Counter(x['Set'] for x in affected))=={'prod-ContextItemDecl':40,'prod-VarDefaultValue':3},'exact two affected sets')
check(all(a[n]=='Unsupported dependency' for n in names),'normal full-run admission reasons')
check(names==set(load(E/'skip-delta.json')['removed']),'published skip delta')
for phase in ['normal','prototype']:
 summary=load(E/(phase+'-summary.json'));check(summary==dict(phase=phase,sets=428,count=31821,delta=43),phase+' summary')
 controls=lines(E/(phase+'-controls.jsonl'));check(len(controls)==30 and all(x['passed'] for x in controls),phase+' thirty lexical controls')
 for request,result in zip(load(E/'lexical-controls.json'),controls):
  check(request['Name']==result['Name'] and request['Expression']==result['Expression'] and request['Before']==result['before'] and request['After']==result['after'],phase+' control identity '+request['Name'])
rows=load(E/'original-receipts.json');check(len(rows)==129,'all 129 original/resource probes')
for phase in ['normal','prototype']:
 actual=[x for x in rows if x['phase']==phase and x['mode']=='actual']
 check(len(actual)==43 and {x['case'] for x in actual}==names,phase+' exact original cohort')
 expected={'Skipped':42,'Passed':1} if phase=='normal' else {'Passed':43}
 check(dict(collections.Counter(x['result']['kind'] for x in actual))==expected,phase+' original assertions')
 if phase=='normal':check([x['case'] for x in actual if x['result']['kind']=='Passed']==['contextDecl-023'],'normal direct parse-error pass explained')
processes=[]
for f,count in [('original-receipts.json',129),('gate-receipts.json',3),('inventory-receipts.json',2)]:
 rr=load(E/f);check(len(rr)==count,f+' count');check(all(x['exitCode']==0 and not x['timeout'] for x in rr),f+' successful bounded exits');processes+=rr
check(len(processes)==134,'134 final processes')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'identical copied source '+p)
 else:
  normalSource=text(R/v['source']);prototype=text(R/p)
  withoutHistory=''.join(l for l in prototype.splitlines(True) if 'Isolated C29 context-routing review' not in l)
  check(withoutHistory==normalSource.replace(r'\bdeclare\s+(copy-namespaces|context|construction)\b',r'\bdeclare\s+(copy-namespaces|construction)\b'),'only regex and diagnostic history changed')
for p in (R/'.guard-tmp/k04-context-routing-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'identical prototype engine '+p.name)
proposal=load(E/'proposal-identities.json');check(proposal['files']==2,'two-file proposal')
patch=''
for file,h in proposal['baseNormalized'].items():
 check(subprocess.check_output(['git','hash-object',file],cwd=R,text=True).strip()==h,'proposal immutable base '+file)
 new=text(E/('proposed-'+Path(file).name+'.txt'));old=text(R/file)
 patch+=''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile='a/'+file,tofile='b/'+file))
check(patch==text(D/'PROPOSED-ADOPTION.patch'),'exact proposal snapshots and patch')
check(sha(D/'PROPOSED-ADOPTION.patch')==proposal['patchSha256'],'proposal patch raw identity')
for p,h in proposal['proposedRaw'].items():check(sha(E/p)==h,'proposed file '+p)
check(subprocess.run(['git','apply','--check',str(D/'PROPOSED-ADOPTION.patch')],cwd=R,capture_output=True).returncode==0,'proposal applies cleanly without applying it')
render=load(E/'render-receipt.json');check(len(render['pages'])==10,'ten rendered pages');check(sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(str(len(checks))+' evidence checks passed.')
