<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Unix-time wide integer calendar correction">
  <h1>Unix-time wide integer calendar boundary</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

This bounded REQ-126 correction follows the [arithmetic fix](INTEGER-PROMOTION.md), merged as PR437 at `fa5c7ec0fb8d1f761e6080a3abd2399a2a21db65` (engine-identical reviewed head `f817b9b3c707cc17da926d0dfefb88a18ec5e7d9`). It corrects valid integer operands at the Unix-time boundary, without changing generic integer constructors or casting. Source remains the existing unpublished1.25.1 patch; public/Braid1.25.0 remains consumer-accepted within REQ-137's delivered scope.

## Behavior

`unix-dateTime(9223372036854775808)` and the adjacent integer now raise **FODT0001** in both explicit4.0 profiles, because those valid integers exceed the supported calendar. They previously failed Int64 narrowing with FORG0001. This is a range-error correction within the existing nonnegative frozen contract, not adoption of the newer signed domain by Frozen.

Decimal-backed values recognized by the runtime's integer-type predicate now receive the same domain and calendar checks as integer-kind operands before conversion to Int64. The lower and upper supported UTC millisecond bounds are **-62135596800000** and **253402300799999** (years1–9999). Frozen checks its nonnegative domain first, so valid negative integer operands still raise FOCA0002; Experimental permits signed epochs and reports FODT0001 beyond the calendar.

Values inside the calendar are safe to narrow and use the existing UTC date construction. A wide integer computation that returns into range, such as `(9223372036854775807 + 1) - 9223372036854775807`, still yields the exact one-millisecond stamp. The same handling covers decimal-backed `xs:unsignedLong` and invocation through references, lookup, partial application and variables. Both Legacy and Xslt40Draft hosts report positive wide integer calendar overflow while retaining frozen negative refusal.

## Explicit limits

Generic `xs:integer('9223372036854775808')` still fails before function invocation. An untyped lexical wide value still follows the existing cast path. Genuine malformed lexical input retains FORG0001. Ordinary decimal operands without integer identity follow the existing conversion path; full function conversion/cardinality, generic storage/constructor/cast support, unary behavior and arbitrary precision remain separate work. This correction neither catches nor rewrites all FORG0001 errors.

The backing representation and calendar are not extended. No new public API, compatibility enum, function signature metadata, fixture, gate or retry policy is introduced. Source version stays1.25.1 because this is another correction in the same unpublished patch. No release or upstream contribution.

## Verification and disposition

Adds32 API and two XSLT host cases. Before the correction,20 API cases failed with12 controls passing. All136 Unix-time API tests pass after the correction, including updated historical wide-error checks now requiring FODT0001. Tests distinguish valid integer overflow, frozen/Experimental negative domains, constructor/lexical controls, range re-entry and host dispatch. The full run and fresh gates are recorded in [machine-readable evidence](UNIX-DATETIME-WIDE-RANGE.json); [raw receipts](evidence/unix-datetime-wide-range/) retain the56-record post-fix diagnostic comparison and gate output.

Release build succeeded with zero errors and the existing CS8604 warning. All **11,423 solution plus84 language-server tests pass (11,507 total)**. Fresh QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across224 configured/executed sets. Skips and existing Experimental retries do not certify full conformance. Ten documentation pages rendered to HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

The current native Unix set improves from20/4/0 to **22/2/0, exit2**. Cases114/115 now meet their existing FODT0001 alternative. The remaining120/121 failures are the newer negative-domain tests evaluated by the frozen runner; no fallback or fixture change masks them. Direct Experimental tests cover signed behavior separately. This is not full current-draft conformance.

Catalog inventory remains76 entries,3,148 feature plus24 chapter checks,224 configured gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. Next: review this bounded correction, then audit generic integer constructor/cast range and error policy before a wider VM change. Braid's accepted1.25.0 dependency remains independent.
