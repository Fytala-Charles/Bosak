// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies list constructors and invalid list-type variable bindings.
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
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verifies list constructors and invalid list-type variable bindings.</summary>
public class SchemaListBindingTests
{
    /// <summary>List names are invalid binding item types even for single or empty values.</summary>
    /// <param name="type">The built-in list type name.</param>
    /// <param name="suffix">The binding occurrence indicator.</param>
    /// <param name="initializer">The value bound to the invalid declared type.</param>
    [Theory]
    [InlineData("NMTOKENS", "", "xs:NMTOKENS('a b c')")]
    [InlineData("IDREFS", "", "xs:IDREFS('a b')")]
    [InlineData("ENTITIES", "", "xs:ENTITIES('a b')")]
    [InlineData("NMTOKENS", "", "'a'")]
    [InlineData("IDREFS", "", "'a'")]
    [InlineData("ENTITIES", "", "'a'")]
    [InlineData("NMTOKENS", "?", "()")]
    [InlineData("IDREFS", "*", "()")]
    [InlineData("ENTITIES", "*", "()")]
    public void Binding_ListItemType_ReportsXpst0051(string type, string suffix, string initializer)
    {
        var error = Assert.ThrowsAny<Exception>(() => Evaluate(
            $"let $v as xs:{type}{suffix} := {initializer} return count($v)"));
        Assert.Contains("XPST0051", error.Message);
    }

    /// <summary>List construction and legal member sequence bindings retain their results.</summary>
    /// <param name="query">The complete standalone query.</param>
    /// <param name="expected">The expected serialized atomic result.</param>
    [Theory]
    [InlineData("count(xs:NMTOKENS('a b c'))", "3")]
    [InlineData("count(xs:IDREFS('a b'))", "2")]
    [InlineData("count(xs:ENTITIES('a b'))", "2")]
    [InlineData("'a b c' castable as xs:NMTOKENS", "true")]
    [InlineData("'a b 12' castable as xs:IDREFS", "false")]
    [InlineData("let $v as xs:NMTOKEN* := xs:NMTOKENS('a b c') return count($v)", "3")]
    [InlineData("let $v as xs:string? := () return count($v)", "0")]
    [InlineData("let $v as xs:integer := 42 return $v", "42")]
    public void Construction_AndLegalBindings_PreserveResults(string query, string expected)
        => Assert.Equal(expected, Evaluate(query));

    /// <summary>A legal item type still reports a value mismatch with XPTY0004.</summary>
    [Fact]
    public void Binding_LegalTypeWithWrongValue_RetainsXpty0004()
    {
        var error = Assert.ThrowsAny<Exception>(() => Evaluate("let $v as xs:integer := 'a' return $v"));
        Assert.Contains("XPTY0004", error.Message);
    }

    private static string Evaluate(string query)
        => new XQueryCompiler().Compile(query).Evaluate(new XQueryContext()).ToString();
}
