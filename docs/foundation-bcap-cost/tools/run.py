"""Run fresh-process workload observations and verify loaded immutable candidate hashes."""
from pathlib import Path
import subprocess,json,hashlib,sys,statistics
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-cost'
if (P/'MANIFEST.json').exists():raise RuntimeError('Sealed cohort; do not overwrite measurements or the pinned host')
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def run(args,n):
 with process_error_mode(),(E/n).open('w',encoding='utf-8') as log:r=subprocess.run([str(a) for a in args],cwd=R,stdout=log,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError('Failure retained: '+n)
run(['dotnet','build',W/'Cost.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'host-build.txt')
i=json.loads((E/'ENGINE-INPUTS.json').read_text());host=W/'bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll';hostSha=sha(host);records=[]
for profile in ['source-free','shared','distinct-matched','distinct','cancellation']:
 for repetition in range(3):
  n=f'{profile}-{repetition}.json';run(['dotnet',host,profile],n);row=json.loads((E/n).read_text());assert row['passed']
  for v in row['bindings']:assert v['sha256']==(hostSha if v['name']=='Bosak.XPath.Runtime.Tests' else i['engines'][v['name']]['sha256']),v['name']
  records.append({'profile':profile,'repetition':repetition,'receipt':n,'sha256':sha(E/n)})
for v in i['engines'].values():assert sha(Path(v['path']))==v['sha256']
(E/'RUNS.json').write_text(json.dumps({'hostAssemblySha256':hostSha,'hostAssemblyPath':str(host),'runs':records,'engineRebuild':False,'budgetsAccepted':False},indent=2)+'\n')
print('Twelve cold/warm workload processes and three cancellation processes passed; immutable loaded engine hashes verified.')
