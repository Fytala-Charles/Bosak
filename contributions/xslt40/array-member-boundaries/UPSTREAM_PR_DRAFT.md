# Submitted upstream PR text

Submitted as [xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22) on 2026-10-12. The historical filename is retained for existing links. Review/acceptance and independent processor confirmation are pending.

Adds six cases to the existing native array test set:

| Case | Boundary |
|---|---|
| array-027 | Explicit empty member versus an empty sequence that emits no member |
| array-028 | Empty sequence-constructor member and a grouped two-item member |
| array-029 | Retained map, nested array and empty-array values |
| array-030 | Original source-node identity and deliberately reordered grouped nodes |
| array-031 | for-each focus (item, position, size) and preserved empty result slots |
| array-032 | Required XTTE4060 for a per-focus result mixing an array-member JNode with an ordinary item |

Expected outcomes derive from the [array-construction rules](https://qt4cg.org/specifications/xslt-40/Overview.html#array-construction), reviewed against [specification source revision e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). Existing grouped/nested/map/JNode cases were reviewed before adding these boundaries. No established test, expected result or root catalog registration is changed.

Validation: the native test-set catalog validates against the suite schema. All six new cases pass using the Bosak XSLT 4.0 catalog adapter. array-032 initially exposed missing error enforcement in Bosak; that implementation was corrected without changing this test's expected error. Exact case outcomes are used because Bosak's substring filters also select inherited square-array cases.

I do not have an independent XSLT 4.0 processor available. These assertions have been checked against the specification, but have not been confirmed by a second implementation. A reviewer cross-check would be welcome, especially for the for-each focus/empty-result and XTTE4060 cases.

The tests use the suite's native catalog/environment structure; there are no Bosak dependencies in the contributed files. [Bosak](https://github.com/Fytala-Charles/Bosak) provides the adapter and [documented specification/consumer examples](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md), with partial XSLT 4.0 coverage. [Assertion review and execution evidence](https://github.com/Fytala-Charles/Bosak/tree/main/contributions/xslt40/array-member-boundaries) are available for reference.
