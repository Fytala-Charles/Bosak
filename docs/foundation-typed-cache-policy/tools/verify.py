"""Replay TA-02 policy model and verify unchanged production/control evidence."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile,xml.etree.ElementTree as ET
sys.dont_write_bytecode=True
from reference import controls
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());assert i['sourceVersion']=='1.53.0' and i['revision'].startswith('b77727b9')
for n,h in i['files'].items():
 raw=(R/n).read_bytes();committed=subprocess.check_output(['git','show',i['revision']+':'+n],cwd=R)
 assert sha(R/n)==h and hashlib.sha256(committed).hexdigest()==i['committedFiles'][n],n
 assert hashlib.sha256(raw.replace(b'\r\n',b'\n')).hexdigest()==i['normalizedFiles'][n]==hashlib.sha256(committed.replace(b'\r\n',b'\n')).hexdigest(),n
assert controls()==json.loads((E/'MODEL-RESULTS.json').read_text())
initial=[];core=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in ['typed-cache-policy-controls.log.json','typed-cache-policy-core-correction.log.json']:
  receipt=json.loads(z.read(name));assert receipt['exitCode']==0 and receipt['failure'] is None
 for name in z.namelist():
  if name.endswith('.trx'):
   target=core if name.startswith('typed-cache-policy-core-correction-trx/') else initial
   target.extend(x.attrib for x in ET.fromstring(z.read(name)).iter() if x.tag.endswith('UnitTestResult'))
assert len(initial)==231 and len(core)==30 and all(x['outcome']=='Passed' for x in initial+core)
actual=[x for x in initial if not x['testName'].startswith('Bosak.XPath.Core.Tests.')]+core
c=json.loads((E/'CONTROLS.json').read_text());assert len(actual)==c['passed']==231 and c['executionsIncludingCoreCorrection']==261 and sorted(actual,key=lambda x:x['testId'])==sorted(c['results'],key=lambda x:x['testId'])
pins=json.loads((R/'docs/foundation-typed-container/evidence/SOURCE-AND-BINARY-PINS.json').read_text());correction=json.loads((E/'BINDING-CORRECTION.json').read_text());empty={r['path']:r for r in correction['initialIncompleteReports']};assert len(empty)==2
seen={};targets=set()
for folder in ['bindings','core-bindings']:
 for f in (E/folder).glob('*.json'):
  name=f.relative_to(P).as_posix()
  if name in empty:assert f.stat().st_size==0 and sha(f)==empty[name]['sha256'];continue
  b=json.loads(f.read_text());assert not b['errors'];targets.update(x['name'] for x in b['assemblies'] if x['name'].endswith('.Tests'))
  for x in b['assemblies']:
   if x['name'] in pins['engines']:assert x['sha256']==pins['engines'][x['name']]['sha256'];seen[x['name']]=x['sha256']
assert {'Bosak.XPath.Core.Tests','Bosak.XPath.Api.Tests','Bosak.XPath.Runtime.Tests'}<=targets and seen==json.loads((E/'LOADED-BINDINGS.json').read_text())['loadedEngines']
s=json.loads((P/'STATUS.json').read_text());assert s['cachePolicySelected'] and s['typedNumericProjectionPolicySelected'] and s['scalarCeilingUnitsPreserved'] and s['atomicBatchCommitSelected'] and s['existingPlainProfilesKeepMarkerRefusal'] and s['referenceModelPassed']
for k in ['independentContainerCacheSelected','productionChanged','ta02RuntimeCacheImplemented','typedNumericRuntimeImplemented','publicSchemaActivated','newPublicSignatureRequiredByPolicy','allFutureSignatureGapsProvenAbsent','runtimeConcurrencyQualified','fullGatesRerun','nativeQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[k] is False,k
subprocess.run([sys.executable,str(R/'docs/foundation-typed-container/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-02 policies/model and 231 distinct existing controls (261 executions); source 1.53.0 unchanged, no runtime cache/typed numeric/public/native/freeze activation.')
