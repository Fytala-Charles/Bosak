"""Replay immutable GC/allocation delivery evidence without executing .NET."""
from pathlib import Path
import hashlib,importlib.util,json,subprocess,zipfile

def main():
 d=Path(__file__).resolve().parent;r=d.parent.parent
 def manifest(folder):
  for name,e in json.loads((folder/'MANIFEST.json').read_text())['files'].items():
   data=(folder/name).read_bytes();assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256'],(folder.name,name)
 manifest(d)
 spec=importlib.util.spec_from_file_location('gc_refresh_gate',r/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
 policy,_,inputs,skips=gate.baseline()
 def replay(folder,label):
  meta=json.loads((folder/(label+'.json')).read_text());data=(folder/(label+'.zip')).read_bytes()
  assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(folder/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name))
   run,result=read('RUN.json'),read('GATE.json')
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json') and result['sourceCorpusUnchanged'] and result['runtimeUnchanged'] and not any(result['corpusDifferences'].values())
   if run['success']:
    assert result['success'] and (meta.get('success') or meta.get('qualificationVerified')) and meta['sourceTreeMatchesHead']
    assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
   else:assert not result['success'] and run['status']=='TimedOut'
   return run
 assert replay(d,'ORIGINAL-PR488')['elapsedSeconds']==1200.234
 assert replay(d,'QUALIFIED-PR487')['elapsedSeconds']==json.loads((d/'QUALIFIED-PR487.json').read_text())['elapsedSeconds']
 assert replay(d,'MAIN-486')['elapsedSeconds']==726.125
 proof=json.loads((d/'ORIGINAL-SOURCE.json').read_text());assert proof['sourceTreeMatchesOriginalHead'] and proof['originalFailurePreserved'] and proof['head']=='116b8245ea7c5a2c54c345624415d0f900e51bea'
 histories=[('xslt30-timing-refresh',['ORIGINAL-PR484','QUALIFIED-PR489']),('xslt30-telemetry-refresh',['ORIGINAL-PR485','EARLIER-DELIVERY','QUALIFIED-PR484','MAIN-489']),('xslt30-case-cost-refresh',['ORIGINAL-PR486','QUALIFIED-PR485','MAIN-484']),('xslt30-stack-refresh',['ORIGINAL-PR487','QUALIFIED-PR486','MAIN-485'])]
 for folder,labels in histories:
  historical=r/'docs'/folder;manifest(historical)
  for label in labels:replay(historical,label)
 def boundary(base,head):return set(subprocess.check_output(['git','diff','--name-only',base,head,'--','src','tests','tools','.github','Bosak.sln'],cwd=r,text=True).splitlines())
 assert not boundary('0432de7c0a357eeeb748d05ec1e9f4148cb737f6','2e56414097c1426d1d9c8279b7e3b38c267b9a17')
 assert boundary('a40951308950b36970f10badc1d13e46833ed1f7','4933d02204e7622f27bc23bb043c9996f7fb857c')=={'tools/xslt30_telemetry.py','tools/test_xslt30_telemetry.py','.github/workflows/xslt30-telemetry.yml'}
 assert boundary('2b292e81522d4246f3a546a1bf7528445133f146','cad7edafd63a02292aa7dd15a49e723595073f2c')=={'tools/xslt30_case_cost.py','tools/test_xslt30_case_cost.py','tools/xslt30-case-cost/CaseCosts.cs.in','.github/workflows/ci.yml'}
 assert boundary('8e2b58ebf6fad4473fbe527dfc19ee571134e84d','d90c820908cd56165f2d07c49dc7383e574a9c84')=={'tools/xslt30_stack_profile.py','tools/test_xslt30_stack_profile.py','.github/workflows/ci.yml'}
 merged=json.loads((d/'MERGE487.json').read_text());assert merged['pr']['state']=='MERGED' and merged['sourceTreeMatchesQualifiedPr']
 controls=json.loads((d/'CONTROLS.json').read_text());assert [x['count'] for x in controls['tests']]==[12,2,16,15] and all(x['exitCode']==0 for x in controls['tests'])
 assert controls['originalBroadRawHashChecked'] and not controls['newCapture'] and not controls['newDotNetExecution'] and controls['workflowValidator']['exitCode']==0 and not controls['workflowValidator']['globalEnvironmentChanged']
 for name,digest in controls['sourceGitBlobSha256'].items():
  old=subprocess.check_output(['git','show',controls['originalSourceHead']+':'+name],cwd=r)
  assert hashlib.sha256(old).hexdigest()==digest and (r/name).read_bytes().replace(b'\r\n',b'\n')==old.replace(b'\r\n',b'\n')
 baseCI=subprocess.check_output(['git','show',controls['ciBase']+':.github/workflows/ci.yml'],cwd=r).replace(b'\r\n',b'\n')
 ci=(r/'.github/workflows/ci.yml').read_bytes().replace(b'\r\n',b'\n');assert ci==baseCI+controls['ciAddition'].encode()
 for name in ['test_xslt30_case_cost.py','test_xslt30_stack_profile.py','test_xslt30_gc_analysis.py','test_xslt30_gc_extract.py']:assert ci.count(('python -m unittest discover -s tools -p '+name).encode())==1
 current=set(subprocess.check_output(['git','diff','--name-only',controls['ciBase'],'--','src','tests','tools','.github','Bosak.sln'],cwd=r,text=True).splitlines());assert current==set(controls['sourceGitBlobSha256'])|{'.github/workflows/ci.yml'}
 for folder in ['xslt30-gc-correlation','xslt30-stack-profile','xslt30-case-cost','xslt30-telemetry-delivery','xslt30-hosted-timing','streaming-wrapper-cache']:
  subprocess.run(['python',str(r/'docs'/folder/'verify.py')],cwd=r,check=True)
 print('Verified original GC domains/captures/failures, source identity, retained qualifications, historical boundaries and all four CI controls; no .NET execution or new capture.')

if __name__=='__main__':main()
