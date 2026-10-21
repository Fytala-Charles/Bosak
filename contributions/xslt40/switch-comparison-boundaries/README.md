# Native switch comparison and result boundaries

**Switch slice merged and upstream draft submitted - 21 October 2026:** [Bosak PR395](https://github.com/Fytala-Charles/Bosak/pull/395) merged as `d53d64bdce34bf3cfdabee98b6ccf2a7ef31f1b4` after exact-head CI passed10,246 solution +84 language-server tests. [Upstream draft PR34](https://github.com/qt4cg/xslt40-test/pull/34) contains the12 native switch cases, based on refreshed default `91328fbf`; [submission proof](submission.json) records exact revisions and the unchanged prepared patch hash. Upstream review/acceptance and independent processor confirmation remain pending. Source1.23.1 is unpublished; public1.23.0 and Braid REQ-135 validation remain unchanged. Merged-main CI39572257850 is running at this checkpoint. Earlier prepared/not-submitted statements below are historical.

## Original preparation and verification

# Native switch comparison and result boundaries

Twelve native XSLT4 switch cases and32 public-API checks strengthen selector atomization, comparison, focus and result semantics. One case exposed a node-identity bug: a selected `when/@select` or `otherwise/@select` passed through result-tree copying inside a raw sequence constructor. Unpublished patch **1.23.1** reuses the existing sequence-placeholder mechanism. Serialized result-tree delivery and function-body collection retain their existing paths. No API or processor-profile default changes.

The initial native result is11/1/0 in each profile; direct public checks are22/2/0. The same input without switch preserves identity; the switch still fails with rooted input nodes. Final new native results are **12/0/0** in both profiles; expanded switch is **22/0/2**, retaining the two existing dependency skips. The original failing assertion remains unchanged. Eight additional direct controls cover when/otherwise in template/function dispatch, giving32 total new public checks. Native and unit counts overlap.

Full Release build and **10,246 solution tests** pass across verified project receipts, including7,305 XSLT tests. The initial sandbox run passes nine projects but45 database fixture tests fail while starting HttpListener with an invalid-handle error; rerunning the unchanged database project outside the sandbox passes all47. Its replacement receipt supplies the verified total; original failures remain preserved. Final refined fixtures separately pass all32 focused checks. Frozen QT3 remains **31,142/0/679**; qt4 gated remains **8,278/0/1,894**. The existing conformance-runner CS8604 warning at Program.cs:1868 is retained. Three catalog documents validate against the native XSD. No gate membership changes.

## Normative basis and gap review

The current specification was fetched and pinned to [`d071fac94ca4f29fa8d515eb8cde5b1f34d22aa5`](https://github.com/qt4cg/qtspecs/blob/d071fac94ca4f29fa8d515eb8cde5b1f34d22aa5/specifications/xslt-40/src/xslt.xml), section `xsl-switch`; SHA-256 `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. Switch applies function-conversion rules to the selector and tests, general comparison using the switch instruction's collation, and returns the selected branch's sequence. The [current draft](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-switch) remains a moving reference.

Contribution base is freshly fetched upstream default `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Existing switch001..009 and901/902 cover basic branches, multiple alternatives, no match, fallback/order, first-match laziness and empty selected-result termination. One original passing candidate duplicated switch009's empty-test control; its inputs were refined to singleton selector-node atomization. Original fixtures and all first-execution receipts are retained separately. The12 prior catalog cases and their expected assertions are unchanged. This is a bounded semantic comparison, not an exhaustive coverage claim.

| Case | Additional boundary | Assertion |
|---|---|---|
| switch-010 | A singleton selector node is atomized before numeric general comparison. | assert-xml |
| switch-011 | An empty selector cannot satisfy the required single atomic item. | XPTY0004 |
| switch-012 | Selector node atomization must still produce only one atomic item. | XPTY0004 |
| switch-013 | A node-valued test sequence is atomized before general comparison. | assert-xml |
| switch-014 | NaN does not equal NaN and the otherwise branch is selected. | assert-xml |
| switch-015 | QName comparison uses expanded names rather than prefix spelling or local name alone. | assert-xml |
| switch-016 | Switch collation governs branch comparison even with a different collation on when. | assert-xml |
| switch-017 | Selector, test and result expressions retain the caller item, position and size. | assert-xml |
| switch-018 | A selected node sequence retains identity and explicitly reversed order. | assert-xml |
| switch-019 | A selected function item is returned without atomization and remains callable. | assert-xml |
| switch-020 | An error in the selected result propagates without falling through to otherwise. | FOER0000 |
| switch-021 | An earlier nonmatching result is not evaluated before the selected constructor. | assert-xml |

## Delivery boundary

The [portable test-only patch](0001-native-tests.patch) is prepared, **not submitted**. Refresh upstream default again immediately before submission, rebase if advanced, record the refreshed exact base and recheck the diff. Do not submit from the older contribution branch. Independent processor confirmation and upstream acceptance remain pending. Array/select PR33 remains separate and under review.

Public libraries remain **1.23.0**. Braid's REQ-135 validation/adoption and native acceptance remain separate; this patch is not a prerequisite for attribute lifecycle validation. No release or Braid pin change. The [evidence](evidence.json) records original/final runs, reductions, exact bases/hashes and full-suite/gate proofs. Source fixtures are under `tests/Bosak.Xslt.Conformance/Fixtures/SwitchBoundaries`; direct tests are `SwitchNativeBoundaryTests`.
