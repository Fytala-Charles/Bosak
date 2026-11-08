"""Validate the qualified C28 scope, immutable evidence and execution receipts."""
from pathlib import Path
import hashlib,json,re,subprocess
E=Path(__file__).resolve().parent
R=E.parents[3]
D=E.parent
checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def text(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def skipmap(p):
 result={};reason=None
 for line in text(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',line)
  if m:reason=m[1]
  elif line.strip() and reason:result[line.strip()]=reason
 return result
def totals(p):
 s=text(p);return [int(re.search(k+r':\s+(\d+)',s)[1]) for k in ['Passed','Failed','Skipped']]
for p,h in load(E/'previous-identities.json').items():check(sha(R/p)==h,'C27 preserved '+p)
for f in ['baseline-binary-identities.json','binary-identities.json']:
 for p,h in load(E/f).items():check(sha(R/p)==h,'binary '+p)
for p,h in load(E/'baseline-binary-identities.json').items():
 q=R/'.guard-tmp/k04-library-context-baseline/bin/Release/net10.0'/Path(p).name
 check(sha(q)==h,'old DLL actually executed '+q.name)
for p,h in load(E/'source-identities.json').items():
 check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'source '+p)
 if p.endswith('.cs'):check('SPDX-License-Identifier: Apache-2.0' in text(R/p)[:1800],'header '+p)
check(text(E/'qualified-test-source.cs.txt')==text(R/'tests/Bosak.XQuery.Tests/LibraryContextTypeScopeTests.cs'),'identical qualified test cohort')
check(re.search(r'Failed:\s+52, Passed:\s+60,.*Total:\s+112',text(E/'regressions-qualified-before.log')) is not None,'old engine 52 failures / 60 passes')
check(re.search(r'Failed:\s+0, Passed:\s+112,.*Total:\s+112',text(E/'regressions-after.log')) is not None,'current engine 112 passes')
s=text(E/'solution-tests.log');counts=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',s)
check(len(counts)==10 and sum(int(n) for n,_ in counts)==23934,'full solution 23934 / ten projects')
check(dict((p,int(n)) for n,p in counts)['Bosak.XQuery.Tests']==535,'XQuery 535 passes')
for suite,expected in [('qt3',[31165,0,656]),('qt4',[8939,0,1920])]:
 check(totals(E/(suite+'.log'))==expected,suite+' exact frozen gate')
 check(skipmap(E/(suite+'-skips.txt'))==skipmap(R/'docs/conformance/k04-normalization-admission/adoption'/(suite+'-gate-skips.txt')),suite+' unchanged skip names/reasons')
for log,expected in [('qt3-set-normal',[4,0,40]),('qt3-set-prototype',[44,0,0]),('qt4-set-normal',[0,0,53])]:check(totals(E/(log+'.log'))==expected,log+' exact totals')
for f,count in [('original-receipts',39),('control-receipts',12),('execution-receipts',80),('library-receipts',5),('set-receipts',3),('normal-receipts',2),('gate-receipts',2)]:
 rows=load(E/(f+'.json'));check(len(rows)==count,f+' count')
 check(all(x['exitCode']==0 and not x['timeout'] for x in rows),f+' successful bounded processes')
for f in ['execution-receipts','library-receipts']:
 old=load(R/'docs/conformance/k04-main-context/evidence'/(f+'.json'));new=load(E/(f+'.json'))
 check([x['result'] for x in old]==[x['result'] for x in new],f+' C26 unchanged results')
check((E/'filter-results.jsonl').read_bytes()==(R/'docs/conformance/k04-main-context/evidence/filter-results.jsonl').read_bytes(),'97 unchanged filter assessments')
check(all(x['result']['kind']=='Passed' for x in load(E/'control-receipts.json')),'all twelve C27 controls pass')
rows=load(E/'original-receipts.json');proto=[x for x in rows if x.get('phase')=='prototype']
check(len(proto)==13 and all(x['result']['kind']=='Passed' for x in proto),'all thirteen originals pass')
check(load(E/'control-requests.json')==load(R/'docs/conformance/k04-context-resource-review/evidence/control-requests.json'),'original controls unchanged')
for p,v in load(E/'copied-source-identities.json').items():
 check(sha(R/p)==v['sha256'],'copied diagnostic source '+p)
 if not v['changed']:check(sha(R/p)==sha(R/v['source']),'unmodified harness source '+p)
render=load(E/'render-receipt.json');check(len(render['pages'])==9,'nine rendered pages')
check(sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'unchanged branded CSS')
for page in render['pages']:check(sha(R/page['html'])==page['sha256'],'render '+page['source'])
if (E/'manifest.json').exists():
 m=load(E/'manifest.json')
 for p,h in m['raw'].items():check(sha(R/p)==h,'manifest raw '+p)
 for p,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'manifest normalized '+p)
allowed=set(load(E/'source-identities.json'))
changed=subprocess.check_output(['git','diff','HEAD','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines()
check(set(changed)<=allowed,'only pinned production source paths changed')
check('1.30.1' in text(R/'src/Directory.Build.props'),'candidate patch version')
check(load(E/'filter-receipt.json')['exitCode']==0,'filter probe exit zero')
for name in ['qt3','qt4']:
 check(skipmap(E/(name+'-set-normal-skips.txt'))==skipmap(R/'docs/conformance/k04-main-context/evidence'/(name+'-normal-skips.txt')),name+' unchanged normal context skips')
print(str(len(checks))+' evidence checks passed.')
