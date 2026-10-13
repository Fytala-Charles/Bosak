// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named enumeration conversion at XSLT declaration boundaries.
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

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named enumeration conversion at XSLT declaration boundaries.</summary>
[Collection("XSLT package registry")]
public class Xslt40EnumerationBindingTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:statuses' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body) => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root status='ready'/>")));
    private const string Alias = "<xsl:item-type name='t:status' as=\"enum('ready','queued')\"/>";

    /// <summary>Accepts global/local members, empty optional values and multiple allowed members.</summary>
    [Fact]
    public void Variables_MembersAndCardinality()
    {
        var result = XElement.Parse(Run(Alias + """
            <xsl:variable name='global' as='t:status' select="'ready'"/>
            <xsl:template match='/'><xsl:variable name='states' as='t:status+' select="($global,'queued')"/>
              <xsl:variable name='optional' as='t:status?' select='()'/>
              <out states="{string-join($states,'|')}" empty='{empty($optional)}'/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "ready|queued", "true" }, result.Attributes().Select(a => a.Value));
    }

    /// <summary>Atomizes source attributes without admitting strings outside the enumeration.</summary>
    [Fact]
    public void Variables_SourceAttribute_IsConverted()
        => Assert.Equal("ready", XElement.Parse(Run(Alias + """
            <xsl:template match='/'><xsl:variable name='state' as='t:status' select='root/@status'/><out><xsl:value-of select='$state'/></out></xsl:template>
            """)).Value);

    /// <summary>Resolves declaration prefixes and default type namespaces without modifying member strings.</summary>
    [Fact]
    public void Variables_NamespaceAliasesAndLiteralMembers()
    {
        var output = XElement.Parse(Run("""
            <xsl:item-type name='t:status' as="enum('t:status','ready?*')"/>
            <xsl:template match='/' xmlns:v='urn:statuses' xpath-default-namespace='urn:statuses'>
              <xsl:variable name='a' as='v:status' select="'t:status'"/>
              <xsl:variable name='b' as='status' select="'ready?*'"/>
              <xsl:variable name='c' as='Q{urn:statuses}status' select='$a'/><out><xsl:value-of select="($a,$b,$c)" separator='|'/></out>
            </xsl:template>
            """));
        Assert.Equal("t:status|ready?*|t:status", output.Value);
    }

    /// <summary>Uses the same type for direct, named-reference and lookup calls and function results.</summary>
    [Fact]
    public void Function_BoundariesAndDynamicCalls()
    {
        var output = XElement.Parse(Run(Alias + """
            <xsl:function name='f:state' as='t:status'><xsl:param name='v' as='t:status'/><xsl:sequence select='$v'/></xsl:function>
            <xsl:template match='/'><xsl:variable name='call' select='f:state#1'/>
              <xsl:variable name='lookup' select="function-lookup(QName('urn:functions','f:state'),1)"/>
              <out><xsl:value-of select="(f:state('ready'),$call('queued'),$lookup('ready'))" separator='|'/></out>
            </xsl:template>
            """));
        Assert.Equal("ready|queued|ready", output.Value);
    }

    /// <summary>Preserves template parameter conversion and the select result error boundary.</summary>
    [Fact]
    public void TemplateParameterAndSelectResult()
    {
        var output = XElement.Parse(Run(Alias + """
            <xsl:template name='report' as='t:status'><xsl:param name='v' as='t:status'/><xsl:select as='t:status'>$v</xsl:select></xsl:template>
            <xsl:template match='/'><out><xsl:call-template name='report'><xsl:with-param name='v' select="'ready'"/></xsl:call-template></out></xsl:template>
            """));
        Assert.Equal("ready", output.Value);
    }

    /// <summary>Supports enumeration types for constructor fields and default values.</summary>
    [Fact]
    public void RecordField_NamedEnumerationAndDefault()
        => Assert.Equal("queued", XElement.Parse(Run(Alias + """
            <xsl:record-type name='t:record' constructor='yes'><xsl:field name='status' as='t:status' select="'queued'"/></xsl:record-type>
            <xsl:template match='/'><out><xsl:value-of select='t:record()?status'/></out></xsl:template>
            """)).Value);

    /// <summary>Rejects invalid membership or cardinality with the variable declaration error.</summary>
    /// <param name="value">The initializer expression.</param>
    [Theory]
    [InlineData("'lost'")]
    [InlineData("1")]
    [InlineData("()")]
    [InlineData("('ready','queued')")]
    public void Variable_InvalidValue_XTTE0570(string value)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run(Alias + "<xsl:template match='/'><xsl:variable name='state' as='t:status' select=\""+value+"\"/><out><xsl:value-of select='$state'/></out></xsl:template>"));
        Assert.Contains("XTTE0570", error.ToString());
    }

    /// <summary>Reports failures at each declaration boundary using its required error code.</summary>
    /// <param name="body">The declarations and initial template.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("<xsl:function name='f:test'><xsl:param name='v' as='t:status'/><xsl:sequence select='$v'/></xsl:function><xsl:template match='/'><xsl:sequence select=\"f:test('lost')\"/></xsl:template>", "XPTY0004")]
    [InlineData("<xsl:function name='f:test' as='t:status'><xsl:sequence select=\"'lost'\"/></xsl:function><xsl:template match='/'><xsl:sequence select='f:test()'/></xsl:template>", "XTTE0780")]
    [InlineData("<xsl:template name='test'><xsl:param name='v' as='t:status'/><xsl:sequence select='$v'/></xsl:template><xsl:template match='/'><xsl:call-template name='test'><xsl:with-param name='v' select=\"'lost'\"/></xsl:call-template></xsl:template>", "XTTE0590")]
    [InlineData("<xsl:template match='/'><xsl:select as='t:status'>'lost'</xsl:select></xsl:template>", "XPTY0004")]
    public void InvalidBoundary_ExactCode(string body, string code)
        => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Run(Alias+body)).ToString());

    /// <summary>Supports inline enumerations in 4.0 but preserves refusal in a mixed 3.0 declaration.</summary>
    [Fact]
    public void InlineEnumeration_AndVersionBoundary()
    {
        Assert.Equal("ready", XElement.Parse(Run("""
            <xsl:template match='/'><xsl:variable name='v' as="enum('ready')" select="'ready'"/><out><xsl:value-of select='$v'/></out></xsl:template>
            """)).Value);
        var error = Assert.ThrowsAny<Exception>(() => Run(Alias+"""
            <xsl:template match='/' version='3.0'><xsl:variable name='v' as='t:status' select="'ready'"/><out><xsl:value-of select='$v'/></out></xsl:template>
            """));
        Assert.Contains("XTTE0570", error.ToString());
    }

    private sealed class PackageResolver : Api.IXsltUriResolver
    {
        /// <summary>Resolves the fixture package.</summary>
        /// <param name="href">The requested location.</param>
        /// <param name="baseUri">The referring module base URI.</param>
        /// <returns>The package document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse("""
            <xsl:package name='urn:enum-package' package-version='1.0' version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:statuses' xmlns:f='urn:functions'>
              <xsl:item-type name='t:status' as="enum('package')"/>
              <xsl:function name='f:state' visibility='public' as='t:status'><xsl:param name='v' as='t:status'/><xsl:sequence select='$v'/></xsl:function>
            </xsl:package>
            """);
    }

    /// <summary>Uses private package aliases for public functions and restores the caller's same-named alias.</summary>
    [Fact]
    public void PackageAliases_AreIsolatedAndRestored()
    {
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage("urn:enum-package","1.0","urn:enum-package:1.0");
        try
        {
            var output = new Api.XsltCompiler { UriResolver = new PackageResolver() }.Compile(Wrap("""
                <xsl:use-package name='urn:enum-package' package-version='1.0'/>
                <xsl:item-type name='t:status' as="enum('caller')"/>
                <xsl:template match='/'><out><first><xsl:value-of select="f:state('package')"/></first>
                  <xsl:variable name='v' as='t:status' select="'caller'"/><second><xsl:value-of select='$v'/></second>
                </out></xsl:template>
                """)).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
            Assert.Equal(new[] { "package", "caller" }, XElement.Parse(output).Elements().Select(e => e.Value));
        }
        finally { Api.XsltFunctionLibrary.ClearPackages(); }
    }
}
