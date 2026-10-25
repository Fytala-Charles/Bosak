<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala division and modulo arithmetic investigation">
  <h1>Division and modulo boundary audit</h1>
  <p>25 October 2026 — source 1.25.2 unpublished; public 1.25.1</p>
</div>

**Integer division precision audit — 25 October 2026:** [Evidence and proposed scope](IDIV-PRECISION-AUDIT.md) add 48 result-range controls and 96 exact-rational comparisons, exposing 48 in-range one-unit discrepancies while documenting precision qualifications. Recommend exact truncation for existing decimal-backed operands with the current Int64 result bounds preserved. Engine/version unchanged; source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


**Integer modulo correction — 25 October 2026:** [Behavior and evidence](INTEGER-MODULO.md) return integer zero for Int64 minimum modulo -1 and preserve integer identity for decimal-backed remainders. Adds 84 direct/runtime regression cases (45 failed before, all pass after). Genuine decimal/floating policies and `idiv` remain unchanged. Source 1.25.2 unpublished/public 1.25.1; REQ-126 stays partial.


This REQ-126 evidence slice follows [integer unary negation](INTEGER-UNARY.md), merged as PR445 at `74472aa8cf904c4b4d44814df8cdbb53a095737f`. It adds 72 supported controls and 84 observed diagnostics across XPath31, frozen XPath40 and XPath40Experimental. No engine, API, version, fixture, gate, retry policy or release change is introduced. Public 1.25.1 and Braid smoke/adoption remain separate.

## Supported controls

The 24 cases per profile cover signed truncation toward zero for `idiv`, remainder sign for ordinary integer operands, genuine decimal/mixed numeric results, `div` returning decimal for integer operands, supported wide operands with in-range quotients, integer division by infinity and error precedence. Zero divisors require FOAR0001, including infinity `idiv` zero; NaN and infinite dividends with nonzero divisors require FOAR0002. Value cases verify exact text and integer identity. All 72 controls pass on the unchanged engine.

The separate diagnostic probe records 28 expressions per profile without normative pass/fail assertions. It includes known failing shapes; its completion is not a conformance pass. The probe first ran against PR445 head `9fe05b8dfa315f4d13116323a68b4992575b4a95`; the merged tree is identical. A second run uses this audit branch at the merge base. Original receipts are retained.

## Observed findings and disposition

| Shape | Observed behavior | Disposition |
|---|---|---|
| `xs:integer('-9223372036854775808') mod -1` | FOAR0002 from Int64 remainder overflow | Bounded modulo correction recommended; the mathematical remainder is representable zero |
| `-9223372036854775808 mod -1` | zero with decimal identity | Same modulo correction must cover literal and runtime paths |
| `9223372036854775808 mod 3` | correct magnitude 2, decimal identity | Preserve integer identity when both operands are integers; mixed genuine decimal results must remain decimal |
| `9223372036854775808 idiv 1` | FOAR0002 on Int64 result narrowing | Separate quotient representation/range policy; do not silently broaden in a remainder fix |
| `xs:integer('-9223372036854775808') idiv -1` | FOAR0002 | Same separate quotient range question |
| `79228162514264337593543950334 idiv 79228162514264337593543950335` | 1 after decimal division rounds near one; mathematical truncated quotient is 0 | Precision-sensitive integer/decimal quotient investigation; do not claim an exact quotient contract from this observation |

The [F&O 3.1 modulo contract](https://www.w3.org/TR/xpath-functions-31/#func-numeric-mod) explicitly covers minimum-magnitude integers divided by -1, with remainder zero. Its [arithmetic result-type rules](https://www.w3.org/TR/xpath-functions-31/#op.numeric) distinguish integer and decimal operands. The [integer division rules](https://www.w3.org/TR/xpath-functions-31/#func-numeric-integer-divide) include implementation precision/overflow qualifications; the observed rounding and result-range findings need a separate, explicit bounded policy. No claim is made that every diagnostic is a specification violation.

## Validation and remaining work

Release build succeeds with zero errors and the existing CS8604 warning. All **11,795 solution +84 language-server tests pass (11,879 total)**; focused controls are **72/0/0**. The 84-record diagnostic rerun exactly matches the original observations. Eleven documentation pages rendered as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance claimed.

[Machine-readable proof](DIVISION-MODULO-AUDIT.json) and [raw diagnostic/test receipts](evidence/division-modulo-audit/) preserve observations, source identity and test counters/hashes. This test-only slice inherits the exact engine and manual-gate evidence of PR445: QT3 31,142/0/679 and frozen qt4 8,436/0/1,894 across 224 configured/executed sets, with 11 existing Experimental fallback passes. The current native Unix set remains 22/2/0 under frozen evaluation. These manual gates are not rerun by this audit; the exact unchanged engine comparison is recorded separately. Skips/retries and diagnostic completion do not certify conformance.

REQ-126/D2/D3/D4/AC-03 remain partial. Catalog totals stay 76 entries, 3,148 feature plus 24 chapter checks, 224 gates and 28 unavailable historical adopted pins; the 72 controls are outside catalog totals. Generic constructor/storage/error policy, decimal precision and double-negation/coercion remain separate. REQ-138 and Braid authoring are outside this branch.

Next: review the audit, then correct modulo minimum/-1 and decimal-backed integer result identity together in a bounded engine slice. Follow that with a separate `idiv` precision/range contract investigation. No publication is performed here.
