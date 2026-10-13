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


**Remaining priority element batches — 2026-10-12:** Four additional native batches are submitted: [select PR #23](https://github.com/qt4cg/xslt40-test/pull/23), [switch PR #24](https://github.com/qt4cg/xslt40-test/pull/24), [item-type PR #25](https://github.com/qt4cg/xslt40-test/pull/25), [record-type PR #26](https://github.com/qt4cg/xslt40-test/pull/26). Eight new cases validate against native catalog schemas: switch 2 pass; select 2 fail (empty select results), item-type 2 fail (XPST0051), record-type/field 2 fail (constructor XPST0017). Normative outputs are retained, with [portable patches and detailed example/evidence pages](../contributions/xslt40/README.md). No independent processor was run; reviewer cross-check requested in each PR. These are initial priority-family contributions, not complete XSLT 4.0 coverage. N02–N05 and REQ-126 stay In Progress while upstream review and engine triage are pending. Source 1.1.4 remains unpublished; public release 1.1.3.


**Select body evaluation — 2026-10-12:** Bosak now recognizes and evaluates xsl:select body XPath in template, function, simple-content and accumulator-rule paths, preserving literal newlines/CDATA and current focus without TVT expansion. Declared result coercion, collation restoration, fallback/content validation and streamed-body analysis are covered by 14 focused regressions. Both unchanged native contribution cases pass; [post-fix evidence](../contributions/xslt40/select-boundaries/POST_FIX_RESULTS.json) retains the broader seqtor result 78/5/9. The five remaining failures concern expression-body namespace declarations, so complete select conformance is not claimed. Original failed receipts and upstream PR #23 remain unchanged; independent confirmation/acceptance are pending. Source candidate is 1.2.0 under the Minor-for-new-features convention, unpublished; public release remains 1.1.3. N02 and REQ-126 stay In Progress.


**Enumeration alias slice — 2026-10-12:** Bosak source candidate 1.2.0 (unpublished) now resolves xsl:item-type enumeration aliases in XPath instance-of and treat expressions, including AVTs, namespace rebinding, EQNames and occurrence indicators. Both unchanged native cases type-alias-023/024 pass; broader type-alias-filter run is 3 pass/29 fail/0 skip (includes inherited related cases). Other alias types, named XSLT @as conversion, package exposure and complete recursive/schema-collision validation remain unsupported. Duplicate winning declarations raise XTSE4030; lower-precedence definitions remain overridable. Original failure receipts are preserved; upstream PR #25 review and independent confirmation are pending. N04 and REQ-126 remain In Progress; public release remains 1.1.3. Next: generated nominative record constructors, with wider alias support and select namespace declarations tracked separately.


**Named record constructor slice — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI.


**Select namespace follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports expression-local XPath 4.0 namespace declarations in the XPath API and xsl:select bodies. Prefix bindings override host bindings; an optional default element namespace must come first. Default ##any matches unprefixed element axis name tests across namespaces while preserving attribute/kind-test boundaries and XML Schema item type resolution. Namespace contexts restore after success or failure. All unchanged native seqtor-140–146 cases pass; the broader seqtor-filter run is 83 pass/0 fail/9 skip. The nine known harness skips, independent confirmation and upstream PR #23 acceptance remain outstanding, so this does not establish complete XSLT 4.0 conformance. N02 and REQ-126 remain In Progress; public release remains 1.1.3. See [current namespace evidence](../contributions/xslt40/select-boundaries/NAMESPACE_RESULTS.json) and the [pinned XPath namespace declaration source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/expressions.xml). Twenty-three regressions were added; catalogue entry/check counts are unchanged. Next: named item-type conversion and remaining record coercion gaps, after this slice passes CI. Earlier select counts are historical.


**Enumeration binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now converts named enumeration aliases at XSLT variable, parameter and result boundaries. Namespace/EQName aliases and occurrence indicators resolve from the declaring instruction; atomization and membership use XPath function conversion rules. Direct/dynamic function metadata uses the same definitions; private package aliases are isolated and restored. Sixteen regressions cover valid values, membership/cardinality errors, source attributes, record fields, template/select/function boundaries and mixed-version refusal. The invoice currency fixture now returns EUR; invalid XYZ still raises XTTE0570. Original native type-alias-023/024 still pass; broader filtered family remains 3/29/0, with structural/recursive/schema/package gaps outstanding. N04 and REQ-126 stay In Progress; no complete alias or package-conformance claim. Public release remains 1.1.3. See [binding evidence](../contributions/xslt40/item-type-boundaries/BINDING_RESULTS.json) and [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Catalogue counts remain 67 entries / 2,578 feature + 24 chapter checks. Next: structural aliases and remaining named-record coercion, after CI. Earlier named-enumeration binding refusals are historical.


**Atomic and array binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports atomic and array item-type aliases at XSLT declaration and function boundaries. Array member types can use forward aliases, nested arrays, optional member sequences and enumeration aliases. Definition namespace bindings are preserved; outer occurrence indicators apply to arrays rather than their members. Supported direct/indirect recursive aliases raise XTSE4035. XPath 4.0 array coercion converts member sequences without atomizing the outer array; the XPath 3.1 path remains unchanged. Native type-alias-001/002 now pass; the filtered family is 6 pass/26 fail/0 skip, including unchanged contribution cases 023/024. Non-enumeration aliases in XPath instance-of/treat, general map/function/choice aliases, complete recursive validation, schema collisions and package exposure remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. See [separate evidence](../contributions/xslt40/item-type-boundaries/ARRAY_BINDING_RESULTS.json). Existing catalogue counts and expected outcomes are unchanged. Earlier slice results are historical. Next: structural aliases in XPath type expressions, other structural alias forms, then remaining named-record coercion.
