<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak record draft alignment and cancellation reliability review">
  <br><br><h1>Record draft alignment and cancellation reliability</h1>
  <p>Pinned specification review and deterministic test execution</p>
</div>

## Record cluster review — 16 October 2026

The unchanged upstream cases `record-003/005/016` were selected from the [N04/N05 baseline](XSLT40-N04-N05-BASELINE.md) for review, not preclassified as engine defects. Fresh upstream default remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. The current specification source is pinned to `qt4cg/qtspecs` revision `72f222929768b411d8e88d96a1a8740483290078`, `specifications/xquery-40/src/expressions.xml`. The source bytes, hash and review receipt are preserved separately from the original baseline.

The pinned draft requires map-to-record coercion to reject keys outside the target field set with XPTY0004; explicit casting is the mechanism that can discard them. Records used as functions accept only declared field names, so an unknown field is also a type error. Bosak's observed errors in these three cases are consistent with those clauses. This establishes an upstream/current-draft alignment discrepancy, not an engine defect or an independently confirmed upstream correction.

| Case | Unchanged upstream expectation | Bosak observation | Draft clause |
|---|---|---|---|
| record-003 | Successful complex addition | XPTY0004 for surplus x while binding the narrower record parameter | [Item coercion rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-item-coercion-rules), XML lines 8578–8619 |
| record-005 | Successful complex addition | Same surplus-field coercion error | Same coercion clause |
| record-016 | Unknown record-as-function field yields empty | XPTY0004 for xyz | [Records as functions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-records-as-functions), XML lines 22525–22554 |

Permalinks to the [pinned coercion source](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xquery-40/src/expressions.xml#L8578) and [pinned dynamic-call source](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xquery-40/src/expressions.xml#L22525) define the review. Live rendered draft links are navigation. No assertion was weakened, no skip/error alias was added, and no engine behavior changed. Native family failures remain visible. No upstream checkout edit, contribution branch, submission or external message was made; established expected-result corrections require upstream consensus.

Existing `RecordTypeTests` distinguish rejecting surplus coercion, discarding surplus keys on explicit casts, rejecting unknown record-as-function lookups and permissive `map:get`. Existing XSLT structural-binding tests cover valid conversion and rejecting extra fields. Those controls remain unchanged and run in the full solution.

## Deterministic cancellation test

The previous test used a seven-million-character comment, Task.Run and CancelAfter(1). The operation could finish before the timer requested cancellation; a larger input or delay would not establish ordering. Prior failing CI and successful rerun receipts remain historical and unchanged.

The replacement uses the existing internal descriptor/snapshot constructors to create a test-local child-list wrapper. Its first traversal occurs inside the public GetValueOfInsertionTarget call, after the initial cancellation check. The wrapper synchronously requests cancellation; the test requires a Cancelled refusal, no target and byte-identical retained source. A second theory case traverses the same graph without cancellation and requires a successful target referencing the original parent. No production source, public API, callback/hook or package version is changed. This tests cancellation during descriptor traversal; it does not claim cancellation at every later insertion phase.

## Verification and continuation

Release build: zero errors, 22 warnings. All 8,625 solution tests and 84 language-server tests pass. The two focused cases pass, including 20 consecutive repeat runs (40 cases). Documentation rendered with markdown-it and canonical Fytala CSS. AGENTS.md conventions remain applicable.

Validation results and hashes are recorded in [the review receipt](XSLT40-RECORD-DRAFT-RELIABILITY.json). This slice preserves the original baseline receipts; draft discrepancies are not reclassified as passing tests. Engine gates need no rerun for this test/documentation-only change; previous frozen counts remain historical evidence. REQ126 remains In Progress, REQ128 engine delivery remains Implemented, public libraries 1.8.0 and VS Code 0.2.4/engine1.8.0 stay immutable. Braid acceptance is independent.

Next review: check the remaining record/named-type pattern cluster against the same pinned draft before selecting another engine change. Keep optional syntax, subtyping, assertion handling and xsl-record instruction coverage separate. The rejected extra-field cluster is no longer the next implementation target.
