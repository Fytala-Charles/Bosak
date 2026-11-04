// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies exact comparisons, numeric map keys and bounded scalar formatting.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact scalar/key integration   (1.49.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks exact order, IEEE map keys, extreme scales and bounded scalar formatting.</summary>
public sealed class ExactScalarTests
{
    private readonly ITestOutputHelper _output;
    private static int _hashSink;
    private static bool _equalSink;
    private static readonly Action Check=()=>{};
    /// <summary>Retains exact-key allocation observations.</summary>
    /// <param name="output">Receipt output.</param>
    public ExactScalarTests(ITestOutputHelper output)=>_output=output;
    private static XdmValue Decimal(string text)=>XdmValue.FromDecimal(ExactDecimalLexical.Parse(text,256,128));
    /// <summary>Random finite decimal order matches independent rational cross multiplication.</summary>
    [Fact] public void ComparisonMatchesIndependentRationals()
    {
        var random=new Random(468);var limits=new ExactArithmeticLimits(128,64);
        for(int i=0;i<1200;i++)
        {
            BigInteger a=random.NextInt64(),b=random.NextInt64();if(i%2!=0)a=-a;if(i%3==0)b=-b;
            int sa=random.Next(0,30),sb=random.Next(0,30);
            var left=limits.Read(XdmValue.FromDecimal(new XdmExactDecimal(a,sa,64,64)));var right=limits.Read(XdmValue.FromDecimal(new XdmExactDecimal(b,sb,64,64)));
            int expected=(a*BigInteger.Pow(10,sb)).CompareTo(b*BigInteger.Pow(10,sa));
            Assert.Equal(Math.Sign(expected),Math.Sign(limits.Compare(left,right,Check)));
            Assert.Equal(-Math.Sign(expected),Math.Sign(limits.Compare(right,left,Check)));
        }
    }
    /// <summary>Large scale gaps use decimal order without expanding powers or output padding.</summary>
    [Fact] public void ExtremeScaleComparisonDoesNotAlign()
    {
        var l=new ExactArithmeticLimits(2,int.MaxValue);var tiny=l.Read(XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue)));
        var one=l.Read(XdmValue.FromInteger(1));var zero=l.Read(XdmValue.FromInteger(0));
        Assert.True(l.Compare(tiny,one,Check)<0);Assert.True(l.Compare(tiny,zero,Check)>0);Assert.True(l.Compare(l.Negate(tiny),l.Negate(one),Check)>0);
        Assert.Throws<NotSupportedException>(()=>l.Format(tiny,Check));
    }
    /// <summary>Power-of-ten frontiers exercise exact digit estimation and equal-order alignment.</summary>
    [Fact] public void DecimalOrderFrontiersAreExact()
    {
        var l=new ExactArithmeticLimits(512,512);
        for(int digits=1;digits<250;digits++)
        {
            var n=BigInteger.Pow(10,digits);var a=l.Read(XdmValue.FromDecimal(new XdmExactDecimal(n-1,digits,512,512)));var b=l.Read(XdmValue.FromInteger(1));
            Assert.True(l.Compare(a,b,Check)<0);var c=l.Read(XdmValue.FromDecimal(new XdmExactDecimal(n+1,digits,512,512)));Assert.True(l.Compare(c,b,Check)>0);
        }
        var x=l.Read(Decimal("0.2"));var y=l.Read(Decimal("0.15"));Assert.True(l.Compare(x,y,Check)>0);
    }
    /// <summary>Availability is observed while comparing orders and before formatted publication.</summary>
    [Fact] public void ComparisonAndFormattingHonorAvailability()
    {
        var l=new ExactArithmeticLimits(16,16);var a=l.Read(Decimal("0.2"));var b=l.Read(Decimal("0.15"));int calls=0;
        Assert.Throws<OperationCanceledException>(()=>l.Compare(a,b,()=>{if(++calls==2)throw new OperationCanceledException();}));
        calls=0;Assert.Throws<OperationCanceledException>(()=>l.Format(a,()=>{if(++calls==2)throw new OperationCanceledException();}));
        Assert.Equal("0.2",l.Format(a,Check));Assert.Equal(5,new ExactArithmeticLimits(1,0).StringCapacity);
    }
    /// <summary>All equivalent primitive numeric map keys share equality and hash codes.</summary>
    [Fact] public void PrimitiveAndWideNumericKeysShareExactHash()
    {
        var comparer=XdmValueEqualityComparer.Instance;
        foreach(var values in new[]{new[]{XdmValue.FromInteger(1),XdmValue.FromDecimal(1.00m),Decimal("1.000000"),XdmValue.FromDouble(1),XdmValue.FromFloat(1)},new[]{XdmValue.FromInteger(0),Decimal("-0.000"),XdmValue.FromDouble(-0d),XdmValue.FromFloat(-0f)}})
            foreach(var a in values)foreach(var b in values){Assert.True(comparer.Equals(a,b));Assert.Equal(comparer.GetHashCode(a),comparer.GetHashCode(b));}
        var wide=BigInteger.Parse("123456789012345678901234567890123456789",CultureInfo.InvariantCulture);
        var i=XdmValue.FromInteger(wide);var d=XdmValue.FromDecimal(new XdmExactDecimal(wide*100,2,64,64));Assert.True(comparer.Equals(i,d));Assert.Equal(comparer.GetHashCode(i),comparer.GetHashCode(d));
    }
    /// <summary>Binary numeric keys retain their exact mathematical values rather than decimal rounding.</summary>
    [Fact] public void IeeeKeysUseExactFiniteValues()
    {
        var c=XdmValueEqualityComparer.Instance;var binary=XdmValue.FromDouble(1.1);
        var exact=Decimal("1.100000000000000088817841970012523233890533447265625");Assert.True(c.Equals(binary,exact));Assert.Equal(c.GetHashCode(binary),c.GetHashCode(exact));Assert.False(c.Equals(binary,Decimal("1.1")));
        var single=XdmValue.FromFloat(1.1f);var singleExact=Decimal("1.10000002384185791015625");Assert.True(c.Equals(single,singleExact));Assert.Equal(c.GetHashCode(single),c.GetHashCode(singleExact));Assert.False(c.Equals(single,binary));
        Assert.False(c.Equals(XdmValue.FromInteger(new BigInteger(9007199254740993L)),XdmValue.FromDouble(9007199254740992d)));
        Assert.True(c.Equals(XdmValue.FromInteger(BigInteger.Parse("100000000000000000000",CultureInfo.InvariantCulture)),XdmValue.FromDouble(1e20)));
    }
    /// <summary>NaN/infinities and the smallest subnormal retain consistent numeric key identity.</summary>
    [Fact] public void SpecialAndSubnormalKeysAreStable()
    {
        var c=XdmValueEqualityComparer.Instance;var nan=XdmValue.FromDouble(double.NaN);var otherNaN=XdmValue.FromFloat(float.NaN);Assert.True(c.Equals(nan,otherNaN));Assert.Equal(c.GetHashCode(nan),c.GetHashCode(otherNaN));
        Assert.False(c.Equals(nan,XdmValue.FromDouble(double.PositiveInfinity)));Assert.False(c.Equals(XdmValue.FromDouble(double.PositiveInfinity),XdmValue.FromDouble(double.NegativeInfinity)));
        Assert.True(c.Equals(XdmValue.FromDouble(double.PositiveInfinity),XdmValue.FromFloat(float.PositiveInfinity)));
        var coefficient=BigInteger.Pow(5,1074);var exact=XdmValue.FromDecimal(new XdmExactDecimal(coefficient,1074,1024,1074));var epsilon=XdmValue.FromDouble(double.Epsilon);
        Assert.True(c.Equals(exact,epsilon));Assert.Equal(c.GetHashCode(exact),c.GetHashCode(epsilon));Assert.False(c.Equals(exact,XdmValue.FromInteger(0)));
    }
    /// <summary>Map updates find mathematically equal wide keys, keep latest annotations and preserve order.</summary>
    [Fact] public void WideMapReplacementAndRemovalPreserveIdentity()
    {
        var n=BigInteger.Pow(10,50)+123;var integer=XdmValue.FromInteger(n);var dec=XdmValue.FromDecimal(new XdmExactDecimal(n,0,64,64),"decimal","Q{u}T");
        var map=new XdmMap();map.Add(integer,XdmValue.FromString("old"));map.Add(XdmValue.FromString("tail"),XdmValue.True);map.Add(dec,XdmValue.FromString("new"));
        Assert.Equal(2,map.Count);Assert.True(map.TryGetValue(integer,out var value));Assert.Equal("new",value.StringValue);Assert.Equal("Q{u}T",map.Keys.First().UserSchemaTypeName);
        var removed=map.WithRemoved(integer);Assert.Equal(1,removed.Count);Assert.Equal(2,map.Count);
    }
    /// <summary>Nonnumeric families remain distinct and extreme scale keys never stringify their exponent.</summary>
    [Fact] public void ExtremeScaleAndNonnumericKeysRemainDistinct()
    {
        var c=XdmValueEqualityComparer.Instance;var x=XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue));var y=XdmValue.FromDecimal(new XdmExactDecimal(10,int.MaxValue,2,int.MaxValue));
        Assert.False(c.Equals(x,y));Assert.False(c.Equals(x,XdmValue.FromInteger(0)));Assert.True(c.Equals(x,x));
        var map=new XdmMap();map.Add(x,XdmValue.True);map.Add(y,XdmValue.False);Assert.Equal(2,map.Count);Assert.True(map.TryGetValue(x,out var value));Assert.True(value.BooleanValue);
        Assert.False(c.Equals(XdmValue.True,XdmValue.FromInteger(1)));Assert.False(c.Equals(XdmValue.FromString("1"),XdmValue.FromInteger(1)));
    }
    private static long Bytes(Action action)
    {for(int i=0;i<500;i++)action();long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;i++)action();return(GC.GetAllocatedBytesForCurrentThread()-before)/10000;}
    /// <summary>Cached wide and extreme-scale keys require no strings, padding or normalization quotients.</summary>
    [Fact] public void CachedExactKeysAvoidAllocation()
    {
        var c=XdmValueEqualityComparer.Instance;var coefficient=BigInteger.Pow(10,10000)+1;var integer=XdmValue.FromInteger(coefficient);var dec=XdmValue.FromDecimal(new XdmExactDecimal(coefficient,int.MaxValue,10001,int.MaxValue));
        var tiny=XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue));
        var rows=new Dictionary<string,long>{{"wide-integer-key-hash",Bytes(()=>_hashSink=c.GetHashCode(integer))},{"wide-decimal-key-hash",Bytes(()=>_hashSink=c.GetHashCode(dec))},{"extreme-scale-key-equality",Bytes(()=>_equalSink=c.Equals(tiny,tiny))}};
        foreach(var row in rows){_output.WriteLine($"Exact key allocation {row.Key}: {row.Value} bytes/operation (10000 iterations; input pre-owned).");Assert.Equal(0,row.Value);}GC.KeepAlive(_hashSink);GC.KeepAlive(_equalSink);
    }
}
