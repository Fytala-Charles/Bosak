"""Replay sealed selected-stack evidence without .NET execution or installed profilers."""
import ast,collections,hashlib,importlib.util,json,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent;root=here.parents[1]
manifest=json.loads((here/'MANIFEST.json').read_text())
def check(data,expected):assert len(data)==expected['bytes'] and hashlib.sha256(data).hexdigest()==expected['sha256']
for name,expected in manifest['files'].items():check((here/name).read_bytes(),expected)
spec=importlib.util.spec_from_file_location('profile',root/'tools/xslt30_stack_profile.py');profile=importlib.util.module_from_spec(spec);spec.loader.exec_module(profile)
policy=profile.baseline()
with zipfile.ZipFile(here/'REFERENCE.zip') as ref:
 reference_hashes=json.loads(ref.read('diagnostic/RUNTIME-HASHES.json'));reference_identity=json.loads(ref.read('diagnostic/IDENTITY.json'))
with zipfile.ZipFile(here/'EVIDENCE.zip') as z:
 assert set(z.namelist())==set(manifest['archiveEntries'])
 for name,expected in manifest['archiveEntries'].items():check(z.read(name),expected)
 read=lambda name:json.loads(z.read(name))
 plan=read('profile/PLAN.json');assert plan['diagnosticOnly'] and not plan['qualification'] and plan['policy']==policy
 for label in ['COLLECT','CONVERT','TOP-EXCLUSIVE','TOP-INCLUSIVE']:
  r=read('profile/'+label+'.json');assert r['status']=='Completed' and r['success'] and r['exitCode']==0 and r['timeoutSeconds']==180 and r['memoryLimitBytes']==8589934592
 command=read('profile/COLLECT.json')['command'];assert command[1:6]==['exec','--fx-version','8.0.28','--roll-forward','Disable']
 child=command[command.index('--')+1:];assert child[1:6]==['exec','--fx-version','10.0.11','--roll-forward','Disable'] and child[-1]=='sx-GeneralComp-gt'
 assert 'dotnet-common,dotnet-sampled-thread-time' in command
 rows=[json.loads(line) for line in z.read('profile/work/case-costs.jsonl').decode().splitlines()]
 profile.validate_cases(rows,policy['expectedResults']);assert len(rows)==54
 native=read('profile/work/case-cost-runtime.json');pin=read('profile/CASE-COST-PIN.json')
 assert native['frameworkVersion']==pin['frameworkVersion']=='10.0.11'
 for name,value in pin['nativeFiles'].items():assert native['modules'][name]['observedLoaded'] and native['modules'][name]['sha256']==value['sha256']
 assert pin['frameworkFiles']==read('profile/TARGET-FRAMEWORK-AFTER.json')
 assert read('profile/COLLECTOR-FRAMEWORK.json')['files']==read('profile/COLLECTOR-FRAMEWORK-AFTER.json')
 assert read('profile/PROFILER-AFTER.json')==plan['profilerFiles']==json.loads((here/'PROFILER-FILES.json').read_text())
 assert read('profile/RUNTIME-BEFORE.json')==read('profile/RUNTIME-AFTER.json')==reference_hashes
 assert len(plan['engineHashes'])==9 and all(reference_hashes[n]==value for n,value in plan['engineHashes'].items())
 assert read('profile/CORPUS-BEFORE.json')==read('profile/CORPUS-AFTER.json')
 end=read('profile/work/case-cost-end.json');assert end['rows']==54 and not end['errors']
 data=read('profile/stack.speedscope.json');computed=profile.stack_report(data);report=json.loads((here/'REPORT.json').read_text())
 for key,value in computed.items():assert report[key]==value,key
 assert report==read('profile/REPORT.json') and report['diagnosticOnly'] and not report['qualification']
 assert report['selectedOutcomes']==dict(collections.Counter(r['result'] for r in rows))=={'Pass':52,'Skip':2}
 assert report['caseWallSeconds']==sum(r['wallSeconds'] for r in rows) and report['caseCpuSeconds']==sum(r['cpuSeconds'] for r in rows)
 assert report['outsideSelection']==14547 and report['selectedCases']==54
 assert hashlib.sha256(z.read('profile/raw.nettrace')).hexdigest()==report['traceSha256'] and len(z.read('profile/raw.nettrace'))<268435456
 assert report['engineCategoryWeight']>0 and report['engineWithoutCallerAncestryWeight']>0
 original=read('profile/REPORT-INITIAL.json');assert original['traceSha256']==report['traceSha256']
 assert hashlib.sha256(z.read('observer/TOOL-AT-COLLECTION.py')).hexdigest()==plan['toolSourceSha256']
 def functions(data):
  text=data.decode();return {n.name:ast.get_source_segment(text,n) for n in ast.parse(text).body if isinstance(n,ast.FunctionDef)}
 a=functions(z.read('observer/TOOL-AT-COLLECTION.py'));b=functions(z.read('observer/xslt30_stack_profile.py'))
 for name in ['prepare','validate','bounded','commands','validate_cases']:assert a[name]==b[name]
 assert b'Ran 16 tests' in z.read('CONTROLS.log') and z.read('CONTROLS.log').rstrip().endswith(b'OK')
 failed=z.read('PR486-FINAL2AD.zip');meta=read('PR486-FINAL2AD-META.json');assert meta['artifact']['digest']=='sha256:'+hashlib.sha256(failed).hexdigest()
 import io
 with zipfile.ZipFile(io.BytesIO(failed)) as old:
  r=json.loads(old.read('RUN.json'));assert r['status']=='TimedOut' and not r['success'] and r['elapsedSeconds']==1200.25
print('Selected outcomes/exclusions, exact caller/engines/native/tool pins, one raw trace, corrected cross-thread stack views, source boundaries, controls and original failed gate verified; no .NET execution.')
