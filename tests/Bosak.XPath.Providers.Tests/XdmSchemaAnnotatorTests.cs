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
//                      | Charles Korthout | 0.4   | 30-09-2026     | REQ-113 (PB-2): ID/IDREF level partition (element vs document episode),                 |
//                      |                  |       |                | DocumentEpisode shape-check skip, CheckDocumentIdentityConstraints, default-ns          |
//                      |                  |       |                | prefix-scan for named-type xsi:type injection                                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 30-09-2026     | REQ-114 (PB-3 C9): default-attribute porting, element-only whitespace stripping,        |
//                      |                  |       |                | GetSchemaContentModel, preserve→xs:anyType, xdt untypedAtomic normalization,            |
//                      |                  |       |                | ref use-site default/fixed pre-injection (RC3)                                           |
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

    // ----- REQ-113 (PB-2): ID/IDREF level partition, document episodes, identity constraints -----

    [Fact]
    public void Validate_ElementLevelDuplicateId_SuppressedButFlagged()
    {
        // XSLT 3.0 §25.4.1.3: raw xs:ID uniqueness is not enforced for element-level
        // validation; the constraint failure surfaces via HasDocumentLevelConstraintFailure.
        var dupSchema = CompileSchema("""
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='doc'>
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name='item' maxOccurs='unbounded'>
                                <xs:complexType>
                                    <xs:attribute name='ref' type='xs:ID'/>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:schema>
            """);
        var element = new XElement("doc",
            new XElement("item", new XAttribute("ref", "a")),
            new XElement("item", new XAttribute("ref", "a")));

        var result = XdmSchemaAnnotator.Validate(element, dupSchema,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.True(result.HasDocumentLevelConstraintFailure);
    }

    [Fact]
    public void Validate_DocumentLevelDuplicateId_SurfacesAsFailure()
    {
        // XSLT 3.0 §25.4.2: the same content validated as a document episode is invalid.
        var dupSchema = CompileSchema("""
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='doc'>
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name='item' maxOccurs='unbounded'>
                                <xs:complexType>
                                    <xs:attribute name='ref' type='xs:ID'/>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:schema>
            """);
        var container = new XElement("__xdm_doc__",
            new XElement("doc",
                new XElement("item", new XAttribute("ref", "a")),
                new XElement("item", new XAttribute("ref", "a"))));

        var result = XdmSchemaAnnotator.Validate(container, dupSchema,
            new XdmValidationOptions(XdmValidationMode.Strict, null, DocumentLevel: true));

        Assert.False(result.IsValid);
        Assert.True(result.HasDocumentLevelConstraintFailure);
        Assert.Contains("is already used as an ID", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_DocumentEpisode_SkipsSecondShapeCheck()
    {
        // A document episode validates the single root directly; the root's own (multi-child)
        // content must not be shape-checked again (validation-0214).
        var htmlSchema = CompileSchema("""
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xs:element name='html'>
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name='head'/>
                            <xs:element name='body'/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:schema>
            """);
        var html = new XElement("html", new XElement("head"), new XElement("body"));

        var result = XdmSchemaAnnotator.Validate(html, htmlSchema,
            new XdmValidationOptions(XdmValidationMode.Strict, null, DocumentLevel: true)
            { DocumentEpisode = true });

        Assert.True(result.IsValid);
        Assert.False(result.HasDocumentLevelConstraintFailure);
    }

    [Fact]
    public void CheckDocumentIdentityConstraints_DuplicateXmlId_ReturnsMessage()
    {
        var schemas = CompileSchema(IdIdrefSchema);
        var root = new XElement("wrapper",
            new XElement("a", new XAttribute(XNamespace.Xml + "id", "k1")),
            new XElement("b", new XAttribute(XNamespace.Xml + "id", "k1")));

        var message = XdmSchemaAnnotator.CheckDocumentIdentityConstraints(root, schemas);

        Assert.NotNull(message);
        Assert.Contains("is already used as an ID", message, StringComparison.Ordinal);
    }

    [Fact]
    public void CheckDocumentIdentityConstraints_DanglingIdrefsType_ReturnsMessage()
    {
        var schemas = CompileSchema(IdIdrefSchema);
        var root = new XElement("wrapper",
            new XElement("e",
                new XAttribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "type", "xs:IDREFS"),
                new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
                "missing"));

        var message = XdmSchemaAnnotator.CheckDocumentIdentityConstraints(root, schemas);

        Assert.NotNull(message);
        Assert.Contains("Reference to undeclared ID is 'missing'", message, StringComparison.Ordinal);
    }

    [Fact]
    public void CheckDocumentIdentityConstraints_ValidIdAndIdrefs_ReturnsNull()
    {
        var schemas = CompileSchema(IdIdrefSchema);
        var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        var xs = "http://www.w3.org/2001/XMLSchema";
        var root = new XElement("wrapper",
            new XElement("target", new XAttribute(XNamespace.Xml + "id", "t1")),
            new XElement("e",
                new XAttribute(xsi + "type", "xs:IDREFS"),
                new XAttribute(XNamespace.Xmlns + "xs", xs),
                "t1"));

        Assert.Null(XdmSchemaAnnotator.CheckDocumentIdentityConstraints(root, schemas));
    }

    [Fact]
    public void Validate_NamedType_DefaultNamespacePrefixScan_ResolvesTypeNamespace()
    {
        // REQ-113 (PB-2): a default-namespace declaration (xmlns='uri') on the element must
        // not be picked up as the prefix for the injected xsi:type (import-schema-072 family).
        var namespacedSchema = CompileSchema("""
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
                       targetNamespace='http://example.com/ns' xmlns:t='http://example.com/ns'
                       elementFormDefault='qualified'>
                <xs:element name='out' type='t:outType'/>
                <xs:complexType name='outType'>
                    <xs:sequence>
                        <xs:element name='code' type='xs:string'/>
                    </xs:sequence>
                </xs:complexType>
            </xs:schema>
            """);
        var ns = "http://example.com/ns";
        var element = new XElement(XNamespace.Get(ns) + "out",
            new XAttribute("xmlns", ns),
            new XElement(XNamespace.Get(ns) + "code", "x"));

        var result = XdmSchemaAnnotator.Validate(element, namespacedSchema,
            new XdmValidationOptions(XdmValidationMode.Strict, new XmlQualifiedName("outType", ns)));

        Assert.True(result.IsValid);
        Assert.False(result.HasDocumentLevelConstraintFailure);
        Assert.DoesNotContain("xmlns", result.FailureMessage ?? "", StringComparison.Ordinal);
    }

    // ----- REQ-114 (PB-3 C9): XSD default attributes port from the validation clone -----

    private const string DefaultAttrSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
        <xs:element name='doc'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element name='item' maxOccurs='unbounded'>
                        <xs:complexType>
                            <xs:attribute name='a' type='xs:string'/>
                            <xs:attribute name='b' type='xs:string' default='B-DEFAULT'/>
                        </xs:complexType>
                    </xs:element>
                </xs:sequence>
            </xs:complexType>
        </xs:element>
    </xs:schema>";

    [Fact]
    public void ValidateSubtree_DefaultAttribute_PortsToLiveTreeWithPsvi()
    {
        var schemas = CompileSchema(DefaultAttrSchema);
        var element = new XElement("doc", new XElement("item", new XAttribute("a", "x")));
        var doc = new XDocument(element);

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        var item = element.Element("item")!;
        // The XSD default attribute added to the validation clone only is ported onto the
        // live element, with its PSVI annotation (validation-0701, import-schema-048).
        var defaultAttr = item.Attribute("b");
        Assert.NotNull(defaultAttr);
        Assert.Equal("B-DEFAULT", defaultAttr!.Value);
        Assert.NotNull(defaultAttr.GetSchemaInfo());
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "string"),
            XDocumentNode.Wrap(defaultAttr).SchemaTypeAnnotation);
        // The existing attribute keeps its value and is paired by name, not position.
        Assert.Equal("x", item.Attribute("a")!.Value);
        Assert.NotNull(item.Attribute("a")!.GetSchemaInfo());
    }

    [Fact]
    public void Validate_Strict_PortsDefaultAttributesToLiveTree()
    {
        var schemas = CompileSchema(DefaultAttrSchema);
        var element = new XElement("doc", new XElement("item", new XAttribute("a", "x")));

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Equal("B-DEFAULT", element.Element("item")!.Attribute("b")?.Value);
    }

    [Fact]
    public void Validate_CloneOnlyNamespaceFixup_DoesNotLeakIntoLiveTree()
    {
        // The clone receives the in-scope namespace bindings for validation (QName content,
        // xsi:type); those clone-only declarations must not be ported back as pseudo-default
        // attributes.
        const string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:q='http://q.com/'>
            <xs:element name='root'>
                <xs:complexType>
                    <xs:attribute name='qn' type='xs:QName'/>
                </xs:complexType>
            </xs:element>
        </xs:schema>";
        var schemas = CompileSchema(xsd);
        var element = new XElement("root", new XAttribute("qn", "q:thing"));
        new XElement("wrap", new XAttribute(XNamespace.Xmlns + "q", "http://q.com/")).Add(element);

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.DoesNotContain(element.Attributes(), a => a.IsNamespaceDeclaration);
        Assert.NotNull(element.Attribute("qn")!.GetSchemaInfo());
    }

    // ----- REQ-114 (PB-3 C9): element-only whitespace stripping on validated subtrees -----

    private const string ContentModelSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
        <xs:element name='elist'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element name='child' type='xs:string'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>
        <xs:element name='mixed'>
            <xs:complexType mixed='true'>
                <xs:sequence>
                    <xs:element name='child' type='xs:string'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>
    </xs:schema>";

    [Fact]
    public void ValidateSubtree_ElementOnlyContent_StripsWhitespaceTextNodes()
    {
        var schemas = CompileSchema(ContentModelSchema);
        var element = XElement.Parse("<elist> <child>x</child> </elist>", LoadOptions.PreserveWhitespace);

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(element.Nodes(),
            n => n is XText text && string.IsNullOrWhiteSpace(text.Value));
    }

    [Fact]
    public void ValidateSubtree_MixedContent_KeepsWhitespaceTextNodes()
    {
        var schemas = CompileSchema(ContentModelSchema);
        var element = XElement.Parse("<mixed> <child>x</child> </mixed>", LoadOptions.PreserveWhitespace);

        var result = XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.True(result.IsValid);
        Assert.Equal(2, element.Nodes().OfType<XText>()
            .Count(t => string.IsNullOrWhiteSpace(t.Value)));
    }

    // ----- REQ-114 (PB-3 C9): GetSchemaContentModel helper for strip-space gating -----

    [Fact]
    public void GetSchemaContentModel_ElementOnlyContent_ReturnsElementOnly()
    {
        var schemas = CompileSchema(ContentModelSchema);
        var element = XElement.Parse("<elist><child>x</child></elist>");
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.Equal(XmlSchemaContentType.ElementOnly, XdmSchemaAnnotator.GetSchemaContentModel(element));
    }

    [Fact]
    public void GetSchemaContentModel_MixedContent_ReturnsMixed()
    {
        var schemas = CompileSchema(ContentModelSchema);
        var element = XElement.Parse("<mixed> <child>x</child> </mixed>", LoadOptions.PreserveWhitespace);
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        Assert.Equal(XmlSchemaContentType.Mixed, XdmSchemaAnnotator.GetSchemaContentModel(element));
    }

    [Fact]
    public void GetSchemaContentModel_UnvalidatedElement_ReturnsNull()
    {
        Assert.Null(XdmSchemaAnnotator.GetSchemaContentModel(new XElement("elist")));
    }

    // ----- REQ-114 (PB-3 C9): preserve annotates unannotated nodes xs:anyType -----

    [Fact]
    public void PreserveSchemaAnnotations_UnannotatedTree_MarksAnyTypeAndUntypedAtomic()
    {
        var element = new XElement("root",
            new XAttribute("a", "1"),
            new XElement("child", "text"));

        var result = XdmSchemaAnnotator.PreserveSchemaAnnotations(element);

        Assert.True(result.IsValid);
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "anyType"),
            XDocumentNode.Wrap(element).SchemaTypeAnnotation);
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "anyType"),
            XDocumentNode.Wrap(element.Element("child")!).SchemaTypeAnnotation);
        // .NET's xdt-namespace untypedAtomic is normalized to xs: at the schema-type surface.
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "untypedAtomic"),
            XDocumentNode.Wrap(element.Attribute("a")!).SchemaTypeAnnotation);
    }

    [Fact]
    public void PreserveSchemaAnnotations_ValidatedSubtree_KeepsExistingPsvi()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("amount", new XAttribute("currency", "EUR"), "12.5");
        XdmSchemaAnnotator.ValidateSubtree(element, schemas);

        XdmSchemaAnnotator.PreserveSchemaAnnotations(element);

        // Existing PSVI annotations survive preserve untouched (import-schema-076 p).
        Assert.Equal(("", "money"), XDocumentNode.Wrap(element).SchemaTypeAnnotation);
    }

    // ----- REQ-114 (PB-3 C9): xs:untypedAtomic normalization (validation-0108) -----

    private static readonly XmlQualifiedName UntypedAtomicTypeName =
        new("untypedAtomic", "http://www.w3.org/2001/XMLSchema");

    [Fact]
    public void ValidateAttribute_UntypedAtomicType_NormalizesToXsNamespace()
    {
        var schemas = CompileSchema(AmountSchema);
        var attribute = new XAttribute("u", "abcd");

        var result = XdmSchemaAnnotator.ValidateAttribute(attribute, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, UntypedAtomicTypeName));

        Assert.True(result.IsValid);
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "untypedAtomic"),
            XDocumentNode.Wrap(attribute).SchemaTypeAnnotation);
    }

    [Fact]
    public void Validate_UntypedAtomicElementType_NormalizesToXsNamespace()
    {
        var schemas = CompileSchema(AmountSchema);
        var element = new XElement("u", "abcd");

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict, UntypedAtomicTypeName));

        Assert.True(result.IsValid);
        Assert.Equal(("http://www.w3.org/2001/XMLSchema", "untypedAtomic"),
            XDocumentNode.Wrap(element).SchemaTypeAnnotation);
    }

    // ----- REQ-114 (RC3): ref + use-site default/fixed pre-injection -----

    private const string RefGlobalSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
        targetNamespace='http://p.com/' xmlns:p='http://p.com/'>
        <xs:attribute name='foo' type='xs:string'/>
    </xs:schema>";

    private const string RefDefaultUserSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
        xmlns:p='http://p.com/'>
        <xs:import namespace='http://p.com/'/>
        <xs:element name='holder'>
            <xs:complexType>
                <xs:attribute ref='p:foo' default='fred'/>
            </xs:complexType>
        </xs:element>
    </xs:schema>";

    private static XmlSchemaSet CompileSchemas(params string[] xsds)
    {
        var set = new XmlSchemaSet();
        foreach (var xsd in xsds)
            set.Add(XmlSchema.Read(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xsd)), null)!);
        set.Compile();
        return set;
    }

    [Fact]
    public void Validate_RefUseSiteDefault_PreInjectsAndPortsAttribute()
    {
        // Without the pre-injection, .NET's GetUnspecifiedDefaultAttributes crashes with
        // ArgumentNullException (new XAttribute(name, null)) on the ref use (import-schema-164).
        var schemas = CompileSchemas(RefGlobalSchema, RefDefaultUserSchema);
        var element = new XElement("holder");
        _ = new XDocument(element);

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        var attr = element.Attribute(XNamespace.Get("http://p.com/") + "foo");
        Assert.NotNull(attr);
        Assert.Equal("fred", attr!.Value);
        Assert.NotNull(attr.GetSchemaInfo());
        // XSD 1.1-style namespace fixup: the result is serializable with a declared prefix.
        Assert.Contains("xmlns", element.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_RefUseSiteDefault_ExistingAttributeIsKept()
    {
        var schemas = CompileSchemas(RefGlobalSchema, RefDefaultUserSchema);
        var p = XNamespace.Get("http://p.com/");
        var element = new XElement("holder",
            new XAttribute(XNamespace.Xmlns + "p", "http://p.com/"),
            new XAttribute(p + "foo", "custom"));

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Single(element.Attributes(), a => !a.IsNamespaceDeclaration);
        Assert.Equal("custom", element.Attribute(p + "foo")?.Value);
    }

    [Fact]
    public void Validate_RefUseSiteFixed_XmlSpace_IsNotInjectedIntoLiveTree()
    {
        // validation-0201/0202 shape: <xs:attribute ref='xml:space' fixed='preserve'/> on an
        // anonymous complex type (xhtml1-transitional style/script). The injection only
        // keeps .NET's validator from crashing; Saxon semantics (pinned by the catalog)
        // leave fixed values OUT of the validated infoset.
        const string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:x='http://www.w3.org/XML/1998/namespace'
            targetNamespace='http://www.w3.org/XML/1998/namespace'>
            <xs:attribute name='space' type='xs:string'/>
        </xs:schema>";
        const string user = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:xml='http://www.w3.org/XML/1998/namespace'>
            <xs:import namespace='http://www.w3.org/XML/1998/namespace'/>
            <xs:element name='style'>
                <xs:complexType mixed='true'>
                    <xs:attribute ref='xml:space' fixed='preserve'/>
                </xs:complexType>
            </xs:element>
        </xs:schema>";
        var schemas = CompileSchemas(xsd, user);
        var element = new XElement("style", "body{}");

        var result = XdmSchemaAnnotator.Validate(element, schemas,
            new XdmValidationOptions(XdmValidationMode.Strict));

        Assert.True(result.IsValid);
        Assert.Null(element.Attribute(XNamespace.Xml + "space"));
    }
}
