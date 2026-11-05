"""Verify the normalization review without changing production or frozen acceptance evidence."""
from pathlib import Path
import json,subprocess,hashlib,re,collections,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;P=E.parent;R=E.parents[3]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()
def hashes(rows):
 actual=subprocess.check_output(['git','hash-object','--stdin-paths'],input=('\n'.join(rows)+'\n').encode(),cwd=R).decode().splitlines();assert actual==list(rows.values())
def totals(p):
 s=read(p);return [int(re.search(r'^'+n+r':\s+(\d+)',s,re.M)[1]) for n in ['Passed','Failed','Skipped']]
pin=load(E/'SPECIFICATION-REVIEW.json');assert pin['kind']=='Review only; not semantic adoption'
assert not git('diff',pin['sourceBase'],'--','src','tests','AGENTS.md','docs/xpath40-catalog/examples/cases.json')
hashes(load(E/'normal-source-identities.json'))
for name,h in load(E/'previous-identities.json').items():assert sha(R/name)==h,name
for name,h in load(R/'docs/conformance/k04-date-optional-fix/evidence/historical-catalog-identities.json').items():assert sha(R/name)==h,name
for name,h in load(E/'binary-identities.json').items():assert sha(R/name)==h,name
inventory=load(E/'normalization-inventory.json');N={'t':'http://www.w3.org/2010/09/qt-fots-catalog'};counts={};decl=[]
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');assert git('-C',str(root),'rev-parse','HEAD')==pin['fixtureRevisions'][suite] and not git('-C',str(root),'status','--porcelain');count=0
 assert sha(root/'catalog.xml')==pin['catalogueHashes']['tests/'+suite+'tests/catalog.xml']
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',N):
  file=root/ref.get('file');relative=file.relative_to(R).as_posix();assert sha(file)==inventory['fixtureHashes'][relative];doc=X.parse(file).getroot()
  for case in doc.findall('t:test-case',N):
   count+=1
   for d in doc.findall('t:dependency',N)+case.findall('t:dependency',N):
    if d.get('type')=='unicode-normalization-form':decl.append(dict(suite=suite,name=case.get('name'),value=d.get('value'),satisfied=d.get('satisfied','true') not in ['false','0'],setFile=relative))
 counts[suite]=count
assert counts==inventory['scannedCases']=={'qt3':31821,'qt4':46985} and decl==inventory['declarations'] and len(decl)==14
assert sum(d['value']=='FULLY-NORMALIZED' for d in decl)==8
assert pin['xpath40']['sha256']=='409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5'
assert pin['signature40']['arguments'][1]=={'name':'form','type':'xs:string?','default':'"NFC"','note':'default-on-empty'}
points={r['codepoint']:r for r in load(E/'codepoint-witnesses.json')};assert len(points)==10
assert points[0x9BE]['canonicalCombiningClass']==0 and points[0x9BE]['composing'] and any(w['composite']==0x9CB for w in points[0x9BE]['canonicalCompositionWitnesses'])
assert not points[0x34F]['composing'] and not points[0x20DD]['composing']
assert points[0x1161]['algorithmicHangulSecond'] and points[0x11A8]['algorithmicHangulSecond'] and points[0x1D165]['canonicalCombiningClass']==216
receipts=load(E/'probe-receipts.json');original=[r for r in receipts if r['kind']=='original'];controls=[r for r in receipts if r['kind']=='control'];assert len(receipts)==70 and len(controls)==62
assert collections.Counter(r['result']['kind'] for r in original)=={'Passed':2,'Failed':6}
assert collections.Counter(r['result']['kind'] for r in controls)=={'Passed':33,'Failed':29}
for row in receipts:assert json.loads(read(E/row['log']).splitlines()[-1])==row['result']
assert sum(r['result']['kind']=='Failed' and ('-full-' in r['name'] or '-compose-' in r['name']) for r in controls)==24
for suite in ['qt3','qt4']:
 records=[json.loads(l) for l in read(E/(suite+'-filter.jsonl')).splitlines()];assert len(records)==4
 for row in records:assert row['xpathSupported']==(not row['name'].endswith('a'))
assert totals(E/'qt3-normal.log')==[42,0,6] and totals(E/'qt4-normal.log')==[41,3,6]
assert 'Upstream defect: expected value has an artifactual leading space' in read(E/'qt3-normal.log')
fails=re.findall(r'^  FAIL ([^:]+):',read(E/'qt4-normal.log'),re.M);assert set(fails)=={'cbcl-fn-normalize-unicode-006','fn-normalize-unicode2args-5a','fn-normalize-unicode-empty'}
allReceipts=receipts+load(E/'normal-set-receipts.json')+load(E/'filter-receipts.json');assert len(allReceipts)==74 and all(r['exitCode']==0 and not r['timeout'] for r in allReceipts)
inherited=load(R/'docs/conformance/k04-date-optional-fix/evidence/validation.json');assert inherited['solutionPassed']==23365 and inherited['normalQt4']==[8895,0,1914]
results=load(P/'RESULTS.json');assert results['semanticAdoption'] is False and results['engineChanged'] is False and results['controlsFailed']==29
manifest=load(E/'manifest.json')
for name,h in manifest['files'].items():assert sha(P/name)==h,name
hashes(manifest['documents'])
renders=load(E/'render-receipts.json');assert len(renders['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==renders['cssSha256']
print('PASS:78806case inventory/14declarations;8originals2passed6diagnostic failures;62controls33passed29failed;74bounded processes;engine/gates/history unchanged.')
