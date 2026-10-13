// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for declaration sequence types in authoring validation.
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
using System.Xml.Linq;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests declaration sequence types using captured static namespace and named-type bindings.</summary>
public class Xslt40AuthoringSequenceTypeTests
{
    private static readonly Uri MainUri = new("file:///sequence-types/main.xsl");
    private static string Sheet(string body) => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='#standard t=urn:types'>" + body + "</xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(string text)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }

    /// <summary>Declaration slots reject undeclared named types even when unused.</summary>
    /// <param name="body">The declaration with an unknown type.</param>
    [Theory]
    [InlineData("<xsl:variable name='v' as='t:missing' select='()'/>")]
    [InlineData("<xsl:param name='v' as='t:missing' select='()'/>")]
    [InlineData("<xsl:template name='target' as='t:missing'/>")]
    [InlineData("<xsl:function name='t:f' as='t:missing'><xsl:sequence select='()'/></xsl:function>")]
    [InlineData("<xsl:template match='/'><xsl:param name='v' as='t:missing' select='()'/></xsl:template>")]
    [InlineData("<xsl:template name='target'><xsl:param name='v'/></xsl:template><xsl:template match='/'><xsl:call-template name='target'><xsl:with-param name='v' as='t:missing' select='()'/></xsl:call-template></xsl:template>")]
    public void UnknownDeclarationTypeIsReported(string body)
    {
        var text = Sheet(body);
        var result = XsltValidation.Validate(Inspect(text));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0051" && d.Range is not null);
        Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Built-in, structural and named sequence types preserve occurrence indicators.</summary>
    /// <param name="type">The declaration type.</param>
    [Theory]
    [InlineData("xs:integer?")]
    [InlineData("t:integer*")]
    [InlineData("array(t:integer)?")]
    [InlineData("map(xs:string,t:integer)*")]
    [InlineData("function(t:integer) as xs:string*")]
    [InlineData("t:r?")]
    public void SupportedSequenceTypeIsAccepted(string type)
    {
        var text = Sheet("<xsl:item-type name='t:integer' as='xs:integer'/><xsl:record-type name='t:r'/><xsl:param name='v' as='" + type + "'/>");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Undeclared type prefixes use the captured namespace map.</summary>
    [Fact]
    public void OmittedPrefixIsRejected()
    {
        var text = Sheet("<xsl:param name='v' as='q:type'/>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0081");
    }

    /// <summary>Excluded declarations do not introduce type diagnostics.</summary>
    [Fact]
    public void ExcludedDeclarationIsIgnored()
    {
        var text = Sheet("<xsl:param name='v' as='t:missing' use-when='false()'/>");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
    }

    /// <summary>Plain attribute metadata and source bytes retain their public contract.</summary>
    [Fact]
    public void TypeMetadataRemainsPlain()
    {
        var text = Sheet("<xsl:param name='v' as='xs:integer?'/>");
        var snapshot = Inspect(text);
        var attribute = snapshot.PrincipalModule.Root.FindDescendant(n => n.Attributes.Any(a => a.LocalName == "as"))!.Attributes.Single(a => a.LocalName == "as");
        Assert.Equal(AuthoringAttributeSlotKind.Plain, attribute.SlotKind);
        Assert.Null(attribute.SlotContext);
        _ = XsltValidation.Validate(snapshot);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }
    /// <summary>Malformed or expression-shaped values are rejected as sequence-type syntax.</summary>
    /// <param name="type">The malformed declaration value.</param>
    [Theory]
    [InlineData("")]
    [InlineData("xs:integer??")]
    [InlineData("xs:integer or true()")]
    public void MalformedTypeIsRejected(string type)
    {
        var text = Sheet("<xsl:param name='v' as='" + type + "'/>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0003");
    }

    /// <summary>Earlier host profiles retain their existing declaration-type validation boundary.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesArePreserved(string version)
    {
        var text = Sheet("<xsl:param name='v' as='t:missing'/>").Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard t=urn:types'", " xmlns:t='urn:types'");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
    }

    /// <summary>Unprefixed names use the captured XPath default type namespace.</summary>
    [Fact]
    public void DefaultTypeNamespaceIsPreserved()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/><xsl:param name='v' as='a?'/>").Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:types'");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

}
