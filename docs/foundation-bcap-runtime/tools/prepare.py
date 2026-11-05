"""Prepare a persistent isolated source candidate; production files remain unchanged."""
from pathlib import Path
import shutil,difflib,json,hashlib,re
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=R/'.guard-tmp/bcap-runtime-v4';S=W/'src';E=P/'evidence';E.mkdir(exist_ok=True)
if S.exists():raise RuntimeError('Existing candidate retained; refuse overwrite')
shutil.copytree(R/'src',S,ignore=shutil.ignore_patterns('bin','obj'))
for n in ['license.md','README.md','NuGet.Config','global.json']:
 if (R/n).exists():shutil.copyfile(R/n,W/n)
changes={}
def change(name,edit):
 p=S/name;before=p.read_text(encoding='utf-8');after=edit(before);assert after!=before
 at=after.rfind('// ===========================================================================================================================================================',0,after.index('using '));assert at>0
 versions=[tuple(map(int,v)) for v in re.findall(r'Charles Korthout\s*\|\s*(\d+)\.(\d+)',before)];major,minor=max(versions)
 history=f'//                      | Charles Korthout | {major}.{minor+1} | 01-11-2026 | Isolated BCAP-01 capture feasibility; not adopted. |\n'
 after=after[:at]+history+after[at:];p.write_bytes(after.encode())
 changes[name]={'originalSha256':hashlib.sha256((R/'src'/name).read_bytes()).hexdigest(),'candidateSha256':hashlib.sha256(p.read_bytes()).hexdigest()}
 patch=''.join(difflib.unified_diff(before.splitlines(True),after.splitlines(True),fromfile='a/src/'+name,tofile='b/src/'+name))
 out=P/'patches'/name;out.parent.mkdir(parents=True,exist_ok=True);out.with_suffix('.patch').write_bytes(patch.encode())
def operation(s):
 return s.replace('internal void ValidateAccess() => Check();','''internal void ValidateAccess() => Check();
        internal CancellationToken RequireOwner(SchemaQuerySession owner)
        {
            Check();
            if (!ReferenceEquals(_owner, owner)) throw new ArgumentException("Foreign executable operation.");
            return _cancellationToken;
        }''')
change('Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs',operation)
def executable(s):
 start=s.index('    public XsltCaptureResult CaptureTransform(IXdmNode? source,')
 end=s.index('\n    /// <summary>',start)
 method=s[start:end].replace('public XsltCaptureResult CaptureTransform(IXdmNode? source, XsltCaptureOptions? options = null)','internal XsltCaptureResult CaptureExplicitCandidate(IXdmNode? source, IXdmNode? global, XsltCaptureOptions options)')
 method=method.replace('var result = engine.Transform(source, options.InitialTemplate, options.InitialMode,\n                    rawResult: false, baseOutputUri: options.BaseOutputUri, captureResultDocuments: true);','''if (source is not null && source.NodeKind != XdmNodeKind.Document)
                    throw new NotSupportedException("Document-root match only.");
                var result = engine.TransformCore(source, options.InitialTemplate, options.InitialMode,
                    false, options.BaseOutputUri, null, true, false, global, false);''')
 assert 'engine.TransformCore' in method
 return s[:end]+'\n'+method+s[end:]
change('Bosak.Xslt/Api/XsltExecutable.cs',executable)
def factory(s):
 start=s.index('    internal static bool TryCompileXslt(');end=s.index('\n    private static bool PlainStatic',start)
 signature='Func<SchemaQuerySession.Operation, SchemaOperationInputs, IXdmNode?, IXdmNode?, string?, string?, string?, Bosak.Xslt.Capture.XsltCaptureOptions, (Bosak.Xslt.Capture.XsltCaptureResult Capture, IReadOnlyList<Bosak.XPath.Runtime.Resources.ControlledResourceReceipt> Receipts)>?'
 method=s[start:end].replace('TryCompileXslt(', 'TryCompileCaptureCandidate(').replace('out SchemaQuerySession? session, out string? unsupported,','out SchemaQuerySession? session, out '+signature+' capture, out string? unsupported,')
 method=method.replace('session = null;', 'session = null; capture = null;')
 method=method.replace('compositeAdmission: SchemaHostExecution.BindXsltAdmission(match, global)); return true;', 'compositeAdmission: SchemaHostExecution.BindXsltAdmission(match, global));\n        capture = new SchemaCaptureCandidate(session, executable).Run; return true;')
 assert 'new SchemaCaptureCandidate' in method
 return s[:end]+'\n'+method+s[end:]
change('Bosak.Xslt/Api/SchemaHostSessionCompilation.cs',factory)
f=P/'tools/SchemaCaptureCandidate.cs.txt';dest=S/'Bosak.Xslt/Api/SchemaCaptureCandidate.cs';dest.write_bytes(f.read_bytes());changes['Bosak.Xslt/Api/SchemaCaptureCandidate.cs']={'originalSha256':None,'candidateSha256':hashlib.sha256(dest.read_bytes()).hexdigest()}
(E/'SOURCE-DELTA.json').write_text(json.dumps(changes,indent=2)+'\n')
print('Prepared isolated candidate with exact source delta; production unchanged.')
