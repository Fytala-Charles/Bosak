// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify mode names and dispatch using applicable XSLT namespace bindings.
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

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests mode declarations, template membership, default modes and runtime dispatch.</summary>
public class Xslt40ModeQNameTests
{
    private static readonly Uri MainUri = new("file:///modes/main.xsl");
    private static string Sheet(string body, string bindings = "#standard p=urn:fixed q=urn:fixed", string extra = "")
        => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' fixed-namespaces='" + bindings + "' " + extra + ">" + body + "</xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }
    private static void AssertExecutes(string text, string expected = "7", string? initialMode = null)
    {
        var snapshot = Inspect(text);
        var validation = XsltValidation.Validate(snapshot);
        Assert.True(validation.Outcome == XsltValidationOutcome.Valid, string.Join("; ", validation.Diagnostics.Select(d => d.Message)));
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        var result = new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root><item/></root>")), initialMode: initialMode);
        Assert.Equal(expected, XElement.Parse(result).Value);
    }

    /// <summary>Fixed-only, rebound, explicit and unprefixed mode names agree across all paths.</summary>
    /// <param name="declaration">The mode declaration name.</param>
    /// <param name="membership">The mode named on the template.</param>
    /// <param name="dispatch">The apply-templates mode.</param>
    [Theory]
    [InlineData("q:m", "q:m", "q:m")]
    [InlineData("p:m", "Q{urn:fixed}m", "q:m")]
    [InlineData("Q{urn:fixed}m", "q:m", "p:m")]
    [InlineData("q:m", "p:m", "Q{urn:fixed}m")]
    [InlineData("m", "Q{}m", "m")]
    public void FixedModeNamesExecute(string declaration, string membership, string dispatch)
        => AssertExecutes(Sheet("<xsl:mode name='" + declaration + "'/><xsl:template match='/' mode='#unnamed'><out><xsl:apply-templates select='root/item' mode='" + dispatch + "'/></out></xsl:template><xsl:template match='item' mode='" + membership + "'>7</xsl:template>"));

    /// <summary>Native rebinding on the instruction does not override fixed mode prefixes.</summary>
    [Fact]
    public void NativeRebindingDoesNotChangeDispatch()
        => AssertExecutes(Sheet("<xsl:mode name='p:m'/><xsl:template match='/' mode='#unnamed'><out><xsl:apply-templates xmlns:p='urn:other' select='root/item' mode='p:m'/></out></xsl:template><xsl:template match='item' mode='q:m'>7</xsl:template>"));

    /// <summary>Stylesheet default-mode uses the fixed binding.</summary>
    [Fact]
    public void StylesheetDefaultModeExecutes()
        => AssertExecutes(Sheet("<xsl:mode name='q:m'/><xsl:template match='/' mode='q:m'><out>7</out></xsl:template>", extra: "default-mode='p:m'"));

    /// <summary>Scoped default modes agree with the template's fixed mode identity.</summary>
    /// <param name="location">The owner of the scoped default-mode.</param>
    [Theory]
    [InlineData("template")]
    [InlineData("instruction")]
    [InlineData("literal")]
    public void ScopedDefaultModeExecutes(string location)
    {
        var templateAttribute = location == "template" ? "default-mode='p:m'" : "";
        var instructionAttribute = location == "instruction" ? "default-mode='p:m'" : "";
        var literalAttribute = location == "literal" ? "xsl:default-mode='p:m'" : "";
        AssertExecutes(Sheet("<xsl:mode name='q:m'/><xsl:template match='/' mode='#unnamed' " + templateAttribute + "><out " + literalAttribute + "><xsl:apply-templates select='root/item' " + instructionAttribute + "/></out></xsl:template><xsl:template match='item' mode='q:m'>7</xsl:template>"));
    }

    /// <summary>Initial modes supplied by consumers use the same fixed mode identity.</summary>
    [Fact]
    public void InitialModeExecutes()
        => AssertExecutes(Sheet("<xsl:mode name='q:m'/><xsl:template match='/' mode='q:m'><out>7</out></xsl:template>"), initialMode: "p:m");

    /// <summary>Each named token in a template mode list uses fixed bindings.</summary>
    [Fact]
    public void ModeListsExecute()
        => AssertExecutes(Sheet("<xsl:mode name='q:m'/><xsl:mode name='q:n'/><xsl:template match='/' mode='#unnamed'><out><xsl:apply-templates select='root/item' mode='q:n'/></out></xsl:template><xsl:template match='item' mode='p:m p:n'>7</xsl:template>"));

