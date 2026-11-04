"""Replay the policy reference and verify unchanged qualified production evidence."""
from pathlib import Path
import hashlib,json,subprocess,sys
sys.dont_write_bytecode=True
from reference import run_controls
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());assert i['sourceVersion']=='1.50.0' and i['baseCommit'].startswith('f9e7c615')
for name,h in i['files'].items():
 assert sha(R/name)==h,name
 assert hashlib.sha256(subprocess.check_output(['git','show',i['baseCommit']+':'+name],cwd=R)).hexdigest()==h,name
assert run_controls()==json.loads((P/'evidence/REFERENCE-RESULTS.json').read_text())
s=json.loads((P/'STATUS.json').read_text());assert s['sourceVersion']=='1.50.0' and s['corePolicySelected'] and s['referenceExperimentPassed']
for key in ['roundingFallback','productionImplemented','runtimeTestsRerun','fullDecimalDivisionQualified','publicApiChanged','ownerAcceptance','apiFrozen','publicationAuthorized']:assert s[key] is False,key
subprocess.run([sys.executable,str(R/'docs/foundation-exact-quotient/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,h in m.items():assert sha(P/name)==h,name
print('Verified division policy: 22 boundary, 7000 independent rational-oracle cases, five availability controls; qualified production unchanged, no runtime div implementation or freeze.')
