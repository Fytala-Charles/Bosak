from pathlib import Path
import json,subprocess,sys,shutil,tempfile
R=Path.cwd();P=R/'docs/foundation-captured-identity-production';E=P/'evidence';rows=json.loads((E/'UNIT-INPUTS.json').read_text(encoding='utf-8'));results=[]
for row in rows:
 if 'database.tests' in row['project']:continue # Separate authorized localhost run.
 binary=Path(row['dll'])
 if 'xslt.tests' in row['project']:
  target=Path(tempfile.mkdtemp(prefix='BosakProduction1811Xslt-'));shutil.copytree(binary.parent,target,dirs_exist_ok=True);binary=target/binary.name
 log=f'docs/foundation-r2-closure/evidence/execution/captured-production-unit-{row["project"]}.log';trx=E/'unit-trx'/row['project']
 print('Testing '+row['project']+' expected '+str(row['expected']),flush=True)
 result=subprocess.run([sys.executable,'docs/foundation-r2-closure/tools/run.py','--timeout-seconds','900','--log',log,'--','dotnet','vstest',str(binary),'--logger:trx','--ResultsDirectory:'+str(trx)],cwd=R)
 results.append({**row,'launchedBinary':str(binary),'log':log,'exitCode':result.returncode,'resultsDirectory':str(trx)})
 (E/'UNIT-EXECUTIONS.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if result.returncode:raise SystemExit(result.returncode)
print('Nine production unit projects completed;database project is a separate authorized lane.',flush=True)
