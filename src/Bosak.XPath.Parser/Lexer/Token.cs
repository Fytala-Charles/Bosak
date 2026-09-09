// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A single token from the XPath lexer. Stores offset and length into the source text rather than al...
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
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// A single token from the XPath lexer. Stores offset and length into the
/// source text rather than allocating a substring.
/// </summary>
public readonly struct Token
{
    /// <summary>The end-of-input token.</summary>
    public static readonly Token Eof = new(TokenKind.Eof, -1, 0);
    /// <summary>The invalid token, produced when no valid token could be scanned.</summary>
    public static readonly Token Invalid = new(TokenKind.Invalid, -1, 0);

    /// <summary>The kind of token that was scanned.</summary>
    public readonly TokenKind Kind;
    /// <summary>The zero-based character offset of the token in the source text.</summary>
    public readonly int Start;
    /// <summary>The length of the token in characters.</summary>
    public readonly int Length;

    /// <summary>
    /// Initializes a token with the given kind and source span.
    /// </summary>
    /// <param name="kind">The kind of token that was scanned.</param>
    /// <param name="start">The zero-based character offset of the token in the source text.</param>
    /// <param name="length">The length of the token in characters.</param>
    public Token(TokenKind kind, int start, int length)
    {
        Kind = kind;
        Start = start;
        Length = length;
    }

    /// <summary>
    /// Returns the text of this token from the original source span.
    /// </summary>
    /// <param name="source">The source text this token was scanned from.</param>
    /// <returns>The characters of the token, or an empty span when <see cref="Length"/> is 0.</returns>
    public ReadOnlySpan<char> Text(ReadOnlySpan<char> source)
        => Length > 0 ? source.Slice(Start, Length) : ReadOnlySpan<char>.Empty;

    /// <inheritdoc/>
    public override string ToString()
        => $"{Kind}@{Start}[{Length}]";
}
