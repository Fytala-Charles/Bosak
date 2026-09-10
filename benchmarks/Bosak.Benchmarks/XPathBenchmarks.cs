// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Benchmarks for XPath 3.1 compile and evaluation through the public API.
// SPECIAL NOTES        : Benchmark harness for the Bosak XPath 3.1 implementation; not shipped.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using BenchmarkDotNet.Attributes;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Benchmarks;

/// <summary>
/// Benchmarks for the XPath 3.1 front end: expression compilation and evaluation of a
/// compiled expression over the shared synthetic catalog document.
/// </summary>
[MemoryDiagnoser]
public class XPathBenchmarks
{
    /// <summary>A moderately complex XPath: path steps, stacked predicates and function calls.</summary>
    public const string ModeratelyComplexExpression =
        "/catalog/item[@price > 50 and @quantity > 10][@category = 'cat7']" +
        "/concat(@id, '|', @name, '|', format-number(number(@price), '#,##0.00'))";

    private const string PathHeavyExpression = "//item[@price > 50 and @quantity > 10]/description";
    private const string StringFunctionsExpression = "string-join(//item/upper-case(@name), ', ')";
    private const string FunctionHeavyExpression = "sum(for $i in 1 to 1000 return $i * 2)";

    private IXdmNode _doc = null!;
    private XPath31Expression _pathHeavy = null!;
    private XPath31Expression _stringFunctions = null!;
    private XPath31Expression _functionHeavy = null!;

    /// <summary>Generates the catalog document and compiles the reusable expressions once.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _doc = CatalogData.CreateDocument();
        _pathHeavy = XPath31Expression.Compile(PathHeavyExpression);
        _stringFunctions = XPath31Expression.Compile(StringFunctionsExpression);
        _functionHeavy = XPath31Expression.Compile(FunctionHeavyExpression);

        // Guard against silently measuring a broken workload.
        if (ResultCounter.CountNodes(_pathHeavy, _doc) == 0)
            throw new InvalidOperationException("Path-heavy benchmark matched no nodes.");
        if (_stringFunctions.Evaluate(_doc).ToString().Length == 0)
            throw new InvalidOperationException("String-functions benchmark produced an empty result.");
        if (_functionHeavy.Evaluate(_doc).ToString() != "1001000")
            throw new InvalidOperationException("Function-heavy benchmark returned an unexpected result.");
    }

    /// <summary>Compiles a moderately complex XPath expression (lex, parse, optimize, lower to IR).</summary>
    /// <returns>The compiled expression.</returns>
    [Benchmark]
    public XPath31Expression Compile_ModerateExpression() => XPath31Expression.Compile(ModeratelyComplexExpression);

    /// <summary>Evaluates a descendant-axis path with numeric predicates over the 2,000-item catalog.</summary>
    /// <returns>The number of matched description elements.</returns>
    [Benchmark]
    public int Evaluate_PathHeavy() => ResultCounter.CountNodes(_pathHeavy, _doc);

    /// <summary>Evaluates a string workload: upper-case over all item names, then string-join.</summary>
    /// <returns>The joined string length.</returns>
    [Benchmark]
    public int Evaluate_StringFunctions() => _stringFunctions.Evaluate(_doc).ToString().Length;

    /// <summary>Evaluates a pure function workload: sum over a 1-to-1000 for expression.</summary>
    /// <returns>The computed sum.</returns>
    [Benchmark]
    public XdmValue Evaluate_FunctionHeavy() => _functionHeavy.Evaluate(_doc);
}
