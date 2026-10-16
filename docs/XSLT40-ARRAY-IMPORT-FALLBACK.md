# Array member templates after apply-imports

Unpublished source **1.14.6** fixes the array built-in fallback used when `xsl:apply-imports` finds no imported matching rule in `text-only-copy` or `shallow-skip` mode. It now uses the existing `?*` member-template helper instead of atomic fallback. Empty arrays remain empty; multi-item/empty members are flattened; node members use normal node dispatch; ordinary/tunnel parameters and named modes reach member rules.

The array caller's import restriction must not constrain a new member template application. The shared helper now saves and temporarily clears that restriction stack, restoring its original order in `finally`. This also handles next-match fallback inside an imported array template. Caller focus/current item restoration from the previous slice is retained. Imported-rule selection itself is unchanged: an imported matching rule still wins before built-in fallback. Atomic and deep-skip controls retain their behavior.

```xml
<xsl:import href="base.xsl"/>
<xsl:template match="array(.*)">
  <xsl:apply-imports/>
</xsl:template>
<!-- If base.xsl has no matching array rule, apply member templates in the current mode. -->
```

This slice adds no public APIs, serializer changes, adapter changes, pattern grammar, priority rules, JNode model changes or new copy-mode/function-item behavior. Generic import/next-match restrictions outside implicit array member application are unchanged.

## Specification and integration base

Refreshed upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`, qtspecs `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [apply-imports/next-match fallback rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15776) require built-in dispatch when no eligible rule exists. The [text-only-copy](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15221) and [shallow-skip](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L15703) rules specify implicit `apply-templates select="?*" mode="#current"`. This starts member rule selection independently of the enclosing array's apply-imports selection. The live [editor draft](https://qt4cg.org/specifications/xslt-40/Overview.html#apply-imports) was consulted too.

PR329 passed exact-head CI run38900094138 at3d7e978238426555e50678b244be0d9f8f1dc5da and merged at `1e7dac4dd427615623dcfd20e74ea5eea143a9e6`, this isolated branch's base. Worktree: D:/Development/Bosak/.guard-tmp/array-import-fallback-source; branch: fix/array-import-fallback.

## Controls and retained attempts

The original15-case source and before log are preserved: **3 pass / 12 fail**. Routing apply-imports to the existing member helper made **14 pass / 1 fail**. The remaining member-next-match control expected `HL` but produced `H1`: the array caller's import restriction incorrectly excluded the lower-priority member rule at the same import precedence. The intermediate route-only engine snapshot and log are preserved separately.

Suspending/restoring the restriction stack fixes that case without changing its expectation. The original15 controls plus16 prior next-match controls then passed **31/0**. Two additional controls verify restoration after a member error and next-match fallback inside an imported array rule; the final combined selection is **33/0**, including **17 new cases**. No fixture errors, expectation corrections or weakened assertions were needed.

The controls cover empty and nested arrays, multi-item/empty member flattening, member and caller focus/current item, ordinary/tunnel parameters, named modes, node traversal, imported-rule selection, fresh member next-match chains, outer import restriction restoration on normal/error exits, XSLT3.0 predicate patterns, atomic fallback and text-only-copy/shallow-skip/deep-skip behavior. In-memory module resolution avoids external filesystem or network dependencies.

## Native evidence

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| array instruction set | 24/5/8 | 24/5/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

No newly passing or lost native case is claimed. All three native family runs still exit1 because failures remain. Before receipts/logs are copied byte-for-byte into `.artifacts/native-before` with their original embedded paths retained. Current receipts are in `.artifacts/xslt40`; the companion JSON records hashes. No upstream edit, submission, error alias, skip or gate promotion was made.

## Verification and next review

All local jobs completed: Release build zero errors/20 final incremental warnings (including NU1900 vulnerability-data lookup failures); solution **9,007/0/0** (XSLT6,073), focused combined **33/0/0**, language server **84/0/0**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. The companion JSON records source, fixture, intermediate engine snapshot, native receipt and log SHA-256 hashes. Canonical docs are synchronized and rendered with markdown-it and canonical Fytala CSS. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged. Source1.14.6 remains unpublished.

Next: exact-head CI/review before integration, then audit the remaining native array-021/022/024/025 version/profile declarations against the current draft and runner behavior. Array-017's known XTDE4060 versus draft-correct XTTE4060 mismatch remains separate. Broader built-in copy/function behavior and the earlier explicit-document multi-atomic spacing observation remain separate too.
