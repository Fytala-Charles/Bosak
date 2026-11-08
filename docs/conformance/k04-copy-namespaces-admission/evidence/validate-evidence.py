"""Validate the exact pending C32 admission against the delivered C31 implementation."""
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
 for l in text(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',l)
  if m:reason=m[1]
  elif l.strip() and reason:check(l.strip() not in out,'unique skip '+l.strip());out[l.strip()]=reason
 return out
def totals(p):return [int(re.search(k+r':\s+(\d+)',text(p))[1]) for k in ['Passed','Failed','Skipped']]
for f in ['previous-identities.json','fixture-identities.json','normal-source-identities.json','binary-identities.json']:
 for p,h in load(E/f).items():check(sha(R/p)==h,'preserved '+p)
for p,h in load(E/'engine-source-identities.json').items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'delivered engine source '+p)
for p,h in load(R/'docs/conformance/k04-copy-namespaces/evidence/binary-identities.json').items():
 if p.startswith('tests/'):check(sha(R/p)==h,'delivered normal engine/harness binary '+p)
check(not subprocess.check_output(['git','diff','HEAD','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'normal source/fixtures/baseline unchanged')
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 folder=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(folder),'rev-parse','HEAD'],text=True).strip()==revision,'frozen revision '+suite)
 check(not subprocess.check_output(['git','-C',str(folder),'status','--porcelain'],text=True).strip(),'clean fixture '+suite)
for name,expected in [('qt3-normal',[31208,0,613]),('qt3-prototype',[31257,0,564]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,name+' exact totals')
a=skipmap(E/'qt3-normal-skips.txt');b=skipmap(E/'qt3-prototype-skips.txt');q=skipmap(E/'qt4-normal-skips.txt')
check(a==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt3-normal-skips.txt'),'approved normal QT3 map unchanged')
check(q==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt4-normal-skips.txt'),'approved normal qt4 map unchanged')
check(len(a)==613 and len(b)==564 and not set(b)-set(a),'49 removals / zero additions')
check(all(a[n]==b[n] for n in b),'all 564 retained reasons unchanged')
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821 and len({x['Set'] for x in normal})==428,'complete 31821-case / 428-set catalog')
check(len({(x['Set'],x['Name']) for x in normal})==31821,'unique catalog case identities')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'independent real predicate '+x['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'independent expression/dependency identity '+x['Name'])
affected=[x for x in normal if x['before']!=x['after']];names={x['Name'] for x in affected};index={x['Name']:x for x in normal}
check(len(affected)==49 and names==set(a)-set(b),'all 49 predicates equal exact gate removals')
check(all(not x['before'] and x['after'] and not x['xpathSupported'] and x['xquerySupported'] for x in affected),'all affected cases XQuery-admissible only')
check(dict(collections.Counter(x['Set'] for x in affected))=={'prod-CompNamespaceConstructor':9,'prod-CopyNamespacesDecl':33,'prod-DirElemConstructor':2,'prod-DirElemContent':4,'misc-CombinedErrorCodes':1},'exact five affected sets')
check(all(a[n]=='Unsupported dependency' for n in names),'all removed original skip reasons')
check(names==set(load(E/'skip-delta.json')['removed']),'published exact skip delta')
prior={x['row']['Name']:x for x in load(R/'docs/conformance/k04-copy-namespaces/evidence/original-assertions.json')}
check(names==set(prior),'exact C31 original cohort')
for x in lines(E/'normal-affected.jsonl'):
 old=prior[x['row']['Name']]
 check(all(x[field]==old[field] for field in ['Expression','Assertion','Modules','Environment','Dependencies']),'original query/assertion/environment unchanged '+x['row']['Name'])
retained=load(E/'retained-guards.json');check(len(retained['constructionAndMixed'])==87,'87 retained construction/mixed cases')
check(all(not index[n]['before'] and not index[n]['after'] for n in retained['constructionAndMixed']),'every construction/mixed refusal retained')
context=load(R/'docs/conformance/k04-context-routing-review/evidence/selected-originals.json');check(len(context)==43 and all(index[x['Name']]['before'] and index[x['Name']]['after'] for x in context),'43 admitted context predicates unchanged')
for n in retained['independentXQ10Exclusions']:check(not index[n]['xquerySupported'],'independent exact-XQ10 exclusion '+n)
for phase in ['normal','prototype']:
 check(load(E/(phase+'-summary.json'))==dict(phase=phase,sets=428,count=31821,delta=49),phase+' catalog summary')
 controls=lines(E/(phase+'-controls.jsonl'));check(len(controls)==37 and all(x['passed'] for x in controls),phase+' 37 lexical controls')
 for request,result in zip(load(E/'lexical-controls.json'),controls):check(request['Name']==result['Name'] and request['Expression']==result['Expression'] and request['Before']==result['before'] and request['After']==result['after'],phase+' lexical control '+request['Name'])
rows=load(E/'original-receipts.json');check(len(rows)==147,'147 original/resource probes')
for phase,expected in [('normal',{'Passed':2,'Skipped':47}),('prototype',{'Passed':49})]:
 actual=[x for x in rows if x['phase']==phase and x['mode']=='actual'];check(len(actual)==49 and {x['case'] for x in actual}==names,phase+' exact original cohort');check(collections.Counter(x['result']['kind'] for x in actual)==expected,phase+' original outcomes')
 if phase=='normal':check({x['case'] for x in actual if x['result']['kind']=='Passed'}=={'K-CopyNamespacesProlog-4','K-CopyNamespacesProlog-5'},'normal direct parse-error witnesses explained')
processes=rows+load(E/'inventory-receipts.json')+load(E/'gate-receipts.json');check(len(processes)==152 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'152 successful bounded diagnostic processes')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied harness source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'identical copied harness source '+p)
 else:
  clean=''.join(l for l in text(R/p).splitlines(True) if 'Isolated C32 copy-namespaces review' not in l)
  check(clean==text(R/v['source']).replace(r'\bdeclare\s+(copy-namespaces|construction)\b',r'\bdeclare\s+(construction)\b'),'only copy alternative and diagnostic history changed')
for p in (R/'.guard-tmp/k04-copy-admission-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'byte-identical copied engine '+p.name)
proposal=load(E/'proposal-identities.json');check(proposal['files']==2,'exact two-file proposal');patch=''
for file,h in proposal['baseNormalized'].items():
 check(subprocess.check_output(['git','hash-object',file],cwd=R,text=True).strip()==h,'proposal base '+file)
 old=text(R/file);new=text(E/('proposed-'+Path(file).name+'.txt'));patch+=''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile='a/'+file,tofile='b/'+file))
check(patch==text(D/'PROPOSED-ADOPTION.patch'),'proposal snapshots reproduce exact patch')
check(sha(D/'PROPOSED-ADOPTION.patch')==proposal['patchSha256'],'raw proposal patch identity')
for p,h in proposal['proposedRaw'].items():check(sha(E/p)==h,'proposed source '+p)
check(subprocess.run(['git','apply','--check',str(D/'PROPOSED-ADOPTION.patch')],cwd=R,capture_output=True).returncode==0,'proposal applies cleanly without application')
r=load(E/'render-receipt.json');check(len(r['pages'])==10,'ten rendered pages');check(sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'unchanged branded CSS')
for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(str(len(checks))+' evidence checks passed.')
