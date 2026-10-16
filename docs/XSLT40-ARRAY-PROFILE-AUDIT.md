# Remaining native array failures: processor profiles and diagnostic probes

This review changes documentation only. Source remains **1.14.6 unpublished**; engine, tracked tests, runner, upstream fixtures, expected results, skips and gate membership are unchanged. PR330 passed exact-head CI run 38903641620 at 1730fc164e58b65ce66e5e1339c8f56e05c2823c and merged at `46bb748d72019177ff597a471ec6762c3298f23e`, this audit branch's base.

## Processor capability differs from stylesheet effective version

The current draft's [XSLT 3.0 compatibility rule](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L7208) defines no differences between effective versions 3.0 and 4.0 for an XSLT 4.0 processor. A 3.0 declaration therefore does **not** establish that a test is invalid for a 4.0 processor. The live [compatibility section](https://qt4cg.org/specifications/xslt-40/Overview.html#backwards-3.0) was checked too.

Bosak currently selects XPath40 when an element's effective version is at least4.0, otherwise XPath31 (`Stylesheet.GetXPathCompatibility`). Its native `--xslt40` runner option enables suite/dependency selection and exact errors; it does not supply an independent4.0 processor capability to the compiler. This experimental profile selection is a **processor-profile gap relative to the draft**, not sufficient evidence to label upstream3.0 metadata defective. Earlier dated notes describing these as profile issues remain historical observations of Bosak behavior, not normative conclusions about stylesheet validity.

A future processor-capability design must separate the selected processor from effective-version compatibility rules while preserving the existing frozen default 3.1/3.0 profile. Selectively upgrading native stylesheets or weakening error matching would conceal the distinction. No such change is made here.

## Four unchanged originals and four isolated probes

Upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. Every audited stylesheet declares 3.0. Catalog dependencies differ: array-021/024/025 use XSLT30+, array-022 uses XSLT40+.

An ignored standalone diagnostic tool ran each original with its original base URI, then a separate copy changing exactly one root-version literal from 3.0 to 4.0. Original bytes are verified identical to upstream. These copies and outputs are retained under `.artifacts/profile-probe`; they are **diagnostics, not native passes, upstream corrections or proposed submissions**.

| Case | Unchanged native observation | Root-version4-only diagnostic | Conclusion |
|---|---|---|---|
| array-021 | XPST0017 for array:members; expects XPTY0004 | XPTY0004 | Existing4.0 function typing reaches the expected error; original failure is masked by profile selection. |
| array-022 | XPST0017 for array:members; expects RU TJ | Empty out element | A separate result-construction problem remains after the profile barrier. |
| array-024 | XTSE0340 from strict-profile match validation | Expected Sue/Schreiber and Debbie/Lockett elements | Profile selection prevents this result; legacy ~array(Type) patterns also occur, so this does not certify current pattern grammar conformance. |
| array-025 | XPST0017 for jkey | XPST0017: jposition#1 not found | Profile selection masks an additional function-catalog mismatch. |

The pinned F&O catalog blob `930edc7ade2bdf7afecdf8019b93e5b450d9f10e` contains 734 function definitions including `jkey` and `jvalue`, and no `jposition` definition. Missing `jposition` is not, by itself, proof of a current-draft engine function gap. No alias or replacement expectation was introduced. The known array-017 XTDE4060 versus draft-correct XTTE4060 mismatch remains unchanged.

## The array-022 blocker is text construction, not child navigation

The original city fixture has 209,579 records and exactly two `Moskva` records, with countries RU and TJ in that order. A separate minimal 4.0 probe uses one map and one array-member JNode:

```xml
<xsl:variable name="city" select="array:members([map{'name':'Moskva','country':'RU'}])[1]"/>
<out children="{count($city/name)}" key="{jkey($city/name)}"
     data="{data($city/name)}" avt="{$city/name}">{$city/name}</out>
```

With expand-text enabled, actual attributes are children=1, key=name, data=Moskva, avt=(jnode), and element text is (jnode). This proves child-name selection and explicit atomization work in the minimal case, while AVT/expanded-text conversion leaks the debug representation. The next bounded implementation should address JNode atomization in XSLT simple-content construction, with happy/edge/error controls, before changing navigation or rewriting the city fixture. See the draft's [simple-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-simple-content).

## Verification and accounting

A fresh unchanged native array run remains **155/5/20**, exit1, with exactly the same passing cases as the preceding receipt. Record 29/35/1 and type-alias 23/8/1 are carried from PR330's verification, not rerun or counted as fresh results here. Original receipts/logs are copied byte-identically into `.artifacts/native-before`. Diagnostic version substitutions do not alter the native counts.

All local jobs completed: Release build zero errors/13 incremental warnings; solution **9,007/0/0** (XSLT6,073) and language server **84/0/0**. No tracked tests were added. Source, original/probe fixture, tool, native receipt and log hashes are in the companion JSON. Frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894** are carried from the engine-equivalent PR330 source, not rerun: `git diff 1730fc16 HEAD -- src tests` is empty. No engine or tracked-test file changes in this slice. Build passes; canonical documentation is synchronized and rendered. Public libraries 1.8.0, VSCode 0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged.

Next: check exact-head CI/review, then implement the independently reproduced JNode simple-content atomization fix. Keep processor-capability architecture, current-pattern cleanup and obsolete-function review as separate work.
