// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Consumes the exact decimal representation through the public reviewed contract.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; exact-decimal capacity foundation (1.46.0).                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>Consumes the reviewed decimal representation through public declarations only.</summary>
public sealed class ExactDecimalContractTests
{
    /// <summary>Constructs exact decimals from caller-selected query capacity without legacy decimal narrowing.</summary>
    /// <param name="coefficient">Public coefficient input.</param>
    /// <param name="scale">Public supplied scale.</param>
    /// <param name="expected">Exact invariant output.</param>
    [Theory]
    [InlineData("123456789012345678901234567890123456789",2,"1234567890123456789012345678901234567.89")]
    [InlineData("-1000000000000000000",19,"-0.1")]
    [InlineData("0",32,"0")]
    public void PublicContractRetainsExactValue(string coefficient,int scale,string expected)
    {
        var limits=new SchemaQueryLimits(1,1,1,64,64);
        var value=new XdmExactDecimal(BigInteger.Parse(coefficient,CultureInfo.InvariantCulture),scale,limits.MaxNumericDigits,limits.MaxDecimalScale);
        Assert.Equal(expected,value.ToInvariantString(128));
    }
}
