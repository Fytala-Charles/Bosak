<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak function signature and keyword contract audit">
  <br><br><h1>Signature and Keyword Contract Audit</h1>
  <p>Registrations, pinned declarations, defaults and reproducible consumer boundaries</p>
</div>

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S1-S2-S5. Function-call/ref/dynamic forms and HOF/profile controls are mapped. C01-C03 remain the signature/default/option/type evidence, not a new grammar-completeness proof. C02 F01-F14 and C03 T01-T05 retain function-specific conversion/options/host limits. Registration count and a passing HOF example do not accept every call contract. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.

**REQ-126 C02 complete — 26 October 2026:** [Function/options reconciliation](FUNCTION-OPTIONS-RECONCILIATION.md) reconciles 30 entries/74 function surfaces with explicit supported boundaries and 14 follow-up groups. Release build and all 21,798 solution tests pass; all 3,148 catalog examples pass separately. The 195 added profile observations include 118 matching controls, two unmet controls for the new `atomic-type-annotation(())` defect, and 75 diagnostics. Eleven closure workpackages remain; C03 is next. C02 closes review scope, not AC-01–04 or the retained defects. Skip fixes, Braid and extension work remain deferred. Original receipts and frozen gates are preserved; no engine/version/release change.

**REQ-126 C01 complete — 26 October 2026:** [Current inventory and ownership record](FUNCTION-INVENTORY-REFRESH.md) refreshes 316 names against published 1.26.3 and pinned current draft b4723ff5: 119 registered 4.0-only names (70 non-constructors, 49 focus constructors), plus 197 existing names. The retained inventory missed `array:members` and `array:of-members`. All 1,581 profile lookup observations completed without probe errors; four changed declarations and remaining semantic/default/empty/error reviews have explicit C02/C03/C06 owners. Twelve closure workpackages remain open; REQ-126 and AC-01–04 remain partial. Finish these packages before skip fixes, Braid or VS Code extension work. Engine, APIs, tests, version and frozen gates are unchanged; existing passing source/gate evidence is inherited. Earlier counts and checkpoint notices are historical.

## Finding and scope

**D2 has a first comparison matrix; behavioral completion remains partial.** Reviewed 2026-10-12 against Bosak `3a078fa4cb8b17233837670d41e0ef951da86cd1`, published version 1.1.2. REQ-126 stays In Progress. This audit covers all **68 non-constructor 4.0-only names** and **20 selected existing functions**, with **148 exact registrations**. The existing-function sample is not an exhaustive inventory of changed 3.1 behavior. The 49 focus constructors have their [separate audit](operators-and-syntax/focus-functions.md).

The [machine-readable comparison](audit-signature-contracts.json) records live registrations, coarse dispatch kinds, optional sequence-type metadata, keyword names/default expressions and pinned function declarations. Source evidence: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), [FunctionSignature](../../src/Bosak.XPath.Runtime/Functions/XPathFunction.cs), and [QT4CG F&O catalog e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). Current review pins do not reconstruct historical adoption.

## How to read the comparison

37 families have no keyword metadata; 47 have names matching a pinned signature; four have different names. Within the 68-name scope, 31 have keyword metadata and 37 lack it. Metadata absence is distinct from absence of positional registration. The char case demonstrates that distinction; behavior is not inferred for every absent-metadata family.

Exact registrations are implementation dispatch entries. Pinned call arities are ranges derived from the declared required/default arguments, not a claim that every named reference/lookup is available. fn:replace is the only selected range difference: the draft permits two arguments, while exact registrations are 3 and 4. Its positional/reference two-argument calls refuse XPST0017 and lookup returns zero; its keyword default form returns 42 for INV-42 with pattern INV-.

Default comparison normalizes only quoted string literals and empty-map spellings. Matching text does not prove evaluation timing, focus capture or complete omission/default-on-empty behavior. jkey, jvalue and node-name have a required parameter in keyword metadata where the pinned declaration supplies `.`. Their registered zero-argument forms are present; this metadata difference alone does not prove those calls fail. Coarse dispatch kinds are not precise draft sequence types or evidence of enforced coercion.

The 148 lookup fixtures verify exact registration availability and default 3.1 visibility; experimental scan is also checked against frozen visibility. They do not invoke every overload with valid data. The **39 behavior fixtures** provide separate value/error evidence for selected defaults, keywords and hash options. The [detailed consumer README](examples/signature-contracts/README.md) publishes complete invoice input/output and runnable files. Original matrix delivery: **187 new checks**; catalog then contained 67 entries, 1,473 feature + 24 chapter checks = 1,497. The subsequent text review below supersedes that count.

