"""Validate the scoped C31 implementation, preserved failures and exact normal gates."""
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
def skipmap(p):
 out={};reason=None
 for l in text(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',l)
  if m:reason=m[1]
  elif l.strip() and reason:check(l.strip() not in out,'unique skip '+l.strip());out[l.strip()]=reason
 return out
def totals(p):return [int(re.search(k+r':\s+(\d+)',text(p))[1]) for k in ['Passed','Failed','Skipped']]
for f in ['previous-identities.json','fixture-identities.json','binary-identities.json','baseline-binary-identities.json']:
 for p,h in load(E/f).items():check(sha(R/p)==h,'preserved '+p)
for p,h in load(E/'baseline-binary-identities.json').items():check(sha(R/'.guard-tmp/k04-copy-namespaces-baseline/bin/Release/net10.0'/Path(p).name)==h,'old binary actually executed '+Path(p).name)
ids=load(E/'source-identities.json')
check(len(ids)==14,'exact fourteen implementation/test/version files')
changed=subprocess.check_output(['git','diff','3a5017df','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines()
check(set(changed)<=set(ids),'only scoped implementation/test paths changed')
for p,h in ids.items():
 check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized implementation '+p)
 if p.endswith('.cs'):
  check('SPDX-License-Identifier: Apache-2.0' in text(R/p)[:1800],'source header '+p)
  if p!='tests/Bosak.XQuery.Tests/CopyNamespacesTests.cs':
   old=subprocess.check_output(['git','show','3a5017df:'+p],cwd=R,text=True,encoding='utf-8');pat=r'Charles Korthout \| (\d+(?:\.\d+)+)\s*\|';before=max(tuple(map(int,v.split('.'))) for v in re.findall(pat,old));after=tuple(map(int,re.findall(pat,text(R/p))[-1].split('.')));check(after>before,'bumped header history '+p)
check('<Version>1.31.0</Version>' in text(R/'src/Directory.Build.props'),'unpublished feature version 1.31.0')
check(text(E/'qualified-test-source.cs.txt')==text(R/'tests/Bosak.XQuery.Tests/CopyNamespacesTests.cs'),'same final regression source')
check(re.search(r'Failed:\s+67, Passed:\s+9,.*Total:\s+76',text(E/'regressions-before.log')) is not None,'old engine 67 failures / 9 passes')
check(re.search(r'Failed:\s+0, Passed:\s+76,.*Total:\s+76',text(E/'regressions-after.log')) is not None,'current engine 76 passes')
check('0 Error(s)' in text(E/'build.log'),'complete solution build')
s=text(E/'solution-tests.log');rr=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',s)
check(len(rr)==10 and sum(int(n) for n,_ in rr)==24010,'fresh solution 24010 / ten projects')
check(dict((p,int(n)) for n,p in rr)['Bosak.XQuery.Tests']==611,'XQuery 611 passes')
check(load(E/'solution-receipt.json')['exitCode']==0,'full solution exit zero')
for name,expected in [('qt3-normal',[31208,0,613]),('qt3-prototype',[31257,0,564]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,name+' exact totals')
a=skipmap(E/'qt3-normal-skips.txt');b=skipmap(E/'qt3-prototype-skips.txt');q=skipmap(E/'qt4-normal-skips.txt')
check(a==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt3-normal-skips.txt'),'normal QT3 frozen map unchanged')
check(q==skipmap(R/'docs/conformance/k04-context-routing-review/adoption/qt4-normal-skips.txt'),'normal qt4 frozen map unchanged')
selected=load(E/'selected-originals.json');names={x['Name'] for x in selected};check(len(names)==49,'49 unchanged originals')
check(set(a)-set(b)==names and not set(b)-set(a) and all(a[n]==b[n] for n in b),'only 49 isolated removals; 564 retained reasons unchanged')
originals=load(E/'original-receipts.json');check(len(originals)==147,'all original/resource probes')
for phase,expected in [('normal',{'Skipped':47,'Passed':2}),('prototype',{'Passed':49})]:
 rows=[x for x in originals if x['phase']==phase and x['mode']=='actual'];check(len(rows)==49 and {x['case'] for x in rows}==names,phase+' exact original cohort');check(collections.Counter(x['result']['kind'] for x in rows)==expected,phase+' original assertions')
old=[json.loads(l) for l in text(R/'docs/conformance/k04-prolog-mode-review/evidence/normal-affected.jsonl').splitlines()];old=[x for x in old if x['CopyNamespaces'] and not x['Construction']];check(old==load(E/'original-assertions.json'),'all original query/assertion snapshots unchanged')
controls=load(E/'control-receipts.json');check(len(controls)==20 and collections.Counter(x['result']['kind'] for x in controls)=={'Passed':16,'Failed':4},'public controls 16 pass; four construction failures retained')
check({x['name'] for x in controls if x['result']['kind']=='Failed'}=={'C30-construction-preserve','C30-construction-strip','C30-construction-duplicate','C30-construction-comment'},'construction contract still deferred')
check(load(E/'control-requests.json')==load(R/'docs/conformance/k04-prolog-mode-review/evidence/control-requests.json'),'frozen public control requests')
context=load(E/'context-receipts.json');prior={x['case']:x['result'] for x in load(R/'docs/conformance/k04-context-routing-review/evidence/original-receipts.json') if x['phase']=='prototype' and x['mode']=='actual'}
check(len(context)==43 and all(x['result']==prior[x['case']] for x in context),'43 admitted context result dictionaries unchanged')
for name,expected in [('qt3-set-normal',[44,0,0]),('qt3-set-prototype',[44,0,0]),('qt4-set-normal',[0,0,53])]:check(totals(E/(name+'.log'))==expected,name+' complete context set')
processes=originals+controls+context+load(E/'gate-receipts.json')+load(E/'set-receipts.json');check(len(processes)==216 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'216 final bounded diagnostic processes')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied diagnostic source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'identical harness source '+p)
 else:
  clean=''.join(l for l in text(R/p).splitlines(True) if 'Isolated C31 copy-namespaces review' not in l)
  check(clean==text(R/v['source']).replace(r'\bdeclare\s+(copy-namespaces|construction)\b',r'\bdeclare\s+(construction)\b'),'only copied copy-namespaces refusal removed plus history')
for p in (R/'.guard-tmp/k04-copy-namespaces-harness/bin/Release/net10.0').glob('Bosak*.dll'):
 if p.name!='Bosak.XPath.Conformance.dll':check(sha(p)==sha(R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0'/p.name),'identical prototype engine '+p.name)
check(load(E/'IMPLEMENTATION-PIN.json')['sourceBase'].startswith('3a5017df'),'pre-implementation adoption pin')
check(load(E/'MODULE-SCOPE-ADDENDUM-PIN.json')['contract'],'prerequisite module-scope adoption addendum')
if (E/'render-receipt.json').exists():
 r=load(E/'render-receipt.json');check(len(r['pages'])==10,'ten rendered pages');check(sha(R/'assets/css/fytala-markdown.css')==r['cssSha256'],'unchanged branded CSS')
 for x in r['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(str(len(checks))+' evidence checks passed.')
