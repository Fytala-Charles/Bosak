// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies exact numeric XDM storage, checked narrowing and evidence-marker safety.
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
using System.Runtime.CompilerServices;
using Bosak.XPath.Core.Xdm;
using Xunit;
using Xunit.Abstractions;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks exact numeric storage, checked legacy views and schema-marker disambiguation.</summary>
public sealed class ExactNumericValueTests
{
    private readonly ITestOutputHelper _output;
    private static XdmValue _sink;
    private static BigInteger _integerSink;
    private static XdmExactDecimal _decimalSink;
    private static decimal _legacySink;
    /// <summary>Retains measured allocation observations in the test receipt.</summary>
    /// <param name="output">The test receipt writer.</param>
    public ExactNumericValueTests(ITestOutputHelper output) => _output=output;
    private static BigInteger Number(string text)=>BigInteger.Parse(text,CultureInfo.InvariantCulture);
    private static XdmExactDecimal Exact(string coefficient,int scale=0)=>new(Number(coefficient),scale,256,int.MaxValue);
    /// <summary>Exact integer factories retain every bit, including values beyond both Int64 endpoints.</summary>
    /// <param name="text">An exact integer representation.</param>
    [Theory]
    [InlineData("0")][InlineData("42")][InlineData("-42")]
    [InlineData("9223372036854775807")][InlineData("-9223372036854775808")]
    [InlineData("9223372036854775808")][InlineData("-9223372036854775809")]
    [InlineData("12345678901234567890123456789012345678901234567890")]
    public void IntegerStorageAndCheckedLegacyView(string text)
    {
        var number=Number(text);var value=XdmValue.FromInteger(number);Assert.Equal(XdmValueKind.Integer,value.Kind);Assert.Equal(number,value.BigIntegerValue);
        Assert.Null(XdmValue.SchemaTransportMarker(value));Assert.Equal(!number.IsZero,value.GetEffectiveBooleanValue());
        if(number>=long.MinValue&&number<=long.MaxValue){Assert.Equal((long)number,value.IntegerValue);Assert.Equal(XdmValue.FromInteger((long)number),value);Assert.Equal(((long)number).ToString(),value.ToString());}
        else{Assert.Throws<OverflowException>(()=>value.IntegerValue);Assert.Throws<OverflowException>(()=>value.ToString());}
    }
    /// <summary>Exact decimal storage and EBV do not use narrowing or output padding.</summary>
    /// <param name="coefficient">The exact coefficient.</param>
    /// <param name="scale">The exact scale.</param>
    [Theory]
    [InlineData("0",0)][InlineData("1234500",4)][InlineData("-1234500",4)]
    [InlineData("123456789012345678901234567890123456789",5)]
    [InlineData("1",1000)][InlineData("-1",2147483647)]
    public void DecimalStorageDoesNotNarrow(string coefficient,int scale)
    {
        var exact=Exact(coefficient,scale);var value=XdmValue.FromDecimal(exact);Assert.Equal(XdmValueKind.Decimal,value.Kind);Assert.Equal(exact,value.ExactDecimalValue);
        Assert.Equal(!exact.Coefficient.IsZero,value.GetEffectiveBooleanValue());Assert.Null(XdmValue.SchemaTransportMarker(value));
        if(exact.Scale>28||BigInteger.Abs(exact.Coefficient).GetBitLength()>96)
        {Assert.Throws<OverflowException>(()=>value.DecimalValue);Assert.Throws<OverflowException>(()=>value.ToString());}
        else Assert.Equal(exact.ToInvariantString(128),value.ToString());
    }
    /// <summary>Legacy decimals have an exact view, including both endpoints and the smallest quantum.</summary>
    [Fact] public void LegacyDecimalEndpointsRoundTripExactly()
    {
        foreach(var number in new[]{decimal.MinValue,decimal.MaxValue,0m,1m,-1m,1e-28m,1.2345678901234567890123456789m,123.4500m})
        {
            var legacy=XdmValue.FromDecimal(number);var exact=legacy.ExactDecimalValue;var fresh=XdmValue.FromDecimal(exact);
            Assert.Equal(number,fresh.DecimalValue);Assert.Equal(legacy.ToString(),fresh.ToString());Assert.Equal(exact,fresh.ExactDecimalValue);
        }
        var signedZero=new decimal(0,0,0,true,28);Assert.Equal(default(XdmExactDecimal),XdmValue.FromDecimal(signedZero).ExactDecimalValue);
    }
    /// <summary>Scale/range refusals reject exact values that a parser could otherwise round to decimal.</summary>
    [Fact] public void LegacyDecimalNarrowingNeverRounds()
    {
        foreach(var value in new[]{Exact("1",29),Exact("123456789012345678901234567891",1),new XdmExactDecimal(new BigInteger(decimal.MaxValue)+1,1,30,28)})
            Assert.Throws<OverflowException>(()=>XdmValue.FromDecimal(value).DecimalValue);
        Assert.Equal(1e-28m,XdmValue.FromDecimal(new XdmExactDecimal(10,29,2,29)).DecimalValue);
        Assert.Equal(decimal.MaxValue,XdmValue.FromDecimal(new XdmExactDecimal(new BigInteger(decimal.MaxValue),0,29,28)).DecimalValue);
        Assert.Equal(decimal.MinValue,XdmValue.FromDecimal(new XdmExactDecimal(new BigInteger(decimal.MinValue),0,29,28)).DecimalValue);
    }
    /// <summary>All four factory overloads preserve descriptive annotations without issuing authority.</summary>
    [Fact] public void FactoryAnnotationsAreDescriptiveOnly()
    {
        var i=XdmValue.FromInteger(Number("123456789012345678901234567890"),"integer","Q{urn:t}Whole");
        var d=XdmValue.FromDecimal(Exact("123456789012345678901234567890",3),"decimal","Q{urn:t}Fraction");
        Assert.Equal("integer",i.SchemaTypeName);Assert.Equal("Q{urn:t}Whole",i.UserSchemaTypeName);
        Assert.Equal("decimal",d.SchemaTypeName);Assert.Equal("Q{urn:t}Fraction",d.UserSchemaTypeName);
        Assert.Null(XdmValue.SchemaTransportMarker(i));Assert.Null(XdmValue.SchemaTransportMarker(d));
    }
    /// <summary>New exact accessors preserve existing kind checks and default behavior.</summary>
    [Fact] public void ExactAccessorsRejectWrongKinds()
    {
        Assert.Throws<InvalidOperationException>(()=>XdmValue.Undefined.BigIntegerValue);
        Assert.Throws<InvalidOperationException>(()=>XdmValue.Undefined.ExactDecimalValue);
        Assert.Throws<InvalidOperationException>(()=>XdmValue.FromDecimal(1m).BigIntegerValue);
        Assert.Throws<InvalidOperationException>(()=>XdmValue.FromInteger(1).ExactDecimalValue);
        Assert.Equal(BigInteger.Zero,XdmValue.FromInteger(BigInteger.Zero).BigIntegerValue);
        Assert.Equal(default,XdmValue.FromDecimal(default(XdmExactDecimal)).ExactDecimalValue);
    }
    private static XdmValue Carrier(int index)=>index switch
    {
        0=>XdmValue.FromInteger(Number("123456789012345678901234567890123456789"),"integer","Q{urn:t}Whole"),
        1=>XdmValue.FromDecimal(Exact("123456789012345678901234567890123456789",5),"decimal","Q{urn:t}Fraction"),
        2=>XdmValue.FromDecimal(Exact("1",1000),"decimal","Q{urn:t}Fraction"),
        _=>XdmValue.FromInteger(new BigInteger(42),"integer","Q{urn:t}Whole")
    };
    private static XdmValue Copy(XdmValue value)=>value.Kind==XdmValueKind.Integer
        ?XdmValue.FromInteger(value.BigIntegerValue,value.SchemaTypeName!,value.UserSchemaTypeName)
        :XdmValue.FromDecimal(value.ExactDecimalValue,value.SchemaTypeName!,value.UserSchemaTypeName);
    /// <summary>Marker attach, replacement and stripping preserve the exact payload and annotations.</summary>
    /// <param name="index">The numeric storage fixture.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void NumericPayloadCannotBeMistakenForMarker(int index)
    {
        var plain=Carrier(index);var marker=new object();var replacement=new object();var marked=XdmValue.SchemaTransportAttach(plain,marker);
        Assert.Null(XdmValue.SchemaTransportMarker(plain));Assert.Same(marker,XdmValue.SchemaTransportMarker(marked));Assert.True(plain.SchemaTransportMatches(marked));
        var replaced=XdmValue.SchemaTransportAttach(marked,replacement);Assert.Same(replacement,XdmValue.SchemaTransportMarker(replaced));
        var stripped=XdmValue.WithoutSchemaEvidence(replaced);Assert.Null(XdmValue.SchemaTransportMarker(stripped));Assert.Equal(plain,stripped);
        Assert.Equal(plain.SchemaTypeName,stripped.SchemaTypeName);Assert.Equal(plain.UserSchemaTypeName,stripped.UserSchemaTypeName);
        Assert.Equal(plain,marked);Assert.Equal(plain.GetHashCode(),marked.GetHashCode());Assert.Contains(plain,new HashSet<XdmValue>{marked});
    }
    /// <summary>Actual kernel admission authenticates markers, exact no-op conversions and source close.</summary>
    /// <param name="index">The numeric storage fixture.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void KernelRetainsExactPayloadAndRejectsRebinding(int index)
    {
        var issuer=new Issuer();var environment=new object();var kernel=new SchemaEvidenceKernel(environment,issuer,1,8);
        using var snapshot=kernel.Open(environment,new object(),issuer.Origin,()=>{},default);var plain=Carrier(index);
        var evidence=kernel.Read(snapshot,issuer.Issue(plain,true),default);var marked=kernel.AttachValue(evidence);
        Assert.Same(evidence,kernel.ReadValue(marked));Assert.Same(evidence,kernel.AdmitConversion(evidence,issuer.Issue(Copy(plain),false),default));
        XdmValue changed=plain.Kind==XdmValueKind.Integer
            ?XdmValue.FromInteger(plain.BigIntegerValue+1,plain.SchemaTypeName!,plain.UserSchemaTypeName)
            :XdmValue.FromDecimal(new XdmExactDecimal(plain.ExactDecimalValue.Coefficient+1,plain.ExactDecimalValue.Scale,256,int.MaxValue),plain.SchemaTypeName!,plain.UserSchemaTypeName);
        var rebound=XdmValue.SchemaTransportAttach(changed,XdmValue.SchemaTransportMarker(marked)!);
        Assert.Throws<ArgumentException>(()=>kernel.ReadValue(rebound));snapshot.Dispose();Assert.Throws<ObjectDisposedException>(()=>kernel.ReadValue(marked));kernel.Close();
    }
    /// <summary>Legacy map keys accept exact values in legacy range and refuse wider values before mutation.</summary>
    [Fact] public void LegacyMapKeysDoNotCollapseWideValues()
    {
        var map=new XdmMap();map.Add(XdmValue.FromInteger(1),XdmValue.True);
        Assert.True(XdmValueEqualityComparer.Instance.Equals(XdmValue.FromDecimal(Exact("100",2)),XdmValue.FromInteger(1)));
        Assert.Equal(XdmValueEqualityComparer.Instance.GetHashCode(XdmValue.FromDecimal(Exact("100",2))),XdmValueEqualityComparer.Instance.GetHashCode(XdmValue.FromInteger(1)));
        Assert.Throws<OverflowException>(()=>map.Add(Carrier(0),XdmValue.False));Assert.Equal(1,map.Count);
        Assert.Throws<OverflowException>(()=>map.Add(Carrier(2),XdmValue.False));Assert.Equal(1,map.Count);
    }
    /// <summary>Numeric payloads add no XdmValue instance fields in the qualified 64-bit process.</summary>
    [Fact] public void ValueCarrierSizeRemainsUnchanged()
    { Assert.Equal(8,IntPtr.Size);Assert.Equal(48,Unsafe.SizeOf<XdmValue>()); }
    private static long Bytes(Action action)
    {
        for(int i=0;i<500;i++)action();long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;i++)action();return(GC.GetAllocatedBytesForCurrentThread()-before)/10000;
    }
    /// <summary>Measures new storage/marker costs and protects legacy inline and cached exact read allocations.</summary>
    [Fact] public void AllocationCostsRetainInlineAndCachedReads()
    {
        var small=new BigInteger(42);var wide=Number("123456789012345678901234567890123456789");var exact=Exact("123456789012345678901234567890123456789",5);
        var integer=XdmValue.FromInteger(wide);var dec=XdmValue.FromDecimal(exact);var narrow=XdmValue.FromDecimal(Exact("12345",2));var marker=new object();
        var rows=new Dictionary<string,long>
        {
            ["legacy-inline-factory"]=Bytes(()=>_sink=XdmValue.FromInteger(42L)),
            ["exact-inline-factory"]=Bytes(()=>_sink=XdmValue.FromInteger(small)),
            ["wide-integer-factory"]=Bytes(()=>_sink=XdmValue.FromInteger(wide)),
            ["exact-decimal-factory"]=Bytes(()=>_sink=XdmValue.FromDecimal(exact)),
            ["mark-inline"]=Bytes(()=>_sink=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(42),marker)),
            ["mark-wide-integer"]=Bytes(()=>_sink=XdmValue.SchemaTransportAttach(integer,marker)),
            ["mark-exact-decimal"]=Bytes(()=>_sink=XdmValue.SchemaTransportAttach(dec,marker)),
            ["read-wide-integer"]=Bytes(()=>_integerSink=integer.BigIntegerValue),
            ["read-exact-decimal"]=Bytes(()=>_decimalSink=dec.ExactDecimalValue),
            ["read-checked-legacy-decimal"]=Bytes(()=>_legacySink=narrow.DecimalValue)
        };
        foreach(var row in rows)_output.WriteLine($"Numeric allocation {row.Key}: {row.Value} bytes/operation (10000 iterations; inputs pre-owned).");
        foreach(var name in new[]{"legacy-inline-factory","exact-inline-factory","mark-inline","read-wide-integer","read-exact-decimal","read-checked-legacy-decimal"})Assert.Equal(0,rows[name]);
        GC.KeepAlive(_sink);GC.KeepAlive(_integerSink);GC.KeepAlive(_decimalSink);GC.KeepAlive(_legacySink);
    }
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal object Origin { get; } = new();
        private object Type { get; } = new();
        private readonly HashSet<ISchemaIssuedScalar> _issued = new(ReferenceEqualityComparer.Instance);
        internal int GetterReads;
        internal ISchemaIssuedScalar Issue(XdmValue value, bool assessed)
        { var scalar = new Scalar(this, value, assessed ? Origin : null); _issued.Add(scalar); return scalar; }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) { return _issued.Contains(value); }
        bool ISchemaEvidenceIssuer.OwnsType(object type) => ReferenceEquals(type, Type);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        private sealed class Scalar(Issuer issuer, XdmValue value, object? origin) : ISchemaIssuedScalar
        {
            XdmValue ISchemaIssuedScalar.Value { get { issuer.GetterReads++; return value; } }
            object ISchemaIssuedScalar.DeclaredType { get { issuer.GetterReads++; return issuer.Type; } }
            object ISchemaIssuedScalar.ActualType { get { issuer.GetterReads++; return issuer.Type; } }
            object? ISchemaIssuedScalar.OriginalAssessment { get { issuer.GetterReads++; return origin; } }
        }
    }
}
