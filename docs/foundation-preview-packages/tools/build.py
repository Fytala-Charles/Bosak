"""Build and pack all nine libraries from the immutable source archive."""
from pathlib import Path
import json,subprocess,sys

P=Path(__file__).resolve().parents[1];R=P.parents[1]
p=json.loads((P/'PLAN.json').read_text());s=Path(p['source']);w=Path(p['work'])
props=[f'-p:Version={p["previewVersion"]}',f'-p:PackageVersion={p["previewVersion"]}',
    f'-p:SourceRevisionId={p["candidateCommit"]}',f'-p:RepositoryCommit={p["candidateCommit"]}',
    '-p:NuGetAudit=false', '-p:ContinuousIntegrationBuild=true']
def run(name,command):
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/preview-1811-{name}.log'),
        '--timeout-seconds','300','--',*command],cwd=R,check=True)
run('build',['dotnet','build',str(s/'src/Bosak.Xslt/Bosak.Xslt.csproj'),'-c','Release',
    '--configfile',str(w/'NuGet.Config'),*props])
for name in p['selectedPackages']:
    run('pack-'+name,['dotnet','pack',str(s/'src'/name/(name+'.csproj')),'-c','Release',
        '--no-build','--no-restore','-o',p['feed'],*props,
        f'-p:CustomAfterMicrosoftCommonTargets={s/p["overlay"]}',f'-p:CandidateSourceRoot={s}'])
print('Built and packed nine local preview libraries; no publication.')
