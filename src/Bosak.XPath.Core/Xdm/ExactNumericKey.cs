// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Represents exact numeric map keys without exponent-expanded strings.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Numeric same-key data with no scale-expanded strings or powers of ten.</summary>
internal readonly record struct ExactNumericKey(BigInteger Coefficient,int Scale,byte Special=0)
{
    internal static ExactNumericKey FromValue(XdmValue value)=>value.Kind switch
    {
        XdmValueKind.Integer=>new(value.BigIntegerValue,0),
        XdmValueKind.Decimal=>Decimal(value),
        XdmValueKind.Double or XdmValueKind.Float=>Binary(value.DoubleValue),
        _=>throw new ArgumentException("A numeric key is required.",nameof(value))
    };
    private static ExactNumericKey Decimal(XdmValue value)
    {var (coefficient,scale)=value.DecimalRepresentation;return Normalize(coefficient,scale);}
    private static ExactNumericKey Normalize(BigInteger coefficient,int scale)
    {
        if(coefficient.IsZero)return default;
        while(scale>=9 && (coefficient%1_000_000_000).IsZero){coefficient/=1_000_000_000;scale-=9;}
        while(scale>0 && (coefficient%10).IsZero){coefficient/=10;scale--;}
        return new(coefficient,scale);
    }
    private static ExactNumericKey Binary(double value)
    {
        if(double.IsNaN(value))return new(0,0,1);
        if(double.IsPositiveInfinity(value))return new(0,0,2);
        if(double.IsNegativeInfinity(value))return new(0,0,3);
        long bits=BitConverter.DoubleToInt64Bits(value);int exponent=(int)((bits>>52)&0x7ff);
        long fraction=bits&0xfffffffffffff;
        BigInteger mantissa=exponent==0?fraction:fraction|(1L<<52);
        int power=exponent==0?-1074:exponent-1075;
        BigInteger coefficient;int scale;
        if(power>=0){coefficient=mantissa<<power;scale=0;}
        else{scale=-power;coefficient=mantissa*BigInteger.Pow(5,scale);}
        return Normalize(bits<0?-coefficient:coefficient,scale);
    }
}
