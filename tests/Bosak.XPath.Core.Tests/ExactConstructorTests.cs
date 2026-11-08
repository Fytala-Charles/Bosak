// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded primitive numeric constructor conversion.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact numeric constructor profile (1.52.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Verifies numeric constructor semantics, capacity, availability and scoped cost.</summary>
/// <param name="output">Captures allocation observations in test receipts.</param>
public sealed class ExactConstructorTests(ITestOutputHelper output)
{
    private static readonly Action Available=()=>{};
    /// <summary>Numeric conversion agrees with independent decimal truncation for signed finite values.</summary>
    [Fact] public void SignedDecimalOracle()
    {
        var l=new ExactArithmeticLimits(32,16);var random=new Random(1052);
        for(int i=0;i<5000;i++)
        {
            int coefficient=random.Next(-1000000,1000000),scale=random.Next(7);var n=new ExactNumber(coefficient,scale,scale==0);
            var q=l.ConvertNumeric(n,true,Available);Assert.True(q.Integer);Assert.Equal(0,q.Scale);
            Assert.Equal(decimal.Truncate(coefficient/(decimal)BigInteger.Pow(10,scale)),(decimal)q.Coefficient);
            var d=l.ConvertNumeric(n,false,Available);Assert.False(d.Integer);Assert.Equal(n.Coefficient,d.Coefficient);Assert.Equal(n.Scale,d.Scale);
        }
    }
    /// <summary>Extreme scales return zero before powers while large caller ceilings alone allocate no large guard.</summary>
    [Fact] public void ExtremeScalesAndLargeLimits()
    {
        var l=new ExactArithmeticLimits(int.MaxValue,int.MaxValue);var tiny=new ExactNumber(-1,int.MaxValue,false);
        Assert.Equal(new ExactNumber(0,0,true),l.ConvertNumeric(tiny,true,Available));Assert.Equal(tiny,l.ConvertNumeric(tiny,false,Available));
        Assert.Equal(new ExactNumber(0,0,true),l.ConvertNumeric(new(0,int.MaxValue,false),true,Available));
        Assert.Equal(new ExactNumber(-12,0,true),l.ConvertNumeric(new(-1299,2,false),true,Available));
    }
    /// <summary>Required admission precedes truncation, including default limits and over-capacity supplied data.</summary>
    [Fact] public void SuppliedQuotaPrecedesShrinkage()
    {
        var l=new ExactArithmeticLimits(3,2);
        Assert.Throws<NotSupportedException>(()=>l.ConvertNumeric(new(1000,2,false),true,Available));
        Assert.Throws<NotSupportedException>(()=>l.ConvertNumeric(new(1,3,false),true,Available));
        Assert.Throws<InvalidOperationException>(()=>default(ExactArithmeticLimits).ConvertNumeric(new(1,0,true),false,Available));
        Assert.Throws<ArgumentException>(()=>l.ConvertNumeric(new(1,1,true),true,Available));
    }
    /// <summary>Wide integral parts retain exact digits beyond CLR integer and decimal limits.</summary>
    [Fact] public void WideValuesRetainTheirIntegralPart()
    {
        var l=new ExactArithmeticLimits(10010,10010);var n=BigInteger.Pow(10,10000)+99;
        Assert.Equal(n/100,l.ConvertNumeric(new(n,2,false),true,Available).Coefficient);
        Assert.Equal(-n/100,l.ConvertNumeric(new(-n,2,false),true,Available).Coefficient);
        Assert.Equal(new ExactNumber(n,0,false),l.ConvertNumeric(new(n,0,true),false,Available));
    }
    /// <summary>Availability checks run before identity conversion and between bounded truncation primitives.</summary>
    [Fact] public void AvailabilityDuringConversion()
    {
        var l=new ExactArithmeticLimits(64,64);var n=new ExactNumber(BigInteger.Pow(10,40)+9,2,false);int total=0;
        l.ConvertNumeric(n,true,()=>total++);Assert.True(total>=5);
        foreach(int stop in new[]{1,3,total}){int calls=0;Assert.Throws<OperationCanceledException>(()=>l.ConvertNumeric(n,true,()=>{if(++calls==stop)throw new OperationCanceledException();}));}
        Assert.Throws<OperationCanceledException>(()=>l.ConvertNumeric(new(1,0,true),false,()=>throw new OperationCanceledException()));
    }
    /// <summary>Owned-input scalar and extreme-scale conversions avoid allocation and capacity failures remain bounded.</summary>
    [Fact] public void ScopedAllocationObservations()
    {
        var l=new ExactArithmeticLimits(2,int.MaxValue);var tiny=new ExactNumber(1,int.MaxValue,false);var one=new ExactNumber(1,0,true);
        var huge=new ExactNumber(BigInteger.Pow(10,1000),0,true);
        foreach(string label in new[]{"tiny-integer","integer-to-decimal","input-refusal"})
        {
            void Run(){if(label=="input-refusal"){try{l.ConvertNumeric(huge,true,Available);}catch(NotSupportedException){return;}throw new Exception("Expected refusal");}else l.ConvertNumeric(label=="tiny-integer"?tiny:one,label=="tiny-integer",Available);}
            for(int i=0;i<500;i++)Run();long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;i++)Run();long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/10000;
            output.WriteLine($"Constructor allocation {label}: {bytes} bytes/operation");Assert.InRange(bytes,0,label=="input-refusal"?16384:0);
        }
    }
}
