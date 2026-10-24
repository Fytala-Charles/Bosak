<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala exact floating-point integer boundaries">
  <h1>Exact floating-point integer boundaries</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

This bounded REQ-126 fix follows the [constructor/cast audit](INTEGER-CAST-AUDIT.md), reviewed at `8fb2efc35ee7bd05b446550d59d5bb608317f9a2` (PR439 merged at `2437a9e1cb77ea7989f01373326532483332ed43`). It corrects floating-point representability checks and accepted large unsigned values across XPath31, XPath40 and Experimental. It does not expand integer storage or adopt newer argument contracts. Source remains the existing unpublished1.25.1 patch; public/Braid1.25.0 remains accepted within REQ-137's delivered scope.

## Corrected behavior

| Conversion | Before on this host | After |
|---|---|---|
| double2⁶³ to integer | silently returned Int64.MaxValue | FOCA0003; castable false |
| double-2⁶³ to integer | exact minimum | same exact minimum |
| double immediately below2⁶³ | exact accepted integer | same exact value |
| double2⁶³ to unsignedLong | could use signed backing and return wrong value | exact9223372036854775808 |
| double immediately below2⁶⁴ to unsignedLong | decimal conversion rounded magnitude | exact18446744073709549568 |
| double2⁶⁴ to unsignedLong | returned an out-of-range value | existing derived-range FORG0001; castable false |

Comparing a floating value with `long.MaxValue` or `ulong.MaxValue` rounds the integer bound upward to2⁶³ or2⁶⁴. The old strict-greater-than guards admitted those thresholds. The shared integer-family floating branch now uses the exact exclusive upper bound2⁶³ and inclusive lower bound-2⁶³. The special unsignedLong floating branch uses exact exclusive2⁶⁴ and uses signed backing only below2⁶³.

Accepted floating unsigned values at or above2⁶³ are integral because their binary spacing exceeds one. Converting through UInt64 and then decimal preserves that exact magnitude instead of applying .NET's double-to-decimal rounding policy. The finite range guard precedes both narrowing operations. Float inputs widen to double and use the same checks.

[F&O3.1 §19.1.2.4](https://www.w3.org/TR/xpath-functions-31/#casting-to-integer) defines finite numeric integer conversion and numeric-limit errors; [XPath3.1 castability](https://www.w3.org/TR/xpath-31/#id-castable) tests whether the cast succeeds. Signed limit errors retain the existing FOCA0003 classification; unsigned derived-range failures retain their existing classification. NaN and infinity still raise FOCA0002. No broad error remapping or new exception catch is added.

## Verification and limits

Adds72 regression cases across three explicit profiles. Each checks constructor, `cast as`, named function reference and `castable`; accepted results check exact magnitude and integer identity. Double/float signed minima, exclusive maxima, adjacent representable values, the unsigned signed-backing transition, zero and nonfinite controls are covered. Before24 cases failed and48 controls passed; after72/0/0, without changes to expectations.

Release build succeeded with zero errors and the existing CS8604 warning. All **11,567 solution plus84 language-server tests pass (11,651 total)**. Fresh QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across224 configured/executed sets. Ten documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

The [machine-readable proof](FLOATING-INTEGER-BOUNDARIES.json) records complete tests and fresh gates; [raw gate/probe receipts](evidence/floating-integer-boundaries/) retain comparison evidence. Existing unsigned Decimal fractional tagging/truncation defects, generic wide constructor/storage/error policy and remaining unary/division audits remain separate. This is not arbitrary-precision support. The existing decimal range/precision limits remain.

No public API, function metadata, compatibility enum, source version, fixture, gate membership, retry or release change. Current native Unix remains22/2/0 under frozen evaluation; its two newer negative-domain failures remain explicit. Direct Experimental signed tests remain separate from native frozen execution. Skips/retries do not certify full conformance.

Catalog totals remain76 entries,3,148 feature plus24 chapter checks,224 gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. Next: review this bounded fix, then correct unsigned Decimal truncation/facet order as a separate slice before wider storage/error changes. No release performed.
