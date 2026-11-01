"""Prepare the next isolated candidate without modifying prior cohorts or production."""
from pathlib import Path
import shutil,json,hashlib,difflib,re
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=R/'.guard-tmp/bcap-outcomes-v3';S=W/'src';OLD=R/'.guard-tmp/bcap-controls-v3'
if W.exists():raise RuntimeError('Existing candidate retained; refuse overwrite')
i=json.loads((R/'docs/foundation-bcap-controls/evidence/INPUTS.json').read_text())
for name,h in i['candidateSources'].items():assert hashlib.sha256((OLD/name).read_bytes()).hexdigest()==h,name
shutil.copytree(OLD/'src',S,ignore=shutil.ignore_patterns('bin','obj'))
for n in ['license.md','README.md','NuGet.Config','global.json']:
 if (OLD/n).exists():shutil.copyfile(OLD/n,W/n)
def edit(n,fn):
 p=S/n;s=p.read_text(encoding='utf-8');a=fn(s);assert a!=s,n;p.write_bytes(a.encode())
def leases(s):
 at=s.index('    private static void Release(')
 return s[:at]+'''    internal IReadOnlyList<Exception> CompleteWithErrors()
    {
        var leases = Interlocked.Exchange(ref _leases, null);
        if (leases is null) return Array.Empty<Exception>();
        var errors = new List<Exception>();
        for (int i = leases.Length - 1; i >= 0; i--)
            try { leases[i].Dispose(); } catch (Exception error) { errors.Add(error); }
        return Array.AsReadOnly(errors.ToArray());
    }
'''+s[at:]
edit('Bosak.XPath.Runtime/Vm/SchemaOperationLeaseSet.cs',leases)
def session(s):
 at=s.index('    /// <summary>Closes admissions')
 method='''    internal (bool HasResult, T? Result, Exception? AdmissionError, Exception? WorkError, IReadOnlyList<Exception> CleanupErrors) RunWithOutcome<T>(
        SchemaQueryEnvironment environment, ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots, Func<Operation, T> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        Operation operation;
        try { operation = OpenOperation(environment, snapshots, cancellation); }
        catch (Exception error) { return (false, default, error, null, Array.Empty<Exception>()); }
        T? result = default; bool hasResult = false; Exception? workError = null;
        try { result = work(operation); hasResult = true; }
        catch (Exception error) { workError = error; }
        var cleanup = operation.CompleteWithErrors();
        return (hasResult, result, null, workError, cleanup);
    }

'''
 s=s[:at]+method+s[at:]
 at=s.index('        internal void Complete()')
 s=s[:at]+'''        internal IReadOnlyList<Exception> CompleteWithErrors()
        {
            if (_leaseSet is not null) return _leaseSet.CompleteWithErrors();
            try { Interlocked.Exchange(ref _lease, null)?.Dispose(); return Array.Empty<Exception>(); }
            catch (Exception error) { return Array.AsReadOnly(new[] { error }); }
        }
'''+s[at:];return s
edit('Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs',session)
def capture_session(s):
 at=s.index('    private readonly XsltCaptureOptions ')
 s=s[:at]+'''    private List<Exception>? _originalErrors;
    internal bool PreserveOriginals { get; set; }
    internal IReadOnlyList<Exception> OriginalErrors => _originalErrors is null ? Array.Empty<Exception>() : Array.AsReadOnly(_originalErrors.ToArray());
    internal void RetainOriginal(Exception error)
    {
        if (!PreserveOriginals) return;
        var errors = _originalErrors ??= new List<Exception>();
        if (!errors.Any(e => ReferenceEquals(e, error))) errors.Add(error);
    }
'''+s[at:]
 # Retain at the lowest serialization boundary, before bounds translate exceptions.
 s=s.replace('''        catch (SerializationByteLimitException ex)
        {''','''        catch (SerializationByteLimitException ex)
        {
            RetainOriginal(ex);''')
 needle='''            throw new XsltCaptureLimitException(kind, identity);
        }
    }'''
 assert needle in s;s=s.replace(needle,'''            throw new XsltCaptureLimitException(kind, identity);
        }
        catch (Exception error) { RetainOriginal(error); throw; }
    }''',1)
 s=s.replace('''    internal XsltCaptureResult BuildResult(Exception? failure)
    {''','''    internal XsltCaptureResult BuildResult(Exception? failure)
    {
        if (failure is not null) RetainOriginal(failure);''')
 s=s.replace('''        catch (ExecutionInterruptException ex)
        {
            RecordInterrupt(ex);
            failure = ex;''','''        catch (ExecutionInterruptException ex)
        {
            RetainOriginal(ex);
            RecordInterrupt(ex);
            failure = ex;''')
 return s
