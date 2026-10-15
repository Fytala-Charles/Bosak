// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Tests abstract optional overrides and static original reference errors.
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
//                      | Charles Korthout | 0.2   | 15-10-2026     | Cover review fixes for expression slots, foreign attributes and text expansion.           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies abstract optional override dispatch and static original reference errors.</summary>
[Collection("XSLT package registry")]
public class Xslt40AbstractOptionalOverrideTests
{
    private const string OriginalParameters = "<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:param name='c' required='no' select=\"'C'\"/>";
    private const string OverrideParameters = "<xsl:param name='x'/><xsl:param name='y' required='no' select=\"'X'\"/><xsl:param name='z' required='no' select=\"'Y'\"/>";
    private static string Function(string parameters, string body) => "<xsl:function name='p:f' visibility='public'>" + parameters + "<xsl:sequence select=\"" + body + "\"/></xsl:function>";
    private static string Library(string parameters = OriginalParameters, string body = "$a || $b || $c", string extra = "") =>
        "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' name='urn:optional-override' package-version='1.0' version='4.0'>" + Function(parameters, body).Replace("visibility='public'", "visibility='abstract'").Replace("<xsl:sequence select=\"" + body + "\"/>", "") + extra + "</xsl:package>";
    private static string Caller(string call, string body = "$x || $y || $z", string parameters = OverrideParameters, string extra = "") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='4.0' exclude-result-prefixes='#all'><xsl:use-package name='urn:optional-override' package-version='1.0'><xsl:override>" + Function(parameters, body) + "</xsl:override></xsl:use-package>" + extra + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Run(string caller, string? library = null, string input = "<root><child/></root>", bool compileOnly = false)
    {
        XsltFunctionLibrary.RegisterPackage("urn:optional-override", "1.0", "file:///override/library.xsl");
        try
        {
            var executable = new XsltCompiler { UriResolver = new Resolver(library ?? Library()) }.Compile(caller, "file:///override/main.xsl");
            if (compileOnly) return "";
            return XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse(input)))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    private static string CompileOnly(string caller) => Run(caller, compileOnly: true);

    /// <summary>Concrete replacements supply every callable abstract arity.</summary>
    /// <param name="call">The call expression.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("p:f('A')", "AXY")]
    [InlineData("p:f('A','D')", "ADY")]
    [InlineData("p:f('A','D','E')", "ADE")]
    [InlineData("p:f(a := 'A', c := 'E')", "AXE")]
    [InlineData("(p:f#1)('A')", "AXY")]
    [InlineData("function-lookup(QName('urn:override','f'),1)('A')", "AXY")]
    public void ConcreteCallPaths(string call, string expected) => Assert.Equal(expected, Run(Caller(call)));

