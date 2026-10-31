// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Verify true empty date-formatting results across profiles and call surfaces.
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
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verify true empty date-formatting results across profiles and call surfaces.</summary>
public sealed class DateFormattingEmptyTests
{
    private const string Fn = "http://www.w3.org/2005/xpath-functions";

    /// <summary>Supplies public call surfaces, nonempty contrasts and typed/error boundaries.</summary>
    /// <returns>Profile, XSLT mode, call, expected summary and expected error code.</returns>
    public static IEnumerable<object[]> Cases()
    {
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        foreach (bool xslt in new[] { false, true })
        foreach (var item in new[]
        {
            ("format-date", "xs:date('2002-03-07')", "[D01]", "07"),
            ("format-dateTime", "xs:dateTime('2002-03-07T12:30:00')", "[D01]", "07"),
            ("format-time", "xs:time('12:30:00')", "[H01]", "12")
        })
        {
            foreach (int arity in new[] { 2, 5 })
            {
                string tail = arity == 2 ? "" : ", 'en', 'AD', ()";
                string args = $"(), '{item.Item3}'{tail}";
                foreach (string call in new[]
                {
                    $"{item.Item1}({args})",
                    $"({item.Item1}#{arity})({args})",
                    $"function-lookup(QName('{Fn}', '{item.Item1}'), {arity})({args})",
                    $"{item.Item1}(parse-xml('<r/>')/r/missing, '{item.Item3}'{tail})",
                    $"{item.Item1}(({item.Item2})[false()], '{item.Item3}'{tail})"
                }) yield return [profile, xslt, call, "true|0|", ""];
                yield return [profile, xslt, $"{item.Item1}({item.Item2}, '{item.Item3}'{tail})", "false|1|" + item.Item4, ""];
            }
            yield return [profile, xslt, $"{item.Item1}((), '[')", "true|0|", ""];
            yield return [profile, xslt, $"{item.Item1}((), '[', 'en', 'Q{{}}unknown', ())", "true|0|", ""];
            yield return [profile, xslt, $"{item.Item1}({item.Item2}, '[')", "", "FOFD1340"];
            yield return [profile, xslt, $"{item.Item1}({item.Item2}, '{item.Item3}', 'en', 'Q{{}}1', ())", "", "FOFD1340"];
            yield return [profile, xslt, $"{item.Item1}((), '{item.Item3}', 42, (), ())", "", "XPTY0004"];
            yield return [profile, xslt, $"{item.Item1}((), 42)", "", "XPTY0004"];
        }
    }

    /// <summary>Distinguishes an empty sequence from an empty string through public evaluation.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="xslt">Whether the shared formatter runs in XSLT mode.</param>
    /// <param name="call">The date-formatting call.</param>
    /// <param name="expected">Exact emptiness, count and lexical summary.</param>
    /// <param name="code">Expected error code, or empty for success.</param>
    [Theory, MemberData(nameof(Cases))]
    public void PublicEmptyContract(XPathCompatibility profile, bool xslt, string call, string expected, string code)
    {
        var context = new EvaluationContext { IsXsltMode = xslt };
        FunctionLibrary.Populate(context);
        string Run() => XPath31Expression.Compile($"let $r := {call} return concat(empty($r), '|', count($r), '|', string($r))",
            new CompileOptions { Compatibility = profile }).Evaluate(context).ToString();
        if (code.Length == 0) Assert.Equal(expected, Run());
        else Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Run()).Message);
    }

    /// <summary>Supplies both internal empty representations without VM conversion normalizing them.</summary>
    /// <returns>Function name, arity, sequence-wrapper flag and XSLT mode.</returns>
    public static IEnumerable<object[]> Representations()
    {
        foreach (string name in new[] { "format-date", "format-dateTime", "format-time" })
        foreach (int arity in new[] { 2, 5 })
        foreach (bool sequence in new[] { false, true })
        foreach (bool xslt in new[] { false, true })
            yield return [name, arity, sequence, xslt];
    }

    /// <summary>Requires a true empty value and optional string metadata for both representations.</summary>
    /// <param name="name">The function name.</param>
    /// <param name="arity">The existing arity.</param>
    /// <param name="sequence">Whether to wrap a zero-item sequence.</param>
    /// <param name="xslt">Whether the shared formatter runs in XSLT mode.</param>
    [Theory, MemberData(nameof(Representations))]
    public void BothRepresentationsAreEmpty(string name, int arity, bool sequence, bool xslt)
    {
        Assert.True(FunctionLibrary.TryGetFunction(Fn, name, arity, out var function));
        XdmValue empty = sequence ? XdmValue.FromSequence(XdmSequence.Empty) : XdmValue.Undefined;
        XdmValue[] args = arity == 2
            ? [empty, XdmValue.FromString("literal")]
            : [empty, XdmValue.FromString("literal"), XdmValue.FromString("en"), XdmValue.FromString("AD"), XdmValue.Undefined];
        var result = function.Implementation(new EvaluationContext { IsXsltMode = xslt }, args);
        Assert.True(result.IsUndefined);
        Assert.Equal("xs:string?", function.ReturnTypeName);
    }
}
