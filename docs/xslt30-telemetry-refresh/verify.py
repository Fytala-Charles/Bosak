"""Verify refreshed telemetry delivery while keeping original evidence immutable."""
from pathlib import Path
import hashlib,importlib.util,json,subprocess,zipfile

def main():
 d=Path(__file__).resolve().parent;r=d.parent.parent
 manifest=json.loads((d/'MANIFEST.json').read_text())
 for name,e in manifest['files'].items():
  data=(d/name).read_bytes();assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256'],name
 spec=importlib.util.spec_from_file_location('telemetry_refresh_gate',r/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
 policy,_,inputs,skips=gate.baseline()
 for label in ['ORIGINAL-PR485','EARLIER-DELIVERY','QUALIFIED-PR484','MAIN-489']:
  meta=json.loads((d/(label+'.json')).read_text());data=(d/(label+'.zip')).read_bytes()
  assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(d/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name))
   run,result=read('RUN.json'),read('GATE.json')
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json')
   assert result['sourceCorpusUnchanged'] and result['runtimeUnchanged'] and not any(result['corpusDifferences'].values())
   if label in ['ORIGINAL-PR485','EARLIER-DELIVERY']:
    assert not result['success'] and not run['success'] and run['status']=='TimedOut'
    expected,last={'ORIGINAL-PR485':(1200.235,'date-020'),'EARLIER-DELIVERY':(1200.063,'sx-treat-015')}[label]
    assert run['elapsedSeconds']==expected and last in z.read('stdout.log').decode('utf-8-sig').splitlines()[-1]
   else:
    assert meta['success'] and meta['sourceTreeMatchesHead'] and result['success'] and run['success']
    assert run['sourceCommit']==meta['sourceCommit'] and run['elapsedSeconds']==meta['elapsedSeconds']
    assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
 controls=json.loads((d/'CONTROLS.json').read_text());assert [x['tests'] for x in controls['results']]==[17,14] and all(x['exitCode']==0 for x in controls['results'])
 assert not controls['newDiagnosticCapture']
 for name,digest in controls['sourceGitBlobSha256'].items():
  original=subprocess.check_output(['git','show',controls['unchangedToolSourceHead']+':'+name],cwd=r)
  assert hashlib.sha256(original).hexdigest()==digest and (r/name).read_bytes().replace(b'\r\n',b'\n')==original.replace(b'\r\n',b'\n')
 workflow=(r/'.github/workflows/xslt30-telemetry.yml').read_text();header=workflow.split('jobs:',1)[0]
 assert 'types: [opened]' in header and 'workflow_dispatch:' in header and 'push:' not in header
 boundary=subprocess.check_output(['git','diff','--name-only','a40951308950b36970f10badc1d13e46833ed1f7','--','src','tests','tools','.github','Bosak.sln'],cwd=r,text=True).splitlines()
 assert set(boundary)==set(controls['sourceGitBlobSha256']),boundary
 merged=json.loads((d/'MERGE484.json').read_text());assert merged['pr']['state']=='MERGED' and merged['sourceTreeMatchesQualifiedPr']
 subprocess.run(['python',str(r/'docs/xslt30-telemetry-delivery/verify.py')],cwd=r,check=True)
 # The earlier delivery verifier deliberately requires a doc-only working tree.
 # Replay its immutable data here and check its boundary at its actual checkpoint.
 earlier=r/'docs/xslt30-timing-refresh'
 for name,e in json.loads((earlier/'MANIFEST.json').read_text())['files'].items():
  data=(earlier/name).read_bytes();assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256']
 for label in ['ORIGINAL-PR484','QUALIFIED-PR489']:
  meta=json.loads((earlier/(label+'.json')).read_text());data=(earlier/(label+'.zip')).read_bytes()
  assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(earlier/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name))
   run,result=read('RUN.json'),read('GATE.json');assert run==meta['run'] and result==meta['gate']
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json') and result['runtimeUnchanged'] and result['sourceCorpusUnchanged'] and not any(result['corpusDifferences'].values())
   if label=='ORIGINAL-PR484':assert not run['success'] and not result['success'] and run['status']=='TimedOut' and run['elapsedSeconds']==1200.156
   else:
    assert meta['qualificationVerified'] and meta['sourceTreeMatchesHead'] and run['success'] and result['success'] and run['elapsedSeconds']==1023.329
    assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
 historicalMerge=json.loads((earlier/'MERGE-489.json').read_text());assert historicalMerge['pr']['state']=='MERGED' and historicalMerge['sourceTreeMatchesQualifiedPr']
 assert not subprocess.check_output(['git','diff','0432de7c0a357eeeb748d05ec1e9f4148cb737f6','2e56414097c1426d1d9c8279b7e3b38c267b9a17','--','src','tests','tools','.github','Bosak.sln'],cwd=r)
 subprocess.run(['python',str(r/'docs/xslt30-hosted-timing/verify.py')],cwd=r,check=True)
 subprocess.run(['python',str(r/'docs/streaming-wrapper-cache/verify.py')],cwd=r,check=True)
 print('Verified two preserved telemetry delivery timeouts, two complete hosted qualifications, unchanged tool/trigger source, 31 controls and original sealed dossiers; no .NET execution.')

if __name__=='__main__':main()
