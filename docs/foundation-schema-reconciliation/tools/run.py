"""Build pinned Apache Schema source in isolation; leave owner and Core engine outputs untouched."""
from pathlib import Path
import json,hashlib,subprocess,shutil,sys
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=R/'.guard-tmp/schema-reconciliation-v2';N=R/'.worktrees/Bosak.Schema.Assessment';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if W.exists():raise RuntimeError('Existing run retained; use a fresh cohort')
assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=N,text=True).strip()=='c3624998eca8c83018e121fa84af611f834a0501'
assert subprocess.check_output(['git','status','--porcelain'],cwd=N,text=True)==''
W.mkdir();S=W/'Schema';shutil.copytree(N/'src/Bosak.Schema',S,ignore=shutil.ignore_patterns('bin','obj'))
sources={str(p.relative_to(N)).replace('\\','/'):sha(p) for p in (N/'src/Bosak.Schema').rglob('*') if p.is_file() and not {'bin','obj'}&set(p.relative_to(N).parts)}
deps={p.stem:{'path':str(p),'sha256':sha(p)} for p in (N/'src/Bosak.Schema/bin/Release/net10.0').glob('*.dll') if p.stem!='Bosak.Schema'}
refs=''.join('<Reference Include="'+n+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in deps.items());props='<PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>'
(W/'Native.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk">'+props+'<PropertyGroup><AssemblyName>Bosak.Schema</AssemblyName><EnableDefaultCompileItems>false</EnableDefaultCompileItems><GenerateDocumentationFile>true</GenerateDocumentationFile></PropertyGroup><ItemGroup><Compile Include="Schema/**/*.cs" />'+refs+'</ItemGroup></Project>')
H=W/'Host';H.mkdir();(H/'Program.cs').write_bytes((P/'tools/NativeFacts.cs.txt').read_bytes());(H/'Facts.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk">'+props+'<PropertyGroup><OutputType>Exe</OutputType></PropertyGroup><ItemGroup><ProjectReference Include="../Native.csproj" />'+refs+'</ItemGroup></Project>')
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'));from run_caller import process_error_mode
def run(args,n):
 with process_error_mode(),(E/n).open('w',encoding='utf-8') as log:r=subprocess.run([str(a) for a in args],cwd=W,stdout=log,stderr=subprocess.STDOUT,timeout=120)
 assert r.returncode==0,n
run(['dotnet','build',H/'Facts.csproj','-c','Release','-p:NuGetAudit=false','-v:q'],'native-build.txt')
for k in range(2):run(['dotnet',H/'bin/Release/net10.0/Facts.dll'],f'native-facts-{k}.json')
a=json.loads((E/'native-facts-0.json').read_text());b=json.loads((E/'native-facts-1.json').read_text());assert a==b and a['passed'] and a['checks']==14
observed={p.stem:sha(p) for p in (H/'bin/Release/net10.0').glob('Bosak.*.dll')}
for binding in a['bindings']:assert observed[binding['name']]==binding['sha256']
for n,v in deps.items():assert sha(Path(v['path']))==v['sha256']
for n,h in sources.items():assert sha(N/n)==h,n
assert subprocess.check_output(['git','status','--porcelain'],cwd=N,text=True)==''
(E/'INPUTS.json').write_bytes((json.dumps({'date':'2026-11-02','nativeRevision':'c3624998eca8c83018e121fa84af611f834a0501','nativeSources':sources,'historicalDependencies':deps,'observedNativeAssemblySha256':observed['Bosak.Schema'],'probeSourceSha256':sha(P/'tools/NativeFacts.cs.txt'),'ownerWorktreeUnchanged':True,'productionChanged':False,'bcapAdapterImplemented':False,'nativeOwnerAcceptance':False},indent=2)+'\n').encode())
print('Fourteen existing native-source facts passed twice; owner worktree and production engine untouched.')
