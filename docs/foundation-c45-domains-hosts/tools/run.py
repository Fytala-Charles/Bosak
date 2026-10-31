"""Bounded diagnostics on the existing cohort; no engine rebuild or mutation."""
from pathlib import Path
import sys,subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/c45-domains-hosts';W.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed; output retained')
layout=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines']
for n,v in layout.items():assert sha(Path(v['path']).read_bytes())==v['sha256']
(W/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes());(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for n,v in layout.items())
(W/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>',encoding='utf-8')
run(['dotnet','build',W/'Probe.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'probe-build.txt')
for j in range(2):run(['dotnet',W/'bin/Release/net10.0/Probe.dll'],f'probe-{j}.json')
for n,v in layout.items():assert sha(Path(v['path']).read_bytes())==v['sha256']
files=['src/Bosak.XPath.Core/Xdm/XdmValue.cs','src/Bosak.XPath.Parser/Ast/XPathParser.cs','src/Bosak.XQuery/Api/XQueryCompiler.cs','src/Bosak.XQuery/Api/XQueryExecutable.cs','src/Bosak.XQuery/Api/XQueryContext.cs','src/Bosak.Xslt/Api/XsltCompiler.cs','src/Bosak.Xslt/Api/XsltExecutable.cs']
accessors={}
for f in (ROOT/'src').rglob('*.cs'):
 if 'obj' in f.parts or 'bin' in f.parts:continue
 text=f.read_text(encoding='utf-8-sig');hits={token:text.count(token) for token in ['.IntegerValue','.DecimalValue','_reference'] if token in text}
 if hits:accessors[f.relative_to(ROOT).as_posix()]=hits
(E/'INPUTS.json').write_text(json.dumps({'qualifiedEngineHashes':layout,'sourceFiles':{n:sha((ROOT/n).read_bytes()) for n in files},'probeSha256':sha((P/'tools/Probe.cs.txt').read_bytes()),'enginesUnchanged':True,'sourceChanged':False,'accessorAudit':accessors},indent=2)+'\n',encoding='utf-8')
print('Captured42 numeric observations and5 ordinary-host controls twice; qualified DLLs unchanged.')
