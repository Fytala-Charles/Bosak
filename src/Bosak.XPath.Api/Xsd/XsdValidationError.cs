// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Represents a single XSD validation error or warning.
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
/// Severity of an XSD validation message.
/// </summary>
public enum XsdValidationSeverity
{
    Warning,
    Error
}

/// <summary>
/// Represents a single XSD validation error or warning.
/// </summary>
public sealed class XsdValidationError
{
    /// <summary>Gets the severity of the validation message.</summary>
    public XsdValidationSeverity Severity { get; }

    /// <summary>Gets the human-readable validation message.</summary>
    public string Message { get; }

    /// <summary>Gets the line number in the XML document where the error occurred, or 0 if unknown.</summary>
    public int LineNumber { get; }

    /// <summary>Gets the column number in the XML document where the error occurred, or 0 if unknown.</summary>
    public int LinePosition { get; }

    /// <summary>Gets the source URI of the schema that reported the error, or empty if unknown.</summary>
    public string SourceUri { get; }

    public XsdValidationError(XsdValidationSeverity severity, string message, int lineNumber = 0, int linePosition = 0, string sourceUri = "")
    {
        Severity = severity;
        Message = message;
        LineNumber = lineNumber;
        LinePosition = linePosition;
        SourceUri = sourceUri;
    }

    public override string ToString()
        => $"[{Severity}] {Message} (at line {LineNumber}, column {LinePosition})";
}
