// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Checks direct issued-value transport through the private compiled session.
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
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks direct issued-value transport through the private compiled session.</summary>
public class SchemaValueTransportTests
{
    /// <summary>VM identity preserves evidence until its snapshot closes.</summary>
    [Fact]
    public void DirectReadAndVmIdentityKeepSnapshotEvidence()
    {
        var issuer = new Issuer(); using var session = Compile(issuer);
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        var value = session.Run(session.Environment, snapshot, op =>
        {
            var input = op.ReadValue(issuer.Issue(true));
            var result = op.Evaluate(new EvaluationContext().WithVariable("v", input));
            Assert.Same(session.ReadEvidence(input), session.ReadEvidence(result));
            Assert.Equal(42, result.IntegerValue); return result;
        });
        Assert.Same(issuer.Origin, session.ReadEvidence(value)!.OriginalAssessment);
        snapshot.Dispose(); Assert.Throws<ObjectDisposedException>(() => session.ReadEvidence(value));
    }

    /// <summary>Conversions reject plain values and sources from another operation snapshot.</summary>
    [Fact]
    public void ConversionRequiresAdmittedSourceInOperationSnapshot()
    {
        var issuer = new Issuer(); using var session = Compile(issuer);
        using var first = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        using var second = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        var other = session.Run(session.Environment, second, op => op.ReadValue(issuer.Issue(true)));
        session.Run(session.Environment, first, op =>
        {
            Assert.Throws<ArgumentException>(() => op.AdmitConversionValue(XdmValue.FromInteger(42), issuer.Issue(false)));
            Assert.Throws<ArgumentException>(() => op.AdmitConversionValue(other, issuer.Issue(false)));
            var value = op.ReadValue(issuer.Issue(true));
            var noop = op.AdmitConversionValue(value, issuer.Issue(false));
            Assert.Same(session.ReadEvidence(value), session.ReadEvidence(noop)); return 0;
        });
    }

    /// <summary>Expired operations reject reads and construction admission.</summary>
    [Fact]
    public void CompletedOperationCannotReadOrAdmitConversion()
    {
        var issuer = new Issuer(); using var session = Compile(issuer);
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        SchemaQuerySession.Operation? expired = null;
        var value = session.Run(session.Environment, snapshot, op => { expired = op; return op.ReadValue(issuer.Issue(true)); });
        Assert.Throws<ObjectDisposedException>(() => expired!.ReadValue(issuer.Issue(true)));
        Assert.Throws<ObjectDisposedException>(() => expired!.AdmitConversionValue(value, issuer.Issue(false)));
    }

    private static SchemaQuerySession Compile(Issuer issuer)
    {
        Assert.True(SchemaSessionCompilation.TryCompile("$v", new CompileOptions(), issuer, 2, 16, out var session, out var reason), reason);
        return session!;
    }
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal object Origin { get; } = new();
        private readonly object _type = new();
        private readonly HashSet<ISchemaIssuedScalar> _issued = new(ReferenceEqualityComparer.Instance);
        internal ISchemaIssuedScalar Issue(bool assessed)
        { var scalar = new Scalar(XdmValue.FromInteger(42), _type, assessed ? Origin : null); _issued.Add(scalar); return scalar; }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => _issued.Contains(value);
        bool ISchemaEvidenceIssuer.OwnsType(object type) => ReferenceEquals(type, _type);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        private sealed class Scalar(XdmValue value, object type, object? origin) : ISchemaIssuedScalar
        {
            XdmValue ISchemaIssuedScalar.Value => value;
            object ISchemaIssuedScalar.DeclaredType => type;
            object ISchemaIssuedScalar.ActualType => type;
            object? ISchemaIssuedScalar.OriginalAssessment => origin;
        }
    }
}
