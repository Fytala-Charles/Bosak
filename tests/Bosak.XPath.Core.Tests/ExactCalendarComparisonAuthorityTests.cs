// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies equal calendar values cannot exchange representation-bound authority.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks C19 without weakening private representation binding.</summary>
public sealed class ExactCalendarComparisonAuthorityTests
{
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Type=new(),Origin=new();
        private readonly HashSet<ISchemaIssuedScalar> _values=new(ReferenceEqualityComparer.Instance);
        internal ISchemaIssuedScalar Issue(XdmValue value){var s=new Scalar(value,Type,Origin);_values.Add(s);return s;}
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value)=>_values.Contains(value);
        bool ISchemaEvidenceIssuer.OwnsType(object type)=>ReferenceEquals(type,Type);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin)=>ReferenceEquals(origin,Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object a,object b)=>ReferenceEquals(a,b);
        private sealed class Scalar(XdmValue value,object type,object origin):ISchemaIssuedScalar
        {
            XdmValue ISchemaIssuedScalar.Value=>value;
            object ISchemaIssuedScalar.DeclaredType=>type;
            object ISchemaIssuedScalar.ActualType=>type;
            object? ISchemaIssuedScalar.OriginalAssessment=>origin;
        }
    }
    /// <summary>Value equivalence is not component identity and cannot authorize a marker rebind.</summary>
    [Fact] public void C19_EqualInstantsCannotRebindAuthority()
    {
        var a=XdmExactCalendarValue.Parse("2000-01-01T00:00:00.123456789Z",XdmExactCalendarKind.DateTime,64,32);
        var b=XdmExactCalendarValue.Parse("2000-01-01T01:00:00.123456789+01:00",XdmExactCalendarKind.DateTime,64,32);
        Assert.Equal(0,XdmExactCalendarComparison.CompareXPath(a,b,0));Assert.True(XdmExactCalendarComparison.SameKey(a,b));Assert.NotEqual(a,b);
        var x=XdmValue.FromExactCalendar(a);var y=XdmValue.FromExactCalendar(b);Assert.False(x.SchemaTransportMatches(y));
        var issuer=new Issuer();var env=new object();var kernel=new SchemaEvidenceKernel(env,issuer,1,2);using var snapshot=kernel.Open(env,new object(),issuer.Origin,()=>{},default);
        var atom=kernel.AttachValue(kernel.Read(snapshot,issuer.Issue(x),default));var forged=XdmValue.SchemaTransportAttach(y,XdmValue.SchemaTransportMarker(atom)!);
        Assert.Throws<ArgumentException>(()=>kernel.ReadValue(forged));Assert.NotNull(kernel.ReadValue(atom));
    }
}
