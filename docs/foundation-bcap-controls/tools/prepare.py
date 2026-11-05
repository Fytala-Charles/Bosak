"""Prepare the next isolated candidate; retain the original cohort unchanged."""
from pathlib import Path
import shutil,subprocess,sys,difflib,hashlib,json,re
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=R/'.guard-tmp/bcap-controls-v3';S=W/'src';E=P/'evidence';E.mkdir(exist_ok=True)
if W.exists():raise RuntimeError('Candidate already exists; preserve it')
old=R/'.guard-tmp/bcap-runtime-v4'
old_inputs=json.loads((R/'docs/foundation-bcap-runtime/evidence/INPUTS.json').read_text())
for name,digest in old_inputs['candidateSources'].items():
 assert hashlib.sha256((old/name).read_bytes()).hexdigest()==digest,name
shutil.copytree(old/'src',S,ignore=shutil.ignore_patterns('bin','obj'))
for n in ['license.md','README.md','NuGet.Config','global.json']:
 if (old/n).exists():shutil.copyfile(old/n,W/n)
def edit(name,fn):
 p=S/name;s=p.read_text(encoding='utf-8');v=fn(s);assert v!=s,name
 p.write_bytes(v.encode())
def gate(s):
 s=s.replace('private readonly CancellationToken _cancellationToken;', 'private readonly ExecutionGate? _parent, _local;\n    private readonly CancellationToken _cancellationToken;')
 at=s.rfind('    /// <summary>',0,s.index('    internal static ExecutionGate Create('))
 s=s[:at]+'''    internal static ExecutionGate Compose(ExecutionGate? parent, ExecutionGate local)
        => parent is null ? local : new ExecutionGate(parent, local);
    private ExecutionGate(ExecutionGate parent, ExecutionGate local) : this(default, 0, long.MaxValue)
    { _parent = parent; _local = local; }
'''+s[at:]
 s=s.replace('internal void Checkpoint()\n    {','internal void Checkpoint()\n    {\n        if (_parent is not null) { _parent.Checkpoint(); _local!.Checkpoint(); return; }')
 s=s.replace('internal void ObserveControls()\n    {','internal void ObserveControls()\n    {\n        if (_parent is not null) { _parent.ObserveControls(); _local!.ObserveControls(); return; }')
 return s
edit('Bosak.XPath.Runtime/ExecutionGate.cs',gate)
edit('Bosak.XPath.Runtime/Vm/EvaluationContext.cs',lambda s:s.replace('SchemaEvidenceKernel.Snapshot? snapshot = null)\n    {\n        ArgumentNullException.ThrowIfNull(ownsNode);','SchemaEvidenceKernel.Snapshot? snapshot = null, bool allowExecutionGate = false)\n    {\n        ArgumentNullException.ThrowIfNull(ownsNode);').replace('|| ExecutionGate is not null || DocumentLoader','|| (!allowExecutionGate && ExecutionGate is not null) || DocumentLoader').replace('DefaultCollation, snapshot);','DefaultCollation, snapshot, allowExecutionGate ? ExecutionGate : null);'))
edit('Bosak.XPath.Runtime/Vm/SchemaOperationInputs.cs',lambda s:s.replace('internal SchemaEvidenceKernel.Snapshot? Snapshot { get; }','internal SchemaEvidenceKernel.Snapshot? Snapshot { get; }\n    internal ExecutionGate? Gate { get; }').replace('_collation, null) { OperationIdentity','_collation, null, Gate) { OperationIdentity').replace('SchemaEvidenceKernel.Snapshot? snapshot)\n','SchemaEvidenceKernel.Snapshot? snapshot, ExecutionGate? gate = null)\n').replace('_focus = focus;', 'Gate = gate; _focus = focus;').replace('BaseUri = _baseUri,','ExecutionGate = Gate, BaseUri = _baseUri,'))
edit('Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs',lambda s:s.replace('Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces)','Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, bool allowExecutionGate = false)').replace('caller.CaptureSchemaInputs(owns, maxBindings, maxNamespaces).ForOperation','caller.CaptureSchemaInputs(owns, maxBindings, maxNamespaces, allowExecutionGate: allowExecutionGate).ForOperation'))
edit('Bosak.Xslt/Api/SchemaCaptureCandidate.cs',lambda s:s.replace('using Bosak.XPath.Runtime.Vm;','using Bosak.XPath.Runtime.Vm;\nusing Bosak.XPath.Runtime;').replace('var result = executable.CaptureExplicitCandidate', 'context.ExecutionGate = ExecutionGate.Compose(context.ExecutionGate, ExecutionGate.Create(linked.Token, copied.TimeLimit, copied.MaxEngineOperations));\n        var result = executable.CaptureExplicitCandidate'))
def observer(s):
 at=s.index('    internal XsltCaptureResult CaptureExplicitCandidate(')
 s=s[:at]+'    private static Action<long>? CandidateProductionObserver;\n    internal static void SetCandidateProductionObserver(Action<long>? observer) => CandidateProductionObserver = observer;\n'+s[at:]
 at=s.index('internal XsltCaptureResult CaptureExplicitCandidate(')
 s=s[:at]+s[at:].replace('new XsltCaptureSession(options, _messageListener, gate);','new XsltCaptureSession(options, _messageListener, gate, CandidateProductionObserver);',1)
 return s
