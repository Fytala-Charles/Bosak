// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Thrown when an XPath expression cannot be parsed due to syntactic errors
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 05-06-2026     | Auto-prefix generic messages with XPST0003 when no error code is present                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Parser;

/// <summary>
/// Thrown when an XPath expression cannot be parsed due to syntactic errors.
/// </summary>
public sealed class ParseException : Exception
{
    public int Position { get; }

    public ParseException(string message, int position)
        : base(FormatMessage(message, position))
    {
        Position = position;
    }

    public ParseException(string message, int position, Exception inner)
        : base(FormatMessage(message, position), inner)
    {
        Position = position;
    }

    private static string FormatMessage(string message, int position)
    {
        if (!message.StartsWith("XPST") && !message.StartsWith("XQST") && !message.StartsWith("XPTY"))
            message = $"XPST0003: {message}";
        return $"Parse error at position {position}: {message}";
    }
}
