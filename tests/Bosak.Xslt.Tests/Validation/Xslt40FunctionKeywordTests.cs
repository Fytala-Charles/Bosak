// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Tests XSLT function keyword arguments and expanded parameter names.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-10-2026     | Add native declaration, linkage and earlier-profile boundaries.                            |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests keyword calls to XSLT functions with namespace-aware parameter matching.</summary>
public class Xslt40FunctionKeywordTests
{
    private static string Sheet(string call, string first = "p:a", string second = "p:b", string extra = "", string version = "4.0")
        => "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' xmlns:q='urn:fixed' exclude-result-prefixes='#all' fixed-namespaces='#standard p=urn:fixed q=urn:fixed'>"
        + "<xsl:function name='p:f'><xsl:param name='" + first + "'/><xsl:param name='" + second + "'/><xsl:sequence select=\"$" + first + " || '|' || $" + second + "\"/></xsl:function>"
        + "<xsl:template match='/'><out><xsl:value-of " + extra + " select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Transform(string text)
        => XElement.Parse(new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Parameter matching uses expanded identities and declaration order.</summary>
    /// <param name="call">The function call.</param>
    [Theory]
    [InlineData("p:f(p:b := 'B', p:a := 'A')")]
    [InlineData("q:f(q:b := 'B', q:a := 'A')")]
    [InlineData("Q{urn:fixed}f(Q{urn:fixed}b := 'B', Q{urn:fixed}a := 'A')")]
    [InlineData("p:f('A', q:b := 'B')")]
    [InlineData("'A' => p:f(q:b := 'B')")]
    [InlineData("p:f(p:a := concat('A', ''), p:b := upper-case('b'))")]
    [InlineData("declare namespace r = 'urn:fixed'; r:f(r:b := 'B', r:a := 'A')")]
    public void ExpandedParametersExecute(string call) => Assert.Equal("A|B", Transform(Sheet(call)));

    /// <summary>Unprefixed parameters remain in no namespace.</summary>
    [Fact]
    public void UnprefixedParametersExecute() => Assert.Equal("A|B", Transform(Sheet("p:f(b := 'B', a := 'A')", "a", "b")));

    /// <summary>Local XML rebinding cannot replace fixed keyword bindings.</summary>
    [Fact]
    public void LocalNativeRebindingIsIgnored() => Assert.Equal("A|B", Transform(Sheet("p:f(p:b := 'B', p:a := 'A')", extra: "xmlns:p='urn:local'")));

    /// <summary>Malformed argument combinations retain static XPST0017.</summary>
    /// <param name="call">The invalid call.</param>
    [Theory]
    [InlineData("p:f(a := 'A', b := 'B')")]
    [InlineData("p:f(Q{urn:other}a := 'A', p:b := 'B')")]
    [InlineData("p:f('A', p:a := 'B')")]
    [InlineData("p:f(p:a := 'A', q:a := 'B')")]
    [InlineData("p:f(p:a := 'A')")]
    [InlineData("p:f(p:a := 'A', p:b := 'B', p:c := 'C')")]
    [InlineData("p:f(?, p:b := 'B')")]
    [InlineData("'A' => p:f(p:a := 'B')")]
    public void InvalidCallsAreRejected(string call) => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Transform(Sheet(call))).Message);

    /// <summary>Built-in keyword defaults remain available beside host function signatures.</summary>
    [Fact]
    public void BuiltInDefaultsRemainAvailable() => Assert.Equal("A|B", Transform(Sheet("p:f(p:a := substring(value := 'XA', start := 2), p:b := 'B')")));

    /// <summary>Keyword expressions work inside AVTs and select bodies.</summary>
    /// <param name="body">The template body.</param>
    [Theory]
    [InlineData("<out value=\"{p:f(p:b := 'B', p:a := 'A')}\"/>")]
    [InlineData("<out><xsl:select>p:f(p:b := 'B', p:a := 'A')</xsl:select></out>")]
    public void OtherExpressionSlotsExecute(string body)
    {
        var text = Sheet("p:f('A','B')");
        var start = text.IndexOf("<out>", StringComparison.Ordinal);
        var end = text.IndexOf("</out>", start, StringComparison.Ordinal) + 6;
        text = text[..start] + body + text[end..];
        var output = XElement.Parse(new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Equal("A|B", output.Attribute("value")?.Value ?? output.Value);
    }

    /// <summary>Function overloads choose the declared arity before keyword matching.</summary>
    [Fact]
    public void OverloadsRemainDistinct()
    {
        var text = Sheet("p:f(p:b := 'B', p:a := p:f(p:single := 'A'))");
        text = text.Replace("<xsl:template", "<xsl:function name='p:f'><xsl:param name='p:single'/><xsl:sequence select='$p:single'/></xsl:function><xsl:template");
        Assert.Equal("A|B", Transform(text));
    }

    /// <summary>Native parameter bindings are captured on each parameter declaration.</summary>
    [Fact]
    public void ParameterLocalNamespacesAreUsed()
    {
        var text = Sheet("p:f(q:a := 'A', q:b := 'B')")
            .Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", "")
            .Replace("name='p:a'/>", "name='p:a' xmlns:p='urn:fixed'/>")
            .Replace("name='p:b'/>", "name='p:b' xmlns:p='urn:fixed'/>")
            .Replace("$p:a", "$q:a").Replace("$p:b", "$q:b");
        Assert.Equal("A|B", Transform(text));
    }

    /// <summary>Earlier profiles retain positional calls and reject keyword syntax.</summary>
    /// <param name="version">The earlier stylesheet version.</param>
    /// <param name="keywords">Whether to use keyword syntax.</param>
    [Theory]
    [InlineData("2.0", false)]
    [InlineData("3.0", false)]
    [InlineData("2.0", true)]
    [InlineData("3.0", true)]
    public void EarlierProfilesRemainUnchanged(string version, bool keywords)
    {
        var text = Sheet(keywords ? "p:f(p:a := 'A', p:b := 'B')" : "p:f('A','B')", version: version)
            .Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", "");
        if (keywords) Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => Transform(text)).Message);
        else Assert.Equal("A|B", Transform(text));
    }

    /// <summary>Included and imported declarations keep their own fixed parameter bindings.</summary>
    /// <param name="kind">The module linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedDeclarationsExecute(string kind)
    {
        var module = Sheet("p:f('A','B')").Replace("urn:fixed", "urn:module");
        module = module[..module.IndexOf("<xsl:template", StringComparison.Ordinal)] + "</xsl:stylesheet>";
        var main = Sheet("q:f(q:b := 'B', q:a := 'A')");
        var start = main.IndexOf("<xsl:function", StringComparison.Ordinal);
        var end = main.IndexOf("</xsl:function>", start, StringComparison.Ordinal) + 15;
        main = main[..start] + "<xsl:" + kind + " href='functions.xsl'/>" + main[end..];
        main = main.Replace("q=urn:fixed", "q=urn:module");
        var compiler = new Api.XsltCompiler { UriResolver = new ModuleResolver(module) };
        var output = compiler.Compile(main, "file:///keywords/main.xsl").TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("A|B", XElement.Parse(output).Value);
    }

    private sealed class ModuleResolver(string module) : Api.IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(module);
    }
}
