<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak abstract optional function override scope">
  <br><br><h1>Abstract optional function overrides</h1>
  <p>Proposed bounded follow-up, 15 October 2026</p>
</div>

## Specification checkpoint

Reviewed the current [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.4.5.2–3.4.5.4, on 2026-10-15. Abstract components may receive concrete overrides. Function compatibility requires matching arity ranges, parameter/return types and determinism, plus applicable streamability rules; keyword names come from the original declaration. Using `xsl:original` for an abstract target is static error **XTSE3075**. Invoking an unresolved abstract component is **XTDE3052**; unresolved abstract components in a top-level package raise **XTSE3080**.

## Current implementation

Published 1.6.0 supports public optional overrides. `ValidateFunctionOverrideSignature` already checks compatibility; the package compiler then explicitly refuses an abstract original with optional parameters. This scope changes no code or version and does not claim XTSE3075 coverage. REQ-126 remains In Progress.

## Proposed first slice

Start with one abstract optional family and a direct concrete override. Before implementation, preserve a public-1.6.0 package baseline and validate fixture declarations against the draft. Proposed acceptance probes:

- Reduced/full calls, replacement defaults, keyword-name retention and package-private scope.
- Arity/type incompatibility with XTSE3070; unchanged upper-bound component identity.
- Semantic original references rejected with XTSE3075 before evaluation: calls, named references, partials, defaults and dead branches. Literal strings containing the same text must remain valid.
- Existing unresolved-abstract behavior remains intact; public override regressions still pass.

Treat transitive abstract adoption and escaping inline closures as separate checkpoints until explicit fixtures establish their behavior. Keep overlapping arity families, package acquisition and editor package signatures outside this slice. Do not remove the guard without compile-time reference validation and preserved regression evidence.

## Delivery gate

Implementation requires happy-path/error regressions, the full solution and language-server checks, unchanged frozen gates **31,142/0/679** and **8,278/0/1,894**, and package-only verification. Any upstream test contribution must start from freshly fetched current upstream and preserve exact-base receipts. Implementation and a later engine release are separate follow-ups; this document records scope only.
