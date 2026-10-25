# Native array / xsl:select boundary batch

**Submission update — 20 October 2026:** Submitted as [qt4cg/xslt40-test PR #33](https://github.com/qt4cg/xslt40-test/pull/33), after fetching upstream master again and confirming it remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Submitted head: `d1aec33e14ce623e2b65c4c592d51804353cf47d`. Both modified catalogs validate, existing cases are unchanged, and all 24 stylesheets match the executed fixtures. Bosak PRs #389/#390 are merged; main CI passed 10,078 solution plus 84 language-server tests. The PR is open with no upstream checks listed at submission. See [submission receipt](submission.json). The preparation record below is retained as historical evidence; no independent processor confirmation is claimed.

Prepared on20 October2026 against freshly fetched xslt40-test master `91328fbf0fed4a5d2813eb3661808ee53cb961ba` and qtspecs `72f222929768b411d8e88d96a1a8740483290078`. Bosak engine base is merged main `22cc7d308e33a38005c192c0d74ff46f0df08b57` (unpublished source1.22.1; public1.22.0). Harness PR387 and switch PR388 are merged and verified. This batch changes tests and documentation only.

The24 native cases add ten array and fourteen xsl:select boundaries. The exact same24 stylesheet files are copied into the standalone [fixture catalog](../../../tests/Bosak.Xslt.Conformance/Fixtures/ArraySelectBoundaries/catalog.xml) and reused by48 public-API unit checks across both processor profiles. [Portable native patch](0001-native-tests.patch), [case manifest](cases.json) and [execution evidence](evidence.json) retain the original expectations. No upstream submission or independent processor confirmation has occurred.

## Coverage and native placement

Array cases extend `tests/insn/array`; xsl:select cases extend `tests/misc/seqtor`. The native family called `select` tests the older select attribute, not the new instruction. Existing array027..032 cover empty/grouped members, nested maps/arrays, direct node identity, select-form focus and mixed JNode error. Existing seqtor125..134 cover empty bodies, basic errors/coercion and fallback placement;140..146 cover expression-local namespaces, literal newline/quotes and atomic focus tuples. This batch targets additional boundaries rather than duplicating those examples.

A namespace-aware scan reviewed9,416 standalone stylesheets and197 target instruction occurrences. Twenty-five files were unparseable; none contained lexical target instructions. The two catalog stylesheet references to non-.xsl XML files are embedded XSLT2 include fixtures without target instructions. Catalog stylesheet definitions reference files, with no inline stylesheet children in the inspected suite. These counts describe the scan, not semantic completeness. Existing test-case contents/assertions were compared with the pinned base and retained unchanged (outer inter-case indentation excluded).

| Case | Additional boundary | Expected assertion |
|---|---|---|
| array-033 | Empty for-each input does not evaluate a failing member constructor. | assert-xml |
| array-034 | Array iteration restores the outer item, position and size. | assert-xml |
| array-035 | Array sequence-constructor form sees the complete iteration focus. | assert-xml |
| array-036 | An ordinary function item remains callable as an array member. | assert-xml |
| array-037 | Nested array-member wrappers recursively unwrap grouped payload without copying nodes. | assert-xml |
| array-038 | Array select cannot be combined with nonempty sequence-constructor content. | XTSE3185 |
| array-039 | Array-member select cannot be combined with nonempty constructor content. | XTSE3185 |
| array-040 | Array select permits whitespace, comments and ignored fallback children. | assert-xml |
| array-041 | Array-member select permits whitespace, comments and ignored fallback children. | assert-xml |
| array-042 | An empty array instruction creates no members, unlike an empty array-member. | assert-xml |
| seqtor-147 | Whitespace-only select body satisfies an explicit empty-sequence result type. | assert-xml |
| seqtor-148 | Whitespace-only select body fails a required singleton result type. | XPTY0004 |
| seqtor-149 | Multiple select results fail a required singleton result type. | XPTY0004 |
| seqtor-150 | Select body can return a callable ordinary function item. | assert-xml |
| seqtor-151 | Select body preserves XML-node identity and explicit reverse order. | assert-xml |
| seqtor-152 | Host xpath-default-namespace applies to select body name tests. | assert-xml |
| seqtor-153 | Instruction-local default collation applies inside select without leaking to siblings. | assert-xml |
| seqtor-154 | Adjacent CDATA sections preserve an XPath string containing a CDATA terminator. | assert-xml |
| seqtor-155 | Comments, processing instructions and xsl:note are removed before body evaluation. | assert-xml |
| seqtor-156 | Fallback-only select body returns empty and does not execute fallback. | assert-xml |
| seqtor-157 | A select expression succeeds without executing a failing fallback. | assert-xml |
| seqtor-158 | A selected expression error propagates rather than invoking fallback. | FOER0000 |
| seqtor-159 | An undeclared variable in the select body raises a static binding error. | XPST0008 |
| seqtor-160 | Malformed select body XPath is rejected when its named template is invoked. | XPST0003 |

## Normative basis and correction

Expected results derive from pinned xslt.xml sections [array-construction](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml), `select-instruction`, `errors`, the shared select/constructor mutual-exclusion rules and element-catalog.xml. Cases038/039 keep XTSE3185; required-result cardinality keeps XPTY0004;158 keeps FOER0000;159/160 keep XPST0008/XPST0003. No expected error was changed to match Bosak.

The original run was21/3/0 in each profile. Two draft candidates incorrectly assumed every processor mode must report errors in an unused template. The pinned `errors` section explicitly permits an additional mode that leaves errors in unexecuted parts unreported. The portable cases159/160 now invoke those templates; their expected codes are unchanged. [Original logs and fixture versions](original-execution/manifest.json) and correction metadata are retained separately. This is a test-precondition correction, not an engine fix or evidence of eager static-error coverage.

The third failure was harness-only: exact-error mode stopped before inspecting XPathErrorException's structured identity, so `error()` carrying FOER0000 with message `fn:error() called` was falsely rejected. With corrected fixture invocation, the unchanged runner reports23/1/0. The separate structured-error harness patch reports24/0/0 and retains wrong-code/namespace negative controls. The48 direct public-API checks pass against the unchanged merged engine regardless of that runner patch.

## Verification and scope

All five standalone/expanded catalog documents validate against the pinned native XSD. Expanded draft array results are168/2/20 (ten new passes; two historical failures retained); expanded seqtor is97/0/9 with corrected structured matching (fourteen new passes). Legacy array165/5/20 and seqtor97/0/9. No new skips, aliases, changed old expectations, gate promotions or engine/version changes. The isolated Release build passes with zero warnings/errors and all **10,078 solution tests pass**, including48 new public-API checks (7,137 XSLT tests total). Fixture attributes preserve the executed XML/XSL bytes across platforms, and original evidence/patch bytes are protected. Exact-head CI must still pass before merge.

The portable patch is prepared, not submitted. Fetch upstream default again immediately before submission, rebase if it advanced, record the refreshed base and recheck the final diff. Preserve all original receipts. jposition#1 and unresolved record callable contracts remain outside this bounded batch.
