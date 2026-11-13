"""Replay case-cost delivery evidence and immutable historical boundaries without .NET."""
from pathlib import Path
import hashlib,importlib.util,json,subprocess,zipfile

def main():
 d=Path(__file__).resolve().parent;r=d.parent.parent
 def manifest(folder):
  for name,e in json.loads((folder/'MANIFEST.json').read_text())['files'].items():
   data=(folder/name).read_bytes();assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256'],(folder.name,name)
 manifest(d)
 spec=importlib.util.spec_from_file_location('case_refresh_gate',r/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
 policy,_,inputs,skips=gate.baseline()
 def replay(folder,label):
  meta=json.loads((folder/(label+'.json')).read_text());data=(folder/(label+'.zip')).read_bytes()
  assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(folder/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name))
   run,result=read('RUN.json'),read('GATE.json')
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json')
   assert result['sourceCorpusUnchanged'] and result['runtimeUnchanged'] and not any(result['corpusDifferences'].values())
   if run['success']:
    assert result['success'] and (meta.get('success') or meta.get('qualificationVerified')) and meta['sourceTreeMatchesHead']
    assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
   else:assert not result['success'] and run['status']=='TimedOut'
   return run
 original=replay(d,'ORIGINAL-PR486');assert original['elapsedSeconds']==1200.250
 assert replay(d,'QUALIFIED-PR485')['elapsedSeconds']==848.750 and replay(d,'MAIN-484')['elapsedSeconds']==1057.812
 for folder,labels in [('xslt30-timing-refresh',['ORIGINAL-PR484','QUALIFIED-PR489']),('xslt30-telemetry-refresh',['ORIGINAL-PR485','EARLIER-DELIVERY','QUALIFIED-PR484','MAIN-489'])]:
  historical=r/'docs'/folder;manifest(historical)
  for label in labels:replay(historical,label)
 assert not subprocess.check_output(['git','diff','0432de7c0a357eeeb748d05ec1e9f4148cb737f6','2e56414097c1426d1d9c8279b7e3b38c267b9a17','--','src','tests','tools','.github','Bosak.sln'],cwd=r)
 telemetryBoundary=subprocess.check_output(['git','diff','--name-only','a40951308950b36970f10badc1d13e46833ed1f7','4933d02204e7622f27bc23bb043c9996f7fb857c','--','src','tests','tools','.github','Bosak.sln'],cwd=r,text=True).splitlines()
 assert set(telemetryBoundary)=={'tools/xslt30_telemetry.py','tools/test_xslt30_telemetry.py','.github/workflows/xslt30-telemetry.yml'}
 merged=json.loads((d/'MERGE485.json').read_text());assert merged['pr']['state']=='MERGED' and merged['sourceTreeMatchesQualifiedPr'];base=merged['pr']['mergeCommit']['oid']
 controls=json.loads((d/'CONTROLS.json').read_text());assert controls['caseCostControls']['tests']==15 and controls['caseCostControls']['exitCode']==0
 assert not controls['newDiagnosticExecution'] and not controls['newDotNetExecution']
 assert controls['workflowValidator']['exitCode']==0 and controls['workflowValidator']['isolatedDependencyDirectory'] and not controls['workflowValidator']['globalEnvironmentChanged']
 assert controls['dependency']['version']=='6.0.3' and 'Release YAML parsed;' in (d/'WORKFLOWS.log').read_text()
 for name,digest in controls['sourceGitBlobSha256'].items():
  old=subprocess.check_output(['git','show',controls['originalSourceHead']+':'+name],cwd=r)
  assert hashlib.sha256(old).hexdigest()==digest and (r/name).read_bytes().replace(b'\r\n',b'\n')==old.replace(b'\r\n',b'\n')
 current=subprocess.check_output(['git','diff','--name-only',base,'--','src','tests','tools','.github','Bosak.sln'],cwd=r,text=True).splitlines()
 assert set(current)==set(controls['sourceGitBlobSha256']),current
 for folder in ['xslt30-case-cost','xslt30-telemetry-delivery','xslt30-hosted-timing','streaming-wrapper-cache']:
  subprocess.run(['python',str(r/'docs'/folder/'verify.py')],cwd=r,check=True)
 print('Verified original case-cost evidence/failures, complete successor qualifications, unchanged source/control boundaries and all historical receipts; no .NET execution.')

if __name__=='__main__':main()
