# Bosak REQ-137 consumer implementation review

Date: 2026-10-24. Disposition: **No blocking implementation findings in the bounded profile; all three upstream CI checks pass.** Publication and consumer integration/acceptance remain separate gates.

## Reviewed evidence

Reviewed [PR431](https://github.com/Fytala-Charles/Bosak/pull/431) at exact head `8f5200297475e5b8e88816d8e1226ee31b13d4ab`, base `4de6968cfb6248b2aa553d930bcf3a63bace9ef4`, in isolated `.dependencies/req137-implementation`. Compared the implementation, public models/capabilities, snapshot entry points, shared lifecycle changes, tests and handoff against the [reviewed design](BOSAK_REQ137_CONSUMER_DESIGN_REVIEW.md), [request CR01-CR14](CHILD_ELEMENT_REORDER_REQUEST.md) and [ADR-024](ADR-024-same-parent-child-element-reordering.md).

The original request SHA-256 remains `adec8514200070994275b8267570b66096892b5622ce4befd90224b8e2e823f3`. The copied Braid design review is byte-identical. The scope proposal is identical after normal Git LF/CRLF checkout conversion. Implementation handoff checkout SHA-256: `04fbd635351ab50db1c261d6981e7453ce6361436c28929e26b4d5e642f6a87c`.

## Findings and contract checks

No blocking implementation findings identified. Both directions are delivered together through separate capability/profile/bounds and operation-specific immutable result models. Opaque snapshot-owned authority binds actual selected/neighbor/parent references and direction; proposal rediscovery rechecks eligibility and adjacency. Equal-byte foreign snapshots and old targets cannot authorize new snapshots.

The pipeline emits one exact retained-byte replacement of A+W+B with B+W+A, preserves outside bytes, checks component replay and returns NoChange only after original eligibility, bounds and cancellation gates. NoChange retains original target/selection and grants no candidate ports. Significant/intervening content and unsupported contexts refuse. Numeric whitespace references preserve their raw spelling and require inspected logical whitespace plus independent lexical recognition.

The proof handles changed global preorder explicitly: it exchanges child positions only under the common parent, maps complete internal subtrees and the separator with checked coordinate translation, verifies raw bytes, all node kinds/names/values and per-owner attributes, rejects duplicate mappings, and exhausts both graphs. Ancestor extents stay fixed. Actual moved selection and fresh snapshot ports support subsequent moves and existing lifecycle operations.

Independent source/exchange/evidence, node/attribute/depth bounds precede candidate allocation and recursive inspection. Copied-evidence budgeting is conservative and may refuse below the physical-source maximum. Cancellation uses bounded cooperative work units rather than preempting every library call. Validation denies ambient acquisition; unexpected defects are not broadly converted into input refusals. Shared lifecycle modifications expose unchanged preflight helpers internally; existing Rename/Delete behavior is otherwise unchanged.

## Executed checks and limits

- Independently executed Release literal-element regression filter: **235 passed, zero failures/skips**, including **71 reorder cases**. Covers four encodings, both directions, absent/raw/numeric separators, unequal populated/repeated-name subtrees, inverse movement, NoChange, exact replay/bijection, authority, refusals, bounds, cancellation, defensive exports, concurrency and lifecycle composition.
- Independently executed full Release XSLT test project: **8,135 passed, zero failures/skips** (includes the focused cases; totals are not additive). TRX `.acceptance/req137-review/req137-xslt.trx`, log `.acceptance/child-element-reorder/implementation-xslt-tests.log`.
- Local TRX: `.acceptance/req137-review/req137-lifecycle.trx`; log: `.acceptance/child-element-reorder/implementation-tests.log`.
- The first local build inherited Braid rules because upstream has no root build-props boundary, producing unrelated CS1591 test documentation errors. An untracked checkout-local empty `Directory.Build.props` isolates normal upstream defaults; no tracked engine or test file was altered. The corrected run passes. This is an environment correction, not an engine fix.
- Upstream [CI40065218522](https://github.com/Fytala-Charles/Bosak/actions/runs/40065218522): all three checks pass at the unchanged reviewed head: Build & test (net10.0), Windows wrapper controls and highlighting. Final status independently checked on 2026-10-24.
- Bosak's handoff reports 11,244 solution plus84 language-server passes and unchanged frozen QT3 31,142/0/679 and qt4 8,434/0/1,896. These are maintainer results, not independently rerun full-solution/conformance results here.

Source version1.25.0 is an unpublished feature candidate; public1.24.0 remains adopted. Owner implementation approval is reported in the upstream handoff; this consumer review grants no merge/release authorization. Next: maintainer/owner merge and release decisions, independent signed public-package verification, then one Braid integration PR for both directions. CR12-CR14 consumer review/atomic Apply/history/redo/worker/export/reopen and native acceptance remain pending; engine tests do not establish them. No upstream message/edit, Braid runtime/dependency change, commit or push was performed.

## Final CI follow-up - 24 October 2026

PR431 remains open at the exact reviewed head 8f520029; all three CI checks now pass. The consumer implementation review is complete with no blocking findings within the declared profile. Latest published release remains1.24.0; no1.25.0 signed packages are available for adoption yet. Recommend proceeding through Bosak owner/maintainer merge and publication decisions. Braid will verify the published source/package provenance and smoke tests before integrating both directions together. No upstream notification, approval, merge, commit or push is performed by this follow-up.
