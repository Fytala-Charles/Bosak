// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 september 2026
// PURPOSE              : Snapshot of a node's is-id / is-idref properties surviving annotation stripping
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-09-2026     | Creation (REQ-109)                                                                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Snapshot of a node's is-id / is-idref properties, taken by
/// <see cref="XdmSchemaAnnotator"/> before the PSVI (<see cref="System.Xml.Schema.IXmlSchemaInfo"/>)
/// annotations are stripped (XSLT <c>validation="strip"</c> and
/// <c>input-type-annotations="strip"</c>). .NET's <see cref="System.Xml.Schema.IXmlSchemaInfo"/>
/// is the only carrier of those properties, so stripping would otherwise destroy them;
/// XSLT 3.0 §3.13 requires the is-id and is-idref properties to survive stripping.
/// </summary>
internal sealed class XdmIdProperties
{
    /// <summary>Initializes the snapshot.</summary>
    /// <param name="isId">The node's is-id property.</param>
    /// <param name="isIdref">The node's is-idref property.</param>
    public XdmIdProperties(bool isId, bool isIdref)
    {
        IsId = isId;
        IsIdref = isIdref;
    }

    /// <summary>Gets the node's is-id property at validation time.</summary>
    public bool IsId { get; }

    /// <summary>Gets the node's is-idref property at validation time.</summary>
    public bool IsIdref { get; }
}
