"""Validate C34 immutable inputs, final gates, original assertions and regressions."""
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
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:out[s.strip()]=reason
 return out
for name in ['previous-identities.json','fixture-identities.json','baseline-binary-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
for p,h in load(E/'source-identities.json').items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'source '+p)
for name,expected in [('qt3-normal',[31257,0,564]),('qt3-prototype',[31340,0,481]),('qt4-normal',[8939,0,1920])]:check(totals(E/(name+'.log'))==expected,'exact '+name)
a=skips(E/'qt3-normal-skips.txt');b=skips(E/'qt3-prototype-skips.txt');q=skips(E/'qt4-normal-skips.txt')
A=R/'docs/conformance/k04-copy-namespaces-admission/adoption'
check(a==skips(A/'qt3-normal-skips.txt') and q==skips(A/'qt4-normal-skips.txt'),'both approved skip maps unchanged')
check(len(a)==564 and len(b)==481 and set(b)<=set(a) and all(a[k]==v for k,v in b.items()),'83 isolated removals / 481 reasons retained')
originals=load(E/'original-receipts.json')
check(len(originals)==261,'261 original processes')
direct=[x for x in originals if x['phase']=='prototype' and x['mode']=='actual']
check(len(direct)==87 and all(x['result']['kind']=='Passed' for x in direct),'87 original assertions pass')
for name,n in [('copy',49),('context',43),('control',8)]:
 rows=load(E/(name+'-receipts.json'))
 check(len(rows)==n and all(x['result']['kind']=='Passed' for x in rows),name+' compatibility passes')
for name in ['original','copy','context','control','gate']:
 for x in load(E/(name+'-receipts.json')):check(x['exitCode']==0 and not x['timeout'],'bounded '+x.get('name','process'))
t=load(E/'typing-results.json');check(t['sourceUnchanged'] and len(t['outcomes'])==3 and all(x['source']==x['copied'] and not x['sameNode'] for x in t['outcomes']),'three typed copies preserve complete observations and identity separation')
check(totals(E/'regressions-after.log')==[73,0,0] and totals(E/'regressions-before.log')==[6,67,0],'73-test old/new cohort')
s=read(E/'solution-tests.log');rows=re.findall(r'Passed!\s+- Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+)',s)
check(len(rows)==10 and sum(int(x[1]) for x in rows)==24082 and all(x[0]==x[2]=='0' for x in rows),'24082 solution passes across ten projects')
old=read(E/'previous-copy-tests.cs.txt');new=read(R/'tests/Bosak.XQuery.Tests/CopyNamespacesTests.cs')
old=re.sub(r'^.*declare construction strip; 1.*\n','',old,flags=re.M)
strip=lambda x:re.sub(r'^//.*\n','',x,flags=re.M)
check(strip(old)==strip(new),'only obsolete copy-unit case removed outside history')
for p in load(E/'source-identities.json'):
 if p.endswith('.cs'):
  s=read(R/p);check(s.startswith('// ====') and 'SPDX-License-Identifier: Apache-2.0' in s and '28-10-2026' in s,'source header '+p)
for p in ['AGENTS.md','tests/Bosak.XPath.Conformance/TestExecutor.cs','tests/Bosak.XPath.Conformance40/Program.cs']:
 check(not subprocess.check_output(['git','diff','11bb1c1e','--',p],cwd=R,text=True).strip(),'unchanged '+p)
for suite in ['qt3tests','qt4tests']:
 check(not subprocess.check_output(['git','-C',str(R/'tests'/suite),'status','--porcelain'],text=True).strip(),'clean frozen '+suite)
manifest=E/'MANIFEST.json'
if manifest.exists():
 m=load(manifest)
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(json.dumps({'status':'passed','checks':len(checks),'solutionPassed':24082,'originalsPassed':87,'regressionsPassed':73}))
