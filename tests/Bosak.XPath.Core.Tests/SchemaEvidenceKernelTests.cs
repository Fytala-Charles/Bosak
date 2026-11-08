// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Checks private schema evidence admission, cache bounds and cleanup ownership.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks private evidence issuance, conversion boundaries and resource ownership.</summary>
public class SchemaEvidenceKernelTests
{
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Decimal = new(), Integer = new(), Origin = new(), OtherOrigin = new();
        private readonly HashSet<ISchemaIssuedScalar> _values = new(ReferenceEqualityComparer.Instance);
        internal Action? Probe;
        internal ISchemaIssuedScalar Issue(XdmValue value, object? origin, object? actual = null, object? declared = null)
        {
            var scalar = new Scalar(value, declared ?? Decimal, actual ?? Decimal, origin);
            _values.Add(scalar); return scalar;
        }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) { Probe?.Invoke(); return _values.Contains(value); }
        bool ISchemaEvidenceIssuer.OwnsType(object value) => ReferenceEquals(value, Decimal) || ReferenceEquals(value, Integer);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object value) => ReferenceEquals(value, Origin) || ReferenceEquals(value, OtherOrigin);
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        private sealed class Scalar(XdmValue value, object declared, object actual, object? origin) : ISchemaIssuedScalar
        {
            XdmValue ISchemaIssuedScalar.Value => value;
            object ISchemaIssuedScalar.DeclaredType => declared;
            object ISchemaIssuedScalar.ActualType => actual;
            object? ISchemaIssuedScalar.OriginalAssessment => origin;
        }
    }
    private sealed class Trap : ISchemaIssuedScalar
    {
        internal int Reads;
        XdmValue ISchemaIssuedScalar.Value { get { Reads++; throw new Exception("Untrusted getter"); } }
        object ISchemaIssuedScalar.DeclaredType { get { Reads++; throw new Exception("Untrusted getter"); } }
        object ISchemaIssuedScalar.ActualType { get { Reads++; throw new Exception("Untrusted getter"); } }
        object? ISchemaIssuedScalar.OriginalAssessment { get { Reads++; throw new Exception("Untrusted getter"); } }
    }

    /// <summary>Unissued containers are refused without invoking their getters.</summary>
    [Fact]
    public void RefusesUnissuedContainerBeforeGetters()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var trap = new Trap();
        Assert.Throws<ArgumentException>(() => kernel.Read(snapshot, trap, default));
        Assert.Equal(0, trap.Reads);
    }

    /// <summary>Immutable values cache by issued reference; capacity never silently evicts a live value.</summary>
    [Fact]
    public void CachesIssuedScalarAndEnforcesValueLimit()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 1);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var scalar = issuer.Issue(XdmValue.FromDecimal(10), issuer.Origin);
        var evidence = kernel.Read(snapshot, scalar, default);
        Assert.Same(evidence, kernel.Read(snapshot, scalar, default));
        Assert.Equal(10m, evidence.Value.DecimalValue);
        Assert.Throws<InvalidOperationException>(() => kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(20), issuer.Origin), default));
        snapshot.Dispose(); Assert.Equal(0, snapshot.CachedValues);
    }

    /// <summary>Issued containers cannot supply undefined, non-atomic, foreign-type or wrong-origin evidence.</summary>
    [Fact]
    public void ValidatesContentsOfIssuedContainers()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 4);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        foreach (var value in new[] { XdmValue.Undefined, XdmValue.FromSequence(XdmSequence.Empty) })
            Assert.Throws<ArgumentException>(() => kernel.Read(snapshot, issuer.Issue(value, issuer.Origin), default));
        Assert.Throws<ArgumentException>(() => kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(1), issuer.OtherOrigin), default));
        Assert.Throws<ArgumentException>(() => kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(1), issuer.Origin, new object()), default));
        Assert.Equal(0, snapshot.CachedValues);
    }

    /// <summary>No-op admission retains source evidence; conversions detach without creating assessment authority.</summary>
    [Fact]
    public void ConversionRequiresIssuedFreshTargetAndDetachesChanges()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 4);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var source = kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(10), issuer.Origin), default);
        Assert.Same(source, kernel.AdmitConversion(source, issuer.Issue(XdmValue.FromDecimal(10), null), default));
        var converted = kernel.AdmitConversion(source, issuer.Issue(XdmValue.FromInteger(10), null, issuer.Integer, issuer.Integer), default);
        Assert.Null(converted.OriginalAssessment);
        Assert.Same(issuer.Integer, converted.ActualType);
        snapshot.Dispose();
        kernel.CheckEvidence(converted);
        Assert.Throws<ObjectDisposedException>(() => kernel.CheckEvidence(source));
    }

    /// <summary>Same primitive kind, value or member alone does not establish an authoritative no-op.</summary>
    [Fact]
    public void ChangedBoundaryValueOrLegacyTypeCannotReuseEvidence()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 4);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var source = kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(10), issuer.Origin), default);
        var targets = new[] {
            issuer.Issue(XdmValue.FromDecimal(11), null),
            issuer.Issue(XdmValue.FromDecimal(10), null, issuer.Decimal, issuer.Integer),
            issuer.Issue(XdmValue.FromDecimal(10, "unsignedLong"), null)
        };
        foreach (var target in targets)
        {
            var result = kernel.AdmitConversion(source, target, default);
            Assert.NotSame(source, result); Assert.Null(result.OriginalAssessment);
        }
        Assert.Throws<ArgumentException>(() => kernel.AdmitConversion(source, new Trap(), default));
        Assert.Throws<ArgumentException>(() => kernel.AdmitConversion(source, issuer.Issue(XdmValue.FromDecimal(10), issuer.Origin), default));
    }

    /// <summary>Equivalent-looking environments, foreign snapshots and fabricated evidence cannot gain admission.</summary>
    [Fact]
    public void RejectsForeignAdmissionsBeforeProviderDispatch()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        Assert.Throws<ArgumentException>(() => kernel.Open(new object(), new object(), issuer.Origin, () => { }, default));
        var other = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        using var foreign = other.Open(environment, new object(), issuer.Origin, () => { }, default);
        issuer.Probe = () => throw new Exception("Must not dispatch");
        Assert.Throws<ArgumentException>(() => kernel.Read(foreign, new Trap(), default));
        var forged = new SchemaEvidenceKernel.Evidence(new object(), XdmValue.FromDecimal(1), issuer.Decimal, issuer.Decimal, null, null);
        Assert.Throws<ArgumentException>(() => kernel.CheckEvidence(forged));
    }

    /// <summary>Closure retains capacity/resources until every raw lease completes and releases once.</summary>
    [Fact]
    public void DefersCleanupAndAdmissionCapacityUntilRawCompletion()
    {
        var issuer = new Issuer(); var environment = new object(); int releases = 0;
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        var identity = new object();
        var snapshot = kernel.Open(environment, identity, issuer.Origin, () => releases++, default);
        Assert.Throws<ArgumentException>(() => kernel.Open(environment, identity, issuer.Origin, () => { }, default));
        var lease = snapshot.Enter(); snapshot.Dispose(); snapshot.Dispose();
        Assert.Equal(0, releases);
        Assert.Throws<ObjectDisposedException>(() => snapshot.Enter());
        Assert.Throws<InvalidOperationException>(() => kernel.Open(environment, new object(), issuer.Origin, () => { }, default));
        lease.Dispose(); lease.Dispose(); Assert.Equal(1, releases);
        using var replacement = kernel.Open(environment, new object(), issuer.Origin, () => releases++, default);
    }

    /// <summary>All cleanup callbacks execute even when one fails, without disposing the borrowed issuer.</summary>
    [Fact]
    public void CleanupFailureDoesNotSkipOtherSnapshots()
    {
        var issuer = new Issuer(); var environment = new object(); int releases = 0;
        var kernel = new SchemaEvidenceKernel(environment, issuer, 2, 2);
        kernel.Open(environment, new object(), issuer.Origin, () => throw new InvalidOperationException("owned cleanup"), default);
        kernel.Open(environment, new object(), issuer.Origin, () => releases++, default);
        Assert.Single(Assert.Throws<AggregateException>(() => kernel.Close()).InnerExceptions);
        Assert.Equal(1, releases); kernel.Close(); Assert.Equal(1, releases);
        Assert.Throws<ObjectDisposedException>(() => kernel.Open(environment, new object(), issuer.Origin, () => { }, default));
    }

    /// <summary>Cleanup cannot run while a provider membership callback is active.</summary>
    /// <returns>The bounded concurrent regression.</returns>
    [Fact]
    public async Task RevocationDuringProviderCallbackRetainsAdmission()
    {
        var issuer = new Issuer(); var environment = new object(); int releases = 0;
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => releases++, default);
        var scalar = issuer.Issue(XdmValue.FromDecimal(1), issuer.Origin);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        issuer.Probe = () => { entered.SetResult(); resume.Task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult(); };
        var read = Task.Run(() => kernel.Read(snapshot, scalar, default));
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            snapshot.Dispose(); Assert.Equal(0, releases);
        }
        finally { resume.TrySetResult(); }
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await read.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal(1, releases);
    }

    /// <summary>Pre-cancellation neither admits a snapshot nor acquires owned cleanup.</summary>
    [Fact]
    public void CancellationDoesNotTransferOwnership()
    {
        var issuer = new Issuer(); var environment = new object(); int releases = 0;
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => kernel.Open(environment, new object(), issuer.Origin, () => releases++, cancel.Token));
        Assert.Equal(0, releases);
    }

    /// <summary>A cleanup fault cannot hide a provider fault while the read owns the last lease.</summary>
    [Fact]
    public void ProviderAndCleanupFailuresAreBothRetained()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 2);
        var primary = new InvalidOperationException("provider"); var cleanup = new ApplicationException("cleanup");
        var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => throw cleanup, default);
        issuer.Probe = () => { snapshot.Dispose(); throw primary; };
        var errors = Assert.Throws<AggregateException>(() => kernel.Read(snapshot, issuer.Issue(XdmValue.FromDecimal(1), issuer.Origin), default)).InnerExceptions;
        Assert.Equal(new Exception[] { primary, cleanup }, errors);
    }
}
