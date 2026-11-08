"""Verify blocked preparation and synthetic tooling controls; never run product tests."""
from pathlib import Path
import hashlib,json,subprocess,sys,tempfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
g=json.loads((P/'GATE.json').read_text());assert g['status']=='Blocked pending gate evidence' and not g['implementationStarted']
for row in g['requiredReplacements']:
 for k in ['path','sha256','validationReceipt']:assert row[k] is None
assert all(v is None for v in g['scopedR2Closure'].values())
assert g['implementationCandidateCommit'] is None
b=g['baseline'];assert sha(R/b['sourceInputs'])==b['sourceInputsSha256'];assert sha(R/b['qualificationReceipt'])==b['qualificationReceiptSha256']
data=json.loads((P/'ACCEPTANCE.json').read_text());old=R/'docs/foundation-w1-readiness/ACCEPTANCE.json';assert sha(old)==data['historicalChecklistSha256']
original=json.loads(old.read_text())['cases'];assert len(data['cases'])==20
for a,b in zip(data['cases'],original):
 for k,v in b.items():assert a[k]==v,(a['id'],k)
required=[c for c in data['cases'] if c['requiredFor']=='W1-A neutral admission']+[data['additionalDrainControl']];assert len(required)==18
assert all(c['status']=='NotRun' for c in required)
cmd=[sys.executable,str(P/'tools/run.py'),'--log',str(P/'evidence/execution/must-not-exist.log'),'--',sys.executable,'-c','raise SystemExit(99)']
r=subprocess.run(cmd,capture_output=True,text=True);assert r.returncode==2 and 'Blocked pending gate evidence' in r.stderr,r
assert not (P/'evidence/execution/must-not-exist.log').exists()
with tempfile.TemporaryDirectory(prefix='w1-plan-controls-',dir=R/'.guard-tmp') as temp:
 f=Path(temp)/'synthetic.trx'
 for label,rows,expected in [('complete',required,0),('empty',[],2),('missing-one',required[:-1],2),('failed',required,2)]:
  root=ET.Element('TestRun');results=ET.SubElement(root,'Results')
  for i,c in enumerate(rows):ET.SubElement(results,'UnitTestResult',testName='Synthetic.'+c['plannedTestNamePrefix']+'control',outcome='Failed' if label=='failed' and i==0 else 'Passed')
  ET.ElementTree(root).write(f,encoding='utf-8',xml_declaration=True)
  r=subprocess.run([sys.executable,str(P/'tools/check-acceptance.py'),'--trx',str(f),'--validate-only'],capture_output=True,text=True)
  assert r.returncode==expected,(label,r.stdout,r.stderr)
assert not (P/'evidence/execution/acceptance').exists(),'Synthetic controls must not mint actual acceptance receipts'
m=P/'MANIFEST.json'
if m.exists():
 for name,h in json.loads(m.read_text()).items():assert sha(P/name)==h,name
print('Verified blocked gate, all 20 retained NotRun cases, 18 W1-A requirements, baseline hashes and five synthetic tooling controls. No .NET execution or runtime qualification.')
