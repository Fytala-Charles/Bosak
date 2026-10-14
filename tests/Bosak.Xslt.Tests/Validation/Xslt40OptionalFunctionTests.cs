// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Tests optional XSLT function arguments and declaration-scoped defaults.
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

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies optional function declarations, calls and default evaluation.</summary>
[Collection("XSLT package registry")]
public class Xslt40OptionalFunctionTests
{
    private const string Parameters = "<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:param name='c' required='no' select=\"'C'\"/>";
    private static string Sheet(string call, string parameters = Parameters, string body = "$a || '|' || $b || '|' || $c", string version = "4.0", string extra = "")
        => "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>"
            + extra + "<xsl:function name='p:f'>" + parameters + "<xsl:sequence select=\"" + body + "\"/></xsl:function>"
            + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Transform(string text, string input = "<root/>")
        => XElement.Parse(new XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse(input)))).Value;

    /// <summary>Reduced positional, keyword and arrow calls preserve supplied values and defaults.</summary>
    /// <param name="call">The call expression.</param>
    /// <param name="expected">The result.</param>
    [Theory]
    [InlineData("p:f('A')", "A|B|C")]
    [InlineData("p:f('A','X')", "A|X|C")]
    [InlineData("p:f('A','X','Y')", "A|X|Y")]
    [InlineData("p:f(a := 'A')", "A|B|C")]
    [InlineData("p:f(c := 'Y', a := 'A')", "A|B|Y")]
    [InlineData("p:f('A', c := 'Y')", "A|B|Y")]
    [InlineData("'A' => p:f(c := 'Y')", "A|B|Y")]
    [InlineData("p:f('A', (), 'Y')", "A||Y")]
    public void CallsBindOptionalDefaults(string call, string expected) => Assert.Equal(expected, Transform(Sheet(call)));

    /// <summary>A fully optional function has a zero-arity form.</summary>
    [Fact]
    public void ZeroArityIsAvailable() => Assert.Equal("D", Transform(Sheet("p:f()", "<xsl:param name='a' required='no' select=\"'D'\"/>", "$a")));

    /// <summary>Named references and function lookup expose every reduced arity.</summary>
    /// <param name="call">The higher-order expression.</param>
    [Theory]
    [InlineData("(p:f#1)('A')")]
    [InlineData("function-lookup(QName('urn:functions','f'),1)('A')")]
    [InlineData("let $f := p:f(?, 'B') return $f('A')")]
    public void ReducedFunctionItemsExecute(string call) => Assert.Equal("A|B|C", Transform(Sheet(call)));

    /// <summary>An absent default is the empty sequence, even without an as type.</summary>
    [Fact]
    public void EmptyDefaultIsNotEmptyStringOrDocument() => Assert.Equal("true", Transform(Sheet("p:f()", "<xsl:param name='a' required='no'/>", "empty($a)")));

    /// <summary>A parameter sequence constructor supplies the default.</summary>
    [Fact]
    public void ConstructorDefaultExecutes() => Assert.Equal("D", Transform(Sheet("p:f()", "<xsl:param name='a' required='no' as='xs:string'><xsl:sequence select=\"'D'\"/></xsl:param>", "$a")));

    /// <summary>Unused defaults are not evaluated for explicitly supplied arguments.</summary>
    [Fact]
    public void SuppliedArgumentSkipsFailingDefault() => Assert.Equal("A", Transform(Sheet("p:f('A')", "<xsl:param name='a' required='no' select='error()'/>", "$a")));

    /// <summary>Defaults use globals rather than caller locals or earlier function parameters.</summary>
    [Fact]
    public void DefaultsUseGlobalScope()
    {
        var text = Sheet("p:f('SUPPLIED')", "<xsl:param name='a'/><xsl:param name='b' required='no' select='$a'/>", "$a || '|' || $b", extra: "<xsl:variable name='a' select=\"'GLOBAL'\"/>");
        text = text.Replace("<out>", "<xsl:variable name='a' select=\"'CALLER'\"/><out>");
        Assert.Equal("SUPPLIED|GLOBAL", Transform(text));
    }

    /// <summary>Each default uses the caller focus while the function body has no focus.</summary>
    [Fact]
    public void DefaultsPreserveCallerFocus()
    {
        var text = Sheet("p:f()", "<xsl:param name='a' required='no' select=\"string(.) || ':' || position() || '/' || last()\"/>", "$a");
        text = text.Replace("<out><xsl:value-of select=\"p:f()\"/></out>", "<out><xsl:for-each select='root/item'><xsl:value-of select=\"p:f() || ';'\"/></xsl:for-each></out>");
        Assert.Equal("X:1/2;Y:2/2;", Transform(text, "<root><item>X</item><item>Y</item></root>"));
    }

    /// <summary>The function body cannot inherit the caller focus through default evaluation.</summary>
    [Fact]
    public void BodyFocusRemainsAbsent() => Assert.Contains("XPDY0002", Assert.ThrowsAny<Exception>(() => Transform(Sheet("p:f()", "<xsl:param name='a' required='no' select='.'/>", "string(.)"))).Message);

    /// <summary>Malformed declarations retain the specified static errors.</summary>
    /// <param name="parameters">The invalid declarations.</param>
    /// <param name="code">The required error code.</param>
    [Theory]
    [InlineData("<xsl:param name='a' select=\"'D'\"/>", "XTSE0760")]
    [InlineData("<xsl:param name='a' required='no'/><xsl:param name='b'/>", "XTSE0761")]
    [InlineData("<xsl:param name='a' required='no' select=\"'D'\"><xsl:sequence select=\"'X'\"/></xsl:param>", "XTSE0620")]
    public void InvalidDeclarationsAreRejected(string parameters, string code) => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(Sheet("'unused'", parameters, "'unused'"))).Message);

    /// <summary>Earlier stylesheet profiles do not accept optional function parameters.</summary>
    /// <param name="version">The earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRejectOptionalParameters(string version) => Assert.Contains("XTSE0020", Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(Sheet("p:f()", "<xsl:param name='a' required='no'/>", "$a", version))).Message);

    /// <summary>Captured authoring signatures validate keyword omissions without loading resources.</summary>
    /// <param name="call">The supported keyword expression.</param>
    [Theory]
    [InlineData("p:f(a := 'A')")]
    [InlineData("p:f(a := 'A', c := 'Y')")]
    public void AuthoringAcceptsOptionalKeywords(string call)
    {
        var text = Sheet(call);
        var uri = new Uri("file:///optional/main.xsl");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), uri, out var source, out _));
        var inspected = XsltAuthoring.Inspect(source!, null, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspected.IsSuccess);
        var snapshot = inspected.Snapshot!;
        var result = XsltValidation.Validate(snapshot);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(uri));
    }
    /// <summary>Reduced function items default from their creation focus.</summary>
    /// <param name="reference">The function item expression.</param>
    [Theory]
    [InlineData("p:f#0")]
    [InlineData("function-lookup(QName('urn:functions','f'),0)")]
    [InlineData("p:f(?)")]
    public void FunctionItemsCaptureDefaultFocus(string reference)
    {
        bool partial = reference.Contains('?');
        var parameters = partial ? "<xsl:param name='x'/><xsl:param name='a' required='no' select='string(.)'/>" : "<xsl:param name='a' required='no' select='string(.)'/>";
        var call = "root/item ! (let $f := " + reference + " return 'OTHER' ! $f(" + (partial ? "'unused'" : "") + "))";
        Assert.Equal("X Y", Transform(Sheet(call, parameters, "$a"), "<root><item>X</item><item>Y</item></root>"));
    }

    /// <summary>Default values must satisfy declared parameter types.</summary>
    [Fact]
    public void TypedDefaultCannotBeEmpty() => Assert.Contains("XTTE0600", Assert.ThrowsAny<Exception>(() => Transform(Sheet("p:f()", "<xsl:param name='a' required='no' as='xs:integer'/>", "$a"))).Message);

    /// <summary>An explicit empty keyword value does not request its default.</summary>
    [Fact]
    public void KeywordEmptyIsExplicit() => Assert.Equal("A||C", Transform(Sheet("p:f(a := 'A', b := ())")));

    /// <summary>Default evaluation restores the caller focus.</summary>
    [Fact]
    public void CallerFocusIsRestored() => Assert.Equal("root", Transform(Sheet("p:f() || name(root)", "<xsl:param name='a' required='no' select=\"''\"/>", "$a")));

    /// <summary>Same-precedence declarations cannot overlap at any arity.</summary>
    [Fact]
    public void OverlappingRangesAreRejected()
    {
        var extra = "<xsl:function name='p:f'><xsl:param name='x'/><xsl:sequence select='$x'/></xsl:function>";
        Assert.Contains("XTSE0770", Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(Sheet("'unused'", extra: extra))).Message);
    }

    /// <summary>Nonoverlapping overloads remain independent.</summary>
    [Fact]
    public void DisjointRangesRemainCallable()
    {
        var extra = "<xsl:function name='p:f'><xsl:sequence select=\"'ZERO'\"/></xsl:function>";
        Assert.Equal("ZERO|A|B|C", Transform(Sheet("p:f() || '|' || p:f('A')", extra: extra)));
    }

    /// <summary>Defaults retain declaration fixed namespaces rather than caller rebinding.</summary>
    [Fact]
    public void DefaultsUseDeclaringNamespaces()
    {
        var text = Sheet("p:f()", "<xsl:param name='a' required='no' select='count(q:item)'/>", "$a")
            .Replace("exclude-result-prefixes='#all'", "xmlns:q='urn:native' fixed-namespaces='#standard p=urn:functions q=urn:fixed' exclude-result-prefixes='#all'")
            .Replace("<out><xsl:value-of select=\"p:f()\"/></out>", "<out><xsl:for-each select='root'><xsl:value-of xmlns:q='urn:caller' select=\"p:f()\"/></xsl:for-each></out>");
        Assert.Equal("1", Transform(text, "<root xmlns:f='urn:fixed' xmlns:n='urn:native'><f:item/><n:item/><n:item/></root>"));
    }

    /// <summary>Optional keyword edits validate and preserve original source bytes.</summary>
    [Fact]
    public void AuthoringKeywordEditExecutes()
    {
        var text = Sheet("'OLD'"); var uri = new Uri("file:///optional/edit.xsl"); var bytes = Encoding.UTF8.GetBytes(text);
        Assert.True(AuthoringSource.TryCreate(bytes, uri, out var source, out _));
        var snapshot = XsltAuthoring.Inspect(source!, null, new AuthoringInspectionOptions { AttemptCompilation = false }).Snapshot!;
        var node = snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "value-of")!;
        var edited = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id, "select", "p:f(a := 'A', c := 'Y')"));
        Assert.True(edited.IsSuccess, edited.Failure?.ToString());
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(edited.Candidate!.Snapshot).Outcome);
        Assert.Equal("A|B|Y", Transform(Encoding.UTF8.GetString(edited.Candidate.EmittedSource.Span)));
        Assert.Equal(bytes, snapshot.ExportOriginal(uri));
    }

    /// <summary>Typed optional holes defer conversion until the default is evaluated.</summary>
    [Fact]
    public void TypedKeywordHoleExecutes()
    {
        const string parameters = "<xsl:param name='a' as='xs:integer'/><xsl:param name='b' as='xs:integer' required='no' select='2'/><xsl:param name='c' as='xs:integer' required='no' select='3'/>";
        Assert.Equal("13", Transform(Sheet("p:f(a := 1, c := 10)", parameters, "$a + $b + $c")));
    }

    /// <summary>Optional booleans use the existing yes/no normalization.</summary>
    /// <param name="required">The false-valued spelling.</param>
    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    public void FalseRequiredSpellingsExecute(string required) => Assert.Equal("D", Transform(Sheet("p:f()", "<xsl:param name='a' required='" + required + "' select=\"'D'\"/>", "$a")));

    /// <summary>Default static base URI comes from the declaration rather than caller.</summary>
    [Fact]
    public void DefaultRetainsDeclarationBaseUri()
    {
        var text = Sheet("p:f()", "<xsl:param name='a' required='no' xml:base='file:///decl/' select='static-base-uri()'/>", "$a")
            .Replace("<xsl:value-of", "<xsl:value-of xml:base='file:///caller/'");
        Assert.Equal("file:///decl/", Transform(text));
    }

    /// <summary>Authoring defaults see globals, not their own or earlier function parameters.</summary>
    [Fact]
    public void AuthoringRejectsParameterReferenceInDefault()
    {
        var text = Sheet("p:f('A')", "<xsl:param name='a'/><xsl:param name='b' required='no' select='$a'/>", "$b");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), new Uri("file:///optional/main.xsl"), out var source, out _));
        var snapshot = XsltAuthoring.Inspect(source!, null, new AuthoringInspectionOptions { AttemptCompilation = false }).Snapshot!;
        Assert.Contains(XsltValidation.Validate(snapshot).Diagnostics, d => d.Code == "XPST0008");
    }

    /// <summary>A partially overriding arity range is a static error.</summary>
    [Fact]
    public void PartialImportRangeIsRejected()
    {
        var imported = Sheet("'unused'");
        var main = Sheet("'unused'", "<xsl:param name='a'/><xsl:param name='b'/>", "$a", extra: "<xsl:import href='functions.xsl'/>");
        Assert.Contains("XTSE0769", Assert.ThrowsAny<Exception>(() => new XsltCompiler { UriResolver = new TextResolver(imported) }.Compile(main, "file:///optional/main.xsl")).Message);
    }

    /// <summary>Complete arity-range overriding selects the higher-precedence defaults.</summary>
    [Fact]
    public void FullImportRangeOverridesDefaults()
    {
        var imported = Sheet("'unused'");
        var main = Sheet("p:f('A')", Parameters.Replace("'B'", "'WINNER'"), extra: "<xsl:import href='functions.xsl'/>");
        var executable = new XsltCompiler { UriResolver = new TextResolver(imported) }.Compile(main, "file:///optional/main.xsl");
        Assert.Equal("A|WINNER|C", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>An eclipsed lower-precedence overlapping family does not reject a valid winner.</summary>
    [Fact]
    public void EclipsedOptionalFamiliesAreAllowed()
    {
        var imported = Sheet("'unused'", extra: "<xsl:function name='p:f'>" + Parameters + "<xsl:sequence select=\"'lower'\"/></xsl:function>");
        var parameters = Parameters.Replace("<xsl:param name='a'/>", "<xsl:param name='a' required='no'/>");
        var main = Sheet("p:f()", parameters, "'WINNER'", extra: "<xsl:import href='functions.xsl'/>");
        var executable = new XsltCompiler { UriResolver = new TextResolver(imported) }.Compile(main, "file:///optional/main.xsl");
        Assert.Equal("WINNER", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>Whitespace-preserving constructor defaults are not mistaken for empty content.</summary>
    [Fact]
    public void PreservedWhitespaceDefaultExecutes() => Assert.Equal("2", Transform(Sheet("p:f()", "<xsl:param name='a' required='no' xml:space='preserve'>  </xsl:param>", "string-length(string($a))")));

    /// <summary>Used-package optional arity linkage remains an explicit unsupported profile.</summary>
    [Fact]
    public void OptionalUsedPackageLinkageIsDeferred()
    {
        const string name = "urn:optional-profile";
        var package = "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:functions' name='" + name + "' package-version='1.0' version='4.0'><xsl:function name='p:f' visibility='public'><xsl:param name='a' required='no'/><xsl:sequence select='$a'/></xsl:function></xsl:package>";
        XsltFunctionLibrary.RegisterPackage(name, "1.0", "file:///optional/library.xsl");
        try
        {
            var text = Sheet("'unused'", extra: "<xsl:use-package name='" + name + "' package-version='1.0'/>");
            var error = Assert.Throws<NotSupportedException>(() => new XsltCompiler { UriResolver = new TextResolver(package) }.Compile(text));
            Assert.Contains("supported profile", error.Message);
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Required omissions, duplicate keywords and excess arguments remain errors.</summary>
    /// <param name="call">The invalid optional-function call.</param>
    [Theory]
    [InlineData("p:f(c := 'Y')")]
    [InlineData("p:f('A', a := 'X')")]
    [InlineData("p:f(a := 'A', b := 'B', c := 'C', d := 'D')")]
    [InlineData("p:f('A','B','C','D')")]
    public void InvalidOptionalCallsAreRejected(string call) => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Transform(Sheet(call))).Message);

    /// <summary>Availability and function-item arity expose exactly the declared range.</summary>
    [Fact]
    public void ArityRangeIsVisibleToLookup() => Assert.Equal("true true true true true", Transform(Sheet("(function-available('p:f',1), function-available('p:f',3), not(function-available('p:f',0)), empty(function-lookup(QName('urn:functions','f'),4)), function-arity(p:f#1) = 1)")));

    private sealed class TextResolver(string text) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(text);
    }

}
