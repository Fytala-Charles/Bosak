from pathlib import Path
import json,subprocess,sys,shutil,tempfile
R=Path.cwd();P=R/'docs/foundation-captured-identity-qualification';E=P/'evidence';d=json.loads((E/'INPUTS.json').read_text(encoding='utf-8'))
initial=E/'UNIT-EXECUTIONS.json';preserved=E/'UNIT-EXECUTIONS.initial.json';assert not preserved.exists();shutil.copy2(initial,preserved)
results=json.loads(initial.read_text(encoding='utf-8'));assert len(results)==8 and results[-1]['exitCode']!=0
fixed={**results[-1],'log':'docs/foundation-r2-closure/evidence/execution/captured-qualification-database-localhost.log','exitCode':0,'resultsDirectory':str(E/'database-fixed-trx'),'localHttpAuthorization':True}
receipt=json.loads((R/(fixed['log']+'.json')).read_text(encoding='utf-8'));assert receipt['exitCode']==0
results[-1]=fixed
for row in d['units'][8:]:
 binary=Path(row['dll'])
 if row['project'].lower()=='bosak.xslt.tests':
  target=Path(tempfile.mkdtemp(prefix='BosakCapturedIdentityXslt-'));shutil.copytree(binary.parent,target,dirs_exist_ok=True);binary=target/binary.name
 slug=row['project'].lower();trx=E/'unit-trx'/slug;log=f'docs/foundation-r2-closure/evidence/execution/captured-qualification-unit-{slug}.log'
 print('Testing '+row['project']+' expected '+str(row['expected']),flush=True)
 command=[sys.executable,'docs/foundation-r2-closure/tools/run.py','--timeout-seconds','900','--log',log,'--','dotnet','vstest',str(binary),'--logger:trx','--ResultsDirectory:'+str(trx)]
 result=subprocess.run(command,cwd=R)
 results.append({**row,'launchedBinary':str(binary),'log':log,'exitCode':result.returncode,'resultsDirectory':str(trx)})
 initial.write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if result.returncode:raise SystemExit(result.returncode)
print('All ten candidate unit projects completed;localhost retry retained separately.',flush=True)
