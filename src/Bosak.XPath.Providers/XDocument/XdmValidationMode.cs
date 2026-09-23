// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : The validation action applied when validating or copying an XDM subtree
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-09-2026     | Creation (REQ-099 seam H4)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// The validation action applied by <see cref="XdmSchemaAnnotator.Validate"/> and
/// <see cref="XdmSchemaAnnotator.ValidateAttribute"/>, mirroring the XSLT
/// <c>validation</c> attribute values (XSLT 3.0 §25.4).
/// </summary>
public enum XdmValidationMode
{
    /// <summary>
    /// Strict validation: a matching top-level element or attribute declaration must exist
    /// in the schema set and the content must be valid against it.
    /// </summary>
    Strict,

    /// <summary>
    /// Lax validation: nodes with a matching top-level declaration are validated; a root
    /// element without a global declaration (and without <c>xsi:type</c>) is treated as
    /// <c>xs:anyType</c> so that declared descendants are still validated.
    /// </summary>
    Lax,

    /// <summary>
    /// No validation is performed; all existing PSVI annotations in the subtree are removed,
    /// leaving every element <c>xs:untyped</c> and every attribute <c>xs:untypedAtomic</c>.
    /// </summary>
    Strip,

    /// <summary>
    /// No validation is performed; existing PSVI annotations in the subtree are kept unchanged.
    /// </summary>
    Preserve,
}
