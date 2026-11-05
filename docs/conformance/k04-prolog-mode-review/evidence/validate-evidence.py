"""Verify C30's failed isolated rehearsal without changing accepted production policy."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
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
  elif line.strip() and reason:check(line.strip() not in out,'unique skip '+line.strip());out[line.strip()]=reason
 return out
def totals(p):return [int(re.search(k+r':\s+(\d+)',text(p))[1]) for k in ['Passed','Failed','Skipped']]
for f in ['previous-identities.json','fixture-identities.json','normal-source-identities.json','binary-identities.json']:
 for p,h in load(E/f).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','HEAD','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'no production source/fixture/baseline change')
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 folder=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(folder),'rev-parse','HEAD'],text=True).strip()==revision,'fixture revision '+suite)
 check(not subprocess.check_output(['git','-C',str(folder),'status','--porcelain'],text=True).strip(),'fixture clean '+suite)
check(totals(E/'qt3-normal.log')==[31208,0,613],'fresh normal QT3')
check(totals(E/'qt3-prototype.log')==[31214,126,481],'failed isolated QT3 totals retained')
check(totals(E/'qt4-normal.log')==[8939,0,1920],'fresh normal qt4')
a=skipmap(E/'qt3-normal-skips.txt');b=skipmap(E/'qt3-prototype-skips.txt');q=skipmap(E/'qt4-normal-skips.txt')
check(a==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt3-normal-skips.txt'),'normal QT3 approved map unchanged')
check(q==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt4-normal-skips.txt'),'normal qt4 approved map unchanged')
check(len(a)==613 and len(b)==481 and not set(b)-set(a),'132 prototype skip removals / no additions')
check(all(a[n]==b[n] for n in b),'481 retained prototype skip names/reasons unchanged')
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821 and len({x['Set'] for x in normal})==428,'complete catalog / 428 sets')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'real harness predicate '+x['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'independent catalog identity '+x['Name'])
affected=lines(E/'normal-affected.jsonl');prototypeAffected=lines(E/'prototype-affected.jsonl');index={x['row']['Name']:x for x in affected}
check(len(affected)==136 and len({x['row']['Set'] for x in affected})==13,'136 changed predicates / 13 sets')
check(collections.Counter((x['Construction'],x['CopyNamespaces']) for x in affected)=={(True,False):75,(False,True):49,(True,True):12},'75 construction-only / 49 copy-only / 12 mixed')
for x,y in zip(affected,prototypeAffected):
 x=dict(x);y=dict(y);x['row']=dict(x['row']);y['row']=dict(y['row']);x['row'].pop('actual');y['row'].pop('actual');check(x==y,'unchanged original expression/assertion '+x['row']['Name'])
check(all(x['row']['xquerySupported'] for x in affected if x['CopyNamespaces'] and not x['Construction']),'all 49 copy-only dependencies admissible')
excluded={x['row']['Name'] for x in affected if not x['row']['xquerySupported']}
check(excluded=={'K-VersionProlog-5','XQTY0086_1','XQTY0086_2','XQTY0086_3'},'four independent dependency exclusions')
check(set(a)-set(b)==set(index)-excluded,'all full-run removals reconciled')
rows=load(E/'original-receipts.json');check(len(rows)==408,'all original/resource probes')
for phase,expected in [('normal',{'Passed':5,'Skipped':131}),('prototype',{'Passed':6,'Failed':130})]:
 selected=[x for x in rows if x['phase']==phase and x['mode']=='actual'];check(len(selected)==136 and {x['case'] for x in selected}==set(index),phase+' exact original cohort');check(dict(collections.Counter(x['result']['kind'] for x in selected))==expected,phase+' exact assertion outcomes')
failed={x['case'] for x in rows if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'}
gateFailed=set(re.findall(r'^  FAIL ([^:]+):',text(E/'qt3-prototype.log'),re.M))
check(len(gateFailed)==126 and gateFailed==failed-excluded,'all 126 complete-gate failures match direct original failures')
normalPassed={x['case'] for x in rows if x['phase']=='normal' and x['mode']=='actual' and x['result']['kind']=='Passed'}
check(normalPassed=={'K-ConstructionProlog-2','K-CopyNamespacesProlog-4','K-CopyNamespacesProlog-5','qischema90007','qischema90008'},'all five direct normal parser-error witnesses named')
check(all('XPST0003' in index[n]['Assertion'] for n in normalPassed),'five passing assertions accept parser error')
check(all('XPST0003' in x['result'].get('message','') for x in rows if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'),'all original failures reach unsupported prolog parser')
controls=load(E/'control-receipts.json');check(len(controls)==20 and collections.Counter(x['result']['kind'] for x in controls)=={'Passed':6,'Failed':14},'public semantic controls 6 pass / 14 fail')
check(all(x['result']['mode']=='public-xquery' for x in controls),'controls bypass routing via public XQuery API')
for x in controls:
 if x['result']['kind']=='Failed':check('XPST0003' in x['result']['error'],'control parser boundary '+x['name'])
for phase in ['normal','prototype']:
 rr=lines(E/(phase+'-controls.jsonl'));check(len(rr)==26 and all(x['passed'] for x in rr),phase+' 26 lexical controls')
 for request,result in zip(load(E/'lexical-controls.json'),rr):check(request['Name']==result['Name'] and request['Expression']==result['Expression'] and request['Before']==result['before'] and request['After']==result['after'],phase+' lexical identity '+request['Name'])
gates=load(E/'gate-receipts.json');check({x['name']:x['exitCode'] for x in gates}=={'qt3-normal':0,'qt3-prototype':2,'qt4-normal':0},'failed-assertion exit 2 preserved separately from normal gates')
processes=rows+controls+gates+load(E/'inventory-receipts.json')+[load(E/'version-early-error-receipt.json')]
check(len(processes)==434 and all(not x['timeout'] and x['exitCode']==(2 if x.get('name')=='qt3-prototype' else 0) for x in processes),'434 bounded diagnostic exits retained')
version=load(E/'version-early-error-receipt.json')['result'];check(version['kind']=='Passed' and 'XQST0038' in version['error'],'version original passes earlier unsupported collation error')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'prototype source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'unmodified harness source '+p)
 else:
  clean=''.join(l for l in text(R/p).splitlines(True) if 'Isolated C30 prolog-mode review' not in l)
  check(clean==text(R/v['source']).replace(r'\bdeclare\s+(copy-namespaces|construction)\b',r'(?!)'),'only two prolog refusals disabled plus diagnostic history')
for p in (R/'.guard-tmp/k04-prolog-mode-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'byte-identical prototype engine '+p.name)
excerpts=load(E/'reviewed-specification-excerpts.json');check(excerpts['sourceSha256']==load(E/'REVIEW-PIN.json')['specification']['sha256'],'immutable reviewed spec source')
check('XQST0067' in excerpts['sections'][0]['text'] and 'XQST0055' in excerpts['sections'][1]['text'],'construction/copy duplicate rules reviewed')
render=load(E/'render-receipt.json');check(len(render['pages'])==10,'ten pages rendered');check(sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'unchanged branded CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'manifest raw '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'manifest normalized '+p)
print(str(len(checks))+' evidence checks passed.')
