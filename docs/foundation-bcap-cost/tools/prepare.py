"""Prepare a separate measurement host referencing immutable qualified engine DLLs."""
from pathlib import Path
import json,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=R/'.guard-tmp/bcap-cost';W.mkdir(exist_ok=True)
if (P/'MANIFEST.json').exists():raise RuntimeError('Sealed cohort; preserve it and select a new dossier/host path for new measurements')
i=json.loads((R/'docs/foundation-bcap-profile/evidence/QUALIFICATION-INPUTS.json').read_text())
for n,v in i['engines'].items():assert hashlib.sha256(Path(v['path']).read_bytes()).hexdigest()==v['sha256'],n
s=(R/'docs/foundation-bcap-profile/tools/Probe.cs.txt').read_text();s=s.replace('try { await Probe.Run(); }','try { await Probe.Measure(args); }')
at=s.index('\nsealed class Provider:');end=s.rfind('\n}',0,at);s=s[:end]+(P/'tools/Measurements.cs.txt').read_text()+s[end:]
(P/'tools/MeasurementHost.cs.txt').write_bytes(s.encode());(W/'Program.cs').write_bytes(s.encode())
refs=''.join('<Reference Include="'+n+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in i['engines'].items())
(W/'Cost.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
(E/'ENGINE-INPUTS.json').write_text(json.dumps({'engines':i['engines'],'qualificationManifestSha256':hashlib.sha256((R/'docs/foundation-bcap-profile/MANIFEST.json').read_bytes()).hexdigest(),'engineRebuild':False,'measurementSourceSha256':hashlib.sha256((P/'tools/MeasurementHost.cs.txt').read_bytes()).hexdigest()},indent=2)+'\n')
print('Prepared separate measurement host; qualified engine DLLs verified and left unchanged.')
