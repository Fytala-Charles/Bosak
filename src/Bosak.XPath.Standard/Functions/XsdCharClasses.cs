// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 juli 2026
// PURPOSE              : XSD regex character-class engine: evaluates \p{}/\P{}, \d \w \s \i \c and class expressions against Unicode 9.0.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-07-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-07-2026     | Sorted \s literal (Complement requires normalized input); FORX0002 on empty char class  |
//                      | Charles Korthout | 0.3   | 19-07-2026     | XPath 'i' flag: case-fold during translation; \p{} escapes unaffected (caselessmatch12-14) |
//                      | Charles Korthout | 0.4   | 19-07-2026     | XPath 'i' flag: use RegexOptions.IgnoreCase, wrap class atoms in (?-i:)                 |
//                      | Charles Korthout | 0.5   | 03-08-2026     | \i/\c use explicit XML 1.0 5e NameStartChar/NameChar ranges (XSD 1.1; regex-syntax-0986/7) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Evaluates the XSD regular-expression character-class constructs — <c>\p{...}</c>/<c>\P{...}</c>
/// general-category and block escapes, the single-letter escapes <c>\d \D \w \W \s \S \i \I \c \C</c>,
/// and bracketed class expressions with ranges, negation (<c>[^...]</c>) and subtraction
/// (<c>[a-[b]]</c>) — against pinned Unicode 9.0.0 data, and emits an equivalent .NET regex
/// fragment. All emitted classes exclude the surrogate range; astral members are emitted as
/// surrogate-pair alternatives so that matching is based on whole Unicode code points.
/// When the XPath <c>i</c> flag is active, class atoms are wrapped in <c>(?-i:...)</c> so that
/// .NET's <see cref="RegexOptions.IgnoreCase"/> expands literals but leaves category and
/// bracketed classes unchanged, matching XPath case-folding semantics.
/// </summary>
internal static class XsdCharClasses
{
    private const int SurrogateLo = 0xD800;
    private const int SurrogateHi = 0xDFFF;
    private const int MaxCodePoint = 0x10FFFF;
    // Universe for negation/complement: every code point except the surrogate range.
    // (Surrogates are XML-noncharacters; astral characters must be matched as pairs, never as
    // individual code units, so no emitted class may contain the surrogate range.)
    private static readonly int[] Universe = [0, SurrogateLo - 1, SurrogateHi + 1, MaxCodePoint];

    /// <summary>
    /// Translates all XSD character-class constructs in <paramref name="pattern"/> to .NET
    /// syntax. Other constructs pass through unchanged.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown with code <c>FORX0002</c> when the
    /// pattern contains an unknown category/block name or a malformed class expression.</exception>
    public static string Translate(string pattern) => Translate(pattern, false);

