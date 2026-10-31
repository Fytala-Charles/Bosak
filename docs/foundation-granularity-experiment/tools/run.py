"""Reproduce synthetic E-03 graph, lookup, cache and cursor comparisons in a private project."""
from pathlib import Path
import hashlib,io,json,os,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=ROOT/'.guard-tmp/granularity-experiment';W.mkdir(parents=True,exist_ok=True)
feed=ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip'
with zipfile.ZipFile(feed) as z:pkg=z.read(next(n for n in z.namelist() if n.lower().endswith('bosak.xpath.core.1.34.2.nupkg')))
with zipfile.ZipFile(io.BytesIO(pkg)) as z:core=z.read('lib/net10.0/Bosak.XPath.Core.dll')
draft=P/'CORE-CANDIDATE-V4.cs.txt'
(W/'Contracts.cs').write_bytes(draft.read_bytes());(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes());(W/'Bosak.XPath.Core.dll').write_bytes(core)
project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="Program.cs"/><Compile Include="Contracts.cs"/><Reference Include="Bosak.XPath.Core"><HintPath>Bosak.XPath.Core.dll</HintPath><Private>true</Private></Reference></ItemGroup></Project>'
(W/'Probe.csproj').write_text(project,encoding='utf-8');(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
build=subprocess.run(['dotnet','build',str(W/'Probe.csproj'),'-c','Release','--configfile',str(W/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
(P/'evidence/build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
if build.returncode:raise RuntimeError(build.stdout+build.stderr)
env=dict(os.environ,DOTNET_TieredCompilation='0',DOTNET_ReadyToRun='0')
for n in range(3):
    run=subprocess.run(['dotnet',str(W/'bin/Release/net10.0/Probe.dll'),str(n)],env=env,capture_output=True,text=True,timeout=120)
    (P/f'evidence/run-{n}.stderr.txt').write_text(run.stderr,encoding='utf-8')
    if run.returncode:raise RuntimeError(run.stdout+run.stderr)
    json.loads(run.stdout);(P/f'evidence/run-{n}.json').write_text(run.stdout,encoding='utf-8')
(P/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',draftPath='docs/foundation-granularity-experiment/CORE-CANDIDATE-V4.cs.txt',draftSha256=hashlib.sha256(draft.read_bytes()).hexdigest(),feedSha256=hashlib.sha256(feed.read_bytes()).hexdigest(),coreSha256=hashlib.sha256(core).hexdigest(),project=project,sizes=[64,4096,65536],cursorBatchSize=64,cacheEntryLimit=8,warmups=3,samples=7,rounds=3,environmentOverrides={'DOTNET_TieredCompilation':'0','DOTNET_ReadyToRun':'0'},parentDraftPath='docs/foundation-advisor-contract/SIGNATURES-V3.cs.txt',parentDraftSha256=hashlib.sha256((ROOT/'docs/foundation-advisor-contract/SIGNATURES-V3.cs.txt').read_bytes()).hexdigest(),scope='Synthetic in-process component graph/lookup implementation; not Schema provider, VM, DB or remote RPC evidence',productionModified=False),indent=2)+'\n',encoding='utf-8')
replay=W/'protocol-replay';replay.mkdir(exist_ok=True)
(replay/'Contracts.cs').write_bytes(draft.read_bytes());(replay/'Program.cs').write_bytes((ROOT/'docs/foundation-advisor-contract/tools/Probe.cs.txt').read_bytes());(replay/'Bosak.XPath.Core.dll').write_bytes(core)
(replay/'Replay.csproj').write_text(project,encoding='utf-8');(replay/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
build=subprocess.run(['dotnet','build',str(replay/'Replay.csproj'),'-c','Release','--configfile',str(replay/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
(P/'evidence/protocol-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
if build.returncode:raise RuntimeError(build.stdout+build.stderr)
run=subprocess.run(['dotnet',str(replay/'bin/Release/net10.0/Replay.dll')],capture_output=True,text=True,timeout=30)
(P/'evidence/protocol-replay.stderr.txt').write_text(run.stderr,encoding='utf-8')
if run.returncode:raise RuntimeError(run.stdout+run.stderr)
json.loads(run.stdout);(P/'evidence/protocol-replay.json').write_text(run.stdout,encoding='utf-8')
print('Three bounded granularity runs retained, with preflight/cache preparation and raw samples.')
