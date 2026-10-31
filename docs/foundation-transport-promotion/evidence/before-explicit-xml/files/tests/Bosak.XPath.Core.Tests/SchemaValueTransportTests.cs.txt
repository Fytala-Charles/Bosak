// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Verifies representation binding and synchronous ownership in private scalar transport.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: durable private transport regressions (1.36.0)                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Verifies representation binding and synchronous ownership in private scalar transport.</summary>
public class SchemaValueTransportTests
{
    /// <summary>Map-key-equivalent representations cannot reuse an existing transport marker.</summary>
    /// <param name="index">The representation-sensitive fixture index.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void RefusesChangedRepresentationBehindExistingMarker(int index)
    {
        var (source, changed) = Changed(index);
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var evidence = kernel.Read(snapshot, issuer.Issue(source, true), default);
        var original = kernel.AttachValue(evidence);
        Assert.Same(evidence, kernel.ReadValue(original));
        var rebound = XdmValue.SchemaTransportAttach(changed, XdmValue.SchemaTransportMarker(original)!);
        Assert.Throws<ArgumentException>(() => kernel.ReadValue(rebound));
    }

    /// <summary>Explicit results keep their representation and detach the source assessment.</summary>
    /// <param name="index">The representation-sensitive fixture index.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void ConversionKeepsChangedRepresentation(int index)
    {
        var (source, changed) = Changed(index);
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var original = kernel.Read(snapshot, issuer.Issue(source, true), default);
        var converted = kernel.AdmitConversion(original, issuer.Issue(changed, false), default);
        Assert.NotSame(original, converted);
        Assert.Null(converted.OriginalAssessment);
        if (changed.Kind is XdmValueKind.Double or XdmValueKind.Float)
            Assert.Equal(BitConverter.DoubleToInt64Bits(changed.DoubleValue), BitConverter.DoubleToInt64Bits(converted.Value.DoubleValue));
        else Assert.Equal(changed.ToString(), converted.Value.ToString());
        snapshot.Dispose();
        Assert.Same(converted, kernel.ReadValue(kernel.AttachValue(converted)));
        kernel.Close();
        Assert.Throws<ObjectDisposedException>(() => kernel.AttachValue(converted));
    }

    /// <summary>Equivalent fresh payload objects preserve exact no-op evidence.</summary>
    /// <param name="index">The fresh-payload fixture index.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void FreshEquivalentPayloadKeepsNoOpEvidence(int index)
    {
        var date = new DateTimeOffset(2026, 10, 31, 1, 2, 3, TimeSpan.Zero);
        var (first, second) = index switch
        {
            0 => (XdmValue.FromString("alpha"), XdmValue.FromString(new string(new[] { 'a', 'l', 'p', 'h', 'a' }))),
            1 => (XdmValue.FromDecimal(12.5m), XdmValue.FromDecimal(12.5m)),
            2 => (XdmValue.FromQName(new XsQName("name", "urn:t", "p")), XdmValue.FromQName(new XsQName("name", "urn:t", "p"))),
            _ => (XdmValue.FromDateTime(date), XdmValue.FromDateTime(date))
        };
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var original = kernel.Read(snapshot, issuer.Issue(first, true), default);
        Assert.Same(original, kernel.AdmitConversion(original, issuer.Issue(second, false), default));
    }

