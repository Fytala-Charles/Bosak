"""Verify the exact owner-approved C32 adoption and fresh normal execution evidence."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
A=Path(__file__).resolve().parent;D=A.parent;E=D/'evidence';R=A.parents[3];checks=[]
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
for p,h in load(A/'review-identities.json').items():check(sha(R/p)==h,'original review preserved '+p)
proposal=load(E/'proposal-identities.json')
for file in proposal['baseNormalized']:
 p=E/('proposed-'+Path(file).name+'.txt')
 check(sha(p)==proposal['proposedRaw'][p.name] and (R/file).read_bytes()==p.read_bytes(),'exact approved source '+file)
check(load(A/'approval.json')['answer']=='Approve exact 49-case adoption (Recommended)','explicit exact owner approval')
changed=subprocess.check_output(['git','diff','451f596a','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).splitlines()
check(set(changed)==set(proposal['baseNormalized']),'only approved two production paths changed')
for file,h in load(E/'normal-source-identities.json').items():
 if file not in proposal['baseNormalized']:check(sha(R/file)==h,'retained production source '+file)
for file,h in load(E/'fixture-identities.json').items():check(sha(R/file)==h,'frozen fixture '+file)
for suite,revision in load(E/'REVIEW-PIN.json')['fixtureRevisions'].items():
 folder=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(folder),'rev-parse','HEAD'],text=True).strip()==revision,'frozen revision '+suite)
 check(not subprocess.check_output(['git','-C',str(folder),'status','--porcelain'],text=True).strip(),'clean fixture '+suite)
for p,h in load(E/'engine-source-identities.json').items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'unchanged delivered engine source '+p)
check('0 Error(s)' in text(A/'build.log'),'complete solution build green')
check(totals(A/'qt3-normal.log')==[31257,0,564],'adopted QT3 totals')
check(totals(A/'qt4-normal.log')==[8939,0,1920],'unchanged qt4 totals')
a=skipmap(E/'qt3-normal-skips.txt');b=skipmap(A/'qt3-normal-skips.txt')
check(len(b)==564 and set(a)-set(b)==set(load(E/'skip-delta.json')['removed']) and not set(b)-set(a),'exact 49 normal skip removals')
check(all(a[n]==b[n] for n in b),'all 564 retained normal skip reasons unchanged')
check(b==skipmap(E/'qt3-prototype-skips.txt'),'adopted normal map equals isolated rehearsal')
check(skipmap(A/'qt4-normal-skips.txt')==skipmap(E/'qt4-normal-skips.txt'),'all qt4 skip names/reasons unchanged')
check('Experimental-level (XPath40Experimental) passes: 11' in text(A/'qt4-normal.log'),'eleven experimental passes retained')
for x in load(A/'gate-receipts.json'):check(x['exitCode']==0 and not x['timeout'],'normal gate bounded exit '+x['name'])
check(load(A/'inventory-receipt.json')['exitCode']==0,'fresh normal catalog inventory exit')
check(lines(A/'adopted-catalog.jsonl')==lines(E/'prototype-catalog.jsonl'),'all 31821 adopted catalog decisions equal rehearsal')
check(lines(A/'adopted-controls.jsonl')==lines(E/'prototype-controls.jsonl'),'all 37 adopted lexical controls equal rehearsal')
check(lines(A/'adopted-affected.jsonl')==lines(E/'prototype-affected.jsonl'),'all 49 adopted originals and assertions equal rehearsal')
s=text(A/'solution-tests.log');rows=re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',s)
check(len(rows)==10 and sum(int(n) for n,_ in rows)==24010,'fresh solution 24010 passes / ten projects')
projects=dict((p,int(n)) for n,p in rows)
check(projects['Bosak.XQuery.Tests']==611 and projects['Bosak.XPath.Standard.Tests']==13797 and projects['Bosak.Xslt.Tests']==8196,'XQuery Standard and XSLT totals')
check(load(A/'solution-receipt.json')['exitCode']==0,'full solution process exit zero')
for f,h in load(A/'binary-identities.json').items():check(sha(R/f)==h,'adopted binary '+f)
comparison=load(A/'binary-comparison.json');check(len(comparison)==9,'nine engine assembly comparisons')
for row in comparison:
 old,new=row['review'],row['adopted']
 check(old['sha256']==load(E/'binary-identities.json')[old['path']],'review binary witness pinned '+row['name'])
 for entry in [old,new]:check(sha(R/entry['path'])==entry['sha256'],'binary comparison identity '+entry['path'])
 check(old['methodAndSignatureSha256']==new['methodAndSignatureSha256'] and old['methodBodies']==new['methodBodies'] and old['assemblyVersion']==new['assemblyVersion'],'unchanged engine methods/signatures '+row['name'])
 check(old['informationalVersion'].endswith('+3a5017df32289bd3a519677629c3eb3d1756ecc0') and new['informationalVersion'].endswith('+451f596a26b3cd8f6197aee5c0d1ad2d203d2841'),'recorded source-commit provenance '+row['name'])
if (A/'render-receipt.json').exists():
 render=load(A/'render-receipt.json');check(len(render['pages'])==9,'nine adoption pages rendered');check(sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'branded CSS retained')
 for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'adoption render '+x['source'])
if (A/'manifest.json').exists():
 m=load(A/'manifest.json')
 for f,h in m['raw'].items():check(sha(R/f)==h,'adoption manifest raw '+f)
 for f,h in m['normalized'].items():check(subprocess.check_output(['git','hash-object',f],cwd=R,text=True).strip()==h,'adoption manifest normalized '+f)
print(str(len(checks))+' adoption evidence checks passed.')
