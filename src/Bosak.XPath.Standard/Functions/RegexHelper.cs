// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 juni 2026
// PURPOSE              : Shared XSD regular-expression translation and validation used by fn:* and xsl:analyze-string.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-06-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 25-06-2026     | Added flags-aware ValidateAndTranslatePattern; $ to \z in non-multiline mode             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Helpers for XSD regular expressions: flag parsing, zero-length checks, replacement-string
/// translation, and conversion of XSD-specific syntax to .NET <see cref="Regex"/> syntax.
/// </summary>
public static class RegexHelper
{
    /// <summary>
    /// Parses the flags string used by <c>fn:matches</c>, <c>fn:replace</c>,
    /// <c>fn:tokenize</c>, <c>fn:analyze-string</c>, and <c>xsl:analyze-string</c>.
    /// </summary>
    public static RegexOptions ParseRegexFlags(string flags, out bool isQuoteMode)
    {
        var options = RegexOptions.None;
        isQuoteMode = false;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break;
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
                case 'q': isQuoteMode = true; break;
                default: throw new InvalidOperationException("FORX0001");
            }
        }
        return options;
    }

    /// <summary>
    /// Validates that <paramref name="pattern"/> conforms to the XSD regular-expression syntax
    /// and translates XSD-specific constructs (such as single-digit backreferences) into a form
    /// that .NET <see cref="Regex"/> understands.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown with code <c>FORX0002</c> when the
    /// pattern is invalid.</exception>
    public static string ValidateAndTranslatePattern(string pattern)
    {
        ValidateXsdRegex(pattern);
        return TranslateEndAnchor(TranslateBackreferences(pattern), multiline: false);
    }

    /// <summary>
    /// Validates and translates a pattern when the regex flags (and therefore the multiline
    /// mode) are known. In non-multiline mode, <c>$</c> is translated to <c>\z</c> so that it
    /// matches only the absolute end of the string, not the position before a final newline.
    /// </summary>
    public static string ValidateAndTranslatePattern(string pattern, RegexOptions options)
    {
        ValidateXsdRegex(pattern);
        return TranslateEndAnchor(TranslateBackreferences(pattern), (options & RegexOptions.Multiline) != 0);
    }

    /// <summary>
    /// Throws <c>FORX0003</c> if the pattern matches a zero-length string.
    /// </summary>
    public static void CheckZeroLengthMatch(string pattern, RegexOptions options)
    {
        if (Regex.IsMatch(string.Empty, pattern, options))
            throw new InvalidOperationException("FORX0003");
    }

    /// <summary>
    /// Translates an XPath/XSD replacement string to a .NET replacement string, escaping
    /// <c>$</c> and <c>\</c> according to the rules of <c>fn:replace</c>.
    /// <paramref name="groupCount"/> is the number of capturing groups in the pattern (excluding
    /// group 0, the whole match).
    /// </summary>
    public static string ValidateAndTranslateReplacement(string replacement, int groupCount)
    {
        var sb = new StringBuilder(replacement.Length);
        for (int i = 0; i < replacement.Length; i++)
        {
            char c = replacement[i];
            if (c == '$')
            {
                if (i + 1 >= replacement.Length)
                    throw new InvalidOperationException("FORX0004");

                char next = replacement[i + 1];
                if (next == '$')
                {
                    sb.Append("$$");
                    i++;
                }
                else if (char.IsDigit(next))
                {
                    int digitsStart = i + 1;
                    int digitsEnd = digitsStart;
                    while (digitsEnd < replacement.Length && char.IsDigit(replacement[digitsEnd])) digitsEnd++;
                    string digits = replacement[digitsStart..digitsEnd];

                    if (digits[0] == '0')
                    {
                        // $0 always refers to the whole match.
                        sb.Append("${0}");
                        if (digits.Length > 1)
                            sb.Append(digits[1..]);
                    }
                    else
                    {
                        int prefixLength = 0;
                        int prefixNumber = 0;
                        for (int k = 1; k <= digits.Length; k++)
                        {
                            int candidate = int.Parse(digits[0..k], System.Globalization.CultureInfo.InvariantCulture);
                            if (candidate <= groupCount)
                            {
                                prefixLength = k;
                                prefixNumber = candidate;
                            }
                        }

                        if (prefixLength == 0)
                        {
                            // No capturing group for any prefix; XPath treats the reference as empty.
                        }
                        else
                        {
                            sb.Append("${");
                            sb.Append(prefixNumber);
                            sb.Append('}');
                            if (prefixLength < digits.Length)
                                sb.Append(digits[prefixLength..]);
                        }
                    }
                    i = digitsEnd - 1;
                }
                else
                {
                    throw new InvalidOperationException("FORX0004");
                }
            }
            else if (c == '\\')
            {
                if (i + 1 >= replacement.Length)
                    throw new InvalidOperationException("FORX0004");

                char next = replacement[i + 1];
                if (next == '\\')
                {
                    // XPath \\ is a single literal backslash; in .NET replacement a backslash is literal.
                    sb.Append('\\');
                    i++;
                }
                else if (next == '$')
                {
                    sb.Append("$$");
                    i++;
                }
                else
                {
                    throw new InvalidOperationException("FORX0004");
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// When the <c>q</c> flag is used, the replacement string is literal. This method escapes
    /// <c>$</c> for .NET <see cref="Regex"/> replacement strings while leaving backslashes unchanged.
    /// </summary>
    public static string EscapeReplacementForQuoteMode(string replacement)
    {
        var sb = new StringBuilder(replacement.Length);
        foreach (char c in replacement)
        {
            if (c == '$')
                sb.Append("$$");
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static void ValidateXsdRegex(string pattern)
    {
        bool escaped = false;
        bool inCharClass = false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (escaped)
            {
                escaped = false;
                // Multi-character escapes such as \p{Lu} contain braces that are not quantifiers.
                if ((c == 'p' || c == 'P') && i + 1 < pattern.Length && pattern[i + 1] == '{')
                {
                    int close = pattern.IndexOf('}', i + 1);
                    if (close < 0)
                        throw new InvalidOperationException("FORX0002");
                    i = close;
                }
                continue;
            }
            if (c == '\\')
            {
                escaped = true;
                continue;
            }
            if (inCharClass)
            {
                if (c == ']') inCharClass = false;
                continue;
            }
            if (c == '[')
            {
                inCharClass = true;
                continue;
            }
            if (c == '{')
            {
                // A valid quantifier is {n}, {n,} or {n,m}.
                int j = i + 1;
                while (j < pattern.Length && char.IsDigit(pattern[j])) j++;
                if (j > i + 1)
                {
                    if (j < pattern.Length && pattern[j] == ',')
                    {
                        j++;
                        while (j < pattern.Length && char.IsDigit(pattern[j])) j++;
                    }
                    if (j < pattern.Length && pattern[j] == '}')
                    {
                        i = j;
                        continue;
                    }
                }
                continue;
            }
            if (c == '}')
            {
                // A right curly brace outside a character class and not part of a quantifier
                // is not a NormalChar in XSD regular expressions.
                throw new InvalidOperationException("FORX0002");
            }
        }
    }

    /// <summary>
    /// XPath/XSD backreferences are written as <c>\N</c> where <c>N</c> is one or more digits.
    /// .NET interprets a digit sequence after the backslash differently: <c>\12</c> is a
    /// backreference to group 12 if it exists, or an octal escape otherwise. XPath semantics
    /// require that <c>\12</c> refer to group 12 when there are at least 12 capturing groups,
    /// and otherwise be treated as the longest valid backreference prefix followed by literal
    /// digits (e.g. <c>\1</c> + literal <c>2</c> when only one group exists).
    /// </summary>
    private static string TranslateEndAnchor(string pattern, bool multiline)
    {
        if (multiline)
            return pattern;

        var sb = new StringBuilder(pattern.Length + 2);
        bool escaped = false;
        bool inCharClass = false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (escaped)
            {
                sb.Append('\\');
                sb.Append(c);
                escaped = false;
                continue;
            }
            if (inCharClass)
            {
                sb.Append(c);
                if (c == ']')
                    inCharClass = false;
                continue;
            }
            if (c == '\\')
            {
                escaped = true;
                continue;
            }
            if (c == '[')
            {
                inCharClass = true;
                sb.Append(c);
                continue;
            }
            if (c == '$')
            {
                sb.Append(@"\z");
                continue;
            }
            sb.Append(c);
        }
        if (escaped)
            sb.Append('\\');
        return sb.ToString();
    }

    private static string TranslateBackreferences(string pattern)
    {
        int groupCount = CountCapturingGroups(pattern);
        var sb = new StringBuilder(pattern.Length + 8);
        bool escaped = false;
        bool inCharClass = false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (inCharClass)
            {
                if (!escaped && c == ']') inCharClass = false;
                escaped = !escaped && c == '\\';
                sb.Append(c);
                continue;
            }
            if (c == '\\')
            {
                if (i + 1 < pattern.Length && char.IsDigit(pattern[i + 1]))
                {
                    int digitsStart = i + 1;
                    int digitsEnd = digitsStart;
                    while (digitsEnd < pattern.Length && char.IsDigit(pattern[digitsEnd])) digitsEnd++;
                    string digits = pattern[digitsStart..digitsEnd];
                    int fullNumber = int.Parse(digits, System.Globalization.CultureInfo.InvariantCulture);
                    int prefixLength = 0;
                    int prefixNumber = 0;
                    for (int k = 1; k <= digits.Length; k++)
                    {
                        int candidate = int.Parse(digits[0..k], System.Globalization.CultureInfo.InvariantCulture);
                        if (candidate > 0 && candidate <= groupCount)
                        {
                            prefixLength = k;
                            prefixNumber = candidate;
                        }
                    }

                    if (prefixLength == 0)
                    {
                        // No valid group for any prefix; leave the escape as-is.
                        sb.Append('\\');
                        sb.Append(digits);
                    }
                    else if (prefixLength == digits.Length)
                    {
                        // The whole digit sequence is a valid group reference.
                        sb.Append('\\');
                        sb.Append(prefixNumber);
                    }
                    else
                    {
                        // Use the longest valid prefix as a backreference and treat the rest
                        // as literal digits.
                        sb.Append("(?:\\");
                        sb.Append(prefixNumber);
                        sb.Append(')');
                        sb.Append(digits[prefixLength..]);
                    }
                    i = digitsEnd - 1;
                    continue;
                }
                escaped = true;
                sb.Append(c);
                continue;
            }
            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                continue;
            }
            if (c == '[')
            {
                inCharClass = true;
                sb.Append(c);
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Counts the number of capturing groups (parenthesized subexpressions) in the regular
    /// expression. Non-capturing groups and character classes are ignored.
    /// </summary>
    public static int CountCapturingGroups(string pattern)
    {
        int count = 0;
        bool escaped = false;
        bool inCharClass = false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (escaped)
            {
                escaped = false;
                continue;
            }
            if (c == '\\')
            {
                escaped = true;
                continue;
            }
            if (inCharClass)
            {
                if (c == ']') inCharClass = false;
                continue;
            }
            if (c == '[')
            {
                inCharClass = true;
                continue;
            }
            if (c == '(')
            {
                // A capturing group is '(' not followed by '?'.
                if (i + 1 >= pattern.Length || pattern[i + 1] != '?')
                    count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Builds a parent map for the capturing groups in a .NET-compatible pattern.
    /// The returned array is indexed by group number; element <c>0</c> is a dummy and
    /// element <c>n</c> contains the number of the enclosing capturing group, or <c>0</c>
    /// if the group is at the top level.
    /// </summary>
    public static int[] GetCapturingGroupParents(string pattern)
    {
        var parents = new List<int> { 0 };
        var groupStack = new Stack<int>();
        var isCapturingStack = new Stack<bool>();
        bool escaped = false;
        bool inCharClass = false;

        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (escaped)
            {
                escaped = false;
                continue;
            }
            if (c == '\\')
            {
                escaped = true;
                continue;
            }
            if (inCharClass)
            {
                if (c == ']') inCharClass = false;
                continue;
            }
            if (c == '[')
            {
                inCharClass = true;
                continue;
            }
            if (c == '(')
            {
                bool isCapturing;
                if (i + 1 >= pattern.Length || pattern[i + 1] != '?')
                {
                    isCapturing = true;
                }
                else
                {
                    // .NET named capturing groups (?<name>...) and (?'name'...) are capturing;
                    // all other (? constructs are not.
                    isCapturing = i + 2 < pattern.Length && (pattern[i + 2] == '<' || pattern[i + 2] == '\'');
                }

                if (isCapturing)
                {
                    int parent = groupStack.Count > 0 ? groupStack.Peek() : 0;
                    int groupNumber = parents.Count;
                    parents.Add(parent);
                    groupStack.Push(groupNumber);
                }

                isCapturingStack.Push(isCapturing);
            }
            else if (c == ')')
            {
                if (isCapturingStack.Count > 0)
                {
                    bool isCapturing = isCapturingStack.Pop();
                    if (isCapturing && groupStack.Count > 0)
                        groupStack.Pop();
                }
            }
        }

        return parents.ToArray();
    }
}
