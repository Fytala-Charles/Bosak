// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : Options for XdmSchemaAnnotator subtree/attribute validation (mode, named type, document level)
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

using System.Xml;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Options controlling <see cref="XdmSchemaAnnotator.Validate(XElement, System.Xml.Schema.XmlSchemaSet, XdmValidationOptions, System.Xml.Schema.ValidationEventHandler?, bool)"/>
/// and the corresponding attribute-level entry point.
/// </summary>
/// <param name="Mode">The validation action to perform.</param>
/// <param name="TypeName">
/// When non-null, the node is validated against this named schema type (the XSLT
/// <c>[xsl:]type</c> attribute semantics) instead of against a matching top-level
/// declaration; <see cref="Mode"/> must then be <see cref="XdmValidationMode.Strict"/> or
/// <see cref="XdmValidationMode.Lax"/> (it is ignored apart from
/// <see cref="XdmValidationMode.Strip"/>/<see cref="XdmValidationMode.Preserve"/>, which
/// short-circuit before any validation). The type must exist in the supplied schema set or
/// be a built-in XML Schema type.
/// </param>
/// <param name="DocumentLevel">
/// When <c>true</c>, the supplied <see cref="XElement"/> acts as the content container of a
/// document node: it must contain exactly one element child and no text node children, and
/// the validation applies to that single child element (XSLT 3.0 §25.4.2).
/// </param>
public sealed record XdmValidationOptions(XdmValidationMode Mode, XmlQualifiedName? TypeName = null, bool DocumentLevel = false);
