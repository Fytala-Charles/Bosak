from pathlib import Path
import subprocess,json,time,concurrent.futures,shutil
r=Path.cwd();selection=json.loads((r/'docs/braid-compatibility/evidence/source-selection.json').read_text());pin=selection['baselineEnginePin'];core=selection['coreHead']
source=r/'.guard-tmp/braid-candidate/tests/Bosak.Braid.Infrastructure.Tests/ReadableInsertionIntegrationTests.cs'
original=source.read_bytes();(r/'docs/braid-compatibility/evidence/original-pin-assertion-sha256.txt').write_text(__import__('hashlib').sha256(original).hexdigest()+'\n')
text=original.decode('utf-8-sig');before='Assert.Equal("1.26.2+'+pin+'", adapter.Capabilities.EngineVersion);';after='Assert.Equal("1.34.1+'+core+'", adapter.Capabilities.EngineVersion);';assert text.count(before)==1
source.write_text(text.replace(before,after),encoding='utf-8',newline='\n')
(r/'docs/braid-compatibility/evidence/pin-expectation-migration.json').write_text(json.dumps({'scope':'Isolated candidate test fixture only; original Braid unchanged','before':before,'after':after,'productionChange':False,'originalFullRunPreserved':True},indent=2)+'\n')
def run(mode):
 cmd=['dotnet','test',f'.guard-tmp/braid-{mode}/tests/Bosak.Braid.Infrastructure.Tests','-c','Release','-p:MinVerSkip=true','--filter','FullyQualifiedName~ReadableCandidate_EncodingAndRepeat_ExactEvidence','--logger','trx','--results-directory',str(r/f'.guard-tmp/braid-{mode}-followup-results')]
 if mode=='baseline':cmd.insert(6,'-p:SourceRevisionId='+pin)
 start=time.monotonic()
 with (r/f'.guard-tmp/braid-{mode}-followup.log').open('w',encoding='utf-8') as log:
  process=subprocess.Popen(cmd,stdout=log,stderr=subprocess.STDOUT)
  try:code=process.wait(timeout=120);row={'mode':mode,'exitCode':code,'timedOut':False}
  except subprocess.TimeoutExpired:
   subprocess.run(['taskkill','/PID',str(process.pid),'/T','/F'],stdout=log,stderr=subprocess.STDOUT);row={'mode':mode,'timedOut':True}
 row.update(elapsedSeconds=round(time.monotonic()-start,3),externalTimeoutSeconds=120);print(row,flush=True);return row
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(run,['baseline','candidate']))
(r/'.guard-tmp/braid-followup-processes.json').write_text(json.dumps(results,indent=2)+'\n')
