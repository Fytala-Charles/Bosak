// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Converts exact decimal carriers to and from CLR decimal without rounding.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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
//                      | Charles Korthout | 0.2   | 04-11-2026     | Expose raw CLR decimal parts for pre-normalization admission quotas (1.48.0).            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Buffers.Binary;
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Converts exact and CLR decimal storage without formatted parsing or rounding.</summary>
internal static class ExactNumericTransport
{
    internal static decimal ToLegacyDecimal(XdmExactDecimal value)
    {
        var magnitude = BigInteger.Abs(value.Coefficient);
        if (value.Scale > 28 || magnitude.GetBitLength() > 96)
            throw new OverflowException("The exact decimal cannot be represented by CLR decimal without rounding.");
        Span<byte> bytes = stackalloc byte[12]; bytes.Clear();
        if (!magnitude.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: false))
            throw new OverflowException("The exact decimal coefficient exceeds CLR decimal capacity.");
        return new decimal(BinaryPrimitives.ReadInt32LittleEndian(bytes), BinaryPrimitives.ReadInt32LittleEndian(bytes[4..]),
            BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]), value.Coefficient.Sign < 0, (byte)value.Scale);
    }
    internal static XdmExactDecimal FromLegacyDecimal(decimal value)
    {
        var (coefficient, scale) = LegacyParts(value);
        return new XdmExactDecimal(coefficient, scale, 29, 28);
    }
    internal static (BigInteger Coefficient, int Scale) LegacyParts(decimal value)
    {
        Span<int> bits = stackalloc int[4]; decimal.GetBits(value, bits);
        Span<byte> bytes = stackalloc byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, bits[0]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], bits[1]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..], bits[2]);
        var coefficient = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        if (bits[3] < 0) coefficient = -coefficient;
        // These bounds describe CLR decimal storage, not a selected query workload.
        return (coefficient, (bits[3] >> 16) & 255);
    }
}
