"""Validate the explicitly approved exact two-case QT3 admission update."""
from pathlib import Path
import json,hashlib,re,subprocess,sys
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
def log(n):
 b=(E/n).read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
m=load(E/'manifest.json');approval=load(E/'owner-approval.json');adopted=load(C/'BASELINE-ADOPTION.json')
assert approval['decision']=='Approved' and approval['answer']=='Approve the exact two-case baseline update (Recommended)'
assert approval['presentedCommit']==m['sourceBase']==adopted['sourceBase']
proposal=git('show',m['sourceBase']+':docs/conformance/k01-schema-list/BASELINE-UPDATE-PROPOSAL.md')
assert proposal==(E/'proposal-as-presented.md').read_bytes() and sha(proposal)==approval['proposalSha256']
assert git('rev-parse',m['sourceBase']+':docs/conformance/k01-schema-list/BASELINE-UPDATE-PROPOSAL.md').decode().strip()==approval['proposalGitBlob']
assert (C/'BASELINE-UPDATE-PROPOSAL.md').read_bytes().replace(b'\r\n',b'\n')==proposal.replace(b'\r\n',b'\n')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,n
for n,h in m['tools'].items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['previousFiles'].items():assert sha((R/n).read_bytes())==h,n
previous=load(C/'evidence/manifest.json')
for n,h in previous['sourceFiles'].items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h,n
runner='tests/Bosak.XPath.Conformance/ConformanceRunner.cs';old=git('show',m['sourceBase']+':'+runner).decode('utf-8').replace('\r\n','\n');current=(R/runner).read_text(encoding='utf-8')
names=['CastAs-ListType-20','CastAs-ListType-28'];assert approval['removedKnownSkips']==names==adopted['removedDocumentedSkips']
for name in names:
 assert len(re.findall(r'^\s*\["'+re.escape(name)+r'"\].*$',old,re.M))==1
 old=re.sub(r'^\s*\["'+re.escape(name)+r'"\].*\n','',old,flags=re.M)
 assert '["'+name+'"]' not in current
def code(t):return re.sub(r'\s+',' ',re.sub(r'^\s*//[^\n]*$','',t,flags=re.M)).strip()
assert code(old)==code(current),'Unexpected executable harness change'
changed=git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines();assert changed==[runner],changed
agents=(R/'AGENTS.md').read_text(encoding='utf-8');before=git('show',m['sourceBase']+':AGENTS.md').decode('utf-8').replace('\r\n','\n')
oldRule=next(l for l in before.splitlines() if l.startswith('- **QT3'));newRule=next(l for l in agents.splitlines() if l.startswith('- **QT3'))
assert '**31,144/0/677**' in newRule and before.replace(oldRule,newRule)==agents
assert approval['approvedQt3']==adopted['currentQt3']==[31144,0,677] and approval['unchangedQt4']==adopted['qt4']['results']==[8436,0,1894]
assert adopted['qt3FreshRun'] is True and adopted['qt4']['freshRun'] is False and adopted['solutionTests']['freshRun'] is False
text=log('qt3-gate.log')
for key,count in [('Total',31821),('Passed',31144),('Failed',0),('Skipped',677)]:assert re.search(r'^'+key+r':\s+'+str(count)+r'\s*$',text,re.M),key
assert '0 Error(s)' in log('build.log')
oldSkips={v['Name']:v['SkipReason'] for v in load(R/'docs/xpath40-catalog/evidence/closure-triage/skip-cases.json') if v['Suite']=='qt3'}
fresh={};reason=None
for line in (E/'qt3-skips.txt').read_text(encoding='utf-8-sig').splitlines():
 match=re.match(r'^=== (\d+) skipped: (.*?) ===$',line)
 if match:reason=match.group(2)
 elif line.strip():assert line not in fresh and reason is not None;fresh[line]=reason
assert len(oldSkips)==679 and len(fresh)==677 and set(oldSkips)-set(fresh)==set(names) and not set(fresh)-set(oldSkips)
assert all(oldSkips[n]==reason for n,reason in fresh.items())
delta=load(E/'skip-delta.json');assert delta['current']==fresh and delta['removed']==names and not delta['added'] and not delta['changedReasons']
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(C/'ADOPTION.json')['qt3FixtureRevision']
assert not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
# The engine and qt4 sources are unchanged; their verified pre-adoption receipts are inherited.
assert adopted['solutionTests']['passed']==previous['validation']['solutionTests']['passed']==21816
provenance=load(E/'binary-provenance.json')
for key in ['current','priorTested']:
 v=provenance[key];b=Path(v['path']).read_bytes();assert sha(b)==v['sha256'] and [q.decode('ascii') for q in re.findall(rb'1\.26\.4\+[0-9a-f]{40}',b)]==v['informationalVersions']
assert provenance['current']['informationalVersions']==['1.26.4+'+m['sourceBase']]
assert provenance['priorTested']['informationalVersions']==['1.26.4+'+previous['sourceBase']]
assert sha(Path(m['harnessBinary']['path']).read_bytes())==m['harnessBinary']['sha256']
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;t=p.read_text(encoding='utf-8');targets=re.findall(r'\]\(([^)]+)\)',t) if n.endswith('BASELINE-ADOPTION.md') else re.findall(r'\[Exact two-case adoption\]\(([^)]+)\)',t)
 for target in targets:
  if not re.match(r'^[A-Za-z][A-Za-z0-9+.-]*:',target) and not target.startswith('#'):assert (p.parent/target.split('#')[0]).exists(),(n,target)
print('K01 adoption verified: explicit approval;QT3 31,144/0/677;exactly two skip removals;all677 remaining names/reasons unchanged;engine and qt4 unchanged;original receipts preserved.')
