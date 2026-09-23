// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Unit tests for XdmSchemaAnnotator subtree validation and schema annotation (REQ-098 seam H3)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests;

/// <summary>
/// Tests for <see cref="XdmSchemaAnnotator"/> (REQ-098 seam H3): subtree validation attaches
/// PSVI to the live XObjects so every typed-value surface works without a round-trip.
/// </summary>
public class XdmSchemaAnnotatorTests
{
    // amount: complex type with simple content (extension of xs:decimal plus a required
    // currency attribute) — proves complex-type annotation surfaces through TypedValue.
    private const string AmountSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
        <xs:element name='amount' type='money'/>
        <xs:complexType name='money'>
            <xs:simpleContent>
                <xs:extension base='xs:decimal'>
                    <xs:attribute name='currency' type='xs:string' use='required'/>
                    <xs:attribute name='code' type='xs:integer'/>
                </xs:extension>
            </xs:simpleContent>
        </xs:complexType>
    </xs:schema>";

    private static XmlSchemaSet CompileSchema(string xsd)
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xsd)), null)!);
        set.Compile();
        return set;
    }

    // ----- ValidateSubtree: valid content -----

    [Fact]
    public void ValidateSubtree_ValidContent_AttachesPsviToAttachedElement()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount",
            new XAttribute("currency", "EUR"),
            new XText("12.5"));
        var doc = new XDocument(element);

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        var node = XDocumentNode.Wrap(element);
        Assert.True(node.IsComplexType);
        Assert.Equal(("", "money"), node.SchemaTypeAnnotation);
        var typed = node.TypedValue;
        Assert.Equal(XdmValueKind.Decimal, typed.Kind);
        Assert.Equal("12.5", typed.ToString());
    }

    [Fact]
    public void ValidateSubtree_ValidContent_AttachesPsviToDetachedElement()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount", new XAttribute("currency", "EUR"), "12.5");

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        Assert.Equal(("", "money"), XDocumentNode.Wrap(element).SchemaTypeAnnotation);
    }

    [Fact]
    public void ValidateSubtree_InvalidContent_InvokesSuppliedHandler()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount", "abc");
        var seen = new List<ValidationEventArgs>();

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas, (sender, e) => seen.Add(e));

        Assert.False(result.IsValid);
        Assert.NotEmpty(seen);
        Assert.Equal(result.Errors.Count, seen.Count(e => e.Severity == XmlSeverityType.Error));
    }

    // ----- ValidateSubtree: invalid content -----

    [Fact]
    public void ValidateSubtree_InvalidContent_ReportsErrorsAndAnnotatesAnyway()
    {
        var schemas = CompileSchema(AmountSchema);
        // Bad decimal content and missing required currency attribute.
        var element = new XElement("amount", "abc");
        var doc = new XDocument(element);

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.All(result.Errors, e => Assert.Equal(XmlSeverityType.Error, e.Severity));
        // Node identity is preserved and partial PSVI is still attached to the live node.
        Assert.Same(doc, element.Document);
        Assert.NotNull(element.GetSchemaInfo());
    }

    [Fact]
    public void ValidateSubtree_InvalidContent_ThrowOnInvalidThrows()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount", "abc");

        var ex = Assert.Throws<XmlSchemaValidationException>(
            () => XdmSchemaAnnotator.ValidateSubtree(element, schemas, throwOnInvalid: true));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void ValidateSubtree_ValidContent_ThrowOnInvalidDoesNotThrow()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount", new XAttribute("currency", "EUR"), "12.5");

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas, throwOnInvalid: true);

        Assert.True(result.IsValid);
    }

    // ----- ValidateSubtree: attribute annotation -----

    [Fact]
    public void ValidateSubtree_AttributesOfValidatedElement_AreAnnotated()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount",
            new XAttribute("currency", "EUR"),
            new XAttribute("code", "978"),
            "12.5");

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        var codeAttr = element.Attribute("code")!;
        Assert.NotNull(codeAttr.GetSchemaInfo());
        var attrNode = XDocumentNode.Wrap(codeAttr);
        Assert.Equal(("", "code"), attrNode.SchemaAttributeDeclaration);
        Assert.Equal(XdmValueKind.Integer, attrNode.TypedValue.Kind);
        Assert.Equal("978", attrNode.TypedValue.ToString());
    }

    // ----- Annotate: host-built IXmlSchemaInfo -----

    private sealed class StubSchemaInfo : IXmlSchemaInfo
    {
        public XmlSchemaValidity Validity => XmlSchemaValidity.Valid;
        public bool IsDefault => false;
        public bool IsNil => false;
        public XmlSchemaSimpleType? MemberType => null;
        public XmlSchemaType SchemaType => XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);
        public XmlSchemaElement? SchemaElement => null;
        public XmlSchemaAttribute? SchemaAttribute => null;
    }

    [Fact]
    public void Annotate_HandWrittenInfo_IsSurfacedByEngine()
    {
        var element = new XElement("value", "42.5");

        XdmSchemaAnnotator.Annotate(element, new StubSchemaInfo());

        var node = XDocumentNode.Wrap(element);
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "decimal"), node.SchemaTypeAnnotation);
        Assert.Equal(XdmValueKind.Decimal, node.TypedValue.Kind);
        Assert.Equal("42.5", node.TypedValue.ToString());
    }

    [Fact]
    public void Annotate_AttributeNode_IsSurfacedByEngine()
    {
        var attr = new XAttribute("value", "42.5");

        XdmSchemaAnnotator.Annotate(attr, new StubSchemaInfo());

        var node = XDocumentNode.Wrap(attr);
        Assert.Equal(XdmValueKind.Decimal, node.TypedValue.Kind);
    }

    // ----- argument validation -----

    [Fact]
    public void ValidateSubtree_NullElement_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => XdmSchemaAnnotator.ValidateSubtree(null!, CompileSchema(AmountSchema)));
    }

    [Fact]
    public void ValidateSubtree_NullSchemas_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => XdmSchemaAnnotator.ValidateSubtree(new XElement("amount"), null!));
    }

    [Fact]
    public void Annotate_NullNode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.Annotate(null!, new StubSchemaInfo()));
    }

    [Fact]
    public void Annotate_NullAnnotation_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.Annotate(new XElement("a"), null!));
    }
}
