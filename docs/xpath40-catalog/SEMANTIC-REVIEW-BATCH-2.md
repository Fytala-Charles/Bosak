<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak second nineteen-entry semantic and applicable-evidence review">
  <h1>Semantic and applicable-evidence review — C10</h1>
  <p>26 October 2026 · batch 2 of 4 · published Bosak 1.26.3</p>
</div>

**C10 completes the prescribed 19-entry review with 55 contract-group receipts. Three closure workpackages remain open.** [The machine report](SEMANTIC-REVIEW-BATCH-2.json) retains each contract, function-specific C02 normal/empty/error evidence, current example execution, language-alignment warning and remaining limit. Review completion is not unconditional entry or owner acceptance.

## Results and samples

All **3,469 fresh existing tests pass**: 3,148 catalog cases and 321 selected library tests. The batch maps **892 distinct cases** from exact entry references and the earlier function-specific receipts. Their **893 fixture/input files match compiled test assets byte-for-byte**, as does the complete manifest. Every case retains explicit kind, compatibility, input, expected result/error and baseline fields.

All **82 Sample Code blocks** have verification paths: exact executed fixtures, contextual fragments, declared outputs, displayed XML input matching, or the independent Python oracle. Five formatted XML inputs match their embedded fixture data, ignoring only formatting whitespace in these element-only examples. The **verbatim Python SHA-256 example executes successfully**, and its digest agrees with the separately executed XPath hash control. It establishes the supplied payload/algorithm result, not arbitrary canonicalization or normalization.

Some TRX arguments are truncated. The receipt retains the truncation suffix and candidate IDs, including the case where the displayed prefix is itself another complete ID. Complete enumeration, unique result IDs, all-Passed results and exact manifest/asset identity establish execution; no unique row identity is invented for ambiguous prefixes.

The **37 supplemental expressions produce 74 observations**: **60 matching controls, 10 unmet current-draft controls, two disputed specification-example controls, and two URI type-coercion observations**. Independent expectations remain unchanged:

| Requirement / observation | Outcome under both 4.0 API profiles | Ownership |
|---|---|---|
| Hangul L+V forms one grapheme cluster | Two clusters | C02 F13, documented segmentation limit |
| Current compare keyword labels `value1`/`value2` | `XPST0017`; released labels still work | C02 F01 metadata/adoption boundary |
| Constructed seconds retain `4.2678` | `4.267` | C02 F10 precision limit |
| Empty atomic-type-annotation refuses `XPTY0004` | Returns a type record | C02 F08-atomic-annotation-empty |
| map:filter rejects a string predicate result | Accepts the truthy string | C02 F07/F08 |
| Published highest computed-key example returns 20 through 25 | Returns 25 | Separate source-consistency finding below |

The two additional URI observations retain the unsupported string-valued Boolean-option behavior without inventing a required coercion/error classification. A typed Boolean false control preserves port 443. Ignored options, historical observed-gap fixtures and unsupported names remain observations or refusals, not conformance.

## Highest: source text versus example

At immutable review source `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`, the highest algorithm sorts by the modified key, chooses boundary item B, then selects input items A contextually equal to B. The framework defines atomic contextual equality using `compare(A,B)=0`. The same function source gives `highest(1 to 25, (), fn { . idiv 10 })` as 20 through 25. Released Bosak returns 25.

[Both exact source fragments](evidence/c10-semantic-batch2/review-sources.json) and the independent example expectation are preserved. This review leaves the interpretation open rather than diagnosing an engine correction from the example alone. The ranking entry now states that ambiguity explicitly; its original observed contract is retained in [the clarification receipt](evidence/c10-semantic-batch2/documentation-corrections.json). No computed-key tie parity is certified.

## Per-entry receipts

