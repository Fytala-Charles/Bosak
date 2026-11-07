"""Verify source-pinned native adapter feasibility; never execute managed workloads."""
from pathlib import Path
import json,hashlib,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
a=read(P/'ADOPTION.json');n=Path(a['nativeRoot']);s=read(P/'STATUS.json')
assert a['nativeStatus']=='' and s['sourceVersion']=='1.81.0'
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
for name,h in {**a['coreFiles'],**a['priorPackets']}.items():assert sha(R/name)==h,name
assert len(read(P/'MAPPING.json')['rows'])==s['mappingRows']==10
c=read(P/'PROTOTYPE-CASES.json');assert c['status']=='PlannedNotExecuted' and len({x['id'] for x in c['cases']})==s['plannedControls']==12
assert s['feasibilityReviewComplete'] and s['newTestsRun']==s['requirementsWaived']==0
assert not any(s[k] for k in ['adapterImplemented','nativePublicBindingQualified','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
for row in read(P/'evidence/DOCUMENTATION-CHECKS.json')['pages']:assert sha(R/row['source'])==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,h in m.items():assert sha(P/name)==h,name
print('Verified TA-45:10 source-pinned mappings,12 planned controls,unchanged native/Core compatibility evidence. No public adapter or owner/budget acceptance;next PV-Q03a.')
