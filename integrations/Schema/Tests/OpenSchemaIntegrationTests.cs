// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests real schema validation, typed transformation and explicit staged XSD 1.1 limits.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Document integration cases and use the public focus builder                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.Schema.Example;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.Schema.Tests;

/// <summary>Exercises actual public Schema source against the current core, without activation.</summary>
public class OpenSchemaIntegrationTests
{
    private const string PriceXml = "<price xmlns='urn:bosak:example:prices' currency='EUR'>12.50</price>";

    /// <summary>Proves named typing, decimal arithmetic and retention of the caller's unannotated tree.</summary>
    [Fact]
    public void AssessedCopyHasNamedTypeAndDecimalArithmeticWithoutMutatingInput()
    {
        var input = XDocument.Parse(PriceXml);
        var before = input.ToString(SaveOptions.DisableFormatting);
        var result = XElement.Parse(PriceExample.Transform(input));
        Assert.Equal("true", (string?)result.Attribute("declared-type"));
        Assert.Equal("true", (string?)result.Attribute("atomic-decimal"));
        Assert.Equal(12.50m, (decimal)result.Attribute("amount")!);
        Assert.Equal(15m, (decimal)result.Attribute("total")!);
        Assert.Equal(before, input.ToString(SaveOptions.DisableFormatting));
        Assert.Null(input.Root!.GetSchemaInfo());
    }

    /// <summary>Rejects malformed decimal values, missing required attributes and invalid child content.</summary>
    /// <param name="xml">The structurally invalid input.</param>
    [Theory]
    [InlineData("<price xmlns='urn:bosak:example:prices' currency='EUR'>not-a-decimal</price>")]
    [InlineData("<price xmlns='urn:bosak:example:prices'>12.50</price>")]
    [InlineData("<price xmlns='urn:bosak:example:prices' currency='EUR'><unexpected/></price>")]
    public void InvalidInputFailsStructuralValidation(string xml)
    {
        var exception = Assert.ThrowsAny<Exception>(() => PriceExample.Transform(XDocument.Parse(xml)));
        Assert.Contains("XTTE1510", exception.ToString());
    }

    /// <summary>Preserves ordinary untyped atomization after a separate schema-aware transformation.</summary>
    [Fact]
    public void OrdinaryInputRemainsUntypedAfterSchemaAwareEvaluation()
    {
        var input = XDocument.Parse(PriceXml);
        PriceExample.Transform(input);
        var expression = XPath31Expression.Compile("data(/*) instance of xs:untypedAtomic");
        var result = expression.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(input)), 1, 1));
        Assert.True(result.BooleanValue);
    }

    /// <summary>Checks staged assertion outcomes while retaining the known structural-validation gap.</summary>
    [Fact]
    public void Xsd11AssertionUsesTypedValuesButIsNotStructuralValidation()
    {
        var schema = CompileAssertion("data(amount) instance of xs:decimal and amount gt 0", XsdLanguageVersion.Xsd11);
        Assert.Empty(schema.Diagnostics);
        var service = SchemaServices.CreateValidationService();
        Assert.True(service.Validate(schema, XElement.Parse("<order><amount>2</amount></order>")).IsValid);
        var invalid = service.Validate(schema, XElement.Parse("<order><amount>-1</amount></order>"));
        Assert.False(invalid.IsValid);
        Assert.Contains(invalid.Diagnostics, d => d.Code == SchemaDiagnosticCodes.AssertionViolated);
        // The current staged service does not check extra children: keep this limitation visible.
        Assert.True(service.Validate(schema, XElement.Parse("<order><amount>2</amount><extra/></order>")).IsValid);
    }

    /// <summary>Refuses an XSD 1.1 construct when the caller explicitly selects XSD 1.0.</summary>
    [Fact]
    public void Xsd11ConstructIsExplicitlyRejectedByXsd10Compilation()
    {
        var schema = CompileAssertion("amount gt 0", XsdLanguageVersion.Xsd10);
        Assert.Contains(schema.Diagnostics, d => d.Code == SchemaDiagnosticCodes.ConstructRequiresXsd11);
    }

    /// <summary>Refuses external-document access in the staged assertion environment.</summary>
    [Fact]
    public void StagedAssertionCannotReadExternalDocuments()
    {
        var schema = CompileAssertion("doc('https://example.invalid/not-allowed')", XsdLanguageVersion.Xsd11);
        Assert.Empty(schema.Diagnostics);
        var result = SchemaServices.CreateValidationService().Validate(schema, XElement.Parse("<order><amount>2</amount></order>"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, d => d.Code == SchemaDiagnosticCodes.AssertionRefused);
    }

    private static ICompiledSchema CompileAssertion(string assertion, XsdLanguageVersion version)
    {
        var document = new XElement(XName.Get("schema", "http://www.w3.org/2001/XMLSchema"),
            new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
            XElement.Parse("""
                <xs:element xmlns:xs="http://www.w3.org/2001/XMLSchema" name="order">
                  <xs:complexType><xs:sequence><xs:element name="amount" type="xs:decimal"/></xs:sequence></xs:complexType>
                </xs:element>
                """));
        document.Descendants(XName.Get("complexType", "http://www.w3.org/2001/XMLSchema")).Single()
            .Add(new XElement(XName.Get("assert", "http://www.w3.org/2001/XMLSchema"), new XAttribute("test", assertion)));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.ToString()));
        return SchemaServices.CreateSchemaCompiler().Compile(stream, new Uri("urn:bosak:example:assertion"), SchemaCompileOptions.Default(version));
    }
}
