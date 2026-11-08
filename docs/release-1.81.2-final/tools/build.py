"""Build the pinned source defaults and seal ten stock packages without an overlay."""
from pathlib import Path
import json,subprocess,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());s=Path(p['source']);w=Path(p['work']);props=['-p:SourceRevisionId='+p['candidateCommit'],'-p:RepositoryCommit='+p['candidateCommit'],'-p:ContinuousIntegrationBuild=true','-p:NuGetAudit=false']
steps=[('solution-build',['dotnet','build',str(s/'Bosak.sln'),'-c','Release','--configfile',str(w/'NuGet.Config'),*props]),('server-publish',['dotnet','publish',str(s/'src/Bosak.LanguageServer'),'-c','Release','-p:UseAppHost=false','--configfile',str(w/'NuGet.Config'),'-o',str(w/'server'),*props])]
steps += [('pack-'+n,['dotnet','pack',str(s/'src'/n/(n+'.csproj')),'-c','Release','--no-build','--no-restore','-o',p['feed'],*props]) for n in p['selectedPackages']]
steps += [('seal-nuget',['python',str(s/'.github/scripts/release_artifacts.py'),'seal','nuget',p['feed']])]
for name,cmd in steps:
 log=P/'evidence'/(name+'.txt');assert not log.exists()
 with log.open('x',encoding='utf-8') as out:result=subprocess.run(cmd,cwd=s,stdout=out,stderr=subprocess.STDOUT,timeout=600)
 (P/'evidence'/(name+'.json')).write_text(json.dumps({'command':cmd,'exitCode':result.returncode,'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()},indent=2)+'\n');print(name,result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
