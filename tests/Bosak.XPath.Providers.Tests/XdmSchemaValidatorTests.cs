// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : Unit tests for the XdmSchemaAnnotator validation-mode service (REQ-099 seam H4)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests;

/// <summary>
/// Tests for the REQ-099 seam H4 validation service: <see cref="XdmSchemaAnnotator.Validate(XElement, XmlSchemaSet, XdmValidationOptions, ValidationEventHandler?, bool)"/>
/// and <see cref="XdmSchemaAnnotator.ValidateAttribute(XAttribute, XmlSchemaSet, XdmValidationOptions, ValidationEventHandler?, bool)"/>
/// cover strict/lax/named-type/document-level validation plus the strip and preserve modes.
/// </summary>
public class XdmSchemaValidatorTests
{
    private const string TestNs = "urn:h4";

    private const string OrderSchema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:h4' xmlns:t='urn:h4' elementFormDefault='qualified'>
            <xs:element name='order' type='t:orderType'/>
            <xs:complexType name='orderType'>
                <xs:sequence>
                    <xs:element name='id' type='xs:integer'/>
                    <xs:element name='amount' type='xs:decimal' minOccurs='0'/>
                </xs:sequence>
                <xs:attribute name='code' type='xs:string' use='required'/>
            </xs:complexType>
            <xs:element name='amount' type='xs:decimal'/>
            <xs:simpleType name='size'>
                <xs:restriction base='xs:integer'>
                    <xs:maxInclusive value='10'/>
                </xs:restriction>
            </xs:simpleType>
            <xs:simpleType name='codeList'>
                <xs:list itemType='xs:QName'/>
            </xs:simpleType>
            <xs:attribute name='limit' type='xs:integer'/>
        </xs:schema>
        """;

    private static readonly XName LimitName = XNamespace.Get(TestNs) + "limit";

    private static XmlSchemaSet CompileOrderSchema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(OrderSchema), null)!);
        set.Compile();
        return set;
    }

    private static XElement OrderElement(string id = "7", string amount = "12.5")
        => new(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "A1"),
            new XElement(XNamespace.Get(TestNs) + "id", id),
            amount.Length == 0 ? null : new XElement(XNamespace.Get(TestNs) + "amount", amount));

    // ----- Validate: strict mode -----

    [Fact]
    public void Validate_Strict_ValidAttachedElement_AnnotatesLiveTree()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        var doc = new XDocument(new XElement("wrapper", element));

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        // Node identity is preserved and the live element carries the PSVI.
        Assert.Same(doc.Root, element.Parent);
        Assert.Equal((TestNs, "orderType"), XDocumentNode.Wrap(element).SchemaTypeAnnotation);
        // Descendants are annotated too: id is xs:integer.
        var id = element.Element(XNamespace.Get(TestNs) + "id")!;
        Assert.Equal(XdmValueKind.Integer, XDocumentNode.Wrap(id).TypedValue.Kind);
    }

    [Fact]
    public void Validate_Strict_InvalidContent_ReportsErrors()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement(id: "not-an-integer");

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.FailureMessage);
    }

    [Fact]
    public void Validate_Strict_UndeclaredRoot_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var element = new XElement("undeclared", "text");

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Strict_MissingRequiredAttribute_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        element.Attribute("code")!.Remove();

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.False(result.IsValid);
    }

    // ----- Validate: lax mode -----

    [Fact]
    public void Validate_Lax_UndeclaredRoot_ValidatesDeclaredDescendants()
    {
        var schemas = CompileOrderSchema();
        // The root has no global declaration; the amount child does (global element).
        var element = new XElement("undeclared",
            new XElement(XNamespace.Get(TestNs) + "amount", "12.5"));

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Lax));

        Assert.True(result.IsValid);
        var amount = element.Element(XNamespace.Get(TestNs) + "amount")!;
        Assert.Equal(XdmValueKind.Decimal, XDocumentNode.Wrap(amount).TypedValue.Kind);
    }

    [Fact]
    public void Validate_Lax_InvalidDeclaredDescendant_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var element = new XElement("undeclared",
            new XElement(XNamespace.Get(TestNs) + "amount", "not-a-decimal"));

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Lax));

        Assert.False(result.IsValid);
    }

    // ----- Validate: named type -----

    [Fact]
    public void Validate_NamedType_ValidContent_AttachesTypeAnnotationWithoutXsiTypeLeak()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        var doc = new XDocument(element);

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("orderType", TestNs)));

        Assert.True(result.IsValid);
        Assert.Equal((TestNs, "orderType"), XDocumentNode.Wrap(element).SchemaTypeAnnotation);
        // The injected xsi:type (and any added namespace declarations) must not leak.
        Assert.Null(element.Attribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "type"));
        Assert.DoesNotContain(element.Attributes(), a => a.IsNamespaceDeclaration && a.Value == TestNs);
    }

    [Fact]
    public void Validate_NamedType_InvalidContent_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement(id: "xyz");

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("orderType", TestNs)));

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_NamedType_BuiltInType_WorksWithoutDeclaration()
    {
        var schemas = CompileOrderSchema();
        var element = new XElement("value", "42");

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("integer", "http://www.w3.org/2001/XMLSchema")));

        Assert.True(result.IsValid);
        Assert.Equal(XdmValueKind.Integer, XDocumentNode.Wrap(element).TypedValue.Kind);
    }

    [Fact]
    public void Validate_NamedType_SimpleTypeOnElementWithElementChildren_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var element = new XElement("z", "abcd", new XElement("a"), "wxyz");

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict,
                new XmlQualifiedName("untypedAtomic", "http://www.w3.org/2001/XMLSchema")));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NamedType_UnknownType_ThrowsArgumentException()
    {
        var schemas = CompileOrderSchema();
        Assert.Throws<ArgumentException>(() => XdmSchemaAnnotator.Validate(new XElement("z"), schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("nope", TestNs))));
    }

    // ----- Validate: document level -----

    [Fact]
    public void Validate_DocumentLevel_SingleElementChild_Validates()
    {
        var schemas = CompileOrderSchema();
        var container = new XElement("doc-container", OrderElement());

        var result = XdmSchemaAnnotator.Validate(container, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, DocumentLevel: true));

        Assert.True(result.IsValid);
        Assert.Equal((TestNs, "orderType"),
            XDocumentNode.Wrap(container.Element(XNamespace.Get(TestNs) + "order")!).SchemaTypeAnnotation);
    }

    [Fact]
    public void Validate_DocumentLevel_TwoElementChildren_FailsShapeCheck()
    {
        var schemas = CompileOrderSchema();
        var container = new XElement("doc-container", OrderElement(), OrderElement());

        var result = XdmSchemaAnnotator.Validate(container, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, DocumentLevel: true));

        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.FailureMessage);
    }

    [Fact]
    public void Validate_DocumentLevel_TextChild_FailsShapeCheck()
    {
        var schemas = CompileOrderSchema();
        var container = new XElement("doc-container", OrderElement(), new XText("trailing"));

        var result = XdmSchemaAnnotator.Validate(container, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, DocumentLevel: true));

        Assert.False(result.IsValid);
        Assert.NotNull(result.FailureMessage);
    }

    // ----- Validate: strip and preserve modes -----

    [Fact]
    public void Validate_Strip_RemovesExistingAnnotations()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        Assert.NotNull(element.GetSchemaInfo());

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strip));

        Assert.True(result.IsValid);
        Assert.Null(element.GetSchemaInfo());
        Assert.Null(element.Element(XNamespace.Get(TestNs) + "id")!.GetSchemaInfo());
    }

    [Fact]
    public void Validate_Preserve_KeepsExistingAnnotations()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        var before = element.GetSchemaInfo();

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Preserve));

        Assert.True(result.IsValid);
        Assert.Same(before, element.GetSchemaInfo());
    }

    // ----- Validate: element-only whitespace stripping -----

    [Fact]
    public void Validate_Strict_ElementOnlyContent_WhitespaceTextNodesRemoved()
    {
        var schemas = CompileOrderSchema();
        var element = new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "A1"),
            new XText("  "),
            new XElement(XNamespace.Get(TestNs) + "id", "7"),
            new XText("\n  "),
            new XElement(XNamespace.Get(TestNs) + "amount", "12.5"),
            new XText("\n"));

        var result = XdmSchemaAnnotator.Validate(element, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.DoesNotContain(element.Nodes(), n => n is XText);
    }

    // ----- Validate: throwOnInvalid and handler -----

    [Fact]
    public void Validate_InvalidContent_ThrowOnInvalidThrows()
    {
        var schemas = CompileOrderSchema();
        var ex = Assert.Throws<XmlSchemaValidationException>(() =>
            XdmSchemaAnnotator.Validate(OrderElement(id: "xyz"), schemas,
                new XdmValidationOptions(XdmValidationMode.Strict), throwOnInvalid: true));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Validate_InvalidContent_HandlerReceivesEvents()
    {
        var schemas = CompileOrderSchema();
        var seen = new List<ValidationEventArgs>();

        var result = XdmSchemaAnnotator.Validate(OrderElement(id: "xyz"), schemas,
            new XdmValidationOptions(XdmValidationMode.Strict), (sender, e) => seen.Add(e));

        Assert.False(result.IsValid);
        Assert.NotEmpty(seen);
    }

    // ----- ValidateAttribute -----

    [Fact]
    public void ValidateAttribute_Declaration_ValidValue_Annotates()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute(LimitName, "42");

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Equal(XdmValueKind.Integer, XDocumentNode.Wrap(attr).TypedValue.Kind);
        Assert.Equal((TestNs, "limit"), XDocumentNode.Wrap(attr).SchemaAttributeDeclaration);
    }

    [Fact]
    public void ValidateAttribute_Declaration_InvalidValue_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute(LimitName, "not-an-integer");

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateAttribute_Strict_NoDeclaration_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("undeclared", "42");

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.FailureMessage);
    }

    [Fact]
    public void ValidateAttribute_Lax_NoDeclaration_IsValidAndUntyped()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("undeclared", "42");

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Lax));

        Assert.True(result.IsValid);
        Assert.Null(attr.GetSchemaInfo());
    }

    [Fact]
    public void ValidateAttribute_NamedType_ValidValue_Annotates()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("anything", "7");

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("size", TestNs)));

        Assert.True(result.IsValid);
        Assert.Equal((TestNs, "size"), XDocumentNode.Wrap(attr).SchemaTypeAnnotation);
        Assert.Equal(XdmValueKind.Integer, XDocumentNode.Wrap(attr).TypedValue.Kind);
    }

    [Fact]
    public void ValidateAttribute_NamedType_InvalidValue_IsInvalid()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("anything", "11"); // exceeds maxInclusive 10

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("size", TestNs)));

        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.FailureMessage);
    }

    [Fact]
    public void ValidateAttribute_NamedType_ComplexType_ThrowsArgumentException()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("anything", "7");

        Assert.Throws<ArgumentException>(() => XdmSchemaAnnotator.ValidateAttribute(attr, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("orderType", TestNs))));
    }

    [Fact]
    public void ValidateAttribute_NamedType_UnknownType_ThrowsArgumentException()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute("anything", "7");

        Assert.Throws<ArgumentException>(() => XdmSchemaAnnotator.ValidateAttribute(attr, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("nope", TestNs))));
    }

    [Fact]
    public void ValidateAttribute_ParentedAttribute_ParentUntouched()
    {
        var schemas = CompileOrderSchema();
        var parent = new XElement("p", new XAttribute(LimitName, "42"));
        var attr = parent.Attribute(LimitName)!;

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Same(parent, attr.Parent);
        Assert.Single(parent.Attributes());
        Assert.Null(parent.GetSchemaInfo());
    }

    [Fact]
    public void ValidateAttribute_InvalidValue_ThrowOnInvalidThrows()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute(LimitName, "xyz");

        Assert.Throws<XmlSchemaValidationException>(() =>
            XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict), throwOnInvalid: true));
    }

    [Fact]
    public void ValidateAttribute_Strip_RemovesAnnotation()
    {
        var schemas = CompileOrderSchema();
        var attr = new XAttribute(LimitName, "42");
        XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strict));
        Assert.NotNull(attr.GetSchemaInfo());

        var result = XdmSchemaAnnotator.ValidateAttribute(attr, schemas, new XdmValidationOptions(XdmValidationMode.Strip));

        Assert.True(result.IsValid);
        Assert.Null(attr.GetSchemaInfo());
    }

    // ----- StripSchemaAnnotations -----

    [Fact]
    public void StripSchemaAnnotations_Document_StripsElementsAndAttributes()
    {
        var schemas = CompileOrderSchema();
        var element = OrderElement();
        var doc = new XDocument(element);
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        Assert.NotNull(element.GetSchemaInfo());

        XdmSchemaAnnotator.StripSchemaAnnotations(doc);

        Assert.Null(element.GetSchemaInfo());
        Assert.All(element.Descendants(), e => Assert.Null(e.GetSchemaInfo()));
        Assert.All(element.DescendantsAndSelf().Attributes(), a => Assert.Null(a.GetSchemaInfo()));
    }

    // ----- IsQNameOrNotationDerived -----

    [Fact]
    public void IsQNameOrNotationDerived_QNameAndDerivedTypes_AreDetected()
    {
        var schemas = CompileOrderSchema();
        var qname = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.QName);
        var listOfQName = (XmlSchemaSimpleType)schemas.GlobalTypes[new XmlQualifiedName("codeList", TestNs)]!;
        var str = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
        var orderType = (XmlSchemaComplexType)schemas.GlobalTypes[new XmlQualifiedName("orderType", TestNs)]!;

        Assert.True(XdmSchemaAnnotator.IsQNameOrNotationDerived(qname));
        Assert.True(XdmSchemaAnnotator.IsQNameOrNotationDerived(listOfQName));
        Assert.False(XdmSchemaAnnotator.IsQNameOrNotationDerived(str));
        Assert.False(XdmSchemaAnnotator.IsQNameOrNotationDerived(orderType));
    }

    // ----- argument validation -----

    [Fact]
    public void Validate_NullArguments_Throw()
    {
        var schemas = CompileOrderSchema();
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.Validate(null!, schemas, new XdmValidationOptions(XdmValidationMode.Strict)));
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.Validate(new XElement("z"), null!, new XdmValidationOptions(XdmValidationMode.Strict)));
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.Validate(new XElement("z"), schemas, null!));
    }

    [Fact]
    public void ValidateAttribute_NullArguments_Throw()
    {
        var schemas = CompileOrderSchema();
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.ValidateAttribute(null!, schemas, new XdmValidationOptions(XdmValidationMode.Strict)));
        Assert.Throws<ArgumentNullException>(() => XdmSchemaAnnotator.ValidateAttribute(new XAttribute("a", "b"), null!, new XdmValidationOptions(XdmValidationMode.Strict)));
    }
}
