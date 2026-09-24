// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 september 2026
// PURPOSE              : Unit tests for schema-aware kind-test compilation via CompileOptions.SchemaSet (REQ-104)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Schema;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>
/// Tests for schema-aware kind tests (<c>schema-element()</c>/<c>schema-attribute()</c>) in
/// statically validated XPath compilation (REQ-104). With a <see cref="CompileOptions.SchemaSet"/>
/// the kind tests compile and their name argument is checked against the set's global
/// declarations (XPST0008 when absent); without a set the legacy no-schema-awareness
/// XPST0008 behavior is unchanged.
/// </summary>
public class SchemaAwareKindTestCompileTests
{
    private const string TestNs = "urn:pat";

    // order/base are global element declarations; limit is a global attribute declaration;
    // the second (no-namespace) schema declares localOrder for the no-namespace case.
    private const string SchemaText = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:pat' xmlns:p='urn:pat' elementFormDefault='qualified'>
            <xs:element name='order' type='xs:string'/>
            <xs:element name='base' type='xs:string'/>
            <xs:attribute name='limit' type='xs:integer'/>
        </xs:schema>
        """;

    private const string NoNamespaceSchemaText = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xs:element name='localOrder' type='xs:string'/>
        </xs:schema>
        """;

    private static XmlSchemaSet CompileSchemas()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(SchemaText), null)!);
        set.Add(XmlSchema.Read(new StringReader(NoNamespaceSchemaText), null)!);
        set.Compile();
        return set;
    }

    private static CompileOptions SchemaAwareOptions(XmlSchemaSet set, string? defaultElementNamespace = null) => new()
    {
        Namespaces = new Dictionary<string, string> { ["p"] = TestNs },
        DefaultElementNamespace = defaultElementNamespace,
        SchemaSet = set
    };

    private static void AssertCompileFails(string expression, CompileOptions options, string expectedCode)
    {
        var ex = Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(expression, options));
        Assert.Contains(expectedCode, ex.Message);
    }

    // ----- schema-aware compilation (SchemaSet supplied) -----

    [Fact]
    public void SchemaElement_Prefixed_DeclaredName_Compiles()
    {
        var compiled = XPath31Expression.Compile("schema-element(p:order)", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaElement_Prefixed_UndeclaredName_XPST0008()
    {
        // The prefix resolves but urn:pat has no global element declaration 'absent'.
        AssertCompileFails("schema-element(p:absent)", SchemaAwareOptions(CompileSchemas()), "XPST0008");
    }

    [Fact]
    public void SchemaElement_UndeclaredPrefix_XPST0081()
    {
        // Prefix errors keep precedence over declaration errors.
        AssertCompileFails("schema-element(q:order)", SchemaAwareOptions(CompileSchemas()), "XPST0081");
    }

    [Fact]
    public void SchemaElement_Unprefixed_WithDefaultElementNamespace_Compiles()
    {
        var compiled = XPath31Expression.Compile("schema-element(order)", SchemaAwareOptions(CompileSchemas(), TestNs));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaElement_Unprefixed_NoDefaultNamespace_XPST0008()
    {
        // Unprefixed expands to no namespace when no default element namespace is in scope;
        // urn:pat's 'order' does not cover Q{}order.
        AssertCompileFails("schema-element(order)", SchemaAwareOptions(CompileSchemas()), "XPST0008");
    }

    [Fact]
    public void SchemaElement_Unprefixed_NoNamespaceDeclaration_Compiles()
    {
        // The no-namespace schema declares localOrder, so Q{}localOrder resolves.
        var compiled = XPath31Expression.Compile("schema-element(localOrder)", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaElement_BracedUriQName_DeclaredName_Compiles()
    {
        var compiled = XPath31Expression.Compile("schema-element(Q{urn:pat}order)", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaElement_BracedUriQName_UndeclaredName_XPST0008()
    {
        AssertCompileFails("schema-element(Q{urn:pat}absent)", SchemaAwareOptions(CompileSchemas()), "XPST0008");
    }

    [Fact]
    public void SchemaAttribute_Prefixed_DeclaredName_Compiles()
    {
        var compiled = XPath31Expression.Compile("schema-attribute(p:limit)", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaAttribute_ElementName_NotAttributeDeclaration_XPST0008()
    {
        // urn:pat 'order' is a global element, not a global attribute declaration.
        AssertCompileFails("schema-attribute(p:order)", SchemaAwareOptions(CompileSchemas()), "XPST0008");
    }

    [Fact]
    public void DocumentNode_SchemaElement_DeclaredName_Compiles()
    {
        var compiled = XPath31Expression.Compile("document-node(schema-element(p:order))", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    [Fact]
    public void SchemaElement_InInstanceOf_Compiles()
    {
        var compiled = XPath31Expression.Compile(". instance of schema-element(p:order)", SchemaAwareOptions(CompileSchemas()));
        Assert.NotNull(compiled);
    }

    // ----- default behavior without a schema set (unchanged) -----

    [Fact]
    public void SchemaElement_NoSchemaSet_Prefixed_XPST0008()
    {
        var options = new CompileOptions { Namespaces = new Dictionary<string, string> { ["p"] = TestNs } };
        AssertCompileFails("schema-element(p:order)", options, "XPST0008");
    }

    [Fact]
    public void SchemaElement_NoSchemaSet_Unprefixed_XPST0008()
    {
        var options = new CompileOptions { Namespaces = new Dictionary<string, string> { ["p"] = TestNs } };
        AssertCompileFails("schema-element(order)", options, "XPST0008");
    }

    [Fact]
    public void SchemaElement_NoSchemaSet_NoNamespaces_XPST0008()
    {
        // The parse-time guard does not depend on a static namespace context.
        AssertCompileFails("schema-element(order)", CompileOptions.Default, "XPST0008");
    }

    // ----- schema set without a namespace map: runtime-deferred resolution -----

    [Fact]
    public void SchemaElement_SchemaSetWithoutNamespaces_Unprefixed_Compiles()
    {
        // No namespace map: the static validator is skipped and prefix/name resolution is
        // deferred to the runtime (which raises XPST0081/XPST0008 there when needed).
        var compiled = XPath31Expression.Compile("schema-element(order)", new CompileOptions { SchemaSet = CompileSchemas() });
        Assert.NotNull(compiled);
    }
}