## Consumer boundaries

| Area | Reproduced outcome | Follow-up |
|---|---|---|
| element-to-map | Implemented keyword input; pinned element refuses XPST0017 | Reconcile keyword spelling with compatibility policy. |
| map-to-element | Implemented keyword input; pinned map refuses XPST0017 | Reconcile keyword spelling with compatibility policy. |
| parse-uri | Implemented keyword uri; pinned value refuses XPST0017 | Keep positional URI alternative; reconcile spelling. |
| compare | Implemented comparand1/comparand2; pinned value1/value2 refuse XPST0017 | Existing documented drift is now in the matrix. |
| char | Positional 65 returns A; keyword call refuses XPST0017 | Review missing metadata against pinned declarations, family by family. |
| replace defaults | Two positional/reference arguments refuse; keyword default succeeds | Scope arity/default and reference/lookup semantics together before an engine fix. |
| Keyword calls after two let bindings | Tested hash keyword call refuses hash#0 XPST0017, while one-let/concat-only probes succeed | Investigate expression traversal before changing engine behavior. Full invoice positional call succeeds. These fixtures establish the observed boundary, not its root cause. |
| Hash options | Empty/empty-map and an unknown named option preserve the digest; non-map options refuse XPTY0004, including tested empty input | No named option semantics are advertised. The pinned contract leaves extra parameters implementation-defined; unknown-key tolerance is not evidence of a missing standardized option. |
| Hash algorithms | BLAKE3 and CRC-32 refuse FOHA0001 | Confirmed missing mandatory pinned algorithms; separate from option-name policy. |

## Comparison matrix

The first 68 rows correspond to the scoped name/arity inventory; the final 20 are the selected existing-function sample. Each lookup link is a complete executable file. Full raw types, defaults, source references and behavior case IDs are in JSON.