| Entry | Receipt | Applicable gates and remaining limits |
|---|---|---|
| [functions/customer-text-cleanup.md](functions/customer-text-cleanup.md) | Limited text contract; incomplete grapheme segmentation; 3 contract groups/81 linked cases | Named tests/examples; no exact applicable assignment; Grapheme segmentation is approximate; missing keywords, implicit trim focus and unknown-option validation are not supported contracts. |
| [functions/element-to-map.md](functions/element-to-map.md) | Limited verified frozen contract; 3 contract groups/5 linked cases | fn-element-to-map, fn-element-to-map-plan; The published simple-content example does not certify every conversion-plan record, name collision, namespace, schema annotation or option; use only tested layouts/options. Current-draft escape-names decoding is unsupported; the new option is not adopted automatically into frozen or Experimental profiles. |
| [functions/higher-order-selection.md](functions/higher-order-selection.md) | Limited verified frozen contract; 3 contract groups/20 linked cases | Named tests/examples; no exact applicable assignment; Callable/subtyping combinations beyond the verified signatures remain unsupported in this consumer reference. |
| [functions/import-array-batches.md](functions/import-array-batches.md) | Limited verified frozen contract; 3 contract groups/53 linked cases | array-build, array-items, array-slice; array:members/of-members are JNode functions owned by C05, not replacements for member/item concatenation. |
| [functions/import-file-names.md](functions/import-file-names.md) | Limited verified frozen contract; 3 contract groups/43 linked cases | fn-index-of-substring; Do not rely on tolerated typed-numeric string arguments; no grapheme-position or path-authorization contract. |
| [functions/invoice-api-uris.md](functions/invoice-api-uris.md) | Limited verified frozen contract; 3 contract groups/110 linked cases | Named tests/examples; no exact applicable assignment; Use validated typed Boolean options and remove raw query when deliberately supplying an empty query map. |
| [functions/map-to-element.md](functions/map-to-element.md) | Limited verified frozen contract; 2 contract groups/5 linked cases | fn-map-to-element; The reduced example does not certify the whole conversion-plan/record/namespaces space; supplementary provider Unicode limits belong to the deferred skip queue. Current-draft escape-names encoding is unsupported; use XML-compatible field names and verified conversion options. |
| [functions/measurement-comparison.md](functions/measurement-comparison.md) | Exact positional comparison; keyword mismatch remains; 3 contract groups/52 linked cases | fn-compare; Do not substitute this comparator for engineering tolerance, generalized numeric conversion or automatic unit normalization. |
| [functions/month-end-cutoffs.md](functions/month-end-cutoffs.md) | Limited Gregorian construction; fractional precision gap; 3 contract groups/77 linked cases | Named tests/examples; no exact applicable assignment; Millisecond precision loss, permissive malformed/multi-item input and interop calendar bounds remain explicit; arbitrary extended-year operations are not certified. |
| [functions/numeric-import-diagnostics.md](functions/numeric-import-diagnostics.md) | Blocked empty-annotation contract; 3 contract groups/39 linked cases | Named tests/examples; no exact applicable assignment; No complete derived-type reflection/function-conversion audit; C03 owns constructor/type/coercion coverage. atomic-type-annotation(()) incorrectly returns a type record in both 4.0 profiles. Required empty-input refusal is explicitly unsupported; bounded correction F08-atomic-annotation-empty retains the independent XPTY0004 expectation. |
| [functions/order-sequence-selection.md](functions/order-sequence-selection.md) | Limited verified frozen contract; 3 contract groups/76 linked cases | fn-foot, fn-insert-separator, fn-items-at, fn-replicate, fn-trunk; Integral-decimal tolerance is not the declared integer-position contract; repeat counts must remain bounded. |
| [functions/ordered-queue-prefix.md](functions/ordered-queue-prefix.md) | Limited verified frozen contract; 3 contract groups/18 linked cases | fn-take-while; Not an arbitrary filter or partition; no operational side effects or general callable-subtyping promise. |
| [functions/payload-fingerprints.md](functions/payload-fingerprints.md) | Limited verified frozen contract; 3 contract groups/35 linked cases | Named tests/examples; no exact applicable assignment; BLAKE3 and CRC-32 remain unsupported FOHA0001; opaque hash options do not establish a missing standard named parameter. |
| [functions/product-dependency-closure.md](functions/product-dependency-closure.md) | Limited verified frozen contract; 3 contract groups/15 linked cases | Named tests/examples; no exact applicable assignment; Only XML nodes in one document are covered; no JNode/cross-document ordering, topological installation order or quantity explosion. |
| [functions/product-map-index.md](functions/product-map-index.md) | Blocked strict filter-predicate contract; 3 contract groups/62 linked cases | Named tests/examples; no exact applicable assignment; map:filter currently accepts nonboolean truthy predicate results; do not use them. Map enumeration is not portable business order. |
| [functions/sequence-distinctness.md](functions/sequence-distinctness.md) | Limited verified frozen contract; 3 contract groups/41 linked cases | Named tests/examples; no exact applicable assignment; Maps do not satisfy the declared atomic input contract despite current tolerance; no implicit case folding or normalization. |
| [functions/shipment-event-sequences.md](functions/shipment-event-sequences.md) | Limited verified frozen contract; 3 contract groups/108 linked cases | fn-contains-subsequence; Use explicit presence/count guards for missing IDs; no complete carrier-state machine or unverified coercion contract. |
| [functions/sort-with.md](functions/sort-with.md) | Limited verified frozen contract; 2 contract groups/3 linked cases | array-sort-with, fn-sort-with; Comparator consistency and arbitrary result/coercion combinations are not certified; avoid NaN/mixed-family business order. |
| [functions/supplier-offer-ranking.md](functions/supplier-offer-ranking.md) | Limited ranking; unresolved source/example consistency; 3 contract groups/76 linked cases | Named tests/examples; no exact applicable assignment; Equal keys do not imply all distinct records are extrema; NaN/exact mixed-number ordering and unverified record fields remain limits. |

Gate membership is checked against the unchanged 224-set source list. Only applicable named sets are recorded; missing assignments use named tests and exact examples. A set name is not a coverage certificate, and none is promoted here. Optional algorithms, XML conversion-plan/name options, arbitrary callback conversion, map order, URI option coercion, full Unicode segmentation and generalized calendar/precision claims remain bounded by the receipts.

## Preservation and reproduction

Current normative review source is an immutable review pin, not a reconstructed adoption revision. C08's approved legacy disposition remains restricted to its original 28 records. All earlier defects and final semantic acceptance remain open.

Engine, tests and executable fixtures remain identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `06b53349`. C02's **21,798 full solution passes** and C00's frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** remain inherited evidence. No fresh full-suite or manual gate execution is claimed. No engine, fixture, profile, filter, version or release change follows. The catalog remains 76 entries and 3,148 feature +24 chapter checks.

```powershell
python docs/xpath40-catalog/build-c10-review.py
python docs/xpath40-catalog/evidence/c10-semantic-batch2/validate-evidence.py --local-artifacts
```

[The manifest](evidence/c10-semantic-batch2/manifest.json) protects original snapshots, exact source/example controls, Python output, source identities, assets, TRX and fresh logs. C00–C09 raw evidence and historical plans remain unchanged. HTML rendering uses unchanged Fytala CSS; no screenshot/PDF acceptance is claimed. Next is C11's third 19-entry batch; C13 retains final owner acceptance. Skip fixes, Braid and extension work remain deferred.
