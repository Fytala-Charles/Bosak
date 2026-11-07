"""Read-only structural preflight for owner workload receipts; never qualification or owner approval."""
from pathlib import Path
import argparse,hashlib,json,re
P=Path(__file__).resolve().parents[1]
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def check(data,root,pins,pin_digest,api_digest):
 errors=[]
 if not isinstance(data,dict):return dict(structureReady=False,errors=['receipt: JSON object required'],qualificationAccepted=False,ownerAcceptanceVerified=False)
 def obj(key):
  value=data.get(key,{})
  if not isinstance(value,dict):errors.append(key+': JSON object required');return {}
  return value
 def need(ok,message):
  if not ok:errors.append(message)
 def ref(value,label):
  if not isinstance(value,dict):errors.append(label+': missing file receipt');return None
  name=value.get('path');digest=value.get('sha256')
  if not isinstance(name,str) or not name:errors.append(label+': missing relative path');return None
  path=(root/name).resolve()
  if not path.is_relative_to(root.resolve()):errors.append(label+': path escapes receipt root');return None
  if not path.is_file():errors.append(label+': file missing');return None
  if not isinstance(digest,str) or sha(path)!=digest.lower():errors.append(label+': hash mismatch');return None
  return path
 def payload(value,label):
  path=ref(value,label)
  if path is None:return None
  try:
   data=json.loads(path.read_text(encoding='utf-8'))
   if not isinstance(data,dict):errors.append(label+': JSON object required');return None
   return data
  except (OSError,ValueError,UnicodeError):errors.append(label+': invalid JSON');return None
 need(data.get('scenario') in [f'PF-{i:02d}' for i in range(1,10)],'scenario: expected PF-01 through PF-09')
 need(data.get('ownerRole') in ['Core','Schema','XmlDb','Braid'],'ownerRole: unsupported or absent')
 c=obj('candidate');expected={n:r['sha256'].lower() for n,r in pins['engines'].items()}
 need(c.get('sourceVersion')==pins['sourceVersion'],'candidate: source version mismatch')
 need(c.get('pinsSha256')==pin_digest,'candidate: exact source/binary manifest mismatch')
 need(c.get('apiSha256')==api_digest,'candidate: API inventory mismatch')
 need(c.get('engines')==expected,'candidate: complete engine hash cohort mismatch')
 consumer=obj('consumer');cohort=consumer.get('cohort');need(cohort in ['source','unchanged-binary'],'consumer: select one source or unchanged-binary cohort')
 need(isinstance(consumer.get('sourceRevision'),str) and re.fullmatch(r'[0-9a-f]{40}',consumer['sourceRevision']) is not None,'consumer: exact source revision missing')
 need(bool(consumer.get('baselinePin')),'consumer: shipped baseline pin missing')
 inputs=payload(consumer.get('inputManifest'),'consumer input manifest')
 if inputs is not None:need(inputs.get('consumerSourceRevision')==consumer.get('sourceRevision') and inputs.get('candidatePinsSha256')==pin_digest,'consumer input manifest: source/candidate binding mismatch')
 if cohort=='source':ref(consumer.get('buildLog'),'source build log')
 if cohort=='unchanged-binary':
  need(consumer.get('rebuilt') is False,'unchanged-binary: consumer must not be rebuilt')
  before=payload(consumer.get('baselineBinaryManifest'),'baseline binary manifest');after=payload(consumer.get('replacementBinaryManifest'),'replacement binary manifest')
  observed=[]
  for label,manifest in [('baseline',before),('replacement',after)]:
   artifacts=manifest.get('consumerArtifacts',{}) if manifest else {};need(isinstance(artifacts,dict) and bool(artifacts),label+': unchanged consumer artifacts required');bound={}
   if isinstance(artifacts,dict):
    for name,value in artifacts.items():
     path=ref(value,label+' consumer '+name)
     if path:bound[name]=sha(path)
   observed.append(bound)
  need(bool(observed[0]) and observed[0]==observed[1],'unchanged-binary: consumer artifact set or bytes changed')
 loaded=data.get('loadedEngines',{});need(isinstance(loaded,dict) and {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=set(loaded),'loaded engines: required typed host assemblies absent')
 if isinstance(loaded,dict):
  need(all(n in expected and h==expected[n] for n,h in loaded.items()),'loaded engines: candidate hash mismatch or unknown engine')
 fixtures=data.get('fixtures',[]);need(isinstance(fixtures,list) and len(fixtures)>0,'fixtures: exact fixture receipts required')
 if isinstance(fixtures,list):
  for i,value in enumerate(fixtures):ref(value,f'fixture {i}')
 need(bool(data.get('queryAndSettings')),'query/settings: explicit workload required')
 ref(data.get('rawResults'),'raw results');ref(data.get('correctnessReceipt'),'correctness receipt')
 m=obj('measurement');need(all(m.get(k) is not None for k in ['environment','method','warmupSamples','measuredSamples','metrics']),'measurement: environment/method/samples/metrics incomplete')
 need(type(m.get('measuredSamples')) is int and m['measuredSamples']>0,'measurement: positive measured sample count required')
 need(isinstance(m.get('method'),str) and bool(m['method'].strip()) and isinstance(m.get('metrics'),list) and bool(m['metrics']),'measurement: nonempty method and metric list required')
 env=m.get('environment',{});need(isinstance(env,dict) and all(env.get(k) for k in ['os','runtime','cpu','configuration','tool']),'measurement: exact platform/build/tool context required')
 need(type(m.get('warmupSamples')) is int and m['warmupSamples']>=0,'measurement: explicit nonnegative warmup count required')
 budget=obj('budget');budget_path=ref(budget.get('receipt'),'budget receipt')
 need(isinstance(budget.get('thresholds'),dict) and bool(budget['thresholds']) and type(budget.get('tolerance')) in [int,float] and budget['tolerance']>=0,'budget: explicit thresholds and nonnegative tolerance required')
 if budget_path:
  try:
   b=json.loads(budget_path.read_text(encoding='utf-8'))
   need(isinstance(b,dict) and b.get('ownerRole')==data.get('ownerRole') and b.get('scenario')==data.get('scenario') and b.get('candidatePinsSha256')==pin_digest,'budget: owner/candidate binding mismatch')
  except (ValueError,UnicodeError):errors.append('budget: invalid JSON receipt')
 return dict(structureReady=not errors,errors=errors,qualificationAccepted=False,ownerAcceptanceVerified=False,scope='Structure/hash preflight only; no execution, semantic/native qualification, budget authenticity or owner approval.')
def main():
 parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('receipt',type=Path);args=parser.parse_args()
 pins_path=P/'evidence/SOURCE-AND-BINARY-PINS.json';api_path=P.parents[1]/'docs/foundation-public-primitives/evidence/public-api.json'
 try:
  result=check(json.loads(args.receipt.read_text(encoding='utf-8')),P,json.loads(pins_path.read_text(encoding='utf-8')),sha(pins_path),sha(api_path))
 except (OSError,ValueError,TypeError):
  result=dict(structureReady=False,errors=['receipt input or pinned context could not be read as valid JSON'],qualificationAccepted=False,ownerAcceptanceVerified=False)
 print(json.dumps(result,indent=2));return 0 if result['structureReady'] else 2
if __name__=='__main__':raise SystemExit(main())
