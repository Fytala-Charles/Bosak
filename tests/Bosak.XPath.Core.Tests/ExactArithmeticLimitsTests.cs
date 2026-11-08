// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded exact arithmetic preflight and supplied representation quotas.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; bounded exact arithmetic profile (1.48.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks exact arithmetic before-allocation capacity and pre-normalization input admission.</summary>
public sealed class ExactArithmeticLimitsTests
{
    private readonly ITestOutputHelper _output;
    /// <summary>Retains refusal cost observations.</summary>
    /// <param name="output">The test receipt writer.</param>
    public ExactArithmeticLimitsTests(ITestOutputHelper output)=>_output=output;
    private static readonly Action Check=()=>{};
    /// <summary>Raw CLR decimal scale/coefficient are checked before normalization.</summary>
    [Fact] public void SuppliedDecimalRepresentationCountsBeforeNormalization()
    {
        var raw=XdmValue.FromDecimal(1.0000m);
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(1,4).Read(raw));
        Assert.Throws<NotSupportedException>(()=>new ExactArithmeticLimits(5,3).Read(raw));
        var admitted=new ExactArithmeticLimits(5,4).Read(raw);Assert.Equal(new BigInteger(1),admitted.Coefficient);Assert.Equal(0,admitted.Scale);Assert.False(admitted.Integer);
        var normalized=XdmValue.FromDecimal(new XdmExactDecimal(10000,4,5,4));Assert.Equal(admitted,new ExactArithmeticLimits(1,0).Read(normalized));
    }
    /// <summary>All operands and limits require explicit valid primitive numeric representation.</summary>
    [Fact] public void RejectsInvalidAndUnsupportedAdmission()
    {
        var limits=new ExactArithmeticLimits(4,4);
        Assert.Throws<InvalidOperationException>(()=>default(ExactArithmeticLimits).Read(XdmValue.FromInteger(0)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new ExactArithmeticLimits(0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new ExactArithmeticLimits(1,-1));
        foreach(var value in new[]{XdmValue.FromDouble(1),XdmValue.FromString("1"),XdmValue.FromInteger(1,"int"),XdmValue.FromInteger(1,"integer","Q{u}T"),XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object())})
            Assert.Throws<NotSupportedException>(()=>limits.Read(value));
        Assert.Throws<ArgumentException>(()=>limits.ToValue(new(1,1,true)));
    }
    /// <summary>Exact integer sum/product frontier agrees with independent bounded BigInteger results.</summary>
    [Fact] public void IntegerFrontierMatchesIndependentResults()
    {
        var random=new Random(468);
        for(int digits=1;digits<=5;digits++)
        {
            int maximum=(int)BigInteger.Pow(10,digits)-1;var limits=new ExactArithmeticLimits(digits,0);
            for(int i=0;i<160;i++)
            {
                int a=random.Next(-maximum,maximum+1),b=random.Next(-maximum,maximum+1);var left=limits.Read(XdmValue.FromInteger(a));var right=limits.Read(XdmValue.FromInteger(b));
                foreach(bool product in new[]{false,true})
                {
                    BigInteger expected=product?(BigInteger)a*b:(BigInteger)a+b;
                    if(BigInteger.Abs(expected).ToString(CultureInfo.InvariantCulture).Length>digits)
                        Assert.Throws<NotSupportedException>(()=>product?limits.Multiply(left,right,Check):limits.Add(left,right,Check));
                    else Assert.Equal(expected,(product?limits.Multiply(left,right,Check):limits.Add(left,right,Check)).Coefficient);
                }
            }
        }
    }
    /// <summary>Intermediate digit and scale capacities apply even when normalization would shrink a result.</summary>
    [Fact] public void IntermediateCapacityCannotBeBypassedByNormalization()
    {
        var l=new ExactArithmeticLimits(2,2);var a=l.Read(XdmValue.FromDecimal(9.9m));var b=l.Read(XdmValue.FromDecimal(0.1m));
        Assert.Throws<NotSupportedException>(()=>l.Add(a,b,Check));
        var zero=l.Add(a,l.Negate(a),Check);Assert.True(zero.Coefficient.IsZero);Assert.Equal(0,zero.Scale);
        Assert.Throws<NotSupportedException>(()=>l.Multiply(l.Read(XdmValue.FromDecimal(2.5m)),l.Read(XdmValue.FromInteger(4)),Check));
        var s=new ExactArithmeticLimits(8,1);var x=s.Read(XdmValue.FromDecimal(0.1m));Assert.Throws<NotSupportedException>(()=>s.Multiply(x,x,Check));
    }
    /// <summary>Alignment preserves exact scale and primitive numeric promotion.</summary>
    [Fact] public void AlignmentAndPromotionAreExact()
    {
        var l=new ExactArithmeticLimits(16,8);var a=l.Read(XdmValue.FromInteger(2));var b=l.Read(XdmValue.FromDecimal(0.03m));
        var result=l.ToValue(l.Add(a,b,Check));Assert.Equal(XdmValueKind.Decimal,result.Kind);Assert.Equal("2.03",result.ExactDecimalValue.ToInvariantString(4));
        Assert.Equal("0.06",l.ToValue(l.Multiply(a,b,Check)).ExactDecimalValue.ToInvariantString(4));
    }
    /// <summary>Availability checks interrupt before publishing a partially completed result.</summary>
    [Fact] public void CancellationCallbackIsObserved()
    {
        var l=new ExactArithmeticLimits(16,8);var a=l.Read(XdmValue.FromInteger(2));int checks=0;
        Assert.Throws<OperationCanceledException>(()=>l.Add(a,a,()=>{if(++checks==2)throw new OperationCanceledException();}));
    }
    /// <summary>Oversized coefficients and extreme alignment/scale refuse without constructing huge arithmetic results.</summary>
    [Fact] public void RefusalPrecedesLargeArithmeticAllocation()
    {
        var l=new ExactArithmeticLimits(32,int.MaxValue);var huge=BigInteger.Pow(10,100000);var wide=new ExactNumber(huge,0,true);
        var tiny=l.Read(XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue)));var one=l.Read(XdmValue.FromInteger(1));
        var cases=new Dictionary<string,Action>{{"oversized-input",()=>l.Multiply(wide,one,Check)},{"extreme-alignment",()=>l.Add(one,tiny,Check)},{"scale-sum-overflow",()=>l.Multiply(tiny,tiny,Check)}};
        foreach(var row in cases)
        {
            for(int i=0;i<4;i++)Assert.Throws<NotSupportedException>(row.Value);
            long before=GC.GetAllocatedBytesForCurrentThread();try{row.Value();throw new Exception("Expected refusal");}catch(NotSupportedException){}
            long bytes=GC.GetAllocatedBytesForCurrentThread()-before;_output.WriteLine($"Arithmetic refusal {row.Key}: {bytes} bytes (inputs already owned).");Assert.InRange(bytes,0,16384);
        }
    }
}
