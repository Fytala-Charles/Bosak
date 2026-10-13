// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify module fixed bindings survive local earlier XPath grammar profiles.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-10-2026     | Add linked module boundaries, lexical scanning and execution parity checks.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests module binding policy independently of a slot's XPath grammar version.</summary>
public class Xslt40LocalProfileNamespaceTests
{
    private static readonly Uri MainUri = new("file:///local-profile/main.xsl");
    private static string Sheet(string body, string? bindings = "#standard p=urn:fixed q=urn:fixed", string version = "4.0")
        => "<?xml version='1.0' encoding='UTF-8'?>\r\n<!--keep source-->\r\n"
            + "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' exclude-result-prefixes='#all'"
            + (bindings is null ? "" : " fixed-namespaces='" + bindings + "'") + ">" + body + "</xsl:stylesheet>";
    private static string Output(string version, string expression = "'old'")
        => "<xsl:template match='/' version='" + version + "'><out><xsl:value-of select=\"" + expression + "\"/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var inspected = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspected.IsSuccess);
        return inspected.Snapshot!;
    }
    private static AuthoringNodeDescriptor ValueOf(AuthoringSnapshot snapshot)
        => snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "value-of")!;
    private static void AssertValid(string text, string expected = "fixed", Resolver? resolver = null, string input = "<root xmlns:p='urn:fixed'><p:value>fixed</p:value></root>")
    {
        var snapshot = Inspect(text, resolver);
        var requests = resolver?.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver?.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        var compiler = new XsltCompiler { UriResolver = resolver is null ? null : new AuthoringModuleUriResolverBridge(resolver) };
        var output = compiler.Compile(text, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse(input)));
        Assert.Equal(expected, XElement.Parse(output).Value);
    }
    private static void AssertInvalid(string text, string code = "XPST0081")
    {
        var snapshot = Inspect(text);
        var result = XsltValidation.Validate(snapshot);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == code && d.ModuleUri == MainUri && d.Range is not null);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Local earlier grammar does not hide fixed-only expression prefixes.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void FixedOnlyExpressionPrefixIsValid(string version)
        => AssertValid(Sheet(Output(version, "string(/root/q:value)")));

    /// <summary>Local earlier grammar does not restore native prefixes omitted from the fixed set.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void OmittedNativeExpressionPrefixIsInvalid(string version)
        => AssertInvalid(Sheet(Output(version, "p:value"), "#standard"));

    /// <summary>AVT expression bindings are module-wide even under a local earlier version.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void FixedOnlyAvtPrefixIsValid(string version)
        => AssertValid(Sheet("<xsl:template match='/' version='" + version + "'><out value='{string(/root/q:value)}'>fixed</out></xsl:template>"));

    /// <summary>AVTs cannot restore omitted native prefixes via local version changes.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void OmittedNativeAvtPrefixIsInvalid(string version)
        => AssertInvalid(Sheet("<xsl:template match='/' version='" + version + "'><out value='{p:value}'/></xsl:template>", "#standard"));

    /// <summary>Patterns use fixed-only bindings while retaining earlier syntax.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void FixedOnlyPatternPrefixIsValid(string version)
        => AssertValid(Sheet("<xsl:template match='q:value' version='" + version + "'><out>fixed</out></xsl:template>"), input: "<value xmlns='urn:fixed'/>");

    /// <summary>Pattern prefix validation rejects native bindings excluded by the module fixed set.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void OmittedNativePatternPrefixIsInvalid(string version)
        => AssertInvalid(Sheet("<xsl:template match='p:value' version='" + version + "'><out/></xsl:template>", "#standard"));

    /// <summary>Fixed bindings override a native prefix at a locally earlier instruction.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void LocalRebindingDoesNotReplaceModuleBindings(string version)
        => AssertValid(Sheet(Output(version, "string(/root/p:value)").Replace("<xsl:value-of", "<xsl:value-of xmlns:p='urn:local'")));

    /// <summary>Fixed namespaces never enable keyword grammar in a locally earlier expression.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierGrammarRemainsFrozen(string version)
        => AssertInvalid(Sheet(Output(version, "substring(value := 'XA', start := 2)")), "XPST0003");

    /// <summary>Expression edits use module fixed bindings without changing bytes outside the value.</summary>
    [Fact]
    public void EditedEarlierSlotExecutesWithFixedBindings()
    {
        var text = Sheet(Output("3.0"));
        var bytes = Encoding.UTF8.GetBytes(text);
        var snapshot = Inspect(text);
        var node = ValueOf(snapshot);
        var originalContext = node.Attributes.Single(a => a.Name == "select").SlotContext!;
        Assert.Equal("3.0", originalContext.EffectiveVersion);
        Assert.Contains(originalContext.InScopeNamespaces, b => b.Prefix == "p" && b.Uri == "urn:native");
        var edit = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id, "select", "string(/root/q:value)"));
        Assert.True(edit.IsSuccess, edit.Failure?.ToString());
        var candidate = edit.Candidate!;
        var range = Assert.Single(candidate.AffectedRanges);
        var start = checked((int)range.StartByteOffset);
        var end = checked((int)(range.StartByteOffset + range.ByteLength));
        Assert.True(bytes.AsSpan(0, start).SequenceEqual(candidate.EmittedSource.Span[..start]));
        Assert.True(bytes.AsSpan(end).SequenceEqual(candidate.EmittedSource.Span[^(bytes.Length - end)..]));
        Assert.Equal(bytes, snapshot.ExportOriginal(MainUri));
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(candidate.Snapshot).Outcome);
        var output = candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse("<root xmlns:q='urn:fixed'><q:value>fixed</q:value></root>")));
        Assert.Equal("fixed", XElement.Parse(output).Value);
    }

    /// <summary>Unavailable prefixes in earlier-profile edits still report start-tag ownership.</summary>
    [Fact]
    public void OmittedNativeEditPrefixRequiresParentChange()
    {
        var snapshot = Inspect(Sheet(Output("3.0"), "#standard"));
        var node = ValueOf(snapshot);
        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id, "select", "p:value"));
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.RequiresParentChange, result.Failure!.Kind);
        Assert.Contains("XPST0081", result.Failure.Message, StringComparison.Ordinal);
        Assert.Equal(node.Attributes.Single(a => a.Name == "select").ValueRange, result.Failure.Range);
        Assert.Equal(node.Range.StartByteOffset, result.Failure.ExpandedRange!.StartByteOffset);
    }

    /// <summary>Captured namespace-document refusal applies to locally earlier slots without retries.</summary>
    /// <param name="version">The local earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void RefusedNamespaceDocumentAppliesToEarlierSlots(string version)
    {
        var resolver = new Resolver(new());
        var text = Sheet(Output(version), "#standard missing.xml");
        var snapshot = Inspect(text, resolver);
        Assert.Equal(1, resolver.Requests);
        for (var i = 0; i < 2; i++)
            Assert.Contains(XsltValidation.Validate(snapshot).Diagnostics, d => d.Code == "XTSE0122");
        var edit = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(ValueOf(snapshot).Id, "select", "'new'"));
        Assert.False(edit.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.ExpressionParseError, edit.Failure!.Kind);
        Assert.Contains("XTSE0122", edit.Failure.Message, StringComparison.Ordinal);
        Assert.Null(edit.Failure.ExpandedRange);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Successful namespace-document capture supports lower grammar without repeated validation requests.</summary>
    [Fact]
    public void NamespaceDocumentBindingsApplyToEarlierSlot()
        => AssertValid(Sheet(Output("3.0", "string(/root/q:value)"), "#standard bindings.xml"), resolver: new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:q='urn:fixed'/>" }));

    /// <summary>Modules without fixed bindings retain native namespace behavior.</summary>
    [Fact]
    public void NoFixedSetRetainsNativeBindings()
        => AssertValid(Sheet(Output("3.0", "string(/root/p:value)"), null), "native", input: "<root xmlns:p='urn:native'><p:value>native</p:value></root>");

    /// <summary>Earlier module roots keep native bindings when a local expression enables 4.0 grammar.</summary>
    /// <param name="version">The earlier module version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierModuleKeepsNativeBindings(string version)
        => AssertValid(Sheet(Output("4.0", "substring(value := string(/root/p:value), start := 1)"), null, version), "native", input: "<root xmlns:p='urn:native'><p:value>native</p:value></root>");

    /// <summary>Each linked module supplies its own fixed or native context.</summary>
    /// <param name="kind">The module linkage instruction.</param>
    /// <param name="moduleVersion">The linked module root version.</param>
    [Theory]
    [InlineData("include", "3.0")]
    [InlineData("import", "3.0")]
    [InlineData("include", "4.0")]
    [InlineData("import", "4.0")]
    public void LinkedModuleBindingsAreIndependent(string kind, string moduleVersion)
    {
        var linked = Sheet(Output("3.0", "string(/root/p:value)"), moduleVersion == "4.0" ? "#standard p=urn:module" : null, moduleVersion)
            .Replace("xmlns:p='urn:native'", "xmlns:p='urn:linked-native'");
        var resolver = new Resolver(new() { ["linked.xsl"] = linked });
        var uri = moduleVersion == "4.0" ? "urn:module" : "urn:linked-native";
        AssertValid(Sheet("<xsl:" + kind + " href='linked.xsl'/>", "#standard p=urn:principal"), resolver: resolver,
            input: "<root xmlns:p='" + uri + "'><p:value>fixed</p:value></root>");
    }

    /// <summary>Lexical prefix checks ignore comments and string literals.</summary>
    [Fact]
    public void CommentsAndStringsDoNotDeclarePrefixes()
        => AssertValid(Sheet(Output("3.0", "string(/root/q:value) || (: missing:ignored (: nested:ignored :) :) ':missing:name'")), "fixed:missing:name");

    /// <summary>A numeric map key colon is not a QName prefix.</summary>
    [Fact]
    public void NumericMapKeyDoesNotRequireBinding()
        => AssertValid(Sheet(Output("3.0", "map{1:'fixed'}?1")));

    /// <summary>Earlier-pattern predicates and wildcard name tests retain module bindings.</summary>
    /// <param name="pattern">The match pattern.</param>
    [Theory]
    [InlineData("q:value[string(.) = 'fixed']")]
    [InlineData("q:*[string(.) = 'fixed']")]
    public void EarlierPatternPredicatesUseFixedBindings(string pattern)
        => AssertValid(Sheet("<xsl:template version='3.0' match=\"" + pattern + "\"><out>fixed</out></xsl:template>"), input: "<value xmlns='urn:fixed'>fixed</value>");

    /// <summary>Prefixes inside path expressions passed to built-ins are checked before a splice.</summary>
    [Fact]
    public void NestedEditPrefixCannotEscapePreflight()
    {
        var snapshot = Inspect(Sheet(Output("3.0"), "#standard"));
        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(ValueOf(snapshot).Id, "select", "string(/root/p:value)"));
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.RequiresParentChange, result.Failure!.Kind);
        Assert.Contains("XPST0081", result.Failure.Message, StringComparison.Ordinal);
    }

    /// <summary>Execution rejects omitted native prefixes in lower-profile match patterns.</summary>
    [Fact]
    public void RuntimePatternRejectsOmittedNativePrefix()
    {
        var text = Sheet("<xsl:template version='3.0' match='p:value'><out/></xsl:template>", "#standard");
        var error = Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<value/>"))));
        Assert.Contains("XPST0081", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Literal-result-element versions select grammar without changing AVT namespace bindings.</summary>
    [Fact]
    public void LiteralResultVersionRetainsModuleBindings()
        => AssertValid(Sheet("<xsl:template match='/'><out xsl:version='3.0' value='{string(/root/q:value)}'>fixed</out></xsl:template>"));

    /// <summary>Fixed bindings do not change the captured default element namespace.</summary>
    [Fact]
    public void DefaultElementNamespaceIsPreserved()
        => AssertValid(Sheet(Output("3.0", "string(/*/value)")).Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:elements'"), input: "<root xmlns='urn:elements'><value>fixed</value></root>");

    /// <summary>Namespace capture retains the slot base URI independently of local grammar.</summary>
    [Fact]
    public void BaseUriIsPreserved()
        => AssertValid(Sheet(Output("3.0", "resolve-uri('child')")).Replace("<xsl:template", "<xsl:template xml:base='sub/'"), "file:///local-profile/sub/child");

    private sealed class Resolver(Dictionary<string, string> resources) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled resource requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!resources.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
