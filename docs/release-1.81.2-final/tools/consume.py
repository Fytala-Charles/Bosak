"""Restore/build/run a consumer with a fresh private cache and only a local feed."""
from pathlib import Path
import hashlib,json,os,shutil,subprocess,sys

P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());w=Path(p['work'])
consumer=w/'consumer';consumer.mkdir();cache=w/'consumer-cache';assert not cache.exists()
cache.mkdir();assert not list(cache.iterdir())
shutil.copyfile(P/'consumer/Program.cs',consumer/'Program.cs')
(consumer/'Consumer.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<RestoreFallbackFolders></RestoreFallbackFolders><RestoreAdditionalProjectFallbackFolders></RestoreAdditionalProjectFallbackFolders>
</PropertyGroup><ItemGroup><PackageReference Include="Bosak.Xslt" Version="[{p['previewVersion']}]" /><PackageReference Include="Bosak.XPath.Providers.Database" Version="[{p['previewVersion']}]" /></ItemGroup></Project>''',encoding='utf-8')
config=consumer/'NuGet.Config'
config.write_text(f'<configuration><packageSources><clear/><add key="local-preview" value="{p["feed"]}"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>')
env=os.environ.copy();env['NUGET_PACKAGES']=str(cache);env['NUGET_HTTP_CACHE_PATH']=str(w/'http-cache');env['NUGET_FALLBACK_PACKAGES']=''
def run(name,command):
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/final-1812-{name}.log'),
        '--timeout-seconds','180','--',*command],env=env,cwd=R,check=True)
run('consumer-restore',['dotnet','restore',str(consumer/'Consumer.csproj'),'--configfile',str(config),'--packages',str(cache),'-p:NuGetAudit=false','-p:RestoreFallbackFolders='])
run('consumer-build',['dotnet','build',str(consumer/'Consumer.csproj'),'-c','Release','--no-restore'])
cohort=json.loads((R/'docs/foundation-workload-1811/COHORT.json').read_text())
hook=Path(cohort['hook']['path']);assert hashlib.sha256(hook.read_bytes()).hexdigest()==cohort['hook']['sha256']
env['DOTNET_STARTUP_HOOKS']=str(hook);env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(P/'evidence/bindings')
run('consumer-run',['dotnet',str(consumer/'bin/Release/net10.0/Consumer.dll'),str(P/'evidence/CONSUMER-RESULTS.json')])
print('Fresh private-cache package-only consumer completed; no project or HintPath references.')
