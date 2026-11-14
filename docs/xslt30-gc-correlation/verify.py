"""Replay GC domains and sampled allocations offline, never executing .NET or the workload."""
import argparse,collections,hashlib,importlib.util,io,json,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent;root=here.parents[1]
p=argparse.ArgumentParser();p.add_argument('--broad-trace',type=Path);args=p.parse_args()
manifest=json.loads((here/'MANIFEST.json').read_text())
def check(data,value):assert len(data)==value['bytes'] and hashlib.sha256(data).hexdigest()==value['sha256']
for name,value in manifest['files'].items():check((here/name).read_bytes(),value)
spec=importlib.util.spec_from_file_location('gc',root/'tools/xslt30_gc_analysis.py');gc=importlib.util.module_from_spec(spec);spec.loader.exec_module(gc)
with zipfile.ZipFile(here/'EVIDENCE.zip') as z:
 assert set(z.namelist())==set(manifest['archiveEntries'])
 for name,value in manifest['archiveEntries'].items():check(z.read(name),value)
 read=lambda name:json.loads(z.read(name))
 for label,filename in [('original','ORIGINAL-REPORT.json'),('narrow','ALLOCATION-REPORT.json')]:
  events=[json.loads(s) for s in z.read(label+'/gc-events.jsonl').decode().splitlines()]
  report=gc.analyze(events)
  if label=='original':report['stackCorrelation']=gc.correlate(read('original/stack.speedscope.json'),report['gcSuspensionWindows'])
  assert json.loads(json.dumps(report))==json.loads((here/filename).read_text())
  assert not report['unmatched']
  meta=read(label+'/gc-meta.json');assert meta['events']==len(events) and meta['workloadExecutions']==0
 assert json.loads((here/'ALLOCATION-REPORT.json').read_text())['allocationTicks']==39308
 assert json.loads((here/'ORIGINAL-REPORT.json').read_text())['allocationTicks']==0
 allowed=json.loads((here/'BASELINE.json').read_text())['allowedTraces']
 for label in ['original','narrow']:assert hashlib.sha256(z.read(label+'/raw.nettrace')).hexdigest()==allowed[label]['sha256']
 policy=json.loads((here/'WORKLOAD-BASELINE.json').read_text());assert hashlib.sha256((here/'REFERENCE.zip').read_bytes()).hexdigest()==policy['referenceSha256']
 for label,expected in [('broad','TraceSizeExceeded'),('narrow','Completed')]:
  receipt=read(label+'/COLLECT.json');assert receipt['status']==expected and receipt['timeoutSeconds']==180 and receipt['memoryLimitBytes']==8589934592
  assert receipt['success']==(label=='narrow')
  assert read(label+'/RUNTIME-BEFORE.json')==read(label+'/RUNTIME-AFTER.json')
  assert read(label+'/CORPUS-BEFORE.json')==read(label+'/CORPUS-AFTER.json')
 narrow=read('narrow/RESULTS.json');assert narrow['diagnosticOnly'] and not narrow['qualification'] and narrow['selectedCases']==54 and narrow['passed']==52 and narrow['skipped']==2
 rows=[json.loads(s) for s in z.read('narrow/work/case-costs.jsonl').decode().splitlines()]
 assert len(rows)==54 and {r['test']:r['result'] for r in rows}==policy['expectedResults']
 assert read('narrow/TARGET-FRAMEWORK-AFTER.json')==read('narrow/CASE-COST-PIN.json')['frameworkFiles']
 assert read('narrow/PROFILER-AFTER.json')==json.loads((here/'PROFILER-FILES.json').read_text())
 command=read('narrow/COLLECT.json')['command'];assert 'Microsoft-Windows-DotNETRuntime:0x1:5' in command and 'gc-verbose' not in command
 broad=json.loads((here/'BROAD-TRACE.json').read_text());assert broad['bytes']>broad['thresholdBytes'] and broad['retainedLocally'] and not broad['publicArchiveContainsRaw']
 if args.broad_trace:assert args.broad_trace.stat().st_size==broad['bytes'] and hashlib.sha256(args.broad_trace.read_bytes()).hexdigest()==broad['sha256']
 for name,count in [('CONTROLS-analysis.log',12),('CONTROLS-extract.log',2)]:
  text=z.read(name).decode('utf-8-sig');assert f'Ran {count} tests' in text and text.rstrip().endswith('OK')
 failed=z.read('PR487-FINAL8FE.zip');meta=read('PR487-FINAL8FE-META.json');assert meta['artifact']['digest']=='sha256:'+hashlib.sha256(failed).hexdigest()
 with zipfile.ZipFile(io.BytesIO(failed)) as archive:
  run=json.loads(archive.read('RUN.json'));assert run['status']=='TimedOut' and not run['success'] and run['elapsedSeconds']==1200.234
print('Original/narrow GC domains, allocation sampling, selected outcomes, failed broad capture, unchanged inventories, controls and original failed gate replayed; no .NET execution. Broad raw bytes checked only when --broad-trace is supplied.')
