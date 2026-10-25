// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verify supported division and modulo controls without pinning observed defects.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Supported arithmetic controls bound the separate division/modulo investigation.</summary>
public sealed class DivisionModuloAuditTests
{
    /// <summary>Provide supported value/type and error-precedence controls.</summary>
    /// <returns>Profile, expression, expected report and optional error code.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Expression, string Expected, bool Error)[]
        {
            ("7 idiv 2", "3|true", false),
            ("-7 idiv 2", "-3|true", false),
            ("7 idiv -2", "-3|true", false),
            ("-7 idiv -2", "3|true", false),
            ("xs:decimal('-7.5') idiv 2", "-3|true", false),
            ("9223372036854775808 idiv 2", "4611686018427387904|true", false),
            ("xs:integer('-9223372036854775808') idiv 1", "-9223372036854775808|true", false),
            ("xs:integer('9223372036854775807') idiv -1", "-9223372036854775807|true", false),
            ("7 mod 2", "1|true", false),
            ("-7 mod 2", "-1|true", false),
            ("7 mod -2", "1|true", false),
            ("-7 mod -2", "-1|true", false),
            ("xs:decimal('-7.5') mod 2", "-1.5|false", false),
            ("7 mod 2.0", "1|false", false),
            ("7 div 2", "3.5|false", false),
            ("9223372036854775808 div 2", "4611686018427387904|false", false),
            ("1 idiv xs:double('INF')", "0|true", false),
            ("0 mod -1", "0|true", false),
            ("1 div 0", "FOAR0001", true),
            ("1 idiv 0", "FOAR0001", true),
            ("1 mod 0", "FOAR0001", true),
            ("xs:double('INF') idiv 0", "FOAR0001", true),
            ("xs:double('NaN') idiv 1", "FOAR0002", true),
            ("xs:double('INF') idiv 1", "FOAR0002", true)
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Validate supported controls; diagnostic defects are retained outside passing assertions.</summary>
    /// <param name="profile">The explicit modern profile.</param>
    /// <param name="expression">The arithmetic expression.</param>
    /// <param name="expected">The value/type report or required error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Cases))]
    public void SupportedControls(XPathCompatibility profile, string expression, string expected, bool error)
    {
        var report = "let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)";
        string Evaluate() => XPath31Expression.Compile(report, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }
}