| Function | Exact registrations | Pinned call arities | Keyword names | Default expressions | Evidence |
|---|---|---|---|---|---|
| [`array:build`](functions/import-array-batches.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/array-build-1.xpath) / [container cases](examples/container-contracts/README.md) |
| [`array:empty`](functions/import-array-batches.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/array-empty-1.xpath) / [container cases](examples/container-contracts/README.md) |
| [`array:items`](functions/import-array-batches.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/array-items-1.xpath) / [container cases](examples/container-contracts/README.md) |
| [`array:slice`](functions/import-array-batches.md) | 1, 2, 3, 4 | 1, 2, 3, 4 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/array-slice-1.xpath) |
| [`array:sort-with`](functions/sort-with.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/array-sort-with-2.xpath) |
| [`fn:all-different`](functions/sequence-distinctness.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-all-different-1.xpath) |
| [`fn:all-equal`](functions/shipment-event-sequences.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-all-equal-1.xpath) |
| [`fn:atomic-equal`](functions/atomic-equal.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-atomic-equal-2.xpath) |
| [`fn:atomic-type-annotation`](functions/numeric-import-diagnostics.md) | 1 | 1 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-atomic-type-annotation-1.xpath) |
| [`fn:build-dateTime`](functions/month-end-cutoffs.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-build-dateTime-1.xpath) / [date/time cases](examples/date-contracts/README.md) |
| [`fn:build-uri`](functions/invoice-api-uris.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-build-uri-1.xpath) / [URI/utility cases](examples/utility-contracts/README.md) |
| [`fn:char`](functions/customer-text-cleanup.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-char-1.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:characters`](functions/customer-text-cleanup.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-characters-1.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:collation-available`](functions/supplier-offer-ranking.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-collation-available-1.xpath) / [URI/utility cases](examples/utility-contracts/README.md) |
| [`fn:contains-subsequence`](functions/shipment-event-sequences.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-contains-subsequence-2.xpath) |
| [`fn:csv-doc`](functions/csv-parsing.md) | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-csv-doc-1.xpath) |
| [`fn:csv-to-xml`](functions/csv-parsing.md) | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-csv-to-xml-1.xpath) |
| [`fn:days-in-month`](functions/month-end-cutoffs.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-days-in-month-1.xpath) / [date/time cases](examples/date-contracts/README.md) |
| [`fn:decode-from-uri`](functions/invoice-api-uris.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-decode-from-uri-1.xpath) / [URI/utility cases](examples/utility-contracts/README.md) |
| [`fn:do-until`](functions/bounded-retries.md) | 3 | 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-do-until-3.xpath) |
| [`fn:drop-while`](functions/ordered-queue-prefix.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-drop-while-2.xpath) |
| [`fn:duplicate-values`](functions/sequence-distinctness.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-duplicate-values-1.xpath) |
| [`fn:duration-to-seconds`](functions/timestamp-deadlines.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-duration-to-seconds-1.xpath) / [date/time cases](examples/date-contracts/README.md) |
| [`fn:element-to-map`](functions/element-to-map.md) | 1, 2 | 1, 2 | names differ from pinned signatures | Same normalized list | [lookup](examples/signature-contracts/fn-element-to-map-1.xpath) |
| [`fn:element-to-map-plan`](functions/element-to-map.md) | 1 | 1 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-element-to-map-plan-1.xpath) |
| [`fn:ends-with-subsequence`](functions/shipment-event-sequences.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-ends-with-subsequence-2.xpath) |
| [`fn:every`](functions/higher-order-selection.md) | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-every-1.xpath) |
| [`fn:foot`](functions/order-sequence-selection.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-foot-1.xpath) |
| [`fn:graphemes`](functions/customer-text-cleanup.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-graphemes-1.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:hash`](functions/payload-fingerprints.md) | 1, 2, 3 | 1, 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-hash-1.xpath) |
| [`fn:highest`](functions/supplier-offer-ranking.md) | 1, 2, 3 | 1, 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-highest-1.xpath) |
| [`fn:identity`](functions/configured-label-callbacks.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-identity-1.xpath) / [URI/utility cases](examples/utility-contracts/README.md) |
| [`fn:index-of-substring`](functions/import-file-names.md) | 2 | 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-index-of-substring-2.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:index-where`](functions/higher-order-selection.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-index-where-2.xpath) |
| [`fn:insert-separator`](functions/order-sequence-selection.md) | 2 | 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-insert-separator-2.xpath) |
| [`fn:is-NaN`](functions/numeric-import-diagnostics.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-is-NaN-1.xpath) / [URI/utility cases](examples/utility-contracts/README.md) |
| [`fn:items-at`](functions/order-sequence-selection.md) | 2 | 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-items-at-2.xpath) |
| [`fn:jkey`](data-model/jnode-navigation.md) | 0, 1 | 0, 1 | names match a pinned signature | Different list | [lookup](examples/signature-contracts/fn-jkey-0.xpath) |
| [`fn:jtree`](data-model/jnode-navigation.md) | 1 | 1 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-jtree-1.xpath) |
| [`fn:jvalue`](data-model/jnode-navigation.md) | 0, 1 | 0, 1 | names match a pinned signature | Different list | [lookup](examples/signature-contracts/fn-jvalue-0.xpath) |
| [`fn:lowest`](functions/supplier-offer-ranking.md) | 1, 2, 3 | 1, 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-lowest-1.xpath) |
| [`fn:map-to-element`](functions/map-to-element.md) | 1, 2 | 1, 2 | names differ from pinned signatures | Same normalized list | [lookup](examples/signature-contracts/fn-map-to-element-1.xpath) |
| [`fn:op`](functions/numeric-import-diagnostics.md) | 1 | 1 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-op-1.xpath) |
| [`fn:pad-string`](functions/customer-text-cleanup.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-pad-string-2.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:parse-csv`](functions/csv-parsing.md) | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-parse-csv-1.xpath) |
| [`fn:parse-uri`](functions/invoice-api-uris.md) | 1, 2 | 1, 2 | names differ from pinned signatures | Same normalized list | [lookup](examples/signature-contracts/fn-parse-uri-1.xpath) |
| [`fn:partial-apply`](functions/configured-label-callbacks.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-partial-apply-2.xpath) |
| [`fn:partition`](functions/consecutive-partition.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-partition-2.xpath) |
| [`fn:replicate`](functions/order-sequence-selection.md) | 2 | 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-replicate-2.xpath) |
| [`fn:scan`](experimental/fn-scan.md) | 3 | 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-scan-3.xpath) |
| [`fn:seconds`](functions/timestamp-deadlines.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-seconds-1.xpath) / [date/time cases](examples/date-contracts/README.md) |
| [`fn:slice`](functions/order-sequence-selection.md) | 1, 2, 3, 4 | 1, 2, 3, 4 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-slice-1.xpath) |
| [`fn:some`](functions/higher-order-selection.md) | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-some-1.xpath) |
| [`fn:sort-by`](functions/supplier-offer-ranking.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-sort-by-2.xpath) |
| [`fn:sort-with`](functions/sort-with.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-sort-with-2.xpath) |
| [`fn:starts-with-subsequence`](functions/shipment-event-sequences.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-starts-with-subsequence-2.xpath) |
| [`fn:substring-after-last`](functions/import-file-names.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-substring-after-last-2.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:substring-before-last`](functions/import-file-names.md) | 2, 3 | 2, 3 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-substring-before-last-2.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:take-while`](functions/ordered-queue-prefix.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-take-while-2.xpath) |
| [`fn:transitive-closure`](functions/product-dependency-closure.md) | 2 | 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-transitive-closure-2.xpath) |
| [`fn:trim-space`](functions/customer-text-cleanup.md) | 1, 2 | 1, 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-trim-space-1.xpath) / [text cases](examples/text-contracts/README.md) |
| [`fn:trunk`](functions/order-sequence-selection.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-trunk-1.xpath) |
| [`fn:unix-dateTime`](functions/timestamp-deadlines.md) | 0, 1 | 0, 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/fn-unix-dateTime-0.xpath) / [date/time cases](examples/date-contracts/README.md) |
| [`fn:while-do`](functions/bounded-retries.md) | 3 | 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-while-do-3.xpath) |
| [`map:build`](functions/product-map-index.md) | 1, 2, 3, 4 | 1, 2, 3, 4 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/map-build-1.xpath) |
| [`map:entries`](functions/product-map-index.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/map-entries-1.xpath) / [container cases](examples/container-contracts/README.md) |
| [`map:filter`](functions/product-map-index.md) | 2 | 2 | absent metadata | No metadata | [lookup](examples/signature-contracts/map-filter-2.xpath) / [container cases](examples/container-contracts/README.md) |
| [`map:items`](functions/product-map-index.md) | 1 | 1 | absent metadata | No metadata | [lookup](examples/signature-contracts/map-items-1.xpath) / [container cases](examples/container-contracts/README.md) |
| `fn:compare` | 2, 3 | 2, 3 | names differ from pinned signatures | Same normalized list | [lookup](examples/signature-contracts/fn-compare-2.xpath) |
| `fn:min` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-min-1.xpath) |
| `fn:max` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-max-1.xpath) |
| `fn:sort` | 1, 2, 3 | 1, 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-sort-1.xpath) |
| `array:sort` | 1, 2, 3 | 1, 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/array-sort-1.xpath) |
| `fn:serialize` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-serialize-1.xpath) |
| `map:merge` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/map-merge-1.xpath) |
| `fn:replace` | 3, 4 | 2, 3, 4 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-replace-3.xpath) |
| `fn:subsequence` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-subsequence-2.xpath) |
| `fn:substring` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-substring-2.xpath) |
| `fn:string-join` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-string-join-1.xpath) |
| `fn:contains` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-contains-2.xpath) |
| `fn:starts-with` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-starts-with-2.xpath) |
| `fn:ends-with` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-ends-with-2.xpath) |
| `fn:index-of` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-index-of-2.xpath) |
| `fn:collation-key` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-collation-key-1.xpath) |
| `fn:contains-token` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-contains-token-2.xpath) |
| `fn:node-name` | 0, 1 | 0, 1 | names match a pinned signature | Different list | [lookup](examples/signature-contracts/fn-node-name-0.xpath) |
| `fn:lang` | 1, 2 | 1, 2 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-lang-1.xpath) |
| `fn:matches` | 2, 3 | 2, 3 | names match a pinned signature | Same normalized list | [lookup](examples/signature-contracts/fn-matches-2.xpath) |

## Remaining D2 work

1. Review the 37 missing keyword-metadata families against their full declarations, then publish realistic keyword/positional comparisons and omitted/empty/invalid outcomes for uncovered forms.
2. Reconcile the four spelling differences, the replace arity boundary and the reproduced two-let keyword failure as separate engine follow-ups; documentation tests currently preserve observed outcomes.
3. Verify default evaluation/focus and default-on-empty behavior beyond the sampled hash/replace cases; matching expression lists are only a textual milestone.
4. Review complete sequence-type/coercion/return contracts and declared option fields for URI, CSV, conversion, sorting, JSON and map/array families. Options maps alone do not certify their fields.
5. Inventory all changed existing 3.1 functions; these 20 are a scoped sample. Continue D3 grammar/type/pattern and D4 provenance/drift/acceptance work after D2 evidence is sufficient.

No engine source, public API, harness or gate membership changes in this delivery. Existing named family tests and applicable gates remain linked in catalog entries. The metadata/availability matrix and example checks do not promote upstream conformance sets or close AC-01–04.

## Text-family behavior supplied

The [text/Unicode review](TEXT-CONTRACT-REVIEW.md) adds 83 checks for eight absent-keyword families; metadata remains absent. 29 of the original 37 families still need targeted behavioral review. Current coverage is 67 entries, 1,556 feature + 24 chapter checks = 1,580. Default/empty options and collation, typed sources and full portable import reports are verified; coercion/option limits are explicit. D2 remains partial. Next: sequence selection/repetition contracts.

## Sequence-family behavior supplied

The [sequence selection/repetition review](SEQUENCE-CONTRACT-REVIEW.md) adds 65 checks for six families, including five more absent-keyword families and supported slice keywords. 13 of the original 37 absent-metadata families now have targeted review; 24 remain and metadata is unchanged. Current coverage: 67 entries, 1,621 feature + 24 chapter checks = 1,645. D2 stays partial. Next: equality/distinctness/subsequence predicate contracts.


The subsequent [predicate contract review](PREDICATE-CONTRACT-REVIEW.md) supplies the five equality/distinctness/subsequence families. Current targeted absent-keyword coverage is 18 of 37; 19 remain, with metadata unchanged. Its 102 added checks bring coverage to 67 entries, 1,723 feature + 24 chapter checks = 1,747. Earlier delivery counts above remain historical. Next: duplicate-values/highest/lowest/index-where comparison and selection contracts; D2 remains partial.


The subsequent [comparison/selection review](SELECTION-CONTRACT-REVIEW.md) brings targeted absent-keyword review to 21 of 37 families, with 16 remaining. Metadata is unchanged. Current catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815. Earlier delivery counts remain historical. Next: map/array container contracts; D2 stays partial.


The subsequent [container contract review](CONTAINER-CONTRACT-REVIEW.md) brings targeted absent-keyword review to 27 of 37 families, with 10 remaining. Metadata is unchanged. Current catalog: 67 entries, 1,880 feature + 24 chapter checks = 1,904, version 1.1.3. Earlier counts and queues are historical. Next: date/time construction and utilities; D2 stays partial.


The subsequent [date/time review](DATE-TIME-CONTRACT-REVIEW.md) brings targeted absent-keyword review to 32 of 37 families, with five remaining. Metadata is unchanged. Current catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012, version 1.1.3. Earlier counts and queues are historical. Next: URI/import utilities; D2 stays partial.


The subsequent [URI/import utility review](URI-UTILITY-CONTRACT-REVIEW.md) completes first targeted review for all 37 absent-keyword families, with none remaining unreviewed. All 37 still lack metadata. Current catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602, version 1.1.3. Earlier counts and queues are historical. Next: exhaustive changed-existing-3.1 contract inventory; D2 stays partial.


**Complete baseline-name inventory — 2026-10-12:** [Existing 3.1 contract inventory](CHANGED-31-CONTRACT-INVENTORY.md) The complete 197-name fn/math/map/array baseline inventory is now supplied, with dated W3C 3.1 declarations, pinned F&O e7ee4ab8 declarations and fresh 3.1/4.0 evidence. Added 353 checks: 320 exact-arity lookups, 17 named-reference refusals, ten focused defaults/behavior checks and six complete stock reports. Thirteen families have positional arity declaration differences; seventeen draft arities are unavailable in the fresh standard context. Type/prose differences remain candidates for semantic review, not certified behavioral changes. Empty substring/subsequence lengths have distinct observed 3.1 and 4.0 results. Empty sort keys and map:merge options refuse XPTY0004. Catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four chapters, 117 assigned names. All 37 reviewed keyword families still lack metadata. REQ-126 stays In Progress, D2 partial; version 1.1.3, no engine/API/harness change or release. Next: fn:sort, array:sort, fn:min, fn:max and map:merge defaults/conversion contracts. Prose classification, D3/D4, historical adoption pins and AC-03 remain open.


**Ordering/default review — 2026-10-12:** [Five-family review and supplier reports](ORDER-DEFAULT-CONTRACT-REVIEW.md) Targeted review of fn:sort, array:sort, fn:min, fn:max and map:merge adds 132 checks (126 focused, six complete supplier reports), preserving source declarations and exact 3.1/4.0/experimental keyword boundaries. min/max have no key callback parameter. Explicit empty sort keys and merge options, unknown sort collations, NaN ordering and malformed merge input/option cardinality remain engine follow-ups. Supplier rankings use numeric price plus stable ID and explicit duplicate combine; normal/reordered results match. Current catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four worked chapters, 117 assigned names. Version 1.1.3, REQ-126 In Progress, D2 partial. All 37 reviewed keyword families still lack metadata; no engine/API/harness change or release. Next: distinct-values, index-of, deep-equal, compare and contains-token equality/collation contracts. Earlier counts and queues are historical; normative prose impact, D3/D4, historical pins and AC-03 remain open.
