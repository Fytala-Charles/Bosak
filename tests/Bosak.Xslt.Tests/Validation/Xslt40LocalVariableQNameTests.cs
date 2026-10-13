// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify module fixed identities for locally earlier variable and parameter names.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Verify resource refusal, duplicate identity, edit ownership and keyword error boundaries.   |
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

/// <summary>Tests locally earlier variable/parameter QNames in a fixed-binding module.</summary>
public class Xslt40LocalVariableQNameTests
{
    private static readonly Uri MainUri = new("file:///local-variables/main.xsl");
    private static string Sheet(string body, string bindings = "#standard p=urn:fixed q=urn:fixed")
        => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' exclude-result-prefixes='#all' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static string Output(string expression = "$q:v")
        => "<xsl:template match='/'><out><xsl:value-of select=\"" + expression + "\"/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var inspected = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspected.IsSuccess);
        return inspected.Snapshot!;
    }
    private static void AssertValid(string text, Resolver? resolver = null)
    {
        var snapshot = Inspect(text, resolver);
        var requests = resolver?.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver?.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
        var compiler = new XsltCompiler { UriResolver = resolver is null ? null : new AuthoringModuleUriResolverBridge(resolver) };
        Assert.Equal("7", XElement.Parse(compiler.Compile(text, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>Declarations and supplied parameters use module identities despite local earlier versions.</summary>
    /// <param name="body">The declaration and reference scenario.</param>
    [Theory]
    [InlineData("<xsl:variable version='3.0' name='p:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$q:v'/></out></xsl:template>")]
    [InlineData("<xsl:param version='2.0' name='p:v' select='7'/><xsl:template match='/'><out><xsl:value-of select='$q:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/' version='3.0'><xsl:variable name='p:v' select='7'/><out><xsl:value-of select='$q:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/'><xsl:variable version='2.0' name='q:v' select='7'/><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/' version='3.0'><xsl:param name='p:a' select='6'/><xsl:param name='q:v' select='$q:a + 1'/><out><xsl:value-of select='$p:v'/></out></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param version='3.0' name='q:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target' version='3.0'><xsl:param name='p:v' select='0'/><xsl:value-of select='$q:v'/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:apply-templates select='/*'><xsl:with-param version='2.0' name='q:v' select='7'/></xsl:apply-templates></out></xsl:template><xsl:template match='root' version='3.0'><xsl:param name='p:v' select='0'/><xsl:value-of select='$q:v'/></xsl:template>")]
    public void LocalDeclarationsAgree(string body) => AssertValid(Sheet(body));

    /// <summary>Even unused lower-profile declarations reject prefixes omitted from the fixed set.</summary>
    /// <param name="body">The unused declaration.</param>
    [Theory]
    [InlineData("<xsl:variable version='3.0' name='p:v' select='7'/><xsl:template match='/'><out/></xsl:template>")]
    [InlineData("<xsl:param version='2.0' name='p:v' select='7'/><xsl:template match='/'><out/></xsl:template>")]
    [InlineData("<xsl:template match='/' version='3.0'><xsl:variable name='p:v' select='7'/><out/></xsl:template>")]
    [InlineData("<xsl:template match='/' version='3.0'><xsl:param name='p:v' select='7'/><out/></xsl:template>")]
    [InlineData("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param version='3.0' name='p:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target'/>")]
    public void UnusedExcludedNamesAreInvalid(string body)
    {
        var text = Sheet(body, "#standard");
        var result = XsltValidation.Validate(Inspect(text));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0280" && d.ModuleUri == MainUri && d.Range is not null);
        Assert.Contains("XTSE0280", Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(text)).Message, StringComparison.Ordinal);
    }

    /// <summary>Local XML rebinding cannot change a fixed declaration identity.</summary>
    [Fact]
    public void LocalRebindingIsIgnored() => AssertValid(Sheet("<xsl:variable version='3.0' xmlns:p='urn:local' name='p:v' select='7'/>" + Output()));

    /// <summary>Explicit and unprefixed names preserve their own namespace identity.</summary>
    /// <param name="name">The declaration name.</param>
    /// <param name="reference">The reference.</param>
    [Theory]
    [InlineData("Q{urn:explicit}v", "$Q{urn:explicit}v")]
    [InlineData("Q{}v", "$v")]
    [InlineData("v", "$v")]
    public void ExplicitIdentityIsRetained(string name, string reference)
        => AssertValid(Sheet("<xsl:variable version='3.0' name='" + name + "' select='7'/>" + Output(reference)).Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:elements'"));

    /// <summary>Malformed lower-profile declaration names retain source-local static syntax errors.</summary>
    /// <param name="name">The malformed name.</param>
    [Theory]
    [InlineData("p:a:b")]
    [InlineData("Q{urn:fixed}")]
    [InlineData("1bad")]
    public void MalformedNamesHaveDiagnostics(string name)
    {
        var text = Sheet("<xsl:template match='/' version='3.0'><xsl:variable name='" + name + "' select='7'/><out/></xsl:template>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XTSE0020" && d.Range is not null);
        Assert.Contains("XTSE0020", Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(text)).Message, StringComparison.Ordinal);
    }

    /// <summary>Linked declarations retain their module bindings independently of the principal map.</summary>
    /// <param name="kind">The linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedVariableBindingsAreIndependent(string kind)
    {
        var resolver = new Resolver(new() { ["variables.xsl"] = Sheet("<xsl:variable version='3.0' name='p:v' select='7'/>", "#standard p=urn:module") });
        AssertValid(Sheet("<xsl:" + kind + " href='variables.xsl'/>" + Output(), "#standard q=urn:module"), resolver);
    }

    /// <summary>Namespace documents supply fixed identities once during inspection.</summary>
    [Fact]
    public void NamespaceDocumentIdentityIsCaptured()
        => AssertValid(Sheet("<xsl:variable version='3.0' name='p:v' select='7'/>" + Output(), "#standard bindings.xml"), new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:p='urn:fixed' xmlns:q='urn:fixed'/>" }));

    /// <summary>Function keyword metadata retains locally earlier parameter declaration identities.</summary>
    [Fact]
    public void LocallyEarlierFunctionParameterKeywordsAgree()
        => AssertValid(Sheet("<xsl:function name='p:f'><xsl:param version='3.0' name='p:v'/><xsl:sequence select='$q:v'/></xsl:function>" + Output("p:f(q:v := 7)")));

    /// <summary>Tunnel parameters pass their fixed identities through locally earlier declarations.</summary>
    [Fact]
    public void TunnelParameterIdentityIsRetained()
        => AssertValid(Sheet("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param version='3.0' tunnel='yes' name='q:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target' version='3.0'><xsl:param tunnel='yes' name='p:v'/><xsl:value-of select='$q:v'/></xsl:template>"));

    /// <summary>Later parameter references remain invalid independently of namespace binding corrections.</summary>
    [Fact]
    public void ForwardParameterReferenceIsStillInvalid()
    {
        var text = Sheet("<xsl:template match='/' version='3.0'><xsl:param name='p:a' select='$q:v'/><xsl:param name='p:v' select='7'/><out/></xsl:template>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0008");
    }

    /// <summary>Literal static exclusions do not declare names or report excluded name errors.</summary>
    [Fact]
    public void LiteralExclusionIsRetained()
        => AssertValid(Sheet("<xsl:variable version='3.0' name='p:v' select='7' use-when='false()'/>" + Output("7"), "#standard"));

    /// <summary>Older module roots retain native variable identities.</summary>
    /// <param name="version">The earlier module profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierModuleRetainsNativeIdentity(string version)
        => AssertValid(Sheet("<xsl:variable name='p:v' select='7'/>" + Output("$p:v")).Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", ""));

    /// <summary>Refused namespace documents are not retried during QName checks or registry capture.</summary>
    [Fact]
    public void RefusedNamespaceDocumentIsNotRetried()
    {
        var resolver = new Resolver(new());
        var text = Sheet("<xsl:variable version='3.0' name='p:v' select='7'/>" + Output("7"), "#standard missing.xml");
        var snapshot = Inspect(text, resolver);
        Assert.Equal(1, resolver.Requests);
        for (var i = 0; i < 2; i++)
            Assert.Contains(XsltValidation.Validate(snapshot).Diagnostics, d => d.Code == "XTSE0122" && d.Range is not null);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Fixed aliases still identify duplicate supplied parameters.</summary>
    [Fact]
    public void DuplicateSuppliedParameterIdentityIsRejected()
    {
        var text = Sheet("<xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param version='3.0' name='p:v' select='7'/><xsl:with-param version='3.0' name='q:v' select='7'/></xsl:call-template></out></xsl:template><xsl:template name='target'><xsl:param name='p:v'/></xsl:template>");
        Assert.Contains("XTSE0670", Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(text)).Message, StringComparison.Ordinal);
    }

    /// <summary>The explicit native identity no longer resolves a declaration bound to the fixed set.</summary>
    [Fact]
    public void NativeIdentityDoesNotAliasFixedDeclaration()
    {
        var text = Sheet("<xsl:variable version='3.0' name='p:v' select='7'/>" + Output("$Q{urn:native}v"));
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0008");
    }

    /// <summary>Edited references preserve source bytes and resolve the declaration's fixed identity.</summary>
    [Fact]
    public void ExpressionEditRetainsDeclarationIdentity()
    {
        var text = Sheet("<xsl:variable version='3.0' name='p:v' select='7'/>" + Output("0"));
        var bytes = Encoding.UTF8.GetBytes(text);
        var snapshot = Inspect(text);
        var node = snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "value-of")!;
        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id, "select", "$q:v"));
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var candidate = result.Candidate!;
        var range = Assert.Single(candidate.AffectedRanges);
        var start = checked((int)range.StartByteOffset);
        var end = checked((int)(range.StartByteOffset + range.ByteLength));
        Assert.True(bytes.AsSpan(0, start).SequenceEqual(candidate.EmittedSource.Span[..start]));
        Assert.True(bytes.AsSpan(end).SequenceEqual(candidate.EmittedSource.Span[^(bytes.Length - end)..]));
        Assert.Equal(bytes, snapshot.ExportOriginal(MainUri));
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(candidate.Snapshot).Outcome);
        Assert.Equal("7", XElement.Parse(candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>The locally earlier function parameter cannot be named by its unrelated native identity.</summary>
    [Fact]
    public void NativeKeywordIdentityIsRejected()
    {
        var text = Sheet("<xsl:function name='p:f'><xsl:param version='3.0' name='p:v'/><xsl:sequence select='$q:v'/></xsl:function>" + Output("p:f(Q{urn:native}v := 7)"));
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0017");
    }

    /// <summary>A 4.0 module without fixed bindings retains local native declaration context.</summary>
    [Fact]
    public void NoFixedSetRetainsNativeDeclarations()
        => AssertValid(Sheet("<xsl:template match='/' version='3.0'><xsl:variable name='p:v' select='7'/><out><xsl:value-of select='$p:v'/></out></xsl:template>").Replace(" fixed-namespaces='#standard p=urn:fixed q=urn:fixed'", ""));

    /// <summary>Local earlier grammar continues rejecting function keyword syntax.</summary>
    [Fact]
    public void EarlierKeywordGrammarRemainsRejected()
    {
        var text = Sheet("<xsl:template match='/' version='3.0'><xsl:param name='p:v' select='7'/><out><xsl:value-of select=\"substring(value := string($q:v), start := 1)\"/></out></xsl:template>");
        Assert.Contains(XsltValidation.Validate(Inspect(text)).Diagnostics, d => d.Code == "XPST0003");
    }

    private sealed class Resolver(Dictionary<string, string> resources) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled resource requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!resources.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
