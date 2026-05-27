// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Abstraction for XML Schema (XSD) validation.
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
/// Abstraction for XML Schema (XSD) validation.
/// </summary>
public interface IXsdValidator
{
    /// <summary>
    /// Validates an XML document against a single XSD schema.
    /// </summary>
    XsdValidationResult Validate(string xml, Stream xsdStream, XsdValidatorOptions? options = null);

    /// <summary>
    /// Validates an XML document against a set of XSD schemas (handles imports/includes).
    /// </summary>
    XsdValidationResult Validate(string xml, IEnumerable<Stream> xsdStreams, XsdValidatorOptions? options = null);

    /// <summary>
    /// Validates an XML document against a single XSD schema.
    /// Non-throwing: returns a result with any errors rather than throwing.
    /// </summary>
    XsdValidationResult TryValidate(string xml, Stream xsdStream, XsdValidatorOptions? options = null);

    /// <summary>
    /// Validates an XML document against a set of XSD schemas.
    /// Non-throwing: returns a result with any errors rather than throwing.
    /// </summary>
    XsdValidationResult TryValidate(string xml, IEnumerable<Stream> xsdStreams, XsdValidatorOptions? options = null);
}
