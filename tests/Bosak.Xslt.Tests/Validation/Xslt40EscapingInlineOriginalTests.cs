// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Tests lexical original binding in escaping inline function closures.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies optional override dispatch, defaults, signatures and original binding.</summary>
[Collection("XSLT package registry")]
public class Xslt40EscapingInlineOriginalTests
{
    private const string OriginalParameters = "<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:param name='c' required='no' select=\"'C'\"/>";
    private const string OverrideParameters = "<xsl:param name='x'/><xsl:param name='y' required='no' select=\"'X'\"/><xsl:param name='z' required='no' select=\"'Y'\"/>";
    private static string Function(string parameters, string body) => "<xsl:function name='p:f' visibility='public'>" + parameters + "<xsl:sequence select=\"" + body + "\"/></xsl:function>";
    private static string Library(string parameters = OriginalParameters, string body = "$a || $b || $c", string extra = "") =>
        "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' name='urn:optional-override' package-version='1.0' version='4.0'>" + Function(parameters, body) + extra + "</xsl:package>";
    private static string Caller(string call, string body = "$x || $y || $z", string parameters = OverrideParameters, string extra = "") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='4.0' exclude-result-prefixes='#all'><xsl:use-package name='urn:optional-override' package-version='1.0'><xsl:override>" + Function(parameters, body) + "</xsl:override></xsl:use-package>" + extra + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Run(string caller, string? library = null, string input = "<root><child/></root>", bool compileOnly = false)
    {
        XsltFunctionLibrary.RegisterPackage("urn:optional-override", "1.0", "file:///override/library.xsl");
        try
        {
            var executable = new XsltCompiler { UriResolver = new Resolver(library ?? Library()) }.Compile(caller, "file:///override/main.xsl");
            return compileOnly ? "" : XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse(input)))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Escaping inline bodies retain the lexical original declaration.</summary>
    /// <param name="body">The returned closure.</param>
    /// <param name="call">Invocation after the override returns.</param>
    /// <param name="expected">The original declaration result.</param>
    [Theory]
    [InlineData("function($v) { xsl:original($v) }", "let $f := p:f('unused') return $f('A')", "ABC")]
    [InlineData("function($v) { xsl:original($v,'D') }", "let $f := p:f('unused') return $f('A')", "ADC")]
    [InlineData("function($v) { xsl:original(a := $v, c := 'E') }", "let $f := p:f('unused') return $f('A')", "ABE")]
    [InlineData("function() { function($v) { xsl:original($v) } }", "let $outer := p:f('unused'), $f := $outer() return $f('A')", "ABC")]
    [InlineData("let $original := xsl:original#1 return function($v) { $original($v) }", "let $f := p:f('unused') return $f('A')", "ABC")]
    [InlineData("function($v) { $x || $v }", "let $f := p:f('CAPTURE') return $f('A')", "CAPTUREA")]
    public void EscapingClosure(string body, string call, string expected) => Assert.Equal(expected, Run(Caller(call, body)));

    /// <summary>A different active override cannot retarget a lexical original call.</summary>
    [Fact]
    public void DifferentOverrideCannotRetargetClosure()
    {
        var caller = Caller("p:g(p:f('capture'))", "function($v) { xsl:original($v) }").Replace("</xsl:override>", Function(OverrideParameters, "$x('A')").Replace("name='p:f'", "name='p:g'") + "</xsl:override>");
        var library = Library(extra: Function(OriginalParameters, "'WRONG'").Replace("name='p:f'", "name='p:g'"));
        Assert.Equal("ABC", Run(caller, library));
    }

    /// <summary>A direct original call remains the control.</summary>
    [Fact]
    public void DirectControl() => Assert.Equal("ABC", Run(Caller("p:f('A')", "xsl:original($x)")));

