// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded exact quotient and remainder arithmetic.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; bounded quotient/remainder profile (1.50.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks exact quotient/remainder against independent decimal arithmetic and finite capacities.</summary>
public sealed class ExactQuotientTests
{
    /// <summary>Signed decimal results and type promotion match independent bounded decimal arithmetic.</summary>
    [Fact] public void RandomDecimalOracleAndIdentity()
    {
        var limits=new ExactArithmeticLimits(32,16);var random=new Random(7391);
        for(int i=0;i<2000;i++)
        {
            int ac=random.Next(-1000000,1000000),bc=random.Next(-1000000,1000000);if(bc==0)bc=1;
            int sa=random.Next(0,5),sb=random.Next(0,5);
            var a=new ExactNumber(ac,sa,sa==0);var b=new ExactNumber(bc,sb,sb==0);
            decimal x=ac/(decimal)BigInteger.Pow(10,sa),y=bc/(decimal)BigInteger.Pow(10,sb);
            var q=limits.QuotientOrRemainder(a,b,false,()=>{});var r=limits.QuotientOrRemainder(a,b,true,()=>{});
            Assert.True(q.Integer);Assert.Equal(decimal.Truncate(x/y),(decimal)q.Coefficient);
            Assert.Equal(x%y,(decimal)r.Coefficient/(decimal)BigInteger.Pow(10,r.Scale));Assert.Equal(a.Integer&&b.Integer,r.Integer);
            Assert.Equal(0,limits.Compare(a,limits.Add(limits.Multiply(q,b,()=>{}),r,()=>{}),()=>{}));
        }
    }
    /// <summary>Huge scale gaps short-circuit smaller/equal magnitudes and refuse alignment before expansion.</summary>
    [Fact] public void ExtremeScalesAndAlignmentRefusal()
    {
        var l=new ExactArithmeticLimits(2,int.MaxValue);var tiny=new ExactNumber(1,int.MaxValue,false);var one=new ExactNumber(1,0,true);
        Assert.Equal(BigInteger.Zero,l.QuotientOrRemainder(tiny,one,false,()=>{}).Coefficient);
        Assert.Equal(tiny,l.QuotientOrRemainder(tiny,one,true,()=>{}));
        Assert.Equal(BigInteger.One,l.QuotientOrRemainder(tiny,tiny,false,()=>{}).Coefficient);
        Assert.Equal(BigInteger.Zero,l.QuotientOrRemainder(tiny,tiny,true,()=>{}).Coefficient);
        Assert.Throws<NotSupportedException>(()=>l.QuotientOrRemainder(one,tiny,false,()=>{}));
        Assert.Throws<NotSupportedException>(()=>l.QuotientOrRemainder(one,tiny,true,()=>{}));
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(2,2).QuotientOrRemainder(new(99,0,true),new(1,1,false),false,()=>{}));
    }
    /// <summary>Zero errors and explicit availability checks precede arithmetic, including fast paths.</summary>
    [Fact] public void ZeroAndAvailability()
    {
        var l=new ExactArithmeticLimits(8,8);foreach(bool remainder in new[]{false,true})
        {
            Assert.Contains("FOAR0001",Assert.Throws<DivideByZeroException>(()=>l.QuotientOrRemainder(new(0,0,true),new(0,0,true),remainder,()=>{})).Message);
            Assert.Throws<OperationCanceledException>(()=>l.QuotientOrRemainder(new(1,0,true),new(2,0,true),remainder,()=>throw new OperationCanceledException()));
            int calls=0;Assert.Throws<OperationCanceledException>(()=>l.QuotientOrRemainder(new(123,0,true),new(2,1,false),remainder,()=>{if(++calls==5)throw new OperationCanceledException();}));
        }
    }
    /// <summary>Quotient and remainder retain coefficients larger than CLR integer and decimal limits.</summary>
    [Fact] public void WideCoefficientsRemainExact()
    {
        var l=new ExactArithmeticLimits(1100,100);var n=BigInteger.Pow(10,1000)+7;var a=new ExactNumber(n,0,true);var b=new ExactNumber(3,0,true);
        Assert.Equal(n/3,l.QuotientOrRemainder(a,b,false,()=>{}).Coefficient);Assert.Equal(n%3,l.QuotientOrRemainder(a,b,true,()=>{}).Coefficient);
        var decimalInput=new ExactNumber(n,2,false);var r=l.QuotientOrRemainder(decimalInput,b,true,()=>{});Assert.False(r.Integer);
        Assert.Equal(0,l.Compare(decimalInput,l.Add(l.Multiply(l.QuotientOrRemainder(decimalInput,b,false,()=>{}),b,()=>{}),r,()=>{}),()=>{}));
    }
}
