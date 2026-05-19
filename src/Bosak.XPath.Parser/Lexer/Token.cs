// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A single token from the XPath lexer. Stores offset and length into the source text rather than al...
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
namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// A single token from the XPath lexer. Stores offset and length into the
/// source text rather than allocating a substring.
/// </summary>
public readonly struct Token
{
    public static readonly Token Eof = new(TokenKind.Eof, -1, 0);
    public static readonly Token Invalid = new(TokenKind.Invalid, -1, 0);

    public readonly TokenKind Kind;
    public readonly int Start;
    public readonly int Length;

    public Token(TokenKind kind, int start, int length)
    {
        Kind = kind;
        Start = start;
        Length = length;
    }

    /// <summary>
    /// Returns the text of this token from the original source span.
    /// </summary>
    public ReadOnlySpan<char> Text(ReadOnlySpan<char> source)
        => Length > 0 ? source.Slice(Start, Length) : ReadOnlySpan<char>.Empty;

    public override string ToString()
        => $"{Kind}@{Start}[{Length}]";
}
