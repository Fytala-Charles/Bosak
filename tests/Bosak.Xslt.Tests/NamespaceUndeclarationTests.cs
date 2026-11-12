// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 November 2026
// PURPOSE              : Tests literal-result prefix undeclarations and XML namespace serialization.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Runtime;
using Bosak.Xslt.Stylesheet;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks prefix shadowing independently from XML version-specific serialization.</summary>
public class NamespaceUndeclarationTests
{
    /// <summary>Raw bindings respect source undeclarations in both XML output versions.</summary>
    /// <param name="version">The output XML version.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("1.1")]
    public void ExplicitUndeclarationSurvivesLiteralNamespaceCollection(string version)
    {
        var executable = new XsltCompiler().Compile($$"""
            <?xml version="1.1"?>
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output method="xml" version="{{version}}" undeclare-prefixes="{{(version == "1.1" ? "yes" : "no")}}" omit-xml-declaration="yes"/>
              <xsl:template name="main">
                <doc xmlns:a="urn:old" xsl:inherit-namespaces="no">
                  <chap xsl:inherit-namespaces="no"><para/><para xmlns:a=""/></chap>
                </doc>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var raw = executable.Transform(null, initialTemplate: "main");
        Assert.Equal("urn:old", Binding(raw, "/doc/chap/para[1]"));
        Assert.Equal("", Binding(raw, "/doc/chap/para[2]"));
        var serialized = executable.TransformToString(null, initialTemplate: "main");
        if (version == "1.1")
            Assert.Contains("xmlns:a=\"\"", serialized);
        else
        {
            Assert.DoesNotContain("xmlns:a=\"\"", serialized);
            Assert.Contains("<chap><para/><para/></chap>", serialized);
        }
    }

    /// <summary>A deeper rebinding overrides an undeclaration without leaking to a sibling.</summary>
    [Fact]
    public void RebindingAfterUndeclarationWinsLocally()
    {
        var executable = new XsltCompiler().Compile("""
            <?xml version="1.1"?>
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output version="1.1" undeclare-prefixes="yes"/>
              <xsl:template name="main">
                <doc xmlns:a="urn:old" xsl:inherit-namespaces="no">
                  <parent xmlns:a="" xsl:inherit-namespaces="no">
                    <leaf xmlns:a="urn:new"><a:child/></leaf><sibling/>
                  </parent>
                </doc>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var raw = executable.Transform(null, initialTemplate: "main");
        Assert.Equal("", Binding(raw, "/doc/parent"));
        Assert.Equal("urn:new", Binding(raw, "/doc/parent/leaf"));
        Assert.Equal("", Binding(raw, "/doc/parent/sibling"));
    }

    /// <summary>An unprefixed attribute named like a shadowed prefix cannot revive that prefix.</summary>
    [Fact]
    public void AttributeHintDoesNotResurrectUndeclaredPrefix()
    {
        var executable = new XsltCompiler().Compile("""
            <?xml version="1.1"?>
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output version="1.1" undeclare-prefixes="yes"/>
              <xsl:template name="main">
                <doc xmlns:a="urn:old" xsl:inherit-namespaces="no"><out xmlns:a="" a="plain"/></doc>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var raw = executable.Transform(null, initialTemplate: "main");
        Assert.Equal("", Binding(raw, "/doc/out"));
        var serialized = executable.TransformToString(null, initialTemplate: "main");
        Assert.Contains("xmlns:a=\"\"", serialized);
        Assert.Contains("a=\"plain\"", serialized);
    }

    /// <summary>Duplicate suppression preserves genuine prefix rebinding and default namespace resets.</summary>
    [Fact]
    public void XmlWriterRetainsRebindingAndDefaultReset()
    {
        var document = XDocument.Parse("""
            <r xmlns="urn:default" xmlns:a="urn:old"><a:child xmlns:a="urn:new"><plain xmlns=""/></a:child></r>
            """);
        var serialized = ResultTreeSerializer.Serialize(XdmValue.FromNode(XDocumentNode.Wrap(document)),
            new OutputProperties { Method = "xml", Version = "1.0", OmitXmlDeclaration = true });
        var roundTrip = XDocument.Parse(serialized);
        Assert.Equal("urn:new", roundTrip.Root!.Elements().Single().Name.NamespaceName);
        Assert.Equal("", roundTrip.Root.Elements().Single().Elements().Single().Name.NamespaceName);
        Assert.Contains("xmlns:a=\"urn:new\"", serialized);
        Assert.Contains("xmlns=\"\"", serialized);
    }

    private static string Binding(XdmValue value, string path)
        => XPath31Expression.Compile($"string(namespace-uri-for-prefix('a', {path}))")
            .Evaluate(value.NodeValue!).StringValue;
}
