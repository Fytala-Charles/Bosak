// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies first-evaluation registration of imported schema constructor functions.
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
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Core.Xdm;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verifies first-evaluation registration of imported schema constructor functions.</summary>
public class SchemaConstructorRegistrationTests
{
    private const string Schema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:k02-c01" targetNamespace="urn:k02-c01">
          <xs:simpleType name="Small"><xs:restriction base="xs:integer"><xs:minInclusive value="1"/><xs:maxInclusive value="9"/></xs:restriction></xs:simpleType>
          <xs:simpleType name="Numbers"><xs:list itemType="xs:integer"/></xs:simpleType>
          <xs:simpleType name="NumberOrDate"><xs:union memberTypes="xs:integer xs:date"/></xs:simpleType>
          <xs:simpleType name="Name"><xs:restriction base="xs:QName"/></xs:simpleType>
        </xs:schema>
        """;
    private const string Import = "import schema namespace t='urn:k02-c01'; ";

    /// <summary>Imported constructors work on a fresh context through all function-call forms.</summary>
    /// <param name="expression">The constructor expression.</param>
    /// <param name="expected">The expected serialized value.</param>
    [Theory]
    [InlineData("t:Small('5')", "5")]
    [InlineData("t:Small#1('5')", "5")]
    [InlineData("function-lookup(xs:QName('t:Small'),1)('5')", "5")]
    [InlineData("deep-equal(t:Numbers('1 2 3'), (1, 2, 3))", "true")]
    [InlineData("t:NumberOrDate('2026-10-27')", "2026-10-27")]
    [InlineData("t:Name('t:hello') eq xs:QName('t:hello')", "true")]
    [InlineData("empty(t:Small(()))", "true")]
    public void FreshContext_ResolvesConstructor(string expression, string expected)
        => Assert.Equal(expected, Evaluate(expression, Context()));

    /// <summary>Invalid facets still raise the constructor's lexical conversion error.</summary>
    [Fact]
    public void InvalidValue_PreservesConversionError()
        => Assert.Contains("FORG0001", Assert.Throws<InvalidOperationException>(() => Evaluate("t:Small('99')", Context())).Message);

    /// <summary>Declaring a function with the imported constructor's name remains a static conflict.</summary>
    [Fact]
    public void DeclaredFunction_PreservesConstructorCollision()
        => Assert.Contains("XQST0034", Assert.Throws<InvalidOperationException>(() => Evaluate("declare function t:Small($x) { $x }; t:Small(5)", Context())).Message);

    /// <summary>Existing host bindings are not replaced by imported constructors.</summary>
    [Fact]
    public void HostBinding_IsPreserved()
    {
        var context = Context();
        context.EvaluationContext.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "urn:k02-c01", LocalName = "Small", Arity = 1,
            ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
            Implementation = (_, _) => XdmValue.FromString("host")
        });
        Assert.Equal("host", Evaluate("t:Small('5')", context));
    }

    /// <summary>Reusing a context does not change constructor behavior after first evaluation.</summary>
    [Fact]
    public void ReusedContext_MatchesFirstEvaluation()
    {
        var context = Context();
        var query = new XQueryCompiler().Compile(Import + "t:Small('5')");
        Assert.Equal("5", query.Evaluate(context).ToString());
        Assert.Equal("5", query.Evaluate(context).ToString());
    }

    private static XQueryContext Context()
        => new XQueryContext().WithSchemaResolver((_, _) => new MemoryStream(Encoding.UTF8.GetBytes(Schema)));

    private static string Evaluate(string expression, XQueryContext context)
        => new XQueryCompiler().Compile(Import + expression).Evaluate(context).ToString();
}
