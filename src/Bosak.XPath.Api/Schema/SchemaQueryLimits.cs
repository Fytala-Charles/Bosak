// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Carries explicit finite limits for the opt-in query profile.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; bounded public ordinary-source facade (1.45.0).                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Document numeric digit/scale enforcement in the exact arithmetic profile (1.48.0).       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;
/// <summary>Explicit finite admission limits; no owner workload defaults are selected.</summary>
/// <remarks>The ordinary string/boolean profile excludes numeric evaluation; the exact arithmetic profile enforces numeric digit/scale limits. Schema caches and typed enumeration remain unavailable.</remarks>
public readonly struct SchemaQueryLimits
{
    /// <summary>Specifies positive counts and nonnegative scale; the default struct is invalid.</summary>
    /// <param name="maxOpenSnapshots">Maximum live admitted snapshots per session.</param>
    /// <param name="maxCachedValuesPerSnapshot">Maximum issued scalar cache entries per snapshot.</param>
    /// <param name="maxTypedValueAtoms">Maximum atoms consumed from one typed-value result.</param>
    /// <param name="maxNumericDigits">Positive maximum exact coefficient digits admitted or created.</param>
    /// <param name="maxDecimalScale">Nonnegative maximum supplied decimal scale.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count or digit bound is not positive, or scale is negative.</exception>
    public SchemaQueryLimits(int maxOpenSnapshots, int maxCachedValuesPerSnapshot, int maxTypedValueAtoms, int maxNumericDigits, int maxDecimalScale)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxOpenSnapshots);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCachedValuesPerSnapshot);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTypedValueAtoms);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxNumericDigits);
        ArgumentOutOfRangeException.ThrowIfNegative(maxDecimalScale);
        MaxOpenSnapshots = maxOpenSnapshots; MaxCachedValuesPerSnapshot = maxCachedValuesPerSnapshot;
        MaxTypedValueAtoms = maxTypedValueAtoms; MaxNumericDigits = maxNumericDigits; MaxDecimalScale = maxDecimalScale;
    }
    /// <summary>Maximum live admitted snapshots.</summary>
    public int MaxOpenSnapshots { get; }
    /// <summary>Maximum issued scalar cache entries per snapshot.</summary>
    public int MaxCachedValuesPerSnapshot { get; }
    /// <summary>Maximum atoms consumed from one result, checked during consumption.</summary>
    public int MaxTypedValueAtoms { get; }
    /// <summary>Maximum coefficient digits checked before normalization and arithmetic allocation.</summary>
    public int MaxNumericDigits { get; }
    /// <summary>Maximum supplied decimal scale, including zero.</summary>
    public int MaxDecimalScale { get; }
    internal void Validate()
    {
        if (MaxOpenSnapshots <= 0 || MaxCachedValuesPerSnapshot <= 0 || MaxTypedValueAtoms <= 0 || MaxNumericDigits <= 0 || MaxDecimalScale < 0)
            throw new ArgumentException("Explicit positive query limits and nonnegative scale are required.");
    }
}
