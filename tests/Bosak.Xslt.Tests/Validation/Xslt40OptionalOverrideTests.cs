// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Tests optional function overrides and bound original function items.
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
//                      | Charles Korthout | 0.2   | 15-10-2026     | Verify compatible abstract optional replacements.                                        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies optional override dispatch, defaults, signatures and original binding.</summary>
[Collection("XSLT package registry")]
public class Xslt40OptionalOverrideTests
{
    private const string OriginalParameters = "<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:param name='c' required='no' select=\"'C'\"/>";
    private const string OverrideParameters = "<xsl:param name='x'/><xsl:param name='y' required='no' select=\"'X'\"/><xsl:param name='z' required='no' select=\"'Y'\"/>";
    private static string Function(string parameters, string body) => "<xsl:function name='p:f' visibility='public'>" + parameters + "<xsl:sequence select=\"" + body + "\"/></xsl:function>";
    private static string Library(string parameters = OriginalParameters, string body = "$a || $b || $c", string extra = "") =>
        "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' name='urn:optional-override' package-version='1.0' version='4.0'>" + Function(parameters, body) + extra + "</xsl:package>";
    private static string Caller(string call, string body = "$x || $y || $z", string parameters = OverrideParameters, string extra = "") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:override' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='4.0' exclude-result-prefixes='#all'><xsl:use-package name='urn:optional-override' package-version='1.0'><xsl:override>" + Function(parameters, body) + "</xsl:override></xsl:use-package>" + extra + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Run(string caller, string? library = null, string input = "<root><child/></root>")
    {
        XsltFunctionLibrary.RegisterPackage("urn:optional-override", "1.0", "file:///override/library.xsl");
        try
        {
            var executable = new XsltCompiler { UriResolver = new Resolver(library ?? Library()) }.Compile(caller, "file:///override/main.xsl");
            return XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse(input)))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Every callable arity dispatches to the override and its defaults.</summary>
    /// <param name="call">The caller expression.</param>
    /// <param name="expected">The expected parameter values.</param>
    [Theory]
    [InlineData("p:f('A')", "AXY")]
    [InlineData("p:f('A','D')", "ADY")]
    [InlineData("p:f('A','D','E')", "ADE")]
    [InlineData("p:f(a := 'A', c := 'E')", "AXE")]
    [InlineData("'A' => p:f(c := 'E')", "AXE")]
    [InlineData("(p:f#1)('A')", "AXY")]
    [InlineData("function-lookup(QName('urn:override','f'),1)('A')", "AXY")]
    [InlineData("let $f := p:f(?, 'D') return $f('A')", "ADY")]
    public void OverrideCallPaths(string call, string expected) => Assert.Equal(expected, Run(Caller(call)));

    /// <summary>Original calls keep the original defaults and parameter names.</summary>
    /// <param name="body">The expression in the overriding body.</param>
    /// <param name="expected">The expected original result.</param>
    [Theory]
    [InlineData("xsl:original($x)", "ABC")]
    [InlineData("xsl:original($x,'D')", "ADC")]
    [InlineData("xsl:original(a := $x, c := 'E')", "ABE")]
    [InlineData("$x => xsl:original(c := 'E')", "ABE")]
    [InlineData("(xsl:original#1)($x)", "ABC")]
    [InlineData("let $f := xsl:original(?, 'D') return $f($x)", "ADC")]
    public void OriginalCallPaths(string body, string expected) => Assert.Equal(expected, Run(Caller("p:f('A')", body)));

    /// <summary>Original items retain their declaration after the override has returned.</summary>
    /// <param name="body">The original function item expression.</param>
    /// <param name="call">The expression invoking the escaped value.</param>
    /// <param name="expected">The expected bound original result.</param>
    [Theory]
    [InlineData("xsl:original#1", "let $f := p:f('unused') return $f('A')", "ABC")]
    [InlineData("xsl:original(?, 'D')", "let $f := p:f('unused') return $f('A')", "ADC")]
    [InlineData("xsl:original#1", "let $f := p:f('unused') return (namespace-uri-from-QName(function-name($f)),local-name-from-QName(function-name($f)),function-arity($f))", "urn:override f 1")]
    public void EscapingOriginalItems(string body, string call, string expected) => Assert.Equal(expected, Run(Caller(call, body)));

    /// <summary>An escaped original stays bound when invoked inside another overriding call.</summary>
    [Fact]
    public void OriginalItemDoesNotRebindToActiveOverride() => Assert.Equal("ABC", Run(Caller("p:f(p:f('capture'))", "if ($x instance of function(*)) then $x('A') else xsl:original#1")));

    /// <summary>The used package's own static calls see every overriding arity.</summary>
    [Fact]
    public void LibraryInternalCallsDispatchToOverride() => Assert.Equal("AXY ADY", Run(Caller("p:wrapper()"), Library(extra: "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"(p:f('A'),p:f('A','D'))\"/></xsl:function>")));

