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
//                      | Charles Korthout | 0.2   | 23-09-2026     | REQ-099 seam H4: FailureMessage added for failures without validator events              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Schema;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Represents the outcome of validating an in-memory subtree against an XML Schema set
/// (see <see cref="XdmSchemaAnnotator.ValidateSubtree"/> and
/// <see cref="XdmSchemaAnnotator.Validate(XElement, XmlSchemaSet, XdmValidationOptions, ValidationEventHandler?, bool)"/>).
/// Regardless of the outcome, PSVI (<see cref="IXmlSchemaInfo"/>) annotations are attached to
/// the live <see cref="System.Xml.Linq.XObject"/>s, so every typed-value surface of the
/// engine reflects the schema immediately after the call returns.
/// </summary>
public sealed record XdmSubtreeValidationResult
{
    internal XdmSubtreeValidationResult(bool isValid, IReadOnlyList<ValidationEventArgs> errors)
        : this(isValid, errors, errors.Count > 0 ? errors[0].Message : null)
    {
    }

    internal XdmSubtreeValidationResult(bool isValid, IReadOnlyList<ValidationEventArgs> errors, string? failureMessage)
    {
        IsValid = isValid;
        Errors = errors;
        FailureMessage = failureMessage;
    }

    /// <summary>
    /// <c>true</c> when validation produced no errors; <c>false</c> otherwise.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// The validation errors (severity <see cref="XmlSeverityType.Error"/>) reported during
    /// validation, in document order. Empty when <see cref="IsValid"/> is <c>true</c>, and
    /// also empty for failures detected without running the validator (for example a
    /// document-shape violation or a named simple-type value check); inspect
    /// <see cref="FailureMessage"/> for those.
    /// </summary>
    public IReadOnlyList<ValidationEventArgs> Errors { get; }

    /// <summary>
    /// A human-readable description of the first failure, or <c>null</c> when
    /// <see cref="IsValid"/> is <c>true</c>. For validator-reported failures this is the
    /// message of the first entry in <see cref="Errors"/>.
    /// </summary>
    public string? FailureMessage { get; }
}
