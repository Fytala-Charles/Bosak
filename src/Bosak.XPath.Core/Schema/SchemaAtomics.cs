// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Observes cached atomic type evidence without granting authority or a lease.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Core.Schema;

/// <summary>Observes engine-admitted cached atomic type evidence without acquiring authority or a lease.</summary>
public static class SchemaAtomics
{
    /// <summary>Observes cached declared/actual type evidence and optional original assessment.</summary>
    /// <param name="value">An atomic carrier issued by a supported engine route.</param>
    /// <param name="provenance">A borrowed engine-guarded metadata view, or null on failure.</param>
    /// <returns>True for a matching live ordinary registry entry or live operation-local constructed receipt. False for plain, expired, malformed, private legacy or current-compatible carriers.</returns>
    /// <remarks>No provider callbacks or leases are acquired. This does not authorize typed execution or prove current provider ownership/compatibility. View getters refuse after engine expiry. Provider handles returned by the view remain borrowed and require explicit owner/raw lifetimes for their own operations. Container/list boundaries and provenance of flattened plain values are not reconstructed.</remarks>
    public static bool TryGetProvenance(in XdmValue value,out ISchemaAtomicProvenance? provenance)
    {
        provenance=null;
        if(!value.IsAtomic||XdmValue.SchemaTransportMarker(value) is not ISchemaAtomicObservation observation)return false;
        if(!observation.TryObserve(in value,out var observed)||observed is null)return false;
        provenance=observed;return true;
    }
}

/// <summary>Trusted engine-only marker observation; never a provider registration SPI.</summary>
internal interface ISchemaAtomicObservation
{
    bool TryObserve(in XdmValue value,out ISchemaAtomicProvenance? provenance);
}
