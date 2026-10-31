"""Validate the C13 owner-decision packet without accepting its proposal."""
from pathlib import Path
import hashlib,json,re,subprocess,sys,xml.etree.ElementTree as ET
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def norm(p):return p.read_bytes().replace(b'\r\n',b'\n')
def git(*args):return subprocess.check_output(['git',*args],cwd=R)
def section(t,n):return re.search(r'^## '+str(n)+r'\.[^\n]*\n(.*?)(?=^## \d+\.|\Z)',t,re.M|re.S).group(1).strip()
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,'index '+n
for n,h in m['tools'].items():assert git('hash-object',n).decode().strip()==h,n
for n,h in m['previousCheckpoints'].items():assert sha((C/n).read_bytes())==h,n
for n,v in load(E/'review-inputs.json').items():
 assert sha((C/n).read_bytes())==v['sha256'] and git('hash-object',(C/n).relative_to(R).as_posix()).decode().strip()==v['gitBlob'],n
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==v['gitBlob']
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props','assets','.crossnote'],cwd=R,check=True,stdout=subprocess.DEVNULL)
command=[sys.executable,str(C/'evidence/c12-semantic-batch4/validate-evidence.py')]
if '--local-artifacts' in sys.argv:command.append('--local-artifacts')
subprocess.run(command,cwd=R,check=True)
before={n:sha(norm(C/n)) for n in m['outputs']};assert before==m['outputs']
subprocess.run([sys.executable,str(C/'build-c13-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
assert before=={n:sha(norm(C/n)) for n in m['outputs']}
p=load(C/'C13-ACCEPTANCE-PROPOSAL.json');review=load(C/'FINAL-CLOSURE-REVIEW.json');plan=load(C/'CLOSURE-PLAN.json')
assert p['acceptanceReceipt'] is None and review['acceptanceReceipt'] is None and p['status']=='Prepared for owner decision; not approved'
assert plan['openClosureWorkpackages']==1 and next(v for v in plan['workpackages'] if v['id']=='C13')['status']=='Open'
assert all(v=='Open/partial' for k,v in plan['acceptance'].items() if k!='AC05')
registry=(R/'docs/FEATURE_REQUESTS.md').read_text(encoding='utf-8');scope=registry.split('### REQ-126:',1)[1].split('\n### REQ-',1)[0]
assert '**Status:** In Progress' in scope and all('- [ ] '+id in scope for id in ['AC-01','AC-02','AC-03','AC-04']) and '- [x] AC-05' in scope
sources=load(E/'entry-sources.json');corrections=load(E/'documentation-corrections.json');corrected={v['entry']:v for v in corrections};assert len(corrected)==6 and len(sources)==76
reports=[load(C/f'SEMANTIC-REVIEW-BATCH-{i}.json') for i in range(1,5)];entries={v['entry']:v for d in reports for v in d['entries']}
assert len(entries)==76 and set(entries)==set(sources)==set(review['entries']) and sum(len(v['contracts']) for v in entries.values())==235
for n,v in sources.items():
 path=C/n;old=git('show',m['documentationBase']+':docs/xpath40-catalog/'+n)
 # Git stores normalized text; the original Windows checkout may use CRLF.
 assert v['sha256'] in {sha(old),sha(old.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n'))}
 assert git('rev-parse',m['documentationBase']+':docs/xpath40-catalog/'+n).decode().strip()==v['gitBlob']
 assert old.decode('utf-8').replace('\r\n','\n')==v['text']
 t=path.read_text(encoding='utf-8');assert len(re.findall(r'^## \d+\.',t,re.M))==16 and sha(path.read_bytes())==review['entries'][n]['currentSha256']
 if n in corrected:
  correction=corrected[n];assert section(t,15)==correction['after'] and v['sections']['15']==correction['before']
  assert all(section(t,i)==v['sections'][str(i)] for i in range(1,17) if i!=15)
 else:assert t==v['text']
 assert review['entries'][n]['contracts']==entries[n]['contracts'] and review['entries'][n]['remaining']==entries[n]['remaining']
 for unit in entries[n]['namedTests']:
  assert git('hash-object',unit['file']).decode().strip()==unit['gitBlob']
  assert all(re.search(r'\b'+re.escape(method)+r'\s*\(', (R/unit['file']).read_text(encoding='utf-8-sig')) for method in unit['methods'])
raw=load(E/'retained-findings.json');assert len(raw)==37 and len(p['findings'])==38 and {v['id'] for v in review['findings']}==set(p['findings'])
for v in raw:
 data=load(C/v['source'])[v['sourceField']];original=data[v['sourceId']] if isinstance(data,dict) else next(q for q in data if q['id']==v['sourceId'])
 assert original==v['original'] and next(q for q in review['findings'] if q['id']==v['id'])['original']==original
unmet=review['unmetBatchControls'];assert len(unmet)==48 and len({v['originalObservation']['id'] for v in unmet})==24
assert sum(v['originalObservation'].get('kind')=='spec-example-control' for v in unmet)==2
for v in unmet:
 assert v['originalObservation'] in reports[int(v['batch'][1:])-9]['unmetControls'] and v['dispositionRecord'] in p['findings']
 # The full original request/expectation is attached; the deterministic builder reads protected original request files.
 assert v['originalObservation'].get('passed') is False
assets=load(E/'compiled-assets.json');assert len(assets)==3048
for n,v in assets.items():assert sha((R/n).read_bytes())==v['sha256']
rows=[]
for name,count in [('documentation.trx',3154),('remaining-chapters.trx',18)]:
 values=[v for v in ET.parse(E/name).getroot().iter() if v.tag.endswith('UnitTestResult')];assert len(values)==count and all(v.get('outcome')=='Passed' for v in values);rows.extend(values)
assert len({v.get('testId') for v in rows})==3172
cases=load(C/'examples/cases.json');byid={v['id']:v for v in cases};catalog=[v for v in rows if 'PublishedExample_MatchesDeclaredOutcome' in v.get('testName','')];assert len(catalog)==len(byid)==3148
candidates=set()
for v in catalog:
 match=re.search(r'id: "([^\"]+)"',v.get('testName'));assert match
 display=match.group(1);truncated=v.get('testName')[match.end():].startswith(('\u00b7','\u2026','.'))
 ids=[n for n in byid if n.startswith(display)] if truncated else [display];assert ids and set(ids)<=set(byid);candidates.update(ids)
assert candidates==set(byid)
links=load(E/'link-audit.json');assert not links['bad'] and len(links['files'])==97 and len(links['links'])==4656
for v in links['links']:assert (R/v['target']).exists() and v['exists'] and v['anchor'] is not False
render=load(E/'render-receipts.json');assert len(render['pages'])==22 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n,h in m['markdownFiles'].items():assert sha((R/n).read_bytes())==h
if '--local-artifacts' in sys.argv:
 for v in assets.values():assert sha((R/v['compiled']).read_bytes())==v['sha256']
 for v in render['pages']:assert sha((R/v['html']).read_bytes())==v['sha256']
print('C13 packet verified:76 entries/235 groups;38 pending dispositions;48 unmet controls retained;3,172 fresh tests;4,656 local links/97 pages. Owner approval absent; C13 remains Open.')
