// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Verifies public provider handoff from a separate assembly without internal access.
// SPECIAL NOTES        : Isolated provider/consumer qualification; not a production or published API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation; isolated public provider handoff and separate consumer controls.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.Schema.ContainerPrototype;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.Schema.PublicConsumer;

/// <summary>Exercises public provider handoff from a separate assembly without friends, reflection or internals.</summary>
public sealed class PublicConsumerTests
{
    private static XDocument Schema() => XDocument.Parse("""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
        <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
        <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
        """);
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
    private static IXdmNode First(XdmSequence sequence)
    { foreach (var value in sequence) return value.NodeValue; throw new InvalidOperationException("Missing node."); }
    private static IXdmNode Child(NativeProviderResult result) => First(First(result.Assessment.ValidatedRoot!.Children(XdmNodeKind.Element)).Children(XdmNodeKind.Element));
    private static XdmValue Evaluate(NativeProviderSession session, SchemaQuerySnapshot snapshot, IXdmNode node) =>
        session.Engine.Run([snapshot], op => op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(node))));
    /// <summary>Public assessment, admission and typed querying need no internal output field.</summary>
    [Fact] public void PublicRoundTrip()
    {
        using var session = new NativeProviderSession(Schema());
        using var result = session.Assess(XDocument.Parse("<order><amount> 01.25 </amount></order>"), new Episode());
        Assert.Equal(SchemaAssessmentStatus.Valid, result.Assessment.Status); Assert.True(result.Assessment.Evidence!.IsComplete);
        using var snapshot = result.OpenSnapshot(); Assert.Equal(1.25m, Evaluate(session, snapshot, Child(result)).DecimalValue);
    }
    /// <summary>Unsuccessful public results cannot be admitted even when their diagnostic output is bounded.</summary>
    /// <param name="xml">Invalid or unsupported input.</param><param name="status">Expected outcome.</param>
    [Theory][InlineData("<order><amount>bad</amount></order>", SchemaAssessmentStatus.Invalid)]
    [InlineData("<order><amount>0.00000000000000000000000000001</amount></order>", SchemaAssessmentStatus.Unsupported)]
    public void FailedResultHasNoAdmission(string xml, SchemaAssessmentStatus status)
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse(xml), new Episode(), new(1, 1));
        Assert.Equal(status, result.Assessment.Status); Assert.Null(result.Assessment.ValidatedRoot);
        Assert.Throws<InvalidOperationException>(() => result.OpenSnapshot());
    }
    /// <summary>Result disposal revokes existing admissions and projected nodes without closing the session.</summary>
    [Fact] public void ResultDisposalRevokesItsOutputOnly()
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse("<order><amount>2</amount></order>"), new Episode());
        using var snapshot = result.OpenSnapshot(); var node = Child(result); result.Dispose(); result.Dispose();
        Assert.Throws<ObjectDisposedException>(() => Evaluate(session, snapshot, node));
        Assert.Throws<ObjectDisposedException>(() => node.StringValue);
        using var next = session.Assess(XDocument.Parse("<order/>"), new Episode()); Assert.Equal(SchemaAssessmentStatus.Valid, next.Assessment.Status);
    }
    /// <summary>Closing an admission does not dispose the provider result; the same result can be admitted again.</summary>
    [Fact] public void AdmissionCanBeReopened()
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse("<order><amount>3</amount></order>"), new Episode());
        using (var first = result.OpenSnapshot()) Assert.Equal(3m, Evaluate(session, first, Child(result)).DecimalValue);
        using var second = result.OpenSnapshot(); Assert.Equal(3m, Evaluate(session, second, Child(result)).DecimalValue);
    }
    /// <summary>A refused duplicate admission does not break the original admission.</summary>
    [Fact] public void DuplicateAdmissionPreservesFirst()
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse("<order><amount>4</amount></order>"), new Episode());
        using var first = result.OpenSnapshot(); Assert.Throws<ArgumentException>(() => result.OpenSnapshot());
        Assert.Equal(4m, Evaluate(session, first, Child(result)).DecimalValue);
    }
    /// <summary>Session disposal forbids new admissions while result-owned data remains independently disposable.</summary>
    [Fact] public void SessionAndResultHaveSeparateOwners()
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse("<order><amount>5</amount></order>"), new Episode());
        session.Dispose(); Assert.Throws<ObjectDisposedException>(() => result.OpenSnapshot());
        Assert.Equal("5", Child(result).StringValue); result.Dispose(); Assert.Throws<ObjectDisposedException>(() => result.Assessment);
    }
    /// <summary>Cancellation during public admission does not consume the input's admission slot.</summary>
    [Fact] public void CanceledAdmissionCanRetry()
    {
        using var session = new NativeProviderSession(Schema()); using var result = session.Assess(XDocument.Parse("<order><amount>6</amount></order>"), new Episode());
        using var cancel = new CancellationTokenSource(); cancel.Cancel(); Assert.Throws<OperationCanceledException>(() => result.OpenSnapshot(cancel.Token));
        using var snapshot = result.OpenSnapshot(); Assert.Equal(6m, Evaluate(session, snapshot, Child(result)).DecimalValue);
    }
    /// <summary>Nodes from another successful result refuse in the selected result's admission.</summary>
    [Fact] public void ForeignResultNodeRefuses()
    {
        using var session = new NativeProviderSession(Schema());
        using var first = session.Assess(XDocument.Parse("<order><amount>7</amount></order>"), new Episode());
        using var second = session.Assess(XDocument.Parse("<order><amount>8</amount></order>"), new Episode());
        using var snapshot = first.OpenSnapshot(); Assert.Throws<ArgumentException>(() => Evaluate(session, snapshot, Child(second)));
    }
    /// <summary>Caller schema/XML edits do not alter the provider's compiled schema or successful result.</summary>
    [Fact] public void CallerDocumentsStayIndependent()
    {
        var schema = Schema(); using var session = new NativeProviderSession(schema); schema.Root!.RemoveAll();
        var xml = XDocument.Parse("<order><amount>9</amount></order>"); using var result = session.Assess(xml, new Episode()); xml.Root!.Value = "changed";
        using var snapshot = result.OpenSnapshot(); Assert.Equal(9m, Evaluate(session, snapshot, Child(result)).DecimalValue);
    }
}
