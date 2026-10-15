<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala escaping inline original closure scope">
  <br><br><h1>Escaping inline original closures</h1>
  <p>Bounded correctness slice, 15 October 2026</p>
</div>

## Reproduced public baseline

Eight isolated probes against public 1.7.0 give **3 pass / 5 fail**. Direct original calls, a captured named-original item and ordinary captured variables pass. Returning an inline function that calls `xsl:original` fails after the overriding frame exits: reduced optional calls, keyword calls and nested closures lose the lexical original. Calling the closure inside a different override incorrectly selects that override's original (`WRONG` rather than `ABC`). [Baseline evidence](XSLT40-ESCAPING-INLINE-CLOSURES-SCOPE.json) preserves the fixture/TRX hashes and fresh public-package provenance separately from all implementation results.

## Intended bounded correction

An original reference inside the lexical overriding function must remain bound to that original declaration when enclosed in an escaping inline function. The current [XSLT 4.0 overriding and reference-binding rules](https://qt4cg.org/specifications/xslt-40/Overview.html#overriding-components) distinguish this static binding from dynamic lookup. The pseudo-name remains absent from `function-lookup` and `function-available`; abstract originals still raise static XTSE3075.

Implement and test lexical original binding through direct/nested closures, optional arities, keywords, declaring defaults/private scope, cross-override calls and exception restoration. Preserve captured variables and absent inline-body focus. Keep general package graph capture, overlapping families and editor package signatures separate. Avoid capturing every function registry for ordinary closures when a narrower binding is sufficient.

This document records scope and baseline only. No engine change, source-version bump, public API, REQ status or gate promotion occurs here. The implementation requires meaningful happy-path/edge controls, full tests, frozen gates and exact candidate-package provenance before review.