    /// <summary>Excluded native prefixes and malformed mode names receive source diagnostics.</summary>
    /// <param name="body">The declaration or reference.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("<xsl:mode name='p:m'/>", "XTSE0280")]
    [InlineData("<xsl:template match='/' mode='p:m'/>", "XTSE0280")]
    [InlineData("<xsl:template match='/'><xsl:apply-templates mode='p:m'/></xsl:template>", "XTSE0280")]
    [InlineData("<xsl:template match='/' default-mode='p:m'/>", "XTSE0280")]
    [InlineData("<xsl:template match='/'><out xsl:default-mode='p:m'/></xsl:template>", "XTSE0280")]
    [InlineData("<xsl:mode name='a:b:c'/>", "XTSE0020")]
    [InlineData("<xsl:template match='/' mode='a:b:c'/>", "XTSE0550")]
    [InlineData("<xsl:template match='/'><xsl:apply-templates mode='a:b:c'/></xsl:template>", "XTSE0020")]
    public void InvalidModeNameIsReported(string body, string code)
    {
        var text = Sheet(body, "#standard");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == code && d.Range is not null);
        Assert.Contains(code, Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Root default-mode errors use the original attribute range.</summary>
    [Fact]
    public void RootDefaultModeErrorIsReported()
    {
        var text = Sheet("", "#standard", "default-mode='p:m'");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XTSE0280" && d.Range is not null);
        Assert.Contains("XTSE0280", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Literal exclusion suppresses invalid mode declarations and references.</summary>
    [Fact]
    public void ExcludedModesAreIgnored()
    {
        var text = Sheet("<xsl:mode name='p:m' use-when='false()'/><xsl:template match='/' mode='p:m' use-when='false()'/>", "#standard");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Earlier profiles preserve their native mode declarations and dispatch.</summary>
    /// <param name="version">The earlier host version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRemainNative(string version)
    {
        var text = Sheet("<xsl:mode name='p:m'/><xsl:template match='/'><out><xsl:apply-templates select='root/item' mode='p:m'/></out></xsl:template><xsl:template match='item' mode='p:m'>7</xsl:template>", "#standard").Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard'", "");
        AssertExecutes(text);
    }

    /// <summary>Namespace documents are captured during inspection, without validation retries.</summary>
    [Fact]
    public void NamespaceDocumentIsCapturedOnce()
    {
        var text = Sheet("<xsl:mode name='q:m'/><xsl:template match='/' mode='q:m'><out>7</out></xsl:template>", "#standard bindings.xml", "default-mode='q:m'");
        var resolver = new Resolver();
        var snapshot = Inspect(text, resolver);
        int requests = resolver.Requests;
        Assert.True(requests > 0);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        var mode = snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "mode")!;
        Assert.Null(Assert.Single(mode.Attributes, a => a.LocalName == "name").SlotContext);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        _ = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver()) }.Compile(text, MainUri.AbsoluteUri);
    }

    /// <summary>Earlier local host profiles retain native bindings inside a 4.0 module.</summary>
    [Fact]
    public void EarlierLocalProfileRemainsNative()
        => AssertExecutes(Sheet("<xsl:mode name='p:m' version='3.0'/><xsl:template match='/' mode='#unnamed'><out><xsl:apply-templates select='root/item' mode='Q{urn:native}m'/></out></xsl:template><xsl:template match='item' mode='p:m' version='3.0'>7</xsl:template>"));

    /// <summary>Existing special template modes retain their dispatch behavior.</summary>
    /// <param name="membership">The template's special mode token.</param>
    [Theory]
    [InlineData("#unnamed")]
    [InlineData("#default")]
    [InlineData("#all")]
    public void SpecialTemplateModesExecute(string membership)
        => AssertExecutes(Sheet("<xsl:template match='/' mode='" + membership + "'><out>7</out></xsl:template>"));

    /// <summary>Invalid template mode lists retain XTSE0550 during validation and compilation.</summary>
    /// <param name="membership">The invalid mode list.</param>
    [Theory]
    [InlineData("")]
    [InlineData("#all q:m")]
    [InlineData("q:m q:m")]
    [InlineData("#current")]
    public void InvalidModeListsAreRejected(string membership)
    {
        var text = Sheet("<xsl:template match='/' mode='" + membership + "'/>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XTSE0550");
        Assert.Contains("XTSE0550", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Included and imported modes use the declaring module's fixed bindings.</summary>
    /// <param name="kind">The module linkage kind.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedModeExecutes(string kind)
    {
        var main = Sheet("<xsl:" + kind + " href='modes.xsl'/><xsl:template match='/' mode='#unnamed'><out><xsl:apply-templates select='root/item' mode='q:m'/></out></xsl:template>");
        var module = Sheet("<xsl:mode name='p:m'/><xsl:template match='item' mode='p:m'>7</xsl:template>");
        var resolver = new Resolver(new() { ["modes.xsl"] = module });
        var snapshot = Inspect(main, resolver);
        int requests = resolver.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(module), snapshot.ExportOriginal(new Uri(MainUri, "modes.xsl")));
        var compiler = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver(new() { ["modes.xsl"] = module })) };
        var output = compiler.Compile(main, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<root><item/></root>")));
        Assert.Equal("7", XElement.Parse(output).Value);
    }

    /// <summary>Explicit mode identities supplied as initial modes keep their accepted forms.</summary>
    /// <param name="initialMode">The initial-mode identity.</param>
    [Theory]
    [InlineData("Q{urn:fixed}m")]
    [InlineData("{urn:fixed}m")]
    public void ExplicitInitialModeExecutes(string initialMode)
        => AssertExecutes(Sheet("<xsl:mode name='q:m'/><xsl:template match='/' mode='q:m'><out>7</out></xsl:template>"), initialMode: initialMode);

    private sealed class Resolver(Dictionary<string, string>? modules = null) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled namespace document requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            string text;
            if (modules is not null)
            {
                if (!modules.TryGetValue(absoluteUri.Segments.Last(), out text!)) return null;
            }
            else
            {
                Assert.Equal(new Uri(MainUri, "bindings.xml"), absoluteUri);
                text = "<bindings xmlns:p='urn:fixed' xmlns:q='urn:fixed'/>";
            }
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
