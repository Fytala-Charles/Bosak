// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Verifies primitive general comparisons and unchanged fallback behavior
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 14-11-2026     | Isolate allocation loops from the parallel test host.                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies primitive comparison semantics and allocation with unchanged fallback routes.</summary>
public class PrimitiveGeneralComparisonTests
{
    /// <summary>Checks six integer operators against explicit expectations and warmed allocation.</summary>
    [Fact]
    public void IntegerOperators_PreserveResultsWithoutTemporaryAllocations()
    {
        var context = new EvaluationContext();
        var left = XdmValue.FromInteger(1); var right = XdmValue.FromInteger(2);
        var operators = new[] { "=", "!=", "<", "<=", ">", ">=" };
        var expected = new[] { false, true, true, true, false, false };
        for (var op = 0; op < operators.Length; op++)
        {
            Assert.Equal(expected[op], VmEngine.ApplyBinaryOperator(operators[op], left, right, context).BooleanValue);

        }
        foreach (var bytes in AllocationProbeRunner.Run("comparisons").Values)
            Assert.Equal(0, bytes);
    }

    /// <summary>Checks numeric promotion, untyped coercion, NaN and type errors against the sequence path.</summary>
    [Fact]
    public void PrimitivePairs_MatchExistingSingletonSequencePath()
    {
        var context = new EvaluationContext();
        var values = new[] { XdmValue.FromInteger(2), XdmValue.FromDecimal(2.5m),
            XdmValue.FromDouble(double.NaN), XdmValue.FromDouble(double.PositiveInfinity),
            XdmValue.FromFloat(2.5f), XdmValue.FromBoolean(true), XdmValue.FromBoolean(false),
            XdmValue.FromString("2"), XdmValue.FromString("2", "untypedAtomic"),
            XdmValue.FromString("invalid", "untypedAtomic"), XdmValue.FromString("Alpha") };
        foreach (var op in new[] { "=", "!=", "<", "<=", ">", ">=" })
        foreach (var left in values)
        foreach (var right in values)
            Assert.Equal(Observe(op, Wrap(left), Wrap(right), context), Observe(op, left, right, context));
        FunctionLibrary.Populate(context);
        context.DefaultCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
        Assert.True(VmEngine.ApplyBinaryOperator("=", XdmValue.FromString("Alpha"), XdmValue.FromString("alpha"), context).BooleanValue);
        Assert.True(VmEngine.ApplyBinaryOperator("!=", XdmValue.FromDouble(double.NaN), XdmValue.FromDouble(double.NaN), context).BooleanValue);
        Assert.True(VmEngine.ApplyBinaryOperator("=", XdmValue.FromString("2", "untypedAtomic"), XdmValue.FromInteger(2), context).BooleanValue);
        Assert.Contains("FORG0001", Observe("=", XdmValue.FromString("invalid", "untypedAtomic"), XdmValue.FromInteger(2), context));
        Assert.Contains("XPTY0004", Observe("=", XdmValue.FromString("2"), XdmValue.FromInteger(2), context));
    }

    /// <summary>Checks empty, array, multi-item and backwards-compatible comparison fallbacks.</summary>
    [Fact]
    public void ExcludedOperands_KeepExistingSemantics()
    {
        var context = new EvaluationContext(); var one = XdmValue.FromInteger(1);
        Assert.False(VmEngine.ApplyBinaryOperator("=", XdmValue.Undefined, one, context).BooleanValue);
        Assert.False(VmEngine.ApplyBinaryOperator("!=", XdmValue.FromSequence(XdmSequence.Empty), one, context).BooleanValue);
        var array = XdmValue.FromArray(new XdmArray(new[] { one, XdmValue.FromInteger(2) }));
        Assert.True(VmEngine.ApplyBinaryOperator("=", array, XdmValue.FromInteger(2), context).BooleanValue);
        var many = XdmValue.FromSequence(MaterializedSequence.FromArray(one, XdmValue.FromInteger(2)));
        Assert.True(VmEngine.ApplyBinaryOperator("=", many, one, context).BooleanValue);
        Assert.True(VmEngine.ApplyBinaryOperator("!=", many, one, context).BooleanValue);
        context.BackwardsCompatible = true;
        Assert.True(VmEngine.ApplyBinaryOperator("=", XdmValue.FromString("2"), XdmValue.FromInteger(2), context).BooleanValue);
        Assert.True(VmEngine.ApplyBinaryOperator("=", XdmValue.FromBoolean(true), XdmValue.FromInteger(2), context).BooleanValue);
        Assert.True(VmEngine.ApplyBinaryOperator("<", XdmValue.FromString("2"), XdmValue.FromString("10"), context).BooleanValue);
    }

    private static XdmValue Wrap(XdmValue value) => XdmValue.FromSequence(MaterializedSequence.FromArray(value));

    /// <summary>Checks lazy comparison early success, fault ordering and repeat enumeration.</summary>
    [Fact]
    public void LazyOperand_StopsAtMatchAndPreservesLaterFault()
    {
        var moves = 0;
        IEnumerable<XdmValue> Items()
        {
            moves++;
            yield return XdmValue.FromInteger(1);
            moves++;
            throw new InvalidOperationException("probe-lazy-fault");
        }
        var lazy = XdmValue.FromSequence(XdmSequence.FromSource(new EnumerableXdmSequence(Items())));
        var context = new EvaluationContext();
        Assert.True(VmEngine.ApplyBinaryOperator("=", lazy, XdmValue.FromInteger(1), context).BooleanValue);
        Assert.Equal(1, moves);
        Assert.Equal("probe-lazy-fault", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyBinaryOperator("=", lazy, XdmValue.FromInteger(2), context)).Message);
        Assert.Equal(3, moves);
    }

    private static string Observe(string op, XdmValue left, XdmValue right, EvaluationContext context)
    {
        try { return VmEngine.ApplyBinaryOperator(op, left, right, context).BooleanValue.ToString(); }
        catch (InvalidOperationException error) { return error.Message; }
    }
}
