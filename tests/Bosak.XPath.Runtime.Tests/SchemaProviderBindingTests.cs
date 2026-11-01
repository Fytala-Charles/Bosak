// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Checks exact provider binding, revocation and borrowed resource ownership.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Resources;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Verifies binding identity, ordering, revocation and borrowed provider lifetime.</summary>
public sealed class SchemaProviderBindingTests
{
    private sealed class Provider : ISchemaSessionProvider, ISchemaEvidenceIssuer, IDisposable
    {
        internal readonly object Origin = new();
        internal bool Available = true, Foreign;
        internal int Binds, IssuerReads, Disposals;
        internal Action? DuringBind;
        internal SchemaQueryEnvironment? Override;
        private sealed class Binding(Provider owner, SchemaQueryEnvironment environment) : ISchemaSessionBinding
        {
            SchemaQueryEnvironment ISchemaSessionBinding.Environment => owner.Override ?? (owner.Foreign ? new SchemaQueryEnvironment(environment.Namespaces, "same") : environment);
            ISchemaEvidenceIssuer ISchemaSessionBinding.Issuer { get { owner.IssuerReads++; return owner; } }
            void ISchemaSessionBinding.ThrowIfUnavailable() { if (!owner.Available) throw new ObjectDisposedException("provider"); }
        }
        ISchemaSessionBinding ISchemaSessionProvider.Bind(SchemaQueryEnvironment environment) { Binds++; DuringBind?.Invoke(); return new Binding(this, environment); }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => false;
        bool ISchemaEvidenceIssuer.OwnsType(object type) => false;
        bool ISchemaEvidenceIssuer.OwnsAssessment(object value) => ReferenceEquals(value, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object a, object b) => ReferenceEquals(a, b);
        void IDisposable.Dispose() { Disposals++; Available = false; }
    }
    private static SchemaQueryEnvironment Environment() => new(ImmutableDictionary<string, string>.Empty, "same");
    private static SchemaQuerySession Create(Provider provider) => SchemaProviderSessionBinding.Create(Environment(), context =>
    {
        Assert.NotNull(context.ResourcePolicy);
        foreach (var route in Enum.GetValues<ControlledResourceRoute>()) Assert.False(context.ResourcePolicy!.IsRouteSupported(route));
        return XdmValue.FromInteger(42);
    }, provider, 4, 16, default);
    /// <summary>Binds once, caches the issuer, requires typed input and leaves provider disposal to its owner.</summary>
    [Fact]
    public void UsesExactBorrowedBinding()
    {
        var provider = new Provider(); using var session = Create(provider); int released = 0;
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), provider.Origin, () => released++);
        session.Run(session.Environment, snapshot, op =>
        {
            Assert.Throws<NotSupportedException>(() => op.Evaluate(new EvaluationContext()));
            var result = op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(), _ => false, 16, 16), 16); long actual = 0;
            result.ForEachAtomic(v => actual = v.IntegerValue); Assert.Equal(42, actual); return 0;
        });
        Assert.Equal(1, provider.Binds); Assert.Equal(1, provider.IssuerReads); session.Dispose(); Assert.Equal(1, released); Assert.Equal(0, provider.Disposals);
    }
    /// <summary>Equal descriptive fingerprints do not admit another environment before issuer access.</summary>
    [Fact]
    public void RejectsForeignBindingBeforeIssuerGetter()
    {
        var provider = new Provider { Foreign = true };
        Assert.Throws<ArgumentException>(() => Create(provider)); Assert.Equal(0, provider.IssuerReads); Assert.Equal(0, provider.Disposals);
    }
    /// <summary>Cancellation during binding avoids reading its issuer and does not dispose a borrowed capability.</summary>
    [Fact]
    public void CancellationDuringBindDoesNotReadIssuer()
    {
        using var cancel = new CancellationTokenSource(); var provider = new Provider { DuringBind = cancel.Cancel };
        Assert.ThrowsAny<OperationCanceledException>(() => SchemaProviderSessionBinding.Create(Environment(), _ => XdmValue.FromInteger(1), provider, 4, 16, cancel.Token));
        Assert.Equal(1, provider.Binds); Assert.Equal(0, provider.IssuerReads); Assert.Equal(0, provider.Disposals);
    }
    /// <summary>Pre-canceled compilation does not invoke provider binding.</summary>
    [Fact]
    public void PreCancellationDoesNotBind()
    {
        using var cancel = new CancellationTokenSource(); cancel.Cancel(); var provider = new Provider();
        Assert.ThrowsAny<OperationCanceledException>(() => SchemaProviderSessionBinding.Create(Environment(), _ => XdmValue.FromInteger(1), provider, 4, 16, cancel.Token)); Assert.Equal(0, provider.Binds);
    }
    /// <summary>Revoked providers reject new snapshot admission while disposal remains available.</summary>
    [Fact]
    public void RevocationRejectsNewAdmission()
    {
        var provider = new Provider(); using var session = Create(provider); provider.Available = false;
        Assert.Throws<ObjectDisposedException>(() => session.OpenSnapshot(session.Environment, new object(), provider.Origin, () => { })); session.Dispose(); Assert.Equal(0, provider.Disposals);
    }
    /// <summary>Raw callbacks retain cleanup ownership while revocation prevents result access.</summary>
    /// <returns>The completed raw operation checks.</returns>
    [Fact]
    public async Task RevocationKeepsRawCleanupOwnership()
    {
        var provider = new Provider(); using var session = Create(provider); int releases = 0;
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), provider.Origin, () => releases++);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw = session.RunAsync(session.Environment, snapshot, async op =>
        {
            var result = op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(), _ => false, 16, 16), 16); entered.SetResult(); await resume.Task;
            Assert.Throws<ObjectDisposedException>(() => result.ForEachAtomic(_ => { })); return 0;
        });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10)); provider.Available = false; session.Dispose(); Assert.Equal(0, releases);
        resume.SetResult(); await raw.WaitAsync(TimeSpan.FromSeconds(10)); Assert.Equal(1, releases); Assert.Equal(0, provider.Disposals);
    }
    /// <summary>Changing the binding's environment after creation cannot bypass identity checks.</summary>
    [Fact]
    public void RechecksBindingIdentity()
    {
        var provider = new Provider(); using var session = Create(provider); provider.Override = Environment();
        Assert.Throws<ArgumentException>(() => session.OpenSnapshot(session.Environment, new object(), provider.Origin, () => { })); Assert.Equal(1, provider.IssuerReads);
    }
    /// <summary>Provider revocation and owner cleanup errors are retained together.</summary>
    [Fact]
    public void RetainsAvailabilityAndCleanupFailures()
    {
        var provider = new Provider(); using var session = Create(provider);
        var snapshot = session.OpenSnapshot(session.Environment, new object(), provider.Origin, () => throw new ApplicationException("cleanup"));
        var error = Assert.Throws<AggregateException>(() => session.Run<int>(session.Environment, snapshot, op =>
        {
            var result = op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(), _ => false, 16, 16), 16); snapshot.Dispose(); provider.Available = false; result.ForEachAtomic(_ => { }); return 0;
        }));
        Assert.IsType<ObjectDisposedException>(error.InnerExceptions[0]); Assert.Equal("cleanup", error.InnerExceptions[1].Message);
    }
    /// <summary>Invalid capacities fail before binding.</summary>
    /// <param name="snapshots">Snapshot bound.</param>
    /// <param name="values">Value bound.</param>
    [Theory][InlineData(0, 16)][InlineData(4, 0)][InlineData(-1, 16)]
    public void RejectsInvalidLimitsBeforeBind(int snapshots, int values)
    {
        var provider = new Provider(); Assert.Throws<ArgumentOutOfRangeException>(() => SchemaProviderSessionBinding.Create(Environment(), _ => XdmValue.FromInteger(1), provider, snapshots, values, default)); Assert.Equal(0, provider.Binds);
    }
}
