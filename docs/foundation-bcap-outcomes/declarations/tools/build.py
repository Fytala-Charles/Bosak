"""Compile isolated declaration assemblies and inventory metadata; no proposed bodies run."""
from pathlib import Path
import json,hashlib,subprocess,sys
ROOT=Path(__file__).resolve().parents[4];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);D=P/'proposal'
W=ROOT/'.guard-tmp/bcap-outcome-declarations';W.mkdir(exist_ok=True);R=W/'Proposed';R.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
layout=json.loads((ROOT/'docs/foundation-xslt-source-membership/evidence/INPUTS.json').read_text())['engines']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
props='<PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors><Version>1.42.0</Version></PropertyGroup>'
groups={
 'Bosak.XPath.Core.C45Proposal':(['Provider','OperationLifetime','ExactNumeric'],[],['Bosak.XPath.Core']),
 'Bosak.XPath.Api.C45Proposal':(['QueryFacade'],['Bosak.XPath.Core.C45Proposal'],['Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime']),
 'Bosak.XQuery.C45Proposal':(['XQuery'],['Bosak.XPath.Core.C45Proposal','Bosak.XPath.Api.C45Proposal'],['Bosak.XPath.Core','Bosak.XQuery']),
 'Bosak.Xslt.C45Proposal':(['Xslt'],['Bosak.XPath.Core.C45Proposal','Bosak.XPath.Api.C45Proposal'],['Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.Xslt'])}
engines={Path(v['path']).stem:v for v in layout.values()}
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
for name,(sources,projects,refs) in groups.items():
 folder=R/'src'/name;folder.mkdir(parents=True,exist_ok=True)
 for source in sources:(folder/(source+'.cs')).write_bytes((D/(source+'.cs.txt')).read_bytes())
 items=''.join('<ProjectReference Include="../'+n+'/'+n+'.csproj" />' for n in projects)
 items+=''.join('<Reference Include="'+n+'"><HintPath>'+engines[n]['path']+'</HintPath></Reference>' for n in refs)
 (folder/(name+'.csproj')).write_text('<Project Sdk="Microsoft.NET.Sdk">'+props+'<ItemGroup>'+items+'</ItemGroup></Project>')
for host in ['Bosak.XQuery.C45Proposal','Bosak.Xslt.C45Proposal']:
 run(['dotnet','build',R/'src'/host/(host+'.csproj'),'-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],host+'-build.txt')
inventory=W/'Inventory';inventory.mkdir(exist_ok=True)
source=(ROOT/'docs/foundation-c45-delta136/tools/Inventory.cs.txt').read_text(encoding='utf-8')
source=source.replace('using System.Reflection;','using System.Reflection;\nusing System.Reflection.Metadata;\nusing System.Reflection.PortableExecutable;')
source=source.replace('publicTypes = declarations.Count','references = References(path), publicTypes = declarations.Count')
source=source.replace('static string Hash(string file)', '''static string[] References(string file) {
 using var stream = File.OpenRead(file); using var pe = new PEReader(stream);
 var reader = pe.GetMetadataReader();
 return reader.AssemblyReferences.Select(h => reader.GetString(reader.GetAssemblyReference(h).Name)).Order(StringComparer.Ordinal).ToArray();
}
static string Hash(string file)''')
(P/'tools/Inventory.cs.txt').write_text(source,encoding='utf-8')
(inventory/'Program.cs').write_bytes((P/'tools/Inventory.cs.txt').read_bytes())
(inventory/'Inventory.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
run(['dotnet','build',inventory/'Inventory.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'inventory-build.txt')
for n in range(2):run(['dotnet',inventory/'bin/Release/net10.0/Inventory.dll',R,E/f'api-{n}.json'],f'inventory-{n}.txt')
a=json.loads((E/'api-0.json').read_text());assert a==json.loads((E/'api-1.json').read_text())
assert len(a['assemblies'])==4 and sum(v['publicTypes'] for v in a['assemblies'])==47
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
(E/'INPUTS.json').write_text(json.dumps({'qualifiedEngines':layout,'declarationSources':{f.name:sha(f) for f in D.glob('*.txt')},'dependencyGraph':{k:{'proposalReferences':v[1],'engineReferences':v[2]} for k,v in groups.items()},'publicTypes':47,'proposalBodiesExecuted':False,'productionExportsAdded':0,'inventoryBaseSourceSha256':sha(ROOT/'docs/foundation-c45-delta136/tools/Inventory.cs.txt'),'inventorySourceSha256':sha(P/'tools/Inventory.cs.txt')},indent=2)+'\n')
print('Compiled 47 proposed types in four isolated layer assemblies; repeat metadata identical; engines unchanged.')
