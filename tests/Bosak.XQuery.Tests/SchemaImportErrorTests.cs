// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies schema compilation diagnostics and independent schema resolver failures.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Xml;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verifies schema compilation diagnostics and independent schema resolver failures.</summary>
public class SchemaImportErrorTests
{
    private const string Query = "import schema namespace t='urn:k02'; 7";

    /// <summary>Invalid or unavailable schema type definitions retain their diagnostic and standard code.</summary>
    /// <param name="baseType">The unavailable base type QName.</param>
    [Theory]
    [InlineData("xs:dayTimeDuration")]
    [InlineData("xs:yearMonthDuration")]
    [InlineData("t:MissingType")]
    public void CompilationFailure_ReportsXqst0012(string baseType)
    {
        var context = Context(Schema(baseType));
        var error = Assert.Throws<InvalidOperationException>(() => Run(context));
        Assert.StartsWith("XQST0012:", error.Message);
        Assert.Contains("Schema compilation failed", error.Message);
        Assert.Contains(baseType.Split(':')[1], error.Message);
    }

    /// <summary>A semantically invalid facet also reports schema compilation failure.</summary>
    [Fact]
    public void InvalidFacet_ReportsXqst0012()
    {
        var context = Context(Schema("xs:integer", "<xs:minInclusive value='not-a-number'/>"));
        Assert.StartsWith("XQST0012:", Assert.Throws<InvalidOperationException>(() => Run(context)).Message);
    }

    /// <summary>Valid schemas still allow the query to execute normally.</summary>
    /// <param name="baseType">The supported XSD 1.0 base type.</param>
    [Theory]
    [InlineData("xs:string")]
    [InlineData("xs:duration")]
    public void ValidSchema_ExecutesQuery(string baseType)
        => Assert.Equal("7", Run(Context(Schema(baseType))));

    /// <summary>A missing schema remains a resolution error rather than invalid definitions.</summary>
    [Fact]
    public void MissingSchema_RetainsXqst0059()
    {
        var context = new XQueryContext().WithSchemaResolver((_, _) => null);
        Assert.Contains("XQST0059", Assert.Throws<InvalidOperationException>(() => Run(context)).Message);
    }

    /// <summary>A resolver I/O failure is preserved without recoding.</summary>
    [Fact]
    public void ResolverIoFailure_IsPreserved()
    {
        var expected = new IOException("K02 resolver failed");
        var context = new XQueryContext().WithSchemaResolver((_, _) => throw expected);
        Assert.Same(expected, Assert.Throws<IOException>(() => Run(context)));
    }

    /// <summary>Malformed XML is not rewritten as a schema compilation diagnostic.</summary>
    [Fact]
    public void MalformedXml_IsPreserved()
        => Assert.Throws<XmlException>(() => Run(Context("<xs:schema")));

    private static string Run(XQueryContext context)
        => new XQueryCompiler().Compile(Query).Evaluate(context).ToString();

    private static XQueryContext Context(string schema)
        => new XQueryContext().WithSchemaResolver((_, _) => new MemoryStream(Encoding.UTF8.GetBytes(schema)));

    private static string Schema(string baseType, string facets = "")
        => $"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:k02' targetNamespace='urn:k02'><xs:simpleType name='Limited'><xs:restriction base='{baseType}'>{facets}</xs:restriction></xs:simpleType></xs:schema>";
}
