// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded exact terminating decimal division.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact terminating division profile (1.51.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks terminating division against independent rational identities and bounded failure paths.</summary>
/// <param name="output">Captures scoped allocation observations in the test receipt.</param>
public sealed class ExactDivisionTests(ITestOutputHelper output)
{
    private static readonly Action Available=()=>{};
    /// <summary>Arbitrary and constructed rational values retain exact results or explicitly refuse nontermination.</summary>
    [Fact] public void IndependentRationalOracle()
    {
        var l=new ExactArithmeticLimits(64,64);var random=new Random(9417);
        for(int i=0;i<5000;i++)
        {
            BigInteger a=random.Next(-1000000,1000000),b=random.Next(1,10001)*(random.Next(2)==0?-1:1);
            if(i>=3000){int common=new[]{1,3,7,11}[random.Next(4)];a*=common;b=common*BigInteger.Pow(2,random.Next(16))*BigInteger.Pow(5,random.Next(16));}
            int sa=random.Next(9),sb=random.Next(9);var d=BigInteger.Abs(b)/BigInteger.GreatestCommonDivisor(BigInteger.Abs(a),BigInteger.Abs(b));
            while(d%2==0)d/=2;while(d%5==0)d/=5;
            if(!d.IsOne){Assert.Contains("terminating",Assert.Throws<NotSupportedException>(()=>l.Divide(new(a,sa,false),new(b,sb,false),Available)).Message);continue;}
            var q=l.Divide(new(a,sa,sa==0),new(b,sb,sb==0),Available);Assert.False(q.Integer);
            Assert.Equal(a*BigInteger.Pow(10,q.Scale+sb),q.Coefficient*b*BigInteger.Pow(10,sa));
            Assert.True(q.Scale==0||q.Coefficient%10!=0);
        }
    }
    /// <summary>Extreme scale cancellation remains finite and exponent overflow cannot escape quotas.</summary>
    [Fact] public void ExtremeScalesAndCapacity()
    {
        var l=new ExactArithmeticLimits(3,int.MaxValue);var tiny=new ExactNumber(1,int.MaxValue,false);var one=new ExactNumber(1,0,true);
        Assert.Equal(tiny,l.Divide(tiny,one,Available));Assert.Equal(new ExactNumber(1,0,false),l.Divide(tiny,tiny,Available));
        Assert.Throws<NotSupportedException>(()=>l.Divide(one,tiny,Available));
        Assert.Throws<NotSupportedException>(()=>l.Divide(tiny,new(2,0,true),Available));
        Assert.Equal(new ExactNumber(5,int.MaxValue,false),l.Divide(tiny,new(2,1,false),Available));
        Assert.Equal(new ExactNumber(625,4,false),new ExactArithmeticLimits(3,4).Divide(one,new(16,0,true),Available));
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(3,8).Divide(one,new(32,0,true),Available));
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(3,8).Divide(new(1000,0,true),new(2,0,true),Available));
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(8,2).Divide(one,new(8,0,true),Available));
    }
    /// <summary>GCD reduction cancels nondecimal denominator factors and wide coefficients remain exact.</summary>
    [Fact] public void GcdCancellationAndWideCoefficient()
    {
        var l=new ExactArithmeticLimits(10010,10010);var n=BigInteger.Pow(10,10000)+8;
        var q=l.Divide(new(n,0,true),new(8,0,true),Available);Assert.Equal(n/8,q.Coefficient);Assert.Equal(0,q.Scale);Assert.False(q.Integer);
        Assert.Equal(new ExactNumber(5,1,false),l.Divide(new(3,0,true),new(6,0,true),Available));
        Assert.Equal(new ExactNumber(-5,1,false),l.Divide(new(-3,0,true),new(6,0,true),Available));
        Assert.Equal(new ExactNumber(40,0,false),l.Divide(new(120,2,false),new(3,2,false),Available));
    }
    /// <summary>Availability is checked inside factor stripping and growth as well as before errors and zero.</summary>
    [Fact] public void AvailabilityDuringNumericWork()
    {
        var l=new ExactArithmeticLimits(1024,1024);var right=new ExactNumber(BigInteger.Pow(5,200),0,true);
        int total=0;l.Divide(new(1,0,true),right,()=>total++);Assert.True(total>50);
        foreach(int stop in new[]{1,5,20,total-2})
        {int calls=0;Assert.Throws<OperationCanceledException>(()=>l.Divide(new(1,0,true),right,()=>{if(++calls==stop)throw new OperationCanceledException();}));Assert.Equal(stop,calls);}
        Assert.Contains("FOAR0001",Assert.Throws<DivideByZeroException>(()=>l.Divide(new(0,0,true),new(0,0,true),Available)).Message);
        Assert.Equal(new ExactNumber(0,0,false),l.Divide(new(0,0,true),new(3,0,true),Available));
    }
    /// <summary>Warmed owned-input refusals allocate only bounded exception/guard overhead rather than scale expansion.</summary>
    [Fact] public void RefusalAllocationStaysBounded()
    {
        var l=new ExactArithmeticLimits(2,int.MaxValue);var one=new ExactNumber(1,0,true);var tiny=new ExactNumber(1,int.MaxValue,false);
        foreach(var pair in new[]{("huge-scale",tiny),("nonterminating",new ExactNumber(3,0,true)),("growth",new ExactNumber(16,0,true))})
        {
            void Refuse(){try{l.Divide(one,pair.Item2,Available);}catch(NotSupportedException){return;}throw new Exception("Expected refusal");}
            for(int i=0;i<500;i++)Refuse();long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Refuse();long bytes=(GC.GetAllocatedBytesForCurrentThread()-start)/1000;
            output.WriteLine($"Division refusal {pair.Item1}: {bytes} bytes/operation");Assert.InRange(bytes,0,16384);
        }
    }
}
