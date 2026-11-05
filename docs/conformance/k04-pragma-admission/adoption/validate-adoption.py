"""Verify the exact approved C38 adoption and fresh normal gate evidence."""
from pathlib import Path
import json,hashlib,re,subprocess
A=Path(__file__).resolve().parent;D=A.parent;E=D/'evidence';R=A.parents[3];checks=[]
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
for p,h in load(A/'previous-identities.json').items():check(sha(R/p)==h,'historical proposal preserved '+p)
check(load(A/'approval.json')['answer']=='Approve exact 55-case pragma adoption (Recommended)','exact owner approval')
approved={'tests/Bosak.XPath.Conformance/TestExecutor.cs':'proposed-executor.cs.txt','AGENTS.md':'proposed-AGENTS.md.txt'}
for p,snapshot in approved.items():check((R/p).read_bytes()==(E/snapshot).read_bytes(),'byte-exact proposed production source '+p)
changed=subprocess.check_output(['git','diff','124a73d1','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines()
check(set(changed)==set(approved),'only exact two production files changed')
for name in ['fixture-identities.json','normal-source-identities.json']:
 for p,h in load(E/name).items():
  if p not in approved:check(sha(R/p)==h,'retained source/fixture '+p)
for suite,revision in load(R/'docs/conformance/k04-pragma-review/evidence/REVIEW-PIN.json')['fixtureRevisions'].items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
check(totals(A/'qt3-normal.log')==[31395,0,426] and totals(A/'qt4-normal.log')==[8939,0,1920],'exact fresh normal gates')
old=skips(E/'qt3-normal-skips.txt');new=skips(A/'qt3-normal-skips.txt');qt4=skips(A/'qt4-normal-skips.txt')
check(new==skips(E/'qt3-prototype-skips.txt') and qt4==skips(E/'qt4-normal-skips.txt'),'normal maps equal exact approved rehearsal')
delta=load(E/'skip-delta.json')
check(len(new)==426 and set(old)-set(new)==set(delta['removed']) and len(delta['removed'])==55 and set(new)<=set(old) and all(old[k]==v for k,v in new.items()),'exact 55 removals / retained reasons')
check(all(x in new for x in delta['excluded']),'two independent exact-XQ10 pragma exclusions retained')
catalog=lines(A/'adopted-catalog.jsonl');qualified=lines(E/'prototype-catalog.jsonl')
check(len(catalog)==31821 and catalog==qualified and len({x['Set'] for x in catalog})==428,'complete normal routing equals qualification')
check(lines(A/'adopted-affected.jsonl')==lines(E/'prototype-affected.jsonl'),'all original expression/assertion/resource snapshots equal')
check(lines(A/'adopted-originals.jsonl')==lines(E/'prototype-originals.jsonl'),'all 60 original query/assertion/resource records equal')
index={x['Name']:x for x in catalog}
for name,path,count,eligible in [
 ('copy','docs/conformance/k04-copy-namespaces-admission/evidence/selected-originals.json',49,49),
 ('context','docs/conformance/k04-context-routing-review/evidence/selected-originals.json',43,43),
 ('construction','docs/conformance/k04-construction-admission/evidence/selected-originals.json',87,83)]:
 cohort=load(R/path)
 check(len(cohort)==count and sum(index[x['Name']]['xpathSupported'] or index[x['Name']]['xquerySupported'] and index[x['Name']]['actual'] for x in cohort)==eligible,name+' runner admission and independent exclusions preserved')
remaining=[index[n] for n,r in new.items() if r=='Unsupported dependency']
check(len(remaining)==370 and all(x['actual'] and not x['xpathSupported'] and not x['xquerySupported'] for x in remaining),'370 remaining dependency skips independently excluded with routing allowed')
check(lines(A/'adopted-controls.jsonl')==lines(E/'prototype-controls.jsonl') and len(lines(A/'adopted-controls.jsonl'))==16,'all 16 actual lexical controls match')
rows=load(A/'gate-receipts.json')+[load(A/'inventory-receipt.json')]
check(all(x['exitCode']==0 and not x['timeout'] for x in rows),'bounded normal gates and inventory pass')
s=read(A/'solution-tests.log');tests=re.findall(r'Passed!\s+- Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+)',s)
check(len(tests)==10 and sum(int(x[1]) for x in tests)==24164 and all(x[0]==x[2]=='0' for x in tests),'24164 fresh solution tests / ten projects')
check(load(A/'solution-receipt.json')['exitCode']==0,'full solution process exit zero')
check('0 Error(s)' in read(A/'build.log'),'complete build zero errors')
comparison=load(A/'binary-comparison.json');check(len(comparison)==9 and all(x['review']['methodAndSignatureSha256']==x['adopted']['methodAndSignatureSha256'] and x['review']['assemblyVersion']==x['adopted']['assemblyVersion'] for x in comparison),'nine engine implementation/signature comparisons')
for p,h in load(A/'binary-identities.json').items():check(sha(R/p)==h,'final binary '+p)
for p,h in load(A/'adopted-source-identities.json').items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'adopted source '+p)
check('Experimental-level (XPath40Experimental) passes: 11' in read(A/'qt4-normal.log'),'11 experimental passes retained')
check(len(re.findall(r'^  Starting:',read(A/'qt4-normal.log'),re.M))==230,'230 gated sets retained')
render=load(A/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten rendered pages with unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (A/'MANIFEST.json').exists():
 m=load(A/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw manifest '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized manifest '+p)
print(json.dumps({'status':'passed','checks':len(checks),'solutionPassed':24164,'qt3':[31395,0,426],'qt4':[8939,0,1920],'removedSkips':55}))
