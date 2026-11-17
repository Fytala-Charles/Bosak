// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 November 2026
// PURPOSE              : Qualifies the owned native decimal API against current Core without friend access.
// SPECIAL NOTES        : Optional public Schema consumer qualification; no Core engine semantic change.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Bosak.Schema.Example;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Schema.CoreConsumer.Tests;

/// <summary>Qualifies the owned native decimal API against current Core without friend access.</summary>
public sealed class NativeDecimalCoreConsumerTests
{
    private static string Schema => File.ReadAllText(Data("native-decimal.xsd"));
    private static string Instance => File.ReadAllText(Data("native-decimal.xml"));
    private static NativeDecimalSessionOptions Options => new(256, 256, 1, 4, 1, 256, 256);
    private static NativeDecimalAssessmentContext Episode => new(new DateTimeOffset(2026, 11, 17, 12, 0, 0, TimeSpan.Zero), 60);

    /// <summary>Checks the complete public example and detached exported result after owner disposal.</summary>
    [Fact]
    public void Example_ValidNativeInput_ReturnsExactValuesAndOriginalLexicalExport()
    {
        var result = XElement.Parse(NativeDecimalExample.Run(Schema, Instance));
        Assert.Equal("3", (string?)result.Attribute("decimal-count"));
        Assert.Equal(new[] { "12.5", "2", "1" }, result.Elements("decimal").Select(e => e.Value));
        Assert.True(XNode.DeepEquals(XDocument.Load(Data("native-decimal.expected.xml")).Root, result.Element("export")!.Elements().Single()));
        Assert.Contains("12.50", result.ToString());
    }

    /// <summary>Checks native assessment rejects structure/coverage without publishing a partial document.</summary>
    [Theory]
    [InlineData("<order><tax>1</tax></order>", NativeDecimalOutcome.Invalid)]
    [InlineData("<order flag='x'/>", NativeDecimalOutcome.Unsupported)]
    [InlineData("<order", NativeDecimalOutcome.Invalid)]
    [InlineData("<!DOCTYPE order SYSTEM 'https://invalid.example/schema'><order/>", NativeDecimalOutcome.Unsupported)]
    public void Assess_NonvalidNativeInput_PublishesNoDocument(string instance, NativeDecimalOutcome expected)
    {
        using var session = NativeDecimalSession.Create(Schema, Options).Value!;
        var refused = session.Assess(instance, Episode);
        Assert.Equal(expected, refused.Outcome);
        Assert.Null(refused.Value);
        var diagnostic = Assert.Single(refused.Diagnostics);
        Assert.InRange(diagnostic.Message.Length, 1, 160);
        Assert.DoesNotContain("invalid.example", diagnostic.Message);
        using var next = session.Assess(Instance, Episode).Value!;
        Assert.Equal("12.5", next.ReadDecimal(0).ToInvariantString(256));
    }

    /// <summary>Checks public disposal, quota recovery and detached value lifetime on the current Core.</summary>
    [Fact]
    public void Dispose_DocumentAndSession_RevokesAccessAndRetainsDetachedValues()
    {
        using var session = NativeDecimalSession.Create(Schema, Options).Value!;
        var first = session.Assess(Instance, Episode).Value!;
        var scalar = first.ReadDecimal(0);
        Assert.Throws<InvalidOperationException>(() => session.Assess(Instance, Episode));
        first.Dispose();
        Assert.Throws<ObjectDisposedException>(() => first.ExportXml());
        var second = session.Assess(Instance, Episode).Value!;
        session.Dispose();
        second.Dispose();
        Assert.Throws<ObjectDisposedException>(() => second.ReadDecimal(0));
        Assert.Equal("12.5", scalar.ToInvariantString(256));
    }

    /// <summary>Checks numeric refusal and cancellation cannot poison the public session gate.</summary>
    [Fact]
    public void LimitsAndCancellation_RefuseWithoutRoundingOrLeakingAdmissions()
    {
        using var session = NativeDecimalSession.Create(Schema, new(256, 256, 1, 4, 1, 1, 0)).Value!;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => session.Assess("<", Episode, cancellation.Token));
        using var document = session.Assess(Instance, Episode).Value!;
        Assert.Throws<NotSupportedException>(() => document.ReadDecimal(0));
        Assert.Throws<OperationCanceledException>(() => document.ReadDecimal(2, cancellation.Token));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.ReadDecimal(3));
        Assert.Equal("1", document.ReadDecimal(2).ToInvariantString(8));
    }

    /// <summary>Checks an invalid or unsupported schema never constructs a usable public session.</summary>
    [Fact]
    public void Compile_NonvalidSchema_PublishesNoSession()
    {
        Assert.Equal(NativeDecimalOutcome.Invalid, NativeDecimalSession.Create("<", Options).Outcome);
        var unsupported = NativeDecimalSession.Create(Schema.Replace("xs:decimal", "xs:string", StringComparison.Ordinal), Options);
        Assert.Equal(NativeDecimalOutcome.Unsupported, unsupported.Outcome);
        Assert.Null(unsupported.Value);
        Assert.Equal("BSK-NATIVE-SCHEMA-UNSUPPORTED", Assert.Single(unsupported.Diagnostics).Code);
    }

    /// <summary>Checks this is the current-Core public consumer, not the standalone package-pin assertion.</summary>
    [Fact]
    public void Identity_CurrentCoreAndNoFriendAccess_AreExplicit()
    {
        Assert.Equal(new Version(1, 81, 17, 0), typeof(XdmExactDecimal).Assembly.GetName().Version);
        var name = typeof(NativeDecimalCoreConsumerTests).Assembly.GetName().Name;
        var friends = typeof(NativeDecimalSession).Assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false)
            .Cast<InternalsVisibleToAttribute>();
        Assert.DoesNotContain(friends, friend => friend.AssemblyName == name);
        Assert.Empty(typeof(NativeDecimalSession).GetConstructors());
        Assert.Empty(typeof(NativeDecimalDocument).GetConstructors());
    }

    private static string Data(string leaf) => Path.Combine(AppContext.BaseDirectory, "TestData", "Schemas", leaf);
}
