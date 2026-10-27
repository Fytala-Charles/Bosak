// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies version-gated radix pictures and unchanged decimal grouping.
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

/// <summary>Verifies version-gated radix pictures and unchanged decimal grouping.</summary>
public sealed class FormatIntegerRadixTests
{
    /// <summary>Formats exact radix values through both 4.0 contexts.</summary>
    /// <param name="value">The integer value.</param>
    /// <param name="picture">The format picture.</param>
    /// <param name="expected">The formatted value.</param>
    [Theory]
    [InlineData(255L, "16^xxxx", "00ff")]
    [InlineData(65535L, "16^XX:XX", "FF:FF")]
    [InlineData(999L, "8^#:#:#:#:XX", "1:7:47")]
    [InlineData(65535L, "16^xx^xx", "ff^ff")]
    [InlineData(21L, "16^xx;o", "15st")]
    [InlineData(26L, "16^x;o", "1ath")]
    [InlineData(31L, "16^X;o", "1Fst")]
    [InlineData(-21L, "16^xx;o", "-15st")]
    [InlineData(0L, "2^XXXX", "0000")]
    [InlineData(-1L, "36^X", "-1")]
    [InlineData(9223372036854775807L, "16^x", "7fffffffffffffff")]
    [InlineData(-9223372036854775808L, "16^x", "-8000000000000000")]
    [InlineData(-9223372036854775808L, "16^x;o", "-8000000000000000th")]
    [InlineData(255L, "16^xx;o(custom)", "ffcustom")]
    [InlineData(255L, "16^xx;t", "ff")]
    [InlineData(65535L, "16^XX.XX", "FF.FF")]
    [InlineData(65535L, "16^XX\ud83d\udca0XX", "FF\ud83d\udca0FF")]
    [InlineData(2L, "2^#XXX", "010")]
    public void FormatsRadix(long value, string picture, string expected)
    {
        foreach (var level in new[] { XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            Assert.Equal(expected, Evaluate($"format-integer({value}, '{picture}')", level));
    }

    /// <summary>Validates zero, boundaries and grouping for every supported base.</summary>
    /// <param name="radix">The base.</param>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    [InlineData(35)]
    [InlineData(36)]
    public void EveryRadix(int radix)
    {
        Assert.Equal("0", Evaluate($"format-integer(0, '{radix}^X')", XPathCompatibility.XPath40));
        Assert.Equal("1", Evaluate($"format-integer(1, '{radix}^X')", XPathCompatibility.XPath40));
        Assert.Equal("-1", Evaluate($"format-integer(-1, '{radix}^X')", XPathCompatibility.XPath40));
        Assert.Equal("10", Evaluate($"format-integer({radix}, '{radix}^X')", XPathCompatibility.XPath40));
        Assert.Equal("11", Evaluate($"format-integer({radix + 1}, '{radix}^X')", XPathCompatibility.XPath40));
        Assert.Equal("0100", Evaluate($"format-integer({radix * radix}, '{radix}^XXXX')", XPathCompatibility.XPath40));
        Assert.Equal("1_000", Evaluate($"format-integer({radix * radix * radix}, '{radix}^X_XXX')", XPathCompatibility.XPath40));
        Assert.Equal("00_1_000", Evaluate($"format-integer({radix * radix * radix}, '{radix}^XX_X_XXX')", XPathCompatibility.XPath40));
        Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyz"[radix - 1].ToString(), Evaluate($"format-integer({radix - 1}, '{radix}^x')", XPathCompatibility.XPath40));
    }

    /// <summary>Rejects invalid radix pictures without admitting them in 3.1.</summary>
    /// <param name="picture">The malformed picture.</param>
    [Theory]
    [InlineData("1^X")]
    [InlineData("37^X")]
    [InlineData("02^X")]
    [InlineData("16^xX")]
    [InlineData("16^Xx")]
    [InlineData("16^X0")]
    [InlineData("16^#X#X")]
    [InlineData("16^-XX")]
    [InlineData("16^XX-")]
    [InlineData("16^X--X")]
    [InlineData("16^##")]
    [InlineData("16^XXX;o(")]
    public void RejectsMalformedRadix(string picture)
    {
        foreach (var level in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40 })
        {
            var error = Assert.ThrowsAny<Exception>(() => Evaluate($"format-integer(255, '{picture}')", level));
            Assert.Contains("FODF1310", error.Message);
        }
    }

    /// <summary>Preserves decimal caret grouping across profiles.</summary>
    /// <param name="picture">The decimal picture.</param>
    /// <param name="expected">The grouped result.</param>
    [Theory]
    [InlineData("10^0000", "00^0999")]
    [InlineData("9^9999", "0^0999")]
    public void PreservesDecimalCaret(string picture, string expected)
    {
        foreach (var level in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40 })
            Assert.Equal(expected, Evaluate($"format-integer(999, '{picture}')", level));
    }

    /// <summary>Keeps new pictures unavailable to the 3.1 execution path.</summary>
    [Fact]
    public void PreservesLowerProfileAndEmptyValue()
    {
        var error = Assert.ThrowsAny<Exception>(() => Evaluate("format-integer(255, '16^xx')", XPathCompatibility.XPath31));
        Assert.Contains("FODF1310", error.Message);
        Assert.Equal("", Evaluate("format-integer((), '16^xx')", XPathCompatibility.XPath40));
    }

    private static string Evaluate(string expression, XPathCompatibility level)
        => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = level }).Evaluate((IXdmNode?)null).ToString();
}
