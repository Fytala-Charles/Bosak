"""Build and replay the standalone proposed lifetime composition against exact draft signatures."""
from pathlib import Path
import hashlib,io,json,subprocess,uuid,zipfile
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=ROOT/'.guard-tmp/provenance-lifetime';W.mkdir(parents=True,exist_ok=True)
feed=ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip'
with zipfile.ZipFile(feed) as z:
    package=z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
with zipfile.ZipFile(io.BytesIO(package)) as z:core=z.read('lib/net10.0/Bosak.XPath.Core.dll')
(W/'Bosak.XPath.Core.dll').write_bytes(core)
(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
draft=P/'SIGNATURES-V2.cs.txt';(W/'Contracts.cs').write_bytes(draft.read_bytes())
project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup><Reference Include="Bosak.XPath.Core"><HintPath>Bosak.XPath.Core.dll</HintPath><Private>true</Private></Reference></ItemGroup></Project>'
(W/'Probe.csproj').write_text(project,encoding='utf-8');(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
build=subprocess.run(['dotnet','build',str(W/'Probe.csproj'),'-c','Release','--configfile',str(W/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
(P/'evidence/build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
if build.returncode:raise RuntimeError(build.stdout+build.stderr)
for run in range(3):
    fixture=W/('owned-fixtures-'+uuid.uuid4().hex)
    child=subprocess.run(['dotnet',str(W/'bin/Release/net10.0/Probe.dll'),str(fixture)],capture_output=True,text=True,timeout=30)
    (P/f'evidence/run-{run}.stderr.txt').write_text(child.stderr,encoding='utf-8')
    if child.returncode:raise RuntimeError(child.stdout+child.stderr)
    result=json.loads(child.stdout)
    assert not fixture.exists(),'Fresh owned fixture directory not removed'
    (P/f'evidence/run-{run}.json').write_text(child.stdout,encoding='utf-8')
(P/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',draftPath='docs/foundation-provenance-lifetime/SIGNATURES-V2.cs.txt',draftSha256=hashlib.sha256(draft.read_bytes()).hexdigest(),feedSha256=hashlib.sha256(feed.read_bytes()).hexdigest(),coreSha256=hashlib.sha256(core).hexdigest(),project=project,parentDraftPath='docs/foundation-contract-design/SIGNATURES.cs.txt',parentDraftSha256=hashlib.sha256((ROOT/'docs/foundation-contract-design/SIGNATURES.cs.txt').read_bytes()).hexdigest(),scope='Standalone implementation of proposed admission interfaces with actual exclusive file handles; not Core/Schema/XmlDb/Braid implementation',productionModified=False,fixtureDirectoriesRemoved=True),indent=2)+'\n',encoding='utf-8')
print('Three controlled lifetime runs complete; all expected failures handled; fresh fixture directories removed.')
