// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for function declarations and calls.
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
//                      | Charles Korthout | 0.2   | 14-10-2026     | Retain module fixed function identities across local earlier profiles.                     |
//                      | Charles Korthout | 0.3   | 14-10-2026     | Retain namespace-free function identity and host scope.                                  |
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests function QName bindings across compilation, calls and authoring diagnostics.</summary>
public class Xslt40FunctionQNameTests
{
    /// <summary>Local earlier declarations retain module identity for positional and keyword calls.</summary>
    /// <param name="name">The declared name.</param>
    /// <param name="call">The call expression.</param>
    /// <param name="version">The local function version.</param>
    [Theory]
    [InlineData("p:f", "q:f(7)", "3.0")]
    [InlineData("q:f", "p:f(7)", "2.0")]
    [InlineData("Q{urn:fixed}f", "q:f(7)", "3.0")]
    [InlineData("p:f", "Q{urn:fixed}f(7)", "2.0")]
    [InlineData("q:f#1", "q:f(7)", "3.0")]
    [InlineData("p:f", "q:f(q:v := 7)", "3.0")]
    [InlineData("q:f", "p:f(Q{urn:fixed}v := 7)", "2.0")]
    public void LocalEarlierFunctionNamesExecute(string name, string call, string version)
    {
        var text = Sheet("<xsl:function version='" + version + "' xmlns:p='urn:local' name='" + name + "'><xsl:param name='p:v'/><xsl:sequence select='$q:v'/></xsl:function><xsl:template match='/'><out><xsl:value-of select='" + call + "'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Local earlier declarations report module-bound QName errors even when unused.</summary>
    /// <param name="name">The invalid name.</param>
    /// <param name="bindings">The module bindings.</param>
    /// <param name="code">The expected static error.</param>
    [Theory]
    [InlineData("p:f", "#standard", "XTSE0280")]
    [InlineData("a:b:c", "#standard", "XTSE0020")]
    [InlineData("p:f", "#standard p=http://www.w3.org/2005/xpath-functions", "XTSE0080")]
    [InlineData("f", "#standard", "XTSE0740")]
    [InlineData("Q{}f", "#standard", "XTSE0740")]
    public void LocalEarlierInvalidNamesAreRejected(string name, string bindings, string code)
    {
        var text = Sheet("<xsl:function version='3.0' name='" + name + "'><xsl:sequence select='7'/></xsl:function>", bindings);
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == code);
        Assert.Contains(code, Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
    }

    private static string Sheet(string body, string bindings = "#standard p=urn:fixed q=urn:fixed") => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static XsltValidationResult Validate(string text) => XsltValidation.Validate(Encoding.UTF8.GetBytes(text), new Uri("file:///functions/main.xsl"));
    private static string Transform(string text) => XElement.Parse(new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Function names agree with fixed-only, rebound and explicit call prefixes.</summary>
    /// <param name="attribute">The declaration attribute.</param>
    /// <param name="name">The declared function name.</param>
    /// <param name="call">The called function name.</param>
    [Theory]
    [InlineData("name", "q:f", "q:f")]
    [InlineData("name", "p:f", "q:f")]
    [InlineData("name", "Q{urn:fixed}f", "q:f")]
    [InlineData("name", "p:f", "Q{urn:fixed}f")]
    [InlineData("name", "q:f#0", "q:f")]
    [InlineData("_name", "q:f", "q:f")]
    [InlineData("_name", "{'q:f'}", "q:f")]
    public void FixedFunctionNamesExecute(string attribute, string name, string call)
    {
        var text = Sheet("<xsl:function " + attribute + "=\"" + name + "\"><xsl:sequence select='7'/></xsl:function><xsl:template match='/'><out><xsl:value-of select='" + call + "()'/></out></xsl:template>");
        if (attribute == "name") Assert.DoesNotContain(Validate(text).Diagnostics, d => d.Code is "XTSE0280" or "XTSE0020");
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Unused invalid declarations receive the same static error in both entry points.</summary>
    /// <param name="name">The declared function name.</param>
    /// <param name="bindings">The applicable namespace bindings.</param>
    /// <param name="code">The expected static error.</param>
    [Theory]
    [InlineData("p:f", "#standard", "XTSE0280")]
    [InlineData("a:b:c", "#standard", "XTSE0020")]
    [InlineData("p:f", "#standard p=http://www.w3.org/2005/xpath-functions", "XTSE0080")]
    [InlineData("f", "#standard", "XTSE0740")]
    [InlineData("Q{}f", "#standard", "XTSE0740")]
    [InlineData("p:f#bad", "#standard p=urn:fixed", "XTSE0020")]
    [InlineData("p:f", "#standard p=http://www.w3.org/2005/xpath-functions/map", "XTSE0080")]
    [InlineData("p:f", "#standard p=http://www.w3.org/2005/xpath-functions/array", "XTSE0080")]
    [InlineData("p:f", "#standard p=http://www.w3.org/2005/xpath-functions/math", "XTSE0080")]
    public void InvalidFunctionNamesAreRejected(string name, string bindings, string code)
    {
        var text = Sheet("<xsl:function name='" + name + "'" + (code == "XTSE0740" ? " visibility='public'" : "") + "><xsl:sequence select='7'/></xsl:function>", bindings);
        Assert.Contains(code, Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == code);
    }

    /// <summary>Earlier host profiles preserve native function namespace identity.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRemainNative(string version)
    {
        var text = Sheet("<xsl:function name='p:f'><xsl:sequence select='7'/></xsl:function><xsl:template match='/'><out><xsl:value-of select='Q{urn:native}f()'/></out></xsl:template>", "#standard").Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard'", "");
        Assert.Equal("7", Transform(text));
    }

    /// <summary>Statically excluded declarations do not require their omitted prefix.</summary>
    [Fact]
    public void ExcludedFunctionIsIgnored()
    {
        var text = Sheet("<xsl:function name='p:f' use-when='false()'/><xsl:template match='/'><out>7</out></xsl:template>", "#standard");
        Assert.DoesNotContain(Validate(text).Diagnostics, d => d.Code == "XTSE0280");
        Assert.Equal("7", Transform(text));
    }
}
