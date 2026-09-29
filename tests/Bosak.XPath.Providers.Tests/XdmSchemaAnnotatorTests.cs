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
//                      | Charles Korthout | 0.2   | 22-10-2026     | REQ-107: canonical PSVI typed-value lexical forms (duration/decimal/anyURI, as-1803)    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 29-09-2026     | REQ-109: extended-year date/time typed values, mixed-content untypedAtomic tag,         |
//                      |                  |       |                | is-id/is-idref surviving annotation stripping                                            |
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

    // ----- REQ-107 (as-1803): canonical lexical forms of PSVI typed values -----

    private static XDocumentNode ValidatedNode(string xsd, XElement element)
    {
        var schemas = CompileSchema(xsd);
        var doc = new XDocument(element);
        doc.Validate(schemas, null, true);
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        return XDocumentNode.Wrap(element);
    }

    [Fact]
    public void TypedValue_Duration_UsesCanonicalXsdForm()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:duration'/>
            </xs:schema>
            """;
        var node = ValidatedNode(xsd, new XElement("d", "-P12M23DT0M59.123S"));
        Assert.Equal("-P1Y23DT59.123S", node.TypedValue.ToString());
    }

    [Fact]
    public void TypedValue_YearMonthOnlyDuration_FoldsMonthsIntoYears()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:duration'/>
            </xs:schema>
            """;
        var node = ValidatedNode(xsd, new XElement("d", "P12M"));
        Assert.Equal("P1Y", node.TypedValue.ToString());
    }

    [Fact]
    public void TypedValue_DayTimeOnlyDuration_OmitsZeroComponents()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:duration'/>
            </xs:schema>
            """;
        var node = ValidatedNode(xsd, new XElement("d", "PT0M59.500S"));
        Assert.Equal("PT59.5S", node.TypedValue.ToString());
    }

    [Fact]
    public void TypedValue_Decimal_StripsTrailingFractionalZeros()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='p' type='xs:decimal'/>
            </xs:schema>
            """;
        var node = ValidatedNode(xsd, new XElement("p", "1000.000"));
        Assert.Equal("1000", node.TypedValue.ToString());
    }

    [Fact]
    public void TypedValue_AnyUri_PreservesLexicalForm()
    {
        // .NET parses xs:anyURI into System.Uri, whose ToString() appends a trailing
        // slash; the typed value must keep the lexical form.
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='u' type='xs:anyURI'/>
            </xs:schema>
            """;
        var node = ValidatedNode(xsd, new XElement("u", "http://www.uri.com"));
        var typed = node.TypedValue;
        Assert.Equal("http://www.uri.com", typed.ToString());
        Assert.Equal("anyURI", typed.SchemaTypeName);
    }

    // ----- REQ-109 (PA-3 tail): extended-year date/time typed values -----
    // .NET parses every XSD date/time datatype into System.DateTime (year 1..9999);
    // XSD years are unbounded, so ParseValue throws for conformant lexicals like
    // -0012-12-03 or 21999-05 and the typed value used to collapse to an untagged string.

    private static XDocumentNode LenientValidatedNode(string xsd, XElement element)
    {
        var schemas = CompileSchema(xsd);
        var doc = new XDocument(element);
        doc.Validate(schemas, (_, _) => { }, true);
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        return XDocumentNode.Wrap(element);
    }

    [Fact]
    public void TypedValue_DateNegativeYear_KeepsDateKindAndAnnotation()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:date'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("d", "-0012-12-03-05:00")).TypedValue;
        Assert.Equal(XdmValueKind.Date, typed.Kind);
        Assert.Equal("date", typed.SchemaTypeName);
    }

    [Fact]
    public void TypedValue_DateTimeNegativeYear_KeepsDateTimeKindAndAnnotation()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='dt' type='xs:dateTime'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("dt", "-0012-01-16T13:20:00Z")).TypedValue;
        Assert.Equal(XdmValueKind.DateTime, typed.Kind);
        Assert.Equal("dateTime", typed.SchemaTypeName);
    }

    [Fact]
    public void TypedValue_GYearNegativeYear_KeepsAnnotation()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='y' type='xs:gYear'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("y", "-0012-05:00")).TypedValue;
        Assert.Equal(XdmValueKind.String, typed.Kind);
        Assert.Equal("gYear", typed.SchemaTypeName);
    }

    [Fact]
    public void TypedValue_GYearMonthExtendedYear_KeepsAnnotation()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='ym' type='xs:gYearMonth'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("ym", "21999-05+14:00")).TypedValue;
        Assert.Equal(XdmValueKind.String, typed.Kind);
        Assert.Equal("gYearMonth", typed.SchemaTypeName);
    }

    [Fact]
    public void TypedValue_DateInRange_StillParsesNormally()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:date'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("d", "2020-06-01")).TypedValue;
        Assert.Equal(XdmValueKind.Date, typed.Kind);
        Assert.Equal("date", typed.SchemaTypeName);
    }

    [Fact]
    public void TypedValue_DateInvalidLexical_FallsBackToUntypedString()
    {
        // A genuinely invalid lexical is not rescued by the extended-year re-parse.
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='d' type='xs:date'/>
            </xs:schema>
            """;
        var typed = LenientValidatedNode(xsd, new XElement("d", "not-a-date")).TypedValue;
        Assert.Equal(XdmValueKind.String, typed.Kind);
        Assert.Null(typed.SchemaTypeName);
    }

    // ----- REQ-109 (PA-3 tail): mixed-content typed value is xs:untypedAtomic -----

    [Fact]
    public void TypedValue_MixedContent_TagsUntypedAtomic()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='m' type='mixedType'/>
                <xs:complexType name='mixedType' mixed='true'>
                    <xs:sequence>
                        <xs:element name='name' type='xs:string'/>
                    </xs:sequence>
                </xs:complexType>
            </xs:schema>
            """;
        var element = new XElement("m", "Mr ", new XElement("name", "Peter"), " has brown hair");
        var typed = LenientValidatedNode(xsd, element).TypedValue;
        Assert.Equal(XdmValueKind.String, typed.Kind);
        Assert.Equal("untypedAtomic", typed.SchemaTypeName);
        Assert.Equal("Mr Peter has brown hair", typed.ToString());
    }

    [Fact]
    public void TypedValue_ElementOnlyContent_HasNoTypedValue()
    {
        const string xsd = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='eo' type='eoType'/>
                <xs:complexType name='eoType'>
                    <xs:sequence>
                        <xs:element name='child' type='xs:string'/>
                    </xs:sequence>
                </xs:complexType>
            </xs:schema>
            """;
        var node = LenientValidatedNode(xsd, new XElement("eo", new XElement("child", "x")));
        Assert.True(node.HasNoTypedValue);
    }

    // ----- REQ-109 (PA-3 tail): is-id / is-idref survive annotation stripping -----

    private const string IdIdrefSchema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xs:element name='doc' type='docType'/>
            <xs:complexType name='docType'>
                <xs:sequence>
                    <xs:element name='id-elem' type='idElemType'/>
                    <xs:element name='plain' type='xs:string' minOccurs='0'/>
                </xs:sequence>
                <xs:attribute name='ident' type='xs:ID'/>
                <xs:attribute name='ref' type='xs:IDREF'/>
            </xs:complexType>
            <xs:complexType name='idElemType'>
                <xs:simpleContent>
                    <xs:extension base='xs:ID'/>
                </xs:simpleContent>
            </xs:complexType>
        </xs:schema>
        """;

    private static XElement ValidatedIdDoc(out XAttribute ident, out XAttribute reference, out XElement idElem, out XElement plain)
    {
        var schemas = CompileSchema(IdIdrefSchema);
        var element = new XElement("doc",
            new XAttribute("ident", "a1"),
            new XAttribute("ref", "a1"),
            new XElement("id-elem", "id1"),
            new XElement("plain", "x"));
        ident = element.Attribute("ident")!;
        reference = element.Attribute("ref")!;
        idElem = element.Element("id-elem")!;
        plain = element.Element("plain")!;
        new XDocument(element).Validate(schemas, (_, _) => { }, true);
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);
        return element;
    }

    [Fact]
    public void Strip_IdAttribute_KeepsIsId()
    {
        var element = ValidatedIdDoc(out var ident, out _, out _, out _);
        XdmSchemaAnnotator.StripSchemaAnnotations(element);
        Assert.True(XDocumentNode.Wrap(ident).IsId);
        Assert.False(XDocumentNode.Wrap(ident).IsIdref);
    }

    [Fact]
    public void Strip_IdrefAttribute_KeepsIsIdref()
    {
        var element = ValidatedIdDoc(out _, out var reference, out _, out _);
        XdmSchemaAnnotator.StripSchemaAnnotations(element);
        Assert.True(XDocumentNode.Wrap(reference).IsIdref);
        Assert.False(XDocumentNode.Wrap(reference).IsId);
    }

    [Fact]
    public void Strip_IdElementContent_KeepsIsId()
    {
        var element = ValidatedIdDoc(out _, out _, out var idElem, out _);
        XdmSchemaAnnotator.StripSchemaAnnotations(element);
        Assert.True(XDocumentNode.Wrap(idElem).IsId);
    }

    [Fact]
    public void Strip_NonIdNode_GetsNoIdProperties()
    {
        var element = ValidatedIdDoc(out _, out _, out _, out var plain);
        XdmSchemaAnnotator.StripSchemaAnnotations(element);
        Assert.False(XDocumentNode.Wrap(plain).IsId);
        Assert.False(XDocumentNode.Wrap(plain).IsIdref);
    }

    [Fact]
    public void Strip_RemovesTypeAnnotations()
    {
        var element = ValidatedIdDoc(out var ident, out _, out _, out _);
        XdmSchemaAnnotator.StripSchemaAnnotations(element);
        Assert.Null(element.GetSchemaInfo());
        Assert.Null(ident.GetSchemaInfo());
    }
}
