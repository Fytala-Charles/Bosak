// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Consumes all reviewed exact numeric carrier signatures through the public API.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact numeric carrier transport (1.47.0).                                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>Compiles and exercises all six reviewed exact XdmValue member signatures publicly.</summary>
public sealed class ExactNumericValueContractTests
{
    /// <summary>All reviewed overloads/accessors are available without access to Core internals.</summary>
    /// <param name="text">An arbitrary precision integer.</param>
    [Theory][InlineData("0")][InlineData("42")][InlineData("123456789012345678901234567890123456789")]
    public void ReviewedNumericWitnessIsUsable(string text)
    {
        var n=BigInteger.Parse(text,CultureInfo.InvariantCulture);var exact=new XdmExactDecimal(n,5,64,64);
        Assert.Equal(n,XdmValue.FromInteger(n).BigIntegerValue);Assert.Equal(n,XdmValue.FromInteger(n,"integer","Q{urn:t}Whole").BigIntegerValue);
        Assert.Equal(exact,XdmValue.FromDecimal(exact).ExactDecimalValue);Assert.Equal(exact,XdmValue.FromDecimal(exact,"decimal","Q{urn:t}Fraction").ExactDecimalValue);
    }
}
