// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Options for controlling XSD validation behavior.
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
/// Options for controlling XSD validation behavior.
/// </summary>
public sealed class XsdValidatorOptions
{
    /// <summary>
    /// Gets or sets whether to treat warnings as errors.
    /// Default is <c>false</c>.
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of errors to collect before stopping.
    /// A value of 0 means unlimited. Default is 0.
    /// </summary>
    public int MaxErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the base URI used to resolve relative schema locations.
    /// </summary>
    public string? BaseUri { get; set; }

    /// <summary>Default options (unlimited errors, warnings not treated as errors).</summary>
    public static XsdValidatorOptions Default => new();
}
