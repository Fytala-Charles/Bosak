// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Tests stylesheet-context diagnostics in the language server.
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

using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

/// <summary>Verifies editor diagnostics use the effective stylesheet context.</summary>
public class DiagnosticsHandlerTests
{
    private static async Task<Diagnostic[]> Diagnose(string text)
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/main.xsl");
        var documents = new DocumentManager();
        documents.Update(uri.ToString(), text);
        var result = await new DiagnosticsHandler(documents).Handle(new DocumentDiagnosticParams
        { TextDocument = new TextDocumentIdentifier(uri) }, default);
        var report = Assert.IsAssignableFrom<RelatedFullDocumentDiagnosticReport>(result);
        var json = Newtonsoft.Json.Linq.JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(report));
        Assert.Equal("full", (string?)json["kind"]);
        Assert.Equal(report.Items.Count(), json["items"]!.Count());
        return report.Items.ToArray();
    }
    private static string Sheet(string body, string version = "4.0") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:p' version='" + version + "'>" + body + "</xsl:stylesheet>";

    /// <summary>Reduced arity and keywords see the declared optional parameter.</summary>
    [Fact]
    public async Task OptionalKeywordCallsAreValid()
    {
        var text = Sheet("""<xsl:function name='p:f'><xsl:param name='a'/><xsl:param name='b' required='no' select="'B'"/><xsl:sequence select='$a,$b'/></xsl:function><xsl:template match='/'><xsl:value-of select="p:f(a := 'A')"/></xsl:template>""");
        Assert.Empty(await Diagnose(text));
    }

    /// <summary>Private names and partial keywords use the XSLT 4.0 host signatures.</summary>
    [Fact]
    public async Task PrivateNoNamespaceCallsAreValid()
    {
        var text = Sheet("<xsl:function name='divide'><xsl:param name='a'/><xsl:param name='b'/><xsl:sequence select='$a div $b'/></xsl:function><xsl:template match='/'><xsl:value-of select='divide(?, b := 3)(9)'/></xsl:template>");
        Assert.Empty(await Diagnose(text));
    }

    /// <summary>Earlier declaration profiles retain the namespace error.</summary>
    [Fact]
    public async Task EarlierProfileRejectsPrivateName()
    {
        var diagnostics = await Diagnose(Sheet("<xsl:function name='divide'><xsl:sequence select='1'/></xsl:function>","3.0"));
        Assert.Contains(diagnostics, d => d.Message.Contains("XTSE0740") && d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>Unknown keywords produce a source-backed error at the expression.</summary>
    [Fact]
    public async Task InvalidKeywordHasSourceLocation()
    {
        var text = Sheet("<xsl:function name='p:f'><xsl:param name='a'/><xsl:sequence select='$a'/></xsl:function>\n<xsl:template match='/'><xsl:value-of select='p:f(missing := 1)'/></xsl:template>");
        var error = Assert.Single((await Diagnose(text)).Where(d => d.Message.Contains("XPST0017")));
        Assert.Equal(1,error.Range.Start.Line);
        Assert.True(error.Range.Start.Character > 0);
    }

    /// <summary>External module references are reported without acquiring their bytes.</summary>
    [Fact]
    public async Task UnresolvedImportProducesCoverageWarning()
    {
        var diagnostics = await Diagnose(Sheet("<xsl:include href='https://example.invalid/functions.xsl'/><xsl:template match='/'/>"));
        Assert.Contains(diagnostics, d => d.Severity == DiagnosticSeverity.Warning);
    }

    /// <summary>Nonliteral static conditions retain explicit incomplete coverage.</summary>
    [Fact]
    public async Task ConditionalDeclarationsProduceCoverageWarning()
    {
        var diagnostics = await Diagnose(Sheet("<xsl:function name='p:f' use-when='system-property(\"xsl:version\") = \"4.0\"'><xsl:sequence select='1'/></xsl:function>"));
        Assert.Contains(diagnostics, d => d.Message.Contains("coverage incomplete") && d.Severity == DiagnosticSeverity.Warning);
    }

    /// <summary>Malformed XML remains an editor error.</summary>
    [Fact]
    public async Task MalformedXmlProducesError() => Assert.Contains(await Diagnose("<xsl:stylesheet"), d => d.Severity == DiagnosticSeverity.Error);
}
