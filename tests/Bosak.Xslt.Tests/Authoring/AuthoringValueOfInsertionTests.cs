// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Verify bounded value-of insertion, source fidelity, contextual validation and node provenance.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-10-2026     | Cover default inspection preprocessing without weakening source eligibility checks.      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Public-boundary fixtures for REQ-128 structural candidates.</summary>
public class AuthoringValueOfInsertionTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri ModuleUri = new("file:///C:/project/main.xsl");
    private static string Sheet(string body, string extra = "", string version = "3.0") =>
        $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}' {extra}><xsl:output omit-xml-declaration='yes' indent='no'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";

    private static AuthoringSnapshot Inspect(string text) => Inspect(new UTF8Encoding(false, true).GetBytes(text));
    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, ModuleUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Snapshot!;
    }

    private static AuthoringNodeDescriptor Find(AuthoringSnapshot snapshot, string local) =>
        snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == local)!;

    private static AuthoringValueOfInsertionCandidate Insert(AuthoringSnapshot snapshot, string local = "out", string expression = "'A'")
    {
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, local));
        Assert.True(target.IsSuccess, target.Failure?.ToString());
        var result = snapshot.ProposeValueOfInsertion(new(target.Target!, expression));
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Candidate!;
    }

    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root) =>
        new[] { root }.Concat(root.Children.SelectMany(Nodes));

    private static string Transform(AuthoringValueOfInsertionCandidate candidate) =>
        new XsltCompiler().Compile(Encoding.UTF8.GetString(candidate.ExportSource()), ModuleUri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));

    /// <summary>G1 proves exact XML output and a zero-length old range.</summary>
    [Fact]
    public void G1_LiteralResult_ProducesExactOutput()
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var candidate = Insert(snapshot);
        Assert.Equal("<out>A</out>", Transform(candidate));
        Assert.Equal(0, candidate.OriginalRange.ByteLength);
        Assert.Same(snapshot, candidate.InputSnapshot);
        Assert.True(candidate.Snapshot.IsCompilable);
        Assert.Equal(AuthoringAttributeSlotKind.Expression, candidate.SelectSlot.SlotKind);
    }

    /// <summary>G2 proves local variables preceding the append position remain in scope.</summary>
    [Fact]
    public void G2_Template_SeesPrecedingLocalVariable()
    {
        var snapshot = Inspect(Sheet("<xsl:variable name='v' select=\"'A'\"/>").Replace("<xsl:output", "<xsl:output method='text'"));
        Assert.Equal("A", Transform(Insert(snapshot, "template", "$v")));
    }

    /// <summary>Existing bytes, trivia, identities and input snapshots survive repeated independent proposals.</summary>
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void Splice_PreservesEverySurvivingByteAndNode(string newline)
    {
        var text = "<?xml version='1.0'?>" + newline + Sheet("<out data='&#65;'><!-- </out> -->" + newline +
            "<same/><same/><?opaque </out> ?>" + newline + "</out>", "xmlns:v='urn:opaque' v:note='untouched'");
        var snapshot = Inspect(text);
        var original = snapshot.ExportOriginal(ModuleUri);
        var candidate = Insert(snapshot);
        var emitted = candidate.ExportSource();
        var p = (int)candidate.OriginalRange.StartByteOffset;
        var d = (int)candidate.InsertedRange.ByteLength;
        Assert.Equal(original[..p], emitted[..p]);
        Assert.Equal(original[p..], emitted[(p + d)..]);
        Assert.Equal(original, snapshot.ExportOriginal(ModuleUri));
        var oldNodes = Nodes(snapshot.PrincipalModule.Root).ToArray();
        Assert.Equal(oldNodes.Length, candidate.NodeCorrespondence.Count);
        Assert.Equal(oldNodes.Length, candidate.NodeCorrespondence.Values.Distinct().Count());
        Assert.Equal(oldNodes.Length + 1, Nodes(candidate.Snapshot.PrincipalModule.Root).Count());
        foreach (var node in oldNodes)
        {
            var surviving = Nodes(candidate.Snapshot.PrincipalModule.Root).Single(n => n.Id == candidate.NodeCorrespondence[node.Id]);
            Assert.Equal(node.Kind, surviving.Kind);
            Assert.Equal(node.ElementName, surviving.ElementName);
            Assert.Equal(node.Text, surviving.Text);
        }
        Assert.Equal(candidate.ExportSource(), Insert(snapshot).ExportSource());
        emitted[0] ^= 1;
        Assert.NotEqual(emitted[0], candidate.ExportSource()[0]);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<int, int>)candidate.NodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<AuthoringNodeDescriptor>)candidate.Snapshot.PrincipalModule.Root.Children).Clear());
    }

    /// <summary>Foreign descriptors and targets cannot be replayed using coincidentally equal integer IDs.</summary>
    [Fact]
    public void ForeignSnapshot_RefusesDescriptorsAndTargets()
    {
        var first = Inspect(Sheet("<out></out>"));
        var second = Inspect(Sheet("<out></out>"));
        Assert.Equal(Find(first, "out").Id, Find(second, "out").Id);
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot, first.GetValueOfInsertionTarget(Find(second, "out")).Failure!.Kind);
        var target = first.GetValueOfInsertionTarget(Find(first, "out")).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot, second.ProposeValueOfInsertion(new(target, "'A'")).Failure!.Kind);
    }

    /// <summary>Unsupported parents and mixed constructors produce classified refusals.</summary>
    [Theory]
    [InlineData("<out/>", "out", AuthoringInsertionFailureKind.SelfClosingParent)]
    [InlineData("<out>text</out>", "out", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out>&#160;</out>", "out", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out><![CDATA[ ]]></out>", "out", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out xml:space='preserve'> </out>", "out", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<xsl:if test='true()'><out></out></xsl:if>", "out", AuthoringInsertionFailureKind.IneligibleParent)]
    [InlineData("<xsl:for-each select='.'><out></out></xsl:for-each>", "out", AuthoringInsertionFailureKind.IneligibleParent)]
    [InlineData("<xsl:try><out></out><xsl:catch/></xsl:try>", "out", AuthoringInsertionFailureKind.IneligibleParent)]
    public void IneligibleSource_IsRefused(string body, string parent, AuthoringInsertionFailureKind expected)
    {
        var snapshot = Inspect(Sheet(body));
        var result = snapshot.GetValueOfInsertionTarget(Find(snapshot, parent));
        Assert.Equal(expected, result.Failure!.Kind);
        Assert.Null(result.Target);
    }

    /// <summary>Invalid expressions return diagnostics without changing the original bytes.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1 +")]
    [InlineData("$missing")]
    [InlineData("unknown:missing()")]
    [InlineData("'\u0001'")]
    public void InvalidSelect_RefusesAtomically(string expression)
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var original = snapshot.ExportOriginal(ModuleUri);
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, expression));
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringInsertionFailureKind.InvalidExpression, result.Failure!.Kind);
        Assert.Equal(original, snapshot.ExportOriginal(ModuleUri));
    }

    /// <summary>XML escaping preserves the exact expanded expression, braces and non-BMP characters.</summary>
    [Theory]
    [InlineData("'&<>\"\t\n\r😀'")]
    [InlineData("map{'key': 'value'}?key")]
    [InlineData("Q{http://www.w3.org/2005/xpath-functions}concat('a', 'b')")]
    public void ExpandedSelect_RoundTripsWithoutAvtEscaping(string expression)
    {
        var candidate = Insert(Inspect(Sheet("<out></out>", "expand-text='yes'")), expression: expression);
        Assert.Equal(expression, candidate.SelectSlot.ExpandedValue);
        Assert.Empty(candidate.InsertedNode.Children);
        Assert.DoesNotContain("{{", candidate.SelectSlot.RawLiteral, StringComparison.Ordinal);
    }

    /// <summary>UTF-8 and BOM-marked UTF-16 preserve exact prefix, suffix and byte coordinates.</summary>
    [Theory]
    [InlineData(65001, false)]
    [InlineData(65001, true)]
    [InlineData(1200, true)]
    [InlineData(1201, true)]
    public void SupportedEncoding_PreservesBomAndCoordinates(int codePage, bool bom)
    {
        Encoding encoding = codePage switch { 1200 => new UnicodeEncoding(false, true, true), 1201 => new UnicodeEncoding(true, true, true), _ => new UTF8Encoding(bom, true) };
        var declaration = codePage == 65001 ? "UTF-8" : "UTF-16";
        var text = $"<?xml version='1.0' encoding='{declaration}'?>\r\n" + Sheet("<!--😀--><out>\r\n</out>");
        var bytes = (bom ? encoding.GetPreamble() : Array.Empty<byte>()).Concat(encoding.GetBytes(text)).ToArray();
        var candidate = Insert(Inspect(bytes));
        var emitted = candidate.ExportSource();
        var p = (int)candidate.OriginalRange.StartByteOffset;
        var d = (int)candidate.InsertedRange.ByteLength;
        Assert.Equal(bytes[..p], emitted[..p]);
        Assert.Equal(bytes[p..], emitted[(p + d)..]);
        Assert.Equal("'A'", candidate.SelectSlot.ExpandedValue);
        Assert.Equal(3, candidate.OriginalRange.StartLine);
    }

    /// <summary>Mismatched declarations and encodings outside the insertion profile are refused.</summary>
    [Theory]
    [InlineData(65001, "US-ASCII")]
    [InlineData(1200, "UTF-8")]
    [InlineData(1201, "UTF-16LE")]
    [InlineData(20127, "US-ASCII")]
    [InlineData(28591, "ISO-8859-1")]
    public void UnsupportedEncoding_IsClassified(int codePage, string declaration)
    {
        var encoding = Encoding.GetEncoding(codePage);
        var text = $"<?xml version='1.0' encoding='{declaration}'?>" + Sheet("<out></out>");
        var snapshot = Inspect(encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray());
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedEncoding,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
    }

    /// <summary>A locally rebound prefix is never reused; the first free local binding is inserted.</summary>
    [Fact]
    public void PrefixSelection_UsesEffectiveBindingsAndFirstFreePrefix()
    {
        var text = $"<stylesheet xmlns='{Xsl}' xmlns:xsl='urn:other' xmlns:xsl1='urn:other1' version='3.0'><template match='/'><out xmlns=''></out></template></stylesheet>";
        var candidate = Insert(Inspect(text));
        Assert.Equal("xsl2", candidate.InsertedNode.ElementName!.Prefix);
        Assert.Contains($"xmlns:xsl2=\"{Xsl}\"", Encoding.UTF8.GetString(candidate.ExportSource()), StringComparison.Ordinal);
    }

    /// <summary>Namespaces, XPath defaults and xml:base are captured at the actual new select slot.</summary>
    [Fact]
    public void Context_IsCapturedFromProspectiveSlot()
    {
        var candidate = Insert(Inspect(Sheet("<outer><out xml:base='nested/'></out></outer>", "xmlns:p='urn:p' xpath-default-namespace='urn:default'")),
            expression: "p:item | item");
        Assert.Equal("urn:default", candidate.SelectSlot.SlotContext!.XpathDefaultNamespace);
        Assert.Equal(new Uri("file:///C:/project/nested/"), candidate.SelectSlot.SlotContext.BaseUri);
        Assert.Contains(candidate.SelectSlot.SlotContext.InScopeNamespaces, n => n.Prefix == "p" && n.Uri == "urn:p");
    }

    /// <summary>Static preprocessing is refused before a resource resolver or user expression can execute.</summary>
    [Theory]
    [InlineData("use-when=\"doc('file:///C:/must-not-read.xml')\"")]
    [InlineData("_select=\"{unparsed-text('https://example.invalid/must-not-fetch')}\"")]
    public void StaticEvaluation_IsOutsideCoverage(string attribute)
    {
        var snapshot = Inspect(Sheet($"<xsl:variable name='v' {attribute}/><out></out>"));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedCoverage,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
    }

    /// <summary>Pre-cancelled discovery and cancelled proposals never return handles or candidates.</summary>
    [Fact]
    public void Cancellation_ReturnsClassifiedRefusal()
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var parent = Find(snapshot, "out");
        var target = snapshot.GetValueOfInsertionTarget(parent).Target!;
        var token = new CancellationToken(true);
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled, snapshot.GetValueOfInsertionTarget(parent, token).Failure!.Kind);
        var result = snapshot.ProposeValueOfInsertion(new(target, "'A'"), token);
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled, result.Failure!.Kind);
        Assert.Null(result.Candidate);
    }

    /// <summary>Source and escaped-candidate limits are measured in encoded bytes including BOM.</summary>
    [Fact]
    public void SizeLimits_RejectOversizedOriginalAndCandidate()
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, "'" + new string('&', 1_700_000) + "'"));
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit, result.Failure!.Kind);
        var oversized = Inspect(Sheet("<!--" + new string('a', AuthoringCapabilities.MaxValueOfInsertionSourceBytes) + "--><out></out>"));
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit, oversized.GetValueOfInsertionTarget(Find(oversized, "out")).Failure!.Kind);
    }

    /// <summary>The shipped capability advertises separate insertion limits and immutable encodings.</summary>
    [Fact]
    public void Capability_IsExplicitAndBounded()
    {
        Assert.True(AuthoringCapabilities.SupportsValueOfInsertion);
        Assert.Equal(8_388_608, AuthoringCapabilities.MaxValueOfInsertionSourceBytes);
        Assert.Equal(new[] { "UTF-8", "UTF-16LE", "UTF-16BE" }, AuthoringCapabilities.ValueOfInsertionSupportedEncodings);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)AuthoringCapabilities.ValueOfInsertionSupportedEncodings).Clear());
        Assert.Empty(typeof(AuthoringValueOfInsertionTarget).GetConstructors());
    }

    /// <summary>A supplementary source character must not be replaced during expression emission.</summary>
    [Fact]
    public void UnpairedSurrogate_IsRefused()
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, "'" + new string((char)0xd800, 1) + "'"));
        Assert.Equal(AuthoringInsertionFailureKind.InvalidExpression, result.Failure!.Kind);
    }

    /// <summary>All argument nulls are programmer errors, including when a token is already cancelled.</summary>
    [Fact]
    public void NullArguments_ThrowBeforeCancellation()
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        Assert.Throws<ArgumentNullException>(() => snapshot.GetValueOfInsertionTarget(null!, new(true)));
        Assert.Throws<ArgumentNullException>(() => snapshot.ProposeValueOfInsertion(null!, new(true)));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        Assert.Throws<ArgumentNullException>(() => new AuthoringValueOfInsertionProposal(null!, "'A'"));
        Assert.Throws<ArgumentNullException>(() => new AuthoringValueOfInsertionProposal(target, null!));
    }

    /// <summary>Existing static errors cannot be hidden by adding a valid instruction.</summary>
    [Theory]
    [InlineData("<xsl:value-of select='1 +'/><out></out>")]
    [InlineData("<xsl:value-of select='$missing'/><out></out>")]
    [InlineData("<xsl:value-of select='unknown:call()'/><out></out>")]
    [InlineData("<xsl:unknown/><out></out>")]
    public void InvalidOriginal_RefusesTarget(string body)
    {
        var snapshot = Inspect(Sheet(body));
        Assert.Equal(AuthoringInsertionFailureKind.InvalidOriginal,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
    }

    /// <summary>The constructor chain excludes stylesheet declarations and function bodies.</summary>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("output")]
    public void NonConstructorParent_IsRefused(string localName)
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        Assert.Equal(AuthoringInsertionFailureKind.IneligibleParent,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, localName)).Failure!.Kind);
    }

    /// <summary>Dependency discovery never calls the original resolver while proposing an insertion.</summary>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    [InlineData("use-package")]
    [InlineData("import-schema")]
    public void Dependency_IsRefusedWithoutResolverFallback(string declaration)
    {
        var text = Sheet("<out></out>").Replace("<xsl:output", $"<xsl:{declaration} href='other.xsl' name='urn:package' namespace='urn:schema'/><xsl:output");
        var resolver = new CountingResolver();
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), ModuleUri, out var source, out _));
        var snapshot = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation = false }).Snapshot!;
        var calls = resolver.Calls;
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedModule,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
        Assert.Equal(calls, resolver.Calls);
    }

    /// <summary>Unsupported effective versions and simplified stylesheets are refused.</summary>
    [Theory]
    [InlineData("2.0")]
    [InlineData("1.0")]
    [InlineData("5.0")]
    public void UnsupportedProfile_IsRefused(string version)
    {
        var snapshot = Inspect(Sheet("<out></out>", version: version));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedProfile,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
    }

    /// <summary>Both supported versions compile the new select in their own grammar.</summary>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void SupportedVersions_CaptureActualGrammar(string version)
    {
        var candidate = Insert(Inspect(Sheet("<out></out>", version: version)));
        Assert.Equal(version, candidate.SelectSlot.SlotContext!.EffectiveVersion);
    }

    /// <summary>Only a local reset of xml:space permits insertion beneath a preserving ancestor.</summary>
    [Fact]
    public void XmlSpace_DefaultResetRestoresEligibility()
    {
        var candidate = Insert(Inspect(Sheet("<outer xml:space='preserve'><out xml:space='default'> </out></outer>")));
        Assert.Equal("'A'", candidate.SelectSlot.ExpandedValue);
    }

    /// <summary>Existing XSLT aliases use ordinal order when xsl itself is rebound.</summary>
    [Fact]
    public void PrefixSelection_PrefersExistingAliasInOrdinalOrder()
    {
        var snapshot = Inspect($"<z:stylesheet xmlns:z='{Xsl}' xmlns:a='{Xsl}' xmlns:xsl='urn:other' version='3.0'><z:template match='/'><out></out></z:template></z:stylesheet>");
        Assert.Equal("a", Insert(snapshot).InsertedNode.ElementName!.Prefix);
    }

    /// <summary>Global declarations, preceding parameters and local function bodies retain static availability.</summary>
    [Fact]
    public void Scope_SeesGlobalParameterAndStylesheetFunction()
    {
        var text = Sheet("<xsl:param name='p' select=\"'P'\"/><out></out>", "xmlns:f='urn:f' exclude-result-prefixes='f'")
            .Replace("<xsl:output", "<xsl:variable name='g' select=\"'G'\"/><xsl:function name='f:join'><xsl:param name='a'/><xsl:param name='b'/><xsl:sequence select='concat($a,$b)'/></xsl:function><xsl:output");
        Assert.Equal("<out>GP</out>", Transform(Insert(Inspect(text), expression: "f:join($g,$p)")));
    }

    /// <summary>A function with no proven signature cannot produce an accepted candidate.</summary>
    [Theory]
    [InlineData("fn:does-not-exist()")]
    [InlineData("Q{urn:unknown}call()")]
    [InlineData("concat()")]
    public void UnprovedFunction_IsExplicitlyRefused(string expression)
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, expression));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Failure!.Kind, new[] { AuthoringInsertionFailureKind.InvalidExpression, AuthoringInsertionFailureKind.UnsupportedCoverage });
    }

    /// <summary>Caller changes to original inspection options cannot re-enable implicit candidate compilation.</summary>
    [Fact]
    public void InspectionOptions_AreNotReusedForCandidateInspection()
    {
        var options = new AuthoringInspectionOptions { AttemptCompilation = false };
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(Sheet("<out></out>")), ModuleUri, out var source, out _));
        var snapshot = XsltAuthoring.Inspect(source!, options: options).Snapshot!;
        options.AttemptCompilation = true;
        options.MaxModuleCount = 0;
        Assert.True(Insert(snapshot).Snapshot.IsCompilable);
        Assert.False(snapshot.IsCompilable);
    }

    /// <summary>The exact eight-MiB candidate boundary is accepted, while one additional byte is refused.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CandidateByteBoundary_IsInclusive(int extraByte)
    {
        var small = Sheet("<!----><out></out>");
        var smallCandidate = Insert(Inspect(small));
        var padding = AuthoringCapabilities.MaxValueOfInsertionSourceBytes - smallCandidate.ExportSource().Length + extraByte;
        var snapshot = Inspect(Sheet("<!--" + new string('a', padding) + "--><out></out>"));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, "'A'"));
        if (extraByte == 0)
        {
            Assert.True(result.IsSuccess, result.Failure?.ToString());
            Assert.Equal(AuthoringCapabilities.MaxValueOfInsertionSourceBytes, result.Candidate!.ExportSource().Length);
        }
        else Assert.Equal(AuthoringInsertionFailureKind.SizeLimit, result.Failure!.Kind);
    }

    /// <summary>Cancellation during substantial processing leaves the original envelope intact.</summary>
    [Fact]
    public async Task Cancellation_DuringProcessingReturnsNoTarget()
    {
        var snapshot = Inspect(Sheet("<!--" + new string('a', 7_000_000) + "--><out></out>"));
        var original = snapshot.ExportOriginal(ModuleUri);
        using var cancellation = new CancellationTokenSource();
        var pending = Task.Run(() => snapshot.GetValueOfInsertionTarget(Find(snapshot, "out"), cancellation.Token));
        cancellation.CancelAfter(1);
        var result = await pending;
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled, result.Failure!.Kind);
        Assert.Null(result.Target);
        Assert.Equal(original, snapshot.ExportOriginal(ModuleUri));
    }

    /// <summary>Unproven original text templates, components and type contexts cannot masquerade as valid.</summary>
    [Theory]
    [InlineData("<xsl:text expand-text='yes'>{unknown:missing()}</xsl:text><out></out>")]
    [InlineData("<xsl:call-template name='missing'/><out></out>")]
    [InlineData("<xsl:variable name='v' as='xs:missing' xmlns:xs='http://www.w3.org/2001/XMLSchema' select='1'/><out></out>")]
    public void UnprovedWholeStylesheetContext_IsRefused(string body)
    {
        var snapshot = Inspect(Sheet(body));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedCoverage,
            snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Failure!.Kind);
    }

    /// <summary>Type-dependent expression forms refuse until their complete static type proof is available.</summary>
    [Theory]
    [InlineData("1 instance of xs:missing")]
    [InlineData("'x' cast as xs:missing")]
    [InlineData("'x' castable as xs:missing")]
    [InlineData("1 treat as xs:missing")]
    public void UnprovedExpressionTypes_AreRefused(string expression)
    {
        var snapshot = Inspect(Sheet("<out></out>"));
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")).Target!;
        var result = snapshot.ProposeValueOfInsertion(new(target, expression));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Failure!.Kind, new[] { AuthoringInsertionFailureKind.UnsupportedCoverage, AuthoringInsertionFailureKind.InvalidExpression });
    }

    /// <summary>Earlier derived compilation cannot hide a static preprocessing construct in retained source.</summary>
    [Theory]
    [InlineData("use-when='true()'", "")]
    [InlineData("", "<xsl:param name='s' static='yes' select='1'/>")]
    [InlineData("", "<xsl:variable name='v' _select='{1}'/>")]
    public void PriorInspectionCompilation_DoesNotChangeSourceEligibility(string attribute, string declaration)
    {
        var text = Sheet($"<out {attribute}></out>").Replace("<xsl:output", declaration + "<xsl:output");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), ModuleUri, out var source, out _));
        var snapshot = XsltAuthoring.Inspect(source!).Snapshot!;
        Assert.True(snapshot.IsCompilable);
        var result = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out"));
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedCoverage, result.Failure!.Kind);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(ModuleUri));
    }

    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) { Calls++; return null; }
    }
}