    /// <summary>Original references are static errors even in uncalled functions and dead branches.</summary>
    /// <param name="expression">The overriding expression.</param>
    [Theory]
    [InlineData("xsl:original($x)")]
    [InlineData("xsl:original#1")]
    [InlineData("xsl:original(?, 'D')")]
    [InlineData("if (false()) then xsl:original($x) else 'ok'")]
    [InlineData("function() { xsl:original($x) }")]
    [InlineData("Q{http://www.w3.org/1999/XSL/Transform}original#1")]
    [InlineData("`value {xsl:original#1}`")]
    [InlineData("$x => xsl:original()")]
    [InlineData("xsl:original (: comment :) ($x)")]
    public void OriginalReferencesAreStaticErrors(string expression) => Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(Caller("'unused'", expression))).Message);

    /// <summary>Original references in optional defaults are also rejected before invocation.</summary>
    [Fact]
    public void OriginalDefaultIsStaticError() => Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(Caller("'unused'", parameters: OverrideParameters.Replace("'X'", "xsl:original('A')")))).Message);

    /// <summary>Literal text and comments do not constitute component references.</summary>
    /// <param name="expression">The overriding expression.</param>
    /// <param name="expected">The result.</param>
    [Theory]
    [InlineData("'xsl:original#1'", "xsl:original#1")]
    [InlineData("(: xsl:original($x) :) 'ok'", "ok")]
    [InlineData("`xsl:original#1`", "xsl:original#1")]
    [InlineData("`{{xsl:original#1}}`", "{xsl:original#1}")]
    public void NonReferencesRemainValid(string expression, string expected) => Assert.Equal(expected, Run(Caller("p:f('A')", expression)));

    /// <summary>Abstract families retain signature compatibility checks.</summary>
    [Fact]
    public void IncompatibleTypesAreRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => CompileOnly(Caller("'unused'", parameters: OverrideParameters.Replace("name='x'", "name='x' as='xs:integer'")))) .Message);

    /// <summary>References in literal result AVTs and expanded text are checked statically.</summary>
    /// <param name="content">The overriding sequence constructor.</param>
    [Theory]
    [InlineData("<out value='{xsl:original#1}'/>")]
    [InlineData("<out xmlns:a='urn:attribute' a:value='{xsl:original#1}'/>")]
    [InlineData("<out xsl:expand-text='yes'>{xsl:original#1}</out>")]
    [InlineData("<xsl:if test='false()'><xsl:sequence select='xsl:original#1'/></xsl:if>")]
    [InlineData("<xsl:value-of select='1' separator='{xsl:original#1}'/>")]
    [InlineData("<xsl:element name='out' namespace='{xsl:original#1}'/>")]
    public void ConstructorReferencesAreStaticErrors(string content)
    {
        var caller = Caller("'unused'").Replace("<xsl:sequence select=\"$x || $y || $z\"/>", content);
        Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(caller)).Message);
    }

    /// <summary>Namespace aliases are resolved at the reference location.</summary>
    [Fact]
    public void NamespaceAliasIsRejected()
    {
        var caller = Caller("'unused'", "alias:original#1").Replace("version='4.0'", "xmlns:alias='http://www.w3.org/1999/XSL/Transform' version='4.0'");
        Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(caller)).Message);
    }

    /// <summary>Concrete defaults retain their principal package's private helper scope.</summary>
    [Fact]
    public void ReplacementDefaultUsesPrivateHelper()
    {
        var parameters = OverrideParameters.Replace("'X'", "p:helper()");
        const string helper = "<xsl:function name='p:helper' visibility='private'><xsl:sequence select=\"'LOCAL'\"/></xsl:function>";
        Assert.Equal("ALOCALY", Run(Caller("p:f('A')", parameters: parameters, extra: helper)));
    }

    /// <summary>Changing the minimum arity remains an incompatible signature.</summary>
    [Fact]
    public void IncompatibleMinimumArityIsRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => CompileOnly(Caller("'unused'", parameters: OverrideParameters.Replace("name='y' required='no' select=\"'X'\"", "name='y'")))).Message);

    /// <summary>Only the upper arity identifies the overridden component.</summary>
    [Fact]
    public void ReducedUpperBoundIsRejected() => Assert.Contains("XTSE3058", Assert.ThrowsAny<Exception>(() => CompileOnly(Caller("'unused'", parameters: OverrideParameters[..OverrideParameters.LastIndexOf("<xsl:param", StringComparison.Ordinal)]))).Message);

    /// <summary>Dynamic availability strings remain ordinary strings.</summary>
    [Fact]
    public void DynamicLookupDoesNotReferenceOriginal() => Assert.Equal("true false", Run(Caller("p:f('A')", "(empty(function-lookup(QName('http://www.w3.org/1999/XSL/Transform','original'),1)),function-available('xsl:original',1))")));

    /// <summary>The implementation's local parameter names do not become public keywords.</summary>
    [Fact]
    public void ImplementationKeywordIsRejected() => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Run(Caller("p:f(x := 'A')"))).Message);

    /// <summary>An abstract function without a replacement remains unresolved in a top-level stylesheet.</summary>
    [Fact]
    public void UnresolvedAbstractStillFails()
    {
        var caller = XDocument.Parse(Caller("'unused'"));
        caller.Descendants(XName.Get("override", "http://www.w3.org/1999/XSL/Transform")).Remove();
        caller.Descendants(XName.Get("use-package", "http://www.w3.org/1999/XSL/Transform")).Single().Add(
            XElement.Parse("<xsl:accept xmlns:xsl='http://www.w3.org/1999/XSL/Transform' component='function' names='p:f#3' visibility='abstract'/>"));
        Assert.Contains("XTSE3080", Assert.ThrowsAny<Exception>(() => CompileOnly(caller.ToString())).Message);
    }

    /// <summary>Literal result template escapes do not create function references.</summary>
    [Fact]
    public void EscapedAvtRemainsLiteral()
    {
        var caller = Caller("p:f('A')").Replace("<xsl:sequence select=\"$x || $y || $z\"/>", "<out value='{{xsl:original#1}}'>ok</out>");
        Assert.Equal("ok", Run(caller));
    }

    /// <summary>Calls originating in the library dispatch to the concrete replacement's scope.</summary>
    [Fact]
    public void UsedPackageCallUsesConcreteScope()
    {
        const string wrapper = "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"p:f('A')\"/></xsl:function>";
        const string helper = "<xsl:function name='p:helper' visibility='private'><xsl:sequence select=\"'LOCAL'\"/></xsl:function>";
        Assert.Equal("ALOCALY", Run(Caller("p:wrapper()", parameters: OverrideParameters.Replace("'X'", "p:helper()"), extra: helper), Library(extra: wrapper)));
    }

    /// <summary>Additional expression and AVT slots reject originals before invocation.</summary>
    /// <param name="content">The overriding constructor containing a reference.</param>
    [Theory]
    [InlineData("""<xsl:for-each-group select='1' group-by='.' collation="{xsl:original('A')}"><xsl:sequence select='.'/></xsl:for-each-group>""")]
    [InlineData("""<xsl:evaluate xpath="'1'" with-params="map{'p': xsl:original#1}"/>""")]
    [InlineData("""<xsl:evaluate xpath="'1'" context-item="xsl:original('A')"/>""")]
    [InlineData("""<xsl:evaluate xpath="'1'" namespace-context="xsl:original('A')"/>""")]
    [InlineData("""<xsl:if test='false()' then="xsl:original('A')"/>""")]
    public void AdditionalSlotsRejectOriginal(string content)
    {
        var caller = Caller("'unused'").Replace("<xsl:sequence select=\"$x || $y || $z\"/>", content);
        Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(caller)).Message);
    }

    /// <summary>Foreign annotations on instructions do not acquire XPath semantics.</summary>
    /// <param name="attribute">An annotation whose local name matches an XPath-bearing slot.</param>
    [Theory]
    [InlineData("select")]
    [InlineData("test")]
    [InlineData("with-params")]
    public void ForeignAnnotationsArePlainText(string attribute)
    {
        var caller = Caller("p:f('A')").Replace("<xsl:sequence select=\"$x || $y || $z\"/>",
            "<xsl:sequence xmlns:a='urn:annotation' a:" + attribute + "='xsl:original#1' select=\"'ok'\"/>");
        Assert.Equal("ok", Run(caller));
    }

    /// <summary>Whitespace normalization agrees with runtime text expansion.</summary>
    /// <param name="expand">The inherited boolean spelling.</param>
    [Theory]
    [InlineData(" yes ")]
    [InlineData(" true ")]
    [InlineData(" 1 ")]
    public void NormalizedExpandTextRejectsOriginal(string expand)
    {
        var caller = Caller("'unused'").Replace("<xsl:sequence select=\"$x || $y || $z\"/>",
            "<out xsl:expand-text='" + expand + "'>{xsl:original('A')}</out>");
        Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => CompileOnly(caller)).Message);
    }

    /// <summary>A normalized local false value disables inherited expansion.</summary>
    [Fact]
    public void LocalExpandTextFalseKeepsLiteralText()
    {
        var caller = Caller("p:f('A')").Replace("<xsl:sequence select=\"$x || $y || $z\"/>",
            "<out xsl:expand-text=' yes '><inner xsl:expand-text=' no '>{xsl:original('A')}</inner></out>");
        Assert.Equal("{xsl:original('A')}", Run(caller));
    }

    private sealed class Resolver(string library) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(library);
    }
}
