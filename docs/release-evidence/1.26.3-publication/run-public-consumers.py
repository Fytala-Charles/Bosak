from pathlib import Path
import subprocess,json,os,datetime,uuid,shutil
pub=Path('D:/Development/Bosak/.guard-tmp/release1263-publication')
stamp=datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%S%fZ')+'-'+uuid.uuid4().hex[:8];attempt=pub/stamp;attempt.mkdir()
receipt={'directory':str(attempt),'status':'RUNNING','checks':[],'nugetOrgOnly':True}
def save(): (attempt/'attempt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
def run(args,log,env=None,cwd=None):
 print('Running '+log.name,flush=True)
 with log.open('wb') as f:p=subprocess.run(args,cwd=cwd,env=env,stdout=f,stderr=subprocess.STDOUT)
 receipt['checks'].append({'log':str(log),'exitCode':p.returncode});save()
 if p.returncode:receipt['status']='FAILED_ATTEMPT_RETAINED';save();raise SystemExit(p.returncode)
for kind in ['broad','focused']:
 name=kind+'-public';project=attempt/name;shutil.copytree(pub/'templates'/kind,project)
 env=os.environ.copy();env['NUGET_PACKAGES']=str(attempt/(name+'-cache'));env['BOSAK_RELEASE_RECEIPT_DIR']=str(attempt)
 run(['dotnet','restore',str(project),'--configfile',str(project/'NuGet.Config'),'--no-http-cache'],attempt/(name+'-restore.log'),env)
 if kind=='broad':run(['dotnet','run','--project',str(project),'-c','Release','--no-restore'],attempt/(name+'-run.log'),env)
 else:run(['dotnet','test',str(project),'-c','Release','--no-restore','--logger','trx;LogFileName=public-focused.trx','--results-directory',str(attempt/'focused-results')],attempt/(name+'-tests.log'),env)
 run(['dotnet','run','--project',str(pub/'metadata/Metadata.csproj'),'-c','Release','--',str(project/'bin/Release/net10.0'),str(attempt/(name+'-versions.json'))],attempt/(name+'-metadata.log'),env)
 run(['dotnet','list','package','--no-restore','--vulnerable','--include-transitive','--format','json','--source','https://api.nuget.org/v3/index.json'],attempt/(name+'-audit.json'),env,project)
 for suffix in ['-versions.json','-audit.json']:shutil.copy2(attempt/(name+suffix),pub/(name+suffix))
receipt['status']='BOTH_PUBLIC_CONSUMERS_PASS_AUDITS_COMPLETE';save()
(pub/'final-attempt.json').write_text(json.dumps({'directory':str(attempt)},indent=2)+'\n',encoding='utf-8')
print('Both fresh nuget.org-only consumers passed; final attempt '+str(attempt),flush=True)
