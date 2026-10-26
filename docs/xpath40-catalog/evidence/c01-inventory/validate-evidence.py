"""Validate C01 immutable inputs, live lookup controls and current plan without rewriting C00."""
from pathlib import Path
import hashlib,json,subprocess,sys,re,tempfile,shutil
E=Path(__file__).resolve().parent;C=E.parents[1];R=C.parents[1]
def sha(b):return hashlib.sha256(b).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def normalized(p):return p.read_bytes().replace(bytes([13,10]),bytes([10]))
m=load(E/'manifest.json')
for name,expected in m['files'].items():assert sha((E/name).read_bytes())==expected,name
for name,expected in m['sourceFiles'].items():assert subprocess.check_output(['git','hash-object',name],cwd=R).decode().strip()==expected,name
subprocess.run(['git','diff','--exit-code',m['engineSource'],'--','src','tests','Directory.Build.props'],cwd=R,check=True,stdout=subprocess.DEVNULL)
for name in ['audit-function-inventory.json','audit-signature-contracts.json','audit-changed31-contracts.json']:
 original=subprocess.check_output(['git','show',m['documentationBase']+':docs/xpath40-catalog/'+name],cwd=R)
 assert normalized(E/('retained-'+name))==original.replace(bytes([13,10]),bytes([10])),name+' historical snapshot'
for name,expected in m['outputs'].items():assert sha(normalized(C/name))==expected,name
subprocess.run([sys.executable,str(C/'build-c01-inventory.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
subprocess.run([sys.executable,str(C/'build-closure-plan.py')],cwd=R,check=True,stdout=subprocess.DEVNULL)
for name,expected in m['outputs'].items():assert sha(normalized(C/name))==expected,name+' deterministic rebuild'
d=load(C/'CURRENT-FUNCTION-INVENTORY.json');s=d['summary'];rows=d['functions']
assert (s['names'],s['registered40OnlyNames'],s['nonConstructorNames'],s['focusConstructorNames'],s['existingNames'],s['lookupObservations'],s['lookupErrors'])==(316,119,70,49,197,1581,0)
assert s['added40OnlyNames']==['array:members','array:of-members'] and not s['removed40OnlyNames']
requests=load(E/'lookup-requests.json');observations=[json.loads(v) for v in (E/'lookup-results.jsonl').read_text().splitlines()]
assert len(requests)==527 and len({(v['name'],v['arity']) for v in requests})==527
assert {(v['name'],v['arity'],v['profile']) for v in observations}=={(q['name'],q['arity'],p) for q in requests for p in ['XPath31','XPath40','XPath40Experimental']}
literal_controls=0
for row in rows:
 assert row['gaps'] and all(g['owner'] in ['C02','C03','C06','C07'] for g in row['gaps'])
 assert row['omittedEmptyErrorMapping']['status'].startswith('Review required') and not row['semanticChangeEstablished']
 for observation in row['lookupObservations']:
  assert observation['error'] is None and observation['result'] in ['0','1']
  for sig in row['registeredSignatures']:
   if sig['arity']!=observation['arity']:continue
   expected=not(observation['profile']=='XPath31' and (sig['frozen'] or sig['experimental'])) and not(observation['profile']=='XPath40' and sig['experimental'])
   assert observation['result']==str(int(expected)),(row['name'],observation)
   literal_controls+=1
assert literal_controls>1400
plan=load(C/'CLOSURE-PLAN.json');assert plan['openClosureWorkpackages']==12
assert [v['id'] for v in plan['workpackages'] if v['status']=='Complete']==['C01']
assert all(v['status']=='Open' for v in plan['workpackages'][1:])
assert plan['acceptance']['AC01']=='Open/partial' and plan['status']=='In Progress'
for name in ['FUNCTION-INVENTORY-REFRESH.md','CLOSURE-PLAN.md']:
 for target in re.findall(r'\]\(([^)]+)\)',(C/name).read_text(encoding='utf-8')):
  if not target.startswith(('http:','https:','#')):assert (C/target.split('#')[0]).exists(),target
if '--local-binaries' in sys.argv:
 for item in m['binaryIdentities']:assert sha(Path(item['path']).read_bytes())==item['sha256'],item['path']
prior=C/'evidence/closure-triage';original=load(prior/'manifest.json')
for name,item in original['files'].items():assert sha((prior/name).read_bytes())==item['sha256'],'C00 '+name
with tempfile.TemporaryDirectory(prefix='bosak-c00-') as folder:
 historical=Path(folder);shutil.copytree(prior,historical/'evidence/closure-triage')
 builder=historical/'build-closure-plan.py';builder.write_bytes(subprocess.check_output(['git','cat-file','blob',original['builderGitBlob']],cwd=R))
 subprocess.run([sys.executable,str(builder)],cwd=R,check=True,stdout=subprocess.DEVNULL)
 for name,expected in original['outputs'].items():assert sha(normalized(historical/name))==expected,'C00 historical '+name
print(f'C01 verified: 316 names; 1,581 profile observations; {literal_controls} literal-registration controls; 12 workpackages open. C00 immutable bytes and original plan reproduced separately. No semantic acceptance.')
