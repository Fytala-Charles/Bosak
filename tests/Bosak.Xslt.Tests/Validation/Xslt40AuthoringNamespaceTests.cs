// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify authoring validation uses applicable XSLT 4.0 static namespace bindings.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests applicable fixed namespace bindings through public authoring and validation APIs.</summary>
public class Xslt40AuthoringNamespaceTests
{
    private static readonly Uri MainUri = new("file:///authoring/main.xsl");
    private static string Sheet(string bindings, string body, string version = "4.0") =>
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='urn:native' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static XsltValidationResult Validate(string text, IAuthoringModuleResolver? resolver = null) =>
        XsltValidation.Validate(Encoding.UTF8.GetBytes(text), MainUri, resolver);

    /// <summary>Fixed-only prefixes are available in expression, AVT and pattern slots.</summary>
    /// <param name="body">The tested slot.</param>
    [Theory]
    [InlineData("<xsl:template match='/'><xsl:value-of select='p:item'/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out value='{p:item}'/></xsl:template>")]
    [InlineData("<xsl:template match='p:item'><out/></xsl:template>")]
    [InlineData("<xsl:template match='p:*'><out/></xsl:template>")]
    public void FixedOnlyPrefixIsValid(string body) =>
        Assert.Equal(XsltValidationOutcome.Valid, Validate(Sheet("#standard p=urn:fixed", body)).Outcome);

    /// <summary>Excluded native prefixes cannot enter the expression static context.</summary>
    /// <param name="body">The tested slot.</param>
    [Theory]
    [InlineData("<xsl:template match='/'><xsl:value-of select='n:item'/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out value='{n:item}'/></xsl:template>")]
    [InlineData("<xsl:template match='n:item'><out/></xsl:template>")]
    [InlineData("<xsl:template match='n:*'><out/></xsl:template>")]
    public void ExcludedNativePrefixIsInvalid(string body)
    {
        var result = Validate(Sheet("#standard", body));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0081");
    }

    /// <summary>Earlier profiles retain XML bindings even with a fixed-namespaces attribute.</summary>
    /// <param name="version">The earlier profile version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesKeepNativeBindings(string version) =>
        Assert.Equal(XsltValidationOutcome.Valid, Validate(Sheet("#standard", "<xsl:template match='n:item'><xsl:value-of select='n:item'/></xsl:template>", version)).Outcome);

    /// <summary>Invalid fixed bindings are classified as static errors without inspection throwing.</summary>
    /// <param name="bindings">The malformed fixed binding.</param>
    [Theory]
    [InlineData("p=")]
    [InlineData("xmlns=urn:wrong")]
    [InlineData("xml=urn:wrong")]
    public void InvalidFixedBindingsAreClassified(string bindings)
    {
        var result = Validate(Sheet(bindings, "<xsl:template match='/'><out value='{1}'/></xsl:template>"));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0122");
    }

    /// <summary>Namespace document resolution remains caller-controlled and snapshot validation does not resolve again.</summary>
    [Fact]
    public void NamespaceDocumentIsResolvedDuringInspectionOnly()
    {
        var resolver = new NamespaceResolver();
        var text = Sheet("#standard bindings.xml", "<xsl:template match='p:item'><xsl:value-of select='p:item'/></xsl:template>");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var inspected = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspected.IsSuccess);
        var requests = resolver.Requests;
        Assert.Equal(1, requests);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(inspected.Snapshot!).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), inspected.Snapshot!.ExportOriginal(MainUri));
    }


    /// <summary>Fixed bindings win over native rebinding during validation and execution.</summary>
    [Fact]
    public void FixedRebindingAgreesWithExecution()
    {
        var text = Sheet("#standard n=urn:fixed", "<xsl:template match='/'><out><xsl:value-of select='n:root'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        var transformer = new Api.XsltCompiler().Compile(text);
        var input = new Bosak.XPath.Providers.Xml.XDocumentNode(System.Xml.Linq.XDocument.Parse("<root xmlns='urn:fixed'>fixed</root>"));
        Assert.Equal("fixed", System.Xml.Linq.XElement.Parse(transformer.TransformToString(input)).Value);
    }

    /// <summary>Inspection keeps native XML descriptors while validation uses fixed bindings.</summary>
    [Fact]
    public void NativeDescriptorBindingsArePreserved()
    {
        var text = Sheet("#standard n=urn:fixed", "<xsl:template match='/'><xsl:value-of select='n:root'/></xsl:template>");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, null, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        var contexts = Flatten(result.Snapshot!.Modules[0].Root).SelectMany(n => n.Attributes).Select(a => a.SlotContext).Where(c => c is not null);
        Assert.All(contexts, c => Assert.Contains(c!.InScopeNamespaces, b => b.Prefix == "n" && b.Uri == "urn:native"));
        Assert.Equal(Encoding.UTF8.GetBytes(text), result.Snapshot.ExportOriginal(MainUri));
    }

    /// <summary>A local earlier version keeps its native namespace bindings.</summary>
    [Fact]
    public void LocalEarlierVersionKeepsNativeBindings() =>
        Assert.Equal(XsltValidationOutcome.Valid, Validate(Sheet("#standard", "<xsl:template match='/' version='3.0'><xsl:value-of select='n:root'/></xsl:template>")).Outcome);

    /// <summary>A refused namespace document is a static diagnostic with no resolver fallback.</summary>
    [Fact]
    public void NamespaceDocumentRefusalIsClassified()
    {
        var resolver = new RefusingResolver();
        var result = Validate(Sheet("#standard missing.xml", "<xsl:template match='/'><xsl:value-of select='1'/><out value='{2}'/></xsl:template>"), resolver);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0122");
        Assert.Equal(1, resolver.Requests);
    }

    private static IEnumerable<AuthoringNodeDescriptor> Flatten(AuthoringNodeDescriptor node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in Flatten(child)) yield return descendant;
    }

    private sealed class RefusingResolver : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of namespace document resolution requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            return null;
        }
    }

    private sealed class NamespaceResolver : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of namespace document resolution requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            Assert.Equal(new Uri("file:///authoring/bindings.xml"), absoluteUri);
            Assert.Equal(MainUri, referencingModuleUri);
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes("<bindings xmlns:p='urn:fixed'/>"), absoluteUri, out var source, out _));
            return source;
        }
    }
}