edit('Bosak.Xslt/Api/XsltExecutable.cs',observer)
def preserve_context(s):
 signature='    internal SchemaOperationInputs CaptureSchemaInputs(Func<IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, SchemaEvidenceKernel.Snapshot? snapshot = null, bool allowExecutionGate = false)'
 assert signature in s
 return s.replace(signature,'''    internal SchemaOperationInputs CaptureSchemaInputs(Func<IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, SchemaEvidenceKernel.Snapshot? snapshot = null)
        => CaptureSchemaInputs(ownsNode, maxBindings, maxNamespaces, snapshot, false);
    internal SchemaOperationInputs CaptureSchemaInputs(Func<IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, SchemaEvidenceKernel.Snapshot? snapshot, bool allowExecutionGate)''')
edit('Bosak.XPath.Runtime/Vm/EvaluationContext.cs',preserve_context)
def preserve_operation(s):
 signature='        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller,\n            Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, bool allowExecutionGate = false)'
 assert signature in s
 return s.replace(signature,'''        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller,
            Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces)
            => CaptureInputs(caller, ownsNode, maxBindings, maxNamespaces, false);
        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller,
            Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces, bool allowExecutionGate)''').replace('maxNamespaces, allowExecutionGate: allowExecutionGate)', 'maxNamespaces, null, allowExecutionGate)')
edit('Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs',preserve_operation)
def preserve_constructor(s):
 at=s.index('    internal SchemaOperationInputs(XdmValue focus,')
 wrapper='''    internal SchemaOperationInputs(XdmValue focus, int position, int size,
        KeyValuePair<(string LocalName, string NamespaceUri), XdmValue>[] variables,
        KeyValuePair<string, string>[] namespaces, string? baseUri,
        string? defaultNamespace, string? definingNamespace, string collation, SchemaEvidenceKernel.Snapshot? snapshot)
        : this(focus, position, size, variables, namespaces, baseUri, defaultNamespace, definingNamespace, collation, snapshot, null) { }
'''
 return (s[:at]+wrapper+s[at:]).replace('ExecutionGate? gate = null)', 'ExecutionGate? gate)')
edit('Bosak.XPath.Runtime/Vm/SchemaOperationInputs.cs',preserve_constructor)
delta={}
for p in S.rglob('*.cs'):
 n=p.relative_to(S).as_posix();baseline=R/'src'/n
 if not baseline.exists():
  (P/'tools/SchemaCaptureCandidate.cs.txt').write_bytes(p.read_bytes());continue
 before=baseline.read_text(encoding='utf-8');after=p.read_text(encoding='utf-8')
 if before==after:continue
 first=after.index('using ') if 'using ' in after else after.index('namespace ')
 at=after.rfind('// ===========================================================================================================================================================',0,first);versions=[tuple(map(int,v)) for v in re.findall(r'Charles Korthout\s*\|\s*(\d+)\.(\d+)',after)];major,minor=max(versions)
 after=after[:at]+f'//                      | Charles Korthout | {major}.{minor+1} | 01-11-2026 | Isolated gate composition and serialization fault controls. |\n'+after[at:];p.write_bytes(after.encode())
 out=P/'patches'/n;out.parent.mkdir(parents=True,exist_ok=True);out.with_suffix('.patch').write_bytes(''.join(difflib.unified_diff(before.splitlines(True),after.splitlines(True),fromfile='a/src/'+n,tofile='b/src/'+n)).encode())
 delta[n]={'originalSha256':hashlib.sha256(baseline.read_bytes()).hexdigest(),'candidateSha256':hashlib.sha256(p.read_bytes()).hexdigest()}
(E/'SOURCE-DELTA.json').write_text(json.dumps(delta,indent=2)+'\n')
for n in ['run.py','regressions.py','seal-inputs.py','verify.py']:
 s=(R/'docs/foundation-bcap-runtime/tools'/n).read_text().replace('bcap-runtime-v4','bcap-controls-v3')
 if not (P/'tools'/n).exists():(P/'tools'/n).write_bytes(s.encode())
if not (P/'tools/Probe.cs.txt').exists():
 s=(R/'docs/foundation-bcap-runtime/tools/Probe.cs.txt').read_text();(P/'tools/Probe.cs.txt').write_bytes(s.encode())
print('Prepared isolated gate-composition candidate with serializer fault seam.')
