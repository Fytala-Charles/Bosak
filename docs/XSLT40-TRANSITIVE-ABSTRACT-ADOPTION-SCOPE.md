<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak transitive abstract function adoption scope">
  <br><br><h1>Transitive abstract function adoption</h1>
  <p>Bounded next-slice scope, 15 October 2026</p>
</div>

## Starting point

[PR #299](https://github.com/Fytala-Charles/Bosak/pull/299) is merged at `d6b984cb41ba3bfbb2a0d4aab6c8a162f5f2e6ab`. Its source/test trees match reviewed revision `7a9422cfbf6a2a6f83d9e9e37ef56860c12bf566`, whose full solution, language-server, frozen-gate and package verification are preserved in the original receipts. Source 1.7.0 remains unpublished; public libraries and the VS Code 0.2.1 bundle remain 1.6.0.

This document scopes the next implementation. It changes no engine code, public API, version, REQ status or gate membership. [Baseline receipt](XSLT40-TRANSITIVE-ABSTRACT-ADOPTION-SCOPE.json).

## Specification checkpoint

Reviewed [XSLT 4.0 sections 3.4.5.2-3.4.5.5](https://qt4cg.org/specifications/xslt-40/Overview.html) on 15 October 2026. A library may accept an abstract component as abstract, allowing another package that uses it indirectly to provide the concrete implementation. Without explicit acceptance or replacement, the abstract component becomes hidden; that default is not equivalent to adopting it as abstract.

An absent implementation invoked from a used package raises XTDE3052. A top-level package containing an explicitly accepted unresolved abstract component raises XTSE3080. Concrete overriding signatures retain XTSE3070 compatibility checks, and references to an abstract original raise static XTSE3075. Keywords come from the original declaration even when replacement parameter names differ.

## Bounded package chain

| Package | Responsibility |
|---|---|
| A | Declares abstract `p:f#3` with required `a` and optional `b`, `c`; public `p:from-a()` calls it. |
| B | Uses A, explicitly accepts `p:f#3` as abstract and `p:from-a#0` as public; public `p:from-b()` calls `p:f`. |
| C | Uses B and supplies one concrete override of `p:f#3`; its optional defaults and private helpers belong to C. |

The defining adoption in B is:

```xml
<xsl:use-package name="urn:transitive:A" package-version="1.0">
  <xsl:accept component="function" names="p:f#3" visibility="abstract"/>
  <xsl:accept component="function" names="p:from-a#0" visibility="public"/>
</xsl:use-package>
```

C overrides the function in its `xsl:use-package` for B. C must not explicitly accept that same function by name alongside the override, which would independently violate XTSE3051. The original range remains 1-3; this slice introduces no overlapping family.

## Measured baseline

Eight isolated package-only probes use archives built from exact merged revision `d6b984cb`. All nine engine runtime dependencies match candidate feed/cache/archive/output bytes and report assembly 1.7.0.0 with the merged informational revision. The direct A-to-C control passes. Seven transitive probes fail, all at XTSE3058:

- Reduced and full principal calls, plus a keyword call using A's parameter names.
- Calls originating in A and in B.
- An uncalled original-reference error probe expecting XTSE3075.
- A minimum-arity mismatch probe expecting XTSE3070.

These are exploratory acceptance baselines outside the unit suite, not seven regressions in the integrated feature. Every failure stops at declaration matching. Runtime behavior after that check is not yet measured. Original fixtures, TRX, logs, fresh cache, archives and metadata remain under `.guard-tmp/transitive-scope-*` and are hash-pinned in the receipt.

## Implementation checkpoints

1. **Resolve adopted declarations with visibility.** `Stylesheet.TryFindFunction` currently searches local declarations, imports and includes, but not accepted components from used packages. Resolve B's adopted abstract family with its effective visibility, original upper arity, types and parameter names. A recursive search that exposes every hidden/private used-package function is insufficient. Preserve direct-family behavior and missing/hidden-component boundaries.
2. **Bind through both adoption edges.** After compilation succeeds, prove that calls in C, B and A all reach C's replacement at reduced/full arities. Check named references, lookup and partial application separately. Keep declaration identity distinct from the package containing its adopted view.
3. **Preserve declaring scope and errors.** Verify replacement defaults/private helpers in C, A's keyword names, XTSE3070 signature failures and compile-time XTSE3075 in uncalled/default/dead-branch references. Retain XTSE3080 for an explicitly accepted unresolved top-level component and XTDE3052 for a hidden absent implementation invoked through an A wrapper.

Before modifying the engine, extend the isolated fixtures to include the negative visibility controls, default/private-scope cases, function items, and original-reference forms. Baseline the final fixture against the merged candidate; retain the public-1.6.0 comparison separately if used. Do not reuse the current 1/7 count for an expanded fixture.

## Delivery gates and boundaries

New functionality requires happy-path and edge-case unit regressions, all solution and language-server tests, the frozen QT3 gate **31,142/0/679**, qt4 gate-only **8,278/0/1,894**, and fresh package-only verification at the exact reviewed commit. Keep accepted evidence separate from initial red baselines and preserve original receipts.

Escaping inline closures, overlapping adopted/local families, package acquisition and editor package signatures remain separate slices. REQ-126 stays In Progress. No upstream test submission is part of this scope; any later contribution must use freshly fetched current upstream with exact-base/rebase receipts. Integration and eventual 1.7.0 publication remain separate decisions.
