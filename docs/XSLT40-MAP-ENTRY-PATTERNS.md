<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak map entry sequence pattern support">
  <br><br><h1>XSLT 4.0 map entry sequence patterns</h1>
  <p>Entry constraints, cardinality and singleton predicate focus</p>
</div>

## Contract — 16 October 2026

Unpublished source candidate **1.10.0** extends the [string-key map pattern slice](XSLT40-STRING-KEY-MAP-PATTERNS.md) with entry-value sequence patterns and outer map predicates. Public libraries remain 1.8.0; published VS Code 0.2.4 bundles engine 1.8.0. There is no new public API, publication, release tag, gate promotion, REQ status change or Braid adoption.

A quoted key can be followed by a colon and a supported nested pattern: `{'node':element(n:bar)}` or `{'amount':.[. instance of xs:decimal]+}`. Without an occurrence indicator the entry must contain exactly one matching item. With `?`, `*` or `+`, cardinality is zero-or-one, zero-or-more or one-or-more respectively; every supplied item must match. `empty-sequence()` requires an existing entry whose value is empty. An absent key never satisfies a declared entry constraint, even one that permits an empty value. Values are tested without coercion.

Repeated key constraints are conjunctive and do not count as multiple distinct map entries. Nested supported map patterns compose recursively. Bare node names, node kind tests, namespace wildcards and the existing predicate-pattern forms retain their declaring namespace/profile context. A bare wildcard node pattern is distinguished from a trailing sequence occurrence indicator. Delimiters are checked before nested node-pattern compilation so malformed constraints fail statically.

Outer predicates evaluate with singleton map focus, and nested predicates with singleton focus for their candidate entry item. Context/current-item state is restored by the existing pattern wrappers. Numeric predicate 1 matches; other numeric results do not. Existing dynamic predicate-error non-match behavior is retained. Constrained map patterns have default priority 0.25; map patterns with outer predicates have priority 0.5. Presence-only, empty and wildcard map priorities remain unchanged. Explicit priorities and next-match continue to work.

The supported nested pattern vocabulary is the existing node/predicate matcher plus supported nested maps. This does not add the separate current `atomic(TypeName)` or `record(TypeName)` syntax, array/JNode pattern families, non-string map key constants, trailing commas or map path suffixes. Existing nested-pattern coverage and limitations remain applicable. Two prior refusal controls for entry constraints and outer predicates are superseded by positive/edge coverage; all other original key-pattern controls remain.

## Consumer example

```xml
<xsl:template match="{'rows':element(row)+, 'label':.[. instance of xs:string], *}[?label ne '']">
  <group label="{?label}"><xsl:copy-of select="?rows"/></group>
</xsl:template>
```

In a version 4.0 stylesheet this requires a nonempty sequence of row elements and a nonempty string label. The trailing wildcard permits unrelated map entries. It does not convert strings to nodes or untyped values to strings.

## Pinned draft and implementation

The reviewed source remains qt4cg/qtspecs `72f222929768b411d8e88d96a1a8740483290078`, `specifications/xslt-40/src/xslt.xml`, blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`:

- [Sequence matching](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13044): entry items and occurrence requirements.
- [Simple-pattern focus](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L12897): predicates and singleton focus.
- [Map constraints](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13075): repeated constraints, presence and extra entries.
- [Map priorities](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13836) and [filtered simple-pattern priorities](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13753).

StringKeyMapPattern retains every entry constraint while deduplicating the required key set. Lexical splitting preserves nested maps, parentheses, predicates, quoted commas and EQName URI delimiters. PatternCompiler compiles each nested matcher in the same static context and applies cardinality in one traversal without materializing a second value sequence. Existing predicate compilation evaluates map suffixes. No mutable global test hook is involved.

## Evidence

PR319 passed exact-head CI and merged at `305cac469855e38542e5010d195fadd2bd42f984`. Refreshed upstream xslt40-test default remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; the detached suite is unchanged. No contribution branch, catalog/expectation change, new skip/error alias or upstream submission was made.

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| record substring | 27 / 37 / 1 | 29 / 35 / 1 |
| record set, within aggregate | 27 / 20 / 0 | 29 / 18 / 0 |
| xsl-record, within aggregate | 0 / 17 / 1 | 0 / 17 / 1 |
| type-alias | 22 / 9 / 1 | 22 / 9 / 1 |

Only unchanged native record-011/012 newly pass in this slice; record-010 remains passing from PR319. Both filtered native-family runs retain exit 1 because failures remain. Skip meanings and partial adapter limits are unchanged. No independent processor execution or family-wide conformance is claimed.

The initial new 43-case fixture on the previous engine produced 6 pass / 37 fail; its exact source bytes/hash and log are retained. An intermediate combined run exposed two parser regressions: accepted trailing comma and unclosed nested node pattern. Both were corrected with assertions unchanged, and that failed log is retained separately. Eight later wildcard/delimiter controls and two empty-sequence whitespace/comment controls bring the final new fixture to 53 cases. Together with 37 retained key-pattern controls, all 90 focused cases pass.

Source, patch, baseline fixture and execution hashes are recorded in [machine-readable evidence](XSLT40-MAP-ENTRY-PATTERNS.json). Original earlier receipts remain unchanged. Next bounded review: current atomic(TypeName) patterns and their priorities, including use within map entry sequence patterns; retain the existing node/predicate semantics and frozen gates.

## Verification

Release builds succeed: initial zero errors/22 warnings, final incremental zero errors/21 warnings. Final source includes empty-sequence token whitespace/comments. Solution 8,715/0/0 (XSLT 5,829); language server 84/0/0; combined focused and repeat runs 90/0/0. Frozen gates remain exactly QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, both exit 0. Native-family counts repeat identically. Documentation rendered with markdown-it and Fytala CSS; AGENTS.md conventions remain applicable. All local verification jobs are complete.
