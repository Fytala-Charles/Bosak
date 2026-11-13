"""Replay preserved timeout and qualified successor; do not execute .NET."""
from pathlib import Path
import hashlib,importlib.util,json,subprocess,zipfile

def main():
 d=Path(__file__).resolve().parent;r=d.parent.parent
 manifest=json.loads((d/'MANIFEST.json').read_text())
 for name,e in manifest['files'].items():
  data=(d/name).read_bytes();assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256'],name
 spec=importlib.util.spec_from_file_location('refresh_gate',r/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
 policy,_,inputs,skips=gate.baseline()
 for label in ['ORIGINAL-PR484','QUALIFIED-PR489']:
  meta=json.loads((d/(label+'.json')).read_text());data=(d/(label+'.zip')).read_bytes()
  assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(d/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name))
   run,result=read('RUN.json'),read('GATE.json')
   assert run==meta['run'] and result==meta['gate']
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json')
   assert result['runtimeUnchanged'] and result['sourceCorpusUnchanged'] and not any(result['corpusDifferences'].values())
   if label=='ORIGINAL-PR484':
    assert run['status']=='TimedOut' and not run['success'] and not result['success'] and run['elapsedSeconds']==1200.156
    assert 'sx-instance-of-101' in z.read('stdout.log').decode('utf-8-sig').splitlines()[-1]
   else:
    assert meta['qualificationVerified'] and meta['sourceTreeMatchesHead'] and run['success'] and result['success']
    assert run['elapsedSeconds']==1023.329
    assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
 merged=json.loads((d/'MERGE-489.json').read_text());assert merged['pr']['state']=='MERGED' and merged['sourceTreeMatchesQualifiedPr']
 assert merged['pr']['mergeCommit']['oid']=='0432de7c0a357eeeb748d05ec1e9f4148cb737f6'
 assert not subprocess.check_output(['git','diff','0432de7c0a357eeeb748d05ec1e9f4148cb737f6','--','src','tests','tools','.github','Bosak.sln'],cwd=r)
 subprocess.run(['python',str(r/'docs/xslt30-hosted-timing/verify.py')],cwd=r,check=True)
 subprocess.run(['python',str(r/'docs/streaming-wrapper-cache/verify.py')],cwd=r,check=True)
 print('Verified preserved PR484 timeout, complete PR489 qualification, both unchanged sealed dossiers and no engine/workflow diff from merged main.')

if __name__=='__main__':main()
