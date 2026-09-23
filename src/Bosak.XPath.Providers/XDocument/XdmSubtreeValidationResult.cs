// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Represents the outcome of validating an in-memory subtree against an XML Schema set
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Schema;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Represents the outcome of validating an in-memory subtree against an XML Schema set
/// (see <see cref="XdmSchemaAnnotator.ValidateSubtree"/>). Regardless of the outcome,
/// PSVI (<see cref="IXmlSchemaInfo"/>) annotations are attached to the live
/// <see cref="System.Xml.Linq.XObject"/>s, so every typed-value surface of the engine
/// reflects the schema immediately after the call returns.
/// </summary>
public sealed record XdmSubtreeValidationResult
{
    internal XdmSubtreeValidationResult(bool isValid, IReadOnlyList<ValidationEventArgs> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// <c>true</c> when validation produced no errors; <c>false</c> otherwise.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// The validation errors (severity <see cref="XmlSeverityType.Error"/>) reported during
    /// validation, in document order. Empty when <see cref="IsValid"/> is <c>true</c>.
    /// </summary>
    public IReadOnlyList<ValidationEventArgs> Errors { get; }
}
