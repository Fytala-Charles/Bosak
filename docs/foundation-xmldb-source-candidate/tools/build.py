"""Restore/build immutable XmlDb sources with an isolated candidate dependency overlay."""
from pathlib import Path
import json,os,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text())
env=os.environ.copy();env['NUGET_PACKAGES']=p['cache'];env['NUGET_FALLBACK_PACKAGES']='';env['GIT_OPTIONAL_LOCKS']='0'
source=Path(p['source']);props=['-p:NuGetAudit=false','-p:RestoreLockedMode=false',
    '-p:CustomAfterMicrosoftCommonTargets='+p['overlay'],'-p:MinVerVersionOverride='+p['minVerVersionOverride']]
for name,command in [
    ('restore',['dotnet','restore',str(source/'Bosak.XmlDb.slnx'),'--configfile',p['config'],'--packages',p['cache'],*props]),
    ('build',['dotnet','build',str(source/'Bosak.XmlDb.slnx'),'-c','Release','--no-restore',*props])]:
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/xmldb-source-{name}.log'),'--timeout-seconds','300','--',*command],
        cwd=R,env=env,check=True)
print('Isolated original XmlDb source build completed against candidate dependencies.')
