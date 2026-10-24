<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala wide-integer Unix-time investigation">
  <h1>Wide-integer Unix-time investigation</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 consumer-accepted</p>
</div>

**Checked integer arithmetic — 24 October 2026:** [Correction and limits](INTEGER-PROMOTION.md) prevent folded binary arithmetic wraparound and preserve integer identity through decimal-backed addition/subtraction/multiplication, with42 profile/call-path regression cases. General casts, unary/division and Unix-time wide conversion remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 stays partial.



This REQ-126 investigation follows the [signed-epoch correction](UNIX-DATETIME-EXPERIMENTAL.md). It adds 24 boundary tests and preserves 74 observed diagnostics, without changing engine behavior. Base: `2580823a057afe68d3555904a9f551b5264d5277`, after PR434 and Braid acceptance PR435. Public 1.25.0 is accepted by Braid within REQ-137's delivered scope; source 1.25.1 remains unpublished.

## Conversion failure is separate from calendar range

The refreshed upstream default pins remain [qtspecs b4723ff5](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml) and [qt4tests 3a2640cf](https://github.com/qt4cg/qt4tests/blob/3a2640cf649a9bff6c7a5b257a1fe7e50450b2c2/fn/unix-dateTime.xml). Review pins do not recover historical adopted specification snapshots. Current native cases114/115 permit their exact extended-year date or FODT0001/FOAR0002; Bosak reports FORG0001. The retained primary-function excerpt defines UTC milliseconds with optional integer input; it does not justify treating valid numeric overflow as malformed lexical input.

| Input | Observed in both explicit profiles |
|---|---|
| `9223372036854775808 instance of xs:integer` | `true` |
| `9223372036854775808 eq 9223372036854775809` | `false`; adjacent values remain distinct |
| `unix-dateTime(253402300799999)` | `9999-12-31T23:59:59.999Z` |
| `unix-dateTime(253402300800000)` or `unix-dateTime(9223372036854775807)` | FODT0001, supported calendar exceeded |
| `unix-dateTime(9223372036854775808)` | FORG0001 before calendar arithmetic |
| `xs:integer('9223372036854775808')` | FORG0001 before the function can be invoked |

The parser's `IntegerLiteralFromText` preserves an Int64-exceeding decimal-representable literal using decimal backing with an integer-literal tag. `UnixDateTime` fast-paths Integer-kind values, then calls `VmEngine.Cast` for other kinds. `TryCast`'s generic integer-family Decimal branch narrows to `long`; the overflow handler in `Cast` maps that overflow to FORG0001. String/untyped input instead fails `long.TryParse` and is classified as lexical failure. Direct, reference, lookup, partial and variable calls show the same boundary. These are observed implementation paths, not a conformance approval of the error.

This distinguishes three issues: bounded calendar range, integer conversion/storage, and full function conversion. A blanket `catch FORG0001 => FODT0001` would also rewrite genuinely malformed lexical input and would miss failures in an argument constructor. It is unsuitable.

## Adjacent arithmetic defect

The separate 18-record arithmetic probe distinguishes constant folding from runtime evaluation in both profiles:

| Expression | Observed result |
|---|---|
| `9223372036854775807 + 1` | `-9223372036854775808` |
| `xs:integer('9223372036854775807') + 1` | `9223372036854775808`, but `instance of xs:integer` is false |
| `9223372036854775807 * 2` | `-2` |
| `xs:integer('9223372036854775807') * 2` | `18446744073709551614` |
| `-9223372036854775807 - 2` | `9223372036854775807` |
| `xs:integer('-9223372036854775807') - 2` | `-9223372036854775809` |

`XPathOptimizer.OptimizeBinary` performs unchecked integer constant arithmetic. Runtime `MultiplyOrAddInteger` uses checked arithmetic and decimal promotion, which preserves these magnitudes but does not retain the integer subtype annotation. This is an independently reproduced defect, not expected behavior encoded in the new regression suite. It also changes the error path of `unix-dateTime(max + 1)`. Broader unary/subtraction/division edge behavior is not certified by this probe.

## Tests and disposition

The 24 new source tests cover wide-literal type identity, distinct/exact values, four long-max invocation paths, the exact upper calendar boundary, genuine lexical refusal and NaN controls in both explicit profiles. They do not require observed wraparound or the disputed wide-integer error. Existing historical observation tests remain explicitly identified.

Fresh native Unix sets: frozen **16/0/0 exit0**, current **20/4/0 exit2**. The runner evaluates frozen compatibility; the negative-domain failures remain alongside the two wide-integer failures. No fixture, gate or retry-policy change. Full manual QT3/qt4 gates were not rerun for this tests/docs-only slice; the engine-identical [preceding implementation receipts](UNIX-DATETIME-EXPERIMENTAL.json) retain31,142/0/679 and8,434/0/1,896 respectively. Neither skips nor retries imply full conformance.

Release build passed with zero errors and the existing CS8604 warning. All **11,347 solution plus84 language-server tests pass (11,431 total)**. Eight documentation pages rendered to HTML with unchanged Fytala CSS; this is not visual/PDF acceptance.

See [machine-readable evidence](UNIX-DATETIME-WIDE-INTEGER.json) and [retained diagnostic/native receipts](evidence/unix-datetime-wide/). Catalog totals remain76 entries,3,148 feature plus24 chapter checks,224 configured gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. No engine/API/version/release change or new upstream contribution.

Recommended next implementation: first correct checked integer constant folding and preserve integer type through runtime promotion, with optimized/runtime equivalence tests and both frozen gates. Then address the generic integer conversion/storage contract with separate lexical, numeric overflow and constructor cases; assess an Experimental-only Unix calendar guard only after those prerequisites. A local Unix-time exception rewrite would leave the underlying arithmetic and constructor defects unresolved.
