"""Create and build an isolated XSLT-only candidate against qualified dependencies."""
from pathlib import Path
import hashlib,json,sys,subprocess,difflib
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=ROOT/'.guard-tmp/xslt-explicit-context';W.mkdir(exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
layout=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text())['engines']
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
source=ROOT/'src/Bosak.Xslt';dest=W/'src/Bosak.Xslt';dest.mkdir(parents=True,exist_ok=True)
pins={}
for f in source.rglob('*'):
 if not f.is_file() or 'bin' in f.relative_to(source).parts or 'obj' in f.relative_to(source).parts:continue
 rel=f.relative_to(source);target=dest/rel;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(f.read_bytes());pins[rel.as_posix()]=sha(f)
def edit(name,transform):
 f=dest/name;original=f.read_text(encoding='utf-8-sig');changed=transform(original);assert changed!=original
 f.write_text(changed,encoding='utf-8')
 (E/(Path(name).stem+'.patch')).write_text(''.join(difflib.unified_diff(original.splitlines(True),changed.splitlines(True),fromfile='a/src/Bosak.Xslt/'+name,tofile='b/src/Bosak.Xslt/'+name)),encoding='utf-8')
def once(s,old,new):assert s.count(old)==1,old[:100];return s.replace(old,new,1)
def engine(s):
 sig='public XdmValue Transform(IXdmNode? source, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null, XdmValue? initialMatchSelection = null, bool captureResultDocuments = false, bool rawTransformResult = false, IXdmNode? globalContextItem = null)'
 s=once(s,sig+'\n    {',sig+'\n        => TransformCore(source, initialTemplate, initialMode, rawResult, baseOutputUri, initialMatchSelection, captureResultDocuments, rawTransformResult, globalContextItem, true);\n\n    internal XdmValue TransformCore(IXdmNode? source, string? initialTemplate, string? initialMode, bool rawResult, string? baseOutputUri, XdmValue? initialMatchSelection, bool captureResultDocuments, bool rawTransformResult, IXdmNode? globalContextItem, bool useSourceAsGlobalContext)\n    {')
 s=once(s,'_stylesheet.GlobalContextItemUse == "required" && source == null','_stylesheet.GlobalContextItemUse == "required" && (useSourceAsGlobalContext ? source == null : globalContextItem == null)')
 s=once(s,'var effectiveGlobalContextItem = globalContextItem ?? source;','var effectiveGlobalContextItem = useSourceAsGlobalContext ? globalContextItem ?? source : globalContextItem;\n        if (!useSourceAsGlobalContext)\n            _context.WithFocus(XdmValue.Undefined, 0, 0);')
 s=once(s,'if (entryRule.CompiledMatch != null && source != null)','if (useSourceAsGlobalContext && entryRule.CompiledMatch != null && source != null)')
 s=once(s,'var initialContextItem = source != null ? XdmValue.FromNode(source) : XdmValue.Undefined;','var namedContext = useSourceAsGlobalContext ? source : effectiveGlobalContextItem;\n                    var initialContextItem = namedContext != null ? XdmValue.FromNode(namedContext) : XdmValue.Undefined;')
 return '// Generated isolated context-policy candidate; original license/history retained below.\n'+s
def executable(s):
 marker='    internal Stylesheet.OutputProperties? LastResultDocumentProperties { get; private set; }'
 bridge='''
    // Isolated opt-in bridge: null global input means absent, never source fallback.
    internal XdmValue TransformWithExplicitContext(IXdmNode? matchInput, IXdmNode? globalInput, string? initialTemplate, EvaluationContext? context)
    {
        if (matchInput != null && matchInput.NodeKind != XdmNodeKind.Document)
            throw new NotSupportedException("This isolated bridge admits only document-root initial match inputs.");
        return RunWithStack(() => new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError)
            .TransformCore(matchInput, initialTemplate, null, true, null, null, true, true, globalInput, false), DefaultTransformStackSize);
    }
'''
 return '// Generated isolated context-policy candidate; original license/history retained below.\n'+once(s,marker,marker+'\n'+bridge)
edit('Runtime/TransformEngine.cs',engine);edit('Api/XsltExecutable.cs',executable)
refs=''.join('<Reference Include="'+Path(v['path']).stem+'"><HintPath>'+v['path']+'</HintPath></Reference>' for v in layout.values() if Path(v['path']).stem!='Bosak.Xslt')
(dest/'Bosak.Xslt.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><LangVersion>12</LangVersion><AssemblyName>Bosak.Xslt</AssemblyName><Version>1.36.0</Version><GenerateDocumentationFile>true</GenerateDocumentationFile></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>')
(W/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>')
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=180)
 if r.returncode:raise RuntimeError(name+' failed; caught output retained')
run(['dotnet','build',dest/'Bosak.Xslt.csproj','-c','Release','--configfile',W/'NuGet.Config','-p:NuGetAudit=false'],'candidate-build.txt')
candidate=dest/'bin/Release/net10.0/Bosak.Xslt.dll'
for v in layout.values():assert sha(Path(v['path']))==v['sha256']
(E/'INPUTS.json').write_text(json.dumps({'qualifiedEngines':layout,'sourceFiles':pins,'candidatePath':str(candidate),'candidateSha256':sha(candidate),'patchedFiles':{n:sha(dest/n) for n in ['Runtime/TransformEngine.cs','Api/XsltExecutable.cs']},'canonicalChanges':False,'productionAccepted':False},indent=2)+'\n')
print('Built isolated XSLT candidate; canonical source and qualified DLLs untouched.')
