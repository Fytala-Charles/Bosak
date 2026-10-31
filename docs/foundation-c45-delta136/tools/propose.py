"""Compile exact isolated C45 declarations; execute no proposed operation bodies."""
from pathlib import Path
import sys,subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/c45-delta136';R=W/'Proposed';R.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(args,log):
 with process_error_mode(),(E/log).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(log+' failed')
v5=ROOT/'docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt';text=v5.read_text(encoding='utf-8').replace('namespace C13901.Draft;','namespace Bosak.XPath.Core.Schema;').replace('True if original atomic provenance is attached.','True if current type evidence is available, including construction without an original assessment.')
(P/'proposal/Provider.cs.txt').write_text(text,encoding='utf-8')
core=R/'src/Bosak.XPath.Core.C45Proposal';api=R/'src/Bosak.XPath.Api.C45Proposal'
core.mkdir(parents=True,exist_ok=True);api.mkdir(parents=True,exist_ok=True)
for f in ['Provider.cs.txt','OperationLifetime.cs.txt']:(core/f.removesuffix('.txt')).write_bytes((P/'proposal'/f).read_bytes())
(api/'QueryFacade.cs').write_bytes((P/'proposal/QueryFacade.cs.txt').read_bytes())
props='<PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors><Version>1.36.0</Version></PropertyGroup>'
coreDll=ROOT/'src/Bosak.XPath.Core/bin/Release/net10.0/Bosak.XPath.Core.dll'
(core/'Bosak.XPath.Core.C45Proposal.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk">'+props+'<ItemGroup><Reference Include="Bosak.XPath.Core"><HintPath>'+str(coreDll)+'</HintPath></Reference></ItemGroup></Project>',encoding='utf-8')
refs=''.join('<Reference Include="'+name+'"><HintPath>'+str(ROOT/'src'/name/'bin/Release/net10.0'/(name+'.dll'))+'</HintPath></Reference>' for name in ['Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'])
(api/'Bosak.XPath.Api.C45Proposal.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk">'+props+'<ItemGroup><ProjectReference Include="../Bosak.XPath.Core.C45Proposal/Bosak.XPath.Core.C45Proposal.csproj"/>'+refs+'</ItemGroup></Project>',encoding='utf-8')
run(['dotnet','build',api/'Bosak.XPath.Api.C45Proposal.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'proposal-build.txt')
run(['dotnet',W/'Inventory/bin/Release/net10.0/Inventory.dll',R,E/'proposed-api.json'],'proposal-inventory.txt')
inventory=json.loads((E/'proposed-api.json').read_text());assert len(inventory['assemblies'])==2 and sum(a['publicTypes'] for a in inventory['assemblies'])==40
for name,record in json.loads((E/'INPUTS.json').read_text())['engines'].items():assert sha(Path(record['path']).read_bytes())==record['sha256'],name
(E/'PROPOSAL-INPUTS.json').write_text(json.dumps({'v5SourceSha256':sha(v5.read_bytes()),'sources':{f.name:sha(f.read_bytes()) for f in (P/'proposal').glob('*.txt')},'proposedPublicTypes':40,'proposalAssemblies':2,'newExportsInProduction':0,'proposedBodiesExecuted':False,'namespacePlacementProposed':True},indent=2)+'\n',encoding='utf-8')
print('Compiled40 proposed public types in two isolated layer assemblies; no proposed operations executed.')
