"""Replay sealed allocation, regression and qualification receipts without executing .NET."""
from pathlib import Path
import hashlib,importlib.util,json,re,zipfile,xml.etree.ElementTree as ET

def main():
 dossier=Path(__file__).resolve().parent;root=dossier.parent.parent
 manifest=json.loads((dossier/'MANIFEST.json').read_text())
 for name,entry in manifest['files'].items():
  data=(dossier/name).read_bytes()
  assert len(data)==entry['bytes'] and hashlib.sha256(data).hexdigest()==entry['sha256'],name
 result=json.loads((dossier/'RESULTS.json').read_text())
 assert result['solutionPassed']==26426 and result['solutionFailed']==0 and result['languageServerPassed']==90
 assert result['baselineAllocationFailureBytes']==9600000 and result['candidateWarmHitAllocationBytes']==0 and not result['wholeSuiteSpeedupClaimed']
 with zipfile.ZipFile(dossier/'EVIDENCE.zip') as z:
  raw=lambda name:z.read(name)
  text=lambda name:raw(name).decode('utf-8-sig')
  load=lambda name:json.loads(raw(name))
  checks=load('local/CHECKS.json');assert len(checks)==4 and all(x['exitCode']==0 for x in checks)
  counts=[int(n) for n in re.findall(r'Passed!\s*- Failed:\s+0, Passed:\s+(\d+), Skipped:\s+0',text('local/UNIT-FIXTURES.log'))]
  assert len(counts)==10 and sum(counts)==26426
  assert re.search(r'Passed:\s+90,',text('local/LSP.log'))
  trx=ET.fromstring(raw('local/baseline.trx'));ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
  tests={x.attrib['testName']:x.attrib['outcome'] for x in trx.findall('.//t:UnitTestResult',ns)}
  assert len(tests)==4 and tests.pop('Bosak.XPath.Providers.Tests.Streaming.StreamingWrapperCacheTests.WarmCacheHitsDoNotAllocate')=='Failed' and set(tests.values())=={'Passed'}
  assert 'Actual:   9600000' in text('local/BASELINE.log')
  baseline=load('local/BASELINE.json');assert len(baseline['engineHashes'])==9 and baseline['expectedAllocationFailureOnly']
  retained=load('baseline/RUNTIME-HASHES.json');assert all(retained[name]==value for name,value in baseline['engineHashes'].items())
  assert load('baseline/GATE.json')['success']
  assert hashlib.sha256(raw('source/StreamingWrapperCacheTests.cs.txt')).hexdigest()==baseline['testSourceSha256']
  for gate,count in [('QT3',424),('QT4',1918)]:
   log=text('local/'+gate+'.log')
   assert re.search(r'^Passed:\s+'+str({'QT3':31397,'QT4':8941}[gate])+r'\s*$',log,re.M)
   assert re.search(r'^Failed:\s+0\s*$',log,re.M)
   assert re.search(r'^Skipped:\s+'+str(count)+r'\s*$',log,re.M)
   def parse(s):
    result={}
    for n,reason,body in re.findall(r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)',s,re.M|re.S):
     names=[x for x in body.splitlines() if x.strip()];assert len(names)==int(n)
     for name in names:assert name not in result;result[name]=reason
    return result
   actual=parse(text('local/'+gate+'-SKIPS.txt'));assert len(actual)==count
   assert actual==parse((root/('docs/conformance/evidence/'+gate.lower()+'-skips.txt')).read_text(encoding='utf-8-sig'))
  spec=importlib.util.spec_from_file_location('wrapper_gate',root/'tools/xslt30_gate.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
  policy,_,inputs,skips=module.baseline()
  plan,run=load('standalone/PLAN.json'),load('standalone/RUN.json')
  for name in ['stdout.log','stderr.log']:assert hashlib.sha256(raw('standalone/'+name)).hexdigest()==run[name+'Sha256']
  rows=module.reconcile(plan,run,text('standalone/stdout.log'),skips,policy)
  assert len(rows)==14601 and load('standalone/GATE.json')['success']
  assert load('standalone/CORPUS-BEFORE.json')==inputs==load('standalone/CORPUS-AFTER.json')
  assert run['success'] and run['status']=='Completed'
 print('Verified: 26,426 solution tests, 90 LSP tests, allocation regression, frozen skips and 14,601 standalone identities. No .NET executed.')

if __name__=='__main__':main()
