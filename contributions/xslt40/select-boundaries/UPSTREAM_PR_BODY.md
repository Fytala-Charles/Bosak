Adds two native select boundary cases to the existing seqtor group.

Reviewed seqtor select cases 113/114 and 124–144: mixed text, empty/error results, variables, child/fallback rules, type checking/coercion and namespace declarations are already covered. New cases isolate literal line-feed preservation plus both quote characters/CDATA less-than under expand-text, and sequence-valued item/position/size focus projected per output row. Current-item coverage exists; the second case complements it with explicit position/size/cardinality observations.

- **seqtor-145**: CDATA map expression preserves a literal newline and both quote characters under expand-text. Bosak: FAIL seqtor-145: XTTE0570: Empty sequence not allowed for type map(*).
- **seqtor-146**: Sequence-valued select body evaluates with the current item, position and size. Bosak: FAIL seqtor-146: Result mismatch.

Expected outcomes follow the [current select-instruction rules](https://qt4cg.org/specifications/xslt-40/Overview.html#select-instruction), reviewed against [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). No established case or expected result is changed. The catalog validates against the native schema.

Exactly 0 new cases pass and 2 fail with the Bosak adapter; processor failures are retained, not converted into expected errors. No independent XSLT 4.0 processor was available. Specification-derived assertions have been reviewed, but a reviewer cross-check is requested before acceptance.

These files have no Bosak dependencies. [Bosak](https://github.com/Fytala-Charles/Bosak) provides the adapter and [specification/consumer documentation and contribution queue](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md); its 4.0 support is partial.
