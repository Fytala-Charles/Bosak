<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-126 catalog completeness audit">
  <br><br><h1>REQ-126 Catalog Completeness Audit</h1>
  <p>Evidence, missing coverage, and the next documentation deliveries</p>
</div>

## Finding and scope

**REQ-126 is not ready to close.** Four worked chapters are supplied, but
the catalog does not yet cover every landed feature or record every deferred and
rejected decision. Existing entries also need more precise provenance and broader
examples. This audit is dated 2026-10-11 against source revision
`09e678f3` (1.1.1 publication documentation); published engine 1.1.1 is revision
`d35cf1d4bd1d1b7143ccb9ab304812f781dc0d50`.

The original audit baseline was 28 entries, 81 catalog cases, three worked chapters, and 13
chapter checks. All entries have the 16 numbered fields. Field presence does not
establish complete or accurate content. Chapter completeness is separate from
whole-feature inventory completeness.

Read alongside [REQ-126 acceptance criteria](../FEATURE_REQUESTS.md#req-126-xpath40-feature-catalog--gof-style-catalog-of-xpathxslt-40-syntax-decisions),
the [REQ-118 adoption dossier](../REQ-118-xpath-xslt-40.md),
and the [catalog index](README.md).

## External specifications and source documentation

These primary sources provide language definitions, migration baselines, draft
history, and upstream tests. Links and section anchors were checked on
2026-10-11. The 4.0 documents are evolving QT4CG drafts, not final W3C
Recommendations. Current links are navigation references; they do not prove that
Bosak adopted every feature or the latest semantics. Record the adopted draft
revision/date and Bosak delivery revision alongside feature-specific references.
When wording or numbering changes, retain historical disposition evidence and
compare the current draft explicitly.

| Area | Primary source | Use |
|---|---|---|
| XPath syntax and types | [XPath 4.0 draft](https://qt4cg.org/specifications/xquery-40/xpath-40.html) | Operators, bindings, grammar, coercion, and compatibility. |
| Functions and operators | [F&O 4.0 draft](https://qt4cg.org/specifications/xpath-functions-40/Overview.html) | Signatures, arguments, results, errors, and function examples. |
| Stylesheet language | [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), [changes since 3.0](https://qt4cg.org/specifications/xslt-40/Overview.html#changes-since-3.0), [incompatibilities](https://qt4cg.org/specifications/xslt-40/Overview.html#incompatibilities) | Instruction behavior and migration differences. |
| Data model | [XDM 4.0 draft](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html) | Atomic items, nodes, maps, arrays, and JNodes. |
| Serialization | [Serialization 4.0 draft](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html) | Output methods, parameters, and serialization errors. |
| Stable expression baseline | [XPath 3.1](https://www.w3.org/TR/xpath-31/), [F&O 3.1](https://www.w3.org/TR/xpath-functions-31/) | Earlier-version syntax and migration examples. |
| Stable stylesheet/model/output baseline | [XSLT 3.0](https://www.w3.org/TR/xslt-30/), [XDM 3.1](https://www.w3.org/TR/xpath-datamodel-31/), [Serialization 3.1](https://www.w3.org/TR/xslt-xquery-serialization-31/) | Portable worked chapters and baseline contracts. |
| Draft source and discussions | [QT4CG specification repository](https://github.com/qt4cg/qtspecs) | Source history, issues, pull requests, and adoption decisions. |
| Upstream 4.0 tests | [qt4tests](https://github.com/qt4cg/qt4tests), [xslt40-test](https://github.com/qt4cg/xslt40-test) | XPath/XQuery versus XSLT evidence; distinguish upstream tests from Bosak gate membership. |
| Upstream 3.x tests | [QT3](https://github.com/w3c/qt3tests), [xslt30-test](https://github.com/w3c/xslt30-test) | Regression baselines and XSLT 3.0 conformance evidence. |

Specification links establish language meaning; Bosak source, named tests, and
conformance runners establish implemented coverage. Deferred/rejected entries
link current alternatives, while their historical disposition must cite the
relevant issue/PR or archived draft when that evidence is supplied.

### Draft-evolution sources used by this audit

| Decision area | Primary history reference | Audit use |
|---|---|---|
| Destructuring and extended bindings | [QT4CG PR #1131](https://github.com/qt4cg/qtspecs/pull/1131) | Proposal context for the delivered binding forms. |
| Method-call scope | [QT4CG issue #2219](https://github.com/qt4cg/qtspecs/issues/2219) | Sequence generalization discussion behind the recorded deferral. |
| Experimental scan | [QT4CG PR #2824](https://github.com/qt4cg/qtspecs/pull/2824) | Proposal history; Bosak tier/gate evidence remains separate. |
| Nominative versus structural records | [QT4CG PR #2934](https://github.com/qt4cg/qtspecs/pull/2934) | Context for keeping the two type decisions distinct. |

These links support the history review. They do not replace an immutable adopted
specification revision or establish that Bosak implements the entire proposal.

## Acceptance assessment

| Criterion | Assessment | Evidence and remaining work |
|---|---|---|
| AC-01: landed slices, tiers, spec and delivery links | Partial | S1/S2/S5 family coverage remains partial; map/array and positional selection are now supplied. The 28 baseline entries now link exact Bosak delivery revisions and named tests. Immutable adopted specification revisions were not retained historically and remain explicit gaps. |
| AC-02: deferred/rejected decisions | Partial | Method-call and renamed mapping-arrow entries exist; dedicated records now cover the missing section 1.2 decisions, including shipped collection-sequence iteration and the partial JNode boundary. Further deferred grammar needs review. |
| AC-03: verified examples and gate evidence | Partial | 168 executable documentation checks pass. Baseline entries now carry named implementation evidence and explicit assigned-set or unassigned status. One concrete stale claim, fn:scan gate status, is corrected with this audit. |
| AC-04: structure, retained entries, drift warnings | Partial | 41 entries have all numbered fields and index links. No entry is removed by this audit. Generic prior-art disclaimers do not replace feature-specific drift explanations; supplementary forms and historical lifecycle need review. |
| AC-05: discoverability and canonical links | Met for current scope | README roadmap/catalog links, integration, and the REQ-118 reference companion exist; registry and index remain In Progress. This does not close other criteria. |

## Runtime function inventory

The [machine-readable snapshot](audit-function-inventory.json) contains the
published engine's public `FunctionLibrary.XPath40OnlyFunctionNames` and
`XPath40ExperimentalOnlyFunctionNames` sets. These are namespace/local-name
pairs, **not function signatures or arities**.

| Registered category | Names | Catalog assessment |
|---|---:|---|
| Frozen `fn:*` | 58 | 23 frozen fn names have family entries; signature and semantic coverage remain partial. |
| Frozen `map:*` | 4 | All four names map to the product-index family; entries/items result-shape gaps are explicit. Map iteration syntax is a different feature. |
| Frozen `array:*` | 5 | All five names have family entries: four batch functions plus array:sort-with in the sorting family. |
| Frozen zero-argument `xs:*` constructors | 49 | One constructor-family entry exists, with an integer example; this does not verify all constructor contracts. |
| Experimental `fn:scan` | 1 | Entry and verified examples exist; gate status was stale. |
| Total | 117 | 82 names map to an existing family entry; 35 have no dedicated family entry. A mapped name is not a completeness pass. |

Mapped non-constructor families now include scalar equality, CSV, element/map
conversion, comparator sorting, JNode navigation, higher-order selection,
partitioning, distinctness, product-map indexing, array batches, positional
sequence selection and experimental scan. The snapshot records an exact matching
gate identifier only when it occurs in GatedSets; null means no exact-name set
is assigned, not proof that no broader set exercises the function.

The snapshot does **not** inventory changed behavior/overloads of existing 3.1
functions, parser grammar, type syntax, XSLT instructions, or resource routes.
For example, upgraded `fn:compare`, QName `min`/`max`, changed map/array overloads,
and collations require an additional semantic/signature audit. The missing-name
count is not a count of missing Markdown files: related functions can share a
well-explained family entry.

### Family coverage and remaining work

| Family | Registered names without a dedicated family entry | Suggested realistic case |
|---|---|---|
| Sequence selection and repetition — supplied in this delivery | [`fn:replicate`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-replicate), [`fn:slice`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-slice), [`fn:items-at`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-items-at), [`fn:foot`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-foot), [`fn:trunk`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trunk), [`fn:insert-separator`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-insert-separator) | Paginate an order export while retaining boundary records. |
| Equality, duplicates, subsequences | [`fn:all-equal`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-all-equal), [`fn:contains-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-contains-subsequence), [`fn:starts-with-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-starts-with-subsequence), [`fn:ends-with-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-ends-with-subsequence) | Check shipment-event identifiers and required processing steps. |
| Higher-order processing | [`fn:take-while`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-take-while), [`fn:drop-while`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-drop-while), [`fn:while-do`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-while-do), [`fn:do-until`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-do-until), [`fn:partial-apply`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-partial-apply), [`fn:transitive-closure`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-transitive-closure), [`fn:identity`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-identity) | Route invoice lines, bound retries, and follow product dependencies with cycles. |
| Ranking, operators, type inspection | [`fn:highest`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-highest), [`fn:lowest`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-lowest), [`fn:sort-by`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-sort-by), [`fn:op`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-op), [`fn:is-NaN`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-is-NaN), [`fn:atomic-type-annotation`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-atomic-type-annotation) | Prioritize work orders and describe imported measurements. |
| Strings and Unicode | [`fn:char`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-char), [`fn:characters`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-characters), [`fn:graphemes`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-graphemes), [`fn:pad-string`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-pad-string), [`fn:trim-space`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trim-space), [`fn:index-of-substring`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-index-of-substring), [`fn:substring-before-last`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-before-last), [`fn:substring-after-last`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-after-last), [`fn:hash`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-hash) | Normalize supplier references without splitting Unicode graphemes. |
| URI and collation | [`fn:parse-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-parse-uri), [`fn:build-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-uri), [`fn:decode-from-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-decode-from-uri), [`fn:collation-available`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-collation-available) | Construct and validate document-resource identifiers. |
| Dates and durations | [`fn:seconds`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-seconds), [`fn:duration-to-seconds`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-duration-to-seconds), [`fn:build-dateTime`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-dateTime), [`fn:unix-dateTime`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-unix-dateTime), [`fn:days-in-month`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-days-in-month) | Convert timestamps and build a monthly dispatch calendar. |
| Maps — supplied in this delivery | [`map:build`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-build), [`map:entries`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-entries), [`map:filter`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-filter), [`map:items`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-items) | Index a product feed with an explicit duplicate-key policy. |
| Arrays — supplied in this delivery | [`array:build`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-build), [`array:empty`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-empty), [`array:items`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-items), [`array:slice`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-slice) | Preserve record boundaries when splitting import batches. |

## Landed syntax and cluster coverage

| Delivery area | Current coverage | Gap to resolve |
|---|---|---|
| REQ-118 S0 | Tier table and manifest modes | Explain arity-sensitive rejection and experimental boundaries in each affected family. |
| S1 / S2 / S5 | Function registry/tests are linked indirectly | Add the missing sequence/string/URI/date/map/array/HOF families above. |
| S3 | Otherwise, keyword arguments, string templates | Add binary/hex/underscore literals and guarded-expression behavior; distinguish valid forms and refusals. |
| S4 | Pipeline, mapping arrow, array/map iteration | Add focus-function syntax and `fn` abbreviation. The existing focus-functions filename currently explains zero-arity constructors, a separate later cluster. |
| S6a / S6b | Enum, choice, record, typed-let entries | Expand coercion/cast/lookup rules and `but with`; current record example does not cover the update construct. |
| S7 | Four instruction families | Expand actual supported forms and errors, particularly xsl:map duplicate functions and map-entry usage. |
| S8 | Arrays and switch | Audit delivered pattern/type-pattern and priority behavior before claiming coverage; no pattern-system entry exists. |
| REQ-123 operator/CSV/conversion clusters | CSV/conversion entries exist | Add fn:op; expand plan-driven conversion and supplementary function forms. |
| REQ-123 comparison/sorting cluster | Sorting and atomic equality | Add comparison semantics, array comparator form, NaN/type annotation, QName ordering and collation behavior. |
| REQ-123 JNode and destructuring clusters | Entries exist | Audit JNode type/path forms and every destructuring shape; a single successful example does not cover all forms. |
| REQ-123 experimental scan | Entry, examples, named gate | Keep its experimental tier distinct despite its promoted test set. |

The delivery history in the repository identifies concrete PRs: operator #102,
CSV #104, conversion #106, atomic equality #108, focus constructors #110,
sorting #112, comparison tail #114, JNode #116, and destructuring #129.
These are starting points for per-entry provenance, not substitutes for checking
the exact source and tests. Specification section numbers must be checked against
the adopted draft revision when entries are authored; this audit does not certify
that historical dossier numbers still match the live draft.

## Deferred and rejected decisions

| Dossier decision | Current record | Needed disposition work |
|---|---|---|
| Method-call `=?>` | Dedicated Deferred entry | Preserve the churn rationale and exact refused shape. |
| Nominative record types | [Deferred record](type-system/nominative-records.md) | Kept distinct from shipped structural record contracts. |
| Iteration over sequences of arrays/maps | [Shipped S4 extension](operators-and-syntax/sequence-collection-iteration.md) | Named tests and three published examples pin collection boundaries and invalid input. |
| fn:scan | Experimental entry | Historical deferral is superseded by experimental adoption; gate status corrected here. |
| JNode built-in template rules and full GNode/JNode model | [Partial-model boundary](data-model/jnode-template-boundary.md) | Navigation shipped; full model and JNode-specific XSLT built-in behavior are not certified. |
| `fn:all` / `fn:some-of` | [Rejected names](rejected/obsolete-quantifier-functions.md) | Exact XPST0017 refusals and accepted predicate alternative supplied. |
| `scan-left` / `scan-right` | [Superseded names](rejected/directional-scan.md) | Refused names and left-fold/array-state semantics recorded. |
| Old mapping arrow `=>>` | Dedicated rejected entry | Retain renamed disposition and link to `=!>`. |
| XPath switch expression | [XPath rejection](rejected/xpath-switch-expression.md) | Exact grammar refusal plus a 3.1 conditional alternative; XQuery and XSLT remain separate. |
| Extensible map types | [Proposal disposition](rejected/extensible-map-types.md) | Supplied cast/coercion examples pin surplus-field behavior; maps themselves remain supported. |

The wider dossier also defers computed XPath node constructors, XSLT type
declarations, enclosing modes, capturing accumulators, fn:apply-templates,
Serialization 4.0 changes, external-parser/validator functions, and XQuery/
streaming deltas. Include them in the broader decision inventory after the
section-1.2 acceptance scope is covered. Do not implement a deferred shape merely
because its documentation is missing.

## Prioritized deliveries

| Priority | Concrete delivery | Completion evidence |
|---|---|---|
| P1 | Repair evidence for the 28 existing entries | Exact adopted spec revision/section, delivery PR/revision, named tests, named conformance sets and actual gate status; retain explicit gaps. |
| P1 | Add the missing decision records | Cover the moving/rejected matrix, with alternatives and refusal evidence; resolve historical deferrals superseded by shipped features. |
| P2 | Add sequence and higher-order function chapters/entries | Real document cases, normal/edge inputs and results, complete code, earlier-version comparison where meaningful, exact compatibility refusals. |
| P2 | Add map/array, Unicode, URI/date, operator/type families | Cover the remaining 35 unassigned registered names through coherent families, including signature-specific behavior; 14 map/array/selection names are now assigned. |
| P2 | Expand syntax and changed-3.1 semantics | Literals, focus functions, guarded expressions, record updates, comparisons/collations, patterns, and supplementary forms. |
| P3 | Reconcile acceptance and publish the complete reference | Zero unassigned adopted features/decisions, accurate index/tier links, verified examples, and explicit exceptions to conformance evidence rules. |

**Recommended next documentation step:** continue the remaining equality/subsequence and
higher-order families afterward, then supplementary signatures and parser/type/pattern forms.
The baseline provenance repair, section-1.2 decision records and first sequence/HOF
routing chapter are supplied below. Immutable historical specification snapshots
remain an explicit gap. No engine work or conformance-set promotion is required
for the next documentation slice.

For XPath entries, distinguish named qt4 sets from unit/example checks. XSLT-only
instructions need XSLT test evidence; do not invent qt4 expression sets for them.
Record the applicable harness and any unavailable 4.0 conformance evidence
explicitly when reconciling AC-03.

## Audit method and verification

1. Read the five REQ-126 criteria, REQ-118 section-1.2 decisions and S0-S8 plan,
   post-1.0 delivery history, 28 entries, and published example manifest.
2. Reflect the two public function-name sets from the published 1.1.0 assembly;
   classify namespace/local-name pairs into existing catalog families.
3. Compare named gate identifiers with the actual `GatedSets` initializer in
   [ConformanceRunner40](../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).
4. Check entry field counts, index membership, snapshot totals, manifest files,
   local links, and rendered Markdown. Retain executable fixtures unchanged.

Audit findings are documentation coverage assessments, not new claims of engine
conformance. The last published full solution/gate results remain in the
[patch release notes](../RELEASE-NOTES-1.1.1.md); this audit does not rerun those engines.

## Historical progress before the map/array/selection delivery: 2026-10-11

PR #155 is merged at `17f9cc7c`. The follow-up supplies 38 entries, 104 manifest
cases and four worked chapters with 24 chapter checks: 128 documentation checks.
The [provenance ledger](PROVENANCE.md) records exact engine/dossier commits and
named tests for all 28 baseline entries, while preserving the historical adopted
specification snapshot gap. Three new function families cover six additional
registered names, reducing unassigned names from 55 to 49. This remains a family
mapping, not complete signature coverage or a new conformance promotion.

The [XPath switch](rejected/xpath-switch-expression.md) and
[extensible-map proposal](rejected/extensible-map-types.md) now have explicit
disposition records. Record casts discard surplus entries; coercion rejects them.
The REQ-118 historical shorthand must not override delivered record tests.

The [routing chapter](worked-examples/order-routing/README.md) also exposes a
runtime integration gap: `version="4.0"` alone does not select XPath40 for
transformation-time expression compilation. It uses the public explicitly opted-in
XPath API for planning and passes its map to portable XSLT. Automatic runtime
compatibility propagation requires a separately tested engine slice; it is not
implemented or certified by this documentation work.

Next priorities are the remaining sequence-selection families, supplementary
signatures and parser/type/pattern coverage. Future engine slices must record an
immutable adopted specification revision at delivery time. REQ-126 stays In Progress.

## Runtime compatibility correction (source 1.1.1, 2026-10-11)

The [separately tested engine correction](../REQ-118-XSLT-XPATH40-COMPATIBILITY.md)
resolves the runtime propagation gap discovered by routing. Effective-version
4.0 expressions select frozen XPath40; older elements retain XPath31 and nested
evaluations restore their caller's profile. Static compilation, AVTs, dynamic
evaluation and module boundaries are covered. The routing chapter now also has a
self-contained stylesheet with five additional checks; the original two-stage
workflow remains the published-1.1.0 alternative. The correction is published in 1.1.1. At that delivery the missing-name count
remained 49; the later map/array/selection delivery below reduces it to 35.
REQ-126 remains In Progress.

The correction records an immutable QT4CG specification source revision at
delivery. This establishes its own reviewed source and does not fill historical
snapshot gaps in the 28 baseline ledger rows.

## Map/array and sequence-selection delivery: 2026-10-11

Three new entries supply 40 exact manifest checks: [product-map indexing](functions/product-map-index.md)
(15), [array import batches](functions/import-array-batches.md) (11), and
[order-export selection](functions/order-sequence-selection.md) (14).
The catalog now has **41 entries, 144 feature checks and 24 chapter checks
(168 total)**. Fourteen names gain family assignments: **82 assigned / 35 unassigned
of 117**. All 16 fields, named unit tests and actual exact-name gate membership
are supplied. Historical adopted-specification snapshots remain unavailable;
these entries record a new review pin instead:
[QT4CG source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).

### Confirmed engine follow-up: map result shapes

In public/source 1.1.1, `map:entries` returns one array item containing single-entry
maps; the reviewed draft specifies a sequence of single-entry maps. Its named
existing unit test already expects array indexing. The new main example uses an
explicit Bosak adapter (`?*`); `entries-boundary` and `entries-baseline` distinguish
the shapes. `map:items` also preserves a grouped multi-item value: the new
`items-sequence-boundary` counts two values while the draft-equivalent 3.1
`map:for-each` baseline counts three items. Existing named unit evidence likewise
expects the grouped result. See [the entry’s consequences](functions/product-map-index.md#11-consequences).

Correct both contracts in a separate REQ-118 bug-fix slice, updating the existing
tests and these probes deliberately. Include ordinary/dynamic calls, empty maps,
multi-item and nested map/array values, entry order, default-3.1 refusal and
caller-profile restoration; run full solution, frozen QT3 and qt4 gates before
release. This catalog assignment is not a completeness/conformance pass.

The current delivery changes documentation and fixtures only. No engine code,
package version or gate membership changes. REQ-126 remains In Progress.

**Delivery verification (2026-10-11):** Release build 0 warnings / 0 errors; full isolated solution **4,143/0/0**, targeted catalog **144/0/0**. All 41 entries/index links, 166 primary links/anchors, 28 baseline provenance records and 144 manifest files verified; 54 documentation pages rendered. Inline expressions/results and named tests match. Engine gates were not rerun for this documentation-only change.

## Map contract correction (source 1.1.2): 2026-10-11

The [REQ-118 engine correction](../REQ-118-MAP-RESULT-SHAPES.md) addresses both
result-shape findings above. `map:entries` returns a sequence of single-entry
maps; `map:items` concatenates value sequences while preserving nested containers.
The product-index main expression no longer uses the Bosak-specific `?*` adapter;
boundary probes now agree with the portable baselines. Twenty-one new standard
checks and two nested-stylesheet profile checks cover direct/dynamic calls,
empty and nested values, order and default-mode isolation. The dossier records
full regression and frozen gate evidence. Map upstream sets remain exploratory;
no sets are promoted. Version 1.1.2 is published and its public artifacts verified. Counts remain
41 entries, 168 documentation checks and 35 unassigned names; REQ-126 stays In Progress.

**Map correction final verification (2026-10-11):** 1.1.2 published and verified; full solution 4,166/0/0, QT3 31,142/0/679, qt4 promoted sets 8,278/0/1,894 with zero gated failures. Targeted map-entries 16/0/1 and map-items 15/0/0 remain exploratory. See [the correction dossier](../REQ-118-MAP-RESULT-SHAPES.md) and [patch release notes](../RELEASE-NOTES-1.1.2.md).

**1.1.2 publication (2026-10-11):** [v1.1.2](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.1.2) at `3aebf76a24b28af7d40aa42e6eaf714b3a596838`; all ten NuGet packages downloaded and verified, eight fresh-cache consumer checks passed. REQ-126 remains In Progress with 35 unassigned names.

