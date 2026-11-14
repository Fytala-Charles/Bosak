"""Replay the sealed current-engine observation without launching .NET or a workload."""
from pathlib import Path
import json,hashlib,zipfile,importlib.util,subprocess,re,collections

def main():
 d=Path(__file__).resolve().parent;r=d.parent.parent;m=json.loads((d/'MANIFEST.json').read_text())
 def check(data,e):assert len(data)==e['bytes'] and hashlib.sha256(data).hexdigest()==e['sha256']
 for name,e in m['files'].items():check((d/name).read_bytes(),e)
 def load(name,path):
  spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
 gate=load('current_receipt_gate',r/'tools/xslt30_gate.py');policy,_,inputs,skips=gate.baseline()
 for label in ['PR488','MAIN-488']:
  meta=json.loads((d/(label+'.json')).read_text());data=(d/(label+'.zip')).read_bytes();assert 'sha256:'+hashlib.sha256(data).hexdigest()==meta['artifact']['digest']
  with zipfile.ZipFile(d/(label+'.zip')) as z:
   read=lambda name:json.loads(z.read(name));run=read('RUN.json');receipt=read('GATE.json')
   assert run['success'] and receipt['success'] and meta['sourceTreeMatchesHead'] and run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
   for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read(name)).hexdigest()==run[name+'Sha256']
   assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json') and receipt['runtimeUnchanged'] and receipt['sourceCorpusUnchanged'] and not any(receipt['corpusDifferences'].values())
   assert len(gate.reconcile(read('PLAN.json'),run,z.read('stdout.log').decode('utf-8-sig'),skips,policy))==14601
 review=json.loads((d/'EXPERIMENT-REVIEW.json').read_text());assert review['recordedBeforeImplementation'] and review['recordedBeforeCapture']
 assert review['main']=='375f2a0158b3c2a0059f52a4ee0adcd5306a6c00' and review['qualifiedEngineCheckpoint']=='af52e427ea122cdeef70ad125a90492ed585cc9e' and len(review['engineHashes'])==9
 assert not subprocess.check_output(['git','diff',review['qualifiedEngineCheckpoint'],review['main'],'--','src','tests/Bosak.Xslt.Conformance','Bosak.sln','Directory.Build.props','global.json'],cwd=r)
 assert not subprocess.check_output(['git','diff',review['main'],'--','src','tests','tools','.github','Bosak.sln','Directory.Build.props','global.json'],cwd=r)
 controls=json.loads((d/'CONTROLS.json').read_text());assert controls['producerGuardTests']==4 and controls['readerGuardTests']==3 and controls['allPassed'] and controls['negativeLauncherMocked'] and not controls['unhandledNegativeDotNetLaunched']
 with zipfile.ZipFile(d/'EVIDENCE.zip') as z:
  assert set(z.namelist())==set(m['archiveEntries'])
  for name,e in m['archiveEntries'].items():check(z.read(name),e)
  read=lambda name:json.loads(z.read(name));raw=lambda name:z.read(name)
  assert raw('orchestration/REVIEW.json')==(d/'EXPERIMENT-REVIEW.json').read_bytes()
  for name,digest in controls['sourceHashes'].items():assert hashlib.sha256(raw('orchestration/'+name)).hexdigest()==digest
  q=read('qualification/GATE.json');assert q['success'] and q['sourceCommit']==review['qualifiedEngineCheckpoint'] and q['counts']==policy['counts']
  assert read('qualification/RUNTIME-HASHES.json')==review['qualifiedRuntimeHashes']
  before=read('capture/RUNTIME-BEFORE.json');assert before==read('capture/RUNTIME-AFTER.json')
  for name,digest in review['engineHashes'].items():assert before[name]==digest
  changed={name for name,value in before.items() if review['qualifiedRuntimeHashes'].get(name)!=value}
  assert changed=={'Bosak.Xslt.Conformance.dll','Bosak.Xslt.Conformance.pdb','Bosak.Xslt.Conformance.deps.json','Bosak.Xslt.Conformance.runtimeconfig.json'}
  assert read('capture/CORPUS-BEFORE.json')==inputs==read('capture/CORPUS-AFTER.json')
  pin=read('capture/CASE-COST-PIN.json');assert pin['frameworkFiles']==read('capture/TARGET-FRAMEWORK-AFTER.json')
  assert read('capture/COLLECTOR-FRAMEWORK.json')['files']==read('capture/COLLECTOR-FRAMEWORK-AFTER.json')
  plan=read('capture/PLAN.json');assert plan['profilerFiles']==read('capture/PROFILER-AFTER.json') and plan['provider']=='Microsoft-Windows-DotNETRuntime:0x1:5'
  collector=read('capture/COLLECT.json');assert collector['success'] and collector['status']=='Completed' and collector['timeoutSeconds']==180 and collector['memoryLimitBytes']==8589934592
  trace=raw('capture/raw.nettrace');traceHash=hashlib.sha256(trace).hexdigest();result=read('capture/RESULTS.json');assert traceHash==result['traceSha256'] and len(trace)==9597530<268435456
  producer=read('capture/PRELAUNCH.json');assert producer['reviewSha256']==hashlib.sha256(raw('orchestration/REVIEW.json')).hexdigest() and producer['sourceSha256']==hashlib.sha256(raw('orchestration/measure.py')).hexdigest()
  identity=read('capture/SOURCE-IDENTITY.json')
  for name,digest in identity['diagnosticSources'].items():assert hashlib.sha256(raw('generated/caller/'+name+'.txt')).hexdigest()==digest
  cost=load('current_reversible_cost',r/'tools/xslt30_case_cost.py');assert cost.instrument(raw('support/tests/Bosak.Xslt.Conformance/Program.cs.txt'))==raw('generated/caller/Program.cs.txt')
  rows=[json.loads(x) for x in raw('capture/work/case-costs.jsonl').decode().splitlines()];support=load('current_selected_stack',r/'tools/xslt30_stack_profile.py');support.validate_cases(rows,plan['policy']['expectedResults'])
  assert len(rows)==54 and sum(x['result']=='Pass' for x in rows)==52 and sum(x['result']=='Skip' for x in rows)==2
  end=read('capture/work/case-cost-end.json');assert end['rows']==54 and not end['errors']
  native=read('capture/work/case-cost-runtime.json');assert native['frameworkVersion']=='10.0.11'
  for name,value in pin['nativeFiles'].items():assert native['modules'][name]['sha256']==value['sha256']
  scope=json.loads((d/'SCOPE.json').read_text());assert scope['otherOriginalCasesOutsideSelectedSet']==14601-54==14547 and scope['filteredRunnerCounts']['unselected']==0
  assert {x['test'] for x in scope['skipReasons']}=={x['test'] for x in rows if x['result']=='Skip'}
  stdout=raw('capture/COLLECT.stdout.log').decode('utf-8-sig')
  for pattern in [r'^Total:\s+54\s*$',r'^Passed:\s+52\s*$',r'^Failed:\s+0\s*$',r'^Skipped:\s+2\s*$',r'^Unselected:\s+0\s*$']:assert re.search(pattern,stdout,re.M)
  reader=read('analysis/READER.json');assert reader['success'] and reader['status']=='Completed' and reader['timeoutSeconds']==120 and reader['memoryLimitBytes']==8589934592
  rr=read('analysis/READ-REVIEW.json');assert rr['onlyAllowedTrace']==traceHash and rr['completeProducerValidated'] and rr['workloadExecutions']==0 and rr['historicalWhitelistUnchanged']
  assert rr['readerSourceSha256']==hashlib.sha256(raw('generated/reader/GcEvents.cs.txt')).hexdigest()
  analysis=load('current_allocation_analysis',r/'tools/xslt30_gc_analysis.py');events=[json.loads(x) for x in raw('analysis/gc-events.jsonl').decode().splitlines()];report=analysis.analyze(events)
  assert json.loads(json.dumps(report))==read('analysis/REPORT.json')==json.loads((d/'REPORT.json').read_text()) and report['unmatched']==[]
  assert report['collections']==227 and report['allocationTicks']==36353 and report['reportedAllocationIntervalBytes']==4874098336
  historical=r/'docs/xslt30-gc-correlation';historicalManifest=json.loads((historical/'MANIFEST.json').read_text())
  for name in ['ALLOCATION-REPORT.json','EVIDENCE.zip']:check((historical/name).read_bytes(),historicalManifest['files'][name])
  prior=json.loads((historical/'ALLOCATION-REPORT.json').read_text());comparison=json.loads((d/'COMPARISON.json').read_text());assert not comparison['controlledBenchmark'] and not comparison['causalDeltaClaimed']
  for row in comparison['rows']:assert row['historical']==prior[row['metric']] and row['current']==report[row['metric']]
  with zipfile.ZipFile(historical/'EVIDENCE.zip') as previous:oldEvents=[json.loads(x) for x in previous.read('narrow/gc-events.jsonl').decode().splitlines()]
  def types(items):return collections.Counter(x['payload'].get('TypeName','') for x in items if x['name']=='GC/AllocationTick')
  oldTypes,newTypes=types(oldEvents),types(events)
  for token,values in comparison['thresholdNameSamples'].items():assert values==dict(historical=sum(v for k,v in oldTypes.items() if token in k),current=sum(v for k,v in newTypes.items() if token in k))
  assert read('analysis/gc-meta.json')['events']==len(events) and read('analysis/gc-meta.json')['workloadExecutions']==0
  runtime=before;readerFiles=read('analysis/IDENTITY.json')['readerRuntimeFiles']
 with zipfile.ZipFile(d/'REFERENCE.zip') as ref:
  for prefix,hashes in [('runtime',runtime),('reader',readerFiles)]:
   assert {name[len(prefix)+1:] for name in ref.namelist() if name.startswith(prefix+'/')}==set(hashes)
   for name,digest in hashes.items():assert hashlib.sha256(ref.read(prefix+'/'+name)).hexdigest()==digest
 merged=json.loads((d/'MERGE488.json').read_text());assert merged['pr']['state']=='MERGED' and merged['sourceTreeMatchesQualifiedPr']
 print('Verified merged qualifications, exact current implementation/binaries, one bounded54-case capture, reader chain, unchanged inventories and GC report; no .NET executed.')

if __name__=='__main__':main()
