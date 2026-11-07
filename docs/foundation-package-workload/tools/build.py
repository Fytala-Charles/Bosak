"""Build the diagnostic consumer from a local-only package feed and fresh cache."""
from pathlib import Path
import json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1]
c=json.loads((P/'COHORT.json').read_text());env=os.environ.copy();env['NUGET_PACKAGES']=c['cache']
env['NUGET_HTTP_CACHE_PATH']=str(Path(c['work'])/'http-cache');env['NUGET_FALLBACK_PACKAGES']=''
for name,command in [
    ('restore',['dotnet','restore',str(P/'workload/Workload.csproj'),'--packages',c['cache'],'--configfile',c['config'],'-p:NuGetAudit=false','-p:RestoreFallbackFolders=']),
    ('build',['dotnet','build',str(P/'workload/Workload.csproj'),'-c','Release','--no-restore'])]:
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/package-workload-{name}.log'),
        '--timeout-seconds','180','--',*command],cwd=R,env=env,check=True)
print('Built package-backed finite workload; native/provider binaries unchanged.')
