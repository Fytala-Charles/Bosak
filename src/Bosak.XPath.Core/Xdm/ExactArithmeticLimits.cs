// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Enforces supplied and intermediate capacity for exact integer/decimal arithmetic.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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
//                      | Charles Korthout | 0.2   | 04-11-2026     | Compare decimal order before bounded alignment and derive scalar output capacity (1.49.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Exact integer/decimal operation data; normalized values retain their primitive numeric kind.</summary>
internal readonly record struct ExactNumber(BigInteger Coefficient, int Scale, bool Integer);

/// <summary>Preflights supplied and intermediate numeric capacities before arithmetic allocation.</summary>
internal readonly struct ExactArithmeticLimits
{
    private readonly int _digits, _scale;
    internal ExactArithmeticLimits(int digits, int scale)
    { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits); ArgumentOutOfRangeException.ThrowIfNegative(scale); _digits=digits; _scale=scale; }
    internal ExactNumber Read(XdmValue value)
    {
        if (XdmValue.SchemaTransportMarker(value) is not null || value.UserSchemaTypeName is not null)
            throw new NotSupportedException("The arithmetic profile requires plain primitive numeric values.");
        if (value.Kind==XdmValueKind.Integer && value.SchemaTypeName is null or "integer")
        { var n=value.BigIntegerValue; Check(n,0); return new(n,0,true); }
        if (value.Kind==XdmValueKind.Decimal && value.SchemaTypeName is null or "decimal")
        {
            var (coefficient,scale)=value.DecimalRepresentation;
            Check(coefficient,scale); var normalized=new XdmExactDecimal(coefficient,scale,_digits,_scale);
            return new(normalized.Coefficient,normalized.Scale,false);
        }
        throw new NotSupportedException("Only primitive integer and decimal inputs are admitted by this arithmetic profile.");
    }
    private void Check(BigInteger coefficient,int scale)
    {
        if (_digits<=0)throw new InvalidOperationException("Explicit numeric limits are required.");
        if (scale<0)throw new ArgumentOutOfRangeException(nameof(scale));
        if (scale>_scale || !ExactNumericCapacity.FitsCoefficient(coefficient,_digits))
            throw new NotSupportedException("Numeric coefficient digits or supplied scale exceed operation capacity.");
    }
    private void Validate(ExactNumber value)
    { Check(value.Coefficient,value.Scale);if(value.Integer&&value.Scale!=0)throw new ArgumentException("An integer cannot have fractional scale."); }
    internal XdmValue ToValue(ExactNumber value)
    { Validate(value);return value.Integer ? XdmValue.FromInteger(value.Coefficient) : XdmValue.FromDecimal(new XdmExactDecimal(value.Coefficient,value.Scale,_digits,_scale)); }
    internal ExactNumber Negate(ExactNumber value) { Validate(value);return value with { Coefficient = -value.Coefficient }; }
    private BigInteger Align(ExactNumber value,int scale)
    {
        int delta=scale-value.Scale;
        if (delta==0 || value.Coefficient.IsZero) return value.Coefficient;
        if (delta>=_digits || !ExactNumericCapacity.FitsCoefficient(value.Coefficient,_digits-delta))
            throw new NotSupportedException("Scale alignment exceeds numeric digit capacity.");
        return value.Coefficient*BigInteger.Pow(10,delta);
    }
    private int Digits(BigInteger coefficient,Action check)
    {
        var magnitude=BigInteger.Abs(coefficient);if(magnitude.IsZero)return 1;
        // 0.301029995 is a strict lower bound on log10(2). UInt128 avoids bit-count multiplication overflow.
        int digits=checked((int)(((UInt128)(magnitude.GetBitLength()-1)*301_029_995)/1_000_000_000+1));
        check();var threshold=BigInteger.Pow(10,digits);
        while(magnitude>=threshold){check();threshold*=10;digits++;}
        return digits;
    }
    internal int Compare(ExactNumber left,ExactNumber right,Action check)
    {
        check();Validate(left);Validate(right);
        int sign=left.Coefficient.Sign.CompareTo(right.Coefficient.Sign);if(sign!=0)return sign;
        if(left.Coefficient.IsZero)return 0;
        if(left.Scale==right.Scale)return left.Coefficient.CompareTo(right.Coefficient);
        long leftOrder=(long)Digits(left.Coefficient,check)-left.Scale;
        long rightOrder=(long)Digits(right.Coefficient,check)-right.Scale;
        check();int order=leftOrder.CompareTo(rightOrder);
        if(order!=0)return left.Coefficient.Sign*order;
        // Equal decimal order bounds necessary alignment by the admitted coefficient width.
        int scale=Math.Max(left.Scale,right.Scale);var a=Align(left,scale);check();var b=Align(right,scale);check();return a.CompareTo(b);
    }
    internal int StringCapacity
        =>(int)Math.Min(int.MaxValue,Math.Max(5L,Math.Max((long)_digits+2,(long)_scale+3)));
    internal string Format(ExactNumber value,Action check)
    {
        check();Validate(value);var result=new XdmExactDecimal(value.Coefficient,value.Scale,_digits,_scale).ToInvariantString(StringCapacity);check();return result;
    }
    internal ExactNumber Add(ExactNumber left,ExactNumber right,Action check)
    {
        check();Validate(left);Validate(right);int scale=Math.Max(left.Scale,right.Scale); var a=Align(left,scale); check(); var b=Align(right,scale); check();
        if (a.Sign==b.Sign && a.Sign!=0 && Math.Max(BigInteger.Abs(a).GetBitLength(),BigInteger.Abs(b).GetBitLength())+1>3L*_digits)
        {
            var maximum=BigInteger.Pow(10,_digits)-1;
            if (BigInteger.Abs(a)>maximum-BigInteger.Abs(b)) throw new NotSupportedException("Numeric addition exceeds digit capacity.");
        }
        check(); var sum=a+b; check();return Normalize(sum,scale,left.Integer&&right.Integer);
    }
    internal ExactNumber Multiply(ExactNumber left,ExactNumber right,Action check)
    {
        check();Validate(left);Validate(right);long scale=(long)left.Scale+right.Scale;
        if (scale>_scale) throw new NotSupportedException("Numeric multiplication exceeds supplied scale capacity.");
        var a=BigInteger.Abs(left.Coefficient);var b=BigInteger.Abs(right.Coefficient);
        if (!a.IsZero && !b.IsZero && a.GetBitLength()+b.GetBitLength()>3L*_digits)
        {
            var maximum=BigInteger.Pow(10,_digits)-1;
            if (a>maximum/b) throw new NotSupportedException("Numeric multiplication exceeds digit capacity.");
        }
        check();var product=left.Coefficient*right.Coefficient;check();return Normalize(product,(int)scale,left.Integer&&right.Integer);
    }
    private ExactNumber Normalize(BigInteger coefficient,int scale,bool integer)
    {
        Check(coefficient,scale);
        if (integer)return new(coefficient,0,true);
        var value=new XdmExactDecimal(coefficient,scale,_digits,_scale);return new(value.Coefficient,value.Scale,false);
    }
}
