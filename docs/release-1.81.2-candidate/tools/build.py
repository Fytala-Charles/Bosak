"""Build ten approved local packages and the matching server from immutable source."""
from pathlib import Path
import json,subprocess,hashlib,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());s=Path(p['source']);w=Path(p['work']);props=['-p:Version='+p['version'],'-p:PackageVersion='+p['version'],'-p:SourceRevisionId='+p['candidateCommit'],'-p:RepositoryCommit='+p['candidateCommit'],'-p:NuGetAudit=false','-p:ContinuousIntegrationBuild=true']
steps=[('solution-build',['dotnet','build',str(s/'Bosak.sln'),'-c','Release','--configfile',str(w/'NuGet.Config'),*props]),('server-publish',['dotnet','publish',str(s/'src/Bosak.LanguageServer'),'-c','Release','-p:UseAppHost=false','--configfile',str(w/'NuGet.Config'),'-o',str(w/'server'),*props])]
steps += [('pack-'+n,['dotnet','pack',str(s/'src'/n/(n+'.csproj')),'-c','Release','--no-build','--no-restore','-o',p['feed'],*props,'-p:CustomAfterMicrosoftCommonTargets='+str(s/p['overlay']),'-p:CandidateSourceRoot='+str(s)]) for n in p['selectedPackages']]
for name,cmd in steps:
 if name=='solution-build' and '--retry' in sys.argv:
  name='solution-build-retry'
 log=P/'evidence'/(name+'.txt');assert not log.exists()
 with log.open('x',encoding='utf-8') as out:result=subprocess.run(cmd,cwd=R,stdout=out,stderr=subprocess.STDOUT,timeout=600)
 (P/'evidence'/(name+'.json')).write_text(json.dumps({'command':cmd,'exitCode':result.returncode,'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()},indent=2)+'\n');print(name,result.returncode,flush=True)
 if result.returncode:raise SystemExit(result.returncode)
print('Ten local packages and matching server built; nothing published.',flush=True)
