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

Waves 1–4 (wrapper cache, lazy axes, function-table clone; per-instruction compiled-XPath
cache, engine Populate skip, static LRE namespace-info cache; serializer span escaping +
copy-on-write namespace bindings; result-tree append fast paths): raw output in `baseline-0.10.0.txt`.
Wave 5 (lazy node-test filtering, copy-free predicate-path views, pooled Filter kept-buffer,
ordered-sequence fast path in document-order normalization) and wave 6 (FLWOR tuple
materialization: sort keys atomized once per tuple, copy-free tuple item views, array-tuple
reuse in the sorted stream) — latest numbers below.

| Benchmark | Baseline | After waves 1–6 | Δ time | Δ allocated |
|---|---|---|---|---|
| Compile_ModerateExpression | 4.85 µs / 18.5 KB | 5.25 µs / 18.5 KB | — | — |
| Evaluate_PathHeavy | 32.77 ms / 56.05 MB | 16.10 ms / 21.18 MB | −51% | −62% |
| Evaluate_StringFunctions | 30.01 ms / 40.71 MB | 10.43 ms / 10.06 MB | −65% | −75% |
| Evaluate_FunctionHeavy | 299.4 µs / 420 KB | 312.9 µs / 366 KB | — | −13% |
| Compile_Flwor | 9.57 µs / 36.5 KB | 10.74 µs / 36.5 KB | — | — |
| Evaluate_Flwor | 44.23 ms / 66.75 MB | 21.49 ms / 24.24 MB | −51% | −64% |
| Compile_Stylesheet | 35.69 µs / 80.2 KB | 38.07 µs / 80.2 KB | — | — |
| Transform_HtmlTable | 193.34 ms / 115.48 MB | 45.92 ms / 43.23 MB | −76% | −63% |

Re-run with: `dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*'`
