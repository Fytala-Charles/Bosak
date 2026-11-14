import zipfile,json,hashlib
from pathlib import Path
p=Path('D:/Development/Bosak/.guard-tmp/scalar-materialization-20261114');r=Path('D:/Development/Bosak/.worktrees/Bosak.ScalarProbe')
runtime=p/'runtime';runtime.mkdir(exist_ok=True)
review=json.loads((p/'REVIEW.json').read_text())
with zipfile.ZipFile(r/'docs/streaming-wrapper-cache/REFERENCE.zip') as z:
 for name,sha in review['engineHashes'].items():
  data=z.read('candidate/'+name);assert hashlib.sha256(data).hexdigest()==sha;(runtime/name).write_bytes(data)
refs=''.join(f'<Reference Include="{Path(n).stem}"><HintPath>runtime/{n}</HintPath><Private>true</Private></Reference>' for n in review['engineHashes'])
(p/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="Probe.cs"/>'+refs+'</ItemGroup></Project>')
