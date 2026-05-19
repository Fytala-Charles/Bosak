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
        : base($"Parse error at position {position}: {message}")
    {
        Position = position;
    }

    public ParseException(string message, int position, Exception inner)
        : base($"Parse error at position {position}: {message}", inner)
    {
        Position = position;
    }
}
