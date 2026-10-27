// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies scalar XML name validation through public map-to-element evaluation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies scalar XML name validation through public map-to-element evaluation.</summary>
public class ElementMapNameValidationTests
{
    /// <summary>XML NameStartChar range endpoints excluding the NCName colon.</summary>
    /// <returns>Independent boundary examples from the XML recommendation.</returns>
    public static IEnumerable<object[]> ValidStarts()
    {
        int[] points = [0x41,0x5a,0x5f,0x61,0x7a,0xc0,0xd6,0xd8,0xf6,0xf8,0x2ff,0x370,0x37d,0x37f,0x1fff,0x200c,0x200d,0x2070,0x218f,0x2c00,0x2fef,0x3001,0xd7ff,0xf900,0xfdcf,0xfdf0,0xfffd,0x10000,0x1d11e,0xeffff];
        foreach (int point in points) yield return [char.ConvertFromUtf32(point)];
    }

    /// <summary>Additional NameChar values that are accepted after the first character.</summary>
    /// <returns>Continuation-only boundary names.</returns>
    public static IEnumerable<object[]> ValidContinuations()
    {
        int[] points = [0x2d,0x2e,0x30,0x39,0xb7,0x300,0x36f,0x203f,0x2040];
        foreach (int point in points) yield return ["A" + char.ConvertFromUtf32(point)];
    }

    /// <summary>Scalar gaps and supplementary values outside the permitted name range.</summary>
    /// <returns>Invalid XML NCName starts.</returns>
    public static IEnumerable<object[]> InvalidStarts()
    {
        int[] points = [0x3a,0xb7,0x300,0x36f,0x37e,0x200b,0x200e,0x206f,0x2190,0x2bff,0x2ff0,0x3000,0xe000,0xf8ff,0xfdd0,0xfdef,0xfffe,0xffff,0xf0000,0x10ffff];
        foreach (int point in points) yield return [char.ConvertFromUtf32(point)];
    }

    /// <summary>Valid starts pass parsing even when the provider cannot store the character yet.</summary>
    /// <param name="name">The valid name to parse.</param>
    [Theory, MemberData(nameof(ValidStarts))]
    public void ValidStart_ParsesEmptyAttribute(string name) => Assert.Equal("r", EmptyAttribute(name));

    /// <summary>Continuation-only scalars are accepted after a valid start.</summary>
    /// <param name="name">The valid compound name.</param>
    [Theory, MemberData(nameof(ValidContinuations))]
    public void ValidContinuation_ParsesEmptyAttribute(string name) => Assert.Equal("r", EmptyAttribute(name));

    /// <summary>Out-of-range starts report the map name error before provider construction.</summary>
    /// <param name="name">The invalid scalar name.</param>
    [Theory, MemberData(nameof(InvalidStarts))]
    public void InvalidStart_ReportsFojs0009(string name) => Invalid(name);

    /// <summary>Ill-formed UTF-16 is rejected without replacement or normalization.</summary>
    /// <param name="index">The malformed code-unit arrangement.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void MalformedUtf16_ReportsFojs0009(int index)
    {
        string high = new((char)0xd800, 1), low = new((char)0xdc00, 1);
        string[] names = [high, low, high + "A", "A" + high, high + high, low + high];
        Invalid(names[index]);
    }

    /// <summary>Ordinary invalid names retain their existing error classification.</summary>
    /// <param name="name">The invalid NCName.</param>
    [Theory]
    [InlineData("")]
    [InlineData("1bad")]
    [InlineData("-bad")]
    [InlineData(".bad")]
    [InlineData("a b")]
    [InlineData("a:b")]
    [InlineData("a:b:c")]
    public void InvalidLexicalName_ReportsFojs0009(string name) => Invalid(name);

    /// <summary>Root, child and attribute paths reject out-of-name-range supplementary scalars.</summary>
    /// <param name="path">The map construction expression.</param>
    [Theory]
    [InlineData("map-to-element(map{$n: 'x'})")]
    [InlineData("map-to-element(map{'r': map{$n: 'x'}})")]
    [InlineData("map-to-element(map{'r': map{'@' || $n: 'x'}})")]
    public void InvalidSupplementary_AllEntryPathsReportFojs0009(string path)
    {
        var context = new EvaluationContext().WithVariable("n", XdmValue.FromString(char.ConvertFromUtf32(0xf0000)));
        Assert.StartsWith("FOJS0009:", Assert.Throws<InvalidOperationException>(() => Run(path, context)).Message);
    }

    /// <summary>Lexical prefixes obey the same scalar validity rules without constructing omitted attributes.</summary>
    /// <param name="prefix">A valid lexical prefix.</param>
    [Theory]
    [InlineData("p")]
    [InlineData("‌")]
    public void ValidPrefix_ResolvesNamespace(string prefix)
    {
        var context = new EvaluationContext().WithNamespace(prefix, "urn:k03").WithVariable("n", XdmValue.FromString(prefix + ":a"));
        Assert.Equal("r", Run("name(map-to-element(map{'r': map{'@' || $n: ()}}, map{'name-format': 'lexical'}))", context).ToString());
    }

    /// <summary>Ordinary valid names still construct and serialize normally.</summary>
    [Fact]
    public void OrdinaryName_ConstructsNormally()
        => Assert.Equal("<root>x</root>", Run("serialize(map-to-element(map{'root': 'x'}))", new EvaluationContext()).ToString());

    private static string EmptyAttribute(string name)
        => Run("name(map-to-element(map{'r': map{'@' || $n: ()}}))", new EvaluationContext().WithVariable("n", XdmValue.FromString(name))).ToString();

    private static void Invalid(string name)
        => Assert.StartsWith("FOJS0009:", Assert.Throws<InvalidOperationException>(() => EmptyAttribute(name)).Message);

    private static XdmValue Run(string expression, EvaluationContext context)
        => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(context);
}
