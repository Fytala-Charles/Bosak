<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.2 candidate release notes">
  <h1>Bosak 1.25.2 candidate release notes</h1>
  <p>25 October 2026 — unsigned local candidate; unpublished</p>
</div>

Bosak 1.25.2 consolidates the bounded unary, modulo and integer-division corrections since public 1.25.1. No new public API is introduced. Installation pins remain 1.25.1 pending separately authorized publication.

- Unary negation promotes Int64 minimum safely and retains integer identity for decimal-backed integer values in literal/runtime paths. Genuine decimal values retain decimal identity.
- Integer modulo returns zero for Int64 minimum modulo -1 and retains integer identity when both operands are integers, including wide decimal backing. Genuine decimal/floating policies remain intact.
- Decimal-backed `idiv` truncates the exact signed coefficient/scale quotient before enforcing the existing Int64 result bounds. Zero divisors retain FOAR0001 and oversized results retain FOAR0002. Decimal-backed overflow messages become consistent; error code and exception type remain intact.

The candidate adds 384 source cases (48 unary, 72 division/modulo controls, 84 modulo, 48 idiv range controls and 132 exact-idiv regressions). The earlier minimum-literal defect-pinning test now verifies corrected integer identity and frozen negative-domain/Experimental calendar errors. Fresh validation passes 12,059 solution +84 LSP tests; QT3 stays 31,142/0/679 and frozen qt4 8,436/0/1,894 across 224 unchanged gates. Current native Unix remains 22/2/0 under frozen evaluation; no full current-draft conformance is claimed.

Ten unsigned local packages pass 153 broad +1,429 focused package-only checks with exact revision/assembly/archive/cache/output provenance and clear transitive audits. Counts overlap source tests. XSLT authoring source is unchanged from 1.25.1. REQ-138 design is separate from this candidate, and Braid's independent 1.25.1 adoption does not establish acceptance of 1.25.2.

Int64 idiv result bounds, generic wide constructors/storage/error policy, ordinary decimal precision, double-negation/coercion and bounded Unix calendar remain explicit limits. REQ-126 remains partial. [Pinned candidate and complete proof](RELEASE-1.25.2-CANDIDATE.md). No tag, public package upload or release has been created.
