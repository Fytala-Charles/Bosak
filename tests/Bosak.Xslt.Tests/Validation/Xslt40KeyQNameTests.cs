// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify key declarations and lookup identities using applicable namespaces.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Cover linked keys, accumulated definitions and declaration edges.                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests key names and string or QName lookup arguments using fixed namespaces.</summary>
public class Xslt40KeyQNameTests
{
    private static readonly Uri MainUri = new("file:///keys/main.xsl");
    private static string Sheet(string body, string bindings = "#standard p=urn:fixed q=urn:fixed")
        => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static string Key(string name, string extra = "") => "<xsl:key name='" + name + "' match='item' use='@id' " + extra + "/>";
    private static string Output(string argument, bool threeArguments = false, string extra = "")
        => "<xsl:template match='/'><out><xsl:value-of " + extra + " select=\"count(key(" + argument + ", 'a'" + (threeArguments ? ", ." : "") + "))\"/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }
    private static string Transform(Api.XsltExecutable executable)
        => XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root><item id='a'/><item id='b'/><other id='a'/></root>")))).Value;
    private static void AssertExecutes(string text, string expected = "1")
    {
        var snapshot = Inspect(text);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        Assert.Equal(expected, Transform(new Api.XsltCompiler().Compile(text)));
    }

    /// <summary>Fixed-only, rebound, EQName and unprefixed names identify the same key.</summary>
    /// <param name="declaration">The key name.</param>
    /// <param name="lookup">The string lookup name.</param>
    [Theory]
    [InlineData("q:k", "q:k")]
    [InlineData("p:k", "q:k")]
    [InlineData("Q{urn:fixed}k", "q:k")]
    [InlineData("q:k", "Q{urn:fixed}k")]
    [InlineData("k", "Q{}k")]
    [InlineData("Q{}k", "k")]
    [InlineData("  q:k  ", "q:k")]
    public void FixedKeyNamesExecute(string declaration, string lookup)
        => AssertExecutes(Sheet(Key(declaration) + Output("'" + lookup + "'")));

    /// <summary>Both key function arities use the same expanded EQName identity.</summary>
    /// <param name="threeArguments">Whether an explicit search root is supplied.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothAritiesExecute(bool threeArguments)
        => AssertExecutes(Sheet(Key("p:k") + Output("'Q{urn:fixed}k'", threeArguments)));

    /// <summary>QName arguments retain their namespace URI regardless of the lexical prefix.</summary>
    /// <param name="argument">The QName-valued lookup argument.</param>
    [Theory]
    [InlineData("QName('urn:fixed','other:k')")]
    [InlineData("QName('urn:fixed','p:k')")]
    [InlineData("QName('urn:fixed','k')")]
    [InlineData("#q:k")]
    public void QNameArgumentUsesItsOwnNamespace(string argument)
        => AssertExecutes(Sheet(Key("q:k") + Output(argument)));

    /// <summary>Native instruction rebinding does not change fixed string lookup prefixes.</summary>
    [Fact]
    public void NativeRebindingDoesNotChangeLookup()
        => AssertExecutes(Sheet(Key("q:k") + Output("'p:k'", extra: "xmlns:p='urn:other'")));

    /// <summary>Invalid declaration names receive source-local static errors.</summary>
    /// <param name="name">The invalid key declaration name.</param>
    /// <param name="code">The expected static error.</param>
    [Theory]
    [InlineData("p:k", "XTSE0280")]
    [InlineData("a:b:c", "XTSE0020")]
    [InlineData("Q{urn:key}", "XTSE0020")]
    [InlineData("", "XTSE0010")]
    [InlineData("  ", "XTSE0020")]
    public void InvalidDeclarationIsReported(string name, string code)
    {
        var text = Sheet(Key(name), "#standard");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == code && d.Range is not null);
        Assert.Contains(code, Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Invalid lookup strings retain XTDE1260 without evaluating them during authoring.</summary>
    /// <param name="lookup">The invalid or missing key name.</param>
    [Theory]
    [InlineData("q:missing")]
    [InlineData("missing:k")]
    [InlineData("Q{urn:fixed}")]
    [InlineData("a:b:c")]
    [InlineData("")]
    public void InvalidLookupReportsDynamicError(string lookup)
    {
        var text = Sheet(Key("q:k") + Output("'" + lookup + "'"));
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        Assert.Contains("XTDE1260", Assert.ThrowsAny<Exception>(() => Transform(new Api.XsltCompiler().Compile(text))).ToString());
    }

    /// <summary>Earlier profiles retain their native key declarations and lookups.</summary>
    /// <param name="version">The host version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRemainNative(string version)
    {
        var text = Sheet(Key("p:k") + Output("'p:k'"), "#standard").Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard'", "");
        AssertExecutes(text);
    }

    /// <summary>Literal exclusions suppress invalid key declaration names.</summary>
    [Fact]
    public void ExcludedKeyIsIgnored()
    {
        var text = Sheet(Key("p:k", "use-when='false()'"), "#standard");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Key name contexts capture caller-resolved namespace documents only during inspection.</summary>
    [Fact]
    public void NamespaceDocumentIsNotResolvedDuringValidation()
    {
        var text = Sheet(Key("q:k") + Output("'q:k'"), "#standard bindings.xml");
        var resolver = new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:q='urn:fixed'/>" });
        var snapshot = Inspect(text, resolver);
        int requests = resolver.Requests;
        Assert.True(requests > 0);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        var key = snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "key")!;
        Assert.Null(Assert.Single(key.Attributes, a => a.LocalName == "name").SlotContext);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        var compiler = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:q='urn:fixed'/>" })) };
        Assert.Equal("1", Transform(compiler.Compile(text, MainUri.AbsoluteUri)));
    }

    /// <summary>Linked declarations keep their own bindings and match principal-module lookups.</summary>
    /// <param name="kind">The module linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedKeyExecutes(string kind)
    {
        var main = Sheet("<xsl:" + kind + " href='keys.xsl'/>" + Output("'q:k'"));
        var module = Sheet(Key("p:k"), "#standard p=urn:fixed");
        var resolver = new Resolver(new() { ["keys.xsl"] = module });
        var snapshot = Inspect(main, resolver);
        int requests = resolver.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(module), snapshot.ExportOriginal(new Uri(MainUri, "keys.xsl")));
        var compiler = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver(new() { ["keys.xsl"] = module })) };
        Assert.Equal("1", Transform(compiler.Compile(main, MainUri.AbsoluteUri)));
    }

    /// <summary>Several declarations of the same expanded key name all contribute to the index.</summary>
    /// <param name="secondName">The alternate spelling of the same key name.</param>
    [Theory]
    [InlineData("q:k")]
    [InlineData("Q{urn:fixed}k")]
    public void SameExpandedNameAccumulatesDefinitions(string secondName)
        => AssertExecutes(Sheet(Key("p:k") + Key(secondName).Replace("match='item'", "match='other'") + Output("'q:k'")), "2");

    /// <summary>A QName in another namespace does not borrow the stylesheet's binding for its prefix.</summary>
    [Fact]
    public void QNameDoesNotBorrowPrefixBinding()
    {
        var text = Sheet(Key("q:k") + Output("QName('urn:other','q:k')"));
        Assert.Contains("XTDE1260", Assert.ThrowsAny<Exception>(() => Transform(new Api.XsltCompiler().Compile(text))).ToString());
    }

    private sealed class Resolver(Dictionary<string, string> modules) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled resource requests.</summary>
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
