"""Replay sealed copied-runner diagnostics without launching .NET or reading installed runtimes."""
import collections,csv,hashlib,importlib.util,io,json,re,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent;root=here.parents[1]
def check(data,value):assert len(data)==value['bytes'] and hashlib.sha256(data).hexdigest()==value['sha256']
manifest=json.loads((here/'MANIFEST.json').read_text())
for name,value in manifest['files'].items():check((here/name).read_bytes(),value)
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
cost=load('cost',root/'tools/xslt30_case_cost.py');gate=cost.load_gate();policy,_,inputs,skips=gate.baseline()
with zipfile.ZipFile(here/'EVIDENCE.zip') as z:
 assert set(z.namelist())==set(manifest['archiveEntries'])
 for name,value in manifest['archiveEntries'].items():check(z.read(name),value)
 read=lambda name:json.loads(z.read(name))
 run,plan,identity=read('diagnostic/RUN.json'),read('diagnostic/PLAN.json'),read('diagnostic/IDENTITY.json')
 assert run['status']=='Completed' and run['success'] and run['sourceCommit'] is None
 assert identity['diagnosticOnly'] and not identity['qualification']
 assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 assert run['command'][1:6]==['exec','--fx-version','10.0.11','--roll-forward','Disable']
 assert read('diagnostic/CORPUS-BEFORE.json')==inputs==read('diagnostic/CORPUS-AFTER.json')
 before=read('diagnostic/RUNTIME-HASHES.json');after=read('diagnostic/RUNTIME-AFTER.json');baseline=read('diagnostic/BASELINE-RUNTIME-HASHES.json')
 assert before==after and baseline==cost.qualified_hashes()
 assert len(identity['engineHashes'])==9 and all(before[n]==baseline[n]==h for n,h in identity['engineHashes'].items())
 assert set(identity['changedApplicationFiles'])=={'Bosak.Xslt.Conformance.dll','Bosak.Xslt.Conformance.pdb','Bosak.Xslt.Conformance.deps.json','Bosak.Xslt.Conformance.runtimeconfig.json'}
 for name in identity['changedApplicationFiles']:assert hashlib.sha256(z.read('diagnostic/runtime/'+name)).hexdigest()==before[name]
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read('diagnostic/'+name)).hexdigest()==run[name+'Sha256']
 rows=gate.reconcile(plan,run,z.read('diagnostic/stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
 costs=[json.loads(s) for s in z.read('diagnostic/work/case-costs.jsonl').decode().splitlines()]
 unmeasured=cost.reconcile_costs(costs,rows)
 recorded=list(csv.DictReader(io.StringIO(z.read('diagnostic/unmeasured-skips.csv').decode('utf-8-sig'))))
 assert recorded==[{k:str(v) for k,v in row.items()} for row in unmeasured]
 pin=read('diagnostic/CASE-COST-PIN.json');native=read('diagnostic/work/case-cost-runtime.json')
 assert pin==read('diagnostic/FRAMEWORK-AFTER.json') and pin['frameworkVersion']==native['frameworkVersion']=='10.0.11'
 assert all(c['frequency']==native['frequency'] for c in costs)
 for name,value in native['modules'].items():assert value['observedLoaded'] and value['sha256']==pin['nativeFiles'][name]['sha256'] and value['path'].lower()==pin['nativeFiles'][name]['path'].lower()
 assert set(native['modules'])==set(pin['nativeFiles'])
 end=read('diagnostic/work/case-cost-end.json');assert end['rows']==len(costs) and not end['errors']
 report=read('diagnostic/REPORT.json');assert report==json.loads((here/'REPORT.json').read_text())
 assert report['totalCases']==14601 and report['timedCases']==len(costs) and report['unmeasuredSetSkips']==len(unmeasured)
 assert report['sumCaseWallSeconds']==sum(c['wallSeconds'] for c in costs) and report['sumCaseCpuSeconds']==sum(c['cpuSeconds'] for c in costs)
 assert report['topCases']==sorted(costs,key=lambda r:r['wallSeconds'],reverse=True)[:25]
 findings=read('FINDINGS.json');assert findings==json.loads((here/'FINDINGS.json').read_text())
 comparison=[c for c in costs if c['testSet'].startswith('sx-GeneralComp-')]
 assert findings['comparisonCases']==len(comparison) and findings['comparisonWallSeconds']==sum(c['wallSeconds'] for c in comparison)
 assert findings['comparisonCpuSeconds']==sum(c['cpuSeconds'] for c in comparison)
 assert findings['comparisonShareOfMeasuredWall']==sum(c['wallSeconds'] for c in comparison)/sum(c['wallSeconds'] for c in costs)
 assert findings['timedOutcomes']==dict(collections.Counter(c['result'] for c in costs))
 with zipfile.ZipFile(io.BytesIO(z.read('CI-LOGS.zip'))) as logs:
  for part,total in [('/7_Test.txt',26422),('Language-server tests.txt',90)]:
   name=next(n for n in logs.namelist() if part in n);text=logs.read(name).decode('utf-8-sig')
   assert not re.search(r'Failed:\s*[1-9]',text)
   assert sum(int(v.replace(',','')) for v in re.findall(r'Passed:\s*([0-9,]+)',text))==total
 original=z.read('original/Program.cs');generated=z.read('diagnostic/diagnostic/Program.cs')
 assert cost.instrument(original)==generated and hashlib.sha256(original).hexdigest()==identity['originalRunnerSourceSha256']
 assert cost.controller(z.read('original/run.py'))==z.read('diagnostic/diagnostic-run.py')
 for name,h in identity['diagnosticSources'].items():assert hashlib.sha256(z.read('diagnostic/diagnostic/'+name)).hexdigest()==h
 assert hashlib.sha256(z.read('observer/xslt30_case_cost.py')).hexdigest()==identity['toolSha256']
 helper=read('diagnostic/controls/RESULTS.json');assert helper['success'] and helper['controls']==3 and helper['caughtRefusal'] and not helper['unhandledNegativeLaunched']
 assert helper['helperSha256']==identity['diagnosticSources']['CaseCosts.cs']
 assert hashlib.sha256(z.read('diagnostic/controls/bin/Release/net10.0/Control.dll')).hexdigest()==helper['assemblySha256']
 log=z.read('CONTROLS.log');text=log.decode('utf-16' if log.startswith((b'\xff\xfe',b'\xfe\xff')) else 'utf-8-sig')
 assert 'Ran 15 tests' in text and text.rstrip().endswith('OK')
 for label in ['DELIVERY861-ORIGINAL','DELIVERY721-ORIGINAL','PR486-ORIGINAL99C']:
  data=z.read(label+'.zip');meta=read(label+'-META.json')
  assert meta['artifact']['digest']=='sha256:'+hashlib.sha256(data).hexdigest()
  with zipfile.ZipFile(io.BytesIO(data)) as failed:
   r=json.loads(failed.read('RUN.json'));g=json.loads(failed.read('GATE.json'))
   assert r['status']=='TimedOut' and not r['success'] and not g['success']
print('Sealed source reversibility, retained engines/native pin, all14601 outcomes, measured/unmeasured coverage, counters, controls and original failed delivery receipts replayed; no .NET execution.')
