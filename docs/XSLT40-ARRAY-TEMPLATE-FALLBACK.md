# Array member templates after next-match

Unpublished source **1.14.5** fixes array fallback in `text-only-copy` and `shallow-skip` modes. When `xsl:next-match` runs out of matching rules for an array, the engine now applies templates to `?*` in the current mode. It previously called the atomic fallback, emitting the debug string `(array)` under text-only-copy or skipping the array's members under shallow-skip.

The same helper handles ordinary unmatched arrays in these two modes. Normal template application flattens multi-item member sequences, dispatches node members through node rules, computes position/size over the selected sequence, carries ordinary/tunnel parameters, and starts a fresh next-match chain for each application. The helper saves and restores the caller's context item, current item, position and size.

```xml
<xsl:template match="array(.*)">
  <xsl:next-match/>
</xsl:template>
<!-- Applying templates to [(), (1, 2), 3] now processes 1, 2 and 3. -->
```

No template-priority, pattern grammar, public API, serializer, adapter or JNode-model change is included. `xsl:apply-imports` still has its separate non-node fallback path and is the next bounded review. Deep/shallow copy modes, other function-item fallback and broader built-in-rule alignment are outside this slice; a next-match deep-skip control continues to emit nothing.

## Primary rules and native review

Refreshed upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`, qtspecs `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [fallback rule](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15776) requires the built-in rule when no next-match candidate remains. [Text-only-copy](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15221) and [shallow-skip](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15703) specify member application for arrays. The live [editor draft](https://qt4cg.org/specifications/xslt-40/Overview.html#built-in-templates-text-only-copy) was also consulted.

Native array-023 uses legacy `~array(Type)` patterns. The corresponding type-pattern section in the current source is commented out; current `array(SequencePattern)` and `atomic(TypeName)` patterns have different priority rules. We did not treat the old test as authority for changing modern priorities. Diagnosis showed its actual nested D/I/L value was `(array)` rather than42. Modern-syntax controls independently reproduced the same runtime fallback defect. The unchanged native assertion now passes with no priority change.

PR328 passed exact-head CI run38896975893 at20e290160f030e3901784d7b974558f33ef2dfc3 and merged at `5997653914322e88c0d8b5efa8014c45a3c81d6c`, this worktree's base. The initial priority hypothesis determined the worktree name; the confirmed implementation branch is `fix/array-template-fallback`.

## Native results

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 154/6/20 | 155/5/20 |
| array instruction set | 23/6/8 | 24/5/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

Only **array-023** newly passes; no prior pass is lost. All three native family runs still exit1 because failures remain. Original baseline logs and receipts are copied byte-for-byte into `.artifacts/native-before`, retaining their embedded original paths. Current receipts are under `.artifacts/xslt40`. No upstream edits, error aliases, skips or gate promotions were made. Array-017 still compares the upstream XTDE4060 expectation against the draft-correct XTTE4060; array-021/022/025 retain profile issues and array-024 retains a pattern-validation issue.

## Regression controls

The original11-case source and before log are preserved: **1 passed / 10 failed**, with `(array)` visible in diagnostics, including the native-shaped D/I/L chain. With unchanged expectations, all11 pass after the implementation. Five further controls bring the final total to **16 passed**; no fixture compilation errors or expectation corrections were needed.

Controls cover empty arrays, empty and multi-item members, nested arrays and fresh next-match chains, selected and unmatched element members, flattened position/size, named mode selection, ordinary/tunnel parameters, caller focus/current-item restoration, ordinary unmatched-array processing, XSLT3.0 predicate dispatch, atomic next-match fallback, shallow-skip and deep-skip behavior.

## Verification and delivery

All local jobs completed: Release build zero errors/two incremental warnings; solution **8,990/0/0** (XSLT6,056), focused **16/0/0**, language server **84/0/0**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. The companion JSON records source, fixture, native receipt and log SHA-256 hashes. Canonical docs are synchronized and rendered with markdown-it and canonical Fytala CSS. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged. No publication or upstream submission is part of this slice.

Next: check exact-head CI/review before integration, then examine `xsl:apply-imports` array fallback with independent controls. Profile/pattern issues and the earlier explicit-document multi-atomic spacing observation remain separate.
