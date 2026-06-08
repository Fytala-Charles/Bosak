// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 June 2026
// PURPOSE              : Holds the current content of all open text documents for the language server.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Concurrent;

namespace Bosak.LanguageServer;

/// <summary>
/// Holds the current content of all open text documents.
/// </summary>
public sealed class DocumentManager
{
    private readonly ConcurrentDictionary<string, string> _documents = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Updates or inserts the content for the given document URI.
    /// </summary>
    /// <param name="uri">The document URI.</param>
    /// <param name="text">The full document text.</param>
    public void Update(string uri, string text)
    {
        _documents[uri] = text;
    }

    /// <summary>
    /// Attempts to retrieve the content for the given document URI.
    /// </summary>
    /// <param name="uri">The document URI.</param>
    /// <param name="text">When this method returns, contains the document text if found.</param>
    /// <returns><c>true</c> if the document was found; otherwise <c>false</c>.</returns>
    public bool TryGet(string uri, out string text)
    {
        return _documents.TryGetValue(uri, out text!);
    }

    /// <summary>
    /// Removes the document with the specified URI.
    /// </summary>
    /// <param name="uri">The document URI.</param>
    public void Remove(string uri)
    {
        _documents.TryRemove(uri, out _);
    }
}