    /// <summary>Cache hits avoid getters and still honor cancellation.</summary>
    [Fact]
    public void CachedReadAvoidsGettersAndHonorsCancellation()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        var scalar = issuer.Issue(XdmValue.FromInteger(42), true);
        var first = kernel.Read(snapshot, scalar, default); int reads = issuer.GetterReads;
        Assert.Same(first, kernel.Read(snapshot, scalar, default));
        Assert.Equal(reads, issuer.GetterReads);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => kernel.Read(snapshot, scalar, cancellation.Token));
        Assert.Equal(reads, issuer.GetterReads);
    }

    /// <summary>Membership retains its lease and preserves primary plus cleanup failures.</summary>
    [Fact]
    public void CachedReadRetainsLeaseAndPreservesBothFailures()
    {
        var issuer = new Issuer(); var environment = new object(); int releases = 0;
        var primary = new InvalidOperationException("membership"); var cleanup = new InvalidOperationException("cleanup");
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { releases++; throw cleanup; }, default);
        var scalar = issuer.Issue(XdmValue.FromInteger(42), true); _ = kernel.Read(snapshot, scalar, default);
        issuer.OnMembership = () => { snapshot.Dispose(); Assert.Equal(0, releases); throw primary; };
        var error = Assert.Throws<AggregateException>(() => kernel.Read(snapshot, scalar, default));
        Assert.Collection(error.Flatten().InnerExceptions, e => Assert.Same(primary, e), e => Assert.Same(cleanup, e));
        kernel.Close(); Assert.Equal(1, releases);
    }

    /// <summary>Default CLR equality, hashing and sets ignore private evidence.</summary>
    [Fact]
    public void EvidenceDoesNotChangeDefaultClrEqualityOrHashing()
    {
        var issuer = new Issuer(); var environment = new object();
        var kernel = new SchemaEvidenceKernel(environment, issuer, 1, 8);
        using var snapshot = kernel.Open(environment, new object(), issuer.Origin, () => { }, default);
        foreach (var plain in new[] { XdmValue.FromInteger(42), XdmValue.FromString("alpha"), XdmValue.FromDecimal(42m) })
        {
            var value = kernel.AttachValue(kernel.Read(snapshot, issuer.Issue(plain, true), default));
            Assert.True(plain.Equals(value)); Assert.True(value.Equals(plain));
            Assert.Equal(plain.GetHashCode(), value.GetHashCode());
            bool containsPlain = new HashSet<XdmValue> { value }.Contains(plain);
            Assert.True(containsPlain);
        }
    }

    private static (XdmValue Source, XdmValue Changed) Changed(int index)
    {
        var date = new DateTimeOffset(2026, 10, 31, 1, 2, 3, TimeSpan.FromHours(1));
        return index switch
        {
            0 => (XdmValue.FromDouble(0d), XdmValue.FromDouble(-0d)),
            1 => (XdmValue.FromFloat(0f), XdmValue.FromFloat(-0f)),
            2 => (XdmValue.FromDouble(double.NaN), XdmValue.FromDouble(BitConverter.Int64BitsToDouble(0x7ff8000000000001))),
            3 => (XdmValue.FromDuration("P1D"), XdmValue.FromDuration("PT24H")),
            4 => (XdmValue.FromQName(new XsQName("name", "urn:t", "p")), XdmValue.FromQName(new XsQName("name", "urn:t", "q"))),
            _ => (XdmValue.FromDateTime(date), XdmValue.FromDateTime(date.ToOffset(TimeSpan.Zero)))
        };
    }

    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal object Origin { get; } = new();
        private object Type { get; } = new();
        private readonly HashSet<ISchemaIssuedScalar> _issued = new(ReferenceEqualityComparer.Instance);
        internal int GetterReads;
        internal Action? OnMembership;
        internal ISchemaIssuedScalar Issue(XdmValue value, bool assessed)
        { var scalar = new Scalar(this, value, assessed ? Origin : null); _issued.Add(scalar); return scalar; }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) { OnMembership?.Invoke(); return _issued.Contains(value); }
        bool ISchemaEvidenceIssuer.OwnsType(object type) => ReferenceEquals(type, Type);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        private sealed class Scalar(Issuer issuer, XdmValue value, object? origin) : ISchemaIssuedScalar
        {
            XdmValue ISchemaIssuedScalar.Value { get { issuer.GetterReads++; return value; } }
            object ISchemaIssuedScalar.DeclaredType { get { issuer.GetterReads++; return issuer.Type; } }
            object ISchemaIssuedScalar.ActualType { get { issuer.GetterReads++; return issuer.Type; } }
            object? ISchemaIssuedScalar.OriginalAssessment { get { issuer.GetterReads++; return origin; } }
        }
    }
}
