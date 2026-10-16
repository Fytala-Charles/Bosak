# XSLT 4.0 implicit documents in variable bindings

Unpublished source **1.14.4** corrects sequence-constructor evaluation for `xsl:variable`, `xsl:param` and `xsl:with-param`. In effective XSLT 4.0, a binding without `as` or `select` retains its result sequence when a direct child is `xsl:map`, `xsl:map-entry`, `xsl:array`, `xsl:array-member` or `xsl:select`. Other non-empty untyped bindings continue to create a temporary document. This is direct-child detection, not a descendant search.

```xml
<xsl:variable name="values">
  <xsl:array for-each="1 to 10">
    <xsl:array-member select="(., -.)"/>
  </xsl:array>
</xsl:variable>
<!-- $values?3 is the sequence (3, -3), without an implicit document. -->
```

The shared sequence-constructor evaluator applies this rule before choosing its existing raw-sequence or temporary-tree path. Global and local bindings, template defaults, ordinary and tunneled supplied parameters share it. The effective version resolver honors local overrides. Explicit types still undergo conversion; explicit document constructors and nested literal XML retain complex-content restrictions. No public API, serializer, runner, JNode representation or error-code alias changes are included.

## Primary rules and review

The reviewed qtspecs revision is `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [variable-value rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L18166) and [disqualifying-element definition](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L18368) require this behavior. The live [editor draft](https://qt4cg.org/specifications/xslt-40/Overview.html#variable-values) was also consulted.

Native array-017 expects XTDE4060, but the pinned [array construction rule](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L29923) specifies **XTTE4060** for this case. Bosak already raises that type error. The upstream expected code remains untouched; this case remains a native failure. A local control locks in XTTE4060.

PR327 passed exact-head CI run38894712063 at ee524484adc2b9d1ba3cd70d8c61c342477f8bca and merged at `2b926bb794735d0c3ab64c95a7a7427d57179cd0`, the base of this isolated worktree. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. No upstream branch, contribution, expectation edit, skip or gate promotion is part of this slice.

## Native results

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 153/7/20 | 154/6/20 |
| array instruction set | 22/7/8 | 23/6/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

Only **array-018** newly passes; no previous pass is lost. All three family runs still exit1 because failures remain. Original before receipts and logs were copied byte-for-byte into `.artifacts/native-before`; their embedded paths still identify the original runs. Current receipts are under `.artifacts/xslt40`.

## Regression controls and retained attempts

The original test source and compile-error log are retained: the first fixture incorrectly used LINQ on a struct sequence enumerator (CS0411). The corrected runnable 21-case fixture establishes **9 pass / 12 fail** before implementation, then **21 pass / 0 fail** afterward. These expectations were retained.

Five exploratory controls were then added. The first expanded run was 24/2: one incorrectly expected `xsl:map-entry` to return a JNode, while the existing instruction returns a singleton map; the other combined explicit-document kind with an unrelated multi-atomic spacing expectation. The original expanded fixture and log remain. The erroneous map-entry JNode assertion was removed (the original map-entry disqualification control remains), and the explicit-document control now checks that a direct array still raises XTDE0450. The spacing observation is retained for separate review, with no production change here.

The final **25 controls pass**, covering all five direct disqualifiers, array member sequences, globals, parameter defaults, supplied and tunnel parameters, ordinary temporary documents, descendant exclusion, older versions, both directions of local version override, mixed sequence order/node identity, array-member JNode kind, explicit type errors, explicit document construction and XTTE4060. Evidence hashes distinguish every fixture and attempt.

## Verification

All local jobs completed successfully: Release build zero errors/zero final incremental warnings; solution **8,974/0/0** (XSLT6,040); language server **84/0/0**; focused **25/0/0**; QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. The companion JSON records source, fixture, log and native receipt SHA-256 hashes. Public libraries1.8.0 and VSCode0.2.4/engine1.8.0 remain published versions; REQ-126 remains In Progress and independent Braid acceptance is unchanged.

Next: review the remaining native array-023 result mismatch against the current draft, preserving profile/pattern issues as separate slices. Also retain the exploratory explicit-document multi-atomic spacing observation for separate investigation.