    /// <summary>Changing requiredness without changing upper arity is an incompatible signature.</summary>
    [Fact]
    public void DifferentMinimumArityIsRejected()
    {
        var parameters = OverrideParameters.Replace("name='y' required='no' select=\"'X'\"", "name='y'");
        Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => Run(Caller("p:f('A','D')", parameters: parameters))).Message);
    }

    /// <summary>Keyword callers use original public names rather than implementation local names.</summary>
    [Fact]
    public void ImplementationKeywordNameIsRejected() => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Run(Caller("p:f(x := 'A')"))).Message);

    /// <summary>The original pseudo-name stays absent from dynamic lookup.</summary>
    [Fact]
    public void OriginalIsHiddenFromDynamicLookup() => Assert.Equal("true true", Run(Caller("p:f('A')", "(empty(function-lookup(QName('http://www.w3.org/1999/XSL/Transform','original'),1)),not(function-available('xsl:original',1)))")));

    /// <summary>The original default and body keep their own package globals.</summary>
    [Fact]
    public void OriginalDefaultsRetainDeclaringGlobals()
    {
        var original = OriginalParameters.Replace("select=\"'B'\"", "select='$suffix'");
        Assert.Equal("ALIBC", Run(Caller("p:f('A')", "xsl:original($x)", extra: "<xsl:variable name='suffix' select=\"'CALLER'\"/>"), Library(original, extra: "<xsl:variable name='suffix' select=\"'LIB'\"/>")));
    }

    /// <summary>Default evaluation preserves the original function item's creation focus.</summary>
    [Fact]
    public void OriginalItemCapturesCreationFocus()
    {
        const string original = "<xsl:param name='a' required='no' select='name(.)'/>";
        const string replacement = "<xsl:param name='x' required='no' select=\"'unused'\"/>";
        Assert.Equal("root", Run(Caller("let $f := p:f(/root) return /root/child ! $f()", "$x ! xsl:original#0", replacement), Library(original, "$a")));
    }

    /// <summary>Direct original defaults use the focus of their actual call.</summary>
    [Fact]
    public void OriginalCallUsesLocalFocus()
    {
        const string original = "<xsl:param name='a' required='no' select='name(.)'/>";
        const string replacement = "<xsl:param name='x' required='no' select=\"'unused'\"/>";
        Assert.Equal("root", Run(Caller("p:f(/root)", "$x ! xsl:original()", replacement), Library(original, "$a")));
    }

    /// <summary>A different active override cannot retarget an escaped original item.</summary>
    [Fact]
    public void OriginalItemStaysBoundInsideDifferentOverride()
    {
        var caller = Caller("p:g(p:f('capture'))", "xsl:original#1").Replace("</xsl:override>", Function(OverrideParameters, "$x('A')").Replace("name='p:f'", "name='p:g'") + "</xsl:override>");
        var library = Library(extra: Function(OriginalParameters, "'WRONG'").Replace("name='p:f'", "name='p:g'"));
        Assert.Equal("ABC", Run(caller, library));
    }

    /// <summary>Original pseudo-functions are not dynamically available with either availability overload.</summary>
    [Fact]
    public void OriginalIsUnavailableAtEveryDynamicArity() => Assert.Equal("false false false false true", Run(Caller("p:f('A')", "(function-available('xsl:original'),function-available('xsl:original',1),function-available('xsl:original',2),function-available('xsl:original',3),empty(function-lookup(QName('http://www.w3.org/1999/XSL/Transform','original'),3)))")));

    /// <summary>Only the original upper bound identifies the component being overridden.</summary>
    [Fact]
    public void ReducedUpperBoundDoesNotIdentifyOriginal() => Assert.Contains("XTSE3058", Assert.ThrowsAny<Exception>(() => Run(Caller("'unused'", "$x || $y", OverrideParameters[..OverrideParameters.LastIndexOf("<xsl:param", StringComparison.Ordinal)]))).Message);

    /// <summary>Final components retain their non-overridable visibility.</summary>
    [Fact]
    public void FinalOptionalOriginalCannotBeOverridden() => Assert.Contains("XTSE3060", Assert.ThrowsAny<Exception>(() => Run(Caller("'unused'"), Library().Replace("visibility='public'", "visibility='final'"))).Message);

    /// <summary>Parameter conversion metadata must remain compatible across the family.</summary>
    [Fact]
    public void DifferentParameterTypeIsRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => Run(Caller("'unused'", parameters: OverrideParameters.Replace("name='x'", "name='x' as='xs:integer'")))).Message);

    /// <summary>Optional abstract functions accept compatible concrete replacements.</summary>
    [Fact]
    public void AbstractOptionalOriginalAcceptsConcreteOverride()
    {
        var library = Library().Replace("visibility='public'", "visibility='abstract'").Replace("<xsl:sequence select=\"$a || $b || $c\"/>", "");
        Assert.Equal("AXY", Run(Caller("p:f('A')"), library));
    }

    /// <summary>Package-private optional helpers remain visible only to the original default.</summary>
    [Fact]
    public void OriginalDefaultUsesPrivateHelper()
    {
        var original = OriginalParameters.Replace("select=\"'B'\"", "select='helper()'");
        const string helper = "<xsl:function name='helper'><xsl:param name='value' required='no' select=\"'LIB'\"/><xsl:sequence select='$value'/></xsl:function>";
        Assert.Equal("ALIBC true", Run(Caller("(p:f('A'),empty(function-lookup(QName('','helper'),0)))", "xsl:original($x)"), Library(original, extra: helper)));
    }

    /// <summary>The pre-existing package-local lookup view resolves reduced original aliases.</summary>
    [Fact]
    public void PackageLookupKeepsReducedOriginalAlias()
    {
        const string wrapper = "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"(p:f('A'),function-lookup(QName('urn:override','f'),1)('A'))\"/></xsl:function>";
        Assert.Equal("AXY ABC", Run(Caller("p:wrapper()"), Library(extra: wrapper)));
    }

    /// <summary>Memoized overriding and original components cannot share cached results.</summary>
    [Fact]
    public void MemoizedOriginalHasDistinctDeclarationIdentity()
    {
        var caller = Caller("(p:f('A','B','C'),p:f(p:f('capture'),'B','C'))", "if ($x instance of function(*)) then $x('A') else if ($x='capture') then xsl:original#1 else 'O' || xsl:original($x,$y,$z)").Replace("visibility='public'", "visibility='public' new-each-time='no'");
        var library = Library().Replace("visibility='public'", "visibility='public' new-each-time='no'");
        Assert.Equal("OABC ABC", Run(caller, library));
    }

    /// <summary>The original pseudo-name is absent outside an overriding function.</summary>
    [Fact]
    public void OriginalIsUnavailableOutsideOverride() => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Run(Caller("xsl:original('A','B','C')"))).Message);

    /// <summary>A helper does not inherit its caller's original pseudo-signature.</summary>
    [Fact]
    public void OriginalIsUnavailableInOrdinaryHelper()
    {
        const string helper = "<xsl:function name='p:helper'><xsl:sequence select=\"xsl:original('A','B','C')\"/></xsl:function>";
        Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Run(Caller("p:f('A')", "p:helper()", extra: helper))).Message);
    }

    /// <summary>An override invoked by its used package retains the using module's globals.</summary>
    [Fact]
    public void OverrideDefaultsUseDeclaringGlobalsFromLibraryCaller()
    {
        var parameters = OverrideParameters.Replace("select=\"'X'\"", "select='$suffix'");
        const string wrapper = "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"p:f('A')\"/></xsl:function><xsl:variable name='suffix' select=\"'LIB'\"/>";
        Assert.Equal("AUSERY", Run(Caller("p:wrapper()", parameters: parameters, extra: "<xsl:variable name='suffix' select=\"'USER'\"/>"), Library(extra: wrapper)));
    }

    /// <summary>Private caller helpers win over same-name private library helpers for override defaults.</summary>
    [Fact]
    public void OverrideDefaultKeepsPrincipalPrivateHelper()
    {
        var parameters = OverrideParameters.Replace("select=\"'X'\"", "select='helper()'");
        const string helper = "<xsl:function name='helper'><xsl:sequence select=\"'USER'\"/></xsl:function>";
        const string wrapper = "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"p:f('A')\"/></xsl:function>";
        Assert.Equal("AUSERY", Run(Caller("p:wrapper()", parameters: parameters, extra: helper), Library(extra: wrapper + helper.Replace("'USER'", "'LIB'"))));
    }

    /// <summary>A library-originated override uses its declaring module's default decimal format.</summary>
    [Fact]
    public void OverrideDefaultKeepsPrincipalDecimalFormat()
    {
        var parameters = OverrideParameters.Replace("select=\"'X'\"", "select=\"format-number(1.5,'0.0')\"");
        const string wrapper = "<xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"p:f('A')\"/></xsl:function><xsl:decimal-format decimal-separator=',' grouping-separator='.'/>";
        Assert.Equal("A1.5Y", Run(Caller("p:wrapper()", parameters: parameters), Library(extra: wrapper)));
    }

    /// <summary>Original default conversion errors keep the ordinary XPath error code.</summary>
    [Fact]
    public void OriginalDefaultTypeErrorIsPreserved()
    {
        var original = OriginalParameters.Replace("name='b'", "name='b' as='xs:integer'").Replace("select=\"'B'\"", "select=\"'bad'\"");
        var replacement = OverrideParameters.Replace("name='y'", "name='y' as='xs:integer'").Replace("select=\"'X'\"", "select='2'");
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(Caller("p:f('A')", "xsl:original($x)", replacement), Library(original))).Message);
    }

    /// <summary>Explicit streamability requirements remain part of signature compatibility.</summary>
    [Fact]
    public void DifferentStreamabilityIsRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => Run(Caller("'unused'"), Library().Replace("visibility='public'", "visibility='public' streamability='unclassified'"))).Message);

    private sealed class Resolver(string library) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(library);
    }
}
