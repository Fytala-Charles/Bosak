// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Verify normalization form whitespace, defaults, keyword calls and profile boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verify normalization form whitespace, defaults, keyword calls and profile boundaries.</summary>
public sealed class NormalizationOptionsTests
{
    private const string Fn = "http://www.w3.org/2005/xpath-functions";
    private const string Input = "codepoints-to-string((65,775))";

    /// <summary>Supplies exact result and error expectations for all supported profiles and XSLT mode.</summary>
    /// <returns>Profile, mode, expression, expected scalar string and optional error code.</returns>
    public static IEnumerable<object[]> Cases()
    {
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        foreach (bool xslt in new[] { false, true })
        {
            bool modern = profile != XPathCompatibility.XPath31;
            foreach (var pair in new[] { ("NFC", "550"), ("NFD", "65,775"), ("NFKC", "550"), ("NFKD", "65,775"), ("FULLY-NORMALIZED", "550"), ("", "65,775") })
            {
                yield return [profile, xslt, $"normalize-unicode({Input}, '{pair.Item1}')", pair.Item2, ""];
                yield return [profile, xslt, $"normalize-unicode({Input}, concat(codepoints-to-string((9,10,13,32)), '{pair.Item1.ToLowerInvariant()}', codepoints-to-string((32,13,10,9))))", pair.Item2, ""];
            }
            foreach (int cp in new[] { 160, 133, 8195, 8232, 8233, 65279, 8203 })
                yield return [profile, xslt, $"normalize-unicode({Input}, concat(codepoints-to-string({cp}), 'NFC', codepoints-to-string({cp})))", "", "FOCH0003"];
            yield return [profile, xslt, $"normalize-unicode({Input}, 'N F C')", "", "FOCH0003"];
            yield return [profile, xslt, $"normalize-unicode({Input}, 'NFC-C22')", "", "FOCH0003"];
            yield return [profile, xslt, $"normalize-unicode({Input}, codepoints-to-string(160))", "", "FOCH0003"];
            yield return [profile, xslt, $"normalize-unicode({Input})", "550", ""];
            foreach (string call in new[]
            {
                $"normalize-unicode({Input}, ())",
                $"(normalize-unicode#2)({Input}, ())",
                $"function-lookup(QName('{Fn}','normalize-unicode'),2)({Input}, ())",
                $"let $f := normalize-unicode(?, ()) return $f({Input})"
            }) yield return [profile, xslt, call, "550", modern ? "" : "XPTY0004"];
            yield return [profile, xslt, "normalize-unicode((), ())", "", modern ? "" : "XPTY0004"];
            yield return [profile, xslt, "normalize-unicode('x', 42)", "", "XPTY0004"];
            yield return [profile, xslt, "normalize-unicode('x', ('NFC','NFD'))", "", "XPTY0004"];
            yield return [profile, xslt, "normalize-unicode(42, 'NFC')", "", "XPTY0004"];
            yield return [profile, xslt, "normalize-unicode(('x','y'), 'NFC')", "", "XPTY0004"];
            yield return [profile, xslt, $"normalize-unicode({Input}, xs:untypedAtomic('NFC'))", "550", ""];
            yield return [profile, xslt, "let $d := parse-xml('<r><v>A&#x307;</v><f> NFC </f></r>') return normalize-unicode($d/r/v, $d/r/f)", "550", ""];
            foreach (string call in new[]
            {
                $"normalize-unicode(value := {Input})",
                $"normalize-unicode(form := (), value := {Input})",
                $"normalize-unicode(form := ' nFc ', value := {Input})"
            }) yield return [profile, xslt, call, "550", modern ? "" : "XPST0003"];
            yield return [profile, xslt, $"normalize-unicode(form := '', value := {Input})", "65,775", modern ? "" : "XPST0003"];
            yield return [profile, xslt, "normalize-unicode(value := (), form := ())", "", modern ? "" : "XPST0003"];
            yield return [profile, xslt, "normalize-unicode(form := 42, value := 'x')", "", modern ? "XPTY0004" : "XPST0003"];
            yield return [profile, xslt, "normalize-unicode((), '')", "", ""];
        }
    }

    /// <summary>Checks exact scalar outputs, singleton string cardinality and profile-specific errors.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="xslt">Whether evaluation uses the shared XSLT mode.</param>
    /// <param name="expression">The call expression.</param>
    /// <param name="expected">The exact comma-separated result scalars.</param>
    /// <param name="code">The required error, or empty for success.</param>
    [Theory, MemberData(nameof(Cases))]
    public void OptionsContract(XPathCompatibility profile, bool xslt, string expression, string expected, string code)
    {
        var context = new EvaluationContext { IsXsltMode = xslt };
        FunctionLibrary.Populate(context);
        string query = $"let $r := ({expression}) return concat(count($r), ':', string-join(string-to-codepoints($r) ! string(.), ','))";
        string Run() => XPath31Expression.Compile(query, new CompileOptions { Compatibility = profile }).Evaluate(context).ToString();
        if (code.Length == 0) Assert.Equal("1:" + expected, Run());
        else Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Run()).Message);
    }
}