    /// <summary>Static references created inside an escaped body keep the original binding.</summary>
    /// <param name="body">The returned inline body.</param>
    /// <param name="call">The eventual invocation.</param>
    /// <param name="expected">The original result.</param>
    [Theory]
    [InlineData("function($v) { xsl:original($v,'D','E') }", "let $f := p:f('unused') return $f('A')", "ADE")]
    [InlineData("function($v) { $v => xsl:original(c := 'E') }", "let $f := p:f('unused') return $f('A')", "ABE")]
    [InlineData("function() { xsl:original#1 }", "let $maker := p:f('unused'), $f := $maker() return $f('A')", "ABC")]
    [InlineData("function() { xsl:original(?, 'D') }", "let $maker := p:f('unused'), $f := $maker() return $f('A')", "ADC")]
    public void EscapedBodyOriginalForms(string body, string call, string expected) => Assert.Equal(expected, Run(Caller(call, body)));

    /// <summary>Original defaults re-enter the declaring library's globals and private helpers.</summary>
    [Fact]
    public void EscapedOriginalDefaultsKeepDeclaringScope()
    {
        var parameters = OriginalParameters.Replace("select=\"'B'\"", "select='p:helper()'");
        const string libraryExtra = "<xsl:variable name='suffix' select=\"'LIB'\"/><xsl:function name='p:helper' visibility='private'><xsl:sequence select='$suffix'/></xsl:function>";
        Assert.Equal("ALIBC", Run(Caller("let $f := p:f('unused') return $f('A')", "function($v) { xsl:original($v) }", extra: "<xsl:variable name='suffix' select=\"'CALLER'\"/>"), Library(parameters, extra: libraryExtra)));
    }

    /// <summary>Lexical binding does not make the original dynamically discoverable.</summary>
    [Fact]
    public void OriginalLookupStaysHiddenInEscapedBody() => Assert.Equal("true", Run(Caller("let $f := p:f('unused') return $f()",
        "function() { empty(function-lookup(QName('http://www.w3.org/1999/XSL/Transform','original'),1)) and not(function-available('xsl:original',1)) }")));

    /// <summary>The active overriding function regains its own original after a closure returns.</summary>
    [Fact]
    public void CallingOverrideBindingIsRestored()
    {
        var caller = Caller("p:g(p:f('capture'))", "function($v) { xsl:original($v) }").Replace("</xsl:override>", Function(OverrideParameters, "($x('A'), xsl:original('A'))").Replace("name='p:f'", "name='p:g'") + "</xsl:override>");
        var library = Library(extra: Function(OriginalParameters, "'RIGHTG'").Replace("name='p:f'", "name='p:g'"));
        Assert.Equal("ABC RIGHTG", Run(caller, library));
    }

    /// <summary>Exceptions also restore the active override's original binding.</summary>
    [Fact]
    public void CallingOverrideBindingIsRestoredAfterError()
    {
        var g = Function(OverrideParameters, "$x('A')").Replace("name='p:f'", "name='p:g'").Replace("<xsl:sequence select=\"$x('A')\"/>", "<xsl:try><xsl:sequence select=\"$x('A')\"/><xsl:catch><xsl:sequence select=\"xsl:original('A')\"/></xsl:catch></xsl:try>");
        var caller = Caller("p:g(p:f('capture'))", "function($v) { error() }").Replace("</xsl:override>", g + "</xsl:override>");
        var library = Library(extra: Function(OriginalParameters, "'RIGHTG'").Replace("name='p:f'", "name='p:g'"));
        Assert.Equal("RIGHTG", Run(caller, library));
    }

    /// <summary>Inline-body focus remains absent when the original evaluates a default.</summary>
    [Fact]
    public void EscapedOriginalDoesNotCaptureInlineBodyFocus()
    {
        const string original = "<xsl:param name='a' required='no' select='name(.)'/>";
        const string replacement = "<xsl:param name='x' required='no' select=\"'unused'\"/>";
        Assert.Contains("XPDY0002", Assert.ThrowsAny<Exception>(() => Run(Caller("let $f := p:f() return $f()", "function() { xsl:original() }", replacement), Library(original, "$a"))).Message);
    }

    /// <summary>An abstract original remains a static error even inside a returned inline body.</summary>
    [Fact]
    public void AbstractOriginalInInlineBodyRemainsStaticError()
    {
        var library = Library().Replace("visibility='public'", "visibility='abstract'").Replace("<xsl:sequence select=\"$a || $b || $c\"/>", "");
        Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => Run(Caller("'unused'", "function($v) { xsl:original($v) }"), library, compileOnly: true)).Message);
    }

    private sealed class Resolver(string library) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(library);
    }
}
