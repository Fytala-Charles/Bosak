// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Exception raised when a streaming (forward-only) source is misused
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// The exception thrown when an operation is attempted that a forward-only streaming
/// source cannot support — for example enumerating the streamed children of the root
/// a second time, or navigating across records that have already been released.
/// </summary>
public sealed class StreamingException : InvalidOperationException
{
    /// <summary>Initializes a new instance of the <see cref="StreamingException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public StreamingException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StreamingException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public StreamingException(string message, System.Exception innerException)
        : base(message, innerException)
    {
    }
}
