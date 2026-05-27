// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Represents the result of an XSD validation operation.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Api.Xsd;

/// <summary>
/// Represents the result of an XSD validation operation.
/// </summary>
public sealed class XsdValidationResult
{
    /// <summary>Gets all validation messages (errors and warnings).</summary>
    public IReadOnlyList<XsdValidationError> Errors { get; }

    /// <summary>Gets a value indicating whether the document is valid (no errors).</summary>
    public bool IsValid => Errors.Count == 0 || !Errors.Any(e => e.Severity == XsdValidationSeverity.Error);

    /// <summary>Gets only the errors (excluding warnings).</summary>
    public IEnumerable<XsdValidationError> OnlyErrors => Errors.Where(e => e.Severity == XsdValidationSeverity.Error);

    /// <summary>Gets only the warnings.</summary>
    public IEnumerable<XsdValidationError> OnlyWarnings => Errors.Where(e => e.Severity == XsdValidationSeverity.Warning);

    public XsdValidationResult(IReadOnlyList<XsdValidationError> errors)
    {
        Errors = errors;
    }

    /// <summary>Creates a result representing a successful validation.</summary>
    public static XsdValidationResult Success() => new(Array.Empty<XsdValidationError>());
}
