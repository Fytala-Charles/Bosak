// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 November 2026
// PURPOSE              : Tests independent observed output delivery and serialization-error timing.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 12-11-2026     | Check principal raw output precedence and the ordinary-template fallback.                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Runtime;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks output isolation and production-time serialization for internal observed execution.</summary>
public class ObservedOutputTests
{
    /// <summary>Explicit principal raw output survives the enclosing initial-template accumulator.</summary>
    /// <param name="explicitPrincipal">Whether output is produced by an explicit principal result document.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RawInitialTemplateRetainsPrincipalValue(bool explicitPrincipal)
    {
        var content = explicitPrincipal
            ? "<xsl:result-document method='json'><xsl:sequence select=\"map{'value':9}\"/></xsl:result-document>"
            : "<xsl:sequence select=\"map{'value':9}\"/>";
        var executable = new XsltCompiler().Compile(XDocument.Parse($"""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output method="json"/>
              <xsl:template name="main">{content}</xsl:template>
            </xsl:stylesheet>
            """));
        var value = executable.Transform(null, initialTemplate: "main", rawResult: true);
        var properties = executable.LastResultDocumentProperties ?? executable.OutputProperties;
        Assert.Equal("{\"value\":9}", ResultTreeSerializer.Serialize(value, properties));
    }

    /// <summary>A logical URI that names an expected file never overwrites that file.</summary>
    [Fact]
    public void ObservedOutputKeepsExpectedFileIndependent()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "BosakObserved-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        try
        {
            var expectedPath = Path.Combine(root, "expected.xml");
            File.WriteAllText(expectedPath, "<expected/>");
            var expectedBytes = File.ReadAllBytes(expectedPath);
            var baseUri = new Uri(root + Path.DirectorySeparatorChar).AbsoluteUri;
            var executable = new XsltCompiler().Compile(XDocument.Parse("""
                <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
                  <xsl:template name="main"><main/>
                    <xsl:result-document href="expected.xml"><actual/></xsl:result-document>
                  </xsl:template>
                </xsl:stylesheet>
                """));
            string? observedUri = null;
            string? observedText = null;
            var result = executable.TransformObserved(null, null, new EvaluationContext(), "main", null,
                null, null, false, baseUri, null, (uri, value, properties) =>
                {
                    observedUri = uri;
                    observedText = ResultTreeSerializer.Serialize(value, properties);
                });
            Assert.Equal(new Uri(new Uri(baseUri), "expected.xml").AbsoluteUri, observedUri);
            Assert.Contains("<actual", observedText);
            Assert.Contains("<main", result.StringValue);
            Assert.Equal(expectedBytes, File.ReadAllBytes(expectedPath));
        }
        finally
        {
            var tempRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            Assert.StartsWith(tempRoot, root, StringComparison.OrdinalIgnoreCase);
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Serialization errors from a secondary observer are raised while xsl:try is active.</summary>
    [Fact]
    public void ObservedSerializationErrorRemainsCatchable()
    {
        var executable = new XsltCompiler().Compile(XDocument.Parse("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                            xmlns:err="http://www.w3.org/2005/xqt-errors" version="3.0">
              <xsl:template name="main">
                <xsl:try>
                  <xsl:result-document href="secondary.xml" method="xml" build-tree="no">
                    <xsl:sequence select="map{'a':1}"/>
                  </xsl:result-document>
                  <unexpected/>
                  <xsl:catch errors="err:SENR0001"><caught/></xsl:catch>
                </xsl:try>
              </xsl:template>
            </xsl:stylesheet>
            """));
        var result = executable.TransformObserved(null, null, new EvaluationContext(), "main", null,
            null, null, false, "file:///bosak-observed/principal.xml", null,
            (_, value, properties) => ResultTreeSerializer.Serialize(value, properties));
        Assert.Contains("<caught", result.StringValue);
        Assert.DoesNotContain("<unexpected", result.StringValue);
    }
}
