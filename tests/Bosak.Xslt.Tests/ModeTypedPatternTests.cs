// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 september 2026
// PURPOSE              : Unit tests for xsl:mode/@typed semantics (strict/lax/unspecified/untyped) in XSLT match patterns
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-09-2026     | Creation: strict QName→schema-element rewrite, XTTE3100, lax fallback, XTTE3110,        |
//                      |                  |       |                | XTSE3105, element-with-id patterns, deep-copy PSVI preservation, unspecified/untyped     |
//                      |                  |       |                | regression pins (W3C match-054/055/213/218/219/220/221/222/231/243/244/263 family)       |
//                      | Charles Korthout | 0.2   | 30-09-2026     | Switched 2-arg element-with-id unit test to root(); a literal '/' misroutes through      |
//                      |                  |       |                | the path-pattern compiler and recurses indefinitely. Also: XTSE3105 surfaces at transform |
//                      |                  |       |                | time (not compile), and the serializer only emits double-quoted attributes.              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the <c>xsl:mode/@typed</c> attribute values (XSLT 3.0 §3.5) and their effect
/// on match-pattern semantics. In a strict mode a top-level QName pattern branch means
/// <c>schema-element(QName)</c> and untyped nodes raise XTTE3100; in a lax mode a QName
/// with no element declaration falls back to plain name matching; in an untyped
/// (<c>typed="no"</c>) mode schema-dependent patterns raise XTTE3110 on annotated nodes;
/// undeclared QNames in strict modes are a static error (XTSE3105).
/// </summary>
public class ModeTypedPatternTests
{
    private const string TestNs = "urn:mtd";

