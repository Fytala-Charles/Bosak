// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Thrown when an XPath expression cannot be parsed due to syntactic errors
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 05-06-2026     | Auto-prefix generic messages with XPST0003 when no error code is present                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Parser;

/// <summary>
/// Thrown when an XPath expression cannot be parsed due to syntactic errors.
/// </summary>
public sealed class ParseException : Exception
{
    /// <summary>The zero-based character offset in the source text where the error was detected.</summary>
    public int Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// </summary>
    /// <param name="message">The error description; an XPST0003 prefix is added when the
    /// message does not already start with an XPST, XQST, or XPTY error code.</param>
    /// <param name="position">The zero-based character offset in the source text where the error was detected.</param>
    public ParseException(string message, int position)
        : base(FormatMessage(message, position))
    {
        Position = position;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error description; an XPST0003 prefix is added when the
    /// message does not already start with an XPST, XQST, or XPTY error code.</param>
    /// <param name="position">The zero-based character offset in the source text where the error was detected.</param>
    /// <param name="inner">The exception that caused this parse error.</param>
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
