# Bosak Benchmarks

BenchmarkDotNet harness for the Bosak XPath 3.1 / XSLT 3.0 / XQuery 3.1 engine.
This project is deliberately **not** part of `Bosak.sln` so that CI stays fast.

All benchmarks share a synthetic catalog document (~2,000 `item` elements with
id/name/price/quantity/category attributes and description/rating children) that is
generated in memory during `GlobalSetup` — no external files are needed and document
construction is never part of the measured workload.

## Benchmarks

| Benchmark | What it measures |
|---|---|
| `XPathBenchmarks.Compile_ModerateExpression` | XPath compile (lex, parse, optimize, lower to IR) of a path + predicates + function calls expression |
| `XPathBenchmarks.Evaluate_PathHeavy` | `//item[@price > 50 and @quantity > 10]/description` over the catalog |
| `XPathBenchmarks.Evaluate_StringFunctions` | `string-join(//item/upper-case(@name), ', ')` over the catalog |
| `XPathBenchmarks.Evaluate_FunctionHeavy` | `sum(for $i in 1 to 1000 return $i * 2)` — pure function/FLWOR arithmetic |
| `XQueryBenchmarks.Compile_Flwor` | XQuery compile of a for/where/order by FLWOR |
| `XQueryBenchmarks.Evaluate_Flwor` | FLWOR evaluation over the catalog |
| `XsltBenchmarks.Compile_Stylesheet` | XSLT compile of an HTML-table stylesheet |
| `XsltBenchmarks.Transform_HtmlTable` | Transform + serialize the catalog to an HTML table (for-each, sort, format-number) |

Every class runs with BenchmarkDotNet's `MemoryDiagnoser`, so the report includes
per-iteration managed allocations — allocation is a key optimization concern.

## Running

From the repository root:

```bash
# Full run (all benchmarks, default BenchmarkDotNet job — takes tens of minutes)
dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*'

# Quick sanity pass (single iteration per benchmark)
dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*' --job dry

# Faster baseline (~minutes), noisier timings; allocations stay exact
dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*' --job short

# A single benchmark
dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*Transform_HtmlTable*'
```

Results are written to `benchmarks/Bosak.Benchmarks/BenchmarkDotNet.Artifacts/results/`
(Markdown, CSV and HTML reports).

Note: always run in `-c Release`. BenchmarkDotNet refuses to benchmark a Debug build
of the benchmark host.

## Baseline results (2026-09-09, .NET 10.0.11, Windows 11 x64, AVX2)

First optimization wave (wrapper cache, lazy axes, standard-function table clone):
full raw output in `baseline-0.10.0.txt`.

| Benchmark | Baseline | After wave 1 | Δ time | Δ allocated |
|---|---|---|---|---|
| Compile_ModerateExpression | 4.85 µs / 18.5 KB | 5.25 µs / 18.5 KB | — | — |
| Evaluate_PathHeavy | 32.77 ms / 56.05 MB | 22.08 ms / 30.96 MB | −33% | −45% |
| Evaluate_StringFunctions | 30.01 ms / 40.71 MB | 15.58 ms / 19.87 MB | −48% | −51% |
| Evaluate_FunctionHeavy | 299.4 µs / 420 KB | 306.2 µs / 405 KB | — | −4% |
| Compile_Flwor | 9.57 µs / 36.5 KB | 11.70 µs / 36.5 KB | — | — |
| Evaluate_Flwor | 44.23 ms / 66.75 MB | 27.61 ms / 36.66 MB | −38% | −45% |
| Compile_Stylesheet | 35.69 µs / 80.2 KB | 37.14 µs / 80.2 KB | — | — |
| Transform_HtmlTable | 193.34 ms / 115.48 MB | 198.37 ms / 103.01 MB | — | −11% |

Re-run with: `dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*'`