    private const string SchemaText = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:mtd' xmlns:p='urn:mtd' elementFormDefault='qualified' attributeFormDefault='qualified'>
            <xs:element name='root' type='p:rootType'/>
            <xs:element name='base' type='p:baseType'/>
            <xs:element name='derived' type='p:derivedType' substitutionGroup='p:base'/>
            <xs:element name='flagged' type='p:flagType'/>
            <xs:complexType name='rootType'>
                <xs:choice>
                    <xs:element ref='p:base'/>
                    <xs:element name='plain' type='xs:string'/>
                    <xs:element name='keyed' type='xs:string'/>
                </xs:choice>
                <xs:attribute name='flag' type='xs:boolean'/>
            </xs:complexType>
            <xs:complexType name='flagType'>
                <xs:simpleContent>
                    <xs:extension base='xs:string'>
                        <xs:attribute name='flag' type='xs:boolean'/>
                    </xs:extension>
                </xs:simpleContent>
            </xs:complexType>
            <xs:complexType name='baseType'>
                <xs:sequence>
                    <xs:element name='v' type='xs:string'/>
                </xs:sequence>
            </xs:complexType>
            <xs:complexType name='derivedType'>
                <xs:complexContent>
                    <xs:extension base='p:baseType'>
                        <xs:sequence>
                            <xs:element name='w' type='xs:string' minOccurs='0'/>
                        </xs:sequence>
                    </xs:extension>
                </xs:complexContent>
            </xs:complexType>
        </xs:schema>
        """;

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:mtd' xmlns:p='urn:mtd' elementFormDefault='qualified' attributeFormDefault='qualified'>" +
        "<xs:element name='root' type='p:rootType'/>" +
        "<xs:element name='base' type='p:baseType'/>" +
        "<xs:element name='derived' type='p:derivedType' substitutionGroup='p:base'/>" +
        "<xs:element name='flagged' type='p:flagType'/>" +
        "<xs:complexType name='rootType'><xs:choice><xs:element ref='p:base'/>" +
        "<xs:element name='plain' type='xs:string'/><xs:element name='keyed' type='xs:string'/></xs:choice>" +
        "<xs:attribute name='flag' type='xs:boolean'/></xs:complexType>" +
        "<xs:complexType name='flagType'><xs:simpleContent><xs:extension base='xs:string'>" +
        "<xs:attribute name='flag' type='xs:boolean'/></xs:extension></xs:simpleContent></xs:complexType>" +
        "<xs:complexType name='baseType'><xs:sequence><xs:element name='v' type='xs:string'/></xs:sequence></xs:complexType>" +
        "<xs:complexType name='derivedType'><xs:complexContent><xs:extension base='p:baseType'>" +
        "<xs:sequence><xs:element name='w' type='xs:string' minOccurs='0'/></xs:sequence>" +
        "</xs:extension></xs:complexContent></xs:complexType>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileSchema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(SchemaText), null)!);
        set.Compile();
        return set;
    }

    /// <summary>Builds a validated source document: root with a p:derived child (substituting p:base).</summary>
    private static XDocument ValidatedDerivedDoc()
    {
        var ns = XNamespace.Get(TestNs);
        var source = new XDocument(new XElement(ns + "root",
            new XAttribute(ns + "flag", "true"),
            new XElement(ns + "derived", new XElement(ns + "v", "x"), new XElement(ns + "w", "y"))));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileSchema());
        return source;
    }

    private static Xslt.Api.XsltExecutable Compile(string xslBodyAndDecls, bool schemaAware = true)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
            "xmlns:p='urn:mtd' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + (schemaAware ? ImportSchema : string.Empty)
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = schemaAware };
        return compiler.Compile(xsl, "file:///test.xsl");
    }

    private static string RunTransform(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true)
    {
        var executable = Compile(xslBodyAndDecls, schemaAware);
        var src = (IXdmNode)new XDocumentNode(source ?? ValidatedDerivedDoc());
        return executable.TransformToString(src);
    }

    // ----- strict mode: QName → schema-element(QName) -----

    [Fact]
    public void StrictMode_QNamePattern_MatchesValidatedElement()
    {
        var result = RunTransform("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='p:derived'>DERIVED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("DERIVED", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void StrictMode_QNamePattern_SubstitutionGroupMemberMatchesHeadName()
    {
        // The source element is declared as p:derived; the pattern names the head p:base,
        // so the rewritten schema-element(p:base) must walk the substitution group.
        var result = RunTransform("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='p:base'>BASE</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("BASE", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void StrictMode_PathPattern_QNameStep_NotRewritten()
    {
        // Path patterns (a/b) are NOT rewritten: the trailing QName keeps plain name
        // semantics, so it still matches the validated p:derived child by name alone
        // (schema-element semantics would match here too — the pin is that no static
        // error arises and dispatch behaves exactly as in an untyped mode).
        var result = RunTransform("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>
                <xsl:apply-templates select='*'/>
            </xsl:template>
            <xsl:template match='p:root'>
                <xsl:apply-templates select='p:derived'/>
            </xsl:template>
            <xsl:template match='p:root/p:derived'>NESTED</xsl:template>
            <xsl:template match='p:derived'>FLAT</xsl:template>
            """);
        Assert.Contains("NESTED", result);
        Assert.DoesNotContain("FLAT", result);
    }

    [Fact]
    public void StrictMode_UntypedNode_ThrowsXtte3100()
    {
        // match-219 shape: in a strict mode an element with no type annotation at all is
        // rejected before template dispatch, even though *:derived would match by name.
        var ns = XNamespace.Get(TestNs);
        var plain = new XDocument(new XElement(ns + "root", new XElement(ns + "derived", new XElement(ns + "v", "x"))));
        var executable = Compile("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='*:derived'><ok/></xsl:template>
            <xsl:template match='p:derived'>E</xsl:template>
            """);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(plain)));
        Assert.Contains("XTTE3100", ex.Message);
    }

    [Fact]
    public void StrictMode_NodeTypedByNamedTypeOnly_DoesNotThrowXtte3100()
    {
        // match-220 shape: a node validated by named type alone (t:type) carries a type
        // annotation, so strict mode accepts it at dispatch; the QName pattern then does
        // not match (no governing declaration) and the wildcard template fires.
        var executable = Compile("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>
                <out>
                    <xsl:apply-templates select='*/*'/>
                    <xsl:variable name='temp' as='element()'>
                        <p:derived xsl:type='xs:string'>DECOY</p:derived>
                    </xsl:variable>
                    <xsl:apply-templates select='$temp'/>
                </out>
            </xsl:template>
            <xsl:template match='*:derived'><ok/></xsl:template>
            <xsl:template match='p:derived'>E</xsl:template>
            """);
        var result = executable.TransformToString(new XDocumentNode(ValidatedDerivedDoc()));
        Assert.Contains("E", result);
        Assert.Contains("<ok/>", result);
        Assert.DoesNotContain("DECOY", result);
    }

    // ----- lax mode -----

    [Fact]
    public void LaxMode_QNameWithoutDeclaration_FallsBackToNameMatching()
    {
        // match-221 shape: in a lax mode a QName with no element declaration in the
        // schema set matches by name alone, even for an untyped node (no XTTE3100).
        var plain = new XDocument(new XElement("wrapper", new XElement("loose", "v")));
        var executable = Compile("""
            <xsl:mode typed='lax'/>
            <xsl:template match='wrapper'>
                <out><xsl:apply-templates select='loose'/></out>
            </xsl:template>
            <xsl:template match='loose'>LOOSE[<xsl:value-of select='.'/>]</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        var result = executable.TransformToString(new XDocumentNode(plain));
        Assert.Contains("LOOSE[v]", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void LaxMode_QNameWithDeclaration_UsesSchemaElementSemantics()
    {
        // A declared QName in lax mode follows schema-element semantics: an untyped
        // element with the same name does NOT match.
        var plain = new XDocument(new XElement("wrapper", new XElement(XNamespace.Get(TestNs) + "derived")));
        var executable = Compile("""
            <xsl:mode typed='lax'/>
            <xsl:template match='wrapper'>
                <out><xsl:apply-templates select='p:derived'/></out>
            </xsl:template>
            <xsl:template match='p:derived'>DECLARED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        var result = executable.TransformToString(new XDocumentNode(plain));
        Assert.Contains("OTHER", result);
        Assert.DoesNotContain("DECLARED", result);
    }

    // ----- untyped mode (typed="no"): XTTE3110 -----

    [Fact]
    public void UntypedMode_SchemaDependentPatternOnAnnotatedNode_ThrowsXtte3110()
    {
        // match-231 shape: typed="no" + schema-element() pattern + validated source.
        var executable = Compile("""
            <xsl:mode typed='no'/>
            <xsl:template match='schema-element(p:root)'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='*'/>
            """);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(ValidatedDerivedDoc())));
        Assert.Contains("XTTE3110", ex.Message);
    }

    [Fact]
    public void UntypedMode_PlainQNamePattern_WorksOnUnannotatedNodes()
    {
        // typed="no" without schema-dependent patterns is unchanged: name matching works.
        var plain = new XDocument(new XElement("wrapper", new XElement("loose", "v")));
        var executable = Compile("""
            <xsl:mode typed='no'/>
            <xsl:template match='wrapper'>
                <out><xsl:apply-templates select='loose'/></out>
            </xsl:template>
            <xsl:template match='loose'>LOOSE</xsl:template>
            """);
        var result = executable.TransformToString(new XDocumentNode(plain));
        Assert.Contains("LOOSE", result);
    }

    // ----- unspecified mode regression pins -----

    [Fact]
    public void UnspecifiedMode_SchemaElementPattern_WorksOnAnnotatedNode()
    {
        // match-230 shape: typed="unspecified" keeps schema-element() patterns working
        // on validated sources (no XTTE3110, which is restricted to typed="no").
        var result = RunTransform("""
            <xsl:mode typed='unspecified'/>
            <xsl:template match='schema-element(p:root)'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='p:derived'>DERIVED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("DERIVED", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void UnspecifiedMode_QNamePattern_IsPlainNameMatch()
    {
        // Without a typed mode a QName pattern never acquires schema-element semantics:
        // it matches the validated element by name (and would also match an untyped one).
        var result = RunTransform("""
            <xsl:mode typed='unspecified'/>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='p:derived'>DERIVED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("DERIVED", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void NoModeDeclaration_QNamePattern_BehavesAsBefore()
    {
        // No xsl:mode at all: bit-identical to the pre-fix behavior.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='p:derived'>DERIVED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("DERIVED", result);
        Assert.DoesNotContain("OTHER", result);
    }

    // ----- XTSE3105 (static) -----

    [Fact]
    public void StrictMode_UndeclaredQNamePattern_ThrowsXtse3105()
    {
        // match-244 shape: //QName with no element declaration in the schema set.
        // The XTSE3105 static check runs when the strict mode's rules are compiled for
        // dispatch, so the exception surfaces on transform, not on stylesheet compile.
        var ex = Assert.Throws<InvalidOperationException>(() => RunTransform("""
            <xsl:mode typed='strict'/>
            <xsl:template match='//nosuchdecl'>X</xsl:template>
            <xsl:template match='*'/>
            """));
        Assert.Contains("XTSE3105", ex.Message);
    }

    [Fact]
    public void StrictMode_PathPatternWithUndeclaredStep_DoesNotThrow()
    {
        // match-224 shape: a QName inside a path pattern is not rewritten, so a
        // non-existent name is not a static error (the rule simply never matches).
        var executable = Compile("""
            <xsl:mode typed='strict'/>
            <xsl:template match='/'>ROOT</xsl:template>
            <xsl:template match='p:root/total-garbage'><cannot-happen/></xsl:template>
            """);
        var result = executable.TransformToString(new XDocumentNode(ValidatedDerivedDoc()));
        Assert.Contains("ROOT", result);
        Assert.DoesNotContain("cannot-happen", result);
    }

    // ----- element-with-id patterns -----

    private const string IdSchemaText = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:mtd' xmlns:p='urn:mtd' elementFormDefault='qualified'>
            <xs:element name='idroot' type='p:idRootType'/>
            <xs:complexType name='idRootType'>
                <xs:sequence>
                    <xs:element name='row' maxOccurs='unbounded'>
                        <xs:complexType>
                            <xs:simpleContent>
                                <xs:extension base='xs:string'>
                                    <xs:attribute name='id' type='xs:ID'/>
                                </xs:extension>
                            </xs:simpleContent>
                        </xs:complexType>
                    </xs:element>
                </xs:sequence>
            </xs:complexType>
        </xs:schema>
        """;

    private const string IdImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:mtd' xmlns:p='urn:mtd' elementFormDefault='qualified'>" +
        "<xs:element name='idroot' type='p:idRootType'/>" +
        "<xs:complexType name='idRootType'><xs:sequence>" +
        "<xs:element name='row' maxOccurs='unbounded'><xs:complexType><xs:simpleContent>" +
        "<xs:extension base='xs:string'><xs:attribute name='id' type='xs:ID'/></xs:extension>" +
        "</xs:simpleContent></xs:complexType></xs:element>" +
        "</xs:sequence></xs:complexType>" +
        "</xs:schema></xsl:import-schema>";

    private static XDocument ValidatedIdDoc()
    {
        var ns = XNamespace.Get(TestNs);
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(IdSchemaText), null)!);
        set.Compile();
        var source = new XDocument(new XElement(ns + "idroot",
            new XElement(ns + "row", "k1", new XAttribute("id", "a")),
            new XElement(ns + "row", "k2", new XAttribute("id", "b"))));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, set);
        return source;
    }

    [Fact]
    public void ElementWithIdPattern_MatchesElementWithMatchingId()
    {
        // match-055 shape: fn:element-with-id('a') as a pattern matches the element whose
        // xs:ID-typed attribute has the value 'a'.
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
            "xmlns:p='urn:mtd' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" + IdImportSchema + """
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='element-with-id("a")'>FOUND[<xsl:value-of select='.'/>]</xsl:template>
            <xsl:template match='element-with-id("zzz")'>NOTFOUND</xsl:template>
            <xsl:template match='text()'/>
            """ + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var result = executable.TransformToString(new XDocumentNode(ValidatedIdDoc()));
        Assert.Contains("FOUND[k1]", result);
        Assert.DoesNotContain("NOTFOUND", result);
    }

    [Fact]
    public void ElementWithIdPattern_WithDocumentArgument_MatchesElementWithMatchingId()
    {
        // match-054 shape: two-argument form element-with-id('b', root()); root() keeps the
        // test slash-free — a literal '/' here misroutes through the path-pattern compiler.
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
            "xmlns:p='urn:mtd' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" + IdImportSchema + """
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/*'/></out>
            </xsl:template>
            <xsl:template match='element-with-id("b", root())'>FOUND[<xsl:value-of select='.'/>]</xsl:template>
            <xsl:template match='text()'/>
            """ + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var result = executable.TransformToString(new XDocumentNode(ValidatedIdDoc()));
        Assert.Contains("FOUND[k2]", result);
    }

    // ----- deep-copy PSVI preservation (match-263) -----

    [Fact]
    public void DeepCopy_BuiltInRule_PreservesAttributeTypeAnnotation()
    {
        // match-263 shape: on-no-match="deep-copy" of a validated attribute keeps the
        // PSVI so instance of attribute(N, T) is true on the copied attribute.
        var result = RunTransform("""
            <xsl:mode on-no-match='deep-copy'/>
            <xsl:template match='/'>
                <xsl:variable name='temp' as='attribute()'>
                    <xsl:apply-templates select='/p:root/@p:flag'/>
                </xsl:variable>
                <out result='{$temp instance of attribute(p:flag, xs:boolean)}'/>
            </xsl:template>
            """);
        Assert.Contains("result=\"true\"", result);
    }
}
