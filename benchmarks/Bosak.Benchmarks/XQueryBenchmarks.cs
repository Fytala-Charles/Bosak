// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Benchmarks for XQuery 3.1 compilation and FLWOR evaluation.
// SPECIAL NOTES        : Benchmark harness for the Bosak XQuery 3.1 implementation; not shipped.
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
using Bosak.XPath.Core.Xdm;
using Bosak.XQuery.Api;

namespace Bosak.Benchmarks;

/// <summary>
/// Benchmarks for the XQuery 3.1 front end: query compilation and a FLWOR
/// (for/where/order by/return) evaluation over the shared synthetic catalog document.
/// </summary>
[MemoryDiagnoser]
public class XQueryBenchmarks
{
    /// <summary>A FLWOR query with for, where and order by clauses over the catalog items.</summary>
    public const string FlworQuery =
        "for $i in //item " +
        "where number($i/@price) > 100 and number($i/@quantity) > 5 " +
        "order by number($i/@price) descending, $i/@id " +
        "return concat($i/@id, ':', $i/@name, ' = ', string($i/@price))";

    private IXdmNode _doc = null!;
    private XQueryExecutable _flwor = null!;

    /// <summary>Generates the catalog document and compiles the FLWOR query once.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _doc = CatalogData.CreateDocument();
        _flwor = new XQueryCompiler().Compile(FlworQuery);

        if (EvaluateFlwor() == 0)
            throw new InvalidOperationException("FLWOR benchmark matched no items.");
    }

    /// <summary>Compiles the FLWOR query (parse, static context, optimize, lower to IR).</summary>
    /// <returns>The compiled query.</returns>
    [Benchmark]
    public XQueryExecutable Compile_Flwor() => new XQueryCompiler().Compile(FlworQuery);

    /// <summary>Evaluates the FLWOR query against the 2,000-item catalog.</summary>
    /// <returns>The number of result items.</returns>
    [Benchmark]
    public int Evaluate_Flwor() => EvaluateFlwor();

    private int EvaluateFlwor()
    {
        var context = new XQueryContext().WithContextItem(XdmValue.FromNode(_doc));
        return ResultCounter.CountItems(_flwor.Evaluate(context));
    }
}
