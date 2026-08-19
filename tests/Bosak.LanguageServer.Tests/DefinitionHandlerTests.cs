// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Unit tests for the language-server go-to-definition handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class DefinitionHandlerTests
{
    private const string SampleXslt = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
            <xsl:param name="global-param"/>
            <xsl:template name="named"/>
            <xsl:template match="/">
                <xsl:value-of select="$global-param"/>
                <xsl:call-template name="named"/>
            </xsl:template>
        </xsl:stylesheet>
        """;

    [Fact]
    public void FindDefinitionForVariableReference()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/sample.xslt");
        var location = DefinitionHandler.FindDefinition(SampleXslt, "$global-param", uri);
        Assert.NotNull(location);
        Assert.Equal(uri, location!.Uri);
    }

    [Fact]
    public void FindDefinitionForNamedTemplate()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/sample.xslt");
        var location = DefinitionHandler.FindDefinition(SampleXslt, "named", uri);
        Assert.NotNull(location);
    }

    [Fact]
    public void FindDefinitionReturnsNullForUnknownSymbol()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/sample.xslt");
        Assert.Null(DefinitionHandler.FindDefinition(SampleXslt, "$nonexistent", uri));
        Assert.Null(DefinitionHandler.FindDefinition(SampleXslt, "nonexistent-fn", uri));
    }

    [Fact]
    public void FindDefinitionReturnsNullForMalformedXml()
    {
        var uri = DocumentUri.FromFileSystemPath("C:/test/broken.xslt");
        Assert.Null(DefinitionHandler.FindDefinition("<xsl:stylesheet", "$x", uri));
    }
}
