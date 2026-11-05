// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies file-backed schema source URIs and relative schema imports.
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
using System.Xml.Schema;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verifies file-backed schema source URIs and relative schema imports.</summary>
public class SchemaSourceUriTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "BosakSchemaUri-" + Guid.NewGuid().ToString("N"));
    private const string RootSchema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:c="urn:k02-child" targetNamespace="urn:k02-root">
          <xs:import namespace="urn:k02-child" schemaLocation="nested/child.xsd"/>
          <xs:element name="root" type="c:Code"/>
        </xs:schema>
        """;
    private const string ChildSchema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="urn:k02-child">
          <xs:include schemaLocation="types.xsd"/>
        </xs:schema>
        """;
    private const string TypesSchema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="urn:k02-child">
          <xs:simpleType name="Code"><xs:restriction base="xs:string"><xs:enumeration value="ok"/></xs:restriction></xs:simpleType>
        </xs:schema>
        """;
    private const string Body = "string(validate strict { <r:root>ok</r:root> })";

    /// <summary>Creates an isolated schema tree with a nested relative import and include.</summary>
    public SchemaSourceUriTests()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "nested"));
        File.WriteAllText(Path.Combine(_directory, "root.xsd"), RootSchema);
        File.WriteAllText(Path.Combine(_directory, "nested", "child.xsd"), ChildSchema);
        File.WriteAllText(Path.Combine(_directory, "nested", "types.xsd"), TypesSchema);
    }

    /// <summary>A resolver-provided file retains its own identity despite misleading query metadata.</summary>
    [Fact]
    public void ResolverFile_ResolvesNestedImportAndInclude()
    {
        var context = new XQueryContext().WithSchemaResolver((_, _) => File.OpenRead(RootPath));
        context.EvaluationContext.BaseUri = new Uri(Path.Combine(_directory, "wrong", "query.xq")).AbsoluteUri;
        Assert.Equal("ok", Run("import schema namespace r='urn:k02-root' at 'unrelated.xsd'; " + Body, context));
        Assert.Contains(context.EvaluationContext.SchemaSet!.Schemas().Cast<XmlSchema>(), s => s.SourceUri == new Uri(RootPath).AbsoluteUri);
    }

    /// <summary>Both existing file-location fallback forms preserve the schema document base URI.</summary>
    /// <param name="fileUri">Whether the location hint is a file URI or an absolute filesystem path.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LocationFallback_ResolvesNestedImportAndInclude(bool fileUri)
    {
        var location = fileUri ? new Uri(RootPath).AbsoluteUri : RootPath.Replace('\\', '/');
        Assert.Equal("ok", Run($"import schema namespace r='urn:k02-root' at '{location}'; " + Body, new XQueryContext()));
    }

    /// <summary>Non-file streams do not borrow an unselected location hint as their schema identity.</summary>
    [Fact]
    public void MemoryResolver_DoesNotBorrowHintIdentity()
    {
        var context = new XQueryContext().WithSchemaResolver((_, _) => new MemoryStream(Encoding.UTF8.GetBytes(RootSchema)));
        var location = new Uri(RootPath).AbsoluteUri;
        Assert.StartsWith("XQST0012:", Assert.Throws<InvalidOperationException>(() => Run($"import schema namespace r='urn:k02-root' at '{location}'; true()", context)).Message);
    }

    /// <summary>A self-contained non-file schema remains usable without any source URI.</summary>
    [Fact]
    public void MemoryResolver_SelfContainedSchemaIsUnchanged()
    {
        var context = new XQueryContext().WithSchemaResolver((_, _) => new MemoryStream(Encoding.UTF8.GetBytes(TypesSchema)));
        Assert.Equal("ok", Run("import schema namespace c='urn:k02-child'; c:Code('ok')", context));
        Assert.All(context.EvaluationContext.SchemaSet!.Schemas().Cast<XmlSchema>(), s => Assert.True(string.IsNullOrEmpty(s.SourceUri)));
    }

    /// <summary>A missing imported file keeps the existing schema compilation error.</summary>
    [Fact]
    public void MissingChild_PreservesXqst0012()
    {
        File.Delete(Path.Combine(_directory, "nested", "child.xsd"));
        var context = new XQueryContext().WithSchemaResolver((_, _) => File.OpenRead(RootPath));
        Assert.StartsWith("XQST0012:", Assert.Throws<InvalidOperationException>(() => Run("import schema namespace r='urn:k02-root'; true()", context)).Message);
    }

    private string RootPath => Path.Combine(_directory, "root.xsd");
    private static string Run(string query, XQueryContext context) => new XQueryCompiler().Compile(query).Evaluate(context).ToString();

    /// <summary>Removes only this test's uniquely allocated temporary schema directory.</summary>
    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
