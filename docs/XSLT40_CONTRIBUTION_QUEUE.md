<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XSLT 4.0 contribution priority queue">
  <br><br><h1>XSLT 4.0 Contribution Priority Queue</h1>
  <p>New instructions, likely everyday use and evidence before submission</p>
</div>

## Decision and ranking

Start with **N01: xsl:array / xsl:array-member**, then xsl:select and xsl:switch. These are new element forms in XSLT 4.0. Array construction already existed as XPath expressions in 3.1; a new instruction does not mean the outcome was impossible in earlier releases. The requested first lane covers new elements, while additions to existing elements and inherited regression work stay in separate lanes.

Reviewed 2026-10-12 against [pinned XSLT source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and [xslt40-test 6205faf9](https://github.com/qt4cg/xslt40-test/tree/6205faf9f32ace6338ac245c41f8eb9a1447926f). The [draft introduction](https://qt4cg.org/specifications/xslt-40/Overview.html#whats-new-in-xslt4) emphasizes map/array processing and stronger type constraints. Live draft links are navigation; the source pin and SHA-256 in the [machine-readable queue](XSLT40_CONTRIBUTION_QUEUE.json) define this review.

Likely use is an editorial estimate from recurring integration, reporting and transformation tasks, not telemetry or an adoption survey. Order combines breadth of use, usefulness of a small portable test contribution and draft/implementation risk. Bosak's ability to run a scenario helps verification; it does not determine the specification's expected result. A high-value red test can still be useful evidence.

| Order | New element family | Estimated use and real case | Next contribution review |
|---|---|---|---|
| N01 | xsl:array / xsl:array-member | High: API response rows, invoice lines, nested batches | Mixed empty/grouped members, nested containers, order and node identity. Audit existing cases before claiming a gap. |
| N02 | xsl:select | High: readable multiline JSON/map expressions in stylesheet content | Expression text/escaping/whitespace, current focus, result type/cardinality and static errors. This is not the old select attribute. |
| N03 | xsl:switch | High: status routing, category mapping and dispatch | Branch precedence, fallback/no match, input type/cardinality and branch evaluation. |
| N04 | xsl:item-type | Medium–high: reusable payload/callback contracts | Type reuse and visibility, unknown names and mismatches across modules. |
| N05 | xsl:record-type / xsl:field | Medium–high: typed order/customer records | Align declarations, fields/coercion and constructor expectations with the current draft first. |

Specification sections: [array construction](https://qt4cg.org/specifications/xslt-40/Overview.html#array-construction), [xsl:select](https://qt4cg.org/specifications/xslt-40/Overview.html#select-instruction), [xsl:switch](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-switch), [named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), [named record types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types). This is a first contribution shortlist, not an exhaustive inventory of new 4.0 elements.

## Coverage evidence and limits

Namespace-aware scanning of parseable upstream *.xsl files found these distinct file footprints:

| Queue family | Stylesheet files mentioning the elements | Existing location to inspect |
|---|---:|---|
| N01 | 35 (union: array 31, array-member 24) | tests/insn/array and tests/type/arrays |
| N02 | 36 | tests/misc/seqtor, also type-alias and other sets; no dedicated instruction set inferred from the legacy select attribute set |
| N03 | 12 | tests/insn/switch and tests/insn/iterate |
| N04 | 24 | tests/type/type-alias and tests/type/record |
| N05 | 1 | tests/type/record/record-023.xsl |

These counts are **not test counts, eligible 4.0 cases, completeness percentages or proven missing coverage**. Embedded/non-xsl/unparseable sources are outside the scan. One stylesheet can serve many cases; inherited dependency branches can be ineligible. The raw queue retains exact file paths for inspection. Candidate gaps remain unconfirmed until assertions and dependencies are reviewed.

The record stylesheet includes older required=no field forms, while the pinned draft's change notes say record fields are no longer optional. The suite also retains an xsl-record set with older naming. This is a draft-alignment review item, not justification to delete or rewrite tests without understanding their versions and history.

Targeted Bosak probes:

| Selection | Observed result | Interpretation |
|---|---|---|
| switch / switch-001 | 1 passed / 0 failed / 9 skipped | One existing new-instruction case executes; other cases remain unverified. |
| array / array-004 | 1 passed / 0 failed / 173 skipped | Grouped-member construction is already tested and passes; do not propose its simple duplicate. |
| array / array-001 | 2 passed / 1 failed / 171 skipped | The actual array-001 fails XTSE0740 on an unqualified function name. Two unrelated square-array cases also match the substring filters. This is not proof the array instruction is broken. |

Filters are substrings and skipped counts include unselected cases. Logs and receipts are under ignored .artifacts/xslt40. The targeted probes use a clean upstream checkout at the recorded pin; full family readiness remains unknown.

## First bounded batch: N01

Use a realistic order-export scenario: each line contributes an array member, some optional data is absent, and a member can itself contain several values or a nested map/array. Determine whether explicit empty members, no emitted member and a multi-value member remain distinguishable through downstream access/serialization.

Before writing tests:

1. Review all relevant native array cases and their dependencies, especially existing grouped/empty/nested scenarios.
2. Map proposed scenarios to the exact pinned rules and identify assertions not already covered.
3. Choose a small batch of independently useful cases with one clear semantic boundary each. Keep fixture business context realistic and minimal.
4. Author them in the existing upstream tests structure and validate their catalog XML. Use Bosak as the adapter and record any failures; seek another processor's confirmation where available.
5. Prepare a focused upstream patch with clause references, expected outcomes and honest run evidence. Do not change an established expected result to fit Bosak.

Local contribution branch: bosak/array-member-review in .upstream/xslt40-test. It is clean at the baseline; no new upstream tests or submission yet. The [adapter/setup guide](XSLT40_UPSTREAM_TESTING.md) provides commands. The matching [Bosak catalog entry](xpath40-catalog/xslt-instructions/xsl-array.md) supplies motivation and earlier-version examples, but is not the normative source of expected results.

## Secondary lanes and lifecycle

| Lane | Items | Scheduling |
|---|---|---|
| Changed contracts on inherited instructions | xsl:map select/duplicates; xsl:if then/else; instruction separators | Follow the first new-element batch. Test only the 4.0 additions, rather than inherited functionality. |
| Shared XPath 4.0 inside stylesheets | Keyword/default arguments, record/map/array expressions and new mapping syntax | Coordinate with qt4tests to add XSLT context/profile evidence rather than duplicate pure XPath tests. |
| Inherited infrastructure/regressions | Schema-awareness issue #90; existing JSON -0/-0e0 finding | Retained triage, moved behind the requested new-element work. |

Track each item as queued → auditing → patch drafted → Bosak validated (or failures recorded) → independently checked where available → upstream PR → accepted. Record the candidate gap, source pin/clause, native cases, patch/commit and run receipts at each transition. Acceptance is an upstream maintainer decision. Re-rank after each batch or relevant draft change; do not treat the initial order as measured fact.

No engine/API/adapter changes, release or upstream submission in this queue step. Version remains 1.1.3; REQ-126 stays In Progress and the separate catalog equality/collation work remains recorded.


## Queue validation

Queue/source SHA-256 and section anchors, five ordered families, exact distinct-file inventories, three recorded Bosak probe counts, clean upstream revision, local links/code fences and rendered queue/setup/canonical documents verified. PR #199 baseline CI passed 6,806 tests; source, adapter and test implementation are unchanged, so the full suite/manual engine gates were not repeated for this planning/documentation-only step. Probe failures remain visible rather than reclassified as passes.


**First upstream test batch — 2026-10-12:** N01 now has a [six-case native array/member batch](../contributions/xslt40/array-member-boundaries/README.md), committed locally on bosak/array-member-review at e72f1b0e8511f78478e66c112201317cfe7b7ad1. Five exact new cases pass; array-032 exposes missing XTTE4060 enforcement and returns A|B. Normative expectations are retained. A portable mail patch, source/expected-outcome mapping, clean-commit receipts and an upstream PR draft preserve the work; no upstream submission or independent processor confirmation yet. Version 1.1.3, REQ-126 In Progress, no Bosak engine/API/adapter change. Next: independent assertion review and separate error triage before submission.


**Array/member correction — 2026-10-12:** The [array/member assertion review](../contributions/xslt40/array-member-boundaries/ASSERTION_REVIEW.md) confirms the six native expectations. Bosak now preserves existing core JNode wrappers in both XSLT evaluation paths, unwraps member values at array construction, and raises XTTE4060 for multi-item per-focus results containing a JNode. All six unchanged native cases pass; the original five-pass/one-failure receipts remain historical. Five additional regression checks cover direct, empty, conditional and variable-carried errors plus valid grouped/nested wrappers. Source candidate 1.1.4 is unpublished; 1.1.3 remains the published version. N01 and REQ-126 stay In Progress; independent processor confirmation and upstream submission remain outstanding.


**Upstream submission — 2026-10-12:** The first six-case native array/member batch is [submitted as xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22). All six exact cases pass with Bosak; specification review is recorded. No second processor was run: independent cross-check was requested in the PR. [Submission details](../contributions/xslt40/array-member-boundaries/SUBMISSION.md) explain the Saxon PE/EE and partial-for-each limitation and the decision to seek upstream review without making local independent execution a prerequisite. Review/acceptance remain pending; N01 and REQ-126 stay In Progress. Bosak fix PR #202 merged after CI 38295568498 passed (6811 unit tests); source 1.1.4 remains unpublished, public release 1.1.3 unchanged.
