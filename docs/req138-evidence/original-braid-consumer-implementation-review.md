# Bosak REQ-138 consumer implementation review

Date: **2026-10-25**. Reviewed head: **417482421343d72496d0f2dedcd45cfb1f463207**, [Bosak PR452](https://github.com/Fytala-Charles/Bosak/pull/452). Disposition: **No blocking implementation findings within the reviewed bounded conditional profile.** Two nonblocking documentation corrections are recommended below. No release, signed-package adoption or Braid runtime/native acceptance is granted by this review.

## Findings

1. **P3 - Synchronize the registry status.** At the reviewed head, `FEATURE_REQUESTS.md` line673 (registry row) and line1994 (detail status) still say "Pending - scope/API proposed; consumer design review required", while its line12 and the implementation handoff correctly state In progress, reviewed design and received owner implementation approval. Update the current registry/detail status to In progress / consumer implementation review; retain preserved original documents unchanged. This is a tracking inconsistency, not a runtime failure.
2. **P3 - Repair literal question marks in current handoff prose.** `REQ-138-IMPLEMENTATION.md` contains literal `?` punctuation in its date banner, `0?4` enum-range description and `IF01?IF15` references/headings. Replace these with intended punctuation or plain ASCII ranges. Some new canonical status prose has the same artifact. The preserved original request/design files and their hashes should remain untouched. This does not affect compiled APIs.

Source references: [registry row](https://github.com/Fytala-Charles/Bosak/blob/417482421343d72496d0f2dedcd45cfb1f463207/docs/FEATURE_REQUESTS.md#L673), [registry detail](https://github.com/Fytala-Charles/Bosak/blob/417482421343d72496d0f2dedcd45cfb1f463207/docs/FEATURE_REQUESTS.md#L1994), [implementation handoff](https://github.com/Fytala-Charles/Bosak/blob/417482421343d72496d0f2dedcd45cfb1f463207/docs/REQ-138-IMPLEMENTATION.md).

## Exact source and evidence

PR base is fec9ff2fc65440bf77d33150f5ff6a4799b40a61. Implementation-only comparison uses reviewed design35f259ff24bd43c8e5b9927e4f62e38c9fcfef23; the implementation receipt separately identifies final engine source5e268552cfd241262ade266ab00fc716591b48cd. These are not substituted for the actual reviewed PR head. All10 source-manifest Git-blob SHA-256 values match the exact PR head. The scope Git-blob hash remains e6913368245aa644b719e3242a9014ca27e5d0dbe91bd70b0e7fe74438c29fcd, and the retained Braid design review is byte-exact against the local original and receipt344936fc73a0faf5a36bca9f4920fd5703b24298203b2192e62a7001e71e12b6.

Compared against the [consumer request IF01-IF15](CONDITIONAL_WRAPPER_REQUEST.md), [ADR-025](ADR-025-conditional-wrapper-authoring.md) and [accepted design review](BOSAK_REQ138_CONSUMER_DESIGN_REVIEW.md). The isolated checkout `.dependencies/req138-implementation-review` has no tracked modifications. An untracked empty root Directory.Build.props prevents inherited Braid build rules from contaminating upstream test defaults; no engine/test source was patched. Evidence is retained under `.acceptance/req138-implementation-review`.

## Source assessment

- Distinct internally constructed Wrap/Unwrap target types and snapshot-reference ownership prevent cross-operation/foreign authority. Discovery and proposal recheck actual descriptors, complete original alignment, supported ancestry and original/candidate restricted validation. Independent graph growth checks precede candidate inspection. Hypothetical Unwrap bypasses precisely the eligible if edge; existing literal lifecycle ancestry checks remain intact.
- Parent-context ordinal XSLT prefix selection excludes E-only declarations and emits no new namespaces. Condition escaping preserves logical tabs/CR/LF and strict Unicode. The actual candidate I/test slot is contextually compiled, and restricted validation uses deny resolvers rather than retaining ambient acquisition authority. Proposals perform no transformation evaluation.
- Wrap inserts O/X and Unwrap removes exactly O/X through two ascending same-original edits. Component/ownership-aware translations distinguish E's endpoints from suffix/ancestor boundaries. Complete raw source replay and ordinary candidate inspection preserve retained E/C and outside bytes. Actual selection is created I after Wrap or retained E after Unwrap.
- ConditionalProof exhausts created/removed/survivor/merge node partitions, per-owner attributes, child order and approved parent/depth changes. Up to two disjoint boundary merges retain ordered original/candidate fragment ranges and exact bytes; normalized logical text is recomputed over complete concatenated raw whitespace, correctly handling new CR+LF seams. Removed wrapper/test metadata is independently aligned rather than ignored because it disappears.
- The additive token overload shares validation/escaping, applies independent source/text/encoded/evidence/graph/depth bounds and proves unchanged whole graph/per-owner attributes except the owned Expression slot. Legacy one-argument behavior and public signatures remain; refusal values are appended without renumbering. Bounded emitted-byte exports are defensive. A complete-byte-identical candidate remains consumer feedback with no Apply/history/redo loss; different bytes are not NoChange based on output.
- Cancellation is cooperative at bounded traversal/scanning/copy/proof and parse/compile boundaries, with checks immediately before success; no hard preemption inside opaque compiler calls is claimed. Expected failures are typed and unexpected defects propagate. Source, scan, graph/depth and conservative complete-evidence bounds remain distinct from total managed heap or repeated caller exports.

## Independently executed tests

| Run | Result | Scope |
|---|---|---|
| Exact-head Release authoring suite | **1,364 passed, zero failed/skipped** | Includes all57 REQ-138 source cases, legacy expression API and existing authoring regressions |
| Separate public-API consumer assembly | **968 passed, zero failed/skipped** | 904 retained compatibility checks,47 public REQ-138 cases and17 independent Braid probes |

The external assembly uses ordinary ProjectReferences to the exact reviewed source and no friend access/reflection bypass. One prior published-package identity test is excluded because this is an unpublished source candidate. Ten internal-construction cancellation/forged-metadata cases are kept in the57-case source suite rather than copied into the external consumer. No signed/public package verification is claimed by these source-reference runs. Counts overlap and must not be added as distinct conformance totals.

The17 independent probes cover four encodings, paired/self-closing populated elements and repeated names, complete Wrap/Edit/Unwrap round-trip source equality, descending replay of both same-original edits, exhaustive graph partitions and actual selection, both CR/LF seams with fragment bytes/logical values, concurrent proposal isolation and cancellation, opaque public constructors, both unambiguous expression overload signatures and exact candidate engine identity.

Raw TRX/logs: `results/authoring.trx`, `results/consumer.trx`, authoring.log and consumer.log under the evidence directory; counters/SHA-256 and source checks are recorded in review.json. The committed57-case focused receipt also verifies. All three [exact-head CI checks](https://github.com/Fytala-Charles/Bosak/actions/runs/40179859140) pass, including the upstream-reported12,200 solution/LSP tests. Upstream supplied QT3 31,142/0/679, frozen qt4 8,436/0/1,894 across224 gates and current-native Unix22/2/0 remain maintainer evidence, not fresh independent conformance runs here.

## Disposition and remaining gates

Recommend resolving the two documentation nits and proceeding through owner/maintainer merge and release decisions. Source1.26.0 remains an unpublished candidate; Braid remains adopted on published1.25.1. Numeric1.25.2 is separate. Owner implementation approval is recorded upstream; this review grants no merge/publication authorization.

REQ-138 is In progress per the implementation handoff, with the registry synchronization requested above. Signed package/source verification, complete Braid conditional adapter/session/evidence/UI integration, atomic Apply/one-step history/NoChange redo, real-worker ExactBytes/export/hash/reopen and focused native acceptance remain pending consumer gates. No upstream edit/message, Braid dependency/runtime change, commit or push was performed by this review.
