"""Run bounded candidate regression and frozen conformance lanes with retained logs."""
from pathlib import Path
import argparse,json,subprocess,os,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());s=Path(p['source']);w=Path(p['work']);a=argparse.ArgumentParser();a.add_argument('--lane',choices=['units','gates'],required=True);lane=a.parse_args().lane
props=['-p:SourceRevisionId='+p['candidateCommit'],'-p:RepositoryCommit='+p['candidateCommit'],'-p:ContinuousIntegrationBuild=true','-p:NuGetAudit=false'];env=os.environ.copy();cohort=json.loads((R/'docs/foundation-package-workload/COHORT.json').read_text());hook=Path(cohort['hook']['path']);assert hashlib.sha256(hook.read_bytes()).hexdigest()==cohort['hook']['sha256'];env['DOTNET_STARTUP_HOOKS']=str(hook);env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(P/'evidence/bindings'/lane)
steps=[('solution-tests',['dotnet','test',str(s/'Bosak.sln'),'-c','Release','--no-build','--no-restore','--logger:trx','--results-directory',str(P/'evidence/solution-trx'),*props]),('server-tests',['dotnet','test',str(s/'tests/Bosak.LanguageServer.Tests'),'-c','Release','-p:RestoreConfigFile='+str(w/'NuGet.Config'),'--logger:trx','--results-directory',str(P/'evidence/server-trx'),*props])] if lane=='units' else [('qt3',['dotnet',str(s/'tests/Bosak.XPath.Conformance/bin/Release/net10.0/Bosak.XPath.Conformance.dll'),str(R/'tests/qt3tests')]),('qt4',['dotnet',str(s/'tests/Bosak.XPath.Conformance40/bin/Release/net10.0/Bosak.XPath.Conformance40.dll'),str(R/'tests/qt4tests')])]
for name,cmd in steps:
 if name in ['qt3','qt4']:
  env['BOSAK_'+name.upper()+'_DUMP_SKIPS']=str(P/'evidence'/(name+'-skips.txt'))
  if name=='qt4':env['BOSAK_QT4_GATE_ONLY']='1'
 log=R/'docs/foundation-r2-closure/evidence/execution'/('final-1812-'+name+'.log');wrapper=[os.sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',str(log),'--timeout-seconds','900','--',*cmd];print('Running',name,flush=True);code=subprocess.run(wrapper,cwd=R,env=env).returncode
 (P/'evidence'/(name+'-execution.json')).write_text(json.dumps({'command':cmd,'exitCode':code,'log':log.relative_to(R).as_posix()},indent=2)+'\n');print(name,code,flush=True)
 if code:raise SystemExit(code)
