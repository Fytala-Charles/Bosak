// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies bounded cardinal variations and German ordinal formatting.
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
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies bounded cardinal variations and German ordinal formatting.</summary>
public sealed class FormatIntegerVariationTests
{
    /// <summary>Checks named variations and unchanged fallback behavior in both language levels.</summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="expected">The exact formatted value.</param>
    [Theory]
    [InlineData("format-integer(20, 'Ww;c(%spellout-cardinal)', 'de')", "Zwanzig")]
    [InlineData("format-integer(0, 'Ww;c(%spellout-cardinal)', 'de')", "Null")]
    [InlineData("format-integer(-20, 'Ww;c(%spellout-cardinal)', 'de')", "-Zwanzig")]
    [InlineData("format-integer(1, 'Ww;c(%spellout-ordinal-feminine)', 'de')", "Erste")]
    [InlineData("format-integer(1, 'Ww;o(%spellout-ordinal-feminine)', 'de')", "Erste")]
    [InlineData("format-integer(1, 'Ww;c(%spellout-ordinal-s)', 'de')", "Erstes")]
    [InlineData("format-integer(1, 'Ww;o(%spellout-ordinal-s)', 'de')", "Erstes")]
    [InlineData("format-integer(1, 'Ww;c(%spellout-ordinal-r)', 'de')", "Erster")]
    [InlineData("format-integer(1, 'Ww;o(%spellout-ordinal-r)', 'de')", "Erster")]
    [InlineData("format-integer(1, 'Ww;c(%spellout-ordinal-n)', 'de')", "Ersten")]
    [InlineData("format-integer(1, 'Ww;o(%spellout-ordinal-n)', 'de')", "Ersten")]
    [InlineData("format-integer(20, 'Ww;c(%spellout-ordinal-s)', 'de')", "Zwanzigstes")]
    [InlineData("format-integer(0, 'Ww;c(%spellout-ordinal-s)', 'de')", "Nulltes")]
    [InlineData("format-integer(-1, 'Ww;c(%spellout-ordinal-s)', 'de')", "-Erstes")]
    [InlineData("format-integer(20, 'Ww;c(%unknown)', 'de')", "Zwanzig")]
    [InlineData("format-integer(20, 'Ww;c(-er)', 'de')", "Zwanzig")]
    [InlineData("format-integer(20, 'Ww;c', 'de')", "Zwanzig")]
    [InlineData("format-integer(20, 'Ww;o(%spellout-ordinal)', 'de')", "Zwanzigste")]
    [InlineData("format-integer(1, 'Ww;o(-es)', 'de')", "Erstes")]
    [InlineData("format-integer(1, 'Ww;o(%unknown)', 'de')", "Erste")]
    [InlineData("format-integer(1, 'w;c(%spellout-ordinal-s)', 'de')", "erstes")]
    [InlineData("format-integer(1, 'W;c(%spellout-ordinal-s)', 'de')", "ERSTES")]
    [InlineData("format-integer(1, '1;c(%spellout-ordinal-s)', 'de')", "1")]
    [InlineData("format-integer(12, '000;c(%unknown)', 'de')", "012")]
    [InlineData("format-integer(20, 'w;c(%unknown)', 'en')", "twenty")]
    [InlineData("format-integer(1, 'Ww;c(%spellout-ordinal-s)', 'en')", "One")]
    [InlineData("format-integer(5, 'Ww;o(%spellout-ordinal-masculine)', 'it')", "Quinto")]
    [InlineData("format-integer(5, 'Ww;o(%spellout-ordinal-feminine)', 'it')", "Quinta")]
    [InlineData("format-integer((), 'Ww;c(')", "")]
    public void FormatsVariation(string expression, string expected)
    {
        foreach (var level in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40 })
        {
            var compiled = XPath31Expression.Compile(expression, new CompileOptions { Compatibility = level });
            Assert.Equal(expected, compiled.Evaluate((IXdmNode?)null).ToString());
        }
    }

    /// <summary>Rejects malformed pictures without confusing unknown names with syntax errors.</summary>
    /// <param name="picture">The malformed picture.</param>
    [Theory]
    [InlineData("Ww;c(")]
    [InlineData("Ww;o(")]
    [InlineData("Ww;c()")]
    [InlineData("Ww;c(%x")]
    [InlineData("Ww;c(%x))")]
    [InlineData("Ww;c((%x)")]
    [InlineData("Ww;o(%x))")]
    public void RejectsMalformedVariation(string picture)
    {
        foreach (var level in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40 })
        {
            var expression = XPath31Expression.Compile($"format-integer(20, '{picture}', 'de')", new CompileOptions { Compatibility = level });
            var error = Assert.ThrowsAny<Exception>(() => expression.Evaluate((IXdmNode?)null));
            Assert.Contains("FODF1310", error.Message);
        }
    }
}
