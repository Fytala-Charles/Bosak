from pathlib import Path
r=Path.cwd();p=r/'.guard-tmp/binary-identity-probe';p.mkdir(exist_ok=True)
baseline=r/'.guard-tmp/braid-baseline/src/Bosak.Braid.Desktop/bin/Release/net10.0'
refs=''.join(f'<Reference Include="{f.stem}"><HintPath>{f.as_posix()}</HintPath><Private>true</Private></Reference>' for f in baseline.glob('Bosak*.dll'))
(p/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
header=(r/'.guard-tmp/xmldb-baseline/probe/Program.cs').read_text().split('using System.Text;')[0].replace('Probes persistent XML provider compatibility and snapshot lifetime in isolation.','Reads unchanged Braid public engine identities during isolated DLL replacement diagnostics.')
(p/'Program.cs').write_text(header+'''using Bosak.Braid.Infrastructure;
using System.Text.Json;
var inspector = new BosakSourceInspector();
Console.WriteLine(JsonSerializer.Serialize(new { inspectorIdentity=inspector.EngineIdentity, capabilityEngineVersion=inspector.Capabilities.EngineVersion, previewIdentity=new BosakPreviewExecutor().EngineIdentity }));
''',encoding='utf-8',newline='\n')
print('Independent diagnostic prepared against baseline public Braid contracts; no consumer rebuild.')
