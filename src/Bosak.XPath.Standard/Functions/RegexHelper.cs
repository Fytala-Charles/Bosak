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
//                      | Charles Korthout | 0.3   | 26-06-2026     | Translate '.' to match Unicode code points, including surrogate pairs                   |
//                      | Charles Korthout | 0.4   | 11-07-2026     | Translate XSD char classes via XsdCharClasses with pinned Unicode 9.0 data              |
//                      | Charles Korthout | 0.5   | 14-07-2026     | Translation/Regex caches; always Compiled (NonBacktracking silently mis-matched U+000A) |
//                      | Charles Korthout | 0.6   | 15-07-2026     | QT3 regex cluster: dot excludes \r, x-flag whitespace strip, FORX0002 for backrefs to unclosed groups
//                      | Charles Korthout | 0.7   | 19-07-2026     | XPath 'i' flag: case-fold during translation; ParseRegexFlags returns caseInsensitive flag      |
//                      | Charles Korthout | 0.8   | 19-07-2026     | XPath 'i' flag: use RegexOptions.IgnoreCase, wrap class atoms in (?-i:)                  |
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
    /// The <c>i</c> flag is mapped to <see cref="RegexOptions.IgnoreCase"/> so that
    /// .NET handles literal case-folding and back-references; the <paramref name="caseInsensitive"/>
    /// out parameter is still returned so that the XSD translator can wrap category and
    /// bracketed class atoms in <c>(?-i:...)</c>, keeping them case-sensitive per XPath semantics.
    /// </summary>
    public static RegexOptions ParseRegexFlags(string flags, out bool isQuoteMode, out bool caseInsensitive)
    {
        var options = RegexOptions.None;
        isQuoteMode = false;
        caseInsensitive = false;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': caseInsensitive = true; options |= RegexOptions.IgnoreCase; break;
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
        return TranslateEndAnchor(TranslateDot(TranslateBackreferences(XsdCharClasses.Translate(pattern)), RegexOptions.None), multiline: false);
    }

    /// <summary>
    /// Validates and translates a pattern when the regex flags (and therefore the multiline
    /// mode) are known. In non-multiline mode, <c>$</c> is translated to <c>\z</c> so that it
    /// matches only the absolute end of the string, not the position before a final newline.
    /// </summary>
    public static string ValidateAndTranslatePattern(string pattern, RegexOptions options, bool caseInsensitive)
    {
        if ((options & RegexOptions.IgnorePatternWhitespace) != 0)
            pattern = StripPatternWhitespace(pattern);
        ValidateXsdRegex(pattern);
        return TranslateEndAnchor(TranslateDot(TranslateBackreferences(XsdCharClasses.Translate(pattern, caseInsensitive)), options), (options & RegexOptions.Multiline) != 0);
    }

    /// <summary>
    /// Implements the XPath <c>x</c> flag: removes whitespace (#x20, #x9, #xD, #xA) from the
    /// pattern prior to matching. Whitespace inside character class expressions is retained.
    /// Removal applies even after a backslash (spec example: <c>hello\ sworld</c> strips to
    /// <c>hello\sworld</c>, the whitespace class), so a pending escape simply carries over
    /// to the next non-whitespace character.
    /// </summary>
    private static string StripPatternWhitespace(string pattern)
    {
        var sb = new StringBuilder(pattern.Length);
        bool escaped = false;
        int classDepth = 0;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (escaped)
            {
                if (c is ' ' or '\t' or '\r' or '\n')
                    continue; // strip; the backslash stays pending for the next character
                sb.Append('\\');
                sb.Append(c);
                escaped = false;
                continue;
            }
            if (c == '\\')
            {
                escaped = true;
                continue;
            }
            if (c == '[')
            {
                classDepth++;
                sb.Append(c);
                continue;
            }
            if (classDepth > 0)
            {
                sb.Append(c);
                if (c == ']')
                    classDepth--;
                continue;
            }
            if (c is ' ' or '\t' or '\r' or '\n')
                continue;
            sb.Append(c);
        }
        if (escaped)
            sb.Append('\\');
        return sb.ToString();
    }

    /// <summary>
    /// Throws <c>FORX0003</c> if the pattern matches a zero-length string.
    /// </summary>
    public static void CheckZeroLengthMatch(string pattern, RegexOptions options)
    {
        if (GetRegex(pattern, options).IsMatch(string.Empty))
            throw new InvalidOperationException("FORX0003");
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Pattern, RegexOptions Options, bool CaseInsensitive), string> TranslationCache = new();

    /// <summary>
    /// <see cref="ValidateAndTranslatePattern(string, RegexOptions, bool)"/> with a cache: hot paths
    /// such as <c>fn:matches</c> may translate the same literal pattern millions of times.
    /// The cache holds at most 512 entries and is cleared when full.
    /// </summary>
    public static string ValidateAndTranslatePatternCached(string pattern, RegexOptions options, bool caseInsensitive)
    {
        var key = (pattern, options, caseInsensitive);
        if (TranslationCache.TryGetValue(key, out var translated))
            return translated;
        if (TranslationCache.Count >= 512)
            TranslationCache.Clear();
        translated = ValidateAndTranslatePattern(pattern, options, caseInsensitive);
        TranslationCache[key] = translated;
        return translated;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Pattern, RegexOptions Options, bool CaseInsensitive), Regex> RegexCache = new();

    /// <summary>
    /// Validates and translates an XSD pattern (cached) and returns a cached <see cref="Regex"/>
    /// for it. The cache is keyed by the original (short) pattern so that hot loops calling
    /// <c>fn:matches</c>/<c>fn:replace</c> with large translated Unicode classes pay only a
    /// small dictionary lookup per call.
    /// </summary>
    public static Regex GetRegexForXsdPattern(string originalPattern, RegexOptions options, bool caseInsensitive)
    {
        var key = (originalPattern, options, caseInsensitive);
        if (RegexCache.TryGetValue(key, out var cached))
            return cached;
        string translated = ValidateAndTranslatePattern(originalPattern, options, caseInsensitive);
        return CacheRegex(key, translated, options);
    }

    /// <summary>
    /// Returns a cached <see cref="Regex"/> for the (already translated) pattern and options.
    /// Patterns are compiled (<see cref="RegexOptions.Compiled"/>): the large translated Unicode
    /// classes run far faster compiled than interpreted. (NonBacktracking was tried but both
    /// rejected large alternations and, worse, silently mis-matched U+000A on some patterns.)
    /// The cache holds at most 512 entries and is cleared when full (patterns are cheap to
    /// recreate).
    /// </summary>
    public static Regex GetRegex(string pattern, RegexOptions options)
    {
        var key = (pattern, options, false);
        if (RegexCache.TryGetValue(key, out var cached))
            return cached;
        return CacheRegex(key, pattern, options);
    }

    private static Regex CacheRegex((string Pattern, RegexOptions Options, bool CaseInsensitive) key, string pattern, RegexOptions options)
    {
        if (RegexCache.Count >= 512)
            RegexCache.Clear();
        var regex = new Regex(pattern, options | RegexOptions.Compiled);
        RegexCache[key] = regex;
        return regex;
    }

    /// <summary>
    /// Throws <c>FORX0003</c> if the pattern matches a zero-length string.
    /// </summary>
    public static void CheckZeroLengthMatch(Regex regex)
    {
        if (regex.IsMatch(string.Empty))
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
        int classDepth = 0;
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
            if (c == '[')
            {
                // Class expressions nest one level for subtraction ([a-d-[b-c]]).
                classDepth++;
                continue;
            }
            if (c == ']')
            {
                // A right square bracket outside a character class is not a NormalChar
                // in XSD regular expressions (re00804: 'a]').
                if (classDepth == 0)
                    throw new InvalidOperationException("FORX0002");
                classDepth--;
                continue;
            }
            if (classDepth > 0)
                continue;
            if (c == '(')
            {
                // XSD/XPath groups are plain (...) or non-capturing (?:...); .NET-only
                // constructs ((?=, (?!, (?<, (?#, (?i:, ...) are invalid (re00767+).
                if (i + 1 < pattern.Length && pattern[i + 1] == '?' &&
                    (i + 2 >= pattern.Length || pattern[i + 2] != ':'))
                    throw new InvalidOperationException("FORX0002");
                continue;
            }
            if (c == '{')
            {
                // A valid quantifier is {n}, {n,} or {n,m}; a bare '{' is not a NormalChar
                // in XSD regular expressions (re00567-9).
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
                throw new InvalidOperationException("FORX0002");
            }
            if (c == '}')
            {
                // A right curly brace outside a character class and not part of a quantifier
                // is not a NormalChar in XSD regular expressions.
                throw new InvalidOperationException("FORX0002");
            }
        }
        if (escaped)
        {
            // A trailing backslash has nothing to escape.
            throw new InvalidOperationException("FORX0002");
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
        {
            // XPath multiline '^' matches at the start of the string and after any newline
            // OTHER than a newline that is the last character; .NET's Multiline '^' also
            // matches at the position after a trailing newline (fn-matches-26). Guard it.
            var mlb = new StringBuilder(pattern.Length + 8);
            bool mlEscaped = false;
            bool mlInClass = false;
            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (mlEscaped)
                {
                    mlb.Append('\\');
                    mlb.Append(c);
                    mlEscaped = false;
                    continue;
                }
                if (mlInClass)
                {
                    mlb.Append(c);
                    if (c == ']')
                        mlInClass = false;
                    continue;
                }
                if (c == '\\')
                {
                    mlEscaped = true;
                    continue;
                }
                if (c == '[')
                {
                    mlInClass = true;
                    mlb.Append(c);
                    continue;
                }
                if (c == '^')
                {
                    // Forbidden position: absolute end preceded by a newline. (On the empty
                    // string '^' still matches at 0, so a plain (?!\z) guard is wrong.)
                    mlb.Append("^(?!(?<=\\n)\\z)");
                    continue;
                }
                mlb.Append(c);
            }
            if (mlEscaped)
                mlb.Append('\\');
            return mlb.ToString();
        }

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

    /// <summary>
    /// Translates XPath/XSD <c>.</c> so that it matches Unicode code points rather than
    /// .NET 16-bit code units. Without this, a surrogate pair (e.g. U+20000) is treated as
    /// two separate characters and <c>.</c> fails to match it. The resulting alternation
    /// prefers a high+low surrogate pair over a single code unit.
    /// </summary>
    private static string TranslateDot(string pattern, RegexOptions options)
    {
        bool matchNewline = (options & RegexOptions.Singleline) != 0;
        const string SurrogatePair = "[\\ud800-\\udbff][\\udc00-\\udfff]";
        // XSD '.' matches any character except #xA and #xD (.NET '.' excludes only \n).
        string single = matchNewline ? "[\\s\\S]" : "[^\\r\\n]";
        string replacement = $"(?:{SurrogatePair}|{single})";

        var sb = new StringBuilder(pattern.Length + 16);
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
            if (c == '.')
            {
                sb.Append(replacement);
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
        var closedGroups = new HashSet<int>();
        var openGroups = new Stack<int>();
        var parenIsCapturing = new Stack<bool>();
        int nextGroup = 0;
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

                    // A back-reference is \[1-9][0-9]*; a leading zero is not a back-reference
                    // (and not any other XSD construct either): (foo)(\077) is invalid.
                    if (digits[0] == '0')
                        throw new InvalidOperationException("FORX0002");

                    // Multi-digit gobbling (F&O 5.6.1.4): trailing digits are part of the
                    // reference only while the number does not exceed the capturing groups
                    // whose opening parenthesis precedes the reference.
                    int openBefore = nextGroup;
                    int prefixLength = 0;
                    int prefixNumber = 0;
                    for (int k = 1; k <= digits.Length; k++)
                    {
                        int candidate = int.Parse(digits[0..k], System.Globalization.CultureInfo.InvariantCulture);
                        if (candidate > 0 && candidate <= openBefore)
                        {
                            prefixLength = k;
                            prefixNumber = candidate;
                        }
                    }

                    // The reference must identify an existing group (re00622: (foo)(\7)).
                    if (prefixLength == 0)
                        throw new InvalidOperationException("FORX0002");

                    // XSD erratum FO.E24: a back-reference to a group whose closing
                    // parenthesis has not yet been seen is an error (FORX0002).
                    if (!closedGroups.Contains(prefixNumber))
                        throw new InvalidOperationException("FORX0002");

                    if (prefixLength == digits.Length)
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
            if (c == '(')
            {
                bool isCapturing = i + 1 >= pattern.Length || pattern[i + 1] != '?';
                parenIsCapturing.Push(isCapturing);
                if (isCapturing)
                {
                    nextGroup++;
                    openGroups.Push(nextGroup);
                }
                sb.Append(c);
                continue;
            }
            if (c == ')')
            {
                if (parenIsCapturing.Count > 0 && parenIsCapturing.Pop())
                    closedGroups.Add(openGroups.Pop());
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
