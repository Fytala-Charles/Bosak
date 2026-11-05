// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Checks numeric coefficient quotas before decimal formatting or normalization.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Checks decimal coefficient capacity without formatting a refused large input.</summary>
internal static class ExactNumericCapacity
{
    // For a nonnegative n with b bits: n < 2^b. Since 8 < 10 < 16,
    // b <= 3*d proves n < 10^d, while b > 4*d proves n >= 10^d.
    // Only the uncertain interval needs a threshold, bounded by the admitted digit capacity.
    internal static bool FitsCoefficient(BigInteger value, int maxDigits)
    {
        var magnitude = BigInteger.Abs(value);
        long bits = magnitude.GetBitLength();
        if (bits <= 3L * maxDigits) return true;
        if (bits > 4L * maxDigits) return false;
        return magnitude < BigInteger.Pow(10, maxDigits);
    }
}
