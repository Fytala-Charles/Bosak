// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for variable declarations and parameter passing.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Correct local earlier-profile variable identity to module fixed bindings.                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests variable and parameter QName agreement across validation and execution.</summary>
public class Xslt40VariableQNameTests
{
    private static readonly Uri MainUri = new("file:///qnames/main.xsl");
    private static string Sheet(string body, string bindings = "#standard p=urn:fixed", string version = "4.0") =>
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static XsltValidationResult Validate(string text) => XsltValidation.Validate(Encoding.UTF8.GetBytes(text), MainUri);
    private static string Transform(string text) => XElement.Parse(new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Declaration names and XPath references share the applicable fixed namespace.</summary>
    /// <param name="body">The declaration and use site.</param>
    [Theory]
    [InlineData("<xsl:variable name='p:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:param name='p:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:variable name='p:v' select='7'/><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:param name='p:v' select='7'/><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param name='p:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target'><xsl:param name='p:v' select='0'/><xsl:value-of select='$p:v'/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:apply-templates select='/*'><xsl:with-param name='p:v' select='7'/></xsl:apply-templates></out></xsl:template><xsl:template match='root'><xsl:param name='p:v' select='0'/><xsl:value-of select='$p:v'/></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:param name='p:a' select='6'/><xsl:param name='p:v' select='$p:a + 1'/><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    public void FixedDeclarationsAgreeWithExecution(string body)
    {
        var text = Sheet(body);
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Fixed-only prefixes work without any matching XML namespace declaration.</summary>
    [Fact]
    public void FixedOnlyDeclarationPrefixWorks()
    {
        var text = Sheet("<xsl:template match='/'><xsl:variable name='q:v' select='7'/><out><xsl:value-of select='$q:v'/></out></xsl:template>", "#standard q=urn:fixed");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Excluded declaration prefixes report the QName static error even when unused.</summary>
    /// <param name="body">The declaration or supplied parameter.</param>
    [Theory]
    [InlineData("<xsl:variable name='p:v' select='7'/><xsl:template match='/'><out/></xsl:template>")]
    [InlineData("<xsl:param name='p:v' select='7'/><xsl:template match='/'><out/></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:variable name='p:v' select='7'/><out/></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:param name='p:v' select='7'/><out/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param name='p:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target'/>")]
    public void ExcludedUnusedDeclarationIsRejected(string body)
    {
        var text = Sheet(body, "#standard");
        var result = Validate(text);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0280");
        Assert.Contains("XTSE0280", Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
    }

    /// <summary>Explicit EQNames and unprefixed names do not inherit fixed prefix bindings.</summary>
    /// <param name="name">The declared name.</param>
    /// <param name="reference">The matching expression reference.</param>
    [Theory]
    [InlineData("Q{urn:explicit}v", "$Q{urn:explicit}v")]
    [InlineData("Q{}v", "$v")]
    [InlineData("v", "$v")]
    public void ExplicitAndUnprefixedNamesArePreserved(string name, string reference)
    {
        var text = Sheet("<xsl:variable name='" + name + "' select='7'/><xsl:template match='/'><out><xsl:value-of select='" + reference + "'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Earlier profiles keep native declaration binding.</summary>
    /// <param name="version">The host version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesKeepNativeNames(string version)
    {
        var text = Sheet("<xsl:variable name='p:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$p:v'/></out></xsl:template>", "#standard", version).Replace(" fixed-namespaces='#standard'", "");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Statically excluded invalid declarations do not enter scope or produce diagnostics.</summary>
    [Fact]
    public void ExcludedDeclarationIsNotChecked()
    {
        var text = Sheet("<xsl:variable name='p:v' select='7' use-when='false()'/><xsl:template match='/'><out>7</out></xsl:template>", "#standard");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }
    /// <summary>Different prefixes for the same fixed namespace identify the same parameter.</summary>
    [Fact]
    public void FixedAliasesPassTheSameParameter()
    {
        var text = Sheet("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param name='q:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target'><xsl:param name='p:v' select='0'/><xsl:value-of select='$p:v'/></xsl:template>", "#standard p=urn:fixed q=urn:fixed");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Parameter defaults cannot refer to a later fixed-name declaration.</summary>
    [Fact]
    public void ForwardParameterReferenceRemainsInvalid()
    {
        var text = Sheet("<xsl:template match='/'><xsl:param name='p:a' select='$p:b'/><xsl:param name='p:b' select='7'/><out/></xsl:template>");
        var result = Validate(text);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008");
    }

    /// <summary>Malformed declaration names report the same static syntax error as compilation.</summary>
    /// <param name="name">The malformed declaration name.</param>
    [Theory]
    [InlineData("p:a:b")]
    [InlineData("Q{urn:fixed}")]
    [InlineData("1bad")]
    public void MalformedNamesAreRejected(string name)
    {
        var text = Sheet("<xsl:template match='/'><xsl:variable name='" + name + "' select='7'/><out/></xsl:template>");
        var result = Validate(text);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0020");
        Assert.Contains("XTSE0020", Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
    }

    /// <summary>Local earlier host versions cannot restore an omitted native declaration prefix.</summary>
    [Fact]
    public void LocalEarlierHostVersionRetainsModuleFixedNames()
    {
        var text = Sheet("<xsl:template match='/' version='3.0'><xsl:variable name='p:v' select='7'/><out><xsl:value-of select='$Q{urn:native}v'/></out></xsl:template>", "#standard");
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == "XTSE0280");
        Assert.Contains("XTSE0280", Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
    }

    /// <summary>QName descriptors retain their public metadata contract and original bytes.</summary>
    [Fact]
    public void DeclarationDescriptorsKeepPublicContract()
    {
        var text = Sheet("<xsl:variable name='p:v' select='7'/><xsl:template match='/'><out/></xsl:template>");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, null, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        var variable = Flatten(result.Snapshot!.Modules[0].Root).Single(n => n.ElementName?.LocalName == "variable");
        var name = variable.Attributes.Single(a => a.LocalName == "name");
        Assert.Equal(AuthoringAttributeSlotKind.QName, name.SlotKind);
        Assert.Null(name.SlotContext);
        Assert.Equal("p:v", name.RawLiteral);
        Assert.Equal(Encoding.UTF8.GetBytes(text), result.Snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Declaration namespace documents resolve during inspection only, through the caller resolver.</summary>
    [Fact]
    public void NamespaceDocumentDeclarationsDoNotResolveDuringValidation()
    {
        var resolver = new NamespaceResolver();
        var text = Sheet("<xsl:variable name='q:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$q:v'/></out></xsl:template>", "#standard bindings.xml");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(result.Snapshot!).Outcome);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), result.Snapshot!.ExportOriginal(MainUri));
    }

    private static IEnumerable<AuthoringNodeDescriptor> Flatten(AuthoringNodeDescriptor node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in Flatten(child)) yield return descendant;
    }

    private sealed class NamespaceResolver : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of approved resolution requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            Assert.Equal(new Uri("file:///qnames/bindings.xml"), absoluteUri);
            Assert.Equal(MainUri, referencingModuleUri);
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes("<bindings xmlns:q='urn:fixed'/>"), absoluteUri, out var source, out _));
            return source;
        }
    }

}
