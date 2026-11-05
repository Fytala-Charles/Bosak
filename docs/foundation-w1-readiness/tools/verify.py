"""Verify finite W1 readiness without claiming implementation or owner acceptance."""
from pathlib import Path
import hashlib,json,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());a=json.loads((P/'ACCEPTANCE.json').read_text())
assert not i['productionChanged'] and not a['implementationStarted'] and not a['r2Closed'] and a['productionVersion']=='1.42.0'
assert [x['id'] for x in a['cases']]==['W1-'+str(n).zfill(2) for n in range(1,21)] and all(x['status']=='NotRun' and x['trigger'] and x['expected'] for x in a['cases'])
plan=(P/'README.md').read_text(encoding='utf-8');assert all('| W1-'+letter+' ' in plan for letter in 'ABCDE')
for name,digest in i['files'].items():assert sha(R/name)==digest,name
for repo in i['localReturnSearch']['repositories']:
 assert repo['observedStatus']=='clean' and not repo['matchingLocalDocuments']
 assert subprocess.check_output(['git','cat-file','-t',repo['revision']],cwd=repo['root']).decode().strip()=='commit'
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified W1 source/owner-search pins, five gated slices and 20 NotRun acceptance cases; implementation not started and R2 still open.')
