# Supported standard attributes on copy instructions

Unpublished source **1.14.17** replaces xsl:copy's copy-only attribute whitelist with the existing supported-standard set plus copy-specific options. Previously, attributes such as expand-text, xpath-default-namespace, default-collation, default-mode and version were rejected as XTSE0090 even though their existing runtime handlers could process them.

The copy validator creates its allowed set through AllowedXsltAttributes and removes input-type-annotations, which belongs to module declarations rather than ordinary instructions. Copy-specific boolean/value checks, namespace/foreign-metadata classification, shadow names and attribute-set references retain their paths. The shared helper and its other callers are unchanged; no runtime or public API change is included.

```xml
<xsl:copy select="$seed" expand-text="yes">
  value {1+1}
</xsl:copy>
<!-- Copied element text: value 2. -->
```

## Specification and integration base

PR341 passed exact-head CI run38934679051 at `02ca511c154dd9dcac33c65f8585f67f79437008` and merged at `331602e53a0934cd1596967577161645eb9a0cf4`, the final base. Baseline began on that exact precursor while CI ran; entire precursor/merge trees were verified identical (`80a04f63659a2b63fe8d90a8bf4829cd7271be19`) before fast-forward alignment. Tested source did not change during alignment.

Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [standard attribute rules](https://qt4cg.org/specifications/xslt-40/Overview.html#standard-attributes) allow the standard attributes on every XSLT element. This slice enables the already-supported shared set for copy; new4.0 schema-role processing is a separate feature, not introduced by whitelist reuse.

## Regression controls

Original27-case baseline: **3 passed / 24 failed**. First implementation: **284/0** combined controls. Expanded31-case baseline: **4 passed / 27 failed**. Final focused selection: **288/0/0** (31 new +257 previous construction/copy controls). Final test source is byte-identical to the expanded pre-fix fixture. No compile errors, fixture/API corrections, assertion changes or weakened expectations were needed; original fixtures/logs and first implementation snapshot are retained.

Acceptance controls cover local version, expand-text, default-mode, default-collation, xpath-default-namespace, exclude-result-prefixes, extension-element-prefixes and default-validation on both ordinary/function copy paths. Behavioral controls verify expansion enabled/disabled, namespace-qualified child lookup, case-insensitive comparison and local default-mode application. Unused function bodies are validated statically. Negative controls retain XTSE0090 for unknown/module-only attributes and XTSE0020 for invalid real boolean options beside standard attributes.

The preexisting use-when handling remains unchanged. Standard attribute value processors are reused; this does not claim all new4.0 standard-attribute semantics or forward-compatible unknown-attribute handling are complete.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR341 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/2 warnings; solution **9,335/0/0** (XSLT6,401), focused combined **288/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.17 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then audit xsl:copy forward-compatible attribute handling. Schema-role support, JNode copying, broader constructor namespace semantics, nonraw text-only API delivery kind, processor capability and obsolete patterns/functions remain separate.
