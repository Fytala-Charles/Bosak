"""Compile V3 declarations and run synthetic ingress/endpoint guard controls only."""
from pathlib import Path
import hashlib,io,json,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=ROOT/'.guard-tmp/advisor-contract';W.mkdir(parents=True,exist_ok=True)
feed=ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip'
with zipfile.ZipFile(feed) as z:
    pkg=z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
with zipfile.ZipFile(io.BytesIO(pkg)) as z:core=z.read('lib/net10.0/Bosak.XPath.Core.dll')
(W/'Bosak.XPath.Core.dll').write_bytes(core);(W/'Contracts.cs').write_bytes((P/'SIGNATURES-V3.cs.txt').read_bytes());(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup><Reference Include="Bosak.XPath.Core"><HintPath>Bosak.XPath.Core.dll</HintPath><Private>true</Private></Reference></ItemGroup></Project>'
(W/'Probe.csproj').write_text(project,encoding='utf-8');(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
a=subprocess.run(['dotnet','build',str(W/'Probe.csproj'),'-c','Release','--configfile',str(W/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120);(P/'evidence/build.txt').write_text(a.stdout+a.stderr,encoding='utf-8')
if a.returncode:raise RuntimeError(a.stdout+a.stderr)
for run in range(3):
    a=subprocess.run(['dotnet',str(W/'bin/Release/net10.0/Probe.dll')],capture_output=True,text=True,timeout=30);(P/f'evidence/run-{run}.stderr.txt').write_text(a.stderr,encoding='utf-8')
    if a.returncode:raise RuntimeError(a.stdout+a.stderr)
    json.loads(a.stdout);(P/f'evidence/run-{run}.json').write_text(a.stdout,encoding='utf-8')
parent=ROOT/'docs/foundation-provenance-lifetime/SIGNATURES-V2.cs.txt'
(P/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',draftSourceSha256=hashlib.sha256((P/'SIGNATURES-V3.cs.txt').read_bytes()).hexdigest(),parentDraftPath='docs/foundation-provenance-lifetime/SIGNATURES-V2.cs.txt',parentDraftSha256=hashlib.sha256(parent.read_bytes()).hexdigest(),feedSha256=hashlib.sha256(feed.read_bytes()).hexdigest(),coreSha256=hashlib.sha256(core).hexdigest(),project=project,scope='Synthetic V3 protocol controls; no actual validator, compiler capture or compatibility algorithm',productionModified=False),indent=2)+'\n',encoding='utf-8')
print('V3 compiled with XML docs/warnings as errors; three synthetic protocol runs retained.')
