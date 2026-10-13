# Array/member assertion review — 2026-10-12

The six assertions were reviewed against the [pinned array-construction source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current rendered specification](https://qt4cg.org/specifications/xslt-40/Overview.html#array-construction). This is a specification review, not execution by an independent XSLT processor. Independent processor confirmation remains outstanding.

| Case | Derivation |
|---|---|
| array-027 | Three explicit member wrappers produce three slots, including the empty second slot. An ordinary empty sequence emits no item, giving two slots in the comparison array. |
| array-028 | Each member sequence constructor emits one wrapper even when its value is empty. The final wrapper contains both after and tail, producing a two-item third member. |
| array-029 | A map, a two-member array and an empty array are each singleton items. The empty array has size zero but contributes one item to its enclosing member. |
| array-030 | The selected original nodes are C and A in that explicit order, followed by B in a separate member. No node constructor or copying instruction intervenes. The identity assertions and C\|A projection follow from retaining the selected items. |
| array-031 | for-each selects three lines. It produces three members even when B returns an empty sequence. The other evaluations have positions 1 and 3 and size 3. |
| array-032 | The member constructor returns a JNode wrapping A; the following sequence instruction returns ordinary B. The two-item per-focus result contains a JNode and therefore raises XTTE4060 before consumption or serialization. |

The original catalog XML outputs match these derivations. No native stylesheet, fixture or expected outcome was changed during the engine correction. The source pin and portable patch remain the first-batch evidence; the original five-pass/one-failure run is historical evidence.

## Root cause and correction

Bosak already has XdmJNode in its core. The older XSLT array-member paths instead returned their raw sequence value, with a stale comment claiming there was no wrapper. This erased the distinction between one wrapped member and ordinary emitted items before the for-each array constructor could inspect it.

The correction uses the existing JNode representation in both sequence and template evaluators. Without for-each, array construction unwraps each emitted JNode into one member. With for-each, it accepts a sole JNode and unwraps its value, retains an ordinary sequence, and raises XTTE4060 when a multi-item result contains a JNode. Array-member construction recursively replaces JNode items with their values, preserving empty sequences and grouped values without flattening map or array containers.

A syntax-only check for an array-member child would miss wrappers supplied by variables, expressions or control-flow instructions and could reject a branch that emits nothing. The regression cases exercise direct, empty-valued, conditional and variable-carried mixed results, plus sole wrappers, grouped values and recursive unwrapping. Existing ordinary-sequence, grouping and nested-array tests remain regression coverage.

## Scope and next action

This correction covers array/member construction. It does not certify all JTree navigation, templates, type matching or serialization. The detailed construction algorithm also rejects multiple JNodes in one per-focus result; its error prose emphasizes mixing with non-JNodes. Preserve this distinction when proposing additional edge cases upstream.

Run another processor against the six native cases before presenting the batch as independently confirmed. The upstream PR remains a local draft. N01 stays in progress until review and submission are complete.


## Submission update

[Upstream PR #22](https://github.com/qt4cg/xslt40-test/pull/22) is open. See [submission details](SUBMISSION.md). Earlier no-submission statements describe the pre-submission state; independent confirmation remains outstanding.
