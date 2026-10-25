// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies supported recursive-array atomization and singleton-consumer controls.
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

/// <summary>Verifies supported recursive-array atomization and singleton-consumer controls.</summary>
public sealed class ArrayAtomizationAuditTests
{
    /// <summary>Provides supported behaviors without treating diagnostic failures as desired outcomes.</summary>
    /// <returns>Profile, expression, expected value/error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Expression, string Expected, bool Error)[]
        {
            ("string-join(data([[],2,[]]) ! string(.), ',')", "2", false),
            ("string-join(data([[1,2],[],[3]]) ! string(.), ',')", "1,2,3", false),
            ("count(data([[],[]]))", "0", false),
            ("([[],2]) cast as xs:integer", "2", false),
            ("([2,[]]) cast as xs:integer", "2", false),
            ("count(([[],[]]) cast as xs:integer?)", "0", false),
            ("([[],2]) castable as xs:integer", "true", false),
            ("([[1,2]]) castable as xs:integer", "false", false),
            ("-[[2]]", "-2", false),
            ("+[[2]]", "2", false),
            ("[2] + 1", "3", false),
            ("[[2]] * 2", "4", false),
            ("[[2]] idiv 1", "2", false),
            ("[[2]] eq 2", "true", false),
            ("[1,2] = 2", "true", false),
            ("count(-[[]])", "0", false),
            ("[1,2] + 0", "XPTY0004", true),
            ("[[1,2]] eq 2", "XPTY0004", true),
            ("([[1,2]]) cast as xs:integer", "XPTY0004", true),
            ("data([map{}])", "FOTY0013", true),
            ("data([function(){1}])", "FOTY0013", true),
            ("[map{}] cast as xs:integer?", "FOTY0013", true),
            ("[xs:untypedAtomic('bad')] cast as xs:integer", "FORG0001", true),
            ("(function($x){-$x})([[2]])", "-2", false),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Preserves supported values and required errors across public profiles.</summary>
    /// <param name="profile">The modern compatibility profile.</param>
    /// <param name="expression">The supported control expression.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void SupportedControls(XPathCompatibility profile, string expression, string expected, bool error)
    {
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }
}