edit('Bosak.Xslt/Capture/XsltCaptureSession.cs',capture_session)
def executable(s):
 at=s.index('    internal XsltCaptureResult CaptureExplicitCandidate(');before=s[:at];tail=s[at:]
 tail=tail.replace('string', 'string',0)
 tail=tail.replace('XsltCaptureOptions options)', 'XsltCaptureOptions options, out IReadOnlyList<Exception> originalErrors)',1)
 tail=tail.replace('new XsltCaptureSession(options, _messageListener, gate, CandidateProductionObserver);','new XsltCaptureSession(options, _messageListener, gate, CandidateProductionObserver) { PreserveOriginals = true };',1)
 tail=tail.replace('return session.BuildResult(failure);','var captured = session.BuildResult(failure);\n        originalErrors = session.OriginalErrors;\n        return captured;',1)
 return before+tail
edit('Bosak.Xslt/Api/XsltExecutable.cs',executable)
def producer(s):
 s=s.replace('''    internal (XsltCaptureResult Capture, IReadOnlyList<ControlledResourceReceipt> Receipts) Run(''','''    internal sealed record Outcome(XsltCaptureResult Capture, IReadOnlyList<ControlledResourceReceipt> Receipts, IReadOnlyList<Exception> ExecutionErrors);
    internal Outcome RunDetailed(''')
 s=s.replace('executable.CaptureExplicitCandidate(match, global, copied);','executable.CaptureExplicitCandidate(match, global, copied, out var originals);').replace('return (result, Array.AsReadOnly(policy.Receipts.ToArray()));','return new Outcome(result, Array.AsReadOnly(policy.Receipts.ToArray()), originals);')
 at=s.rfind('\n}')
 method='''
    internal (XsltCaptureResult Capture, IReadOnlyList<ControlledResourceReceipt> Receipts) Run(
        SchemaQuerySession.Operation operation, SchemaOperationInputs inputs, IXdmNode? match, IXdmNode? global,
        string? template, string? mode, string? outputBase, XsltCaptureOptions options)
    {
        var detailed = RunDetailed(operation, inputs, match, global, template, mode, outputBase, options);
        return (detailed.Capture, detailed.Receipts);
    }
''';return s[:at]+method+s[at:]
edit('Bosak.Xslt/Api/SchemaCaptureCandidate.cs',producer)
owned_result='(Bosak.Xslt.Capture.XsltCaptureResult? Capture, IReadOnlyList<Bosak.XPath.Runtime.Resources.ControlledResourceReceipt> Receipts, IReadOnlyList<Exception> ExecutionErrors, Exception? AdmissionError, Exception? WorkError, IReadOnlyList<Exception> CleanupErrors, bool IsSuccess)'
owned_func='Func<System.Collections.Immutable.ImmutableArray<SchemaEvidenceKernel.Snapshot>, EvaluationContext, Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool>, IXdmNode?, IXdmNode?, string?, string?, string?, Bosak.Xslt.Capture.XsltCaptureOptions, CancellationToken, '+owned_result+'>'
def owned_producer(s):
 s=s.replace('internal sealed record Outcome(XsltCaptureResult Capture, IReadOnlyList<ControlledResourceReceipt> Receipts, IReadOnlyList<Exception> ExecutionErrors);\n    internal Outcome RunDetailed(', 'internal (XsltCaptureResult Capture, IReadOnlyList<ControlledResourceReceipt> Receipts, IReadOnlyList<Exception> ExecutionErrors) RunDetailed(').replace('return new Outcome(result, Array.AsReadOnly(policy.Receipts.ToArray()), originals);','return (result, Array.AsReadOnly(policy.Receipts.ToArray()), originals);')
 at=s.rfind('\n}')
 method='''
    internal '''+owned_result+''' RunOwned(
        System.Collections.Immutable.ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots, EvaluationContext caller,
        Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> owns, IXdmNode? match, IXdmNode? global,
        string? template, string? mode, string? outputBase, XsltCaptureOptions options, CancellationToken cancellation)
    {
        var outcome = session.RunWithOutcome(session.Environment, snapshots,
            op => RunDetailed(op, op.CaptureInputs(caller, owns, 16, 16, true), match, global, template, mode, outputBase, options), cancellation);
        var capture = outcome.HasResult ? outcome.Result.Capture : null;
        var receipts = outcome.HasResult ? outcome.Result.Receipts : Array.Empty<ControlledResourceReceipt>();
        var executionErrors = outcome.HasResult ? outcome.Result.ExecutionErrors : Array.Empty<Exception>();
        bool success = capture?.IsSuccess == true && executionErrors.Count == 0 && outcome.AdmissionError is null
            && outcome.WorkError is null && outcome.CleanupErrors.Count == 0;
        return (capture, receipts, executionErrors, outcome.AdmissionError, outcome.WorkError, outcome.CleanupErrors, success);
    }
'''
 return s[:at]+method+s[at:]
