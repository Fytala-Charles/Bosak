// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Validates a price and transforms its schema-derived decimal value.
// SPECIAL NOTES        : Development integration example; no production core API change.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Schema.Example;

/// <summary>Demonstrates XSD 1.0 validation and typed arithmetic through the public Schema factory.</summary>
public static class PriceExample
{
    private const string Schema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   xmlns:p="urn:bosak:example:prices" targetNamespace="urn:bosak:example:prices"
                   elementFormDefault="qualified">
          <xs:element name="price" type="p:PriceType"/>
          <xs:complexType name="PriceType">
            <xs:simpleContent>
              <xs:extension base="xs:decimal">
                <xs:attribute name="currency" type="xs:string" use="required"/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:schema>
        """;

    private const string Stylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                        xmlns:xs="http://www.w3.org/2001/XMLSchema"
                        xmlns:p="urn:bosak:example:prices" exclude-result-prefixes="xs p">
          <xsl:import-schema namespace="urn:bosak:example:prices" schema-location="price.xsd"/>
          <xsl:output omit-xml-declaration="yes"/>
          <xsl:template name="main">
            <xsl:variable name="assessed">
              <xsl:copy-of select="/p:price" validation="strict"/>
            </xsl:variable>
            <result declared-type="{$assessed/* instance of element(*, p:PriceType)}"
                    atomic-decimal="{data($assessed/*) instance of xs:decimal}"
                    amount="{data($assessed/*)}" total="{data($assessed/*) * 1.20}"/>
          </xsl:template>
        </xsl:stylesheet>
        """;

    /// <summary>Validates a copied price node and computes its total using the assessed decimal value.</summary>
    /// <param name="input">Caller-owned XML document containing a price in the example namespace.</param>
    /// <returns>An XML result reporting the named type, atomic type, amount and total.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    /// <remarks>The caller's tree is retained unchanged. Structural validation uses XSD 1.0.</remarks>
    public static string Transform(XDocument input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var compiler = SchemaAwareXslt.CreateCompiler();
        // Serve exactly this in-memory schema. Never return null and trigger URI fallback.
        compiler.SchemaResolver = (ns, hints) =>
            ns == "urn:bosak:example:prices" && hints.SequenceEqual(new[] { "price.xsd" })
                ? new MemoryStream(Encoding.UTF8.GetBytes(Schema), writable: false)
                : throw new InvalidOperationException("The example permits only its bundled price schema.");
        var executable = compiler.Compile(Stylesheet, "urn:bosak:example:prices:stylesheet");
        return executable.TransformToString(new XDocumentNode(input), initialTemplate: "main");
    }
}
