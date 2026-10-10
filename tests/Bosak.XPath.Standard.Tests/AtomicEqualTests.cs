// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 oktober 2026
// PURPOSE              : Unit tests verifying correctness of the underlying implementation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-123 fn:atomic-equal, F&O 4.0 §2.2.1): map-key equality incl. PR2168 binary |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>
/// Tests for fn:atomic-equal (F&O 4.0 §2.2.1): the map-key equality rule (op:same-key)
/// exposed as a function. Semantics pinned against qt4tests fn/atomic-equal.xml.
/// </summary>
public class AtomicEqualTests
{
    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    private static string[] Seq40(string xpath)
    {
        var result = Eval40(xpath);
        if (result.IsUndefined)
            return [];
        if (!result.IsSequence)
            return [result.ToString()!];
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    private static bool Bool40(string xpath) => Seq40(xpath)[0] == "true";

    private static InvalidOperationException Error40(string xpath)
        => Assert.Throws<InvalidOperationException>(() => Eval40(xpath));

    // ----- string family: codepoint equality, collation-independent ------------

    [Fact]
    public void Strings_UntypedAndUri_Interchangeable()
    {
        Assert.True(Bool40("fn:atomic-equal(xs:untypedAtomic('abc'), xs:string('abc'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:anyURI('abc'), xs:string('abc'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:untypedAtomic('abc'), xs:anyURI('abc'))"));
        Assert.False(Bool40("fn:atomic-equal('abc', 'xyz')"));
    }

    [Fact]
    public void Strings_IgnoreDefaultCollation()
    {
        // atomic-equal-002 shape: the default collation must NOT be consulted.
        Assert.True(Bool40(
            "fn:atomic-equal('ABC', 'abc') => not()"));
    }

    // ----- numerics: exact mathematical magnitude -------------------------------

    [Fact]
    public void Numerics_NanEqualsNan_CrossFloatDouble()
    {
        Assert.True(Bool40("fn:atomic-equal(xs:double('NaN'), xs:double('NaN'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:double('NaN'), xs:float('NaN'))"));
        Assert.False(Bool40("fn:atomic-equal(xs:double('NaN'), xs:double('INF'))"));
    }

    [Fact]
    public void Numerics_Infinities()
    {
        Assert.False(Bool40("fn:atomic-equal(xs:double('-INF'), xs:double('INF'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:float('-INF'), xs:double('-INF'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:float('INF'), xs:double('INF'))"));
    }

    [Fact]
    public void Numerics_ExactComparison_NoDoubleRounding()
    {
        // atomic-equal-007: 1.1 (decimal) is NOT the same key as 1.1e0 (double).
        Assert.False(Bool40("fn:atomic-equal(1.1, 1.1e0)"));
        // Exact representable values across types ARE equal (atomic-equal-009/010).
        Assert.True(Bool40("fn:atomic-equal(16777218, xs:double('16777218'))"));
        Assert.True(Bool40("fn:atomic-equal(16777218, xs:decimal('16777218'))"));
        Assert.True(Bool40("fn:atomic-equal(16777218, xs:float('16777218'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:double(16777218), xs:decimal('16777218'))"));
        Assert.True(Bool40("fn:atomic-equal(0.5, xs:double('0.5'))"));
    }

    [Fact]
    public void Numerics_ZeroSignIgnored()
    {
        Assert.True(Bool40("fn:atomic-equal(0, xs:double('-0'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:float('0'), xs:double('-0.0e0'))"));
    }

    // ----- date/time: timezone-presence rule ------------------------------------

    [Fact]
    public void DateTime_TimezonePresenceRequired()
    {
        // Values with a timezone are never equal to values without one; the
        // implicit timezone is NOT used (atomic-equal-013 shape).
        Assert.True(Bool40(
            "fn:atomic-equal(xs:dateTime('2015-04-08T02:30:00Z'), xs:dateTime('2015-04-08T03:30:00+01:00'))"));
        Assert.False(Bool40(
            "fn:atomic-equal(xs:dateTime('2015-04-08T02:30:00'), xs:dateTime('2015-04-08T02:30:00Z'))"));
    }

    [Fact]
    public void Time_SameInstantDifferentTimezones()
    {
        Assert.True(Bool40("fn:atomic-equal(xs:time('17:00:00Z'), xs:time('12:00:00-05:00'))"));
    }

    [Fact]
    public void GYear_DistinguishedByValue()
    {
        Assert.True(Bool40("fn:atomic-equal(xs:gYear('2015'), xs:gYear('2015'))"));
        Assert.False(Bool40("fn:atomic-equal(xs:gYear('2015'), xs:gYear('2014'))"));
        Assert.False(Bool40("fn:atomic-equal(xs:gYear('2015'), '2015')"));
    }

    // ----- QName: namespace URI + local name, prefix irrelevant ------------------

    [Fact]
    public void QName_PrefixIgnored()
    {
        Assert.True(Bool40(
            "fn:atomic-equal(QName('http://example.org', 'ns1:foo'), QName('http://example.org', 'ns2:foo'))"));
        Assert.False(Bool40(
            "fn:atomic-equal(QName('http://example.org', 'foo'), QName('', 'foo'))"));
        Assert.False(Bool40(
            "fn:atomic-equal(QName('http://example.org', 'ns1:foo'), QName('http://example.org', 'ns1:bar'))"));
    }

    // ----- duration: normalized totals ------------------------------------------

    [Fact]
    public void Duration_NormalizedEquality()
    {
        Assert.True(Bool40("fn:atomic-equal(xs:duration('P1Y'), xs:duration('P12M'))"));
        Assert.True(Bool40("fn:atomic-equal(xs:duration('P12M'), xs:duration('P12M'))"));
        Assert.False(Bool40("fn:atomic-equal(xs:duration('P1Y'), xs:duration('P1Y1D'))"));
    }

    // ----- binary: PR2168 mutual hex/base64 comparability ------------------------

    [Fact]
    public void Binary_HexAndBase64ComparableByOctets()
    {
        Assert.True(Bool40(
            "fn:atomic-equal(xs:hexBinary('ff'), xs:base64Binary(xs:hexBinary('ff')))"));
        Assert.False(Bool40(
            "fn:atomic-equal(xs:hexBinary('ff'), xs:base64Binary(xs:hexBinary('fe')))"));
        Assert.True(Bool40(
            "fn:atomic-equal(xs:hexBinary('0FB7'), xs:hexBinary('0FB7'))"));
        // A plain string is not a binary value.
        Assert.False(Bool40("fn:atomic-equal(xs:hexBinary('ff'), 'ff')"));
    }

    // ----- cross-type mismatches are never equal ---------------------------------

    [Fact]
    public void MixedTypes_False()
    {
        Assert.False(Bool40("fn:atomic-equal('1', 1)"));
        Assert.False(Bool40("fn:atomic-equal(1, true())"));
        Assert.False(Bool40("fn:atomic-equal(QName('', 'a'), 'a')"));
        Assert.False(Bool40("fn:atomic-equal(xs:date('2015-04-08'), xs:dateTime('2015-04-08T00:00:00'))"));
    }

    // ----- node arguments are atomized -------------------------------------------

    [Fact]
    public void NodeArguments_AtomizedToUntyped()
    {
        Assert.True(Bool40("fn:atomic-equal(parse-xml('<a>x</a>')/a/text(), 'x')"));
        Assert.True(Bool40("fn:atomic-equal(parse-xml('<a>x</a>')/a, 'x')"));
    }

    // ----- keyword arguments ------------------------------------------------------

    [Fact]
    public void KeywordArguments_Work()
    {
        Assert.True(Bool40("fn:atomic-equal(value1 := 'a', value2 := 'a')"));
        Assert.False(Bool40("fn:atomic-equal(value1 := 'a', value2 := 'b')"));
    }

    // ----- cardinality / error behaviour -----------------------------------------

    [Fact]
    public void EmptyArguments_RaiseXpty0004()
    {
        var ex = Error40("fn:atomic-equal((), 'a')");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void MultiItemArguments_RaiseXpty0004()
    {
        var ex = Error40("fn:atomic-equal(('a', 'b'), 'a')");
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- parser companions exercised by the corpus ------------------------------

    [Fact]
    public void ArrowTarget_QuantifierKeywordsAsFunctionNames()
    {
        // atomic-equal-010 shape: => every() desugars to fn:every(.).
        Assert.Equal("true", Seq40(
            "(1 to 5) => every(fn($x) { $x ge 1 })")[0]);
        Assert.Equal("true", Seq40(
            "(1, 2, 3) => some(fn($x) { $x eq 2 })")[0]);
    }

    [Fact]
    public void IfExpr_BracedBranches()
    {
        Assert.Equal("yes", Seq40("if (1 eq 1) { 'yes' } else { 'no' }")[0]);
        Assert.Equal("no", Seq40("if (1 eq 2) { 'yes' } else { 'no' }")[0]);
    }

    [Fact]
    public void IfExpr_BracedThenWithoutElse_YieldsEmpty()
    {
        Assert.Empty(Seq40("if (1 eq 2) { 'x' }"));
        Assert.Equal("x", Seq40("if (1 eq 1) { 'x' }")[0]);
    }

    [Fact]
    public void IfExpr_ThenBranchWithoutElse_StillParseErrorIn40()
    {
        Assert.ThrowsAny<Exception>(() => Eval40("if (1 eq 1) then 'x'"));
    }
}
