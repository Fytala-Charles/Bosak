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
    public static string Translate(string pattern)
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
                    sb.Append(EmitAtom(set));
                    continue;
                }
                // Any other escape passes through unchanged.
                sb.Append('\\');
                sb.Append(e);
                i += 2;
                continue;
            }
            if (c == '[')
            {
                int[] set = ParseClassExpression(pattern, ref i);
                sb.Append(EmitAtom(set));
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

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
            case 's': return [0x20, 0x20, 0x09, 0x09, 0x0A, 0x0A, 0x0D, 0x0D];
            case 'S': return Complement([0x20, 0x20, 0x09, 0x09, 0x0A, 0x0A, 0x0D, 0x0D]);
            case 'i': return Union(Union(UnicodeData90.GetCategoryRanges("L")!, UnicodeData90.GetCategoryRanges("Nl")!), [0x3A, 0x3A, 0x5F, 0x5F]);
            case 'I': return Complement(Union(Union(UnicodeData90.GetCategoryRanges("L")!, UnicodeData90.GetCategoryRanges("Nl")!), [0x3A, 0x3A, 0x5F, 0x5F]));
            case 'c': return NameChars();
            case 'C': return Complement(NameChars());
            default:
                throw new InvalidOperationException("FORX0002");
        }
    }

    private static int[]? _wordNonChars;

    // \W = P | Z | C (so \w is everything else).
    private static int[] WordNonChars() => _wordNonChars ??= Union(Union(
        UnicodeData90.GetCategoryRanges("P")!, UnicodeData90.GetCategoryRanges("Z")!),
        UnicodeData90.GetCategoryRanges("C")!);

    private static int[]? _nameChars;

    // \c = L | M | N | {':', '_', '-', '.', U+00B7, U+203F..U+2040}
    private static int[] NameChars() => _nameChars ??= Union(Union(Union(
        UnicodeData90.GetCategoryRanges("L")!, UnicodeData90.GetCategoryRanges("M")!),
        UnicodeData90.GetCategoryRanges("N")!),
        [0x2D, 0x2E, 0x3A, 0x3A, 0x5F, 0x5F, 0xB7, 0xB7, 0x203F, 0x2040]);

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
    private static int[] ParseClassExpression(string pattern, ref int i)
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
                FlushPending(set, pendingSingle);
                i++;
                break;
            }
            if (c == '-' && i + 1 < pattern.Length && pattern[i + 1] == '[')
            {
                // Subtraction: flush a pending single first (a-[x] = {a} - [x]).
                FlushPending(set, pendingSingle);
                pendingSingle = -1;
                i++; // skip '-'; ParseClassExpression expects '['
                int[] sub = ParseClassExpression(pattern, ref i);
                int[] current = Normalize(set);
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
            return (e switch
            {
                'n' => 0x0A,
                'r' => 0x0D,
                't' => 0x09,
                '\\' or '|' or '.' or '-' or '^' or '$' or '?' or '*' or '+' or
                '{' or '}' or '(' or ')' or '[' or ']' => e,
                _ => throw new InvalidOperationException("FORX0002")
            }, null);
        }
        // A literal code point, possibly an astral character (surrogate pair).
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
        => Difference(Universe, set);

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
