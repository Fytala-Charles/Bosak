"""Verify closure/skip inventories, reproduced plan outputs, source identity and retained receipts."""
from pathlib import Path
import json,hashlib,subprocess,sys,collections,re
e=Path(__file__).resolve().parent;r=e.parents[3];c=r/'docs/xpath40-catalog'
def sha(data):return hashlib.sha256(data).hexdigest()
def load(name):return json.loads((e/name).read_text(encoding='utf-8'))
m=load('manifest.json')
def verifyBytes():
 for name,item in m['files'].items():
  p=e/name;assert sha(p.read_bytes())==item['sha256'],name
  if '--staged' in sys.argv:assert sha(subprocess.check_output(['git','show',':'+p.relative_to(r).as_posix()],cwd=r))==item['sha256'],name+' index'
verifyBytes()
assert subprocess.check_output(['git','hash-object','docs/xpath40-catalog/build-closure-plan.py'],cwd=r).decode().strip()==m['builderGitBlob']
for name,expected in m['outputs'].items():assert sha((c/name).read_bytes().replace(bytes([13,10]),bytes([10])))==expected,name
subprocess.run([sys.executable,str(c/'build-closure-plan.py')],cwd=r,check=True)
for name,expected in m['outputs'].items():assert sha((c/name).read_bytes().replace(bytes([13,10]),bytes([10])))==expected,name+' nondeterministic output'
verifyBytes()
source=load('source-identities.json')
for name,item in source['sources'].items():
 assert subprocess.check_output(['git','hash-object',name],cwd=r).decode().strip()==item['gitBlob'],name
 assert subprocess.check_output(['git','rev-parse',source['publishedSource']+':'+name],cwd=r).decode().strip()==item['gitBlob'],name
if '--local-binaries' in sys.argv:
 for item in source['localHarnessBinaries']+[source['generatedDiagnosticBinary']]:assert sha(Path(item['path']).read_bytes())==item['sha256']
inventory=load('catalog-inventory.json');entries=inventory['entries'];assert len(entries)==76
for item in entries:
 name=(c/item['entry']).relative_to(r).as_posix()
 assert subprocess.check_output(['git','hash-object',name],cwd=r).decode().strip()==subprocess.check_output(['git','rev-parse',source['documentationBase']+':'+name],cwd=r).decode().strip(),name
for row in inventory['grammarRows']:
 for name in row['source']:assert (r/name).exists()
 for item in row['testEvidence']:assert (r/item['file']).exists() and (not item['method'] or item['method']+'(' in (r/item['file']).read_text(encoding='utf-8-sig'))
cases=load('skip-cases.json');assert len(cases)==2573 and len({(x['Suite'],x['Name']) for x in cases})==2573
filters=[json.loads(line) for line in (e/'filter-results.jsonl').read_text().splitlines()];assert len(filters)==2512
for result in filters:
 assert not result['xpathSupported']
 if result['Suite']=='qt3':assert not(result['xqueryDependenciesSupported'] and result['canRouteXquery'])
selection=load('diagnostic-selection.json');receipts=load('diagnostic-receipts.json');assert len(selection)==len(receipts)==26
assert {(x['Suite'],x['Name']) for x in selection}=={(x['suite'],x['name']) for x in receipts}
assert all(x['exitCode']==0 and not x['timeout'] and x['result'] for x in receipts)
assert collections.Counter(x['result']['kind'] for x in receipts)==dict(Passed=6,Failed=8,HarnessException=7,Skipped=5)
for suite,expected in [('qt3',[31142,0,679]),('qt4',[8436,0,1894])]:
 text=(e/(suite+'.log')).read_text(encoding='utf-8-sig');assert [int(re.search('^'+x+r':\s*(\d+)',text,re.M)[1]) for x in ['Passed','Failed','Skipped']]==expected
for doc in ['CLOSURE-PLAN.md','CONFORMANCE-SKIP-TRIAGE.md']:
 for target in re.findall(r'\]\(([^)]+)\)',(c/doc).read_text(encoding='utf-8')):
  if not target.startswith(('http:','https:','#')):assert (c/target.split('#')[0]).exists(),target
inherited=load('inherited-validation.json');assert subprocess.check_output(['git','hash-object',inherited['proof']],cwd=r).decode().strip()==inherited['gitBlob']
print('Verified 13 workpackages, 76 exact batch members, 18 grammar rows, 28 pin gaps, 2,573 skips, 2,512 actual filter results, 26 diagnostic receipts and unchanged source/gates.')
