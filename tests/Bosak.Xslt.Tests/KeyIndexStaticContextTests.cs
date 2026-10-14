// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Verify key match/use compilation in the declaring static context.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================



using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies key patterns and use expressions retain declaring static contexts.</summary>
public class KeyIndexStaticContextTests
{
    private static readonly Uri MainUri = new("file:///keys/main.xsl");
    private const string Input = "<root xmlns:i='urn:fixed' xmlns:n='urn:native'><i:item id='a' i:id='a'/><n:item id='a'/></root>";
    private static string Sheet(string key, string version = "4.0", string bindings = "#standard p=urn:fixed q=urn:fixed", string lookup = "'a'", string extra = "")
        => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' exclude-result-prefixes='#all' fixed-namespaces='" + bindings + "'>" + extra + "<xsl:key version='" + version + "' name='k' " + key + "<xsl:template match='/'><out><xsl:value-of select=\"count(key('k'," + lookup + "))\"/></out></xsl:template></xsl:stylesheet>";
    private static void AssertCount(string text, string expected = "1")
    {
        var original = Encoding.UTF8.GetBytes(text);
        var validation = XsltValidation.Validate(original, MainUri);
        Assert.Equal(XsltValidationOutcome.Valid, validation.Outcome);
        Assert.Equal(expected, XElement.Parse(new XsltCompiler().Compile(text, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse(Input)))).Value);
    }

    /// <summary>Pattern QNames, predicates and use expressions all receive module fixed bindings.</summary>
    /// <param name="key">The key attributes or sequence constructor.</param>
    /// <param name="version">The local grammar version.</param>
    [Theory]
    [InlineData("match='p:item' use='@id'/>", "4.0")]
    [InlineData("match='p:item' use='@id'/>", "3.0")]
    [InlineData("match='q:item' use='@p:id'/>", "4.0")]
    [InlineData("match='p:item' use='@q:id'/>", "3.0")]
    [InlineData("match='p:*' use='@id'/>", "4.0")]
    [InlineData("match='p:*' use='@id'/>", "3.0")]
    [InlineData("match='p:item[@q:id=&quot;a&quot;]' use='@id'/>", "4.0")]
    [InlineData("match='p:item[@q:id=&quot;a&quot;]' use='@id'/>", "3.0")]
    [InlineData("match='p:item'><xsl:sequence select='@q:id'/></xsl:key>", "4.0")]
    [InlineData("match='p:item'><xsl:sequence select='@q:id'/></xsl:key>", "3.0")]
    public void FixedContextsIndexExpectedNode(string key, string version) => AssertCount(Sheet(key, version));

    /// <summary>The key's own XML base controls use-expression URI resolution.</summary>
    [Fact]
    public void DeclarationBaseUriIsUsed()
        => AssertCount(Sheet("match='p:item' use='resolve-uri(&quot;file.txt&quot;)' xml:base='sub/'/>", lookup: "'file:///keys/sub/file.txt'"));

    /// <summary>Host function keyword metadata is available while compiling a key use expression.</summary>
    [Fact]
    public void UseExpressionReceivesFunctionKeywordSignature()
        => AssertCount(Sheet("match='p:item' use='q:f(q:value := @p:id)'/>", extra: "<xsl:function name='q:f'><xsl:param name='q:value'/><xsl:sequence select='$q:value'/></xsl:function>"));

    /// <summary>Earlier native modules still compile their own namespace bindings.</summary>
    [Fact]
    public void EarlierNativeContextRemainsNative()
    {
        var text = Sheet("match='p:item' use='@id'/>").Replace("version='4.0'", "version='3.0'").Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", "");
        AssertCount(text);
    }

    /// <summary>Pattern literals containing colons are preserved.</summary>
    [Fact]
    public void PatternLiteralIsNotRewritten()
        => AssertCount(Sheet("match='p:item[concat(@id, &quot;:p:item&quot;) = &quot;a:p:item&quot;]' use='@id'/>", "3.0"));

    /// <summary>The declaration's 4.0 grammar enables use-expression syntax independently of the caller.</summary>
    [Fact]
    public void UseGrammarFollowsDeclaration()
        => AssertCount(Sheet("match='p:item' use=\"@p:id ?? 'missing'\"/>")
            .Replace("<xsl:template match='/'", "<xsl:template version='3.0' match='/'"));

    /// <summary>A locally earlier declaration rejects 4.0 use-expression syntax.</summary>
    [Fact]
    public void EarlierDeclarationDoesNotBorrowCallerGrammar()
    {
        var text = Sheet("match='p:item' use=\"@p:id ?? 'missing'\"/>", "3.0");
        Assert.NotEqual(XsltValidationOutcome.Valid, XsltValidation.Validate(Encoding.UTF8.GetBytes(text), MainUri).Outcome);
        Assert.Contains("XPST", Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(text, MainUri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse(Input)))).ToString());
    }

    /// <summary>Excluded native prefixes cannot be borrowed while building the index.</summary>
    /// <param name="key">The invalid pattern or use expression.</param>
    /// <param name="version">The declaration profile.</param>
    [Theory]
    [InlineData("match='p:item' use='@id'/>", "4.0")]
    [InlineData("match='p:item' use='@id'/>", "3.0")]
    [InlineData("match='item' use='@p:id'/>", "4.0")]
    [InlineData("match='item' use='@p:id'/>", "3.0")]
    public void ExcludedPrefixesRemainStaticErrors(string key, string version)
    {
        var text = Sheet(key, version, "#standard");
        Assert.Contains(XsltValidation.Validate(Encoding.UTF8.GetBytes(text), MainUri).Diagnostics, d => d.Code == "XPST0081");
        Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(text, MainUri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse(Input)))).ToString());
    }

    /// <summary>Linked keys compile patterns and use expressions in their own module bindings.</summary>
    /// <param name="kind">The linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedDeclarationKeepsItsContext(string kind)
    {
        var module = "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='#standard p=urn:fixed'><xsl:key version='3.0' name='k' match='p:item' use='@p:id'/></xsl:stylesheet>";
        var main = Sheet("match='none' use='@id'/>", bindings: "#standard p=urn:caller", extra: "<xsl:" + kind + " href='keys.xsl'/>");
        var resolver = new Resolver(module);
        var executable = new XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(resolver) }.Compile(main, MainUri.AbsoluteUri);
        int requests = resolver.Requests;
        Assert.Equal("1", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse(Input)))).Value);
        Assert.Equal(requests, resolver.Requests);
    }

    private sealed class Resolver(string module) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of module requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(module), absoluteUri, out var source, out _));
            return source;
        }
    }
}