    /// <summary>
    /// Translates all XSD character-class constructs in <paramref name="pattern"/> to .NET
    /// syntax. When <paramref name="caseInsensitive"/> is <c>true</c>, emitted class atoms
    /// (category escapes, single-letter escapes, and bracketed class expressions) are wrapped
    /// in <c>(?-i:...)</c> so that .NET's <see cref="RegexOptions.IgnoreCase"/> only affects
    /// literal characters, matching XPath case-folding semantics.
    /// </summary>
    public static string Translate(string pattern, bool caseInsensitive)
    {
        var sb = new StringBuilder(pattern.Length + 16);
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            if (c == '\\')
            {
                if (i + 1 >= pattern.Length)
                    throw new InvalidOperationException("FORX0002");
                char e = pattern[i + 1];
                if (IsClassEscapeLetter(e))
                {
                    i += 2;
                    int[] set = ReadEscapeSet(pattern, ref i, e);
                    // Category escapes \p{...} and \P{...} are never case-folded by the i flag;
                    // wrap them in (?-i:...) so .NET's RegexOptions.IgnoreCase leaves them alone.
                    bool isCategoryEscape = e is 'p' or 'P';
                    sb.Append(caseInsensitive && isCategoryEscape ? MakeCaseSensitiveAtom(set) : EmitAtom(set));
                    continue;
                }
                // Any other escape passes through unchanged, but only XSD SingleCharEsc
                // letters and back-reference digits are legal here (.NET-only escapes such
                // as \x, \u, \A, \b are rejected: re00627, re00767, re00791).
                if (e is 'n' or 'r' or 't' or '\\' or '|' or '.' or '?' or '*' or '+' or
                    '(' or ')' or '{' or '}' or '$' or '-' or '[' or ']' or '^' ||
                    char.IsDigit(e))
                {
                    sb.Append('\\');
                    sb.Append(e);
                    i += 2;
                    continue;
                }
                throw new InvalidOperationException("FORX0002");
            }
            if (c == '[')
            {
                int[] set = ParseClassExpression(pattern, ref i, caseInsensitive);
                // Bracketed class expressions are case-folded under the i flag; the fold is
                // applied during parsing (single code points) and completed by the caller's
                // RegexOptions.IgnoreCase (ranges).  Do not wrap in (?-i:...).
                sb.Append(EmitAtom(set));
                continue;
            }
            // Literal characters pass through unchanged.  RegexOptions.IgnoreCase (applied by
            // the caller) handles case-insensitive matching for literals, while class atoms are
            // wrapped in (?-i:...) to prevent .NET from expanding them.
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    private static string MakeCaseSensitiveAtom(int[] set) => "(?-i:" + EmitAtom(set) + ")";

    private static bool IsClassEscapeLetter(char e) => e is 'p' or 'P' or 'd' or 'D' or 'w' or 'W' or 's' or 'S' or 'i' or 'I' or 'c' or 'C';

    /// <summary>
    /// Reads the set denoted by a class escape starting at <paramref name="escape"/>; for
    /// <c>\p{...}</c>/<c>\P{...}</c> the position must be just after the escape letter.
    /// </summary>
    private static int[] ReadEscapeSet(string pattern, ref int i, char escape)
    {
        switch (escape)
        {
            case 'p':
            case 'P':
            {
                if (i >= pattern.Length || pattern[i] != '{')
                    throw new InvalidOperationException("FORX0002");
                int close = pattern.IndexOf('}', i + 1);
                if (close < 0)
                    throw new InvalidOperationException("FORX0002");
                string name = pattern[(i + 1)..close];
                i = close + 1;
                int[] set = LookupNamedSet(name);
                return escape == 'P' ? Complement(set) : set;
            }
            case 'd': return CopyOf(UnicodeData90.GetCategoryRanges("Nd")!);
            case 'D': return Complement(UnicodeData90.GetCategoryRanges("Nd")!);
            case 'w': return Complement(WordNonChars());
            case 'W': return WordNonChars();
            case 's': return [0x09, 0x09, 0x0A, 0x0A, 0x0D, 0x0D, 0x20, 0x20];
            case 'S': return Complement([0x09, 0x09, 0x0A, 0x0A, 0x0D, 0x0D, 0x20, 0x20]);
            case 'i': return CopyOf(NameStartCharRanges);
            case 'I': return Complement(NameStartCharRanges);
            case 'c': return CopyOf(NameCharRanges);
            case 'C': return Complement(NameCharRanges);
            default:
                throw new InvalidOperationException("FORX0002");
        }
    }

    private static int[]? _wordNonChars;

    // \W = P | Z | C (so \w is everything else).
    private static int[] WordNonChars() => _wordNonChars ??= Union(Union(
        UnicodeData90.GetCategoryRanges("P")!, UnicodeData90.GetCategoryRanges("Z")!),
        UnicodeData90.GetCategoryRanges("C")!);

    // \i = NameStartChar (XML 1.0 5th edition): ':' | [A-Z] | '_' | [a-z] | [#xC0-#xD6] |
    // [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] |
    // [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] |
    // [#x10000-#xEFFFF]. The explicit ranges (not Unicode categories) are the XSD 1.1
    // definition; they deliberately include unassigned code points inside the listed
    // blocks (regex-syntax-0986: U+212E ESTIMATED SYMBOL is an initial name character).
    private static readonly int[] NameStartCharRanges =
    [
        0x3A, 0x3A, 0x41, 0x5A, 0x5F, 0x5F, 0x61, 0x7A, 0xC0, 0xD6, 0xD8, 0xF6,
        0xF8, 0x2FF, 0x370, 0x37D, 0x37F, 0x1FFF, 0x200C, 0x200D, 0x2070, 0x218F,
        0x2C00, 0x2FEF, 0x3001, 0xD7FF, 0xF900, 0xFDCF, 0xFDF0, 0xFFFD, 0x10000, 0xEFFFF
    ];

    // \c = NameChar (XML 1.0 5th edition): NameStartChar | '-' | '.' | [0-9] | #xB7 |
    // [#x0300-#x036F] | [#x203F-#x2040] (regex-syntax-0987).
    private static readonly int[] NameCharRanges =
    [
        0x2D, 0x2E, 0x30, 0x3A, 0x41, 0x5A, 0x5F, 0x5F, 0x61, 0x7A, 0xB7, 0xB7,
        0xC0, 0xD6, 0xD8, 0xF6, 0xF8, 0x2FF, 0x300, 0x36F, 0x370, 0x37D, 0x37F, 0x1FFF,
        0x200C, 0x200D, 0x203F, 0x2040, 0x2070, 0x218F, 0x2C00, 0x2FEF, 0x3001, 0xD7FF,
        0xF900, 0xFDCF, 0xFDF0, 0xFFFD, 0x10000, 0xEFFFF
    ];

    private static int[] CopyOf(int[] set)
    {
        var copy = new int[set.Length];
        Array.Copy(set, copy, set.Length);
        return copy;
    }

    private static int[] LookupNamedSet(string name)
    {
        if (name.StartsWith("Is", StringComparison.Ordinal))
        {
            var block = UnicodeData90.GetBlockRange(name);
            if (block is null)
                throw new InvalidOperationException("FORX0002");
            return [block.Value.Lo, block.Value.Hi];
        }
        int[]? ranges = UnicodeData90.GetCategoryRanges(name);
        if (ranges is null)
            throw new InvalidOperationException("FORX0002");
        return ranges;
    }

    /// <summary>
    /// Parses a complete class expression <c>[...]</c> starting at the opening bracket
    /// (<paramref name="i"/> points at <c>[</c>); on return <paramref name="i"/> is just past
    /// the closing bracket. Handles negation and one level of subtraction (which may itself
    /// contain a nested subtraction, per the XSD grammar).
    /// </summary>
    private static int[] ParseClassExpression(string pattern, ref int i, bool caseInsensitive)
    {
        // pattern[i] == '['
        i++;
        bool negated = false;
        if (i < pattern.Length && pattern[i] == '^')
        {
            negated = true;
            i++;
        }

        var set = new List<int>();
        int pendingSingle = -1; // a single char that may still become the start of a range
        while (true)
        {
            if (i >= pattern.Length)
                throw new InvalidOperationException("FORX0002");
            char c = pattern[i];
            if (c == ']')
            {
                if (set.Count == 0 && pendingSingle < 0)
                {
                    // XSD grammar requires at least one member: [] and [^] are invalid.
                    throw new InvalidOperationException("FORX0002");
                }
                FlushPending(set, pendingSingle);
                i++;
                break;
            }
            if (c == '-' && i + 1 < pattern.Length && pattern[i + 1] == '[')
            {
                if (set.Count == 0 && pendingSingle < 0)
                {
                    // Subtraction requires a non-empty base (e.g. [^-[bc]] is invalid).
                    throw new InvalidOperationException("FORX0002");
                }
                // Subtraction: flush a pending single first (a-[x] = {a} - [x]).
                FlushPending(set, pendingSingle);
                pendingSingle = -1;
                i++; // skip '-'; ParseClassExpression expects '['
                int[] sub = ParseClassExpression(pattern, ref i, caseInsensitive);
                int[] current = Normalize(set);
                if (caseInsensitive)
                    current = CaseFoldSet(current);
                if (i >= pattern.Length || pattern[i] != ']')
                    throw new InvalidOperationException("FORX0002");
                i++;
                return FinishClass(negated, current, sub);
            }
            if (c == '-' && pendingSingle >= 0 && i + 1 < pattern.Length && pattern[i + 1] != ']')
            {
                // Range: pendingSingle '-' endpoint
                i++;
                var (hi, hiSet) = ReadClassAtom(pattern, ref i);
                if (hiSet is not null || hi < pendingSingle)
                    throw new InvalidOperationException("FORX0002");
                AddRange(set, pendingSingle, hi);
                pendingSingle = -1;
                continue;
            }

            var (single, atomSet) = ReadClassAtom(pattern, ref i);
            if (atomSet is not null)
            {
                FlushPending(set, pendingSingle);
                pendingSingle = -1;
                UnionInto(set, atomSet);
            }
            else
            {
                FlushPending(set, pendingSingle);
                pendingSingle = single;
            }
        }

        int[] normalized = Normalize(set);
        if (caseInsensitive)
            normalized = CaseFoldSet(normalized);
        return negated ? Complement(normalized) : normalized;
    }

    // Negation binds before subtraction: [^A-[B]] = (U - A) - B ; [A-[B]] = A - B.
    private static int[] FinishClass(bool negated, int[] current, int[] sub)
        => negated ? Difference(Complement(current), sub) : Difference(current, sub);

    private static void FlushPending(List<int> set, int pendingSingle)
    {
        if (pendingSingle >= 0)
            AddRange(set, pendingSingle, pendingSingle);
    }

    /// <summary>
    /// Reads one class member: either a single code point (<c>set</c> null) or an escape
    /// denoting a set (<c>set</c> non-null, <c>single</c> meaningless).
    /// </summary>
    private static (int Single, int[]? Set) ReadClassAtom(string pattern, ref int i)
    {
        char c = pattern[i];
        if (c == '\\')
        {
            if (i + 1 >= pattern.Length)
                throw new InvalidOperationException("FORX0002");
            char e = pattern[i + 1];
            i += 2;
            if (IsClassEscapeLetter(e))
                return (0, ReadEscapeSet(pattern, ref i, e));
            int cp = e switch
            {
                'n' => 0x0A,
                'r' => 0x0D,
                't' => 0x09,
                '\\' or '|' or '.' or '-' or '^' or '$' or '?' or '*' or '+' or
                '{' or '}' or '(' or ')' or '[' or ']' => e,
                _ => throw new InvalidOperationException("FORX0002")
            };
            return (cp, null);
        }
        // A literal code point, possibly an astral character (surrogate pair).
        // An unescaped '[' is not a valid class member in XSD (only the -[ subtraction form
        // nests classes, and that is handled by the caller).
        if (c == '[')
            throw new InvalidOperationException("FORX0002");
        if (c >= SurrogateLo && c <= 0xDBFF && i + 1 < pattern.Length &&
            pattern[i + 1] >= 0xDC00 && pattern[i + 1] <= SurrogateHi)
        {
            int cp = 0x10000 + ((c - SurrogateLo) << 10) + (pattern[i + 1] - 0xDC00);
            i += 2;
            return (cp, null);
        }
        i++;
        return (c, null);
    }

    /// <summary>
    /// Returns the case-insensitive closure of a normalized range set. Each code point in the
    /// input ranges is expanded with its simple case-folded variants (BMP only). Used for the
    /// XPath <c>i</c> flag on bracketed class expressions; category/escape escapes inside
    /// classes are folded by the surrounding class fold.
    /// </summary>
    private static int[] CaseFoldSet(ReadOnlySpan<int> ranges)
    {
        var result = new List<int>(ranges.Length * 2);
        for (int k = 0; k < ranges.Length; k += 2)
        {
            int lo = ranges[k];
            int hi = ranges[k + 1];
            for (int cp = lo; cp <= hi; cp++)
            {
                AddRange(result, cp, cp);
                if (cp < 0x10000)
                {
                    char c = (char)cp;
                    char lower = char.ToLowerInvariant(c);
                    char upper = char.ToUpperInvariant(c);
                    if (lower != c)
                        AddRange(result, lower, lower);
                    if (upper != c && upper != lower)
                        AddRange(result, upper, upper);
                }
            }
        }
        return Normalize(result);
    }

    // ------------------------------------------------------------------
    // Range-set operations. Sets are flat arrays [lo1, hi1, lo2, hi2, ...]
    // of inclusive bounds, sorted and non-overlapping.
    // ------------------------------------------------------------------

    private static void AddRange(List<int> set, int lo, int hi)
    {
        set.Add(lo);
        set.Add(hi);
    }

    private static void UnionInto(List<int> set, int[] other)
    {
        set.AddRange(other);
    }

    private static int[] Union(int[] a, int[] b)
    {
        var list = new List<int>(a.Length + b.Length);
        list.AddRange(a);
        list.AddRange(b);
        return Normalize(list);
    }

    private static int[] Normalize(List<int> ranges)
    {
        if (ranges.Count == 0)
            return [];
        var pairs = new List<(int Lo, int Hi)>(ranges.Count / 2);
        for (int k = 0; k < ranges.Count; k += 2)
            pairs.Add((ranges[k], ranges[k + 1]));
        pairs.Sort((x, y) => x.Lo != y.Lo ? x.Lo.CompareTo(y.Lo) : x.Hi.CompareTo(y.Hi));
        var result = new List<int>(ranges.Count);
        int lo = pairs[0].Lo, hi = pairs[0].Hi;
        for (int k = 1; k < pairs.Count; k++)
        {
            if (pairs[k].Lo <= hi + 1)
            {
                if (pairs[k].Hi > hi)
                    hi = pairs[k].Hi;
            }
            else
            {
                result.Add(lo);
                result.Add(hi);
                lo = pairs[k].Lo;
                hi = pairs[k].Hi;
            }
        }
        result.Add(lo);
        result.Add(hi);
        return result.ToArray();
    }

    private static int[] Complement(int[] set)
        => Difference(Universe, Normalize(new List<int>(set)));

    /// <summary>Computes <paramref name="a"/> minus <paramref name="b"/>; both normalized.</summary>
    private static int[] Difference(int[] a, int[] b)
    {
        var result = new List<int>(a.Length);
        int j = 0;
        for (int k = 0; k < a.Length; k += 2)
        {
            int lo = a[k];
            int hi = a[k + 1];
            int cur = lo;
            while (j < b.Length && b[j + 1] < cur)
                j += 2;
            int jj = j;
            while (jj < b.Length && b[jj] <= hi)
            {
                if (b[jj] > cur)
                {
                    result.Add(cur);
                    result.Add(Math.Min(b[jj] - 1, hi));
                }
                cur = Math.Max(cur, b[jj + 1] + 1);
                if (cur > hi)
                    break;
                jj += 2;
            }
            if (cur <= hi)
            {
                result.Add(cur);
                result.Add(hi);
            }
        }
        return result.ToArray();
    }

    // ------------------------------------------------------------------
    // Emission
    // ------------------------------------------------------------------

    /// <summary>
    /// Emits a .NET regex atom for the given set: a BMP class, astral surrogate-pair
    /// alternatives, or a combination. Empty sets emit a never-matching class.
    /// </summary>
    private static string EmitAtom(int[] set)
    {
        // No emitted class may contain the surrogate range.
        set = Difference(set, [SurrogateLo, SurrogateHi]);
        if (set.Length == 0)
            return "(?:[^\\s\\S])";

        var parts = new List<string>(4);
        var bmp = new List<int>();
        var astral = new List<int>();
        for (int k = 0; k < set.Length; k += 2)
        {
            if (set[k + 1] <= 0xFFFF)
            {
                bmp.Add(set[k]);
                bmp.Add(set[k + 1]);
            }
            else if (set[k] > 0xFFFF)
            {
                astral.Add(set[k]);
                astral.Add(set[k + 1]);
            }
            else
            {
                bmp.Add(set[k]);
                bmp.Add(0xFFFF);
                astral.Add(0x10000);
                astral.Add(set[k + 1]);
            }
        }

        if (bmp.Count > 0)
        {
            var cls = new StringBuilder(bmp.Count * 6 + 2);
            cls.Append('[');
            for (int k = 0; k < bmp.Count; k += 2)
            {
                AppendHex(cls, bmp[k]);
                if (bmp[k + 1] != bmp[k])
                {
                    cls.Append('-');
                    AppendHex(cls, bmp[k + 1]);
                }
            }
            cls.Append(']');
            parts.Add(cls.ToString());
        }

        for (int k = 0; k < astral.Count; k += 2)
        {
            int lo = astral[k];
            int hi = astral[k + 1];
            int hsLo = 0xD800 + ((lo - 0x10000) >> 10);
            int hsHi = 0xD800 + ((hi - 0x10000) >> 10);
            for (int hs = hsLo; hs <= hsHi; hs++)
            {
                int unitLo = hs == hsLo ? (lo - 0x10000) & 0x3FF : 0;
                int unitHi = hs == hsHi ? (hi - 0x10000) & 0x3FF : 0x3FF;
                var part = new StringBuilder(16);
                AppendHex(part, hs);
                if (unitLo == unitHi)
                {
                    AppendHex(part, 0xDC00 + unitLo);
                }
                else
                {
                    part.Append('[');
                    AppendHex(part, 0xDC00 + unitLo);
                    part.Append('-');
                    AppendHex(part, 0xDC00 + unitHi);
                    part.Append(']');
                }
                parts.Add(part.ToString());
            }
        }

        return parts.Count == 1 ? parts[0] : "(?:" + string.Join("|", parts) + ")";
    }

    private static void AppendHex(StringBuilder sb, int codeUnit)
        => sb.Append("\\u").Append(codeUnit.ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
}
