// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Verify library context declaration errors through public import and module loading.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verify library context declaration errors through public import and module loading.</summary>
public sealed class ContextDeclarationErrorTests
{
    private const string ModuleUri = "urn:bosak:c25";

    /// <summary>Supplies valid declarations, duplicate variants and retained library restrictions.</summary>
    /// <returns>Module-loading mode, declaration text and required error code.</returns>
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool loadDynamically in new[] { false, true })
        foreach (var pair in new[]
        {
            ("declare context item external;", ""),
            ("declare context item as xs:integer external;", ""),
            ("declare context item external; declare context item external;", "XQST0099"),
            ("declare context item as xs:integer external; declare context item as xs:integer external;", "XQST0099"),
            ("declare (:first:) context item external; declare context (:second:) item external;", "XQST0099"),
            ("declare context item external; declare context item as xs:integer external;", "XQST0099"),
            ("declare context item := 1;", "XQST0113"),
            ("declare context item as xs:integer := 1;", "XQST0113"),
            ("declare context item external := 1;", "XQST0113"),
            ("declare context item as xs:integer external := 1;", "XQST0113")
        }) yield return [loadDynamically, pair.Item1, pair.Item2];
    }

    /// <summary>Checks error identity and valid modules using both public loading paths.</summary>
    /// <param name="loadDynamically">Whether to use fn:load-xquery-module rather than an import.</param>
    /// <param name="declaration">The library context declaration text.</param>
    /// <param name="code">The required error, or empty for a valid module.</param>
    [Theory, MemberData(nameof(Cases))]
    public void LibraryDeclarationContract(bool loadDynamically, string declaration, string code)
    {
        var compiler = new XQueryCompiler().WithModule(ModuleUri,
            $"module namespace m = '{ModuleUri}'; {declaration} declare function m:answer() {{ 7 }};");
        string query = loadDynamically
            ? $"let $m := load-xquery-module('{ModuleUri}', map{{'context-item':27}}) return $m('functions')(QName('{ModuleUri}','answer'))(0)()"
            : $"import module namespace m = '{ModuleUri}'; m:answer()";
        string Run() => compiler.Compile(query).Evaluate(new XQueryContext().WithContextItem(XdmValue.FromInteger(27))).ToString();
        if (code.Length == 0) Assert.Equal("7", Run());
        else
        {
            string message = Assert.ThrowsAny<Exception>(() => Run()).Message;
            Assert.Contains(code, message);
            if (code == "XQST0099") Assert.DoesNotContain("XQST0113", message);
            if (loadDynamically) Assert.Contains("FOQM0003", message);
        }
    }

    /// <summary>Retains typed external-context validation in both public module paths.</summary>
    /// <param name="loadDynamically">Whether to load the module dynamically.</param>
    [Theory, InlineData(false), InlineData(true)]
    public void RetainsWrongExternalTypeError(bool loadDynamically)
    {
        var compiler = new XQueryCompiler().WithModule(ModuleUri,
            $"module namespace m = '{ModuleUri}'; declare context item as xs:integer external; declare function m:answer() {{ 7 }};");
        string query = loadDynamically
            ? $"load-xquery-module('{ModuleUri}', map{{'context-item':'London'}})"
            : $"import module namespace m = '{ModuleUri}'; m:answer()";
        var error = Assert.ThrowsAny<Exception>(() => compiler.Compile(query).Evaluate(new XQueryContext().WithContextItem(XdmValue.FromString("London"))));
        Assert.Contains(loadDynamically ? "FOQM0005" : "XPTY0004", error.Message);
    }

    /// <summary>Retains the existing main-module declaration boundary.</summary>
    /// <param name="query">The unsupported main-module declaration.</param>
    [Theory]
    [InlineData("declare context item := 3; .")]
    [InlineData("declare context item external; .")]
    public void MainDeclarationsRemainOutsideThisFix(string query) =>
        Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => new XQueryCompiler().Compile(query)).Message);
}
