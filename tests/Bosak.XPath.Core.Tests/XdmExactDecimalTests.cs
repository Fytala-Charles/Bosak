// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Verifies exact decimal normalization, capacity, lexical ingress and allocation refusals.
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
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks exact decimal semantics, supplied capacity and allocation-before-refusal boundaries.</summary>
public sealed class XdmExactDecimalTests
{
    private readonly ITestOutputHelper _output;
    /// <summary>Captures measured refusal allocations in the retained test receipt.</summary>
    /// <param name="output">Test receipt output.</param>
    public XdmExactDecimalTests(ITestOutputHelper output) => _output = output;
    private static BigInteger Number(string value) => BigInteger.Parse(value, CultureInfo.InvariantCulture);
    private static XdmExactDecimal Value(string coefficient, int scale=0) => new(Number(coefficient),scale,256,512);
    /// <summary>Normalizes positive, negative, zero and long trailing-zero coefficients exactly.</summary>
    /// <param name="coefficient">Supplied coefficient.</param>
    /// <param name="scale">Supplied scale.</param>
    /// <param name="normalized">Expected normalized coefficient.</param>
    /// <param name="normalizedScale">Expected normalized scale.</param>
    /// <param name="text">Expected invariant text.</param>
    [Theory]
    [InlineData("0",512,"0",0,"0")]
    [InlineData("1234500",4,"12345",2,"123.45")]
    [InlineData("-1234500",4,"-12345",2,"-123.45")]
    [InlineData("1",5,"1",5,"0.00001")]
    [InlineData("-123",5,"-123",5,"-0.00123")]
    [InlineData("1000000000",9,"1",0,"1")]
    [InlineData("1000000000",8,"10",0,"10")]
    [InlineData("1000000000000000000",19,"1",1,"0.1")]
    [InlineData("-1000000000000000000",18,"-1",0,"-1")]
    [InlineData("12345678901234567890123456789012345",3,"12345678901234567890123456789012345",3,"12345678901234567890123456789012.345")]
    public void NormalizesAndFormats(string coefficient,int scale,string normalized,int normalizedScale,string text)
    {
        var value=Value(coefficient,scale);Assert.Equal(Number(normalized),value.Coefficient);Assert.Equal(normalizedScale,value.Scale);
        Assert.Equal(text,value.ToInvariantString(text.Length));
        if(text.Length>1)Assert.Throws<NotSupportedException>(()=>value.ToInvariantString(text.Length-1));
    }
    /// <summary>Default and all admitted signed lexical zeros have the same normalized value.</summary>
    [Fact] public void DefaultIsNumericZero()
    { var zero=default(XdmExactDecimal);Assert.Equal(BigInteger.Zero,zero.Coefficient);Assert.Equal(0,zero.Scale);Assert.Equal("0",zero.ToInvariantString(1));Assert.Equal(zero,Value("0",128));Assert.Equal(zero.GetHashCode(),Value("0",128).GetHashCode()); }
    /// <summary>Rejects invalid bounds separately from supplied representation capacity.</summary>
    [Fact] public void InvalidArgumentsDifferFromCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>new XdmExactDecimal(1,0,0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new XdmExactDecimal(1,0,1,-1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new XdmExactDecimal(1,-1,1,0));
        Assert.Throws<NotSupportedException>(()=>new XdmExactDecimal(1,1,1,0));
        Assert.Throws<NotSupportedException>(()=>new XdmExactDecimal(0,1,1,0));
        Assert.Throws<NotSupportedException>(()=>new XdmExactDecimal(1000,3,3,3));
        Assert.Throws<ArgumentOutOfRangeException>(()=>default(XdmExactDecimal).ToInvariantString(0));
    }
    /// <summary>Digit quotas are exact at powers of ten, including both signs and the bit-bound interval.</summary>
    [Fact] public void DigitCapacityMatchesPowerOfTenBoundaries()
    {
        for(int digits=1;digits<=256;digits++)
        {
            var threshold=BigInteger.Pow(10,digits);var admitted=threshold-1;
            Assert.Equal(admitted,new XdmExactDecimal(admitted,0,digits,0).Coefficient);
            Assert.Equal(-admitted,new XdmExactDecimal(-admitted,0,digits,0).Coefficient);
            Assert.Throws<NotSupportedException>(()=>new XdmExactDecimal(threshold,0,digits,0));
            Assert.Throws<NotSupportedException>(()=>new XdmExactDecimal(-threshold,0,digits,0));
        }
    }
    /// <summary>Normalization has consistent value equality, object equality and dictionary hashing.</summary>
    [Fact] public void EqualityAndHashIgnoreSuppliedTrailingZeros()
    {
        var x=Value("1234500",4);var y=Value("12345",2);Assert.True(x.Equals(y));Assert.True(x.Equals((object)y));Assert.Equal(x.GetHashCode(),y.GetHashCode());
        Assert.False(x.Equals(Value("12346",2)));Assert.False(x.Equals(null));Assert.False(x.Equals("123.45"));
        var map=new Dictionary<XdmExactDecimal,string>{{x,"same"}};Assert.Equal("same",map[y]);
    }
    /// <summary>Formatting is invariant across a culture whose decimal separator is a comma.</summary>
    [Fact] public void InvariantFormattingIgnoresCurrentCulture()
    {
        var previous=CultureInfo.CurrentCulture;
        try{CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("fr-FR");Assert.Equal("-123.45",Value("-1234500",4).ToInvariantString(7));}
        finally{CultureInfo.CurrentCulture=previous;}
    }
    /// <summary>Zero padding and output bounds handle large scales without integer overflow.</summary>
    [Fact] public void ScalePaddingAndExtremeLimits()
    {
        var small=new XdmExactDecimal(1,128,1,128);Assert.Equal("0."+new string('0',127)+"1",small.ToInvariantString(130));
        Assert.Throws<NotSupportedException>(()=>small.ToInvariantString(129));
        var extreme=new XdmExactDecimal(-1,int.MaxValue,1,int.MaxValue);Assert.Throws<NotSupportedException>(()=>extreme.ToInvariantString(int.MaxValue));
        Assert.Equal("1",new XdmExactDecimal(1,0,int.MaxValue,int.MaxValue).ToInvariantString(1));
        Assert.Equal(default,new XdmExactDecimal(0,int.MaxValue,1,int.MaxValue));
    }
    /// <summary>A bounded independent decimal reference checks many exact representable cases.</summary>
    [Fact] public void MatchesIndependentDecimalReference()
    {
        var random=new Random(468);
        for(int i=0;i<400;i++)
        {
            long coefficient=random.NextInt64();if(i%2!=0)coefficient=-coefficient;int scale=random.Next(0,13);
            decimal divisor=1;for(int j=0;j<scale;j++)divisor*=10;
            string expected=((decimal)coefficient/divisor).ToString("0.############################",CultureInfo.InvariantCulture);
            Assert.Equal(expected,new XdmExactDecimal(coefficient,scale,19,12).ToInvariantString(64));
        }
    }
    /// <summary>Long normalized zero suffixes preserve exactly the part outside the supplied scale.</summary>
    [Fact] public void BlockNormalizationRetainsUnscaledZeros()
    {
        var coefficient=BigInteger.Pow(10,180)*12345;
        var value=new XdmExactDecimal(coefficient,175,185,175);
        Assert.Equal(new BigInteger(1234500000),value.Coefficient);Assert.Equal(0,value.Scale);
        Assert.Equal("1234500000",value.ToInvariantString(10));
    }
    /// <summary>Lexical ingress accepts XSD decimal grammar and trims only XML boundary whitespace.</summary>
    /// <param name="text">Supplied decimal lexical representation.</param>
    /// <param name="expected">Normalized numeric text.</param>
    [Theory]
    [InlineData("+00123.4500","123.45")]
    [InlineData("-.5","-0.5")]
    [InlineData("12.","12")]
    [InlineData(" \t\r\n-0.000 \t","0")]
    [InlineData(".00001","0.00001")]
    [InlineData("12345678901234567890123456789012345.6789","12345678901234567890123456789012345.6789")]
    public void LexicalIngressIsExact(string text,string expected)
    { Assert.Equal(expected,ExactDecimalLexical.Parse(text,128,64).ToInvariantString(256)); }
    /// <summary>Malformed decimal syntax refuses instead of using floating-point or culture fallback.</summary>
    /// <param name="text">Malformed decimal representation.</param>
    [Theory]
    [InlineData("")][InlineData(" \t")][InlineData(".")][InlineData("+")][InlineData("-.")]
    [InlineData("1e2")][InlineData("1,2")][InlineData("1 2")][InlineData("1.2.3")]
    [InlineData("NaN")][InlineData("INF")][InlineData("１２")][InlineData("١٢")][InlineData("\u00a01\u00a0")]
    public void LexicalIngressRejectsMalformedInput(string text)
    { Assert.Throws<FormatException>(()=>ExactDecimalLexical.Parse(text,128,64)); }
    /// <summary>All supplied lexical digits and scale count before normalization, including zeros.</summary>
    [Fact] public void LexicalQuotasPrecedeNormalization()
    {
        Assert.Throws<NotSupportedException>(()=>ExactDecimalLexical.Parse("0001",3,0));
        Assert.Equal("1",ExactDecimalLexical.Parse("0001",4,0).ToInvariantString(1));
        Assert.Throws<NotSupportedException>(()=>ExactDecimalLexical.Parse("1.0000",5,3));
        Assert.Throws<NotSupportedException>(()=>ExactDecimalLexical.Parse("0.0000",5,3));
        Assert.Equal("1",ExactDecimalLexical.Parse("1.0000",5,4).ToInvariantString(1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>ExactDecimalLexical.Parse("1",0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>ExactDecimalLexical.Parse("1",1,-1));
    }
    /// <summary>Every partial block size matches an independent BigInteger parse.</summary>
    [Fact] public void LexicalChunkBoundariesMatchBigInteger()
    {
        for(int length=1;length<=80;length++)
        {
            var digits=new string(Enumerable.Range(0,length).Select(i=>(char)('1'+i%9)).ToArray());
            var value=ExactDecimalLexical.Parse("-"+digits,80,0);Assert.Equal(-Number(digits),value.Coefficient);Assert.Equal(0,value.Scale);
        }
    }
    private long RefusalBytes(Action action)
    {
        static void Refuse(Action attempt){try{attempt();throw new Xunit.Sdk.XunitException("Expected capacity refusal.");}catch(NotSupportedException){}}
        for(int i=0;i<4;i++)Refuse(action);
        long before=GC.GetAllocatedBytesForCurrentThread();Refuse(action);long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
        _output.WriteLine($"Caught capacity refusal allocated {bytes} bytes after warmup; the input coefficient/text was already owned.");return bytes;
    }
    /// <summary>Refusing a pre-owned 100001-digit coefficient avoids full coefficient formatting.</summary>
    [Fact] public void ConstructorRefusalDoesNotFormatLargeCoefficient()
    {
        var coefficient=BigInteger.Pow(10,100000);var negative=-coefficient;
        Assert.InRange(RefusalBytes(()=>_ = new XdmExactDecimal(coefficient,0,32,0)),0,16384);
        Assert.InRange(RefusalBytes(()=>_ = new XdmExactDecimal(negative,0,32,0)),0,16384);
    }
    /// <summary>Output refusal does not allocate a huge digit string or zero padding.</summary>
    [Fact] public void FormattingRefusalPrecedesLargeAllocation()
    {
        var value=new XdmExactDecimal(BigInteger.Pow(10,100000),0,100001,0);
        Assert.InRange(RefusalBytes(()=>value.ToInvariantString(32)),0,16384);
        var tiny=new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue);
        Assert.InRange(RefusalBytes(()=>tiny.ToInvariantString(32)),0,16384);
    }
    /// <summary>Lexical quotas refuse before coefficient allocation, including uncertain malformed tails.</summary>
    [Fact] public void LexicalRefusalPrecedesCoefficientAllocation()
    {
        var large=new string('9',200000)+"x";
        Assert.InRange(RefusalBytes(()=>ExactDecimalLexical.Parse(large,32,32)),0,16384);
        var fractional="0."+new string('0',200000);
        Assert.InRange(RefusalBytes(()=>ExactDecimalLexical.Parse(fractional,int.MaxValue,32)),0,16384);
    }
}
