// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Composes private schema admission with whole evaluation operation ownership.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      | Charles Korthout | 0.2   | 31-10-2026     | Compose issued-value reads and explicit conversion transport (1.36.0)                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Private compiler-issued immutable static context; equality never admits an environment.</summary>
internal sealed class SchemaQueryEnvironment
{
    internal const string DescriptorFormat = "bosak-private-static-v1";
    internal SchemaQueryEnvironment(ImmutableDictionary<string, string> namespaces, string descriptor)
    {
        Namespaces = namespaces;
        Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(descriptor)));
    }
    internal ImmutableDictionary<string, string> Namespaces { get; }
    internal string Fingerprint { get; }
}

/// <summary>Directly composes private evidence admission with a frozen compiled evaluator.</summary>
internal sealed class SchemaQuerySession : IDisposable
{
    private readonly SchemaEvidenceKernel _kernel;
    private readonly Func<EvaluationContext, XdmValue> _evaluate;

    internal SchemaQuerySession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate,
        ISchemaEvidenceIssuer issuer, int snapshotLimit, int valueLimit)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(evaluate);
        Environment = environment;
        _evaluate = evaluate;
        _kernel = new SchemaEvidenceKernel(environment, issuer, snapshotLimit, valueLimit);
    }

    internal SchemaQueryEnvironment Environment { get; }
    internal SchemaEvidenceKernel.Evidence? ReadEvidence(XdmValue value) => _kernel.ReadValue(value);
    internal SchemaEvidenceKernel.Snapshot OpenSnapshot(SchemaQueryEnvironment environment, object identity,
        object assessment, Action release, CancellationToken cancellationToken = default)
        => _kernel.Open(environment, identity, assessment, release, cancellationToken);

    internal T Run<T>(SchemaQueryEnvironment environment, SchemaEvidenceKernel.Snapshot snapshot,
        Func<Operation, T> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        _kernel.AdmitEnvironment(environment);
        _kernel.CheckSnapshot(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        var operation = new Operation(this, snapshot, snapshot.Enter(), cancellationToken);
        try { return work(operation); }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, operation.Complete); throw; }
        finally { operation.Complete(); }
    }

    internal async Task<T> RunAsync<T>(SchemaQueryEnvironment environment, SchemaEvidenceKernel.Snapshot snapshot,
        Func<Operation, Task<T>> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        _kernel.AdmitEnvironment(environment);
        _kernel.CheckSnapshot(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        // This raw task, not any caller's cancellable wait, owns the operation until work completes.
        var operation = new Operation(this, snapshot, snapshot.Enter(), cancellationToken);
        try { return await work(operation).ConfigureAwait(false); }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, operation.Complete); throw; }
        finally { operation.Complete(); }
    }

    /// <summary>Closes admissions without disposing the borrowed issuer.</summary>
    public void Dispose() => _kernel.Close();

    /// <summary>Borrowed operation for the complete evaluation/drain or detachment callback.</summary>
    internal sealed class Operation
    {
        private readonly SchemaQuerySession _owner;
        private readonly SchemaEvidenceKernel.Snapshot _snapshot;
        private readonly CancellationToken _cancellationToken;
        private IDisposable? _lease;
        internal Operation(SchemaQuerySession owner, SchemaEvidenceKernel.Snapshot snapshot, IDisposable lease, CancellationToken cancellationToken)
        { _owner = owner; _snapshot = snapshot; _lease = lease; _cancellationToken = cancellationToken; }
        private void Check()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _lease) is null, this);
            _cancellationToken.ThrowIfCancellationRequested();
            _owner._kernel.CheckSnapshot(_snapshot);
        }
        internal void ValidateAccess() => Check();
        internal XdmValue ReadValue(ISchemaIssuedScalar value)
        { Check(); return _owner._kernel.AttachValue(_owner._kernel.Read(_snapshot, value, _cancellationToken)); }
        internal XdmValue AdmitConversionValue(XdmValue source, ISchemaIssuedScalar constructed)
        {
            Check();
            var evidence = _owner._kernel.ReadValue(source)
                ?? throw new ArgumentException("An admitted source value is required.", nameof(source));
            if (evidence.Snapshot is not null && !ReferenceEquals(evidence.Snapshot, _snapshot))
                throw new ArgumentException("Source belongs to another operation snapshot.", nameof(source));
            return _owner._kernel.AttachValue(_owner._kernel.AdmitConversion(evidence, constructed, _cancellationToken));
        }
        internal XdmValue Evaluate(EvaluationContext context)
        {
            Check();
            return _owner._evaluate(context);
        }
        internal SchemaEvidenceKernel.Evidence Read(ISchemaIssuedScalar value)
        { Check(); return _owner._kernel.Read(_snapshot, value, _cancellationToken); }
        internal SchemaEvidenceKernel.Evidence AdmitConversion(SchemaEvidenceKernel.Evidence source, ISchemaIssuedScalar constructed)
        { Check(); return _owner._kernel.AdmitConversion(source, constructed, _cancellationToken); }
        internal void Complete() => Interlocked.Exchange(ref _lease, null)?.Dispose();
    }
}
