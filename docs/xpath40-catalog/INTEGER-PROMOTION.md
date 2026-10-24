<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala checked integer arithmetic correction">
  <h1>Checked integer arithmetic and promotion</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

This bounded REQ-126 correction follows the [wide-integer investigation](UNIX-DATETIME-WIDE-INTEGER.md), merged as PR436 at base `2ff6804ad4a7236b7f8980f1ca76294810043e00`. It fixes binary integer addition, subtraction and multiplication consistency across constant folding and runtime promotion. It applies across modern profiles as an arithmetic bug fix, without adopting additional newer-draft function contracts. Source remains the existing unpublished patch1.25.1; public/Braid1.25.0 is unchanged and consumer-accepted within REQ-137's delivered profile.

## Behavior and implementation

| Expression | Before | After |
|---|---|---|
| `9223372036854775807 + 1` | wrapped negative minimum | `9223372036854775808`, integer |
| `9223372036854775807 * 2` | `-2` | `18446744073709551614`, integer |
| `-9223372036854775807 - 2` | wrapped positive maximum | `-9223372036854775809`, integer |
| runtime `xs:integer('9223372036854775807') + 1` | exact magnitude, decimal type | same magnitude, integer type |
| runtime `0 - xs:integer('-9223372036854775808')` | negation overflow could wrap | `9223372036854775808`, integer |

`XPathOptimizer.OptimizeBinary` uses checked Int64 arithmetic and leaves overflowing operations for runtime evaluation. Decimal-backed literal folding retains the integer tag only when both operands are integer literals. Runtime addition/subtraction/multiplication share checked integer arithmetic, and promote an Int64 overflow to decimal backing tagged as `integer`. Subtraction evaluates `a-b` directly rather than negating `b`, avoiding the minimum-value negation trap.

Existing decimal-backed arithmetic also preserves the result's integer type when both operands have integer identity. Mixed integer/decimal arithmetic remains decimal, even for an integral result. Float/double and backwards-compatible dispatch paths are unchanged. No new public API or arbitrary-precision representation is introduced.

## Scope and remaining limits

The backing store remains .NET decimal with its existing range and precision limits. This is not an arbitrary-precision integer implementation or certification of every integer operation. Unary negation, division/idiv/mod, constructor/cast narrowing and other integer-consuming functions remain separate audit work. In particular, direct wide literals still fail Unix-time Int64 conversion with FORG0001; the current native Unix set remains20/4/0 under frozen evaluation. The correction prevents prior constant-folding wraparound from selecting a misleading negative-domain error path; it does not repair the remaining Unix conversion contract.

Frozen Unix negative refusal and Experimental signed epochs remain distinct. No fixture, gate membership, retry policy, package publication or upstream contribution changes.

## Verification

The 42-case suite spans XPath31, XPath40 and XPath40Experimental. Each case compares a folding-eligible expression with an equivalent runtime expression and checks exact magnitude plus integer identity. Cases include Int64 bounds, multiplication, minimum-value subtraction, wide/wide operations, chained promotion, a return into Int64 range and genuine decimal controls.

Before the correction,30 cases failed and12 controls passed. The first after-run passed39 and failed3 because a test wrote `$a-$b`, parsed as a hyphenated variable name; adding whitespace produced42/0/0. Original runs remain retained; no expected result was weakened. A separate18-record post-fix diagnostic probe retains comparisons with the earlier investigation.

Release build succeeded with zero errors; existing CS8604 and NU1900 vulnerability-metadata availability warnings remain. All **11,389 solution plus84 language-server tests pass (11,473 total)**. Fresh QT3 is **31,142/0/679**. Frozen qt4 is **8,436/0/1,894**, with224 configured/executed sets. The two former compile-overflow skips, `op-numeric-integer-divide-3` and `op-numeric-mod-6`, now reach runtime FOAR0002 accepted by their unchanged assertions. The initial verification script expected prior totals and rejected this improvement; original raw logs were reconciled without rewriting fixtures or assertions. Current native Unix remains20/4/0 under frozen evaluation. Ten documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

Final build, complete test totals and fresh QT3/frozen qt4 results are recorded in [machine-readable evidence](INTEGER-PROMOTION.json). Raw gate/probe receipts are [retained separately](evidence/integer-promotion/). Skips and existing Experimental retries do not certify full conformance.

Catalog totals stay76 entries,3,148 feature plus24 chapter checks,224 configured gates and28 unavailable historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. Next: review the bounded arithmetic fix, then address wide-integer conversion/storage with separate constructor, lexical and numeric-overflow evidence. No release performed.
