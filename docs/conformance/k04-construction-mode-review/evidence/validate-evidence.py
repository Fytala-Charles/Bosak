"""Validate failed construction qualification and independent typed-copy observations."""
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
def sm(p):
 out={};reason=None
 for l in text(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',l)
  if m:reason=m[1]
  elif l.strip() and reason:check(l.strip() not in out,'unique skip '+l.strip());out[l.strip()]=reason
 return out
def totals(p):return [int(re.search(k+r':\s+(\d+)',text(p))[1]) for k in ['Passed','Failed','Skipped']]
for f in ['previous-identities.json','fixture-identities.json','normal-source-identities.json','binary-identities.json']:
 for p,h in load(E/f).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','HEAD','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'no production/fixture/baseline change')
for p,h in load(R/'docs/conformance/k04-copy-namespaces-admission/adoption/binary-identities.json').items():check(sha(R/p)==h,'same delivered normal binary '+p)
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 folder=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(folder),'rev-parse','HEAD'],text=True).strip()==revision,'fixture revision '+suite)
 check(not subprocess.check_output(['git','-C',str(folder),'status','--porcelain'],text=True).strip(),'clean fixture '+suite)
for name,expected in [('qt3-normal',[31257,0,564]),('qt3-prototype',[31261,79,481]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,name+' exact totals')
a=sm(E/'qt3-normal-skips.txt');b=sm(E/'qt3-prototype-skips.txt');q=sm(E/'qt4-normal-skips.txt')
check(a==sm(R/'docs/conformance/k04-copy-namespaces-admission/adoption/qt3-normal-skips.txt'),'normal QT3 approved map unchanged')
check(q==sm(R/'docs/conformance/k04-copy-namespaces-admission/adoption/qt4-normal-skips.txt'),'normal qt4 approved map unchanged')
check(len(a)==564 and len(b)==481 and not set(b)-set(a) and all(a[n]==b[n] for n in b),'83 isolated removals / 481 retained reasons unchanged')
normal=lines(E/'normal-catalog.jsonl');proto=lines(E/'prototype-catalog.jsonl')
check(len(normal)==len(proto)==31821 and len({x['Set'] for x in normal})==428,'complete catalog')
for x,y in zip(normal,proto):
 check(x['actual']==x['before'] and y['actual']==y['after'],'actual normal/prototype predicate '+x['Name'])
 x=dict(x);y=dict(y);x.pop('actual');y.pop('actual');check(x==y,'source/dependency identity '+x['Name'])
index={x['Name']:x for x in normal};affected=lines(E/'normal-affected.jsonl');names={x['row']['Name'] for x in affected}
check(len(affected)==87 and len({x['row']['Set'] for x in affected})==10,'87 originals across ten sets')
old=[json.loads(l) for l in text(R/'docs/conformance/k04-prolog-mode-review/evidence/normal-affected.jsonl').splitlines()];old={x['row']['Name']:x for x in old if x['Construction']}
check(set(old)==names and sum(x['CopyNamespaces'] for x in old.values())==12,'exact 75 construction-only / 12 mixed C30 cohort')
for x in affected:check(all(x[k]==old[x['row']['Name']][k] for k in ['Expression','Assertion','Modules','Environment','Dependencies']),'original expression/assertion/resources unchanged '+x['row']['Name'])
copy=load(R/'docs/conformance/k04-copy-namespaces/evidence/selected-originals.json');context=load(R/'docs/conformance/k04-context-routing-review/evidence/selected-originals.json')
check(len(copy)==49 and all(index[x['Name']]['before'] and index[x['Name']]['after'] for x in copy),'49 copy-only admissions retained')
check(len(context)==43 and all(index[x['Name']]['before'] and index[x['Name']]['after'] for x in context),'43 context admissions retained')
excluded={x['row']['Name'] for x in affected if not x['row']['xquerySupported']};check(excluded=={'K-VersionProlog-5','XQTY0086_1','XQTY0086_2','XQTY0086_3'},'four independent exact-XQ10 exclusions')
check(set(a)-set(b)==names-excluded,'every isolated removal reconciled')
rows=load(E/'original-receipts.json');check(len(rows)==261,'261 original/resource processes')
for phase,expected in [('normal',{'Passed':3,'Skipped':84}),('prototype',{'Passed':4,'Failed':83})]:
 rr=[x for x in rows if x['phase']==phase and x['mode']=='actual'];check(len(rr)==87 and {x['case'] for x in rr}==names,phase+' exact cohort');check(collections.Counter(x['result']['kind'] for x in rr)==expected,phase+' exact assertions')
failed={x['case'] for x in rows if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'};passed=names-failed
check(passed=={'K-ConstructionProlog-2','qischema90007','qischema90008','version_declaration-005'},'four accepted alternatives named')
check(all('XPST0003' in x['result']['message'] for x in rows if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'),'all direct failures reach unsupported parser')
gateFailed=set(re.findall(r'^  FAIL ([^:]+):',text(E/'qt3-prototype.log'),re.M));check(len(gateFailed)==79 and gateFailed==failed-excluded,'all 79 gated failures reconcile with direct probes')
controls=load(E/'control-receipts.json');check(len(controls)==8 and collections.Counter(x['result']['kind'] for x in controls)=={'Passed':4,'Failed':4},'public controls 4/4')
check({x['name'] for x in controls if x['result']['kind']=='Failed'}=={'C33-strip','C33-preserve','C33-duplicate','C33-mixed'},'valid modes and XQST0067 remain blocked')
for phase in ['normal','prototype']:
 rr=lines(E/(phase+'-controls.jsonl'));check(len(rr)==19 and all(x['passed'] for x in rr),phase+' nineteen lexical controls')
 for req,result in zip(load(E/'lexical-controls.json'),rr):check(req['Name']==result['Name'] and req['Expression']==result['Expression'] and req['Before']==result['before'] and req['After']==result['after'],phase+' lexical identity '+req['Name'])
gates=load(E/'gate-receipts.json');check({x['name']:x['exitCode'] for x in gates}=={'qt3-normal':0,'qt3-prototype':2,'qt4-normal':0},'failed isolated exit 2 preserved')
processes=rows+controls+gates+load(E/'inventory-receipts.json')+[load(E/'typing-receipt.json'),load(E/'version-early-error-receipt.json')]
check(len(processes)==276 and all(not x['timeout'] and x['exitCode']==(2 if x.get('name')=='qt3-prototype' else 0) for x in processes),'276 bounded diagnostic exits')
check(load(E/'version-early-error-receipt.json')['result']['kind']=='Passed','earlier XQST0038 alternative independently reproduced')
typing=load(E/'typing-results.json');check(typing['schemaValidated'] and typing['sourceUnchanged'] and len(typing['outcomes'])==3,'validated typed source unchanged across three paths')
for row in typing['outcomes']:
 src={(x['kind'],x['name']):x for x in row['source']};dst={(x['kind'],x['name']):x for x in row['copied']};check(not row['sameNode'],'distinct copied identity '+row['constructor'])
 check(src['Element','root']['type']==dst['Element','root']['type']=='Q{urn:c33}Root','root type retained '+row['constructor'])
 check(src['Element','count']['type'].endswith('}int') and dst['Element','count']['type'] is None,'descendant type lost '+row['constructor'])
 check(len(src['Element','refs']['typedValues'])==2 and len(dst['Element','refs']['typedValues'])==1,'IDREFS cardinality changed '+row['constructor'])
 check(src['Element','nil']['nilled'] and not dst['Element','nil']['nilled'] and not src['Element','nil']['typedValues'] and len(dst['Element','nil']['typedValues'])==1,'nilled/empty typed value lost '+row['constructor'])
 check(src['Attribute','id']['isId'] and dst['Attribute','id']['isId'] and dst['Attribute','id']['type'] is None,'ID flag retained while type lost '+row['constructor'])
 check(src['Attribute','qname']['type'].endswith('}QName') and dst['Attribute','qname']['type'] is None,'QName annotation lost '+row['constructor'])
check('Bosak.Schema' not in text(E/'typing-project.csproj.txt'),'typed witness uses existing core interface without add-on dependency')
spec=load(E/'reviewed-specification-excerpts.json');check('implementation-defined default' in spec['sections'][0]['text'] and 'XQST0067' in spec['sections'][0]['text'],'implementation-defined default and duplicate rule pinned')
for x in load(E/'schema-context.json')['documents']:check(sha(R/x['snapshot'])==x['sha256'],'exact sibling document snapshot '+x['source'])
check('`Proposed`' in text(E/'schema-context/ADR-006-xsd11-engine-seam-compatibility.md.txt'),'schema callback ADR remains proposed')
check('Pending' in text(E/'core-req139-snapshot.txt') and '- [ ] ADR-006 accepted' in text(E/'core-req139-snapshot.txt'),'core REQ-139 remains pending/unaccepted')
check(text(E/'core-req139-snapshot.txt') in text(R/'docs/FEATURE_REQUESTS.md'),'REQ-139 section unchanged by this review')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'unchanged harness source '+p)
 else:
  clean=''.join(l for l in text(R/p).splitlines(True) if 'Isolated C33 construction review' not in l)
  check(clean==text(R/v['source']).replace(r'\bdeclare\s+(construction)\b',r'(?!)'),'only construction refusal and history changed')
for p in (R/'.guard-tmp/k04-construction-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'byte-identical isolated engine '+p.name)
r=load(E/'render-receipt.json');check(len(r['pages'])==10,'ten rendered pages');check(sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'unchanged branded CSS')
for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(str(len(checks))+' evidence checks passed.')
