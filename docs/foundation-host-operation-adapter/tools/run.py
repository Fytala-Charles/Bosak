"""Run bounded all-host adapter observations against qualified binaries."""
from pathlib import Path
import sys,subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/host-operation-adapter';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
layout=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
prior=ROOT/'docs/foundation-c45-domains-hosts/tools/Probe.cs.txt'
typed='sealed class TypedNode'+prior.read_text(encoding='utf-8').split('sealed class TypedNode',1)[1]
(P/'tools/TypedNode.cs.txt').write_text('// Generated reused test double; COPYRIGHT: Fytala; SPDX-License-Identifier: Apache-2.0\nusing Bosak.XPath.Core.Xdm;\n'+typed,encoding='utf-8')
for name in ['Adapter','TypedNode']:(W/(name+'.cs')).write_bytes((P/'tools'/(name+'.cs.txt')).read_bytes())
(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
refs=''.join('<Reference Include="'+Path(v['path']).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for v in layout.values())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'build.txt')
for n in range(2):run(['dotnet',W/'bin/Release/net10.0/Probe.dll'],f'run-{n}.json')
a=json.loads((E/'run-0.json').read_text());assert a==json.loads((E/'run-1.json').read_text()) and all(x['passed'] for x in a['results'])
engines={Path(v['path']).stem:v for v in layout.values()}
for binding in a['bindings']:assert binding['sha256']==engines[binding['name']]['sha256']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
sources=['src/Bosak.Xslt/Api/XsltExecutable.cs','src/Bosak.Xslt/Runtime/TransformEngine.cs','src/Bosak.XQuery/Api/XQueryCompiler.cs','src/Bosak.XPath.Api/CompileOptions.cs']
(E/'INPUTS.json').write_text(json.dumps({'qualifiedEngines':layout,'sources':{n:sha(ROOT/n) for n in sources},'adapterSources':{p.name:sha(p) for p in (P/'tools').glob('*.cs.txt')},'typedNodeOriginSha256':sha(prior),'engineChanges':False,'authenticatedIntegration':False,'repeatIdentical':True},indent=2)+'\n')
print('Verified '+str(a['checks'])+' adapter/host checks twice; actual loaded bindings and engine pins unchanged.')
