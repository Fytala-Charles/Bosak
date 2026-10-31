"""Prepare one isolated context-routing prototype and preserve original case/resource metadata."""
from pathlib import Path
import difflib
import hashlib
import json
import shutil
import xml.etree.ElementTree as X
from xml.sax.saxutils import escape

E = Path(__file__).resolve().parent
P = E.parent
R = E.parents[3]
TARGET = R/'.guard-tmp/k04-construction-admission-harness'
DRIVER = R/'.guard-tmp/k04-construction-admission-driver'
TARGET.mkdir(exist_ok=True)
DRIVER.mkdir(exist_ok=True)
project = R/'tests/Bosak.XPath.Conformance'
old = (project/'TestExecutor.cs').read_text(encoding='utf-8')
pattern = r'\bdeclare\s+(construction)\b'
assert old.count(pattern)==1
start = old.index('        // The prolog gate matches')
end = old.index('        return true;', start)
new = old[:start] + old[end:]
start = new.index('    // Prolog forms the XQuery parser does NOT support')
end = new.index('    /// <summary>',start)
new = new[:start] + new[end:]
line = '//                      | Charles Korthout | 0.30 '
lines = new.splitlines(True)
index = next(i for i,l in enumerate(lines) if l.startswith(line))+1
lines.insert(index,'//                      | Charles Korthout | 0.31  | 28-10-2026     | Isolated C35 construction review; normal source and policy remain unchanged.          |\n')
new = ''.join(lines)
(E/'prototype-executor.cs.txt').write_text(new,encoding='utf-8')
(P/'PROTOTYPE-ROUTING.patch').write_bytes(''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile='a/tests/Bosak.XPath.Conformance/TestExecutor.cs',tofile='b/tests/Bosak.XPath.Conformance/TestExecutor.cs')).encode('utf-8'))
identities = {}
for source in project.glob('*.cs'):
    data = new.encode('utf-8') if source.name=='TestExecutor.cs' else source.read_bytes()
    (TARGET/source.name).write_bytes(data)
    identities[(TARGET/source.name).relative_to(R).as_posix()] = {'source':source.relative_to(R).as_posix(),'changed':source.name=='TestExecutor.cs','sha256':hashlib.sha256(data).hexdigest()}
(E/'copied-source-identities.json').write_text(json.dumps(identities,indent=2)+'\n',encoding='utf-8')
directory = project/'bin/Release/net10.0'
refs = ''.join('<Reference Include="'+escape(p.stem)+'"><HintPath>'+escape(str(p))+'</HintPath></Reference>' for p in sorted(directory.glob('*.dll')) if p.stem!='Bosak.XPath.Conformance')
csproj = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Conformance</AssemblyName><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><LangVersion>preview</LangVersion></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
(TARGET/'Probe.csproj').write_text(csproj,encoding='utf-8')
(E/'prototype.csproj.txt').write_text(csproj,encoding='utf-8')

# Use the real parser/environment/executor via reflection, with the actual normal guarded-loader setting.
source = (R/'docs/conformance/k04-main-context/evidence/executor.cs.txt').read_text(encoding='utf-8')
source = source.replace('string Dir(string suite)=>root+', 'string Dir(string suite)=>suite=="qt3" && Environment.GetEnvironmentVariable("C35_QT3_HARNESS") is {Length:>0} custom ? custom : root+',1)
point = source.index(' if(args[0]=="public-xquery"){')
source = source[:point]+''' if(args[0]=="resources"){
  object? Field(string field)=>T(a,"TestEnvironment").GetProperty(field,flags)!.GetValue(env);
  Console.WriteLine(JsonSerializer.Serialize(new{row.Suite,row.Name,Sources=Field("Sources"),Schemas=Field("Schemas"),Parameters=Field("Parameters"),Namespaces=Field("Namespaces"),ContextItemSelect=Field("ContextItemSelect"),BaseUri=Field("BaseUri"),Modules=T(a,"TestCase").GetProperty("Modules",flags)!.GetValue(test)}));return;
 }
'''+source[point:]
oldcall='var result=Invoke(et,"Execute",Activator.CreateInstance(et,true),test,env)!;'
newcall='var executor=Activator.CreateInstance(et,true)!; et.GetProperty("UseQueryOwnedSchemasForDiagnostics",flags)!.SetValue(executor,true); var result=Invoke(et,"Execute",executor,test,env)!;'
assert oldcall in source
source = source.replace(oldcall,newcall,1)
(DRIVER/'Program.cs').write_text(source,encoding='utf-8')
(E/'executor.cs.txt').write_text(source,encoding='utf-8')
shutil.copyfile(R/'docs/conformance/k04-main-context/evidence/executor.csproj.txt',DRIVER/'Probe.csproj')
shutil.copyfile(DRIVER/'Probe.csproj',E/'executor.csproj.txt')
print('Prepared isolated one-rule prototype and actual-executor driver; normal source untouched.')
