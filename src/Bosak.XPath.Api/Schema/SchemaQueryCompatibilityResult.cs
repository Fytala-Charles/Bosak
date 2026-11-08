// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Describes the three-decision result of an engine-authenticated compatible admission.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Schema;
namespace Bosak.XPath.Api.Schema;

/// <summary>Immutable metadata describing compatible admission; dispose a successful Snapshot separately.</summary>
public sealed class SchemaQueryCompatibilityResult
{
    internal SchemaQueryCompatibilityResult(SchemaCompatibilityDecision decision,SchemaQuerySnapshot? snapshot)
    {Decision=decision;Snapshot=snapshot;}
    /// <summary>The exact compatible, incompatible or unsupported decision.</summary>
    public SchemaCompatibilityDecision Decision { get; }
    /// <summary>The new target-session admission for Compatible; otherwise null.</summary>
    public SchemaQuerySnapshot? Snapshot { get; }
    /// <summary>A bounded engine diagnostic for negative decisions; null for Compatible.</summary>
    public string? DiagnosticCode=>Decision switch
    {SchemaCompatibilityDecision.Compatible=>null,SchemaCompatibilityDecision.Incompatible=>"BOSAK-COMPATIBILITY-INCOMPATIBLE",_=>"BOSAK-COMPATIBILITY-UNSUPPORTED"};
}
