// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify authoring keyword signatures using captured declaration namespaces.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Add precedence, exclusion, overload and deferred coverage boundaries.                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================


using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests captured XSLT function signatures during authoring keyword validation.</summary>
public class Xslt40AuthoringFunctionKeywordTests
{
    private static readonly Uri MainUri = new("file:///keywords/main.xsl");
    private static string Sheet(string body, string bindings = "#standard p=urn:fixed q=urn:fixed")
        => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' exclude-result-prefixes='#all' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static string Function(string first = "p:a", string second = "p:b", string name = "p:f")
        => "<xsl:function name='" + name + "'><xsl:param name='" + first + "'/><xsl:param name='" + second + "'/><xsl:sequence select=\"$" + first + " || '|' || $" + second + "\"/></xsl:function>";
    private static string Output(string call, string extra = "")
        => "<xsl:template match='/'><out><xsl:value-of " + extra + " select=\"" + call + "\"/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        Assert.False(result.Snapshot!.IsCompilable);
        return result.Snapshot;
    }
    private static void AssertValid(string text, Resolver? resolver = null)
    {
        var snapshot = Inspect(text, resolver);
        var requests = resolver?.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver?.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        var compiler = new Api.XsltCompiler { UriResolver = resolver is null ? null : new AuthoringModuleUriResolverBridge(resolver) };
        var result = compiler.Compile(text, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("A|B", XElement.Parse(result).Value);
    }

    /// <summary>Keyword identities and argument order agree with runtime compilation.</summary>
    /// <param name="call">The function call.</param>
    [Theory]
    [InlineData("p:f(p:b := 'B', p:a := 'A')")]
    [InlineData("q:f(q:b := 'B', q:a := 'A')")]
    [InlineData("Q{urn:fixed}f(Q{urn:fixed}b := 'B', Q{urn:fixed}a := 'A')")]
    [InlineData("p:f('A', q:b := 'B')")]
    [InlineData("'A' => p:f(q:b := 'B')")]
    [InlineData("p:f(p:a := substring(value := 'XA', start := 2), p:b := 'B')")]
    [InlineData("declare namespace r = 'urn:fixed'; r:f(r:b := 'B', r:a := 'A')")]
    public void ExpandedKeywordsValidate(string call) => AssertValid(Sheet(Function() + Output(call)));

    /// <summary>Unprefixed parameter keywords do not use the default element namespace.</summary>
    [Fact]
    public void UnprefixedParametersValidate() => AssertValid(Sheet(Function("a", "b") + Output("p:f(b := 'B', a := 'A')")).Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:elements'"));

    /// <summary>Native local rebinding cannot replace a fixed keyword namespace.</summary>
    [Fact]
    public void FixedRebindingValidates() => AssertValid(Sheet(Function() + Output("p:f(p:b := 'B', p:a := 'A')", "xmlns:p='urn:local'")));

    /// <summary>AVT expressions receive the same captured function signatures.</summary>
    [Fact]
    public void AvtKeywordsValidate() => AssertValid(Sheet(Function() + "<xsl:template match='/'><out value=\"{p:f(p:b := 'B', p:a := 'A')}\"><xsl:value-of select=\"p:f(p:a := 'A', p:b := 'B')\"/></out></xsl:template>"));

    /// <summary>Invalid keyword calls retain source-local static diagnostics.</summary>
    /// <param name="call">The invalid call.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("p:f(a := 'A', b := 'B')", "XPST0017")]
    [InlineData("p:f(Q{urn:other}a := 'A', p:b := 'B')", "XPST0017")]
    [InlineData("p:f('A', p:a := 'B')", "XPST0017")]
    [InlineData("p:f(p:a := 'A', q:a := 'B')", "XPST0017")]
    [InlineData("p:f(p:a := 'A')", "XPST0017")]
    [InlineData("p:f(p:a := 'A', p:b := 'B', p:c := 'C')", "XPST0017")]
    [InlineData("p:f(?, p:b := 'B')", "XPST0017")]
    [InlineData("'A' => p:f(p:a := 'B')", "XPST0017")]
    [InlineData("p:f(missing:a := 'A', p:b := 'B')", "XPST0081")]
    public void InvalidKeywordsHaveRanges(string call, string code)
    {
        var result = XsltValidation.Validate(Inspect(Sheet(Function() + Output(call))));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == code && d.ModuleUri == MainUri && d.Range is not null);
    }

    /// <summary>Earlier profiles keep their grammar rejection of keywords.</summary>
    /// <param name="version">The earlier stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRejectKeywords(string version)
    {
        var text = Sheet(Function() + Output("p:f(p:b := 'B', p:a := 'A')")).Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", "");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0003");
    }

    /// <summary>Included and imported declarations retain their own namespace bindings.</summary>
    /// <param name="kind">The linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedFunctionsValidate(string kind)
    {
        var main = Sheet("<xsl:" + kind + " href='functions.xsl'/>" + Output("q:f(q:b := 'B', q:a := 'A')"), "#standard q=urn:module");
        var resolver = new Resolver(new() { ["functions.xsl"] = Sheet(Function(), "#standard p=urn:module") });
        AssertValid(main, resolver);
    }

    /// <summary>Namespace documents resolve during inspection but not repeated validation.</summary>
    [Fact]
    public void NamespaceDocumentsAreCaptured()
    {
        var resolver = new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:p='urn:fixed' xmlns:q='urn:fixed'/>" });
        AssertValid(Sheet(Function() + Output("q:f(q:b := 'B', q:a := 'A')"), "#standard bindings.xml"), resolver);
    }

    /// <summary>Parameter-local native namespaces agree between authoring and execution.</summary>
    [Fact]
    public void LocalParameterBindingsValidate()
    {
        var text = Sheet(Function() + Output("p:f(q:b := 'B', q:a := 'A')"))
            .Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", "")
            .Replace("xmlns:p='urn:native'", "xmlns:p='urn:native' xmlns:q='urn:fixed'")
            .Replace("name='p:a'/>", "name='p:a' xmlns:p='urn:fixed'/>")
            .Replace("name='p:b'/>", "name='p:b' xmlns:p='urn:fixed'/>")
            .Replace("$p:a", "$q:a").Replace("$p:b", "$q:b");
        AssertValid(text);
    }

    /// <summary>Literal exclusions do not contribute function or parameter signatures.</summary>
    [Fact]
    public void LiteralExclusionsAreIgnored()
    {
        var text = Sheet(Function().Replace("<xsl:param name='p:a'/>", "<xsl:param name='missing:ignored' use-when='false()'/><xsl:param name='p:a'/>")
            + "<xsl:function name='missing:excluded' use-when='false()'/>" + Output("p:f(p:b := 'B', p:a := 'A')"));
        AssertValid(text);
    }

    /// <summary>Overloads resolve their own parameter names by supplied arity.</summary>
    [Fact]
    public void OverloadKeywordsValidate()
    {
        var extra = "<xsl:function name='p:f'><xsl:param name='p:single'/><xsl:sequence select='$p:single'/></xsl:function>";
        AssertValid(Sheet(Function() + extra + Output("p:f(p:b := 'B', p:a := p:f(p:single := 'A'))")));
    }

    /// <summary>The principal function declaration wins over an imported signature.</summary>
    [Fact]
    public void PrincipalOverridesImportedSignature()
    {
        var main = Sheet("<xsl:import href='functions.xsl'/>" + Function() + Output("p:f(p:b := 'B', p:a := 'A')"));
        AssertValid(main, new Resolver(new() { ["functions.xsl"] = Sheet(Function("p:x", "p:y")) }));
    }

    /// <summary>Later sibling imports supply the winning parameter identities.</summary>
    [Fact]
    public void LaterSiblingImportWins()
    {
        var main = Sheet("<xsl:import href='first.xsl'/><xsl:import href='second.xsl'/>" + Output("p:f(p:b := 'B', p:a := 'A')"));
        AssertValid(main, new Resolver(new() { ["first.xsl"] = Sheet(Function("p:x", "p:y")), ["second.xsl"] = Sheet(Function()) }));
    }

    /// <summary>Signatures outside the supported contract report coverage rather than invalid calls.</summary>
    /// <param name="declaration">The unsupported declaration.</param>
    /// <param name="call">The keyword call.</param>
    [Theory]
    [InlineData("<xsl:function _name=\"{'p:f'}\"><xsl:param name='p:a'/><xsl:sequence select='$p:a'/></xsl:function>", "p:f(p:a := 'A')")]
    [InlineData("<xsl:function name='p:f'><xsl:param name='p:a' _name=\"{'p:x'}\"/><xsl:sequence select='$p:a'/></xsl:function>", "p:f(p:a := 'A')")]
    [InlineData("<xsl:function name='p:f'><xsl:param name='p:a' required='no' select=\"'A'\"/><xsl:param name='p:b' required='no' select=\"'B'\"/><xsl:sequence select='$p:a || $p:b'/></xsl:function>", "p:f(p:b := 'B')")]
    public void DeferredSignaturesAreCoverageGaps(string declaration, string call)
    {
        var result = XsltValidation.Validate(Inspect(Sheet(declaration + Output(call))));
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, result.Outcome);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "XPST0017");
        Assert.Contains(result.UnsupportedCoverage, g => g.Construct == "function-keywords");
    }

    /// <summary>Namespace document refusal is not retried during signature capture or validation.</summary>
    [Fact]
    public void RefusedNamespaceDocumentIsNotRetried()
    {
        var resolver = new Resolver(new());
        var text = Sheet(Function() + Output("p:f(p:a := 'A', p:b := 'B')"), "#standard missing.xml");
        var snapshot = Inspect(text, resolver);
        Assert.Equal(1, resolver.Requests);
        Assert.Contains(XsltValidation.Validate(snapshot).Diagnostics, d => d.Code == "XTSE0122");
        Assert.Contains(XsltValidation.Validate(snapshot).Diagnostics, d => d.Code == "XTSE0122");
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Conditional declarations cannot provide a confidently captured keyword signature.</summary>
    [Fact]
    public void ConditionalFunctionIsDeferred()
    {
        var text = Sheet(Function().Replace("name='p:f'", "name='p:f' use-when='1 eq 1'") + Output("p:f(p:a := 'A', p:b := 'B')"));
        var result = XsltValidation.Validate(Inspect(text));
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, result.Outcome);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "XPST0017");
        Assert.Contains(result.UnsupportedCoverage, g => g.Construct == "function-keywords");
    }

    /// <summary>Refused module graphs do not manufacture an unknown-keyword diagnostic.</summary>
    [Fact]
    public void RefusedModuleGraphIsDeferred()
    {
        var text = Sheet("<xsl:include href='missing.xsl'/>" + Output("p:f(p:a := 'A', p:b := 'B')"));
        var snapshot = Inspect(text, new Resolver(new()));
        var result = XsltValidation.Validate(snapshot);
        Assert.Equal(XsltValidationOutcome.Refused, result.Outcome);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "XPST0017");
        Assert.Contains(result.UnsupportedCoverage, g => g.Construct == "function-keywords");
    }

    /// <summary>Invalid imported keyword calls report the original module and attribute range.</summary>
    [Fact]
    public void ImportedDiagnosticIsSourceLocal()
    {
        var main = Sheet("<xsl:import href='functions.xsl'/>" + Output("p:f(p:a := 'A', p:b := 'B')"));
        var resolver = new Resolver(new() { ["functions.xsl"] = Sheet(Function() + "<xsl:template name='bad'><out value=\"{p:f(p:a := 'A', p:a := 'B')}\"/></xsl:template>") });
        var snapshot = Inspect(main, resolver);
        var result = XsltValidation.Validate(snapshot);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0017" && d.ModuleUri == new Uri("file:///keywords/functions.xsl") && d.Range is not null);
    }

    /// <summary>The supplied count distinguishes overloaded signatures with legacy arity suffixes.</summary>
    [Fact]
    public void AritySuffixKeepsParameterSignature() => AssertValid(Sheet(Function(name: "p:f#2") + Output("p:f(p:a := 'A', p:b := 'B')")));

    /// <summary>Unknown prefixes cannot match unprefixed host parameters.</summary>
    [Fact]
    public void UnknownPrefixCannotBorrowNoNamespace()
    {
        var text = Sheet(Function("a", "b") + Output("p:f(missing:a := 'A', b := 'B')"));
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0081");
    }

    private sealed class Resolver(Dictionary<string, string> modules) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of resource requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!modules.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