edit('Bosak.Xslt/Api/SchemaCaptureCandidate.cs',owned_producer)
def owned_factory(s):
 start=s.index('    internal static bool TryCompileCaptureCandidate(');end=s.index('\n    private static bool PlainStatic',start)
 method=s[start:end];a=method.index('out Func<');b=method.index(' capture,',a)
 method=method[:a]+'out '+owned_func+'?'+method[b:]
 method=method.replace('TryCompileCaptureCandidate(', 'TryCompileOutcomeCandidate(').replace('new SchemaCaptureCandidate(session, executable).Run;', 'new SchemaCaptureCandidate(session, executable).RunOwned;')
 return s[:end]+'\n'+method+s[end:]
edit('Bosak.Xslt/Api/SchemaHostSessionCompilation.cs',owned_factory)
delta={}
for p in S.rglob('*.cs'):
 name=p.relative_to(S).as_posix();base=R/'src'/name
 if not base.exists():(P/'tools/SchemaCaptureCandidate.cs.txt').write_bytes(p.read_bytes());continue
 a=base.read_text(encoding='utf-8');b=p.read_text(encoding='utf-8')
 if a==b:continue
 first=b.index('using ') if 'using ' in b else b.index('namespace ');at=b.rfind('// ===========================================================================================================================================================',0,first);major,minor=max(tuple(map(int,v)) for v in re.findall(r'Charles Korthout\s*\|\s*(\d+)\.(\d+)',b))
 b=b[:at]+f'//                      | Charles Korthout | {major}.{minor+1} | 01-11-2026 | Isolated capture and cleanup outcome preservation. |\n'+b[at:];p.write_bytes(b.encode())
 out=P/'patches'/name;out.parent.mkdir(parents=True,exist_ok=True);out.with_suffix('.patch').write_bytes(''.join(difflib.unified_diff(a.splitlines(True),b.splitlines(True),fromfile='a/src/'+name,tofile='b/src/'+name)).encode())
 delta[name]={'originalSha256':hashlib.sha256(base.read_bytes()).hexdigest(),'candidateSha256':hashlib.sha256(p.read_bytes()).hexdigest()}
(E/'SOURCE-DELTA.json').write_text(json.dumps(delta,indent=2)+'\n')
for n in ['run.py','regressions.py','seal-inputs.py','verify.py']:
 if not (P/'tools'/n).exists():(P/'tools'/n).write_bytes((R/'docs/foundation-bcap-controls/tools'/n).read_text().replace('bcap-controls-v3','bcap-outcomes-v3').encode())
if not (P/'tools/Probe.cs.txt').exists():(P/'tools/Probe.cs.txt').write_bytes((R/'docs/foundation-bcap-controls/tools/Probe.cs.txt').read_bytes())
print('Prepared isolated original-exception retention and nonthrowing terminal outcome candidate.')
